using ModernWpf.Controls;
using System.Threading;

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
                Forward = new DrillTransition { Mode = DrillTransitionMode.DrillInIncoming },
                Backward = new DrillTransition { Mode = DrillTransitionMode.DrillOutIncoming }
            };
        });

        private static readonly ThreadLocal<NavigationOutTransition> Out = new ThreadLocal<NavigationOutTransition>(() =>
        {
            return new NavigationOutTransition
            {
                Forward = new DrillTransition { Mode = DrillTransitionMode.DrillInOutgoing },
                Backward = new DrillTransition { Mode = DrillTransitionMode.DrillOutOutgoing }
            };
        });
    }
}
