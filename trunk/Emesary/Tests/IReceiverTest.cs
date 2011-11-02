using Emesary;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Tests
{
    [TestClass()]
    public class IReceiverTest : Emesary.IReceiver
    {
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

        internal virtual Emesary.IReceiver CreateIReceiver()
        {
            Emesary.IReceiver target = this;
            return target;
        }

        [TestMethod()]
        public void ReceiveTest()
        {
            Emesary.IReceiver target = CreateIReceiver();
            Emesary.INotification message = new Notification(this);
            Emesary.ReceiptStatus expected = Emesary.ReceiptStatus.OK;
            Emesary.ReceiptStatus actual;
            actual = target.Receive(message);
            Assert.AreEqual(expected, actual);
        }

        public ReceiptStatus Receive(INotification message)
        {
            if (message.Value == this)
                return ReceiptStatus.OK;

            return ReceiptStatus.NotProcessed;
        }
    }
}
