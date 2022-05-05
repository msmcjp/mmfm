using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mmfm.ViewModel
{
    [Flags]
    public enum FileConflictAction
    {
        None = 0,
        Overwrite = 1,
        Skip = 2,
        Newer = 4,
        ApplyToAll = 8
    }

    public static class FileConflictActionExtension
    {
        public static bool? CanWrite(this FileConflictAction action, string source, string destination)
        {
            if (File.Exists(source) == false)
            {
                return null;
            }

            if (File.Exists(destination) == false)
            {
                return true;
            }

            if(action.HasFlag(FileConflictAction.Overwrite))
            {
                return true;
            }

            if(action.HasFlag(FileConflictAction.Newer))
            {
                return new FileInfo(source).LastWriteTime > new FileInfo(destination).LastWriteTime;
            }

            return false;
        }
    }
}
