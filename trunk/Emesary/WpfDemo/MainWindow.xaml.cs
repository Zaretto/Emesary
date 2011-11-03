using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Emesary;

namespace WpfDemo
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, IReceiver
    {
        public MainWindow()
        {
            InitializeComponent();

            CreateSampleData();
            
            DataContext = this;

            // important to register ourselves with the global transmitter to ensure that we receive global notifications, otherwise
            // the Receive method will never be called.
            GlobalTransmitter.Register(this);
        }
        /// <summary>
        /// create some sample data.
        /// </summary>
        private void CreateSampleData()
        {
            var p = new List<Person>();
            p.Add(new Person { Name = "t", DateOfBirth = "22-OCT-1968", HealthcareNumber = "NH2205191901" });
            p.Add(new Person { Name = "A. Gibson", DateOfBirth = "05-DEC-1965", HealthcareNumber = "NH1250120303" });
            p.Add(new Person { Name = "A. Walker", DateOfBirth = "05-DEC-1965", HealthcareNumber = "NH1560120303" });
            p.Add(new Person { Name = "A. Hedgehog", DateOfBirth = "24-OCT-1970", HealthcareNumber = "NH2308120304" });
            People = p;
        }

        /// <summary>
        /// People registered for testing purposes.
        /// </summary>
        public IEnumerable<Person> People { get; set; }

        /// <summary>
        /// Handle any notification
        /// </summary>
        /// <param name="message"></param>
        /// <returns>OK if handled, Fail or Abort if fails, NotProcessed if message not processed.</returns>
        public ReceiptStatus Receive(INotification message)
        {
            ///
            /// we are interested in inquiry notifications - so see if an inquiry notification has been received
            /// and if so we can answer the question whether or not the value is duplicated within our data.
            /// Other recipieints may well also respond that the value is duplicated in their data so we must return 
            /// OK rather than Completed. If we were definitively able to say that this message had been conclusively handled
            /// then we return Completed and no other recipient will receive this notification.
            if (message is InquiryNotification)
            {
                var inqMessage = message as InquiryNotification;

                if (People.Any(ip => ip.Name == inqMessage.InquiryValue))
                    return ReceiptStatus.Fail;

                return ReceiptStatus.OK;
            }
            return ReceiptStatus.NotProcessed;
        }
    }
}
