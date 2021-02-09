using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mmfm
{
    public class FavoriteRegisterViewModel : INotifyPropertyChanged
    {
        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        public FavoriteRegisterViewModel(string path)
        {
            FullPath = path;
            FavoriteName = Path.GetFileName(FullPath);
        }

        public string FullPath
        {
            get;
            private set;
        }

        private string favoriteName;
        public string FavoriteName
        {
            get => favoriteName;
            set
            {
                favoriteName = value;
                OnPropertyChanged(nameof(FavoriteName));
            }
        }
    }
}
