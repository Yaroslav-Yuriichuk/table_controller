using System;
using System.Windows.Threading;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.GenericAttributeProfile;

namespace Stacker.Bluetooth
{
    public class ConnectedDevice
    {
        private Desk desk;

        private BluetoothLEDevice device;
        private GattCharacteristic moveTableCharacteristic;

        private DispatcherTimer sendCommandToMoveUpTimer;
        private DispatcherTimer sendCommandToMoveDownTimer;

        public Desk Desk
        {
            get => desk;
        }

        public bool IsTableMovingUp { get; private set; }
        public bool IsTableMovingDown { get; private set; }

        public ConnectedDevice(Desk desk, BluetoothLEDevice device, GattCharacteristic moveTableCharacteristic)
        {
            this.desk = desk;
            this.device = device;
            this.moveTableCharacteristic = moveTableCharacteristic;
        }

        private void SetUpTimers()
        {
            sendCommandToMoveUpTimer = new DispatcherTimer()
            {
                Interval = new TimeSpan(0, 0, 0, 0, 200),
            };
            sendCommandToMoveUpTimer.Tick += SendCommandToMoveUp;

            sendCommandToMoveDownTimer = new DispatcherTimer()
            {
                Interval = new TimeSpan(0, 0, 0, 0, 200),
            };
            sendCommandToMoveDownTimer.Tick += SendCommandToMoveDown;
        }

        private async void SendCommandToMoveUp(object sender, EventArgs e)
        {
            bool successful = await BluetoothService.SendCommandToMoveUp(moveTableCharacteristic);
            if (!successful)
            {
                IsTableMovingUp = false;
                sendCommandToMoveUpTimer.Stop();
            }
        }

        private async void SendCommandToMoveDown(object sender, EventArgs e)
        {
            bool successful = await BluetoothService.SendCommandToMoveDown(moveTableCharacteristic);
            if (!successful)
            {
                IsTableMovingDown = false;
                sendCommandToMoveDownTimer.Stop();
            }
        }

        public void StartMovingTableUp()
        {
            if (IsTableMovingUp)
            {
                sendCommandToMoveUpTimer.Stop();
                IsTableMovingUp = false;
                return;
            }
            
            if (IsTableMovingDown)
            {
                sendCommandToMoveDownTimer.Stop();
                IsTableMovingDown = false;
            }

            sendCommandToMoveUpTimer.Start();
            IsTableMovingUp = true;
        }

        public void StartMovingTableDown()
        {
            if (IsTableMovingDown)
            {
                sendCommandToMoveDownTimer.Stop();
                IsTableMovingDown = false;
                return;
            }

            if (IsTableMovingUp)
            {
                sendCommandToMoveUpTimer.Stop();
                IsTableMovingUp = false;
            }

            sendCommandToMoveDownTimer.Start();
            IsTableMovingDown = true;
        }
    }
}