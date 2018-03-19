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

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Linq;
using NetMQ;
using NetMQ.Sockets;
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
        private ConcurrentDictionary<IReceiver, int> V = new ConcurrentDictionary<IReceiver, int>();
        private int CurrentRecipientIndex = 0;
        /**
            * Registers an object to receive messsages from this transmitter. 
            * This object is added to the top of the list of objects to be notified. This is deliberate as 
            * the sequence of registration and message receipt can influence the way messages are processing
            * when ReceiptStatus of Abort or Finished are encountered. So it was a deliberate decision that the
            * most recently registered recipients should process the messages/events first.
            */
        public virtual void Register(IReceiver R)
        {
            if (!V.Keys.Any(xx => xx == R))
                V[R] = Interlocked.Increment(ref CurrentRecipientIndex);
        }
        /*
            *  Removes an object from receving message from this transmitter
            */
        public virtual void DeRegister(IReceiver R)
        {
            int out_idx;
            V.TryRemove(R, out out_idx);
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
            ReceiptStatus return_status = ReceiptStatus.NotProcessed;
            using (var server = new ResponseSocket("@tcp://localhost:5556")) // bind
            using (var client = new RequestSocket(">tcp://localhost:5556"))  // connect
            {
                // Send a message from the client socket
                client.SendFrame("Hello");

                // Receive the message from the server socket
                string m1 = server.ReceiveFrameString();
                Console.WriteLine("From Client: {0}", m1);

                // Send a response back from the server
                server.SendFrame("Hi Back");

                // Receive the response from the client socket
                string m2 = client.ReceiveFrameString();
                Console.WriteLine("From Server: {0}", m2);
            }
            try
            {
                foreach (IReceiver R in V.OrderByDescending(xx => xx.Value).Select(xx => xx.Key).ToList())
                {
                    if (R != null)
                    {
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
                                return ReceiptStatus.Abort;

                            case ReceiptStatus.Finished:
                                return ReceiptStatus.OK;
                        }
                    }
                }
            }
            catch
            {
                throw;
                // return_status = ReceiptStatus.Abort;
            }
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
