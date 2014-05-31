/*---------------------------------------------------------------------------
 *
 *	Title                : EMESARY Queued Transmitter
 *
 *	File Type            : Implementation File
 *
 *	Description          : Queued Transmitter class
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
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Emesary
{

    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public class QueuedTransmitter : Transmitter//, IReceiver
    {
        public const int DefaultRetrySeconds = 60;

        const string QueuedTransmitterQueueID = "QueuedTransmitter-NotificationList";
        private AutoResetEvent messageWaitEvent = new AutoResetEvent(false);
        NotificationList pendingList;
        public QueuedTransmitter(string queueID)
        {
            this.queueID = queueID + QueuedTransmitterQueueID;
            pendingList = new NotificationList(queueID);
        }

        public string queueID { get; set; }
        public void createObjects()
        {
            if (pendingList != null)
                return;

            //TODO: Load from persistent store.
            if (pendingList == null)
                pendingList = new NotificationList(queueID);
        }

        /// <summary>
        /// Queue notification for processing with lambda after completion to allow for processing based on result.
        /// </summary>
        /// <param name="M"></param>
        /// <param name="completed"></param>
        /// <returns></returns>
        public ReceiptStatus NotifyAll(IQueueNotification M, Func<IQueueNotification, ReceiptStatus, ReceiptStatus> completed)
        {
            M.Completed = completed;
            pendingList.Add(M as INotification);
            messageWaitEvent.Set();
            return ReceiptStatus.OK;
        }

        /// <summary>
        /// Queue notification for processing.
        /// </summary>
        /// <param name="M"></param>
        /// <returns></returns>
        public override ReceiptStatus NotifyAll(INotification M)
        {
            pendingList.Add(M);
            messageWaitEvent.Set();
            return ReceiptStatus.Pending; // Pending is effectively OK.;
        }

        public void ProcessPending()
        {
            if (pendingList.items.Count <= 0)
                return;

            // Iterate through all notifications that are ready to be processed.
            //            foreach (var notification in pendingList.items.Where(n => DateTime.UtcNow >= n.whenNextReadyToSend))
            foreach (var notification in pendingList.items.ToList())
            {
                if (notification is IQueueNotification)
                {
                    var qnotification = notification as IQueueNotification;
//                    if (DateTime.UtcNow <= qnotification.WhenNextReadyToSend)
                    if (!qnotification.IsReadyToSend)
                        continue;

                    //bool process_failed = false;
                    //bool processed_ok = false;

                    //
                    // only notify when the notification ready to be sent. This may be used in addition to the whenNextReadyToSend
                    // to priver fine grained control over retries.
                    if (qnotification.IsReadyToSend)
                    {
                        System.Diagnostics.Debug.WriteLine("Process pending " + DateTime.Now.ToShortTimeString() + " - " + notification.ToString());

                        ReceiptStatus notify_result = base.NotifyAll(notification);

                        //switch (notify_result)
                        //{
                        //    case ReceiptStatus.Abort:
                        //    case ReceiptStatus.Fail:
                        //        process_failed = true;
                        //        break;

                        //    case ReceiptStatus.OK:
                        //    case ReceiptStatus.Finished:
                        //        processed_ok = true;
                        //        break;

                        //    case ReceiptStatus.Pending:
                        //    case ReceiptStatus.NotProcessed:
                        //    default:
                        //        break;
                        //}
                        if (qnotification.Completed != null)
                            qnotification.Completed(qnotification, notify_result);
                        
                        if (qnotification.IsComplete || qnotification.IsTimedOut)
                        {
                            pendingList.Remove(notification);
                        }
                        if (notify_result == ReceiptStatus.Finished || notify_result == ReceiptStatus.Abort)
                            break;
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("Process pending " + DateTime.Now.ToShortTimeString() + " - not ready: " + notification.ToString());
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("QueuedTransmitter; Notify non queued message {0}", notification.ToString());
                    ReceiptStatus notify_result = base.NotifyAll(notification);

                    pendingList.Remove(notification);
                }
            }
        }

        public ReceiptStatus Receive(INotification message)
        {
            // stub - maybe this could act as a bridge by some method - probably providing a transmitter to pass on to - such as the global transmitter
            // during construction.
            return Emesary.ReceiptStatus.NotProcessed;
        }

        public void WaitForMessage()
        {
            messageWaitEvent.WaitOne();
        }
    }
}
