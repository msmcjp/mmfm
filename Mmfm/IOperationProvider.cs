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
        /// Fires when an operation progressed.
        /// </summary>
        event OperationProgressedEventHandler OperationProgressed;

        /// <summary>
        /// Count of operation. 
        /// </summary>
        int Count { get; }

        /// <summary>
        /// Operation 
        /// </summary>
        Action Operation { get; }

        /// <summary>
        /// Cancellation token
        /// </summary>
        CancellationTokenSource TokenSource { get; set; }
    }
}
