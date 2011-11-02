using Emesary;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Tests
{
    [TestClass()]
    public class TransmitterTest : IReceiver
    {
        int receiveCount = 0;
        private TestContext testContextInstance;

        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        [TestMethod()]
        public void TransmitterConstructorTest()
        {
            Emesary.Transmitter target = new Emesary.Transmitter();
            if (target.NotifyAll(new Notification(this)) != ReceiptStatus.NotProcessed)
                Assert.Fail("Newly constructed Transmitter with no recipients must return Not Processed");
        }

        [TestMethod()]
        public void DeRegisterTest()
        {
            Emesary.Transmitter target = new Emesary.Transmitter();
            target.Register(this);
            target.DeRegister(this);
            receiveCount = 0;
            GlobalTransmitter.NotifyAll(new Notification(this));
            Assert.AreEqual(receiveCount, 0);
        }

        [TestMethod()]
        public void FailedTest()
        {
            Assert.AreEqual(Emesary.Transmitter.Failed(ReceiptStatus.Abort), true);
            Assert.AreEqual(Emesary.Transmitter.Failed(ReceiptStatus.Fail), true);
            Assert.AreEqual(Emesary.Transmitter.Failed(ReceiptStatus.Finished), false);
            Assert.AreEqual(Emesary.Transmitter.Failed(ReceiptStatus.NotProcessed), false);
            Assert.AreEqual(Emesary.Transmitter.Failed(ReceiptStatus.OK), false);
            Assert.AreEqual(Emesary.Transmitter.Failed(ReceiptStatus.Pending), false);
        }

        [TestMethod()]
        public void NotifyAllTest()
        {
            Emesary.Transmitter target = new Emesary.Transmitter(); // TODO: Initialize to an appropriate value
            target.Register(this);
            receiveCount = 0;
            target.NotifyAll(new Notification(this));
            Assert.AreNotEqual(receiveCount, 0);
            target.DeRegister(this);
        }

        [TestMethod()]
        public void RegisterTest()
        {
            Emesary.Transmitter target = new Emesary.Transmitter(); // TODO: Initialize to an appropriate value
            target.Register(this);
            receiveCount = 0;
            target.NotifyAll(new Notification(this));
            Assert.AreNotEqual(receiveCount, 0);
            target.DeRegister(this);
        }

        public ReceiptStatus Receive(INotification message)
        {
            if (message.Value == this)
            {
                receiveCount++;
                return ReceiptStatus.OK;
            }
            return ReceiptStatus.NotProcessed;
        }
    }
}
