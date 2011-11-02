using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Emesary
{
    public class NotificationList
    {
        public NotificationList(string queueId)
        {
            QueueID = queueId;
            items = new List<QueueNotification>();
        }
        public string Id { get; private set; }

        public string QueueID { get; set; }

        public List<QueueNotification> items { get; private set; }

        internal void Add(INotification M)
        {
            if (M is QueueNotification)
            {
                items.Add(M as QueueNotification);
            }
            else
                throw new NotImplementedException();
        }

        internal void Remove(INotification M)
        {
            if (M is QueueNotification)
            {
                items.Remove(M as QueueNotification);
            }
            else
                throw new NotImplementedException();
        }
    }
}