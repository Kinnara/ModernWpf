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
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Windows.System.Profile;
using Frame = ModernWpf.Controls.Frame;
using Page = ModernWpf.Controls.Page;

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
            SetControlExamplesTheme();
        }

        private void SetControlExamplesTheme()
        {
            var controlExamples = (this.contentFrame.Content as UIElement)?.FindDescendants<ControlExample>();

            if (controlExamples != null)
            {
                foreach (var controlExample in controlExamples)
                {
                    var exampleContent = controlExample.Example as FrameworkElement;
                    exampleContent.ToggleTheme();
                    controlExample.ExampleContainer.ToggleTheme();
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
            if (innerPage != null)
            {
                MethodInfo dynMethod = innerPage?.GetType().GetMethod("OnNavigatedFrom",
                    BindingFlags.NonPublic | BindingFlags.Instance);
                dynMethod.Invoke(innerPage, new object[] { e });
            }
            base.OnNavigatedFrom(e);
        }

        private void OnContentRootSizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (contentColumn.ActualWidth >= 1000)
            {
                contentFrame.Width = 1028;
                contentFrame.HorizontalAlignment = HorizontalAlignment.Left;
            }
            else
            {
                contentFrame.Width = double.NaN;
                contentFrame.HorizontalAlignment = HorizontalAlignment.Stretch;
            }

            if (Application.Current.MainWindow.ActualWidth >= 1372)
            {
                seeAlsoPanel.Width = double.NaN;
                Grid.SetRow(seeAlsoPanel, 0);
                Grid.SetColumn(seeAlsoPanel, 2);
                Grid.SetColumnSpan(seeAlsoPanel, 1);

                Grid.SetColumnSpan(SourcePanel, 3);

                Grid.SetColumnSpan(DocumentationPanel, 3);

                Grid.SetRow(RelatedControlsPanel, 2);
                Grid.SetColumn(RelatedControlsPanel, 0);
                Grid.SetColumnSpan(RelatedControlsPanel, 1);

                Grid.SetRow(FeedbackPanel, 3);
                Grid.SetColumn(FeedbackPanel, 0);
                Grid.SetColumnSpan(FeedbackPanel, 3);

                rightMargin.Width = new GridLength(20);
                contentRoot.Padding = new Thickness(56, 0, 12, 36);
            }
            else
            {
                seeAlsoPanel.Width = double.NaN;
                Grid.SetRow(seeAlsoPanel, 3);
                Grid.SetColumn(seeAlsoPanel, 0);
                Grid.SetColumnSpan(seeAlsoPanel, 3);

                Grid.SetColumnSpan(SourcePanel, 1);

                Grid.SetColumnSpan(DocumentationPanel, 1);

                Grid.SetRow(RelatedControlsPanel, 0);
                Grid.SetColumn(RelatedControlsPanel, 2);
                Grid.SetColumnSpan(RelatedControlsPanel, 1);

                Grid.SetRow(FeedbackPanel, 1);
                Grid.SetColumn(FeedbackPanel, 2);
                Grid.SetColumnSpan(FeedbackPanel, 1);

                if (Application.Current.MainWindow.ActualWidth < (double)App.Current.Resources["Breakpoint640Plus"])
                {
                    rightMargin.Width = new GridLength(0);
                    contentRoot.Padding = new Thickness(14, 0, 14, 0);
                }
                else
                {
                    rightMargin.Width = new GridLength(20);
                    contentRoot.Padding = new Thickness(56, 0, 12, 36);
                }
            }
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (AnalyticsInfo.VersionInfo.DeviceFamily == "Windows.Xbox")
            {
                svPanel.Margin = new Thickness(0, 0, 48, 27);
            }
        }
    }
}
