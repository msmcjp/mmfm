using System.IO;

namespace Mmfm
{
    public static class DriveInfoExtensions
    {
        public static string DriveDescription(this DriveInfo di)
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

        public static System.Drawing.Icon DriveIcon(this DriveInfo di)
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
