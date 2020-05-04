using System;
using System.ComponentModel;
using System.Windows;

namespace ModernWpf.Controls
{
    [Obsolete]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class DrillTransition : TransitionElement
    {
        public static readonly DependencyProperty ModeProperty =
            DependencyProperty.Register(nameof(Mode), typeof(DrillTransitionMode), typeof(DrillTransition));

        public DrillTransitionMode Mode
        {
            get => (DrillTransitionMode)GetValue(ModeProperty);
            set => SetValue(ModeProperty, value);
        }

        public override ITransition GetTransition(UIElement element)
        {
            return Transitions.Drill(element, Mode);
        }
    }
}
