#include "pch.h"
#include "CppUnitTest.h"
#include <list>
#include <cstdio>
#include <string>
#include <cassert>

using namespace Microsoft::VisualStudio::CppUnitTestFramework;

namespace TestEmesaryC
{
    template< typename... Args >
    std::string string_sprintf(const char* format, Args... args) {
        int length = std::snprintf(nullptr, 0, format, args...);
        assert(length >= 0);

        char* buf = new char[length + 1];
        std::snprintf(buf, length + 1, format, args...);

        std::string str(buf);
        delete[] buf;
        return str;
    }


    static void summary(const Emesary::TimeStamp& timeStamp, const Emesary::Transmitter* transmitter, const char* id) {
        Logger::WriteMessage(string_sprintf("[%s]: invocations %d\n", id, transmitter->SentMessageCount()).c_str());
        double elapsed_seconds = timeStamp.elapsedMSec() / 1000.0f;
        Logger::WriteMessage(string_sprintf("[%s]:  -> elapsed %d\n", id, timeStamp.elapsedMSec()).c_str());
        Logger::WriteMessage(string_sprintf("[%s]: took %lf seconds which is %lf/sec\n", id, elapsed_seconds, transmitter->SentMessageCount() / elapsed_seconds).c_str());
    }

    std::atomic<int> nthread{ 0 };
    std::atomic<int> noperations{ 0 };
    const int MaxIterationsBase = 9999;
    const int MaxIterationsThreaded = 50;
    const int MaxIterationsBaseMultipleRecipients = 999;
    const int num_threads = 352;

    class TestBaseNotification : public Emesary::INotification
    {
    public:
        TestBaseNotification() : index(0) {}

        int index;
        virtual const char* GetType() { return "Test"; }
    };
    class TestThreadNotification : public Emesary::INotification
    {
    protected:
        const char* baseValue;
    public:
        TestThreadNotification(const char* v) : baseValue(v) {}
    
        virtual const char* GetType() { return baseValue; }
    };

    class TestBaseRecipient : public Emesary::IReceiver
    {
    public:
        TestBaseRecipient() : receiveCount(0)
        {
        }

        std::atomic<int> receiveCount;

        virtual Emesary::ReceiptStatus Receive(Emesary::INotification& n)
        {
            if (n.GetType() == "Test")
            {
                TestBaseNotification& tbn = dynamic_cast<TestBaseNotification&>(n);
                receiveCount++;
                return Emesary::ReceiptStatus::OK;
            }

            return Emesary::ReceiptStatus::OK;
        }
    };

    class TestThreadBaseRecipient : public Emesary::IReceiver
{
public:
    virtual Emesary::ReceiptStatus Receive(Emesary::INotification& n)
    {
        return Emesary::ReceiptStatus::NotProcessed;
    }
};

class TestThreadRecipient : public Emesary::IReceiver
{
    Emesary::ITransmitter* transmitter;
public:
    TestThreadRecipient(Emesary::ITransmitter* _transmitter, bool addDuringReceive)
        : transmitter(_transmitter), addDuringReceive(addDuringReceive), receiveCount(0), ourType("TestThread")
    {
        r1 = new TestThreadBaseRecipient();
    }
    Emesary::IReceiverPtr r1;
    std::string ourType;
    bool addDuringReceive;
    std::atomic<int> receiveCount;

    virtual Emesary::ReceiptStatus Receive(Emesary::INotification& n)
    {
        if (ourType == n.GetType())
        {
            //            SGSharedPtr<TestThreadBaseRecipient> r1 = new TestThreadBaseRecipient();

                        // Unused: TestThreadNotification *tn = dynamic_cast<TestThreadNotification *>(&n);
            receiveCount++;

            TestThreadNotification onwardNotification("AL");
            transmitter->NotifyAll(onwardNotification);
            if (addDuringReceive) {
                transmitter->Register(r1);
                transmitter->NotifyAll(onwardNotification);
                transmitter->DeRegister(r1);
            }
            return Emesary::ReceiptStatus::OK;
        }
        return Emesary::ReceiptStatus::OK;
    }
};

	TEST_CLASS(TestEmesaryC)
	{
	public:

        TEST_METHOD(testEmesaryBase)
        {
            Emesary::Transmitter* globalTransmitter = Emesary::GlobalTransmitter::instance();
            Emesary::TimeStamp timeStamp;
            timeStamp.stamp();

            Emesary::ObjectPtr <TestBaseRecipient> r = new TestBaseRecipient();
            TestBaseNotification tn;
            globalTransmitter->Register(r);
            Assert::AreEqual(globalTransmitter->Count(), 1u);
            for (int i = 0; i < MaxIterationsBase; i++)
            {
                tn.index = i;
                Assert::AreEqual(r->receiveCount.load(), i);
                globalTransmitter->NotifyAll(tn);
                Assert::AreEqual(r->receiveCount.load(), i+1);

                //System.Threading.Thread.Sleep(rng.Next(MaxSleep));
                noperations++;
            }
            globalTransmitter->DeRegister(r);
            Assert::AreEqual(globalTransmitter->Count(), 0u);
            globalTransmitter->NotifyAll(tn);
            Assert::AreEqual(globalTransmitter->Count(), 0u);
            summary(timeStamp, globalTransmitter, "base");
        }

        TEST_METHOD(testEmesaryMultipleRecipients)
        {
            Emesary::Transmitter* globalTransmitter = Emesary::GlobalTransmitter::instance();
            Emesary::TimeStamp timeStamp;
            timeStamp.stamp();



            // create and register test recipients
            std::vector<Emesary::ObjectPtr<TestBaseRecipient>> recips;
            for (int i = 0; i < 5; i++)
            {
                Emesary::ObjectPtr<TestBaseRecipient> newRecipient = new TestBaseRecipient;
                recips.push_back(newRecipient);
                globalTransmitter->Register(newRecipient);
            }

            // check that TestThreadNotification are ignored
            int rcount = globalTransmitter->SentMessageCount();
            Assert::AreEqual(globalTransmitter->Count(), recips.size());
            {
                TestThreadNotification ttn("TestThread");
                // send a bunch of notifications.
                for (int i = 0; i < MaxIterationsBaseMultipleRecipients; i++)
                {
                    globalTransmitter->NotifyAll(ttn);
                    //System.Threading.Thread.Sleep(rng.Next(MaxSleep));
                }
                std::for_each(recips.begin(), recips.end(), [&globalTransmitter](Emesary::ObjectPtr<TestBaseRecipient> r) {
                    Assert::AreEqual(0, r->receiveCount.load());
                    });
            }
            Assert::AreNotEqual(rcount, globalTransmitter->SentMessageCount());

            rcount = globalTransmitter->SentMessageCount();
            {
                TestBaseNotification tbn;
                for (int i = 0; i < MaxIterationsBaseMultipleRecipients; i++)
                {
                    tbn.index = i;
                    globalTransmitter->NotifyAll(tbn);
                    noperations++;
                }
                std::for_each(recips.begin(), recips.end(), [&globalTransmitter](Emesary::ObjectPtr<TestBaseRecipient> r) {
                    Assert::AreEqual(r->receiveCount.load(), MaxIterationsBaseMultipleRecipients);
                    });

            }
            // now degregister all 
            std::for_each(recips.begin(), recips.end(), [&globalTransmitter](Emesary::ObjectPtr<TestBaseRecipient> r) {
                globalTransmitter->DeRegister(r);
                });

            Assert::AreEqual(globalTransmitter->Count(), 0u);
            {
                TestThreadNotification ttn("TestThread");
                globalTransmitter->NotifyAll(ttn);
            }
            Assert::AreEqual(globalTransmitter->Count(), 0u);

            summary(timeStamp, globalTransmitter, "base");
        }
        static void doThreadTest(const std::string& id, bool addDuringReceive) {
            std::list<std::thread*> threads;
            Emesary::Transmitter* transmitter = Emesary::GlobalTransmitter::instance();
            Emesary::TimeStamp timeStamp;

            for (int i = 0; i < num_threads; i++) {
                auto thread = new std::thread([&id, transmitter, addDuringReceive] {
                    int threadId = nthread.fetch_add(1);

                    Emesary::ObjectPtr<TestThreadRecipient> r = new TestThreadRecipient(transmitter, addDuringReceive);

                    //Logger::WriteMessage(string_sprintf("[%s]: starting thread %d\n", id.c_str(), threadId).c_str());

                    TestThreadNotification tn("TestThread");

                    for (int i = 0; i < MaxIterationsThreaded; i++)
                    {
                        transmitter->Register(r);
                        transmitter->NotifyAll(tn);
                        transmitter->DeRegister(r);
                        //System.Threading.Thread.Sleep(rng.Next(MaxSleep));
                        noperations++;
                    }
                    //Logger::WriteMessage(string_sprintf("[%s]: #%d invocations %d\n", id.c_str(), threadId, (int)r->receiveCount).c_str());
                    });
                threads.push_back(thread);
            }
            for (auto i = threads.begin(); i != threads.end(); i++)
            {
                (*i)->join();
            }
            for (auto i = threads.begin(); i != threads.end(); i++)
            {
                delete* i;
            }
            summary(timeStamp, transmitter, id.c_str());
        }
        TEST_METHOD(testThreading) {
            doThreadTest("testThreading", false);
        }
        TEST_METHOD(testThreadingAddDuringReceive) {
            doThreadTest("thread add/receive", true);
        }
        TEST_METHOD(threadRecipients) {
            Emesary::Transmitter* globalTransmitter = Emesary::GlobalTransmitter::instance();

            Emesary::ObjectPtr<TestThreadRecipient> r = new TestThreadRecipient(globalTransmitter, true);
            TestThreadNotification tn("TestThread");
            globalTransmitter->Register(r);
            Emesary::TimeStamp timeStamp;
            timeStamp.stamp();

            for (int i = 0; i < MaxIterationsThreaded; i++)
            {
                globalTransmitter->NotifyAll(tn);
                noperations++;
            }
            globalTransmitter->DeRegister(r);
            summary(timeStamp, globalTransmitter, "threadRecipients");
        }
    };
}


//
//
//#include <iostream>
//
//#include <simgear/threads/SGThread.hxx>
//#include <simgear/emesary/Emesary.hxx>
//#include <list>
//#include <simgear/misc/test_macros.hxx>
//
//using std::cout;
//using std::cerr;
//using std::endl;
//

//class TestThreadNotification : public Emesary::INotification
//{
//protected:
//    const char* baseValue;
//public:
//    TestThreadNotification(const char* v) : baseValue(v) {}
//
//    virtual const char* GetType() { return baseValue; }
//};
//

//
//class EmesaryTest
//{
//public:
//
//    void Emesary_MultiThreadTransmitterTest(Emesary::ITransmitter* transmitter, bool addDuringReceive)
//    {
//        std::list<EmesaryTestThread*> threads;
//
//        for (int i = 0; i < num_threads; i++)
//        {
//            EmesaryTestThread* thread = new EmesaryTestThread(transmitter, addDuringReceive);
//            threads.push_back(thread);
//            thread->start();
//        }
//        for (std::list<EmesaryTestThread*>::iterator i = threads.begin(); i != threads.end(); i++)
//        {
//            (*i)->join();
//        }
//    }
//};
//
//void testEmesaryThreaded()
//{
//    printf("Testing multithreaded operations\n");
//
//    Emesary::Transmitter* globalTransmitter = Emesary::GlobalTransmitter::instance();
//
//    SGSharedPtr < TestThreadRecipient> r = new TestThreadRecipient(globalTransmitter, false);
//    TestThreadNotification tn("TestThread");
//    globalTransmitter->Register(r);
//    Emesary::TimeStamp timeStamp;
//    timeStamp.stamp();
//    printf(" -- simple receive\n");
//    for (int i = 0; i < MaxIterationsThreaded; i++)
//    {
//        globalTransmitter->NotifyAll(tn);
//        //System.Threading.Thread.Sleep(rng.Next(MaxSleep));
//        noperations++;
//    }
//
//    globalTransmitter->DeRegister(r);
//    printf("invocations %d\n", globalTransmitter->SentMessageCount());
//    double elapsed_seconds = timeStamp.elapsedMSec() / 1000.0f;
//    printf(" -> elapsed %d\n", timeStamp.elapsedMSec());
//    printf("took %lf seconds which is %lf/sec\n", elapsed_seconds, globalTransmitter->SentMessageCount() / elapsed_seconds);
//
//    EmesaryTest t;
//    t.Emesary_MultiThreadTransmitterTest(globalTransmitter, false);
//
//    elapsed_seconds = timeStamp.elapsedMSec() / 1000.0f;
//    printf(" -> elapsed %d\n", timeStamp.elapsedMSec());
//    printf("took %lf seconds which is %lf/sec\n", elapsed_seconds, globalTransmitter->SentMessageCount() / elapsed_seconds);
//}
//
//void testEmesaryThreadedAddDuringReceive()
//{
//    Emesary::TimeStamp timeStamp;
//    timeStamp.stamp();
//    Emesary::Transmitter* globalTransmitter = Emesary::GlobalTransmitter::instance();
//
//    EmesaryTest t;
//    t.Emesary_MultiThreadTransmitterTest(globalTransmitter, true);
//
//    summary(timeStamp, globalTransmitter, "ThreadedAddReceive");
//}
////////////////////////
///// basic tests
//
//int main(int ac, char** av)
//{
//    testEmesaryBase();
//
//    testEmesaryMultipleRecipients();
//
//    testEmesaryThreaded();
//
//    testEmesaryThreadedAddDuringReceive();
//
//    std::cout << "all tests passed" << std::endl;
//    return 0;
//}
