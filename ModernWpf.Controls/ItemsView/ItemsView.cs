// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using ModernWpf.Automation.Peers;
using ModernWpf.Controls.Primitives;

namespace ModernWpf.Controls
{
    [TemplatePart(Name = ItemsRepeaterPartName, Type = typeof(ItemsRepeater))]
    [TemplatePart(Name = ScrollHostPartName, Type = typeof(ItemsRepeaterScrollHost))]
    [TemplatePart(Name = ScrollViewPartName, Type = typeof(ScrollViewer))]
    public class ItemsView : Control
    {
        internal const string ItemsRepeaterPartName = "PART_ItemsRepeater";
        internal const string ScrollHostPartName = "PART_ScrollHost";
        internal const string ScrollViewPartName = "PART_ScrollView";

        private static readonly DependencyPropertyDescriptor IsSelectedPropertyDescriptor =
            DependencyPropertyDescriptor.FromProperty(ItemContainer.IsSelectedProperty, typeof(ItemContainer));

        private readonly ItemsViewElementFactory _defaultItemTemplate = new ItemsViewElementFactory();
        private readonly HashSet<ItemContainer> _preparedItemContainers = new HashSet<ItemContainer>();
        private readonly SelectionModel _selectionModel = new SelectionModel();
        private readonly SelectionModel _currentElementSelectionModel = new SelectionModel();

        private ItemsSourceView _itemsSourceView;
        private ItemsRepeater _itemsRepeater;
        private ItemsRepeaterScrollHost _scrollHost;
        private ItemsViewScrollHost _scrollView;
        private bool _ensuringItemTemplate;
        private bool _movingKeyboardFocus;
        private bool _updatingContainerSelection;

        static ItemsView()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(ItemsView),
                new FrameworkPropertyMetadata(typeof(ItemsView)));
        }

        public ItemsView()
        {
            _selectionModel.SelectionChanged += OnSelectionModelSelectionChanged;
            _currentElementSelectionModel.SingleSelect = true;
            _currentElementSelectionModel.SelectionChanged += OnCurrentElementSelectionChanged;
            UpdateSelectionMode();
        }

        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register(
                nameof(ItemsSource),
                typeof(object),
                typeof(ItemsView),
                new FrameworkPropertyMetadata(null, OnItemsSourceChanged));

        public object ItemsSource
        {
            get => GetValue(ItemsSourceProperty);
            set => SetValue(ItemsSourceProperty, value);
        }

        public static readonly DependencyProperty ItemTemplateProperty =
            DependencyProperty.Register(
                nameof(ItemTemplate),
                typeof(object),
                typeof(ItemsView),
                new FrameworkPropertyMetadata(null, OnItemTemplateChanged));

        public object ItemTemplate
        {
            get => GetValue(ItemTemplateProperty);
            set => SetValue(ItemTemplateProperty, value);
        }

        public static readonly DependencyProperty LayoutProperty =
            DependencyProperty.Register(
                nameof(Layout),
                typeof(Layout),
                typeof(ItemsView),
                new FrameworkPropertyMetadata(null, OnLayoutChanged));

        public Layout Layout
        {
            get => (Layout)GetValue(LayoutProperty);
            set => SetValue(LayoutProperty, value);
        }

        public static readonly DependencyProperty ItemTransitionProviderProperty =
            DependencyProperty.Register(
                nameof(ItemTransitionProvider),
                typeof(ItemCollectionTransitionProvider),
                typeof(ItemsView),
                new FrameworkPropertyMetadata(null, OnItemTransitionProviderChanged));

        public ItemCollectionTransitionProvider ItemTransitionProvider
        {
            get => (ItemCollectionTransitionProvider)GetValue(ItemTransitionProviderProperty);
            set => SetValue(ItemTransitionProviderProperty, value);
        }

        public static readonly DependencyProperty IsItemInvokedEnabledProperty =
            DependencyProperty.Register(
                nameof(IsItemInvokedEnabled),
                typeof(bool),
                typeof(ItemsView),
                new FrameworkPropertyMetadata(false, OnIsItemInvokedEnabledChanged));

        public bool IsItemInvokedEnabled
        {
            get => (bool)GetValue(IsItemInvokedEnabledProperty);
            set => SetValue(IsItemInvokedEnabledProperty, value);
        }

        public static readonly DependencyProperty SelectionModeProperty =
            DependencyProperty.Register(
                nameof(SelectionMode),
                typeof(ItemsViewSelectionMode),
                typeof(ItemsView),
                new FrameworkPropertyMetadata(ItemsViewSelectionMode.Single, OnSelectionModeChanged),
                IsValidSelectionMode);

        public ItemsViewSelectionMode SelectionMode
        {
            get => (ItemsViewSelectionMode)GetValue(SelectionModeProperty);
            set => SetValue(SelectionModeProperty, value);
        }

        public static readonly DependencyProperty VerticalScrollControllerProperty =
            DependencyProperty.Register(
                nameof(VerticalScrollController),
                typeof(IScrollController),
                typeof(ItemsView),
                new FrameworkPropertyMetadata(null, OnVerticalScrollControllerChanged));

        public IScrollController VerticalScrollController
        {
            get => (IScrollController)GetValue(VerticalScrollControllerProperty);
            set => SetValue(VerticalScrollControllerProperty, value);
        }

        public static readonly DependencyProperty CornerRadiusProperty =
            Border.CornerRadiusProperty.AddOwner(
                typeof(ItemsView),
                new FrameworkPropertyMetadata(new CornerRadius(4.0)));

        public CornerRadius CornerRadius
        {
            get => (CornerRadius)GetValue(CornerRadiusProperty);
            set => SetValue(CornerRadiusProperty, value);
        }

        private static readonly DependencyPropertyKey ScrollViewPropertyKey =
            DependencyProperty.RegisterReadOnly(
                nameof(ScrollView),
                typeof(ScrollViewer),
                typeof(ItemsView),
                new FrameworkPropertyMetadata(null));

        public static readonly DependencyProperty ScrollViewProperty = ScrollViewPropertyKey.DependencyProperty;

        public ScrollViewer ScrollView => (ScrollViewer)GetValue(ScrollViewProperty);

        private static readonly DependencyPropertyKey CurrentItemIndexPropertyKey =
            DependencyProperty.RegisterReadOnly(
                nameof(CurrentItemIndex),
                typeof(int),
                typeof(ItemsView),
                new FrameworkPropertyMetadata(-1));

        public static readonly DependencyProperty CurrentItemIndexProperty = CurrentItemIndexPropertyKey.DependencyProperty;

        public int CurrentItemIndex => (int)GetValue(CurrentItemIndexProperty);

        private static readonly DependencyPropertyKey SelectedItemPropertyKey =
            DependencyProperty.RegisterReadOnly(
                nameof(SelectedItem),
                typeof(object),
                typeof(ItemsView),
                new FrameworkPropertyMetadata(null));

        public static readonly DependencyProperty SelectedItemProperty = SelectedItemPropertyKey.DependencyProperty;

        public object SelectedItem => GetValue(SelectedItemProperty);

        public IReadOnlyList<object> SelectedItems => _selectionModel.SelectedItems;

        public event TypedEventHandler<ItemsView, ItemsViewItemInvokedEventArgs> ItemInvoked;

        public event TypedEventHandler<ItemsView, ItemsViewSelectionChangedEventArgs> SelectionChanged;

        public override void OnApplyTemplate()
        {
            DetachTemplateParts();
            base.OnApplyTemplate();

            _scrollHost = GetTemplateChild(ScrollHostPartName) as ItemsRepeaterScrollHost;
            _scrollView = GetTemplateChild(ScrollViewPartName) as ItemsViewScrollHost;
            _itemsRepeater = GetTemplateChild(ItemsRepeaterPartName) as ItemsRepeater;

            SetValue(ScrollViewPropertyKey, _scrollView);

            if (_scrollView != null)
            {
                _scrollView.VerticalScrollController = VerticalScrollController;
            }

            EnsureItemTemplate();

            if (_itemsRepeater != null)
            {
                _itemsRepeater.ElementPrepared += OnItemsRepeaterElementPrepared;
                _itemsRepeater.ElementClearing += OnItemsRepeaterElementClearing;
                _itemsRepeater.ElementIndexChanged += OnItemsRepeaterElementIndexChanged;
                _itemsRepeater.ItemsSource = ItemsSource;
                _itemsRepeater.ItemTemplate = ItemTemplate;
                _itemsRepeater.Layout = Layout;
                _itemsRepeater.ItemTransitionProvider = ItemTransitionProvider;
            }
        }

        public bool TryGetItemIndex(
            double horizontalViewportRatio,
            double verticalViewportRatio,
            out int index)
        {
            index = -1;
            if (_itemsRepeater == null || _scrollView == null)
            {
                return false;
            }

            double horizontalRatio = NormalizeViewportRatio(horizontalViewportRatio);
            double verticalRatio = NormalizeViewportRatio(verticalViewportRatio);
            Size viewportSize = _scrollView.GetEffectiveViewportSize();
            var target = new Point(
                horizontalRatio * viewportSize.Width,
                verticalRatio * viewportSize.Height);
            double smallestDistance = double.MaxValue;

            foreach (ItemContainer itemContainer in _preparedItemContainers)
            {
                int itemIndex = _itemsRepeater.GetElementIndex(itemContainer);
                if (itemIndex < 0 || !itemContainer.IsVisible)
                {
                    continue;
                }

                Rect bounds;
                try
                {
                    bounds = itemContainer.TransformToVisual(_scrollView).TransformBounds(
                        new Rect(new Point(), itemContainer.RenderSize));
                }
                catch (InvalidOperationException)
                {
                    continue;
                }

                double horizontalDistance = target.X < bounds.Left
                    ? bounds.Left - target.X
                    : target.X > bounds.Right ? target.X - bounds.Right : 0.0;
                double verticalDistance = target.Y < bounds.Top
                    ? bounds.Top - target.Y
                    : target.Y > bounds.Bottom ? target.Y - bounds.Bottom : 0.0;
                double distance = horizontalDistance * horizontalDistance + verticalDistance * verticalDistance;

                if (distance < smallestDistance || (distance == smallestDistance && itemIndex < index))
                {
                    smallestDistance = distance;
                    index = itemIndex;
                }
            }

            return index >= 0;
        }

        public void StartBringItemIntoView(int index, BringIntoViewOptions options)
        {
            ValidateItemIndex(index);
            if (_itemsRepeater == null)
            {
                throw new InvalidOperationException("The ItemsRepeater template part is not available.");
            }

            FrameworkElement element = _itemsRepeater.GetOrCreateElement(index) as FrameworkElement ??
                throw new InvalidOperationException("The realized item must derive from FrameworkElement.");
            element.UpdateLayout();

            if (_scrollHost != null && options != null)
            {
                _scrollHost.StartBringIntoView(
                    element,
                    options.HorizontalAlignmentRatio,
                    options.VerticalAlignmentRatio,
                    options.HorizontalOffset,
                    options.VerticalOffset,
                    options.AnimationDesired,
                    options.TargetRect);
                _scrollHost.InvalidateArrange();
                _scrollHost.UpdateLayout();
            }
            else if (options?.TargetRect is Rect targetRect)
            {
                element.BringIntoView(targetRect);
            }
            else
            {
                element.BringIntoView();
            }
        }

        public void Select(int itemIndex)
        {
            ValidateItemIndex(itemIndex);
            _selectionModel.Select(itemIndex);
        }

        public void Deselect(int itemIndex)
        {
            ValidateItemIndex(itemIndex);
            _selectionModel.Deselect(itemIndex);
        }

        public bool IsSelected(int itemIndex)
        {
            ValidateItemIndex(itemIndex);
            return _selectionModel.IsSelected(itemIndex) == true;
        }

        public void SelectAll()
        {
            _selectionModel.SelectAllFlat();
        }

        public void DeselectAll()
        {
            _selectionModel.ClearSelection();
        }

        public void InvertSelection()
        {
            if (_itemsSourceView == null)
            {
                return;
            }

            IReadOnlyList<IndexPath> selectedIndices = _selectionModel.SelectedIndices;
            int indexEnd = _itemsSourceView.Count - 1;
            for (int selectedIndex = selectedIndices.Count - 1; selectedIndex >= 0; selectedIndex--)
            {
                IndexPath path = selectedIndices[selectedIndex];
                int index = path.GetAt(0);
                if (index < indexEnd)
                {
                    _selectionModel.SelectRange(new IndexPath(index + 1), new IndexPath(indexEnd));
                }

                _selectionModel.DeselectAt(path);
                indexEnd = index - 1;
            }

            if (indexEnd >= 0)
            {
                _selectionModel.SelectRange(new IndexPath(0), new IndexPath(indexEnd));
            }
        }

        protected override AutomationPeer OnCreateAutomationPeer()
        {
            return new ItemsViewAutomationPeer(this);
        }

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            base.OnPreviewKeyDown(e);
            if (e.Handled || _itemsSourceView == null || _itemsSourceView.Count == 0)
            {
                return;
            }

            if (ProcessKeyboardShortcut(e.Key, Keyboard.Modifiers))
            {
                e.Handled = true;
                return;
            }

            if (!IsNavigationKey(e.Key))
            {
                return;
            }

            int currentIndex = CurrentItemIndex;
            if (currentIndex < 0)
            {
                ItemContainer focusedContainer = FindItemContainer(Keyboard.FocusedElement as DependencyObject);
                currentIndex = focusedContainer == null || _itemsRepeater == null
                    ? -1
                    : _itemsRepeater.GetElementIndex(focusedContainer);
            }

            if (currentIndex < 0)
            {
                currentIndex = e.Key == Key.End ? _itemsSourceView.Count - 1 : 0;
            }

            int targetIndex = GetNavigationTarget(currentIndex, e.Key);
            if (targetIndex < 0 || targetIndex >= _itemsSourceView.Count)
            {
                return;
            }

            StartBringItemIntoView(targetIndex, null);
            ItemContainer targetContainer = GetRealizedItemContainer(targetIndex);
            if (targetContainer == null)
            {
                return;
            }

            _movingKeyboardFocus = true;
            try
            {
                SetCurrentItemIndex(targetIndex);
                targetContainer.Focus();
                ApplyFocusedSelectionPolicy(targetIndex, Keyboard.Modifiers);
            }
            finally
            {
                _movingKeyboardFocus = false;
            }

            e.Handled = true;
        }

        internal bool ProcessKeyboardShortcut(Key key, ModifierKeys modifiers)
        {
            if (key != Key.A ||
                (modifiers & ModifierKeys.Control) == 0 ||
                _itemsSourceView == null ||
                _itemsSourceView.Count == 0 ||
                (SelectionMode != ItemsViewSelectionMode.Multiple &&
                    SelectionMode != ItemsViewSelectionMode.Extended))
            {
                return false;
            }

            SelectAll();
            return true;
        }

        internal IReadOnlyList<IndexPath> SelectedIndices => _selectionModel.SelectedIndices;

        internal ItemContainer GetRealizedItemContainer(int index)
        {
            return _itemsRepeater?.TryGetElement(index) as ItemContainer;
        }

        private static bool IsValidSelectionMode(object value)
        {
            var mode = (ItemsViewSelectionMode)value;
            return mode >= ItemsViewSelectionMode.None && mode <= ItemsViewSelectionMode.Extended;
        }

        private static bool IsNavigationKey(Key key)
        {
            return key == Key.Home || key == Key.End ||
                key == Key.Left || key == Key.Right ||
                key == Key.Up || key == Key.Down ||
                key == Key.PageUp || key == Key.PageDown;
        }

        private static double NormalizeViewportRatio(double value)
        {
            if (double.IsNaN(value))
            {
                return 0.5;
            }

            if (double.IsInfinity(value))
            {
                throw new ArgumentException("The viewport ratio must be finite or NaN.", nameof(value));
            }

            return Math.Max(0.0, Math.Min(1.0, value));
        }

        private static void OnItemsSourceChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            ((ItemsView)sender).OnItemsSourceChanged(args.NewValue);
        }

        private static void OnItemTemplateChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            ((ItemsView)sender).OnItemTemplateChanged();
        }

        private static void OnLayoutChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            var owner = (ItemsView)sender;
            if (owner._itemsRepeater != null)
            {
                owner._itemsRepeater.Layout = (Layout)args.NewValue;
            }
        }

        private static void OnItemTransitionProviderChanged(
            DependencyObject sender,
            DependencyPropertyChangedEventArgs args)
        {
            var owner = (ItemsView)sender;
            if (owner._itemsRepeater != null)
            {
                owner._itemsRepeater.ItemTransitionProvider =
                    (ItemCollectionTransitionProvider)args.NewValue;
            }
        }

        private static void OnIsItemInvokedEnabledChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            ((ItemsView)sender).UpdatePreparedItemContainerModes();
        }

        private static void OnSelectionModeChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            ((ItemsView)sender).UpdateSelectionMode();
        }

        private static void OnVerticalScrollControllerChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            var owner = (ItemsView)sender;
            if (owner._scrollView != null)
            {
                owner._scrollView.VerticalScrollController = (IScrollController)args.NewValue;
            }
        }

        private void OnItemsSourceChanged(object value)
        {
            if (_itemsSourceView != null)
            {
                _itemsSourceView.CollectionChanged -= OnItemsSourceCollectionChanged;
            }

            _itemsSourceView = value == null ? null : new ItemsSourceView(value);
            if (_itemsSourceView != null)
            {
                _itemsSourceView.CollectionChanged += OnItemsSourceCollectionChanged;
            }

            _selectionModel.Source = value;
            _currentElementSelectionModel.Source = value;
            EnsureItemTemplate();

            if (_itemsRepeater != null)
            {
                _itemsRepeater.ItemsSource = value;
                _itemsRepeater.ItemTemplate = ItemTemplate;
            }
        }

        private void OnItemTemplateChanged()
        {
            if (!_ensuringItemTemplate)
            {
                EnsureItemTemplate();
            }

            if (_itemsRepeater != null)
            {
                _itemsRepeater.ItemTemplate = ItemTemplate;
            }
        }

        private void EnsureItemTemplate()
        {
            if (_ensuringItemTemplate || ItemTemplate != null || _itemsSourceView == null || _itemsSourceView.Count == 0)
            {
                return;
            }

            if (_itemsSourceView.GetAt(0) is UIElement)
            {
                return;
            }

            _ensuringItemTemplate = true;
            try
            {
                SetCurrentValue(ItemTemplateProperty, _defaultItemTemplate);
            }
            finally
            {
                _ensuringItemTemplate = false;
            }
        }

        private void OnItemsSourceCollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
        {
            EnsureItemTemplate();
            UpdateAutomationSetMetadata();
        }

        private void OnItemsRepeaterElementPrepared(
            ItemsRepeater sender,
            ItemsRepeaterElementPreparedEventArgs args)
        {
            if (!(args.Element is ItemContainer itemContainer))
            {
                throw new InvalidOperationException("ItemTemplate's root element must be an ItemContainer.");
            }

            ConfigureItemContainer(itemContainer);
            bool selected = _selectionModel.IsSelected(args.Index) == true;
            if (itemContainer.IsSelected && !selected && SelectionMode != ItemsViewSelectionMode.None)
            {
                _selectionModel.Select(args.Index);
                selected = _selectionModel.IsSelected(args.Index) == true;
            }

            SetContainerSelection(itemContainer, selected);
            AttachItemContainer(itemContainer);
#if NET48_OR_NEWER
            AutomationProperties.SetPositionInSet(itemContainer, args.Index + 1);
            AutomationProperties.SetSizeOfSet(itemContainer, _itemsSourceView?.Count ?? 0);
#endif
        }

        private void OnItemsRepeaterElementClearing(
            ItemsRepeater sender,
            ItemsRepeaterElementClearingEventArgs args)
        {
            if (args.Element is ItemContainer itemContainer)
            {
                DetachItemContainer(itemContainer);
                itemContainer.CanUserInvokeInternal = ItemContainerUserInvokeMode.Auto;
                itemContainer.CanUserSelectInternal = ItemContainerUserSelectMode.Auto;
                itemContainer.MultiSelectModeInternal = ItemContainerMultiSelectMode.Auto;
                SetContainerSelection(itemContainer, false);
            }

#if NET48_OR_NEWER
            args.Element.ClearValue(AutomationProperties.PositionInSetProperty);
            args.Element.ClearValue(AutomationProperties.SizeOfSetProperty);
#endif
        }

        private void OnItemsRepeaterElementIndexChanged(
            ItemsRepeater sender,
            ItemsRepeaterElementIndexChangedEventArgs args)
        {
#if NET48_OR_NEWER
            AutomationProperties.SetPositionInSet(args.Element, args.NewIndex + 1);
#endif
        }

        private void AttachItemContainer(ItemContainer itemContainer)
        {
            if (!_preparedItemContainers.Add(itemContainer))
            {
                return;
            }

            itemContainer.ItemInvoked += OnItemContainerInvoked;
            itemContainer.GotKeyboardFocus += OnItemContainerGotKeyboardFocus;
            IsSelectedPropertyDescriptor.AddValueChanged(itemContainer, OnItemContainerIsSelectedChanged);
        }

        private void DetachItemContainer(ItemContainer itemContainer)
        {
            if (!_preparedItemContainers.Remove(itemContainer))
            {
                return;
            }

            itemContainer.ItemInvoked -= OnItemContainerInvoked;
            itemContainer.GotKeyboardFocus -= OnItemContainerGotKeyboardFocus;
            IsSelectedPropertyDescriptor.RemoveValueChanged(itemContainer, OnItemContainerIsSelectedChanged);
        }

        private void ConfigureItemContainer(ItemContainer itemContainer)
        {
            itemContainer.CanUserInvokeInternal = ItemContainerUserInvokeMode.Auto |
                (IsItemInvokedEnabled
                    ? ItemContainerUserInvokeMode.UserCanInvoke
                    : ItemContainerUserInvokeMode.UserCannotInvoke);
            itemContainer.CanUserSelectInternal = ItemContainerUserSelectMode.Auto |
                (SelectionMode == ItemsViewSelectionMode.None
                    ? ItemContainerUserSelectMode.UserCannotSelect
                    : ItemContainerUserSelectMode.UserCanSelect);

            ItemContainerMultiSelectMode multiSelectMode;
            switch (SelectionMode)
            {
                case ItemsViewSelectionMode.Multiple:
                    multiSelectMode = ItemContainerMultiSelectMode.Multiple;
                    break;
                case ItemsViewSelectionMode.Extended:
                    multiSelectMode = ItemContainerMultiSelectMode.Extended;
                    break;
                default:
                    multiSelectMode = ItemContainerMultiSelectMode.Single;
                    break;
            }

            itemContainer.MultiSelectModeInternal = ItemContainerMultiSelectMode.Auto | multiSelectMode;
        }

        private void UpdatePreparedItemContainerModes()
        {
            foreach (ItemContainer itemContainer in _preparedItemContainers)
            {
                ConfigureItemContainer(itemContainer);
            }
        }

        private void OnItemContainerIsSelectedChanged(object sender, EventArgs args)
        {
            if (_updatingContainerSelection || !(sender is ItemContainer itemContainer) || _itemsRepeater == null)
            {
                return;
            }

            int itemIndex = _itemsRepeater.GetElementIndex(itemContainer);
            if (itemIndex < 0)
            {
                return;
            }

            bool modelSelected = _selectionModel.IsSelected(itemIndex) == true;
            if (itemContainer.IsSelected)
            {
                if (!modelSelected)
                {
                    if (SelectionMode == ItemsViewSelectionMode.None)
                    {
                        SetContainerSelection(itemContainer, false);
                    }
                    else
                    {
                        _selectionModel.Select(itemIndex);
                    }
                }
            }
            else if (modelSelected)
            {
                DeselectWithAnchorPreservation(itemIndex);
            }
        }

        private void OnItemContainerGotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs args)
        {
            if (_movingKeyboardFocus || !(sender is ItemContainer itemContainer) || _itemsRepeater == null)
            {
                return;
            }

            int index = _itemsRepeater.GetElementIndex(itemContainer);
            if (index < 0)
            {
                return;
            }

            bool focusWasInside = IsDescendantOf(args.OldFocus as DependencyObject, _itemsRepeater);
            SetCurrentItemIndex(index);
            if (focusWasInside)
            {
                ApplyFocusedSelectionPolicy(index, Keyboard.Modifiers);
            }
        }

        private void OnItemContainerInvoked(ItemContainer sender, ItemContainerInvokedEventArgs args)
        {
            if (_itemsRepeater == null)
            {
                return;
            }

            int index = _itemsRepeater.GetElementIndex(sender);
            if (index < 0)
            {
                return;
            }

            bool handled = false;
            bool raiseItemInvoked = false;
            switch (args.InteractionTrigger)
            {
                case ItemContainerInteractionTrigger.PointerReleased:
                    handled = ProcessInteraction(sender, index, FocusState.Pointer);
                    raiseItemInvoked = SelectionMode == ItemsViewSelectionMode.None;
                    break;
                case ItemContainerInteractionTrigger.DoubleTap:
                    raiseItemInvoked = SelectionMode != ItemsViewSelectionMode.None;
                    break;
                case ItemContainerInteractionTrigger.EnterKey:
                    handled = ProcessInteraction(sender, index, FocusState.Keyboard);
                    raiseItemInvoked = true;
                    break;
                case ItemContainerInteractionTrigger.SpaceKey:
                    handled = ProcessInteraction(sender, index, FocusState.Keyboard);
                    raiseItemInvoked = SelectionMode == ItemsViewSelectionMode.None;
                    break;
                case ItemContainerInteractionTrigger.AutomationInvoke:
                    raiseItemInvoked = true;
                    break;
            }

            if (!args.Handled && raiseItemInvoked && IsItemInvokedEnabled)
            {
                RaiseItemInvoked(index);
                handled = true;
            }

            args.Handled |= handled;
        }

        private bool ProcessInteraction(ItemContainer itemContainer, int index, FocusState focusState)
        {
            SetCurrentItemIndex(index);
            bool focused = itemContainer.Focus();
            ApplyInteractedSelectionPolicy(index, Keyboard.Modifiers);
            if (focusState == FocusState.Pointer)
            {
                itemContainer.BringIntoView();
            }

            return focused;
        }

        private void ApplyInteractedSelectionPolicy(int index, ModifierKeys modifiers)
        {
            bool control = (modifiers & ModifierKeys.Control) != 0;
            bool shift = (modifiers & ModifierKeys.Shift) != 0;

            switch (SelectionMode)
            {
                case ItemsViewSelectionMode.Single:
                    if (!control || !IsSelected(index))
                    {
                        _selectionModel.Select(index);
                    }
                    else
                    {
                        _selectionModel.Deselect(index);
                    }
                    break;
                case ItemsViewSelectionMode.Multiple:
                    ApplyMultipleInteraction(index, shift);
                    break;
                case ItemsViewSelectionMode.Extended:
                    ApplyExtendedInteraction(index, control, shift);
                    break;
            }
        }

        private void ApplyFocusedSelectionPolicy(int index, ModifierKeys modifiers)
        {
            bool control = (modifiers & ModifierKeys.Control) != 0;
            bool shift = (modifiers & ModifierKeys.Shift) != 0;

            switch (SelectionMode)
            {
                case ItemsViewSelectionMode.Single:
                    if (!control)
                    {
                        _selectionModel.Select(index);
                    }
                    break;
                case ItemsViewSelectionMode.Multiple:
                    if (shift)
                    {
                        ApplyRangeFromAnchor(index, IsAnchorSelected());
                    }
                    break;
                case ItemsViewSelectionMode.Extended:
                    if (shift && control)
                    {
                        if (_selectionModel.AnchorIndex != null)
                        {
                            _selectionModel.SelectRangeFromAnchor(index);
                        }
                    }
                    else if (shift)
                    {
                        SelectOnlyRangeFromAnchor(index);
                    }
                    else if (!control)
                    {
                        _selectionModel.ClearSelection();
                        _selectionModel.Select(index);
                    }
                    break;
            }
        }

        private void ApplyMultipleInteraction(int index, bool shift)
        {
            if (shift && _selectionModel.AnchorIndex != null)
            {
                bool anchorSelected = IsAnchorSelected();
                bool indexSelected = _selectionModel.IsSelected(index) == true;
                if (anchorSelected != indexSelected)
                {
                    ApplyRangeFromAnchor(index, anchorSelected);
                }
            }
            else if (_selectionModel.IsSelected(index) == true)
            {
                _selectionModel.Deselect(index);
            }
            else
            {
                _selectionModel.Select(index);
            }
        }

        private void ApplyExtendedInteraction(int index, bool control, bool shift)
        {
            if (shift)
            {
                SelectOnlyRangeFromAnchor(index);
            }
            else if (control)
            {
                if (_selectionModel.IsSelected(index) == true)
                {
                    _selectionModel.Deselect(index);
                }
                else
                {
                    _selectionModel.Select(index);
                }
            }
            else if (_selectionModel.IsSelected(index) != true)
            {
                _selectionModel.ClearSelection();
                _selectionModel.Select(index);
            }
        }

        private void SelectOnlyRangeFromAnchor(int index)
        {
            IndexPath anchor = _selectionModel.AnchorIndex;
            if (anchor == null)
            {
                return;
            }

            _selectionModel.ClearSelection();
            _selectionModel.AnchorIndex = anchor;
            _selectionModel.SelectRangeFromAnchor(index);
        }

        private void ApplyRangeFromAnchor(int index, bool select)
        {
            if (_selectionModel.AnchorIndex == null)
            {
                return;
            }

            if (select)
            {
                _selectionModel.SelectRangeFromAnchor(index);
            }
            else
            {
                _selectionModel.DeselectRangeFromAnchor(index);
            }
        }

        private bool IsAnchorSelected()
        {
            IndexPath anchor = _selectionModel.AnchorIndex;
            return anchor != null && _selectionModel.IsSelectedAt(anchor) == true;
        }

        private void DeselectWithAnchorPreservation(int index)
        {
            IndexPath anchor = _selectionModel.AnchorIndex;
            _selectionModel.Deselect(index);
            if (anchor != null)
            {
                _selectionModel.AnchorIndex = anchor;
            }
        }

        private void UpdateSelectionMode()
        {
            _selectionModel.SingleSelect = false;
            if (SelectionMode == ItemsViewSelectionMode.None)
            {
                _selectionModel.ClearSelection();
            }
            else if (SelectionMode == ItemsViewSelectionMode.Single)
            {
                _selectionModel.SingleSelect = true;
            }

            UpdatePreparedItemContainerModes();
        }

        private void OnSelectionModelSelectionChanged(
            SelectionModel sender,
            SelectionModelSelectionChangedEventArgs args)
        {
            SetValue(SelectedItemPropertyKey, _selectionModel.SelectedItem);

            foreach (ItemContainer itemContainer in _preparedItemContainers)
            {
                int index = _itemsRepeater?.GetElementIndex(itemContainer) ?? -1;
                if (index >= 0)
                {
                    SetContainerSelection(itemContainer, _selectionModel.IsSelected(index) == true);
                }
            }

            SelectionChanged?.Invoke(this, new ItemsViewSelectionChangedEventArgs());
            if (UIElementAutomationPeer.FromElement(this) is ItemsViewAutomationPeer peer)
            {
                peer.RaiseSelectionChanged();
            }
        }

        private void OnCurrentElementSelectionChanged(
            SelectionModel sender,
            SelectionModelSelectionChangedEventArgs args)
        {
            IndexPath selectedIndex = _currentElementSelectionModel.SelectedIndex;
            SetValue(
                CurrentItemIndexPropertyKey,
                selectedIndex == null ? -1 : selectedIndex.GetAt(0));
        }

        private void SetContainerSelection(ItemContainer itemContainer, bool selected)
        {
            if (itemContainer.IsSelected == selected)
            {
                return;
            }

            _updatingContainerSelection = true;
            try
            {
                itemContainer.IsSelected = selected;
            }
            finally
            {
                _updatingContainerSelection = false;
            }
        }

        private void SetCurrentItemIndex(int index)
        {
            if (index < 0)
            {
                _currentElementSelectionModel.ClearSelection();
            }
            else
            {
                _currentElementSelectionModel.Select(index);
            }
        }

        private void RaiseItemInvoked(int index)
        {
            if (_itemsSourceView != null && index >= 0 && index < _itemsSourceView.Count)
            {
                ItemInvoked?.Invoke(this, new ItemsViewItemInvokedEventArgs(_itemsSourceView.GetAt(index)));
            }
        }

        private int GetNavigationTarget(int currentIndex, Key key)
        {
            switch (key)
            {
                case Key.Home:
                    return 0;
                case Key.End:
                    return _itemsSourceView.Count - 1;
                case Key.PageUp:
                case Key.PageDown:
                    if (TryGetItemIndex(0.5, key == Key.PageUp ? 0.0 : 1.0, out int pageIndex) && pageIndex != currentIndex)
                    {
                        return pageIndex;
                    }

                    return Math.Max(
                        0,
                        Math.Min(
                            _itemsSourceView.Count - 1,
                            currentIndex + (key == Key.PageUp ? -Math.Max(1, _preparedItemContainers.Count) : Math.Max(1, _preparedItemContainers.Count))));
            }

            IndexBasedLayoutOrientation orientation = Layout?.IndexBasedLayoutOrientation ?? IndexBasedLayoutOrientation.None;
            if (orientation == IndexBasedLayoutOrientation.LeftToRight && (key == Key.Left || key == Key.Right))
            {
                return Math.Max(0, Math.Min(_itemsSourceView.Count - 1, currentIndex + (key == Key.Left ? -1 : 1)));
            }

            if (orientation == IndexBasedLayoutOrientation.TopToBottom && (key == Key.Up || key == Key.Down))
            {
                return Math.Max(0, Math.Min(_itemsSourceView.Count - 1, currentIndex + (key == Key.Up ? -1 : 1)));
            }

            return FindDirectionalNavigationTarget(currentIndex, key);
        }

        private int FindDirectionalNavigationTarget(int currentIndex, Key key)
        {
            ItemContainer current = GetRealizedItemContainer(currentIndex);
            if (current == null || _itemsRepeater == null)
            {
                return -1;
            }

            Rect currentBounds = GetElementBounds(current, _itemsRepeater);
            if (currentBounds.IsEmpty)
            {
                return -1;
            }

            Point currentCenter = new Point(
                currentBounds.Left + currentBounds.Width / 2.0,
                currentBounds.Top + currentBounds.Height / 2.0);
            int bestIndex = -1;
            double bestPrimary = double.MaxValue;
            double bestSecondary = double.MaxValue;

            foreach (ItemContainer candidate in _preparedItemContainers)
            {
                int candidateIndex = _itemsRepeater.GetElementIndex(candidate);
                if (candidateIndex < 0 || candidateIndex == currentIndex)
                {
                    continue;
                }

                Rect bounds = GetElementBounds(candidate, _itemsRepeater);
                if (bounds.IsEmpty)
                {
                    continue;
                }

                Point center = new Point(bounds.Left + bounds.Width / 2.0, bounds.Top + bounds.Height / 2.0);
                double horizontal = center.X - currentCenter.X;
                double vertical = center.Y - currentCenter.Y;
                double primary;
                double secondary;

                switch (key)
                {
                    case Key.Left:
                        primary = -horizontal;
                        secondary = Math.Abs(vertical);
                        break;
                    case Key.Right:
                        primary = horizontal;
                        secondary = Math.Abs(vertical);
                        break;
                    case Key.Up:
                        primary = -vertical;
                        secondary = Math.Abs(horizontal);
                        break;
                    case Key.Down:
                        primary = vertical;
                        secondary = Math.Abs(horizontal);
                        break;
                    default:
                        return -1;
                }

                if (primary <= 0.5)
                {
                    continue;
                }

                if (primary < bestPrimary - 0.5 ||
                    (Math.Abs(primary - bestPrimary) <= 0.5 && secondary < bestSecondary))
                {
                    bestPrimary = primary;
                    bestSecondary = secondary;
                    bestIndex = candidateIndex;
                }
            }

            return bestIndex;
        }

        private static Rect GetElementBounds(UIElement element, Visual ancestor)
        {
            try
            {
                return element.TransformToAncestor(ancestor).TransformBounds(
                    new Rect(new Point(), element.RenderSize));
            }
            catch (InvalidOperationException)
            {
                return Rect.Empty;
            }
        }

        private void ValidateItemIndex(int index)
        {
            if (_itemsSourceView == null)
            {
                throw new InvalidOperationException("ItemsSource does not have a value.");
            }

            if (index < 0 || index >= _itemsSourceView.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(index), "Index is out of bounds.");
            }
        }

        private void UpdateAutomationSetMetadata()
        {
#if NET48_OR_NEWER
            int count = _itemsSourceView?.Count ?? 0;
            if (_itemsRepeater == null)
            {
                return;
            }

            foreach (ItemContainer itemContainer in _preparedItemContainers)
            {
                int index = _itemsRepeater.GetElementIndex(itemContainer);
                if (index >= 0)
                {
                    AutomationProperties.SetPositionInSet(itemContainer, index + 1);
                    AutomationProperties.SetSizeOfSet(itemContainer, count);
                }
            }
#endif
        }

        private void DetachTemplateParts()
        {
            if (_itemsRepeater != null)
            {
                _itemsRepeater.ElementPrepared -= OnItemsRepeaterElementPrepared;
                _itemsRepeater.ElementClearing -= OnItemsRepeaterElementClearing;
                _itemsRepeater.ElementIndexChanged -= OnItemsRepeaterElementIndexChanged;
            }

            var containers = new List<ItemContainer>(_preparedItemContainers);
            foreach (ItemContainer itemContainer in containers)
            {
                DetachItemContainer(itemContainer);
            }

            if (_scrollView != null)
            {
                _scrollView.VerticalScrollController = null;
            }

            _itemsRepeater = null;
            _scrollHost = null;
            _scrollView = null;
            SetValue(ScrollViewPropertyKey, null);
        }

        private ItemContainer FindItemContainer(DependencyObject element)
        {
            DependencyObject current = element;
            while (current != null && !ReferenceEquals(current, this))
            {
                if (current is ItemContainer itemContainer && _preparedItemContainers.Contains(itemContainer))
                {
                    return itemContainer;
                }

                current = VisualTreeHelper.GetParent(current);
            }

            return null;
        }

        private static bool IsDescendantOf(DependencyObject element, DependencyObject ancestor)
        {
            DependencyObject current = element;
            while (current != null)
            {
                if (ReferenceEquals(current, ancestor))
                {
                    return true;
                }

                current = VisualTreeHelper.GetParent(current);
            }

            return false;
        }

        private enum FocusState
        {
            Pointer,
            Keyboard
        }
    }
}
