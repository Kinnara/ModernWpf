// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Windows;

namespace ModernWpf
{
    /// <summary>
    ///     Describes a WinUI-style visual-state setter.
    /// </summary>
    public class VisualStateSetter : DependencyObject
    {
        public static readonly DependencyProperty TargetProperty =
            DependencyProperty.Register(
                nameof(Target),
                typeof(string),
                typeof(VisualStateSetter));

        public static readonly DependencyProperty PropertyProperty =
            DependencyProperty.Register(
                nameof(Property),
                typeof(string),
                typeof(VisualStateSetter));

        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register(
                nameof(Value),
                typeof(object),
                typeof(VisualStateSetter));

        /// <summary>
        ///     Target path such as "Element.Property" or "Element.(Grid.Column)".
        /// </summary>
        public string Target
        {
            get => (string)GetValue(TargetProperty);
            set => SetValue(TargetProperty, value);
        }

        /// <summary>
        ///     Property path used when <see cref="Target"/> only names an element.
        /// </summary>
        public string Property
        {
            get => (string)GetValue(PropertyProperty);
            set => SetValue(PropertyProperty, value);
        }

        /// <summary>
        ///     Value to apply while the owning state is active.
        /// </summary>
        public object Value
        {
            get => GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
        }
    }
}
