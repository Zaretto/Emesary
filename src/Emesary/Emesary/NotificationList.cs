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
            items = new ConcurrentQueue<INotification>();
        }
        public string Id { get; private set; }

        public string QueueID { get; set; }

        public int Count
        {
            get
            {
                if (items != null)
                    return items.Count;
                return 0;
            }
        }

        ConcurrentQueue<INotification> items { get; set; }

        internal void Add(INotification M)
        {
            items.Enqueue(M);
        }

        internal INotification Next()
        {
            INotification M;
            if (items.TryDequeue(out M))
                return M;
            return null;
        }
    }

}