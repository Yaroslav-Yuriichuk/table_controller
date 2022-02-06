using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;

namespace Stacker.Bluetooth
{
    public class DeviceScanner
    {
        private DeviceWatcher deviceWatcher;

        public Action<DeviceInformation> OnDeviceAdded;
        public Action<DeviceInformationUpdate> OnDeviceUpdated;
        public Action<DeviceInformationUpdate> OnDeviceRemoved;

        public DeviceScanner()
        {
            Initialize();
        }

        public void StartScanning()
        {
            deviceWatcher.Start();
        }

        public void StopScanning()
        {
            deviceWatcher.Stop();
        }

        private void Initialize()
        {
            string[] requestedProperties = { "System.Devices.Aep.DeviceAddress", "System.Devices.Aep.IsConnected" };

            deviceWatcher =
                        DeviceInformation.CreateWatcher(
                                Windows.Devices.Bluetooth.BluetoothLEDevice.GetDeviceSelectorFromPairingState(false),
                                requestedProperties,
                                DeviceInformationKind.AssociationEndpoint);

            // Register event handlers before starting the watcher.
            // Added, Updated and Removed are required to get all nearby devices
            deviceWatcher.Added += DeviceWatcher_Added;
            deviceWatcher.Updated += DeviceWatcher_Updated;
            deviceWatcher.Removed += DeviceWatcher_Removed;
        }

        private void DeviceWatcher_Removed(DeviceWatcher sender, DeviceInformationUpdate args)
        {
            OnDeviceRemoved?.Invoke(args);
        }

        private void DeviceWatcher_Updated(DeviceWatcher sender, DeviceInformationUpdate args)
        {
            OnDeviceUpdated?.Invoke(args);
        }

        private void DeviceWatcher_Added(DeviceWatcher sender, DeviceInformation args)
        {
            if (args.Name != string.Empty)
            {
                //Console.WriteLine(args.Name);
                OnDeviceAdded?.Invoke(args);
            }    
        }
    }
}
