using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mmfm
{
    public class FoldersViewModel : ItemsViewModel<FileViewModel>
    {
        private Action<FileViewModel> LaunchAction
        {
            get;
            set;
        }

        public FoldersViewModel(Action<FileViewModel> action)
        {
            LaunchAction = action;
        }

        protected override void Launch(FileViewModel item)
        {
            LaunchAction?.Invoke(SelectedItem);
        }
    }
}
