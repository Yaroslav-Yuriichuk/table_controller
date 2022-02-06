using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using Stacker.Controllers;
using Stacker.Enums;

namespace Stacker.DaysData
{
    public class DayDataCounter
    {
        public Action OnTick;

        private DispatcherTimer timer;

        public DayDataCounter()
        {
            SetUpTimer();
        }

        public void Start()
        {
            timer.Start();
        }

        public void Stop()
        {
            timer.Stop();
        }

        private void SetUpTimer()
        {
            timer = new DispatcherTimer()
            {
                Interval = new TimeSpan(0, 0, 59) //Timer error
            };
            timer.Tick += TimerTick;
        }

        private void TimerTick(object sender, EventArgs e)
        {
            if (StackerController.Instance.CurrentPosition == Position.UP)
            {
                StackerController.Instance.Today.MinutesInStayMode++;
            }
            else
            {
                StackerController.Instance.Today.MinutesInSitMode++;
            }
            StackerController.Instance.OnTodayDayDataChanged?.Invoke();
        }
    }
}
