using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Mmfm
{
    public interface IOperationProvider
    {
        /// <summary>
        /// Fires when the operation is progressed.
        /// </summary>
        event OperationProgressedEventHandler OperationProgressed;

        /// <summary>
        /// Count of the operation. 
        /// </summary>
        int Count { get; }

        /// <summary>
        /// Provides the operation task.
        /// </summary>
        /// <param name="cancellationTokenSource">
        /// The cancellation token object. 
        /// The operation task must be cancelled by a request of the object.
        /// </param>
        /// <returns>The task executes the operation.</returns>
        Func<Task> ProvideOperationTaskWith(CancellationToken cancellationToken);        
    }
}
