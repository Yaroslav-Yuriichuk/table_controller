using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Table.Enums;
using System.Windows.Controls;
using Windows.Devices.Enumeration;

namespace Table
{
    class Desk
    {
        private Button deskUI;
        private DeviceInformation device;
        private DeskConnectionState connectionState;

        public Button DeskUI => deskUI;
        public DeviceInformation Device => device;
        public DeskConnectionState ConnectionState => connectionState;

        public Desk(Button deskUI, DeviceInformation device, DeskConnectionState connectionState)
        {
            this.deskUI = deskUI;
            this.device = device;
            this.connectionState = connectionState;
        }
    }
}
