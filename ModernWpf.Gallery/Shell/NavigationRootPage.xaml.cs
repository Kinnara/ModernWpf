using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Threading;
using ModernWpf.Controls;
using ModernWpf.Controls.Primitives;
using ModernWpf.Gallery.Testing;
using ModernWpf.Gallery.Models;
using ModernWpf.Gallery.Pages;
using ModernWpf.Gallery.Pages.WpfGallery;

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

        private static readonly IReadOnlyDictionary<string, string> WpfGalleryNavigationResourceAliases = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            { "NavigationViewItemBackground", "TreeViewItemBackground" },
            { "NavigationViewItemBackgroundPointerOver", "TreeViewItemBackgroundPointerOver" },
            { "NavigationViewItemBackgroundPressed", "TreeViewItemBackgroundPressed" },
            { "NavigationViewItemBackgroundDisabled", "TreeViewItemBackgroundDisabled" },
            { "NavigationViewItemBackgroundChecked", "TreeViewItemBackgroundSelected" },
            { "NavigationViewItemBackgroundCheckedPointerOver", "TreeViewItemBackgroundSelectedPointerOver" },
            { "NavigationViewItemBackgroundCheckedPressed", "TreeViewItemBackgroundSelectedPressed" },
            { "NavigationViewItemBackgroundCheckedDisabled", "TreeViewItemBackgroundSelectedDisabled" },
            { "NavigationViewItemBackgroundSelected", "TreeViewItemBackgroundSelected" },
            { "NavigationViewItemBackgroundSelectedPointerOver", "TreeViewItemBackgroundSelectedPointerOver" },
            { "NavigationViewItemBackgroundSelectedPressed", "TreeViewItemBackgroundSelectedPressed" },
            { "NavigationViewItemBackgroundSelectedDisabled", "TreeViewItemBackgroundSelectedDisabled" },
            { "NavigationViewItemForeground", "TreeViewItemForeground" },
            { "NavigationViewItemForegroundPointerOver", "TreeViewItemForegroundPointerOver" },
            { "NavigationViewItemForegroundPressed", "TreeViewItemForegroundPressed" },
            { "NavigationViewItemForegroundDisabled", "TreeViewItemForegroundDisabled" },
            { "NavigationViewItemForegroundChecked", "TreeViewItemForegroundSelected" },
            { "NavigationViewItemForegroundCheckedPointerOver", "TreeViewItemForegroundSelectedPointerOver" },
            { "NavigationViewItemForegroundCheckedPressed", "TreeViewItemForegroundSelectedPressed" },
            { "NavigationViewItemForegroundCheckedDisabled", "TreeViewItemForegroundSelectedDisabled" },
            { "NavigationViewItemForegroundSelected", "TreeViewItemForegroundSelected" },
            { "NavigationViewItemForegroundSelectedPointerOver", "TreeViewItemForegroundSelectedPointerOver" },
            { "NavigationViewItemForegroundSelectedPressed", "TreeViewItemForegroundSelectedPressed" },
            { "NavigationViewItemForegroundSelectedDisabled", "TreeViewItemForegroundSelectedDisabled" },
            { "NavigationViewItemBorderBrush", "TreeViewItemBorderBrush" },
            { "NavigationViewItemBorderBrushPointerOver", "TreeViewItemBorderBrushPointerOver" },
            { "NavigationViewItemBorderBrushPressed", "TreeViewItemBorderBrushPressed" },
            { "NavigationViewItemBorderBrushDisabled", "TreeViewItemBorderBrushDisabled" },
            { "NavigationViewItemBorderBrushChecked", "TreeViewItemBorderBrushSelected" },
            { "NavigationViewItemBorderBrushCheckedPointerOver", "TreeViewItemBorderBrushSelectedPointerOver" },
            { "NavigationViewItemBorderBrushCheckedPressed", "TreeViewItemBorderBrushSelectedPressed" },
            { "NavigationViewItemBorderBrushCheckedDisabled", "TreeViewItemBorderBrushSelectedDisabled" },
            { "NavigationViewItemBorderBrushSelected", "TreeViewItemBorderBrushSelected" },
            { "NavigationViewItemBorderBrushSelectedPointerOver", "TreeViewItemBorderBrushSelectedPointerOver" },
            { "NavigationViewItemBorderBrushSelectedPressed", "TreeViewItemBorderBrushSelectedPressed" },
            { "NavigationViewItemBorderBrushSelectedDisabled", "TreeViewItemBorderBrushSelectedDisabled" },
            { "NavigationViewSelectionIndicatorForeground", "TreeViewItemSelectionIndicatorForeground" }
        };

        private NavigationViewItem _homeNavigationItem;
        private NavigationViewItem _whatsNewNavigationItem;
        private NavigationViewItem _allControlsNavigationItem;
        private NavigationTarget _currentTarget;
        private bool _isProgrammaticNavigation;
        private bool _themeHandlersAttached;
        private const double TopLevelNavigationContentLeftMargin = 20;
        private const double ChildGlyphNavigationContentLeftMargin = -12;
        private const double ChildTextNavigationContentLeftMargin = 4;
        private const double TopLevelNavigationContentVerticalOffset = -2;
        private const double ChildNavigationContentVerticalOffset = -2;
        private static readonly Thickness DefaultNavigationSelectionIndicatorMargin = new Thickness(0);
        private static readonly Thickness ChildNavigationSelectionIndicatorMargin = new Thickness(-35, 0, 0, -6);
        private static readonly Thickness DefaultNavigationItemButtonMargin = new Thickness(4, 2, 4, 2);
        private static readonly Thickness ChildNavigationSelectedBackgroundMargin = new Thickness(12, 7, -5, -5);
        private static readonly Thickness ChildNavigationSelectedContentOffset = new Thickness(4, 2, 0, 0);
        private static readonly Color WpfGalleryLightNavigationPaneBackgroundColor = Color.FromRgb(250, 250, 250);

        public NavigationRootPage()
        {
            InitializeComponent();
            if (GalleryDiagnostics.IsEnabled)
            {
                AutomationProperties.SetAutomationId(this, "GalleryNavigationRoot");
                AutomationProperties.SetAutomationId(Navigation, "GalleryNavigationView");
                AutomationProperties.SetAutomationId(ContentHost, "GalleryContentHost");
            }

            AlignNavigationViewShellResourcesWithWpfGallery();
            Loaded += OnLoaded;
            Unloaded += OnUnloaded;
            VisualTestStatusPanel.Visibility = GalleryDiagnostics.IsEnabled
                ? Visibility.Visible
                : Visibility.Collapsed;
            SuppressNavigationViewDefaultExpandGlyph();

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
            _allControlsNavigationItem = CreateNavigationItem("All Controls", NavigationTarget.AllControls(), CreateWpfGalleryGlyphIcon("AllControls"));

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
                Margin = target.Kind == NavigationTargetKind.Item
                    ? new Thickness(20, 1, 0, 1)
                    : new Thickness(8, 1, 0, 1),
                Tag = target
            };
            AutomationProperties.SetName(item, title);
            return item;
        }

        private static object CreateNavigationItemContent(string title, NavigationTarget target, IconElement icon)
        {
            var glyph = GetFontIconGlyph(icon);
            var showDisclosureChevron = target.Kind == NavigationTargetKind.Group;
            var verticalOffset = target.Kind == NavigationTargetKind.Item
                ? ChildNavigationContentVerticalOffset
                : TopLevelNavigationContentVerticalOffset;
            // These offsets preserve NavigationView behavior while matching the official WPF Gallery TreeView columns.
            if (glyph == null)
            {
                return CreateNavigationTextContent(
                    title,
                    target.Kind == NavigationTargetKind.Item ? ChildTextNavigationContentLeftMargin : TopLevelNavigationContentLeftMargin,
                    verticalOffset,
                    showDisclosureChevron);
            }

            return CreateNavigationGlyphContent(
                title,
                glyph,
                target.Kind == NavigationTargetKind.Item ? ChildGlyphNavigationContentLeftMargin : TopLevelNavigationContentLeftMargin,
                16,
                verticalOffset,
                showDisclosureChevron);
        }

        private static string GetFontIconGlyph(IconElement icon)
        {
            return (icon as FontIcon)?.Glyph;
        }

        private static Grid CreateNavigationGlyphContent(
            string title,
            string glyph,
            double leftMargin,
            double textGap,
            double verticalOffset,
            bool showDisclosureChevron,
            double glyphColumnWidth = 16,
            double glyphFontSize = 16)
        {
            var grid = CreateNavigationContentGrid(showDisclosureChevron ? 0 : leftMargin);
            grid.Tag = glyph;
            if (showDisclosureChevron)
            {
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(20) });
            }

            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(glyphColumnWidth) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(textGap) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            var glyphText = new TextBlock
            {
                MaxWidth = glyphColumnWidth,
                VerticalAlignment = VerticalAlignment.Center,
                FontSize = glyphFontSize,
                Margin = new Thickness(0, verticalOffset, 0, 0),
                Text = glyph,
                Focusable = false
            };
            AutomationProperties.SetName(glyphText, title + " Page");
            var fontFamily = Application.Current.TryFindResource("SymbolThemeFontFamily") as FontFamily;
            if (fontFamily != null)
            {
                glyphText.FontFamily = fontFamily;
            }

            var titleText = CreateNavigationTitleText(title, verticalOffset);
            var glyphColumn = showDisclosureChevron ? 1 : 0;
            Grid.SetColumn(glyphText, glyphColumn);
            Grid.SetColumn(titleText, glyphColumn + 2);

            grid.Children.Add(glyphText);
            grid.Children.Add(titleText);
            AddDisclosureChevron(grid, showDisclosureChevron, verticalOffset);
            return grid;
        }

        private static Grid CreateNavigationTextContent(string title, double leftMargin, double verticalOffset, bool showDisclosureChevron)
        {
            var grid = CreateNavigationContentGrid(showDisclosureChevron ? 0 : leftMargin);
            if (showDisclosureChevron)
            {
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(20) });
            }

            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            var titleText = CreateNavigationTitleText(title, verticalOffset);
            if (showDisclosureChevron)
            {
                Grid.SetColumn(titleText, 1);
            }

            grid.Children.Add(titleText);
            AddDisclosureChevron(grid, showDisclosureChevron, verticalOffset);
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

        private static TextBlock CreateNavigationTitleText(string title, double verticalOffset)
        {
            return new TextBlock
            {
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, verticalOffset, 0, 0),
                Text = title
            };
        }

        private static void AddDisclosureChevron(Grid grid, bool showDisclosureChevron, double verticalOffset)
        {
            if (!showDisclosureChevron)
            {
                return;
            }

            var chevron = new TextBlock
            {
                Width = 15,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Center,
                FontSize = 10,
                Margin = new Thickness(0, verticalOffset, 0, 0),
                Focusable = false,
                Text = "\uE76C",
                RenderTransformOrigin = new Point(0.5, 0.5),
                RenderTransform = new RotateTransform()
            };

            var fontFamily = Application.Current.TryFindResource("SymbolThemeFontFamily") as FontFamily;
            if (fontFamily != null)
            {
                chevron.FontFamily = fontFamily;
            }

            BindingOperations.SetBinding(
                chevron.RenderTransform,
                RotateTransform.AngleProperty,
                new Binding(nameof(NavigationViewItem.IsExpanded))
                {
                    RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(NavigationViewItem), 1),
                    Converter = TreeViewChevronAngleConverter.Instance
                });

            Grid.SetColumn(chevron, 0);
            grid.Children.Add(chevron);
        }

        private void SuppressNavigationViewDefaultExpandGlyph()
        {
            Resources["NavigationViewItemExpandedPath"] = Geometry.Empty;
            Navigation.Resources["NavigationViewItemExpandedPath"] = Geometry.Empty;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            AttachThemeHandlers();
            AlignNavigationViewShellResourcesWithWpfGallery();
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            DetachThemeHandlers();
        }

        private void AttachThemeHandlers()
        {
            if (_themeHandlersAttached)
            {
                return;
            }

            ThemeManager.Current.ActualApplicationThemeChanged += OnActualApplicationThemeChanged;
            SystemParameters.StaticPropertyChanged += OnSystemParametersChanged;
            _themeHandlersAttached = true;
        }

        private void DetachThemeHandlers()
        {
            if (!_themeHandlersAttached)
            {
                return;
            }

            ThemeManager.Current.ActualApplicationThemeChanged -= OnActualApplicationThemeChanged;
            SystemParameters.StaticPropertyChanged -= OnSystemParametersChanged;
            _themeHandlersAttached = false;
        }

        private void OnActualApplicationThemeChanged(ThemeManager sender, object args)
        {
            AlignNavigationViewShellResourcesWithWpfGallery();
        }

        private void OnSystemParametersChanged(object sender, PropertyChangedEventArgs e)
        {
            if (string.Equals(e.PropertyName, nameof(SystemParameters.HighContrast), StringComparison.Ordinal))
            {
                AlignNavigationViewShellResourcesWithWpfGallery();
            }
        }

        private void AlignNavigationViewShellResourcesWithWpfGallery()
        {
            foreach (var alias in WpfGalleryNavigationResourceAliases)
            {
                Navigation.Resources[alias.Key] = TryFindResource(alias.Value);
            }

            var paneBackground = GetWpfGalleryNavigationPaneBackground();
            Navigation.Resources["NavigationViewDefaultPaneBackground"] = paneBackground;
            Navigation.Resources["NavigationViewExpandedPaneBackground"] = paneBackground;
            Navigation.Resources["NavigationViewItemSeparatorForeground"] = paneBackground;
            AlignNavigationViewShellChromeWithWpfGallery(paneBackground);
        }

        private void AlignNavigationViewShellChromeWithWpfGallery(Brush paneBackground)
        {
            var menuScrollViewer = FindVisualChild<ScrollViewer>(
                Navigation,
                scrollViewer => string.Equals(scrollViewer.Name, "MenuItemsScrollViewer", StringComparison.Ordinal));
            if (menuScrollViewer != null)
            {
                menuScrollViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Hidden;
            }

            var rootSplitView = FindVisualChild<SplitView>(
                Navigation,
                splitView => string.Equals(splitView.Name, "RootSplitView", StringComparison.Ordinal));
            if (rootSplitView != null)
            {
                rootSplitView.Background = paneBackground;
                rootSplitView.PaneBackground = paneBackground;
                rootSplitView.BorderBrush = paneBackground;
                rootSplitView.CornerRadius = new CornerRadius(0);
            }

            var paneContentGrid = FindVisualChild<Border>(
                Navigation,
                border => string.Equals(border.Name, "PaneContentGrid", StringComparison.Ordinal));
            if (paneContentGrid != null)
            {
                paneContentGrid.BorderBrush = paneBackground;
            }

            var paneShadow = FindVisualChild<ThemeShadowChrome>(
                Navigation,
                shadow => string.Equals(shadow.Name, "ShadowCaster", StringComparison.Ordinal));
            if (paneShadow != null)
            {
                paneShadow.Visibility = Visibility.Collapsed;
                paneShadow.Opacity = 0;
                paneShadow.Depth = 0;
                paneShadow.IsShadowEnabled = false;
            }
        }

        private Brush GetWpfGalleryNavigationPaneBackground()
        {
            if (SystemParameters.HighContrast)
            {
                return SystemColors.WindowBrush;
            }

            if (ThemeManager.Current.ActualApplicationTheme == ApplicationTheme.Dark)
            {
                return TryFindResource("SolidBackgroundFillColorBaseBrush") as Brush
                    ?? new SolidColorBrush(Color.FromRgb(32, 32, 32));
            }

            return new SolidColorBrush(WpfGalleryLightNavigationPaneBackgroundColor);
        }

        private void OnSettingsButtonClick(object sender, RoutedEventArgs e)
        {
            RaiseSettingsOpenedNotification(SettingsButton);
        }

        internal void OpenSettings()
        {
            Navigate(NavigationTarget.Settings(), true);
        }

        private static void RaiseSettingsOpenedNotification(UIElement element)
        {
#if NET8_0_OR_GREATER
            var peer = UIElementAutomationPeer.FromElement(element)
                ?? UIElementAutomationPeer.CreatePeerForElement(element);
            peer?.RaiseNotificationEvent(
                AutomationNotificationKind.Other,
                AutomationNotificationProcessing.ImportantMostRecent,
                "Settings Page Opened",
                "ButtonClickedActivity");
#endif
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
                    DispatcherPriority.Background,
                    new Action(() =>
                    {
                        if (_currentTarget != null && _currentTarget.Equals(target))
                        {
                            ContentHost.UpdateLayout();
                            GalleryDiagnostics.PrepareInteractiveVisualState(ContentHost);
                            ContentHost.UpdateLayout();
                            SetVisualTestState(route, "Ready:" + route);
                            GalleryDiagnostics.WriteVisualArtifacts(Window.GetWindow(this) ?? (DependencyObject)this);
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
                var page = WpfGallerySectionPageFactory.Create(group);
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
            else if (!string.IsNullOrEmpty(target.UniqueId))
            {
                _itemContainers.TryGetValue(target.UniqueId, out selectedItem);
            }

            _isProgrammaticNavigation = true;
            ExpandNavigationPath(target);
            if (target.Kind == NavigationTargetKind.Item)
            {
                Navigation.UpdateLayout();
            }

            ApplyNavigationSelection(selectedItem);
            _isProgrammaticNavigation = false;
        }

        private void ApplyNavigationSelection(NavigationViewItem selectedItem)
        {
            Navigation.SelectedItem = null;
            ClearNavigationSelection(Navigation.MenuItems);
            if (selectedItem == null)
            {
                return;
            }

            Navigation.SelectedItem = selectedItem;
            selectedItem.IsSelected = true;
            if (selectedItem.Tag is NavigationTarget { Kind: NavigationTargetKind.Item } target &&
                _parentContainers.TryGetValue(target.UniqueId, out var parentItem))
            {
                parentItem.IsChildSelected = true;
            }

            AlignSelectionIndicatorWithWpfGalleryTreeView(selectedItem);
        }

        private static void AlignSelectionIndicatorWithWpfGalleryTreeView(NavigationViewItem selectedItem)
        {
            var indicator = FindVisualChild<FrameworkElement>(
                selectedItem,
                element => string.Equals(element.Name, "SelectionIndicator", StringComparison.Ordinal));
            if (indicator == null)
            {
                return;
            }

            indicator.HorizontalAlignment = HorizontalAlignment.Left;
            indicator.VerticalAlignment = VerticalAlignment.Center;
            indicator.Margin = selectedItem.Tag is NavigationTarget { Kind: NavigationTargetKind.Item }
                ? ChildNavigationSelectionIndicatorMargin
                : DefaultNavigationSelectionIndicatorMargin;

            AlignSelectedNavigationItemBackgroundWithWpfGalleryTreeView(selectedItem);
            AlignSelectedNavigationItemContentWithWpfGalleryTreeView(selectedItem);
        }

        private static void AlignSelectedNavigationItemBackgroundWithWpfGalleryTreeView(NavigationViewItem selectedItem)
        {
            var layoutRoot = GetNavigationItemLayoutRoot(selectedItem);
            if (layoutRoot == null)
            {
                return;
            }

            layoutRoot.Margin = selectedItem.Tag is NavigationTarget { Kind: NavigationTargetKind.Item }
                ? ChildNavigationSelectedBackgroundMargin
                : DefaultNavigationItemButtonMargin;
        }

        private static void ResetNavigationItemBackgroundAlignment(NavigationViewItem item)
        {
            var layoutRoot = GetNavigationItemLayoutRoot(item);
            if (layoutRoot != null)
            {
                layoutRoot.Margin = DefaultNavigationItemButtonMargin;
            }
        }

        private static void AlignSelectedNavigationItemContentWithWpfGalleryTreeView(NavigationViewItem selectedItem)
        {
            if (selectedItem.Tag is not NavigationTarget { Kind: NavigationTargetKind.Item })
            {
                ResetNavigationItemContentAlignment(selectedItem);
                return;
            }

            if (selectedItem.Content is Grid contentGrid)
            {
                var defaultMargin = GetDefaultNavigationItemContentMargin(selectedItem);
                contentGrid.Margin = new Thickness(
                    defaultMargin.Left + ChildNavigationSelectedContentOffset.Left,
                    defaultMargin.Top + ChildNavigationSelectedContentOffset.Top,
                    defaultMargin.Right + ChildNavigationSelectedContentOffset.Right,
                    defaultMargin.Bottom + ChildNavigationSelectedContentOffset.Bottom);
            }
        }

        private static void ResetNavigationItemContentAlignment(NavigationViewItem item)
        {
            if (item.Content is Grid contentGrid)
            {
                contentGrid.Margin = GetDefaultNavigationItemContentMargin(item);
            }
        }

        private static Thickness GetDefaultNavigationItemContentMargin(NavigationViewItem item)
        {
            if (item.Tag is NavigationTarget { Kind: NavigationTargetKind.Item })
            {
                return new Thickness(item.Content is Grid { Tag: string } ? ChildGlyphNavigationContentLeftMargin : ChildTextNavigationContentLeftMargin, 0, 0, 0);
            }

            if (item.Tag is NavigationTarget { Kind: NavigationTargetKind.Group })
            {
                return new Thickness(0);
            }

            return new Thickness(TopLevelNavigationContentLeftMargin, 0, 0, 0);
        }

        private static Border GetNavigationItemLayoutRoot(NavigationViewItem item)
        {
            return FindVisualChild<Border>(
                item,
                border => string.Equals(border.Name, "LayoutRoot", StringComparison.Ordinal));
        }

        private static T FindVisualChild<T>(DependencyObject element, Func<T, bool> predicate)
            where T : DependencyObject
        {
            if (element == null)
            {
                return null;
            }

            for (var i = 0; i < VisualTreeHelper.GetChildrenCount(element); i++)
            {
                var child = VisualTreeHelper.GetChild(element, i);
                if (child is T match && predicate(match))
                {
                    return match;
                }

                var descendant = FindVisualChild(child, predicate);
                if (descendant != null)
                {
                    return descendant;
                }
            }

            return null;
        }

        private static void ClearNavigationSelection(System.Collections.IEnumerable items)
        {
            foreach (var item in items)
            {
                if (item is NavigationViewItem navigationItem)
                {
                    navigationItem.IsSelected = false;
                    navigationItem.IsChildSelected = false;
                    ResetNavigationItemBackgroundAlignment(navigationItem);
                    ResetNavigationItemContentAlignment(navigationItem);
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

        private sealed class TreeViewChevronAngleConverter : IValueConverter
        {
            public static readonly TreeViewChevronAngleConverter Instance = new TreeViewChevronAngleConverter();

            public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            {
                return value is bool isExpanded && isExpanded ? 90d : 0d;
            }

            public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            {
                throw new NotSupportedException();
            }
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
