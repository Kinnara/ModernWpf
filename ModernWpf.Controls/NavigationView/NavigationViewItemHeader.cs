// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Windows;
using System.Windows.Controls;

namespace ModernWpf.Controls
{
    public class NavigationViewItemHeader : NavigationViewItemBase
    {
        const string c_rootGrid = "NavigationViewItemHeaderRootGrid";

        static NavigationViewItemHeader()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(NavigationViewItemHeader),
                new FrameworkPropertyMetadata(typeof(NavigationViewItemHeader)));
        }

        public NavigationViewItemHeader()
        {
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            if (GetSplitView() is { } splitView)
            {
                splitView.IsPaneOpenChanged += OnSplitViewPropertyChanged;
                splitView.DisplayModeChanged += OnSplitViewPropertyChanged;

                UpdateIsClosedCompact();
            }

            if (GetTemplateChild(c_rootGrid) is Grid rootGrid)
            {
                m_rootGrid = rootGrid;
            }

            UpdateVisualState(false /*useTransitions*/);
            UpdateItemIndentation();

            // TODO: WPF - Header Animation
            /*
            var visual = ElementCompositionPreview.GetElementVisual(*this);
            NavigationView.CreateAndAttachHeaderAnimation(visual);
            */
        }

        void OnSplitViewPropertyChanged(DependencyObject sender, DependencyProperty args)
        {
            if (args == SplitView.IsPaneOpenProperty ||
                args == SplitView.DisplayModeProperty)
            {
                UpdateIsClosedCompact();
            }
        }

        void UpdateIsClosedCompact()
        {
            if (GetSplitView() is { } splitView)
            {
                // Check if the pane is closed and if the splitview is in either compact mode.
                m_isClosedCompact = !splitView.IsPaneOpen && (splitView.DisplayMode == SplitViewDisplayMode.CompactOverlay || splitView.DisplayMode == SplitViewDisplayMode.CompactInline);
                UpdateVisualState(true /*useTransitions*/);
            }
        }

        void UpdateVisualState(bool useTransitions)
        {
            VisualStateManager.GoToState(this, m_isClosedCompact && IsTopLevelItem ? "HeaderTextCollapsed" : "HeaderTextVisible", useTransitions);
        }

        private protected override void OnNavigationViewItemBaseDepthChanged()
        {
            UpdateItemIndentation();
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

        bool m_isClosedCompact = false;

        Grid m_rootGrid;
    }
}
