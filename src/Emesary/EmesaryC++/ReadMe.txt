C++ Version of Emesary.

Source: https://github.com/Zaretto/Emesary

The basic premise of the Emesary messaging system is to allow the
decoupled operation of the various components that comprise any system
- usually within the same process.

The basic unit of communication is a Notification, which is passed
around to any and or all objects that implement the IReceive
interface. Using Interfaces and ihneritance it is possible to pass
around Notifications that have special meanings to certain objects and
allow them to perform the appropriate function.

In our design Notifications are created and sent via a call to
NotifyAll. Any object within the system can implement the IReceive
interface and register itself with either a Queue (WIP) or a
Transmitter to receive all notifications that are sent out. The
underlying concept is that one part of a system knows that something
needs to be done without needing to know how to do it. The part of the
system that needs something done simply creates a notification and
sends it out. Once received by the part of the system that is capable
of performing the requested Notification the relevant actions will be
carried out and a status of OK or Finished returned. See the section
on return codes for a complete explanation of these codes and what
they mean.

// Simple test notification
    class TestThreadNotification : public Emesary::INotification
    {
    protected:
        void *value;
    public:
        TestThreadNotification(void *v) : value(v) {}

        virtual void Value(void *v) { value = v; }
        virtual void* Value() { return value; }
        virtual int get_id(void) const
        {
            return 2211;
        }
    };

// simple test recipient
    class TestThreadRecipient : public Emesary::IReceiver
    {
    public:
        TestThreadRecipient() : receiveCount(0)
        {

        }

        std::atomic<int> receiveCount;
        virtual Emesary::ReceiptStatus Receive(Emesary::INotification &n)
        {
            if (n.Value() == this)
            {
                // simple test to increment counter when notification
                // refers to this class. The Value() property is opaque
                // however the meaning can be defined within a notification
                // but usually by casting.
                TestThreadNotification *tn = dynamic_cast<TestThreadNotification *>(&n);
                if (tn)
                {
                    //printf("Received test notification\n");
                }
                receiveCount++;
            }
            return Emesary::ReceiptStatusOK;
        }
    };

// need to have global variable for GlobalTransmitter
Emesary::Transmitter GlobalTransmitter;

// simple test method.
void test()
{
    TestThreadRecipient r;
    char temp[100];
    sprintf(temp, "Notif %d", threadId);
    TestThreadNotification tn(&r);

    GlobalTransmitter.Register(r);
    GlobalTransmitter.NotifyAll(tn);
    GlobalTransmitter.DeRegister(r);
}
ref. http://chateau-logic.com/content/c-wpf-application-plumbing-using-emesary

