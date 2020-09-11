// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Input;
using ModernWpf.Automation.Peers;
using ModernWpf.Controls.Primitives;
using ModernWpf.Input;
using static CppWinRTHelpers;
using static ModernWpf.Controls.NavigationViewItemHelper;
using PointerRoutedEventArgs = System.Windows.Input.MouseEventArgs;

namespace ModernWpf.Controls
{
    public partial class NavigationViewItem : NavigationViewItemBase
    {
        const string c_navigationViewItemPresenterName = "NavigationViewItemPresenter";
        const string c_repeater = "NavigationViewItemMenuItemsHost";
        const string c_rootGrid = "NVIRootGrid";
        const string c_childrenFlyout = "ChildrenFlyout";
        const string c_flyoutContentGrid = "FlyoutContentGrid";

        // Visual States
        const string c_pressedSelected = "PressedSelected";
        const string c_pointerOverSelected = "PointerOverSelected";
        const string c_selected = "Selected";
        const string c_pressed = "Pressed";
        const string c_pointerOver = "PointerOver";
        const string c_disabled = "Disabled";
        const string c_enabled = "Enabled";
        const string c_normal = "Normal";
        const string c_chevronHidden = "ChevronHidden";
        const string c_chevronVisibleOpen = "ChevronVisibleOpen";
        const string c_chevronVisibleClosed = "ChevronVisibleClosed";

        static NavigationViewItem()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(NavigationViewItem),
                new FrameworkPropertyMetadata(typeof(NavigationViewItem)));
        }

        public NavigationViewItem()
        {
            SetValue(MenuItemsPropertyKey, new ObservableCollection<object>());
        }

        internal void UpdateVisualStateNoTransition()
        {
            UpdateVisualState(false /*useTransition*/);
        }

        private protected override void OnNavigationViewItemBaseDepthChanged()
        {
            UpdateItemIndentation();
            PropagateDepthToChildren(Depth + 1);
        }

        private protected override void OnNavigationViewItemBaseIsSelectedChanged()
        {
            UpdateVisualStateForPointer();
        }

        private protected override void OnNavigationViewItemBasePositionChanged()
        {
            UpdateVisualStateNoTransition();
            ReparentRepeater();
        }

        public override void OnApplyTemplate()
        {
            // Stop UpdateVisualState before template is applied. Otherwise the visuals may be unexpected
            m_appliedTemplate = false;

            UnhookEventsAndClearFields();

            base.OnApplyTemplate();

            // Find selection indicator
            // Retrieve pointers to stable controls 
            IControlProtected controlProtected = this;
            m_helper.Init(controlProtected);

            if (GetTemplateChildT<Grid>(c_rootGrid, controlProtected) is { } rootGrid)
            {
                m_rootGrid = rootGrid;

                if (FlyoutBase.GetAttachedFlyout(rootGrid) is { } flyoutBase)
                {
                    m_flyoutClosingRevoker = new FlyoutBaseClosingRevoker(flyoutBase, OnFlyoutClosing);
                }
            }

            HookInputEvents(controlProtected);

            IsEnabledChanged += OnIsEnabledChanged;

            m_toolTip = GetTemplateChildT<ToolTip>("ToolTip", controlProtected);

            if (GetSplitView() is { } splitView)
            {
                splitView.IsPaneOpenChanged += OnSplitViewPropertyChanged;
                splitView.DisplayModeChanged += OnSplitViewPropertyChanged;
                splitView.CompactPaneLengthChanged += OnSplitViewPropertyChanged;

                UpdateCompactPaneLength();
                UpdateIsClosedCompact();
            }

            // Retrieve reference to NavigationView
            if (GetNavigationView() is { } nvImpl)
            {
                if (GetTemplateChildT<ItemsRepeater>(c_repeater, controlProtected) is { } repeater)
                {
                    m_repeater = repeater;

                    // Primary element setup happens in NavigationView
                    m_repeaterElementPreparedRevoker = new ItemsRepeaterElementPreparedRevoker(repeater, nvImpl.OnRepeaterElementPrepared);
                    m_repeaterElementClearingRevoker = new ItemsRepeaterElementClearingRevoker(repeater, nvImpl.OnRepeaterElementClearing);

                    repeater.ItemTemplate = nvImpl.GetNavigationViewItemsFactory();
                }

                UpdateRepeaterItemsSource();
            }

            if (GetTemplateChildT<FlyoutBase>(c_childrenFlyout, controlProtected) is { } childrenFlyout)
            {
                childrenFlyout.Offset = 0;
            }

            m_flyoutContentGrid = GetTemplateChildT<Grid>(c_flyoutContentGrid, controlProtected);

            m_appliedTemplate = true;

            UpdateItemIndentation();
            UpdateVisualStateNoTransition();
            ReparentRepeater();
            // We dont want to update the repeater visibilty during OnApplyTemplate if NavigationView is in a mode when items are shown in a flyout
            if (!ShouldRepeaterShowInFlyout())
            {
                ShowHideChildren();
            }

            /*
            var visual = ElementCompositionPreview.GetElementVisual(this);
            NavigationView.CreateAndAttachHeaderAnimation(visual);
            */
        }

        void UpdateRepeaterItemsSource()
        {
            if (m_repeater is { } repeater)
            {
                object itemsSource;
                {
                    object init()
                    {
                        if (MenuItemsSource is { } menuItemsSource)
                        {
                            return menuItemsSource;
                        }
                        return MenuItems;
                    }
                    itemsSource = init();
                }
                m_itemsSourceViewCollectionChangedRevoker?.Revoke();
                repeater.ItemsSource = itemsSource;
                m_itemsSourceViewCollectionChangedRevoker = new ItemsSourceView.CollectionChangedRevoker(repeater.ItemsSourceView, OnItemsSourceViewChanged);
            }
        }

        private void OnItemsSourceViewChanged(object sender, NotifyCollectionChangedEventArgs args)
        {
            UpdateVisualStateForChevron();
        }

        internal UIElement GetSelectionIndicator()
        {
            var selectIndicator = m_helper.GetSelectionIndicator();
            if (GetPresenter() is { } presenter)
            {
                selectIndicator = presenter.GetSelectionIndicator();
            }
            return selectIndicator;
        }

        void OnSplitViewPropertyChanged(DependencyObject sender, DependencyProperty args)
        {
            if (args == SplitView.CompactPaneLengthProperty)
            {
                UpdateCompactPaneLength();
            }
            else if (args == SplitView.IsPaneOpenProperty ||
                args == SplitView.DisplayModeProperty)
            {
                UpdateIsClosedCompact();
                ReparentRepeater();
            }
        }

        void UpdateCompactPaneLength()
        {
            if (GetSplitView() is { } splitView)
            {
                SetValue(CompactPaneLengthPropertyKey, splitView.CompactPaneLength);

                // Only update when on left
                if (GetPresenter() is { } presenter)
                {
                    presenter.UpdateCompactPaneLength(splitView.CompactPaneLength, IsOnLeftNav());
                }
            }
        }

        internal void UpdateIsClosedCompact()
        {
            if (GetSplitView() is { } splitView)
            {
                // Check if the pane is closed and if the splitview is in either compact mode.
                m_isClosedCompact = !splitView.IsPaneOpen
                    && (splitView.DisplayMode == SplitViewDisplayMode.CompactOverlay || splitView.DisplayMode == SplitViewDisplayMode.CompactInline);

                UpdateVisualState(true /*useTransitions*/);

                if (GetPresenter() is { } presenter)
                {
                    presenter.UpdateClosedCompactVisualState(IsTopLevelItem, m_isClosedCompact);
                }
            }
        }

        void UpdateNavigationViewItemToolTip()
        {
            var toolTipContent = ToolTipService.GetToolTip(this);

            // no custom tooltip, then use suggested tooltip
            if (toolTipContent == null || toolTipContent == m_suggestedToolTipContent)
            {
                if (ShouldEnableToolTip())
                {
                    ToolTipService.SetToolTip(this, m_suggestedToolTipContent);
                }
                else
                {
                    ToolTipService.SetToolTip(this, null);
                }
            }
        }

        void SuggestedToolTipChanged(object newContent)
        {
            var potentialString = newContent;
            bool stringableToolTip = (potentialString != null && potentialString is string);

            object newToolTipContent = null;
            if (stringableToolTip)
            {
                newToolTipContent = newContent;
            }

            // Both customer and NavigationViewItem can update ToolTipContent by ToolTipService.SetToolTip or XAML
            // If the ToolTipContent is not the same as m_suggestedToolTipContent, then it's set by customer.
            // Customer's ToolTip take high priority, and we never override Customer's ToolTip.
            var toolTipContent = ToolTipService.GetToolTip(this);
            if (m_suggestedToolTipContent is { } oldToolTipContent)
            {
                if (oldToolTipContent == toolTipContent)
                {
                    ToolTipService.SetToolTip(this, null);
                }
            }

            m_suggestedToolTipContent = newToolTipContent;
        }

        void OnIsExpandedPropertyChanged(DependencyPropertyChangedEventArgs args)
        {
            if (FrameworkElementAutomationPeer.FromElement(this) is AutomationPeer peer)
            {
                var navViewItemPeer = (NavigationViewItemAutomationPeer)peer;
                navViewItemPeer.RaiseExpandCollapseAutomationEvent(
                    IsExpanded ?
                        ExpandCollapseState.Expanded :
                        ExpandCollapseState.Collapsed
                );
            }
        }

        void OnIconPropertyChanged(DependencyPropertyChangedEventArgs args)
        {
            UpdateVisualStateNoTransition();
        }

        void OnMenuItemsPropertyChanged(DependencyPropertyChangedEventArgs args)
        {
            UpdateRepeaterItemsSource();
            UpdateVisualStateForChevron();
        }

        void OnMenuItemsSourcePropertyChanged(DependencyPropertyChangedEventArgs args)
        {
            UpdateRepeaterItemsSource();
            UpdateVisualStateForChevron();
        }

        void OnHasUnrealizedChildrenPropertyChanged(DependencyPropertyChangedEventArgs args)
        {
            UpdateVisualStateForChevron();
        }

        void ShowSelectionIndicator(bool visible)
        {
            if (GetSelectionIndicator() is { } selectionIndicator)
            {
                selectionIndicator.Opacity = visible ? 1.0 : 0.0;
            }
        }

        void UpdateVisualStateForIconAndContent(bool showIcon, bool showContent)
        {
            if (m_navigationViewItemPresenter is { } presenter)
            {
                var stateName = showIcon ? (showContent ? "IconOnLeft" : "IconOnly") : "ContentOnly";
                VisualStateManager.GoToState(presenter, stateName, false /*useTransitions*/);
            }
        }

        void UpdateVisualStateForNavigationViewPositionChange()
        {
            var position = Position;
            var stateName = c_OnLeftNavigation;

            bool handled = false;

            switch (position)
            {
                case NavigationViewRepeaterPosition.LeftNav:
                case NavigationViewRepeaterPosition.LeftFooter:
                    if (SharedHelpers.IsRS4OrHigher() && false /*Application.Current.FocusVisualKind == FocusVisualKind.Reveal*/)
                    {
                        // OnLeftNavigationReveal is introduced in RS6. 
                        // Will fallback to stateName for the customer who re-template rs5 NavigationViewItem
                        if (VisualStateManager.GoToState(this, c_OnLeftNavigationReveal, false /*useTransitions*/))
                        {
                            handled = true;
                        }
                    }
                    break;
                case NavigationViewRepeaterPosition.TopPrimary:
                case NavigationViewRepeaterPosition.TopFooter:
                    if (SharedHelpers.IsRS4OrHigher() && false /*Application.Current.FocusVisualKind == FocusVisualKind.Reveal*/)
                    {
                        stateName = c_OnTopNavigationPrimaryReveal;
                    }
                    else
                    {
                        stateName = c_OnTopNavigationPrimary;
                    }
                    break;
                case NavigationViewRepeaterPosition.TopOverflow:
                    stateName = c_OnTopNavigationOverflow;
                    break;
            }

            if (!handled)
            {
                VisualStateManager.GoToState(this, stateName, false /*useTransitions*/);
            }
        }

        void UpdateVisualStateForKeyboardFocusedState()
        {
            var focusState = "KeyboardNormal";
            if (m_hasKeyboardFocus)
            {
                focusState = "KeyboardFocused";
            }

            VisualStateManager.GoToState(this, focusState, false /*useTransitions*/);
        }

        void UpdateVisualStateForToolTip()
        {
            // Since RS5, ToolTip apply to NavigationViewItem directly to make Keyboard focus has tooltip too.
            // If ToolTip TemplatePart is detected, fallback to old logic and apply ToolTip on TemplatePart.
            if (m_toolTip is { } toolTip)
            {
                var shouldEnableToolTip = ShouldEnableToolTip();
                var toolTipContent = m_suggestedToolTipContent;
                if (shouldEnableToolTip && toolTipContent != null)
                {
                    toolTip.Content = toolTipContent;
                    toolTip.IsEnabled = true;
                }
                else
                {
                    toolTip.Content = null;
                    toolTip.IsEnabled = false;
                }
            }
            else
            {
                UpdateNavigationViewItemToolTip();
            }
        }

        void UpdateVisualStateForPointer()
        {
            // DisabledStates and CommonStates
            var enabledStateValue = c_enabled;
            bool isSelected = IsSelected;
            var selectedStateValue = c_normal;
            if (IsEnabled)
            {
                if (isSelected)
                {
                    if (m_isPressed)
                    {
                        selectedStateValue = c_pressedSelected;
                    }
                    else if (m_isPointerOver)
                    {
                        selectedStateValue = c_pointerOverSelected;
                    }
                    else
                    {
                        selectedStateValue = c_selected;
                    }
                }
                else if (m_isPointerOver)
                {
                    if (m_isPressed)
                    {
                        selectedStateValue = c_pressed;
                    }
                    else
                    {
                        selectedStateValue = c_pointerOver;
                    }
                }
                else if (m_isPressed)
                {
                    selectedStateValue = c_pressed;
                }
            }
            else
            {
                enabledStateValue = c_disabled;
                if (isSelected)
                {
                    selectedStateValue = c_selected;
                }
            }

            // There are scenarios where the presenter may not exist.
            // For example, the top nav settings item. In that case,
            // update the states for the item itself.
            if (m_navigationViewItemPresenter is { } presenter)
            {
                VisualStateManager.GoToState(presenter, enabledStateValue, true);
                VisualStateManager.GoToState(presenter, selectedStateValue, true);
            }
            else
            {
                VisualStateManager.GoToState(this, enabledStateValue, true);
                VisualStateManager.GoToState(this, selectedStateValue, true);
            }
        }

        void UpdateVisualState(bool useTransitions)
        {
            if (!m_appliedTemplate)
                return;

            UpdateVisualStateForPointer();

            UpdateVisualStateForNavigationViewPositionChange();

            bool shouldShowIcon = ShouldShowIcon();
            bool shouldShowContent = ShouldShowContent();

            if (IsOnLeftNav())
            {
                if (m_navigationViewItemPresenter is { } presenter)
                {
                    // Backward Compatibility with RS4-, new implementation prefer IconOnLeft/IconOnly/ContentOnly
                    VisualStateManager.GoToState(presenter, shouldShowIcon ? "IconVisible" : "IconCollapsed", useTransitions);
                }
            }

            UpdateVisualStateForToolTip();

            UpdateVisualStateForIconAndContent(shouldShowIcon, shouldShowContent);

            // visual state for focus state. top navigation use it to provide different visual for selected and selected+focused
            UpdateVisualStateForKeyboardFocusedState();

            UpdateVisualStateForChevron();
        }

        void UpdateVisualStateForChevron()
        {
            if (m_navigationViewItemPresenter is { } presenter)
            {
                var chevronState = HasChildren() && !(m_isClosedCompact && ShouldRepeaterShowInFlyout()) ? (IsExpanded ? c_chevronVisibleOpen : c_chevronVisibleClosed) : c_chevronHidden;
                VisualStateManager.GoToState(presenter, chevronState, true);
            }
        }

        internal bool HasChildren()
        {
            return MenuItems.Count > 0
                || (MenuItemsSource != null && m_repeater != null && m_repeater.ItemsSourceView.Count > 0)
                || HasUnrealizedChildren;
        }

        bool ShouldShowIcon()
        {
            return Icon != null;
        }

        bool ShouldEnableToolTip()
        {
            // We may enable Tooltip for IconOnly in the future, but not now
            return IsOnLeftNav() && m_isClosedCompact;
        }

        bool ShouldShowContent()
        {
            return Content != null;
        }

        bool IsOnLeftNav()
        {
            var position = Position;
            return position == NavigationViewRepeaterPosition.LeftNav || position == NavigationViewRepeaterPosition.LeftFooter;
        }

        bool IsOnTopPrimary()
        {
            return Position == NavigationViewRepeaterPosition.TopPrimary;
        }

        UIElement GetPresenterOrItem()
        {
            if (m_navigationViewItemPresenter is { } presenter)
            {
                return presenter;
            }
            else
            {
                return this;
            }
        }

        NavigationViewItemPresenter GetPresenter()
        {
            NavigationViewItemPresenter presenter = null;
            if (m_navigationViewItemPresenter != null)
            {
                presenter = m_navigationViewItemPresenter;
            }
            return presenter;
        }

        internal ItemsRepeater GetRepeater() { return m_repeater; }

        internal void ShowHideChildren()
        {
            if (m_repeater is { } repeater)
            {
                bool shouldShowChildren = IsExpanded;
                var visibility = shouldShowChildren ? Visibility.Visible : Visibility.Collapsed;
                repeater.Visibility = visibility;

                if (ShouldRepeaterShowInFlyout())
                {
                    if (shouldShowChildren)
                    {
                        // Verify that repeater is parented correctly
                        if (!m_isRepeaterParentedToFlyout)
                        {
                            ReparentRepeater();
                        }

                        // There seems to be a race condition happening which sometimes
                        // prevents the opening of the flyout. Queue callback as a workaround.
                        SharedHelpers.QueueCallbackForCompositionRendering(
                            () =>
                            {
                                FlyoutBase.ShowAttachedFlyout(m_rootGrid);
                            });
                    }
                    else
                    {
                        if (FlyoutBase.GetAttachedFlyout(m_rootGrid) is { } flyout)
                        {
                            flyout.Hide();
                        }
                    }
                }
            }
        }

        void ReparentRepeater()
        {
            if (HasChildren())
            {
                if (m_repeater is { } repeater)
                {
                    if (ShouldRepeaterShowInFlyout() && !m_isRepeaterParentedToFlyout)
                    {
                        // Reparent repeater to flyout
                        // TODO: Replace removeatend with something more specific
                        m_rootGrid.Children.Remove(repeater);
                        m_flyoutContentGrid.Children.Add(repeater);
                        m_isRepeaterParentedToFlyout = true;

                        PropagateDepthToChildren(0);
                    }
                    else if (!ShouldRepeaterShowInFlyout() && m_isRepeaterParentedToFlyout)
                    {
                        m_flyoutContentGrid.Children.Remove(repeater);
                        m_rootGrid.Children.Add(repeater);
                        m_isRepeaterParentedToFlyout = false;

                        PropagateDepthToChildren(1);
                    }
                }
            }
        }

        internal bool ShouldRepeaterShowInFlyout()
        {
            return (m_isClosedCompact && IsTopLevelItem) || IsOnTopPrimary();
        }

        internal bool IsRepeaterVisible()
        {
            if (m_repeater is { } repeater)
            {
                return repeater.Visibility == Visibility.Visible;
            }
            return false;
        }

        void UpdateItemIndentation()
        {
            // Update item indentation based on its depth
            if (m_navigationViewItemPresenter is { } presenter)
            {
                var newLeftMargin = Depth * c_itemIndentation;
                presenter.UpdateContentLeftIndentation(newLeftMargin);
            }
        }

        internal void PropagateDepthToChildren(int depth)
        {
            if (m_repeater is { } repeater)
            {
                var itemsCount = repeater.ItemsSourceView.Count;
                for (int index = 0; index < itemsCount; index++)
                {
                    if (repeater.TryGetElement(index) is { } element)
                    {
                        if (element is NavigationViewItemBase nvib)
                        {
                            nvib.Depth = depth;
                        }
                    }
                }
            }
        }

        internal void OnExpandCollapseChevronTapped(object sender, TappedRoutedEventArgs args)
        {
            IsExpanded = !IsExpanded;
            args.Handled = true;
        }

        void OnFlyoutClosing(object sender, FlyoutBaseClosingEventArgs args)
        {
            IsExpanded = false;
        }

        // IUIElement / IUIElementOverridesHelper
        protected override AutomationPeer OnCreateAutomationPeer()
        {
            return new NavigationViewItemAutomationPeer(this);
        }

        // IContentControlOverrides / IContentControlOverridesHelper
        protected override void OnContentChanged(object oldContent, object newContent)
        {
            base.OnContentChanged(oldContent, newContent);
            SuggestedToolTipChanged(newContent);
            UpdateVisualStateNoTransition();

            if (!IsOnLeftNav())
            {
                // Content has changed for the item, so we want to trigger a re-measure
                if (GetNavigationView() is { } navView)
                {
                    navView.TopNavigationViewItemContentChanged();
                }
            }
        }

        protected override void OnGotFocus(RoutedEventArgs e)
        {
            base.OnGotFocus(e);
            var originalSource = e.OriginalSource as Control;
            if (originalSource != null)
            {
                // It's used to support bluebar have difference appearance between focused and focused+selection. 
                // For example, we can move the SelectionIndicator 3px up when focused and selected to make sure focus rectange doesn't override SelectionIndicator. 
                // If it's a pointer or programatic, no focus rectangle, so no action
                /*
                var focusState = originalSource.FocusState;
                if (focusState == FocusState.Keyboard)
                */
                if (originalSource.IsKeyboardFocused && InputManager.Current.MostRecentInputDevice is KeyboardDevice)
                {
                    m_hasKeyboardFocus = true;
                    UpdateVisualStateNoTransition();
                }
            }
        }

        protected override void OnLostFocus(RoutedEventArgs e)
        {
            base.OnLostFocus(e);
            if (m_hasKeyboardFocus)
            {
                m_hasKeyboardFocus = false;
                UpdateVisualStateNoTransition();
            }
        }

        void OnPresenterPointerPressed(object sender, PointerRoutedEventArgs args)
        {
            Debug.Assert(!m_isPressed);
            Debug.Assert(!m_isMouseCaptured);

            // TODO: Update to look at presenter instead
            m_isPressed = args.LeftButton == MouseButtonState.Pressed || args.RightButton == MouseButtonState.Pressed;

            var presenter = GetPresenterOrItem();

            Debug.Assert(presenter != null);

            if (presenter.CaptureMouse())
            {
                m_isMouseCaptured = true;
            }

            UpdateVisualState(true);
        }

        void OnPresenterPointerReleased(object sender, PointerRoutedEventArgs args)
        {
            if (m_isPressed)
            {
                m_isPressed = false;

                if (m_isMouseCaptured)
                {
                    var presenter = GetPresenterOrItem();

                    Debug.Assert(presenter != null);

                    presenter.ReleaseMouseCapture();
                }
            }

            UpdateVisualState(true);
        }

        void OnPresenterPointerEntered(object sender, PointerRoutedEventArgs args)
        {
            ProcessPointerOver(args);
        }

        void OnPresenterPointerMoved(object sender, PointerRoutedEventArgs args)
        {
            ProcessPointerOver(args);
        }

        void OnPresenterPointerExited(object sender, PointerRoutedEventArgs args)
        {
            m_isPointerOver = false;
            UpdateVisualState(true);
        }

        void OnPresenterPointerCanceled(object sender, PointerRoutedEventArgs args)
        {
            ProcessPointerCanceled(args);
        }

        void OnPresenterPointerCaptureLost(object sender, PointerRoutedEventArgs args)
        {
            ProcessPointerCanceled(args);
        }

        void OnIsEnabledChanged(object sender, DependencyPropertyChangedEventArgs args)
        {
            if (!IsEnabled)
            {
                m_isPressed = false;
                m_isPointerOver = false;

                if (m_isMouseCaptured)
                {
                    var presenter = GetPresenterOrItem();

                    Debug.Assert(presenter != null);

                    presenter.ReleaseMouseCapture();
                    m_isMouseCaptured = false;
                }
            }

            UpdateVisualState(true);
        }

        internal void RotateExpandCollapseChevron(bool isExpanded)
        {
            if (GetPresenter() is { } presenter)
            {
                presenter.RotateExpandCollapseChevron(isExpanded);
            }
        }

        void ProcessPointerCanceled(PointerRoutedEventArgs args)
        {
            m_isPressed = false;
            m_isPointerOver = false;
            m_isMouseCaptured = false;
            UpdateVisualState(true);
        }

        void ProcessPointerOver(PointerRoutedEventArgs args)
        {
            if (!m_isPointerOver)
            {
                m_isPointerOver = true;
                UpdateVisualState(true);
            }
        }

        void HookInputEvents(IControlProtected controlProtected)
        {
            UIElement presenter;
            {
                presenter = init();
                UIElement init()
                {
                    if (GetTemplateChildT<NavigationViewItemPresenter>(c_navigationViewItemPresenterName, controlProtected) is { } presenter)
                    {
                        m_navigationViewItemPresenter = presenter;
                        return presenter;
                    }
                    // We don't have a presenter, so we are our own presenter.
                    return this;
                }
            }

            Debug.Assert(presenter != null);

            // Handlers that set flags are skipped when args.Handled is already True.
            presenter.MouseDown += OnPresenterPointerPressed;
            presenter.MouseEnter += OnPresenterPointerEntered;
            presenter.MouseMove += OnPresenterPointerMoved;

            // Handlers that reset flags are not skipped when args.Handled is already True to avoid broken states.
            presenter.AddHandler(MouseUpEvent, new MouseButtonEventHandler(OnPresenterPointerReleased), true /*handledEventsToo*/);
            presenter.AddHandler(MouseLeaveEvent, new MouseEventHandler(OnPresenterPointerExited), true /*handledEventsToo*/);
            presenter.AddHandler(LostMouseCaptureEvent, new MouseEventHandler(OnPresenterPointerCaptureLost), true /*handledEventsToo*/);
        }

        void UnhookInputEvents()
        {
            var presenter = m_navigationViewItemPresenter as UIElement ?? this;
            presenter.MouseDown -= OnPresenterPointerPressed;
            presenter.MouseEnter -= OnPresenterPointerEntered;
            presenter.MouseMove -= OnPresenterPointerMoved;
            presenter.RemoveHandler(MouseUpEvent, new MouseButtonEventHandler(OnPresenterPointerReleased));
            presenter.RemoveHandler(MouseLeaveEvent, new MouseEventHandler(OnPresenterPointerExited));
            presenter.RemoveHandler(LostMouseCaptureEvent, new MouseEventHandler(OnPresenterPointerCaptureLost));
        }

        void UnhookEventsAndClearFields()
        {
            UnhookInputEvents();

            m_flyoutClosingRevoker?.Revoke();
            m_splitViewIsPaneOpenChangedRevoker?.Revoke();
            m_splitViewDisplayModeChangedRevoker?.Revoke();
            m_splitViewCompactPaneLengthChangedRevoker?.Revoke();
            m_repeaterElementPreparedRevoker?.Revoke();
            m_repeaterElementClearingRevoker?.Revoke();
            IsEnabledChanged -= OnIsEnabledChanged;
            m_itemsSourceViewCollectionChangedRevoker?.Revoke();

            m_rootGrid = null;
            m_navigationViewItemPresenter = null;
            m_toolTip = null;
            m_repeater = null;
            m_flyoutContentGrid = null;
        }

        SplitViewIsPaneOpenChangedRevoker m_splitViewIsPaneOpenChangedRevoker;
        SplitViewDisplayModeChangedRevoker m_splitViewDisplayModeChangedRevoker;
        SplitViewCompactPaneLengthChangedRevoker m_splitViewCompactPaneLengthChangedRevoker;

        ItemsRepeaterElementPreparedRevoker m_repeaterElementPreparedRevoker;
        ItemsRepeaterElementClearingRevoker m_repeaterElementClearingRevoker;
        ItemsSourceView.CollectionChangedRevoker m_itemsSourceViewCollectionChangedRevoker;

        FlyoutBaseClosingRevoker m_flyoutClosingRevoker;

        ToolTip m_toolTip;
        NavigationViewItemHelper<NavigationViewItem> m_helper = new NavigationViewItemHelper<NavigationViewItem>();
        NavigationViewItemPresenter m_navigationViewItemPresenter;
        object m_suggestedToolTipContent;
        ItemsRepeater m_repeater;
        Grid m_flyoutContentGrid;
        Grid m_rootGrid;

        bool m_isClosedCompact = false;

        bool m_appliedTemplate = false;
        bool m_hasKeyboardFocus = false;

        // Visual state tracking
        bool m_isMouseCaptured = false;
        bool m_isPressed = false;
        bool m_isPointerOver = false;

        bool m_isRepeaterParentedToFlyout = false;
    }
}
