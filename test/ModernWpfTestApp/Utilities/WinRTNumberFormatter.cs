using ModernWpf.Controls;
using Windows.Globalization.NumberFormatting;

namespace MUXControlsTestApp.Utilities
{
    public class WinRTNumberFormatter : INumberBoxNumberFormatter
    {
        private readonly DecimalFormatter _formatter;

        public WinRTNumberFormatter(DecimalFormatter formatter)
        {
            _formatter = formatter;
        }

        public string FormatDouble(double value)
        {
            return _formatter.FormatDouble(value);
        }

        public double? ParseDouble(string text)
        {
            return _formatter.ParseDouble(text);
        }
    }
}
