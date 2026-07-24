using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows.Controls;

namespace ModernWpf.Gallery.Helpers
{
    /// <summary>
    /// Validation rule that ensures the input contains only English alphabetic characters (a-z, A-Z).
    /// </summary>
    public class AlphabeticValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            var input = value as string;

            if (string.IsNullOrEmpty(input))
            {
                return ValidationResult.ValidResult;
            }

            // Check if the input contains only English alphabetic characters (a-z, A-Z)
            if (!Regex.IsMatch(input, @"^[a-zA-Z]+$"))
            {
                return new ValidationResult(false, "Only English alphabetic characters (a-z, A-Z) are allowed.");
            }

            return ValidationResult.ValidResult;
        }
    }
}
