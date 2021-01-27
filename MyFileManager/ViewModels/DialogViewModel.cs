using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;

namespace MyFileManager
{
    public class DialogViewModel
    {
        public string Title { get; set; }

        public object Content { get; set; }

        public bool? Result { get; set; }

        public DialogViewModel()
        {

        }
    }
}
