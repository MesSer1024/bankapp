using BankApp._code;
using BankAppLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
