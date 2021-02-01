using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mmfm
{
    public static class DefaultFolderShortcuts
    {
        public  static FolderShortcutViewModel[] PC()
        {
            var itemGroup = "\U0001f4bb PC";
            var entries = new FolderShortcutViewModel[] {
                new FolderShortcutViewModel(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "Desktop" , itemGroup, IconExtractor.Extract("shell32.dll", 34, true) ),
                new FolderShortcutViewModel(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "My Documents", itemGroup, IconExtractor.Extract("shell32.dll", 1, true) ),
                new FolderShortcutViewModel(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures), "My Pictures", itemGroup, IconExtractor.Extract("shell32.dll", 325, true) )
             }.ToList();

            foreach (var di in DriveInfo.GetDrives())
            {
                entries.Add(new FolderShortcutViewModel(di.Name, $"{di.Name.Trim(Path.DirectorySeparatorChar)} {DriveDescription(di)}", itemGroup, DriveIcon(di)));
            }

            return entries.ToArray();
        }

        private static string DriveDescription(DriveInfo di)
        {
            if (di.VolumeLabel.Length > 0)
            {
                return di.VolumeLabel;
            }

            switch (di.DriveType)
            {
                case DriveType.Fixed:
                    return "Local Disk";

                case DriveType.Network:
                    return "Network Drive";

                case DriveType.Removable:
                    return "Removable Media";

                default:
                    return null;
            }
        }

        private static System.Drawing.Icon DriveIcon(DriveInfo di)
        {
            switch (di.DriveType)
            {
                case DriveType.Fixed:
                    return IconExtractor.Extract("shell32.dll", 79, true);

                case DriveType.Network:
                    return IconExtractor.Extract("shell32.dll", 273, true);

                case DriveType.Removable:
                    return IconExtractor.Extract("shell32.dll", 7, true);

                default:
                    return null;
            }
        }

    }
}
