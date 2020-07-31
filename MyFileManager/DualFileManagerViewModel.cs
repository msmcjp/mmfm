using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MyFileManager
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
        public FileManagerViewModel ActiveFileManager => First.IsActive ? First : (Second.IsActive ? Second : null);

        public DualFileManagerViewModel()
        {
            First.PropertyChanged += First_PropertyChanged;
            Second.PropertyChanged += Second_PropertyChanged;
        }

        private void Second_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if(e.PropertyName == "IsActive")
            {
                OnPropertyChanged("ActiveFileManager");
            }
        }

        private void First_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "IsActive")
            {
                OnPropertyChanged("ActiveFileManager");
            }
        }
    }
}
