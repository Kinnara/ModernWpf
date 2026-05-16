// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Markup;

namespace ModernWpf
{
    /// <summary>
    ///     A visual state with WinUI-style state setters.
    /// </summary>
    [ContentProperty("Storyboard")]
    public class VisualStateEx : VisualState
    {
        /// <summary>
        ///     Property setters applied while this visual state is active.
        /// </summary>
        public Collection<VisualStateSetter> Setters
        {
            get
            {
                if (_setters == null)
                {
                    _setters = new Collection<VisualStateSetter>();
                }

                return _setters;
            }
        }

        private Collection<VisualStateSetter> _setters;
    }
}
