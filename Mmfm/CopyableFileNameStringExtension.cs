using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mmfm
{
    public static class CopyableFileNameStringExtension
    {
        public static string CopyableFileName(this string path)
        {
            var newPath = path;
            while (File.Exists(newPath) || Directory.Exists(newPath))
            {
                newPath = Path.Combine(
                    Path.GetDirectoryName(newPath),
                    $"{Path.GetFileNameWithoutExtension(newPath)}{Properties.Resources.Posfix_Copy}{Path.GetExtension(newPath)}"
                );
            }
            return newPath;
        }
    }
}
