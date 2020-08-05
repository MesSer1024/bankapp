using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json;

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

    public class ErrorHandler
    {
        private List<ParseError> _errors;
        public ErrorHandler()
        {
            _errors = new List<ParseError>();
        }

        public void add(ParseError error)
        {
            _errors.Add(error);
        }

        public void clear()
        {
            _errors.Clear();
        }

        public List<ParseError> getParseErrors()
        {
            return _errors;
        }
    }

    public class ParseError
    {
        public string SourceLine { get; set; }
        public int LineNumber { get; set; }
        public string Error { get; set; }
    }

    public class BankLib
    {
        private ErrorHandler _errorHandler;

        public BankLib(ErrorHandler errorHandler)
        {
            _errorHandler = errorHandler;
        }

        private class SaveDataState
        {
            public DateTime LastModification { get; set; }
            public List<Transaction> Transactions { get; set; }
        }

        public void parseTransactions(string[] lines, ref List<Transaction> transactions)
        {
            var floatInfo = new NumberFormatInfo();
            floatInfo.NegativeSign = "-";
            //floatInfo.CurrencyDecimalSeparator = ",";
            floatInfo.NumberDecimalSeparator = ",";
            floatInfo.NumberGroupSeparator = ".";
            int count = 0;

            String replacedDate = "";

            foreach (var s in lines)
            {
                count++;

                //sanity check
                if (String.IsNullOrEmpty(s))
                    continue;
                var parts = s.Split(';').ToArray();
                var foo = parts.ToList();
                if (parts.Length != 8)
                {
                    //[0]	"Bokföringsdag"	string
                    //[1]	"Belopp"	string
                    //[2]	"Avsändare"	string
                    //[3]	"Mottagare"	string
                    //[4]	"Namn"	string
                    //[5]	"Rubrik"	string
                    //[6]	"Saldo"	string
                    //[7]	"Valuta"	string

                    if (_errorHandler != null)
                        _errorHandler.add(new ParseError() { Error = "Splitting line on ';' resulted in too many or too few parts", LineNumber = count, SourceLine = s });
                    continue;
                }

                //remove special case lines [every time a field has a ',' in it, the line starts with "-character]

                var date = parts[0];
                if (date.Contains("Invalid"))
                {
                    date = replacedDate;
                    if (date.Contains("Invalid"))
                        date = "2020-01-01";
                }


                var t = new Transaction() { Date = date, Info = parts[5] };

                //create line which only contains information related to money part of the transaction [create string and remove everything else]

                var moneyLine = parts[1].Split(',')[0];
                int amt = 0;
                if (int.TryParse(moneyLine, NumberStyles.AllowLeadingSign | NumberStyles.AllowThousands, floatInfo, out amt))
                {
                    t.Amount = amt;
                }
                else
                {
                    if (_errorHandler != null)
                        _errorHandler.add(new ParseError() { LineNumber = count, SourceLine = s, Error = String.Format("Unable to convert {0} to a number", moneyLine) });
                    continue;
                }
                transactions.Add(t);

                replacedDate = date;
            }

        }

        public void Save(List<Transaction> transactions, string path = "./output/LastState.mdb")
        {
            var state = new SaveDataState() { Transactions = transactions, LastModification = DateTime.Now };
            var jsonText = JsonConvert.SerializeObject(state);

            var file = new FileInfo(path);
            if (!file.Directory.Exists)
                file.Directory.Create();

            using (var sw = new StreamWriter(file.FullName, false))
            {
                sw.Write(jsonText);
                sw.Flush();
            }
            File.Copy(file.FullName, Path.Combine(file.Directory.FullName, "state_" + DateTime.Now.Ticks + ".mdb"));
        }

        public List<Transaction> Load(string path = "./output/LastState.mdb")
        {
            var state = JsonConvert.DeserializeObject<SaveDataState>(File.ReadAllText(path));
            return state.Transactions;
        }
    }
}
