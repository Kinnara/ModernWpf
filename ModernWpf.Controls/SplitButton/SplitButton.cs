// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
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
    public class SplitButton : ContentControl, ICommandSource
    {
        static SplitButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(SplitButton), new FrameworkPropertyMetadata(typeof(SplitButton)));
        }

        public SplitButton()
        {
            KeyDown += OnSplitButtonKeyDown;
            KeyUp += OnSplitButtonKeyUp;

            InputBindings.Add(new KeyBinding(new OpenFlyoutCommand(this), Key.Down, ModifierKeys.Alt));
        }

        #region CornerRadius

        public static readonly DependencyProperty CornerRadiusProperty =
            ControlHelper.CornerRadiusProperty.AddOwner(typeof(SplitButton));

        public CornerRadius CornerRadius
        {
            get => (CornerRadius)GetValue(CornerRadiusProperty);
            set => SetValue(CornerRadiusProperty, value);
        }

        #endregion

        #region UseSystemFocusVisuals

        public static readonly DependencyProperty UseSystemFocusVisualsProperty =
            FocusVisualHelper.UseSystemFocusVisualsProperty.AddOwner(typeof(SplitButton));

        public bool UseSystemFocusVisuals
        {
            get => (bool)GetValue(UseSystemFocusVisualsProperty);
            set => SetValue(UseSystemFocusVisualsProperty, value);
        }

        #endregion

        #region FocusVisualMargin

        public static readonly DependencyProperty FocusVisualMarginProperty =
            FocusVisualHelper.FocusVisualMarginProperty.AddOwner(typeof(SplitButton));

        public Thickness FocusVisualMargin
        {
            get => (Thickness)GetValue(FocusVisualMarginProperty);
            set => SetValue(FocusVisualMarginProperty, value);
        }

        #endregion

        #region Command

        public static readonly DependencyProperty CommandProperty =
            ButtonBase.CommandProperty.AddOwner(typeof(SplitButton));

        public ICommand Command
        {
            get => (ICommand)GetValue(CommandProperty);
            set => SetValue(CommandProperty, value);
        }

        #endregion

        #region CommandParameter

        public static readonly DependencyProperty CommandParameterProperty =
            ButtonBase.CommandParameterProperty.AddOwner(typeof(SplitButton));

        public object CommandParameter
        {
            get => GetValue(CommandParameterProperty);
            set => SetValue(CommandParameterProperty, value);
        }

        #endregion

        #region CommandTarget

        public static readonly DependencyProperty CommandTargetProperty =
            ButtonBase.CommandTargetProperty.AddOwner(typeof(SplitButton));

        public IInputElement CommandTarget
        {
            get => (IInputElement)GetValue(CommandTargetProperty);
            set => SetValue(CommandTargetProperty, value);
        }

        #endregion

        #region Flyout

        public static readonly DependencyProperty FlyoutProperty =
            DependencyProperty.Register(
                nameof(Flyout),
                typeof(FlyoutBase),
                typeof(SplitButton),
                new PropertyMetadata(OnFlyoutChanged));

        public FlyoutBase Flyout
        {
            get => (FlyoutBase)GetValue(FlyoutProperty);
            set => SetValue(FlyoutProperty, value);
        }

        private static void OnFlyoutChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((SplitButton)d).OnFlyoutChanged((FlyoutBase)e.OldValue, (FlyoutBase)e.NewValue);
        }

        #endregion

        private static readonly DependencyProperty PrimaryButtonIsPressedProperty =
            DependencyProperty.Register(
                "PrimaryButtonIsPressed",
                typeof(bool),
                typeof(SplitButton),
                new FrameworkPropertyMetadata(OnVisualPropertyChanged));

        private static readonly DependencyProperty PrimaryButtonIsMouseOverProperty =
            DependencyProperty.Register(
                "PrimaryButtonIsMouseOver",
                typeof(bool),
                typeof(SplitButton),
                new FrameworkPropertyMetadata(OnVisualPropertyChanged));

        private static readonly DependencyProperty SecondaryButtonIsPressedProperty =
            DependencyProperty.Register(
                "SecondaryButtonIsPressed",
                typeof(bool),
                typeof(SplitButton),
                new FrameworkPropertyMetadata(OnVisualPropertyChanged));

        private static readonly DependencyProperty SecondaryButtonIsMouseOverProperty =
            DependencyProperty.Register(
                "SecondaryButtonIsMouseOver",
                typeof(bool),
                typeof(SplitButton),
                new FrameworkPropertyMetadata(OnVisualPropertyChanged));

        private static readonly DependencyProperty FlyoutPlacementProperty =
            FlyoutBase.PlacementProperty.AddOwner(
                typeof(SplitButton),
                new FrameworkPropertyMetadata(OnFlyoutPlacementChanged));

        public event TypedEventHandler<SplitButton, SplitButtonClickEventArgs> Click;

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            UnregisterEvents();

            m_primaryButton = GetTemplateChild("PrimaryButton") as Button;
            m_secondaryButton = GetTemplateChild("SecondaryButton") as Button;

            if (m_primaryButton != null)
            {
                m_primaryButton.Click += OnClickPrimary;

                this.SetBinding(PrimaryButtonIsPressedProperty, ButtonBase.IsPressedProperty, m_primaryButton);
                this.SetBinding(PrimaryButtonIsMouseOverProperty, IsMouseOverProperty, m_primaryButton);
            }

            if (m_secondaryButton != null)
            {
                var secondaryName = Strings.SplitButtonSecondaryButtonName;
                AutomationProperties.SetName(m_secondaryButton, secondaryName);

                m_secondaryButton.Click += OnClickSecondary;

                this.SetBinding(SecondaryButtonIsPressedProperty, ButtonBase.IsPressedProperty, m_secondaryButton);
                this.SetBinding(SecondaryButtonIsMouseOverProperty, IsMouseOverProperty, m_secondaryButton);
            }

            UpdateVisualStates();

            m_hasLoaded = true;
        }

        protected override AutomationPeer OnCreateAutomationPeer()
        {
            return new SplitButtonAutomationPeer(this);
        }

        private void OnFlyoutChanged(FlyoutBase oldFlyout, FlyoutBase newFlyout)
        {
            RegisterFlyoutEvents(oldFlyout, newFlyout);

            UpdateVisualStates();
        }

        private void RegisterFlyoutEvents(FlyoutBase oldFlyout, FlyoutBase newFlyout)
        {
            if (oldFlyout != null)
            {
                oldFlyout.Opened -= OnFlyoutOpened;
                oldFlyout.Closed -= OnFlyoutClosed;
                ClearValue(FlyoutPlacementProperty);
            }

            if (newFlyout != null)
            {
                newFlyout.Opened += OnFlyoutOpened;

                newFlyout.Closed += OnFlyoutClosed;

                this.SetBinding(FlyoutPlacementProperty, FlyoutBase.PlacementProperty, newFlyout);
            }
        }

        private static void OnVisualPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((SplitButton)d).UpdateVisualStates();
        }

        internal void UpdateVisualStates(bool useTransitions = true)
        {
            // place the secondary button
            if (m_isKeyDown)
            {
                VisualStateManager.GoToState(this, "SecondaryButtonSpan", useTransitions);
            }
            else
            {
                VisualStateManager.GoToState(this, "SecondaryButtonRight", useTransitions);
            }

            // change visual state
            var primaryButton = m_primaryButton;
            var secondaryButton = m_secondaryButton;
            if (primaryButton != null && m_secondaryButton != null)
            {
                if (m_isFlyoutOpen)
                {
                    VisualStateManager.GoToState(this, "FlyoutOpen", useTransitions);
                }
                // SplitButton and ToggleSplitButton share a template -- this section is driving the checked states for ToggleSplitButton.
                else if (InternalIsChecked)
                {
                    if (m_isKeyDown)
                    {
                        if (primaryButton.IsPressed || secondaryButton.IsPressed || m_isKeyDown)
                        {
                            VisualStateManager.GoToState(this, "CheckedTouchPressed", useTransitions);
                        }
                        else
                        {
                            VisualStateManager.GoToState(this, "Checked", useTransitions);
                        }
                    }
                    else if (primaryButton.IsPressed)
                    {
                        VisualStateManager.GoToState(this, "CheckedPrimaryPressed", useTransitions);
                    }
                    else if (primaryButton.IsMouseOver)
                    {
                        VisualStateManager.GoToState(this, "CheckedPrimaryPointerOver", useTransitions);
                    }
                    else if (secondaryButton.IsPressed)
                    {
                        VisualStateManager.GoToState(this, "CheckedSecondaryPressed", useTransitions);
                    }
                    else if (secondaryButton.IsMouseOver)
                    {
                        VisualStateManager.GoToState(this, "CheckedSecondaryPointerOver", useTransitions);
                    }
                    else
                    {
                        VisualStateManager.GoToState(this, "Checked", useTransitions);
                    }
                }
                else
                {
                    if (m_isKeyDown)
                    {
                        if (primaryButton.IsPressed || secondaryButton.IsPressed || m_isKeyDown)
                        {
                            VisualStateManager.GoToState(this, "TouchPressed", useTransitions);
                        }
                        else
                        {
                            VisualStateManager.GoToState(this, "Normal", useTransitions);
                        }
                    }
                    else if (primaryButton.IsPressed)
                    {
                        VisualStateManager.GoToState(this, "PrimaryPressed", useTransitions);
                    }
                    else if (primaryButton.IsMouseOver)
                    {
                        VisualStateManager.GoToState(this, "PrimaryPointerOver", useTransitions);
                    }
                    else if (secondaryButton.IsPressed)
                    {
                        VisualStateManager.GoToState(this, "SecondaryPressed", useTransitions);
                    }
                    else if (secondaryButton.IsMouseOver)
                    {
                        VisualStateManager.GoToState(this, "SecondaryPointerOver", useTransitions);
                    }
                    else
                    {
                        VisualStateManager.GoToState(this, "Normal", useTransitions);
                    }
                }
            }
        }

        internal bool IsFlyoutOpen => m_isFlyoutOpen;

        internal void OpenFlyout()
        {
            var flyout = Flyout;
            if (flyout != null)
            {
                flyout.ShowAt(this);
            }
        }

        internal void CloseFlyout()
        {
            var flyout = Flyout;
            if (flyout != null)
            {
                flyout.Hide();
            }
        }

        internal virtual void OnClickPrimary(object sender, RoutedEventArgs e)
        {
            Click?.Invoke(this, new SplitButtonClickEventArgs());

            if (FrameworkElementAutomationPeer.FromElement(this) is AutomationPeer peer)
            {
                peer.RaiseAutomationEvent(AutomationEvents.InvokePatternOnInvoked);
            }
        }

        internal virtual bool InternalIsChecked => false;

        private void OnFlyoutOpened(object sender, object e)
        {
            m_isFlyoutOpen = true;
            UpdateVisualStates();
            SharedHelpers.RaiseAutomationPropertyChangedEvent(this, ExpandCollapseState.Collapsed, ExpandCollapseState.Expanded);
        }

        private void OnFlyoutClosed(object sender, object e)
        {
            m_isFlyoutOpen = false;
            UpdateVisualStates();
            SharedHelpers.RaiseAutomationPropertyChangedEvent(this, ExpandCollapseState.Expanded, ExpandCollapseState.Collapsed);
        }

        private static void OnFlyoutPlacementChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((SplitButton)d).UpdateVisualStates();
        }

        private void OnClickSecondary(object sender, RoutedEventArgs e)
        {
            OpenFlyout();
        }

        private void OnSplitButtonKeyDown(object sender, KeyEventArgs args)
        {
            Key key = args.Key;
            if (key == Key.Space || key == Key.Enter)
            {
                m_isKeyDown = true;
                UpdateVisualStates();
            }
        }

        private void OnSplitButtonKeyUp(object sender, KeyEventArgs args)
        {
            Key key = args.Key;
            if (key == Key.Space || key == Key.Enter)
            {
                m_isKeyDown = false;
                UpdateVisualStates();

                // Consider this a click on the primary button
                if (IsEnabled)
                {
                    OnClickPrimary(null, null);
                    args.Handled = true;
                }
            }
            //else if (key == Key.Down)
            //{
            //    bool menuKeyDown = Keyboard.Modifiers.HasFlag(ModifierKeys.Alt);

            //    if (IsEnabled && menuKeyDown)
            //    {
            //        // Open the menu on alt-down
            //        OpenFlyout();
            //        args.Handled = true;
            //    }
            //}
            else if (key == Key.F4 && IsEnabled)
            {
                // Open the menu on F4
                OpenFlyout();
                args.Handled = true;
            }
        }

        private void UnregisterEvents()
        {
            if (m_primaryButton != null)
            {
                m_primaryButton.Click -= OnClickPrimary;

                ClearValue(PrimaryButtonIsPressedProperty);
                ClearValue(PrimaryButtonIsMouseOverProperty);
            }

            if (m_secondaryButton != null)
            {
                m_secondaryButton.Click -= OnClickSecondary;

                ClearValue(SecondaryButtonIsPressedProperty);
                ClearValue(SecondaryButtonIsMouseOverProperty);
            }
        }

        internal bool m_hasLoaded;

        private Button m_primaryButton;
        private Button m_secondaryButton;

        private bool m_isFlyoutOpen;
        private bool m_isKeyDown;

        private readonly CornerRadiusFilterConverter m_cornerRadiusFilterConverter = new CornerRadiusFilterConverter();

        private class OpenFlyoutCommand : ICommand
        {
            public OpenFlyoutCommand(SplitButton owner)
            {
                m_owner = owner;
            }

            public event EventHandler CanExecuteChanged;

            public bool CanExecute(object parameter)
            {
                return true;
            }

            public void Execute(object parameter)
            {
                m_owner.OpenFlyout();
            }

            private readonly SplitButton m_owner;
        }
    }
}
