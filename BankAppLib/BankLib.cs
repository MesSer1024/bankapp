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
            floatInfo.CurrencyDecimalSeparator = ",";
            floatInfo.NumberGroupSeparator = ".";
            int count = 0;

            foreach (var s in lines)
            {
                count++;

                //sanity check
                if (String.IsNullOrEmpty(s))
                    continue;
                var parts = s.Split(',').ToArray();
                var foo = parts.ToList();
                if (parts.Length < 5 || parts.Length > 10)
                {
                    if (_errorHandler != null)
                        _errorHandler.add(new ParseError() { Error = "Splitting line on ',' resulted in too many or too few parts", LineNumber = count, SourceLine = s });
                    continue;
                }

                //remove special case lines [every time a field has a ',' in it, the line starts with "-character]
                if(parts[1].StartsWith("\"")) {
                    foo[1] += " " + foo[2];
                    foo.RemoveAt(2);
                    parts = foo.ToArray();
                }

                var t = new Transaction() { Date = parts[0], Info = parts[1] };
                
                //create line which only contains information related to money part of the transaction [create string and remove everything else]
                var cnt = parts[0].Length + parts[1].Length + parts[2].Length + 3;
                var moneyLine = s.Substring(cnt);
                int amt = 0;
                moneyLine = moneyLine.Replace("\"", "");
                parts = moneyLine.Split(',');
                if (parts.Length < 2 || parts.Length > 4)
                {
                    if(_errorHandler != null)
                        _errorHandler.add(new ParseError() { LineNumber = count, SourceLine = s, Error = "Sanity check on part of line containing transaction amount were wrong, line=" + moneyLine });
                    continue;
                }

                if (int.TryParse(parts[0], NumberStyles.AllowLeadingSign | NumberStyles.AllowThousands, floatInfo, out amt))
                {
                    t.Amount = amt;
                }
                else
                {
                    if (_errorHandler != null)
                        _errorHandler.add(new ParseError() { LineNumber = count, SourceLine = s, Error = String.Format("Unable to convert {0} to a number", parts[0]) });
                    continue;
                }
                transactions.Add(t);
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
