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

namespace MyFileManager
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        private HotKey.HotKey hotKey;

        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = new DualFileManagerViewModel();

            if (hotKey != null)
            {
                hotKey.Pressed -= HotKey_Pressed;
                hotKey.Dispose();
            }
            try
            {
                hotKey = new HotKey.HotKey(this, ModifierKeys.Control, HotKeyConverter.ConvertFromString(";"));
                hotKey.Pressed += HotKey_Pressed;
            }
            catch
            {
                MessageBox.Show("Hot-key is already in use. Please use another key.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void HotKey_Pressed(HotKey.HotKey obj)
        {
            if (IsVisible)
            {
                Hide();
            }
            else
            {
                Show();
                Activate();
            }
        }

        private void ApplicationQuit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void Option_Click(object sender, RoutedEventArgs e)
        {
            new OptionWindow { Owner = this }.ShowDialog();
        }
    }
}
