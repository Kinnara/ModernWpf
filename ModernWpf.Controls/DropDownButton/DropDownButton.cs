// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using ModernWpf.Automation.Peers;
using ModernWpf.Controls.Primitives;
using ModernWpf.Media.Animation;

namespace ModernWpf.Controls
{
    public partial class DropDownButton : Button
    {
        static DropDownButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(DropDownButton), new FrameworkPropertyMetadata(typeof(DropDownButton)));
        }

        public DropDownButton()
        {
        }

        #region Flyout

        private static void OnFlyoutPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((DropDownButton)d).OnFlyoutPropertyChanged();
        }

        private void OnFlyoutPropertyChanged()
        {
            RegisterFlyoutEvents();
        }

        #endregion

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            RegisterFlyoutEvents();
        }

        internal bool IsFlyoutOpen => m_isFlyoutOpen;

        internal void OpenFlyout()
        {
            Flyout?.ShowAt(this);
        }

        internal void CloseFlyout()
        {
            Flyout?.Hide();
        }

        private void RegisterFlyoutEvents()
        {
            if (m_registeredFlyout != null)
            {
                m_registeredFlyout.Opened -= OnFlyoutOpened;
                m_registeredFlyout.Closed -= OnFlyoutClosed;
                m_registeredFlyout = null;
            }

            var flyout = Flyout;
            if (flyout != null)
            {
                flyout.Opened += OnFlyoutOpened;
                flyout.Closed += OnFlyoutClosed;
                m_registeredFlyout = flyout;
            }
        }

        private void OnFlyoutOpened(object sender, object e)
        {
            m_isFlyoutOpen = true;
            SharedHelpers.RaiseAutomationPropertyChangedEvent(this, ExpandCollapseState.Collapsed, ExpandCollapseState.Expanded);
        }

        private void OnFlyoutClosed(object sender, object e)
        {
            m_isFlyoutOpen = false;
            SharedHelpers.RaiseAutomationPropertyChangedEvent(this, ExpandCollapseState.Expanded, ExpandCollapseState.Collapsed);
        }

        protected override AutomationPeer OnCreateAutomationPeer()
        {
            return new DropDownButtonAutomationPeer(this);
        }

        private bool m_isFlyoutOpen;
        private FlyoutBase m_registeredFlyout;
    }
}
