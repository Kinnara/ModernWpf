using MahApps.Metro.Controls;

namespace ModernWpf.MahApps.Controls
{
    /// <summary>
    /// Provides event data for the HamburgerMenuEx.DisplayModeChanged event.
    /// </summary>
    public sealed class HamburgerMenuDisplayModeChangedEventArgs
    {
        internal HamburgerMenuDisplayModeChangedEventArgs(SplitViewDisplayMode displayMode)
        {
            DisplayMode = displayMode;
        }

        /// <summary>
        /// Gets the new display mode.
        /// </summary>
        /// <returns>The new display mode.</returns>
        public SplitViewDisplayMode DisplayMode { get; }
    }
}
