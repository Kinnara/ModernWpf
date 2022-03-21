using ModernWpf.Controls;
using ModernWpf.SampleApp.DataModel;
using ModernWpf.SampleApp.Properties;
using SamplesCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ModernWpf.SampleApp
{
    /// <summary>
    /// ItemPage.xaml 的交互逻辑
    /// </summary>
    public partial class ItemPage : Page
    {
        private ControlInfoDataItem _item;
        private ElementTheme? _currentElementTheme;

        public ControlInfoDataItem Item
        {
            get { return _item; }
            set { _item = value; }
        }

        public ItemPage()
        {
            InitializeComponent();

            Loaded += (s, e) => SetInitialVisuals();
            this.Unloaded += this.ItemPage_Unloaded;
        }

        private void ItemPage_Unloaded(object sender, RoutedEventArgs e)
        {
            // Notifying the pageheader that this Itempage was unloaded
            NavigationRootPage.Current.PageHeader.Event_ItemPage_Unloaded(sender, e);
        }

        public void SetInitialVisuals()
        {
            NavigationRootPage.Current.NavigationViewLoaded = OnNavigationViewLoaded;
            NavigationRootPage.Current.PageHeader.TopCommandBar.Visibility = Visibility.Visible;
            NavigationRootPage.Current.PageHeader.CopyLinkAction = OnCopyLink;
            NavigationRootPage.Current.PageHeader.ToggleThemeAction = OnToggleTheme;
            NavigationRootPage.Current.PageHeader.ResetCopyLinkButton();

            if (NavigationRootPage.Current.IsFocusSupported)
            {
                this.Focus();
            }

            //if (UIHelper.IsScreenshotMode)
            //{
            //    var controlExamples = (this.contentFrame.Content as UIElement)?.FindDescendants<ControlExample>();

            //    if (controlExamples != null)
            //    {
            //        foreach (var controlExample in controlExamples)
            //        {
            //            VisualStateManager.GoToState(controlExample, "ScreenshotMode", false);
            //        }
            //    }
            //}
        }

        private void OnNavigationViewLoaded()
        {
            NavigationRootPage.Current.EnsureNavigationSelection(this.Item.UniqueId);
        }

        private void OnCopyLink()
        {
            //ProtocolActivationClipboardHelper.Copy(this.Item);
        }

        private void OnToggleTheme()
        {
            //var currentElementTheme = ((_currentElementTheme ?? ElementTheme.Default) == ElementTheme.Default) ? ThemeHelper.ActualTheme : _currentElementTheme.Value;
            //var newTheme = currentElementTheme == ElementTheme.Dark ? ElementTheme.Light : ElementTheme.Dark;
            //SetControlExamplesTheme(newTheme);
        }

        private void SetControlExamplesTheme(ElementTheme theme)
        {
            var controlExamples = (this.contentFrame.Content as UIElement)?.FindDescendants<ControlExample>();

            if (controlExamples != null)
            {
                _currentElementTheme = theme;
                foreach (var controlExample in controlExamples)
                {
                    var exampleContent = controlExample.Example as FrameworkElement;
                    //exampleContent.RequestedTheme = theme;
                    //controlExample.ExampleContainer.RequestedTheme = theme;
                }
            }
        }

        private void OnRelatedControlClick(object sender, RoutedEventArgs e)
        {
            ButtonBase b = (ButtonBase)sender;

            this.Frame.Navigate(typeof(ItemPage), b.DataContext.ToString());
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            var item = await ControlInfoDataSource.Instance.GetItemAsync((string)e.ExtraData);

            if (item != null)
            {
                Item = item;

                // Load control page into frame.
                var loader = new ResourceManager(typeof(Resources));

                string pageRoot = loader.GetString("PageStringRoot");

                string pageString = pageRoot + item.UniqueId + "Page";
                Type pageType = Type.GetType(pageString);

                if (pageType != null)
                {
                    // Pagetype is not null!
                    // So lets generate the github links and set them!
                    var gitHubBaseURI = "https://github.com/microsoft/Xaml-Controls-Gallery/tree/master/XamlControlsGallery/ControlPages/";
                    var pageName = pageType.Name + ".xaml";
                    PageCodeGitHubLink.NavigateUri = new Uri(gitHubBaseURI + pageName + ".cs");
                    PageMarkupGitHubLink.NavigateUri = new Uri(gitHubBaseURI + pageName);

                    this.contentFrame.Navigate(pageType);
                }

                NavigationRootPage.Current.NavigationView.Header = item?.Title;
            }
            DataContext = Item;

            base.OnNavigatedTo(e);
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            //SetControlExamplesTheme(ThemeHelper.ActualTheme);

            base.OnNavigatingFrom(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            NavigationRootPage.Current.NavigationViewLoaded = null;
            if (NavigationRootPage.Current.PageHeader != null)
            {
                NavigationRootPage.Current.PageHeader.TopCommandBar.Visibility = Visibility.Collapsed;
                NavigationRootPage.Current.PageHeader.CopyLinkAction = null;
                NavigationRootPage.Current.PageHeader.ToggleThemeAction = null;
            }

            // We use reflection to call the OnNavigatedFrom function the user leaves this page
            // See this PR for more information: https://github.com/microsoft/Xaml-Controls-Gallery/pull/145
            Frame contentFrameAsFrame = contentFrame as Frame;
            Page innerPage = contentFrameAsFrame.Content as Page;
            try
            {
                MethodInfo dynMethod = innerPage.GetType().GetMethod("OnNavigatedFrom",
                    BindingFlags.NonPublic | BindingFlags.Instance);
                dynMethod.Invoke(innerPage, new object[] { e });
            }
            catch { }
            base.OnNavigatedFrom(e);
        }

        private void OnContentRootSizeChanged(object sender, SizeChangedEventArgs e)
        {
            string targetState = "NormalFrameContent";

            if ((contentColumn.ActualWidth) >= 1000)
            {
                targetState = "WideFrameContent";
            }

            VisualStateManager.GoToState(this, targetState, false);
        }
    }
}
