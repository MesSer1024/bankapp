using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankApp.Messages
{
    class UserTypeSelectedMessage : IMessage
    {
        public UserTypeSelectedMessage(UserConfiguration.UserType userType)
        {
            SelectedUserType = userType;
        }

        public UserConfiguration.UserType SelectedUserType { get; private set; }
    }
}
