namespace ModernWpf.Controls
{
    internal class DefaultNumberRounder
    {
        public double RoundDouble(double value)
        {
            return double.Parse(value.ToString("G12"));
        }
    }
}
