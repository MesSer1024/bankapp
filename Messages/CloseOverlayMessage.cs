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
        internal readonly OverlayTypes OverlayType;

        public UIElement Overlay { get; set; }
    }
}
