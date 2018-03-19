using Emesary;
using System.Threading.Tasks;

namespace SampleQueueOperation
{
    /// <summary>
    /// this is the recipient that will receive from the queue a notification.
    /// The recipient itself does not know that the message has come from a queue as it is
    /// upto the instantor of this object to register with the appropriate queue or non-queued transmitter
    /// </summary>
    internal class AcceptProcess : Emesary.IReceiver
    {
        public ReceiptStatus Receive(INotification message)
        {
            if (message is AcceptProductNotification)
            {
                System.Console.WriteLine("perform some provisioning operation {0}", System.Threading.Thread.CurrentThread.ManagedThreadId);

                return ReceiptStatus.Finished; // Definitely and finally processed this
            }
            return ReceiptStatus.NotProcessed;
        }
    }

    /// <summary>
    /// directly process inline a message.
    /// </summary>
    public class InlineDirect
    {
        public static bool finished;
        private static QueuedTransmitter queue;
        public static void Process()
        {
            var batch_processing = new AcceptProcess();

            System.Console.WriteLine("Starting queue handler {0}", System.Threading.Thread.CurrentThread.ManagedThreadId);
            /*
             * the queue needs to be processed in a background thread; as Emesary does not directly implement or use multithreading or processes
             */
#if INLINE
            queue = new QueuedTransmitter("ProcessingQueue");
            Task.Factory.StartNew(() =>
            {
                while (!finished)
                {
                    queue.WaitForMessage();
                    System.Console.WriteLine("Queue: message received");
                    queue.ProcessPending();
                }
            })//.ContinueWith(t => queue_finished = true,
                //TaskScheduler.FromCurrentSynchronizationContext())
            ;
            queue.Register(batch_processing);
            queue.NotifyAll(new AcceptProductNotification("provision", "username=a gibson"), (notification, status) =>
#else
            Emesary.QueueRunner qr = new Emesary.QueueRunner(Emesary.GlobalQueue.queue);
            System.Threading.Thread queueRunnerThread = new System.Threading.Thread(new System.Threading.ThreadStart(qr.queueRun));
            queueRunnerThread.IsBackground = true;
            queue = Emesary.GlobalQueue.queue;
            queue.Register(batch_processing);
            queueRunnerThread.Start();

            Emesary.GlobalQueue.queue.NotifyAll(new AcceptProductNotification("provision", "username=a gibson"), (notification, status) =>
#endif
            {
                if (status == ReceiptStatus.NotProcessed)
                    return ReceiptStatus.NotProcessed;

                if (status == ReceiptStatus.OK)
                {
                    System.Console.WriteLine("Process provisioning completed {0}", System.Threading.Thread.CurrentThread.ManagedThreadId);
                    // just for the purposes of this example; stop the waiting for the queue. in a real application the queue thread would be
                    // in a long running process that doesn't exit
                    InlineDirect.finished = true;
                }
                return ReceiptStatus.Finished;
            });

            while (!InlineDirect.finished)
                System.Threading.Thread.Sleep(1000);
        }
    }

}