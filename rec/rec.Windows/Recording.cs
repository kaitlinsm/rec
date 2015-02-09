using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.UI.Xaml;

namespace rec
{
    public class Recording
    {
        private DispatcherTimer dt;
        private Stopwatch sw;
        private Action<object, TimeSpan> tickEventHanlder;


        public Recording(Action<object, TimeSpan> tickEventHandler)
        {
            if (tickEventHandler == null)
                throw new ArgumentNullException("tickEventHandler", "Can't be null!");

            this.dt = new DispatcherTimer();
            this.sw = new Stopwatch();
            this.tickEventHanlder = tickEventHandler;
            this.dt.Tick += sw_Tick;

            this.dt.Interval = new TimeSpan(0, 0, 0, 0, 100);
        }

        void sw_Tick(object sender, object e)
        {
            this.tickEventHanlder(sender, this.sw.Elapsed);
        }

        internal void Start()
        {
            dt.Start();
            sw.Start();
        }

        internal bool IsRecording
        {
            get
            {
                return this.sw.IsRunning;
            }
        }

        internal void Stop()
        {
            dt.Stop();
            sw.Stop();
        }

        
        
    }
}
