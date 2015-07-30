using Emesary;
using System.Threading.Tasks;

namespace SampleQueueOperation
{
    /*
     * this sample demonstrates request response based processing
     */

    /// <summary>
    /// message for request response based processing. as this inherits from AcceptProductNotification it doesn't need
    /// to implement the queue notification interfaces
    /// </summary>
    public class AcceptProductNotificationRequest : AcceptProductNotification
    {
        public AcceptProductNotificationRequest(string action, string parameters)
            : base(action, parameters)
        {
        }
    }

    /// <summary>
    /// message that is sent when the request has been processed.
    /// </summary>
    public class AcceptProductNotificationResponse : Emesary.Notification, Emesary.IQueueNotification
    {
        public AcceptProductNotificationResponse(string info)
            : base(info)
        {
            Information = info;
        }

        public System.Func<IQueueNotification, ReceiptStatus, ReceiptStatus> Completed
        {
            get;
            set;
        }

        public string Information { get; set; }

        public bool IsComplete { get { return true; } }

        public bool IsReadyToSend
        {
            get
            {
                return true;
            }
        }

        public bool IsTimedOut
        {
            get { return false; }
        }
    }

    internal class PerformProcess : Emesary.IReceiver
    {
        public ReceiptStatus Receive(INotification message)
        {
            if (message is AcceptProductNotificationRequest)
            {
                var msg = message as AcceptProductNotificationRequest;
                System.Console.WriteLine("perform some provisioning operation {0}", System.Threading.Thread.CurrentThread.ManagedThreadId);
                ProtocolBsaed.queue.NotifyAll(new AcceptProductNotificationResponse("Provisioned " + msg.MessageType + " " + msg.Parameters));
                return ReceiptStatus.Finished; // Definitely and finally processed this
            }
            return ReceiptStatus.NotProcessed;
        }
    }

    internal class ResponseProcess : Emesary.IReceiver
    {
        public ReceiptStatus Receive(INotification message)
        {
            if (message is AcceptProductNotificationResponse)
            {
                var m = message as AcceptProductNotificationResponse;
                System.Console.WriteLine("ResponseProcess : completed {0} : {1}", m.Information, System.Threading.Thread.CurrentThread.ManagedThreadId);

                /// again just for the example to stop the queue
                ProtocolBsaed.finished = true;
                return ReceiptStatus.Finished; // Definitely and finally processed this
            }
            return ReceiptStatus.NotProcessed;
        }
    }

    /// <summary>
    /// demonstration of the request response; most is the same except there is a response notification sent and received.
    /// </summary>
    public class ProtocolBsaed
    {
        public static bool finished;
        public static QueuedTransmitter queue;

        public static void Process()
        {
            queue = new QueuedTransmitter("ProcessingQueue");
            var batch_processing = new PerformProcess();
            var batch_response_process = new ResponseProcess();

            System.Console.WriteLine("Starting protocal based queue handler {0}", System.Threading.Thread.CurrentThread.ManagedThreadId);
            Task.Factory.StartNew(() =>
            {
                while (!finished)
                {
                    queue.WaitForMessage();
                    System.Console.WriteLine("ProtocolQueue: message received");
                    queue.ProcessPending();
                }
            })//.ContinueWith(t => queue_finished = true,
                //TaskScheduler.FromCurrentSynchronizationContext())
            ;
            queue.Register(batch_processing);
            queue.Register(batch_response_process);

            queue.NotifyAll(new AcceptProductNotificationRequest("provision", "username=a gibson"));

            while (!finished)
                System.Threading.Thread.Sleep(1000);
        }
    }
}