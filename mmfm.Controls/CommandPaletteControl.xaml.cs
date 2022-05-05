using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Windows.UI.Core;

namespace Mmfm.Controls
{
    /// <summary>
    /// CommandPaletteControl.xaml の相互作用ロジック
    /// </summary>
    public partial class CommandPaletteControl : UserControl
    {
        public CommandPaletteControl()
        {
            InitializeComponent();
        }

        private void CommandText_PreviewKeyDown(object sender, KeyRoutedEventArgs e)
        {
           
        }

        private void UserControl_GotFocus(object sender, RoutedEventArgs e)
        {
            CommandText.Focus(FocusState.Keyboard);
        }
    }
}
