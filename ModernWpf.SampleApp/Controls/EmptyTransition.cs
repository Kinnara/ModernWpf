using ModernWpf.Controls;
using System;
using System.Windows;
using System.Windows.Media.Animation;

namespace ModernWpf.SampleApp.Controls
{
    public class EmptyTransition : TransitionElement
    {
        public override ITransition GetTransition(UIElement element, Size containerSize)
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
