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
        public static List<ViewTransaction> AllTransactions { get; set; }
        public static List<Category> AllCategories { get; set; }
    }

    /// <summary>
    /// Interaction logic for AutoCategorizeScreen.xaml
    /// </summary>
    public partial class AutoCategorizeScreen : UserControl
    {
        private List<ViewTransaction> _allTransactions;
        private AutoCategorizeViewModel _viewModel;

        public AutoCategorizeScreen(List<ViewTransaction> transactions)
        {
            _allTransactions = transactions;
            _viewModel = new AutoCategorizeViewModel();
            AutoCategorizeViewModel.AllCategories = BankApplicationState.UserConfig.Categories;
            AutoCategorizeViewModel.AllTransactions = _allTransactions;
            InitializeComponent();

            foreach (var item in AutoCategorizeViewModel.AllTransactions)
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
                Category c = getSuggestedCategoryForTransaction(transaction, BankApplicationState.UserConfig.Categories);
                transaction.WantedCategory = c;
            }
        }

        private Category getSuggestedCategoryForTransaction(ViewTransaction transaction, List<Category> categories)
        {
            foreach(var c in categories)
            {
                if (isTransactionPartOfCategoryRules(transaction, c))
                {
                    return c;
                }
            }
            return BankApplicationState.UserConfig.Categories[7]; //övrigt...
        }

        private bool isTransactionPartOfCategoryRules(ViewTransaction transaction, Category c)
        {
            //config.Categories.Add(new Category(Category.CategorySetting.ExcludeEverywhere) { Identifier = config.Categories.Count, CategoryName = "EXCLUDE", Description = "Exclude this transaction from all calculations and pie charts as if it never happened" });
            //config.Categories.Add(new Category() { Identifier = config.Categories.Count, CategoryName = "Boende", Description = "Fasta kostnader för boende, bostadslån, el, vatten, försäkring osv" });
            //config.Categories.Add(new Category() { Identifier = config.Categories.Count, CategoryName = "Boende_extra", Description = "Övriga kostnader som berör hemmet: möbler, gardiner, teknikprylar, idas kläder, mediciner" });
            //config.Categories.Add(new Category() { Identifier = config.Categories.Count, CategoryName = "Transport", Description = "Tågresor, bil, bensin, cykel" });
            //config.Categories.Add(new Category() { Identifier = config.Categories.Count, CategoryName = "Mat", Description = "Mat och produkter inhandlade på ica/willys ..." });
            //config.Categories.Add(new Category() { Identifier = config.Categories.Count, CategoryName = "Mat_extra", Description = "Mat eller dryck relaterat till restauranger, systembolaget, pubbar, fest ..." });
            //config.Categories.Add(new Category() { Identifier = config.Categories.Count, CategoryName = "Nöje", Description = "Minigolf, bio, spa, hotell, utlandsresor, nöjesfält ..." });
            //config.Categories.Add(new Category() { Identifier = config.Categories.Count, CategoryName = "Övrigt", Description = "Engångskostnader eller saker som inte passar på andra ställen: barnvagn, ..." });
            //config.Categories.Add(new Category() { Identifier = config.Categories.Count, CategoryName = "Sparande", Description = "Sparande: ammortering osv" });
            string desc = transaction.Description;
            int amount = (int)transaction.Amount;

            switch (c.Identifier)
            {
                case 1: // boende
                    {
                        if (desc.Contains("lån") && desc.Contains(" 3997 20 "))
                            return true;
                        if (desc.Contains(" PG 938400-9"))
                            return true;
                        if (desc.Contains(" TRYGG HANSA "))
                            return true;
                        if (desc.Contains(" VATTENFALL "))
                            return true;
                        if (desc.Contains(" PG 820004-0")) //telia
                            return true;
                        if (desc.Contains(" PG 920003-1")) //tv-licens
                            return true;
                        if (desc.Contains("Vardagspaketet")) //avgift visa-kort
                            return true;
                        if (desc.Contains("BG 820-0040")) //telia sonera
                            return true;
                        if (desc.Contains("BG 230-0176")) //barnvård
                            return true;
                        if (desc.Contains("BG 5786-2690")) //folksam
                            return true;
                        if (desc.Contains("PG 4131300-8")) //vattenfall
                            return true;
                        if (desc.Contains("BG 5014-0045")) //e.on
                            return true;
                        if (desc.Contains("BG 5428-5200")) //riksbyggen
                            return true;
                        if (desc.Contains("BG 5097-1282")) //trygg hansa
                            return true;
                        if (desc.Contains("BG 802-2220")) //länsförsäkring
                            return true;
                    }
                    break;
                case 2: //boende_extra
                    {
                        if (amount >= 1400) //avoid auto-setting large amounts
                            return false;

                        if (desc.Contains(" CLAS OHLSON"))
                            return true;

                        if (desc.Contains(" ZARA"))
                            return true;
                        if (desc.Contains(" HEMTEX"))
                            return true;

                        if (desc.Contains(" ÅHLENS"))
                            return true;
                        if (desc.Contains(" KAPPAHL"))
                            return true;
                        if (desc.Contains(" H M "))
                            return true;
                        if (desc.Contains("POLARN O PYRET"))
                            return true;
                        if (desc.Contains("LINDEX"))
                            return true;
                        if (desc.Contains("LAGER 157"))
                            return true;
                        if (desc.Contains(" STADIUM"))
                            return true;
                        if (desc.Contains("TEAM SPORTIA"))
                            return true;
                        if (desc.Contains(" IKEA "))
                            return true;
                        if (desc.Contains("APOTEK"))
                            return true;
                    }
                    break;
                case 3: //transport: resor, bil
                    {
                        if (desc.Contains(" SJ "))
                            return true;
                        if (desc.Contains(" EUROPCAR"))
                            return true;
                        if (desc.Contains(" QPARK "))
                            return true;
                        if (desc.Contains(" Taxi "))
                            return true;
                        if (desc.Contains("TÅG"))
                            return true;
                        if (amount < -225)
                        {
                            if (desc.Contains(" STATOIL"))
                                return true;
                            if (desc.Contains(" QSTAR"))
                                return true;
                            if (desc.Contains(" INGO"))
                                return true;
                            if (desc.Contains(" OKQ8"))
                                return true;
                            if (desc.Contains(" PREEM"))
                                return true;
                            if (desc.Contains(" TANKA"))
                                return true;
                        }
                    }
                    break;
                case 4: //mat
                    {
                        if (desc.Contains(" WILLYS "))
                            return true;
                        if (desc.Contains(" ICA "))
                            return true;
                        if (desc.Contains(" CITY GROSS"))
                            return true;
                        if (desc.Contains(" BÖNOR O BLAD"))
                            return true;
                        if (desc.Contains("TEHORNAN"))
                            return true;
                        if (desc.Contains("HEMKÖP"))
                            return true;
                        if (desc.Contains(" HEMKOP"))
                            return true;
                        if (desc.Contains("COOP "))
                            return true;
                        if (desc.Contains(" RIFIFI"))
                            return true;
                        if (desc.Contains(" FORNO ROMANO"))
                            return true;
                        if (desc.Contains(" RIFIFI"))
                            return true;
                    }
                    break;
                case 5: //mat_Extra
                    {
                        if (desc.Contains("ONLINEPIZZA "))
                            return true;
                        if (desc.Contains(" Onlinepizza"))
                            return true;
                        if (desc.Contains("PIZZ"))
                            return true;
                        if (desc.Contains("SYSTEMBOLAGET"))
                            return true;
                        if (desc.Contains("KONDITORI"))
                            return true;
                        if (desc.Contains("RESTAURANT") || desc.Contains("RESTAURANG"))
                            return true;
                    }
                    break;
                case 6: //nöje
                    if (desc.Contains("HBO*NORDIC"))
                        return true;
                    break;
                case 7: //övrigt
                    break;
                case 8: //sparande
                    {
                        if (desc.Contains(" PG 607224-3"))
                            return true;
                    }
                    break;
                default:
                    return false;
            }
            return false;
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
