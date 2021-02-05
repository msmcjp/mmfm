using Mmfm.Commands;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Mmfm
{
    public class OperationProgressViewModel : INotifyPropertyChanged
    {
        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        public OperationProgressViewModel(IEnumerable<IOperationProvider> operationProviders)
        {
            OperationProviders = operationProviders;
            foreach(var operationProvider in OperationProviders)
            {
                operationProvider.TokenSource = tokenSource;
                operationProvider.OperationProgressed += OperationProvider_OperationProgressed;
            }
        }

        ~OperationProgressViewModel()
        {
            foreach (var operationProvider in OperationProviders)
            {
                operationProvider.TokenSource = new CancellationTokenSource();
                operationProvider.OperationProgressed -= OperationProvider_OperationProgressed;
            }
        }

        public IEnumerable<IOperationProvider> OperationProviders
        {
            get;
            private set;
        }

        private CancellationTokenSource tokenSource = new CancellationTokenSource();
        private int _value = 0;

        private ICommand startOperationCommand;
        public ICommand StartOperationCommand
        {
            get
            {
                if (startOperationCommand == null)
                {
                    startOperationCommand = new RelayCommand(async () =>
                    {
                        Count = OperationProviders.Sum(p => p.Count);
                        value = 0;
                        foreach(var op in OperationProviders)
                        {
                            _value = 0;
                            await Task.Run(op.Operation, tokenSource.Token);
                            if (tokenSource.IsCancellationRequested)
                            {
                                break;
                            }
                            value += op.Count;
                        }
                        OperationFinished = true;
                    });
                }
                return startOperationCommand;
            }
        }

        private string caption;
        public string Caption
        {
            get => caption;
            set
            {
                caption = value;
                OnPropertyChanged("Caption");
            }
        }

        private string current;
        public string Current
        {
            get => current;
            private set
            {
                current = value;
                OnPropertyChanged("Current");
            }
        }

        private bool operationFinished;
        public bool OperationFinished
        {
            get => operationFinished;
            private set
            {
                operationFinished = value;
                OnPropertyChanged("OperationFinished");
            }
        }

        public bool IsCancellationRequested => tokenSource.IsCancellationRequested;

        private int count;
        public int Count
        {
            get => count;
            private set
            {
                count = value;
                OnPropertyChanged("Count");
            }
        }

        private int value;
        public int Value => value + _value;   

        private ICommand cancelCommand;
        public ICommand CancelCommand
        {
            get
            {
                if(cancelCommand == null)
                {
                    cancelCommand = new RelayCommand(() => tokenSource.Cancel());
                }
                return cancelCommand;
            }
        }

        private void OperationProvider_OperationProgressed(object sender, OperationProgressedEventArgs e)
        {
            _value = e.Value;
            Current = e.Current;
            OnPropertyChanged("Value");
        }
    }
}
