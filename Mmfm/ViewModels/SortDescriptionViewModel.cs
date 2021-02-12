using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Mmfm
{
    public class SortDescriptionViewModel<T> : ISortDescription<T>, INotifyPropertyChanged
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

        public string HeaderText
        {
            get
            {
                if(IsDescending == null) { return DisplayName; }
                return $"{DisplayName} {((IsDescending == true) ? '\U000025bc' : '\U000025b2')}";
            }
        }
        
        private bool? isDescending;
        public bool? IsDescending
        {
            get => isDescending;
            set
            {
                isDescending = value;
                OnPropertyChanged(nameof(IsDescending));
                OnPropertyChanged(nameof(HeaderText));
            }
        }
        
        public SortDescriptionViewModel(string displayName, Expression<Func<T, object>> sortExpression)
        {
            DisplayName = displayName;
            SortExpression = sortExpression;
            IsDescending = null;
        }
    }
}
