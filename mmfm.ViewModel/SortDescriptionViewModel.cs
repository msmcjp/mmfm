using Mmfm.Model;
using System;
using System.ComponentModel;
using System.Linq.Expressions;

namespace Mmfm.ViewModel
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

        public string Name { get; }
        
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
        
        public SortDescriptionViewModel(string name, Expression<Func<T, object>> sortExpression, string displayName = null)
        {
            Name = name;
            DisplayName = displayName ?? name;
            SortExpression = sortExpression;
            IsDescending = null;
        }
    }
}
