using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;

namespace Mmfm.ViewModel
{
    public class ValidFileNameAttribute : ValidationAttribute
    {
        private bool IsValidFileName(string s) => !Path.GetInvalidFileNameChars().Any(c => s.Contains(c));

        public override bool IsValid(object value)
        {
            if (value is string == false)
            {
                return false;
            }
            return IsValidFileName((string)value);
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value is string == false || IsValidFileName((string)value) == false)
            {
                return new ValidationResult(ErrorMessage);
            }
            return ValidationResult.Success;
        }
    }
}
