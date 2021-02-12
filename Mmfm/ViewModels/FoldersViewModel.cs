using Mmfm.Commands;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Windows.Input;

namespace Mmfm
{
    public class FoldersViewModel : ItemsViewModel<FileViewModel>
    {
        private Action<FileViewModel> LaunchAction
        {
            get;
            set;
        }

        private static IDictionary<string, ISortDescription<FileViewModel>> sortDescriptions = new Dictionary<string, ISortDescription<FileViewModel>>
        {
            { "Name" , new SortDescriptionViewModel<FileViewModel>("Name", x => x.Name) },
            { "Modified" ,new SortDescriptionViewModel<FileViewModel>("Modified at", x => x.ModifiedAt) },
        };

        public FoldersViewModel(Action<FileViewModel> action) : base(sortDescriptions)
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
