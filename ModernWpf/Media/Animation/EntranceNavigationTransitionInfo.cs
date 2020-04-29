using ModernWpf.Controls;
using System.Threading;

namespace ModernWpf.Media.Animation
{
    /// <summary>
    /// Specifies the animation to run when content appears on a Page.
    /// </summary>
    public sealed class EntranceNavigationTransitionInfo : NavigationTransitionInfo
    {
        /// <summary>
        /// Initializes a new instance of the EntranceNavigationTransitionInfo class.
        /// </summary>
        public EntranceNavigationTransitionInfo()
        {
        }

        internal override NavigationInTransition GetNavigationInTransition()
        {
            return In.Value;
        }

        internal override NavigationOutTransition GetNavigationOutTransition()
        {
            return Out.Value;
        }

        private static readonly ThreadLocal<NavigationInTransition> In = new ThreadLocal<NavigationInTransition>(() =>
        {
            return new NavigationInTransition
            {
                Forward = new SlideTransition { Mode = SlideTransitionMode.SlideUp },
                Backward = new FadeTransition { Mode = FadeTransitionMode.FadeIn }
            };
        });

        private static readonly ThreadLocal<NavigationOutTransition> Out = new ThreadLocal<NavigationOutTransition>(() =>
        {
            return new NavigationOutTransition
            {
                Forward = new FadeTransition { Mode = FadeTransitionMode.FadeOut },
                Backward = new SlideTransition { Mode = SlideTransitionMode.SlideDown }
            };
        });
    }
}
