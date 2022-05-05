using Mmfm.Model;
using System;
using System.Collections.Generic;

namespace Mmfm.ViewModel
{
    public class FoldersViewModel : ItemsViewModel<FileViewModel>
    {
        private Action<FileViewModel> LaunchAction
        {
            get;
            set;
        }

        public FoldersViewModel(Action<FileViewModel> action) : base(new Dictionary<string, ISortDescription<FileViewModel>>
        {
            { "Name" , new SortDescriptionViewModel<FileViewModel>("Name", x => x.Name, Properties.Resources.Folders_Name) },
            { "Modified" ,new SortDescriptionViewModel<FileViewModel>("Modified", x => x.ModifiedAt, Properties.Resources.Folders_ModifiedAt) },
        })
        {
            LaunchAction = action;
        }

        protected override void Launch(FileViewModel item)
        {
            LaunchAction?.Invoke(SelectedItem);
        }

        protected override IEnumerable<ISortDescription<FileViewModel>> OrderBy(ISortDescription<FileViewModel> current)
        {   
            var isNotAlias = new SortDescriptionViewModel<FileViewModel>("IsNotAlias", x => x.IsNotAlias);
            return new ISortDescription<FileViewModel>[] { isNotAlias, current };            
        }
    }
}
