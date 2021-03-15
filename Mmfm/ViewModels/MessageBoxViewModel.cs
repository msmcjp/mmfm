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
            DefaultResult = MessageBoxResult.None;
        }

        public string Caption
        {
            get;
            set;
        }

        public string Text
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

        public MessageBoxResult DefaultResult
        {
            get;
            set;
        }

        public MessageBoxResult Result
        {
            get;
            set;
        }
    }
}
