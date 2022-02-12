using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Stacker.Timers;

namespace Stacker.ApplicationWindows
{
    public partial class Notification : Window
    {
        public static Action OnSnooze;

        private SingleTickTimer closeTimer;

        #region WINDOW_METHODS

        public Notification(string message, bool isMainWindowOpened)
        {
            InitializeComponent();
            SetUpTimers();
            Subscribe();
            Message.Content = message;
            PlaceNotification(isMainWindowOpened);
            closeTimer.Start();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            Unsubscribe();
            base.OnClosing(e);
        }

        #endregion

        #region EVENTS

        private void Subscribe()
        {
            MainWindow.OnClose += CloseNotification;
        }

        private void Unsubscribe()
        {
            MainWindow.OnClose -= CloseNotification;
        }

        #endregion

        #region SET_UP

        private void SetUpTimers()
        {
            closeTimer = new SingleTickTimer(CloseNotification, new TimeSpan(0, 0, 20));
        }

        #endregion

        #region BUTTON_FUNCTIONS

        private void AcceptButtonClick(object sender, RoutedEventArgs e)
        {
            Accept();
        }

        private void SnoozeButtonClick(object sender, RoutedEventArgs e)
        {
            Snooze();
        }

        #endregion

        #region ACTIONS

        private void Accept()
        {
            CloseNotification();
        }

        private void Snooze()
        {
            OnSnooze?.Invoke();
            CloseNotification();
        }

        private void CloseNotification()
        {
            this.Close();
        }

        #endregion

        #region UI

        private void PlaceNotification(bool isMainWindowOpened)
        {
            int Margin = isMainWindowOpened ? 15 : 10;

            var desktopWorkingArea = System.Windows.SystemParameters.WorkArea;
            this.Left = desktopWorkingArea.Right - this.Width - Margin;
            this.Top = desktopWorkingArea.Bottom - this.Height - Margin;
        }

        #endregion
    }
}
