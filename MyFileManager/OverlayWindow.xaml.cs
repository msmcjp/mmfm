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
using System.Windows.Shapes;

namespace MyFileManager
{
    /// <summary>
    /// OverlayWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class OverlayWindow : Window
    {
        public OverlayWindow()
        {
            InitializeComponent();
        }

        private void Window_Deactivated(object sender, EventArgs e)
        {
            Hide();
        }

        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            // Hide with ESC key
            if(e.Key == Key.Escape)
            {
                Hide();
            }
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            // Prevent to close window with Alt+F4
            if (e.Key == Key.System && e.SystemKey == Key.F4)
            {
                e.Handled = true;
            }
        }
    }
}
