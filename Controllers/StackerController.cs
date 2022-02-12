using Stacker.Bluetooth;
using Stacker.Timers;
using System;
using System.Collections.Generic;
using Stacker.DaysData;
using Stacker.ApplicationWindows;
using Windows.Devices.Enumeration;
using Microsoft.Win32;

namespace Stacker.Controllers
{
    public class StackerController
    {
        #region Shitty singleton

        private static StackerController instance;

        public static StackerController Instance
        {
            get => instance ??= new StackerController();
        }

        #endregion

        #region Constants

        private const int MaxTimeUp = 120;
        private const int MaxTimeDown = 120;
        private const int MinTimeUp = 2;
        private const int MinTimeDown = 2;

        private const int MinutesAfterNotificationBeforeMoving = 1;
        private const int SnoozeTimeInMinutes = 1;

        #endregion

        #region Events

        public Action OnTodayDayDataChanged;
        public Action<string> OnNotificationToBeSent;

        public Action<DeviceInformation> OnDeviceAdded;
        public Action<DeviceInformationUpdate> OnDeviceRemoved;
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
        private UpdatableSingleTickTimer snoozeTimer;
        private DayDataCounter dayDataCounter;

        public List<Desk> FoundDesks { get; private set; } = new List<Desk>();

        public ConnectedDevice ConnectedDevice { get; private set; }
        private DeviceScanner deviceScanner;

        private StackerController()
        {
            Today = DayDataSaver.LoadTodayDayData();
            SetUpTimers();
            dayDataCounter = new DayDataCounter();
            deviceScanner = new DeviceScanner();
            Subscribe();
            deviceScanner.StartScanning();
        }

        #region Events

        private void Subscribe()
        {
            MainWindow.OnClose += Unsubscribe;
            MainWindow.OnClose += SaveData;
            Notification.OnSnooze += Snooze;
            deviceScanner.OnDeviceAdded += AddDevice;
            deviceScanner.OnDeviceRemoved += RemoveDevice;
            SystemEvents.SessionSwitch +=
                new SessionSwitchEventHandler(SystemEvents_SessionSwitch);
        }

        public void Unsubscribe()
        {
            MainWindow.OnClose -= Unsubscribe;
            MainWindow.OnClose -= SaveData;
            Notification.OnSnooze -= Snooze;
            deviceScanner.OnDeviceAdded -= AddDevice;
            deviceScanner.OnDeviceRemoved -= RemoveDevice;
            SystemEvents.SessionSwitch -=
                new SessionSwitchEventHandler(SystemEvents_SessionSwitch);
        }

        #endregion

        #region User settings

        private void SaveSettings()
        {
            Properties.Settings.Default.Save();
        }

        private void SaveData()
        {
            DayDataSaver.SaveTodayDayData(Today);
        }

        #endregion

        #region Set up

        private void SetUpTimers()
        {
            moveTableAfterNotificationTimer = new SingleTickTimer(MoveTable,
                new TimeSpan(0, MinutesAfterNotificationBeforeMoving, 0));

            notificationTimer = new AlternatingTicksTimer(
                NotifyAboutUp, new TimeSpan(0, Properties.Settings.Default.UserTimeDown - 1, 0),
                NotifyAboutDown, new TimeSpan(0, Properties.Settings.Default.UserTimeUp - 1, 0));

            snoozeTimer = new UpdatableSingleTickTimer();
        }

        #endregion

        #region Timers functions

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

        private void Snooze()
        {
            snoozeTimer.UpdateOnTick(
                CurrentPosition == Position.UP ? NotifyAboutDown : NotifyAboutUp);
            snoozeTimer.Start(new TimeSpan(0, SnoozeTimeInMinutes, 0)
                - moveTableAfterNotificationTimer.ElapsedTimeInCurrentInterval);

            moveTableAfterNotificationTimer.Stop();
        }

        #endregion

        #region Timers modifications

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

        #region Timers util

        private void StopAllTimers()
        {
            notificationTimer.Stop();
            moveTableAfterNotificationTimer.Stop();
            dayDataCounter.Stop();
            snoozeTimer.Stop();
        }

        #endregion

        #region Notificatio

        private void SendNotification(string message)
        {
            OnNotificationToBeSent?.Invoke(message);
        }

        #endregion

        #region Lock / Unclock

        private void SystemEvents_SessionSwitch(object sender, SessionSwitchEventArgs e)
        {
            switch (e.Reason)
            {
                case SessionSwitchReason.SessionLock:
                    StopAllTimers();
                    break;
                case SessionSwitchReason.SessionUnlock:
                    //TODO: autoconnect
                    ConnectedDevice?.StartMovingTableDown();
                    CurrentPosition = Position.DOWN;
                    CurrentState = State.NORMAL;
                    break;
            }
        }

        #endregion

        #region Bluetooth

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
            Console.WriteLine($"Add: {args.Id}");
            OnDeviceAdded?.Invoke(args);
        }

        private void RemoveDevice(DeviceInformationUpdate args)
        {
            Console.WriteLine($"Remove: {args.Id}");
            OnDeviceRemoved?.Invoke(args);
        }

        #endregion
    }
}
