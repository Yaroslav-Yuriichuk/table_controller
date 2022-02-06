using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace Stacker.DaysData
{
    public static class DayDataSaver
    {
        private const string TodayDayDataFile = "TodayDayData.dat";
        private const string AllDaysDataFile = "AllDaysData.dat";

        public static DayData LoadTodayDayData()
        {
            if (!File.Exists(TodayDayDataFile))
            {
                return new DayData(DateTime.Now, 0, 0);
            }

            DayData todayDayData
                = new JavaScriptSerializer().Deserialize<DayData>(File.ReadAllText(TodayDayDataFile));
            if (todayDayData == null)
            {
                return new DayData(DateTime.Now, 0, 0);
            }
            if (todayDayData.Date.Date == DateTime.Now.Date)
            {
                return todayDayData;
            }
            return new DayData(DateTime.Now, 0, 0);
        }
    
        public static List<DayData> LoadAllDaysData()
        {
            List<DayData> allDaysData = new List<DayData>();

            if (File.Exists(AllDaysDataFile))
            {
                allDaysData
                    = new JavaScriptSerializer().Deserialize<List<DayData>>(File.ReadAllText(AllDaysDataFile));
            }

            return allDaysData;
        }
    
        public static void SaveTodayDayData(DayData todayDayData)
        {
            File.WriteAllText(TodayDayDataFile, new JavaScriptSerializer().Serialize(todayDayData));
        }

        public static void SaveAllDaysData(List<DayData> allDaysData)
        {
            File.WriteAllText(AllDaysDataFile, new JavaScriptSerializer().Serialize(allDaysData));
        }

        public static void UpdateAllDaysData(DayData todayDayData)
        {
            List<DayData> allDaysData = LoadAllDaysData();
            allDaysData.Add(todayDayData);
            SaveAllDaysData(allDaysData);
        }
    }
}
