using Emesary;

namespace SampleQueueOperation
{

    internal class Program
    {
        private static void Main(string[] args)
        {
            InlineDirect.Process();
            ProtocolBsaed.Process();
        }
    }
}