using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Threading;
using ModernWpf.Controls;
using ModernWpf.Gallery.Testing;
using ModernWpf.Gallery.Models;
using ModernWpf.Gallery.Pages;

namespace ModernWpf.Gallery.Shell
{
    public partial class NavigationRootPage
    {
        private readonly Stack<NavigationTarget> _backStack = new Stack<NavigationTarget>();
        private readonly Dictionary<string, NavigationViewItem> _itemContainers = new Dictionary<string, NavigationViewItem>(StringComparer.OrdinalIgnoreCase);
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

            if (string.Equals(normalized, "NewControls", StringComparison.OrdinalIgnoreCase))
            {
                return NavigationTarget.Home();
            }

            if (linkKind != NavigationLinkKind.Item && GalleryCatalog.FindGroup(normalized) != null)
            {
                return NavigationTarget.Group(normalized);
            }

            if (linkKind != NavigationLinkKind.Category && GalleryCatalog.FindItem(normalized) != null)
            {
                return NavigationTarget.Item(normalized);
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

            var parts = value.Trim('/').Split(new[] { '/', '\\' }, 2, StringSplitOptions.RemoveEmptyEntries);
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

            return value.Trim('/');
        }

        private void BuildNavigationMenu()
        {
            Navigation.MenuItems.Add(CreateNavigationItem("Home", NavigationTarget.Home(), Symbol.Home));
            Navigation.MenuItems.Add(CreateNavigationItem("All controls", NavigationTarget.AllControls(), Symbol.ViewAll));
            Navigation.MenuItems.Add(new NavigationViewItemSeparator());

            foreach (var group in GalleryCatalog.Groups)
            {
                var groupItem = CreateNavigationItem(group.Title, NavigationTarget.Group(group.UniqueId), Symbol.List);
                groupItem.IsExpanded = !group.IsSpecialSection;

                foreach (var item in group.Items)
                {
                    var child = CreateNavigationItem(item.Title, NavigationTarget.Item(item.UniqueId), Symbol.Page);
                    groupItem.MenuItems.Add(child);
                    _itemContainers[item.UniqueId] = child;
                }

                Navigation.MenuItems.Add(groupItem);
                _itemContainers[group.UniqueId] = groupItem;
            }
        }

        private static NavigationViewItem CreateNavigationItem(string title, NavigationTarget target, Symbol symbol)
        {
            var item = new NavigationViewItem
            {
                Content = title,
                Icon = new SymbolIcon(symbol),
                Tag = target
            };
            AutomationProperties.SetAutomationId(item, "GalleryNav_" + FormatRoute(target).Replace("/", "_"));
            return item;
        }

        private void OnNavigationItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
        {
            if (_isProgrammaticNavigation || args.IsSettingsInvoked)
            {
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
                selectedItem = Navigation.MenuItems.OfType<NavigationViewItem>().FirstOrDefault();
            }
            else if (target.Kind == NavigationTargetKind.AllControls)
            {
                selectedItem = Navigation.MenuItems.OfType<NavigationViewItem>().Skip(1).FirstOrDefault();
            }
            else if (!string.IsNullOrEmpty(target.UniqueId))
            {
                _itemContainers.TryGetValue(target.UniqueId, out selectedItem);
            }

            _isProgrammaticNavigation = true;
            Navigation.SelectedItem = selectedItem;
            _isProgrammaticNavigation = false;
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
        AllControls,
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
