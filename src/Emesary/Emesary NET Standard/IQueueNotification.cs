using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Emesary
{
    public interface IQueueNotification
    {
        ///// <summary>
        ///// Message epoch. UTC.
        ///// All datetimes must be stored as UTC
        ///// </summary>
        //DateTime CreatedDate { get; set; }

        ///// <summary>
        ///// The datetime in UTC when this message expires.
        ///// </summary>
        //DateTime ExpiryDate { get; set; }

        ///// <summary>
        ///// The datetime in UTC when this message will be ready to be sent.
        ///// If the recipient(s) managing this notification are able to predict reliably when
        ///// this notification should next be sent out this filed should contain the value.
        ///// The queue will use this to decide which pending messages are ready to be sent.
        ///// Default value is the creation date. Do not set this unless you are certain of the date.
        ///// Can be used to schedule regular events.
        ///// </summary>
        //DateTime WhenNextReadyToSend { get ; set; }

        /// <summary>
        /// Used to control the sending of notifications. If this returns false then the Transmitter 
        /// should not send this notification.
        /// </summary>
        /// <returns></returns>
        bool IsReadyToSend { get; }

        /// <summary>
        /// Used to control the timeout. If this notification has timed out - then the processor is entitled 
        /// to true.
        /// </summary>
        /// <returns></returns>
        bool IsTimedOut { get; }

        /// <summary>
        /// when this notification has completed the processing recipient must set this to true.
        /// the processing recipient is responsible for follow on notifications.
        /// a notification can remain as complete until the transmit queue decides to remove it from the queue.
        /// there is no requirement that elements are removed immediately upon completion merely that once complete
        /// the transmitter should not notify any more elements.
        /// The current notification loop may be completed - following the usual convention unless Completed or Abort
        /// is returned as the status.
        /// </summary>
        bool IsComplete { get; }

        Func<IQueueNotification, ReceiptStatus, ReceiptStatus> Completed { get; set; }
    }
}
