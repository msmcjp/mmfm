using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyFileManager
{
    public class FileTraverseOperation : IOperationProvider
    {
        public event OperationProgressedEventHandler OperationProgressed;
  
        public int MaxValue
        {
            get;
            private set;
        }

        public int Value
        {
            get;
            private set;
        }

        public IEnumerable<string> Paths
        {
            get;
            private set;
        }

        public Func<string, string, bool> Action
        {
            get;
            private set;
        }

        public bool BottomUp
        {
            get;
            set;
        }

        public FileTraverseOperation(IEnumerable<string> paths, Func<string, string, bool> action)
        {
            Paths = paths;
            Action = action;
            MaxValue = 0; Traverse(() => MaxValue++);
            Value = 0;
            BottomUp = false;
        }

        public Action Operation => () => Traverse(Action);

        private void Traverse(Action action)
        {
            Traverse((_, _) => { action(); return true; });
        }

        private void Traverse(Func<string, string, bool> action)
        {
            Value = 0;
            foreach (var path in Paths)
            {
                if (Traverse(Path.GetDirectoryName(path), path, action))
                {
                    return;
                }
            }
        }

        private bool ProgressOperation(string origin, string path, Func<string, string, bool> action)
        {
            var cancelled = (action(origin, path.Substring(origin.Length)) == false);
            OnOperationProgressed(Path.GetFileName(path), ++Value);

            return cancelled;
        }

        private bool Traverse(string origin, string path, Func<string, string, bool> action)
        {
            if (BottomUp == false && ProgressOperation(origin, path, action))
            {
                return true;
            }

            if (new FileInfo(path).Attributes.HasFlag(FileAttributes.Directory))
            {
                foreach (var childPath in Directory.GetFiles(path))
                {
                    if (Traverse(origin, childPath, action))
                    {
                        return true;
                    }
                }

                foreach (var childPath in Directory.GetDirectories(path))
                {
                    if (Traverse(origin, childPath, action))
                    {
                        return true;
                    }
                }
            }

            if (BottomUp == true && ProgressOperation(origin, path, action))
            {
                return true;
            }

            return false;
        }

        private void OnOperationProgressed(string current, int value)
        {
            OperationProgressed?.Invoke(this, new OperationProgressedEventArgs(current, value));
        }
    }
}
