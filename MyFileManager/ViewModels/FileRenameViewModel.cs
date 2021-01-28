﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyFileManager
{
    public class FileRenameViewModel : INotifyPropertyChanged
    {
        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        private string path;
        private string current;
        private string fileNameWithoutExtension;
        private string fileExtension;

        public FileRenameViewModel(string path)
        {
            this.path = path;
            Current = Path.GetFileName(path);
        }

        public string Current
        {
            get => current;
            private set
            {
                current = value;
                OnPropertyChanged("Current");
                FileNameWithoutExtension = System.IO.Path.GetFileNameWithoutExtension(current);
                FileExtension = System.IO.Path.GetExtension(current).TrimStart("."[0]);
            }
        }

        public string FileNameWithoutExtension
        {
            get => fileNameWithoutExtension;
            set
            {
                fileNameWithoutExtension = value;
                OnPropertyChanged("FileNameWithoutExtension");
            }
        }

        public string FileExtension
        {
            get => fileExtension;
            set
            {
                fileExtension = value;
                OnPropertyChanged("FileExtension");
            }
        }

        public string Next => Path.Combine(Path.GetDirectoryName(path), $"{FileNameWithoutExtension}.{FileExtension}");
    }
}
