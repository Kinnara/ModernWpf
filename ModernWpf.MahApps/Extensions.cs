namespace ModernWpf.MahApps
{
    internal static class Extensions
    {
        public static string DefaultIfNullOrEmpty(this string s, string defaultValue)
        {
            return !string.IsNullOrEmpty(s) ? s : defaultValue;
        }
    }
}
