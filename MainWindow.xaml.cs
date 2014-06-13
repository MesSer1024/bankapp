using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
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
        Unknown,
        House,
        Food,
        Transport,
        Other,
    }

    public class ViewTransaction
    {
        public Transaction transaction { get; private set; }
        public string Description { get { return transaction.Info; } }
        public string Date { get { return transaction.Date; } }
        public double Amount { get { return transaction.Amount; } }
        public TransactionCategory Category 
        { 
            get { return (TransactionCategory)transaction.Category; } 
            set { transaction.Category = (int)value; } 
        }

        public ViewTransaction(Transaction t)
        {
            transaction = t;
        }
    }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private List<ViewTransaction> _transactions;

        public MainWindow()
        {
            InitializeComponent();
            _transactions = new List<ViewTransaction>();
            onLoad(null, null);
        }

        private void showColumnChart()
        {
            List<KeyValuePair<string, int>> MyValue = new List<KeyValuePair<string, int>>();
            MyValue.Add(new KeyValuePair<string, int>("Mahak", 300));
            MyValue.Add(new KeyValuePair<string, int>("Pihu", 250));
            MyValue.Add(new KeyValuePair<string, int>("Rahul", 289));
            MyValue.Add(new KeyValuePair<string, int>("Raj", 256));
            MyValue.Add(new KeyValuePair<string, int>("Vikas", 140));
            ColumnChart1.DataContext = MyValue;
            PieChart1.DataContext = MyValue;
        }

        private void ComboBox_DropDownClosed_1(object sender, EventArgs e)
        {
            if (_grid.SelectedItems.Count > 0)
            {
                var box = sender as ComboBox;
                if (box.SelectedValue == null)
                    return;
                var category = (TransactionCategory)box.SelectedValue;
                foreach (ViewTransaction t in _grid.SelectedItems)
                {
                    t.Category = category;
                }
                _grid.Items.Refresh();
            }
        }

        private void onSave(object sender, ExecutedRoutedEventArgs e)
        {
            var transactions = new List<Transaction>();
            foreach (var t in _transactions)
            {
                transactions.Add(t.transaction);
            }
            new BankLib(Console.Out).Save(transactions);
        }

        private void onLoad(object sender, ExecutedRoutedEventArgs e)
        {
            var foo = new BankLib(Console.Out);
            var transactions = foo.Load();
            foreach (var t in transactions)
            {
                _transactions.Add(new ViewTransaction(t));
            }
            //var transactions = new List<Transaction>();
            //foo.parseFile("../../_assets/export.csv", ref transactions);
            //foo.parseFile("../../_assets/export2.csv", ref transactions);
            _grid.ItemsSource = _transactions;
            showColumnChart();
        }

        private void CommandBinding_Executed_3(object sender, ExecutedRoutedEventArgs e)
        {

        }
    }
}
