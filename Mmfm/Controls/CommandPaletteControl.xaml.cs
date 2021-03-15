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

namespace Mmfm
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

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            CommandText.Focus();
        }

        private void CommandText_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            var count = CommandsList.ItemContainerGenerator.Items.Count;
            if(count == 0)
            {
                return;
            }
            var delta = 0;
            if(e.Key == Key.Up) { delta = -1; e.Handled = true; }
            if(e.Key == Key.Down) { delta = 1; e.Handled = true; }
            CommandsList.SelectedIndex = (CommandsList.SelectedIndex + delta + count) % count;
            CommandsList.ScrollIntoView(CommandsList.SelectedItem);
        }
    }
}
