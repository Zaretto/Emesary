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
using System.Collections.Generic;
using System.Text;

namespace Emesary
{
    public enum ReceiptStatus
    {
        /// <summary>
        /// Processing completed successfully
        /// </summary>
        OK = 0,

        /// <summary>
        /// Individual item failure
        /// </summary>
        Fail = 1,

        /// <summary>
        /// Fatal error; stop processing any further recipieints of this message. Implicitly fail
        /// </summary>
        Abort = 2, 			// stop processing this event and fail

        /// <summary>
        /// Definitive completion - do not send message to any further recipieints
        /// </summary>
        Finished = 3, 		// stop processing this event and return success

        /// <summary>
        /// Return value when method doesn't process a message.
        /// </summary>
        NotProcessed = 4, 	// recipient didn't recognise this event

        /// <summary>
        /// Message has been sent but the return status cannot be determined as it has not been processed by the recipient. 
        /// </summary>
        /// <notes>
        /// For example a queue or outgoing bridge
        /// </notes>
        Pending = 5,     	// Not yet processed

        /// <summary>
        /// Message has been definitively handled but the return value cannot be determined. The message will not be sent any further
        /// </summary>
        /// <notes>
        /// For example a point to point forwardeing bridge
        /// </notes>
        PendingFinished = 6,// definitively not yet processed  (e.g. forwarded to queue)
    }
}
