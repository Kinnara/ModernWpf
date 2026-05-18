using System.Windows;

namespace ModernWpf.Controls
{
    partial class NavigationView
    {
        public event TypedEventHandler<NavigationView, NavigationViewSelectionChangedEventArgs> SelectionChanged;
        public event TypedEventHandler<NavigationView, NavigationViewItemInvokedEventArgs> ItemInvoked;
        public event TypedEventHandler<NavigationView, NavigationViewDisplayModeChangedEventArgs> DisplayModeChanged;
        public event TypedEventHandler<NavigationView, NavigationViewBackRequestedEventArgs> BackRequested;
        public event TypedEventHandler<NavigationView, object> PaneClosed;
        public event TypedEventHandler<NavigationView, NavigationViewPaneClosingEventArgs> PaneClosing;
        public event TypedEventHandler<NavigationView, object> PaneOpened;
        public event TypedEventHandler<NavigationView, object> PaneOpening;
        public event TypedEventHandler<NavigationView, NavigationViewItemExpandingEventArgs> Expanding;
        public event TypedEventHandler<NavigationView, NavigationViewItemCollapsedEventArgs> Collapsed;

        private void OnMenuItemContainerStyleSelectorPropertyChanged(DependencyPropertyChangedEventArgs args)
        {
        }

        private static object CoerceToGreaterThanZero(DependencyObject d, object baseValue)
        {
            if (baseValue is double value)
            {
                ((NavigationView)d).CoerceToGreaterThanZero(ref value);
                return value;
            }
            return baseValue;
        }
    }
}
