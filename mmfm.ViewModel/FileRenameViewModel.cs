using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;

namespace Mmfm.ViewModel
{
    public class FileRenameViewModel : ContentDialogViewModel, INotifyDataErrorInfo
    {
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
                OnPropertyChanged(nameof(Current));
                FileNameWithoutExtension = System.IO.Path.GetFileNameWithoutExtension(current);
                FileExtension = System.IO.Path.GetExtension(current).TrimStart("."[0]);
            }
        }

        [ValidFileName]
        public string FileNameWithoutExtension
        {
            get => fileNameWithoutExtension;
            set
            {
                fileNameWithoutExtension = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(NextFileName));
                OnPropertyChanged(nameof(CanRename));
            }
        }

        [ValidFileName]
        public string FileExtension
        {
            get => fileExtension;
            set
            {
                fileExtension = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(NextFileName));
                OnPropertyChanged(nameof(CanRename));
            }
        }

        [Required(ErrorMessageResourceType = typeof(Properties.Resources), ErrorMessageResourceName ="Rename_NotInput")]
        [ValidFileName(ErrorMessageResourceType = typeof(Properties.Resources), ErrorMessageResourceName = "Rename_Invalid")]
        public string NextFileName => fileNameWithoutExtension?.Length + fileExtension?.Length > 0 ? $"{FileNameWithoutExtension}.{FileExtension}" : "";

        public bool CanRename => GetErrors(nameof(NextFileName)).GetEnumerator().MoveNext() == false;

        public string NextPath => Path.Combine(Path.GetDirectoryName(path), $"{FileNameWithoutExtension}.{FileExtension}");    
    }
}
