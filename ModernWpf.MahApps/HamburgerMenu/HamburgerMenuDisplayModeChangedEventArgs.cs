using MahApps.Metro.Controls;
using System;

namespace ModernWpf.MahApps.Controls
{
    /// <summary>
    /// Provides event data for the HamburgerMenuEx.DisplayModeChanged event.
    /// </summary>
    [Obsolete]
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
