using System;
using System.Windows;
using System.Windows.Media.Animation;

namespace ModernWpf.Media.Animation
{
    /// <summary>
    /// Represents the preconfigured opacity animation that applies to controls when
    /// they are first shown.
    /// </summary>
    public sealed class FadeInThemeAnimation : DoubleAnimationBase
    {
        private readonly DoubleAnimation _da = new DoubleAnimation(1, TimeSpan.FromMilliseconds(167));

        static FadeInThemeAnimation()
        {
            Storyboard.TargetPropertyProperty.OverrideMetadata(typeof(FadeInThemeAnimation), new FrameworkPropertyMetadata(new PropertyPath(UIElement.OpacityProperty)));
        }

        /// <summary>
        /// Initializes a new instance of the FadeInThemeAnimation class.
        /// </summary>
        public FadeInThemeAnimation()
        {
        }

        /// <summary>
        /// Identifies the TargetName dependency property.
        /// </summary>
        public static readonly DependencyProperty TargetNameProperty = Storyboard.TargetNameProperty.AddOwner(typeof(FadeInThemeAnimation));

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
            return new FadeInThemeAnimation();
        }

        protected override double GetCurrentValueCore(double defaultOriginValue, double defaultDestinationValue, AnimationClock animationClock)
        {
            return _da.GetCurrentValue(defaultOriginValue, defaultDestinationValue, animationClock);
        }
    }
}
