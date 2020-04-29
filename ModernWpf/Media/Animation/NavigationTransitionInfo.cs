using ModernWpf.Controls;
using System.Windows;

namespace ModernWpf.Media.Animation
{
    /// <summary>
    /// Provides parameter info for the Frame.Navigate method. Controls how the transition
    /// animation runs during the navigation action.
    /// </summary>
    public class NavigationTransitionInfo : DependencyObject
    {
        /// <summary>
        /// Initializes a new instance of the NavigationTransitionInfo class.
        /// </summary>
        protected NavigationTransitionInfo()
        {
        }

        internal virtual NavigationInTransition GetNavigationInTransition()
        {
            return null;
        }

        internal virtual NavigationOutTransition GetNavigationOutTransition()
        {
            return null;
        }

        //protected virtual string GetNavigationStateCore();
        //protected virtual void SetNavigationStateCore(string navigationState);
    }
}
