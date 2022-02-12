using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace Stacker.Timers
{
    public class SingleTickTimer
    {
        protected DispatcherTimer timer;
        protected Action onTick;

        protected DateTime timeIntervalStarted;
        protected bool isActive;

        public TimeSpan RemainingTimeToTick
        {
            get => timer.Interval - (DateTime.Now - timeIntervalStarted);
        }

        public TimeSpan ElapsedTimeInCurrentInterval
        {
            get => DateTime.Now - timeIntervalStarted;
        }

        public SingleTickTimer(Action tick, TimeSpan interval)
        {
            onTick = tick;
            timer = new DispatcherTimer()
            {
                Interval = interval
            };
            timer.Tick += InvokeOnTick;
        }

        public void Start()
        {
            if (onTick != null)
            {
                timer.Start();
                timeIntervalStarted = DateTime.Now;
                isActive = true;
            }
        }

        public void Stop()
        {
            timer.Stop();
            isActive = false;
        }

        private void InvokeOnTick(object sender, EventArgs e)
        {
            onTick?.Invoke();
            Stop();
        }
    }
}
