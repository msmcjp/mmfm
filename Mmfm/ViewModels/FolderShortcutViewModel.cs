using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

namespace Mmfm
{
    public class FolderShortcutViewModel : INotifyPropertyChanged
    {
        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        public static implicit operator FileViewModel(FolderShortcutViewModel s)
        {
            return FileViewModel.CreateAlias(s.Path, s.Name, s.ItemGroup, s.Icon);
        }        

        private string path;
        private Icon icon;
        private string name;
        private string itemGroup;

        [JsonConstructor]
        public FolderShortcutViewModel(string path, string name) : this(path, name, null, IconExtractor.Extract(path)) 
        { 
        }

        public FolderShortcutViewModel(string path, string name, string itemGroup, Icon icon)
        {
            Path = path;
            Name = name;
            Icon = icon;
            ItemGroup = itemGroup;
        }

        public string Path
        {
            get => path;
            private set
            {
                path = value;
            }
        }

        public string Name
        {
            get => name;
            private set
            {
                name = value;
                OnPropertyChanged(nameof(Name));
            }
        }

        [JsonIgnore]
        public string ItemGroup
        {
            get => itemGroup;
            private set
            {
                itemGroup = value;
                OnPropertyChanged(nameof(ItemGroup));
            }
        }
              
        [JsonIgnore]
        public Icon Icon
        {
            get => icon;
            private set
            {
                icon = value;
                OnPropertyChanged(nameof(Icon));
            }
        }
    }
}
