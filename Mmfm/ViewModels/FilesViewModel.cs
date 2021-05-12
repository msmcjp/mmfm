using Mmfm.Commands;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Dynamic;
using System.IO;
using System.Windows.Input;

namespace Mmfm
{
    public class FilesViewModel : ItemsViewModel<FileViewModel>
    {
        protected override void Launch(FileViewModel item)
        {
            try
            {
                var startInfo = new ProcessStartInfo(item.Path)
                {
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    WorkingDirectory = Path.GetDirectoryName(item.Path)
                };
                Process.Start(startInfo);
            }
            catch (Win32Exception)
            {

            }
        }

        public FilesViewModel() : base(new Dictionary<string, ISortDescription<FileViewModel>>()
        {
            { "Name" , new SortDescriptionViewModel<FileViewModel>("Name", x => x.Name, Properties.Resources.Files_Name) },
            { "Extension", new SortDescriptionViewModel<FileViewModel>("Extension", x => x.Extension, Properties.Resources.Files_Extension) },
            { "Modified" , new SortDescriptionViewModel<FileViewModel>("Modified", x => x.ModifiedAt, Properties.Resources.Files_ModifiedAt) },
            { "Size" ,new SortDescriptionViewModel<FileViewModel>("Size", x => x.FileSize, Properties.Resources.Files_Size) },
        })
        {
          
        }
    }
}
