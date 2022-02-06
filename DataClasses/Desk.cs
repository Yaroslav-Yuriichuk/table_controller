using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Stacker.Enums;
using System.Windows.Controls;
using System.Windows.Media;
using Windows.ApplicationModel.Contacts;
using Windows.Devices.Enumeration;

namespace Stacker
{
    public class Desk
    {
        #region CONSTANTS

        private const string CONNECTED_COLOR = "#23DA36";
        private const string NOT_CONNECTED_COLOR = "#D58186";
        private const string CONNECTING_COLOR = "#F0A202";

        #endregion
        
        private Button deskUI;
        private Border statusUI;
        private DeviceInformation device;
        private DeskConnectionState connectionState;

        public Button DeskUI => deskUI;
        public Border StatusUI => statusUI;

        public DeviceInformation Device => device;
        public DeskConnectionState ConnectionState {
            get => connectionState;
            set
            {
                connectionState = value;
                switch (value)
                {
                    case DeskConnectionState.CONNECTED:
                        UpdateConnectionStateToConnected();
                        break;
                    case DeskConnectionState.CONNECTING:
                        UpdateConnectionStateToConnecting();
                        break;
                    case DeskConnectionState.NOT_CONNECTED:
                        UpdateConnectionStateToNotConnected();
                        break;
                }
            }
        }

        public Desk(Button deskUI, Border statusUI, DeviceInformation device, DeskConnectionState connectionState)
        {
            this.deskUI = deskUI;
            this.statusUI = statusUI;
            this.device = device;
            ConnectionState = connectionState;
        }

        private void UpdateConnectionStateToConnected()
        {
            StatusUI.Background = (SolidColorBrush)new BrushConverter().ConvertFromString(CONNECTED_COLOR);
        }
        
        private void UpdateConnectionStateToNotConnected()
        {
            StatusUI.Background = (SolidColorBrush)new BrushConverter().ConvertFromString(NOT_CONNECTED_COLOR);
        }

        private void UpdateConnectionStateToConnecting()
        {
            StatusUI.Background = (SolidColorBrush)new BrushConverter().ConvertFromString(CONNECTING_COLOR);
        }
    }
}
