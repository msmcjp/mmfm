using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Mmfm
{
    public class DualFileManagerViewModel : INotifyPropertyChanged
    {
        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        public FileManagerViewModel First { get; } = new FileManagerViewModel();
        public FileManagerViewModel Second { get; } = new FileManagerViewModel();

        public FileManagerViewModel ActiveFileManager
        {
            get;
            private set;
        }

        public DualFileManagerViewModel()
        {
            First.PropertyChanged += FileManager_PropertyChanged;
            Second.PropertyChanged += FileManager_PropertyChanged;
        }

        private void FileManager_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if(e.PropertyName == "IsActive" && ((FileManagerViewModel)sender).IsActive)
            {
                ActiveFileManager = (FileManagerViewModel)sender;
                OnPropertyChanged("ActiveFileManager");
            }
        }
    }
}
