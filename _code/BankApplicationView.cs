using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BankAppLib;
using BankApp.Screens;
using System.Windows;

namespace BankApp._code
{
    public enum ScreenTypes
    {
        UserSelection,
        ShowTransactions,
    }

    public enum OverlayTypes
    {
        EditTransactionCategories,
        ParseTransactions,
        ModifyTransactions,
    }

    class BankApplicationView
    {
        private MainWindow _view;

        public BankApplicationView(MainWindow view)
        {
            _view = view;
        }

        public void changeScreen(ScreenTypes screenId)
        {
            //teardown previous screen
            foreach (IBankScreen c in _view._content.Children)
                c.CloseScreen();
            _view._content.Children.Clear();

            //create screen based on type
            switch (screenId)
            {
                case ScreenTypes.UserSelection:
                    _view._content.Children.Add(new UserSelection());
                    break;
                case ScreenTypes.ShowTransactions:
                    _view._content.Children.Add(new ShowTransactionsScreen(BankApplicationState.AllTransactions));
                    break;
            }

            //enable new screen
            foreach (IBankScreen c in _view._content.Children)
                c.ShowScreen();
        }

        public void showOverlay(OverlayTypes overlayId)
        {

        }

        public void closeOverlay(UIElement overlayId)
        {
            _view._content.Children.Remove(overlayId);
        }
    }
}
