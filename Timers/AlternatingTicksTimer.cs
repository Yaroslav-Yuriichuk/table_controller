using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace Stacker.Timers
{
    public class AlternatingTicksTimer
    {
        private DispatcherTimer timer;
        private DateTime timeIntervalStarted;

        private Action[] actions;
        private TimeSpan[] intervals;

        private int currentActionIndex = 0;
        private bool isActive = false;

        public AlternatingTicksTimer(Action firstTick, TimeSpan firstInterval,
            Action secondTick, TimeSpan secondInterval)
        {
            timer = new DispatcherTimer();
            timer.Tick += InvokeOnTick;

            actions = new Action[] { firstTick, secondTick };
            intervals = new TimeSpan[] { firstInterval, secondInterval };
        }

        public void Start()
        {
            timer.Interval = intervals[currentActionIndex];
            timer.Start();
            timeIntervalStarted = DateTime.Now;
            isActive = true;
        }

        public void Stop()
        {
            timer.Stop();
            isActive = false;
        }

        public void UpdateFirstInterval(TimeSpan newInterval)
        {
            UpdateInterval(newInterval, 0);
        }

        public void UpdateSecondInterval(TimeSpan newInterval)
        {
            UpdateInterval(newInterval, 1);
        }

        private void UpdateInterval(TimeSpan newInterval, int index)
        {
            if (index != currentActionIndex)
            {
                intervals[GetOppositeIndexToCurrent()] = newInterval;
                return;
            }

            if (!isActive)
            {
                intervals[currentActionIndex] = newInterval;
                return;
            }

            Stop();

            double elapsedTimeInMilliseconds =
                        (DateTime.Now - timeIntervalStarted).TotalMilliseconds;

            int remainingTimeInMilliseconds = (int)newInterval.TotalMinutes * 60 * 1000
                - (int)elapsedTimeInMilliseconds;

            if (remainingTimeInMilliseconds < 0)
            {
                actions[currentActionIndex]?.Invoke();
                currentActionIndex = GetOppositeIndexToCurrent();
                return;
            }

            timer.Interval = new TimeSpan(0, 0, 0, 0, remainingTimeInMilliseconds);
            timer.Start();
            isActive = true;
        }

        private void InvokeOnTick(object sender, EventArgs e)
        {
            actions[currentActionIndex]?.Invoke();
            currentActionIndex = GetOppositeIndexToCurrent();
            timer.Stop();
        }

        private int GetOppositeIndexToCurrent()
        {
            return (currentActionIndex + 1) % 2;
        }
    }
}
