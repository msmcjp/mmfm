﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MyFileManager
{
    public class FocusCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return parameter is FrameworkElement;
        }

        public void Execute(object parameter)
        {
            (parameter as FrameworkElement).Focus();
        }
    }
}
