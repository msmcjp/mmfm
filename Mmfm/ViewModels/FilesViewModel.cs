using Mmfm.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Mmfm
{
    public class FilesViewModel : INotifyPropertyChanged
    {
        private FileViewModel selectedItem;

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        private ObservableCollection<FileViewModel> items = new ObservableCollection<FileViewModel>();
        public ObservableCollection<FileViewModel> Items { 
            get => items;
            set
            {
                items = value;
                OnPropertyChanged("Items");
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

        public FileViewModel[] SelectedItems
        {
            get => Items.Where(item => item.IsSelected).ToArray();
        }

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
                            var startInfo = new ProcessStartInfo(SelectedItem.Path) { UseShellExecute = true };
                            Process.Start(startInfo);
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
