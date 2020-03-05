using System;
using System.Windows;
using System.Windows.Media.Animation;
using ModernWpf.Controls.Primitives;

namespace ModernWpf.Media.Animation
{
    /// <summary>
    /// Represents a preconfigured animation that runs after a pointer down is detected on an item or element and the tap action is released.
    /// </summary>
    public sealed class PointerUpThemeAnimation : BooleanAnimationUsingKeyFrames
    {

        static PointerUpThemeAnimation()
        {
            Storyboard.TargetPropertyProperty.OverrideMetadata(typeof(PointerUpThemeAnimation), new FrameworkPropertyMetadata(new PropertyPath(PointerUpDownHelper.IsPressedProperty)));
        }

        /// <summary>
        /// Initializes a new instance of the PointerUpThemeAnimation class.
        /// </summary>
        public PointerUpThemeAnimation()
        {
            var one = new DiscreteBooleanKeyFrame(false, KeyTime.FromTimeSpan(TimeSpan.Zero));
            this.KeyFrames.Add(one);
        }

        /// <summary>
        /// Identifies the TargetName dependency property.
        /// </summary>
        public static readonly DependencyProperty TargetNameProperty = Storyboard.TargetNameProperty.AddOwner(typeof(PointerUpThemeAnimation));

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
            return new PointerUpThemeAnimation();
        }
    }
}
