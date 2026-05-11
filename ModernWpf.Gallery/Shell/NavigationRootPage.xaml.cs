using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using ModernWpf.Controls;
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
            return new NavigationViewItem
            {
                Content = title,
                Icon = new SymbolIcon(symbol),
                Tag = target
            };
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
            if (_currentTarget != null && addBackEntry && !_currentTarget.Equals(target))
            {
                _backStack.Push(_currentTarget);
            }

            _currentTarget = target;
            ContentHost.Content = CreatePage(target);
            SelectNavigationItem(target);
            UpdateBackButton();
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
    }

    internal enum NavigationTargetKind
    {
        Home,
        AllControls,
        Group,
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
