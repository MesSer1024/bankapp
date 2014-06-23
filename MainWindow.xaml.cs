using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using BankAppLib;

namespace BankApp
{
    public enum TransactionCategory
    {
        Undefined,
        House,
        Food,
        Transport,
        Other,
        ALL,
    }

    public class ViewTransaction
    {
        public Transaction transaction { get; private set; }
        public string Description { get { return transaction.Info; } }
        public string Date { get { return transaction.Date; } }
        public DateTime DateObject { get { return _date; } }
        public double Amount { get { return transaction.Amount; } }
        public TransactionCategory Category 
        { 
            get { return transaction.Amount > 0 ? TransactionCategory.Undefined : (TransactionCategory)transaction.Category; } 
            set { transaction.Category = (int)value; } 
        }

        public ViewTransaction(Transaction t)
        {
            transaction = t;
            _date = DateTime.Parse(t.Date);
        }

        private DateTime _date;
        public bool isIncome { get { return transaction.Amount > 0; } }
    }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, IMessageListener
    {
        private UIElement _overlay;

        public MainWindow()
        {
            WpfUtils.MainDispatcher = this.Dispatcher;
            InitializeComponent();
            MessageManager.addListener(this);
            _content.Children.Add(new ShowTransactionsScreen());
        }

        private void onSave(object sender, ExecutedRoutedEventArgs e)
        {
            MessageManager.queueMessage(new SaveTransactionsMessage());
        }

        private void onLoad(object sender, ExecutedRoutedEventArgs e)
        {
            MessageManager.queueMessage(new LoadTransactionsMessage());
        }

        private void onAddToDatabase(object sender, RoutedEventArgs e)
        {
            _overlay = new ParseBankInput();
            _content.Children.Add(_overlay);
        }

        public void onMessage(IMessage message)
        {
            if (message is CloseOverlayMessage)
            {
                var msg = message as CloseOverlayMessage;
                _content.Children.Remove(msg.Overlay);
            }
        }
    }
}
