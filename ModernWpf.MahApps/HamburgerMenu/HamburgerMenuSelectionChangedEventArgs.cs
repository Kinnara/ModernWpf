using System;

namespace ModernWpf.MahApps.Controls
{
    /// <summary>
    /// Provides data for the HamburgerMenuEx.SelectionChanged event.
    /// </summary>
    [Obsolete]
    public sealed class HamburgerMenuSelectionChangedEventArgs
    {
        /// <summary>
        /// Gets the newly selected menu item.
        /// </summary>
        /// <returns>The newly selected menu item.</returns>
        public object SelectedItem { get; internal set; }
    }
}
