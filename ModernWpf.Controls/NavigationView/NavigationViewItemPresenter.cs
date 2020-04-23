// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using ModernWpf.Input;
using static CppWinRTHelpers;
using static ModernWpf.Controls.NavigationViewItemHelper;

namespace ModernWpf.Controls.Primitives
{
    public class NavigationViewItemPresenter : ContentControl, IControlProtected
    {
        const string c_contentGrid = "PresenterContentRootGrid";
        const string c_expandCollapseChevron = "ExpandCollapseChevron";
        const string c_expandCollapseRotateExpandedStoryboard = "ExpandCollapseRotateExpandedStoryboard";
        const string c_expandCollapseRotateCollapsedStoryboard = "ExpandCollapseRotateCollapsedStoryboard";

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
            InputHelper.SetIsTapEnabled(this, true);
        }

        #region Icon

        public static readonly DependencyProperty IconProperty =
            DependencyProperty.Register(
                nameof(Icon),
                typeof(IconElement),
                typeof(NavigationViewItemPresenter),
                null);

        public IconElement Icon
        {
            get => (IconElement)GetValue(IconProperty);
            set => SetValue(IconProperty, value);
        }

        #endregion

        #region CornerRadius

        public static readonly DependencyProperty CornerRadiusProperty =
            ControlHelper.CornerRadiusProperty.AddOwner(typeof(NavigationViewItemPresenter));

        public CornerRadius CornerRadius
        {
            get => (CornerRadius)GetValue(CornerRadiusProperty);
            set => SetValue(CornerRadiusProperty, value);
        }

        #endregion

        public override void OnApplyTemplate()
        {
            IControlProtected controlProtected = this;

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
                    InputHelper.SetIsTapEnabled(expandCollapseChevron, true);
                    InputHelper.AddTappedHandler(expandCollapseChevron, navigationViewItem.OnExpandCollapseChevronTapped);
                }

                navigationViewItem.UpdateVisualStateNoTransition();


                // We probably switched displaymode, so restore width now, otherwise the next time we will restore is when the CompactPaneLength changes
                // TODO: WPF - Refactor null check
                if (navigationViewItem.GetNavigationView()?.PaneDisplayMode != NavigationViewPaneDisplayMode.Top)
                {
                    UpdateCompactPaneLength(m_compactPaneLengthValue, true);
                }
            }

            m_chevronExpandedStoryboard = GetTemplateChildT<Storyboard>(c_expandCollapseRotateExpandedStoryboard, this);
            m_chevronCollapsedStoryboard = GetTemplateChildT<Storyboard>(c_expandCollapseRotateCollapsedStoryboard, this);

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
            }
            else
            {
                if (m_chevronCollapsedStoryboard is { } closedStoryboard)
                {
                    closedStoryboard.Begin();
                }
            }
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
                if (GetTemplateChildT<ColumnDefinition>(c_iconBoxColumnDefinitionName, this) is { } iconGridColumn)
                {
                    iconGridColumn.Width = new GridLength(compactPaneLength);
                }
            }
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
    }
}
