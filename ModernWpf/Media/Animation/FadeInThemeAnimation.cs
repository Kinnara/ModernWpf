using System;
using System.Windows;
using System.Windows.Media.Animation;

namespace ModernWpf.Media.Animation
{
    /// <summary>
    /// Represents the preconfigured opacity animation that applies to controls when
    /// they are first shown.
    /// </summary>
    public sealed class FadeInThemeAnimation : AnimationTimeline
    {
        private static readonly DoubleAnimation _da = new DoubleAnimation
        {
            From = 0,
            To = 1,
            Duration = TimeSpan.FromMilliseconds(250)
        };

        static FadeInThemeAnimation()
        {
            Storyboard.TargetPropertyProperty.OverrideMetadata(typeof(FadeInThemeAnimation),
                new FrameworkPropertyMetadata(new PropertyPath(UIElement.OpacityProperty)));

            DurationProperty.OverrideMetadata(typeof(FadeInThemeAnimation),
                new FrameworkPropertyMetadata(new Duration(TimeSpan.FromMilliseconds(250))));
        }

        /// <summary>
        /// Initializes a new instance of the FadeInThemeAnimation class.
        /// </summary>
        public FadeInThemeAnimation()
        {
        }

        public override Type TargetPropertyType
        {
            get
            {
                ReadPreamble();

                return typeof(Double);
            }
        }

        public override object GetCurrentValue(object defaultOriginValue, object defaultDestinationValue, AnimationClock animationClock)
        {
            return _da.GetCurrentValue(defaultOriginValue, defaultDestinationValue, animationClock);
        }

        protected override Freezable CreateInstanceCore()
        {
            return new FadeInThemeAnimation();
        }
    }
}
