/*---------------------------------------------------------------------------
 *
 *	Title                : EMESARY Global Queue
 *
 *	File Type            : Implementation File
 *
 *	Description          : A Global queue is a convenience for systems that only require
 *	                     : a single queue.
 * 
 *  References           : http://www.chateau-logic.com/content/class-based-inter-object-communication
 *
 *	Author               : Richard Harrison (richard@zaretto.com)
 *
 *	Creation Date        : 24 October 2011
 *
 *	Version              : $Header: $
 *
 *  Copyright © 2011 Richard Harrison           All Rights Reserved.
 *
 *---------------------------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Emesary
{
    public class GlobalQueue
    {
        private static QueuedTransmitter globalQueue = new QueuedTransmitter("global-queue");

        public static QueuedTransmitter queue { get { return globalQueue; } }
        
        public static void Register(IReceiver R)
        {
            globalQueue.Register(R);
        }

        public static void DeRegister(IReceiver R)
        {
            globalQueue.DeRegister(R);
        }

        public static ReceiptStatus NotifyAll(INotification M)
        {
            return globalQueue.NotifyAll(M);
        }
    }
}
