using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stacker.Timers
{
    public class UpdatableSingleTickTimer : SingleTickTimer
    {
        public UpdatableSingleTickTimer() : base(null, new TimeSpan()) { }

        public void Start(TimeSpan interval)
        {
            timer.Interval = interval;
            Start();
        }

        public void UpdateOnTick(Action tick)
        {
            onTick = tick;
        }
    }
}
