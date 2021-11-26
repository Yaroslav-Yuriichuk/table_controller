﻿using System;
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
using System.Windows.Threading;

namespace Table
{
    /// <summary>
    /// Логика взаимодействия для Notification.xaml
    /// </summary>
    public partial class Notification : Window
    {
        private MainWindow mainWindow;
        private DispatcherTimer closeTimer;
        public Notification(MainWindow mainWindow, string command)
        {
            InitializeComponent();
            this.mainWindow = mainWindow;
            placeNotification();
            NotificationLabel.Content = "Table will go " + command + " in one minute!";
            SetUpTimers();
        }

        private void SetUpTimers()
        {
            closeTimer = new DispatcherTimer();
            closeTimer.Interval = new TimeSpan(0, 0, 25);
            closeTimer.Tick += Accept;
            closeTimer.Start();
        }

        private void placeNotification()
        {
            var desktopWorkingArea = System.Windows.Forms.Screen.PrimaryScreen.WorkingArea;
            this.Left = desktopWorkingArea.Right - this.Width;
            this.Top = desktopWorkingArea.Bottom - this.Height;
        }

        private void Accept_Button_Click(object sender, RoutedEventArgs e)
        {
            Accept(sender, e);
        }

        private void Decline_Button_Click(object sender, RoutedEventArgs e)
        {
            closeTimer.Stop();
            mainWindow.addActionDone("Declined");
            mainWindow.setNotificationNotOpened();
            this.Close();
        }

        private void Accept(object sender, EventArgs e)
        {
            closeTimer.Stop();
            mainWindow.addActionDone("Accepted");
            mainWindow.setNotificationNotOpened();
            this.Close();
        }
    }
}
