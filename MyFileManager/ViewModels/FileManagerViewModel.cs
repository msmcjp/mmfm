using Msmc.Patterns.Messenger;
using MyFileManager.Commands;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace MyFileManager
{
    public class FileManagerViewModel : INotifyPropertyChanged
    {
        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        public event OperationProgressedEventHandler OperationProgressed;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        private bool isActive;

        private static DirectoryEntryViewModel[] DefaultEntries()
        {
            var entries = new DirectoryEntryViewModel[] {
                new DirectoryEntryViewModel { Path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop), Name = "Desktop" , Icon = IconExtractor.Extract("shell32.dll", 34, true) },
                new DirectoryEntryViewModel { Path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), Name = "My Documents",   Icon = IconExtractor.Extract("shell32.dll", 1, true) },
                new DirectoryEntryViewModel { Path = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures), Name = "My Pictures", Icon = IconExtractor.Extract("shell32.dll", 325, true) }
             }.ToList();

            foreach (var di in DriveInfo.GetDrives())
            {
                entries.Add(new DirectoryEntryViewModel { Path = di.Name, Name = $"{di.Name.Trim(Path.DirectorySeparatorChar)} {DriveDescription(di)}", Icon = DriveIcon(di) });
            }

            return entries.ToArray();
        }

        private static string DriveDescription(DriveInfo di)
        {
            if (di.VolumeLabel.Length > 0)
            {
                return di.VolumeLabel;
            }

            switch (di.DriveType)
            {
                case DriveType.Fixed:
                    return "Local Disk";

                case DriveType.Network:
                    return "Network Drive";

                case DriveType.Removable:
                    return "Removable Media";

                default:
                    return null;
            }
        }

        private static System.Drawing.Icon DriveIcon(DriveInfo di)
        {
            switch (di.DriveType)
            {
                case DriveType.Fixed:
                    return IconExtractor.Extract("shell32.dll", 79, true);

                case DriveType.Network:
                    return IconExtractor.Extract("shell32.dll", 273, true);

                case DriveType.Removable:
                    return IconExtractor.Extract("shell32.dll", 7, true);

                default:
                    return null;
            }
        }

        public CurrentDirectoryViewModel CurrentDirectory { get; } = new CurrentDirectoryViewModel(DefaultEntries());

        public FilesViewModel Files { get; } = new FilesViewModel();

        public bool IsActive { get => isActive; set { isActive = value; OnPropertyChanged("IsActive"); } }

        public string[] SelectedPaths
        {
            get
            {
                var targets = CurrentDirectory.SelectedItems.Concat(Files.SelectedItems).Select(i => i.Path).ToArray();
                if (targets.Count() == 0 && SelectedItem != null)
                {
                    targets = new string[1] { SelectedItem.Path };
                }
                return targets;
            }
        }

        private FileViewModel lastSelectedItem = null;
        public FileViewModel SelectedItem => lastSelectedItem;

        public string SelectionStatusText
        {
            get
            {
                var fc = Files.SelectedItems.Count();
                var dc = CurrentDirectory.SelectedItems.Count();

                if (fc + dc == 0 && SelectedItem != null)
                {
                    _ = SelectedItem.IsFolder ? dc++ : fc++;
                }

                if (fc + dc == 0) { return ""; }

                var text = "";
                if (fc > 0)
                {
                    text += $@"{fc} file{(fc > 1 ? "s" : "")}{(dc > 0 ? " and " : "")}";
                }

                if (dc > 0)
                {
                    text += $@"{dc} folder{(dc > 1 ? "s" : "")}";
                }

                return $"{text} {(fc + dc > 1 ? "are" : "is")} selected.";
            }
        }     

        public void SelectAll()
        {
            foreach (var item in Files.Items)
            {
                if (item.IsNotAlias)
                {
                    item.IsSelected = true;
                }
            }

            foreach (var item in CurrentDirectory.SubDirectories)
            {
                if (item.IsNotAlias)
                {
                    item.IsSelected = true;
                }
            }
        }

        public void DeselectAll()
        {
            foreach (var item in Files.Items)
            {
                item.IsSelected = false;
            }

            foreach (var item in CurrentDirectory.SubDirectories)
            {
                item.IsSelected = false;
            }
        }

        public void BackToParent()
        {
            CurrentDirectory.BackToParent();
        }

        public void CopyToClipboard()
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
        
        private FileConflictAction ConfirmFileConflictAction(string source, ref string destination)
        {
            if (source == destination)
            {
                destination = source.CopyableFileName();
                return FileConflictAction.Overwrite;
            }

            if(File.Exists(destination) == false)
            {
                return FileConflictAction.Overwrite;
            }

            var content = new FileConflictViewModel(source, destination);
            var dialog = new DialogViewModel
            {
                Title = "Confirm",
                Content = content
            };
            Messenger.Default.Send(dialog);

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
                Caption = "An error occurred.",
                Icon = MessageBoxImage.Error,
                Button = MessageBoxButton.YesNo,
                Text = messageText
            };

            Messenger.Default.Send(message);

            switch (message.Result)
            {
                case MessageBoxResult.Yes:
                    return true;
                default:
                    return false;
            }
        }

        public void CopyFiles()
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
                        if (File.Exists(destination))
                        {
                            var attributes = new FileInfo(destination).Attributes;
                            if (attributes.HasFlag(FileAttributes.ReadOnly))
                            {
                                attributes &= ~FileAttributes.ReadOnly;
                                File.SetAttributes(destination, attributes);
                                restoreReadOnly = true;
                            }
                        }

                        try
                        {
                            File.Copy(source, destination, true);
                        }
                        catch(UnauthorizedAccessException)
                        {
                            return ConfirmContinueOperation(Properties.Resources.Copy_UnauthorizedException);  
                        }

                        if (restoreReadOnly)
                        {
                            var attributes = new FileInfo(destination).Attributes;
                            attributes |= FileAttributes.ReadOnly;
                            File.SetAttributes(destination, attributes);
                        }
                    }
                }
                return true;
            });

            Messenger.Default.Send(new DialogViewModel
            {
                Title = "Copy file",
                Content = new OperationProgressViewModel(operation) { Title = $"Copying {operation.MaxValue} files." }
            });                  
            CurrentDirectory.RaiseCurrentChanged();
        }

        public void DeleteFiles(bool moveToRecycleBin = true)
        {
            var operation = new FileTraverseOperation(SelectedPaths, (o, p) =>
            {
                var path = o + p;
                var fi = new FileInfo(path);

                // TODO: 読み取り専用のファイルを削除するか確認が必要
                File.SetAttributes(path, FileAttributes.Normal);

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
                            File.Delete(path);
                        }
                    }
                    return true;
                }
                catch(UnauthorizedAccessException)
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
            Messenger.Default.Send(message);
            if (message.Result == MessageBoxResult.No)
            {
                return;
            }

            Messenger.Default.Send(new DialogViewModel { 
                Title = "Delete files",
                Content = new OperationProgressViewModel(operation) { Title = $"Deleting {operation.MaxValue} files." }
            });            
            CurrentDirectory.RaiseCurrentChanged();
        }

        public FileManagerViewModel()
        {
            CurrentDirectory.CurrentChanged += CurrentDirectory_CurrentChanged;
            CurrentDirectory.PropertyChanged += CurrentDirectory_PropertyChanged;
            Files.PropertyChanged += Files_PropertyChanged;            
        }

        private void CurrentDirectory_CurrentChanged(object sender, EventArgs e)
        {
            lastSelectedItem = null;

            foreach(var item in Files.Items)
            {
                item.PropertyChanged -= File_PropertyChanged;
            }
            Files.Items.Clear();

            if (Directory.Exists(CurrentDirectory.FullPath) == true)
            {
                try
                {
                    foreach (var path in Directory.GetFiles(CurrentDirectory.FullPath))
                    {
                        var item = new FileViewModel(path);
                        item.PropertyChanged += File_PropertyChanged;
                        Files.Items.Add(item);
                    }
                }
                catch (UnauthorizedAccessException)
                {

                }
            }          

            OnPropertyChanged("SelectionStatusText");
            OnPropertyChanged("SelectedPaths");
        }

        private void File_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            OnPropertyChanged("Files");
            OnPropertyChanged("SelectionStatusText");
            OnPropertyChanged("SelectedPaths");
        }

        private void Files_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "SelectedItem" && Files.SelectedItem != null)
            {
                lastSelectedItem = Files.SelectedItem;
            }
            OnPropertyChanged("SelectionStatusText");
            OnPropertyChanged("SelectedPaths");
        }

        private void CurrentDirectory_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "SelectedItem" && CurrentDirectory.SelectedItem != null)
            {
                lastSelectedItem = CurrentDirectory.SelectedItem;
            }
            OnPropertyChanged("SelectionStatusText");
            OnPropertyChanged("SelectedPaths");
        }
    }
}
