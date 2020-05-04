// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System;
using System.ComponentModel;
using System.Windows;

namespace ModernWpf.Controls
{
    /// <summary>
    /// Has
    /// <see cref="T:ModernWpf.Controls.TransitionElement"/>s
    /// for the designer experiences.
    /// </summary>
    [Obsolete]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class NavigationTransition : DependencyObject
    {
        /// <summary>
        /// The
        /// <see cref="T:System.Windows.DependencyProperty"/>
        /// for the backward
        /// <see cref="T:ModernWpf.Controls.NavigationTransition"/>.
        /// </summary>
        public static readonly DependencyProperty BackwardProperty =
            DependencyProperty.Register("Backward", typeof(TransitionElement), typeof(NavigationTransition), null);

        /// <summary>
        /// The
        /// <see cref="T:System.Windows.DependencyProperty"/>
        /// for the forward
        /// <see cref="T:ModernWpf.Controls.NavigationTransition"/>.
        /// </summary>
        public static readonly DependencyProperty ForwardProperty =
            DependencyProperty.Register("Forward", typeof(TransitionElement), typeof(NavigationTransition), null);

        /// <summary>
        /// The navigation transition will begin.
        /// </summary>
        public event RoutedEventHandler BeginTransition;

        /// <summary>
        /// The navigation transition has ended.
        /// </summary>
        public event RoutedEventHandler EndTransition;

        /// <summary>
        /// Gets or sets the backward
        /// <see cref="T:ModernWpf.Controls.NavigationTransition"/>.
        /// </summary>
        public TransitionElement Backward
        {
            get
            {
                return (TransitionElement)GetValue(BackwardProperty);
            }
            set
            {
                SetValue(BackwardProperty, value);
            }
        }

        /// <summary>
        /// Gets or sets the forward
        /// <see cref="T:ModernWpf.Controls.NavigationTransition"/>.
        /// </summary>
        public TransitionElement Forward
        {
            get
            {
                return (TransitionElement)GetValue(ForwardProperty);
            }
            set
            {
                SetValue(ForwardProperty, value);
            }
        }

        /// <summary>
        /// Triggers <see cref="E:ModernWpf.Controls.NavigationTransition.BeginTransition"/>.
        /// </summary>
        internal void OnBeginTransition()
        {
            if (BeginTransition != null)
            {
                BeginTransition(this, new RoutedEventArgs());
            }
        }

        /// <summary>
        /// Triggers <see cref="E:ModernWpf.Controls.NavigationTransition.EndTransition"/>.
        /// </summary>
        internal void OnEndTransition()
        {
            if (EndTransition != null)
            {
                EndTransition(this, new RoutedEventArgs());
            }
        }
    }
}