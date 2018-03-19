using System;
using System.Collections.Generic;
using System.Linq;
using System.Messaging;
using System.Text;
using System.Threading.Tasks;
    using Emesary;
using TestsNet451;


namespace MSMQ_dev
{

    class Program
    {

        static void Main(string[] args)
        {
            var MSMQName = @".\Private$\emesarytest";
            if  (args.Length > 0)
            MSMQName = args[0];

            System.Console.WriteLine("using queue {0}", MSMQName);

            if (args.Length > 1)
            {
                /*
                 * setup a bridge that will route "AcceptProductNotification" via MSMQ
                 */
                var bridge = Emesary.MSMQBridge<AcceptProductNotification>.Outgoing(MSMQName, GlobalTransmitter.Transmitter);
                Emesary.GlobalTransmitter.Register(bridge);

                SendTestMessages(MSMQName);
            }
            else
            {
                System.Console.WriteLine("Listening on {0}", MSMQName);

                /*
                 * We will use a local transmitter for this example. The incoming bridge could equally well be registered
                 * with the GlobalTransmitter
                 */ 
                var localTransmitter = new Transmitter();
                /*
                 * Create bridge; the incoming side does not need to be as specific as the outgoing as it will receive
                 * messages from the specified MSMQ and pass the messages onwards to the specified transmitter
                 * - This way only a single incoming MSMQ bridge is required to receive all messages
                 * It is also possible to setup the incoming bridge to only route specific Notifications - this could
                 * be useful where one queue is serving two systems.
                 */
                var incomingBridge = Emesary.MSMQBridge<Notification>.Incoming(MSMQName, GlobalTransmitter.Transmitter);

                /*
                 * Register the local recipient with the same transmitter that the incoming bridge uses 
                 */
                GlobalTransmitter.Register(new DebugRecipient());
//                localTransmitter.Register(new DebugRecipient());
                do
                {
                    System.Threading.Thread.Sleep(500);
                } while (incomingBridge.Active);
            }
        }

        private static void SendTestMessages(string MSMQName)
        {
            /*
             * Send some messages. The first two should not get routed to MSMQ
             */
            Emesary.GlobalTransmitter.NotifyAll(new Emesary.QueueNotification("test-non-routed-message"));
            Emesary.GlobalTransmitter.NotifyAll(new Emesary.QueueNotification("test-non-routed-message2"));
            Emesary.GlobalTransmitter.NotifyAll(new AcceptProductNotification("accept", DateTime.Now.ToString()));
            Emesary.GlobalTransmitter.NotifyAll(new AcceptProductNotification("reject", DateTime.Now.ToString()));
            Emesary.GlobalTransmitter.NotifyAll(new AcceptProductNotification("accept", DateTime.Now.ToString()));
            Emesary.GlobalTransmitter.NotifyAll(new AcceptProductNotification("reject", DateTime.Now.ToString()));
            Emesary.GlobalTransmitter.NotifyAll(new AcceptProductNotification("accept", DateTime.Now.ToString()));
            Emesary.GlobalTransmitter.NotifyAll(new AcceptProductNotification("reject", DateTime.Now.ToString()));
            Emesary.GlobalTransmitter.NotifyAll(new AcceptProductNotification("accept", DateTime.Now.ToString()));
            Emesary.GlobalTransmitter.NotifyAll(new AcceptProductNotification("end", DateTime.Now.ToString()));

            var b2 = Emesary.MSMQBridge<Notification>.Outgoing(MSMQName, GlobalTransmitter.Transmitter);
            Emesary.GlobalTransmitter.Register(b2);
            Emesary.GlobalTransmitter.NotifyAll(new Emesary.Notification("test-message"));
            Emesary.GlobalTransmitter.NotifyAll(new Emesary.QueueNotification("non-routed-notification22"));
        }
    }
}