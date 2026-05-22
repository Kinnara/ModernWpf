using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows.Controls;

namespace ModernWpf.Gallery.Helpers
{
    public sealed class AlphabeticValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            var text = value as string;
            if (string.IsNullOrEmpty(text))
            {
                return ValidationResult.ValidResult;
            }

            if (!Regex.IsMatch(text, @"^[a-zA-Z]+$"))
            {
                return new ValidationResult(false, "Only English alphabetic characters (a-z, A-Z) are allowed.");
            }

            return ValidationResult.ValidResult;
        }
    }
}
