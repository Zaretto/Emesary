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
using System.ComponentModel;

namespace WpfDemo
{
   /// <summary>
    /// Provides a composite control for any text editting. 
    /// The control will display a warning triangle when the name is in error (e.g. duplicated) using a 
    /// InquiryNotification via Emesary connected objects to decide if the value is duplicated.
    /// This could also be extended to provide detached validation.
    /// </summary>
    public partial class ValidatingTextBox : UserControl, INotifyPropertyChanged, IReceiver
    {
        public ValidatingTextBox()
        {
            InitializeComponent();
            GlobalTransmitter.Register(this);
        }

        /// <summary>
        /// Dependency Property Bindings
        /// </summary>
        public static readonly DependencyProperty TextProperty =
                DependencyProperty.Register(
                    "Text",
                    typeof(string),
                    typeof(ValidatingTextBox),
                    new FrameworkPropertyMetadata(null,
                        FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                        new PropertyChangedCallback(ChangeText)) );

        /// <summary>
        /// Register Error property 
        /// </summary>
        public static readonly DependencyProperty ErrorProperty =
                DependencyProperty.Register(
                    "Error",
                    typeof(string),
                    typeof(ValidatingTextBox));

        /// <summary>
        /// Text box contents
        /// </summary>
        public string Text
        {
            get { return GetValue(TextProperty) as string; }
            set
            {
                SetValue(TextProperty, value);
                PropertyChanged(this, new PropertyChangedEventArgs("Text"));
            }
        }

        /// <summary>
        /// Prompt displayed to the left of the text box. Not locale friendly.
        /// </summary>
        public string Prompt
        {
            get { return promptTextBlock.Content as string; }
            set { promptTextBlock.Content = value; }
        }

        /// <summary>
        /// Error message when the field is in error
        /// </summary>
        public string Error
        {
            get { return GetValue(ErrorProperty) as string; }
            set
            {
                SetValue(ErrorProperty, value);
                PropertyChanged(this, new PropertyChangedEventArgs("Error"));
            }
        }

        /// <summary>
        /// callback when text has been changed
        /// </summary>
        /// <param name="NewText"></param>
        private void UpdateText(string NewText)
        {
            reval();
        }

        private static void ChangeText(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
            if (e != null && e.NewValue != null)
                (source as ValidatingTextBox).UpdateText(e.NewValue.ToString());
        }

        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// IReceiver method. Compare the value with the text in our field - except of course
        /// if the inquirer is ourselves which is always ok.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public ReceiptStatus Receive(INotification message)
        {
            if (message.Value != this 
                && message is InquiryNotification)
            {
                var inqMessage = message as InquiryNotification;

                if (GetValue(TextProperty) as string == inqMessage.InquiryValue)
                    return ReceiptStatus.Fail;

                return ReceiptStatus.OK;
            }
            return ReceiptStatus.NotProcessed;
        }

        /// <summary>
        /// re-evaluate the field to see if the value is duplicated.
        /// </summary>
        private void reval()
        {
            duplicatedName.Visibility = System.Windows.Visibility.Hidden;
            ValidationMessage.Visibility = System.Windows.Visibility.Hidden;

            Notification inquiry = new InquiryNotification(InquiryNotification.InquiryType.CanUse, Text, this);
            
            if (GlobalTransmitter.NotifyAll(inquiry) == ReceiptStatus.Fail)
            {
                ValidationMessage.Text = "Duplicated Entry";
                duplicatedName.Visibility = System.Windows.Visibility.Visible;
                ValidationMessage.Visibility = System.Windows.Visibility.Visible;
            }
        }
    }
}
