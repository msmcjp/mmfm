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

        public FileViewModel(string path, string name) : this(path, name, null)
        {

        }

        public FileViewModel(string path, string alias, Icon icon)
        {
            this.path = path;
            name = alias;
            isAlias = true;
            if (icon != null)
            {
                iconImage = Imaging.CreateBitmapSourceFromHIcon(icon.Handle, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            }
        }

        public FileViewModel(string path)
        {
            this.path = path;
            name = System.IO.Path.GetFileNameWithoutExtension(path);
            extension = System.IO.Path.GetExtension(path);

            var fi = new FileInfo(path);
            modifiedAt = fi.LastWriteTime;

            if (fi.Attributes.HasFlag(FileAttributes.Directory) == false)
            {
                fileSize = fi.Length;
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

        public bool IsNotAlias => !isAlias;

        public bool IsFolder => Directory.Exists(path);

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
            set
            {
                name = value;
                OnPropertyChanged("Name");
            }
        }

        public string Extension
        {
            get => extension;
            set
            {
                extension = value;
                OnPropertyChanged("Extension");
            }
        }

        public string ModifiedAt => isAlias ? "" : modifiedAt.ToString("yyyy/MM/dd HH:mm");

        public string FileSize => isAlias ? "" : fileSize.ToFileSize();

        public string Path => path;

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
