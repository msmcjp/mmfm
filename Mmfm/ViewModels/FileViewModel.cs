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
        private Icon icon;
        private string name;
        private string extension;
        private DateTime modifiedAt;
        private long fileSize;
        private bool isAlias;
        private string itemGroup;

        public static FileViewModel CreateAlias(string path, string aliasName, string itemGroup = null, Icon icon = null)
        {
            return new FileViewModel(path, aliasName, itemGroup, icon);
        }

        private FileViewModel(string path, string aliasName, string itemGroup, Icon icon)
        {
            Path = path;
            Name = aliasName;
            isAlias = true;
            Icon = icon;
            ItemGroup = itemGroup;
        }

        public FileViewModel(string path, string itemGroup = null)
        {
            Path = path;
            Name = System.IO.Path.GetFileNameWithoutExtension(path);
            isAlias = false;
            Icon = IconExtractor.Extract(Path, true);
            ItemGroup = itemGroup;
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

        public bool IsSelected
        {
            get => isSelected;
            set
            {
                isSelected = value;
                OnPropertyChanged("IsSelected");
            }
        }

        public Icon Icon
        {
            get => icon;
            private set
            {
                icon = value;
                OnPropertyChanged("Icon");
            }
        }

        public BitmapSource IconImage
        {
            get
            {
                if (Icon != null)
                {
                    return Imaging.CreateBitmapSourceFromHIcon(Icon.Handle, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions()); ;
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

        public string ItemGroup
        {
            get => itemGroup;
            private set
            {
                itemGroup = value;
                OnPropertyChanged("ItemGroup");
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

            return Path == ((FileViewModel)obj).Path && isAlias == ((FileViewModel)obj).isAlias;
        }

        public override int GetHashCode()
        {
            return Path.GetHashCode() ^ isAlias.GetHashCode();
        }
    }
}
