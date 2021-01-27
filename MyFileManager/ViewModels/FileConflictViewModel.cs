﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MyFileManager
{
    public class FileConflictViewModel : INotifyPropertyChanged
    {
        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        public FileConflictViewModel(string source, string destination)
        {
            Source = source;
            Destination = destination;           
        }

        public string Source
        {
            get;
            private set;
        }

        public string Destination
        {
            get;
            private set;
        }

        private bool overwrite;
        public bool Overwrite
        {
            get => overwrite;
            set
            {
                overwrite = value;
                OnPropertyChanged("Overwrite");
            }
        }

        private bool skip;
        public bool Skip
        {
            get => skip;
            set
            {
                skip = value;
                OnPropertyChanged("Skip");
            }
        }

        private bool newer;
        public bool Newer
        {
            get => newer;
            set
            {
                newer = value;
                OnPropertyChanged("Newer");
            }
        }

        private bool applyToAll;
        public bool ApplyToAll
        {
            get => applyToAll;
            set
            {
                applyToAll = value;
                OnPropertyChanged("ApplyToAll");
            }
        }

        public string FileName => Path.GetFileName(Source);

        public string SourceDirectory => $"\U0001f5c2 {Path.GetDirectoryName(Source)}";

        public string SourceLastWriteTime => $"\U0001f551 {new FileInfo(Source).LastWriteTime}";

        public string DestinationDirectory => $"\U0001f5c2 {Path.GetDirectoryName(Destination)}";

        public string DestinationLastWriteTime => $"\U0001f551 {new FileInfo(Destination).LastWriteTime}";
    }
}
