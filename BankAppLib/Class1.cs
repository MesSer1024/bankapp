using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BankAppLib
{
    public class Transaction
    {
        public String Info { get; set; }
        public String Date { get; set; }
        public double Amount { get; set; }
        public int Category { get; set; }

        public override string ToString()
        {
            return String.Format("{0}\t{1}\t{2}\t{3}", Date, Info, Category, Amount);
        }
    }

    public class Class1
    {
        public void parseFile(string path, ref List<Transaction> transactions)
        {
            var file = new FileInfo(path);
            Debug.Assert(file.Exists);
            var floatInfo = new NumberFormatInfo();
            floatInfo.NegativeSign = "-";
            floatInfo.CurrencyDecimalSeparator = ",";
            floatInfo.NumberGroupSeparator = ".";

            var lines = File.ReadLines(file.FullName);
            var sb = new StringBuilder();
            foreach (var s in lines)
            {
                if (String.IsNullOrEmpty(s))
                    continue;
                var parts = s.Split(',').ToArray();
                var foo = parts.ToList();
                if (parts.Length < 5 || parts.Length > 10)
                {
                    sb.AppendLine("Failed parsing line: " + s);
                    continue;
                }

                if(parts[1].StartsWith("\"")) {
                    foo[1] += " " + foo[2];
                    foo.RemoveAt(2);
                    parts = foo.ToArray();
                }
                var t = new Transaction() { Date = parts[0], Info = parts[1] };
                var cnt = parts[0].Length + parts[1].Length + parts[2].Length + 3;
                var line = s.Substring(cnt);
                int amt = 0;
                line = line.Replace("\"", "");
                parts = line.Split(',');
                if (parts.Length < 2 || parts.Length > 4)
                {
                    sb.AppendLine("Failed parsing amount parts: " + s);
                    continue;
                }

                if (int.TryParse(parts[0], NumberStyles.AllowLeadingSign | NumberStyles.AllowThousands, floatInfo, out amt))
                {
                    t.Amount = amt;
                }
                else
                {
                    sb.AppendLine("Failed parsing amount: " + s);
                    continue;
                }
                transactions.Add(t);
            }
            Console.WriteLine("Errors: \n", sb.ToString());
            int asdf = transactions.Count;
        }
    }
}
