using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Mmfm
{
    public static class DropFileListExtension
    {
        public static IDataObject ToFileDropList(this IEnumerable<string> strings, bool move = false)
        {
            if(strings == null || strings.Count() == 0)
            {
                return null;
            }

            var dropList = new System.Collections.Specialized.StringCollection();
            dropList.AddRange(strings.ToArray());

            var data = new DataObject();
            data.SetFileDropList(dropList);
            if (move)
            {
                var bytes = new byte[] { (byte)(DragDropEffects.Move), 0, 0, 0 };
                data.SetData("Preferred DropEffect", new MemoryStream(bytes));
            }

            return data;
        }

        public static IEnumerable<string> FromFileDropList(this IDataObject data, out bool move)
        {
            move = false;

            if (data?.GetDataPresent(DataFormats.FileDrop) == false)
            {
                return Enumerable.Empty<string>();
            }

            var paths = data.GetData(DataFormats.FileDrop) as string[];
            var stream = data.GetData("Preferred DropEffect") as MemoryStream;
            if (stream != null)
            {
                var effects = (DragDropEffects)stream.ReadByte();
                move = effects.HasFlag(DragDropEffects.Move);
            }
            return paths;
        }
    }
}
