using System;
using System.Windows;

namespace ModernWpf.Controls
{
    [Obsolete]
    public class FadeTransition : TransitionElement
    {
        public static readonly DependencyProperty ModeProperty =
            DependencyProperty.Register(nameof(Mode), typeof(FadeTransitionMode), typeof(FadeTransition));

        public FadeTransitionMode Mode
        {
            get => (FadeTransitionMode)GetValue(ModeProperty);
            set => SetValue(ModeProperty, value);
        }

        public override ITransition GetTransition(UIElement element)
        {
            return Transitions.Fade(element, Mode);
        }
    }
}
