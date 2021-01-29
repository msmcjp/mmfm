using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

namespace Mmfm
{
    public class FileViewModel : INotifyPropertyChanged
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
        private BitmapSource iconImage;
        private string name;
        private string extension;
        private DateTime modifiedAt;
        private long fileSize;
        private bool isAlias;
        private string itemGroup;

        public static FileViewModel CreateAlias(string path, string aliasName, string itemGroup)
        {
            return new FileViewModel(path, aliasName, null, itemGroup);
        }

        public static FileViewModel CreatePC(string path, string name, Icon icon)
        {
            return new FileViewModel(path, name, icon, "\U0001f4bb PC");
        }

        public static FileViewModel CreateFavorite(string path, string name, Icon icon)
        {
            return new FileViewModel(path, name, icon, "\U0001f496 Favorite");
        }

        private FileViewModel(string path, string aliasName, Icon icon, string itemGroup)
        {
            Path = path;
            Name = aliasName;
            isAlias = true;
            ItemGroup = itemGroup;
            if (icon != null)
            {
                iconImage = Imaging.CreateBitmapSourceFromHIcon(icon.Handle, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            }
        }

        public FileViewModel(string path, string itemGroup)
        {
            Path = path;
            ItemGroup = itemGroup;
            Name = System.IO.Path.GetFileNameWithoutExtension(path);
            Extension = System.IO.Path.GetExtension(path);

            var fi = new FileInfo(path);
            modifiedAt = fi.LastWriteTime;

            if (fi.Attributes.HasFlag(FileAttributes.Directory) == false)
            {
                fileSize = fi.Length;
            }
        }

        public string Path
        {
            get => path;
            private set
            {
                path = value;
            }
        }

        public string ItemGroup
        {
            get => itemGroup;
            private set
            {
                itemGroup = value;
            }
        }

        public bool IsSelected
        {
            get => isSelected;
            set
            {
                isSelected = value;
                OnPropertyChanged("IsSelected");
            }
        }

        public BitmapSource IconImage
        {
            get
            {
                if (iconImage == null && isAlias == false)
                {
                    var icon = IconExtractor.Extract(Path, true);
                    if (icon != null)
                    {
                        iconImage = Imaging.CreateBitmapSourceFromHIcon(icon.Handle, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                    }
                }
                return iconImage;
            }
        }

        public string Name
        {
            get => name;
            private set
            {
                name = value;
                OnPropertyChanged("Name");
            }
        }

        public string Extension
        {
            get => extension;
            private set
            {
                extension = value;
                OnPropertyChanged("Extension");
            }
        }

        public bool IsNotAlias => !isAlias;

        public bool IsFolder => Directory.Exists(path);

        public string ModifiedAt => isAlias ? "" : modifiedAt.ToString("yyyy/MM/dd HH:mm");

        public string FileSize => isAlias ? "" : fileSize.ToFileSize();
      
        public override bool Equals(object obj)
        {
            if (obj == null || this.GetType() != obj.GetType())
            {
                return false;
            }

            return Path == ((FileViewModel)obj).Path;
        }

        public override int GetHashCode()
        {
            return Path.GetHashCode();
        }
    }
}
