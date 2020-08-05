using BankApp._code;
using BankAppLib;
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
using System.Windows.Shapes;

namespace BankApp
{
    public class AutoCategorizeViewModel
    {
        public static List<ViewTransaction> TransactionsToCategorize { get; set; }
        public static List<Category> AllCategories { get; set; }
    }

    /// <summary>
    /// Interaction logic for AutoCategorizeScreen.xaml
    /// </summary>
    public partial class AutoCategorizeScreen : UserControl
    {
        private List<ViewTransaction> _transactionsToCategorize;
        private AutoCategorizeViewModel _viewModel;

        public AutoCategorizeScreen(List<ViewTransaction> transactions/*, Dictionary<Category, List<IKeywordRule>> mappingDictionary*/)
        {
            _transactionsToCategorize = transactions;
            _viewModel = new AutoCategorizeViewModel();
            AutoCategorizeViewModel.AllCategories = BankApplicationState.UserConfig.Categories;
            AutoCategorizeViewModel.TransactionsToCategorize = _transactionsToCategorize;

            InitializeComponent();

            foreach (var item in AutoCategorizeViewModel.TransactionsToCategorize)
            {
                setSuggestedCategory(item);
            }
            _grid.DataContext = _viewModel;
        }

        private void writeItems(List<ViewTransaction> subset)
        {
            var sb = new StringBuilder();
            foreach (var item in subset)
            {
                sb.AppendFormat("Desc: {0}\t\t\t Amount: {1}\n", item.Description, item.Amount);
            }
            File.WriteAllText("./output/everything.txt", sb.ToString());
        }

        private void onClose(object sender, RoutedEventArgs e)
        {
            MessageManager.queueMessage(new CloseOverlayMessage() { Overlay = this });
        }


        public void setSuggestedCategory(ViewTransaction transaction)
        {
            if (transaction.Amount > 0)
            {
                var c = BankApplicationState.UserConfig.Categories.Find(a => a.Setting == Category.CategorySetting.Income);
                transaction.WantedCategory = c;
                return;
            }
            else
            {
                Category c = getSuggestedCategoryForTransaction(transaction);
                if (c == null)
                    c = BankApplicationState.UserConfig.Categories[7]; //övrigt
                transaction.WantedCategory = c;
            }
        }

        private Category getSuggestedCategoryForTransaction(ViewTransaction transaction)
        {
            var rules = BankApplicationState.UserConfig.Rules;
            foreach(var rule in rules)
            {
                if(rule.IsRuleTriggeredByTransaction(transaction))
                {
                    return rule.TargetCategory;
                }
            }
            return null;
        }

        private void onAcceptClicked(object sender, RoutedEventArgs e)
        {
            var items = _grid.SelectedCells;
            var sb = new StringBuilder();
            foreach (var item in items)
            {
                var t = item.Item as ViewTransaction;
                if (t == null)
                    throw new Exception();
                t.UsedCategory = t.WantedCategory;
            }
        }
    }
}
