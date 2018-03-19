/*---------------------------------------------------------------------------
 *
 *	Title                : EMESARY Class based inter-object communication
 *	                     : Asynchronous Notifications.
 *
 *	File Type            : Implementation File
 *
 *	Description          : Only asynchronous notifications can be routed over any sort of out of process
 *	                     : bridge - INotification is good for in process - but once out of process the
 *	                     : return status has a different meaning - namely that it indicates whether the
 *	                     : notification processing has been dispatched - rather than the status return
 *	                     : from an individual recipient
 * 
 *  References           : http://www.chateau-logic.com/content/class-based-inter-object-communication#asynchronous
 *
 *	Author               : Richard Harrison (richard@zaretto.com)
 *
 *	Creation Date        : 27 August 2012
 *
 *	Version              : $Header: $
 *
 *  Copyright © 2009 Richard Harrison           All Rights Reserved.
 *
 *---------------------------------------------------------------------------*/
namespace Emesary
{
    /// <summary>
    /// Asynchronous notifications can be routed over a bridge, however unlike
    /// the inprocess notifications the Transmit method will return Indeterminate as 
    /// the status cannot be know.
    /// The model for Asynchronous notifications is different - they can be notify only
    /// or result in a return AsynchronousNotification that is the result of the incoming notification
    /// that will be received by an incoming bridge and dispatched. Matching of outgoing/incoming is via
    /// the UniqueID - however this isn't a recommended design - it is much better to design the Notifications
    /// such that the Outgoing results in an Incoming that is handled seperately.
    /// e.g. for User Authentication
    /// 1. Transmit UserAuthenticationRequest
    /// 2. ReciptStatus indeterminate (sent out)
    /// 3. (Later on) UserAuthenticationRespose received via bridge
    /// 4. Handle UserAuthenticationRespose - probably within the same class that made the request
    ///    in the Receive method.
    /// </summary>
    public interface IAsyncNotification : INotification
    {
        /// <summary>
        /// Identifies uniquely within this notifcation space.
        /// </summary>
        string UniqueID { get; set; }
    }
}
