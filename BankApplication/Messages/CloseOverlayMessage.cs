using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using BankApp._code;

namespace BankApp
{
    class CloseOverlayMessage : IMessage
    {
        public UIElement Overlay { get; set; }
    }
}
