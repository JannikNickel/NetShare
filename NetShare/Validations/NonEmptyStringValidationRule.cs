using System.Globalization;
using System.Windows.Controls;

namespace NetShare.Validations
{
    public class NonEmptyStringValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            string str = (string)value;
            if(string.IsNullOrEmpty(str) || string.IsNullOrWhiteSpace(str))
            {
                return new ValidationResult(false, "Input can't be empty or whitespace!");
            }
            return ValidationResult.ValidResult;
        }
    }
}
