using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using ModernWpf.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ModernWpf.SampleApp.ControlPages;
using Windows.Foundation.Metadata;
using Windows.System;
using ModernWpf.SampleApp.DataModel;
using Windows.Gaming.Input;
using Windows.System.Profile;
using System.Windows.Automation;
using System.Diagnostics;
using Windows.Devices.Input;
using ModernWpf.SampleApp.Helper;
using System.Windows.Controls;
using Page = ModernWpf.Controls.Page;
using Frame = ModernWpf.Controls.Frame;
using ModernWpf.SampleApp.Common;

namespace ModernWpf.SampleApp
{
    /// <summary>
    /// NavigationRootPage.xaml 的交互逻辑
    /// </summary>
    public partial class NavigationRootPage : Page
    {
        public static NavigationRootPage Current;
        public static Frame RootFrame = null;

        public VirtualKey ArrowKey;

        private RootFrameNavigationHelper _navHelper;
        private bool _isGamePadConnected;
        private bool _isKeyboardConnected;
        private NavigationViewItem _allControlsMenuItem;
        private NavigationViewItem _newControlsMenuItem;

        public static NavigationRootPage GetForElement(object obj)
        {
            UIElement element = (UIElement)obj;
            Window window = WindowHelper.GetWindowForElement(element);
            if (window != null)
            {
                return (NavigationRootPage)window.Content;
            }
            return null;
        }

        public NavigationView NavigationView
        {
            get { return NavigationViewControl; }
        }

        public Action NavigationViewLoaded { get; set; }

        public DeviceType DeviceFamily { get; set; }

        public bool IsFocusSupported
        {
            get
            {
                return DeviceFamily == DeviceType.Xbox || _isGamePadConnected || _isKeyboardConnected;
            }
        }

        public PageHeader PageHeader
        {
            get
            {
                return VisualTree.FindDescendants<PageHeader>(NavigationViewControl).FirstOrDefault();
            }
        }

        public NavigationRootPage()
        {
            InitializeComponent();
            // Workaround for VisualState issue that should be fixed
            // by https://github.com/microsoft/microsoft-ui-xaml/pull/2271
            //NavigationViewControl.PaneDisplayMode = NavigationViewPaneDisplayMode.Left;

            _navHelper = new RootFrameNavigationHelper(rootFrame, NavigationViewControl);

            SetDeviceFamily();
            AddNavigationMenuItems();
            Current = this;
            RootFrame = rootFrame;

            Gamepad.GamepadAdded += OnGamepadAdded;
            Gamepad.GamepadRemoved += OnGamepadRemoved;

            _isKeyboardConnected = Convert.ToBoolean(new KeyboardCapabilities().KeyboardPresent);

            // remove the solid-colored backgrounds behind the caption controls and system back button if we are in left mode
            // This is done when the app is loaded since before that the actual theme that is used is not "determined" yet
            Loaded += delegate (object sender, RoutedEventArgs e)
            {
                //NavigationOrientationHelper.UpdateTitleBar(NavigationOrientationHelper.IsLeftMode);
            };
        }

        public static string GetAppTitleFromSystem
        {
            get
            {
                if (PackagedAppHelper.IsPackagedApp)
                {
                    return Windows.ApplicationModel.Package.Current.DisplayName;
                }
                else
                {
                    return System.Reflection.Assembly.GetExecutingAssembly().GetName().Name.ToString();
                }
            }
        }

        public bool CheckNewControlSelected()
        {
            return _newControlsMenuItem.IsSelected;
        }

        public void EnsureNavigationSelection(string id)
        {
            foreach (object rawGroup in this.NavigationView.MenuItems)
            {
                if (rawGroup is NavigationViewItem group)
                {
                    foreach (object rawItem in group.MenuItems)
                    {
                        if (rawItem is NavigationViewItem item)
                        {
                            if ((string)item.Tag == id)
                            {
                                group.IsExpanded = true;
                                NavigationView.SelectedItem = item;
                                item.IsSelected = true;
                                return;
                            }
                        }
                    }
                }
            }
        }

        private void AddNavigationMenuItems()
        {
            foreach (var group in ControlInfoDataSource.Instance.Groups.OrderBy(i => i.Title))
            {
                var itemGroup = new NavigationViewItem() { Content = group.Title, Tag = group.UniqueId, DataContext = group, Icon = GetIcon(group.ImageIconPath) };

                var groupMenuFlyoutItem = new MenuItem() { Header = $"Copy Link to {group.Title} Samples", Icon = new FontIcon() { Glyph = "\uE8C8" }, Tag = group };
                groupMenuFlyoutItem.Click += this.OnMenuFlyoutItemClick;
                itemGroup.ContextMenu = new ContextMenu() { Items = { groupMenuFlyoutItem } };

                AutomationProperties.SetName(itemGroup, group.Title);

                foreach (var item in group.Items)
                {
                    var itemInGroup = new NavigationViewItem() { IsEnabled = item.IncludedInBuild, Content = item.Title, Tag = item.UniqueId, DataContext = item, Icon = GetIcon(item.ImageIconPath) };

                    var itemInGroupMenuFlyoutItem = new MenuItem() { Header = $"Copy Link to {item.Title} Sample", Icon = new FontIcon() { Glyph = "\uE8C8" }, Tag = item };
                    itemInGroupMenuFlyoutItem.Click += this.OnMenuFlyoutItemClick;
                    itemInGroup.ContextMenu = new ContextMenu() { Items = { itemInGroupMenuFlyoutItem } };

                    itemGroup.MenuItems.Add(itemInGroup);
                    AutomationProperties.SetName(itemInGroup, item.Title);
                }

                NavigationViewControl.MenuItems.Add(itemGroup);

                if (group.UniqueId == "AllControls")
                {
                    this._allControlsMenuItem = itemGroup;
                }
                else if (group.UniqueId == "NewControls")
                {
                    this._newControlsMenuItem = itemGroup;
                }
            }

            // Move "What's New" and "All Controls" to the top of the NavigationView
            NavigationViewControl.MenuItems.Remove(_allControlsMenuItem);
            NavigationViewControl.MenuItems.Remove(_newControlsMenuItem);
            NavigationViewControl.MenuItems.Insert(0, _allControlsMenuItem);
            NavigationViewControl.MenuItems.Insert(0, _newControlsMenuItem);

            // Separate the All/New items from the rest of the categories.
            NavigationViewControl.MenuItems.Insert(2, new NavigationViewItemSeparator());

            _newControlsMenuItem.Loaded += OnNewControlsMenuItemLoaded;
            NavigationViewControl.SelectedItem = _newControlsMenuItem;
        }

        private void OnMenuFlyoutItemClick(object sender, RoutedEventArgs e)
        {
            switch ((sender as MenuItem).Tag)
            {
                case ControlInfoDataItem item:
                    //ProtocolActivationClipboardHelper.Copy(item);
                    return;
                case ControlInfoDataGroup group:
                    //ProtocolActivationClipboardHelper.Copy(group);
                    return;
            }
        }

        private static IconElement GetIcon(string imagePath)
        {
            return imagePath.ToLowerInvariant().EndsWith(".png") ?
                        (IconElement)new BitmapIcon() { UriSource = new Uri(imagePath, UriKind.RelativeOrAbsolute), ShowAsMonochrome = false } :
                        (IconElement)new FontIcon()
                        {
                            // FontFamily = new FontFamily("Segoe MDL2 Assets"),
                            Glyph = imagePath
                        };
        }

        private void SetDeviceFamily()
        {
            var familyName = AnalyticsInfo.VersionInfo.DeviceFamily;

            if (!Enum.TryParse(familyName.Replace("Windows.", string.Empty), out DeviceType parsedDeviceType))
            {
                parsedDeviceType = DeviceType.Other;
            }

            DeviceFamily = parsedDeviceType;
        }

        private void OnNewControlsMenuItemLoaded(object sender, RoutedEventArgs e)
        {
            if (IsFocusSupported && NavigationViewControl.DisplayMode == NavigationViewDisplayMode.Expanded)
            {
                //controlsSearchBox.Focus(FocusState.Keyboard);
            }
        }

        private void OnGamepadRemoved(object sender, Gamepad e)
        {
            _isGamePadConnected = Gamepad.Gamepads.Any();
        }

        private void OnGamepadAdded(object sender, Gamepad e)
        {
            _isGamePadConnected = Gamepad.Gamepads.Any();
        }

        private void OnNavigationViewControlLoaded(object sender, RoutedEventArgs e)
        {
            // Delay necessary to ensure NavigationView visual state can match navigation
            Task.Delay(500).ContinueWith(_ => this.NavigationViewLoaded?.Invoke(), TaskScheduler.FromCurrentSynchronizationContext());
        }

        private void OnNavigationViewSelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            if (args.IsSettingsSelected)
            {
                if (rootFrame.CurrentSourcePageType != typeof(SettingsPage))
                {
                    rootFrame.Navigate(typeof(SettingsPage));
                }
            }
            else
            {
                var selectedItem = args.SelectedItemContainer;

                if (selectedItem == _allControlsMenuItem)
                {
                    if (rootFrame.CurrentSourcePageType != typeof(AllControlsPage))
                    {
                        rootFrame.Navigate(typeof(AllControlsPage));
                    }
                }
                else if (selectedItem == _newControlsMenuItem)
                {
                    if (rootFrame.CurrentSourcePageType != typeof(NewControlsPage))
                    {
                        rootFrame.Navigate(typeof(NewControlsPage));
                    }
                }
                else
                {
                    if (selectedItem.DataContext is ControlInfoDataGroup)
                    {
                        var itemId = ((ControlInfoDataGroup)selectedItem.DataContext).UniqueId;
                        rootFrame.Navigate(typeof(SectionPage), itemId);
                    }
                    else if (selectedItem.DataContext is ControlInfoDataItem)
                    {
                        var item = (ControlInfoDataItem)selectedItem.DataContext;
                        rootFrame.Navigate(typeof(ItemPage), item.UniqueId);
                    }
                }
            }
        }

        private void OnRootFrameNavigated(object sender, NavigationEventArgs e)
        {
            if (rootFrame.SourcePageType == typeof(AllControlsPage) ||
                rootFrame.SourcePageType == typeof(NewControlsPage))
            {
                NavigationViewControl.AlwaysShowHeader = false;
            }
            else
            {
                NavigationViewControl.AlwaysShowHeader = true;
            }
        }

        private void OnControlsSearchBoxTextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            {
                var suggestions = new List<ControlInfoDataItem>();

                var querySplit = sender.Text.Split(' ');
                foreach (var group in ControlInfoDataSource.Instance.Groups)
                {
                    var matchingItems = group.Items.Where(
                        item =>
                        {
                            // Idea: check for every word entered (separated by space) if it is in the name, 
                            // e.g. for query "split button" the only result should "SplitButton" since its the only query to contain "split" and "button"
                            // If any of the sub tokens is not in the string, we ignore the item. So the search gets more precise with more words
                            bool flag = item.IncludedInBuild;
                            foreach (string queryToken in querySplit)
                            {
                                // Check if token is not in string
                                if (item.Title.IndexOf(queryToken, StringComparison.CurrentCultureIgnoreCase) < 0)
                                {
                                    // Token is not in string, so we ignore this item.
                                    flag = false;
                                }
                            }
                            return flag;
                        });
                    foreach (var item in matchingItems)
                    {
                        suggestions.Add(item);
                    }
                }
                if (suggestions.Count > 0)
                {
                    controlsSearchBox.ItemsSource = suggestions.OrderByDescending(i => i.Title.StartsWith(sender.Text, StringComparison.CurrentCultureIgnoreCase)).ThenBy(i => i.Title);
                }
                else
                {
                    controlsSearchBox.ItemsSource = new string[] { "No results found" };
                }
            }
        }

        private void OnControlsSearchBoxQuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            if (args.ChosenSuggestion != null && args.ChosenSuggestion is ControlInfoDataItem)
            {
                var infoDataItem = args.ChosenSuggestion as ControlInfoDataItem;
                var itemId = infoDataItem.UniqueId;
                EnsureItemIsVisibleInNavigation(infoDataItem.Title);
                RootFrame.Navigate(typeof(ItemPage), itemId);
            }
            else if (!string.IsNullOrEmpty(args.QueryText))
            {
                RootFrame.Navigate(typeof(SearchResultsPage), args.QueryText);
            }
        }

        public void EnsureItemIsVisibleInNavigation(string name)
        {
            bool changedSelection = false;
            foreach (object rawItem in NavigationView.MenuItems)
            {
                // Check if we encountered the separator
                if (!(rawItem is NavigationViewItem))
                {
                    // Skipping this item
                    continue;
                }

                var item = rawItem as NavigationViewItem;

                // Check if we are this category
                if ((string)item.Content == name)
                {
                    NavigationView.SelectedItem = item;
                    changedSelection = true;
                }
                // We are not :/
                else
                {
                    // Maybe one of our items is? ಠಿ_ಠ
                    if (item.MenuItems.Count != 0)
                    {
                        foreach (NavigationViewItem child in item.MenuItems)
                        {
                            if ((string)child.Content == name)
                            {
                                // We are the item corresponding to the selected one, update selection!

                                // Deal with differences in displaymodes
                                if (NavigationView.PaneDisplayMode == NavigationViewPaneDisplayMode.Top)
                                {
                                    // In Topmode, the child is not visible, so set parent as selected
                                    // Everything else does not work unfortunately
                                    NavigationView.SelectedItem = item;
                                }
                                else
                                {
                                    // Expand so we animate
                                    item.IsExpanded = true;
                                    // Ensure parent is expanded so we actually show the selection indicator
                                    NavigationView.UpdateLayout();
                                    // Set selected item
                                    NavigationView.SelectedItem = child;
                                }
                                // Set to true to also skip out of outer for loop
                                changedSelection = true;
                                // Break out of child iteration for loop
                                break;
                            }
                        }
                    }
                }
                // We updated selection, break here!
                if (changedSelection)
                {
                    break;
                }
            }
        }

        private void NavigationViewControl_PaneClosing(NavigationView sender, NavigationViewPaneClosingEventArgs args)
        {
            UpdateAppTitleMargin(sender);
        }

        private void NavigationViewControl_PaneOpening(NavigationView sender, object args)
        {
            UpdateAppTitleMargin(sender);
        }

        private void NavigationViewControl_DisplayModeChanged(NavigationView sender, NavigationViewDisplayModeChangedEventArgs args)
        {
            Thickness currMargin = AppTitleBar.Margin;
            if (sender.DisplayMode == NavigationViewDisplayMode.Minimal)
            {
                AppTitleBar.Margin = new Thickness((sender.CompactPaneLength * 2), currMargin.Top, currMargin.Right, currMargin.Bottom);

            }
            else
            {
                AppTitleBar.Margin = new Thickness(sender.CompactPaneLength, currMargin.Top, currMargin.Right, currMargin.Bottom);
            }
            AppTitleBar.Visibility = sender.PaneDisplayMode == NavigationViewPaneDisplayMode.Top ? Visibility.Collapsed : Visibility.Visible;
            UpdateAppTitleMargin(sender);
            UpdateHeaderMargin(sender);
        }

        private void UpdateAppTitleMargin(NavigationView sender)
        {
            const int smallLeftIndent = 4, largeLeftIndent = 24;


            Thickness currMargin = AppTitle.Margin;

            if ((sender.DisplayMode == NavigationViewDisplayMode.Expanded && sender.IsPaneOpen) ||
                     sender.DisplayMode == NavigationViewDisplayMode.Minimal)
            {
                AppTitle.Margin = new Thickness(smallLeftIndent, currMargin.Top, currMargin.Right, currMargin.Bottom);
            }
            else
            {
                AppTitle.Margin = new Thickness(largeLeftIndent, currMargin.Top, currMargin.Right, currMargin.Bottom);
            }
        }

        private void UpdateHeaderMargin(NavigationView sender)
        {
            if (PageHeader != null)
            {
                if (sender.DisplayMode == NavigationViewDisplayMode.Minimal)
                {
                    PageHeader.HeaderPadding = (Thickness)App.Current.Resources["PageHeaderMinimalPadding"];
                }
                else
                {
                    PageHeader.HeaderPadding = (Thickness)App.Current.Resources["PageHeaderDefaultPadding"];
                }
            }
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (AnalyticsInfo.VersionInfo.DeviceFamily == "Windows.Xbox")
            {
                XboxContentSafeRect.Visibility = Visibility.Visible;
            }
        }
    }

    public enum DeviceType
    {
        Desktop,
        Mobile,
        Other,
        Xbox
    }
}
