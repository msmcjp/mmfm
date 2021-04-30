using Mmfm.Commands;
using Mmfm.Converters;
using ModernWpf.Controls;
using Msmc.Patterns.Messenger;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using Windows.Services.Store;
using WinRT;

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

            var viewModel = DataContext as MainViewModel;

            hotKey?.Dispose();
            viewModel.IsShowingContentDialog = true;
            var currentElement = FocusManager.GetFocusedElement(this);

            contentDialog.Focus();
            var result = await contentDialog.ShowAsync(ContentDialogPlacement.InPlace);

            TryRegisterHotKey(viewModel.Settings.HotKey, out hotKey);
            viewModel.IsShowingContentDialog = false;
            currentElement?.Focus();

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
                    if (dataTemplate == null)
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
            if (location.X >= 0 && location.Y >= 0)
            {
                Left = location.X;
                Top = location.Y;
                Width = size.Width;
                Height = size.Height;
            }
        }

        private bool TryRegisterHotKey(string keyGestureString, out HotKey.HotKey hotKey)
        {
            try
            {
                hotKey = RegisterHotKey(keyGestureString);
                return true;
            }
            catch
            {
                hotKey = null;
                return false;
            }
        }

        private HotKey.HotKey RegisterHotKey(string keyGestureString)
        {
            var keyGesture = (KeyGesture)new KeyGestureConverter().ConvertFromString(keyGestureString);
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

            return hotKey;
        }

        private async Task RegisterHotKeyAsync(string keyGestureString)
        {
            try
            {
                hotKey = RegisterHotKey(keyGestureString);
            }
            catch (NotSupportedException)
            {
                var messageBox = new MessageBoxViewModel
                {
                    Caption = "Hot-key is not supported.",
                    Button = MessageBoxButton.OK,
                    Icon = MessageBoxImage.Error,
                };
                await Messenger.Default.SendAsync(messageBox);
            }
            catch (ArgumentException)
            {
                var messageBox = new MessageBoxViewModel
                {
                    Caption = "Invalid Hot-key definition.",
                    Text = $"{keyGestureString} is invalid format.",
                    Button = MessageBoxButton.OK,
                    Icon = MessageBoxImage.Error,
                };
                await Messenger.Default.SendAsync(messageBox);
            }
            catch
            {
                var messageBox = new MessageBoxViewModel
                {
                    Caption = "Hot-key is already in use. Please use another key.",
                    Button = MessageBoxButton.OK,
                    Icon = MessageBoxImage.Error,
                };
                await Messenger.Default.SendAsync(messageBox);
            }
        }

        private ICommand updateSettingsCommand;
        public ICommand UpdateSettingsCommand
        {
            get
            {
                if (updateSettingsCommand == null)
                {
                    updateSettingsCommand = new AsyncRelayCommand<Settings>(async (settings) =>
                    {
                        SetFontFamily(new FontFamily(settings.FontFamily));
                        SetFontSize(settings.FontSize);
                        ModernWpf.ThemeManager.Current.ApplicationTheme = settings.Theme;

                        try
                        {
                            ModernWpf.ThemeManager.Current.AccentColor = (Color)ColorConverter.ConvertFromString(settings.AccentColor);
                        }
                        catch (FormatException)
                        {
                            await Messenger.Default.SendAsync(new MessageBoxViewModel
                            {
                                Caption = "Accent color is invalid.",
                                Text = $"{settings.AccentColor} is invalid color format.",
                                Button = MessageBoxButton.OK
                            });
                        }

                        Show(); Activate();
                        hotKey?.Dispose();
                        await RegisterHotKeyAsync(settings.HotKey);
                    });
                }
                return updateSettingsCommand;
            }
        }

        private void SetFontFamily(FontFamily fontFamily)
        {
            object[] keys =
            {
                SystemFonts.MessageFontFamilyKey,
                "ContentControlThemeFontFamily",
                "PivotHeaderItemFontFamily",
                "PivotTitleFontFamily"
            };

            foreach (var key in keys)
            {
                App.Current.Resources[key] = fontFamily;
            }
        }

        private void SetFontSize(double fontSize)
        {
            App.Current.Resources["FileManagerFontSize"] = fontSize;
        }

        private string[] GetAssemblyResourceNames()
        {
            var assembly = App.ResourceAssembly;
            string resourceName = assembly.GetName().Name + ".g.resources";
            using (var stream = assembly.GetManifestResourceStream(resourceName))
            {
                using (var reader = new System.Resources.ResourceReader(stream))
                {
                    return reader.Cast<System.Collections.DictionaryEntry>().Select(entry => (string)entry.Key).ToArray();
                }
            }
        }

        private ICommand loadResourcesCommand;
        public ICommand LoadResourcesCommand
        {
            get
            {
                if(loadResourcesCommand == null)
                {
                    loadResourcesCommand = new RelayCommand<IEnumerable<string>>((resourceNames) =>
                    {
                        var assemblyResourceNames = GetAssemblyResourceNames();
                        resourceNames
                            .Where(name => assemblyResourceNames.Contains(name.ToLowerInvariant()))
                            .Select(name => new Uri(name, UriKind.Relative))
                            .ForEach(uri => App.Current.Resources.MergedDictionaries.Add(new ResourceDictionary { Source = uri }));                        
                    });
                }
                return loadResourcesCommand;
            }
        }

        #region License management
        private StoreContext storeContext;
        private StoreAppLicense appLicense;

        public async void InitializeLicenseAsync()
        {
            if (storeContext == null)
            {
                storeContext = StoreContext.GetDefault();
                storeContext.As<IInitializeWithWindow>().Initialize(new WindowInteropHelper(this).Handle);
            }

            ProgressRing.IsActive = true;
            appLicense = await storeContext.GetAppLicenseAsync();
            ProgressRing.IsActive = false;
            if (appLicense.IsTrial)
            {
                await ShowStoreContextControlAsync(appLicense);
            }
            storeContext.OfflineLicensesChanged += StoreContext_OfflineLicensesChanged;
        }

        private async void StoreContext_OfflineLicensesChanged(StoreContext sender, object args)
        {
            ProgressRing.IsActive = true;
            appLicense = await storeContext.GetAppLicenseAsync();
            ProgressRing.IsActive = false;
            if (appLicense.IsActive)
            {
                if (appLicense.IsTrial)
                {
                    // Trial period has been expired.
                    await ShowStoreContextControlAsync(appLicense);
                }
                else
                {
                    // Show the features that are available only with a full license.
                }
            }
        }

        private async Task ShowStoreContextControlAsync(StoreAppLicense appLicense)
        {
            var storeContextViewModel = new StoreContextViewModel(appLicense);
            do
            {
                await Messenger.Default.SendAsync(storeContextViewModel);
                if (storeContextViewModel.Result == ContentDialogResult.Primary)
                {
                    var productResult = await storeContext.GetStoreProductForCurrentAppAsync();
                    if(productResult.ExtendedError == null)
                    {
                        var puchaseResult = await productResult.Product.RequestPurchaseAsync();
                        if(puchaseResult.ExtendedError == null)
                        {
                            if (puchaseResult.Status == StorePurchaseStatus.Succeeded ||
                                puchaseResult.Status == StorePurchaseStatus.AlreadyPurchased)
                            {
                                break;
                            }
                        }
                    }                   
                }
            } while (storeContextViewModel.IsExpired);
        }
        #endregion

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
