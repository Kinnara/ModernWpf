using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModernWpf.Controls
{
    /// <summary>
    /// enumeration for the different transition types
    /// </summary>
    public enum TransitionType
    {
        /// <summary>
        /// Use the VisualState DefaultTransition
        /// </summary>
        Default,

        /// <summary>
        /// Use the VisualState Normal
        /// </summary>
        Normal,

        /// <summary>
        /// Use the VisualState UpTransition
        /// </summary>
        Up,

        /// <summary>
        /// Use the VisualState DownTransition
        /// </summary>
        Down,

        /// <summary>
        /// Use the VisualState RightTransition
        /// </summary>
        Right,

        /// <summary>
        /// Use the VisualState RightReplaceTransition
        /// </summary>
        RightReplace,

        /// <summary>
        /// Use the VisualState LeftTransition
        /// </summary>
        Left,

        /// <summary>
        /// Use the VisualState LeftReplaceTransition
        /// </summary>
        LeftReplace,

        /// <summary>
        /// Use a custom VisualState, the name must be set using CustomVisualStatesName property
        /// </summary>
        Custom
    }

    /// <summary>
    /// For specifying where the navigation index is placed relative to the <see cref="FlipViewItem"/>.
    /// </summary>
    public enum NavigationIndexPlacement
    {
        /// <summary>
        /// Index on left side
        /// </summary>
        Left,

        /// <summary>
        /// Index on right side
        /// </summary>
        Right,

        /// <summary>
        /// Index on top side
        /// </summary>
        Top,

        /// <summary>
        /// Index on bottom side
        /// </summary>
        Bottom,

        /// <summary>
        /// Index on left side over the item
        /// </summary>
        LeftOverItem,

        /// <summary>
        /// Index on right side over the item
        /// </summary>
        RightOverItem,

        /// <summary>
        /// Index on top side over the item
        /// </summary>
        TopOverItem,

        /// <summary>
        /// Index on bottom side over the item
        /// </summary>
        BottomOverItem
    }

    public enum NavigationButtonsPosition
    {
        Inside,
        Outside
    }
}
