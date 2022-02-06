using System;
using System.Windows;
using System.Windows.Controls;
using System.ComponentModel;
using Stacker.Enums;
using Windows.Devices.Enumeration;
using Stacker.ApplicationWindows;
using Stacker.Controllers;

namespace Stacker
{
    public partial class NewMainWindow : Window
    {
        #region EVENTS

        public static Action OnClose;

        #endregion

        private bool isHeightAdjustWindowOpened = false;

        private System.Windows.Forms.NotifyIcon ni;

        #region BLUETOOTH_FIELDS

        private int generatedDeviceId = 0;

        #endregion

        #region WINDOW_METHODS

        public NewMainWindow()
        {
            InitializeComponent();
            Subscribe();
            UpdateTimeLabels();
            this.WindowState = WindowState.Normal;
            SetUpIcon();
            UpdateIntervalLabels();
            PlaceWindow();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            OnClose?.Invoke();
            ni.Dispose();
            Unsubscribe();
            StackerController.Instance.Unsubscribe();
            base.OnClosing(e);
        }

        #endregion

        #region EVENTS

        private void Subscribe()
        {
            HeightAdjust.OnClose += SetHeightAdjustNotOpened;
            StackerController.Instance.OnDeviceAdded += AddDeskToList;
            StackerController.Instance.OnNotificationToBeSent += SendNotification;
            StackerController.Instance.OnTodayDayDataChanged += UpdateTimeLabels;
        }

        private void Unsubscribe()
        {
            HeightAdjust.OnClose -= SetHeightAdjustNotOpened;
            StackerController.Instance.OnDeviceAdded -= AddDeskToList;
            StackerController.Instance.OnNotificationToBeSent -= SendNotification;
            StackerController.Instance.OnTodayDayDataChanged -= UpdateTimeLabels;
        }

        #endregion

        #region XAML

        private void AddDeskToList(DeviceInformation device)
        {
            Console.WriteLine(device.Name);
            this.Dispatcher.Invoke(() =>
            {
                Tuple<System.Windows.Controls.Button, Border> newDeskUI = XAMLUtil.CreateDeskTemplate(
                    $"desk{generatedDeviceId++}", device.Name, DesksList, Connect);
                StackerController.Instance.FoundDesks.Add(
                    new Desk(newDeskUI.Item1, newDeskUI.Item2, device, DeskConnectionState.NOT_CONNECTED));

                DesksList.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(40) });
                DesksList.Children.Add(newDeskUI.Item1);
            });
        }

        private void PlaceWindow()
        {
            const int Margin = 10;
            var desktopWorkingArea = System.Windows.Forms.Screen.PrimaryScreen.WorkingArea;
            this.Left = desktopWorkingArea.Right - this.Width - Margin;
            this.Top = desktopWorkingArea.Bottom - this.Height - Margin;
        }

        private void UpdateTimeLabels()
        {
            TodaySitModeLabel.Content = Util.MinutesToHoursString(
                StackerController.Instance.Today.MinutesInSitMode);
            TodayStayModeLabel.Content = Util.MinutesToHoursString(
                StackerController.Instance.Today.MinutesInStayMode);
        }

        private void UpdateIntervalLabels()
        {
            SitModeIntervalLabel.Content = Util.MinutesToHoursString(
                Properties.Settings.Default.UserTimeDown);
            StayModeIntervalLabel.Content = Util.MinutesToHoursString(
                Properties.Settings.Default.UserTimeUp);
        }

        #endregion

        #region GENERAL_BUTTONS

        private void RefreshDesks(object sender, RoutedEventArgs e)
        {
            
        }

        private void Connect(object sender, RoutedEventArgs e)
        {
            StackerController.Instance.Connect(((System.Windows.Controls.Button)sender).Name);
        }

        private void OpenHeightAdjustWindow(object sender, RoutedEventArgs e)
        {
            if (!isHeightAdjustWindowOpened)
            {
                new HeightAdjust().Show();
                isHeightAdjustWindowOpened = true;
            }
        }

        #endregion

        #region TIMERS_BUTTONS

        private void IncrementTimeUp(object sender, RoutedEventArgs e)
        {
            StackerController.Instance.IncrementTimeUp();
            UpdateIntervalLabels();
        }

        private void DecrementTimeUp(object sender, RoutedEventArgs e)
        {
            StackerController.Instance.DecrementTimeUp();
            UpdateIntervalLabels();
        }

        private void IncrementTimeDown(object sender, RoutedEventArgs e)
        {
            StackerController.Instance.IncrementTimeDown();
            UpdateIntervalLabels();
        }

        private void DecrementTimeDown(object sender, RoutedEventArgs e)
        {
            StackerController.Instance.DecrementTimeDown();
            UpdateIntervalLabels();
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

        void SetUpIcon()
        {
            ni = new System.Windows.Forms.NotifyIcon();
            ni.Icon = new System.Drawing.Icon("Table.ico");
            ni.Text = "Stacker";
            ni.Visible = true;
            ni.Click += IconClicked;

            ni.ContextMenuStrip = new System.Windows.Forms.ContextMenuStrip();
            ni.ContextMenuStrip.Items.Add("Open", System.Drawing.Image.FromFile("Table.ico"), IconOpenClicked);
            ni.ContextMenuStrip.Items.Add("Close", System.Drawing.Image.FromFile("Table.ico"), IconCloseClicked);
        }

        #endregion

        #region NOTIFICATION

        private void SendNotification(string message)
        {
            new NewNotification(message, this.WindowState == WindowState.Normal).Show();
        }

        #endregion

        #region HELPER_FUNCTIONS

        private void SetHeightAdjustNotOpened()
        {
            isHeightAdjustWindowOpened = false;
        }

        #endregion

        #region BLUETOOTH

        private void MoveTableUpButtonPressed(object sender, RoutedEventArgs e)
        {
            StackerController.Instance.ConnectedDevice?.StartMovingTableUp();
        }

        private void MoveTableDownButtonPressed(object sender, RoutedEventArgs e)
        {
            StackerController.Instance.ConnectedDevice?.StartMovingTableDown();
        }

        #endregion
    }
}
