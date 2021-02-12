using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Mmfm
{
    public class SortDescription<T> : ISortDescription<T>, INotifyPropertyChanged
    {
        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        public string DisplayName { get; }
        
        public Expression<Func<T, object>> SortExpression { get; }

        private bool isDescending;
        public bool IsDescending
        {
            get => isDescending;
            set
            {
                isDescending = value;
                OnPropertyChanged(nameof(IsDescending));
            }
        }
        
        public SortDescription(string displayName, Expression<Func<T, object>> sortExpression)
        {
            DisplayName = displayName;
            SortExpression = sortExpression;
            IsDescending = false;
        }
    }
}
