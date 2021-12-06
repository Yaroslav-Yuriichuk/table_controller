using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Table.Enums;
using System.Windows.Controls;

namespace Table
{
    class Desk
    {
        private Button deskUI;
        private DeskConnectionState connectionState;

        public Desk(Button deskUI, DeskConnectionState connectionState)
        {
            this.deskUI = deskUI;
            this.connectionState = connectionState;
        }
    }
}
