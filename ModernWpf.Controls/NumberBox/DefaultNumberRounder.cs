using System.Globalization;

namespace ModernWpf.Controls
{
    internal class DefaultNumberRounder
    {
        public int SignificantDigits { get; set; } = 10;

        public double RoundDouble(double value)
        {
            // WPF bindings promote Single values to NumberBox.Value's double
            // type. Preserve the Single's shortest round-trip representation
            // instead of displaying digits introduced by that conversion.
            var singleValue = (float)value;
            if ((double)singleValue == value)
            {
                return double.Parse(
                    singleValue.ToString("R", CultureInfo.InvariantCulture),
                    CultureInfo.InvariantCulture);
            }

            return double.Parse(
                value.ToString("G" + SignificantDigits, CultureInfo.InvariantCulture),
                CultureInfo.InvariantCulture);
        }
    }
}
