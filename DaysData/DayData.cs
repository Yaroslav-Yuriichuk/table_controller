using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stacker
{
    [Serializable]
    public class DayData
    {
        private DateTime date;
        private int minutesInSitMode;
        private int minutesInStayMode;

        public DateTime Date
        {
            get => date;
            set => date = value;
        }
        public int MinutesInSitMode
        {
            get => minutesInSitMode;
            set => minutesInSitMode = value;
        }
        public int MinutesInStayMode
        {
            get => minutesInStayMode;
            set => minutesInStayMode = value;
        }

        public DayData() { }

        public DayData(DateTime date, int minutesInSitMode, int minutesInStayMode)
        {
            this.date = date;
            this.minutesInSitMode = minutesInSitMode;
            this.minutesInStayMode = minutesInStayMode;
        }
    }
}
