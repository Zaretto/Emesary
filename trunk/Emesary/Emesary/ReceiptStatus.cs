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
        OK = 0,   			// info
        Fail = 1, 			// if any item fails then send message may return fail
        Abort = 2, 			// stop processing this event and fail
        Finished = 3, 		// stop processing this event and return success
        NotProcessed = 4, 	// recipient didn't recognise this event
        Pending = 5,     	// Not yet processed
        Indeterminate =6,   // Dispatched asynchronously - so cannot know receipt status.
    }
}
