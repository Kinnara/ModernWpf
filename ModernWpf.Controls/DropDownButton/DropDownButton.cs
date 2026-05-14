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
    public class DropDownButton : Button
    {
        static DropDownButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(DropDownButton), new FrameworkPropertyMetadata(typeof(DropDownButton)));
        }

        public DropDownButton()
        {
        }

        #region BackgroundSizing

        public static readonly DependencyProperty BackgroundSizingProperty =
            ControlHelper.BackgroundSizingProperty.AddOwner(typeof(DropDownButton));

        public BackgroundSizing BackgroundSizing
        {
            get => (BackgroundSizing)GetValue(BackgroundSizingProperty);
            set => SetValue(BackgroundSizingProperty, value);
        }

        #endregion

        #region CharacterSpacing

        public static readonly DependencyProperty CharacterSpacingProperty =
            ControlHelper.CharacterSpacingProperty.AddOwner(typeof(DropDownButton));

        public int CharacterSpacing
        {
            get => (int)GetValue(CharacterSpacingProperty);
            set => SetValue(CharacterSpacingProperty, value);
        }

        #endregion

        #region ContentTransitions

        public static readonly DependencyProperty ContentTransitionsProperty =
            ControlHelper.ContentTransitionsProperty.AddOwner(typeof(DropDownButton));

        public TransitionCollection ContentTransitions
        {
            get => (TransitionCollection)GetValue(ContentTransitionsProperty);
            set => SetValue(ContentTransitionsProperty, value);
        }

        #endregion

        #region CornerRadius

        public static readonly DependencyProperty CornerRadiusProperty =
            ControlHelper.CornerRadiusProperty.AddOwner(typeof(DropDownButton));

        public CornerRadius CornerRadius
        {
            get => (CornerRadius)GetValue(CornerRadiusProperty);
            set => SetValue(CornerRadiusProperty, value);
        }

        #endregion

        #region IsTextScaleFactorEnabled

        public static readonly DependencyProperty IsTextScaleFactorEnabledProperty =
            ControlHelper.IsTextScaleFactorEnabledProperty.AddOwner(typeof(DropDownButton));

        public bool IsTextScaleFactorEnabled
        {
            get => (bool)GetValue(IsTextScaleFactorEnabledProperty);
            set => SetValue(IsTextScaleFactorEnabledProperty, value);
        }

        #endregion

        #region UseSystemFocusVisuals

        public static readonly DependencyProperty UseSystemFocusVisualsProperty =
            FocusVisualHelper.UseSystemFocusVisualsProperty.AddOwner(typeof(DropDownButton));

        public bool UseSystemFocusVisuals
        {
            get => (bool)GetValue(UseSystemFocusVisualsProperty);
            set => SetValue(UseSystemFocusVisualsProperty, value);
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

        #region Flyout

        public static readonly DependencyProperty FlyoutProperty =
            FlyoutService.FlyoutProperty.AddOwner(
                typeof(DropDownButton),
                new FrameworkPropertyMetadata(OnFlyoutPropertyChanged));

        public FlyoutBase Flyout
        {
            get => (FlyoutBase)GetValue(FlyoutProperty);
            set => SetValue(FlyoutProperty, value);
        }

        private static void OnFlyoutPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((DropDownButton)d).OnFlyoutPropertyChanged(e);
        }

        private void OnFlyoutPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue is FlyoutBase oldFlyout)
            {
                oldFlyout.Opened -= OnFlyoutOpened;
                oldFlyout.Closed -= OnFlyoutClosed;
            }

            if (e.NewValue is FlyoutBase newFlyout)
            {
                newFlyout.Opened += OnFlyoutOpened;
                newFlyout.Closed += OnFlyoutClosed;
            }
        }

        #endregion

        internal bool IsFlyoutOpen => m_isFlyoutOpen;

        internal void OpenFlyout()
        {
            Flyout?.ShowAt(this);
        }

        internal void CloseFlyout()
        {
            Flyout?.Hide();
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
    }
}
