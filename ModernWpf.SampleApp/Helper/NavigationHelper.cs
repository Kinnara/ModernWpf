using ModernWpf.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Navigation;
using Windows.Foundation.Metadata;

namespace ModernWpf.SampleApp.Helper
{
    /// <summary>
    /// RootFrameNavigationHelper registers for standard mouse and keyboard
    /// shortcuts used to go back and forward. There should be only one
    /// RootFrameNavigationHelper per view, and it should be associated with the
    /// root frame.
    /// </summary>
    /// <example>
    /// To make use of RootFrameNavigationHelper, create an instance of the
    /// RootNavigationHelper such as in the constructor of your root page.
    /// <code>
    ///     public MyRootPage()
    ///     {
    ///         this.InitializeComponent();
    ///         this.rootNavigationHelper = new RootNavigationHelper(MyFrame);
    ///     }
    /// </code>
    /// </example>
    [Windows.Foundation.Metadata.WebHostHidden]
    public class RootFrameNavigationHelper
    {
        private Frame Frame { get; set; }
        private NavigationView CurrentNavView { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="RootNavigationHelper"/> class.
        /// </summary>
        /// <param name="rootFrame">A reference to the top-level frame.
        /// This reference allows for frame manipulation and to register navigation handlers.</param>
        public RootFrameNavigationHelper(Frame rootFrame, NavigationView currentNavView)
        {
            this.Frame = rootFrame;
            this.Frame.Navigated += (s, e) =>
            {
                // Update the Back button whenever a navigation occurs.
                UpdateBackButton();
            };
            this.CurrentNavView = currentNavView;

            CurrentNavView.BackRequested += NavView_BackRequested;
        }

        private void NavView_BackRequested(NavigationView sender, NavigationViewBackRequestedEventArgs args)
        {
            TryGoBack();
        }

        private bool TryGoBack()
        {
            // don't go back if the nav pane is overlayed
            if (this.CurrentNavView.IsPaneOpen && (this.CurrentNavView.DisplayMode == NavigationViewDisplayMode.Compact || this.CurrentNavView.DisplayMode == NavigationViewDisplayMode.Minimal))
            {
                return false;
            }

            bool navigated = false;
            if (this.Frame.CanGoBack)
            {
                this.Frame.GoBack();
                navigated = true;
            }

            return navigated;
        }

        private bool TryGoForward()
        {
            bool navigated = false;
            if (this.Frame.CanGoForward)
            {
                this.Frame.GoForward();
                navigated = true;
            }
            return navigated;
        }

        private void SystemNavigationManager_BackRequested(object sender, BackRequestedEventArgs e)
        {
            if (!e.Handled)
            {
                e.Handled = TryGoBack();
            }
        }

        private void UpdateBackButton()
        {
            this.CurrentNavView.IsBackEnabled = this.Frame.CanGoBack ? true : false;
        }
    }

    /// <summary>
    /// Represents the method that will handle the <see cref="NavigationHelper.LoadState"/>event
    /// </summary>
    public delegate void LoadStateEventHandler(object sender, LoadStateEventArgs e);
    /// <summary>
    /// Represents the method that will handle the <see cref="NavigationHelper.SaveState"/>event
    /// </summary>
    public delegate void SaveStateEventHandler(object sender, SaveStateEventArgs e);

    /// <summary>
    /// Class used to hold the event data required when a page attempts to load state.
    /// </summary>
    public class LoadStateEventArgs : EventArgs
    {
        /// <summary>
        /// The parameter value passed to <see cref="Frame.Navigate(Type, object)"/>
        /// when this page was initially requested.
        /// </summary>
        public object NavigationParameter { get; private set; }
        /// <summary>
        /// A dictionary of state preserved by this page during an earlier
        /// session.  This will be null the first time a page is visited.
        /// </summary>
        public Dictionary<string, object> PageState { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="LoadStateEventArgs"/> class.
        /// </summary>
        /// <param name="navigationParameter">
        /// The parameter value passed to <see cref="Frame.Navigate(Type, object)"/>
        /// when this page was initially requested.
        /// </param>
        /// <param name="pageState">
        /// A dictionary of state preserved by this page during an earlier
        /// session.  This will be null the first time a page is visited.
        /// </param>
        public LoadStateEventArgs(object navigationParameter, Dictionary<string, object> pageState)
            : base()
        {
            this.NavigationParameter = navigationParameter;
            this.PageState = pageState;
        }
    }
    /// <summary>
    /// Class used to hold the event data required when a page attempts to save state.
    /// </summary>
    public class SaveStateEventArgs : EventArgs
    {
        /// <summary>
        /// An empty dictionary to be populated with serializable state.
        /// </summary>
        public Dictionary<string, object> PageState { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SaveStateEventArgs"/> class.
        /// </summary>
        /// <param name="pageState">An empty dictionary to be populated with serializable state.</param>
        public SaveStateEventArgs(Dictionary<string, object> pageState)
            : base()
        {
            this.PageState = pageState;
        }
    }
}
