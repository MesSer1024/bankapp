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
        private Transaction _transaction;

        public string Description { get { return _transaction.Info; } }
        public string Date { get { return _transaction.Date; } }
        public double Amount { get { return _transaction.Amount; } }
        public TransactionCategory Category { get { return (TransactionCategory)_transaction.Category; } set { _transaction.Category = (int)value; } }


        public ViewTransaction(Transaction t)
        {
            _transaction = t;
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

        private void _grid_MouseUp_1(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Right)
            {
                foreach (var t in _grid.SelectedItems)
                {
                    var tran = t as Transaction;
                    tran.Category += 1;
                }
                //_grid.ItemsSource = _transactions; //#TODO Fix
                _grid.Items.Refresh();
            }
        }

        private void _grid_KeyUp_1(object sender, KeyEventArgs e)
        {

        }
    }
}
