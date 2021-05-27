// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shell;
using System.Windows.Threading;
using ModernWpf.Automation.Peers;
using ModernWpf.Input;
using ModernWpf.Media.Animation;
using static CppWinRTHelpers;
using static ModernWpf.ResourceAccessor;

namespace ModernWpf.Controls
{
    enum TopNavigationViewLayoutState
    {
        Uninitialized = 0,
        Initialized
    }

    enum NavigationRecommendedTransitionDirection
    {
        FromOverflow, // mapping to SlideNavigationTransitionInfo FromLeft
        FromLeft, // SlideNavigationTransitionInfo
        FromRight, // SlideNavigationTransitionInfo
        Default // Currently it's mapping to EntranceNavigationTransitionInfo and is subject to change.
    }

    public partial class NavigationView : ContentControl, IControlProtected
    {
        // General items
        const string c_togglePaneButtonName = "TogglePaneButton";
        const string c_paneTitleHolderFrameworkElement = "PaneTitleHolder";
        const string c_paneTitleFrameworkElement = "PaneTitleTextBlock";
        const string c_rootSplitViewName = "RootSplitView";
        const string c_menuItemsHost = "MenuItemsHost";
        const string c_footerMenuItemsHost = "FooterMenuItemsHost";
        const string c_selectionIndicatorName = "SelectionIndicator";
        const string c_paneContentGridName = "PaneContentGrid";
        const string c_rootGridName = "RootGrid";
        const string c_contentGridName = "ContentGrid";
        const string c_searchButtonName = "PaneAutoSuggestButton";
        const string c_paneToggleButtonIconGridColumnName = "PaneToggleButtonIconWidthColumn";
        const string c_togglePaneTopPadding = "TogglePaneTopPadding";
        const string c_contentPaneTopPadding = "ContentPaneTopPadding";
        const string c_contentLeftPadding = "ContentLeftPadding";
        const string c_navViewBackButton = "NavigationViewBackButton";
        const string c_navViewBackButtonToolTip = "NavigationViewBackButtonToolTip";
        const string c_navViewCloseButton = "NavigationViewCloseButton";
        const string c_navViewCloseButtonToolTip = "NavigationViewCloseButtonToolTip";
        const string c_paneShadowReceiverCanvas = "PaneShadowReceiver";
        const string c_flyoutRootGrid = "FlyoutRootGrid";

        // DisplayMode Top specific items
        const string c_topNavMenuItemsHost = "TopNavMenuItemsHost";
        const string c_topNavFooterMenuItemsHost = "TopFooterMenuItemsHost";
        const string c_topNavOverflowButton = "TopNavOverflowButton";
        const string c_topNavMenuItemsOverflowHost = "TopNavMenuItemsOverflowHost";
        const string c_topNavGrid = "TopNavGrid";
        const string c_topNavContentOverlayAreaGrid = "TopNavContentOverlayAreaGrid";
        const string c_leftNavPaneAutoSuggestBoxPresenter = "PaneAutoSuggestBoxPresenter";
        const string c_topNavPaneAutoSuggestBoxPresenter = "TopPaneAutoSuggestBoxPresenter";
        const string c_paneTitlePresenter = "PaneTitlePresenter";

        // DisplayMode Left specific items
        const string c_leftNavFooterContentBorder = "FooterContentBorder";
        const string c_leftNavPaneHeaderContentBorder = "PaneHeaderContentBorder";
        const string c_leftNavPaneCustomContentBorder = "PaneCustomContentBorder";

        const string c_itemsContainer = "ItemsContainerGrid";
        const string c_itemsContainerRow = "ItemsContainerRow";
        const string c_visualItemsSeparator = "VisualItemsSeparator";
        const string c_menuItemsScrollViewer = "MenuItemsScrollViewer";
        const string c_footerItemsScrollViewer = "FooterItemsScrollViewer";

        const string c_paneHeaderOnTopPane = "PaneHeaderOnTopPane";
        const string c_paneTitleOnTopPane = "PaneTitleOnTopPane";
        const string c_paneCustomContentOnTopPane = "PaneCustomContentOnTopPane";
        const string c_paneFooterOnTopPane = "PaneFooterOnTopPane";
        const string c_paneHeaderCloseButtonColumn = "PaneHeaderCloseButtonColumn";
        const string c_paneHeaderToggleButtonColumn = "PaneHeaderToggleButtonColumn";
        const string c_paneHeaderContentBorderRow = "PaneHeaderContentBorderRow";

        const int c_backButtonHeight = 40;
        const int c_backButtonWidth = 40;
        const int c_paneToggleButtonHeight = 40;
        const int c_paneToggleButtonWidth = 40;
        const int c_toggleButtonHeightWhenShouldPreserveNavigationViewRS3Behavior = 56;
        const int c_backButtonRowDefinition = 1;
        const float c_paneElevationTranslationZ = 32;

        const int c_mainMenuBlockIndex = 0;
        const int c_footerMenuBlockIndex = 1;

        const int s_itemNotFound = -1;

        static readonly Size c_infSize = new Size(double.PositiveInfinity, double.PositiveInfinity);

        private static readonly ResourceAccessor ResourceAccessor = new ResourceAccessor(typeof(NavigationView));

        /*
        ~NavigationView()
        {
            UnhookEventsAndClearFields(true);
        }
        */

        protected override AutomationPeer OnCreateAutomationPeer()
        {
            return new NavigationViewAutomationPeer(this);
        }

        void UnhookEventsAndClearFields(bool isFromDestructor = false)
        {
            if (m_coreTitleBar != null)
            {
                m_coreTitleBar.LayoutMetricsChanged -= OnTitleBarMetricsChanged;
                m_coreTitleBar.IsVisibleChanged -= OnTitleBarIsVisibleChanged;
            }
            if (m_paneToggleButton != null)
            {
                m_paneToggleButton.Click -= OnPaneToggleButtonClick;
            }

            m_settingsItem = null;

            if (m_paneSearchButton != null)
            {
                m_paneSearchButton.Click -= OnPaneSearchButtonClick;
                m_paneSearchButton = null;
            }

            m_paneHeaderOnTopPane = null;
            m_paneTitleOnTopPane = null;

            m_itemsContainerSizeChangedRevoker?.Revoke();

            if (m_paneTitleHolderFrameworkElement != null)
            {
                m_paneTitleHolderFrameworkElement.SizeChanged -= OnPaneTitleHolderSizeChanged;
                m_paneTitleHolderFrameworkElement = null;
            }

            m_paneTitleFrameworkElement = null;
            m_paneTitlePresenter = null;

            m_paneHeaderCloseButtonColumn = null;
            m_paneHeaderToggleButtonColumn = null;
            m_paneHeaderContentBorderRow = null;

            if (m_leftNavRepeater != null)
            {
                m_leftNavRepeater.ElementPrepared -= OnRepeaterElementPrepared;
                m_leftNavRepeater.ElementClearing -= OnRepeaterElementClearing;
                m_leftNavRepeater.IsVisibleChanged -= OnRepeaterIsVisibleChanged;
                m_leftNavRepeaterGettingFocusHelper?.Dispose();
                m_leftNavRepeater = null;
            }

            if (m_topNavRepeater != null)
            {
                m_topNavRepeater.ElementPrepared -= OnRepeaterElementPrepared;
                m_topNavRepeater.ElementClearing -= OnRepeaterElementClearing;
                m_topNavRepeater.IsVisibleChanged -= OnRepeaterIsVisibleChanged;
                m_topNavRepeaterGettingFocusHelper?.Dispose();
                m_topNavRepeater = null;
            }

            if (m_leftNavFooterMenuRepeater != null)
            {
                m_leftNavFooterMenuRepeater.ElementPrepared -= OnRepeaterElementPrepared;
                m_leftNavFooterMenuRepeater.ElementClearing -= OnRepeaterElementClearing;
                m_leftNavFooterMenuRepeater.IsVisibleChanged -= OnRepeaterIsVisibleChanged;
                m_leftNavFooterMenuRepeaterGettingFocusHelper?.Dispose();
                m_leftNavFooterMenuRepeater = null;
            }

            if (m_topNavFooterMenuRepeater != null)
            {
                m_topNavFooterMenuRepeater.ElementPrepared -= OnRepeaterElementPrepared;
                m_topNavFooterMenuRepeater.ElementClearing -= OnRepeaterElementClearing;
                m_topNavFooterMenuRepeater.IsVisibleChanged -= OnRepeaterIsVisibleChanged;
                m_topNavFooterMenuRepeaterGettingFocusHelper?.Dispose();
                m_topNavFooterMenuRepeater = null;
            }

            m_footerItemsCollectionChangedRevoker?.Revoke();
            m_menuItemsCollectionChangedRevoker?.Revoke();

            if (m_topNavRepeaterOverflowView != null)
            {
                m_topNavRepeaterOverflowView.ElementPrepared -= OnRepeaterElementPrepared;
                m_topNavRepeaterOverflowView.ElementClearing -= OnRepeaterElementClearing;
                m_topNavRepeaterOverflowView = null;
            }

            m_topNavOverflowItemsCollectionChangedRevoker?.Revoke();

            if (isFromDestructor)
            {
                m_selectionModel.SelectionChanged -= OnSelectionModelSelectionChanged;
            }
        }

        static NavigationView()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(NavigationView), new FrameworkPropertyMetadata(typeof(NavigationView)));
        }

        public NavigationView()
        {
            SetValue(TemplateSettingsPropertyKey, new NavigationViewTemplateSettings());

            SizeChanged += OnSizeChanged;

            m_selectionModelSource = new List<object>(2);
            m_selectionModelSource.Add(null);
            m_selectionModelSource.Add(null);

            var items = new ObservableCollection<object>();
            SetValue(MenuItemsPropertyKey, items);

            var footerItems = new ObservableCollection<object>();
            SetValue(FooterMenuItemsPropertyKey, footerItems);

            var weakThis = new WeakReference<NavigationView>(this);
            m_topDataProvider.OnRawDataChanged(
                args =>
                {
                    if (weakThis.TryGetTarget(out var target))
                    {
                        target.OnTopNavDataSourceChanged(args);
                    }
                });

            Unloaded += OnUnloaded;
            Loaded += OnLoaded;

            m_selectionModel.SingleSelect = true;
            m_selectionModel.Source = m_selectionModelSource;
            m_selectionModel.SelectionChanged += OnSelectionModelSelectionChanged;
            m_selectionModel.ChildrenRequested += OnSelectionModelChildrenRequested;

            m_navigationViewItemsFactory = new NavigationViewItemsFactory();

            m_bitmapCache = new BitmapCache();
#if NET462_OR_NEWER
            m_bitmapCache.RenderAtScale = VisualTreeHelper.GetDpi(this).PixelsPerDip;
#endif
        }

        void OnSelectionModelChildrenRequested(SelectionModel selectionModel, SelectionModelChildrenRequestedEventArgs e)
        {
            // this is main menu or footer
            if (e.SourceIndex.GetSize() == 1)
            {
                e.Children = e.Source;
            }
            else if (e.Source is NavigationViewItem nvi)
            {
                e.Children = GetChildren(nvi);
            }
            else if (GetChildrenForItemInIndexPath(e.SourceIndex, true /*forceRealize*/) is { } children)
            {
                e.Children = children;
            }
        }

        void OnFooterItemsSourceCollectionChanged(object sender, object e)
        {
            UpdateFooterRepeaterItemsSource(false /*sourceCollectionReset*/, true /*sourceCollectionChanged*/);

            // Pane footer items changed. This means we might need to reevaluate the pane layout.
            UpdatePaneLayout();
        }

        void OnOverflowItemsSourceCollectionChanged(object sender, object e)
        {
            if (m_topNavRepeaterOverflowView.ItemsSourceView.Count == 0)
            {
                SetOverflowButtonVisibility(Visibility.Collapsed);
            }
        }

        void OnSelectionModelSelectionChanged(SelectionModel selectionModel, SelectionModelSelectionChangedEventArgs e)
        {
            var selectedItem = selectionModel.SelectedItem;

            // Ignore this callback if:
            // 1. the SelectedItem property of NavigationView is already set to the item
            //    being passed in this callback. This is because the item has already been selected
            //    via API and we are just updating the m_selectionModel state to accurately reflect the new selection.
            // 2. Template has not been applied yet. SelectionModel's selectedIndex state will get properly updated
            //    after the repeater finishes loading.
            // TODO: Update SelectedItem comparison to work for the exact same item datasource scenario
            if (m_shouldIgnoreNextSelectionChange || selectedItem == SelectedItem || !m_appliedTemplate)
            {
                return;
            }

            bool setSelectedItem = true;
            var selectedIndex = selectionModel.SelectedIndex;

            if (IsTopNavigationView())
            {
                // If selectedIndex does not exist, means item is being deselected through API
                var isInOverflow = (selectedIndex != null && selectedIndex.GetSize() > 1)
                    ? selectedIndex.GetAt(0) == c_mainMenuBlockIndex && !m_topDataProvider.IsItemInPrimaryList(selectedIndex.GetAt(1))
                    : false;
                if (isInOverflow)
                {
                    // We only want to close the overflow flyout and move the item on selection if it is a leaf node
                    bool itemShouldBeMoved;
                    {
                        bool init()
                        {
                            if (GetContainerForIndexPath(selectedIndex) is { } selectedContainer)
                            {
                                if (selectedContainer is NavigationViewItem selectedNVI)
                                {
                                    if (DoesNavigationViewItemHaveChildren(selectedNVI))
                                    {
                                        return false;
                                    }
                                }
                            }
                            return true;
                        }
                        itemShouldBeMoved = init();
                    }

                    if (itemShouldBeMoved)
                    {
                        SelectandMoveOverflowItem(selectedItem, selectedIndex, true /*closeFlyout*/);
                        setSelectedItem = false;
                    }
                    else
                    {
                        m_moveTopNavOverflowItemOnFlyoutClose = true;
                    }
                }
            }

            if (setSelectedItem)
            {
                SetSelectedItemAndExpectItemInvokeWhenSelectionChangedIfNotInvokedFromAPI(selectedItem);
            }
        }

        void SelectandMoveOverflowItem(object selectedItem, IndexPath selectedIndex, bool closeFlyout)
        {
            // SelectOverflowItem is moving data in/out of overflow.
            try
            {
                m_selectionChangeFromOverflowMenu = true;

                if (closeFlyout)
                {
                    CloseTopNavigationViewFlyout();
                }

                if (!IsSelectionSuppressed(selectedItem))
                {
                    SelectOverflowItem(selectedItem, selectedIndex);
                }
            }
            finally
            {
                m_selectionChangeFromOverflowMenu = false;
            }
        }

        // We only need to close the flyout if the selected item is a leaf node
        void CloseFlyoutIfRequired(NavigationViewItem selectedItem)
        {
            var selectedIndex = m_selectionModel.SelectedIndex;
            bool isInModeWithFlyout;
            {
                bool init()
                {
                    if (m_rootSplitView is { } splitView)
                    {
                        // Check if the pane is closed and if the splitview is in either compact mode.
                        var splitViewDisplayMode = splitView.DisplayMode;
                        return (!splitView.IsPaneOpen && (splitViewDisplayMode == SplitViewDisplayMode.CompactOverlay || splitViewDisplayMode == SplitViewDisplayMode.CompactInline)) ||
                                PaneDisplayMode == NavigationViewPaneDisplayMode.Top;
                    }
                    return false;
                }
                isInModeWithFlyout = init();
            }

            if (isInModeWithFlyout && selectedIndex != null && !DoesNavigationViewItemHaveChildren(selectedItem))
            {
                // Item selected is a leaf node, find top level parent and close flyout
                if (GetContainerForIndex(selectedIndex.GetAt(1), selectedIndex.GetAt(0) == c_footerMenuBlockIndex /* inFooter */) is { } rootItem)
                {
                    if (rootItem is NavigationViewItem nvi)
                    {
                        var nviImpl = nvi;
                        if (nviImpl.ShouldRepeaterShowInFlyout())
                        {
                            nvi.IsExpanded = false;
                        }
                    }
                }
            }
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            // Stop update anything because of PropertyChange during OnApplyTemplate. Update them all together at the end of this function
            m_appliedTemplate = false;

            UnhookEventsAndClearFields();

            IControlProtected controlProtected = this;

            // Set up the pane toggle button click handler
            if (GetTemplateChild(c_togglePaneButtonName) is Button paneToggleButton)
            {
                m_paneToggleButton = paneToggleButton;
                paneToggleButton.Click += OnPaneToggleButtonClick;

                SetPaneToggleButtonAutomationName();

                // TODO: WPF - KeyboardAccelerator
                /*
                if (SharedHelpers::IsRS3OrHigher())
                {
                    winrt::KeyboardAccelerator keyboardAccelerator;
                    keyboardAccelerator.Key(winrt::VirtualKey::Back);
                    keyboardAccelerator.Modifiers(winrt::VirtualKeyModifiers::Windows);
                    paneToggleButton.KeyboardAccelerators().Append(keyboardAccelerator);
                }
                */

                WindowChrome.SetIsHitTestVisibleInChrome(paneToggleButton, true);
            }

            m_leftNavPaneHeaderContentBorder = GetTemplateChild(c_leftNavPaneHeaderContentBorder) as ContentControl;
            m_leftNavPaneCustomContentBorder = GetTemplateChild(c_leftNavPaneCustomContentBorder) as ContentControl;
            m_leftNavFooterContentBorder = GetTemplateChild(c_leftNavFooterContentBorder) as ContentControl;
            m_paneHeaderOnTopPane = GetTemplateChild(c_paneHeaderOnTopPane) as ContentControl;
            m_paneTitleOnTopPane = GetTemplateChild(c_paneTitleOnTopPane) as ContentControl;
            m_paneCustomContentOnTopPane = GetTemplateChild(c_paneCustomContentOnTopPane) as ContentControl;
            m_paneFooterOnTopPane = GetTemplateChild(c_paneFooterOnTopPane) as ContentControl;

            // Get a pointer to the root SplitView
            if (GetTemplateChild(c_rootSplitViewName) is SplitView splitView)
            {
                m_rootSplitView = splitView;
                splitView.IsPaneOpenChanged += OnSplitViewClosedCompactChanged;
                splitView.DisplayModeChanged += OnSplitViewClosedCompactChanged;

                if (SharedHelpers.IsRS3OrHigher()) // These events are new to RS3/v5 API
                {
                    splitView.PaneClosed += OnSplitViewPaneClosed;
                    splitView.PaneClosing += OnSplitViewPaneClosing;
                    splitView.PaneOpened += OnSplitViewPaneOpened;
                    splitView.PaneOpening += OnSplitViewPaneOpening;
                }

                UpdateIsClosedCompact();
            }

            m_topNavGrid = GetTemplateChild(c_topNavGrid) as Grid;

            // Change code to NOT do this if we're in top nav mode, to prevent it from being realized:
            if (GetTemplateChild(c_menuItemsHost) is ItemsRepeater leftNavRepeater)
            {
                m_leftNavRepeater = leftNavRepeater;

                // API is currently in preview, so setting this via code.
                // Disabling virtualization for now because of https://github.com/microsoft/microsoft-ui-xaml/issues/2095
                if (leftNavRepeater.Layout is StackLayout stackLayout)
                {
                    var stackLayoutImpl = stackLayout;
                    stackLayoutImpl.DisableVirtualization = true;
                }

                leftNavRepeater.ElementPrepared += OnRepeaterElementPrepared;
                leftNavRepeater.ElementClearing += OnRepeaterElementClearing;

                leftNavRepeater.IsVisibleChanged += OnRepeaterIsVisibleChanged;

                m_leftNavRepeaterGettingFocusHelper = new GettingFocusHelper(leftNavRepeater);
                m_leftNavRepeaterGettingFocusHelper.GettingFocus += OnRepeaterGettingFocus;

                leftNavRepeater.ItemTemplate = m_navigationViewItemsFactory;
            }

            // Change code to NOT do this if we're in left nav mode, to prevent it from being realized:
            if (GetTemplateChild(c_topNavMenuItemsHost) is ItemsRepeater topNavRepeater)
            {
                m_topNavRepeater = topNavRepeater;

                // API is currently in preview, so setting this via code
                if (topNavRepeater.Layout is StackLayout stackLayout)
                {
                    var stackLayoutImpl = stackLayout;
                    stackLayoutImpl.DisableVirtualization = true;
                }

                topNavRepeater.ElementPrepared += OnRepeaterElementPrepared;
                topNavRepeater.ElementClearing += OnRepeaterElementClearing;

                topNavRepeater.IsVisibleChanged += OnRepeaterIsVisibleChanged;

                m_topNavRepeaterGettingFocusHelper = new GettingFocusHelper(topNavRepeater);
                m_topNavRepeaterGettingFocusHelper.GettingFocus += OnRepeaterGettingFocus;

                topNavRepeater.ItemTemplate = m_navigationViewItemsFactory;
            }

            // Change code to NOT do this if we're in left nav mode, to prevent it from being realized:
            if (GetTemplateChild(c_topNavMenuItemsOverflowHost) is ItemsRepeater topNavListOverflowRepeater)
            {
                m_topNavRepeaterOverflowView = topNavListOverflowRepeater;

                // API is currently in preview, so setting this via code.
                // Disabling virtualization for now because of https://github.com/microsoft/microsoft-ui-xaml/issues/2095
                if (topNavListOverflowRepeater.Layout is StackLayout stackLayout)
                {
                    var stackLayoutImpl = stackLayout;
                    stackLayoutImpl.DisableVirtualization = true;
                }

                topNavListOverflowRepeater.ElementPrepared += OnRepeaterElementPrepared;
                topNavListOverflowRepeater.ElementClearing += OnRepeaterElementClearing;

                topNavListOverflowRepeater.ItemTemplate = m_navigationViewItemsFactory;
            }

            if (GetTemplateChild(c_topNavOverflowButton) is Button topNavOverflowButton)
            {
                m_topNavOverflowButton = topNavOverflowButton;
                AutomationProperties.SetName(topNavOverflowButton, ResourceAccessor.GetLocalizedStringResource(SR_NavigationOverflowButtonName));
                topNavOverflowButton.Content = ResourceAccessor.GetLocalizedStringResource(SR_NavigationOverflowButtonText);
                // TODO: WPF - Header Animation
                /*
                auto visual = winrt::ElementCompositionPreview::GetElementVisual(topNavOverflowButton);
                CreateAndAttachHeaderAnimation(visual);
                */

                var toolTip = ToolTipService.GetToolTip(topNavOverflowButton);
                if (toolTip is null)
                {
                    var tooltip = new ToolTip();
                    tooltip.Content = ResourceAccessor.GetLocalizedStringResource(SR_NavigationOverflowButtonToolTip);
                    ToolTipService.SetToolTip(topNavOverflowButton, tooltip);
                }

                if (FlyoutService.GetFlyout(topNavOverflowButton) is { } flyoutBase)
                {
                    /*
                    if (winrt::IFlyoutBase6 topNavOverflowButtonAsFlyoutBase6 = flyoutBase)
                    {
                        topNavOverflowButtonAsFlyoutBase6.ShouldConstrainToRootBounds(false);
                    }
                    */
                    flyoutBase.Closing += OnFlyoutClosing;
                    flyoutBase.Offset = 0;
                }
            }

            // Change code to NOT do this if we're in top nav mode, to prevent it from being realized:
            if (GetTemplateChildT<ItemsRepeater>(c_footerMenuItemsHost, controlProtected) is { } leftFooterMenuNavRepeater)
            {
                m_leftNavFooterMenuRepeater = leftFooterMenuNavRepeater;

                // API is currently in preview, so setting this via code.
                // Disabling virtualization for now because of https://github.com/microsoft/microsoft-ui-xaml/issues/2095
                if (leftFooterMenuNavRepeater.Layout is StackLayout stackLayout)
                {
                    var stackLayoutImpl = stackLayout;
                    stackLayoutImpl.DisableVirtualization = true;
                }

                leftFooterMenuNavRepeater.ElementPrepared += OnRepeaterElementPrepared;
                leftFooterMenuNavRepeater.ElementClearing += OnRepeaterElementClearing;

                leftFooterMenuNavRepeater.IsVisibleChanged += OnRepeaterIsVisibleChanged;

                m_leftNavFooterMenuRepeaterGettingFocusHelper = new GettingFocusHelper(leftFooterMenuNavRepeater);
                m_leftNavFooterMenuRepeaterGettingFocusHelper.GettingFocus += OnRepeaterGettingFocus;

                leftFooterMenuNavRepeater.ItemTemplate = m_navigationViewItemsFactory;
            }

            // Change code to NOT do this if we're in left nav mode, to prevent it from being realized:
            if (GetTemplateChildT<ItemsRepeater>(c_topNavFooterMenuItemsHost, controlProtected) is { } topFooterMenuNavRepeater)
            {
                m_topNavFooterMenuRepeater = topFooterMenuNavRepeater;

                // API is currently in preview, so setting this via code.
                // Disabling virtualization for now because of https://github.com/microsoft/microsoft-ui-xaml/issues/2095
                if (topFooterMenuNavRepeater.Layout is StackLayout stackLayout)
                {
                    var stackLayoutImpl = stackLayout;
                    stackLayoutImpl.DisableVirtualization = true;
                }

                topFooterMenuNavRepeater.ElementPrepared += OnRepeaterElementPrepared;
                topFooterMenuNavRepeater.ElementClearing += OnRepeaterElementClearing;

                topFooterMenuNavRepeater.IsVisibleChanged += OnRepeaterIsVisibleChanged;

                m_topNavFooterMenuRepeaterGettingFocusHelper = new GettingFocusHelper(topFooterMenuNavRepeater);
                m_topNavFooterMenuRepeaterGettingFocusHelper.GettingFocus += OnRepeaterGettingFocus;

                topFooterMenuNavRepeater.ItemTemplate = m_navigationViewItemsFactory;
            }

            m_topNavContentOverlayAreaGrid = GetTemplateChild(c_topNavContentOverlayAreaGrid) as Border;
            m_leftNavPaneAutoSuggestBoxPresenter = GetTemplateChild(c_leftNavPaneAutoSuggestBoxPresenter) as ContentControl;
            m_topNavPaneAutoSuggestBoxPresenter = GetTemplateChild(c_topNavPaneAutoSuggestBoxPresenter) as ContentControl;

            // Get pointer to the pane content area, for use in the selection indicator animation
            m_paneContentGrid = GetTemplateChild(c_paneContentGridName) as UIElement;

            m_contentLeftPadding = GetTemplateChild(c_contentLeftPadding) as FrameworkElement;

            m_paneHeaderCloseButtonColumn = GetTemplateChild(c_paneHeaderCloseButtonColumn) as ColumnDefinition;
            m_paneHeaderToggleButtonColumn = GetTemplateChild(c_paneHeaderToggleButtonColumn) as ColumnDefinition;
            m_paneHeaderContentBorderRow = GetTemplateChild(c_paneHeaderContentBorderRow) as RowDefinition;
            m_paneTitleFrameworkElement = GetTemplateChild(c_paneTitleFrameworkElement) as FrameworkElement;
            m_paneTitlePresenter = GetTemplateChild(c_paneTitlePresenter) as ContentControl;

            if (GetTemplateChild(c_paneTitleHolderFrameworkElement) is FrameworkElement paneTitleHolderFrameworkElement)
            {
                m_paneTitleHolderFrameworkElement = paneTitleHolderFrameworkElement;
                paneTitleHolderFrameworkElement.SizeChanged += OnPaneTitleHolderSizeChanged;
            }

            // Set automation name on search button
            if (GetTemplateChild(c_searchButtonName) is Button button)
            {
                m_paneSearchButton = button;
                button.Click += OnPaneSearchButtonClick;

                var searchButtonName = ResourceAccessor.GetLocalizedStringResource(SR_NavigationViewSearchButtonName);
                AutomationProperties.SetName(button, searchButtonName);
                var toolTip = new ToolTip();
                toolTip.Content = searchButtonName;
                ToolTipService.SetToolTip(button, toolTip);
            }

            if (GetTemplateChild(c_navViewBackButton) is Button backButton)
            {
                m_backButton = backButton;
                backButton.Click += OnBackButtonClicked;

                string navigationName = ResourceAccessor.GetLocalizedStringResource(SR_NavigationBackButtonName);
                AutomationProperties.SetName(backButton, navigationName);

                WindowChrome.SetIsHitTestVisibleInChrome(backButton, true);
            }

            // Register for changes in title bar layout
            if (CoreApplicationViewTitleBar.GetTitleBar(this) is { } coreTitleBar)
            {
                m_coreTitleBar = coreTitleBar;
                coreTitleBar.LayoutMetricsChanged += OnTitleBarMetricsChanged;
                coreTitleBar.IsVisibleChanged += OnTitleBarIsVisibleChanged;

                if (ShouldPreserveNavigationViewRS4Behavior())
                {
                    m_togglePaneTopPadding = GetTemplateChild(c_togglePaneTopPadding) as FrameworkElement;
                    m_contentPaneTopPadding = GetTemplateChild(c_contentPaneTopPadding) as FrameworkElement;
                }
            }

            if (GetTemplateChild(c_navViewBackButtonToolTip) is ToolTip backButtonToolTip)
            {
                string navigationBackButtonToolTip = ResourceAccessor.GetLocalizedStringResource(SR_NavigationBackButtonToolTip);
                backButtonToolTip.Content = navigationBackButtonToolTip;
            }

            if (GetTemplateChild(c_navViewCloseButton) is Button closeButton)
            {
                m_closeButton = closeButton;
                closeButton.Click += OnPaneToggleButtonClick;

                string navigationName = ResourceAccessor.GetLocalizedStringResource(SR_NavigationCloseButtonName);
                AutomationProperties.SetName(closeButton, navigationName);

                WindowChrome.SetIsHitTestVisibleInChrome(closeButton, true);
            }

            if (GetTemplateChild(c_navViewCloseButtonToolTip) is ToolTip closeButtonToolTip)
            {
                string navigationCloseButtonToolTip = ResourceAccessor.GetLocalizedStringResource(SR_NavigationButtonOpenName);
                closeButtonToolTip.Content = navigationCloseButtonToolTip;
            }

            m_itemsContainerRow = GetTemplateChildT<RowDefinition>(c_itemsContainerRow, controlProtected);
            m_menuItemsScrollViewer = GetTemplateChildT<FrameworkElement>(c_menuItemsScrollViewer, controlProtected);
            m_footerItemsScrollViewer = GetTemplateChildT<FrameworkElement>(c_footerItemsScrollViewer, controlProtected);
            m_visualItemsSeparator = GetTemplateChildT<FrameworkElement>(c_visualItemsSeparator, controlProtected);

            m_itemsContainerSizeChangedRevoker?.Revoke();
            if (GetTemplateChildT<FrameworkElement>(c_itemsContainer, controlProtected) is { } itemsContainerRow)
            {
                m_itemsContainerSizeChangedRevoker = new FrameworkElementSizeChangedRevoker(itemsContainerRow, OnItemsContainerSizeChanged);
            }

            if (SharedHelpers.IsRS2OrHigher())
            {
                // Get hold of the outermost grid and enable XYKeyboardNavigationMode
                // However, we only want this to work in the content pane + the hamburger button (which is not inside the splitview)
                // so disable it on the grid in the content area of the SplitView
                if (GetTemplateChildT<Grid>(c_rootGridName, controlProtected) is { } rootGrid)
                {
                    KeyboardNavigation.SetDirectionalNavigation(rootGrid, KeyboardNavigationMode.Contained);
                }

                if (GetTemplateChildT<Grid>(c_contentGridName, controlProtected) is { } contentGrid)
                {
                    KeyboardNavigation.SetDirectionalNavigation(contentGrid, KeyboardNavigationMode.None);
                }
            }

            // TODO: WPF - AccessKey
            //m_accessKeyInvokedRevoker = AccessKeyInvoked(winrt::auto_revoke, { this, &NavigationView::OnAccessKeyInvoked });

            UpdatePaneShadow();

            m_appliedTemplate = true;

            // Do initial setup
            UpdatePaneDisplayMode();
            UpdateHeaderVisibility();
            UpdatePaneTitleFrameworkElementParents();
            UpdateTitleBarPadding();
            UpdatePaneTabFocusNavigation();
            UpdateBackAndCloseButtonsVisibility();
            UpdateSingleSelectionFollowsFocusTemplateSetting();
            UpdatePaneVisibility();
            UpdateVisualState();
            UpdatePaneTitleMargins();
            UpdatePaneLayout();
        }

        void UpdateRepeaterItemsSource(bool forceSelectionModelUpdate)
        {
            object itemsSource;
            {
                object init()
                {
                    if (MenuItemsSource is { } menuItemsSource)
                    {
                        return menuItemsSource;
                    }
                    else
                    {
                        UpdateSelectionForMenuItems();
                        return MenuItems;
                    }
                };
                itemsSource = init();
            }

            // Selection Model has same representation of data regardless
            // of pane mode, so only update if the ItemsSource data itself
            // has changed.
            if (forceSelectionModelUpdate)
            {
                m_selectionModelSource[0] = itemsSource;
            }

            m_menuItemsCollectionChangedRevoker?.Revoke();
            m_menuItemsSource = new InspectingDataSource(itemsSource);
            m_menuItemsCollectionChangedRevoker = new ItemsSourceView.CollectionChangedRevoker(m_menuItemsSource, OnMenuItemsSourceCollectionChanged);

            if (IsTopNavigationView())
            {
                UpdateLeftRepeaterItemSource(null);
                UpdateTopNavRepeatersItemSource(itemsSource);
                InvalidateTopNavPrimaryLayout();
            }
            else
            {
                UpdateTopNavRepeatersItemSource(null);
                UpdateLeftRepeaterItemSource(itemsSource);
            }
        }

        void UpdateLeftRepeaterItemSource(object items)
        {
            UpdateItemsRepeaterItemsSource(m_leftNavRepeater, items);
            // Left pane repeater has a new items source, update pane layout.
            UpdatePaneLayout();
        }

        void UpdateTopNavRepeatersItemSource(object items)
        {
            // Change data source and setup vectors
            m_topDataProvider.SetDataSource(items);

            // rebinding
            UpdateTopNavPrimaryRepeaterItemsSource(items);
            UpdateTopNavOverflowRepeaterItemsSource(items);
        }

        void UpdateTopNavPrimaryRepeaterItemsSource(object items)
        {
            if (items != null)
            {
                UpdateItemsRepeaterItemsSource(m_topNavRepeater, m_topDataProvider.GetPrimaryItems());
            }
            else
            {
                UpdateItemsRepeaterItemsSource(m_topNavRepeater, null);
            }
        }

        void UpdateTopNavOverflowRepeaterItemsSource(object items)
        {
            m_topNavOverflowItemsCollectionChangedRevoker?.Revoke();

            if (m_topNavRepeaterOverflowView is { } overflowRepeater)
            {
                if (items != null)
                {
                    var itemsSource = m_topDataProvider.GetOverflowItems();
                    overflowRepeater.ItemsSource = itemsSource;

                    // We listen to changes to the overflow menu item collection so we can set the visibility of the overflow button
                    // to collapsed when it no longer has any items.
                    //
                    // Normally, MeasureOverride() kicks off updating the button's visibility, however, it is not run when the overflow menu
                    // only contains a *single* item and we
                    // - either remove that menu item or
                    // - remove menu items displayed in the NavigationView pane until there is enough room for the single overflow menu item
                    //   to be displayed in the pane
                    m_topNavOverflowItemsCollectionChangedRevoker = new ItemsSourceView.CollectionChangedRevoker(overflowRepeater.ItemsSourceView, OnOverflowItemsSourceCollectionChanged);
                }
                else
                {
                    overflowRepeater.ItemsSource = null;
                }
            }
        }

        void UpdateItemsRepeaterItemsSource(ItemsRepeater ir,
             object itemsSource)
        {
            if (ir != null)
            {
                ir.ItemsSource = itemsSource;
            }
        }

        void UpdateFooterRepeaterItemsSource(bool sourceCollectionReset, bool sourceCollectionChanged)
        {
            if (!m_appliedTemplate) return;

            object itemsSource;
            {
                itemsSource = init();
                object init()
                {
                    if (FooterMenuItemsSource is { } menuItemsSource)
                    {
                        return menuItemsSource;
                    }
                    UpdateSelectionForMenuItems();
                    return FooterMenuItems;
                }
            }


            UpdateItemsRepeaterItemsSource(m_leftNavFooterMenuRepeater, null);
            UpdateItemsRepeaterItemsSource(m_topNavFooterMenuRepeater, null);

            if (m_settingsItem is null || sourceCollectionChanged || sourceCollectionReset)
            {
                var dataSource = new List<object>();

                if (m_settingsItem is null)
                {
                    m_settingsItem = new NavigationViewItem();
                    var settingsItem = m_settingsItem;
                    settingsItem.Name = "SettingsItem";
                    m_navigationViewItemsFactory.SettingsItem(settingsItem);
                }

                if (sourceCollectionReset)
                {
                    if (m_footerItemsSource != null)
                    {
                        m_footerItemsSource.CollectionChanged -= OnFooterItemsSourceCollectionChanged;
                    }
                    m_footerItemsSource = null;
                }

                if (m_footerItemsSource is null)
                {
                    m_footerItemsSource = new InspectingDataSource(itemsSource);
                    m_footerItemsCollectionChangedRevoker = new ItemsSourceView.CollectionChangedRevoker(m_footerItemsSource, OnFooterItemsSourceCollectionChanged);
                }

                if (m_footerItemsSource != null)
                {
                    var settingsItem = m_settingsItem;
                    var size = m_footerItemsSource.Count;

                    for (int i = 0; i < size; i++)
                    {
                        var item = m_footerItemsSource.GetAt(i);
                        dataSource.Add(item);
                    }

                    if (IsSettingsVisible)
                    {
                        CreateAndHookEventsToSettings();
                        // add settings item to the end of footer
                        dataSource.Add(settingsItem);
                    }
                }

                m_selectionModelSource[1] = dataSource;
            }

            if (IsTopNavigationView())
            {
                UpdateItemsRepeaterItemsSource(m_topNavFooterMenuRepeater, m_selectionModelSource[1]);
            }
            else
            {
                if (m_leftNavFooterMenuRepeater is { } repeater)
                {
                    UpdateItemsRepeaterItemsSource(m_leftNavFooterMenuRepeater, m_selectionModelSource[1]);

                    // Footer items changed and we need to recalculate the layout.
                    // However repeater "lags" behind, so we need to force it to reevaluate itself now.
                    repeater.InvalidateMeasure();
                    repeater.UpdateLayout();

                    // Footer items changed, so let's update the pane layout.
                    UpdatePaneLayout();
                }

                if (m_settingsItem is { } settings)
                {
                    settings.BringIntoView();
                }
            }
        }

        void OnFlyoutClosing(object sender, FlyoutBaseClosingEventArgs args)
        {
            // If the user selected an parent item in the overflow flyout then the item has not been moved to top primary yet.
            // So we need to move it.
            if (m_moveTopNavOverflowItemOnFlyoutClose && !m_selectionChangeFromOverflowMenu)
            {
                m_moveTopNavOverflowItemOnFlyoutClose = false;

                var selectedIndex = m_selectionModel.SelectedIndex;
                if (selectedIndex.GetSize() > 0)
                {
                    if (GetContainerForIndex(selectedIndex.GetAt(1), false /*infooter*/) is { } firstContainer)
                    {
                        if (firstContainer is NavigationViewItem firstNVI)
                        {
                            // We want to collapse the top level item before we move it
                            firstNVI.IsExpanded = false;
                        }
                    }

                    SelectandMoveOverflowItem(SelectedItem, selectedIndex, false /*closeFlyout*/);
                }
            }
        }

        void OnNavigationViewItemIsSelectedPropertyChanged(DependencyObject sender, DependencyProperty args)
        {
            if (sender is NavigationViewItem nvi)
            {
                // Check whether the container that triggered this call back is the selected container
                bool isContainerSelectedInModel = IsContainerTheSelectedItemInTheSelectionModel(nvi);
                bool isSelectedInContainer = nvi.IsSelected;

                if (isSelectedInContainer && !isContainerSelectedInModel)
                {
                    var indexPath = GetIndexPathForContainer(nvi);
                    UpdateSelectionModelSelection(indexPath);
                }
                else if (!isSelectedInContainer && isContainerSelectedInModel)
                {
                    var indexPath = GetIndexPathForContainer(nvi);
                    var indexPathFromModel = m_selectionModel.SelectedIndex;

                    if (indexPathFromModel != null && indexPath.CompareTo(indexPathFromModel) == 0)
                    {
                        m_selectionModel.DeselectAt(indexPath);
                    }
                }

                if (isSelectedInContainer)
                {
                    nvi.IsChildSelected = false;
                }
            }
        }

        void OnNavigationViewItemExpandedPropertyChanged(DependencyObject sender, DependencyProperty args)
        {
            if (sender is NavigationViewItem nvi)
            {
                if (nvi.IsExpanded)
                {
                    RaiseExpandingEvent(nvi);
                }

                ShowHideChildrenItemsRepeater(nvi);

                if (!nvi.IsExpanded)
                {
                    RaiseCollapsedEvent(nvi);
                }
            }
        }

        void RaiseItemInvokedForNavigationViewItem(NavigationViewItem nvi)
        {
            object nextItem = null;
            var prevItem = SelectedItem;
            var parentIR = GetParentItemsRepeaterForContainer(nvi);

            if (parentIR.ItemsSourceView is { } itemsSourceView)
            {
                var inspectingDataSource = (InspectingDataSource)itemsSourceView;
                var itemIndex = parentIR.GetElementIndex(nvi);

                // Check that index is NOT -1, meaning it is actually realized
                if (itemIndex != -1)
                {
                    // Something went wrong, item might not be realized yet.
                    nextItem = inspectingDataSource.GetAt(itemIndex);
                }
            }

            // Determine the recommeded transition direction.
            // Any transitions other than `Default` only apply in top nav scenarios.

            NavigationRecommendedTransitionDirection recommendedDirection;
            {
                NavigationRecommendedTransitionDirection init()
                {
                    if (IsTopNavigationView() && nvi.SelectsOnInvoked)
                    {
                        bool isInOverflow = parentIR == m_topNavRepeaterOverflowView;
                        if (isInOverflow)
                        {
                            return NavigationRecommendedTransitionDirection.FromOverflow;
                        }
                        else if (prevItem != null)
                        {
                            return GetRecommendedTransitionDirection(NavigationViewItemBaseOrSettingsContentFromData(prevItem), nvi);
                        }
                    }
                    return NavigationRecommendedTransitionDirection.Default;
                };
                recommendedDirection = init();
            }

            RaiseItemInvoked(nextItem, IsSettingsItem(nvi) /*isSettings*/, nvi, recommendedDirection);
        }

        internal void OnNavigationViewItemInvoked(NavigationViewItem nvi)
        {
            m_shouldRaiseItemInvokedAfterSelection = true;

            var selectedItem = SelectedItem;
            bool updateSelection = m_selectionModel != null && nvi.SelectsOnInvoked;
            if (updateSelection)
            {
                var ip = GetIndexPathForContainer(nvi);

                // Determine if we will update collapse/expand which will happen iff the item has children
                if (DoesNavigationViewItemHaveChildren(nvi))
                {
                    m_shouldIgnoreUIASelectionRaiseAsExpandCollapseWillRaise = true;
                }
                UpdateSelectionModelSelection(ip);
            }

            // Item was invoked but already selected, so raise event here.
            if (selectedItem == SelectedItem)
            {
                RaiseItemInvokedForNavigationViewItem(nvi);
            }

            ToggleIsExpandedNavigationViewItem(nvi);
            ClosePaneIfNeccessaryAfterItemIsClicked(nvi);

            if (updateSelection)
            {
                CloseFlyoutIfRequired(nvi);
            }
        }

        bool IsRootItemsRepeater(DependencyObject element)
        {
            if (element != null)
            {
                return (element == m_topNavRepeater ||
                    element == m_leftNavRepeater ||
                    element == m_topNavRepeaterOverflowView ||
                    element == m_leftNavFooterMenuRepeater ||
                    element == m_topNavFooterMenuRepeater);
            }
            return false;
        }

        bool IsRootGridOfFlyout(DependencyObject element)
        {
            if (element is Grid grid)
            {
                return grid.Name == c_flyoutRootGrid;
            }
            return false;
        }

        ItemsRepeater GetParentRootItemsRepeaterForContainer(NavigationViewItemBase nvib)
        {
            var parentIR = GetParentItemsRepeaterForContainer(nvib);
            var currentNvib = nvib;
            while (!IsRootItemsRepeater(parentIR))
            {
                currentNvib = GetParentNavigationViewItemForContainer(currentNvib);
                if (currentNvib is null)
                {
                    return null;
                }

                parentIR = GetParentItemsRepeaterForContainer(currentNvib);
            }
            return parentIR;
        }

        internal ItemsRepeater GetParentItemsRepeaterForContainer(NavigationViewItemBase nvib)
        {
            if (VisualTreeHelper.GetParent(nvib) is { } parent)
            {
                if (parent is ItemsRepeater parentIR)
                {
                    return parentIR;
                }
            }
            return null;
        }

        NavigationViewItem GetParentNavigationViewItemForContainer(NavigationViewItemBase nvib)
        {
            // TODO: This scenario does not find parent items when in a flyout, which causes problems if item if first loaded
            // straight in the flyout. Fix. This logic can be merged with the 'GetIndexPathForContainer' logic below.
            DependencyObject parent = GetParentItemsRepeaterForContainer(nvib);
            if (!IsRootItemsRepeater(parent))
            {
                while (parent != null)
                {
                    parent = VisualTreeHelper.GetParent(parent);
                    if (parent is NavigationViewItem nvi)
                    {
                        return nvi;
                    }
                }
            }
            return null;
        }

        IndexPath GetIndexPathForContainer(NavigationViewItemBase nvib)
        {
            var path = new List<int>();
            bool isInFooterMenu = false;

            DependencyObject child = nvib;
            var parent = VisualTreeHelper.GetParent(child);
            if (parent == null)
            {
                return IndexPath.CreateFromIndices(path);
            }

            // Search through VisualTree for a root itemsrepeater
            while (parent != null && !IsRootItemsRepeater(parent) && !IsRootGridOfFlyout(parent))
            {
                if (parent is ItemsRepeater parentIR)
                {
                    if (child is UIElement childElement)
                    {
                        path.Insert(0, parentIR.GetElementIndex(childElement));
                    }
                }
                child = parent;
                parent = VisualTreeHelper.GetParent(parent);
            }

            // If the item is in a flyout, then we need to final index of its parent
            if (IsRootGridOfFlyout(parent))
            {
                if (m_lastItemExpandedIntoFlyout is { } nvi)
                {
                    child = nvi;
                    parent = IsTopNavigationView() ? m_topNavRepeater : m_leftNavRepeater;
                }
            }

            // If item is in one of the disconnected ItemRepeaters, account for that in IndexPath calculations
            if (parent == m_topNavRepeaterOverflowView)
            {
                // Convert index of selected item in overflow to index in datasource
                var containerIndex = m_topNavRepeaterOverflowView.GetElementIndex(child as UIElement);
                var item = m_topDataProvider.GetOverflowItems()[containerIndex];
                var indexAtRoot = m_topDataProvider.IndexOf(item);
                path.Insert(0, indexAtRoot);
            }
            else if (parent == m_topNavRepeater)
            {
                // Convert index of selected item in overflow to index in datasource
                var containerIndex = m_topNavRepeater.GetElementIndex(child as UIElement);
                var item = m_topDataProvider.GetPrimaryItems()[containerIndex];
                var indexAtRoot = m_topDataProvider.IndexOf(item);
                path.Insert(0, indexAtRoot);
            }
            else if (parent is ItemsRepeater parentIR)
            {
                path.Insert(0, parentIR.GetElementIndex(child as UIElement));
            }

            isInFooterMenu = parent == m_leftNavFooterMenuRepeater || parent == m_topNavFooterMenuRepeater;

            path.Insert(0, isInFooterMenu ? c_footerMenuBlockIndex : c_mainMenuBlockIndex);

            return IndexPath.CreateFromIndices(path);
        }

        internal void OnRepeaterElementPrepared(ItemsRepeater ir, ItemsRepeaterElementPreparedEventArgs args)
        {
            if (args.Element is NavigationViewItemBase nvib)
            {
                var nvibImpl = nvib;
                nvibImpl.SetNavigationViewParent(this);
                nvibImpl.IsTopLevelItem = IsTopLevelItem(nvib);

                // Visual state info propagation
                NavigationViewRepeaterPosition position;
                {
                    NavigationViewRepeaterPosition init()
                    {
                        if (IsTopNavigationView())
                        {
                            if (ir == m_topNavRepeater)
                            {
                                return NavigationViewRepeaterPosition.TopPrimary;
                            }
                            if (ir == m_topNavFooterMenuRepeater)
                            {
                                return NavigationViewRepeaterPosition.TopFooter;
                            }
                            return NavigationViewRepeaterPosition.TopOverflow;
                        }
                        if (ir == m_leftNavFooterMenuRepeater)
                        {
                            return NavigationViewRepeaterPosition.LeftFooter;
                        }
                        return NavigationViewRepeaterPosition.LeftNav;
                    }
                    position = init();
                }
                nvibImpl.Position = position;

                if (GetParentNavigationViewItemForContainer(nvib) is { } parentNVI)
                {
                    var parentNVIImpl = parentNVI;
                    var itemDepth = parentNVIImpl.ShouldRepeaterShowInFlyout() ? 0 : parentNVIImpl.Depth + 1;
                    nvibImpl.Depth = itemDepth;
                }
                else
                {
                    nvibImpl.Depth = 0;
                }

                // Apply any custom container styling
                ApplyCustomMenuItemContainerStyling(nvib, ir, args.Index);

                if (args.Element is NavigationViewItem nvi)
                {
                    // Propagate depth to children items if they exist
                    int childDepth;
                    {
                        int init()
                        {
                            if (position == NavigationViewRepeaterPosition.TopPrimary)
                            {
                                return 0;
                            }
                            return nvibImpl.Depth + 1;

                        }
                        childDepth = init();
                    }
                    nvi.PropagateDepthToChildren(childDepth);

                    // Register for item events
                    InputHelper.AddTappedHandler(nvi, OnNavigationViewItemTapped);
                    nvi.KeyDown += OnNavigationViewItemKeyDown;
                    nvi.GotFocus += OnNavigationViewItemOnGotFocus;
                    nvi.IsSelectedChanged += OnNavigationViewItemIsSelectedPropertyChanged;
                    nvi.IsExpandedChanged += OnNavigationViewItemExpandedPropertyChanged;
                }
            }
        }

        void ApplyCustomMenuItemContainerStyling(NavigationViewItemBase nvib, ItemsRepeater ir, int index)
        {
            if (MenuItemContainerStyle is { } menuItemContainerStyle)
            {
                nvib.Style = menuItemContainerStyle;
            }
            else if (MenuItemContainerStyleSelector is { } menuItemContainerStyleSelector)
            {
                if (ir.ItemsSourceView is { } itemsSourceView)
                {
                    if (itemsSourceView.GetAt(index) is { } item)
                    {
                        if (menuItemContainerStyleSelector.SelectStyle(item, nvib) is { } selectedStyle)
                        {
                            nvib.Style = selectedStyle;
                        }
                    }
                }
            }
        }

        internal void OnRepeaterElementClearing(ItemsRepeater ir, ItemsRepeaterElementClearingEventArgs args)
        {
            if (args.Element is NavigationViewItemBase nvib)
            {
                var nvibImpl = nvib;
                nvibImpl.Depth = 0;
                nvibImpl.IsTopLevelItem = false;
                if (nvib is NavigationViewItem nvi)
                {
                    // Revoke all the events that we were listing to on the item
                    InputHelper.RemoveTappedHandler(nvi, OnNavigationViewItemTapped);
                    nvi.KeyDown -= OnNavigationViewItemKeyDown;
                    nvi.GotFocus -= OnNavigationViewItemOnGotFocus;
                    nvi.IsSelectedChanged -= OnNavigationViewItemIsSelectedPropertyChanged;
                    nvi.IsExpandedChanged -= OnNavigationViewItemExpandedPropertyChanged;
                }
            }
        }

        internal NavigationViewItemsFactory GetNavigationViewItemsFactory() { return m_navigationViewItemsFactory; }

        // Hook up the Settings Item Invoked event listener
        void CreateAndHookEventsToSettings()
        {
            if (m_settingsItem is null)
            {
                return;
            }

            var settingsItem = m_settingsItem;
            var settingsIcon = new SymbolIcon(Symbol.Setting);
            settingsItem.Icon = settingsIcon;

            // Do localization for settings item label and Automation Name
            var localizedSettingsName = ResourceAccessor.GetLocalizedStringResource(SR_SettingsButtonName);
            AutomationProperties.SetName(settingsItem, localizedSettingsName);
            settingsItem.Tag = localizedSettingsName;
            UpdateSettingsItemToolTip();

            // Add the name only in case of horizontal nav
            if (!IsTopNavigationView())
            {
                settingsItem.Content = localizedSettingsName;
            }
            else
            {
                settingsItem.Content = null;
            }

            // hook up SettingsItem
            SetValue(SettingsItemPropertyKey, settingsItem);
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            if (IsTopNavigationView() && IsTopPrimaryListVisible())
            {
                if (double.IsInfinity(availableSize.Width))
                {
                    // We have infinite space, so move all items to primary list
                    m_topDataProvider.MoveAllItemsToPrimaryList();
                }
                else
                {
                    HandleTopNavigationMeasureOverride(availableSize);
#if DEBUG
                    if (m_topDataProvider.Size() > 0)
                    {
                        // We should always have at least one item in primary.
                        Debug.Assert(m_topDataProvider.GetPrimaryItems().Count > 0);
                    }
#endif // DEBUG
                }
            }

            LayoutUpdated -= OnLayoutUpdated;
            LayoutUpdated += OnLayoutUpdated;
            m_layoutUpdatedToken = true;

            return base.MeasureOverride(availableSize);
        }

        void OnLayoutUpdated(object sender, object e)
        {
            // We only need to handle once after MeasureOverride, so revoke the token.
            LayoutUpdated -= OnLayoutUpdated;
            m_layoutUpdatedToken = false;

            // In topnav, when an item in overflow menu is clicked, the animation is delayed because that item is not move to primary list yet.
            // And it depends on LayoutUpdated to re-play the animation. m_lastSelectedItemPendingAnimationInTopNav is the last selected overflow item.
            if (m_lastSelectedItemPendingAnimationInTopNav is { } lastSelectedItemInTopNav)
            {
                m_lastSelectedItemPendingAnimationInTopNav = null;
                // WPF: Wait for layout
                Dispatcher.BeginInvoke(() =>
                {
                    AnimateSelectionChanged(lastSelectedItemInTopNav);
                }, DispatcherPriority.Send);
            }

            if (m_OrientationChangedPendingAnimation)
            {
                m_OrientationChangedPendingAnimation = false;
                AnimateSelectionChanged(SelectedItem);
            }
        }

        void OnSizeChanged(object sender, SizeChangedEventArgs args)
        {
            var width = args.NewSize.Width;
            UpdateAdaptiveLayout(width);
            UpdateTitleBarPadding();
            UpdateBackAndCloseButtonsVisibility();
            UpdatePaneLayout();
        }

        void OnItemsContainerSizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdatePaneLayout();
        }

        // forceSetDisplayMode: On first call to SetDisplayMode, force setting to initial values
        void UpdateAdaptiveLayout(double width, bool forceSetDisplayMode = false)
        {
            // In top nav, there is no adaptive pane layout
            if (IsTopNavigationView())
            {
                return;
            }

            if (m_rootSplitView == null)
            {
                return;
            }

            // If we decide we want it to animate open/closed when you resize the
            // window we'll have to change how we figure out the initial state
            // instead of this:
            m_initialListSizeStateSet = false; // see UpdateIsClosedCompact()

            NavigationViewDisplayMode displayMode = NavigationViewDisplayMode.Compact;

            var paneDisplayMode = PaneDisplayMode;
            if (paneDisplayMode == NavigationViewPaneDisplayMode.Auto)
            {
                if (width >= ExpandedModeThresholdWidth)
                {
                    displayMode = NavigationViewDisplayMode.Expanded;
                }
                else if (width < CompactModeThresholdWidth)
                {
                    displayMode = NavigationViewDisplayMode.Minimal;
                }
            }
            else if (paneDisplayMode == NavigationViewPaneDisplayMode.Left)
            {
                displayMode = NavigationViewDisplayMode.Expanded;
            }
            else if (paneDisplayMode == NavigationViewPaneDisplayMode.LeftCompact)
            {
                displayMode = NavigationViewDisplayMode.Compact;
            }
            else if (paneDisplayMode == NavigationViewPaneDisplayMode.LeftMinimal)
            {
                displayMode = NavigationViewDisplayMode.Minimal;
            }
            else
            {
                Environment.FailFast(null);
            }

            if (!forceSetDisplayMode && m_InitialNonForcedModeUpdate)
            {
                if (displayMode == NavigationViewDisplayMode.Minimal ||
                    displayMode == NavigationViewDisplayMode.Compact)
                {
                    ClosePane();
                }
                m_InitialNonForcedModeUpdate = false;
            }

            var previousMode = DisplayMode;
            SetDisplayMode(displayMode, forceSetDisplayMode);

            if (displayMode == NavigationViewDisplayMode.Expanded && IsPaneVisible)
            {
                if (!m_wasForceClosed)
                {
                    OpenPane();
                }
            }

            if (previousMode == NavigationViewDisplayMode.Expanded
                && displayMode == NavigationViewDisplayMode.Compact)
            {
                m_initialListSizeStateSet = false;
                ClosePane();
            }
        }

        void UpdatePaneLayout()
        {
            if (!IsTopNavigationView())
            {
                double totalAvailableHeight;
                {
                    totalAvailableHeight = init();
                    double init()
                    {
                        if (m_itemsContainerRow is { } paneContentRow)
                        {
                            // 20px is the padding between the two item lists
                            if (m_leftNavFooterContentBorder is { } paneFooter)
                            {
                                return paneContentRow.ActualHeight - 29 - paneFooter.ActualHeight;
                            }
                            else
                            {
                                return paneContentRow.ActualHeight - 29;
                            }
                        }
                        return 0.0;
                    }
                }

                // Only continue if we have a positive amount of space to manage.
                if (totalAvailableHeight > 0)
                {
                    // We need this value more than twice, so cache it.
                    var totalAvailableHeightHalf = totalAvailableHeight / 2;

                    double heightForMenuItems;
                    {
                        heightForMenuItems = init();
                        double init()
                        {
                            if (m_footerItemsScrollViewer is { } footerItemsScrollViewer)
                            {
                                if (m_leftNavFooterMenuRepeater is { } footerItemsRepeater)
                                {
                                    // We know the actual height of footer items, so use that to determine how to split pane.
                                    if (m_leftNavRepeater is { } menuItems)
                                    {
                                        var footersActualHeight = footerItemsRepeater.ActualHeight;
                                        var menuItemsActualHeight = menuItems.ActualHeight;
                                        if (totalAvailableHeight > menuItemsActualHeight + footersActualHeight)
                                        {
                                            // We have enough space for two so let everyone get as much as they need.
                                            footerItemsScrollViewer.MaxHeight = footersActualHeight;
                                            if (m_visualItemsSeparator is { } separator)
                                            {
                                                separator.Visibility = Visibility.Collapsed;
                                            }
                                            return totalAvailableHeight - footersActualHeight;
                                        }
                                        else if (menuItemsActualHeight <= totalAvailableHeightHalf)
                                        {
                                            // Footer items exceed over the half, so let's limit them.
                                            footerItemsScrollViewer.MaxHeight = totalAvailableHeight - menuItemsActualHeight;
                                            if (m_visualItemsSeparator is { } separator)
                                            {
                                                separator.Visibility = Visibility.Visible;
                                            }
                                            return menuItemsActualHeight;
                                        }
                                        else if (footersActualHeight <= totalAvailableHeightHalf)
                                        {
                                            // Menu items exceed over the half, so let's limit them.
                                            footerItemsScrollViewer.MaxHeight = footersActualHeight;
                                            if (m_visualItemsSeparator is { } separator)
                                            {
                                                separator.Visibility = Visibility.Visible;
                                            }
                                            return totalAvailableHeight - footersActualHeight;
                                        }
                                        else
                                        {
                                            // Both are more than half the height, so split evenly.
                                            footerItemsScrollViewer.MaxHeight = totalAvailableHeightHalf;
                                            if (m_visualItemsSeparator is { } separator)
                                            {
                                                separator.Visibility = Visibility.Visible;
                                            }
                                            return totalAvailableHeightHalf;
                                        }
                                    }
                                    else
                                    {
                                        // Couldn't determine the menuItems.
                                        // Let's just take all the height and let the other repeater deal with it.
                                        return totalAvailableHeight - footerItemsRepeater.ActualHeight;
                                    }
                                }
                                // We have no idea how much space to occupy as we are not able to get the size of the footer repeater.
                                // Stick with 50% as backup.
                                footerItemsScrollViewer.MaxHeight = totalAvailableHeightHalf;
                            }
                            // We couldn't find a good strategy, so limit to 50% percent for the menu items.
                            return totalAvailableHeightHalf;
                        }
                    }
                    // Footer items should have precedence as that usually contains very
                    // important items such as settings or the profile.

                    if (m_menuItemsScrollViewer is { } menuItemsScrollViewer)
                    {
                        // Update max height for menu items.
                        menuItemsScrollViewer.MaxHeight = heightForMenuItems;
                    }
                }
            }
        }

        void OnPaneToggleButtonClick(object sender, RoutedEventArgs args)
        {
            if (IsPaneOpen)
            {
                m_wasForceClosed = true;
                ClosePane();
            }
            else
            {
                m_wasForceClosed = false;
                OpenPane();
            }
        }

        void OnPaneSearchButtonClick(object sender, RoutedEventArgs args)
        {
            m_wasForceClosed = false;
            OpenPane();

            if (AutoSuggestBox is { } autoSuggestBox)
            {
                Dispatcher.BeginInvoke(() =>
                {
                    autoSuggestBox.Focus();
                }, DispatcherPriority.Loaded);
            }
        }

        void OnPaneTitleHolderSizeChanged(object sender, SizeChangedEventArgs args)
        {
            UpdateBackAndCloseButtonsVisibility();
        }

        void OpenPane()
        {
            try
            {
                m_isOpenPaneForInteraction = true;
                IsPaneOpen = true;
            }
            finally
            {
                m_isOpenPaneForInteraction = false;
            }
        }

        // Call this when you want an uncancellable close
        void ClosePane()
        {
            CollapseMenuItemsInRepeater(m_leftNavRepeater);
            try
            {
                m_isOpenPaneForInteraction = true;
                IsPaneOpen = false; // the SplitView is two-way bound to this value 
            }
            finally
            {
                m_isOpenPaneForInteraction = false;
            }
        }

        // Call this when NavigationView itself is going to trigger a close
        // where you will stop the close if the cancel is triggered
        bool AttemptClosePaneLightly()
        {
            bool pendingPaneClosingCancel = false;

            //if (SharedHelpers.IsRS3OrHigher())
            {
                var eventArgs = new NavigationViewPaneClosingEventArgs();
                PaneClosing?.Invoke(this, eventArgs);
                pendingPaneClosingCancel = eventArgs.Cancel;
            }

            if (!pendingPaneClosingCancel || m_wasForceClosed)
            {
                m_blockNextClosingEvent = true;
                ClosePane();
                return true;
            }

            return false;
        }

        void OnSplitViewClosedCompactChanged(DependencyObject sender, DependencyProperty args)
        {
            if (args == SplitView.IsPaneOpenProperty ||
                args == SplitView.DisplayModeProperty)
            {
                UpdateIsClosedCompact();
            }
        }

        void OnSplitViewPaneClosed(DependencyObject sender, object obj)
        {
            PaneClosed?.Invoke(this, null);
        }

        void OnSplitViewPaneClosing(DependencyObject sender, SplitViewPaneClosingEventArgs args)
        {
            bool pendingPaneClosingCancel = false;
            if (PaneClosing != null)
            {
                if (!m_blockNextClosingEvent) // If this is true, we already sent one out "manually" and don't need to forward SplitView's event
                {
                    var eventArgs = new NavigationViewPaneClosingEventArgs();
                    eventArgs.SplitViewClosingArgs(args);
                    PaneClosing(this, eventArgs);
                    pendingPaneClosingCancel = eventArgs.Cancel;
                }
                else
                {
                    m_blockNextClosingEvent = false;
                }
            }

            if (!pendingPaneClosingCancel) // will be set in above event!
            {
                if (m_rootSplitView is { } splitView)
                {
                    if (m_leftNavRepeater is { } paneList)
                    {
                        if (splitView.DisplayMode == SplitViewDisplayMode.CompactOverlay || splitView.DisplayMode == SplitViewDisplayMode.CompactInline)
                        {
                            // See UpdateIsClosedCompact 'RS3+ animation timing enhancement' for explanation:
                            VisualStateManager.GoToState(this, "ListSizeCompact", true /*useTransitions*/);
                            UpdatePaneToggleSize();
                        }
                    }
                }
            }
        }

        void OnSplitViewPaneOpened(DependencyObject sender, object obj)
        {
            PaneOpened?.Invoke(this, null);
        }

        void OnSplitViewPaneOpening(DependencyObject sender, object obj)
        {
            if (m_leftNavRepeater != null)
            {
                // See UpdateIsClosedCompact 'RS3+ animation timing enhancement' for explanation:
                VisualStateManager.GoToState(this, "ListSizeFull", true /*useTransitions*/);
            }

            PaneOpening?.Invoke(this, null);
        }

        void UpdateIsClosedCompact()
        {
            if (m_rootSplitView is { } splitView)
            {
                // Check if the pane is closed and if the splitview is in either compact mode.
                var splitViewDisplayMode = splitView.DisplayMode;
                m_isClosedCompact = !splitView.IsPaneOpen && (splitViewDisplayMode == SplitViewDisplayMode.CompactOverlay || splitViewDisplayMode == SplitViewDisplayMode.CompactInline);
                VisualStateManager.GoToState(this, m_isClosedCompact ? "ClosedCompact" : "NotClosedCompact", true /*useTransitions*/);

                // Set the initial state of the list size
                if (!m_initialListSizeStateSet)
                {
                    m_initialListSizeStateSet = true;
                    VisualStateManager.GoToState(this, m_isClosedCompact ? "ListSizeCompact" : "ListSizeFull", true /*useTransitions*/);
                }
                else if (false /*!SharedHelpers.IsRS3OrHigher()*/) // Do any changes that would otherwise happen on opening/closing for RS2 and earlier:
                {
                    // RS3+ animation timing enhancement:
                    // Pre-RS3, we didn't have the full suite of Closed, Closing, Opened,
                    // Opening events on SplitView. So when doing open/closed operations,
                    // we have to do them immediately. Just one example: on RS2 when you
                    // close the pane, the PaneTitle will disappear *immediately* which
                    // looks janky. But on RS4, it'll have its visibility set after the
                    // closed event fires.
                    VisualStateManager.GoToState(this, m_isClosedCompact ? "ListSizeCompact" : "ListSizeFull", true /*useTransitions*/);
                }

                UpdateTitleBarPadding();
                UpdateBackAndCloseButtonsVisibility();
                UpdatePaneTitleMargins();
                UpdatePaneToggleSize();
            }
        }

        void UpdatePaneButtonsWidths()
        {
            double newButtonWidths;
            {
                double init()
                {
                    if (DisplayMode == NavigationViewDisplayMode.Minimal)
                    {
                        return c_paneToggleButtonWidth;
                    }
                    return CompactPaneLength;
                }
                newButtonWidths = init();
            }

            if (m_backButton is { } backButton)
            {
                backButton.Width = newButtonWidths;
            }
            if (m_paneToggleButton is { } paneToggleButton)
            {
                paneToggleButton.MinWidth = newButtonWidths;
                if (paneToggleButton.GetTemplateChild<ColumnDefinition>(c_paneToggleButtonIconGridColumnName) is { } paneToggleButtonIconColumn)
                {
                    paneToggleButtonIconColumn.Width = new GridLength(newButtonWidths);
                }
            }
        }

        void OnBackButtonClicked(object sender, RoutedEventArgs args)
        {
            var eventArgs = new NavigationViewBackRequestedEventArgs();
            BackRequested?.Invoke(this, eventArgs);
        }

        bool IsOverlay()
        {
            if (m_rootSplitView is { } splitView)
            {
                return splitView.DisplayMode == SplitViewDisplayMode.Overlay;
            }
            else
            {
                return false;
            }
        }

        bool IsLightDismissible()
        {
            if (m_rootSplitView is { } splitView)
            {
                return splitView.DisplayMode != SplitViewDisplayMode.Inline && splitView.DisplayMode != SplitViewDisplayMode.CompactInline;
            }
            else
            {
                return false;
            }
        }

        bool ShouldShowBackButton()
        {
            if (m_backButton != null && !ShouldPreserveNavigationViewRS3Behavior())
            {
                if (DisplayMode == NavigationViewDisplayMode.Minimal && IsPaneOpen)
                {
                    return false;
                }

                return ShouldShowBackOrCloseButton();
            }

            return false;
        }

        bool ShouldShowCloseButton()
        {
            if (m_backButton != null && !ShouldPreserveNavigationViewRS3Behavior() && m_closeButton != null)
            {
                if (!IsPaneOpen)
                {
                    return false;
                }

                var paneDisplayMode = PaneDisplayMode;

                if (paneDisplayMode != NavigationViewPaneDisplayMode.LeftMinimal &&
                    (paneDisplayMode != NavigationViewPaneDisplayMode.Auto || DisplayMode != NavigationViewDisplayMode.Minimal))
                {
                    return false;
                }

                return ShouldShowBackOrCloseButton();
            }

            return false;
        }

        bool ShouldShowBackOrCloseButton()
        {
            var visibility = IsBackButtonVisible;
            return (visibility == NavigationViewBackButtonVisible.Visible || (visibility == NavigationViewBackButtonVisible.Auto && !SharedHelpers.IsOnXbox()));
        }

        // The automation name and tooltip for the pane toggle button changes depending on whether it is open or closed
        // put the logic here as it will be called in a couple places
        void SetPaneToggleButtonAutomationName()
        {
            string navigationName;
            if (IsPaneOpen)
            {
                navigationName = ResourceAccessor.GetLocalizedStringResource(SR_NavigationButtonOpenName);
            }
            else
            {
                navigationName = ResourceAccessor.GetLocalizedStringResource(SR_NavigationButtonClosedName);
            }

            if (m_paneToggleButton is { } paneToggleButton)
            {
                AutomationProperties.SetName(paneToggleButton, navigationName);
                var toolTip = new ToolTip();
                toolTip.Content = navigationName;
                ToolTipService.SetToolTip(paneToggleButton, toolTip);
            }
        }

        void UpdateSettingsItemToolTip()
        {
            if (m_settingsItem is { } settingsItem)
            {
                if (!IsTopNavigationView() && IsPaneOpen)
                {
                    ToolTipService.SetToolTip(settingsItem, null);
                }
                else
                {
                    var localizedSettingsName = ResourceAccessor.GetLocalizedStringResource(SR_SettingsButtonName);
                    var toolTip = new ToolTip();
                    toolTip.Content = localizedSettingsName;
                    ToolTipService.SetToolTip(settingsItem, toolTip);
                }
            }
        }

        // Updates the PaneTitleHolder.Visibility and PaneTitleTextBlock.Parent properties based on the PaneDisplayMode, PaneTitle and IsPaneToggleButtonVisible properties.
        void UpdatePaneTitleFrameworkElementParents()
        {
            if (m_paneTitleHolderFrameworkElement is { } paneTitleHolderFrameworkElement)
            {
                var isPaneToggleButtonVisible = IsPaneToggleButtonVisible;
                var isTopNavigationView = IsTopNavigationView();

                paneTitleHolderFrameworkElement.Visibility =
                    (isPaneToggleButtonVisible ||
                        isTopNavigationView ||
                        PaneTitle.Length == 0 ||
                        (PaneDisplayMode == NavigationViewPaneDisplayMode.LeftMinimal && !IsPaneOpen)) ?
                    Visibility.Collapsed : Visibility.Visible;

                if (m_paneTitleFrameworkElement is { } paneTitleFrameworkElement)
                {
                    var first = SetPaneTitleFrameworkElementParent(m_paneToggleButton, paneTitleFrameworkElement, isTopNavigationView || !isPaneToggleButtonVisible);
                    var second = SetPaneTitleFrameworkElementParent(m_paneTitlePresenter, paneTitleFrameworkElement, isTopNavigationView || isPaneToggleButtonVisible);
                    var third = SetPaneTitleFrameworkElementParent(m_paneTitleOnTopPane, paneTitleFrameworkElement, !isTopNavigationView || isPaneToggleButtonVisible);
                    (first ?? second ?? third)?.Invoke();
                }
            }
        }

        Action SetPaneTitleFrameworkElementParent(ContentControl parent, FrameworkElement paneTitle, bool shouldNotContainPaneTitle)
        {
            if (parent != null)
            {
                if ((parent.Content == paneTitle) == shouldNotContainPaneTitle)
                {
                    if (shouldNotContainPaneTitle)
                    {
                        parent.Content = null;
                    }
                    else
                    {
                        return () => { parent.Content = paneTitle; };
                    }
                }
            }
            return null;
        }

        static readonly Point c_frame1point1 = new Point(0.9, 0.1);
        static readonly Point c_frame1point2 = new Point(1.0, 0.2);
        static readonly Point c_frame2point1 = new Point(0.1, 0.9);
        static readonly Point c_frame2point2 = new Point(0.2, 1.0);

        void AnimateSelectionChangedToItem(object selectedItem)
        {
            if (selectedItem != null && !IsSelectionSuppressed(selectedItem))
            {
                AnimateSelectionChanged(selectedItem);
            }
        }

        // Please clear the field m_lastSelectedItemPendingAnimationInTopNav when calling this method to prevent garbage value and incorrect animation
        // when the layout is invalidated as it's called in OnLayoutUpdated.
        void AnimateSelectionChanged(object nextItem)
        {
            // If we are delaying animation due to item movement in top nav overflow, dont do anything
            if (m_lastSelectedItemPendingAnimationInTopNav != null)
            {
                return;
            }

            UIElement prevIndicator = m_activeIndicator;
            UIElement nextIndicator = FindSelectionIndicator(nextItem);

            bool haveValidAnimation = false;
            // It's possible that AnimateSelectionChanged is called multiple times before the first animation is complete.
            // To have better user experience, if the selected target is the same, keep the first animation
            // If the selected target is not the same, abort the first animation and launch another animation.
            if (m_prevIndicator != null || m_nextIndicator != null) // There is ongoing animation
            {
                if (nextIndicator != null && m_nextIndicator == nextIndicator) // animate to the same target, just wait for animation complete
                {
                    if (prevIndicator != null && prevIndicator != m_prevIndicator)
                    {
                        ResetElementAnimationProperties(prevIndicator, 0.0);
                    }
                    haveValidAnimation = true;
                }
                else
                {
                    // If the last animation is still playing, force it to complete.
                    OnAnimationComplete(null, null);
                }
            }

            if (!haveValidAnimation)
            {
                UIElement paneContentGrid = m_paneContentGrid;

                if ((prevIndicator != nextIndicator) && paneContentGrid != null && prevIndicator != null && nextIndicator != null && SharedHelpers.IsAnimationsEnabled)
                {
                    // Make sure both indicators are visible and in their original locations
                    ResetElementAnimationProperties(prevIndicator, 1.0);
                    ResetElementAnimationProperties(nextIndicator, 1.0);

                    // get the item positions in the pane
                    Point point = new Point(0, 0);
                    double prevPos;
                    double nextPos;

                    Point prevPosPoint = prevIndicator.SafeTransformToVisual(paneContentGrid).Transform(point);
                    Point nextPosPoint = nextIndicator.SafeTransformToVisual(paneContentGrid).Transform(point);
                    Size prevSize = prevIndicator.RenderSize;
                    Size nextSize = nextIndicator.RenderSize;

                    bool areElementsAtSameDepth = false;
                    if (IsTopNavigationView())
                    {
                        prevPos = prevPosPoint.X;
                        nextPos = nextPosPoint.X;
                        areElementsAtSameDepth = prevPosPoint.Y == nextPosPoint.Y;
                    }
                    else
                    {
                        prevPos = prevPosPoint.Y;
                        nextPos = nextPosPoint.Y;
                        areElementsAtSameDepth = prevPosPoint.X == nextPosPoint.X;
                    }

                    var storyboard = new Storyboard { FillBehavior = FillBehavior.Stop };

                    if (!areElementsAtSameDepth)
                    {
                        bool isNextBelow = prevPosPoint.Y < nextPosPoint.Y;
                        if (prevIndicator.RenderSize.Height > prevIndicator.RenderSize.Width)
                        {
                            PlayIndicatorNonSameLevelAnimations(prevIndicator, true, isNextBelow ? false : true, storyboard.Children);
                        }
                        else
                        {
                            PlayIndicatorNonSameLevelTopPrimaryAnimation(prevIndicator, true, storyboard.Children);
                        }

                        if (nextIndicator.RenderSize.Height > nextIndicator.RenderSize.Width)
                        {
                            PlayIndicatorNonSameLevelAnimations(nextIndicator, false, isNextBelow ? true : false, storyboard.Children);
                        }
                        else
                        {
                            PlayIndicatorNonSameLevelTopPrimaryAnimation(nextIndicator, false, storyboard.Children);
                        }

                    }
                    else
                    {

                        double outgoingEndPosition = nextPos - prevPos;
                        double incomingStartPosition = prevPos - nextPos;

                        // Play the animation on both the previous and next indicators
                        PlayIndicatorAnimations(prevIndicator,
                            0,
                            outgoingEndPosition,
                            prevSize,
                            nextSize,
                            true,
                            storyboard.Children);
                        PlayIndicatorAnimations(nextIndicator,
                            incomingStartPosition,
                            0,
                            prevSize,
                            nextSize,
                            false,
                            storyboard.Children);
                    }

                    m_prevIndicator = prevIndicator;
                    m_nextIndicator = nextIndicator;

                    storyboard.Completed += OnAnimationComplete;

                    storyboard.Begin(this, true);
                    storyboard.Pause(this);
                    storyboard.SeekAlignedToLastTick(this, TimeSpan.Zero, TimeSeekOrigin.BeginTime);
                    Dispatcher.BeginInvoke(() =>
                    {
                        storyboard.Resume(this);
                    }, DispatcherPriority.Loaded);
                }
                else
                {
                    // if all else fails, or if animations are turned off, attempt to correctly set the positions and opacities of the indicators.
                    ResetElementAnimationProperties(prevIndicator, 0.0);
                    ResetElementAnimationProperties(nextIndicator, 1.0);
                }

                m_activeIndicator = nextIndicator;
            }
        }

        void PlayIndicatorNonSameLevelAnimations(UIElement indicator, bool isOutgoing, bool fromTop, TimelineCollection animations)
        {
            // Determine scaling of indicator (whether it is appearing or dissapearing)
            double beginScale = isOutgoing ? 1.0 : 0.0;
            double endScale = isOutgoing ? 0.0 : 1.0;
            var scaleAnim = new DoubleAnimationUsingKeyFrames
            {
                KeyFrames =
                {
                    new DiscreteDoubleKeyFrame(beginScale, KeyTime.FromPercent(0.0)),
                    new SplineDoubleKeyFrame(endScale, KeyTime.FromPercent(1.0), new KeySpline(new Point(0.8,0), c_frame2point2)),
                },
                Duration = TimeSpan.FromMilliseconds(600)
            };
            animations.Add(scaleAnim);

            // Determine where the indicator is animating from/to
            Size size = indicator.RenderSize;
            double dimension = IsTopNavigationView() ? size.Width : size.Height;
            double newCenter = fromTop ? 0.0 : dimension;
            var indicatorCenterPoint = new Point();
            indicatorCenterPoint.Y = newCenter;

            Storyboard.SetTarget(scaleAnim, indicator);
            Storyboard.SetTargetProperty(scaleAnim, s_scaleYPath);
            PrepareIndicatorForAnimation(indicator, indicatorCenterPoint);
        }

        void PlayIndicatorNonSameLevelTopPrimaryAnimation(UIElement indicator, bool isOutgoing, TimelineCollection animations)
        {
            // Determine scaling of indicator (whether it is appearing or dissapearing)
            double beginScale = isOutgoing ? 1.0 : 0.0;
            double endScale = isOutgoing ? 0.0 : 1.0;
            var scaleAnim = new DoubleAnimationUsingKeyFrames
            {
                KeyFrames =
                {
                    new DiscreteDoubleKeyFrame(beginScale, KeyTime.FromPercent(0.0)),
                    new SplineDoubleKeyFrame(endScale, KeyTime.FromPercent(1.0), new KeySpline(new Point(0.8,0), c_frame2point2)),
                },
                Duration = TimeSpan.FromMilliseconds(600)
            };
            animations.Add(scaleAnim);

            // Determine where the indicator is animating from/to
            Size size = indicator.RenderSize;
            double newCenter = size.Width / 2;
            var indicatorCenterPoint = new Point();
            indicatorCenterPoint.Y = newCenter;

            Storyboard.SetTarget(scaleAnim, indicator);
            Storyboard.SetTargetProperty(scaleAnim, s_scaleXPath);
            PrepareIndicatorForAnimation(indicator, indicatorCenterPoint);
        }

        void PlayIndicatorAnimations(UIElement indicator, double from, double to, Size beginSize, Size endSize, bool isOutgoing, TimelineCollection animations)
        {
            Size size = indicator.RenderSize;
            double dimension = IsTopNavigationView() ? size.Width : size.Height;

            double beginScale = 1.0;
            double endScale = 1.0;
            if (IsTopNavigationView() && Math.Abs(size.Width) > 0.001f)
            {
                beginScale = beginSize.Width / size.Width;
                endScale = endSize.Width / size.Width;
            }

            var posAnim = new DoubleAnimationUsingKeyFrames
            {
                KeyFrames =
                {
                    new DiscreteDoubleKeyFrame(from < to ? from : (from + (dimension * (beginScale - 1))), KeyTime.FromPercent(0.0)),
                    new DiscreteDoubleKeyFrame(from < to ? (to + (dimension * (endScale - 1))) : to, KeyTime.FromPercent(0.333)),
                },
                Duration = TimeSpan.FromMilliseconds(600)
            };
            Storyboard.SetTarget(posAnim, indicator);
            animations.Add(posAnim);

            var scaleAnim = new DoubleAnimationUsingKeyFrames
            {
                KeyFrames =
                {
                    new DiscreteDoubleKeyFrame(beginScale, KeyTime.FromPercent(0.0)),
                    new SplineDoubleKeyFrame(
                        Math.Abs(to - from) / dimension + (from < to ? endScale : beginScale),
                        KeyTime.FromPercent(0.333),
                        new KeySpline(c_frame1point1, c_frame1point2)),
                    new SplineDoubleKeyFrame(endScale, KeyTime.FromPercent(1.0), new KeySpline(c_frame2point1, c_frame2point2)),
                },
                Duration = TimeSpan.FromMilliseconds(600)
            };
            Storyboard.SetTarget(scaleAnim, indicator);
            animations.Add(scaleAnim);

            var centerAnim = new DoubleAnimationUsingKeyFrames
            {
                KeyFrames =
                {
                    new DiscreteDoubleKeyFrame(from < to ? 0.0 : dimension, KeyTime.FromPercent(0.0)),
                    new DiscreteDoubleKeyFrame(from < to ? dimension : 0.0, KeyTime.FromPercent(1.0)),
                },
                Duration = TimeSpan.FromMilliseconds(200)
            };
            Storyboard.SetTarget(centerAnim, indicator);
            animations.Add(centerAnim);

            if (isOutgoing)
            {
                // fade the outgoing indicator so it looks nice when animating over the scroll area
                var opacityAnim = new DoubleAnimationUsingKeyFrames
                {
                    KeyFrames =
                    {
                        new DiscreteDoubleKeyFrame(1.0, KeyTime.FromPercent(0.0)),
                        new DiscreteDoubleKeyFrame(1.0, KeyTime.FromPercent(0.333)),
                        new SplineDoubleKeyFrame(0.0, KeyTime.FromPercent(1.0), new KeySpline(c_frame2point1, c_frame2point2)),
                    },
                    Duration = TimeSpan.FromMilliseconds(600)
                };
                Storyboard.SetTarget(opacityAnim, indicator);
                Storyboard.SetTargetProperty(opacityAnim, s_opacityPath);
                animations.Add(opacityAnim);
            }

            if (IsTopNavigationView())
            {
                Storyboard.SetTargetProperty(posAnim, s_translateXPath);
                Storyboard.SetTargetProperty(scaleAnim, s_scaleXPath);
                Storyboard.SetTargetProperty(centerAnim, s_centerXPath);
            }
            else
            {
                Storyboard.SetTargetProperty(posAnim, s_translateYPath);
                Storyboard.SetTargetProperty(scaleAnim, s_scaleYPath);
                Storyboard.SetTargetProperty(centerAnim, s_centerYPath);
            }

            PrepareIndicatorForAnimation(indicator);
        }

        void PrepareIndicatorForAnimation(UIElement indicator, Point? centerPoint = null)
        {
            if (!(indicator.RenderTransform is TransformGroup transformGroup &&
                  transformGroup.Children.Count == 2 &&
                  transformGroup.Children[0] is ScaleTransform &&
                  transformGroup.Children[1] is TranslateTransform))
            {
                indicator.RenderTransform = new TransformGroup
                {
                    Children =
                    {
                        new ScaleTransform(),
                        new TranslateTransform()
                    }
                };
            }

            if (centerPoint.HasValue)
            {
                var scaleTransform = (ScaleTransform)((TransformGroup)indicator.RenderTransform).Children[0];
                scaleTransform.CenterX = centerPoint.Value.X;
                scaleTransform.CenterY = centerPoint.Value.Y;
            }

            if (indicator.CacheMode == null)
            {
                indicator.CacheMode = m_bitmapCache;
            }
        }

        void OnAnimationComplete(object sender, EventArgs args)
        {
            var indicator = m_prevIndicator;
            ResetElementAnimationProperties(indicator, 0.0);
            m_prevIndicator = null;

            indicator = m_nextIndicator;
            ResetElementAnimationProperties(indicator, 1.0);
            m_nextIndicator = null;
        }

        void ResetElementAnimationProperties(UIElement element, double desiredOpacity)
        {
            if (element != null)
            {
                element.Opacity = desiredOpacity;

                if (element.RenderTransform is TransformGroup transformGroup &&
                    transformGroup.Children.Count == 2 &&
                    transformGroup.Children[0] is ScaleTransform scaleTransform &&
                    transformGroup.Children[1] is TranslateTransform translateTransform)
                {
                    translateTransform.BeginAnimation(TranslateTransform.XProperty, null);
                    translateTransform.BeginAnimation(TranslateTransform.YProperty, null);
                    scaleTransform.BeginAnimation(ScaleTransform.ScaleXProperty, null);
                    scaleTransform.BeginAnimation(ScaleTransform.ScaleYProperty, null);
                    scaleTransform.ClearValue(ScaleTransform.CenterXProperty);
                    scaleTransform.ClearValue(ScaleTransform.CenterYProperty);
                }
                else
                {
                    element.ClearValue(UIElement.RenderTransformProperty);
                }

                element.BeginAnimation(OpacityProperty, null);

                /*
                if (Visual visual = ElementCompositionPreview.GetElementVisual(element))
                {
                    visual.Offset(float3(0.0f, 0.0f, 0.0f));
                    visual.Scale(float3(1.0f, 1.0f, 1.0f));
                    visual.Opacity(desiredOpacity);
                }
                */
            }
        }

        NavigationViewItemBase NavigationViewItemBaseOrSettingsContentFromData(object data)
        {
            return GetContainerForData<NavigationViewItemBase>(data);
        }

        NavigationViewItem NavigationViewItemOrSettingsContentFromData(object data)
        {
            return GetContainerForData<NavigationViewItem>(data);
        }

        bool IsSelectionSuppressed(object item)
        {
            if (item != null)
            {
                if (NavigationViewItemOrSettingsContentFromData(item) is { } nvi)
                {
                    return !nvi.SelectsOnInvoked;
                }
            }

            return false;
        }

        bool ShouldPreserveNavigationViewRS4Behavior()
        {
            // Since RS5, we support topnav
            return m_topNavGrid == null;
        }

        bool ShouldPreserveNavigationViewRS3Behavior()
        {
            // Since RS4, we support backbutton
            return m_backButton == null;
        }

        UIElement FindSelectionIndicator(object item)
        {
            if (item != null)
            {
                if (NavigationViewItemOrSettingsContentFromData(item) is { } container)
                {
                    if (container.GetSelectionIndicator() is { } indicator)
                    {
                        return indicator;
                    }
                    else
                    {
                        // Indicator was not found, so maybe the layout hasn't updated yet.
                        // So let's do that now.
                        container.UpdateLayout();
                        return container.GetSelectionIndicator();
                    }
                }
            }
            return null;
        }

        void RaiseSelectionChangedEvent(object nextItem, bool isSettingsItem, NavigationRecommendedTransitionDirection recommendedDirection = NavigationRecommendedTransitionDirection.Default)
        {
            var eventArgs = new NavigationViewSelectionChangedEventArgs();
            eventArgs.SelectedItem = nextItem;
            eventArgs.IsSettingsSelected = isSettingsItem;
            if (NavigationViewItemBaseOrSettingsContentFromData(nextItem) is { } container)
            {
                eventArgs.SelectedItemContainer = container;
            }
            eventArgs.RecommendedNavigationTransitionInfo = CreateNavigationTransitionInfo(recommendedDirection);
            SelectionChanged?.Invoke(this, eventArgs);
        }

        // SelectedItem change can be invoked by API or user's action like clicking. if it's not from API, m_shouldRaiseInvokeItemInSelectionChange would be true
        // If nextItem is selectionsuppressed, we should undo the selection. We didn't undo it OnSelectionChange because we want change by API has the same undo logic.
        void ChangeSelection(object prevItem, object nextItem)
        {
            bool isSettingsItem = IsSettingsItem(nextItem);

            if (IsSelectionSuppressed(nextItem))
            {
                // This should not be a common codepath. Only happens if customer passes a 'selectionsuppressed' item via API.
                UndoSelectionAndRevertSelectionTo(prevItem, nextItem);
                RaiseItemInvoked(nextItem, isSettingsItem);
            }
            else
            {
                // Other transition other than default only apply to topnav
                // when clicking overflow on topnav, transition is from bottom
                // otherwise if prevItem is on left side of nextActualItem, transition is from left
                //           if prevItem is on right side of nextActualItem, transition is from right
                // click on Settings item is considered Default
                NavigationRecommendedTransitionDirection recommendedDirection;
                {
                    NavigationRecommendedTransitionDirection init()
                    {
                        if (IsTopNavigationView())
                        {
                            if (m_selectionChangeFromOverflowMenu)
                            {
                                return NavigationRecommendedTransitionDirection.FromOverflow;
                            }
                            else if (prevItem != null && nextItem != null)
                            {
                                return GetRecommendedTransitionDirection(NavigationViewItemBaseOrSettingsContentFromData(prevItem),
                                    NavigationViewItemBaseOrSettingsContentFromData(nextItem));
                            }
                        }
                        return NavigationRecommendedTransitionDirection.Default;
                    }
                    recommendedDirection = init();
                }

                // Bug 17850504, Customer may use NavigationViewItem.IsSelected in ItemInvoke or SelectionChanged Event.
                // To keep the logic the same as RS4, ItemInvoke is before unselect the old item
                // And SelectionChanged is after we selected the new item.
                var selectedItem = SelectedItem;
                if (m_shouldRaiseItemInvokedAfterSelection)
                {
                    // If selection changed inside ItemInvoked, the flag does not get said to false and the event get's raised again,so we need to set it to false now!
                    m_shouldRaiseItemInvokedAfterSelection = false;
                    RaiseItemInvoked(nextItem, isSettingsItem, NavigationViewItemOrSettingsContentFromData(nextItem), recommendedDirection);
                }
                // Selection was modified inside ItemInvoked, skip everything here!
                if (selectedItem != SelectedItem)
                {
                    return;
                }
                UnselectPrevItem(prevItem, nextItem);
                ChangeSelectStatusForItem(nextItem, true /*selected*/);

                try
                {
                    // Selection changed and we need to notify UIA
                    // HOWEVER expand collapse can also trigger if an item can expand/collapse
                    // There are multiple cases when selection changes:
                    // - Through click on item with no children -> No expand/collapse change
                    // - Through click on item with children -> Expand/collapse change
                    // - Through API with item without children -> No expand/collapse change
                    // - Through API with item with children -> No expand/collapse change
                    if (!m_shouldIgnoreUIASelectionRaiseAsExpandCollapseWillRaise)
                    {
                        if (FrameworkElementAutomationPeer.FromElement(this) is AutomationPeer peer)
                        {
                            var navViewItemPeer = (NavigationViewAutomationPeer)peer;
                            navViewItemPeer.RaiseSelectionChangedEvent(
                                prevItem, nextItem
                            );
                        }
                    }
                }
                finally
                {
                    m_shouldIgnoreUIASelectionRaiseAsExpandCollapseWillRaise = false;
                }

                RaiseSelectionChangedEvent(nextItem, isSettingsItem, recommendedDirection);
                AnimateSelectionChanged(nextItem);

                if (NavigationViewItemOrSettingsContentFromData(nextItem) is { } nvi)
                {
                    ClosePaneIfNeccessaryAfterItemIsClicked(nvi);
                }
            }
        }

        void UpdateSelectionModelSelection(IndexPath ip)
        {
            var prevIndexPath = m_selectionModel.SelectedIndex;
            m_selectionModel.SelectAt(ip);
            UpdateIsChildSelected(prevIndexPath, ip);
        }

        void UpdateIsChildSelected(IndexPath prevIP, IndexPath nextIP)
        {
            if (prevIP != null && prevIP.GetSize() > 0)
            {
                UpdateIsChildSelectedForIndexPath(prevIP, false /*isChildSelected*/);
            }

            if (nextIP != null && nextIP.GetSize() > 0)
            {
                UpdateIsChildSelectedForIndexPath(nextIP, true /*isChildSelected*/);
            }
        }

        void UpdateIsChildSelectedForIndexPath(IndexPath ip, bool isChildSelected)
        {
            // Update the isChildSelected property for every container on the IndexPath (with the exception of the actual container pointed to by the indexpath)
            var container = GetContainerForIndex(ip.GetAt(1), ip.GetAt(0) == c_footerMenuBlockIndex /*inFooter*/);
            // first index is fo mainmenu or footer
            // second is index of item in mainmenu or footer
            // next in menuitem children 
            var index = 2;
            while (container != null)
            {
                if (container is NavigationViewItem nvi)
                {
                    nvi.IsChildSelected = isChildSelected;
                    if (nvi.GetRepeater() is { } nextIR)
                    {
                        if (index < ip.GetSize() - 1)
                        {
                            container = nextIR.TryGetElement(ip.GetAt(index));
                            index++;
                            continue;
                        }
                    }
                }
                container = null;
            }
        }

        void RaiseItemInvoked(object item,
            bool isSettings,
            NavigationViewItemBase container = null,
            NavigationRecommendedTransitionDirection recommendedDirection = NavigationRecommendedTransitionDirection.Default)
        {
            var invokedItem = item;
            var invokedContainer = container;

            var eventArgs = new NavigationViewItemInvokedEventArgs();

            if (container != null)
            {
                invokedItem = container.Content;
            }
            else
            {
                // InvokedItem is container for Settings, but Content of item for other ListViewItem
                if (!isSettings)
                {
                    if (NavigationViewItemBaseOrSettingsContentFromData(item) is { } containerFromData)
                    {
                        invokedItem = containerFromData.Content;
                        invokedContainer = containerFromData;
                    }
                }
                else
                {
                    Debug.Assert(item != null);
                    invokedContainer = item as NavigationViewItemBase;
                    Debug.Assert(invokedContainer != null);
                }
            }
            eventArgs.InvokedItem = invokedItem;
            eventArgs.InvokedItemContainer = invokedContainer;
            eventArgs.IsSettingsInvoked = isSettings;
            eventArgs.RecommendedNavigationTransitionInfo = CreateNavigationTransitionInfo(recommendedDirection);
            ItemInvoked?.Invoke(this, eventArgs);
        }

        // forceSetDisplayMode: On first call to SetDisplayMode, force setting to initial values
        void SetDisplayMode(NavigationViewDisplayMode displayMode, bool forceSetDisplayMode = false)
        {
            // Need to keep the VisualStateGroup "DisplayModeGroup" updated even if the actual
            // display mode is not changed. This is due to the fact that there can be a transition between
            // 'Minimal' and 'MinimalWithBackButton'.
            UpdateVisualStateForDisplayModeGroup(displayMode);

            if (forceSetDisplayMode || DisplayMode != displayMode)
            {
                // Update header visibility based on what the new display mode will be
                UpdateHeaderVisibility(displayMode);

                UpdatePaneTabFocusNavigation();

                UpdatePaneToggleSize();

                RaiseDisplayModeChanged(displayMode);
            }
        }

        // To support TopNavigationView, DisplayModeGroup in visualstate(We call it VisualStateDisplayMode) is decoupled with DisplayMode.
        // The VisualStateDisplayMode is the combination of TopNavigationView, DisplayMode, PaneDisplayMode.
        // Here is the mapping:
        //    TopNav . Minimal
        //    PaneDisplayMode.Left || (PaneDisplayMode.Auto && DisplayMode.Expanded) . Expanded
        //    PaneDisplayMode.LeftCompact || (PaneDisplayMode.Auto && DisplayMode.Compact) . Compact
        //    Map others to Minimal or MinimalWithBackButton 
        NavigationViewVisualStateDisplayMode GetVisualStateDisplayMode(NavigationViewDisplayMode displayMode)
        {
            var paneDisplayMode = PaneDisplayMode;

            if (IsTopNavigationView())
            {
                return NavigationViewVisualStateDisplayMode.Minimal;
            }

            if (paneDisplayMode == NavigationViewPaneDisplayMode.Left ||
                (paneDisplayMode == NavigationViewPaneDisplayMode.Auto && displayMode == NavigationViewDisplayMode.Expanded))
            {
                return NavigationViewVisualStateDisplayMode.Expanded;
            }

            if (paneDisplayMode == NavigationViewPaneDisplayMode.LeftCompact ||
                (paneDisplayMode == NavigationViewPaneDisplayMode.Auto && displayMode == NavigationViewDisplayMode.Compact))
            {
                return NavigationViewVisualStateDisplayMode.Compact;
            }

            // In minimal mode, when the NavView is closed, the HeaderContent doesn't have
            // its own dedicated space, and must 'share' the top of the NavView with the 
            // pane toggle button ('hamburger' button) and the back button.
            // When the NavView is open, the close button is taking space instead of the back button.
            if (ShouldShowBackButton() || ShouldShowCloseButton())
            {
                return NavigationViewVisualStateDisplayMode.MinimalWithBackButton;
            }
            else
            {
                return NavigationViewVisualStateDisplayMode.Minimal;
            }
        }

        void UpdateVisualStateForDisplayModeGroup(NavigationViewDisplayMode displayMode)
        {
            if (m_rootSplitView is { } splitView)
            {
                var visualStateDisplayMode = GetVisualStateDisplayMode(displayMode);
                var visualStateName = "";
                var splitViewDisplayMode = SplitViewDisplayMode.Overlay;
                var visualStateNameMinimal = "Minimal";

                switch (visualStateDisplayMode)
                {
                    case NavigationViewVisualStateDisplayMode.MinimalWithBackButton:
                        visualStateName = "MinimalWithBackButton";
                        splitViewDisplayMode = SplitViewDisplayMode.Overlay;
                        break;
                    case NavigationViewVisualStateDisplayMode.Minimal:
                        visualStateName = visualStateNameMinimal;
                        splitViewDisplayMode = SplitViewDisplayMode.Overlay;
                        break;
                    case NavigationViewVisualStateDisplayMode.Compact:
                        visualStateName = "Compact";
                        splitViewDisplayMode = SplitViewDisplayMode.CompactOverlay;
                        break;
                    case NavigationViewVisualStateDisplayMode.Expanded:
                        visualStateName = "Expanded";
                        splitViewDisplayMode = SplitViewDisplayMode.CompactInline;
                        break;
                }

                // When the pane is made invisible we need to collapse the pane part of the SplitView
                if (!IsPaneVisible)
                {
                    splitViewDisplayMode = SplitViewDisplayMode.CompactOverlay;
                }

                var handled = false;
                if (visualStateName == visualStateNameMinimal && IsTopNavigationView())
                {
                    // TopNavigationMinimal was introduced in 19H1. We need to fallback to Minimal if the customer uses an older template.
                    handled = VisualStateManager.GoToState(this, "TopNavigationMinimal", false /*useTransitions*/);
                }
                if (!handled)
                {
                    VisualStateManager.GoToState(this, visualStateName, false /*useTransitions*/);
                }
                splitView.DisplayMode = splitViewDisplayMode;
            }
        }

        void OnNavigationViewItemTapped(object sender, TappedRoutedEventArgs args)
        {
            if (sender is NavigationViewItem nvi)
            {
                OnNavigationViewItemInvoked(nvi);
                nvi.Focus();
                args.Handled = true;
            }
        }

        void OnNavigationViewItemKeyDown(object sender, KeyEventArgs args)
        {
            if (args.Key == Key.Enter ||
                args.Key == Key.Space)
            {
                if (args.IsRepeat)
                {
                    return;
                }
            }

            if (sender is NavigationViewItem nvi)
            {
                HandleKeyEventForNavigationViewItem(nvi, args);
            }
        }

        void HandleKeyEventForNavigationViewItem(NavigationViewItem nvi, KeyEventArgs args)
        {
            var key = args.Key;
            switch (key)
            {
                case Key.Enter:
                case Key.Space:
                    args.Handled = true;
                    OnNavigationViewItemInvoked(nvi);
                    break;
                case Key.Home:
                    args.Handled = true;
                    KeyboardFocusFirstItemFromItem(nvi);
                    break;
                case Key.End:
                    args.Handled = true;
                    KeyboardFocusLastItemFromItem(nvi);
                    break;
                case Key.Down:
                    FocusNextDownItem(nvi, args);
                    break;
                case Key.Up:
                    FocusNextUpItem(nvi, args);
                    break;
                case Key.Right:
                    FocusNextRightItem(nvi, args);
                    break;
            }
        }

        void FocusNextUpItem(NavigationViewItem nvi, KeyEventArgs args)
        {
            if (args.OriginalSource != nvi)
            {
                return;
            }

            bool shouldHandleFocus = true;
            var nviImpl = nvi;
            var nextFocusableElement = FocusManagerEx.FindNextFocusableElement(FocusNavigationDirection.Up);

            if (nextFocusableElement is NavigationViewItem nextFocusableNVI)
            {

                var nextFocusableNVIImpl = nextFocusableNVI;

                if (nextFocusableNVIImpl.Depth == nviImpl.Depth)
                {
                    // If we not at the top of the list for our current depth and the item above us has children, check whether we should move focus onto a child
                    if (DoesNavigationViewItemHaveChildren(nextFocusableNVI))
                    {
                        // Focus on last lowest level visible container
                        if (nextFocusableNVIImpl.GetRepeater() is { } childRepeater)
                        {
                            //if (FocusManager.FindLastFocusableElement(childRepeater) is { } lastFocusableElement)
                            //{
                            //    if (lastFocusableElement is Control lastFocusableNVI)
                            //    {
                            //        args.Handled = lastFocusableNVI.Focus(/*FocusState.Keyboard*/);
                            //    }
                            //}
                            if (childRepeater.MoveFocus(new TraversalRequest(FocusNavigationDirection.Last)))
                            {
                                args.Handled = true;
                            }
                            else
                            {
                                args.Handled = nextFocusableNVIImpl.Focus(/*FocusState.Keyboard*/);
                            }

                        }
                    }
                    else
                    {
                        // Traversing up a list where XYKeyboardFocus will result in correct behavior
                        shouldHandleFocus = false;
                    }
                }
            }

            // We are at the top of the list, focus on parent
            if (shouldHandleFocus && !args.Handled && nviImpl.Depth > 0)
            {
                if (GetParentNavigationViewItemForContainer(nvi) is { } parentContainer)
                {
                    args.Handled = parentContainer.Focus(/*FocusState.Keyboard*/);
                }
            }
        }

        // If item has focusable children, move focus to first focusable child, otherise just defer to default XYKeyboardFocus behavior
        void FocusNextDownItem(NavigationViewItem nvi, KeyEventArgs args)
        {
            if (args.OriginalSource != nvi)
            {
                return;
            }

            if (DoesNavigationViewItemHaveChildren(nvi))
            {
                var nviImpl = nvi;
                if (nviImpl.GetRepeater() is { } childRepeater)
                {
                    //var firstFocusableElement = FocusManager.FindFirstFocusableElement(childRepeater);
                    //if (firstFocusableElement is Control controlFirst)
                    //{
                    //    args.Handled = controlFirst.Focus(/*FocusState.Keyboard*/);
                    //}
                    args.Handled = childRepeater.MoveFocus(new TraversalRequest(FocusNavigationDirection.First));
                }
            }

            // WPF
            if (!args.Handled)
            {
                args.Handled = nvi.MoveFocus(new TraversalRequest(FocusNavigationDirection.Down));
            }
        }

        // WPF
        void FocusNextRightItem(NavigationViewItem nvi, KeyEventArgs args)
        {
            args.Handled = nvi.MoveFocus(new TraversalRequest(FocusNavigationDirection.Right));
        }

        void KeyboardFocusFirstItemFromItem(NavigationViewItemBase nvib)
        {
            UIElement firstElement;
            {
                UIElement init()
                {
                    var parentIR = GetParentRootItemsRepeaterForContainer(nvib);
                    return parentIR.TryGetElement(0);
                }
                firstElement = init();
            }

            if (firstElement is Control controlFirst)
            {
                controlFirst.Focus();
            }
        }

        void KeyboardFocusLastItemFromItem(NavigationViewItemBase nvib)
        {
            var parentIR = GetParentRootItemsRepeaterForContainer(nvib);

            if (parentIR.ItemsSourceView is { } itemsSourceView)
            {
                var lastIndex = itemsSourceView.Count - 1;
                if (parentIR.TryGetElement(lastIndex) is { } lastElement)
                {
                    if (lastElement is Control controlLast)
                    {
                        controlLast.Focus(/*FocusState.Programmatic*/);
                    }
                }
            }
        }

        void OnRepeaterGettingFocus(object sender, GettingFocusEventArgs args)
        {
            // if focus change was invoked by tab key
            // and there is selected item in ItemsRepeater that gatting focus
            // we should put focus on selected item
            if (m_TabKeyPrecedesFocusChange && args.InputDevice == FocusInputDeviceKind.Keyboard && m_selectionModel.SelectedIndex != null)
            {
                if (args.OldFocusedElement is { } oldFocusedElement)
                {
                    if (sender is ItemsRepeater newRootItemsRepeater)
                    {
                        bool isFocusOutsideCurrentRootRepeater;
                        {
                            isFocusOutsideCurrentRootRepeater = init();
                            bool init()
                            {
                                bool isFocusOutsideCurrentRootRepeater = true;
                                var treeWalkerCursor = oldFocusedElement;

                                // check if last focused element was in same root repeater
                                while (treeWalkerCursor != null)
                                {
                                    if (treeWalkerCursor is NavigationViewItemBase oldFocusedNavigationItemBase)
                                    {
                                        var oldParentRootRepeater = GetParentRootItemsRepeaterForContainer(oldFocusedNavigationItemBase);
                                        isFocusOutsideCurrentRootRepeater = oldParentRootRepeater != newRootItemsRepeater;
                                        break;
                                    }

                                    treeWalkerCursor = VisualTreeHelper.GetParent(treeWalkerCursor);
                                }

                                return isFocusOutsideCurrentRootRepeater;
                            }
                        }

                        object rootRepeaterForSelectedItem;
                        {
                            rootRepeaterForSelectedItem = init();
                            object init()
                            {
                                if (IsTopNavigationView())
                                {
                                    return m_selectionModel.SelectedIndex.GetAt(0) == c_mainMenuBlockIndex ? m_topNavRepeater : m_topNavFooterMenuRepeater;
                                }
                                return m_selectionModel.SelectedIndex.GetAt(0) == c_mainMenuBlockIndex ? m_leftNavRepeater : m_leftNavFooterMenuRepeater;
                            }
                        }

                        // If focus is coming from outside the root repeater,
                        // and selected item is within current repeater
                        // we should put focus on selected item
                        if (args is IGettingFocusEventArgs2 argsAsIGettingFocusEventArgs2)
                        {
                            if (newRootItemsRepeater == rootRepeaterForSelectedItem && isFocusOutsideCurrentRootRepeater)
                            {
                                var selectedContainer = GetContainerForIndexPath(m_selectionModel.SelectedIndex, true /* lastVisible */);
                                if (argsAsIGettingFocusEventArgs2.TrySetNewFocusedElement(selectedContainer))
                                {
                                    args.Handled = true;
                                }
                            }
                        }
                    }
                }
            }

            m_TabKeyPrecedesFocusChange = false;
        }

        void OnNavigationViewItemOnGotFocus(object sender, RoutedEventArgs e)
        {
            if (sender is NavigationViewItem nvi)
            {
                // Achieve selection follows focus behavior
                if (IsNavigationViewListSingleSelectionFollowsFocus())
                {
                    // if nvi is already selected we don't need to invoke it again
                    // otherwise ItemInvoked fires twice when item was tapped
                    // or fired when window gets focus
                    if (nvi.SelectsOnInvoked && !nvi.IsSelected)
                    {
                        if (IsTopNavigationView())
                        {
                            if (GetParentItemsRepeaterForContainer(nvi) is { } parentIR)
                            {
                                if (parentIR != m_topNavRepeaterOverflowView)
                                {
                                    OnNavigationViewItemInvoked(nvi);
                                }
                            }
                        }
                        else
                        {
                            OnNavigationViewItemInvoked(nvi);
                        }
                    }
                }
            }
        }

        internal void OnSettingsInvoked()
        {
            var settingsItem = m_settingsItem;
            if (settingsItem != null)
            {
                OnNavigationViewItemInvoked(settingsItem);
            }
        }

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            m_TabKeyPrecedesFocusChange = false;
            base.OnPreviewKeyDown(e);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            var eventArgs = e;
            var key = eventArgs.Key;

            bool handled = false;
            m_TabKeyPrecedesFocusChange = false;

            switch (key)
            {
                /*
                case Key.GamepadView:
                    if (!IsPaneOpen && !IsTopNavigationView())
                    {
                        OpenPane();
                        handled = true;
                    }
                    break;
                case Key.GoBack:
                case Key.XButton1:
                    if (IsPaneOpen && IsLightDismissible())
                    {
                        handled = AttemptClosePaneLightly();
                    }
                    break;
                case Key.GamepadLeftShoulder:
                    handled = BumperNavigation(-1);
                    break;
                case Key.GamepadRightShoulder:
                    handled = BumperNavigation(1);
                    break;
                */
                case Key.Tab:
                    // arrow keys navigation through ItemsRepeater don't get here
                    // so handle tab key to distinguish between tab focus and arrow focus navigation
                    m_TabKeyPrecedesFocusChange = true;
                    break;
                case Key.Left:
                    bool isAltPressed = Keyboard.Modifiers.HasFlag(ModifierKeys.Alt);

                    if (isAltPressed && IsPaneOpen && IsLightDismissible())
                    {
                        handled = AttemptClosePaneLightly();
                    }

                    break;
            }

            eventArgs.Handled = handled;

            base.OnKeyDown(e);
        }

        /*
        bool BumperNavigation(int offset)
        {
            // By passing an offset indicating direction (ideally 1 or -1, meaning right or left respectively)
            // we'll try to move focus to an item. We won't be moving focus to items in the overflow menu and this won't
            // work on left navigation, only dealing with the top primary list here and only with items that don't have
            // !SelectsOnInvoked set to true. If !SelectsOnInvoked is true, we'll skip the item and try focusing on the next one
            // that meets the conditions, in the same direction.
            var shoulderNavigationEnabledParamValue = ShoulderNavigationEnabled;
            var shoulderNavigationForcedDisabled = (shoulderNavigationEnabledParamValue == NavigationViewShoulderNavigationEnabled.Never);

            if (!IsTopNavigationView()
                || !IsNavigationViewListSingleSelectionFollowsFocus()
                || shoulderNavigationForcedDisabled)
            {
                return false;
            }

            var shoulderNavigationSelectionFollowsFocusEnabled = (SelectionFollowsFocus == NavigationViewSelectionFollowsFocus.Enabled
                && shoulderNavigationEnabledParamValue == NavigationViewShoulderNavigationEnabled.WhenSelectionFollowsFocus);

            var shoulderNavigationEnabled = (shoulderNavigationSelectionFollowsFocusEnabled
                || shoulderNavigationEnabledParamValue == NavigationViewShoulderNavigationEnabled.Always);

            if (!shoulderNavigationEnabled)
            {
                return false;
            }

            var item = SelectedItem;

            if (item != null)
            {
                if (NavigationViewItemOrSettingsContentFromData(item) is { } nvi)
                {
                    var index = m_topDataProvider.IndexOf(item, NavigationViewSplitVectorID.PrimaryList);

                    if (index >= 0)
                    {
                        if (m_topNavRepeater is { } topNavRepeater)
                        {
                            var topPrimaryListSize = m_topDataProvider.GetPrimaryListSize();
                            index += offset;

                            while (index > -1 && index < topPrimaryListSize)
                            {
                                var newItem = topNavRepeater.TryGetElement(index);
                                if (newItem is NavigationViewItem newNavViewItem)
                                {
                                    // This is done to skip Separators or other items that are not NavigationViewItems
                                    if (newNavViewItem.SelectsOnInvoked)
                                    {
                                        newNavViewItem.IsSelected = true;
                                        return true;
                                    }
                                }

                                index += offset;
                            }
                        }
                    }
                }
            }

            return false;
        }
        */

        bool SelectSelectableItemWithOffset(int startIndex, int offset, ItemsRepeater repeater, int repeaterCollectionSize)
        {
            startIndex += offset;
            while (startIndex > -1 && startIndex < repeaterCollectionSize)
            {
                var newItem = repeater.TryGetElement(startIndex);
                if (newItem is NavigationViewItem newNavViewItem)
                {
                    // This is done to skip Separators or other items that are not NavigationViewItems
                    if (newNavViewItem.SelectsOnInvoked)
                    {
                        newNavViewItem.IsSelected = true;
                        return true;
                    }
                }

                startIndex += offset;
            }
            return false;
        }

        internal object MenuItemFromContainer(DependencyObject container)
        {
            if (container != null)
            {
                if (container is NavigationViewItemBase nvib)
                {
                    if (GetParentItemsRepeaterForContainer(nvib) is { } parentRepeater)
                    {
                        var containerIndex = parentRepeater.GetElementIndex(nvib);
                        if (containerIndex >= 0)
                        {
                            return GetItemFromIndex(parentRepeater, containerIndex);
                        }
                    }
                }
            }
            return null;
        }

        internal DependencyObject ContainerFromMenuItem(object item)
        {
            if (item != null)
            {
                return NavigationViewItemBaseOrSettingsContentFromData(item);
            }

            return null;
        }

        void OnTopNavDataSourceChanged(NotifyCollectionChangedEventArgs args)
        {
            CloseTopNavigationViewFlyout();

            // Assume that raw data doesn't change very often for navigationview.
            // So here is a simple implementation and for each data item change, it request a layout change
            // update this in the future if there is performance problem

            // If it's Uninitialized, it means that we didn't start the layout yet.
            if (m_topNavigationMode != TopNavigationViewLayoutState.Uninitialized)
            {
                m_topDataProvider.MoveAllItemsToPrimaryList();
            }

            m_lastSelectedItemPendingAnimationInTopNav = null;
        }

        internal int GetNavigationViewItemCountInPrimaryList()
        {
            return m_topDataProvider.GetNavigationViewItemCountInPrimaryList();
        }

        internal int GetNavigationViewItemCountInTopNav()
        {
            return m_topDataProvider.GetNavigationViewItemCountInTopNav();
        }

        internal SplitView GetSplitView()
        {
            return m_rootSplitView;
        }

        internal TopNavigationViewDataProvider GetTopDataProvider() { return m_topDataProvider; }

        internal void TopNavigationViewItemContentChanged()
        {
            if (m_appliedTemplate)
            {
                m_topDataProvider.InvalidWidthCache();
                InvalidateMeasure();
            }
        }

        /*
        void OnAccessKeyInvoked(object sender, AccessKeyInvokedEventArgs args)
        {
            if (args.Handled)
            {
                return;
            }

            // For topnav, invoke Morebutton, otherwise togglebutton
            var button = IsTopNavigationView() ? m_topNavOverflowButton : m_paneToggleButton;
            if (button != null)
            {
                if (FrameworkElementAutomationPeer.FromElement(button) is ButtonAutomationPeer peer)
                {
                    peer.Invoke();
                    args.Handled(true);
                }
            }
        }
        */

        NavigationTransitionInfo CreateNavigationTransitionInfo(NavigationRecommendedTransitionDirection recommendedTransitionDirection)
        {
            // In current implementation, if click is from overflow item, just recommend FromRight Slide animation.
            if (recommendedTransitionDirection == NavigationRecommendedTransitionDirection.FromOverflow)
            {
                recommendedTransitionDirection = NavigationRecommendedTransitionDirection.FromRight;
            }

            if ((recommendedTransitionDirection == NavigationRecommendedTransitionDirection.FromLeft
                || recommendedTransitionDirection == NavigationRecommendedTransitionDirection.FromRight)
                && SharedHelpers.IsRS5OrHigher())
            {
                SlideNavigationTransitionInfo sliderNav = new SlideNavigationTransitionInfo();
                SlideNavigationTransitionEffect effect =
                    recommendedTransitionDirection == NavigationRecommendedTransitionDirection.FromRight ?
                    SlideNavigationTransitionEffect.FromRight :
                    SlideNavigationTransitionEffect.FromLeft;
                // PR 1895355: Bug 17724768: Remove Side-to-Side navigation transition velocity key
                // https://microsoft.visualstudio.com/_git/os/commit/7d58531e69bc8ad1761cff938d8db25f6fb6a841
                // We want to use Effect, but it's not in all os of rs5. as a workaround, we only apply effect to the os which is already remove velocity key.
                if (sliderNav is ISlideNavigationTransitionInfo2 sliderNav2)
                {
                    sliderNav.Effect = effect;
                }
                return sliderNav;
            }
            else
            {
                EntranceNavigationTransitionInfo defaultInfo = new EntranceNavigationTransitionInfo();
                return defaultInfo;
            }
        }

        NavigationRecommendedTransitionDirection GetRecommendedTransitionDirection(DependencyObject prev, DependencyObject next)
        {
            var recommendedTransitionDirection = NavigationRecommendedTransitionDirection.Default;
            var ir = m_topNavRepeater;

            if (prev != null && next != null && ir != null)
            {
                var prevIndexPath = GetIndexPathForContainer(prev as NavigationViewItemBase);
                var nextIndexPath = GetIndexPathForContainer(next as NavigationViewItemBase);

                var compare = prevIndexPath.CompareTo(nextIndexPath);

                switch (compare)
                {
                    case -1:
                        recommendedTransitionDirection = NavigationRecommendedTransitionDirection.FromRight;
                        break;
                    case 1:
                        recommendedTransitionDirection = NavigationRecommendedTransitionDirection.FromLeft;
                        break;
                    default:
                        recommendedTransitionDirection = NavigationRecommendedTransitionDirection.Default;
                        break;
                }
            }
            return recommendedTransitionDirection;
        }

        NavigationViewTemplateSettings GetTemplateSettings()
        {
            return TemplateSettings;
        }

        bool IsNavigationViewListSingleSelectionFollowsFocus()
        {
            return (SelectionFollowsFocus == NavigationViewSelectionFollowsFocus.Enabled);
        }

        void UpdateSingleSelectionFollowsFocusTemplateSetting()
        {
            GetTemplateSettings().SingleSelectionFollowsFocus = IsNavigationViewListSingleSelectionFollowsFocus();
        }

        void OnMenuItemsSourceCollectionChanged(object sender, object args)
        {
            if (!IsTopNavigationView())
            {
                if (m_leftNavRepeater is { } repeater)
                {
                    repeater.UpdateLayout();
                }
                UpdatePaneLayout();
            }
        }

        void OnSelectedItemPropertyChanged(DependencyPropertyChangedEventArgs args)
        {

            var newItem = args.NewValue;
            var oldItem = args.OldValue;

            ChangeSelection(oldItem, newItem);

            if (m_appliedTemplate && IsTopNavigationView())
            {
                if (!m_layoutUpdatedToken ||
                    (newItem != null && m_topDataProvider.IndexOf(newItem) != s_itemNotFound && m_topDataProvider.IndexOf(newItem, NavigationViewSplitVectorID.PrimaryList) == s_itemNotFound)) // selection is in overflow
                {
                    InvalidateTopNavPrimaryLayout();
                }
            }
        }

        void SetSelectedItemAndExpectItemInvokeWhenSelectionChangedIfNotInvokedFromAPI(object item)
        {
            SelectedItem = item;
        }

        void ChangeSelectStatusForItem(object item, bool selected)
        {
            if (NavigationViewItemOrSettingsContentFromData(item) is { } container)
            {
                // If we unselect an item, ListView doesn't tolerate setting the SelectedItem to null. 
                // Instead we remove IsSelected from the item itself, and it make ListView to unselect it.
                // If we select an item, we follow the unselect to simplify the code.
                container.IsSelected = selected;
            }
            else if (selected)
            {
                // If we are selecting an item and have not found a realized container for it,
                // we may need to manually resolve a container for this in order to update the
                // SelectionModel's selected IndexPath.
                var ip = GetIndexPathOfItem(item);
                if (ip != null && ip.GetSize() > 0)
                {
                    // The SelectedItem property has already been updated. So we want to block any logic from executing
                    // in the SelectionModel selection changed callback.
                    try
                    {
                        m_shouldIgnoreNextSelectionChange = true;
                        UpdateSelectionModelSelection(ip);
                    }
                    finally
                    {
                        m_shouldIgnoreNextSelectionChange = false;
                    }
                }
            }
        }

        bool IsSettingsItem(object item)
        {
            bool isSettingsItem = false;
            if (item != null)
            {
                if (m_settingsItem is { } settingItem)
                {
                    isSettingsItem = (settingItem == item) || (settingItem.Content == item);
                }
            }
            return isSettingsItem;
        }

        void UnselectPrevItem(object prevItem, object nextItem)
        {
            if (prevItem != null && prevItem != nextItem)
            {
                var setIgnoreNextSelectionChangeToFalse = !m_shouldIgnoreNextSelectionChange;
                try
                {
                    m_shouldIgnoreNextSelectionChange = true;
                    ChangeSelectStatusForItem(prevItem, false /*selected*/);
                }
                finally
                {
                    if (setIgnoreNextSelectionChangeToFalse)
                    {
                        m_shouldIgnoreNextSelectionChange = false;
                    }
                }
            }
        }

        void UndoSelectionAndRevertSelectionTo(object prevSelectedItem, object nextItem)
        {
            object selectedItem = null;
            if (prevSelectedItem != null)
            {
                if (IsSelectionSuppressed(prevSelectedItem))
                {
                    AnimateSelectionChanged(null);
                }
                else
                {
                    ChangeSelectStatusForItem(prevSelectedItem, true /*selected*/);
                    AnimateSelectionChangedToItem(prevSelectedItem);
                    selectedItem = prevSelectedItem;
                }
            }
            else
            {
                // Bug 18033309, A SelectsOnInvoked=false item is clicked, if we don't unselect it from listview, the second click will not raise ItemClicked
                // because listview doesn't raise SelectionChange.
                ChangeSelectStatusForItem(nextItem, false /*selected*/);
            }
            SelectedItem = selectedItem;
        }

        void CloseTopNavigationViewFlyout()
        {
            if (m_topNavOverflowButton is { } button)
            {
                if (button.Flyout() is { } flyout)
                {
                    flyout.Hide();
                }
            }
        }

        void UpdateVisualState(bool useTransitions = false)
        {
            if (m_appliedTemplate)
            {
                var box = AutoSuggestBox;
                VisualStateManager.GoToState(this, box != null ? "AutoSuggestBoxVisible" : "AutoSuggestBoxCollapsed", false /*useTransitions*/);

                bool isVisible = IsSettingsVisible;
                VisualStateManager.GoToState(this, isVisible ? "SettingsVisible" : "SettingsCollapsed", false /*useTransitions*/);

                if (IsTopNavigationView())
                {
                    UpdateVisualStateForOverflowButton();
                }
                else
                {
                    UpdateLeftNavigationOnlyVisualState(useTransitions);
                }
            }
        }

        void UpdateVisualStateForOverflowButton()
        {
            var state = (OverflowLabelMode == NavigationViewOverflowLabelMode.MoreLabel) ?
                "OverflowButtonWithLabel" :
                "OverflowButtonNoLabel";
            VisualStateManager.GoToState(this, state, false /* useTransitions*/);
        }

        void UpdateLeftNavigationOnlyVisualState(bool useTransitions)
        {
            bool isToggleButtonVisible = IsPaneToggleButtonVisible;
            VisualStateManager.GoToState(this, isToggleButtonVisible ? "TogglePaneButtonVisible" : "TogglePaneButtonCollapsed", false /*useTransitions*/);
        }

        void InvalidateTopNavPrimaryLayout()
        {
            if (m_appliedTemplate && IsTopNavigationView())
            {
                InvalidateMeasure();
            }
        }

        double MeasureTopNavigationViewDesiredWidth(Size availableSize)
        {
            return LayoutUtils.MeasureAndGetDesiredWidthFor(m_topNavGrid, availableSize);
        }

        double MeasureTopNavMenuItemsHostDesiredWidth(Size availableSize)
        {
            return LayoutUtils.MeasureAndGetDesiredWidthFor(m_topNavRepeater, availableSize);
        }

        double GetTopNavigationViewActualWidth()
        {
            double width = LayoutUtils.GetActualWidthFor(m_topNavGrid);
            Debug.Assert(width < double.MaxValue);
            return width;
        }

        bool HasTopNavigationViewItemNotInPrimaryList()
        {
            return m_topDataProvider.GetPrimaryListSize() != m_topDataProvider.Size();
        }

        void ResetAndRearrangeTopNavItems(Size availableSize)
        {
            if (HasTopNavigationViewItemNotInPrimaryList())
            {
                m_topDataProvider.MoveAllItemsToPrimaryList();
            }
            ArrangeTopNavItems(availableSize);
        }

        void HandleTopNavigationMeasureOverride(Size availableSize)
        {
            // Determine if TopNav is in Overflow
            if (HasTopNavigationViewItemNotInPrimaryList())
            {
                HandleTopNavigationMeasureOverrideOverflow(availableSize);
            }
            else
            {
                HandleTopNavigationMeasureOverrideNormal(availableSize);
            }

            if (m_topNavigationMode == TopNavigationViewLayoutState.Uninitialized)
            {
                m_topNavigationMode = TopNavigationViewLayoutState.Initialized;
            }
        }

        void HandleTopNavigationMeasureOverrideNormal(Size availableSize)
        {
            var desiredWidth = MeasureTopNavigationViewDesiredWidth(c_infSize);
            if (desiredWidth > availableSize.Width)
            {
                ResetAndRearrangeTopNavItems(availableSize);
            }
        }

        void HandleTopNavigationMeasureOverrideOverflow(Size availableSize)
        {
            var desiredWidth = MeasureTopNavigationViewDesiredWidth(c_infSize);
            if (desiredWidth > availableSize.Width)
            {
                ShrinkTopNavigationSize(desiredWidth, availableSize);
            }
            else if (desiredWidth < availableSize.Width)
            {
                var fullyRecoverWidth = m_topDataProvider.WidthRequiredToRecoveryAllItemsToPrimary();
                if (availableSize.Width >= desiredWidth + fullyRecoverWidth + m_topNavigationRecoveryGracePeriodWidth)
                {
                    // It's possible to recover from Overflow to Normal state, so we restart the MeasureOverride from first step
                    ResetAndRearrangeTopNavItems(availableSize);
                }
                else
                {
                    var movableItems = FindMovableItemsRecoverToPrimaryList(availableSize.Width - desiredWidth, new List<int>()/*includeItems*/);
                    m_topDataProvider.MoveItemsToPrimaryList(movableItems);
                }
            }
        }

        void ArrangeTopNavItems(Size availableSize)
        {
            SetOverflowButtonVisibility(Visibility.Collapsed);
            var desiredWidth = MeasureTopNavigationViewDesiredWidth(c_infSize);
            if (!(desiredWidth < availableSize.Width))
            {
                // overflow
                SetOverflowButtonVisibility(Visibility.Visible);
                var desiredWidthForOverflowButton = MeasureTopNavigationViewDesiredWidth(c_infSize);

                Debug.Assert(desiredWidthForOverflowButton >= desiredWidth);
                m_topDataProvider.OverflowButtonWidth(desiredWidthForOverflowButton - desiredWidth);

                ShrinkTopNavigationSize(desiredWidthForOverflowButton, availableSize);
            }
        }

        void SetOverflowButtonVisibility(Visibility visibility)
        {
            if (visibility != TemplateSettings.OverflowButtonVisibility)
            {
                GetTemplateSettings().OverflowButtonVisibility = visibility;
            }
        }

        void SelectOverflowItem(object item, IndexPath ip)
        {

            object itemBeingMoved;
            {
                object init()
                {
                    if (ip.GetSize() > 2)
                    {
                        return GetItemFromIndex(m_topNavRepeaterOverflowView, m_topDataProvider.ConvertOriginalIndexToIndex(ip.GetAt(1)));
                    }
                    return item;
                }
                itemBeingMoved = init();
            }

            // Calculate selected overflow item size.
            var selectedOverflowItemIndex = m_topDataProvider.IndexOf(itemBeingMoved);
            Debug.Assert(selectedOverflowItemIndex != s_itemNotFound);
            var selectedOverflowItemWidth = m_topDataProvider.GetWidthForItem(selectedOverflowItemIndex);

            bool needInvalidMeasure = !m_topDataProvider.IsValidWidthForItem(selectedOverflowItemIndex);

            if (!needInvalidMeasure)
            {
                var actualWidth = GetTopNavigationViewActualWidth();
                var desiredWidth = MeasureTopNavigationViewDesiredWidth(c_infSize);
                Debug.Assert(desiredWidth <= actualWidth);

                // Calculate selected item size
                var selectedItemIndex = s_itemNotFound;
                var selectedItemWidth = 0.0;
                if (SelectedItem is { } selectedItem)
                {
                    selectedItemIndex = m_topDataProvider.IndexOf(selectedItem);
                    if (selectedItemIndex != s_itemNotFound)
                    {
                        selectedItemWidth = m_topDataProvider.GetWidthForItem(selectedItemIndex);
                    }
                }

                var widthAtLeastToBeRemoved = desiredWidth + selectedOverflowItemWidth - actualWidth;

                // calculate items to be removed from primary because a overflow item is selected. 
                // SelectedItem is assumed to be removed from primary first, then added it back if it should not be removed
                var itemsToBeRemoved = FindMovableItemsToBeRemovedFromPrimaryList(widthAtLeastToBeRemoved, new List<int>() /*excludeItems*/);

                // calculate the size to be removed
                var toBeRemovedItemWidth = m_topDataProvider.CalculateWidthForItems(itemsToBeRemoved);

                var widthAvailableToRecover = toBeRemovedItemWidth - widthAtLeastToBeRemoved;
                var itemsToBeAdded = FindMovableItemsRecoverToPrimaryList(widthAvailableToRecover, new List<int> { selectedOverflowItemIndex }/*includeItems*/);

                CollectionHelper.unique_push_back(itemsToBeAdded, selectedOverflowItemIndex);

                // Keep track of the item being moved in order to know where to animate selection indicator
                m_lastSelectedItemPendingAnimationInTopNav = itemBeingMoved;
                if (ip != null && ip.GetSize() > 0)
                {
                    foreach (var it in itemsToBeRemoved)
                    {
                        if (it == ip.GetAt(1))
                        {
                            if (m_activeIndicator is { } indicator)
                            {
                                // If the previously selected item is being moved into overflow, hide its indicator
                                // as we will no longer need to animate from its location.
                                AnimateSelectionChanged(null);
                            }
                            break;
                        }
                    }
                }

                if (m_topDataProvider.HasInvalidWidth(itemsToBeAdded))
                {
                    needInvalidMeasure = true;
                }
                else
                {
                    // Exchange items between Primary and Overflow
                    {
                        m_topDataProvider.MoveItemsToPrimaryList(itemsToBeAdded);
                        m_topDataProvider.MoveItemsOutOfPrimaryList(itemsToBeRemoved);
                    }

                    if (NeedRearrangeOfTopElementsAfterOverflowSelectionChanged(selectedOverflowItemIndex))
                    {
                        needInvalidMeasure = true;
                    }

                    if (!needInvalidMeasure)
                    {
                        SetSelectedItemAndExpectItemInvokeWhenSelectionChangedIfNotInvokedFromAPI(item);
                        InvalidateMeasure();
                    }
                }
            }

            // TODO: Verify that this is no longer needed and delete
            if (needInvalidMeasure)
            {
                // not all items have known width, need to redo the layout
                m_topDataProvider.MoveAllItemsToPrimaryList();
                SetSelectedItemAndExpectItemInvokeWhenSelectionChangedIfNotInvokedFromAPI(item);
                InvalidateTopNavPrimaryLayout();
            }
        }

        bool NeedRearrangeOfTopElementsAfterOverflowSelectionChanged(int selectedOriginalIndex)
        {
            bool needRearrange = false;

            var primaryList = m_topDataProvider.GetPrimaryItems();
            var primaryListSize = primaryList.Count;
            var indexInPrimary = m_topDataProvider.ConvertOriginalIndexToIndex(selectedOriginalIndex);
            // We need to verify that through various overflow selection combinations, the primary
            // items have not been put into a state of non-logical item layout (aka not in proper sequence).
            // To verify this, if the newly selected item has items following it in the primary items:
            // - we verify that they are meant to follow the selected item as specified in the original order
            // - we verify that the preceding item is meant to directly precede the selected item in the original order
            // If these two conditions are not met, we move all items to the primary list and trigger a re-arrangement of the items.
            if (indexInPrimary < (int)(primaryListSize - 1))
            {
                var nextIndexInPrimary = indexInPrimary + 1;
                var nextIndexInOriginal = selectedOriginalIndex + 1;
                var prevIndexInOriginal = selectedOriginalIndex - 1;

                // Check whether item preceding the selected is not directly preceding
                // in the original.
                if (indexInPrimary > 0)
                {
                    List<int> prevIndexInVector = new List<int>();
                    prevIndexInVector.Add(nextIndexInPrimary - 1);
                    var prevOriginalIndexOfPrevPrimaryItem = m_topDataProvider.ConvertPrimaryIndexToIndex(prevIndexInVector);
                    if (prevOriginalIndexOfPrevPrimaryItem[0] != prevIndexInOriginal)
                    {
                        needRearrange = true;
                    }
                }


                // Check whether items following the selected item are out of order
                while (!needRearrange && nextIndexInPrimary < (int)primaryListSize)
                {
                    List<int> nextIndexInVector = new List<int>();
                    nextIndexInVector.Add(nextIndexInPrimary);
                    var originalIndex = m_topDataProvider.ConvertPrimaryIndexToIndex(nextIndexInVector);
                    if (nextIndexInOriginal != originalIndex[0])
                    {
                        needRearrange = true;
                        break;
                    }
                    nextIndexInPrimary++;
                    nextIndexInOriginal++;
                }
            }

            return needRearrange;
        }

        void ShrinkTopNavigationSize(double desiredWidth, Size availableSize)
        {
            UpdateTopNavigationWidthCache();

            var selectedItemIndex = GetSelectedItemIndex();

            var possibleWidthForPrimaryList = MeasureTopNavMenuItemsHostDesiredWidth(c_infSize) - (desiredWidth - availableSize.Width);
            if (possibleWidthForPrimaryList >= 0)
            {
                // Remove all items which is not visible except first item and selected item.
                var itemToBeRemoved = FindMovableItemsBeyondAvailableWidth(possibleWidthForPrimaryList);
                // should keep at least one item in primary
                KeepAtLeastOneItemInPrimaryList(itemToBeRemoved, true/*shouldKeepFirst*/);
                m_topDataProvider.MoveItemsOutOfPrimaryList(itemToBeRemoved);
            }

            // measure again to make sure SelectedItem is realized
            desiredWidth = MeasureTopNavigationViewDesiredWidth(c_infSize);

            var widthAtLeastToBeRemoved = desiredWidth - availableSize.Width;
            if (widthAtLeastToBeRemoved > 0)
            {
                var itemToBeRemoved = FindMovableItemsToBeRemovedFromPrimaryList(widthAtLeastToBeRemoved, new List<int> { selectedItemIndex });

                // At least one item is kept on primary list
                KeepAtLeastOneItemInPrimaryList(itemToBeRemoved, false/*shouldKeepFirst*/);

                // There should be no item is virtualized in this step
                Debug.Assert(!m_topDataProvider.HasInvalidWidth(itemToBeRemoved));
                m_topDataProvider.MoveItemsOutOfPrimaryList(itemToBeRemoved);
            }
        }

        List<int> FindMovableItemsRecoverToPrimaryList(double availableWidth, List<int> includeItems)
        {
            List<int> toBeMoved = new List<int>();

            var size = m_topDataProvider.Size();

            // Included Items take high priority, all of them are included in recovery list
            foreach (var index in includeItems)
            {
                var width = m_topDataProvider.GetWidthForItem(index);
                toBeMoved.Add(index);
                availableWidth -= width;
            }

            int i = 0;
            while (i < size && availableWidth > 0)
            {
                if (!m_topDataProvider.IsItemInPrimaryList(i) && !CollectionHelper.contains(includeItems, i))
                {
                    var width = m_topDataProvider.GetWidthForItem(i);
                    if (availableWidth >= width)
                    {
                        toBeMoved.Add(i);
                        availableWidth -= width;
                    }
                    else
                    {
                        break;
                    }
                }
                i++;
            }
            // Keep at one item is not in primary list. Two possible reason: 
            //  1, Most likely it's caused by m_topNavigationRecoveryGracePeriod
            //  2, virtualization and it doesn't have cached width
            if (i == size && !toBeMoved.Empty())
            {
                toBeMoved.RemoveLast();
            }
            return toBeMoved;
        }

        List<int> FindMovableItemsToBeRemovedFromPrimaryList(double widthAtLeastToBeRemoved, List<int> excludeItems)
        {
            List<int> toBeMoved = new List<int>();

            int i = m_topDataProvider.Size() - 1;
            while (i >= 0 && widthAtLeastToBeRemoved > 0)
            {
                if (m_topDataProvider.IsItemInPrimaryList(i))
                {
                    if (!CollectionHelper.contains(excludeItems, i))
                    {
                        var width = m_topDataProvider.GetWidthForItem(i);
                        toBeMoved.Add(i);
                        widthAtLeastToBeRemoved -= width;
                    }
                }
                i--;
            }

            return toBeMoved;
        }

        List<int> FindMovableItemsBeyondAvailableWidth(double availableWidth)
        {
            List<int> toBeMoved = new List<int>();
            if (m_topNavRepeater is { } ir)
            {
                int selectedItemIndexInPrimary = m_topDataProvider.IndexOf(SelectedItem, NavigationViewSplitVectorID.PrimaryList);
                int size = m_topDataProvider.GetPrimaryListSize();

                double requiredWidth = 0;

                for (int i = 0; i < size; i++)
                {
                    if (i != selectedItemIndexInPrimary)
                    {
                        bool shouldMove = true;
                        if (requiredWidth <= availableWidth)
                        {
                            var container = ir.TryGetElement(i);
                            if (container != null)
                            {
                                if (container is UIElement containerAsUIElement)
                                {
                                    var width = containerAsUIElement.DesiredSize.Width;
                                    requiredWidth += width;
                                    shouldMove = requiredWidth > availableWidth;
                                }
                            }
                            else
                            {
                                // item is virtualized but not realized.                    
                            }
                        }

                        if (shouldMove)
                        {
                            toBeMoved.Add(i);
                        }
                    }
                }
            }

            return m_topDataProvider.ConvertPrimaryIndexToIndex(toBeMoved);
        }

        void KeepAtLeastOneItemInPrimaryList(List<int> itemInPrimaryToBeRemoved, bool shouldKeepFirst)
        {
            if (!itemInPrimaryToBeRemoved.Empty() && itemInPrimaryToBeRemoved.Count == m_topDataProvider.GetPrimaryListSize())
            {
                if (shouldKeepFirst)
                {
                    itemInPrimaryToBeRemoved.RemoveAt(0);
                }
                else
                {
                    itemInPrimaryToBeRemoved.RemoveLast();
                }
            }
        }

        int GetSelectedItemIndex()
        {
            return m_topDataProvider.IndexOf(SelectedItem);
        }

        double GetPaneToggleButtonWidth()
        {
            return (double)(SharedHelpers.FindResource("PaneToggleButtonWidth", this, (double)c_paneToggleButtonWidth));
        }

        double GetPaneToggleButtonHeight()
        {
            return (double)(SharedHelpers.FindResource("PaneToggleButtonHeight", this, (double)c_paneToggleButtonHeight));
        }

        void UpdateTopNavigationWidthCache()
        {
            int size = m_topDataProvider.GetPrimaryListSize();
            if (m_topNavRepeater is { } ir)
            {
                for (int i = 0; i < size; i++)
                {
                    var container = ir.TryGetElement(i);
                    if (container != null)
                    {
                        if (container is UIElement containerAsUIElement)
                        {
                            var width = containerAsUIElement.DesiredSize.Width;
                            m_topDataProvider.UpdateWidthForPrimaryItem(i, width);
                        }
                    }
                    else
                    {
                        break;
                    }
                }
            }
        }

        bool IsTopNavigationView()
        {
            return PaneDisplayMode == NavigationViewPaneDisplayMode.Top;
        }

        bool IsTopPrimaryListVisible()
        {
            return m_topNavRepeater != null && (TemplateSettings.TopPaneVisibility == Visibility.Visible);
        }

        void CoerceToGreaterThanZero(ref double value)
        {
            // Property coercion for OpenPaneLength, CompactPaneLength, CompactModeThresholdWidth, ExpandedModeThresholdWidth
            value = Math.Max(value, 0.0);
        }

        void PropertyChanged(DependencyPropertyChangedEventArgs args)
        {
            DependencyProperty property = args.Property;

            if (property == IsPaneOpenProperty)
            {
                OnIsPaneOpenChanged();
                UpdateVisualStateForDisplayModeGroup(DisplayMode);
            }
            else if (property == CompactModeThresholdWidthProperty ||
                property == ExpandedModeThresholdWidthProperty)
            {
                UpdateAdaptiveLayout(ActualWidth);
            }
            else if (property == AlwaysShowHeaderProperty || property == HeaderProperty)
            {
                UpdateHeaderVisibility();
            }
            else if (property == SelectedItemProperty)
            {
                OnSelectedItemPropertyChanged(args);
            }
            else if (property == PaneTitleProperty)
            {
                UpdatePaneTitleFrameworkElementParents();
                UpdateBackAndCloseButtonsVisibility();
                UpdatePaneToggleSize();
            }
            else if (property == IsBackButtonVisibleProperty)
            {
                UpdateBackAndCloseButtonsVisibility();
                UpdateAdaptiveLayout(ActualWidth);
                if (IsTopNavigationView())
                {
                    InvalidateTopNavPrimaryLayout();
                }

                /*
                if (g_IsTelemetryProviderEnabled && IsBackButtonVisible == NavigationViewBackButtonVisible.Collapsed)
                {
                    //  Explicitly disabling BackUI on NavigationView
                    TraceLoggingWrite(
                        g_hTelemetryProvider,
                        "NavigationView_DisableBackUI",
                        TraceLoggingDescription("Developer explicitly disables the BackUI on NavigationView"));
                }
                */
                // Enabling back button shifts grid instead of resizing, so let's update the layout.
                if (m_backButton is { } backButton)
                {
                    backButton.UpdateLayout();
                }
                UpdatePaneLayout();
            }
            else if (property == MenuItemsSourceProperty)
            {
                UpdateRepeaterItemsSource(true /*forceSelectionModelUpdate*/);
            }
            else if (property == MenuItemsProperty)
            {
                UpdateRepeaterItemsSource(true /*forceSelectionModelUpdate*/);
            }
            else if (property == FooterMenuItemsSourceProperty)
            {
                UpdateFooterRepeaterItemsSource(true /*sourceCollectionReset*/, true /*sourceCollectionChanged*/);
            }
            else if (property == FooterMenuItemsProperty)
            {
                UpdateFooterRepeaterItemsSource(true /*sourceCollectionReset*/, true /*sourceCollectionChanged*/);
            }
            else if (property == PaneDisplayModeProperty)
            {
                // m_wasForceClosed is set to true because ToggleButton is clicked and Pane is closed.
                // When PaneDisplayMode is changed, reset the force flag to make the Pane can be opened automatically again.
                m_wasForceClosed = false;

                CollapseTopLevelMenuItems((NavigationViewPaneDisplayMode)args.OldValue);
                UpdatePaneToggleButtonVisibility();
                UpdatePaneDisplayMode((NavigationViewPaneDisplayMode)args.OldValue, (NavigationViewPaneDisplayMode)args.NewValue);
                UpdatePaneTitleFrameworkElementParents();
                UpdatePaneVisibility();
                UpdateVisualState();
                UpdatePaneButtonsWidths();
            }
            else if (property == IsPaneVisibleProperty)
            {
                UpdatePaneVisibility();
                UpdateVisualStateForDisplayModeGroup(DisplayMode);

                // When NavView is in expaneded mode with fixed window size, setting IsPaneVisible to false doesn't closes the pane
                // We manually close/open it for this case
                if (!IsPaneVisible && IsPaneOpen)
                {
                    ClosePane();
                }

                if (IsPaneVisible && DisplayMode == NavigationViewDisplayMode.Expanded && !IsPaneOpen)
                {
                    OpenPane();
                }
            }
            else if (property == OverflowLabelModeProperty)
            {
                if (m_appliedTemplate)
                {
                    UpdateVisualStateForOverflowButton();
                    InvalidateTopNavPrimaryLayout();
                }
            }
            else if (property == AutoSuggestBoxProperty)
            {
                InvalidateTopNavPrimaryLayout();
            }
            else if (property == SelectionFollowsFocusProperty)
            {
                UpdateSingleSelectionFollowsFocusTemplateSetting();
            }
            else if (property == IsPaneToggleButtonVisibleProperty)
            {
                UpdatePaneTitleFrameworkElementParents();
                UpdateBackAndCloseButtonsVisibility();
                UpdatePaneToggleButtonVisibility();
                UpdateVisualState();
            }
            else if (property == IsSettingsVisibleProperty)
            {
                UpdateFooterRepeaterItemsSource(false /*sourceCollectionReset*/, true /*sourceCollectionChanged*/);
            }
            else if (property == CompactPaneLengthProperty)
            {
                // Need to update receiver margins when CompactPaneLength changes
                UpdatePaneShadow();

                // Update pane-button-grid width when pane is closed and we are not in minimal
                UpdatePaneButtonsWidths();
            }
            else if (property == IsTitleBarAutoPaddingEnabledProperty)
            {
                UpdateTitleBarPadding();
            }
            else if (property == MenuItemTemplateProperty ||
                property == MenuItemTemplateSelectorProperty)
            {
                SyncItemTemplates();
            }
        }

        void UpdateNavigationViewItemsFactory()
        {
            object newItemTemplate = MenuItemTemplate;
            if (newItemTemplate == null)
            {
                newItemTemplate = MenuItemTemplateSelector;
            }
            m_navigationViewItemsFactory.UserElementFactory(newItemTemplate);
        }

        void SyncItemTemplates()
        {
            UpdateNavigationViewItemsFactory();
        }

        void OnRepeaterIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue)
            {
                Dispatcher.BeginInvoke(() =>
                {
                    var repeater = (ItemsRepeater)sender;
                    if (repeater.IsLoaded)
                    {
                        OnRepeaterLoaded(sender, null);
                    }
                }, DispatcherPriority.Loaded);
            }
        }

        void OnRepeaterLoaded(object sender, RoutedEventArgs args)
        {
            if (SelectedItem is { } item)
            {
                if (!IsSelectionSuppressed(item))
                {
                    if (NavigationViewItemOrSettingsContentFromData(item) is { } navViewItem)
                    {
                        navViewItem.IsSelected = true;
                    }
                }
                AnimateSelectionChanged(item);
            }
        }

        // If app is .net app, the lifetime of NavigationView maybe depends on garbage collection.
        // Unlike other revoker, TitleBar is in global space and we need to stop receiving changed event when it's unloaded.
        // So we do hook it in Loaded and Unhook it in Unloaded
        void OnUnloaded(object sender, RoutedEventArgs args)
        {
            if (m_coreTitleBar is { } coreTitleBar)
            {
                coreTitleBar.LayoutMetricsChanged -= OnTitleBarMetricsChanged;
                coreTitleBar.IsVisibleChanged -= OnTitleBarIsVisibleChanged;
            }
        }

        void OnLoaded(object sender, RoutedEventArgs args)
        {
            if (m_coreTitleBar is { } coreTitleBar)
            {
                coreTitleBar.LayoutMetricsChanged += OnTitleBarMetricsChanged;
                coreTitleBar.IsVisibleChanged += OnTitleBarIsVisibleChanged;
            }
            // Update pane buttons now since we the CompactPaneLength is actually known now.
            UpdatePaneButtonsWidths();
        }

        void OnIsPaneOpenChanged()
        {
            var isPaneOpen = IsPaneOpen;
            if (isPaneOpen && m_wasForceClosed)
            {
                m_wasForceClosed = false; // remove the pane open flag since Pane is opened.
            }
            else if (!m_isOpenPaneForInteraction && !isPaneOpen)
            {
                if (m_rootSplitView is { } splitView)
                {
                    // splitview.IsPaneOpen and nav.IsPaneOpen is two way binding. If nav.IsPaneOpen=false and splitView.IsPaneOpen=true,
                    // then the pane has been closed by API and we treat it as a forced close.
                    // If, however, splitView.IsPaneOpen=false, then nav.IsPaneOpen is just following the SplitView here and the pane
                    // was closed, for example, due to app window resizing. We don't set the force flag in this situation.
                    m_wasForceClosed = splitView.IsPaneOpen;
                }
                else
                {
                    // If there is no SplitView (for example it hasn't been loaded yet) then nav.IsPaneOpen was set directly
                    // so we treat it as a closed force.
                    m_wasForceClosed = true;
                }
            }

            SetPaneToggleButtonAutomationName();
            UpdatePaneTabFocusNavigation();
            UpdateSettingsItemToolTip();
            UpdatePaneTitleFrameworkElementParents();

            if (SharedHelpers.IsThemeShadowAvailable())
            {
                if (m_rootSplitView is { } splitView)
                {
                    var displayMode = splitView.DisplayMode;
                    var isOverlay = displayMode == SplitViewDisplayMode.Overlay || displayMode == SplitViewDisplayMode.CompactOverlay;
                    if (splitView.Pane is { } paneRoot)
                    {
                        /*
                        var currentTranslation = paneRoot.Translation();
                        var translation = float3{ currentTranslation.x, currentTranslation.y, IsPaneOpen && isOverlay ? c_paneElevationTranslationZ : 0.0f };
                        paneRoot.Translation(translation);
                        */
                    }
                }
            }
            UpdatePaneButtonsWidths();
        }

        void UpdatePaneToggleButtonVisibility()
        {
            var visible = IsPaneToggleButtonVisible && !IsTopNavigationView();
            GetTemplateSettings().PaneToggleButtonVisibility = Util.VisibilityFromBool(visible);
        }

        void UpdatePaneDisplayMode()
        {
            if (!m_appliedTemplate)
            {
                return;
            }
            if (!IsTopNavigationView())
            {
                UpdateAdaptiveLayout(ActualWidth, true /*forceSetDisplayMode*/);

                SwapPaneHeaderContent(m_leftNavPaneHeaderContentBorder, m_paneHeaderOnTopPane, "PaneHeader");
                SwapPaneHeaderContent(m_leftNavPaneCustomContentBorder, m_paneCustomContentOnTopPane, "PaneCustomContent");
                SwapPaneHeaderContent(m_leftNavFooterContentBorder, m_paneFooterOnTopPane, "PaneFooter");

                CreateAndHookEventsToSettings();

                /*
                if (this is IUIElement8 thisAsUIElement8)
                {
                    if (m_paneToggleButton is { } paneToggleButton)
                    {
                        thisAsUIElement8.KeyTipTarget(paneToggleButton);
                    }
                }
                */

            }
            else
            {
                ClosePane();
                SetDisplayMode(NavigationViewDisplayMode.Minimal, true);

                SwapPaneHeaderContent(m_paneHeaderOnTopPane, m_leftNavPaneHeaderContentBorder, "PaneHeader");
                SwapPaneHeaderContent(m_paneCustomContentOnTopPane, m_leftNavPaneCustomContentBorder, "PaneCustomContent");
                SwapPaneHeaderContent(m_paneFooterOnTopPane, m_leftNavFooterContentBorder, "PaneFooter");

                CreateAndHookEventsToSettings();

                /*
                if (this is IUIElement8 thisAsUIElement8)
                {
                    if (m_topNavOverflowButton is { } topNavOverflowButton)
                    {
                        thisAsUIElement8.KeyTipTarget(topNavOverflowButton);
                    }
                }
                */
            }

            UpdateContentBindingsForPaneDisplayMode();
            UpdateRepeaterItemsSource(false /*forceSelectionModelUpdate*/);
            UpdateFooterRepeaterItemsSource(false /*sourceCollectionReset*/, false /*sourceCollectionChanged*/);
            if (SelectedItem is { } selectedItem)
            {
                m_OrientationChangedPendingAnimation = true;
            }
        }

        void UpdatePaneDisplayMode(NavigationViewPaneDisplayMode oldDisplayMode, NavigationViewPaneDisplayMode newDisplayMode)
        {
            if (!m_appliedTemplate)
            {
                return;
            }

            UpdatePaneDisplayMode();

            // For better user experience, We help customer to Open/Close Pane automatically when we switch between LeftMinimal <. Left.
            // From other navigation PaneDisplayMode to LeftMinimal, we expect pane is closed.
            // From LeftMinimal to Left, it is expected the pane is open. For other configurations, this seems counterintuitive.
            // See #1702 and #1787
            if (!IsTopNavigationView())
            {
                if (IsPaneOpen)
                {
                    if (newDisplayMode == NavigationViewPaneDisplayMode.LeftMinimal)
                    {
                        ClosePane();
                    }
                }
                else
                {
                    if (oldDisplayMode == NavigationViewPaneDisplayMode.LeftMinimal
                        && newDisplayMode == NavigationViewPaneDisplayMode.Left)
                    {
                        OpenPane();
                    }
                }
            }
        }

        void UpdatePaneVisibility()
        {
            var templateSettings = GetTemplateSettings();
            if (IsPaneVisible)
            {
                if (IsTopNavigationView())
                {
                    templateSettings.LeftPaneVisibility = Visibility.Collapsed;
                    templateSettings.TopPaneVisibility = Visibility.Visible;
                }
                else
                {
                    templateSettings.TopPaneVisibility = Visibility.Collapsed;
                    templateSettings.LeftPaneVisibility = Visibility.Visible;
                }

                VisualStateManager.GoToState(this, "PaneVisible", false /*useTransitions*/);
            }
            else
            {
                templateSettings.TopPaneVisibility = Visibility.Collapsed;
                templateSettings.LeftPaneVisibility = Visibility.Collapsed;

                VisualStateManager.GoToState(this, "PaneCollapsed", false /*useTransitions*/);
            }
        }

        void SwapPaneHeaderContent(ContentControl newParentTrackRef, ContentControl oldParentTrackRef, string propertyPathName)
        {
            if (newParentTrackRef is { } newParent)
            {
                if (oldParentTrackRef is { } oldParent)
                {
                    oldParent.ClearValue(ContentControl.ContentProperty);
                }

                SharedHelpers.SetBinding(propertyPathName, newParent, ContentControl.ContentProperty);
            }
        }

        void UpdateContentBindingsForPaneDisplayMode()
        {
            UIElement autoSuggestBoxContentControl = null;
            UIElement notControl = null;
            if (!IsTopNavigationView())
            {
                autoSuggestBoxContentControl = m_leftNavPaneAutoSuggestBoxPresenter;
                notControl = m_topNavPaneAutoSuggestBoxPresenter;
            }
            else
            {
                autoSuggestBoxContentControl = m_topNavPaneAutoSuggestBoxPresenter;
                notControl = m_leftNavPaneAutoSuggestBoxPresenter;
            }

            if (autoSuggestBoxContentControl != null)
            {
                if (notControl != null)
                {
                    notControl.ClearValue(ContentControl.ContentProperty);
                }

                SharedHelpers.SetBinding("AutoSuggestBox", autoSuggestBoxContentControl, ContentControl.ContentProperty);
            }
        }

        void UpdateHeaderVisibility()
        {
            if (!m_appliedTemplate)
            {
                return;
            }

            UpdateHeaderVisibility(DisplayMode);
        }

        void UpdateHeaderVisibility(NavigationViewDisplayMode displayMode)
        {
            // Ignore AlwaysShowHeader property in case DisplayMode is Minimal and it's not Top NavigationView
            bool showHeader = AlwaysShowHeader || (!IsTopNavigationView() && displayMode == NavigationViewDisplayMode.Minimal);

            // Like bug 17517627, Customer like WallPaper Studio 10 expects a HeaderContent visual even if Header() is null. 
            // App crashes when they have dependency on that visual, but the crash is not directly state that it's a header problem.   
            // NavigationView doesn't use quirk, but we determine the version by themeresource.
            // As a workaround, we 'quirk' it for RS4 or before release. if it's RS4 or before, HeaderVisible is not related to Header().
            // If theme resource is RS5 or later, we will not show header if header is null.
            if (SharedHelpers.IsRS5OrHigher())
            {
                showHeader = Header != null && showHeader;
            }
            VisualStateManager.GoToState(this, showHeader ? "HeaderVisible" : "HeaderCollapsed", false /*useTransitions*/);
        }

        void UpdatePaneTabFocusNavigation()
        {
            if (!m_appliedTemplate)
            {
                return;
            }

            if (SharedHelpers.IsRS2OrHigher())
            {
                KeyboardNavigationMode mode = KeyboardNavigationMode.Local;

                if (m_rootSplitView is { } splitView)
                {
                    // If the pane is open in an overlay (light-dismiss) mode, trap keyboard focus inside the pane
                    if (IsPaneOpen && (splitView.DisplayMode == SplitViewDisplayMode.Overlay || splitView.DisplayMode == SplitViewDisplayMode.CompactOverlay))
                    {
                        mode = KeyboardNavigationMode.Cycle;
                    }
                }

                if (m_paneContentGrid is { } paneContentGrid)
                {
                    //paneContentGrid.TabFocusNavigation(mode);
                    KeyboardNavigation.SetTabNavigation(paneContentGrid, mode);
                }
            }
        }

        void UpdatePaneToggleSize()
        {
            if (!ShouldPreserveNavigationViewRS3Behavior())
            {
                if (m_rootSplitView is { } splitView)
                {
                    double width = GetPaneToggleButtonWidth();
                    double togglePaneButtonWidth = width;

                    if (ShouldShowBackButton() && splitView.DisplayMode == SplitViewDisplayMode.Overlay)
                    {
                        double backButtonWidth = c_backButtonWidth;
                        if (m_backButton is { } backButton)
                        {
                            backButtonWidth = backButton.Width;
                        }

                        width += backButtonWidth;
                    }

                    if (!m_isClosedCompact && PaneTitle?.Length > 0)
                    {
                        if (splitView.DisplayMode == SplitViewDisplayMode.Overlay && IsPaneOpen)
                        {
                            width = OpenPaneLength;
                            togglePaneButtonWidth = OpenPaneLength - ((ShouldShowBackButton() || ShouldShowCloseButton()) ? c_backButtonWidth : 0);
                        }
                        else if (!(splitView.DisplayMode == SplitViewDisplayMode.Overlay && !IsPaneOpen))
                        {
                            width = OpenPaneLength;
                            togglePaneButtonWidth = OpenPaneLength;
                        }
                    }

                    if (m_paneToggleButton is { } toggleButton)
                    {
                        toggleButton.Width = togglePaneButtonWidth;
                    }
                }
            }
        }

        void UpdateBackAndCloseButtonsVisibility()
        {
            if (!m_appliedTemplate)
            {
                return;
            }

            var shouldShowBackButton = ShouldShowBackButton();
            var backButtonVisibility = Util.VisibilityFromBool(shouldShowBackButton);
            var visualStateDisplayMode = GetVisualStateDisplayMode(DisplayMode);
            bool useLeftPaddingForBackOrCloseButton =
                (visualStateDisplayMode == NavigationViewVisualStateDisplayMode.Minimal && !IsTopNavigationView()) ||
                visualStateDisplayMode == NavigationViewVisualStateDisplayMode.MinimalWithBackButton;
            double leftPaddingForBackOrCloseButton = 0.0;
            double paneHeaderPaddingForToggleButton = 0.0;
            double paneHeaderPaddingForCloseButton = 0.0;
            double paneHeaderContentBorderRowMinHeight = 0.0;

            GetTemplateSettings().BackButtonVisibility = backButtonVisibility;

            if (m_paneToggleButton != null && IsPaneToggleButtonVisible)
            {
                paneHeaderContentBorderRowMinHeight = GetPaneToggleButtonHeight();
                paneHeaderPaddingForToggleButton = GetPaneToggleButtonWidth();

                if (useLeftPaddingForBackOrCloseButton)
                {
                    leftPaddingForBackOrCloseButton = paneHeaderPaddingForToggleButton;
                }
            }

            if (m_backButton is { } backButton)
            {
                if (ShouldPreserveNavigationViewRS4Behavior())
                {
                    backButton.Visibility = backButtonVisibility;
                }

                if (useLeftPaddingForBackOrCloseButton && backButtonVisibility == Visibility.Visible)
                {
                    leftPaddingForBackOrCloseButton += backButton.Width;
                }
            }

            if (m_closeButton is { } closeButton)
            {
                var closeButtonVisibility = Util.VisibilityFromBool(ShouldShowCloseButton());

                closeButton.Visibility = closeButtonVisibility;

                if (closeButtonVisibility == Visibility.Visible)
                {
                    paneHeaderContentBorderRowMinHeight = Math.Max(paneHeaderContentBorderRowMinHeight, closeButton.Height);

                    if (useLeftPaddingForBackOrCloseButton)
                    {
                        paneHeaderPaddingForCloseButton = closeButton.Width;
                        leftPaddingForBackOrCloseButton += paneHeaderPaddingForCloseButton;
                    }
                }
            }

            if (m_contentLeftPadding is { } contentLeftPadding)
            {
                contentLeftPadding.Width = leftPaddingForBackOrCloseButton;
            }

            if (m_paneHeaderToggleButtonColumn is { } paneHeaderToggleButtonColumn)
            {
                // Account for the PaneToggleButton's width in the PaneHeader's placement.
                paneHeaderToggleButtonColumn.Width = GridLengthHelper.FromValueAndType(paneHeaderPaddingForToggleButton, GridUnitType.Pixel);
            }

            if (m_paneHeaderCloseButtonColumn is { } paneHeaderCloseButtonColumn)
            {
                // Account for the CloseButton's width in the PaneHeader's placement.
                paneHeaderCloseButtonColumn.Width = GridLengthHelper.FromValueAndType(paneHeaderPaddingForCloseButton, GridUnitType.Pixel);
            }

            if (m_paneTitleHolderFrameworkElement is { } paneTitleHolderFrameworkElement)
            {
                if (paneHeaderContentBorderRowMinHeight == 0.00 && paneTitleHolderFrameworkElement.Visibility == Visibility.Visible)
                {
                    // Handling the case where the PaneTottleButton is collapsed and the PaneTitle's height needs to push the rest of the NavigationView's UI down.
                    paneHeaderContentBorderRowMinHeight = paneTitleHolderFrameworkElement.ActualHeight;
                }
            }

            if (m_paneHeaderContentBorderRow is { } paneHeaderContentBorderRow)
            {
                paneHeaderContentBorderRow.MinHeight = paneHeaderContentBorderRowMinHeight;
            }

            if (m_paneContentGrid is { } paneContentGridAsUIE)
            {
                if (paneContentGridAsUIE is Grid paneContentGrid)
                {
                    var rowDefs = paneContentGrid.RowDefinitions;

                    if (rowDefs.Count >= c_backButtonRowDefinition)
                    {
                        var rowDef = rowDefs[c_backButtonRowDefinition];

                        int backButtonRowHeight = 0;
                        if (!IsOverlay() && shouldShowBackButton)
                        {
                            backButtonRowHeight = c_backButtonHeight;
                        }
                        else if (ShouldPreserveNavigationViewRS3Behavior())
                        {
                            // This row represented the height of the hamburger+margin in RS3 and prior
                            backButtonRowHeight = c_toggleButtonHeightWhenShouldPreserveNavigationViewRS3Behavior;
                        }

                        var length = GridLengthHelper.FromPixels(backButtonRowHeight);
                        rowDef.Height = length;
                    }
                }
            }

            if (!ShouldPreserveNavigationViewRS4Behavior())
            {
                VisualStateManager.GoToState(this, shouldShowBackButton ? "BackButtonVisible" : "BackButtonCollapsed", false /*useTransitions*/);
            }
            UpdateTitleBarPadding();
        }

        void UpdatePaneTitleMargins()
        {
            if (ShouldPreserveNavigationViewRS4Behavior())
            {
                if (m_paneTitleFrameworkElement is { } paneTitleFrameworkElement)
                {
                    double width = GetPaneToggleButtonWidth();

                    if (ShouldShowBackButton() && IsOverlay())
                    {
                        width += c_backButtonWidth;
                    }

                    paneTitleFrameworkElement.Margin = new Thickness(width, 0, 0, 0); // see "Hamburger title" on uni
                }
            }
        }

        void UpdateSelectionForMenuItems()
        {
            // Allow customer to set selection by NavigationViewItem.IsSelected.
            // If there are more than two items are set IsSelected=true, the first one is actually selected.
            // If SelectedItem is set, IsSelected is ignored.
            //         <NavigationView.MenuItems>
            //              <NavigationViewItem Content = "Collection" IsSelected = "True" / >
            //         </NavigationView.MenuItems>
            if (SelectedItem == null)
            {
                bool foundFirstSelected = false;

                // firstly check Menu items
                if (MenuItems is IList menuItems)
                {
                    foundFirstSelected = UpdateSelectedItemFromMenuItems(menuItems);
                }

                // then do same for footer items and tell wenever selected item alreadyfound in MenuItems
                if (FooterMenuItems is IList footerItems)
                {
                    UpdateSelectedItemFromMenuItems(footerItems, foundFirstSelected);
                }
            }
        }

        bool UpdateSelectedItemFromMenuItems(IList menuItems, bool foundFirstSelected = false)
        {
            for (int i = 0; i < menuItems.Count; i++)
            {
                if (menuItems[i] is NavigationViewItem item)
                {
                    if (item.IsSelected)
                    {
                        if (!foundFirstSelected)
                        {
                            try
                            {
                                m_shouldIgnoreNextSelectionChange = true;
                                SelectedItem = item;
                                foundFirstSelected = true;
                            }
                            finally
                            {
                                m_shouldIgnoreNextSelectionChange = false;
                            }
                        }
                        else
                        {
                            item.IsSelected = false;
                        }
                    }
                }
            }
            return foundFirstSelected;
        }

        void OnTitleBarMetricsChanged(object sender, object args)
        {
            UpdateTitleBarPadding();
        }

        void OnTitleBarIsVisibleChanged(CoreApplicationViewTitleBar sender, object args)
        {
            UpdateTitleBarPadding();
        }

        void ClosePaneIfNeccessaryAfterItemIsClicked(NavigationViewItem selectedContainer)
        {
            if (IsPaneOpen &&
                DisplayMode != NavigationViewDisplayMode.Expanded &&
                !DoesNavigationViewItemHaveChildren(selectedContainer) &&
                !m_shouldIgnoreNextSelectionChange)
            {
                ClosePane();
            }
        }

        bool NeedTopPaddingForRS5OrHigher(CoreApplicationViewTitleBar coreTitleBar)
        {
            // Starting on RS5, we will be using the following IsVisible API together with ExtendViewIntoTitleBar
            // to decide whether to try to add top padding or not.
            // We don't add padding when in fullscreen or tablet mode.
            return coreTitleBar.IsVisible && coreTitleBar.ExtendViewIntoTitleBar
                && !IsFullScreenOrTabletMode();
        }

        void UpdateTitleBarPadding()
        {
            if (!m_appliedTemplate)
            {
                return;
            }

            double topPadding = 0;

            if (m_coreTitleBar is { } coreTitleBar)
            {
                bool needsTopPadding = false;

                // Do not set a top padding when the IsTitleBarAutoPaddingEnabled property is set to False.
                if (IsTitleBarAutoPaddingEnabled)
                {
                    if (ShouldPreserveNavigationViewRS3Behavior())
                    {
                        needsTopPadding = true;
                    }
                    else if (ShouldPreserveNavigationViewRS4Behavior())
                    {
                        // For RS4 apps maintain the behavior that we shipped for RS4.
                        // We keep this behavior for app compact purposes.
                        needsTopPadding = !coreTitleBar.ExtendViewIntoTitleBar;
                    }
                    else
                    {
                        needsTopPadding = NeedTopPaddingForRS5OrHigher(coreTitleBar);
                    }
                }

                if (needsTopPadding)
                {
                    // Only add extra padding if the NavView is the "root" of the app,
                    // but not if the app is expanding into the titlebar
                    UIElement root = (Window.GetWindow(this) ?? Application.Current.MainWindow).Content as UIElement;
                    GeneralTransform gt = this.SafeTransformToVisual(root);
                    Point pos = gt.Transform(new Point());

                    if (pos.Y == 0.0)
                    {
                        topPadding = coreTitleBar.Height;
                    }
                }

                if (ShouldPreserveNavigationViewRS4Behavior())
                {
                    {
                        if (m_togglePaneTopPadding is { } fe)
                        {
                            fe.Height = topPadding;
                        }
                    }

                    {
                        if (m_contentPaneTopPadding is { } fe)
                        {
                            fe.Height = topPadding;
                        }
                    }
                }

                var paneTitleHolderFrameworkElement = m_paneTitleHolderFrameworkElement;
                var paneToggleButton = m_paneToggleButton;

                bool setPaneTitleHolderFrameworkElementMargin = paneTitleHolderFrameworkElement != null && paneTitleHolderFrameworkElement.Visibility == Visibility.Visible;
                bool setPaneToggleButtonMargin = !setPaneTitleHolderFrameworkElementMargin && paneToggleButton != null && paneToggleButton.Visibility == Visibility.Visible;

                if (setPaneTitleHolderFrameworkElementMargin || setPaneToggleButtonMargin)
                {
                    var thickness = ThicknessHelper.FromLengths(0, 0, 0, 0);

                    if (ShouldShowBackButton())
                    {
                        if (IsOverlay())
                        {
                            thickness = ThicknessHelper.FromLengths(c_backButtonWidth, 0, 0, 0);
                        }
                        else
                        {
                            thickness = ThicknessHelper.FromLengths(0, c_backButtonHeight, 0, 0);
                        }
                    }
                    else if (ShouldShowCloseButton() && IsOverlay())
                    {
                        thickness = ThicknessHelper.FromLengths(c_backButtonWidth, 0, 0, 0);
                    }

                    if (setPaneTitleHolderFrameworkElementMargin)
                    {
                        // The PaneHeader is hosted by PaneTitlePresenter and PaneTitleHolder.
                        paneTitleHolderFrameworkElement.Margin = thickness;
                    }
                    else
                    {
                        // The PaneHeader is hosted by PaneToggleButton
                        paneToggleButton.Margin = thickness;
                    }
                }
            }

            if (TemplateSettings is { } templateSettings)
            {
                // 0.0 and 0.00000000 is not the same in double world. try to reduce the number of TopPadding update event. epsilon is 0.1 here.
                if (Math.Abs(templateSettings.TopPadding - topPadding) > 0.1)
                {
                    GetTemplateSettings().TopPadding = topPadding;
                }
            }
        }

        void RaiseDisplayModeChanged(NavigationViewDisplayMode displayMode)
        {
            SetValue(DisplayModePropertyKey, displayMode);
            var eventArgs = new NavigationViewDisplayModeChangedEventArgs();
            eventArgs.DisplayMode = displayMode;
            DisplayModeChanged?.Invoke(this, eventArgs);
        }

        // This method attaches the series of animations which are fired off dependent upon the amount 
        // of space give and the length of the strings involved. It occurs upon re-rendering.
        void CreateAndAttachHeaderAnimation(Visual visual)
        {
            /*
            var compositor = visual.Compositor();
            var cubicFunction = compositor.CreateCubicBezierEasingFunction({ 0.0f, 0.35f }, { 0.15f, 1.0f });
            var moveAnimation = compositor.CreateVector3KeyFrameAnimation();
            moveAnimation.Target("Offset");
            moveAnimation.InsertExpressionKeyFrame(1.0f, "this.FinalValue", cubicFunction);
            moveAnimation.Duration(200ms);

            var collection = compositor.CreateImplicitAnimationCollection();
            collection.Insert("Offset", moveAnimation);
            visual.ImplicitAnimations(collection);
            */
        }

        bool IsFullScreenOrTabletMode()
        {
            /*
            // ApplicationView.GetForCurrentView() is an expensive call - make sure to cache the ApplicationView
            if (!m_applicationView)
            {
                m_applicationView = ViewManagement.ApplicationView.GetForCurrentView();
            }

            // UIViewSettings.GetForCurrentView() is an expensive call - make sure to cache the UIViewSettings
            if (!m_uiViewSettings)
            {
                m_uiViewSettings = ViewManagement.UIViewSettings.GetForCurrentView();
            }

            bool isFullScreenMode = m_applicationView.IsFullScreenMode();
            bool isTabletMode = m_uiViewSettings.UserInteractionMode() == ViewManagement.UserInteractionMode.Touch;

            return isFullScreenMode || isTabletMode;
            */
            return false;
        }

        void UpdatePaneShadow()
        {
            /*
            if (SharedHelpers.IsThemeShadowAvailable())
            {
                Canvas shadowReceiver = GetTemplateChildT<Canvas>(c_paneShadowReceiverCanvas, this);
                if (shadowReceiver == null)
                {
                    shadowReceiver = new Canvas();
                    shadowReceiver.Name = (c_paneShadowReceiverCanvas);

                    if (GetTemplateChildT<Grid>(c_contentGridName, this) is { } contentGrid)
                    {
                        Grid.SetRowSpan(shadowReceiver, contentGrid.RowDefinitions.Count);
                        Grid.SetRow(shadowReceiver, 0);
                        // Only register to columns if those are actually defined
                        if (contentGrid.ColumnDefinitions.Count > 0)
                        {
                            Grid.SetColumn(shadowReceiver, 0);
                            Grid.SetColumnSpan(shadowReceiver, contentGrid.ColumnDefinitions.Count);
                        }
                        contentGrid.Children.Add(shadowReceiver);

                        ThemeShadow shadow;
                        shadow.Receivers().Append(shadowReceiver);
                        if (m_rootSplitView is { } splitView)
                        {
                            if (splitView.Pane is { } paneRoot)
                            {
                                if (paneRoot is IUIElement10 paneRoot_uiElement10)
                                {
                                    paneRoot_uiElement10.Shadow(shadow);
                                }
                            }
                        }
                    }
                }


                // Shadow will get clipped if casting on the splitView.Content directly
                // Creating a canvas with negative margins as receiver to allow shadow to be drawn outside the content grid 
                Thickness shadowReceiverMargin = new Thickness(0, -c_paneElevationTranslationZ, -c_paneElevationTranslationZ, -c_paneElevationTranslationZ);

                // Ensuring shadow is aligned to the left
                shadowReceiver.HorizontalAlignment = (HorizontalAlignment.Left);

                // Ensure shadow is as wide as the pane when it is open
                shadowReceiver.Width = (OpenPaneLength);
                shadowReceiver.Margin = (shadowReceiverMargin);
            }
            */
        }

        T GetContainerForData<T>(object data) where T : class
        {
            if (data == null)
            {
                return null;
            }

            if (data is T nvi)
            {
                return nvi;
            }

            if (m_settingsItem is { } settingsItem)
            {
                if (settingsItem == data || settingsItem.Content == data)
                {
                    return settingsItem as T;
                }
            }

            // First conduct a basic top level search in main menu, which should succeed for a lot of scenarios.
            var mainRepeater = IsTopNavigationView() ? m_topNavRepeater : m_leftNavRepeater;
            var itemIndex = GetIndexFromItem(mainRepeater, data);
            if (itemIndex >= 0)
            {
                if (mainRepeater.TryGetElement(itemIndex) is { } container)
                {
                    return container as T;
                }
            }

            // then look in footer menu
            var footerRepeater = IsTopNavigationView() ? m_topNavFooterMenuRepeater : m_leftNavFooterMenuRepeater;
            itemIndex = GetIndexFromItem(footerRepeater, data);
            if (itemIndex >= 0)
            {
                if (footerRepeater.TryGetElement(itemIndex) is { } container)
                {
                    return container as T;
                }
            }

            // If unsuccessful, unfortunately we are going to have to search through the whole tree
            // TODO: Either fix or remove implementation for TopNav.
            // It may not be required due to top nav rarely having realized children in its default state.
            {
                if (SearchEntireTreeForContainer(mainRepeater, data) is { } container)
                {
                    return container as T;
                }
            }

            {
                if (SearchEntireTreeForContainer(footerRepeater, data) is { } container)
                {
                    return container as T;
                }
            }

            return null;
        }

        UIElement SearchEntireTreeForContainer(ItemsRepeater rootRepeater, object data)
        {
            // TODO: Temporary inefficient solution that results in unnecessary time complexity, fix.
            var index = GetIndexFromItem(rootRepeater, data);
            if (index != -1)
            {
                return rootRepeater.TryGetElement(index);
            }

            for (int i = 0; i < GetContainerCountInRepeater(rootRepeater); i++)
            {
                if (rootRepeater.TryGetElement(i) is { } container)
                {
                    if (container is NavigationViewItem nvi)
                    {
                        if (nvi.GetRepeater() is { } nviRepeater)
                        {
                            if (SearchEntireTreeForContainer(nviRepeater, data) is { } foundElement)
                            {
                                return foundElement;
                            }
                        }
                    }
                }
            }
            return null;
        }

        IndexPath SearchEntireTreeForIndexPath(ItemsRepeater rootRepeater, object data, bool isFooterRepeater)
        {
            for (int i = 0; i < GetContainerCountInRepeater(rootRepeater); i++)
            {
                if (rootRepeater.TryGetElement(i) is { } container)
                {
                    if (container is NavigationViewItem nvi)
                    {
                        var ip = new IndexPath(new List<int> { isFooterRepeater ? c_footerMenuBlockIndex : c_mainMenuBlockIndex, i });
                        if (SearchEntireTreeForIndexPath(nvi, data, ip) is { } indexPath)
                        {
                            return indexPath;
                        }
                    }
                }
            }
            return null;
        }

        // There are two possibilities here if the passed in item has children. Either the children of the passed in container have already been realized,
        // in which case we simply just iterate through the children containers, or they have not been realized yet and we have to iterate through the data
        // and manually realize each item.
        IndexPath SearchEntireTreeForIndexPath(NavigationViewItem parentContainer, object data, IndexPath ip)
        {
            bool areChildrenRealized = false;
            if (parentContainer.GetRepeater() is { } childrenRepeater)
            {
                if (DoesRepeaterHaveRealizedContainers(childrenRepeater))
                {
                    areChildrenRealized = true;
                    for (int i = 0; i < GetContainerCountInRepeater(childrenRepeater); i++)
                    {
                        if (childrenRepeater.TryGetElement(i) is { } container)
                        {
                            if (container is NavigationViewItem nvi)
                            {
                                var newIndexPath = ip.CloneWithChildIndex(i);
                                if (nvi.Content == data)
                                {
                                    return newIndexPath;
                                }
                                else
                                {
                                    if (SearchEntireTreeForIndexPath(nvi, data, newIndexPath) is { } foundIndexPath)
                                    {
                                        return foundIndexPath;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            //If children are not realized, manually realize and search.
            if (!areChildrenRealized)
            {
                if (GetChildren(parentContainer) is { } childrenData)
                {
                    // Get children data in an enumarable form
                    var newDataSource = childrenData as ItemsSourceView;
                    if (childrenData != null && newDataSource == null)
                    {
                        newDataSource = new InspectingDataSource(childrenData);
                    }

                    for (int i = 0; i < newDataSource.Count; i++)
                    {
                        var newIndexPath = ip.CloneWithChildIndex(i);
                        var childData = newDataSource.GetAt(i);
                        if (childData == data)
                        {
                            return newIndexPath;
                        }
                        else
                        {
                            // Resolve databinding for item and search through that item's children
                            if (ResolveContainerForItem(childData, i) is { } nvib)
                            {
                                if (nvib is NavigationViewItem nvi)
                                {
                                    // Process x:bind
                                    //if (CachedVisualTreeHelpers.GetDataTemplateComponent(nvi) is { } extension)
                                    //{
                                    //    // Clear out old data. 
                                    //    extension.Recycle();
                                    //    int nextPhase = VirtualizationInfo.PhaseReachedEnd;
                                    //    // Run Phase 0
                                    //    extension.ProcessBindings(childData, i, 0 /* currentPhase */, nextPhase);

                                    //    // TODO: If nextPhase is not -1, ProcessBinding for all the phases
                                    //}

                                    if (SearchEntireTreeForIndexPath(nvi, data, newIndexPath) is { } foundIndexPath)
                                    {
                                        return foundIndexPath;
                                    }

                                    //TODO: Recycle container!
                                }
                            }
                        }
                    }
                }
            }

            return null;
        }

        NavigationViewItemBase ResolveContainerForItem(object item, int index)
        {
            var args = new ElementFactoryGetArgs();
            args.Data = item;
            args.Index = index;

            if (m_navigationViewItemsFactory.GetElement(args) is { } container)
            {
                if (container is NavigationViewItemBase nvib)
                {
                    return nvib;
                }
            }
            return null;
        }

        void RecycleContainer(UIElement container)
        {
            var args = new ElementFactoryRecycleArgs();
            args.Element = container;
            m_navigationViewItemsFactory.RecycleElement(args);
        }

        int GetContainerCountInRepeater(ItemsRepeater ir)
        {
            if (ir != null)
            {
                if (ir.ItemsSourceView is { } repeaterItemSourceView)
                {
                    return repeaterItemSourceView.Count;
                }
            }
            return -1;
        }

        bool DoesRepeaterHaveRealizedContainers(ItemsRepeater ir)
        {
            if (ir != null)
            {
                if (ir.TryGetElement(0) != null)
                {
                    return true;
                }
            }
            return false;
        }

        int GetIndexFromItem(ItemsRepeater ir, object data)
        {
            if (ir != null)
            {
                if (ir.ItemsSourceView is { } itemsSourceView)
                {
                    return itemsSourceView.IndexOf(data);
                }
            }
            return -1;
        }

        object GetItemFromIndex(ItemsRepeater ir, int index)
        {
            if (ir != null)
            {
                if (ir.ItemsSourceView is { } itemsSourceView)
                {
                    return itemsSourceView.GetAt(index);
                }
            }
            return null;
        }

        IndexPath GetIndexPathOfItem(object data)
        {
            if (data is NavigationViewItemBase nvib)
            {
                return GetIndexPathForContainer(nvib);
            }

            // In the databinding scenario, we need to conduct a search where we go through every item,
            // realizing it if necessary.
            if (IsTopNavigationView())
            {
                // First search through primary list
                {
                    if (SearchEntireTreeForIndexPath(m_topNavRepeater, data, false /*isFooterRepeater*/) is { } ip)
                    {
                        return ip;
                    }
                }

                // If item was not located in primary list, search through overflow
                {
                    if (SearchEntireTreeForIndexPath(m_topNavRepeaterOverflowView, data, false /*isFooterRepeater*/) is { } ip)
                    {
                        return ip;
                    }
                }

                // If item was not located in primary list and overflow, search through footer
                {
                    if (SearchEntireTreeForIndexPath(m_topNavFooterMenuRepeater, data, true /*isFooterRepeater*/) is { } ip)
                    {
                        return ip;
                    }
                }
            }
            else
            {
                {
                    if (SearchEntireTreeForIndexPath(m_leftNavRepeater, data, false /*isFooterRepeater*/) is { } ip)
                    {
                        return ip;
                    }
                }

                // If item was not located in primary list, search through footer
                {
                    if (SearchEntireTreeForIndexPath(m_leftNavFooterMenuRepeater, data, true /*isFooterRepeater*/) is { } ip)
                    {
                        return ip;
                    }
                }
            }

            return new IndexPath(new List<int>(0));
        }

        UIElement GetContainerForIndex(int index, bool inFooter)
        {
            if (IsTopNavigationView())
            {
                // Get the repeater that is presenting the first item
                var ir = inFooter ? m_topNavFooterMenuRepeater
                    : (m_topDataProvider.IsItemInPrimaryList(index) ? m_topNavRepeater : m_topNavRepeaterOverflowView);

                // Get the index of the item in the repeater
                var irIndex = inFooter ? index : m_topDataProvider.ConvertOriginalIndexToIndex(index);

                // Get the container of the first item
                if (ir.TryGetElement(irIndex) is { } container)
                {
                    return container;
                }
            }
            else
            {
                if ((inFooter ? m_leftNavFooterMenuRepeater.TryGetElement(index)
                    : m_leftNavRepeater.TryGetElement(index)) is { } container)
                {
                    return container as NavigationViewItemBase;
                }
            }
            return null;
        }

        NavigationViewItemBase GetContainerForIndexPath(IndexPath ip, bool lastVisible = false)
        {
            if (ip != null && ip.GetSize() > 0)
            {
                if (GetContainerForIndex(ip.GetAt(1), ip.GetAt(0) == c_footerMenuBlockIndex /*inFooter*/) is { } container)
                {
                    if (lastVisible)
                    {
                        if (container is NavigationViewItem nvi)
                        {
                            if (!nvi.IsExpanded)
                            {
                                return nvi;
                            }
                        }
                    }

                    // TODO: Fix below for top flyout scenario once the flyout is introduced in the XAML.
                    // We want to be able to retrieve containers for items that are in the flyout.
                    // This will return null if requesting children containers of
                    // items in the primary list, or unrealized items in the overflow popup.
                    // However this should not happen.
                    return GetContainerForIndexPath(container, ip, lastVisible);
                }
            }
            return null;
        }

        NavigationViewItemBase GetContainerForIndexPath(UIElement firstContainer, IndexPath ip, bool lastVisible)
        {
            var container = firstContainer;
            if (ip.GetSize() > 2)
            {
                for (int i = 2; i < ip.GetSize(); i++)
                {
                    bool succeededGettingNextContainer = false;
                    if (container is NavigationViewItem nvi)
                    {
                        if (lastVisible && nvi.IsExpanded == false)
                        {
                            return nvi;
                        }

                        if (nvi.GetRepeater() is { } nviRepeater)
                        {
                            if (nviRepeater.TryGetElement(ip.GetAt(i)) is { } nextContainer)
                            {
                                container = nextContainer;
                                succeededGettingNextContainer = true;
                            }
                        }
                    }
                    // If any of the above checks failed, it means something went wrong and we have an index for a non-existent repeater.
                    if (!succeededGettingNextContainer)
                    {
                        return null;
                    }
                }
            }
            return container as NavigationViewItemBase;
        }

        bool IsContainerTheSelectedItemInTheSelectionModel(NavigationViewItemBase nvib)
        {
            if (m_selectionModel.SelectedItem is { } selectedItem)
            {
                var selectedItemContainer = selectedItem as NavigationViewItemBase;
                if (selectedItemContainer == null)
                {
                    selectedItemContainer = GetContainerForIndexPath(m_selectionModel.SelectedIndex);
                }

                return selectedItemContainer == nvib;
            }
            return false;
        }

        internal ItemsRepeater LeftNavRepeater()
        {
            return m_leftNavRepeater;
        }

        internal NavigationViewItem GetSelectedContainer()
        {
            if (SelectedItem is { } selectedItem)
            {
                if (selectedItem is NavigationViewItem selectedItemContainer)
                {
                    return selectedItemContainer;
                }
                else
                {
                    return NavigationViewItemOrSettingsContentFromData(selectedItem);
                }
            }
            return null;
        }

        internal void Expand(NavigationViewItem item)
        {
            ChangeIsExpandedNavigationViewItem(item, true /*isExpanded*/);
        }

        internal void Collapse(NavigationViewItem item)
        {
            ChangeIsExpandedNavigationViewItem(item, false /*isExpanded*/);
        }

        bool DoesNavigationViewItemHaveChildren(NavigationViewItem nvi)
        {
            return nvi.MenuItems.Count > 0 || nvi.MenuItemsSource != null || nvi.HasUnrealizedChildren;
        }

        void ToggleIsExpandedNavigationViewItem(NavigationViewItem nvi)
        {
            ChangeIsExpandedNavigationViewItem(nvi, !nvi.IsExpanded);
        }

        void ChangeIsExpandedNavigationViewItem(NavigationViewItem nvi, bool isExpanded)
        {
            if (DoesNavigationViewItemHaveChildren(nvi))
            {
                nvi.IsExpanded = isExpanded;
            }
        }

        NavigationViewItem FindLowestLevelContainerToDisplaySelectionIndicator()
        {
            var indexIntoIndex = 1;
            var selectedIndex = m_selectionModel.SelectedIndex;
            if (selectedIndex != null && selectedIndex.GetSize() > 1)
            {
                if (GetContainerForIndex(selectedIndex.GetAt(indexIntoIndex), selectedIndex.GetAt(0) == c_footerMenuBlockIndex /* inFooter */) is { } container)
                {
                    if (container is NavigationViewItem nvi)
                    {
                        var nviImpl = nvi;
                        var isRepeaterVisible = nviImpl.IsRepeaterVisible();
                        while (nvi != null && isRepeaterVisible && !nvi.IsSelected && nvi.IsChildSelected)
                        {
                            indexIntoIndex++;
                            isRepeaterVisible = false;
                            if (nviImpl.GetRepeater() is { } repeater)
                            {
                                if (repeater.TryGetElement(selectedIndex.GetAt(indexIntoIndex)) is { } childContainer)
                                {
                                    nvi = childContainer as NavigationViewItem;
                                    nviImpl = nvi;
                                    isRepeaterVisible = nviImpl.IsRepeaterVisible();
                                }
                            }
                        }
                        return nvi;
                    }
                }
            }
            return null;
        }

        void ShowHideChildrenItemsRepeater(NavigationViewItem nvi)
        {
            var nviImpl = nvi;

            nviImpl.ShowHideChildren();

            if (nviImpl.ShouldRepeaterShowInFlyout())
            {
                if (nvi.IsExpanded)
                {
                    m_lastItemExpandedIntoFlyout = nvi;
                }
                else
                {
                    m_lastItemExpandedIntoFlyout = null;
                }
            }

            // If SelectedItem is being hidden/shown, animate SelectionIndicator
            if (!nvi.IsSelected && nvi.IsChildSelected)
            {
                if (!nviImpl.IsRepeaterVisible() && nvi.IsChildSelected)
                {
                    AnimateSelectionChanged(nvi);
                }
                else
                {
                    AnimateSelectionChanged(FindLowestLevelContainerToDisplaySelectionIndicator());
                }
            }

            nviImpl.RotateExpandCollapseChevron(nvi.IsExpanded);
        }

        object GetChildren(NavigationViewItem nvi)
        {
            if (nvi.MenuItems.Count > 0)
            {
                return nvi.MenuItems;
            }
            return nvi.MenuItemsSource;
        }

        ItemsRepeater GetChildRepeaterForIndexPath(IndexPath ip)
        {
            if (GetContainerForIndexPath(ip) is NavigationViewItem container)
            {
                return container.GetRepeater();
            }
            return null;
        }


        object GetChildrenForItemInIndexPath(IndexPath ip, bool forceRealize = false)
        {
            if (ip != null && ip.GetSize() > 1)
            {
                if (GetContainerForIndex(ip.GetAt(1), ip.GetAt(0) == c_footerMenuBlockIndex /*inFooter*/) is { } container)
                {
                    return GetChildrenForItemInIndexPath(container, ip, forceRealize);
                }
            }
            return null;
        }

        object GetChildrenForItemInIndexPath(UIElement firstContainer, IndexPath ip, bool forceRealize = false)
        {
            var container = firstContainer;
            bool shouldRecycleContainer = false;
            if (ip.GetSize() > 2)
            {
                for (int i = 2; i < ip.GetSize(); i++)
                {
                    bool succeededGettingNextContainer = false;
                    if (container is NavigationViewItem nvi)
                    {
                        var nextContainerIndex = ip.GetAt(i);
                        var nviRepeater = nvi.GetRepeater();
                        if (nviRepeater != null && DoesRepeaterHaveRealizedContainers(nviRepeater))
                        {
                            if (nviRepeater.TryGetElement(nextContainerIndex) is { } nextContainer)
                            {
                                container = nextContainer;
                                succeededGettingNextContainer = true;
                            }
                        }
                        else if (forceRealize)
                        {
                            if (GetChildren(nvi) is { } childrenData)
                            {
                                if (shouldRecycleContainer)
                                {
                                    RecycleContainer(nvi);
                                    shouldRecycleContainer = false;
                                }

                                // Get children data in an enumarable form
                                var newDataSource = childrenData as ItemsSourceView;
                                if (childrenData != null && newDataSource == null)
                                {
                                    newDataSource = new InspectingDataSource(childrenData);
                                }

                                if (newDataSource.GetAt(nextContainerIndex) is { } data)
                                {
                                    // Resolve databinding for item and search through that item's children
                                    if (ResolveContainerForItem(data, nextContainerIndex) is { } nvib)
                                    {
                                        if (nvib is NavigationViewItem nextContainer)
                                        {
                                            // Process x:bind
                                            //if (CachedVisualTreeHelpers.GetDataTemplateComponent(nextContainer) is { } extension)
                                            //{
                                            //    // Clear out old data. 
                                            //    extension.Recycle();
                                            //    int nextPhase = VirtualizationInfo.PhaseReachedEnd;
                                            //    // Run Phase 0
                                            //    extension.ProcessBindings(data, nextContainerIndex, 0 /* currentPhase */, nextPhase);

                                            //    // TODO: If nextPhase is not -1, ProcessBinding for all the phases
                                            //}

                                            container = nextContainer;
                                            shouldRecycleContainer = true;
                                            succeededGettingNextContainer = true;
                                        }
                                    }
                                }
                            }
                        }

                    }
                    // If any of the above checks failed, it means something went wrong and we have an index for a non-existent repeater.
                    if (!succeededGettingNextContainer)
                    {
                        return null;
                    }
                }
            }

            {
                if (container is NavigationViewItem nvi)
                {
                    var children = GetChildren(nvi);
                    if (shouldRecycleContainer)
                    {
                        RecycleContainer(nvi);
                    }
                    return children;
                }
            }

            return null;
        }

        void CollapseTopLevelMenuItems(NavigationViewPaneDisplayMode oldDisplayMode)
        {
            // We want to make sure only top level items are visible when switching pane modes
            if (oldDisplayMode == NavigationViewPaneDisplayMode.Top)
            {
                CollapseMenuItemsInRepeater(m_topNavRepeater);
                CollapseMenuItemsInRepeater(m_topNavRepeaterOverflowView);
            }
            else
            {
                CollapseMenuItemsInRepeater(m_leftNavRepeater);
            }
        }

        void CollapseMenuItemsInRepeater(ItemsRepeater ir)
        {
            for (int index = 0; index < GetContainerCountInRepeater(ir); index++)
            {
                if (ir.TryGetElement(index) is { } element)
                {
                    if (element is NavigationViewItem nvi)
                    {
                        ChangeIsExpandedNavigationViewItem(nvi, false /*isExpanded*/);
                    }
                }
            }
        }

        void RaiseExpandingEvent(NavigationViewItemBase container)
        {
            var eventArgs = new NavigationViewItemExpandingEventArgs(this);
            eventArgs.ExpandingItemContainer = container;
            Expanding?.Invoke(this, eventArgs);
        }

        void RaiseCollapsedEvent(NavigationViewItemBase container)
        {
            var eventArgs = new NavigationViewItemCollapsedEventArgs(this);
            eventArgs.CollapsedItemContainer = container;
            Collapsed?.Invoke(this, eventArgs);
        }

        bool IsTopLevelItem(NavigationViewItemBase nvib)
        {
            return IsRootItemsRepeater(GetParentItemsRepeaterForContainer(nvib));
        }

        DependencyObject IControlProtected.GetTemplateChild(string childName)
        {
            return GetTemplateChild(childName);
        }

#if NET462_OR_NEWER
        protected override void OnDpiChanged(DpiScale oldDpi, DpiScale newDpi)
        {
            base.OnDpiChanged(oldDpi, newDpi);

            m_bitmapCache.RenderAtScale = newDpi.PixelsPerDip;
        }
#endif

        bool m_InitialNonForcedModeUpdate = true;

        NavigationViewItemsFactory m_navigationViewItemsFactory;

        // Visual components
        Button m_paneToggleButton;
        SplitView m_rootSplitView;
        NavigationViewItem m_settingsItem;
        RowDefinition m_itemsContainerRow;
        FrameworkElement m_menuItemsScrollViewer;
        FrameworkElement m_footerItemsScrollViewer;
        UIElement m_paneContentGrid;
        ColumnDefinition m_paneToggleButtonIconGridColumn;
        FrameworkElement m_paneTitleHolderFrameworkElement;
        FrameworkElement m_paneTitleFrameworkElement;
        FrameworkElement m_visualItemsSeparator;
        Button m_paneSearchButton;
        Button m_backButton;
        Button m_closeButton;
        ItemsRepeater m_leftNavRepeater;
        ItemsRepeater m_topNavRepeater;
        ItemsRepeater m_leftNavFooterMenuRepeater;
        ItemsRepeater m_topNavFooterMenuRepeater;
        Button m_topNavOverflowButton;
        ItemsRepeater m_topNavRepeaterOverflowView;
        Grid m_topNavGrid;
        Border m_topNavContentOverlayAreaGrid;

        // Indicator animations
        UIElement m_prevIndicator;
        UIElement m_nextIndicator;
        UIElement m_activeIndicator;
        object m_lastSelectedItemPendingAnimationInTopNav;

        FrameworkElement m_togglePaneTopPadding;
        FrameworkElement m_contentPaneTopPadding;
        FrameworkElement m_contentLeftPadding;

        CoreApplicationViewTitleBar m_coreTitleBar;

        ContentControl m_leftNavPaneAutoSuggestBoxPresenter;
        ContentControl m_topNavPaneAutoSuggestBoxPresenter;

        ContentControl m_leftNavPaneHeaderContentBorder;
        ContentControl m_leftNavPaneCustomContentBorder;
        ContentControl m_leftNavFooterContentBorder;

        ContentControl m_paneHeaderOnTopPane;
        ContentControl m_paneTitleOnTopPane;
        ContentControl m_paneCustomContentOnTopPane;
        ContentControl m_paneFooterOnTopPane;
        ContentControl m_paneTitlePresenter;

        ColumnDefinition m_paneHeaderCloseButtonColumn;
        ColumnDefinition m_paneHeaderToggleButtonColumn;
        RowDefinition m_paneHeaderContentBorderRow;

        NavigationViewItem m_lastItemExpandedIntoFlyout;

        // Event Tokens
        bool m_layoutUpdatedToken;
        FrameworkElementSizeChangedRevoker m_itemsContainerSizeChangedRevoker;

        ItemsSourceView.CollectionChangedRevoker m_menuItemsCollectionChangedRevoker;
        ItemsSourceView.CollectionChangedRevoker m_footerItemsCollectionChangedRevoker;

        ItemsSourceView.CollectionChangedRevoker m_topNavOverflowItemsCollectionChangedRevoker;

        bool m_wasForceClosed = false;
        bool m_isClosedCompact = false;
        bool m_blockNextClosingEvent = false;
        bool m_initialListSizeStateSet = false;

        TopNavigationViewDataProvider m_topDataProvider = new TopNavigationViewDataProvider();

        SelectionModel m_selectionModel = new SelectionModel();
        List<object> m_selectionModelSource;

        ItemsSourceView m_menuItemsSource = null;
        ItemsSourceView m_footerItemsSource = null;

        bool m_appliedTemplate = false;

        // flag is used to stop recursive call. eg:
        // Customer select an item from SelectedItem property->ChangeSelection update ListView->LIstView raise OnSelectChange(we want stop here)->change property do do animation again.
        // Customer clicked listview->listview raised OnSelectChange->SelectedItem property changed->ChangeSelection->Undo the selection by SelectedItem(prevItem) (we want it stop here)->ChangeSelection again ->...
        bool m_shouldIgnoreNextSelectionChange = false;
        // Used to disable raising selection change iff settings item gets restored because of displaymode change
        bool m_shouldIgnoreNextSelectionChangeBecauseSettingsRestore = false;
        // A flag to track that the selectionchange is caused by selection a item in topnav overflow menu
        bool m_selectionChangeFromOverflowMenu = false;
        // Flag indicating whether selection change should raise item invoked. This is needed to be able to raise ItemInvoked before SelectionChanged while SelectedItem should point to the clicked item
        bool m_shouldRaiseItemInvokedAfterSelection = false;

        TopNavigationViewLayoutState m_topNavigationMode = TopNavigationViewLayoutState.Uninitialized;

        // A threshold to stop recovery from overflow to normal happens immediately on resize.
        float m_topNavigationRecoveryGracePeriodWidth = 5f;

        // There are three ways to change IsPaneOpen:
        // 1, customer call IsPaneOpen=true/false directly or nav.IsPaneOpen is binding with a variable and the value is changed.
        // 2, customer click ToggleButton or splitView.IsPaneOpen->nav.IsPaneOpen changed because of window resize
        // 3, customer changed PaneDisplayMode.
        // 2 and 3 are internal implementation and will call by ClosePane/OpenPane. the flag is to indicate 1 if it's false
        bool m_isOpenPaneForInteraction = false;

        bool m_moveTopNavOverflowItemOnFlyoutClose = false;

        bool m_shouldIgnoreUIASelectionRaiseAsExpandCollapseWillRaise = false;

        bool m_OrientationChangedPendingAnimation = false;

        bool m_TabKeyPrecedesFocusChange = false;

        GettingFocusHelper m_leftNavRepeaterGettingFocusHelper;
        GettingFocusHelper m_topNavRepeaterGettingFocusHelper;
        GettingFocusHelper m_leftNavFooterMenuRepeaterGettingFocusHelper;
        GettingFocusHelper m_topNavFooterMenuRepeaterGettingFocusHelper;

        readonly BitmapCache m_bitmapCache;

        static readonly PropertyPath s_opacityPath = new PropertyPath(OpacityProperty);
        static readonly PropertyPath s_centerXPath = new PropertyPath("(UIElement.RenderTransform).(TransformGroup.Children)[0].(ScaleTransform.CenterX)");
        static readonly PropertyPath s_centerYPath = new PropertyPath("(UIElement.RenderTransform).(TransformGroup.Children)[0].(ScaleTransform.CenterY)");
        static readonly PropertyPath s_scaleXPath = new PropertyPath("(UIElement.RenderTransform).(TransformGroup.Children)[0].(ScaleTransform.ScaleX)");
        static readonly PropertyPath s_scaleYPath = new PropertyPath("(UIElement.RenderTransform).(TransformGroup.Children)[0].(ScaleTransform.ScaleY)");
        static readonly PropertyPath s_translateXPath = new PropertyPath("(UIElement.RenderTransform).(TransformGroup.Children)[1].(TranslateTransform.X)");
        static readonly PropertyPath s_translateYPath = new PropertyPath("(UIElement.RenderTransform).(TransformGroup.Children)[1].(TranslateTransform.Y)");
    }
}
