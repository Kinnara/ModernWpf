using System.Runtime.CompilerServices;
using System.Windows.Media;

namespace ModernWpf
{
    internal static class WinRTColorHelper
    {
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static Color ToColor(this Windows.UI.Color color)
        {
            return Color.FromArgb(color.A, color.R, color.G, color.B);
        }
    }
}
