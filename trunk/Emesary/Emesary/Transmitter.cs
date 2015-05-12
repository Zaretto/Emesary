/*---------------------------------------------------------------------------
 *
 *	Title                : EMESARY Class based inter-object communication
 *
 *	File Type            : Implementation File
 *
 *	Description          : Provides generic inter-object communication. For an object to receive a message it
 *	                     : must first register with a Transmitter, such as GlobalTransmitter, and implement the 
 *	                     : IReceiver interface. That's it.
 *	                     : To send a message use a Transmitter with an object. That's all there is to it.
 *	                     : 
 * 
 *  References           : http://www.chateau-logic.com/content/class-based-inter-object-communication
 *
 *	Author               : Richard Harrison (richard@zaretto.com)
 *
 *	Creation Date        : 24 September 2009
 *
 *	Version              : $Header: $
 *
 *  Copyright © 2009 Richard Harrison           All Rights Reserved.
 *
 *---------------------------------------------------------------------------*/

using System.Collections.Generic;

namespace Emesary
{
    /**
     * Description: Transmits Message derived objects. Each instance of this class provides a
     * databus to which any number of receivers can attach to. 
     *
     * Messages may be inherited and customised between individual systems.
     */
    public class Transmitter : ITransmitter
    {
        private List<IReceiver> V = new List<IReceiver>();
        private List<IReceiver> pendingRemovals = new List<IReceiver>();
        private List<IReceiver> pendingAdditions = new List<IReceiver>();
        private object Interlock = new object();
        /**
            * Registers an object to receive messsages from this transmitter. 
            * This object is added to the top of the list of objects to be notified. This is deliberate as 
            * the sequence of registration and message receipt can influence the way messages are processing
            * when ReceiptStatus of Abort or Finished are encountered. So it was a deliberate decision that the
            * most recently registered recipients should process the messages/events first.
            */
        public virtual void Register(IReceiver R)
        {
            if (V.IndexOf(R) < 0)
            {
                if (inProgressCount <= 0)
                    V.Insert(0, R);
                else
                    pendingAdditions.Add(R);
            }
        }

        int inProgressCount = 0;
        /*
            *  Removes an object from receving message from this transmitter
            */
        public virtual void DeRegister(IReceiver R)
        {
            if (inProgressCount <= 0)
                V.Remove(R);
            else
                pendingRemovals.Add(R);
        }

        /*
            * Notify all registered recipients. Stop when receipt status of abort or finished are received.
            * The receipt status from this method will be 
            *  - OK > message handled
            *  - Fail > message not handled. A status of Abort from a recipient will result in our status
            *           being fail as Abort means that the message was not and cannot be handled, and
            *           allows for usages such as access controls.
            * NOTE: When I first designed Emesary I always intended to have message routing and the ability
            *       for each recipient to specify an area of interest to allow performance improvements
            *       however this has not yet been implemented - but the concept is still there and
            *       could be implemented by extending the IReceiver interface to allow for this.
            */
        public virtual ReceiptStatus NotifyAll(INotification M)
        {
            if (inProgressCount <= 0 && pendingRemovals.Count > 0)
            {
                lock (Interlock)
                {
                    foreach (var r in pendingRemovals)
                    {
                        V.Remove(r);
                    }
                    pendingRemovals.Clear();
                }
            }
            if (inProgressCount <= 0 && pendingAdditions.Count > 0)
            {
                lock (Interlock)
                {
                    foreach (var r in pendingAdditions)
                    {
                        if (V.IndexOf(r) < 0)
                            V.Insert(0, r);
                    }
                    pendingAdditions.Clear();
                }
            }

            //
            // defer removals whilst processing notifications.
            System.Threading.Interlocked.Increment(ref inProgressCount);

            ReceiptStatus return_status = ReceiptStatus.NotProcessed;

            try
            {
                foreach (IReceiver R in V)
                {
                    //
                    // do not notify any objects pending removal.
                    if (pendingRemovals.Contains(R))
                        break;

                    ReceiptStatus rstat = R.Receive(M);
                    switch (rstat)
                    {
                        case ReceiptStatus.Fail:
                            return_status = ReceiptStatus.Fail;
                            break;
                        case ReceiptStatus.Pending:
                            return_status = ReceiptStatus.Pending;
                            break;
                        case ReceiptStatus.PendingFinished:
                            return rstat;

                        case ReceiptStatus.NotProcessed:
                            break;
                        case ReceiptStatus.OK:
                            if (return_status == ReceiptStatus.NotProcessed)
                                return_status = rstat;
                            break;

                        case ReceiptStatus.Abort:
                            System.Threading.Interlocked.Decrement(ref inProgressCount);
                            return ReceiptStatus.Abort;

                        case ReceiptStatus.Finished:
                            System.Threading.Interlocked.Decrement(ref inProgressCount);
                            return ReceiptStatus.OK;
                    }

                }
            }
            catch
            {
                System.Threading.Interlocked.Decrement(ref inProgressCount);
                throw;
                // return_status = ReceiptStatus.Abort;
            }
            System.Threading.Interlocked.Decrement(ref inProgressCount);
            return return_status;
        }

        public static bool Failed(ReceiptStatus receiptStatus)
        {
            //
            // failed is either Fail or Abort.
            // NotProcessed isn't a failure because it hasn't been processed.
            return receiptStatus == ReceiptStatus.Fail
                    || receiptStatus == ReceiptStatus.Abort;
        }
    }
}
