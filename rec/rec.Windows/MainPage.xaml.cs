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

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace rec
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public Recording CurrentRecording { get; set; }

        private TimeSpan currentMax = new TimeSpan(0, 0, 10);

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

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentRecording.IsRecording)
            {
                CurrentRecording.Stop();
            }
            else
            {
                CurrentRecording.Start();
            }
                
        }
    }
}
