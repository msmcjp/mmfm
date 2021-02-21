using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace Mmfm
{
    public class FileViewModel : INotifyPropertyChanged, IHasIsSelected
    {
        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        private bool isSelected;
        private string path;
        private Lazy<Icon> icon;
        private string name;
        private string extension;
        private bool isAlias;
        private string itemGroup;
        private bool isCut;
        private Lazy<FileInfo> fileInfo;

        public static FileViewModel CreateAlias(string aliasName, string path, Icon icon = null, string itemGroup = null)
        {
            return new FileViewModel(aliasName, path, icon, itemGroup);
        }

        public static FileViewModel CreateFolder(string path, string itemGroup = null)
        {
            return new FileViewModel(path, itemGroup)
            {
                Name = System.IO.Path.GetFileName(path)
            };
        }

        public static FileViewModel CreateFile(string path, string itemGroup = null)
        {
            var name = System.IO.Path.GetFileNameWithoutExtension(path);
            var extension = System.IO.Path.GetExtension(path);
            return new FileViewModel(path, itemGroup)
            {
                Name = string.IsNullOrEmpty(name) ? extension : name,
                Extension = string.IsNullOrEmpty(name) ? "" : extension
            };          
        }

        private FileViewModel(string aliasName, string path, Icon icon, string itemGroup)
        {
            Path = path;
            Name = aliasName;
            isAlias = true;
            Icon = new Lazy<Icon>(icon);
            ItemGroup = itemGroup;
            fileInfo = new Lazy<FileInfo>(() => new FileInfo(path));
        }

        private FileViewModel(string path, string itemGroup)
        {
            Path = path;
            isAlias = false;
            Icon = new Lazy<Icon>(() => IconExtractor.Extract(Path, true));
            ItemGroup = itemGroup;
            fileInfo = new Lazy<FileInfo>(() => new FileInfo(path));
        }

        public string Path
        {
            get => path;
            private set
            {
                path = value;
            }
        }

        public bool IsSelected
        {
            get => isSelected;
            set
            {
                isSelected = value;
                OnPropertyChanged(nameof(IsSelected));
            }
        }

        public Lazy<Icon> Icon
        {
            get => icon;
            private set
            {
                icon = value;
                OnPropertyChanged(nameof(Icon));
            }
        }

        public BitmapSource IconImage
        {
            get
            {
                if (Icon?.Value != null)
                {
                    var icon = Imaging.CreateBitmapSourceFromHIcon(
                        Icon.Value.Handle,
                        Int32Rect.Empty,
                        BitmapSizeOptions.FromEmptyOptions()
                    );
                    icon.Freeze();
                    return icon;
                }
                return null;
            }
        }

        public string Name
        {
            get => name;
            private set
            {
                name = value;
                OnPropertyChanged(nameof(Name));
            }
        }

        public string Extension
        {
            get => extension;
            private set
            {
                extension = value;
                OnPropertyChanged(nameof(Extension));
            }
        }

        public string ItemGroup
        {
            get => itemGroup;
            private set
            {
                itemGroup = value;
                OnPropertyChanged(nameof(ItemGroup));
            }
        }

        public bool IsHidden => fileInfo.Value.Attributes.HasFlag(FileAttributes.Hidden);

        public bool IsCut
        {
            get => isCut;
            set
            {
                isCut = value;
                OnPropertyChanged(nameof(IsCut));
                OnPropertyChanged(nameof(IconOpacity));
            }
        }

        public double IconOpacity => IsHidden || IsCut ? 0.4 : 1.0; 

        public bool IsNotAlias => !isAlias;

        public bool IsFolder => Directory.Exists(path);

        public string ModifiedAt => isAlias ? "" : fileInfo.Value.LastWriteTime.ToString("yyyy/MM/dd HH:mm");

        public string FileSizeText => isAlias ? "" : FileSize.ToFileSize();

        public long FileSize
        {
            get
            {
                if (fileInfo.Value.Attributes.HasFlag(FileAttributes.Directory) == false)
                {
                    return fileInfo.Value.Length;
                }
                return 0;
            }
        }        
      
        public override bool Equals(object obj)
        {
            if (obj == null || this.GetType() != obj.GetType())
            {
                return false;
            }

            return Path == ((FileViewModel)obj).Path && isAlias == ((FileViewModel)obj).isAlias;
        }

        public override int GetHashCode()
        {
            return Path.GetHashCode() ^ isAlias.GetHashCode();
        }
    }
}
