using Mmfm.Commands;
using ModernWpf.Controls;
using Msmc.Patterns.Messenger;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
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

        private bool CanExecute() => Navigation?.FullPath.Length > 0 && SelectedPaths.Count > 0;

        private bool CanPaste() => Directory.Exists(Navigation?.FullPath) && Clipboard.GetDataObject().FromFileDropList(out _)?.Count() > 0;        

        public IEnumerable<ICommandItem> Commands => new ICommandItem[]
        {
            new CommandItemViewModel("Copy", "Ctrl+C", new RelayCommand(() => Copy(), CanExecute)),
            new CommandItemViewModel("Cut", "Ctrl+X", new RelayCommand(() => Cut(), CanExecute)),
            new CommandItemViewModel("Paste", "Ctrl+V", new AsyncRelayCommand(async () => await PasteAsync(Clipboard.GetDataObject()), CanPaste)),
            new CommandItemViewModel("Move to Recycle Bin", "Delete", new AsyncRelayCommand(async () => await DeleteFilesAsync(), CanExecute)),
            new CommandItemViewModel("Delete", "Shift+Delete", new AsyncRelayCommand(async () => await DeleteFilesAsync(false), CanExecute)),
            new CommandItemViewModel("Rename", "F2", new AsyncRelayCommand(async () => await RenameFilesAsync(), CanExecute)),
            new CommandItemViewModel("Zip", null, new AsyncRelayCommand(async () => await ZipAsync(), CanExecute)),
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

        public IEnumerable<FolderShortcutViewModel> Shortcuts => null;

        public event EventHandler SettingsChanged;

        public FileOperation()
        {
            Messenger.Default.Register<ClipboardManager.Notification>(this, (n) =>
            {
                foreach (var item in Host.First.Navigation.Items.Concat(Host.Second.Navigation.Items))
                {
                    item.IsCut = n.Move && n.Paths.Contains(item.Path);
                }
            });

            Messenger.Default.RegisterAsyncMessage<DropFileListMessage>(this, async (m) => {
                await PasteAsync(m.DataObject);            
            });
        }

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

        private async Task<bool> ConfirmContinueOperationAsync(string messageText)
        {
            var message = new MessageBoxViewModel
            {
                Caption = Properties.Resources.Caption_Error,
                Icon = MessageBoxImage.Error,
                Button = MessageBoxButton.YesNo,
                Text = messageText
            };

            await Messenger.SendAsync(message);

            switch (message.Result)
            {
                case MessageBoxResult.Yes:
                    return false;
                default:
                    return true;
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

        private async Task PasteAsync(IDataObject dataObject)
        {
            bool move;
            var paths = dataObject.FromFileDropList(out move);
            conflictAction = FileConflictAction.None;
            
            var operations = paths
            .Select(path => CopyOrMoveOperation(
                path, 
                Path.Combine(Navigation.FullPath, Path.GetFileName(path)), 
                move
            ))
            .Where(o => o != null)
            .ToArray();

            var dialog = new OperationProgressViewModel(operations)
            {
                Caption = $"{(move ? Properties.Resources.File_Moving : Properties.Resources.File_Copying)} {operations.Sum(o => o.Count)} files."
            };

            await Messenger.SendAsync(dialog);

            if(move && dialog.IsCancellationRequested == false)
            {
                ClipboardManager.Clear();
            }
        }
        
        private FileTraverseOperation CopyOrMoveOperation(string from, string to, bool move)
        {
            if(from == to)
            {
                if (move)
                {
                    return null;
                }
                to = to.CopyableFileName();
            }

            return new FileTraverseOperation(from, async (path) =>
            {
                var dest = path.Replace(from, to);

                if(path == dest)
                {
                    return false;
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
                        if (await ConfirmContinueOperationAsync(Properties.Resources.Copy_UnauthorizedException) == false)
                        {
                            return true;
                        }
                    }
                    catch(IOException)
                    {

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
                        return await ConfirmContinueOperationAsync(Properties.Resources.Copy_UnauthorizedException);
                    }
                    catch (IOException)
                    {
                        return await ConfirmContinueOperationAsync(Properties.Resources.Copy_UnauthorizedException);
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
            return new FileTraverseOperation(path, async (aPath) => 
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
                    return await ConfirmContinueOperationAsync(Properties.Resources.Delete_UnauthorizedException);
                }
            });
        }

        private async Task DeleteFilesAsync(bool moveToRecycleBin = true)
        {
            var operations = SelectedPaths.Select(path => DeleteOperation(path, moveToRecycleBin)).ToArray();

            var selectionText = Host.ActiveFileManager.SelectionText;
            var messageBox = new MessageBoxViewModel
            {
                Button = MessageBoxButton.YesNo,
                Icon = MessageBoxImage.Question,
                Caption = Properties.Resources.MessageBox_Confirm,
                Text = string.Format(moveToRecycleBin ? Properties.Resources.File_Recycling : Properties.Resources.File_Deleting, selectionText)
            };
            await Messenger.SendAsync(messageBox);
            if (messageBox.Result != MessageBoxResult.Yes)
            {
                return;
            }

            await Messenger.SendAsync(new OperationProgressViewModel(operations)
            {
                Caption = string.Format(Properties.Resources.File_DeletingProgress, operations.Sum(o => o.Count))
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
                    if (await ConfirmContinueOperationAsync(Properties.Resources.Rename_UnauthorizedException))
                    {
                        continue;
                    }
                    break;
                }
            }
        }

        private async Task ZipAsync()
        {
            var zipPath = Path.Combine(
                Navigation.FullPath, 
                Path.ChangeExtension(SelectedPaths.First(), ".zip")
            ).CopyableFileName();

            using (var archive = ZipFile.Open(zipPath, ZipArchiveMode.Create))
            {
                var operations = SelectedPaths.Select(path => ZipOperation(archive, path)).ToArray();

                var dialog = new OperationProgressViewModel(operations)
                {
                    Caption = string.Format(Properties.Resources.File_Zipping, operations.Sum(o => o.Count))
                };

                await Messenger.SendAsync(dialog);
            }
        }

        private FileTraverseOperation ZipOperation(ZipArchive archive, string path) => new FileTraverseOperation(path, async (aPath) => 
        {
            try
            {
                if(new FileInfo(aPath).Attributes.HasFlag(FileAttributes.Directory))
                {
                    return false;
                }
                var entryName = aPath.Substring(Navigation.FullPath.Length).TrimStart(Path.DirectorySeparatorChar);
                archive.CreateEntryFromFile(aPath, entryName);
                return false;
            }
            catch (UnauthorizedAccessException ex)
            {
                return await ConfirmContinueOperationAsync(Properties.Resources.Zip_UnauthorizedException);
            }
        });        
    }
}
