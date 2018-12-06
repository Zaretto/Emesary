using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Emesary
{
    public class BridgableNotification : QueueNotification
    {
        public BridgableNotification(object value)
            : base(value)
        { }
    }
}
