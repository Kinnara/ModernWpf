// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace ModernWpf.Controls
{
    /// <summary>
    /// Defines constants that specify how panes are shown in a <see cref="TwoPaneView"/>.
    /// </summary>
    /// <seealso cref="TwoPaneView"/>
    /// <seealso cref="TwoPaneView.Mode"/>
    public enum TwoPaneViewMode
    {
        /// <summary>
        /// Only one pane is shown.
        /// </summary>
        SinglePane = 0,

        /// <summary>
        /// Panes are shown side-by-side.
        /// </summary>
        Wide = 1,

        /// <summary>
        /// Panes are shown top-bottom.
        /// </summary>
        Tall = 2,
    }

    /// <summary>
    /// Defines constants that specify which pane has priority in a TwoPaneView.
    /// </summary>
    /// <seealso cref="TwoPaneView"/>
    /// <seealso cref="TwoPaneView.PanePriority"/>
    public enum TwoPaneViewPriority
    {
        /// <summary>
        /// Pane 1 has priority.
        /// </summary>
        Pane1 = 0,
        /// <summary>
        /// Pane 2 has priority.
        /// </summary>
        Pane2 = 1,
    }

    /// <summary>
    /// Defines constants that specify how panes are shown in a <see cref="TwoPaneView"/> in tall mode.
    /// </summary>
    /// <seealso cref="TwoPaneView"/>
    /// <seealso cref="TwoPaneView.TallModeConfiguration"/>
    public enum TwoPaneViewTallModeConfiguration
    {
        /// <summary>
        /// Only the pane that has priority is shown, the other pane is hidden.
        /// </summary>
        SinglePane = 0,

        /// <summary>
        /// The pane that has priority is shown on top, the other pane is shown on the bottom.
        /// </summary>
        TopBottom = 1,

        /// <summary>
        /// The pane that has priority is shown on the bottom, the other pane is shown on top.
        /// </summary>
        BottomTop = 2,
    }

    /// <summary>
    /// Defines constants that specify how panes are shown in a <see cref="TwoPaneView"/> in wide mode.
    /// </summary>
    /// <seealso cref="TwoPaneView"/>
    /// <seealso cref="TwoPaneView.WideModeConfiguration"/>
    public enum TwoPaneViewWideModeConfiguration
    {
        /// <summary>
        /// Only the pane that has priority is shown, the other pane is hidden.
        /// </summary>
        SinglePane = 0,

        /// <summary>
        /// The pane that has priority is shown on the left, the other pane is shown on the right.
        /// </summary>
        LeftRight = 1,

        /// <summary>
        /// The pane that has priority is shown on the right, the other pane is shown on the left.
        /// </summary>
        RightLeft = 2,
    }

    internal enum ViewMode
    {
        Pane1Only,
        Pane2Only,
        LeftRight,
        RightLeft,
        TopBottom,
        BottomTop,
        None
    };
}
