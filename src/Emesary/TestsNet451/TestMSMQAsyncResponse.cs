using Emesary;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Messaging;

namespace TestsNet451
{
    class DebugRecipient : IReceiver
    {
        public DebugRecipient(string id)
        {
            Ident = id;
        }
        public ReceiptStatus Receive(INotification message)
        {
            System.Console.WriteLine("Debug: {0} {1}", Ident, message.ToString());
            return ReceiptStatus.NotProcessed;
        }

        public static void AddToTransmitter(string ident, Transmitter t)
        {
            var dr = new DebugRecipient(ident);
            t.Register(dr);
        }
        public string Ident { get; set; }
    }
    //public class NotifyAllWithAsynchronousResult<T1,T2> : IReceiver
    //    where T1 : ITrackedNotification
    //    where T2 : ITrackedNotification
    //{
    //    public T2 NotifyAndWait(T1 msg, Transmitter via, Transmitter Responder)
    //    {
    //        in_msg = msg;
    //        TrackingId = msg.TrackingId;
    //        Responder.Register(this);
    //        via.NotifyAll(msg);
    //        response_msg = default(T2);
    //        var endTime = DateTime.Now.AddSeconds(20);
    //        while (DateTime.Now < endTime && !finished)
    //        {
    //            System.Console.WriteLine("Waiting for {0}", TrackingId);
    //            System.Threading.Thread.Sleep(1000);
    //        }
    //        if (finished)
    //        {
    //            System.Console.WriteLine("Received response ", response_msg.Value);
    //            return response_msg;
    //        }
    //        Responder.DeRegister(this);
    //        return default(T2); // timeout
    //    }
  
    //    public ReceiptStatus Receive(INotification message)
    //    {
    //        if (message is T1)
    //        {
    //            var nm = message as ITrackedNotification;
    //            if (nm.TrackingId == TrackingId)
    //            {
    //                System.Console.WriteLine("ASYNC: rcvd ", nm.TrackingId);
    //                response_msg = (T2)nm;
    //                finished = true;
    //                return ReceiptStatus.OK;
    //            }
    //        }
    //        return ReceiptStatus.NotProcessed;
    //    }

    //    public INotification in_msg { get; set; }

    //    public T2 response_msg { get; set; }

    //    public bool finished { get; set; }

    //    public string TrackingId { get; set; }
    //}
    [Serializable]
    public class ProvisionServiceNotification : Emesary.Notification, Emesary.ITrackedNotification, Emesary.IQueueNotification
    {
        public ProvisionServiceNotification(string uniqueId, string action, string parameters)
            : base(action)
        {
            MessageType = action;
            Parameters = parameters;
            TrackingId = uniqueId;
        }
        public string TrackingId { get; set; }

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
    public class TestMSMQAsyncResponse : Emesary.IReceiver
    {
#if REMOTE_TEST
        private const string MSMQName = @"FormatName:Direct=OS:win-40dkyfxrine\private$\emesarytest";
#else
        private const string MSMQName = @".\Private$\emesarytest";
#endif
       //private const string MSMQName = @"FormatName:Direct=TCP:192.168.1.94\private$\emesarytest";\
        private int received = 0;

        [TestMethod()]
        public void Bridge_AsyncResponseTests()
        {
            //if (!MessageQueue.Exists(MSMQName))
            //{
            //    MessageQueue.Create(MSMQName);
            //}
            var webapiTransmitter = new Transmitter();
            var serviceBridgeTransmitter = new Transmitter();
            serviceTransmitter = new Transmitter();
            //
            //  webapi -> service (via bridge) -> receive -> transmit onto serviceTransmitter (could be bridged)

            var incomingBridge = Emesary.MSMQBridge<ProvisionServiceNotification>.Incoming(MSMQName, serviceBridgeTransmitter);
            serviceBridgeTransmitter.Register(this);
            DebugRecipient.AddToTransmitter("webapi", webapiTransmitter);
            DebugRecipient.AddToTransmitter("service", serviceTransmitter);
            DebugRecipient.AddToTransmitter("bridge", serviceBridgeTransmitter);

            var outgoingBridge = Emesary.MSMQBridge<ProvisionServiceNotification>.Outgoing(MSMQName, webapiTransmitter);
            //webapiTransmitter.Register(outgoingBridge);

            var asy = new MSMQBridge.NotifyAllWithAsynchronousResult<ProvisionServiceNotification, ProvisionServiceNotification>();
            webapiTransmitter.NotifyAll(new Emesary.QueueNotification("test-message"));
            var response = asy.NotifyAndWait(new ProvisionServiceNotification("011", "reject", "444,555"), webapiTransmitter, serviceTransmitter);
            Assert.AreEqual(response.Parameters, "T99");
            Assert.AreEqual(response.MessageType, "From service");
            Assert.AreEqual(response.TrackingId, "011");
            Assert.IsTrue(asy.Finished,"Should finish");
            Assert.IsTrue(asy.Success, "Should succeed");

            incomingBridge.Shutdown();
            outgoingBridge.Shutdown();
        }
        public ReceiptStatus Receive(INotification message)
        {
            System.Console.WriteLine("received {0} {1}", received, message.Value.ToString());
            received++;
            if (message is ProvisionServiceNotification)
            {
                var ap = message as ProvisionServiceNotification;
                if (ap.TrackingId == "012")
                    System.Threading.Thread.Sleep(20000);

                serviceTransmitter.NotifyAll(new ProvisionServiceNotification(ap.TrackingId, "From service", "T99"));
                System.Console.WriteLine("Accept {0} {1}", ap.Parameters, ap.Value);
                System.Threading.Thread.Sleep(2000);
            }
            if (message.Value == this)
                return ReceiptStatus.OK;

            return ReceiptStatus.NotProcessed;
        }
        [TestMethod()]
        public void Bridge_AsyncResponseTests_Timeout()
        {
            //if (!MessageQueue.Exists(MSMQName))
            //{
            //    MessageQueue.Create(MSMQName);
            //}
            var webapiTransmitter = new Transmitter();
            var serviceBridgeTransmitter = new Transmitter();
            serviceTransmitter = new Transmitter();
            //
            //  webapi -> service (via bridge) -> receive -> transmit onto serviceTransmitter (could be bridged)

            var incomingBridge = Emesary.MSMQBridge<ProvisionServiceNotification>.Incoming(MSMQName, serviceBridgeTransmitter);
            serviceBridgeTransmitter.Register(this);
            DebugRecipient.AddToTransmitter("webapi", webapiTransmitter);
            DebugRecipient.AddToTransmitter("service", serviceTransmitter);
            DebugRecipient.AddToTransmitter("bridge", serviceBridgeTransmitter);

            var outgoingBridge = Emesary.MSMQBridge<ProvisionServiceNotification>.Outgoing(MSMQName, webapiTransmitter);
            webapiTransmitter.Register(outgoingBridge);
            
            var asy = new MSMQBridge.NotifyAllWithAsynchronousResult<ProvisionServiceNotification, ProvisionServiceNotification>(2);
            webapiTransmitter.NotifyAll(new Emesary.QueueNotification("test-message"));

            // by setting this to 012 we will get a 20 second timeout that should cause no message to be received or returned
            var response = asy.NotifyAndWait(new ProvisionServiceNotification("012", "reject", "444,555"), webapiTransmitter, serviceTransmitter);
            Assert.IsNull(response);

            incomingBridge.Shutdown();
            outgoingBridge.Shutdown();
        }


        public Transmitter serviceTransmitter { get; set; }
    }
}