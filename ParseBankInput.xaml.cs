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
using BankAppLib;

namespace BankApp
{
    /// <summary>
    /// Interaction logic for ParseBankInput.xaml
    /// </summary>
    public partial class ParseBankInput : UserControl
    {
        public ParseBankInput()
        {
            InitializeComponent();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            var errorHandler = new ErrorHandler();
            var lib = new BankLib(errorHandler);
            var transactions = new List<Transaction>();
            var sr = new StringReader(_input.Text);
            string line;
            var strings = new List<string>();
            while((line = sr.ReadLine()) != null) {
                strings.Add(line);
            }
            lib.parseTransactions(strings.ToArray(), ref transactions);
            if (errorHandler.getParseErrors().Count > 0)
            {
                var sb = new StringBuilder();
                errorHandler.getParseErrors().ForEach(error =>
                {
                    sb.AppendLine(error.Error + " | " + error.SourceLine);
                });
                MessageBox.Show(sb.ToString(), "Parsing Error");
            }
            if (transactions.Count > 0)
            {
                MessageManager.queueMessage(new InsertTransactionsMessage(transactions.ToArray()));
                MessageManager.queueMessage(new CloseOverlayMessage() { Overlay = this });
                MessageBox.Show(String.Format("Added {0} items to transaction database", transactions.Count));
            }
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            MessageManager.queueMessage(new CloseOverlayMessage() { Overlay = this });
        }
    }
}
