// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using ModernWpf.Input;
using static CppWinRTHelpers;
using static ModernWpf.Controls.NavigationViewItemHelper;

namespace ModernWpf.Controls.Primitives
{
    public partial class NavigationViewItemPresenter : ContentControl, IControlProtected
    {
        const string c_contentGrid = "PresenterContentRootGrid";
        const string c_expandCollapseChevron = "ExpandCollapseChevron";
        const string c_expandCollapseRotateExpandedStoryboard = "ExpandCollapseRotateExpandedStoryboard";
        const string c_expandCollapseRotateCollapsedStoryboard = "ExpandCollapseRotateCollapsedStoryboard";
        const string c_expandCollapseRotateTransform = "ExpandCollapseChevronRotateTransform";

        const string c_iconBoxColumnDefinitionName = "IconColumn";

        static NavigationViewItemPresenter()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(NavigationViewItemPresenter),
                new FrameworkPropertyMetadata(typeof(NavigationViewItemPresenter)));

            HorizontalContentAlignmentProperty.OverrideMetadata(
                typeof(NavigationViewItemPresenter),
                new FrameworkPropertyMetadata(HorizontalAlignment.Center));

            VerticalContentAlignmentProperty.OverrideMetadata(
                typeof(NavigationViewItemPresenter),
                new FrameworkPropertyMetadata(VerticalAlignment.Center));
        }

        public NavigationViewItemPresenter()
        {
            TemplateSettings = new NavigationViewItemPresenterTemplateSettings();
            InputHelper.SetIsTapEnabled(this, true);
        }

        public override void OnApplyTemplate()
        {
            IControlProtected controlProtected = this;

            if (m_expandCollapseChevron != null)
            {
                UnhookExpandCollapseChevronEvents(m_expandCollapseChevron, m_expandCollapseChevronNavigationViewItem);
                m_expandCollapseChevron = null;
                m_expandCollapseChevronNavigationViewItem = null;
            }

            // Retrieve pointers to stable controls 
            m_helper.Init(this);

            if (GetTemplateChildT<Grid>(c_contentGrid, this) is { } contentGrid)
            {
                m_contentGrid = contentGrid;
            }

            if (GetNavigationViewItem() is { } navigationViewItem)
            {
                if (GetTemplateChildT<Grid>(c_expandCollapseChevron, this) is { } expandCollapseChevron)
                {
                    m_expandCollapseChevron = expandCollapseChevron;
                    m_expandCollapseChevronNavigationViewItem = navigationViewItem;
                    HookExpandCollapseChevronEvents(expandCollapseChevron, navigationViewItem);
                }
                navigationViewItem.UpdateVisualStateNoTransition();
                navigationViewItem.UpdateIsClosedCompact();

                // We probably switched displaymode, so restore width now, otherwise the next time we will restore is when the CompactPaneLength changes
                if (navigationViewItem.GetNavigationView() is { } navigationView)
                {
                    if (navigationView.PaneDisplayMode != NavigationViewPaneDisplayMode.Top)
                    {
                        UpdateCompactPaneLength(m_compactPaneLengthValue, true);
                    }
                }
            }

            //m_chevronExpandedStoryboard = GetTemplateChildT<Storyboard>(c_expandCollapseRotateExpandedStoryboard, this);
            //m_chevronCollapsedStoryboard = GetTemplateChildT<Storyboard>(c_expandCollapseRotateCollapsedStoryboard, this);
            if (this.GetTemplateRoot() is FrameworkElement templateRoot)
            {
                m_chevronExpandedStoryboard = templateRoot.Resources[c_expandCollapseRotateExpandedStoryboard] as Storyboard;
                m_chevronCollapsedStoryboard = templateRoot.Resources[c_expandCollapseRotateCollapsedStoryboard] as Storyboard;
            }

            m_expandCollapseRotateTransform = GetTemplateChildT<RotateTransform>(c_expandCollapseRotateTransform, this);

            UpdateMargin();
        }

        internal void RotateExpandCollapseChevron(bool isExpanded)
        {
            if (isExpanded)
            {
                if (m_chevronExpandedStoryboard is { } openStoryboard)
                {
                    openStoryboard.Begin();
                }

                if (m_expandCollapseRotateTransform != null)
                {
                    m_expandCollapseRotateTransform.Angle = 180;
                }
            }
            else
            {
                if (m_chevronCollapsedStoryboard is { } closedStoryboard)
                {
                    closedStoryboard.Begin();
                }

                if (m_expandCollapseRotateTransform != null)
                {
                    m_expandCollapseRotateTransform.Angle = 0;
                }
            }
        }

        void OnExpandCollapseChevronMouseLeftButtonDown(object sender, MouseButtonEventArgs args)
        {
            if (sender is UIElement chevron)
            {
                m_isExpandCollapseChevronPressed = true;
                m_isExpandCollapseChevronMouseCaptured = chevron.CaptureMouse();
            }

            args.Handled = true;
        }

        void OnExpandCollapseChevronMouseLeftButtonUp(object sender, MouseButtonEventArgs args)
        {
            var wasPressed = m_isExpandCollapseChevronPressed;
            ReleaseExpandCollapseChevronMouseCapture(sender as UIElement);

            if (wasPressed && sender is UIElement chevron)
            {
                InputHelper.RaiseTapped(chevron, args.Timestamp);
            }

            args.Handled = true;
        }

        void OnExpandCollapseChevronLostMouseCapture(object sender, MouseEventArgs args)
        {
            m_isExpandCollapseChevronPressed = false;
            m_isExpandCollapseChevronMouseCaptured = false;
        }

        void HookExpandCollapseChevronEvents(UIElement chevron, NavigationViewItem navigationViewItem)
        {
            chevron.AddHandler(MouseLeftButtonDownEvent, new MouseButtonEventHandler(OnExpandCollapseChevronMouseLeftButtonDown), true /*handledEventsToo*/);
            chevron.AddHandler(MouseLeftButtonUpEvent, new MouseButtonEventHandler(OnExpandCollapseChevronMouseLeftButtonUp), true /*handledEventsToo*/);
            chevron.AddHandler(LostMouseCaptureEvent, new MouseEventHandler(OnExpandCollapseChevronLostMouseCapture), true /*handledEventsToo*/);
            InputHelper.AddTappedHandler(chevron, navigationViewItem.OnExpandCollapseChevronTapped);
        }

        void UnhookExpandCollapseChevronEvents(UIElement chevron, NavigationViewItem navigationViewItem)
        {
            chevron.RemoveHandler(MouseLeftButtonDownEvent, new MouseButtonEventHandler(OnExpandCollapseChevronMouseLeftButtonDown));
            chevron.RemoveHandler(MouseLeftButtonUpEvent, new MouseButtonEventHandler(OnExpandCollapseChevronMouseLeftButtonUp));
            chevron.RemoveHandler(LostMouseCaptureEvent, new MouseEventHandler(OnExpandCollapseChevronLostMouseCapture));
            if (navigationViewItem != null)
            {
                InputHelper.RemoveTappedHandler(chevron, navigationViewItem.OnExpandCollapseChevronTapped);
            }
        }

        void ReleaseExpandCollapseChevronMouseCapture(UIElement chevron)
        {
            m_isExpandCollapseChevronPressed = false;

            if (m_isExpandCollapseChevronMouseCaptured && chevron != null)
            {
                chevron.ReleaseMouseCapture();
            }

            m_isExpandCollapseChevronMouseCaptured = false;
        }

        internal UIElement GetSelectionIndicator()
        {
            return m_helper.GetSelectionIndicator();
        }

        // TODO: WPF - GoToElementStateCore
        /*
        bool GoToElementStateCore(string state, bool useTransitions)
        {
            // GoToElementStateCore: Update visualstate for itself.
            // VisualStateManager.GoToState: update visualstate for it's first child.

            // If NavigationViewItemPresenter is used, two sets of VisualStateGroups are supported. One set is help to switch the style and it's NavigationViewItemPresenter itself and defined in NavigationViewItem
            // Another set is defined in style for NavigationViewItemPresenter.
            // OnLeftNavigation, OnTopNavigationPrimary, OnTopNavigationOverflow only apply to itself.
            if (state == c_OnLeftNavigation || state == c_OnLeftNavigationReveal || state == c_OnTopNavigationPrimary
                || state == c_OnTopNavigationPrimaryReveal || state == c_OnTopNavigationOverflow)
            {
                return base.GoToElementStateCore(state, useTransitions);
            }
            return VisualStateManager.GoToState(this, state, useTransitions);
        }
        */

        NavigationViewItem GetNavigationViewItem()
        {
            NavigationViewItem navigationViewItem = null;

            // winrt::DependencyObject obj = operator winrt::DependencyObject();
            DependencyObject obj = this;

            if (SharedHelpers.GetAncestorOfType<NavigationViewItem>(VisualTreeHelper.GetParent(obj)) is { } item)
            {
                navigationViewItem = item;
            }
            return navigationViewItem;
        }

        internal void UpdateContentLeftIndentation(double leftIndentation)
        {
            m_leftIndentation = leftIndentation;
            UpdateMargin();
        }

        void UpdateMargin()
        {
            if (m_contentGrid is { } grid)
            {
                var oldGridMargin = grid.Margin;
                grid.Margin = new Thickness(m_leftIndentation, oldGridMargin.Top, oldGridMargin.Right, oldGridMargin.Bottom);
            }
        }

        internal void UpdateCompactPaneLength(double compactPaneLength, bool shouldUpdate)
        {
            m_compactPaneLengthValue = compactPaneLength;

            if (shouldUpdate)
            {
                var templateSettings = TemplateSettings;
                var gridLength = compactPaneLength;

                templateSettings.IconWidth = gridLength;
                templateSettings.SmallerIconWidth = gridLength - 8;
            }
        }

        internal void UpdateClosedCompactVisualState(bool isTopLevelItem, bool isClosedCompact)
        {
            // We increased the ContentPresenter margin to align it visually with the expand/collapse chevron. This updated margin is even applied when the
            // NavigationView is in a visual state where no expand/collapse chevrons are shown, leading to more content being cut off than necessary.
            // This is the case for top-level items when the NavigationView is in a compact mode and the NavigationView pane is closed. To keep the original
            // cutoff visual experience intact, we restore  the original ContentPresenter margin for such top-level items only (children shown in a flyout
            // will use the updated margin).
            var stateName = isClosedCompact && isTopLevelItem
                ? "ClosedCompactAndTopLevelItem"
                : "NotClosedCompactAndTopLevelItem";

            VisualStateManager.GoToState(this, stateName, false /*useTransitions*/);
        }

        DependencyObject IControlProtected.GetTemplateChild(string childName)
        {
            return GetTemplateChild(childName);
        }

        double m_compactPaneLengthValue = 40;

        NavigationViewItemHelper<NavigationViewItemPresenter> m_helper = new NavigationViewItemHelper<NavigationViewItemPresenter>();
        Grid m_contentGrid;
        Grid m_expandCollapseChevron;

        double m_leftIndentation = 0;

        Storyboard m_chevronExpandedStoryboard;
        Storyboard m_chevronCollapsedStoryboard;

        RotateTransform m_expandCollapseRotateTransform;
        NavigationViewItem m_expandCollapseChevronNavigationViewItem;
        bool m_isExpandCollapseChevronPressed;
        bool m_isExpandCollapseChevronMouseCaptured;
    }
}
