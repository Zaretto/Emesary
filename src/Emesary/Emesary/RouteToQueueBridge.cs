using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Emesary
{
    public class RouteToQueueBridge : IReceiver
    {
        /// <summary>
        /// Create queue forwarder
        /// </summary>
        /// <param name="linkedQueue">Queue to forward to</param>
        /// <param name="FinishAfterForwarded">Whether or not to finish as a pending operation</param>
        public RouteToQueueBridge(QueuedTransmitter linkedQueue, bool FinishAfterForwarded)
        {
            this.LinkedQueue = linkedQueue;
            if (FinishAfterForwarded)
                this.ReturnReceipt = ReceiptStatus.PendingFinished;
            else
                this.ReturnReceipt = ReceiptStatus.Pending;
        }
        public RouteToQueueBridge(QueuedTransmitter linkedQueue, ReceiptStatus returnReceipt)
        {
            this.LinkedQueue = linkedQueue;
            this.ReturnReceipt = returnReceipt;
        }
        /// <summary>
        /// Receive and forward to queue. 
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public ReceiptStatus Receive(INotification message)
        {
            if (message is QueueNotification && LinkedQueue != null)
            {
                LinkedQueue.NotifyAll(message);
                return ReturnReceipt;
            }
            return ReceiptStatus.NotProcessed;
        }

        public QueuedTransmitter LinkedQueue { get; set; }

        public ReceiptStatus ReturnReceipt { get; set; }
    }
}
