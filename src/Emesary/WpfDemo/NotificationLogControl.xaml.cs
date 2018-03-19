using Emesary;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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

namespace WpfDemo
{
    public class PropertyChangedBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                PropertyChangedEventHandler handler = PropertyChanged;
                if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
            }));
        }
    }
    public class LogEntry : PropertyChangedBase
    {
        public DateTime DateTime { get; set; }

        public int Index { get; set; }

        public string Message { get; set; }
    }

    public class CollapsibleLogEntry : LogEntry
    {
        public List<LogEntry> Contents { get; set; }
    }
    /// <summary>
    /// Interaction logic for NotificationLogControl.xaml
    /// </summary>
    public partial class NotificationLogControl : UserControl, IReceiver
    {
        public ObservableCollection<LogEntry> LogEntries { get; set; }

        public NotificationLogControl()
        {
            InitializeComponent();

            DataContext = LogEntries = new ObservableCollection<LogEntry>();
            App.Notifier.Register(this);
        }


        public ReceiptStatus Receive(INotification message)
        {
            LogEntries.Add(new LogEntry
                {
                    Index = LogEntries.Count,
                    DateTime=DateTime.Now,
                    Message = message.ToString() 
                });
            return ReceiptStatus.NotProcessed;
        }
    }
}