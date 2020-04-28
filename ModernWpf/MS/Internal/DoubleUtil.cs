namespace MS.Internal
{
    internal static class DoubleUtil
    {
        public static int DoubleToInt(double val)
        {
            return (0 < val) ? (int)(val + 0.5) : (int)(val - 0.5);
        }
    }
}
