using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BankAppLib;

namespace BankApp
{
    class InsertTransactionsMessage : IMessage
    {
        private BankAppLib.Transaction[] _transactions;

        public BankAppLib.Transaction[] Transactions
        {
            get { return _transactions; }
        }

        public InsertTransactionsMessage(Transaction[] transactions)
        {
            // TODO: Complete member initialization
            _transactions = transactions;
        }

    }
}
