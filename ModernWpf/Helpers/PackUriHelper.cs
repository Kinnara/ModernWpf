using System;

namespace ModernWpf
{
    internal static class PackUriHelper
    {
        public static Uri GetAbsoluteUri(string path)
        {
            return new Uri($"pack://application:,,,/ModernWpf;component/{path}");
        }
    }
}
