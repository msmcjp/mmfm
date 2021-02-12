using Mmfm.Commands;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Input;

namespace Mmfm
{
    public class FilesViewModel : ItemsViewModel<FileViewModel>
    {
        protected override void Launch(FileViewModel item)
        {
            try
            {
                var startInfo = new ProcessStartInfo(item.Path) { UseShellExecute = true };
                Process.Start(startInfo);
            }
            catch (Win32Exception)
            {

            }
        }

        public object SortDescriptions { get; } = new
        {
            Name = new SortDescription<FileViewModel>("Name", x => x.Name),
            Extension = new SortDescription<FileViewModel>("Extension", x => x.Extension),
            Modified = new SortDescription<FileViewModel>("Modified at", x => x.ModifiedAt),
            Size = new SortDescription<FileViewModel>("Size", x => x.FileSize),
        };

        private ICommand sortCommand;
        public override ICommand SortCommand
        {
            get
            {
                if (sortCommand == null)
                {
                    sortCommand = new RelayCommand<SortDescription<FileViewModel>>(
                        (desc) => {
                            desc.IsDescending = !desc.IsDescending;
                            OrderBy = new SortDescription<FileViewModel>[] { desc };
                        }
                    );
                }
                return sortCommand;
            }
        }
    }
}
