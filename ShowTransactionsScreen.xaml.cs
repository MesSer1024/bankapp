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
        private List<ViewTransaction> _transactions;
        private const int ARBITARY_TIME_TO_WAIT_BEFORE_UPDATING_VIEW_ITEMS = 50;
        private FilterHandler _filters;

        public ShowTransactionsScreen()
        {
            InitializeComponent();
            MessageManager.addListener(this);
            _transactions = new List<ViewTransaction>();
            _filters = new FilterHandler(new Button[5] { _filter1, _filter2, _filter3, _filter4, _filter5 }, 4);
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

            public FilterHandler(Button[] buttons, int selectedIndex = 0)
            {
                _buttons = buttons;
                _orgBrush = _buttons[0].Background.CloneCurrentValue();
                buttons[0].Content = "1";
                buttons[1].Content = "3";
                buttons[2].Content = "6";
                buttons[3].Content = "12";
                buttons[4].Content = "All";
                setMarked(selectedIndex);
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

        private void refreshUIElements()
        {
            var chartData = new Dictionary<string, int>();
            foreach (var t in _transactions)
            {
                var s = Enum.GetName(typeof(TransactionCategory), t.Category);
                if (chartData.ContainsKey(s))
                    chartData[s] += (int)t.Amount;
                else
                    chartData.Add(s, (int)t.Amount);
            }
            var item = Keyboard.FocusedElement;
            var selIndex = _grid.SelectedIndex;
            var filterTime = DateTime.Now.AddMonths(0 - _filters.getMonths());
            _grid.Items.Filter = (object t) => { return ((ViewTransaction)t).DateObject.CompareTo(filterTime) >= 0; };
            //_grid.Items.Refresh();
            _grid.SelectedIndex = selIndex;
            if (selIndex >= 0) {
                WpfUtils.toMainThread(() => {
                    var row = _grid.ItemContainerGenerator.ContainerFromIndex(selIndex) as DataGridRow;
                    if (row != null) {
                        var presenter = GetVisualChild<DataGridCellsPresenter>(row);
                        var cell = presenter.ItemContainerGenerator.ContainerFromIndex(3) as DataGridCell;
                        Keyboard.Focus(cell);
                        cell.Focus();
                    }
                }, ARBITARY_TIME_TO_WAIT_BEFORE_UPDATING_VIEW_ITEMS);
            }
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
            if (_grid.SelectedItems.Count > 0)
            {
                var box = sender as ComboBox;
                if (box.SelectedValue == null)
                    return;
                var category = (TransactionCategory)box.SelectedValue;
                setCategoryForSelectedItems(category);
            }
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
            foreach (var t in _transactions)
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
                    _transactions.Add(new ViewTransaction(t));
                }
            } catch (Exception ex) {
                MessageBox.Show(ex.ToString());
            }
            //var transactions = new List<Transaction>();
            //foo.parseFile("../../_assets/export.csv", ref transactions);
            //foo.parseFile("../../_assets/export2.csv", ref transactions);
            //foo.Save(transactions);
            _grid.ItemsSource = _transactions;

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
                foreach (var t in msg.Transactions)
                {
                    _transactions.Add(new ViewTransaction(t));
                }
            }
        }
    }
}
