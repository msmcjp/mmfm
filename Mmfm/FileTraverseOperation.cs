using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
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

        public Func<string, Task<bool>> Action
        {
            get;
            private set;
        }

        public FileTraverseOperation(string path, Func<string, Task<bool>> action)
        {
            Path = path;
            Action = action;
            Count = TraverseAsync(null, CancellationToken.None).CountAsync().Result;
        }

        public Func<Task> ProvideOperationTaskWith(CancellationToken token)
        {
            return () => TraverseAsync(Action, token).ToArrayAsync().AsTask();
        }

        private SemaphoreSlim traverseSemaphore = new SemaphoreSlim(1, 1);
        private bool isCancellationRequestedByAction = false;

        private async IAsyncEnumerable<FileInfo> TraverseAsync(Func<string, Task<bool>> action, [EnumeratorCancellation] CancellationToken token)
        {
            var count = 0;
            isCancellationRequestedByAction = false;
            await foreach(var fileInfo in TraverseAsync(Path, async (path) =>
            {
                OperationProgressed?.Invoke(this, new OperationProgressedEventArgs(System.IO.Path.GetFileName(path), ++count));

                var cancelled = false;
                if(action != null)
                {
                    cancelled = await action.Invoke(path);
                }
                return cancelled;
            }, token)
            ){
                yield return fileInfo;
            }
        }

        private async IAsyncEnumerable<FileInfo> TraverseAsync(string path, Func<string, Task<bool>> action, [EnumeratorCancellation] CancellationToken token)
        {
            if (token.IsCancellationRequested)
            {
                yield break;
            }
 
            if(File.Exists(path) == false && Directory.Exists(path) == false)
            {
                yield break;
            }

            var fileInfo = new FileInfo(path);
  
            if (fileInfo.Attributes.HasFlag(FileAttributes.Directory))
            {
                var children = Directory.GetDirectories(path).Concat(Directory.GetFiles(path)).ToArray().ToAsyncEnumerable();
                await foreach (var child in children.SelectMany(child => TraverseAsync(child, action, token)))
                {
                    yield return child;
                }
            }

            traverseSemaphore.Wait(); 
            try
            {
                if (isCancellationRequestedByAction)
                {
                    yield break;
                }

                if (await action?.Invoke(path) == true)
                {
                    isCancellationRequestedByAction = true;
                }
            }
            finally
            {
                traverseSemaphore.Release();
            }

            yield return fileInfo;
        }
    }
}
