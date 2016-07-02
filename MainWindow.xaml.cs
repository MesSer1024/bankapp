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
using Microsoft.Win32;

namespace BankApp
{
    public enum SuggestedCategory
    {
        Undefined,
        EXCLUDED,
        Savings,
        Household,
        Household_Furniture_Medicine_Stuff,
        Food_Regular,
        Food_EatingOutAndDrinks,
        Transportation,
        ClothesAndStuff,
        Vacation,
        Incomes,
        Incomes_Unknown,
        ALL,
    }

    public class ViewTransaction
    {
        public Transaction transaction { get; private set; }
        public string Description { get { return transaction.Info; } }
        public string Date { get { return transaction.Date; } }
        public DateTime DateObject { get { return _date; } }
        public double Amount { get { return transaction.Amount; } }
        public SuggestedCategory Suggested { get; set; }
        public SuggestedCategory Category 
        { 
            get { return (SuggestedCategory)transaction.Category; } 
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
        private List<ViewTransaction> _allTransactions = new List<ViewTransaction>();

        public MainWindow()
        {
            WpfUtils.MainDispatcher = this.Dispatcher;
            InitializeComponent();
            MessageManager.addListener(this);
            _content.Children.Add(new ShowTransactionsScreen(_allTransactions));
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

        private void onAddToDatabaseMany(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog();
            dlg.Filter = "CSV Files (.csv)|*.csv";

            dlg.Multiselect = true;
            dlg.FileOk += (a,b) =>
            {
                var files = dlg.FileNames;
                var errorHandler = new ErrorHandler();
                var lib = new BankLib(errorHandler);
                var transactions = new List<Transaction>();

                foreach (var path in files)
                {
                    var file = new FileInfo(path);
                    if (!file.Exists)
                        throw new Exception("foo");

                    var lines = File.ReadAllLines(file.FullName);
                    lib.parseTransactions(lines, ref transactions);
                }

                if (errorHandler.getParseErrors().Count > 0)
                {
                    var sb = new StringBuilder();
                    errorHandler.getParseErrors().ForEach(error =>
                    {
                        sb.AppendLine(error.Error + " | " + error.SourceLine);
                    });
                    MessageBox.Show(sb.ToString(), "Parsing Error");
                }
                if (transactions.Count > 0)
                {
                    MessageManager.queueMessage(new InsertTransactionsMessage(transactions.ToArray()));
                    //MessageManager.queueMessage(new CloseOverlayMessage() { Overlay = this });
                }

            };
            dlg.ShowDialog();
        }

        public void onMessage(IMessage message)
        {
            if (message is CloseOverlayMessage)
            {
                var msg = message as CloseOverlayMessage;
                _content.Children.Remove(msg.Overlay);
            }
            else if (message is SaveTransactionsMessage)
            {
                SaveDatabase();
            }
            else if (message is LoadTransactionsMessage)
            {
                LoadDatabase();
            }
            else if (message is InsertTransactionsMessage)
            {
                var msg = message as InsertTransactionsMessage;
                var orgCount = _allTransactions.Count;
                foreach (var t in msg.Transactions)
                {
                    bool add = true;
                    foreach (var a in _allTransactions)
                    {
                        if (a.Description == t.Info && a.Date == t.Date && a.Amount == t.Amount)
                        {
                            add = false;
                            break;
                        }
                    }
                    if (add)
                        _allTransactions.Add(new ViewTransaction(t));
                }
                MessageBox.Show(String.Format("Parsed {0} items and added {1} to database as unique entries", msg.Transactions.Length, _allTransactions.Count - orgCount));
                MessageManager.queueMessage(new DatabaseUpdatedMessage());
            }

        }

        private void onAutoCategorize(object sender, RoutedEventArgs e)
        {
            _overlay = new AutoCategorizeScreen(_allTransactions);
            _content.Children.Add(_overlay);
        }

        private void SaveDatabase()
        {
            var transactions = new List<Transaction>();
            foreach (var t in _allTransactions)
            {
                transactions.Add(t.transaction);
            }
            new BankLib(null).Save(transactions);
        }

        private void LoadDatabase()
        {
            var foo = new BankLib(null);
            try
            {
                var transactions = foo.Load();
                foreach (var t in transactions)
                {
                    bool add = true;
                    foreach (var a in _allTransactions)
                    {
                        if (a.Description == t.Info && a.Date == t.Date && a.Amount == t.Amount)
                        {
                            add = false;
                            break;
                        }
                    }
                    if (add)
                        _allTransactions.Add(new ViewTransaction(t));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
            MessageManager.queueMessage(new DatabaseUpdatedMessage());
        }

    }
}
