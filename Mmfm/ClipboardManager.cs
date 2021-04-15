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
                Paths = Clipboard.GetDataObject().FromFileDropList(out move);
                Move = move;
            }

            public IEnumerable<string> Paths { get; private set; }

            public bool Move { get; private set; }
        }  

        public static void Clear()
        {
            Clipboard.Clear();
            Messenger.Default.Send(new Notification());
        }

        public static void SetDropFileList(IEnumerable<string> paths, bool move)
        {
            Clipboard.SetDataObject(paths.ToFileDropList(move), true);
            Messenger.Default.Send(new Notification());
        }
    }
}
