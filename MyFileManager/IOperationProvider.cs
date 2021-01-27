using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyFileManager
{
    public interface IOperationProvider
    {
        event OperationProgressedEventHandler OperationProgressed;

        int MaxValue { get; }

        Action Operation { get; }
    }
}
