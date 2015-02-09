using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.Media.MediaProperties;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.UI.Popups;
using Windows.Media.Capture;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace rec
{
    public enum Format
    {
        m4a,
        mp3,
        wma,
        wav
    }

    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public Recording CurrentRecording { get; set; }

        private TimeSpan currentMax = new TimeSpan(0, 0, 10);
        private MediaCapture _mediaCapture;
        private IRandomAccessStream _audioStream;
        private FileSavePicker _fileSavePicker;
        private Format _selectedFormat = Format.wav;

        private AudioEncodingQuality _selectedQuality = AudioEncodingQuality.High;

        public MainPage()
        {
            this.CurrentRecording = new Recording((s, ts) =>
            {
                var time = String.Format(
                    "{0:00}:{1:00}:{2:00}", 
                    ts.Hours, ts.Minutes, ts.Seconds);
                var ms = String.Format(".{0:0}", ts.Milliseconds / 100);

                this.txtTime.Text = time;
                this.txtMs.Text = ms;

                if (ts >= currentMax)
                {
                    currentMax = ts.TotalSeconds > 60 && ts.TotalMinutes < 2 ? new TimeSpan(0, 2, 0) :
                        ts.TotalMinutes > 2 && ts.TotalMinutes < 5 ? new TimeSpan(0, 5, 0) :
                        currentMax.Add(new TimeSpan(0, 5, 0));
                }

                this.pbRecording.Maximum = 100;
                this.pbRecording.Value = 100 * ts.TotalMilliseconds / currentMax.TotalMilliseconds;
                

            });
            
            this.InitializeComponent();

            UpdateMaxTimeDisplay();
        }

        private void UpdateMaxTimeDisplay()
        {
            this.txtMax.Text =
                currentMax.TotalMinutes > 0 ? currentMax.TotalMinutes.ToString() + " minutes" :
                currentMax.TotalSeconds.ToString() + " seconds";
        }

        private void InitFileSavePicker()
        {
            _fileSavePicker = new FileSavePicker();
            _fileSavePicker.FileTypeChoices.Add("Encoding", new List<string>() { "." + this._selectedFormat.ToString() });
            _fileSavePicker.SuggestedStartLocation = PickerLocationId.MusicLibrary;
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentRecording.IsRecording)
            {
                await this._mediaCapture.StopRecordAsync();
                CurrentRecording.Stop();
                
                InitFileSavePicker();
        
                var mediaFile = await _fileSavePicker.PickSaveFileAsync();

                if (mediaFile != null)
                {
                    using (var dataReader = new DataReader(_audioStream.GetInputStreamAt(0)))
                    {
                        await dataReader.LoadAsync((uint)_audioStream.Size);
                        byte[] buffer = new byte[(int)_audioStream.Size];
                        dataReader.ReadBytes(buffer);
                        await FileIO.WriteBytesAsync(mediaFile, buffer);
                    }
                }
            }
            else
            {
                this._mediaCapture = new MediaCapture();
                
                var captureInitSettings = new MediaCaptureInitializationSettings();
                
                captureInitSettings.StreamingCaptureMode = StreamingCaptureMode.Audio;
                
                await _mediaCapture.InitializeAsync(captureInitSettings);
                _mediaCapture.Failed += MediaCaptureOnFailed;
                _mediaCapture.RecordLimitationExceeded += MediaCaptureOnRecordLimitationExceeded;

                MediaEncodingProfile encodingProfile = null;
                switch (this._selectedFormat)
                {
                    case Format.m4a:
                        encodingProfile = MediaEncodingProfile.CreateM4a(this._selectedQuality);
                        break;
                    case Format.mp3:
                        encodingProfile = MediaEncodingProfile.CreateMp3(this._selectedQuality);
                        break;
                    case Format.wma:
                        encodingProfile = MediaEncodingProfile.CreateWma(this._selectedQuality);
                        break;
                    case Format.wav:
                        encodingProfile = MediaEncodingProfile.CreateWav(this._selectedQuality);
                        break;
                    default:
                        break;
                }
                
                this._audioStream = new InMemoryRandomAccessStream();
                await _mediaCapture.StartRecordToStreamAsync(encodingProfile, this._audioStream);

                CurrentRecording.Start();
            }
                
        }

        private async void MediaCaptureOnRecordLimitationExceeded(MediaCapture sender)
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, async () =>
            {
                await sender.StopRecordAsync();
                var warningMessage = new MessageDialog("The recording has stopped because you exceeded the maximum recording length.", "Recording Stoppped");
                await warningMessage.ShowAsync();
            });

        }

        private async void MediaCaptureOnFailed(MediaCapture sender, MediaCaptureFailedEventArgs errorEventArgs)
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, async () =>
            {
                var warningMessage = new MessageDialog(String.Format("The audio capture failed: {0}", errorEventArgs.Message), "Capture Failed");
                await warningMessage.ShowAsync();
            });
        }
    }
}
