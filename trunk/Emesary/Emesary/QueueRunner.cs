using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Emesary
{
    /// <summary>
    /// background queue processing.
    /// </summary>
    public class QueueRunner
    {
        QueuedTransmitter qt;
        bool sessionOpened = false;

        public QueueRunner(QueuedTransmitter t)
        {
            qt = t;
        }
        public void queueRun()
        {
            if (!sessionOpened)
            {
                sessionOpened = true;
            }
            while (!StopQueueRequest)
            {
                qt.ProcessPending();

                qt.WaitForMessage();
            }

        }

        /// <summary>
        /// true to request the queue processing to stop at the next convenient moment
        /// </summary>
        public bool StopQueueRequest { get; set; }
    }
}
