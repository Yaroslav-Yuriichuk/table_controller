using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Windows.Forms;
using System.Drawing;
using Microsoft.Win32;
using Stacker.Properties;
using System.Web.Script.Serialization;
using System.IO;

namespace Stacker
{
    public partial class MainWindow : Window
    {
        private DispatcherTimer notificationAboutUpTimer;
        private DispatcherTimer notificationAboutDownTimer;
        private DispatcherTimer moveTableTimer;
        private DispatcherTimer screenTimer;
        private Notification notification;
        private System.Windows.Forms.NotifyIcon ni;
        //private DateTime startTime;
        private Position currentPosition;
        private State currentState;
        private Period currentLoadedPeriod;
        private DateTime lockTime, unLockTime;
        private DateTime timeNotificationIntervalStarted;
        private DayInfo today;
        private List<DayInfo> lastWeekDayInfos;
        private List<DayInfo> lastMonthDayInfos;
        private List<DayInfo> allDaysDayInfos;
        public MainWindow()
        {
            InitializeComponent();
            currentPosition = Position.DOWN;
            currentState = State.NORMAL;
            currentLoadedPeriod = Period.LAST_WEEK;
            lastWeekDayInfos = null;
            lastMonthDayInfos = null;
            allDaysDayInfos = null;
            setUpTimers();
            SetUpIcon();
            placeProgram();
            SystemEvents.SessionSwitch +=
                new SessionSwitchEventHandler(SystemEvents_SessionSwitch);
            //SystemEvents.PowerModeChanged += OnSleepMode;
            loadAllDaysInfo(false);
            loadLastWeekInfo(true);
            loadLastMonthInfo(false);
            ScreenOn.Content = today.ToHoursString();
            Date.Content = "Today: " + DateTime.Now.Date.ToShortDateString();
            displaySettings();
            timeNotificationIntervalStarted = DateTime.Now;
        }

        public void addActionDone(string action)
        {
            Times.Items.Add(action + "  ---  " + DateTime.Now.ToLongTimeString());
        }

        public void addTime()
        {
            Times.Items.Add(DateTime.Now.ToLongTimeString());
        }

        public void setNotificationNotOpened()
        {
            notification = null;
        }

        private void displaySettings()
        {
            TimeUp.Content = "Time in upper position: " + minutesToHoursString(Properties.Settings.Default.timeUp);
            TimeDown.Content = "Time in lower position: " + minutesToHoursString(Properties.Settings.Default.timeDown);
        }

        private void saveSettings()
        {
            Properties.Settings.Default.Save();
        }

        private void handleDaySwitch(bool isSwitchedInSleepMode)
        {
            allDaysDayInfos.Add(today);
            PreviousDays.Items.Add(today.ToFullInfoString());
            Date.Content = "Today: " + DateTime.Now.Date.ToShortDateString();

            if (isSwitchedInSleepMode)
            {
                today = new DayInfo(DateTime.Now.Date.ToShortDateString(), 0);
                ScreenOn.Content = "0h 0m";
            }
            else
            {
                today = new DayInfo(DateTime.Now.Date.ToShortDateString(), -1);
            }

        }

        private void saveDaysInfo()
        {
            const int WEEK_LENGTH = 7;
            const int MONTH_LENGTH = 31;

            File.WriteAllText("allDaysInfo.dat", new JavaScriptSerializer().Serialize(allDaysDayInfos));
            File.WriteAllText("lastWeekInfo.dat", new JavaScriptSerializer().Serialize(
                lastWeekDayInfos.GetRange(Math.Max(lastWeekDayInfos.Count() - WEEK_LENGTH, 0), WEEK_LENGTH)));
            File.WriteAllText("lastMonthInfo.dat", new JavaScriptSerializer().Serialize(
                lastMonthDayInfos.GetRange(Math.Max(lastMonthDayInfos.Count() - MONTH_LENGTH, 0), MONTH_LENGTH)));
        }

        private void loadAllDaysInfo(bool display)
        {
            if (!alreadyLoaded(Period.ALL_DAYS))
            {
                allDaysDayInfos = loadDaysInfo(Period.ALL_DAYS);
                currentLoadedPeriod = Period.ALL_DAYS;
                if (display) displayDaysInfos(allDaysDayInfos);
            }
        }

        private void loadLastWeekInfo(bool display)
        {
            if (!alreadyLoaded(Period.LAST_WEEK))
            {
                lastWeekDayInfos = loadDaysInfo(Period.LAST_WEEK);
                currentLoadedPeriod = Period.LAST_WEEK;
                if (display) displayDaysInfos(lastWeekDayInfos);
            }
        }

        private void loadLastMonthInfo(bool display)
        {
            if (!alreadyLoaded(Period.LAST_MONTH))
            {
                lastMonthDayInfos = loadDaysInfo(Period.LAST_MONTH);
                currentLoadedPeriod = Period.LAST_MONTH;
                if (display) displayDaysInfos(lastMonthDayInfos);
            }
        }

        private bool alreadyLoaded(Period period)
        {
            Dictionary<Period, List<DayInfo>> periodLists = new Dictionary<Period, List<DayInfo>>() {
                { Period.LAST_WEEK, lastWeekDayInfos },
                { Period.LAST_MONTH, lastMonthDayInfos },
                { Period.ALL_DAYS, allDaysDayInfos }
            };

            return periodLists[period] != null;
        }

        private List<DayInfo> loadDaysInfo(Period period)
        {

            Dictionary<Period, string> fileNames = new Dictionary<Period, string>() {
                { Period.LAST_WEEK, "lastWeekInfo.dat" },
                { Period.LAST_MONTH, "lastMonthInfo.dat" },
                { Period.ALL_DAYS, "allDaysInfo.dat" }
            };

            /*Dictionary<Period, double> periodLengthInDays = new Dictionary<Period, double>() {
                { Period.LAST_WEEK, 7 },
                { Period.LAST_MONTH, 31 },
                { Period.ALL_DAYS, double.PositiveInfinity}
            };*/

            List<DayInfo> daysInfos;

            if (File.Exists(fileNames[period]))
            {
                daysInfos
                    = new JavaScriptSerializer().Deserialize<List<DayInfo>>(File.ReadAllText(fileNames[period]));
                // allDaysDayInfos = new JavaScriptSerializer().Deserialize<List<DayInfo>>(File.ReadAllText("daysInfo.dat"));
                if (daysInfos[daysInfos.Count - 1].Date == DateTime.Now.Date.ToShortDateString())
                {
                    today = daysInfos[daysInfos.Count - 1];
                    daysInfos.RemoveAt(daysInfos.Count - 1);
                }
                else
                {
                    today = new DayInfo(DateTime.Now.Date.ToShortDateString(), 0);
                }
            }
            else
            {
                today = new DayInfo(DateTime.Now.Date.ToShortDateString(), 0);
                daysInfos = new List<DayInfo>();
            }

            return daysInfos;
        }

        private void displayDaysInfos(List<DayInfo> daysInfos)
        {
            foreach (DayInfo day in daysInfos)
            {
                PreviousDays.Items.Add(day.ToFullInfoString());
            }
        }

        private void placeProgram()
        {
            var desktopWorkingArea = System.Windows.Forms.Screen.PrimaryScreen.WorkingArea;
            this.Left = desktopWorkingArea.Right - this.Width;
            this.Top = desktopWorkingArea.Bottom - this.Height;
        }

        private string minutesToHoursString(int minutes)
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

        private Position getOppositePositionToCurrent()
        {
            return (currentPosition == Position.UP) ? Position.DOWN : Position.UP;
        }

        private DispatcherTimer getNotitficationTimerforCurrentPosition()
        {
            return (currentPosition == Position.UP) ? notificationAboutDownTimer : notificationAboutUpTimer;
        }

        private int getNotificationIntervalForCurrentPosition()
        {
            return (currentPosition == Position.UP) ? Properties.Settings.Default.timeUp - 1
                : Properties.Settings.Default.timeDown - 1;
        }

        private void sendNotification(string command)
        {
            this.notification = new Notification(this, command);
            notification.Show();
        }

        void setUpTimers()
        {
            moveTableTimer = new DispatcherTimer();
            moveTableTimer.Interval = new TimeSpan(0, 1, 0);
            moveTableTimer.Tick += moveTable;

            notificationAboutDownTimer = new DispatcherTimer();
            notificationAboutDownTimer.Tick += notifyAboutDown;

            notificationAboutUpTimer = new DispatcherTimer();
            notificationAboutUpTimer.Interval = new TimeSpan(0, Properties.Settings.Default.timeDown - 1, 0);
            notificationAboutUpTimer.Tick += notifyAboutUp;
            notificationAboutUpTimer.Start();

            screenTimer = new DispatcherTimer();
            screenTimer.Interval = new TimeSpan(0, 1, 0);
            screenTimer.Tick += screenOnTimerTick;
            screenTimer.Start();
        }

        void SetUpIcon()
        {
            ni = new System.Windows.Forms.NotifyIcon();
            ni.Icon = new System.Drawing.Icon("Table.ico");
            ni.Text = "TableController";
            ni.Visible = true;
            ni.Click += IconClicked;

            ni.ContextMenuStrip = new System.Windows.Forms.ContextMenuStrip();
            ni.ContextMenuStrip.Items.Add("Open", System.Drawing.Image.FromFile("Table.ico"), IconOpenClicked);
            ni.ContextMenuStrip.Items.Add("Close", System.Drawing.Image.FromFile("Table.ico"), IconCloseClicked);
        }

        private void IconCloseClicked(object sender, EventArgs e)
        {
            this.Close();
        }

        private void IconOpenClicked(object sender, EventArgs e)
        {
            this.Show();
            this.WindowState = WindowState.Normal;
        }

        private void IconClicked(object sender, EventArgs e)
        {
            System.Windows.Forms.MouseEventArgs mouseEvent = (System.Windows.Forms.MouseEventArgs) e;
            if (mouseEvent.Button != System.Windows.Forms.MouseButtons.Right)
            {
                this.Show();
                this.WindowState = WindowState.Normal;
            }
        }

        private void CloseButtonClick(object sender, EventArgs e)
        {
            this.Hide();
        }

        private void btnIncTimeUp_Click(object sender, RoutedEventArgs e)
        {
            if (Properties.Settings.Default.timeUp < Properties.Settings.Default.maxTimeUp)
            {
                Properties.Settings.Default.timeUp += 1;
                saveSettings();
                displaySettings();

                if (currentState == State.NORMAL && currentPosition == Position.UP)
                {
                    double elapsedTimeInMilliseconds = 
                        (DateTime.Now - timeNotificationIntervalStarted).TotalMilliseconds;

                    notificationAboutDownTimer.Stop();

                    int remainingMilliseconds = (Properties.Settings.Default.timeUp - 1) * 60 * 1000
                        - (int)elapsedTimeInMilliseconds;

                    notificationAboutDownTimer.Interval = new TimeSpan(0, 0, 0, 0, remainingMilliseconds);

                    notificationAboutDownTimer.Start();
                }
            }
        }

        private void btnDecTimeUp_Click(object sender, RoutedEventArgs e)
        {
            if (Properties.Settings.Default.timeUp > Properties.Settings.Default.minTimeUp)
            {
                Properties.Settings.Default.timeUp -= 1;
                saveSettings();
                displaySettings();

                if (currentState == State.NORMAL && currentPosition == Position.UP)
                {
                    double elapsedTimeInMilliseconds =
                        (DateTime.Now - timeNotificationIntervalStarted).TotalMilliseconds;

                    notificationAboutDownTimer.Stop();

                    int remainingMilliseconds = (Properties.Settings.Default.timeUp - 1) * 60 * 1000
                        - (int)elapsedTimeInMilliseconds;

                    if (remainingMilliseconds > 0)
                    {
                        notificationAboutDownTimer.Interval = new TimeSpan(0, 0, 0, 0, remainingMilliseconds);

                        notificationAboutDownTimer.Start();
                    }
                    else
                    {
                        notifyAboutDown(sender, e);
                    }
                }
            }
        }

        private void btnIncTimeDown_Click(object sender, RoutedEventArgs e)
        {
            if (Properties.Settings.Default.timeDown < Properties.Settings.Default.maxTimeDown)
            {
                Properties.Settings.Default.timeDown += 1;
                saveSettings();
                displaySettings();

                if (currentPosition == Position.DOWN && currentState == State.NORMAL)
                {
                    double elapsedTimeInMilliseconds =
                        (DateTime.Now - timeNotificationIntervalStarted).TotalMilliseconds;

                    notificationAboutUpTimer.Stop();

                    int remainingMilliseconds = (Properties.Settings.Default.timeDown - 1) * 60 * 1000
                        - (int)elapsedTimeInMilliseconds;

                    notificationAboutDownTimer.Interval = new TimeSpan(0, 0, 0, 0, remainingMilliseconds);

                    notificationAboutUpTimer.Start();
                }
            }
        }

        private void btnDecTimeDown_Click(object sender, RoutedEventArgs e)
        {
            if (Properties.Settings.Default.timeDown > Properties.Settings.Default.minTimeDown)
            {
                Properties.Settings.Default.timeDown -= 1;
                saveSettings();
                displaySettings();

                if (currentState == State.NORMAL && currentPosition == Position.DOWN)
                {
                    double elapsedTimeInMilliseconds =
                        (DateTime.Now - timeNotificationIntervalStarted).TotalMilliseconds;

                    notificationAboutUpTimer.Stop();

                    int remainingMilliseconds = (Properties.Settings.Default.timeDown - 1) * 60 * 1000
                        - (int)elapsedTimeInMilliseconds;

                    if (remainingMilliseconds > 0)
                    {
                        notificationAboutUpTimer.Interval = new TimeSpan(0, 0, 0, 0, remainingMilliseconds);
                        notificationAboutUpTimer.Start();
                    } 
                    else
                    {
                        notifyAboutUp(sender, e);
                    }
                }
            }
        }

        protected override void OnStateChanged(EventArgs e)
        {
            if (WindowState == System.Windows.WindowState.Minimized)
                this.Hide();

            base.OnStateChanged(e);
        }

        protected override void OnClosing(CancelEventArgs e)
        {

            if (notification != null)
            {
                notification.Close();
            }
            this.ni.Dispose();
            allDaysDayInfos.Add(today);
            lastWeekDayInfos.Add(today);
            lastMonthDayInfos.Add(today);
            saveDaysInfo();            

        }

        private void notifyAboutUp(object sender, EventArgs e)
        {
            sendNotification(getOppositePositionToCurrent().ToString());
            notificationAboutUpTimer.Stop();
            moveTableTimer.Start();
            currentState = State.WAITING_FOR_ACCEPTANCE;
        }

        private void notifyAboutDown(object sender, EventArgs e)
        {
            sendNotification(getOppositePositionToCurrent().ToString());
            notificationAboutDownTimer.Stop();
            moveTableTimer.Start();
            currentState = State.WAITING_FOR_ACCEPTANCE;
        }

        private void screenOnTimerTick(object sender, EventArgs e)
        {
            if (DateTime.Now.Date.ToShortDateString() != today.Date)
            {
                handleDaySwitch(false);
            }
            today.MinutesScreenOn += 1;
            ScreenOn.Content = today.ToHoursString();
        }

        private void moveTable(object sender, EventArgs e)
        {
            moveTableTimer.Stop();
            currentPosition = getOppositePositionToCurrent();
            Times.Items.Add("Table moved " + currentPosition.ToString());
            sendNotification("crazy");
            DispatcherTimer notificationTimer = getNotitficationTimerforCurrentPosition();
            int intervalInMinutes = getNotificationIntervalForCurrentPosition();
            notificationTimer.Interval = new TimeSpan(0, intervalInMinutes, 0);
            notificationTimer.Start();
            timeNotificationIntervalStarted = DateTime.Now;
            currentState = State.NORMAL;
        }

        private void SystemEvents_SessionSwitch(object sender, SessionSwitchEventArgs e)
        {
            switch (e.Reason)
            {
                case SessionSwitchReason.SessionLock:
                    lockTime = DateTime.Now;
                    screenTimer.Stop();
                    notificationAboutUpTimer.Stop();
                    notificationAboutDownTimer.Stop();
                    moveTableTimer.Stop();
                    break;
                case SessionSwitchReason.SessionUnlock:
                    unLockTime = DateTime.Now;
                    Times.Items.Add(lockTime.ToLongTimeString()
                        + " - "
                        + unLockTime.ToLongTimeString());
                    if (DateTime.Now.Date.ToShortDateString() != today.Date)
                    {
                        handleDaySwitch(true);
                    }
                    screenTimer.Start();
                    currentPosition = Position.DOWN;
                    notificationAboutUpTimer.Interval = new TimeSpan(0, Properties.Settings.Default.timeDown, 0);
                    notificationAboutUpTimer.Start();
                    timeNotificationIntervalStarted = DateTime.Now;
                    break;
            }
        }


        /*void OnSleepMode(Object sender, PowerModeChangedEventArgs e)
        {
            if (e.Mode == PowerModes.Suspend)
            {
                this.lockTime = DateTime.Now;
            }
            else if (e.Mode == PowerModes.Resume)
            {
                this.unLockTime = DateTime.Now;
                Times.Items.Add(this.lockTime.ToLongTimeString()
                    + " - "
                    + this.unLockTime.ToLongTimeString());
            }
        }*/
    }
}
