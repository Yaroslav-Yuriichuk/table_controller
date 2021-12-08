using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Table
{
    static class Util
    {
        public static string MinutesToHoursString(int minutes)
        {
            if (minutes < 60)
            {
                return minutes + "m";
            }
            else if (minutes % 60 == 0)
            {
                return minutes / 60 + "h";
            }
            else
            {
                return minutes / 60 + "h " + minutes % 60 + "m";
            }
        }
    }
}
