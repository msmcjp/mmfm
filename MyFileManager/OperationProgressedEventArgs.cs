using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyFileManager
{
    public class OperationProgressedEventArgs
    {
        public string Current
        {
            get;
            private set;
        }

        public int Value
        {
            get;
            private set;
        }

        public OperationProgressedEventArgs(string current, int value)
        {
            Current = current;
            Value = value;
        }
    }

    public delegate void OperationProgressedEventHandler(object sender, OperationProgressedEventArgs e);
}
