using Mmfm.Commands;
using ModernWpf.Controls;
using Msmc.Patterns.Messenger;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace Mmfm.Plugins
{
    public class FileOperation : IPluggable<DualFileManagerViewModel>
    {
        public string Name => "File";
 
        private NavigationViewModel Navigation => Host.ActiveFileManager?.Navigation;

        private IList<string> SelectedPaths => Host.ActiveFileManager.SelectedPaths;

        private IList<FileViewModel> SelectedItems => Host.ActiveFileManager.SelectedItems;

        private bool CanExecute()
        {
            return Navigation?.FullPath.Length > 0 && SelectedPaths.Count > 0;
        }

        private bool CanPaste()
        {
            return Directory.Exists(Navigation?.FullPath) && ClipboardManager.GetDropFileList(out _)?.Length > 0;
        }

        public IEnumerable<ICommandItem> Commands => new ICommandItem[]
        {
            new CommandItemViewModel("Copy", "Ctrl+C", new RelayCommand(() => Copy(), CanExecute)),
            new CommandItemViewModel("Cut", "Ctrl+X", new RelayCommand(() => Cut(), CanExecute)),
            new CommandItemViewModel("Paste", "Ctrl+V", new AsyncRelayCommand(async () => await PasteAsync(), CanPaste)),
            new CommandItemViewModel("Move to Recycle Bin", "Delete", new AsyncRelayCommand(async () => await DeleteFilesAsync(), CanExecute)),
            new CommandItemViewModel("Delete", "Shift+Delete", new AsyncRelayCommand(async () => await DeleteFilesAsync(false), CanExecute)),
            new CommandItemViewModel("Rename", "F2", new AsyncRelayCommand(async () => await RenameFilesAsync(), CanExecute)),
        };

        public Messenger Messenger
        {
            get;
            set;
        }

        public DualFileManagerViewModel Host
        {
            get;
            set;
        }

        public object Settings
        {
            get;
            set;
        }

        public event EventHandler SettingsChanged;

        private void OnSettingsChanged()
        {
            SettingsChanged?.Invoke(this, EventArgs.Empty);
        }

        public void ResetToDefault()
        {

        }

        private async Task<FileConflictAction> ConfirmFileConflictActionAsync(string source, string destination)
        {
            if (File.Exists(destination) == false)
            {
                return FileConflictAction.Overwrite;
            }

            var dialog = new FileConflictViewModel(source, destination);          
            await Messenger.SendAsync(dialog);

            var action = FileConflictAction.None;

            if (dialog.Result == ContentDialogResult.Primary)
            {
                action = FileConflictAction.Overwrite;
            }

            if (dialog.Result == ContentDialogResult.Secondary)
            {
                action = FileConflictAction.Skip;
            }

            if (dialog.Result != ContentDialogResult.None && dialog.ApplyToAll)
            {
                action |= FileConflictAction.ApplyToAll;
            }

            return action;
        }

        private bool ConfirmContinueOperation(string messageText)
        {
            var message = new MessageBoxViewModel
            {
                Caption = "Error",
                Icon = MessageBoxImage.Error,
                Button = MessageBoxButton.YesNo,
                Text = messageText
            };

            Messenger.Send(message);

            switch (message.Result)
            {
                case MessageBoxResult.Yes:
                    return true;
                default:
                    return false;
            }
        }  

        private FileConflictAction conflictAction;

        private void Copy()
        {
            ClipboardManager.SetDropFileList(SelectedPaths, false);
        }

        private void Cut()
        {
            ClipboardManager.SetDropFileList(SelectedPaths, true);
        }

        private async Task PasteAsync()
        {
            bool move = false;
            var paths = ClipboardManager.GetDropFileList(out move);

            conflictAction = FileConflictAction.None;

            var operations = paths.Select(path => CopyOrMoveOperation(path, Navigation.FullPath, move)).ToArray();
            var dialog = new OperationProgressViewModel(operations)
            {
                Caption = $"{(move ? "Moving" : "Copying")} {operations.Sum(o => o.Count)} files."
            };

            await Messenger.SendAsync(dialog);

            if(move && dialog.IsCancellationRequested == false)
            {
                ClipboardManager.Clear();
            }
        }
        
        private FileTraverseOperation CopyOrMoveOperation(string from, string to, bool move)
        {
            return new FileTraverseOperation(from, async (path) =>
            {
                var dest = to + path.Substring(Path.GetDirectoryName(from).Length);

                if (path == dest)
                {
                    // Do not move when source and destination is same
                    if (move)
                    {
                        return false;
                    }
                    dest = dest.CopyableFileName();
                }

                if (new FileInfo(path).Attributes.HasFlag(FileAttributes.Directory))
                {
                    try
                    {
                        if (Directory.Exists(dest) == false)
                        {
                            Directory.CreateDirectory(dest);
                        }

                        if (move)
                        {
                            Directory.Delete(path);
                        }
                    }
                    catch (UnauthorizedAccessException)
                    {
                        if (ConfirmContinueOperation(Properties.Resources.Copy_UnauthorizedException) == false)
                        {
                            return true;
                        }
                    }
                    return false;
                }

                if (conflictAction.HasFlag(FileConflictAction.ApplyToAll) == false)
                {
                    if ((conflictAction = await ConfirmFileConflictActionAsync(path, dest)) == FileConflictAction.None)
                    {
                        return true;
                    }
                }
                   
                if (conflictAction.CanWrite(path, dest) == true)
                {
                    var restoreReadOnly = false;
                    if (File.Exists(dest))
                    {
                        var attributes = new FileInfo(dest).Attributes;
                        if (attributes.HasFlag(FileAttributes.ReadOnly))
                        {
                            attributes &= ~FileAttributes.ReadOnly;
                            File.SetAttributes(dest, attributes);
                            restoreReadOnly = true;
                        }
                    }

                    try
                    {
                        Directory.CreateDirectory(Path.GetDirectoryName(dest));
                        if (move)
                        {
                            if (path != dest)
                            {
                                File.Delete(dest);
                                File.Move(path, dest);
                            }
                        }
                        else
                        {
                            File.Copy(path, dest, true);
                        }
                    }
                    catch (UnauthorizedAccessException)
                    {
                        return ConfirmContinueOperation(Properties.Resources.Copy_UnauthorizedException);
                    }

                    if (restoreReadOnly)
                    {
                        var attributes = new FileInfo(dest).Attributes;
                        attributes |= FileAttributes.ReadOnly;
                        File.SetAttributes(dest, attributes);
                    }
                }
                return false;
            });
        }

        private FileTraverseOperation DeleteOperation(string path, bool moveToRecycleBin)
        {
            return new FileTraverseOperation(path, (aPath) => Task.Run(()=>
            {
                var fi = new FileInfo(aPath);

                // TODO: 読み取り専用のファイルを削除するか確認が必要
                File.SetAttributes(aPath, FileAttributes.Normal);

                try
                {
                    if (fi.Attributes.HasFlag(FileAttributes.Directory))
                    {
                        Directory.Delete(aPath);
                    }
                    else
                    {
                        if (moveToRecycleBin)
                        {
                            FileOperationAPIWrapper.MoveToRecycleBin(aPath);
                        }
                        else
                        {
                            File.Delete(aPath);
                        }
                    }
                    return false;
                }
                catch (UnauthorizedAccessException)
                {
                    return ConfirmContinueOperation(Properties.Resources.Delete_UnauthorizedException);
                }
            }));
        }

        private async Task DeleteFilesAsync(bool moveToRecycleBin = true)
        {
            var operations = SelectedPaths.Select(path => DeleteOperation(path, moveToRecycleBin)).ToArray();

            var selectionText = Host.ActiveFileManager.SelectionText;
            var messageBox = new MessageBoxViewModel
            {
                Button = MessageBoxButton.YesNo,
                Icon = MessageBoxImage.Question,
                Caption = "Confirm",
                Text = $"Are you sure you want to {(moveToRecycleBin ? $"send {selectionText} to the Recyle Bin" : $"delete {selectionText}")}?"
            };
            await Messenger.SendAsync(messageBox);
            if (messageBox.Result == MessageBoxResult.No)
            {
                return;
            }

            await Messenger.SendAsync(new OperationProgressViewModel(operations)
            {
                Caption = $"Deleting {operations.Sum(o => o.Count)} files."
            });
        }

        private async Task RenameFilesAsync()
        {
            foreach (var path in SelectedPaths)
            {
                var dialog = new FileRenameViewModel(path);               
                await Messenger.SendAsync(dialog);

                if (dialog.Result == ModernWpf.Controls.ContentDialogResult.None)
                {
                    break;
                }

                try
                {
                    if (path != dialog.NextPath)
                    {
                        if(new FileInfo(path).Attributes.HasFlag(FileAttributes.Directory))
                        {
                            Directory.Move(path, dialog.NextPath);
                        }
                        else
                        {
                            File.Move(path, dialog.NextPath);
                        }
                    }
                }
                catch (UnauthorizedAccessException)
                {
                    if (ConfirmContinueOperation(Properties.Resources.Rename_UnauthorizedException))
                    {
                        continue;
                    }
                    break;
                }
            }
        }        
    }
}
