using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Mmfm
{
    public class MessageBoxViewModel
    {      
        public MessageBoxViewModel()
        {
            Text = "";
            Caption = "";
            Button = MessageBoxButton.OK;
            Icon = MessageBoxImage.None;
            Result = MessageBoxResult.None;
            Options = MessageBoxOptions.None;
        }

        public string Text
        {
            get;
            set;
        }

        public string Caption
        {
            get;
            set;
        }

        public MessageBoxButton Button
        {
            get;
            set;
        }

        public MessageBoxImage Icon
        {
            get;
            set;
        }

        public MessageBoxResult Result
        {
            get;
            set;
        }

        public MessageBoxOptions Options
        {
            get;
            set;
        }
    }
}
