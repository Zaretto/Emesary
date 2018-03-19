using Emesary;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Tests
{
    [TestClass()]
    public class GlobalTransmitterTest : IReceiver
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
        public void DeRegisterTest()
        {
            Emesary.GlobalTransmitter.Register(this);
            Emesary.GlobalTransmitter.DeRegister(this);
            receiveCount = 0;
            GlobalTransmitter.NotifyAll(new Notification(this));
            Assert.AreEqual(receiveCount, 0);
        }

        [TestMethod()]
        public void NotifyAllTest()
        {
            Emesary.GlobalTransmitter.Register(this);
            receiveCount = 0;
            GlobalTransmitter.NotifyAll(new Notification(this));
            Assert.AreNotEqual(receiveCount, 0);
            Emesary.GlobalTransmitter.DeRegister(this);
        }

        [TestMethod()]
        public void RegisterTest()
        {
            Emesary.GlobalTransmitter.Register(this);
            receiveCount = 0;
            GlobalTransmitter.NotifyAll(new Notification(this));
            Assert.AreNotEqual(receiveCount, 0);
            Emesary.GlobalTransmitter.DeRegister(this);
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
