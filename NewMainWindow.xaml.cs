using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;
using System.Windows.Markup;
using System.Windows.Threading;
using System.Windows.Forms;
using System.ComponentModel;
using System.IO;
using System.Web.Script.Serialization;
using Table;
using Table.Enums;

namespace Table
{
    /// <summary>
    /// Логика взаимодействия для NewMainWindow.xaml
    /// </summary>
    public partial class NewMainWindow : Window
    {
        #region CONSTANTS
        private const int MaxTimeUp = 120;
        private const int MaxTimeDown = 120;
        private const int MinTimeUp = 2;
        private const int MinTimeDown = 2;
        #endregion

        #region EVENTS

        public static Action OnClose;

        #endregion

        private bool isHeightAdjustWindowOpened = false;

        private DispatcherTimer notificationTimer;
        private DispatcherTimer moveTableTimer;
        private DispatcherTimer dayInfoTimer;

        private System.Windows.Forms.NotifyIcon ni;

        private DayModeInfo today;
        private Position currentPosition;
        private State currentState;
        private DateTime timeNotificationIntervalStarted;

        private List<Desk> foundDesks;
        private Desk connectedDesk;

        #region WINDOW_METHODS

        public NewMainWindow()
        {
            InitializeComponent();
            currentPosition = Position.DOWN;
            currentState = State.NORMAL;
            SetUpTimers();
            SetUpIcon();
            today = LoadTodayInfo();
            OnClose += SaveDaysInfo;
            HeightAdjust.OnClose += SetHeightAdjustNotOpened;
            UpdateTimeLabels();
            UpdateIntervalLabels();
            foundDesks = new List<Desk>();
            // PlaceProgram();
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);

            this.DragMove();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            OnClose?.Invoke();
            HeightAdjust.OnClose -= SetHeightAdjustNotOpened;
            base.OnClosing(e);
        }

        #endregion

        #region XAML
        private System.Windows.Controls.Button CreateDeskTemplate(string xName,string name)
        {
            System.Windows.Controls.Button desk = new System.Windows.Controls.Button();

            desk.Name = xName;
            desk.MouseDoubleClick += Connect;
            desk.VerticalAlignment = VerticalAlignment.Top;
            desk.Margin = new Thickness(0, 3, 0, 0);

            Style style = System.Windows.Application.Current.FindResource("DeskButtonStyle") as Style;

            string template = "<ControlTemplate xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation' TargetType=\"Button\"> " +
                                    "<Border Background = \"{TemplateBinding Background}\" Width = \"160\" Height = \"30\" CornerRadius = \"15\">" +
                                            "<Grid>" +
                                                "<Grid.ColumnDefinitions>" +
                                                    "<ColumnDefinition></ColumnDefinition>" +
                                                    "<ColumnDefinition Width = \"25\"></ColumnDefinition>" +
                                                "</Grid.ColumnDefinitions>" +
                                               $"<Label Content = \"{name}\" FontSize = \"14\" FontWeight = \"DemiBold\" VerticalAlignment = \"Top\" Margin = \"10 0 0 0\" />" +
                                                "<Border Grid.Column = \"1\" Height = \"15\" Width = \"15\" Background = \"#23DA36\" CornerRadius = \"7.5\" />" + 
                                            "</Grid>" +
                                    "</Border>" +
                              "</ControlTemplate> ";


            desk.Style = style;
            desk.Template = (ControlTemplate) XamlReader.Parse(template);

            Grid.SetRow(desk, DesksList.Children.Count);

            return desk;
        }

        private void AddDeskToList()
        {
            DesksList.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(40) });
            System.Windows.Controls.Button newDesk = CreateDeskTemplate($"desk{DesksList.Children.Count}",
                $"DESK_{DesksList.Children.Count}");
            foundDesks.Add(new Desk(newDesk, DeskConnectionState.NOT_CONNECTED));
            DesksList.Children.Add(newDesk);
        }

        private void PlaceProgram()
        {
            var desktopWorkingArea = System.Windows.Forms.Screen.PrimaryScreen.WorkingArea;
            this.Left = desktopWorkingArea.Right - this.Width - 10;
            this.Top = desktopWorkingArea.Bottom - this.Height - 10;
        }

        private void UpdateTimeLabels()
        {
            TodaySitModeLabel.Content = Util.MinutesToHoursString(today.MinutesInSitMode);
            TodayStayModeLabel.Content = Util.MinutesToHoursString(today.MinutesInStayMode);
        }

        private void UpdateIntervalLabels()
        {
            SitModeIntervalLabel.Content = Util.MinutesToHoursString(Properties.Settings.Default.UserTimeDown);
            StayModeIntervalLabel.Content = Util.MinutesToHoursString(Properties.Settings.Default.UserTimeUp);
        }

        #endregion

        #region DAYS_INFO

        private List<DayModeInfo> LoadAllDaysModeInfo()
        {
            List<DayModeInfo> daysModeInfos = new List<DayModeInfo>();

            if (File.Exists("AllDaysModeInfo.dat"))
            {
                daysModeInfos
                    = new JavaScriptSerializer().Deserialize<List<DayModeInfo>>(File.ReadAllText("AllDaysModeInfo.dat"));
            }

            return daysModeInfos;
        }

        private DayModeInfo LoadTodayInfo()
        {
            DayModeInfo todayInfo = new DayModeInfo(DateTime.Now, 0, 0);

            // Maybe program already been opened today and closed
            if (File.Exists("TodayModeInfo.dat"))
            {
                DayModeInfo tmpTodayInfo
                    = new JavaScriptSerializer().Deserialize<DayModeInfo>(File.ReadAllText("TodayModeInfo.dat"));
                if (tmpTodayInfo.Date.Date == DateTime.Now.Date)
                {
                    todayInfo = tmpTodayInfo;
                }
            }

            return todayInfo;
        }

        private void SaveDaysInfo()
        {
            File.WriteAllText("AllDaysModeInfo.dat", new JavaScriptSerializer().Serialize(
                LoadAllDaysModeInfo()));
            File.WriteAllText("TodayModeInfo.dat", new JavaScriptSerializer().Serialize(today));
        }

        #endregion

        #region USER_SETTINGS

        private void SaveSettings()
        {
            Properties.Settings.Default.Save();
        }

        #endregion

        #region GENERAL_BUTTONS

        private void RefreshDesks(object sender, RoutedEventArgs e)
        {
            AddDeskToList();
        }

        private void Connect(object sender, RoutedEventArgs e)
        {
            Console.WriteLine(((System.Windows.Controls.Button)sender).Name);
        }

        private void OpenHeightAdjustWindow(object sender, RoutedEventArgs e)
        {
            if (!isHeightAdjustWindowOpened)
            {
                isHeightAdjustWindowOpened = true;
                HeightAdjust heightAdjust = new HeightAdjust();
                heightAdjust.Show();
            }
        }

        #endregion

        #region TIMERS_BUTTONS

        private void IncrementTimeUp(object sender, RoutedEventArgs e)
        {
            if (Properties.Settings.Default.UserTimeUp < MaxTimeUp)
            {
                Properties.Settings.Default.UserTimeUp += 1;
                SaveSettings();
                UpdateIntervalLabels();

                if (currentState == State.NORMAL && currentPosition == Position.UP)
                {
                    notificationTimer.Stop();

                    double elapsedTimeInMilliseconds = 
                        (DateTime.Now - timeNotificationIntervalStarted).TotalMilliseconds;

                    int remainingTimeInMilliseconds = (Properties.Settings.Default.UserTimeUp - 1) * 60 * 1000
                        - (int)elapsedTimeInMilliseconds;

                    notificationTimer.Interval = new TimeSpan(0, 0, 0, 0, remainingTimeInMilliseconds);
                    notificationTimer.Start();
                }
            }
        }

        private void DecrementTimeUp(object sender, RoutedEventArgs e)
        {
            if (Properties.Settings.Default.UserTimeUp > MinTimeUp)
            {
                Properties.Settings.Default.UserTimeUp -= 1;
                SaveSettings();
                UpdateIntervalLabels();

                if (currentState == State.NORMAL && currentPosition == Position.UP)
                {
                    notificationTimer.Stop();

                    double elapsedTimeInMilliseconds =
                        (DateTime.Now - timeNotificationIntervalStarted).TotalMilliseconds;

                    int remainingTimeInMilliseconds = (Properties.Settings.Default.UserTimeUp - 1) * 60 * 1000
                        - (int)elapsedTimeInMilliseconds;

                    if (remainingTimeInMilliseconds > 0)
                    {
                        notificationTimer.Interval = new TimeSpan(0, 0, 0, 0, remainingTimeInMilliseconds);
                        notificationTimer.Start();
                    }
                    else
                    {
                        NotifyAboutDown(sender, e);
                    }
                }
            }
        }

        private void IncrementTimeDown(object sender, RoutedEventArgs e)
        {
            if (Properties.Settings.Default.UserTimeDown < MaxTimeDown)
            {
                Properties.Settings.Default.UserTimeDown += 1;
                SaveSettings();
                UpdateIntervalLabels();

                if (currentPosition == Position.DOWN && currentState == State.NORMAL)
                {
                    notificationTimer.Stop();

                    double elapsedTimeInMilliseconds =
                        (DateTime.Now - timeNotificationIntervalStarted).TotalMilliseconds;

                    int remainingTimeInMilliseconds = (Properties.Settings.Default.UserTimeDown - 1) * 60 * 1000
                        - (int)elapsedTimeInMilliseconds;

                    notificationTimer.Interval = new TimeSpan(0, 0, 0, 0, remainingTimeInMilliseconds);
                    notificationTimer.Start();
                }
            }
        }

        private void DecrementTimeDown(object sender, RoutedEventArgs e)
        {
            if (Properties.Settings.Default.UserTimeDown > MinTimeDown)
            {
                Properties.Settings.Default.UserTimeDown -= 1;
                SaveSettings();
                UpdateIntervalLabels();

                if (currentState == State.NORMAL && currentPosition == Position.DOWN)
                {
                    notificationTimer.Stop();

                    double elapsedTimeInMilliseconds =
                        (DateTime.Now - timeNotificationIntervalStarted).TotalMilliseconds;

                    int remainingTimeInMilliseconds = (Properties.Settings.Default.UserTimeDown - 1) * 60 * 1000
                        - (int)elapsedTimeInMilliseconds;

                    if (remainingTimeInMilliseconds > 0)
                    {
                        notificationTimer.Interval = new TimeSpan(0, 0, 0, 0, remainingTimeInMilliseconds);
                        notificationTimer.Start();
                    }
                    else
                    {
                        NotifyAboutUp(sender, e);
                    }
                }
            }
        }

        #endregion

        #region ICON_BUTTONS

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
            System.Windows.Forms.MouseEventArgs mouseEvent = (System.Windows.Forms.MouseEventArgs)e;
            if (mouseEvent.Button != System.Windows.Forms.MouseButtons.Right)
            {
                // Toggle Window
                if (this.WindowState == WindowState.Minimized)
                {
                    this.Show();
                    this.WindowState = WindowState.Normal;
                }
                else
                {
                    this.Hide();
                    this.WindowState = WindowState.Minimized;
                }
            }
        }

        #endregion

        #region SET_UP

        private void SetUpTimers()
        {
            dayInfoTimer = new DispatcherTimer();
            dayInfoTimer.Interval = new TimeSpan(0, 1, 0);
            dayInfoTimer.Tick += DayInfoTimerTick;
            dayInfoTimer.Start();

            moveTableTimer = new DispatcherTimer();
            moveTableTimer.Interval = new TimeSpan(0, 1, 0);
            moveTableTimer.Tick += MoveTable;

            notificationTimer = new DispatcherTimer();
            notificationTimer.Interval = new TimeSpan(0, Properties.Settings.Default.UserTimeDown - 1, 0);
            notificationTimer.Tick += NotifyAboutUp;
            StartNotificationTimer();
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

        #endregion

        #region TIMERS_FUNCTIONS

        private void NotifyAboutUp(object sender, EventArgs e)
        {
            SendNotification("Table will go Up in 1 minute!");
            notificationTimer.Stop();
            moveTableTimer.Start();
            currentState = State.WAITING_FOR_ACCEPTANCE;
        }

        private void NotifyAboutDown(object sender, EventArgs e)
        {
            SendNotification("Table will go DOWN in 1 minute!");
            notificationTimer.Stop();
            moveTableTimer.Start();
            currentState = State.WAITING_FOR_ACCEPTANCE;
        }

        private void MoveTable(object sender, EventArgs e)
        {
            moveTableTimer.Stop();
            Position nextPosition = GetOppositePositionToCurrent();
            SendNotification("Table moved " + nextPosition.ToString());
            UpdateNotificationTimer(nextPosition);
            currentPosition = nextPosition;
            currentState = State.NORMAL;
            StartNotificationTimer();
        }

        private void DayInfoTimerTick(object sender, EventArgs e)
        {
            if (currentPosition == Position.DOWN)
            {
                today.MinutesInSitMode += 1;
            }
            else
            {
                today.MinutesInStayMode += 1;
            }
            UpdateTimeLabels();
        }

        #endregion

        #region TIMERS_UTIL

        private void StartNotificationTimer()
        {
            notificationTimer.Start();
            timeNotificationIntervalStarted = DateTime.Now;
        }

        // Updates nitification timer when table is about to change position
        private void UpdateNotificationTimer(Position nextPosition)
        {
            if (nextPosition == Position.UP)
            {
                notificationTimer.Interval = new TimeSpan(0, Properties.Settings.Default.UserTimeUp - 1, 0);
                notificationTimer.Tick -= NotifyAboutUp;
                notificationTimer.Tick += NotifyAboutDown;
            }
            else
            {
                notificationTimer.Interval = new TimeSpan(0, Properties.Settings.Default.UserTimeDown - 1, 0);
                notificationTimer.Tick -= NotifyAboutDown;
                notificationTimer.Tick += NotifyAboutUp;
            }
        }

        #endregion

        #region POSITIONS_UTIL

        private Position GetOppositePositionToCurrent()
        {
            return (currentPosition == Position.DOWN) ? Position.UP : Position.DOWN;
        }

        #endregion

        #region NOTIFICATION

        private void SendNotification(string command)
        {
            new Notification(this, command).Show();
        }

        #endregion

        #region HELPER_FUNCTIONS

        private void SetHeightAdjustNotOpened()
        {
            isHeightAdjustWindowOpened = false;
        }

        #endregion

        #region BLUETOOTH

        #endregion
    }
}
