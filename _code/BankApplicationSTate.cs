using BankApp._code;
using BankAppLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static BankApp._code.RuleGenerator;

namespace BankApp
{
    public class Category
    {
        public enum CategorySetting
        {
            Default,
            Income,
            ExcludeEverywhere,
            ALL,
        }

        public CategorySetting Setting { get; private set; }
        public int Identifier { get; set; }
        public string CategoryName { get; set; }
        public string Description { get; set; }
        public Category(CategorySetting setting = CategorySetting.Default)
        {
            Setting = setting;
        }

        public override string ToString()
        {
            return "fii";
        }
    }

    public class UserConfiguration
    {
        public enum UserType
        {
            Shared,
            Daniel,
            //Anna,
        }

        public UserType SettingMode { get; private set; }
        public List<Category> Categories { get; private set; }
        public List<IKeywordRule> Rules { get; private set; }     
        public string SaveFolder { get; private set; }

        public UserConfiguration(UserType userType)
        {
            SettingMode = userType;
            switch (userType)
            {
                case UserType.Shared:
                    setupShared(this);
                    break;
                default:
                case UserType.Daniel:
                    setupUser1(this);
                    break;
            }
        }

        private static void setupShared(UserConfiguration config)
        {
            config.Categories = new List<Category>();
            config.Categories.Add(new Category(Category.CategorySetting.ExcludeEverywhere) { Identifier = config.Categories.Count, CategoryName = "EXCLUDE", Description = "Exclude this transaction from all calculations and pie charts as if it never happened" });
            config.Categories.Add(new Category() { Identifier = config.Categories.Count, CategoryName = "Boende", Description = "Fasta kostnader för boende, bostadslån, el, vatten, försäkring osv" });
            config.Categories.Add(new Category() { Identifier = config.Categories.Count, CategoryName = "Boende_extra", Description = "Övriga kostnader som berör hemmet: möbler, gardiner, teknikprylar, idas kläder, mediciner" });
            config.Categories.Add(new Category() { Identifier = config.Categories.Count, CategoryName = "Transport", Description = "Tågresor, bil, bensin, cykel" });
            config.Categories.Add(new Category() { Identifier = config.Categories.Count, CategoryName = "Mat", Description = "Mat och produkter inhandlade på ica/willys ..." });
            config.Categories.Add(new Category() { Identifier = config.Categories.Count, CategoryName = "Mat_extra", Description = "Mat eller dryck relaterat till restauranger, systembolaget, pubbar, fest ..." });
            config.Categories.Add(new Category() { Identifier = config.Categories.Count, CategoryName = "Nöje", Description = "Minigolf, bio, spa, hotell, utlandsresor, nöjesfält ..." });
            config.Categories.Add(new Category() { Identifier = config.Categories.Count, CategoryName = "Övrigt", Description = "Engångskostnader eller saker som inte passar på andra ställen: barnvagn, ..." });
            config.Categories.Add(new Category() { Identifier = config.Categories.Count, CategoryName = "Sparande", Description = "Sparande: ammortering osv" });
            config.Categories.Add(new Category(Category.CategorySetting.Income) { Identifier = config.Categories.Count, CategoryName = "INKOMST", Description = "Alla inkomster på ett och samma ställe" });

            config.SaveFolder = "./output/shared/";


            config.Rules = new List<IKeywordRule>();

            {
                //boende
                var cat = config.Categories.Find(a => a.Identifier == 1);
                var living = config.Rules;
                living.Add(new KeywordRule(" 3997 20 52737", cat));
                living.Add(new KeywordRule(" PG 938400-9", cat));
                living.Add(new KeywordRule(" TRYGG HANSA", cat));
                living.Add(new KeywordRule(" VATTENFALL ", cat));
                living.Add(new KeywordRule(" PG 820004-0", cat)); //telia
                living.Add(new KeywordRule(" PG 920003-1", cat)); //tv-licens
                living.Add(new KeywordRule("Vardagspaketet", cat)); //kortavgift
                living.Add(new KeywordRule("BG 820-0040", cat)); //telia
                living.Add(new KeywordRule("BG 230-0176", cat)); //barnvård
                living.Add(new KeywordRule("BG 5786-2690", cat)); //folksam
                living.Add(new KeywordRule("PG 4131300-8", cat)); //vattenfall
                living.Add(new KeywordRule("BG 5014-0045", cat)); //e.on
                living.Add(new KeywordRule("BG 5428-5200", cat)); //riksbyggen
                living.Add(new KeywordRule("BG 5097-1282", cat)); //trygg hansa
                living.Add(new KeywordRule("BG 802-2220", cat)); //länsförsäkring
            }

            {
                //boende_extra
                var cat = config.Categories.Find(a => a.Identifier == 2);
                var living_extra = config.Rules;
                living_extra.Add(new KeywordRule(" CLAS OHLSON", cat));
                living_extra.Add(new KeywordRule(" ZARA", cat));
                living_extra.Add(new KeywordRule(" HEMTEX", cat));
                living_extra.Add(new KeywordRule(" ÅHLENS", cat));
                living_extra.Add(new KeywordRule(" KAPPAHL", cat));
                living_extra.Add(new KeywordRule(" H M ", cat));
                living_extra.Add(new KeywordRule(" POLARN O PYRET", cat));
                living_extra.Add(new KeywordRule(" LINDEX", cat));
                living_extra.Add(new KeywordRule("LAGER 157", cat));
                living_extra.Add(new KeywordRule(" STADIUM", cat));
                living_extra.Add(new KeywordRule("TEAM SPORTIA", cat));
                living_extra.Add(new KeywordRule(" IKEA ", cat));
                living_extra.Add(new KeywordRule("APOTEK", cat));
            }
            {
                //TRANSPORT: RESOR, BIL ...
                var cat = config.Categories.Find(a => a.Identifier == 3);
                var transport = config.Rules;
                transport.Add(new KeywordRule(" SJ ", cat));
                transport.Add(new KeywordRule(" EUROPCAR", cat));
                transport.Add(new KeywordRule(" QPARK", cat));
                transport.Add(new KeywordRule(" Taxi ", cat));
                transport.Add(new KeywordRule("TÅG", cat));
                transport.Add(new MathKeywordRule(" STATOIL", cat, -225, true));
                transport.Add(new MathKeywordRule(" QSTAR", cat, -225, true));
                transport.Add(new MathKeywordRule(" INGO", cat, -225, true));
                transport.Add(new MathKeywordRule(" OKQ8", cat, -225, true));
                transport.Add(new MathKeywordRule(" PREEM", cat, -225, true));
                transport.Add(new MathKeywordRule(" TANKA", cat, -225, true));
            }

            {
                //Mat
                var cat = config.Categories.Find(a => a.Identifier == 4);
                var food = config.Rules;
                food.Add(new KeywordRule(" WILLYS ", cat));
                food.Add(new KeywordRule(" ICA ", cat));
                food.Add(new KeywordRule(" CITY GROSS", cat));
                food.Add(new KeywordRule(" BÖNOR O BLAD", cat));
                food.Add(new KeywordRule("TEHORNAN", cat));
                food.Add(new KeywordRule("HEMKÖP", cat));
                food.Add(new KeywordRule(" HEMKOP", cat));
                food.Add(new KeywordRule("COOP ", cat));
            }

            {
                //mat_Extra
                var cat = config.Categories.Find(a => a.Identifier == 5);
                var food_extra = config.Rules;
                food_extra.Add(new KeywordRule("ONLINEPIZZA ", cat));
                food_extra.Add(new KeywordRule(" Onlinepizza", cat));
                food_extra.Add(new KeywordRule(" RIFIFI", cat));
                food_extra.Add(new KeywordRule(" FORNO ROMANO", cat));
                food_extra.Add(new KeywordRule("PIZZ", cat));
                food_extra.Add(new KeywordRule("SYSTEMBOLAGET", cat));
                food_extra.Add(new KeywordRule("KONDITORI", cat));
                food_extra.Add(new KeywordRule("RESTAURANT", cat));
                food_extra.Add(new KeywordRule("RESTAURANG", cat));
            }

            {
                //nöje
                var cat = config.Categories.Find(a => a.Identifier == 6);
                var fun = config.Rules;
                fun.Add(new KeywordRule("HBO*NORDIC", cat));

            }
            {
                //övrigt
                var cat = config.Categories.Find(a => a.Identifier == 7);
                var other = config.Rules;
            }
            {
                //sparande
                var cat = config.Categories.Find(a => a.Identifier == 8);
                var savings = config.Rules;
                savings.Add(new KeywordRule(" PG 607224-3", cat));
            }
        }

        private static void setupUser1(UserConfiguration config)
        {
            config.Categories = new List<Category>();
            config.Categories.Add(new Category(Category.CategorySetting.ExcludeEverywhere) { Identifier = config.Categories.Count, CategoryName = "EXCLUDE", Description = "Exclude this transaction from all calculations and pie charts as if it never happened" });
            config.Categories.Add(new Category() { Identifier = config.Categories.Count, CategoryName = "Levnadskostnad", Description = "Kostnader svåra att undvika: mobilabbonnemang, linser, läkarvård, mediciner" });
            config.Categories.Add(new Category() { Identifier = config.Categories.Count, CategoryName = "Luncher", Description = "Äta ute på dagtid, dvs utan middag & alkohol" });
            config.Categories.Add(new Category() { Identifier = config.Categories.Count, CategoryName = "Fest", Description = "Middag, alkohol, hotell vid resor osv - SUPANDE" });
            config.Categories.Add(new Category() { Identifier = config.Categories.Count, CategoryName = "Nöje", Description = "Spel, musik-abonnemang, anime-abb osv" });
            config.Categories.Add(new Category() { Identifier = config.Categories.Count, CategoryName = "Övrigt", Description = "Engångskostnader eller saker som inte passar på andra ställen" });
            config.Categories.Add(new Category() { Identifier = config.Categories.Count, CategoryName = "Kläder", Description = "Kläder, skor ..." });
            config.Categories.Add(new Category() { Identifier = config.Categories.Count, CategoryName = "Lån", Description = "Privatlån, studielån eller andra lån" });
            config.Categories.Add(new Category() { Identifier = config.Categories.Count, CategoryName = "Sparande", Description = "Sparande:" });
            config.Categories.Add(new Category(Category.CategorySetting.Income) { Identifier = config.Categories.Count, CategoryName = "INKOMST", Description = "Alla inkomster på ett och samma ställe" });

            config.SaveFolder = "./output/user1/";
        }
    }

    public static class BankApplicationState
    {
        public static UserConfiguration UserConfig { get; set; }
        public static BankAppLib.BankLib Database { get; private set; }
        public static List<ViewTransaction> AllTransactions { get; private set; }
        public static ErrorHandler ErrorThingy { get; private set; }
        static BankApplicationState()
        {
            AllTransactions = new List<ViewTransaction>();
            ErrorThingy = new ErrorHandler();
            Database = new BankLib(ErrorThingy);
        }
    }
}
