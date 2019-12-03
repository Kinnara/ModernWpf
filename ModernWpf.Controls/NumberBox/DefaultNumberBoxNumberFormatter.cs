namespace ModernWpf.Controls
{
    internal class DefaultNumberBoxNumberFormatter : INumberBoxNumberFormatter
    {
        public string FormatDouble(double value)
        {
            return value.ToString();
        }

        public double? ParseDouble(string text)
        {
            if (double.TryParse(text, out double result))
            {
                return result;
            }
            return null;
        }
    }
}
