// EmesaryC++Console.cpp : Defines the entry point for the console application.
//

#include "stdafx.h"
#include "Emesary.hxx"

#include <Windows.h>
#include <process.h>
#include <atomic>

	Emesary::Transmitter GlobalTransmitter;
	std::atomic<int> nthread = 0;
	std::atomic<int> noperations = 0;
	const int MaxIterations = 9999;


	class TestThreadNotification : public Emesary::INotification
	{
	public:
		TestThreadNotification(const char *v) : value(v) {}

		virtual const char * GetType() { return value; }
		virtual int get_id(void) const
		{
			return 2211;
		}
	protected:
		const char* value;
	};

	class TestThreadRecipient : public Emesary::IReceiver
	{
	public:
		TestThreadRecipient(std::string type) : receiveCount(0)
		{
			thisType = type;
		}
		std::string thisType;
		std::atomic<int> receiveCount;
		virtual Emesary::ReceiptStatus Receive(Emesary::INotification &n)
		{
			if (thisType  == n.GetType())
			{
                TestThreadNotification *tn = dynamic_cast<TestThreadNotification *>(&n);
                if (tn)
                {
                    printf("Received test notification\n");
                }
				receiveCount++;
				TestThreadNotification onwardNotification("AL");
				GlobalTransmitter.NotifyAll(onwardNotification);
				return Emesary::ReceiptStatus::OK;
			}
			//else if (message.Value == 11)
			//{

			//}
			return Emesary::ReceiptStatus::OK;
		}
	};

	//	typedef unsigned(__stdcall* _beginthreadex_proc_type)(void*);
	//	DWORD __stdcall  ThreadFunc(LPVOID lpThreadParameter)
	unsigned __stdcall  ThreadFunc(void *lpThreadParameter)
	{
		int threadId = nthread.fetch_add(1);

		//System.Threading.Interlocked.Increment(ref nthread);
		//var rng = new Random();
		TestThreadRecipient *r = new TestThreadRecipient("TestThread2");
		char temp[100];
		sprintf(temp, "Notif %d", threadId);
		printf("starting thread %s\n", temp);
		TestThreadNotification tn(r->thisType.c_str());
		for (int i = 0; i < MaxIterations; i++)
		{
			GlobalTransmitter.Register(r);
			GlobalTransmitter.NotifyAll(tn);
			GlobalTransmitter.DeRegister(r);
			//System.Threading.Thread.Sleep(rng.Next(MaxSleep));
			noperations++;
		}
		printf("%s invocations %d\n", temp, (int)r->receiveCount);
		printf("finish thread %s\n", temp);
		return 0;
	}

	class EmesaryTest
	{
	public:

		void action(int v)
		{
			//System.Threading.Interlocked.Increment(ref nthread);
			//var rng = new Random();
			//for (var i = 0; i < MaxIterations; i++)
			//{
			//	target.Register(this);
			//	target.NotifyAll(new Notification(this));
			//	target.DeRegister(this);
			//	//System.Threading.Thread.Sleep(rng.Next(MaxSleep));
			//}
		}

		void Emesary_MultiThreadTransmitterTest()
		{
			//int names[] = {2, 27, 43, 82, 86, 100, 110, 116, 121, 125, 127, 134, 150, 163, 176, 177, 210, 224, 242, 260, 262, 268, 276, 282, 289, 325, 339, 353, 358, 379, 383, 393, 394, 424, 432, 438, 441, 463, 466, 468, 469, 476, 482, 491, 506, 510, 514, 518, 529, 567, 571, 579, 590, 598, 603, 615, 618, 628, 649, 651, 683, 687, 692, 700, 706, 707, 722, 751, 777, 779, 793, 805, 811, 817, 824, 853, 855, 858, 859, 870, 901, 909, 913, 923, 924, 925, 939, 944, 945, 949, 954, 955, 958, 960, 961, 988, 992, 1016, 1018, 1025, 1026, 1027, 1039, 1051, 1069, 1076, 1083, 1085, 1095, 1110, 1140, 1143, 1157, 1159, 1168, 1183, 1193, 1217, 1220, 1231, 1240, 1270, 1289, 1297, 1315, 1317, 1321, 1322, 1329, 1331, 1333, 1339, 1342, 1359, 1364, 1375, 1378, 1382, 1383, 1410, 1415, 1424, 1429, 1451, 1468, 1506, 1524, 1528, 1529, 1540, 1551, 1556, 1589, 1598, 1609, 1612, 1616, 1621, 1645, 1671, 1695, 1712, 1722, 1748, 1752, 1763, 1765, 1783, 1793, 1796, 1797, 1799, 1813, 1820, 1821, 1830, 1869, 1874, 1893, 1919, 1929, 1932, 1951, 1974, 1980, 1982, 1985, 1986, 1991, 1993, 1994};
			//const int MaxIterations = 99999;
			//const int MaxSleep = 2; //ms
			//std::atomic<int> nthread = 0;
			//Emesary::Transmitter target;
			int num_threads = MAXIMUM_WAIT_OBJECTS;
			//System::Threading::Tasks::ParallelOptions ^options = gcnew System::Threading::Tasks::ParallelOptions();
			//options->MaxDegreeOfParallelism = 100;
			//List<int> ^listOfItems = gcnew List<int>();
			//for (int vv = 0; vv < sizeof(names) / sizeof(int); vv++)
			//	listOfItems->Add(names[vv]);

			//System::Threading::Tasks::Parallel::ForEach(listOfItems, options, gcnew Action<int>(this, &UnitTest1::action));
			//System.Console.WriteLine("{0} nthread={1}", receiveCount, nthread);
			HANDLE* hThread = (HANDLE*)calloc(num_threads, sizeof(HANDLE));

			unsigned int threadId;
			//HANDLE hThread;
			//DWORD threadId;
			//			hThread = CreateThread(NULL, 0, ThreadFunc, NULL, 0, &threadId);
			for (int i = 0; i < num_threads; i++)
			{
				hThread[i] = (HANDLE)_beginthreadex(NULL, 0, ThreadFunc, NULL, 0, &threadId);

			}
			WaitForMultipleObjects(num_threads, hThread, true, INFINITE);
			int v = nthread;
			printf("%d messages sent\n", GlobalTransmitter.SentMessageCount());
		}
	};

int main()
{
	TestThreadRecipient* r = new TestThreadRecipient("TestThread2");
	TestThreadNotification tn(r->thisType.c_str());
	GlobalTransmitter.Register(r);
	for (int i = 0; i < MaxIterations*MaxIterations; i++)
	{
		GlobalTransmitter.NotifyAll(tn);
		//System.Threading.Thread.Sleep(rng.Next(MaxSleep));
 		noperations++;
	}
	GlobalTransmitter.DeRegister(r);
	printf("invocations %d\n", GlobalTransmitter.SentMessageCount());

	EmesaryTest t;
	t.Emesary_MultiThreadTransmitterTest();
    return 0;
}

