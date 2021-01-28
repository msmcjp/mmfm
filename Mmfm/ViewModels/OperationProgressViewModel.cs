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

        public OperationProgressViewModel(IOperationProvider operationProvider)
        {
            OperationProvider = operationProvider;
            OperationProvider.OperationProgressed += ProgressProvider_OperationProgressed;
        }

        ~OperationProgressViewModel()
        {
            OperationProvider.OperationProgressed -= ProgressProvider_OperationProgressed;
        }

        public IOperationProvider OperationProvider
        {
            get;
            private set;
        }

        private ICommand startOperationCommand;
        public ICommand StartOperationCommand
        {
            get
            {
                if (startOperationCommand == null)
                {
                    startOperationCommand = new RelayCommand(async () =>
                    {
                        Maximum = OperationProvider.MaxValue;
                        Value = 0;
                        await Task.Run(OperationProvider.Operation);                                                                     
                        OperationFinished = true;
                    });
                }
                return startOperationCommand;
            }
        }

        private string title;
        public string Title
        {
            get => title;
            set
            {
                title = value;
                OnPropertyChanged("Title");
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

        private int maximum;
        public int Maximum
        {
            get => maximum;
            private set
            {
                maximum = value;
                OnPropertyChanged("Maximum");
            }
        }

        private int value;
        public int Value
        {
            get => value;
            private set
            {
                this.value = value;
                OnPropertyChanged("Value");
            }
        }

        private void ProgressProvider_OperationProgressed(object sender, OperationProgressedEventArgs e)
        {
            Value = e.Value;
            Current = e.Current;
        }
    }
}
