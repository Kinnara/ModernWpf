using System;
using System.Windows;
using System.Windows.Media.Animation;

namespace ModernWpf.Media.Animation
{
    /// <summary>
    /// Represents the preconfigured opacity animation that applies to controls when
    /// they are removed from the UI or hidden.
    /// </summary>
    public sealed class FadeOutThemeAnimation : DoubleAnimationBase
    {
        private readonly DoubleAnimation _da = new DoubleAnimation(0, TimeSpan.FromMilliseconds(167));

        static FadeOutThemeAnimation()
        {
            Storyboard.TargetPropertyProperty.OverrideMetadata(typeof(FadeOutThemeAnimation), new FrameworkPropertyMetadata(new PropertyPath(UIElement.OpacityProperty)));
        }

        /// <summary>
        /// Initializes a new instance of the FadeOutThemeAnimation class.
        /// </summary>
        public FadeOutThemeAnimation()
        {
        }

        /// <summary>
        /// Identifies the TargetName dependency property.
        /// </summary>
        public static readonly DependencyProperty TargetNameProperty = Storyboard.TargetNameProperty.AddOwner(typeof(FadeOutThemeAnimation));

        /// <summary>
        /// Gets or sets the reference name of the control element being targeted.
        /// </summary>
        /// <returns>
        /// The reference name. This is typically the **x:Name** of the relevant element
        /// as declared in XAML.
        /// </returns>
        public string TargetName
        {
            get => (string)GetValue(TargetNameProperty);
            set => SetValue(TargetNameProperty, value);
        }

        protected override Freezable CreateInstanceCore()
        {
            return new FadeOutThemeAnimation();
        }

        protected override double GetCurrentValueCore(double defaultOriginValue, double defaultDestinationValue, AnimationClock animationClock)
        {
            return _da.GetCurrentValue(defaultOriginValue, defaultDestinationValue, animationClock);
        }
    }
}
