namespace Mmfm.Model
{
    public class OperationProgressedEventArgs
    {
        public string StatusText
        {
            get;
            private set;
        }

        public int Current
        {
            get;
            private set;
        }

        public OperationProgressedEventArgs(string statusText, int current)
        {
            StatusText = statusText;
            Current = current;
        }
    }

    public delegate void OperationProgressedEventHandler(object sender, OperationProgressedEventArgs e);
}
