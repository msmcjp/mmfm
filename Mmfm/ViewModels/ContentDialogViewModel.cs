using ModernWpf.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Input;
using System.Linq;
using System.ComponentModel.DataAnnotations;

namespace Mmfm
{
    public abstract class ContentDialogViewModel : INotifyPropertyChanged, INotifyDataErrorInfo
    {        
        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName]string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            Validate(propertyName);
        }
        #endregion

        #region INotifyDataErrorInfo
        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;
        
        private Dictionary<string, IEnumerable<string>> validationErrors = new Dictionary<string, IEnumerable<string>>();

        private void Validate(string propertyName)
        {
            var value = GetType().GetProperty(propertyName).GetValue(this);
            var context = new ValidationContext(this) { MemberName = propertyName };
            var errors = new List<ValidationResult>();
       
            validationErrors.Remove(propertyName);
            if (Validator.TryValidateProperty(value, context, errors) == false)
            {
                validationErrors.Add(
                    propertyName, 
                    errors.Select(e => e.ErrorMessage).ToList().AsReadOnly()
                );
            }
            ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
        }

        public bool HasErrors => validationErrors.SelectMany(error => error.Value).Count() > 0;

        public System.Collections.IEnumerable GetErrors(string propertyName) => validationErrors.ContainsKey(propertyName) ? validationErrors[propertyName] : Enumerable.Empty<string>();
        #endregion

        public ContentDialogResult Result { get; set; }  
    }
}
