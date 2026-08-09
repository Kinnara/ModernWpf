using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Threading;
using ModernWpf.Automation.Peers;
using static ModernWpf.ResourceAccessor;

namespace ModernWpf.Controls
{
    [ContentProperty(nameof(TabItems))]
    [TemplatePart(Name = TabListViewName, Type = typeof(TabViewListView))]
    [TemplatePart(Name = TabContentPresenterName, Type = typeof(ContentPresenter))]
    [TemplatePart(Name = AddButtonName, Type = typeof(ButtonBase))]
    [TemplatePart(Name = ScrollViewerName, Type = typeof(ScrollViewer))]
    [TemplatePart(Name = ScrollDecreaseButtonName, Type = typeof(ButtonBase))]
    [TemplatePart(Name = ScrollIncreaseButtonName, Type = typeof(ButtonBase))]
    public partial class TabView : Control
    {
        private const string TabListViewName = "PART_TabListView";
        private const string TabContentPresenterName = "PART_TabContentPresenter";
        private const string AddButtonName = "PART_AddButton";
        private const string ScrollViewerName = "PART_ScrollViewer";
        private const string ScrollDecreaseButtonName = "PART_ScrollDecreaseButton";
        private const string ScrollIncreaseButtonName = "PART_ScrollIncreaseButton";
        private const string DragPayloadFormat = "ModernWpf.Controls.TabView.DragPayload";
        private const double ScrollAmount = 50.0;
        private const double ScrollThreshold = 0.1;
        private static readonly ResourceAccessor ResourceAccessor = new ResourceAccessor(typeof(TabView));

        private TabViewListView _tabListView;
        private ContentPresenter _tabContentPresenter;
        private ButtonBase _addButton;
        private ButtonBase _scrollDecreaseButton;
        private ButtonBase _scrollIncreaseButton;
        private ScrollViewer _scrollViewer;
        private INotifyCollectionChanged _itemsSourceNotifier;
        private bool _updatingSelection;
        private bool _preparingContainers;
        private bool _reordering;
        private bool _overflowUpdatePending;
        private bool _bringSelectedTabPending;

        static TabView()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(TabView), new FrameworkPropertyMetadata(typeof(TabView)));
            KeyboardNavigation.TabNavigationProperty.OverrideMetadata(
                typeof(TabView),
                new FrameworkPropertyMetadata(KeyboardNavigationMode.Local));
            FocusableProperty.OverrideMetadata(typeof(TabView), new FrameworkPropertyMetadata(false));
            IsTabStopProperty.OverrideMetadata(typeof(TabView), new FrameworkPropertyMetadata(false));
        }

        public TabView()
        {
            var tabItems = new ObservableCollection<object>();
            tabItems.CollectionChanged += OnEffectiveItemsCollectionChanged;
            SetValue(TabItemsPropertyKey, tabItems);

            Loaded += OnLoaded;
        }

        public event TypedEventHandler<TabView, TabViewTabCloseRequestedEventArgs> TabCloseRequested;

        public event TypedEventHandler<TabView, TabViewTabDroppedOutsideEventArgs> TabDroppedOutside;

        public event TypedEventHandler<TabView, object> AddTabButtonClick;

        public event TypedEventHandler<TabView, NotifyCollectionChangedEventArgs> TabItemsChanged;

        public event SelectionChangedEventHandler SelectionChanged;

        public event TypedEventHandler<TabView, TabViewTabDragStartingEventArgs> TabDragStarting;

        public event TypedEventHandler<TabView, TabViewTabDragCompletedEventArgs> TabDragCompleted;

        public event DragEventHandler TabStripDragOver;

        public event DragEventHandler TabStripDrop;

        public event TypedEventHandler<TabView, TabViewTabTearOutWindowRequestedEventArgs> TabTearOutWindowRequested;

        public event TypedEventHandler<TabView, TabViewTabTearOutRequestedEventArgs> TabTearOutRequested;

        public event TypedEventHandler<TabView, TabViewExternalTornOutTabsDroppingEventArgs> ExternalTornOutTabsDropping;

        public event TypedEventHandler<TabView, TabViewExternalTornOutTabsDroppedEventArgs> ExternalTornOutTabsDropped;

        internal TabViewTabTearOutWindowRequestedEventArgs RaiseTabTearOutWindowRequested(object[] items, UIElement[] tabs)
        {
            var args = new TabViewTabTearOutWindowRequestedEventArgs(items, tabs);
            TabTearOutWindowRequested?.Invoke(this, args);
            return args;
        }

        internal void RaiseTabTearOutRequested(object[] items, UIElement[] tabs, Window newWindow)
        {
            TabTearOutRequested?.Invoke(this, new TabViewTabTearOutRequestedEventArgs(items, tabs, newWindow));
        }

        internal TabViewExternalTornOutTabsDroppingEventArgs RaiseExternalTornOutTabsDropping(object[] items, UIElement[] tabs, int dropIndex)
        {
            var args = new TabViewExternalTornOutTabsDroppingEventArgs(items, tabs, dropIndex);
            ExternalTornOutTabsDropping?.Invoke(this, args);
            return args;
        }

        internal void RaiseExternalTornOutTabsDropped(object[] items, UIElement[] tabs, int dropIndex)
        {
            ExternalTornOutTabsDropped?.Invoke(this, new TabViewExternalTornOutTabsDroppedEventArgs(items, tabs, dropIndex));
        }

        internal TabViewTabDragStartingEventArgs RaiseTabDragStarting(IDataObject data, object item, TabViewItem tab)
        {
            var args = new TabViewTabDragStartingEventArgs(data, item, tab);
            TabDragStarting?.Invoke(this, args);
            return args;
        }

        internal void RaiseTabDragCompleted(DragDropEffects dropResult, object item, TabViewItem tab)
        {
            TabDragCompleted?.Invoke(this, new TabViewTabDragCompletedEventArgs(dropResult, item, tab));
        }

        internal void RaiseTabDroppedOutside(object item, TabViewItem tab)
        {
            TabDroppedOutside?.Invoke(this, new TabViewTabDroppedOutsideEventArgs(item, tab));
        }

        internal void StartDrag(TabViewItem tab)
        {
            var index = IndexOfTab(tab);
            if (index < 0 || !CanDragTabs)
            {
                return;
            }

            var item = GetItemAt(index);
            var payload = new TabViewDragPayload(this, item, tab);
            var data = new DataObject();
            data.SetData(DragPayloadFormat, payload);
            data.SetData(typeof(TabViewItem), tab);
            data.SetData(typeof(object), item);

            var startingArgs = RaiseTabDragStarting(data, item, tab);
            if (startingArgs.Cancel)
            {
                return;
            }

            var canceled = false;
            QueryContinueDragEventHandler queryContinueDrag = (_, args) => canceled |= args.EscapePressed;
            tab.QueryContinueDrag += queryContinueDrag;
            DragDropEffects result;
            tab.SetDragging(true);
            try
            {
                result = DragDrop.DoDragDrop(tab, data, DragDropEffects.Move);
            }
            finally
            {
                tab.SetDragging(false);
                tab.QueryContinueDrag -= queryContinueDrag;
            }

            if (result == DragDropEffects.None && !canceled)
            {
                CompleteDroppedOutside(item, tab, GetCurrentScreenPosition(tab));
            }

            RaiseTabDragCompleted(result, item, tab);
        }

        internal bool ReorderTab(int oldIndex, int newIndex)
        {
            var count = GetItemCount();
            if (!CanReorderTabs ||
                oldIndex < 0 ||
                oldIndex >= count ||
                newIndex < 0 ||
                newIndex >= count ||
                oldIndex == newIndex)
            {
                return false;
            }

            var selectedItem = SelectedItem;
            var source = TabItemsSource ?? TabItems;
            var notifier = source as INotifyCollectionChanged;
            _reordering = true;
            try
            {
                if (ReferenceEquals(source, TabItems))
                {
                    TabItems.Move(oldIndex, newIndex);
                }
                else if (source is IList list && !list.IsReadOnly && !list.IsFixedSize)
                {
                    var item = list[oldIndex];
                    list.RemoveAt(oldIndex);
                    list.Insert(newIndex, item);
                }
                else
                {
                    return false;
                }
            }
            finally
            {
                _reordering = false;
            }

            if (notifier == null)
            {
                RefreshItemsSource();
                TabItemsChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            }

            var selectedIndex = selectedItem == null ? -1 : IndexOfItem(selectedItem);
            SetSelection(
                selectedIndex,
                selectedIndex >= 0 ? GetItemAt(selectedIndex) : null,
                ResolveTabViewItemForIndex(selectedIndex),
                false);
            Dispatcher.BeginInvoke(DispatcherPriority.Loaded, new Action(PrepareRealizedContainers));
            return true;
        }

        internal void CompleteDroppedOutside(object item, TabViewItem tab, Point screenPosition)
        {
            RaiseTabDroppedOutside(item, tab);
            if (!CanTearOutTabs)
            {
                return;
            }

            var items = new[] { item };
            UIElement[] tabs = { tab };
            var windowArgs = RaiseTabTearOutWindowRequested(items, tabs);
            var newWindow = windowArgs.NewWindow;
            if (newWindow == null)
            {
                return;
            }

            RaiseTabTearOutRequested(items, tabs, newWindow);
            if (!double.IsNaN(screenPosition.X) && !double.IsInfinity(screenPosition.X))
            {
                newWindow.Left = screenPosition.X;
            }
            if (!double.IsNaN(screenPosition.Y) && !double.IsInfinity(screenPosition.Y))
            {
                newWindow.Top = screenPosition.Y;
            }

            if (!newWindow.IsVisible)
            {
                newWindow.Show();
            }
        }

        internal bool CompleteExternalDrop(object[] items, UIElement[] tabs, int dropIndex)
        {
            if (!CanTearOutTabs)
            {
                return false;
            }

            dropIndex = Math.Max(0, Math.Min(dropIndex, GetItemCount()));
            var droppingArgs = RaiseExternalTornOutTabsDropping(items, tabs, dropIndex);
            if (!droppingArgs.AllowDrop)
            {
                return false;
            }

            RaiseExternalTornOutTabsDropped(items, tabs, dropIndex);
            return true;
        }

        public override void OnApplyTemplate()
        {
            UnhookTemplateParts();
            base.OnApplyTemplate();

            _tabListView = GetTemplateChild(TabListViewName) as TabViewListView;
            _tabContentPresenter = GetTemplateChild(TabContentPresenterName) as ContentPresenter;
            _addButton = GetTemplateChild(AddButtonName) as ButtonBase;
            _scrollViewer = GetTemplateChild(ScrollViewerName) as ScrollViewer;
            _scrollDecreaseButton = GetTemplateChild(ScrollDecreaseButtonName) as ButtonBase;
            _scrollIncreaseButton = GetTemplateChild(ScrollIncreaseButtonName) as ButtonBase;

            if (_tabListView != null)
            {
                _tabListView.Owner = this;
                _tabListView.ItemContainerGenerator.StatusChanged += OnGeneratorStatusChanged;
                _tabListView.AddHandler(UIElement.DragOverEvent, new DragEventHandler(OnTabStripDragOver), true);
                _tabListView.AddHandler(UIElement.DropEvent, new DragEventHandler(OnTabStripDrop), true);
                ApplyItemTemplate();
                RefreshItemsSource();
            }

            if (_addButton != null)
            {
                _addButton.Click += OnAddButtonClick;
                _addButton.PreviewKeyDown += OnAddButtonPreviewKeyDown;
                SetLocalizedButtonText(
                    _addButton,
                    SR_TabViewAddButtonName,
                    SR_TabViewAddButtonTooltip);
            }

            if (_scrollDecreaseButton != null)
            {
                _scrollDecreaseButton.Click += OnScrollDecreaseButtonClick;
                SetLocalizedButtonText(
                    _scrollDecreaseButton,
                    SR_TabViewScrollDecreaseButtonTooltip,
                    SR_TabViewScrollDecreaseButtonTooltip);
            }

            if (_scrollIncreaseButton != null)
            {
                _scrollIncreaseButton.Click += OnScrollIncreaseButtonClick;
                SetLocalizedButtonText(
                    _scrollIncreaseButton,
                    SR_TabViewScrollIncreaseButtonTooltip,
                    SR_TabViewScrollIncreaseButtonTooltip);
            }

            if (_scrollViewer != null)
            {
                _scrollViewer.ScrollChanged += OnScrollViewerScrollChanged;
            }

            PrepareRealizedContainers();
            EnsureSelection();
            UpdateSelectedContent();
            UpdateTabWidths();
            QueueOverflowUpdate(true);
        }

        public DependencyObject ContainerFromItem(object item)
        {
            if (_tabListView == null)
            {
                return item as TabViewItem;
            }

            var container = _tabListView.ItemContainerGenerator.ContainerFromItem(item);
            return ResolveTabViewItem(container) ?? item as TabViewItem;
        }

        public DependencyObject ContainerFromIndex(int index)
        {
            if (_tabListView == null)
            {
                return GetItemAt(index) as TabViewItem;
            }

            var container = _tabListView.ItemContainerGenerator.ContainerFromIndex(index);
            return ResolveTabViewItem(container) ?? GetItemAt(index) as TabViewItem;
        }

        protected override AutomationPeer OnCreateAutomationPeer()
        {
            return new TabViewAutomationPeer(this);
        }

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            base.OnPreviewKeyDown(e);

            if (e.Handled)
            {
                return;
            }

            e.Handled = ProcessKeyboardShortcut(e.Key, Keyboard.Modifiers);
        }

        internal bool ProcessKeyboardShortcut(Key key, ModifierKeys modifiers)
        {
            if ((modifiers & ModifierKeys.Control) == 0)
            {
                return false;
            }

            if (key == Key.Tab)
            {
                SelectRelative((modifiers & ModifierKeys.Shift) == 0 ? 1 : -1, true);
                return true;
            }

            if (key == Key.F4 && SelectedTab?.IsClosable == true)
            {
                RequestClose(SelectedTab);
                return true;
            }

            return false;
        }

        internal TabViewItem SelectedTab => ResolveTabViewItemForIndex(SelectedIndex);

        internal void SelectTab(TabViewItem tab)
        {
            if (tab == null || !tab.IsEnabled || tab.Visibility != Visibility.Visible)
            {
                return;
            }

            var index = IndexOfTab(tab);
            if (index >= 0)
            {
                SetSelection(index, GetItemAt(index), tab, true);
            }
        }

        internal void OnContainerSelectionChanged(TabViewItem tab, bool isSelected)
        {
            if (_updatingSelection || !isSelected)
            {
                return;
            }

            SelectTab(tab);
        }

        internal void RequestClose(TabViewItem tab)
        {
            if (tab == null || !tab.IsClosable)
            {
                return;
            }

            var index = IndexOfTab(tab);
            if (index < 0)
            {
                return;
            }

            var args = new TabViewTabCloseRequestedEventArgs(GetItemAt(index), tab);
            TabCloseRequested?.Invoke(this, args);
            tab.RaiseCloseRequested(args);
        }

        internal bool MoveFocus(bool moveForward)
        {
            var focusOrder = new List<Control>();
            for (var index = 0; index < GetItemCount(); index++)
            {
                var tab = ResolveTabViewItemForIndex(index);
                if (!IsFocusable(tab))
                {
                    continue;
                }

                focusOrder.Add(tab);
                if (tab.IsCloseButtonFocusable)
                {
                    focusOrder.Add(tab.CloseButton);
                }
            }

            if (IsFocusable(_addButton))
            {
                focusOrder.Add((Control)_addButton);
            }

            if (focusOrder.Count == 0)
            {
                return false;
            }

            var focusedIndex = focusOrder.FindIndex(control =>
                ReferenceEquals(Keyboard.FocusedElement, control));
            if (focusedIndex < 0)
            {
                focusedIndex = focusOrder.FindIndex(control => control.IsKeyboardFocusWithin);
            }
            if (focusedIndex < 0)
            {
                return false;
            }

            var nextIndex = (focusedIndex + (moveForward ? 1 : -1) + focusOrder.Count) % focusOrder.Count;
            var nextControl = focusOrder[nextIndex];
            var originalIsTabStop = nextControl.IsTabStop;
            try
            {
                nextControl.IsTabStop = true;
                return nextControl.Focus();
            }
            finally
            {
                nextControl.IsTabStop = originalIsTabStop;
            }
        }

        internal void UpdateTabSeparators()
        {
            var count = GetItemCount();
            for (var index = 0; index < count; index++)
            {
                var tab = ResolveTabViewItemForIndex(index);
                if (tab == null)
                {
                    continue;
                }

                var nextTab = index + 1 < count
                    ? ResolveTabViewItemForIndex(index + 1)
                    : null;
                var shouldHide =
                    index == SelectedIndex ||
                    index + 1 == SelectedIndex ||
                    tab.IsMouseOver ||
                    nextTab?.IsMouseOver == true;
                tab.SetSeparatorOpacity(shouldHide ? 0.0 : 1.0);
            }
        }

        internal void PrepareRealizedContainers()
        {
            if (_preparingContainers || _tabListView == null)
            {
                return;
            }

            _preparingContainers = true;
            try
            {
                var count = GetItemCount();
                for (var index = 0; index < count; index++)
                {
                    var item = GetItemAt(index);
                    var tab = ResolveTabViewItemForIndex(index);
                    if (tab == null)
                    {
                        continue;
                    }

                    tab.Owner = this;
                    tab.Item = item;

                    _updatingSelection = true;
                    try
                    {
                        tab.IsSelected = index == SelectedIndex;
                    }
                    finally
                    {
                        _updatingSelection = false;
                    }

                    tab.UpdateVisualState(false);
                }

                UpdateTabSeparators();
                UpdateSelectedContent();
                UpdateTabWidths();
            }
            finally
            {
                _preparingContainers = false;
            }
        }

        internal void ClearRealizedContainer(DependencyObject element, object item)
        {
            var tab = ResolveTabViewItem(element);
            if (tab != null && ReferenceEquals(tab.Owner, this))
            {
                tab.Owner = null;
                tab.Item = null;
            }
        }

        internal void OnTabHeaderChanged(TabViewItem tab)
        {
            if (ReferenceEquals(tab, SelectedTab))
            {
                UpdateSelectedContent();
            }
        }

        private static void OnLayoutPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var tabView = (TabView)d;
            tabView.PrepareRealizedContainers();
            tabView.UpdateTabWidths();
        }

        private static void OnItemTemplatePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var tabView = (TabView)d;
            tabView.ApplyItemTemplate();
            tabView.Dispatcher.BeginInvoke(DispatcherPriority.Loaded, new Action(tabView.PrepareRealizedContainers));
        }

        private static void OnTabItemsSourcePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var tabView = (TabView)d;
            tabView.UnsubscribeFromItemsSource();
            tabView.SubscribeToItemsSource(e.NewValue as INotifyCollectionChanged);
            tabView.RefreshItemsSource();
            tabView.OnEffectiveItemsCollectionChanged(
                tabView,
                new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        private static void OnSelectedIndexPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var tabView = (TabView)d;
            if (!tabView._updatingSelection)
            {
                tabView.SelectIndexFromProperty((int)e.NewValue);
            }
        }

        private static void OnSelectedItemPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var tabView = (TabView)d;
            if (!tabView._updatingSelection)
            {
                var index = e.NewValue == null ? -1 : tabView.IndexOfItem(e.NewValue);
                tabView.SetSelection(index, index >= 0 ? tabView.GetItemAt(index) : null, tabView.ResolveTabViewItemForIndex(index), true);
            }
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            PrepareRealizedContainers();
            EnsureSelection();
        }

        private void SubscribeToItemsSource(INotifyCollectionChanged notifier)
        {
            _itemsSourceNotifier = notifier;
            if (_itemsSourceNotifier != null)
            {
                CollectionChangedEventManager.AddHandler(
                    _itemsSourceNotifier,
                    OnEffectiveItemsCollectionChanged);
            }
        }

        private void UnsubscribeFromItemsSource()
        {
            if (_itemsSourceNotifier != null)
            {
                CollectionChangedEventManager.RemoveHandler(
                    _itemsSourceNotifier,
                    OnEffectiveItemsCollectionChanged);
                _itemsSourceNotifier = null;
            }
        }

        private void OnEffectiveItemsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            TabItemsChanged?.Invoke(this, e);
            RaiseTabItemsAutomationChanged(e);

            if (_reordering)
            {
                return;
            }

            if (TabItemsSource == null)
            {
                RefreshItemsSource();
            }

            var selectedItem = SelectedItem;
            var selectedIndex = selectedItem == null ? -1 : IndexOfItem(selectedItem);
            if (selectedIndex < 0)
            {
                var candidateIndex = e.OldStartingIndex >= 0 ? e.OldStartingIndex : SelectedIndex;
                if (candidateIndex >= GetItemCount())
                {
                    candidateIndex = GetItemCount() - 1;
                }

                selectedIndex = FindSelectableIndex(candidateIndex, 1, true);
            }

            if (selectedIndex < 0 && GetItemCount() > 0)
            {
                selectedIndex = FindSelectableIndex(0, 1, false);
            }

            SetSelection(
                selectedIndex,
                selectedIndex >= 0 ? GetItemAt(selectedIndex) : null,
                ResolveTabViewItemForIndex(selectedIndex),
                true);

            Dispatcher.BeginInvoke(DispatcherPriority.Loaded, new Action(PrepareRealizedContainers));
        }

        private void RaiseTabItemsAutomationChanged(NotifyCollectionChangedEventArgs e)
        {
            var peer = UIElementAutomationPeer.FromElement(this);
            peer?.RaiseAutomationEvent(AutomationEvents.StructureChanged);
#if NETCOREAPP
            if (peer != null && e.Action == NotifyCollectionChangedAction.Add)
            {
                peer.RaiseNotificationEvent(
                    AutomationNotificationKind.ItemAdded,
                    AutomationNotificationProcessing.MostRecent,
                    ResourceAccessor.GetLocalizedStringResource(SR_TabViewNewTabAddedNotification),
                    "TabViewItemAdded");
            }
#endif
        }

        private void RefreshItemsSource()
        {
            if (_tabListView == null)
            {
                return;
            }

            _tabListView.ItemsSource = TabItemsSource ?? TabItems;
        }

        private void ApplyItemTemplate()
        {
            if (_tabListView == null)
            {
                return;
            }

            _tabListView.ItemTemplate = TabItemTemplate;
            _tabListView.ItemTemplateSelector = TabItemTemplateSelector;
        }

        private void EnsureSelection()
        {
            var count = GetItemCount();
            if (count == 0)
            {
                SetSelection(-1, null, null, SelectedItem != null || SelectedIndex != -1);
                return;
            }

            var index = SelectedItem == null ? SelectedIndex : IndexOfItem(SelectedItem);
            if (!IsSelectableIndex(index))
            {
                index = FindSelectableIndex(Math.Max(0, index), 1, true);
            }

            SetSelection(index, index >= 0 ? GetItemAt(index) : null, ResolveTabViewItemForIndex(index), false);
        }

        private void SelectIndexFromProperty(int index)
        {
            if (index < 0 || index >= GetItemCount())
            {
                SetSelection(-1, null, null, true);
                return;
            }

            if (!IsSelectableIndex(index))
            {
                index = FindSelectableIndex(index, 1, true);
            }

            SetSelection(index, index >= 0 ? GetItemAt(index) : null, ResolveTabViewItemForIndex(index), true);
        }

        private void SetSelection(int index, object item, TabViewItem tab, bool raiseEvent)
        {
            var oldItem = SelectedItem;
            var oldIndex = SelectedIndex;
            if (oldIndex == index && ReferenceEquals(oldItem, item))
            {
                SynchronizeContainerSelection(index);
                UpdateSelectedContent();
                QueueOverflowUpdate(true);
                return;
            }

            _updatingSelection = true;
            try
            {
                SetCurrentValue(SelectedIndexProperty, index);
                SetCurrentValue(SelectedItemProperty, item);
                SynchronizeContainerSelection(index);
            }
            finally
            {
                _updatingSelection = false;
            }

            UpdateSelectedContent();
            UpdateTabWidths();
            QueueOverflowUpdate(true);

            if (tab != null && IsKeyboardFocusWithin)
            {
                tab.Focus();
            }

            if (raiseEvent && !ReferenceEquals(oldItem, item))
            {
                var removed = oldItem == null ? Array.Empty<object>() : new[] { oldItem };
                var added = item == null ? Array.Empty<object>() : new[] { item };
                SelectionChanged?.Invoke(
                    this,
                    new SelectionChangedEventArgs(Selector.SelectionChangedEvent, removed, added));
            }
        }

        private void SynchronizeContainerSelection(int selectedIndex)
        {
            var count = GetItemCount();
            for (var index = 0; index < count; index++)
            {
                var tab = ResolveTabViewItemForIndex(index);
                if (tab != null)
                {
                    tab.IsSelected = index == selectedIndex;
                    tab.UpdateVisualState();
                }
            }

            UpdateTabSeparators();
        }

        private void UpdateSelectedContent()
        {
            if (_tabContentPresenter == null)
            {
                return;
            }

            var tab = SelectedTab;
            _tabContentPresenter.Content = tab?.Content;
            _tabContentPresenter.ContentTemplate = tab?.ContentTemplate;
            _tabContentPresenter.ContentTemplateSelector = tab?.ContentTemplateSelector;
        }

        private void UpdateTabWidths()
        {
            var count = GetItemCount();
            if (count == 0)
            {
                QueueOverflowUpdate();
                return;
            }

            var minimumWidth = GetDoubleResource("TabViewItemMinWidth", 48.0);
            var maximumWidth = GetDoubleResource("TabViewItemMaxWidth", 240.0);
            var compactWidth = GetDoubleResource("TabViewItemHeaderIconSize", 16.0) + 32.0;
            var equalWidth = double.NaN;
            if (TabWidthMode == TabViewWidthMode.Equal && _scrollViewer?.ViewportWidth > 0.0)
            {
                var availableWidth = _scrollViewer.ViewportWidth;
                if (_scrollDecreaseButton?.Visibility == Visibility.Visible)
                {
                    availableWidth += _scrollDecreaseButton.ActualWidth;
                }
                if (_scrollIncreaseButton?.Visibility == Visibility.Visible)
                {
                    availableWidth += _scrollIncreaseButton.ActualWidth;
                }

                equalWidth = Math.Max(minimumWidth, Math.Min(maximumWidth, availableWidth / count));
            }

            for (var index = 0; index < count; index++)
            {
                var tab = ResolveTabViewItemForIndex(index);
                if (tab == null)
                {
                    continue;
                }

                if (TabWidthMode == TabViewWidthMode.Compact && index != SelectedIndex)
                {
                    tab.Width = compactWidth;
                }
                else
                {
                    tab.Width = TabWidthMode == TabViewWidthMode.Equal ? equalWidth : double.NaN;
                }
            }

            QueueOverflowUpdate();
        }

        private double GetDoubleResource(string key, double fallback)
        {
            var value = TryFindResource(key);
            return value is double number && !double.IsNaN(number) && number >= 0.0 ? number : fallback;
        }

        private void SelectRelative(int delta, bool focus)
        {
            var count = GetItemCount();
            if (count == 0)
            {
                return;
            }

            var index = SelectedIndex;
            for (var attempt = 0; attempt < count; attempt++)
            {
                index = (index + delta + count) % count;
                if (IsSelectableIndex(index))
                {
                    var tab = ResolveTabViewItemForIndex(index);
                    SetSelection(index, GetItemAt(index), tab, true);
                    if (focus)
                    {
                        tab?.Focus();
                    }
                    return;
                }
            }
        }

        private int FindSelectableIndex(int start, int delta, bool searchBothDirections)
        {
            var count = GetItemCount();
            if (count == 0)
            {
                return -1;
            }

            start = Math.Max(0, Math.Min(start, count - 1));
            for (var index = start; index >= 0 && index < count; index += delta)
            {
                if (IsSelectableIndex(index))
                {
                    return index;
                }
            }

            if (searchBothDirections)
            {
                for (var index = start - delta; index >= 0 && index < count; index -= delta)
                {
                    if (IsSelectableIndex(index))
                    {
                        return index;
                    }
                }
            }

            return -1;
        }

        private bool IsSelectableIndex(int index)
        {
            if (index < 0 || index >= GetItemCount())
            {
                return false;
            }

            var item = GetItemAt(index);
            var tab = ResolveTabViewItemForIndex(index) ?? item as TabViewItem;
            return tab == null || (tab.IsEnabled && tab.Visibility == Visibility.Visible);
        }

        private int IndexOfItem(object item)
        {
            var items = GetItemsSnapshot();
            for (var index = 0; index < items.Count; index++)
            {
                if (ReferenceEquals(items[index], item) || Equals(items[index], item))
                {
                    return index;
                }
            }

            return -1;
        }

        private int IndexOfTab(TabViewItem tab)
        {
            var count = GetItemCount();
            for (var index = 0; index < count; index++)
            {
                if (ReferenceEquals(ResolveTabViewItemForIndex(index), tab))
                {
                    return index;
                }
            }

            return -1;
        }

        private int GetItemCount()
        {
            if (_tabListView != null)
            {
                return _tabListView.Items.Count;
            }

            return GetItemsSnapshot().Count;
        }

        private object GetItemAt(int index)
        {
            if (index < 0)
            {
                return null;
            }

            if (_tabListView != null && index < _tabListView.Items.Count)
            {
                return _tabListView.Items[index];
            }

            var items = GetItemsSnapshot();
            return index < items.Count ? items[index] : null;
        }

        private List<object> GetItemsSnapshot()
        {
            var result = new List<object>();
            var source = TabItemsSource ?? TabItems;
            if (source != null)
            {
                foreach (var item in source)
                {
                    result.Add(item);
                }
            }

            return result;
        }

        private TabViewItem ResolveTabViewItemForIndex(int index)
        {
            if (index < 0)
            {
                return null;
            }

            var item = GetItemAt(index);
            if (item is TabViewItem tabItem)
            {
                return tabItem;
            }

            var container = _tabListView?.ItemContainerGenerator.ContainerFromIndex(index);
            return ResolveTabViewItem(container);
        }

        private static TabViewItem ResolveTabViewItem(DependencyObject root)
        {
            if (root is TabViewItem tabItem)
            {
                return tabItem;
            }

            if (root == null)
            {
                return null;
            }

            var count = VisualTreeHelper.GetChildrenCount(root);
            for (var index = 0; index < count; index++)
            {
                var result = ResolveTabViewItem(VisualTreeHelper.GetChild(root, index));
                if (result != null)
                {
                    return result;
                }
            }

            return null;
        }

        private void OnGeneratorStatusChanged(object sender, EventArgs e)
        {
            if (_tabListView?.ItemContainerGenerator.Status == GeneratorStatus.ContainersGenerated)
            {
                Dispatcher.BeginInvoke(DispatcherPriority.Loaded, new Action(PrepareRealizedContainers));
            }
        }

        private void OnAddButtonClick(object sender, RoutedEventArgs e)
        {
            AddTabButtonClick?.Invoke(this, null);
        }

        private void OnAddButtonPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Handled || (e.Key != Key.Left && e.Key != Key.Right))
            {
                return;
            }

            var moveForward =
                (FlowDirection == FlowDirection.LeftToRight && e.Key == Key.Right) ||
                (FlowDirection == FlowDirection.RightToLeft && e.Key == Key.Left);
            e.Handled = MoveFocus(moveForward);
        }

        private void OnScrollDecreaseButtonClick(object sender, RoutedEventArgs e)
        {
            if (_scrollViewer != null)
            {
                _scrollViewer.ScrollToHorizontalOffset(
                    Math.Max(0.0, _scrollViewer.HorizontalOffset - ScrollAmount));
            }
        }

        private void OnScrollIncreaseButtonClick(object sender, RoutedEventArgs e)
        {
            if (_scrollViewer != null)
            {
                _scrollViewer.ScrollToHorizontalOffset(
                    Math.Min(_scrollViewer.ScrollableWidth, _scrollViewer.HorizontalOffset + ScrollAmount));
            }
        }

        private void OnScrollViewerScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            UpdateOverflowButtons();
        }

        private void OnTabStripDragOver(object sender, DragEventArgs e)
        {
            if (TryGetDragPayload(e.Data, out var payload))
            {
                if (ReferenceEquals(payload.Source, this))
                {
                    e.Effects = CanReorderTabs ? DragDropEffects.Move : DragDropEffects.None;
                }
                else
                {
                    e.Effects = CanTearOutTabs ? DragDropEffects.Move : DragDropEffects.None;
                }

                e.Handled = e.Effects != DragDropEffects.None;
            }

            if (!AllowDropTabs)
            {
                e.Effects = DragDropEffects.None;
            }

            TabStripDragOver?.Invoke(this, e);
        }

        private void OnTabStripDrop(object sender, DragEventArgs e)
        {
            if (AllowDropTabs && TryGetDragPayload(e.Data, out var payload))
            {
                var dropIndex = GetDropIndex(e.GetPosition(_tabListView));
                if (ReferenceEquals(payload.Source, this))
                {
                    var oldIndex = IndexOfItem(payload.Item);
                    if (oldIndex >= 0 && dropIndex > oldIndex)
                    {
                        dropIndex--;
                    }

                    if (dropIndex >= GetItemCount())
                    {
                        dropIndex = GetItemCount() - 1;
                    }

                    e.Handled = ReorderTab(oldIndex, dropIndex);
                }
                else
                {
                    e.Handled = CompleteExternalDrop(
                        new[] { payload.Item },
                        new UIElement[] { payload.Tab },
                        dropIndex);
                }

                if (e.Handled)
                {
                    e.Effects = DragDropEffects.Move;
                }
            }

            TabStripDrop?.Invoke(this, e);
        }

        private int GetDropIndex(Point position)
        {
            var count = GetItemCount();
            for (var index = 0; index < count; index++)
            {
                var tab = ResolveTabViewItemForIndex(index);
                if (tab == null)
                {
                    continue;
                }

                var tabPosition = tab.TranslatePoint(new Point(), _tabListView);
                if (position.X < tabPosition.X + tab.ActualWidth / 2.0)
                {
                    return index;
                }
            }

            return count;
        }

        private static bool TryGetDragPayload(IDataObject data, out TabViewDragPayload payload)
        {
            payload = data?.GetDataPresent(DragPayloadFormat) == true
                ? data.GetData(DragPayloadFormat) as TabViewDragPayload
                : null;
            return payload != null;
        }

        private static Point GetCurrentScreenPosition(TabViewItem tab)
        {
            try
            {
                return tab.PointToScreen(Mouse.GetPosition(tab));
            }
            catch (InvalidOperationException)
            {
                return new Point(double.NaN, double.NaN);
            }
        }

        private void UnhookTemplateParts()
        {
            if (_tabListView != null)
            {
                _tabListView.ItemContainerGenerator.StatusChanged -= OnGeneratorStatusChanged;
                _tabListView.RemoveHandler(UIElement.DragOverEvent, new DragEventHandler(OnTabStripDragOver));
                _tabListView.RemoveHandler(UIElement.DropEvent, new DragEventHandler(OnTabStripDrop));
                _tabListView.Owner = null;
            }

            if (_addButton != null)
            {
                _addButton.Click -= OnAddButtonClick;
                _addButton.PreviewKeyDown -= OnAddButtonPreviewKeyDown;
            }

            if (_scrollDecreaseButton != null)
            {
                _scrollDecreaseButton.Click -= OnScrollDecreaseButtonClick;
            }

            if (_scrollIncreaseButton != null)
            {
                _scrollIncreaseButton.Click -= OnScrollIncreaseButtonClick;
            }

            if (_scrollViewer != null)
            {
                _scrollViewer.ScrollChanged -= OnScrollViewerScrollChanged;
            }
        }

        private static bool IsFocusable(Control control)
        {
            return control != null &&
                control.IsEnabled &&
                control.Visibility == Visibility.Visible &&
                control.Focusable;
        }

        private static void SetLocalizedButtonText(ButtonBase button, string nameResource, string tooltipResource)
        {
            var name = ResourceAccessor.GetLocalizedStringResource(nameResource);
            if (string.IsNullOrEmpty(AutomationProperties.GetName(button)))
            {
                AutomationProperties.SetName(button, name);
            }

            if (ToolTipService.GetToolTip(button) == null)
            {
                ToolTipService.SetToolTip(
                    button,
                    ResourceAccessor.GetLocalizedStringResource(tooltipResource));
            }
        }

        private void QueueOverflowUpdate(bool bringSelectedTabIntoView = false)
        {
            _bringSelectedTabPending |= bringSelectedTabIntoView;
            if (_overflowUpdatePending)
            {
                return;
            }

            _overflowUpdatePending = true;
            Dispatcher.BeginInvoke(DispatcherPriority.Loaded, new Action(() =>
            {
                _overflowUpdatePending = false;
                UpdateOverflowButtons();
                if (_bringSelectedTabPending)
                {
                    _bringSelectedTabPending = false;
                    BringSelectedTabIntoView();
                    UpdateOverflowButtons();
                }
            }));
        }

        private void UpdateOverflowButtons()
        {
            if (_scrollViewer == null)
            {
                return;
            }

            var hasOverflow = _scrollViewer.ScrollableWidth > ScrollThreshold;
            if (TabWidthMode == TabViewWidthMode.Equal && GetItemCount() > 0)
            {
                var availableWidth = _scrollViewer.ViewportWidth;
                if (_scrollDecreaseButton?.Visibility == Visibility.Visible)
                {
                    availableWidth += _scrollDecreaseButton.ActualWidth;
                }
                if (_scrollIncreaseButton?.Visibility == Visibility.Visible)
                {
                    availableWidth += _scrollIncreaseButton.ActualWidth;
                }

                hasOverflow = GetItemCount() * GetDoubleResource("TabViewItemMinWidth", 48.0) >
                    availableWidth + ScrollThreshold;
            }

            var visibility = hasOverflow ? Visibility.Visible : Visibility.Collapsed;
            var visibilityChanged =
                _scrollDecreaseButton?.Visibility != visibility ||
                _scrollIncreaseButton?.Visibility != visibility;
            if (_scrollDecreaseButton != null)
            {
                _scrollDecreaseButton.Visibility = visibility;
                _scrollDecreaseButton.IsEnabled = hasOverflow &&
                    _scrollViewer.HorizontalOffset > ScrollThreshold;
            }

            if (_scrollIncreaseButton != null)
            {
                _scrollIncreaseButton.Visibility = visibility;
                _scrollIncreaseButton.IsEnabled = hasOverflow &&
                    _scrollViewer.HorizontalOffset < _scrollViewer.ScrollableWidth - ScrollThreshold;
            }

            if (visibilityChanged)
            {
                Dispatcher.BeginInvoke(DispatcherPriority.Loaded, new Action(UpdateTabWidths));
            }
        }

        private void BringSelectedTabIntoView()
        {
            var selectedTab = SelectedTab;
            if (selectedTab != null && _scrollViewer != null)
            {
                selectedTab.BringIntoView(new Rect(0.0, 0.0, selectedTab.ActualWidth, selectedTab.ActualHeight));
            }
        }

        private sealed class TabViewDragPayload
        {
            public TabViewDragPayload(TabView source, object item, TabViewItem tab)
            {
                Source = source;
                Item = item;
                Tab = tab;
            }

            public TabView Source { get; }

            public object Item { get; }

            public TabViewItem Tab { get; }
        }
    }
}
