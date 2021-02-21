using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Mmfm
{
    public class FileTraverseOperation : IOperationProvider
    {
        public event OperationProgressedEventHandler OperationProgressed;

        public int Count
        {
            get;
            private set;
        }

        public string Path
        {
            get;
            private set;
        }

        public Func<string, bool> Action
        {
            get;
            private set;
        }

        public FileTraverseOperation(string path, Func<string, bool> action)
        {
            Path = path;
            Action = action;
            Count = Traverse().Count();
         }

        public Action Operation => () => Traverse(Action).Count();
        
        public CancellationTokenSource TokenSource { get; set; } = new CancellationTokenSource();

        private IEnumerable<string> Traverse(Func<string, bool> action = null)
        {
            var count = 0;
            return Traverse(Path, (path) =>
            {
                var cancelled = action?.Invoke(path);
                OperationProgressed?.Invoke(this, new OperationProgressedEventArgs(path, ++count));
                return cancelled == true;
            });
        }

        private IEnumerable<string> Traverse(string path, Func<string, bool> action)
        {
            if (TokenSource.IsCancellationRequested)
            {
                yield break;
            }
 
            if(File.Exists(path) == false && Directory.Exists(path) == false)
            {
                yield break;
            }

            if (new FileInfo(path).Attributes.HasFlag(FileAttributes.Directory))
            {
                var children = Directory.GetDirectories(path).Concat(Directory.GetFiles(path)).ToArray();
                foreach(var child in children.SelectMany(child => Traverse(child, action)))
                {
                    yield return child;
                }
            }

            if (action?.Invoke(path) == true)
            {
                yield break;
            }
            yield return path;
        }
    }
}
