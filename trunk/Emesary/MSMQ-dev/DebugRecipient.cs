using Emesary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TestsNet451;

namespace MSMQ_dev
{

    public class DebugRecipient : IReceiver
    {
        public ReceiptStatus Receive(INotification message)
        {
            System.Console.WriteLine("received {0} {1}", received, message.Value.ToString());
            received++;
            if (message is AcceptProductNotification)
            {
                var ap = message as AcceptProductNotification;
                System.Console.WriteLine("Accept {0} {1}", ap.Parameters, ap.Value);
            }
       
            return ReceiptStatus.NotProcessed;
        }

        public int received { get; set; }
    }
}