using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace MyFileManager
{
    public class FilesViewModel : INotifyPropertyChanged
    {
        private bool isFocused;
        private FileViewModel selectedItem;

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;        

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        public ObservableCollection<FileViewModel> Items { get; } = new ObservableCollection<FileViewModel>();       

        public bool IsFocused
        {
            get => isFocused;
            set
            {
                isFocused = value;
                OnPropertyChanged("IsFocused");
            }
        }

        public FileViewModel SelectedItem
        {
            get => selectedItem;
            set
            {
                selectedItem = value;
                OnPropertyChanged("SelectedItem");
            }
        }

        public FileViewModel[] SelectedItems => Items.Where(item => item.IsSelected).ToArray();              

        public ICommand SelectCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    if (SelectedItem != null)
                    {
                        SelectedItem.IsSelected = !SelectedItem.IsSelected;
                    }
                });
            }
        }

        public ICommand LaunchCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    try
                    {
                        if (SelectedItem != null)
                        {
                            System.Diagnostics.Process.Start(SelectedItem.Path);
                        }
                    }
                    catch (Win32Exception)
                    {

                    }
                });
            }
        }
    }
}
