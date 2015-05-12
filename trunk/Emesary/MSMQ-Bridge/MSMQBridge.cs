using System;
using System.Messaging;

namespace Emesary
{
    /// <summary>
    /// Bridge between to Emesary transmitters using MSMQ. This allows any message that can be routed via MSMQ to be 
    /// sent via a defined MSMQ Queue
    /// </summary>
    /// <notes>
    /// using MSMQ Queue may be a tautology but it means a queue that is setup using MSMQ.
    /// </notes>
    /// <typeparam name="T1"></typeparam>
    public class MSMQBridge<T1> : IReceiver, IDisposable where T1 : INotification
    {
        /// <summary>
        /// construct incoming bridge
        /// </summary>
        /// <param name="transmitter"></param>
        /// <param name="MSMQueueName"></param>
        public static Emesary.MSMQBridge<T1> Incoming(string MSMQueueName, ITransmitter incomingTransmitter)
        {
            var bridge = new MSMQBridge<T1>
            {
                queue = new MessageQueue(MSMQueueName),
                OnwardsTransmitter = incomingTransmitter,
                Active = true,
            };

            bridge.queue.MessageReadPropertyFilter.SetAll();
            bridge.queue.Formatter = new BinaryMessageFormatter();


            //bridge.ReceiveLoop();
            if (incomingTransmitter != null)
            {
                System.Threading.Tasks.Task.Factory.StartNew(() =>
                {
                    MessageQueueTransaction currentTransaction = null;// new MessageQueueTransaction();
                    //MessageQueueTransaction currentTransaction = null;
                    TimeSpan receiveTimeOut = new TimeSpan(0, 0, 8);
                    System.Threading.Thread.Sleep(1 * 1000);


                    while (bridge.Active)
                    {
                        try
                        {
                            Message msmqMessage;
                            if (currentTransaction != null)
                                currentTransaction.Begin();

                            if (currentTransaction != null)
                                msmqMessage = bridge.queue.Receive(receiveTimeOut, currentTransaction);
                            else
                                msmqMessage = bridge.queue.Receive(receiveTimeOut);

                            bridge.ProcessReceivedMessage(msmqMessage);
                            //foreach (var msmgMessage in queue.GetAllMessages())
                            //{
                            //    ProcessReceivedMessage(msmgMessage);
                            //}
                            if (currentTransaction != null)
                                currentTransaction.Commit();
                        }
                        catch (MessageQueueException mqex)
                        {
                            System.Console.WriteLine("{0}", mqex);
                            switch (mqex.MessageQueueErrorCode)
                            {
                                case MessageQueueErrorCode.IllegalQueuePathName:
                                    System.Console.WriteLine("FATAL: {0}", mqex.MessageQueueErrorCode);
                                    bridge.Active = false;
                                    break;

                                case MessageQueueErrorCode.IOTimeout:
                                    if (currentTransaction != null)
                                        currentTransaction.Abort();
                                    break;

                                default:
                                    if (currentTransaction != null)
                                        currentTransaction.Abort();
                                    System.Console.WriteLine("FATAL: {0}", mqex.MessageQueueErrorCode);
                                    //bridge.Active = false;
                                    break;
                            }
                        }
                    }
                });
            }
            else
            {
                System.Console.WriteLine("Required onwards transmitter to create receive MSMG bridge");
                throw new Exception("Required onwards transmitter to create receive MSMG bridge");
            }
            System.Console.WriteLine("MSMQ Bridge incoming exit");
            return bridge;
        }

       private void ReceiveLoop()
       {
           Active = true;

           if (OnwardsTransmitter != null)
           {
               System.Threading.Tasks.Task.Factory.StartNew(() =>
               {
                   var currentTransaction = new MessageQueueTransaction();
                   //MessageQueueTransaction currentTransaction = null;
                   TimeSpan receiveTimeOut = new TimeSpan(0, 0, 8);
                   System.Threading.Thread.Sleep(1 * 1000);

                   while (Active)
                   {
                       try
                       {
                           if (currentTransaction != null)
                               currentTransaction.Begin();

                           var msmgMessage = queue.Receive(receiveTimeOut, currentTransaction);
                           ProcessReceivedMessage(msmgMessage);
                           //foreach (var msmgMessage in queue.GetAllMessages())
                           //{
                           //    ProcessReceivedMessage(msmgMessage);
                           //}
                           if (currentTransaction != null)
                               currentTransaction.Commit();
                       }
                       catch (MessageQueueException mqex)
                       {
                           switch (mqex.MessageQueueErrorCode)
                           {
                               case MessageQueueErrorCode.IOTimeout:
                                   if (currentTransaction != null)
                                       currentTransaction.Abort();
                                   break;

                               default:
                                   if (currentTransaction != null)
                                       currentTransaction.Abort();
                                   break;
                           }
                       }
                       System.Console.WriteLine("Queue: message received");
                   }
               });
           }
           else
               throw new Exception("Required onwards transmitter to create receive MSMG bridge");
       }

        /// <summary>
        /// construct outgoing bridge
        /// </summary>
        /// <param name="MSMQueueName"></param>
        public static Emesary.MSMQBridge<T1> Outgoing(string MSMQueueName, ITransmitter incomingTransmitter)
        {
            //if (!System.Messaging.MessageQueue.Exists(MSMQueueName))
            //    throw new SystemException("MSMQ " + MSMQueueName + " does not exist");
            var bridge = new MSMQBridge<T1>();
            bridge.queue = new MessageQueue(MSMQueueName);
            bridge.queue.Formatter = new BinaryMessageFormatter();
            bridge.queue.MessageReadPropertyFilter.SetAll();

            incomingTransmitter.Register(bridge);
            bridge.IncomingTransmitter = incomingTransmitter;
            return bridge;
        }

        public bool Active { get; set; }

        public ITransmitter IncomingTransmitter { get; set; }

        public ITransmitter OnwardsTransmitter { get; set; }

        public MessageQueue queue { get; set; }

        public void ConnectToMSMQ(string queueName)
        {
        }

        public void Dispose()
        {
            if (IncomingTransmitter != null)
            {
                IncomingTransmitter.DeRegister(this);
                IncomingTransmitter = null;
            }
        }

        public void Notify(INotification _message)
        {
            var msmqMessage = new Message();
            msmqMessage.Formatter = new BinaryMessageFormatter();
            msmqMessage.Body = _message;
            msmqMessage.Recoverable = true;
            //var currentTransaction = new MessageQueueTransaction();
            //currentTransaction.Begin();
            queue.Send(msmqMessage, _message.GetType().Name );
            //currentTransaction.Abort();
        }

        public void ProcessReceivedMessage(Message msg)
        {
            if (OnwardsTransmitter == null)
                throw new Exception("No onwards queue for processing of received msmq message");

            msg.Formatter = new BinaryMessageFormatter();
            try
            {
                INotification receivedMessage = (INotification)msg.Body;
                OnwardsTransmitter.NotifyAll(receivedMessage);
            }
            catch (InvalidOperationException ex)
            {
                System.Console.WriteLine("Exception during MSMQ processing " + ex);
                throw;
            }
            catch (Exception ex)
            {
                System.Console.WriteLine("Exception during MSMQ processing " + ex);
                throw;
            }
        }

        public ReceiptStatus Receive(INotification message)
        {
            if (message is T1)
                Notify(message);
            return ReceiptStatus.Pending;
        }

        public void Shutdown()
        {
            Active = false;
        }
    }
}