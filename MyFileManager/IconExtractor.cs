using System;
using System.Drawing;
using System.Runtime.InteropServices;

namespace MyFileManager
{
    public class IconExtractor
    {
        private const uint SHGFI_ICON = 0x100;
        private const uint SHGFI_LARGEICON = 0x0;
        private const uint SHGFI_SMALLICON = 0x000000001;

        [StructLayout(LayoutKind.Sequential)]
        private struct SHFILEINFO
        {
            public IntPtr hIcon;
            public int iIcon;
            public uint dwAttributes;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public string szDisplayName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
            public string szTypeName;
        };

        [DllImport("shell32.dll")]
        private static extern IntPtr SHGetFileInfo(string pszPath, uint dwFileAttributes, ref SHFILEINFO psfi, uint cbSizeFileInfo, uint uFlags);

        public static Icon Extract(string path, bool largeIcon = true)
        {
            SHFILEINFO shinfo = new SHFILEINFO();
            SHGetFileInfo(
                path,
                0,
                ref shinfo,
                (uint)Marshal.SizeOf(shinfo),
                SHGFI_ICON | (largeIcon ? SHGFI_LARGEICON : SHGFI_SMALLICON)
            );
            try
            {
                return Icon.FromHandle(shinfo.hIcon);
            }
            catch (ArgumentException)
            {
                return null;
            }
        }

        [DllImport("Shell32.dll", EntryPoint = "ExtractIconExW", CharSet = CharSet.Unicode, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        private static extern int ExtractIconEx(string sFile, int iIndex, out IntPtr piLargeVersion, out IntPtr piSmallVersion, int amountIcons);

        public static Icon Extract(string file, int number, bool largeIcon = true)
        {
            IntPtr large;
            IntPtr small;
            ExtractIconEx(file, number, out large, out small, 1);
            try
            {
                return Icon.FromHandle(largeIcon ? large : small);
            }
            catch
            {
                return null;
            }
        }
    }

}
