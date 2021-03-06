﻿using BankApp.Messages;
using System;
using System.Collections.Generic;
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

namespace BankApp.Screens
{
    /// <summary>
    /// Interaction logic for UserSelection.xaml
    /// </summary>
    public partial class UserSelection : UserControl, IBankScreen
    {
        public UserSelection()
        {
            InitializeComponent();
        }

        private void onUser1Clicked(object sender, RoutedEventArgs e)
        {
            MessageManager.queueMessage(new UserTypeSelectedMessage(UserConfiguration.UserType.Daniel));
        }

        private void onSharedClick(object sender, RoutedEventArgs e)
        {
            MessageManager.queueMessage(new UserTypeSelectedMessage(UserConfiguration.UserType.Shared));
        }

        public void CloseScreen()
        {
        }

        public void ShowScreen()
        {
        }
    }
}
