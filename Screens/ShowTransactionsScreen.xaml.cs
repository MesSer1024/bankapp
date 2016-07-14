using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.DataVisualization.Charting;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using BankAppLib;
using BankApp.Screens;
using BankApp._code;
using System.IO;

namespace BankApp
{
    /// <summary>
    /// Interaction logic for ShowTransactionsScreen.xaml
    /// </summary>
    public partial class ShowTransactionsScreen : UserControl, IMessageListener, IBankScreen
    {
        public List<Category> Categories { get; private set; }

        private List<ViewTransaction> _allTransactions;
        private const int ARBITARY_TIME_TO_WAIT_BEFORE_UPDATING_VIEW_ITEMS = 50;
        private FilterHandler _filters;
        private bool _initialized;
        private Category _lastSeriesFilter;
        private int _lastYear;
        private List<Category> AvailableViewCategories { get; set; }

        public ShowTransactionsScreen(List<ViewTransaction> transactions)
        {

            InitializeComponent();
            AvailableViewCategories = new List<Category>();
            AvailableViewCategories.Add(new Category(Category.CategorySetting.ALL) { CategoryName = "* ALL", Description = "All items", Identifier = 20000 });
            AvailableViewCategories.AddRange(BankApplicationState.UserConfig.Categories);
            _categoryDropdown.ItemsSource = AvailableViewCategories;
            _allTransactions = transactions;
            _filters = new FilterHandler(new Button[5] { _filter1, _filter2, _filter3, _filter4, _filter5 });
            _filters.setMarked(0);

            _currentMonthSelector.SelectedIndex = DateTime.Now.Month - 1;
            _initialized = true;
            _lastYear = DateTime.Now.Year;
            _year.Text = _lastYear.ToString();
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
            var year = int.Parse(_year.Text);
            var filtered = new DateTime(year, _currentMonthSelector.SelectedIndex + 1, 1);
            filtered = filtered.AddMonths(1).AddDays(-1); //getting to last day of current month
            return filtered;
        }

        private DateTime getFilteredStartTime() {
            //basically we want to set start time depending on x months earlier than end time
            var end = getFilteredEndTime();
            var start = end.AddMonths(0 - _filters.getMonths()).AddDays(7); //cheating by adding enough days to get into next month... issue with 27th of february - 1 month = 27th of january
            return new DateTime(start.Year, start.Month, 1);
        }

        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private ViewTransaction generateNewElementFromRange(List<ViewTransaction> transactions)
        {
            var first = new ViewTransaction(transactions[0].transaction);
            if (transactions.Count < 2)
                return first;

            for(int i=1; i < transactions.Count; ++i)
            {
                var item = transactions[i];
                first.Amount += item.Amount;
            }
            return first;
        }

        private void refreshUIElements()
        {
            var enumNames = getCategories();
            _wrapPanel.Children.Clear();
            for (int i = 0; i < enumNames.Count; ++i)
            {
                var l = new Label();
                l.Content = String.Format("{0} = {1}", i, enumNames[i].CategoryName);
                _wrapPanel.Children.Add(l);
            }

            //find active items given current filter settings
            var filterStartTime = getFilteredStartTime();
            var filterEndTime = getFilteredEndTime();
            var subset = _allTransactions.Where(a => { return a.DateObject >= filterStartTime && a.DateObject <= filterEndTime; });

            bool groupItems = _groupItemsCheckbox.IsChecked.HasValue ? (bool)_groupItemsCheckbox.IsChecked : false;
            var sb = new StringBuilder();

            if (groupItems)
            {
                var foo = new Dictionary<string, List<ViewTransaction>>();
                foreach(var item in subset)
                {
                    var cleanString = getGroupableString(item.Description);
                    if (foo.ContainsKey(cleanString))
                    {
                        foo[cleanString].Add(item);
                    }
                    else
                    {
                        //bool match = false;
                        //int stringDistance = int.MaxValue;
                        //string bestMatch = "";
                        //foreach (var s in foo.Keys)
                        //{
                        //    int dist = LevenshteinDistance.Compute(s, cleanString);
                        //    if (dist < stringDistance)
                        //    {
                        //        stringDistance = dist;
                        //        bestMatch = s;
                        //        match = true;
                        //    }
                        //}
                        //if (match && stringDistance < 3)
                        //{
                        //    Console.WriteLine("{2}\t : Combining {0} \t\t\t& {1}", cleanString, bestMatch, stringDistance);
                        //    foo[bestMatch].Add(item);
                        //}
                        //else
                        //{
                            foo.Add(cleanString, new List<ViewTransaction>() { item });
                        //}
                    }
                }

                var container = new List<ViewTransaction>();
                foreach(var item in foo)
                {
                    container.Add(generateNewElementFromRange(item.Value));
                }
                subset = container;
            }



            //populate each transaction category excluding incomes
            var totalIncome = 0;
            var totalExpense = 0;
            var chartData = new Dictionary<string, int>();
            foreach(var c in BankApplicationState.UserConfig.Categories)
            {
                chartData.Add(c.CategoryName, 0);
            }
            foreach (var t in subset)
            {
                if (t.UsedCategory.Setting == Category.CategorySetting.ExcludeEverywhere)
                    continue;

                if (t.Amount > 0)
                    totalIncome += (int)t.Amount;
                else 
                    totalExpense += (int)t.Amount;

                if (t.UsedCategory.Setting == Category.CategorySetting.Income)
                    continue; //don't show in piechart

                var key = t.UsedCategory.CategoryName;
                chartData[key] += (int)t.Amount;
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
            if (_pieChart.Series.Count > 0)
            {
                (_pieChart.Series[0] as PieSeries).MouseUp -= series_MouseUp;
            }
            _pieChart.DataContext = chartData;
            var series = _pieChart.Series[0] as PieSeries;
            series.MouseUp += series_MouseUp;
        }

        private string getGroupableString(string description)
        {
            if (description.Contains("Kortköp "))
            {
                var parts = description.Split(' ');
                int len = parts[0].Length;
                len += parts[1].Length;
                len += 2;
                var s = description.Substring(len);
                return s;
            }
            return description;
        }

        void series_MouseUp(object sender, MouseButtonEventArgs e)
        {
            var series = sender as PieSeries;
            if (series != null && series.SelectedItem != null)
            {
                var item = (KeyValuePair<string, int>)series.SelectedItem;
                var categoryName = item.Key;
                var category = AvailableViewCategories.Find(a => a.CategoryName == categoryName);

                if (_lastSeriesFilter == category)
                {
                    _lastSeriesFilter = null;
                    setItemCategoryFilter(_lastSeriesFilter);
                }
                else
                {
                    _lastSeriesFilter = category;
                    setItemCategoryFilter(category);
                }
                _categoryDropdown.SelectedItem = _lastSeriesFilter;
                series.SelectedItem = _lastSeriesFilter;
            }
            else
            {
                Console.WriteLine("Unable to find series for clicked item: {0}", sender);
            }
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
            var category = (Category)box.SelectedValue;
            setItemCategoryFilter(category);
        }

        private void setItemCategoryFilter(Category c)
        {
            if (c == null || c == AvailableViewCategories[0])
            {
                _grid.Items.Filter = null;
                return;
            }

            _grid.Items.Filter = a =>
            {
                var item = a as ViewTransaction;
                return item.UsedCategory.Identifier == c.Identifier;
            };
        }

        private void setCategoryForSelectedItems(Category category, bool refreshUI = true) {
            foreach (ViewTransaction t in _grid.SelectedItems) {
                t.UsedCategory = category;
            }
            if(refreshUI)
                refreshUIElements();
        }

        private void CommandBinding_Executed_3(object sender, ExecutedRoutedEventArgs e)
        {

        }

        private void _grid_key(object sender, KeyEventArgs e) {
            if (e.Key != Key.LeftCtrl && Keyboard.IsKeyDown(Key.LeftCtrl) && e.IsRepeat == false) {
                    if(e.Key >= Key.D0 && e.Key <= Key.D9)
                {
                    var c = getCategoryByIndex(e.Key - Key.D0);
                    setCategoryForSelectedItems(c);
                }
            }
            else if (e.Key != Key.LeftAlt && Keyboard.IsKeyDown(Key.LeftAlt) && e.IsRepeat == false)
            {
                switch (e.SystemKey)
                {
                    case Key.Left:
                        {
                            var old = _currentMonthSelector.SelectedIndex;
                            _currentMonthSelector.SelectedIndex = (_currentMonthSelector.SelectedIndex + 11) % 12;
                            if (_currentMonthSelector.SelectedIndex > old)
                                _year.Text = (int.Parse(_year.Text) - 1).ToString();
                            refreshUIElements();
                        }
                        break;
                    case Key.Right:
                        {
                            var old = _currentMonthSelector.SelectedIndex;
                            _currentMonthSelector.SelectedIndex = (_currentMonthSelector.SelectedIndex + 13) % 12;
                            if (_currentMonthSelector.SelectedIndex < old)
                                _year.Text = (int.Parse(_year.Text) + 1).ToString();
                            refreshUIElements();
                        }
                        break;
                }
            }
        }

        private static Category getCategoryByIndex(int idx)
        {
            return BankApplicationState.UserConfig.Categories[idx];
        }

        private void onFilterButtonClick(object sender, RoutedEventArgs e)
        {
            string name = ((FrameworkElement)e.Source).Name;
            _filters.setMarked(name);
            refreshUIElements();
        }

        public void onMessage(IMessage message)
        {
        }

        private List<Category> getCategories()
        {
            return BankApplicationState.UserConfig.Categories;
        }

        public void foobar(object sender, SelectionChangedEventArgs e) {
            if(_initialized)
                refreshUIElements();
        }

        private void onYearChanged(object sender, KeyboardFocusChangedEventArgs e)
        {
            var currYear = int.Parse(_year.Text);
            if(_lastYear != currYear) {
                refreshUIElements();
            }
        }

        private void previewKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Keyboard.Focus(_grid);
            }
        }

        public void CloseScreen()
        {
            MessageManager.removeListener(this);
        }

        public void ShowScreen()
        {
            MessageManager.addListener(this);
        }

        private void onGroupItemsClicked(object sender, RoutedEventArgs e)
        {
            refreshUIElements();
        }
    }
}
