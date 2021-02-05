using Mmfm.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Mmfm
{
    public class FilesViewModel : ItemsViewModel<FileViewModel>
    {
        protected override void Launch(FileViewModel item)
        {
            try
            {
                var startInfo = new ProcessStartInfo(SelectedItem.Path) { UseShellExecute = true };
                Process.Start(startInfo);
            }
            catch (Win32Exception)
            {

            }
        }
    }
}
