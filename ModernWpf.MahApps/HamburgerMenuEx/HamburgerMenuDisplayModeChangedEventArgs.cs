using MahApps.Metro.Controls;

namespace ModernWpf.MahApps.Controls
{
    public sealed class HamburgerMenuDisplayModeChangedEventArgs
    {
        internal HamburgerMenuDisplayModeChangedEventArgs(SplitViewDisplayMode displayMode)
        {
            DisplayMode = displayMode;
        }

        public SplitViewDisplayMode DisplayMode { get; }
    }
}
