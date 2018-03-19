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
    /*
     * Mainly a convenience - the global transmitter is exactly what it sounds like - an instance
     * of a transmitter that can be used globally.
     */
    public class GlobalTransmitter
    {
        private static Transmitter _globalNotifier = new Transmitter();
        public static Transmitter Transmitter { get { return _globalNotifier; } }

        public static void Register(IReceiver R)
        {
            _globalNotifier.Register(R);
        }

        public static void DeRegister(IReceiver R)
        {
            _globalNotifier.DeRegister(R);
        }

        public static ReceiptStatus NotifyAll(INotification M)
        {
            return _globalNotifier.NotifyAll(M);
        }
    }
}
