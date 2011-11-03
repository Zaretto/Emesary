/*---------------------------------------------------------------------------
 *
 *	Title                : EMESARY Queued Notification
 *
 *	File Type            : Implementation File
 *
 *	Description          : Queued Notification base class - using DataObjects.NET for
 *	                     : the persistency.
 *	                     : 
 *	Author               : Richard Harrison (richard@zaretto.com)
 *
 *	Creation Date        : 14 MAR 2011
 *
 *	Version              : $Header: $
 *
 *  Copyright © 2011 Richard Harrison           All Rights Reserved.
 *
 *---------------------------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.Text;

namespace Emesary
{
    public class QueueNotification : INotification
    {
        private static double DefaultTimeout = 320;// seconds

        public QueueNotification(object Value)
        {
            this.Value = Value;
            TimedOut = false;
            complete = false;
            createdDate = DateTime.UtcNow;
            expiryDate = createdDate.AddSeconds(QueueNotification.DefaultTimeout);
            whenNextReadyToSend = createdDate;
        }

        /// <summary>
        /// queued elements cannot be linked to objects as this is not supported with the
        /// DataObjects.NET 
        /// </summary>
        public object Value {get; set;}

        /// <summary>
        /// Message epoch. UTC.
        /// All datetimes must be stored as UTC
        /// </summary>
        public DateTime createdDate { get; set; }

        /// <summary>
        /// The datetime in UTC when this message expires.
        /// </summary>
        public DateTime expiryDate { get; set; }

        /// <summary>
        /// The datetime in UTC when this message will be ready to be sent.
        /// If the recipient(s) managing this notification are able to predict reliably when
        /// this notification should next be sent out this filed should contain the value.
        /// The queue will use this to decide which pending messages are ready to be sent.
        /// Default value is the creation date. Do not set this unless you are certain of the date.
        /// Can be used to schedule regular events.
        /// </summary>
        public DateTime whenNextReadyToSend { get; set; }

        public bool complete { get; set; }

        /// <summary>
        /// Used to control the sending of notifications. If this returns false then the Transmitter 
        /// should not send this notification.
        /// </summary>
        /// <returns></returns>
        public bool IsReadyToSend { get { return !TimedOut; } }

        /// <summary>
        /// Used to control the timeout. If this notification has timed out - then the processor is entitled 
        /// to true.
        /// </summary>
        /// <returns></returns>
        public bool TimedOut { get; set; }

        /// <summary>
        /// when this notification has completed the processing recipient must set this to true.
        /// the processing recipient is responsible for follow on notifications.
        /// a notification can remain as complete until the transmit queue decides to remove it from the queue.
        /// there is no requirement that elements are removed immediately upon completion merely that once complete
        /// the transmitter should not notify any more elements.
        /// The current notification loop may be completed - following the usual convention unless Completed or Abort
        /// is returned as the status.
        /// </summary>
        public bool IsComplete
        {
            get
            {
                return complete || TimedOut;
            }
        }
    }

}
