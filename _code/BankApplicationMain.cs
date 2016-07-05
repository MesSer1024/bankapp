using BankApp.Messages;
using BankAppLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankApp._code
{
    class BankApplicationMain : IMessageListener
    {
        private ErrorHandler _errorHandler = new ErrorHandler();
        private BankApplicationView _view;

        public BankApplicationMain(MainWindow view)
        {
            MessageManager.addListener(this);
            _view = new BankApplicationView(view);
        }

        ~BankApplicationMain()
        {
            MessageManager.removeListener(this);
        }

        public void onMessage(IMessage message)
        {
            if (message is ApplicationInitializedMessage)
            {
                _view.changeScreen(ScreenTypes.UserSelection);
            }
            else if (message is UserTypeSelectedMessage)
            {
                var msg = message as UserTypeSelectedMessage;
                var config = new UserConfiguration(msg.SelectedUserType);

                ChangeUserConfiguration(config);
                SetupAndLoadDatabase(config);
                ShowTransactionScreen(config);
            }
            else if (message is CloseOverlayMessage)
            {
                var msg = message as CloseOverlayMessage;
                _view.closeOverlay(msg.Overlay);
            } else if (message is SaveTransactionsMessage)
            {
                SaveDatabase();
            }
        }

        private void ChangeUserConfiguration(UserConfiguration config)
        {
            BankApplicationState.AllTransactions.Clear();
            BankApplicationState.UserConfig = config;
            MessageManager.queueMessage(new UserConfigurationChangedMessage());
        }

        private void SetupAndLoadDatabase(UserConfiguration config)
        {
            var file = new FileInfo(Path.Combine(config.SaveFolder, "laststate.mdb"));
            if (file.Exists)
            {
                var transactions = BankApplicationState.Database.Load(file.FullName);
                foreach (var t in transactions)
                {
                    bool add = true;
                    foreach (var a in BankApplicationState.AllTransactions)
                    {
                        if (a.Description == t.Info && a.Date == t.Date && a.Amount == t.Amount)
                        {
                            add = false;
                            break;
                        }
                    }
                    if (add)
                        BankApplicationState.AllTransactions.Add(new ViewTransaction(t));
                }
            }
        }

        private void ShowTransactionScreen(UserConfiguration config)
        {
            _view.changeScreen(ScreenTypes.ShowTransactions);
        }

        private void SaveDatabase()
        {
            var transactions = new List<Transaction>();
            foreach (var t in BankApplicationState.AllTransactions)
            {
                transactions.Add(t.transaction);
            }

            BankApplicationState.Database.Save(transactions, Path.Combine(BankApplicationState.UserConfig.SaveFolder, "LastState.mdb"));
        }
    }
}
