// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Windows;
using System.Windows.Media;

namespace ModernWpf.Controls
{
    /// <summary>
    /// Represents a shareable object used to create an icon that displays and controls a visual that can animate in response to user interaction and visual state changes.
    /// </summary>
    public class AnimatedIconSource : IconSource
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AnimatedIconSource"/> class.
        /// </summary>
        public AnimatedIconSource()
        {
        }

        /// <summary>
        /// Identifies the <see cref="FallbackIconSource"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty FallbackIconSourceProperty =
            DependencyProperty.Register(
                nameof(FallbackIconSource),
                typeof(IconSource),
                typeof(AnimatedIcon));

        /// <summary>
        /// Gets or sets the static icon to use when the animated icon cannot run.
        /// </summary>
        /// <value>The static icon to use when the animated icon cannot run. The default is <see langword="null"/>.</value>
        public IconSource FallbackIconSource
        {
            get => (IconSource)GetValue(FallbackIconSourceProperty);
            set => SetValue(FallbackIconSourceProperty, value);
        }

        /// <summary>
        /// Identifies the <see cref="MirroredWhenRightToLeft"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty MirroredWhenRightToLeftProperty =
            DependencyProperty.Register(
                nameof(MirroredWhenRightToLeft),
                typeof(bool),
                typeof(AnimatedIcon));

        /// <summary>
        /// Gets or sets a value that indicates whether the icon is mirrored when the <see cref="FlowDirection"/> is RightToLeft.
        /// </summary>
        /// <value><see langword="true"/>, if the icon is mirrored when the <see cref="FlowDirection"/> is <see cref="FlowDirection.RightToLeft"/>. Otherwise, <see langword="false"/>. The default is <see langword="false"/>.</value>
        public bool MirroredWhenRightToLeft
        {
            get => (bool)GetValue(MirroredWhenRightToLeftProperty);
            set => SetValue(MirroredWhenRightToLeftProperty, value);
        }

        /// <summary>
        /// Identifies the <see cref="Source"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty SourceProperty =
            DependencyProperty.Register(
                nameof(Source),
                typeof(AnimatedVisualSource),
                typeof(AnimatedIcon));

        /// <summary>
        /// Gets or sets the animated visual shown by the <see cref="AnimatedIcon"/> object.
        /// </summary>
        /// <value>The animated visual shown by the <see cref="AnimatedIcon"/>. The default is <see langword="null"/>.</value>
        public AnimatedVisualSource Source
        {
            get => (AnimatedVisualSource)GetValue(SourceProperty);
            set => SetValue(SourceProperty, value);
        }

        /// <summary>
        /// Identifies the State dependency property.
        /// </summary>
        public static readonly DependencyProperty StateProperty =
            DependencyProperty.Register(
                "State",
                typeof(string),
                typeof(AnimatedIcon));


        /// <summary>
        /// Retrieves the value of the AnimatedIcon.State attached property for the specified <see cref="DependencyObject"/>.
        /// </summary>
        /// <param name="control">The object from which the property value is retrieved.</param>
        /// <returns>The current value of the AnimatedIcon.State attached property on the specified dependency object.</returns>
        public static string GetState(DependencyObject control)
        {
            return (string)control.GetValue(StateProperty);
        }

        /// <summary>
        /// Specifies the value of the AnimatedIcon.State attached property for the specified <see cref="DependencyObject"/>.
        /// </summary>
        /// <param name="control">The element on which to set the attached property.</param>
        /// <param name="value">The value of the AnimatedIcon.State attached property on the specified dependency object.</param>
        public static void SetState(DependencyObject control, string value)
        {
            control.SetValue(StateProperty, value);
        }

        public static readonly DependencyProperty FontSizeProperty =
            DependencyProperty.Register(
                nameof(FontSize),
                typeof(double),
                typeof(AnimatedIcon),
                new PropertyMetadata(20.0));

        public double FontSize
        {
            get => (double)GetValue(FontSizeProperty);
            set => SetValue(FontSizeProperty, value);
        }

        /// <inheritdoc/>
        public override IconElement CreateIconElementCore()
        {
            AnimatedIcon animatedIcon = new AnimatedIcon();

            animatedIcon.Source = Source;
            animatedIcon.FallbackIconSource = FallbackIconSource;
            animatedIcon.MirroredWhenRightToLeft = MirroredWhenRightToLeft;
            animatedIcon.FontSize = FontSize;
            var newForeground = Foreground;
            if (newForeground != null)
            {
                animatedIcon.Foreground = newForeground;
            }

            return animatedIcon;
        }
    }
}
