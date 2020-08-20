// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Windows;
using System.Windows.Controls;
using static CppWinRTHelpers;

namespace ModernWpf.Controls
{
    public class NavigationViewItemSeparator : NavigationViewItemBase
    {
        const string c_rootGrid = "NavigationViewItemSeparatorRootGrid";

        static NavigationViewItemSeparator()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(NavigationViewItemSeparator),
                new FrameworkPropertyMetadata(typeof(NavigationViewItemSeparator)));
        }

        public NavigationViewItemSeparator()
        {
        }

        void UpdateVisualState(bool useTransitions)
        {
            if (m_appliedTemplate)
            {
                const string groupName = "NavigationSeparatorLineStates";
                var stateName = (Position != NavigationViewRepeaterPosition.TopPrimary && Position != NavigationViewRepeaterPosition.TopFooter)
                    ? m_isClosedCompact
                        ? "HorizontalLineCompact"
                        : "HorizontalLine"
                    : "VerticalLine";

                VisualStateUtil.GoToStateIfGroupExists(this, groupName, stateName, false /*useTransitions*/);
            }
        }

        public override void OnApplyTemplate()
        {
            // Stop UpdateVisualState before template is applied. Otherwise the visual may not the same as we expect
            m_appliedTemplate = false;
            base.OnApplyTemplate();

            if (GetTemplateChildT<Grid>(c_rootGrid, this) is { } rootGrid)
            {
                m_rootGrid = rootGrid;
            }

            if (GetSplitView() is { } splitView)
            {
                splitView.IsPaneOpenChanged += OnSplitViewPropertyChanged;
                splitView.DisplayModeChanged += OnSplitViewPropertyChanged;

                UpdateIsClosedCompact(false);
            }

            m_appliedTemplate = true;
            UpdateVisualState(false /*useTransition*/);
            UpdateItemIndentation();
        }

        private protected override void OnNavigationViewItemBaseDepthChanged()
        {
            UpdateVisualState(false /*useTransition*/);
        }

        private protected override void OnNavigationViewItemBasePositionChanged()
        {
            UpdateVisualState(false /*useTransition*/);
        }

        void OnSplitViewPropertyChanged(DependencyObject sender, DependencyProperty args)
        {
            UpdateIsClosedCompact(true);
        }

        void UpdateItemIndentation()
        {
            // Update item indentation based on its depth
            if (m_rootGrid is { } rootGrid)
            {
                var oldMargin = rootGrid.Margin;
                var newLeftMargin = Depth * c_itemIndentation;
                rootGrid.Margin = new Thickness(newLeftMargin, oldMargin.Top, oldMargin.Right, oldMargin.Bottom);
            }
        }

        void UpdateIsClosedCompact(bool updateVisualState)
        {
            if (GetSplitView() is { } splitView)
            {
                // Check if the pane is closed and if the splitview is in either compact mode
                m_isClosedCompact = !splitView.IsPaneOpen
                    && (splitView.DisplayMode == SplitViewDisplayMode.CompactOverlay || splitView.DisplayMode == SplitViewDisplayMode.CompactInline);

                if (updateVisualState)
                {
                    UpdateVisualState(false /*useTransition*/);
                }
            }
        }

        bool m_appliedTemplate = false;
        bool m_isClosedCompact = false;

        Grid m_rootGrid;
    }
}
