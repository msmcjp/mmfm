using Msmc.Patterns.Messenger;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Mmfm
{
    public static class ClipboardManager
    {
        public class Notification
        {
            public Notification()
            {
                bool move;
                Paths = GetDropFileList(out move);
                Move = move;
            }

            public string[] Paths { get; private set; }

            public bool Move { get; private set; }
        }  

        public static void Clear()
        {
            Clipboard.Clear();
            Messenger.Default.Send(new Notification());
        }

        public static void SetDropFileList(IEnumerable<string> paths, bool move)
        {
            var dropList = new System.Collections.Specialized.StringCollection();
            dropList.AddRange(paths.ToArray());

            var data = new DataObject();
            data.SetFileDropList(dropList);
            if (move)
            {
                var bytes = new byte[] { (byte)(move ? DragDropEffects.Move : DragDropEffects.Copy), 0, 0, 0 };
                data.SetData("Preferred DropEffect", new MemoryStream(bytes));
            }
            Clipboard.SetDataObject(data, true);
            Messenger.Default.Send(new Notification());
        }

        public static string[] GetDropFileList(out bool move)
        {
            move = false;
            var data = Clipboard.GetDataObject();
            if (data?.GetDataPresent(DataFormats.FileDrop) == false)
            {
                return new string[0];
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
