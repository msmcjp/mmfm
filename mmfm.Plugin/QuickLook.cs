using Mmfm.Commands;
using Msmc.Patterns.Messenger;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Mmfm.Plugins
{
    public class QuickLook : IPluggable<DualFileManagerViewModel>
    {
        public string Name => "Quick Look";
   
        private string SelectedPath => Host?.ActiveFileManager?.SelectedItem?.Path;

        private bool CanExecute() => File.Exists(SelectedPath) || Directory.Exists(SelectedPath);

        public IEnumerable<ICommandItem> Commands => new ICommandItem[]
        {
            new CommandItemViewModel("Toggle QuickLook", "Alt+Q", new AsyncRelayCommand(async () => await SendMessageAsync(Toggle, SelectedPath), CanExecute)),
        };

        public IEnumerable<FolderShortcutViewModel> Shortcuts => null;

        public Messenger Messenger
        {
            get;
            set;
        }

        private DualFileManagerViewModel host;
        public DualFileManagerViewModel Host 
        {
            get => host;
            set
            {
                if (host != null)
                {
                    host.First.PropertyChanged -= Host_PropertyChanged;
                    host.Second.PropertyChanged -= Host_PropertyChanged;
                }

                host = value;

                if (host != null)
                {
                    host.First.PropertyChanged += Host_PropertyChanged;
                    host.Second.PropertyChanged += Host_PropertyChanged;
                }
            }
        }

        public object Settings
        {
            get;
            set;
        }

        public event EventHandler SettingsChanged;

        public void ResetToDefault()
        {
            
        }

        private async Task ShowErrorMessageAsync(string errorMessage)
        {
            var message = new MessageBoxViewModel
            {
                Caption = Properties.Resources.Caption_Error,
                Icon = MessageBoxImage.Error,
                Button = MessageBoxButton.OK,
                Text = errorMessage
            };

            await Messenger.SendAsync(message);
        }

        private void Host_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(FileManagerViewModel.SelectedItem) && IsQuickLookVisible)
            {
                SendMessage(Switch, SelectedPath);
            }
        }

        #region QuickLook Integration
        private delegate bool EnumWindowsDelegate(IntPtr hWnd, IntPtr lparam);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private extern static bool EnumWindows(EnumWindowsDelegate lpEnumFunc, IntPtr lparam);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern int GetWindowTextLength(IntPtr hwnd);

        private bool IsQuickLookVisible
        {
            get
            {
                bool found = false;
                EnumWindows(new EnumWindowsDelegate((hWnd, lParam) => {
                    var l = GetWindowTextLength(hWnd);
                    StringBuilder className = new StringBuilder(l);
                    GetClassName(hWnd, className, l);
                    if (className.ToString().StartsWith("HwndWrapper[QuickLook.exe;;"))
                    {
                        found = true;
                        return false;
                    }
                    return true;
                }), IntPtr.Zero);
                return found;
            }           
        }

        private const string Toggle = "QuickLook.App.PipeMessages.Toggle";
        private const string Switch = "QuickLook.App.PipeMessages.Switch";
        private readonly string PipeName = "QuickLook.App.Pipe." + WindowsIdentity.GetCurrent().User?.Value;

        private void SendMessage(string pipeMessage, string path)
        {
            if (File.Exists(path) == false && Directory.Exists(path) == false)
            {
                return;
            }

            using (var client = new NamedPipeClientStream(".", PipeName, PipeDirection.Out))
            {
                client.Connect(1000);

                using (var writer = new StreamWriter(client))
                {
                    writer.WriteLine($"{pipeMessage}|{path}");
                    writer.Flush();
                }
            }
        }

        private async Task SendMessageAsync(string pipeMessage, string path)
        {                  
            try
            {
                SendMessage(pipeMessage, path);
            }
            catch (TimeoutException)
            {
                await ShowErrorMessageAsync(Properties.Resources.QuickLook_Timeout);
            }
            catch (Exception e)
            {
                await ShowErrorMessageAsync(e.Message);
            }
        }
        #endregion
    }
}
