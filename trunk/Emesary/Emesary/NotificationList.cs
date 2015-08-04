using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace Emesary
{

    public class NotificationList
    {
        public NotificationList(string queueId)
        {
            QueueID = queueId;
            items = new ConcurrentBag<INotification>();
        }
        public string Id { get; private set; }

        public string QueueID { get; set; }

        public ConcurrentBag<INotification> items { get; private set; }

        internal void Add(INotification M)
        {
            items.Add(M);
        }

        internal void Remove(INotification M)
        {
            items.Remove(M);
        }
    }

}