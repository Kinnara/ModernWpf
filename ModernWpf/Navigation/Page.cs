using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Navigation;

namespace ModernWpf.Controls
{
    /// <summary>
    /// Represents content that a Frame control can navigate to.
    /// </summary>
    public class Page : PageFunctionBase
    {
        static Page()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Page), new FrameworkPropertyMetadata(typeof(Page)));
            BackgroundProperty.OverrideMetadata(typeof(Page), new FrameworkPropertyMetadata(Brushes.Transparent));
            FontSizeProperty.OverrideMetadata(typeof(Page), new FrameworkPropertyMetadata(14d));
        }

        /// <summary>
        /// Initializes a new instance of the Page class.
        /// </summary>
        public Page()
        {
        }

        #region Frame

        private static readonly DependencyPropertyKey FramePropertyKey =
            DependencyProperty.RegisterReadOnly(
                nameof(Frame),
                typeof(Frame),
                typeof(Page),
                null);

        /// <summary>
        /// Identifies the Frame dependency property.
        /// </summary>
        public static readonly DependencyProperty FrameProperty = FramePropertyKey.DependencyProperty;

        /// <summary>
        /// Gets the controlling Frame for the Page content.
        /// </summary>
        /// <returns>
        /// The controlling Frame for the Page content.
        /// </returns>
        public Frame Frame
        {
            get => (Frame)GetValue(FrameProperty);
            private set => SetValue(FramePropertyKey, value);
        }

        private void UpdateFrame(NavigationService navigationService)
        {
            if (navigationService != null && Frame.GetFrame(navigationService) is { } frame)
            {
                Frame = frame;
            }
            else
            {
                ClearValue(FramePropertyKey);
            }
        }

        #endregion

        /// <summary>
        /// Invoked when the Page is loaded and becomes the current source of a parent Frame.
        /// </summary>
        /// <param name="e">
        /// Event data that can be examined by overriding code. The event data is representative
        /// of the pending navigation that will load the current Page. Usually the most relevant
        /// property to examine is Parameter.
        /// </param>
        protected virtual void OnNavigatedTo(NavigationEventArgs e)
        {
        }

        /// <summary>
        /// Invoked immediately before the Page is unloaded and is no longer the current
        /// source of a parent Frame.
        /// </summary>
        /// <param name="e">
        /// Event data that can be examined by overriding code. The event data is representative
        /// of the navigation that will unload the current Page unless canceled. The navigation
        /// can potentially be canceled by setting Cancel.
        /// </param>
        protected virtual void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
        }

        /// <summary>
        /// Invoked immediately after the Page is unloaded and is no longer the current source
        /// of a parent Frame.
        /// </summary>
        /// <param name="e">
        /// Event data that can be examined by overriding code. The event data is representative
        /// of the navigation that has unloaded the current Page.
        /// </param>
        protected virtual void OnNavigatedFrom(NavigationEventArgs e)
        {
        }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);

            if (e.Property.PropertyType == NavigationServiceType &&
                e.Property.OwnerType == NavigationServiceType)
            {
                UpdateFrame((NavigationService)e.NewValue);
            }
        }

        internal void InternalOnNavigatedTo(NavigationEventArgs e) => OnNavigatedTo(e);
        internal void InternalOnNavigatingFrom(NavigatingCancelEventArgs e) => OnNavigatingFrom(e);
        internal void InternalOnNavigatedFrom(NavigationEventArgs e) => OnNavigatedFrom(e);

        private static readonly Type NavigationServiceType = typeof(NavigationService);
    }
;
}
