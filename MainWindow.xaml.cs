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
using BankApp._code;
using BankApp.Messages;

namespace BankApp
{
    public class ViewTransaction
    {
        public Transaction transaction { get; private set; }
        public string Description { get { return transaction.Info; } }
        public string Date { get; set; }
        public string DateOriginal { get { return transaction.Date; } }
        public DateTime DateObject { get { return _date; } }
        public double Amount { get; set; }
        public double RealAmount { get { return transaction.Amount; } }
        public Category WantedCategory { get; set; }
        public Category UsedCategory {
            get { return getCategoryByIdentifier(transaction.Category); }
            set { transaction.Category = value.Identifier; }
        }

        private Category getCategoryByIdentifier(int categoryId)
        {
            return BankApplicationState.UserConfig.Categories[categoryId];
        }

        public ViewTransaction(Transaction t)
        {
            Amount = t.Amount;
            Date = t.Date;
            transaction = t;
            if(transaction.Category >= BankApplicationState.UserConfig.Categories.Count)
            {
                transaction.Category = BankApplicationState.UserConfig.Categories.Find(a => a.Setting == Category.CategorySetting.Default).Identifier;
            }
            UsedCategory = BankApplicationState.UserConfig.Categories[transaction.Category];
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
        BankApplicationMain _main;

        public MainWindow()
        {
            _main = new BankApplicationMain(this);
            WpfUtils.MainDispatcher = this.Dispatcher;
            InitializeComponent();
            MessageManager.addListener(this);
            MessageManager.queueMessage(new ApplicationInitializedMessage());
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
            _content.Children.Add(new ParseBankInput());
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
        }

        private void onAutoCategorize(object sender, RoutedEventArgs e)
        {
            _content.Children.Add(new AutoCategorizeScreen(BankApplicationState.AllTransactions));
        }
    }
}
