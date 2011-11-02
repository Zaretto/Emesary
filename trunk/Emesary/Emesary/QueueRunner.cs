using System;
using System.Collections.Generic;
using System.Linq;
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
            qt.ProcessPending();

            Thread.Sleep(200); // TODO: Review thread frame rate.
        }

    }
}
