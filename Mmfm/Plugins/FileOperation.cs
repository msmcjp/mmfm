using Mmfm.Commands;
using Msmc.Patterns.Messenger;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;

namespace Mmfm.Plugins
{
    public class FileOperation : IPluggable<DualFileManagerViewModel>
    {
        public string Name => "File";
 
        private NavigationViewModel Navigation => Host.ActiveFileManager.Navigation;

        private IList<string> SelectedPaths => Host.ActiveFileManager.SelectedPaths;

        private IList<FileViewModel> SelectedItems => Host.ActiveFileManager.SelectedItems;

        private bool CanExecute()
        {
            return Navigation.FullPath.Length > 0 && SelectedPaths.Count > 0;
        }

        private bool CanPaste()
        {
            return Directory.Exists(Navigation.FullPath) && ClipboardManager.GetDropFileList(out _)?.Length > 0;
        }

        public IEnumerable<ICommandItem> Commands => new ICommandItem[]
        {
            new CommandItemViewModel("Copy", "Ctrl+C", new RelayCommand(() => Copy(), CanExecute)),
            new CommandItemViewModel("Cut", "Ctrl+X", new RelayCommand(() => Cut(), CanExecute)),
            new CommandItemViewModel("Paste", "Ctrl+V", new RelayCommand(() => Paste(), CanPaste)),
            new CommandItemViewModel("Move to Recycle Bin", "Delete", new RelayCommand(() => DeleteFiles(), CanExecute)),
            new CommandItemViewModel("Delete", "Shift+Delete", new RelayCommand(() => DeleteFiles(false), CanExecute)),
            new CommandItemViewModel("Rename", "F2", new RelayCommand(() => RenameFiles(), CanExecute)),
        };

        public IMessenger Messenger
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

        public event EventHandler RequestInputBindingsUpdate;

        private void OnRequestInputBIndingsUpdate()
        {
            RequestInputBindingsUpdate?.Invoke(this, EventArgs.Empty);
        }

        public void ResetToDefault()
        {

        }

        private FileConflictAction ConfirmFileConflictAction(string source, ref string destination)
        {
            if (File.Exists(destination) == false)
            {
                return FileConflictAction.Overwrite;
            }

            var content = new FileConflictViewModel(source, destination);
            var dialog = new DialogViewModel
            {
                Title = "Confirm",
                Content = content
            };
            Messenger.Send(dialog);

            var action = FileConflictAction.None;
            if (dialog.Result == true)
            {
                if (content.ApplyToAll) { action |= FileConflictAction.ApplyToAll; }
                if (content.Skip) { action |= FileConflictAction.Skip; }
                if (content.Overwrite) { action |= FileConflictAction.Overwrite; }
                if (content.Newer) { action |= FileConflictAction.Newer; }
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

        private void Paste()
        {
            bool move = false;
            var paths = ClipboardManager.GetDropFileList(out move);

            conflictAction = FileConflictAction.None;

            var operations = paths.Select(path => CopyOrMoveOperation(path, Navigation.FullPath, move)).ToArray();
            var content = new OperationProgressViewModel(operations) 
            { 
                Caption = $"{(move ? "Moving" : "Copying")} {operations.Sum(o => o.Count)} files." 
            };

            Messenger.Send(new DialogViewModel
            {
                Title = move ? "Move" : "Copy",
                Content = content
            });

            Host.Refresh();            
            if(move && content.IsCancellationRequested == false)
            {
                ClipboardManager.Clear();
            }
        }

        private FileTraverseOperation CopyOrMoveOperation(string from, string to, bool move)
        {
            return new FileTraverseOperation(from, (path) =>
            {
                var dest = to + path.Substring(Path.GetDirectoryName(from).Length);

                // Do nothing in case of source and destination is same
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
                        if (ConfirmContinueOperation(Properties.Resources.Copy_UnauthorizedException) == false)
                        {
                            return true;
                        }
                    }
                    return false;
                }

                // 
                if (path == dest)
                {
                    if (move)
                    {
                        return false;
                    }
                    dest = path.CopyableFileName();
                }

                if (conflictAction.HasFlag(FileConflictAction.ApplyToAll) == false)
                {
                    if ((conflictAction = ConfirmFileConflictAction(path, ref dest)) == FileConflictAction.None)
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
                            if(path != dest)
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
            return new FileTraverseOperation(path, (aPath) =>
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
                    return true;
                }
                catch (UnauthorizedAccessException)
                {
                    return ConfirmContinueOperation(Properties.Resources.Delete_UnauthorizedException);
                }
            });
        }

        private void DeleteFiles(bool moveToRecycleBin = true)
        {
            var operations = SelectedPaths.Select(path => DeleteOperation(path, moveToRecycleBin)).ToArray();

            var message = new MessageBoxViewModel
            {
                Button = MessageBoxButton.YesNo,
                Icon = MessageBoxImage.Question,
                Caption = "Confirm",
                Text = $"Are you sure to delete {operations.Sum(o => o.Count)} files?"
            };
            Messenger.Send(message);
            if (message.Result == MessageBoxResult.No)
            {
                return;
            }

            Messenger.Send(new DialogViewModel
            {
                Title = "Delete",
                Content = new OperationProgressViewModel(operations) 
                { 
                    Caption = $"Deleting {operations.Sum(o => o.Count)} files." 
                }
            });
            Navigation.Refresh();
        }

        private void RenameFiles()
        {
            foreach (var path in SelectedPaths)
            {
                var content = new FileRenameViewModel(path);
                var dialog = new DialogViewModel
                {
                    Title = "Rename",
                    Content = content
                };

                Messenger.Send(dialog);

                if (dialog.Result == false)
                {
                    break;
                }

                try
                {
                    if (path != content.Next)
                    {
                        if(new FileInfo(path).Attributes.HasFlag(FileAttributes.Directory))
                        {
                            Directory.Move(path, content.Next);
                        }
                        else
                        {
                            File.Move(path, content.Next);
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

            Navigation.Refresh();
        }        
    }
}
