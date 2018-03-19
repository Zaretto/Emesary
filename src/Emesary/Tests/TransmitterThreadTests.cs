using Emesary;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading.Tasks;

namespace Tests
{
    [TestClass()]
    public class TransmitterThreadTests : IReceiver
    {
        int receiveCount = 0;

        [TestMethod()]
        public void MultiThreadTransmitterTest()
        {
            var names = ("1,2,3,4,5,6,a,b,c,d,e,f,g,h,i,j,k,l,1,2,3,4,5,6,a,b,c,d,e,f,g,h,i,j,k,l,1,2,3,4,5,6,a,b,c,d,e,f,g,h,i,j,k,l,").Split(',');
            const int MaxIterations = 99999;
            const int MaxSleep = 2; //ms
            int nthread = 0;
            Emesary.Transmitter target = new Emesary.Transmitter();
            Parallel.ForEach(names,
            new ParallelOptions { MaxDegreeOfParallelism = 100 },
            name =>
            {
                System.Threading.Interlocked.Increment(ref nthread);
                var rng = new Random();
                for (var i = 0; i < MaxIterations; i++)
                {
                    target.Register(this);
                    target.NotifyAll(new Notification(this));
                    target.DeRegister(this);
                    //System.Threading.Thread.Sleep(rng.Next(MaxSleep));
                }
            });
            System.Console.WriteLine("{0} nthread={1}", receiveCount, nthread);
        }


        public ReceiptStatus Receive(INotification message)
        {
            if (message.Value == this)
            {
                System.Threading.Interlocked.Increment(ref receiveCount);
                Emesary.GlobalTransmitter.NotifyAll(new Notification(11));
                return ReceiptStatus.OK;
            }
            //else if (message.Value == 11)
            //{

            //}
            return ReceiptStatus.NotProcessed;
        }
    }
}
