using Mmfm.Commands;
using System;
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

        public FoldersViewModel(Action<FileViewModel> action)
        {
            LaunchAction = action;
        }

        protected override void Launch(FileViewModel item)
        {
            LaunchAction?.Invoke(SelectedItem);
        }

        public object SortDescriptions { get; } = new
        {
            Name = new SortDescription<FileViewModel>("Name", x => x.Name),
            Modified = new SortDescription<FileViewModel>("Modified at", x => x.ModifiedAt),
        };

        public SortDescription<FileViewModel> IsNotAlias { get; } = new SortDescription<FileViewModel>("IsNotAlias", x => x.IsNotAlias);

        private ICommand sortCommand;
        public override ICommand SortCommand 
        {
            get
            {
                if(sortCommand == null)
                {
                    sortCommand = new RelayCommand<SortDescription<FileViewModel>>(
                        (desc) => {
                            desc.IsDescending = !desc.IsDescending;
                            OrderBy = new SortDescription<FileViewModel>[] { IsNotAlias, desc };
                        }
                    );
                }
                return sortCommand;
            }
        }
    }
}
