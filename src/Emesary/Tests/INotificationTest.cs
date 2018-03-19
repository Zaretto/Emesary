using Emesary;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Tests
{
    [TestClass()]
    public class INotificationTest
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

        internal virtual Emesary.INotification CreateINotification()
        {
            Emesary.INotification target = new Notification(this);
            return target;
        }

        [TestMethod()]
        public void ValueTest()
        {
            Emesary.INotification target = CreateINotification(); 
            object expected = this; 
            object actual;
            target.Value = expected;
            actual = target.Value;
            Assert.AreEqual(expected, actual);
        }
    }
}
