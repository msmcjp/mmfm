using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MyFileManager
{
    public class FileManagerViewModel : INotifyPropertyChanged
    {
        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        private bool isActive;

        private static KeyValuePair<string, string>[] InitialDirectories()
        {
            var initial = new KeyValuePair<string, string>[] {
                new KeyValuePair<string, string>(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "Desktop" ),
                new KeyValuePair<string, string>(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "My Documents" ),
                new KeyValuePair<string, string>(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures), "My Pictures" ),
             }.ToList();

            foreach(var di in DriveInfo.GetDrives())
            {
                initial.Add(new KeyValuePair<string, string>(di.Name, $"{di.Name.Trim(Path.DirectorySeparatorChar)} {DriveDescription(di)}"));
            }

            return initial.ToArray();
        }

        private static string DriveDescription(DriveInfo di)
        {
            if(di.VolumeLabel.Length > 0)
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

        public CurrentDirectoryViewModel CurrentDirectory { get; } = new CurrentDirectoryViewModel(InitialDirectories());

        public FilesViewModel Files { get; } = new FilesViewModel();

        public bool IsActive { get => isActive; set { isActive = value; OnPropertyChanged("IsActive"); } }

        public FileViewModel[] SelectedItems => CurrentDirectory.SelectedItems.Concat(Files.SelectedItems).ToArray();

        public FileViewModel SelectedItem => CurrentDirectory.SelectedItem ?? Files.SelectedItem;

        public string SelectionStatusText
        {
            get
            {
                var fc = Files.SelectedItems.Count();
                var dc = CurrentDirectory.SelectedItems.Count();

                if(fc + dc == 0) { return ""; }

                string text = "";
                if(fc > 0)
                {
                    text += $@"{fc} file{(fc > 1 ? "s" : "")}{(dc > 0 ? " and " : "")}";                  
                }

                if(dc > 0)
                {
                    text += $@"{dc} folder{(dc > 1 ? "s" : "")}";
                }

                return $"{text} {(fc + dc > 1 ? "are" : "is")} selected.";
            }
        }

        public ICommand ShowContextCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    var targets = SelectedItems.Select(i => new FileInfo(i.Path)).ToArray();
                    if(targets.Count() == 0 && SelectedItem != null)
                    {
                        targets = new FileInfo[1] { new FileInfo(SelectedItem.Path) };
                    }       

                    new Peter.ShellContextMenu().ShowContextMenu(
                        targets,
                        System.Windows.Forms.Control.MousePosition
                    );
                });
            }
        }

        public ICommand SelectAllCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    foreach(var item in Files.Items)
                    {
                        if (item.IsNotAlias)
                        {
                            item.IsSelected = true;
                        }
                    }

                    foreach(var item in CurrentDirectory.SubDirectories)
                    {
                        if (item.IsNotAlias)
                        {
                            item.IsSelected = true;
                        }
                    }
                });
            }
        }

        public ICommand DeselectAllCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    foreach (var item in Files.Items)
                    {
                        item.IsSelected = false;
                    }

                    foreach (var item in CurrentDirectory.SubDirectories)
                    {
                        item.IsSelected = false;
                    }
                });
            }
        }

        public FileManagerViewModel()
        {
            CurrentDirectory.CurrentChanged += CurrentDirectory_CurrentChanged;
            CurrentDirectory.PropertyChanged += CurrentDirectory_PropertyChanged;
            Files.PropertyChanged += Files_PropertyChanged;
        }

        private void CurrentDirectory_CurrentChanged(object sender, EventArgs e)
        {
            foreach(var item in Files.Items)
            {
                item.PropertyChanged -= File_PropertyChanged;
            }
            Files.Items.Clear();

            if (Directory.Exists(CurrentDirectory.FullPath) == false)
            {
                return;
            }

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

            OnPropertyChanged("SelectedItems");
        }

        private void File_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            OnPropertyChanged("Files");
        }

        private void Files_PropertyChanged(object sender, PropertyChangedEventArgs e)
        { 
            OnPropertyChanged("SelectionStatusText");
        }

        private void CurrentDirectory_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            OnPropertyChanged("SelectionStatusText");
        }
    }
}
