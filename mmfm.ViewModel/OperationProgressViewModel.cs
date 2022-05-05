using Mmfm.Model;
using Mmfm.ViewModel.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Mmfm.ViewModel
{
    public class OperationProgressViewModel : ContentDialogViewModel, IDisposable
    {
        public OperationProgressViewModel(IEnumerable<IOperationProvider> operationProviders) : base()
        {
            OperationProviders = operationProviders;
            foreach(var operationProvider in OperationProviders)
            {
                operationProvider.OperationProgressed += OperationProvider_OperationProgressed;
            }
        }

        public IEnumerable<IOperationProvider> OperationProviders
        {
            get;
            private set;
        }

        private CancellationTokenSource cancellationTokenSource = null;
        private int current = 0;

        private ICommand startOperationCommand;
        public ICommand StartOperationCommand
        {
            get
            {
                if (startOperationCommand == null)
                {
                    startOperationCommand = new RelayCommand(async () =>
                    {
                        cancellationTokenSource?.Dispose();
                        cancellationTokenSource = new CancellationTokenSource();

                        Count = OperationProviders.Sum(p => p.Count);
                        total = 0;
                        var cancellationToken = cancellationTokenSource.Token;
                        foreach (var op in OperationProviders)
                        {
                            current = 0;
                            await Task.Run(
                                op.ProvideOperationTaskWith(cancellationToken), 
                                cancellationToken
                            );
                            if (cancellationToken.IsCancellationRequested)
                            {
                                break;
                            }
                            total += op.Count;
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
                OnPropertyChanged(nameof(Caption));
            }
        }

        private string statusText;
        public string StatusText
        {
            get => $"({Value}/{Count}) {statusText}";
            private set
            {
                statusText = value;
                OnPropertyChanged(nameof(StatusText));
            }
        }

        private bool operationFinished;
        public bool OperationFinished
        {
            get => operationFinished;
            private set
            {
                operationFinished = value;
                OnPropertyChanged(nameof(OperationFinished));
            }
        }

        public bool IsCancellationRequested => cancellationTokenSource?.IsCancellationRequested ?? false;

        private int count;
        public int Count
        {
            get => count;
            private set
            {
                count = value;
                OnPropertyChanged(nameof(Count));
            }
        }

        private int total;
        public int Value => total + current;   

        private ICommand cancelCommand;
        public ICommand CancelCommand
        {
            get
            {
                if(cancelCommand == null)
                {
                    cancelCommand = new RelayCommand(() => cancellationTokenSource?.Cancel());
                }
                return cancelCommand;
            }
        }

        private void OperationProvider_OperationProgressed(object sender, OperationProgressedEventArgs e)
        {
            current = e.Current;
            StatusText = e.StatusText;
            OnPropertyChanged(nameof(Value));
        }

        private bool disposed = false;
        public void Dispose()
        {
            if (disposed)
            {
                return;
            }

            foreach (var operationProvider in OperationProviders)
            {
                operationProvider.OperationProgressed -= OperationProvider_OperationProgressed;
            }

            cancellationTokenSource?.Dispose();
            cancellationTokenSource = null;
        }
    }
}
