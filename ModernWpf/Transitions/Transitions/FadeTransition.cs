using System.Windows;

namespace ModernWpf.Controls
{
    public class FadeTransition : TransitionElement
    {
        public static readonly DependencyProperty ModeProperty =
            DependencyProperty.Register(nameof(Mode), typeof(FadeTransitionMode), typeof(FadeTransition));

        public FadeTransitionMode Mode
        {
            get => (FadeTransitionMode)GetValue(ModeProperty);
            set => SetValue(ModeProperty, value);
        }

        public override ITransition GetTransition(UIElement element, Size containerSize)
        {
            return Transitions.Fade(element, Mode);
        }
    }
}
