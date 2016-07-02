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
    /// <summary>
    /// Interaction logic for AutoCategorizeScreen.xaml
    /// </summary>
    public partial class AutoCategorizeScreen : UserControl
    {
        private List<ViewTransaction> _allTransactions;

        public AutoCategorizeScreen(List<ViewTransaction> transactions)
        {
            InitializeComponent();
            _allTransactions = transactions;

            var subset = _allTransactions;
            var selIndex = _grid.SelectedIndex;
            foreach (var item in subset)
            {
                setSuggestedCategory(item, subset);
            }
            _grid.ItemsSource = subset;
            selIndex = Math.Max(0, Math.Min(selIndex, subset.Count()));
            _grid.SelectedIndex = selIndex;
            //writeItems(subset);
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

        public void setSuggestedCategory(ViewTransaction transaction, List<ViewTransaction> transactions)
        {
            string desc = transaction.Description;
            int amount = (int)transaction.Amount;
            if (amount > 0)
            {
                SuggestedCategory category = getSuggestedIncomeCategory(desc, amount);
                transaction.Suggested = category;
                return;
            }
            else
            {
                SuggestedCategory category = getSuggestedExpenceCategory(desc, amount);
                transaction.Suggested = category;
                return;
            }
        }

        private SuggestedCategory getSuggestedIncomeCategory(string desc, int amount)
        {
            if (amount <= 0)
                throw new NotImplementedException();

            if (desc.Contains("Överföring"))
            {
                return SuggestedCategory.Incomes;
            }
            return SuggestedCategory.Incomes_Unknown;
        }

        private SuggestedCategory getSuggestedExpenceCategory(string desc, int amount)
        {
            if (amount >= 0)
                throw new NotImplementedException();

            //household - ränta, el, försäkring
            if (desc.Contains("lån") && desc.Contains(" 3997 20 "))
                return SuggestedCategory.Household;
            if (desc.Contains(" PG 938400-9"))
                return SuggestedCategory.Household;
            if (desc.Contains(" TRYGG HANSA "))
                return SuggestedCategory.Household;
            if (desc.Contains(" VATTENFALL "))
                return SuggestedCategory.Household;
            if (desc.Contains(" PG 820004-0")) //telia
                return SuggestedCategory.Household;
            if (desc.Contains(" PG 920003-1")) //tv-licens
                return SuggestedCategory.Household;
            if (desc.Contains("Vardagspaketet")) //avgift visa-kort
                return SuggestedCategory.Household;
            if (desc.Contains("BG 820-0040")) //telia sonera
                return SuggestedCategory.Household;
            if (desc.Contains("BG 230-0176")) //barnvård
                return SuggestedCategory.Household;
            if (desc.Contains("BG 5786-2690")) //folksam
                return SuggestedCategory.Household;
            if (desc.Contains("PG 4131300-8")) //vattenfall
                return SuggestedCategory.Household;
            if (desc.Contains("BG 5014-0045")) //e.on
                return SuggestedCategory.Household;
            if (desc.Contains("BG 5428-5200")) //riksbyggen
                return SuggestedCategory.Household;


            //ammortering
            if (desc.Contains(" PG 607224-3"))
                return SuggestedCategory.Savings;


            //food and regular stuff from super market
            if (desc.Contains(" WILLYS "))
                return SuggestedCategory.Food_Regular;
            if (desc.Contains(" ICA "))
                return SuggestedCategory.Food_Regular;
            if (desc.Contains(" CITY GROSS"))
                return SuggestedCategory.Food_Regular;
            if (desc.Contains(" BÖNOR O BLAD"))
                return SuggestedCategory.Food_Regular;
            if (desc.Contains("TEHORNAN"))
                return SuggestedCategory.Food_Regular;
            if (desc.Contains("HEMKÖP"))
                return SuggestedCategory.Food_Regular;
            if (desc.Contains(" HEMKOP"))
                return SuggestedCategory.Food_Regular;
            if (desc.Contains("COOP "))
                return SuggestedCategory.Food_Regular;


            // eating out and buying 'unregular' food / drinks
            if (desc.Contains("ONLINEPIZZA "))
                return SuggestedCategory.Food_EatingOutAndDrinks;
            if (desc.Contains(" Onlinepizza"))
                return SuggestedCategory.Food_EatingOutAndDrinks;
            if (desc.Contains("PIZZ"))
                return SuggestedCategory.Food_EatingOutAndDrinks;
            if (desc.Contains("SYSTEMBOLAGET"))
                return SuggestedCategory.Food_EatingOutAndDrinks;
            if (desc.Contains("KONDITORI"))
                return SuggestedCategory.Food_EatingOutAndDrinks;
            if (desc.Contains("HBO*NORDIC"))
                return SuggestedCategory.Food_EatingOutAndDrinks;
            if (desc.Contains("RESTAURANT") || desc.Contains("RESTAURANG"))
                return SuggestedCategory.Food_EatingOutAndDrinks;


            //Transportation
            if (desc.Contains(" SJ "))
                return SuggestedCategory.Transportation;
            if (desc.Contains(" EUROPCAR"))
                return SuggestedCategory.Transportation;
            if (desc.Contains(" QPARK "))
                return SuggestedCategory.Transportation;
            if (desc.Contains(" Taxi "))
                return SuggestedCategory.Transportation;
            if (desc.Contains("TÅG"))
                return SuggestedCategory.Transportation;
            if (amount < -225)
            {
                if (desc.Contains(" STATOIL"))
                    return SuggestedCategory.Transportation;
                if (desc.Contains(" QSTAR"))
                    return SuggestedCategory.Transportation;
                if (desc.Contains(" INGO"))
                    return SuggestedCategory.Transportation;
                if (desc.Contains(" OKQ8"))
                    return SuggestedCategory.Transportation;
                if (desc.Contains(" PREEM"))
                    return SuggestedCategory.Transportation;
                if (desc.Contains(" TANKA"))
                    return SuggestedCategory.Transportation;
            }


            //household non-basic stuff
            if (desc.Contains(" IKEA "))
                return SuggestedCategory.Household_Furniture_Medicine_Stuff;
            if (desc.Contains("APOTEK"))
                return SuggestedCategory.Household_Furniture_Medicine_Stuff;


            //Clothes
            if (desc.Contains(" ÅHLENS"))
                return SuggestedCategory.ClothesAndStuff;
            if (desc.Contains(" KAPPAHL"))
                return SuggestedCategory.ClothesAndStuff;
            if (desc.Contains(" H M "))
                return SuggestedCategory.ClothesAndStuff;
            if (desc.Contains("POLARN O PYRET"))
                return SuggestedCategory.ClothesAndStuff;
            if (desc.Contains("LINDEX"))
                return SuggestedCategory.ClothesAndStuff;
            if (desc.Contains("LAGER 157"))
                return SuggestedCategory.ClothesAndStuff;
            if (desc.Contains(" STADIUM"))
                return SuggestedCategory.ClothesAndStuff;
            if (desc.Contains("TEAM SPORTIA"))
                return SuggestedCategory.ClothesAndStuff;


            //fallback mechanics
            return SuggestedCategory.Undefined;
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
                t.Category = t.Suggested;
            }
        }
    }
}
