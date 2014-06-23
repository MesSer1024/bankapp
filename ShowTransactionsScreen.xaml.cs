using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;
using BankAppLib;

namespace BankApp
{
    /// <summary>
    /// Interaction logic for ShowTransactionsScreen.xaml
    /// </summary>
    public partial class ShowTransactionsScreen : UserControl, IMessageListener
    {
        private List<ViewTransaction> _allTransactions;
        private const int ARBITARY_TIME_TO_WAIT_BEFORE_UPDATING_VIEW_ITEMS = 50;
        private FilterHandler _filters;
        private bool _initialized;

        public ShowTransactionsScreen()
        {
            InitializeComponent();
            MessageManager.addListener(this);
            _allTransactions = new List<ViewTransaction>();
            _filters = new FilterHandler(new Button[5] { _filter1, _filter2, _filter3, _filter4, _filter5 });
            _filters.setMarked(0);

            _currentMonthSelector.SelectedIndex = DateTime.Now.Month - 1;
            _initialized = true;
        }

        ~ShowTransactionsScreen()
        {
            MessageManager.removeListener(this);
        }

        private class FilterHandler
        {
            private Button[] _buttons;
            private int _selectedIndex;
            private Brush _orgBrush;

            public FilterHandler(Button[] buttons)
            {
                _buttons = buttons;
                _orgBrush = _buttons[0].Background.CloneCurrentValue();
                buttons[0].Content = "1";
                buttons[1].Content = "3";
                buttons[2].Content = "6";
                buttons[3].Content = "12";
                buttons[4].Content = "All";
                setMarked(0);
            }

            internal int getMonths()
            {
                return _selectedIndex == 0 ? 1 :
                    _selectedIndex == 1 ? 3 :
                    _selectedIndex == 2 ? 6 :
                    _selectedIndex == 3 ? 12 : 20000;
            }

            public void setMarked(string name)
            {
                for (int i = 0; i < _buttons.Length; i++)
                {
                    if (_buttons[i].Name == name)
                    {
                        setMarked(i);
                        return;
                    }
                }
                throw new Exception("Unable to find item with name: " + name);
            }

            public void setMarked(int selectedIndex)
            {
                if (_selectedIndex >= 0)
                {
                    _buttons[_selectedIndex].Background = _orgBrush;
                }
                _selectedIndex = selectedIndex;
                _buttons[_selectedIndex].Background = Brushes.Yellow;
            }
        }

        private DateTime getFilteredEndTime() {
            //basically current year and then month depending on month in drop-down
            var filtered = new DateTime(DateTime.Now.Year, _currentMonthSelector.SelectedIndex + 1, 1);
            filtered = filtered.AddMonths(1).AddDays(-1); //getting to last day of current month
            return filtered;
        }

        private DateTime getFilteredStartTime() {
            //basically we want to set start time depending on x months earlier than end time
            var end = getFilteredEndTime();
            var start = end.AddMonths(0 - _filters.getMonths()).AddDays(7); //cheating by adding enough days to get into next month... issue with 27th of february - 1 month = 27th of january
            return new DateTime(start.Year, start.Month, 1);
        }

        private void refreshUIElements()
        {
            //find active items given current filter settings
            var filterStartTime = getFilteredStartTime();
            var filterEndTime = getFilteredEndTime();
            var subset = _allTransactions.Where(a => { return a.DateObject >= filterStartTime && a.DateObject <= filterEndTime; });

            //populate each transaction category excluding incomes
            var totalIncome = 0;
            var totalExpense = 0;
            var chartData = new Dictionary<string, int>();
            foreach (var t in subset)
            {
                if (t.Amount > 0) {
                    totalIncome += (int)t.Amount;
                    continue;
                } else {
                    totalExpense += (int)t.Amount;
                }

                var s = Enum.GetName(typeof(TransactionCategory), t.Category);
                if (chartData.ContainsKey(s))
                    chartData[s] += (int)t.Amount;
                else
                    chartData.Add(s, (int)t.Amount);
            }
            var selIndex = _grid.SelectedIndex;
            _grid.ItemsSource = subset;
            _grid.SelectedIndex = selIndex;
            selIndex = Math.Max(0, Math.Min(selIndex, subset.Count()));
            WpfUtils.toMainThread(() => {
                _totalText.Content = "Total income in time span: \nTotal expenses in time span: \nSubTotal:";
                _totalAmount.Content = String.Format("{0}\n{1}\n{2}", totalIncome, -totalExpense, (totalIncome + totalExpense));
                var row = _grid.ItemContainerGenerator.ContainerFromIndex(selIndex) as DataGridRow;
                if (row != null) {
                    var presenter = GetVisualChild<DataGridCellsPresenter>(row);
                    var cell = presenter.ItemContainerGenerator.ContainerFromIndex(3) as DataGridCell;
                    Keyboard.Focus(cell);
                    cell.Focus();
                }
            }, ARBITARY_TIME_TO_WAIT_BEFORE_UPDATING_VIEW_ITEMS);
            PieChart1.DataContext = chartData;
        }

        public static T GetVisualChild<T>(Visual parent) where T : Visual {
            T child = default(T);
            int numVisuals = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < numVisuals; i++) {
                Visual v = (Visual)VisualTreeHelper.GetChild(parent, i);
                child = v as T;
                if (child == null) {
                    child = GetVisualChild<T>(v);
                }
                if (child != null) {
                    break;
                }
            }
            return child;
        }

        private void ComboBox_DropDownClosed_1(object sender, EventArgs e)
        {
            var box = sender as ComboBox;
            if (box.SelectedValue == null)
                return;
            var category = (TransactionCategory)box.SelectedValue;
            setItemCategoryFilter(category);
        }

        private void setItemCategoryFilter(TransactionCategory c)
        {
            if (c == TransactionCategory.ALL)
            {
                _grid.Items.Filter = null;
                return;
            }

            _grid.Items.Filter = (a) =>
            {
                var item = a as ViewTransaction;
                return item.Category == c;
            };
        }

        private void setCategoryForSelectedItems(TransactionCategory category) {
            foreach (ViewTransaction t in _grid.SelectedItems) {
                t.Category = category;
            }
            refreshUIElements();
        }

        private void onSave()
        {
            var transactions = new List<Transaction>();
            foreach (var t in _allTransactions)
            {
                transactions.Add(t.transaction);
            }
            new BankLib(null).Save(transactions);
        }

        private void onLoad()
        {
            var foo = new BankLib(null);
            try {
                var transactions = foo.Load();
                foreach (var t in transactions) {
                    _allTransactions.Add(new ViewTransaction(t));
                }
            } catch (Exception ex) {
                MessageBox.Show(ex.ToString());
            }
            //var transactions = new List<Transaction>();
            //foo.parseFile("../../_assets/export.csv", ref transactions);
            //foo.parseFile("../../_assets/export2.csv", ref transactions);
            //foo.Save(transactions);

            string[] enumNames = Enum.GetNames(typeof(TransactionCategory));
            int n = Math.Min(_wrapPanel.Children.Count, enumNames.Length);
            for (int i = 0; i < n; ++i)
            {
                Label l = _wrapPanel.Children[i] as Label;
                l.Content = String.Format("{0} = {1}", i, enumNames[i]);
            }
            _wrapPanel.Children.RemoveRange(n, _wrapPanel.Children.Count - n);

            refreshUIElements();

        }

        private void CommandBinding_Executed_3(object sender, ExecutedRoutedEventArgs e)
        {

        }

        private void _grid_key(object sender, KeyEventArgs e) {
            if (Keyboard.IsKeyDown(Key.LeftCtrl) && e.IsRepeat == false) {
                switch (e.Key) {
                    case Key.D0:
                        setCategoryForSelectedItems((TransactionCategory)0);
                        break;
                    case Key.D1:
                        setCategoryForSelectedItems((TransactionCategory)1);
                        break;
                    case Key.D2:
                        setCategoryForSelectedItems((TransactionCategory)2);
                        break;
                    case Key.D3:
                        setCategoryForSelectedItems((TransactionCategory)3);
                        break;
                    case Key.D4:
                        setCategoryForSelectedItems((TransactionCategory)4);
                        break;
                }
            }
        }

        private void onFilterButtonClick(object sender, RoutedEventArgs e)
        {
            string name = ((FrameworkElement)e.Source).Name;
            _filters.setMarked(name);
            refreshUIElements();
        }

        public void onMessage(IMessage message)
        {
            if (message is SaveTransactionsMessage)
            {
                onSave();
            }
            else if (message is LoadTransactionsMessage)
            {
                onLoad();
            }
            else if (message is InsertTransactionsMessage)
            {
                var msg = message as InsertTransactionsMessage;
                var orgCount = _allTransactions.Count;
                foreach (var t in msg.Transactions)
                {
                    if(_allTransactions.TrueForAll( a => a.Description != t.Info && a.Date != t.Date && a.Amount != t.Amount))
                        _allTransactions.Add(new ViewTransaction(t));
                }
                MessageBox.Show(String.Format("Parsed {0} items and added {1} to database as unique entries", msg.Transactions.Length, _allTransactions.Count - orgCount));
                refreshUIElements();
            }
        }

        public void foobar(object sender, SelectionChangedEventArgs e) {
            if(_initialized)
                refreshUIElements();
        }
    }
}
