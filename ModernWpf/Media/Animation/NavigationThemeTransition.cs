using System.Windows;
using System.Windows.Markup;

namespace ModernWpf.Media.Animation
{
    /// <summary>
    /// Provides page navigation animations.
    /// </summary>
    [ContentProperty(nameof(DefaultNavigationTransitionInfo))]
    public sealed class NavigationThemeTransition : Transition
    {
        /// <summary>
        /// Initializes a new instance of the NavigationThemeTransition class.
        /// </summary>
        public NavigationThemeTransition()
        {
        }

        #region DefaultNavigationTransitionInfo

        /// <summary>
        /// Identifies the DefaultNavigationTransitionInfo dependency property.
        /// </summary>
        public static readonly DependencyProperty DefaultNavigationTransitionInfoProperty =
            DependencyProperty.Register(
                nameof(DefaultNavigationTransitionInfo),
                typeof(NavigationTransitionInfo),
                typeof(NavigationThemeTransition),
                null);

        /// <summary>
        /// Gets or sets the default transition used when navigating between pages.
        /// </summary>
        /// <returns>
        /// The default transition used when navigating between pages.
        /// </returns>
        public NavigationTransitionInfo DefaultNavigationTransitionInfo
        {
            get => (NavigationTransitionInfo)GetValue(DefaultNavigationTransitionInfoProperty);
            set => SetValue(DefaultNavigationTransitionInfoProperty, value);
        }

        #endregion
    }
}
