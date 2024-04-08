using System.Globalization;
using System.Windows.Controls;

namespace NetShare.Validations
{
    public class IntegerValidationRule : ValidationRule
    {
        public int Min { get; set; } = 0;
        public int Max { get; set; } = int.MaxValue;

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            string? str = value as string;
            if(int.TryParse(str, NumberFormatInfo.InvariantInfo, out int val))
            {
                if(val >= Min && val <= Max)
                {
                    return ValidationResult.ValidResult;
                }
            }
            return new ValidationResult(false, "Could not parse value or out of range!");
        }
    }
}
