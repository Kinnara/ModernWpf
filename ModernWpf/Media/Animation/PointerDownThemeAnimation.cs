using System;
using System.Windows;
using System.Windows.Media.Animation;
using ModernWpf.Controls.Primitives;

namespace ModernWpf.Media.Animation
{
    /// <summary>
    /// Represents a preconfigured animation that runs when a pointer down is detected on an item or element.
    /// </summary>
    public sealed class PointerDownThemeAnimation : BooleanAnimationUsingKeyFrames
    {

        static PointerDownThemeAnimation()
        {
            Storyboard.TargetPropertyProperty.OverrideMetadata(typeof(PointerDownThemeAnimation), new FrameworkPropertyMetadata(new PropertyPath(PointerUpDownHelper.IsPressedProperty)));
        }

        /// <summary>
        /// Initializes a new instance of the PointerDownThemeAnimation class.
        /// </summary>
        public PointerDownThemeAnimation()
        {
            var one = new DiscreteBooleanKeyFrame(true, KeyTime.FromTimeSpan(TimeSpan.Zero));
            this.KeyFrames.Add(one);
        }

        /// <summary>
        /// Identifies the TargetName dependency property.
        /// </summary>
        public static readonly DependencyProperty TargetNameProperty = Storyboard.TargetNameProperty.AddOwner(typeof(PointerDownThemeAnimation));

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
            return new PointerDownThemeAnimation();
        }
    }
}
