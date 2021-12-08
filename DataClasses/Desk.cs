using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Stacker.Enums;
using System.Windows.Controls;
using Windows.Devices.Enumeration;

namespace Stacker
{
    class Desk
    {
        private Button deskUI;
        private Border statusUI;
        private DeviceInformation device;
        private DeskConnectionState connectionState;

        public Button DeskUI => deskUI;
        public Border StatusUI => statusUI;

        public DeviceInformation Device => device;
        public DeskConnectionState ConnectionState => connectionState;

        public Desk(Button deskUI, Border statusUI, DeviceInformation device, DeskConnectionState connectionState)
        {
            this.deskUI = deskUI;
            this.statusUI = statusUI;
            this.device = device;
            this.connectionState = connectionState;
        }
    }
}
