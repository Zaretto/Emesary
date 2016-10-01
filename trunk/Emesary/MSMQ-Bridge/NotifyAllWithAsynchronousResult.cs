using Emesary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MSMQBridge
{
    /// <summary>
    /// Synchronizes two (possibly bridged) Transmitters operating asynchronously to allow for a request->response within a given time period
    /// </summary>
    /// <typeparam name="T1"></typeparam>
    /// <typeparam name="T2"></typeparam>
    public class NotifyAllWithAsynchronousResult<T1, T2> : IReceiver
        where T1 : ITrackedNotification
        where T2 : ITrackedNotification
    {
        /// <summary>
        /// Construct; with an optional timeout that by default is 1 minute
        /// </summary>
        /// <param name="TimeoutSeconds"></param>
        public NotifyAllWithAsynchronousResult(double TimeoutSeconds = 60)
        {
            DefaultTimeout = TimeSpan.FromSeconds(TimeoutSeconds);
            ThreadLock = new object();
        }

        /// <summary>
        /// Notify the Outgoing transmitter and wait for a matching message to come in on the incoming transmitter
        /// within the timeout period.
        /// </summary>
        /// <note>
        /// Tracking ID's must be universally unique. 
        /// If it is not possible to generate universally unique then globally unique will suffice. 
        /// If that is not possible then they should be as different as possible; if this is not possible then find another way to do this....</note>
        /// <param name="notification"></param>
        /// <param name="Outgoing"></param>
        /// <param name="Incoming"></param>
        /// <returns></returns>
        public T2 NotifyAndWait(T1 notification, Transmitter Outgoing, Transmitter Incoming)
        {
            /*
             * Keep the originating request, we will need the tracking ID as it was invoked so take a copy.
             */
            Request = notification;
            TrackingId = notification.TrackingId;
            
            /*
             * register the incoming prior to the notification as a really quick response may otherwise be missed.
             * This class is considered to be transient; and as such it will only be registered on the incoming transmitter
             * for the duration of the operation.
             */
            Incoming.Register(this);
            Outgoing.NotifyAll(notification);
            
            Response = default(T2);
            
            var endTime = DateTime.Now.Add(DefaultTimeout);
            try
            {

                /*
                 */
                while (DateTime.Now < endTime && !Finished)
                {
                    System.Threading.Thread.Sleep(66); // 66 ms (15 hz) should be a sufficiently rapid, but not stressful polling period.
                }
                lock (ThreadLock)
                {
                    if (Finished)
                    {
                        Success = true;
                        return Response;
                    }
                }
            }
            catch(Exception)
            {
                /*
                 * ensure nicely cleaned up
                 */
                Finished = false;
                Success = false;
                Response = default(T2);
            }
            finally
            {
                // must always deregister at the end of the operation.
                Incoming.DeRegister(this);
            }
            return default(T2);
        }

        /*
         * receive message from the incoming transmitter; if it is matching the tracking ID then grab it.
         * I did consider that this should return a "Finished" response; however I cannot be certain that this will always be correct.
         */
        public ReceiptStatus Receive(INotification message)
        {
            if (message is T1)
            {
                var nm = message as ITrackedNotification;
                if (nm.TrackingId == TrackingId)
                {
                    lock (ThreadLock)
                    {
                        Response = (T2)nm;
                        Finished = true;
                        return ReceiptStatus.OK;
                    }
                }
            }
            return ReceiptStatus.NotProcessed;
        }

        /// <summary>
        /// incoming request notification
        /// </summary>
        public T1 Request { get; set; }

        /// <summary>
        /// received response notification
        /// </summary>
        public T2 Response { get; set; }

        /// <summary>
        /// Completed normally
        /// </summary>
        public bool Finished { get; set; }

        /// <summary>
        /// Tracking ID to use 
        /// </summary>
        public string TrackingId { get; protected set; }

        /// <summary>
        /// Success - generally equivalent to finished; however finished is set in the receive loop so there could be a small time period when one
        /// is set and the other isn't set as the operation hasn't finished (as this will be operating in a multi-threaded environment)
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Default Timeout
        /// </summary>
        public TimeSpan DefaultTimeout { get; set; }

        public object ThreadLock { get; set; }
    }
}
