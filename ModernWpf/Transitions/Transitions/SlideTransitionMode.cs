// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System;
using System.ComponentModel;

namespace ModernWpf.Controls
{
    /// <summary>
    /// The slide transition modes.
    /// </summary>
    [Obsolete]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public enum SlideTransitionMode
    {
        /// <summary>
        /// The slide up transition mode.
        /// </summary>
        SlideUp,
        /// <summary>
        /// The slide down transition mode.
        /// </summary>
        SlideDown,
        /// <summary>
        /// The slide left, fade in transition mode.
        /// </summary>
        SlideLeftIn,
        /// <summary>
        /// The slide left, fade out transition mode.
        /// </summary>
        SlideLeftOut,
        /// <summary>
        /// The slide right, fade in transition mode.
        /// </summary>
        SlideRightIn,
        /// <summary>
        /// The slide right, fade out transition mode.
        /// </summary>
        SlideRightOut
    };
}