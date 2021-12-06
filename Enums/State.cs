using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Table
{
    public enum State
    {
        SNOOZED, // snoozed from notification
        WAITING_FOR_ACCEPTANCE, // notification sent, waiting for decision
        NORMAL // normal state, not notified yet
    }
}
