using MyFileManager.Converters;
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
        private Window overlayWindow = new OverlayWindow();
        private Stack<Window> dialogStack = new Stack<Window>();

        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = new MainViewModel();          

            try
            {
                hotKey = new HotKey.HotKey(this, ModifierKeys.Control, HotKeyConverter.ConvertFromString(";"));
                hotKey.Pressed += HotKey_Pressed;
            }
            catch
            {
                MessageBox.Show("Hot-key is already in use. Please use another key.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

            Msmc.Patterns.Messenger.Messenger.Default.Register<MessageBoxViewModel>(this, (vm) =>
            {
                MessageBox.Show(this, vm.Text, vm.Caption, vm.Button, vm.Icon, vm.Result, vm.Options);
            });
            
            Msmc.Patterns.Messenger.Messenger.Default.Register<DialogViewModel>(this, (vm) =>
            {
                this.Dispatcher.Invoke(() =>
                {
                    var owner = (dialogStack.Count == 0) ? this : dialogStack.Peek();
                    var window = new DialogWindow { Owner = owner, DataContext = vm };
                    dialogStack.Push(window);
                    vm.Result = window.ShowDialog();
                    dialogStack.Pop();
                });
            });

            Msmc.Patterns.Messenger.Messenger.Default.Register<OverlayViewModel>(this, (vm) =>
            {
                overlayWindow.DataContext = vm.Content;
                overlayWindow.Show();
                overlayWindow.Top = this.Top + SystemParameters.WindowCaptionHeight;
                overlayWindow.Left = this.Left + this.Width / 2 - overlayWindow.Width / 2;
            });
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

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            overlayWindow.Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            First.DirectoryList.Focus();
        }
    }
}
