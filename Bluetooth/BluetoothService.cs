using System;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Storage.Streams;
using Stacker.Enums;

namespace Stacker.Bluetooth
{
    public static class BluetoothService
    {
        #region CONSTANTS

        private const string MoveTableCharacteristic = "ff01";
        private static byte[] MoveTabeUpCommand = { 0xF1, 0xF1, 0x01, 0x00, 0x01, 0x7E };
        private static byte[] MoveTableDownCommand = { 0xF1, 0xF1, 0x02, 0x00, 0x02, 0x7E };

        #endregion

        public static async Task<ConnectedDevice> ConnectDevice(Desk desk)
        {
            Console.WriteLine(desk.Device.Name);
            // Note: BluetoothLEDevice.FromIdAsync must be called from a UI thread because it may prompt for consent.
            BluetoothLEDevice bluetoothLeDevice = await BluetoothLEDevice.FromIdAsync(desk.Device.Id);
            desk.ConnectionState = DeskConnectionState.CONNECTING;
            // ...
            GattCharacteristic moveTableCharacteristic = null;

            GattDeviceServicesResult serviceResult = await bluetoothLeDevice.GetGattServicesAsync();

            if (serviceResult.Status == GattCommunicationStatus.Success)
            {
                var services = serviceResult.Services;
                foreach (var service in services)
                {
                    GattCharacteristicsResult charachterisicResult = await service.GetCharacteristicsAsync();

                    if (charachterisicResult.Status == GattCommunicationStatus.Success)
                    {
                        var characteristics = charachterisicResult.Characteristics;
                        foreach (var characteristic in characteristics)
                        {
                            GattCharacteristicProperties properties = characteristic.CharacteristicProperties;

                            if (properties.HasFlag(GattCharacteristicProperties.WriteWithoutResponse)
                                && characteristic.Uuid.ToString("N").Substring(4, 4) == MoveTableCharacteristic)
                            {
                                moveTableCharacteristic = characteristic;
                                desk.ConnectionState = DeskConnectionState.CONNECTED;
                                return new ConnectedDevice(desk, bluetoothLeDevice, moveTableCharacteristic);
                            }
                        }
                    }
                }
            }

            desk.ConnectionState = DeskConnectionState.NOT_CONNECTED;
            return null;
        }
        
        public static async Task<bool> SendCommandToMoveUp(GattCharacteristic moveTableCharacteristic)
        {
            var writer = new DataWriter();
            writer.WriteBytes(MoveTabeUpCommand);

            GattCommunicationStatus result = await moveTableCharacteristic.WriteValueAsync(writer.DetachBuffer());
            if (result != GattCommunicationStatus.Success)
            {
                Console.WriteLine("Failed to move up");
                return false;
            }

            return true;
        }
        
        public static async Task<bool> SendCommandToMoveDown(GattCharacteristic moveTableCharacterisric)
        {
            var writer = new DataWriter();
            writer.WriteBytes(MoveTableDownCommand);

            GattCommunicationStatus result = await moveTableCharacterisric.WriteValueAsync(writer.DetachBuffer());
            if (result != GattCommunicationStatus.Success)
            {
                Console.WriteLine("Failed to move down");
                return false;
            }

            return true;
        }
    }
}