﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Emesary
{
    public interface ITrackedNotification : INotification
    {
        string TrackingId { get; set; }
    }

}
