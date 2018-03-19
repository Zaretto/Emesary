using Emesary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SampleQueueOperation
{
    /// <summary>
    /// this is our sample notification for when a product is accepted. It forms the basis of this sample.
    /// As this message is to be sent via a queue the IQueueNotification methods IsReadyToSend, IsTimedOut and IsComplete 
    /// need to be implemented - these can be used by a notification that knows it isn't ready (e.g. the message is
    /// waiting for a response, or when it has timedout). The management of these parameters is best done by the notification itself
    /// rather than the processor as it gives more flexibility.
    /// </summary>
    public class AcceptProductNotification : Emesary.Notification, Emesary.IQueueNotification
    {
        public AcceptProductNotification(string action, string parameters)
            : base(action)
        {
            MessageType = action;
            Parameters = parameters;
        }

        public bool IsReadyToSend
        {
            get { return true; } // Always ready
        }

        public bool IsTimedOut
        {
            get { return false; } // Never timedout 
        }

        public bool IsComplete { get { return true; } } // Always complete

        public System.Func<IQueueNotification, ReceiptStatus, ReceiptStatus> Completed
        {
            get;
            set;
        }

        public string MessageType { get; set; }

        public string Parameters { get; set; }
    }
}
