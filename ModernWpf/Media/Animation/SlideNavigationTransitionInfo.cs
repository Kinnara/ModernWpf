using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace ModernWpf.Media.Animation
{
    /// <summary>
    /// Defines constants that describe the type of animation to play during a slide
    /// transition.
    /// </summary>
    public enum SlideNavigationTransitionEffect
    {
        /// <summary>
        /// The exiting page fades out and the entering page enters from the bottom.
        /// </summary>
        FromBottom = 0,
        /// <summary>
        /// The exiting page leaves to the right of the panel and the entering page enters
        /// from the left.
        /// </summary>
        FromLeft = 1,
        /// <summary>
        /// The exiting page leaves to the left of the panel and the entering page enters
        /// from the right.
        /// </summary>
        FromRight = 2
    }

    /// <summary>
    /// Provides the parameters for a slide navigation transition.
    /// </summary>
    public sealed class SlideNavigationTransitionInfo : NavigationTransitionInfo, ISlideNavigationTransitionInfo2
    {
        /// <summary>
        /// Initializes a new instance of the **SlideNavigationTransitionInfo** class.
        /// </summary>
        public SlideNavigationTransitionInfo()
        {
        }

        #region Effect

        /// <summary>
        /// Identifies the Effect dependency property.
        /// </summary>
        public static readonly DependencyProperty EffectProperty =
            DependencyProperty.Register(
                nameof(Effect),
                typeof(SlideNavigationTransitionEffect),
                typeof(SlideNavigationTransitionInfo),
                null);

        /// <summary>
        /// Gets or sets the type of animation effect to play during the slide transition.
        /// </summary>
        /// <returns>
        /// The type of animation effect to play during the slide transition.
        /// </returns>
        public SlideNavigationTransitionEffect Effect
        {
            get => (SlideNavigationTransitionEffect)GetValue(EffectProperty);
            set => SetValue(EffectProperty, value);
        }

        #endregion

        internal override NavigationAnimation GetEnterAnimation(FrameworkElement element, bool movingBackwards)
        {
            var storyboard = new Storyboard();

            var effect = Effect;
            if (effect == SlideNavigationTransitionEffect.FromBottom)
            {
                if (movingBackwards)
                {
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
                    var yAnim = new DoubleAnimationUsingKeyFrames
                    {
                        KeyFrames =
                        {
                            new DiscreteDoubleKeyFrame(200, TimeSpan.Zero),
                            new SplineDoubleKeyFrame(0, EnterDuration, DecelerateKeySpline)
                        }
                    };
                    Storyboard.SetTargetProperty(yAnim, TranslateYPath);
                    storyboard.Children.Add(yAnim);

                    var opacityAnim = new DoubleAnimation(1, TimeSpan.Zero);
                    Storyboard.SetTargetProperty(opacityAnim, OpacityPath);
                    storyboard.Children.Add(opacityAnim);

                    element.SetCurrentValue(UIElement.RenderTransformProperty, new TranslateTransform());
                }
            }
            else
            {
                bool fromLeft;
                if (effect == SlideNavigationTransitionEffect.FromLeft)
                {
                    fromLeft = !movingBackwards;
                }
                else
                {
                    fromLeft = movingBackwards;
                }

                var xAnim = new DoubleAnimationUsingKeyFrames
                {
                    KeyFrames =
                    {
                        new DiscreteDoubleKeyFrame(fromLeft ? -200 : 200, TimeSpan.Zero),
                        new SplineDoubleKeyFrame(0, EnterDuration, DecelerateKeySpline)
                    }
                };
                Storyboard.SetTargetProperty(xAnim, TranslateXPath);
                storyboard.Children.Add(xAnim);

                var opacityAnim = new DoubleAnimation(1, TimeSpan.Zero);
                Storyboard.SetTargetProperty(opacityAnim, OpacityPath);
                storyboard.Children.Add(opacityAnim);

                element.SetCurrentValue(UIElement.RenderTransformProperty, new TranslateTransform());
            }

            return new NavigationAnimation(element, storyboard);
        }

        internal override NavigationAnimation GetExitAnimation(FrameworkElement element, bool movingBackwards)
        {
            var storyboard = new Storyboard();

            var effect = Effect;
            if (effect == SlideNavigationTransitionEffect.FromBottom)
            {
                if (movingBackwards)
                {
                    var yAnim = new DoubleAnimationUsingKeyFrames
                    {
                        KeyFrames =
                        {
                            new DiscreteDoubleKeyFrame(0, TimeSpan.Zero),
                            new SplineDoubleKeyFrame(200, ExitDuration, AccelerateKeySpline)
                        }
                    };
                    Storyboard.SetTargetProperty(yAnim, TranslateYPath);
                    storyboard.Children.Add(yAnim);

                    var opacityAnim = new DoubleAnimationUsingKeyFrames
                    {
                        KeyFrames =
                        {
                            new DiscreteDoubleKeyFrame(1, TimeSpan.Zero),
                            new DiscreteDoubleKeyFrame(0, ExitDuration)
                        }
                    };
                    Storyboard.SetTargetProperty(opacityAnim, OpacityPath);
                    storyboard.Children.Add(opacityAnim);

                    element.SetCurrentValue(UIElement.RenderTransformProperty, new TranslateTransform());
                }
                else
                {
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
                }
            }
            else
            {
                bool toLeft;
                if (effect == SlideNavigationTransitionEffect.FromLeft)
                {
                    toLeft = movingBackwards;
                }
                else
                {
                    toLeft = !movingBackwards;
                }

                var xAnim = new DoubleAnimationUsingKeyFrames
                {
                    KeyFrames =
                    {
                        new DiscreteDoubleKeyFrame(0, TimeSpan.Zero),
                        new SplineDoubleKeyFrame(toLeft ? -200 : 200, ExitDuration, AccelerateKeySpline)
                    }
                };
                Storyboard.SetTargetProperty(xAnim, TranslateXPath);
                storyboard.Children.Add(xAnim);

                var opacityAnim = new DoubleAnimationUsingKeyFrames
                {
                    KeyFrames =
                    {
                        new DiscreteDoubleKeyFrame(1, TimeSpan.Zero),
                        new DiscreteDoubleKeyFrame(0, ExitDuration)
                    }
                };
                Storyboard.SetTargetProperty(opacityAnim, OpacityPath);
                storyboard.Children.Add(opacityAnim);

                element.SetCurrentValue(UIElement.RenderTransformProperty, new TranslateTransform());
            }

            return new NavigationAnimation(element, storyboard);
        }
    }

    internal interface ISlideNavigationTransitionInfo2
    {
        SlideNavigationTransitionEffect Effect { get; set; }
    }
}
