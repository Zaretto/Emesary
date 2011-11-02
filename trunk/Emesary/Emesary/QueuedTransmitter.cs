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
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Emesary
{

    public class QueuedTransmitter : Transmitter, IReceiver
    {
        public const int DefaultRetrySeconds = 60;

        NotificationList pendingList;
        const string QueuedTransmitterQueueID = "QueuedTransmitter-NotificationList";

        public string queueID { get; set; }

        public QueuedTransmitter(string queueID)
        {
            this.queueID = queueID + QueuedTransmitterQueueID;
            pendingList = new NotificationList(queueID);
            GlobalTransmitter.Register(this);
        }

        public void createObjects()
        {
            if (pendingList != null)
                return;

//            pendingList = // Load from persistent store.
            if (pendingList == null)
                pendingList = new NotificationList(queueID);
        }

        new public ReceiptStatus NotifyAll(INotification M)
        {
            //
            // we strictly filter the notifications that are allowed to be sent by a queue.
            if (M is QueueNotification)
            {
//TODO: for testing this will only work via the quque.                ReceiptStatus notify_result = base.NotifyAll(M);
                ReceiptStatus notify_result = ReceiptStatus.Pending;
                if (notify_result == ReceiptStatus.Pending)
                {
                    pendingList.Add(M);
                    notify_result = ReceiptStatus.OK; // Pending is effectively OK.
                }
                return notify_result;
            }
            else
            {
                throw new System.NotSupportedException();
            }
        }

        //TODO: review the return code; does it need to send anything back?
        // called from: external.
        public void ProcessPending()
        {
            Console.WriteLine("Process pending "+DateTime.Now.ToShortTimeString() );
            List<INotification> toRemove = new List<INotification>();

            // Iterate through all notifications that are ready to be processed.
            foreach (var notification in pendingList.items.Where(n => DateTime.UtcNow >= n.whenNextReadyToSend))
            {
                bool process_failed = false;
                bool processed_ok = false;

                QueueNotification qnotification = notification as QueueNotification;

                //
                // only notify when the notification ready to be sent. This may be used in addition to the whenNextReadyToSend
                // to priver fine grained control over retries.
                if (!qnotification.IsComplete && qnotification.IsReadyToSend)
                {
                Console.WriteLine("Process pending " + DateTime.Now.ToShortTimeString()+" - "+notification.ToString());

                    ReceiptStatus notify_result = base.NotifyAll(notification);

                    switch (notify_result)
                    {
                        case ReceiptStatus.Abort:
                        case ReceiptStatus.Fail:
                            process_failed = true;
                            break;

                        case ReceiptStatus.OK:
                        case ReceiptStatus.Finished:
                            processed_ok = true;
                            break;

                        case ReceiptStatus.Pending:
                        case ReceiptStatus.NotProcessed:
                        default:
                            break;
                    }
                    if (processed_ok)
                    {
                        toRemove.Add(notification);
                        //TODO: need to think about the protocol here; at the moment the design is that
                        //TODO: the recipient that processes the notification successfully will send out
                        //TODO: the appropriate notification.
                        if (notify_result == ReceiptStatus.Finished)
                            break;
                    }
                    if (process_failed)
                    {
                        if (notification.TimedOut)
                            toRemove.Add(notification);
                        //TODO: maybe we send this back wrapped in a TimeOut notification
                        //TODO: in anycase we may need some generic way of telling that this has failed.
                        if (notify_result == ReceiptStatus.Abort)
                            break;
                    }
                }
                else{
                Console.WriteLine("Process pending " + DateTime.Now.ToShortTimeString()+" - not ready: "+notification.ToString());

                }

            }

            foreach (var notification in toRemove)
            {
                pendingList.Remove(notification);
                // possibly need to remove the notification when it is attached to a persistent store
            }
        }

        public ReceiptStatus Receive(INotification message)
        {
            return Emesary.ReceiptStatus.NotProcessed;
        }
    }
}
