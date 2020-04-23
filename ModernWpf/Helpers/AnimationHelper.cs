using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace ModernWpf
{
    internal static class AnimationHelper
    {
        public static void DeferBegin(Storyboard storyboard)
        {
            storyboard.CurrentStateInvalidated += OnStoryboardCurrentStateInvalidated;

            static void OnStoryboardCurrentStateInvalidated(object sender, EventArgs e)
            {
                if (sender is Clock clock &&
                    clock.HasControllableRoot &&
                    clock.CurrentState == ClockState.Active &&
                    !clock.IsPaused)
                {
                    clock.Controller.Pause();
                    clock.Dispatcher.BeginInvoke(() =>
                    {
                        Debug.Assert(clock.IsPaused || clock.CurrentState != ClockState.Active);
                        if (clock.IsPaused)
                        {
                            clock.Controller.Resume();
                        }
                    }, DispatcherPriority.Loaded);
                }
            }
        }

        public static void DeferTransitions(VisualStateGroup group)
        {
            foreach (VisualTransition transition in group.Transitions)
            {
                var storyboard = transition.Storyboard;
                if (storyboard != null)
                {
                    DeferBegin(storyboard);
                }
            }
        }
    }
}
