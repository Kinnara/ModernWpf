using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media.Animation;

namespace ModernWpf.Controls
{
    [Obsolete]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class SuppressTransition : TransitionElement
    {
        public override ITransition GetTransition(UIElement element)
        {
            var storyboard = new Storyboard();
            var da = new DoubleAnimation(1, TimeSpan.FromMilliseconds(1));
            Storyboard.SetTargetProperty(da, new PropertyPath(UIElement.OpacityProperty));
            storyboard.Children.Add(da);
            Storyboard.SetTarget(storyboard, element);
            return new Transition(element, storyboard);
        }
    }
}
