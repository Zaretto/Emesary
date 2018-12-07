#include "stdafx.h"

using namespace System;
using namespace System::Text;
using namespace System::Collections::Generic;
using namespace Microsoft::VisualStudio::TestTools::UnitTesting;

namespace TestEmesaryC
{
	public class TestRecipient : public Emesary::IReceiver
	{
	public:
		TestRecipient() : InvocationCount(0)
		{

		}
		int InvocationCount;
		virtual Emesary::ReceiptStatus Receive(Emesary::INotification &n)
		{
			InvocationCount++;;
			return Emesary::ReceiptStatusOK;
		}
	};
	public class TestNotification : public Emesary::INotification
	{
	protected:
		void *value;
	public:
		TestNotification(void *v) : value(v) {}

		virtual void Value(void *v) { value = v; }
		virtual void* Value() { return value; }
		virtual int get_id(void) const
		{
			return 2211;
		}
	};
	[TestClass]
	public ref class UnitTest
	{
	private:
		TestContext^ testContextInstance;

	public: 
		/// <summary>
		///Gets or sets the test context which provides
		///information about and functionality for the current test run.
		///</summary>
		property Microsoft::VisualStudio::TestTools::UnitTesting::TestContext^ TestContext
		{
			Microsoft::VisualStudio::TestTools::UnitTesting::TestContext^ get()
			{
				return testContextInstance;
			}
			System::Void set(Microsoft::VisualStudio::TestTools::UnitTesting::TestContext^ value)
			{
				testContextInstance = value;
			}
		};

		#pragma region Additional test attributes
		//
		//You can use the following additional attributes as you write your tests:
		//
		//Use ClassInitialize to run code before running the first test in the class
		//[ClassInitialize()]
		//static void MyClassInitialize(TestContext^ testContext) {};
		//
		//Use ClassCleanup to run code after all tests in a class have run
		//[ClassCleanup()]
		//static void MyClassCleanup() {};
		//
		//Use TestInitialize to run code before running each test
		//[TestInitialize()]
		//void MyTestInitialize() {};
		//
		//Use TestCleanup to run code after each test has run
		//[TestCleanup()]
		//void MyTestCleanup() {};
		//
		#pragma endregion 

		[TestMethod]
		void Emesary_C_BasicTest()
		{
			TestRecipient r;
			Emesary::Transmitter testTransmitter;
			testTransmitter.Register(r);
			Assert::AreEqual(1, testTransmitter.Count());
			TestNotification notification("NOTIFY");
			testTransmitter.NotifyAll(notification);
			Assert::AreEqual(1, r.InvocationCount);
			testTransmitter.DeRegister(r);
			Assert::AreEqual(0, testTransmitter.Count());
		};
	};
}
