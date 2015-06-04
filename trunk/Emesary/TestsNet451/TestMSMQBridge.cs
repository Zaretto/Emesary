using Emesary;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Messaging;

namespace TestsNet451
{
    /// <summary>
    /// this is our sample notification for when a product is accepted. It forms the basis of this sample.
    /// As this message is to be sent via a queue the IQueueNotification methods IsReadyToSend, IsTimedOut and IsComplete 
    /// need to be implemented - these can be used by a notification that knows it isn't ready (e.g. the message is
    /// waiting for a response, or when it has timedout). The management of these parameters is best done by the notification itself
    /// rather than the processor as it gives more flexibility.
    /// </summary>
    [Serializable]
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


    [TestClass]
    public class TestMSMQBridge : Emesary.IReceiver
    {
#if REMOTE_TEST
        private const string MSMQName = @"FormatName:Direct=OS:win-40dkyfxrine\private$\emesarytest";
#else
        private const string MSMQName = @".\Private$\emesarytest";
#endif
       //private const string MSMQName = @"FormatName:Direct=TCP:192.168.1.94\private$\emesarytest";\
        private int received = 0;

        [TestMethod()]
        public void BridgeToMSMQTest()
        {
            //if (!MessageQueue.Exists(MSMQName))
            //{
            //    MessageQueue.Create(MSMQName);
            //}
            var localTransmitter = new Transmitter();
            var incomingBridge = Emesary.MSMQBridge<AcceptProductNotification>.Incoming(MSMQName, localTransmitter);
            localTransmitter.Register(this);

            var bridge = Emesary.MSMQBridge<AcceptProductNotification>.Outgoing(MSMQName, GlobalTransmitter.Transmitter);
            Emesary.GlobalTransmitter.Register(bridge);

            Emesary.GlobalTransmitter.NotifyAll(new Emesary.QueueNotification("test-message"));
            Emesary.GlobalTransmitter.NotifyAll(new Emesary.QueueNotification("test-message2"));
            Emesary.GlobalTransmitter.NotifyAll(new AcceptProductNotification("accept", "123,144"));
            Emesary.GlobalTransmitter.NotifyAll(new AcceptProductNotification("reject", "444,555"));
            Emesary.GlobalTransmitter.NotifyAll(new AcceptProductNotification("accept", "123,144"));
            Emesary.GlobalTransmitter.NotifyAll(new AcceptProductNotification("reject", "444,555"));
            Emesary.GlobalTransmitter.NotifyAll(new AcceptProductNotification("accept", "123,144"));
            Emesary.GlobalTransmitter.NotifyAll(new AcceptProductNotification("reject", "444,555"));
            Emesary.GlobalTransmitter.NotifyAll(new AcceptProductNotification("accept", "123,144"));
            Emesary.GlobalTransmitter.NotifyAll(new AcceptProductNotification("end", "444,555"));

            //            Emesary.GlobalTransmitter.DeRegister(bridge);
            int retryLimit = 20;
            while (received < 6 && retryLimit-- > 0)
            {
                System.Threading.Thread.Sleep(500);
            };
            Assert.IsTrue(received > 0, "Expected to receive message from MSMQ Bridge");
            incomingBridge.Shutdown();
            System.Threading.Thread.Sleep(600);
        }
        public ReceiptStatus Receive(INotification message)
        {
            System.Console.WriteLine("received {0} {1}", received, message.Value.ToString());
            received++;
            if (message is AcceptProductNotification)
            {
                var ap = message as AcceptProductNotification;
                System.Console.WriteLine("Accept {0} {1}", ap.Parameters, ap.Value);
            }
            if (message.Value is TestMSMQBridge)
            {
                var mqb = message.Value as TestMSMQBridge;
                System.Console.WriteLine("Using {0}", mqb);
            }
            if (message.Value == this)
                return ReceiptStatus.OK;

            return ReceiptStatus.NotProcessed;
        }
    }
}