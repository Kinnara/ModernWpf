// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System.Windows;

namespace ModernWpf.Controls
{
    /// <summary>
    /// Provides slide <see cref="T:ModernWpf.Controls.ITransition"/>s.
    /// </summary>
    public class SlideTransition : TransitionElement
    {
        /// <summary>
        /// The
        /// <see cref="T:System.Windows.DependencyProperty"/>
        /// for the
        /// <see cref="T:ModernWpf.Controls.SlideTransitionMode"/>.
        /// </summary>
        public static readonly DependencyProperty ModeProperty =
            DependencyProperty.Register("Mode", typeof(SlideTransitionMode), typeof(SlideTransition), null);

        /// <summary>
        /// The <see cref="T:ModernWpf.Controls.SlideTransitionMode"/>.
        /// </summary>
        public SlideTransitionMode Mode
        {
            get
            {
                return (SlideTransitionMode)GetValue(ModeProperty);
            }
            set
            {
                SetValue(ModeProperty, value);
            }
        }

        /// <summary>
        /// Creates a new
        /// <see cref="T:ModernWpf.Controls.ITransition"/>
        /// for a
        /// <see cref="T:System.Windows.UIElement"/>.
        /// Saves and clears the existing
        /// <see cref="F:System.Windows.UIElement.RenderTransformProperty"/>
        /// value before the start of the transition, then restores it after it is stopped or completed.
        /// </summary>
        /// <param name="element">The <see cref="T:System.Windows.UIElement"/>.</param>
        /// <returns>The <see cref="T:ModernWpf.Controls.ITransition"/>.</returns>
        public override ITransition GetTransition(UIElement element)
        {
            return Transitions.Slide(element, Mode);
        }
    }
}