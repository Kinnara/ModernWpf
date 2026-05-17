using System.Globalization;

namespace ModernWpf.Controls
{
    internal class DefaultNumberRounder
    {
        public int SignificantDigits { get; set; } = 10;

        public double RoundDouble(double value)
        {
            return double.Parse(
                value.ToString("G" + SignificantDigits, CultureInfo.InvariantCulture),
                CultureInfo.InvariantCulture);
        }
    }
}
