using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MyFileManager
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
        private string name;
        private string extension;
        private DateTime modifiedAt;
        private long fileSize;
        private bool isAlias;

        public FileViewModel(string path, string alias)
        {
            this.path = path;
            name = alias;
            isAlias = true; 
        }

        public FileViewModel(string path)
        {
            this.path = path;
            name = System.IO.Path.GetFileNameWithoutExtension(path);
            extension = System.IO.Path.GetExtension(path);

            var fi = new FileInfo(path);
            modifiedAt = fi.LastWriteTime;

            if(fi.Attributes.HasFlag(FileAttributes.Directory) == false)
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

        public string FileSize => isAlias ? ""  : fileSize.ToFileSize();

        public string Path => path;
    }
}
