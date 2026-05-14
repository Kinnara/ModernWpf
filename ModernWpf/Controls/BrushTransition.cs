using System;
using System.Windows;

namespace ModernWpf.Controls
{
    public class BrushTransition : DependencyObject
    {
        public static readonly DependencyProperty DurationProperty =
            DependencyProperty.Register(
                nameof(Duration),
                typeof(TimeSpan),
                typeof(BrushTransition),
                new PropertyMetadata(TimeSpan.Zero));

        public TimeSpan Duration
        {
            get => (TimeSpan)GetValue(DurationProperty);
            set => SetValue(DurationProperty, value);
        }
    }
}
