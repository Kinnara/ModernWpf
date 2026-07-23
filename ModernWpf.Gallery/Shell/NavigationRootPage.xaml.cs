using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using ModernWpf.Controls;
using ModernWpf.Gallery.Testing;
using ModernWpf.Gallery.Models;
using ModernWpf.Gallery.Pages;
using ModernWpf.Gallery.Pages.WpfGallery;

namespace ModernWpf.Gallery.Shell
{
    public partial class NavigationRootPage
    {
        private readonly Stack<NavigationTarget> _backStack = new Stack<NavigationTarget>();
        private readonly Stack<NavigationTarget> _forwardStack = new Stack<NavigationTarget>();
        private readonly Dictionary<string, NavigationViewItem> _itemContainers = new Dictionary<string, NavigationViewItem>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, NavigationViewItem> _parentContainers = new Dictionary<string, NavigationViewItem>(StringComparer.OrdinalIgnoreCase);
        private static readonly ISet<string> WpfGalleryGroupIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "DesignGuidance",
            "Samples",
            "BasicInput",
            "Collections",
            "DateAndCalendar",
            "Layout",
            "Media",
            "Navigation",
            "StatusAndInfo",
            "Text",
            "System"
        };

        private static readonly IReadOnlyDictionary<string, string> WpfGalleryGlyphs = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "Home", "\uE80F" },
            { "WhatsNew", "\uEB51" },
            { "AllControls", "\uE8A9" },
            { "DesignGuidance", "\uEB3C" },
            { "Color", "\uE790" },
            { "Typography", "\uE8D2" },
            { "Spacing", "\uE8B3" },
            { "Geometry", "\uE743" },
            { "Iconography", "\uED58" },
            { "Samples", "\uEF58" },
            { "BasicInput", "\uE73A" },
            { "Collections", "\uE80A" },
            { "DateAndCalendar", "\uEC92" },
            { "Layout", "\uF246" },
            { "Media", "\uE8B9" },
            { "Navigation", "\uE700" },
            { "StatusAndInfo", "\uE8F2" },
            { "Text", "\uE8D2" },
            { "System", "\uE7F8" }
        };

        private NavigationViewItem _homeNavigationItem;
        private NavigationViewItem _whatsNewNavigationItem;
        private NavigationViewItem _allControlsNavigationItem;
        private NavigationTarget _currentTarget;
        private bool _isProgrammaticNavigation;
        private readonly DispatcherTimer _visualTestCommandTimer;
        private const string DefaultNavigationGroupGlyph = "\uEA37";
        private const string DefaultNavigationItemGlyph = "\uE729";

        public NavigationRootPage()
        {
            InitializeComponent();
            if (GalleryDiagnostics.IsEnabled)
            {
                AutomationProperties.SetAutomationId(this, "GalleryNavigationRoot");
                AutomationProperties.SetAutomationId(GetNavigationView(), "GalleryNavigationView");
                AutomationProperties.SetAutomationId(GetContentHost(), "GalleryContentHost");
            }

            Loaded += OnLoaded;
            Unloaded += OnUnloaded;
            GetVisualTestStatusPanel().Visibility = GalleryDiagnostics.IsEnabled
                ? Visibility.Visible
                : Visibility.Collapsed;
            if (GalleryDiagnostics.IsEnabled)
            {
                _visualTestCommandTimer = new DispatcherTimer(DispatcherPriority.Background)
                {
                    Interval = TimeSpan.FromMilliseconds(75)
                };
                _visualTestCommandTimer.Tick += OnVisualTestCommandTimerTick;
            }
            BuildNavigationMenu();
            Navigate(NavigationTarget.Home(), false);
        }

        public bool CanGoBack
        {
            get { return _backStack.Count > 0; }
        }

        public void GoBack()
        {
            if (_backStack.Count == 0)
            {
                return;
            }

            var target = _backStack.Pop();
            if (_currentTarget != null)
            {
                _forwardStack.Push(_currentTarget);
            }

            Navigate(target, false, true);
        }

        public void GoForward()
        {
            if (_forwardStack.Count == 0)
            {
                return;
            }

            var target = _forwardStack.Pop();
            if (_currentTarget != null)
            {
                _backStack.Push(_currentTarget);
            }

            Navigate(target, false, true);
        }

        public void NavigateTo(string navigationValue)
        {
            var target = ResolveNavigationTarget(navigationValue);
            if (target != null)
            {
                Navigate(target, false);
            }
        }

        internal static NavigationTarget ResolveNavigationTarget(string navigationValue)
        {
            if (string.IsNullOrWhiteSpace(navigationValue))
            {
                return NavigationTarget.Home();
            }

            var normalized = GalleryCatalog.NormalizeLookupId(NormalizeNavigationValue(navigationValue.Trim(), out var linkKind));
            if (string.IsNullOrWhiteSpace(normalized))
            {
                return NavigationTarget.Home();
            }

            if (string.Equals(normalized, "Home", StringComparison.OrdinalIgnoreCase))
            {
                return NavigationTarget.Home();
            }

            if (string.Equals(normalized, "AllControls", StringComparison.OrdinalIgnoreCase))
            {
                return NavigationTarget.AllControls();
            }

            if (string.Equals(normalized, "WhatsNew", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(normalized, "What's New", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(normalized, "Whats New", StringComparison.OrdinalIgnoreCase))
            {
                return NavigationTarget.WhatsNew();
            }

            if (string.Equals(normalized, "Settings", StringComparison.OrdinalIgnoreCase))
            {
                return NavigationTarget.Settings();
            }

            if (string.Equals(normalized, "NewControls", StringComparison.OrdinalIgnoreCase))
            {
                return NavigationTarget.Home();
            }

            if (linkKind != NavigationLinkKind.Item && GalleryCatalog.FindGroup(normalized) != null)
            {
                return NavigationTarget.Group(normalized);
            }

            var item = linkKind == NavigationLinkKind.Category ? null : GalleryCatalog.FindItem(normalized);
            if (item != null)
            {
                return NavigationTarget.Item(item.UniqueId);
            }

            return null;
        }

        private static string NormalizeNavigationValue(string value, out NavigationLinkKind linkKind)
        {
            linkKind = NavigationLinkKind.Unknown;

            if (Uri.TryCreate(value, UriKind.Absolute, out var uri))
            {
                var host = uri.Host;
                var path = Uri.UnescapeDataString(uri.AbsolutePath.Trim('/'));

                if (string.Equals(host, "item", StringComparison.OrdinalIgnoreCase))
                {
                    linkKind = NavigationLinkKind.Item;
                    return path;
                }

                if (string.Equals(host, "category", StringComparison.OrdinalIgnoreCase))
                {
                    linkKind = NavigationLinkKind.Category;
                    return path;
                }

                return string.IsNullOrEmpty(path) ? host : path;
            }

            var decodedValue = Uri.UnescapeDataString(value.Trim('/'));
            var parts = decodedValue.Split(new[] { '/', '\\' }, 2, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 2)
            {
                if (string.Equals(parts[0], "item", StringComparison.OrdinalIgnoreCase))
                {
                    linkKind = NavigationLinkKind.Item;
                    return parts[1];
                }

                if (string.Equals(parts[0], "category", StringComparison.OrdinalIgnoreCase))
                {
                    linkKind = NavigationLinkKind.Category;
                    return parts[1];
                }
            }

            return decodedValue;
        }

        private void BuildNavigationMenu()
        {
            _homeNavigationItem = CreateNavigationItem("Home", NavigationTarget.Home(), CreateWpfGalleryGlyphIcon("Home"));
            _whatsNewNavigationItem = CreateNavigationItem("What's New", NavigationTarget.WhatsNew(), CreateWpfGalleryGlyphIcon("WhatsNew"));
            _allControlsNavigationItem = CreateNavigationItem("All Controls", NavigationTarget.AllControls(), CreateWpfGalleryGlyphIcon("AllControls"));
            var navigation = GetNavigationView();

            navigation.MenuItems.Add(_homeNavigationItem);
            navigation.MenuItems.Add(_whatsNewNavigationItem);

            foreach (var group in GalleryCatalog.Groups)
            {
                var groupItem = CreateNavigationItem(group.Title, NavigationTarget.Group(group.UniqueId), CreateNavigationIcon(group.UniqueId, true, false));
                groupItem.IsExpanded = false;

                foreach (var item in group.Items)
                {
                    var child = CreateNavigationItem(item.Title, NavigationTarget.Item(item.UniqueId), CreateNavigationIcon(item.UniqueId, false, WpfGalleryGroupIds.Contains(group.UniqueId)));
                    groupItem.MenuItems.Add(child);
                    _itemContainers[item.UniqueId] = child;
                    _parentContainers[item.UniqueId] = groupItem;
                }

                navigation.MenuItems.Add(groupItem);
                _itemContainers[group.UniqueId] = groupItem;

                if (string.Equals(group.UniqueId, "Samples", StringComparison.OrdinalIgnoreCase))
                {
                    navigation.MenuItems.Add(_allControlsNavigationItem);
                }
            }
        }

        private NavigationViewItem CreateNavigationItem(string title, NavigationTarget target, IconElement icon)
        {
            var item = new NavigationViewItem
            {
                Content = title,
                Icon = icon,
                Tag = target
            };
            AutomationProperties.SetName(item, title);
            return item;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            _visualTestCommandTimer?.Start();
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            _visualTestCommandTimer?.Stop();
        }

        private void OnVisualTestCommandTimerTick(object sender, EventArgs e)
        {
            GalleryDiagnostics.TryProcessVisualScrollRequest(Window.GetWindow(this) ?? (DependencyObject)this);
        }

        private StackPanel GetVisualTestStatusPanel()
        {
            var root = (Grid)Content;
            return root.Children.OfType<StackPanel>().Single();
        }

        private TextBlock GetVisualTestStatusText(string automationId)
        {
            return GetVisualTestStatusPanel()
                .Children
                .OfType<TextBlock>()
                .Single(text => string.Equals(
                    AutomationProperties.GetAutomationId(text),
                    automationId,
                    StringComparison.Ordinal));
        }

        internal void OpenSettings()
        {
            Navigate(NavigationTarget.Settings(), true);
        }

        internal void ToggleNavigationPane()
        {
            var navigation = GetNavigationView();
            navigation.IsPaneOpen = !navigation.IsPaneOpen;
        }

        private static IconElement CreateNavigationIcon(string uniqueId, bool isGroup, bool isWpfGalleryChild)
        {
            var wpfGlyphIcon = CreateWpfGalleryGlyphIcon(uniqueId);
            if (wpfGlyphIcon != null)
            {
                return wpfGlyphIcon;
            }

            if (isWpfGalleryChild)
            {
                return null;
            }

            return CreateFontIcon(isGroup ? DefaultNavigationGroupGlyph : DefaultNavigationItemGlyph);
        }

        private static IconElement CreateWpfGalleryGlyphIcon(string uniqueId)
        {
            if (!WpfGalleryGlyphs.TryGetValue(uniqueId, out var glyph))
            {
                return null;
            }

            return CreateFontIcon(glyph);
        }

        private static FontIcon CreateFontIcon(string glyph)
        {
            var icon = new FontIcon
            {
                Glyph = glyph
            };

            var fontFamily = Application.Current.TryFindResource("SymbolThemeFontFamily") as FontFamily;
            if (fontFamily != null)
            {
                icon.FontFamily = fontFamily;
            }

            return icon;
        }

        private void OnNavigationItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
        {
            if (_isProgrammaticNavigation)
            {
                return;
            }

            if (args.IsSettingsInvoked)
            {
                Navigate(NavigationTarget.Settings(), true);
                return;
            }

            var container = args.InvokedItemContainer as FrameworkElement;
            var target = container == null ? null : container.Tag as NavigationTarget;
            if (target != null)
            {
                Navigate(target, true, expandNavigationPath: target.Kind != NavigationTargetKind.Group);
            }
        }

        internal void OnSearchTextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (args.Reason != AutoSuggestionBoxTextChangeReason.UserInput)
            {
                return;
            }

            var suggestions = GalleryCatalog.Search(sender.Text).Take(12).ToArray();
            sender.ItemsSource = suggestions.Length == 0 ? new object[] { "No results found" } : suggestions.Cast<object>().ToArray();
        }

        internal void OnSearchQuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            var item = args.ChosenSuggestion as GalleryItem;
            if (item == null && !string.IsNullOrWhiteSpace(args.QueryText))
            {
                item = GalleryCatalog.Search(args.QueryText).FirstOrDefault();
            }

            if (item != null)
            {
                Navigate(NavigationTarget.Item(item.UniqueId), true);
            }
        }

        private void Navigate(NavigationTarget target, bool addBackEntry, bool preserveForwardStack = false, bool expandNavigationPath = true)
        {
            var route = FormatRoute(target);
            SetVisualTestState(route, "Navigating:" + route);

            try
            {
                var isNewTarget = _currentTarget == null || !_currentTarget.Equals(target);
                if (isNewTarget)
                {
                    if (_currentTarget != null && addBackEntry)
                    {
                        _backStack.Push(_currentTarget);
                    }

                    if (!preserveForwardStack)
                    {
                        _forwardStack.Clear();
                    }
                }

                _currentTarget = target;
                var contentHost = GetContentHost();
                contentHost.Content = CreatePage(target);
                SelectNavigationItem(target, expandNavigationPath);
                UpdateBackButton();

                Dispatcher.BeginInvoke(
                    DispatcherPriority.Background,
                    new Action(() =>
                    {
                        if (_currentTarget != null && _currentTarget.Equals(target))
                        {
                            contentHost.UpdateLayout();
                            GalleryDiagnostics.PrepareInteractiveVisualState(contentHost);
                            contentHost.UpdateLayout();
                            GalleryDiagnostics.WriteVisualArtifacts(Window.GetWindow(this) ?? (DependencyObject)this);
                            SetVisualTestState(route, "Ready:" + route);
                        }
                    }));
            }
            catch (Exception ex)
            {
                GalleryDiagnostics.RecordException(ex);
                SetVisualTestState(route, "Failed:" + route);
                throw;
            }
        }

        private object CreatePage(NavigationTarget target)
        {
            if (target.Kind == NavigationTargetKind.Home)
            {
                var page = new DashboardPage();
                page.ItemRequested = item => Navigate(NavigationTarget.Item(item.UniqueId), true);
                page.GroupRequested = group => Navigate(NavigationTarget.Group(group.UniqueId), true);
                page.AllControlsRequested = () => Navigate(NavigationTarget.AllControls(), true);
                return page;
            }

            if (target.Kind == NavigationTargetKind.AllControls)
            {
                var page = new AllSamplesPage();
                page.ItemRequested = item => Navigate(NavigationTarget.Item(item.UniqueId), true);
                return page;
            }

            if (target.Kind == NavigationTargetKind.WhatsNew)
            {
                var page = new WhatsNewPage();
                page.ItemRequested = uniqueId => Navigate(NavigationTarget.Item(uniqueId), true);
                return page;
            }

            if (target.Kind == NavigationTargetKind.Settings)
            {
                return new SettingsPage();
            }

            if (target.Kind == NavigationTargetKind.Group)
            {
                var group = GalleryCatalog.FindGroup(target.UniqueId);
                var page = WpfGallerySectionPageFactory.Create(group);
                page.ItemRequested = item => Navigate(NavigationTarget.Item(item.UniqueId), true);
                return page;
            }

            var itemPage = new ItemPage(GalleryCatalog.FindItem(target.UniqueId));
            itemPage.ItemRequested = item => Navigate(NavigationTarget.Item(item.UniqueId), true);
            return itemPage;
        }

        private System.Windows.Controls.Frame GetContentHost()
        {
            var contentBorder = (Border)GetNavigationView().Content;
            return (System.Windows.Controls.Frame)contentBorder.Child;
        }

        private NavigationView GetNavigationView()
        {
            var root = (Grid)Content;
            return root.Children.OfType<NavigationView>().Single();
        }

        private void SelectNavigationItem(NavigationTarget target, bool expandNavigationPath = true)
        {
            var navigation = GetNavigationView();
            NavigationViewItem selectedItem = null;
            if (target.Kind == NavigationTargetKind.Home)
            {
                selectedItem = _homeNavigationItem;
            }
            else if (target.Kind == NavigationTargetKind.WhatsNew)
            {
                selectedItem = _whatsNewNavigationItem;
            }
            else if (target.Kind == NavigationTargetKind.AllControls)
            {
                selectedItem = _allControlsNavigationItem;
            }
            else if (target.Kind == NavigationTargetKind.Settings)
            {
                selectedItem = navigation.SettingsItem as NavigationViewItem;
            }
            else if (!string.IsNullOrEmpty(target.UniqueId))
            {
                _itemContainers.TryGetValue(target.UniqueId, out selectedItem);
            }

            _isProgrammaticNavigation = true;
            if (expandNavigationPath)
            {
                ExpandNavigationPath(target);
            }
            if (target.Kind == NavigationTargetKind.Item)
            {
                navigation.UpdateLayout();
            }

            ApplyNavigationSelection(navigation, selectedItem);
            _isProgrammaticNavigation = false;
        }

        private void ApplyNavigationSelection(NavigationView navigation, NavigationViewItem selectedItem)
        {
            navigation.SelectedItem = null;
            ClearNavigationSelection(navigation.MenuItems);
            if (selectedItem == null)
            {
                return;
            }

            navigation.SelectedItem = selectedItem;
            selectedItem.IsSelected = true;
            if (selectedItem.Tag is NavigationTarget { Kind: NavigationTargetKind.Item } target &&
                _parentContainers.TryGetValue(target.UniqueId, out var parentItem))
            {
                parentItem.IsChildSelected = true;
            }
        }

        private static void ClearNavigationSelection(System.Collections.IEnumerable items)
        {
            foreach (var item in items)
            {
                if (item is NavigationViewItem navigationItem)
                {
                    navigationItem.IsSelected = false;
                    navigationItem.IsChildSelected = false;
                    ClearNavigationSelection(navigationItem.MenuItems);
                }
            }
        }

        private void ExpandNavigationPath(NavigationTarget target)
        {
            if (target.Kind == NavigationTargetKind.Group)
            {
                if (_itemContainers.TryGetValue(target.UniqueId, out var groupItem))
                {
                    groupItem.IsExpanded = true;
                }
            }
            else if (target.Kind == NavigationTargetKind.Item)
            {
                if (_parentContainers.TryGetValue(target.UniqueId, out var parentItem))
                {
                    parentItem.IsExpanded = true;
                }
            }
        }

        private void UpdateBackButton()
        {
            var canGoBack = CanGoBack;
            GetNavigationView().IsBackEnabled = canGoBack;
            var window = Window.GetWindow(this) as MainWindow;
            if (window != null)
            {
                window.UpdateCanNavigateBack();
            }
        }

        private void SetVisualTestState(string route, string readyState)
        {
            GalleryDiagnostics.RecordRoute(route);
            GalleryDiagnostics.SetReadyState(readyState);

            GetVisualTestStatusText("GalleryVisualTestCurrentRoute").Text = GalleryDiagnostics.CurrentRoute;
            GetVisualTestStatusText("GalleryVisualTestReadyState").Text = GalleryDiagnostics.ReadyState;
            GetVisualTestStatusText("GalleryVisualTestLastException").Text = GalleryDiagnostics.LastException;
            GalleryDiagnostics.WriteStatusFile();
        }

        private void OnVisualTestRefreshArtifactsClick(object sender, RoutedEventArgs e)
        {
            if (!GalleryDiagnostics.IsEnabled)
            {
                return;
            }

            GalleryDiagnostics.WriteVisualArtifacts(Window.GetWindow(this) ?? (DependencyObject)this);
            GalleryDiagnostics.WriteStatusFile();
        }

        internal static string FormatRoute(NavigationTarget target)
        {
            if (target == null || target.Kind == NavigationTargetKind.Home)
            {
                return "home";
            }

            if (target.Kind == NavigationTargetKind.AllControls)
            {
                return "AllControls";
            }

            if (target.Kind == NavigationTargetKind.WhatsNew)
            {
                return "WhatsNew";
            }

            if (target.Kind == NavigationTargetKind.Settings)
            {
                return "settings";
            }

            if (target.Kind == NavigationTargetKind.Group)
            {
                return "category/" + target.UniqueId;
            }

            return "item/" + target.UniqueId;
        }

    }

    internal enum NavigationTargetKind
    {
        Home,
        WhatsNew,
        AllControls,
        Settings,
        Group,
        Item
    }

    internal enum NavigationLinkKind
    {
        Unknown,
        Category,
        Item
    }

    internal sealed class NavigationTarget : IEquatable<NavigationTarget>
    {
        private NavigationTarget(NavigationTargetKind kind, string uniqueId)
        {
            Kind = kind;
            UniqueId = uniqueId;
        }

        public NavigationTargetKind Kind { get; }
        public string UniqueId { get; }

        public static NavigationTarget Home()
        {
            return new NavigationTarget(NavigationTargetKind.Home, string.Empty);
        }

        public static NavigationTarget AllControls()
        {
            return new NavigationTarget(NavigationTargetKind.AllControls, string.Empty);
        }

        public static NavigationTarget WhatsNew()
        {
            return new NavigationTarget(NavigationTargetKind.WhatsNew, string.Empty);
        }

        public static NavigationTarget Settings()
        {
            return new NavigationTarget(NavigationTargetKind.Settings, string.Empty);
        }

        public static NavigationTarget Group(string uniqueId)
        {
            return new NavigationTarget(NavigationTargetKind.Group, uniqueId);
        }

        public static NavigationTarget Item(string uniqueId)
        {
            return new NavigationTarget(NavigationTargetKind.Item, uniqueId);
        }

        public bool Equals(NavigationTarget other)
        {
            return other != null &&
                Kind == other.Kind &&
                string.Equals(UniqueId, other.UniqueId, StringComparison.OrdinalIgnoreCase);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as NavigationTarget);
        }

        public override int GetHashCode()
        {
            return ((int)Kind * 397) ^ (UniqueId == null ? 0 : StringComparer.OrdinalIgnoreCase.GetHashCode(UniqueId));
        }
    }
}
