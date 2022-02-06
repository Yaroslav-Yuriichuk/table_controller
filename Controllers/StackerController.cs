using Stacker.Bluetooth;
using Stacker.Timers;
using System;
using System.Collections.Generic;
using Stacker.DaysData;
using Windows.Devices.Enumeration;
using Microsoft.Win32;

namespace Stacker.Controllers
{
    public class StackerController
    {
        #region SHITTY_SINGLETON

        private static StackerController instance;

        public static StackerController Instance
        {
            get => instance ??= new StackerController();
        }

        #endregion

        #region CONSTANTS

        private const int MaxTimeUp = 120;
        private const int MaxTimeDown = 120;
        private const int MinTimeUp = 2;
        private const int MinTimeDown = 2;

        private const int MinutesAfterNotificationBeforeMoving = 1;
        private const int SNOOZE_TIME = 5;

        #endregion

        #region EVENTS

        public Action OnTodayDayDataChanged;
        public Action<string> OnNotificationToBeSent;
        public Action<DeviceInformation> OnDeviceAdded;
        #endregion

        public Position CurrentPosition { get; private set; } = Position.DOWN;
        public State CurrentState { get; private set; } = State.NORMAL;

        private DayData today;
        public DayData Today
        {
            get
            {
                if (today.Date.Date != DateTime.Now.Date)
                {
                    DayDataSaver.UpdateAllDaysData(today);
                    today = new DayData(DateTime.Now, 0, 0);
                }
                return today;
            }
            private set => today = value;
        }

        private AlternatingTicksTimer notificationTimer;
        private SingleTickTimer moveTableAfterNotificationTimer;
        //private DispatcherTimer snoozeTimer;
        private DayDataCounter dayDataCounter;

        public List<Desk> FoundDesks { get; private set; } = new List<Desk>();

        public ConnectedDevice ConnectedDevice { get; private set; }
        public DeviceScanner DeviceScanner { get; private set; }

        private StackerController()
        {
            Today = DayDataSaver.LoadTodayDayData();
            SetUpTimers();
            dayDataCounter = new DayDataCounter();
            DeviceScanner = new DeviceScanner();
            Subscribe();
            DeviceScanner.StartScanning();
            dayDataCounter.Start();
            notificationTimer.Start();
        }

        #region EVENTS

        private void Subscribe()
        {
            DeviceScanner.OnDeviceAdded += AddDevice;
            SystemEvents.SessionSwitch +=
                new SessionSwitchEventHandler(SystemEvents_SessionSwitch);
        }

        public void Unsubscribe()
        {
            DeviceScanner.OnDeviceAdded -= AddDevice;
            SystemEvents.SessionSwitch -=
                new SessionSwitchEventHandler(SystemEvents_SessionSwitch);
        }

        #endregion

        #region USER_SETTINGS

        private void SaveSettings()
        {
            Properties.Settings.Default.Save();
        }

        #endregion

        #region SET_UP

        private void SetUpTimers()
        {
            moveTableAfterNotificationTimer = new SingleTickTimer(MoveTable,
                new TimeSpan(0, MinutesAfterNotificationBeforeMoving, 0));

            notificationTimer = new AlternatingTicksTimer(
                NotifyAboutUp, new TimeSpan(0, Properties.Settings.Default.UserTimeDown - 1, 0),
                NotifyAboutDown, new TimeSpan(0, Properties.Settings.Default.UserTimeUp - 1, 0));
        }

        #endregion

        #region TIMERS_FUNCTIONS

        private void NotifyAboutUp()
        {
            SendNotification("Table will go Up in 1 minute!");
            moveTableAfterNotificationTimer.Start();
            CurrentState = State.WAITING_FOR_ACCEPTANCE;
        }

        private void NotifyAboutDown()
        {
            SendNotification("Table will go DOWN in 1 minute!");
            moveTableAfterNotificationTimer.Start();
            CurrentState = State.WAITING_FOR_ACCEPTANCE;
        }

        private void MoveTable()
        {
            Position nextPosition = Util.OppositePosition(CurrentPosition);
            switch (nextPosition)
            {
                case Position.UP:
                    ConnectedDevice?.StartMovingTableUp();
                    break;
                case Position.DOWN:
                    ConnectedDevice?.StartMovingTableDown();
                    break;
            }
            //SendNotification("Table moved " + nextPosition.ToString());
            CurrentPosition = nextPosition;
            CurrentState = State.NORMAL;
            notificationTimer.Start();
        }

        #endregion

        #region TIMERS_MODIFICATIONS

        public void IncrementTimeUp()
        {
            if (Properties.Settings.Default.UserTimeUp < MaxTimeUp)
            {
                Properties.Settings.Default.UserTimeUp += 1;
                SaveSettings();
                notificationTimer.UpdateSecondInterval(
                    new TimeSpan(0, Properties.Settings.Default.UserTimeUp - 1, 0));
            }
        }

        public void DecrementTimeUp()
        {
            if (Properties.Settings.Default.UserTimeUp > MinTimeUp)
            {
                Properties.Settings.Default.UserTimeUp -= 1;
                SaveSettings();
                notificationTimer.UpdateSecondInterval(
                    new TimeSpan(0, Properties.Settings.Default.UserTimeUp - 1, 0));
            }
        }

        public void IncrementTimeDown()
        {
            if (Properties.Settings.Default.UserTimeDown < MaxTimeDown)
            {
                Properties.Settings.Default.UserTimeDown += 1;
                SaveSettings();
                notificationTimer.UpdateFirstInterval(
                    new TimeSpan(0, Properties.Settings.Default.UserTimeDown - 1, 0));
            }
        }

        public void DecrementTimeDown()
        {
            if (Properties.Settings.Default.UserTimeDown > MinTimeDown)
            {
                Properties.Settings.Default.UserTimeDown -= 1;
                SaveSettings();
                notificationTimer.UpdateFirstInterval(
                    new TimeSpan(0, Properties.Settings.Default.UserTimeDown - 1, 0));
            }
        }

        #endregion

        #region TIMER_UTIL

        private void StopAllTimers()
        {
            notificationTimer.Stop();
            moveTableAfterNotificationTimer.Stop();
            dayDataCounter.Stop();
        }

        #endregion

        #region NOTIFICATION

        private void SendNotification(string message)
        {
            OnNotificationToBeSent?.Invoke(message);
        }

        #endregion

        #region LOCK_UNLOCK

        private void SystemEvents_SessionSwitch(object sender, SessionSwitchEventArgs e)
        {
            switch (e.Reason)
            {
                case SessionSwitchReason.SessionLock:
                    StopAllTimers();
                    break;
                case SessionSwitchReason.SessionUnlock:
                    ConnectedDevice?.StartMovingTableDown();
                    CurrentPosition = Position.DOWN;
                    CurrentState = State.NORMAL;
                    //TODO: autoconnect
                    break;
            }
        }

        #endregion

        #region BLUETOOTH

        public async void Connect(string deskXName)
        {
            ConnectedDevice = await BluetoothService.ConnectDevice(FoundDesks.Find(desk =>
            {
                return deskXName == desk.DeskUI.Name;
            }));
            if (ConnectedDevice != null)
            {
                dayDataCounter.Start();
                notificationTimer.Start();
            }
        }

        private void AddDevice(DeviceInformation args)
        {
            OnDeviceAdded?.Invoke(args);
        }

        #endregion
    }
}
