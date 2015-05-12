using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading;

namespace Tests
{
    /// <summary>
    /// Summary description for Queues
    /// </summary>
    [TestClass]
    public class QueueTest : Emesary.IReceiver
    {
        public QueueTest()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
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

        #region Additional test attributes
        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Use TestInitialize to run code before running each test 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        bool received = false;

        [TestMethod()]
        public void BackgroundQueueTest()
        {
            Emesary.GlobalQueue.queue.Register(this);
            Emesary.QueueRunner qr = new Emesary.QueueRunner(Emesary.GlobalQueue.queue);
            
            Emesary.GlobalQueue.NotifyAll(new Emesary.QueueNotification(this));

            Thread queueRunnerThread = new Thread(new ThreadStart(qr.queueRun));
            queueRunnerThread.IsBackground = true;

            ReceiptDisabled = false;
            queueRunnerThread.Start();
            Thread.Sleep(1000);

            Assert.IsTrue(received, "Notification received from queue");
            qr.StopQueueRequest = true;
            System.Threading.Thread.Sleep(1000);
            Assert.IsFalse(queueRunnerThread.IsAlive, "Queue Runner should still have stopped");
        }

        [TestMethod()]
        public void BackgroundQueueDelayedProcessing()
        {
            Emesary.GlobalQueue.queue.Register(this);
            Emesary.QueueRunner qr = new Emesary.QueueRunner(Emesary.GlobalQueue.queue);

            Emesary.GlobalQueue.NotifyAll(new Emesary.QueueNotification(this));

            Thread queueRunnerThread = new Thread(new ThreadStart(qr.queueRun));
            queueRunnerThread.IsBackground = true;

            ReceiptDisabled = true;
            queueRunnerThread.Start();
            Thread.Sleep(1000);
            ReceiptDisabled = false;
            Thread.Sleep(1000);

            Assert.IsTrue(received, "Notification received from queue");
            qr.StopQueueRequest = true;
            System.Threading.Thread.Sleep(1000);
            Assert.IsFalse(queueRunnerThread.IsAlive, "Queue Runner should still have stopped");
        }

        public Emesary.ReceiptStatus Receive(Emesary.INotification message)
        {
            if (ReceiptDisabled)
            {
                System.Console.WriteLine("Received whilst disabled ", message.ToString());
                return Emesary.ReceiptStatus.NotProcessed;
            }

            if (message.Value == this)
            {
                received = true;
                invocationCount++;                
                return Emesary.ReceiptStatus.OK;
            }
            return Emesary.ReceiptStatus.NotProcessed;
        }

        public int invocationCount { get; set; }

        public bool ReceiptDisabled { get; set; }
    }
}
