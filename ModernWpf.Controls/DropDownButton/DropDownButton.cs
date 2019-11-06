// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using ModernWpf.Automation.Peers;
using ModernWpf.Controls.Primitives;

namespace ModernWpf.Controls
{
    public class DropDownButton : Button
    {
        static DropDownButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(DropDownButton), new FrameworkPropertyMetadata(typeof(DropDownButton)));
        }

        public DropDownButton()
        {
        }

        #region CornerRadius

        public static readonly DependencyProperty CornerRadiusProperty =
            ControlHelper.CornerRadiusProperty.AddOwner(typeof(DropDownButton));

        public CornerRadius CornerRadius
        {
            get => (CornerRadius)GetValue(CornerRadiusProperty);
            set => SetValue(CornerRadiusProperty, value);
        }

        #endregion

        #region FocusVisualMargin

        public static readonly DependencyProperty FocusVisualMarginProperty =
            FocusVisualHelper.FocusVisualMarginProperty.AddOwner(typeof(DropDownButton));

        public Thickness FocusVisualMargin
        {
            get => (Thickness)GetValue(FocusVisualMarginProperty);
            set => SetValue(FocusVisualMarginProperty, value);
        }

        #endregion

        #region DropDownMenu

        public static readonly DependencyProperty DropDownMenuProperty =
            DependencyProperty.Register(
                nameof(DropDownMenu),
                typeof(ContextMenu),
                typeof(DropDownButton),
                new PropertyMetadata(OnDropDownMenuChanged));

        public ContextMenu DropDownMenu
        {
            get => (ContextMenu)GetValue(DropDownMenuProperty);
            set => SetValue(DropDownMenuProperty, value);
        }

        private static void OnDropDownMenuChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((DropDownButton)d).OnDropDownMenuChanged((ContextMenu)e.OldValue, (ContextMenu)e.NewValue);
        }

        private void OnDropDownMenuChanged(ContextMenu oldDropDownMenu, ContextMenu newDropDownMenu)
        {
            if (oldDropDownMenu != null)
            {
                oldDropDownMenu.Opened -= OnDropDownMenuOpened;
                oldDropDownMenu.Closed -= OnDropDownMenuClosed;
            }

            if (newDropDownMenu != null)
            {
                newDropDownMenu.Opened += OnDropDownMenuOpened;
                newDropDownMenu.Closed += OnDropDownMenuClosed;
            }
        }

        #endregion

        protected override void OnClick()
        {
            base.OnClick();
            OpenDropDownMenu();
        }

        protected override void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            if (m_isDropDownOpen)
            {
                e.Handled = true;
            }

            base.OnPreviewMouseLeftButtonDown(e);
        }

        internal bool IsDropDownOpen => m_isDropDownOpen;

        internal void OpenDropDownMenu()
        {
            var dropDownMenu = DropDownMenu;
            if (dropDownMenu != null)
            {
                dropDownMenu.Placement = PlacementMode.Bottom;
                dropDownMenu.PlacementTarget = this;
                dropDownMenu.PlacementRectangle = new Rect(new Point(0, -4), new Point(RenderSize.Width, RenderSize.Height + 4));
                dropDownMenu.IsOpen = true;
            }
        }

        internal void CloseDropDownMenu()
        {
            var dropDownMenu = DropDownMenu;
            if (dropDownMenu != null)
            {
                dropDownMenu.IsOpen = false;
            }
        }

        private void OnDropDownMenuOpened(object sender, RoutedEventArgs e)
        {
            m_isDropDownOpen = true;
            SharedHelpers.RaiseAutomationPropertyChangedEvent(this, ExpandCollapseState.Collapsed, ExpandCollapseState.Expanded);
        }

        private void OnDropDownMenuClosed(object sender, RoutedEventArgs e)
        {
            m_isDropDownOpen = false;
            SharedHelpers.RaiseAutomationPropertyChangedEvent(this, ExpandCollapseState.Expanded, ExpandCollapseState.Collapsed);
        }

        protected override AutomationPeer OnCreateAutomationPeer()
        {
            return new DropDownButtonAutomationPeer(this);
        }

        private bool m_isDropDownOpen;
    }
}
