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
        private DispatcherTimer timer;
        private Action OnTick;

        public SingleTickTimer(Action tick, TimeSpan timeSpan)
        {
            OnTick = tick;
            timer = new DispatcherTimer()
            {
                Interval = timeSpan
            };
            timer.Tick += InvokeOnTick;
        }

        public void Start()
        {
            timer.Start();
        }

        public void Stop()
        {
            timer.Stop();
        }

        private void InvokeOnTick(object sender, EventArgs e)
        {
            OnTick?.Invoke();
            timer.Stop();
        }
    }
}
