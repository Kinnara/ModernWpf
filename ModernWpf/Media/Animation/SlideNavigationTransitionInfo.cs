using ModernWpf.Controls;
using System.Threading;
using System.Windows;

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

        internal override NavigationInTransition GetNavigationInTransition()
        {
            return Effect switch
            {
                SlideNavigationTransitionEffect.FromBottom => FromBottomIn.Value,
                SlideNavigationTransitionEffect.FromLeft => FromLeftIn.Value,
                SlideNavigationTransitionEffect.FromRight => FromRightIn.Value,
                _ => null,
            };
        }

        internal override NavigationOutTransition GetNavigationOutTransition()
        {
            return Effect switch
            {
                SlideNavigationTransitionEffect.FromBottom => FromBottomOut.Value,
                SlideNavigationTransitionEffect.FromLeft => FromLeftOut.Value,
                SlideNavigationTransitionEffect.FromRight => FromRightOut.Value,
                _ => null,
            };
        }

        private static readonly ThreadLocal<NavigationInTransition> FromBottomIn = new ThreadLocal<NavigationInTransition>(() =>
        {
            return new NavigationInTransition
            {
                Forward = new SlideTransition { Mode = SlideTransitionMode.SlideUp },
                Backward = new FadeTransition { Mode = FadeTransitionMode.FadeIn }
            };
        });

        private static readonly ThreadLocal<NavigationOutTransition> FromBottomOut = new ThreadLocal<NavigationOutTransition>(() =>
        {
            return new NavigationOutTransition
            {
                Forward = new FadeTransition { Mode = FadeTransitionMode.FadeOut },
                Backward = new SlideTransition { Mode = SlideTransitionMode.SlideDown }
            };
        });

        private static readonly ThreadLocal<NavigationInTransition> FromLeftIn = new ThreadLocal<NavigationInTransition>(() =>
        {
            return new NavigationInTransition
            {
                Forward = new SlideTransition { Mode = SlideTransitionMode.SlideRightIn },
                Backward = new SlideTransition { Mode = SlideTransitionMode.SlideLeftIn }
            };
        });

        private static readonly ThreadLocal<NavigationOutTransition> FromLeftOut = new ThreadLocal<NavigationOutTransition>(() =>
        {
            return new NavigationOutTransition
            {
                Forward = new SlideTransition { Mode = SlideTransitionMode.SlideRightOut },
                Backward = new SlideTransition { Mode = SlideTransitionMode.SlideLeftOut }
            };
        });

        private static readonly ThreadLocal<NavigationInTransition> FromRightIn = new ThreadLocal<NavigationInTransition>(() =>
        {
            return new NavigationInTransition
            {
                Forward = new SlideTransition { Mode = SlideTransitionMode.SlideLeftIn },
                Backward = new SlideTransition { Mode = SlideTransitionMode.SlideRightIn }
            };
        });

        private static readonly ThreadLocal<NavigationOutTransition> FromRightOut = new ThreadLocal<NavigationOutTransition>(() =>
        {
            return new NavigationOutTransition
            {
                Forward = new SlideTransition { Mode = SlideTransitionMode.SlideLeftOut },
                Backward = new SlideTransition { Mode = SlideTransitionMode.SlideRightOut }
            };
        });
    }

    internal interface ISlideNavigationTransitionInfo2
    {
        SlideNavigationTransitionEffect Effect { get; set; }
    }
}
