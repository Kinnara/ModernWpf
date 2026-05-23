using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using ModernWpf.Controls;
using ModernWpf.Controls.Primitives;
using ModernWpf.Gallery.Testing;
using ModernWpf.Gallery.Models;
using ModernWpf.Gallery.Pages;

namespace ModernWpf.Gallery.Shell
{
    public partial class NavigationRootPage
    {
        private readonly Stack<NavigationTarget> _backStack = new Stack<NavigationTarget>();
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
            { "AllControls", "\uE71D" },
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

        public NavigationRootPage()
        {
            InitializeComponent();

            BuildNavigationMenu();
            Navigate(NavigationTarget.Home(), false);
        }

        public void GoBack()
        {
            if (_backStack.Count == 0)
            {
                return;
            }

            Navigate(_backStack.Pop(), false);
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

            var normalized = NormalizeNavigationValue(navigationValue.Trim(), out var linkKind);
            if (string.IsNullOrWhiteSpace(normalized))
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
            _allControlsNavigationItem = CreateNavigationItem("All controls", NavigationTarget.AllControls(), CreateWpfGalleryGlyphIcon("AllControls"));

            Navigation.MenuItems.Add(_homeNavigationItem);
            Navigation.MenuItems.Add(_whatsNewNavigationItem);

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

                Navigation.MenuItems.Add(groupItem);
                _itemContainers[group.UniqueId] = groupItem;

                if (string.Equals(group.UniqueId, "Samples", StringComparison.OrdinalIgnoreCase))
                {
                    Navigation.MenuItems.Add(_allControlsNavigationItem);
                }
            }
        }

        private static NavigationViewItem CreateNavigationItem(string title, NavigationTarget target, IconElement icon)
        {
            var item = new NavigationViewItem
            {
                Content = CreateNavigationItemContent(title, target, icon),
                Margin = new Thickness(0, 1, 0, 1),
                Tag = target
            };
            AutomationProperties.SetName(item, title);
            AutomationProperties.SetAutomationId(item, "GalleryNav_" + FormatRoute(target).Replace("/", "_"));
            return item;
        }

        private static object CreateNavigationItemContent(string title, NavigationTarget target, IconElement icon)
        {
            var glyph = GetFontIconGlyph(icon);
            // These offsets preserve NavigationView behavior while matching the official WPF Gallery TreeView columns.
            if (glyph == null)
            {
                return CreateNavigationTextContent(title, target.Kind == NavigationTargetKind.Item ? 31 : 28);
            }

            return CreateNavigationGlyphContent(title, glyph, target.Kind == NavigationTargetKind.Item ? 15 : 28);
        }

        private static string GetFontIconGlyph(IconElement icon)
        {
            return (icon as FontIcon)?.Glyph;
        }

        private static Grid CreateNavigationGlyphContent(string title, string glyph, double leftMargin)
        {
            var grid = CreateNavigationContentGrid(leftMargin);
            grid.Tag = glyph;
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(16) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(16) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            var glyphText = new TextBlock
            {
                MaxWidth = 16,
                VerticalAlignment = VerticalAlignment.Center,
                FontSize = 16,
                Text = glyph,
                Focusable = false
            };
            AutomationProperties.SetName(glyphText, title + " Page");
            var fontFamily = Application.Current.TryFindResource("SymbolThemeFontFamily") as FontFamily;
            if (fontFamily != null)
            {
                glyphText.FontFamily = fontFamily;
            }

            var titleText = CreateNavigationTitleText(title);
            Grid.SetColumn(titleText, 2);

            grid.Children.Add(glyphText);
            grid.Children.Add(titleText);
            return grid;
        }

        private static Grid CreateNavigationTextContent(string title, double leftMargin)
        {
            var grid = CreateNavigationContentGrid(leftMargin);
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.Children.Add(CreateNavigationTitleText(title));
            return grid;
        }

        private static Grid CreateNavigationContentGrid(double leftMargin)
        {
            return new Grid
            {
                MinHeight = 30,
                Margin = new Thickness(leftMargin, 0, 0, 0)
            };
        }

        private static TextBlock CreateNavigationTitleText(string title)
        {
            return new TextBlock
            {
                VerticalAlignment = VerticalAlignment.Center,
                Text = title
            };
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

            return new SymbolIcon(isGroup ? Symbol.List : Symbol.Page);
        }

        private static IconElement CreateWpfGalleryGlyphIcon(string uniqueId)
        {
            if (!WpfGalleryGlyphs.TryGetValue(uniqueId, out var glyph))
            {
                return null;
            }

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
                Navigate(target, true);
            }
        }

        private void OnSearchTextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (args.Reason != AutoSuggestionBoxTextChangeReason.UserInput)
            {
                return;
            }

            var suggestions = GalleryCatalog.Search(sender.Text).Take(12).ToArray();
            sender.ItemsSource = suggestions.Length == 0 ? new object[] { "No results found" } : suggestions.Cast<object>().ToArray();
        }

        private void OnSearchQuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
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

        private void Navigate(NavigationTarget target, bool addBackEntry)
        {
            var route = FormatRoute(target);
            SetVisualTestState(route, "Navigating:" + route);

            try
            {
                if (_currentTarget != null && addBackEntry && !_currentTarget.Equals(target))
                {
                    _backStack.Push(_currentTarget);
                }

                _currentTarget = target;
                ContentHost.Content = CreatePage(target);
                SelectNavigationItem(target);
                UpdateBackButton();

                Dispatcher.BeginInvoke(
                    DispatcherPriority.ContextIdle,
                    new Action(() =>
                    {
                        if (_currentTarget != null && _currentTarget.Equals(target))
                        {
                            ContentHost.UpdateLayout();
                            GalleryDiagnostics.PrepareInteractiveVisualState(ContentHost);
                            ContentHost.UpdateLayout();
                            GalleryDiagnostics.WriteVisualArtifacts(ContentHost);
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
                var page = new HomePage();
                page.ItemRequested = item => Navigate(NavigationTarget.Item(item.UniqueId), true);
                page.GroupRequested = group => Navigate(NavigationTarget.Group(group.UniqueId), true);
                page.AllControlsRequested = () => Navigate(NavigationTarget.AllControls(), true);
                return page;
            }

            if (target.Kind == NavigationTargetKind.AllControls)
            {
                var page = new AllControlsPage();
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
                var page = new SectionPage(group);
                page.ItemRequested = item => Navigate(NavigationTarget.Item(item.UniqueId), true);
                return page;
            }

            var itemPage = new ItemPage(GalleryCatalog.FindItem(target.UniqueId));
            itemPage.ItemRequested = item => Navigate(NavigationTarget.Item(item.UniqueId), true);
            return itemPage;
        }

        private void SelectNavigationItem(NavigationTarget target)
        {
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
                selectedItem = Navigation.SettingsItem as NavigationViewItem;
            }
            else if (!string.IsNullOrEmpty(target.UniqueId))
            {
                _itemContainers.TryGetValue(target.UniqueId, out selectedItem);
            }

            _isProgrammaticNavigation = true;
            ExpandNavigationPath(target);
            Navigation.SelectedItem = selectedItem;
            _isProgrammaticNavigation = false;
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
            var canGoBack = _backStack.Count > 0;
            Navigation.IsBackEnabled = canGoBack;
            var window = Window.GetWindow(this) as MainWindow;
            if (window != null)
            {
                window.SetBackButtonVisible(canGoBack);
            }
        }

        private void SetVisualTestState(string route, string readyState)
        {
            GalleryDiagnostics.RecordRoute(route);
            GalleryDiagnostics.SetReadyState(readyState);

            VisualTestCurrentRouteText.Text = GalleryDiagnostics.CurrentRoute;
            VisualTestReadyStateText.Text = GalleryDiagnostics.ReadyState;
            VisualTestLastExceptionText.Text = GalleryDiagnostics.LastException;
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
