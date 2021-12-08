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
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Stacker.ApplicationWindows
{
    /// <summary>
    /// Логика взаимодействия для NewNotification.xaml
    /// </summary>
    public partial class NewNotification : Window
    {
        private DispatcherTimer closeTimer;

        #region WINDOW_METHODS

        public NewNotification(string message, bool isMainWindowOpened)
        {
            InitializeComponent();
            NewMainWindow.OnClose += CloseNotification;
            SetUpTimers();
            Message.Content = message;
            PlaceNotification(isMainWindowOpened);
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            NewMainWindow.OnClose -= CloseNotification;
            base.OnClosing(e);
        }

        #endregion

        #region SET_UP

        private void SetUpTimers()
        {
            closeTimer = new DispatcherTimer();
            closeTimer.Interval = new TimeSpan(0, 0, 25);
            closeTimer.Tick += Accept;
            closeTimer.Start();
        }

        #endregion

        #region BUTTON_FUNCTIONS

        private void AcceptButtonClick(object sender, RoutedEventArgs e)
        {
            Accept(sender, e);
        }

        #endregion

        #region ACTIONS

        private void Accept(object sender, EventArgs e)
        {
            closeTimer.Stop();
            CloseNotification();
        }

        #endregion

        #region XAML

        private void PlaceNotification(bool isMainWindowOpened)
        {
            int Margin = isMainWindowOpened ? 15 : 10;
            var desktopWorkingArea = System.Windows.Forms.Screen.PrimaryScreen.WorkingArea;
            this.Left = desktopWorkingArea.Right - this.Width - Margin;
            this.Top = desktopWorkingArea.Bottom - this.Height - Margin;
        }

        #endregion

        private void CloseNotification()
        {
            this.Close();
        }
    }
}
