using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace ModernWpf.Media.Animation
{
    /// <summary>
    /// Specifies the animation to run when a user navigates forward in a logical hierarchy,
    /// like from a master list to a detail page.
    /// </summary>
    public sealed class DrillInNavigationTransitionInfo : NavigationTransitionInfo
    {
        /// <summary>
        /// Initializes a new instance of the DrillInNavigationTransitionInfo class.
        /// </summary>
        public DrillInNavigationTransitionInfo()
        {
        }

        internal override NavigationAnimation GetEnterAnimation(FrameworkElement element, bool movingBackwards)
        {
            var storyboard = new Storyboard();

            if (movingBackwards)
            {
                var scaleXAnim = new DoubleAnimationUsingKeyFrames
                {
                    KeyFrames =
                    {
                        new DiscreteDoubleKeyFrame(1.15, TimeSpan.Zero),
                        new SplineDoubleKeyFrame(1, EnterDuration, DecelerateKeySpline)
                    }
                };
                Storyboard.SetTargetProperty(scaleXAnim, ScaleXPath);
                storyboard.Children.Add(scaleXAnim);

                var scaleYAnim = new DoubleAnimationUsingKeyFrames
                {
                    KeyFrames =
                    {
                        new DiscreteDoubleKeyFrame(1.15, TimeSpan.Zero),
                        new SplineDoubleKeyFrame(1, EnterDuration, DecelerateKeySpline)
                    }
                };
                Storyboard.SetTargetProperty(scaleYAnim, ScaleYPath);
                storyboard.Children.Add(scaleYAnim);

                var opacityAnim = new DoubleAnimationUsingKeyFrames
                {
                    KeyFrames =
                    {
                        new DiscreteDoubleKeyFrame(0, TimeSpan.Zero),
                        new SplineDoubleKeyFrame(1, EnterDuration, DecelerateKeySpline)
                    }
                };
                Storyboard.SetTargetProperty(opacityAnim, OpacityPath);
                storyboard.Children.Add(opacityAnim);
            }
            else
            {
                var scaleXAnim = new DoubleAnimationUsingKeyFrames
                {
                    KeyFrames =
                    {
                        new DiscreteDoubleKeyFrame(0.9, TimeSpan.Zero),
                        new SplineDoubleKeyFrame(1, MaxMoveDuration, DecelerateKeySpline)
                    }
                };
                Storyboard.SetTargetProperty(scaleXAnim, ScaleXPath);
                storyboard.Children.Add(scaleXAnim);

                var scaleYAnim = new DoubleAnimationUsingKeyFrames
                {
                    KeyFrames =
                    {
                        new DiscreteDoubleKeyFrame(0.9, TimeSpan.Zero),
                        new SplineDoubleKeyFrame(1, MaxMoveDuration, DecelerateKeySpline)
                    }
                };
                Storyboard.SetTargetProperty(scaleYAnim, ScaleYPath);
                storyboard.Children.Add(scaleYAnim);

                var opacityAnim = new DoubleAnimationUsingKeyFrames
                {
                    KeyFrames =
                    {
                        new DiscreteDoubleKeyFrame(0, TimeSpan.Zero),
                        new SplineDoubleKeyFrame(1, MaxMoveDuration, DecelerateKeySpline)
                    }
                };
                Storyboard.SetTargetProperty(opacityAnim, OpacityPath);
                storyboard.Children.Add(opacityAnim);
            }

            element.SetCurrentValue(UIElement.RenderTransformProperty, new ScaleTransform());
            element.SetCurrentValue(UIElement.RenderTransformOriginProperty, new Point(0.5, 0.5));

            return new NavigationAnimation(element, storyboard);
        }

        internal override NavigationAnimation GetExitAnimation(FrameworkElement element, bool movingBackwards)
        {
            var storyboard = new Storyboard();

            var opacityAnim = new DoubleAnimationUsingKeyFrames
            {
                KeyFrames =
                    {
                        new DiscreteDoubleKeyFrame(1, TimeSpan.Zero),
                        new SplineDoubleKeyFrame(0, ExitDuration, AccelerateKeySpline)
                    }
            };
            Storyboard.SetTargetProperty(opacityAnim, OpacityPath);
            storyboard.Children.Add(opacityAnim);

            return new NavigationAnimation(element, storyboard);
        }
    }
}
