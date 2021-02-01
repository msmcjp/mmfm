using Mmfm.Commands;
using Msmc.Patterns.Messenger;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;

namespace Mmfm.Plugin
{
    public class File : IPluggable<DualFileManagerViewModel>
    {
        public string Name => "File";
 
        private CurrentDirectoryViewModel CurrentDirectory => Host.ActiveFileManager.CurrentDirectory;

        private string[] SelectedPaths => Host.ActiveFileManager.SelectedPaths;

        private bool CanExecute()
        {
            return CurrentDirectory.FullPath.Length > 0;
        }

        public IEnumerable<ICommandItem> Commands => new ICommandItem[]
        {
            new CommandItemViewModel("Copy", "Ctrl+C", new RelayCommand(() => CopyToClipboard(), CanExecute)),
            new CommandItemViewModel("Paste", "Ctrl+V", new RelayCommand(() => CopyFiles(), CanExecute)),
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

        public dynamic Settings
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

        public void Plugged()
        {
        }

        private FileConflictAction ConfirmFileConflictAction(string source, ref string destination)
        {
            if (source == destination)
            {
                destination = source.CopyableFileName();
                return FileConflictAction.Overwrite;
            }

            if (System.IO.File.Exists(destination) == false)
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
 
        private void CopyToClipboard()
        {
            if (SelectedPaths.Length == 0)
            {
                return;
            }

            var files = new System.Collections.Specialized.StringCollection();
            foreach (var path in SelectedPaths)
            {
                files.Add(path);
            }
            Clipboard.SetFileDropList(files);
        }

        private void CopyFiles()
        {
            var paths = Clipboard.GetFileDropList().Cast<string>();
            var action = FileConflictAction.None;
            var operation = new FileTraverseOperation(paths, (origin, path) =>
            {
                var source = origin + path;
                var destination = CurrentDirectory.FullPath + path;

                if (new FileInfo(source).Attributes.HasFlag(FileAttributes.Directory))
                {
                    Directory.CreateDirectory(destination);
                }
                else
                {
                    if (action.HasFlag(FileConflictAction.ApplyToAll) == false)
                    {
                        if ((action = ConfirmFileConflictAction(source, ref destination)) == FileConflictAction.None)
                        {
                            return false;
                        }
                    }
                    if (action.CanWrite(source, destination) == true)
                    {
                        var restoreReadOnly = false;
                        if (System.IO.File.Exists(destination))
                        {
                            var attributes = new FileInfo(destination).Attributes;
                            if (attributes.HasFlag(FileAttributes.ReadOnly))
                            {
                                attributes &= ~FileAttributes.ReadOnly;
                                System.IO.File.SetAttributes(destination, attributes);
                                restoreReadOnly = true;
                            }
                        }

                        try
                        {
                            System.IO.File.Copy(source, destination, true);
                        }
                        catch (UnauthorizedAccessException)
                        {
                            return ConfirmContinueOperation(Properties.Resources.Copy_UnauthorizedException);
                        }

                        if (restoreReadOnly)
                        {
                            var attributes = new FileInfo(destination).Attributes;
                            attributes |= FileAttributes.ReadOnly;
                            System.IO.File.SetAttributes(destination, attributes);
                        }
                    }
                }
                return true;
            });

            Messenger.Send(new DialogViewModel
            {
                Title = "Copy",
                Content = new OperationProgressViewModel(operation) { Title = $"Copying {operation.MaxValue} files." }
            });
            CurrentDirectory.RaiseCurrentChanged();
        }

        private void DeleteFiles(bool moveToRecycleBin = true)
        {
            var operation = new FileTraverseOperation(SelectedPaths, (o, p) =>
            {
                var path = o + p;
                var fi = new FileInfo(path);

                // TODO: 読み取り専用のファイルを削除するか確認が必要
                System.IO.File.SetAttributes(path, FileAttributes.Normal);

                try
                {
                    if (fi.Attributes.HasFlag(FileAttributes.Directory))
                    {
                        Directory.Delete(path);
                    }
                    else
                    {
                        if (moveToRecycleBin)
                        {
                            FileOperationAPIWrapper.MoveToRecycleBin(path);
                        }
                        else
                        {
                            System.IO.File.Delete(path);
                        }
                    }
                    return true;
                }
                catch (UnauthorizedAccessException)
                {
                    return ConfirmContinueOperation(Properties.Resources.Delete_UnauthorizedException);
                }
            })
            {
                BottomUp = true
            };

            var message = new MessageBoxViewModel
            {
                Button = MessageBoxButton.YesNo,
                Icon = MessageBoxImage.Question,
                Caption = "Confirm",
                Text = $"Are you sure to delete {operation.MaxValue} files?"
            };
            Messenger.Send(message);
            if (message.Result == MessageBoxResult.No)
            {
                return;
            }

            Messenger.Send(new DialogViewModel
            {
                Title = "Delete",
                Content = new OperationProgressViewModel(operation) { Title = $"Deleting {operation.MaxValue} files." }
            });
            CurrentDirectory.RaiseCurrentChanged();
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
                        System.IO.File.Move(path, content.Next);
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

            CurrentDirectory.RaiseCurrentChanged();
        }        
    }
}
