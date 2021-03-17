using Mmfm.Commands;
using Mmfm.Converters;
using ModernWpf.Controls;
using Msmc.Patterns.Messenger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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
using System.Windows.Threading;

namespace Mmfm
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        private HotKey.HotKey hotKey;
        private Window overlayWindow = new OverlayWindow();

        private async Task<ContentDialogResult> ShowContentDialog(ContentDialog contentDialog)
        {
            // First ContentDialog is shown as InPlace.
            if (ContentDialogBorder.Child == null)
            {
                ContentDialogBorder.Child = contentDialog;
            }

            var result = await contentDialog.ShowAsync(ContentDialogPlacement.InPlace);

            if (ContentDialogBorder.Child == contentDialog)
            {
                ContentDialogBorder.Child = null;
            }

            return result;
        }

        public MainWindow()
        {
            InitializeComponent();

            Messenger.Default.RegisterAsyncMessage<MessageBoxViewModel>(
            this, 
            false,
            async (vm) =>
            {
                await this.Dispatcher.Invoke(async () =>
                {
                    var contentDialog = vm.BuildContentDialog();
                    contentDialog.DataContext = vm;

                    vm.Result = vm.ToMessageBoxResult(await ShowContentDialog(contentDialog));
                });
            });

            Messenger.Default.RegisterAsyncMessage(
            this,
            true,
            (Func<ContentDialogViewModel, Task>)(async (vm) =>
            {                   
                await this.Dispatcher.Invoke(async () =>
                {
                    var dataTemplate = Application.Current.FindResource(new DataTemplateKey(vm.GetType())) as DataTemplate;
                    if(dataTemplate == null)
                    {
                        return;
                    }

                    var contentDialog = (ContentDialog)dataTemplate.LoadContent();
                    contentDialog.DataContext = vm;

                    vm.Result = await ShowContentDialog(contentDialog);
                });                   
            }));

            Messenger.Default.Register<OverlayViewModel>(this, (vm) =>
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

        private ICommand changeThemeCommand;
        public ICommand ChangeThemeCommand
        {
            get
            {
                if(changeThemeCommand == null)
                {
                    changeThemeCommand = new RelayCommand<ModernWpf.ApplicationTheme>((theme) =>
                    {
                        ModernWpf.ThemeManager.Current.ApplicationTheme = theme;
                    });
                }
                return changeThemeCommand;
            }
        }

        private ICommand changeAccentColorCommand;
        public ICommand ChangeAccentColorCommand
        {
            get
            {
                if (changeAccentColorCommand == null)
                {
                    changeAccentColorCommand = new AsyncRelayCommand<string>(async (color) =>
                    {
                        try
                        {
                            ModernWpf.ThemeManager.Current.AccentColor = (Color)ColorConverter.ConvertFromString(color);
                        }
                        catch (FormatException)
                        {
                            await Messenger.Default.SendAsync(new MessageBoxViewModel
                            {
                                Caption = "Accent color is invalid.",
                                Text = $"{color} is invalid color format.",
                                Button = MessageBoxButton.OK                                
                            });
                        }
                    });
                }
                return changeAccentColorCommand;
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
