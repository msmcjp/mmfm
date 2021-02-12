using Mmfm.Commands;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Dynamic;
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

        private static IDictionary<string, ISortDescription<FileViewModel>> sortDescriptions = new Dictionary<string, ISortDescription<FileViewModel>>()
        {
            { "Name" , new SortDescriptionViewModel<FileViewModel>("Name", x => x.Name) },
            { "Extension", new SortDescriptionViewModel<FileViewModel>("Extension", x => x.Extension) },
            { "Modified" , new SortDescriptionViewModel<FileViewModel>("Modified at", x => x.ModifiedAt) },
            { "Size" ,new SortDescriptionViewModel<FileViewModel>("Size", x => x.FileSize) },
        };

        public FilesViewModel() : base(sortDescriptions)
        {
          
        }
    }
}
