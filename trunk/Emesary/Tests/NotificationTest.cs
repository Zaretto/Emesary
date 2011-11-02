using Emesary;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Tests
{
    [TestClass()]
    public class NotificationTest
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


        [TestMethod()]
        public void NotificationConstructorTest()
        {
            object value = this;
            Emesary.Notification target = new Emesary.Notification(value);
            Assert.AreEqual(target.Value, this);
        }
    }
}
