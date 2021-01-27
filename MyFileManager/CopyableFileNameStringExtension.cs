using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyFileManager
{
    public static class CopyableFileNameStringExtension
    {
        public static string CopyableFileName(this string path)
        {
            var newPath = path;
            while (File.Exists(newPath))
            {
                newPath = Path.Combine(
                    Path.GetDirectoryName(path),
                    $"{Path.GetFileNameWithoutExtension(path)}{Properties.Resources.Posfix_Copy}{Path.GetExtension(path)}"
                );
            }
            return newPath;
        }
    }
}
