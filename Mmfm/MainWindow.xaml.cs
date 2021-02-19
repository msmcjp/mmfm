using Mmfm.Commands;
using Mmfm.Converters;
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

            Msmc.Patterns.Messenger.Messenger.Default.Register<MessageBoxViewModel>(this, (vm) =>
            {
                vm.Result = MessageBox.Show(this, vm.Text, vm.Caption, vm.Button, vm.Icon, vm.Result, vm.Options);
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

            RestoreWindow();
        }

        public void SaveWindow()
        {
            var defaults = Properties.Settings.Default;
            defaults.WindowLocation = new System.Drawing.Point((int)Left, (int)Top);
            defaults.WindowSize = new System.Drawing.Size((int)Width, (int)Height);
            defaults.GridSize1 = new System.Drawing.Size(
                (int)Grid.ColumnDefinitions[0].ActualWidth,
                (int)First.Grid.RowDefinitions[1].ActualHeight
            );
            defaults.GridSize2 = new System.Drawing.Size(
                (int)Grid.ColumnDefinitions[2].ActualWidth,
                (int)Second.Grid.RowDefinitions[1].ActualHeight
            );
            defaults.Save();
        }

        public void RestoreGrid()
        {
            var defaults = Properties.Settings.Default;

            var gridPosition1 = defaults.GridSize1;
            if (gridPosition1.Width > 0)
            {
                Grid.ColumnDefinitions[0].Width = new GridLength(
                    gridPosition1.Width,
                    GridUnitType.Star
                );
            }
            if (gridPosition1.Height > 0)
            {
                First.Grid.RowDefinitions[1].Height = new GridLength(
                    gridPosition1.Height,
                    GridUnitType.Star
                );

                First.Grid.RowDefinitions[3].Height = new GridLength(
                    First.Grid.RowDefinitions.Take(3).Sum(x => x.ActualHeight),
                    GridUnitType.Star
                );
            }

            var gridPosition2 = defaults.GridSize2;
            if (gridPosition2.Width > 0)
            {
                Grid.ColumnDefinitions[2].Width = new GridLength(
                    gridPosition2.Width,
                    GridUnitType.Star
                );
            }
            if (gridPosition2.Height > 0)
            {
                Second.Grid.RowDefinitions[1].Height = new GridLength(
                    gridPosition2.Height,
                    GridUnitType.Star
                );

                Second.Grid.RowDefinitions[3].Height = new GridLength(
                    Second.Grid.RowDefinitions.Take(3).Sum(x => x.ActualHeight),
                    GridUnitType.Star
                );
            }
        }

        private void RestoreWindow()
        {
            var defaults = Properties.Settings.Default;
            var location = defaults.WindowLocation;
            var size = defaults.WindowSize;
            if(location.X >= 0 && location.Y >= 0)
            {
                Left = location.X;
                Top = location.Y;
                Width = size.Width;
                Height = size.Height;
            }
        }

        private ICommand registerHotKeyCommand;
        public ICommand RegisterHotKeyCommand
        {
            get
            {
                if(registerHotKeyCommand == null)
                {
                    registerHotKeyCommand = new RelayCommand<string>((keyDefinition) =>
                    {
                        Show();
                        Activate();
                        hotKey?.Dispose();
                        try
                        {
                            var keyGesture = (KeyGesture)new ExKeyGestureConverter().ConvertFromString(keyDefinition);
                            hotKey = new HotKey.HotKey(this, keyGesture);
                            hotKey.Pressed += (o) =>
                            {
                                if (IsActive)
                                {
                                    Hide();
                                }
                                else
                                {
                                    Show();
                                    Activate();
                                }
                            };
                        }
                        catch (NotSupportedException)
                        {
                            MessageBox.Show("Hot-key is not supported.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                        catch (ArgumentException)
                        {
                            MessageBox.Show("Invalid Hot-key definition.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                        catch
                        {
                            MessageBox.Show("Hot-key is already in use. Please use another key.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                        }
                    });
                }
                return registerHotKeyCommand;
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
