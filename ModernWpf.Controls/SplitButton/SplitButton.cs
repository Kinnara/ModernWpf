// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using ModernWpf.Automation.Peers;
using ModernWpf.Controls.Primitives;
using ModernWpf.Media.Animation;
using static ModernWpf.ResourceAccessor;

namespace ModernWpf.Controls
{
    public class SplitButton : ContentControl, ICommandSource
    {
        private static readonly ResourceAccessor ResourceAccessor = new ResourceAccessor(typeof(SplitButton));

        static SplitButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(SplitButton), new FrameworkPropertyMetadata(typeof(SplitButton)));
        }

        public SplitButton()
        {
            KeyDown += OnSplitButtonKeyDown;
            KeyUp += OnSplitButtonKeyUp;
            IsEnabledChanged += OnSplitButtonIsEnabledChanged;
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

        #region ContentTransitions

        public static readonly DependencyProperty ContentTransitionsProperty =
            ControlHelper.ContentTransitionsProperty.AddOwner(typeof(SplitButton));

        public TransitionCollection ContentTransitions
        {
            get => (TransitionCollection)GetValue(ContentTransitionsProperty);
            set => SetValue(ContentTransitionsProperty, value);
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
            ((SplitButton)d).OnFlyoutChanged();
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
                RegisterPointerEvents(m_primaryButton);
            }

            if (m_secondaryButton != null)
            {
                var secondaryName = ResourceAccessor.GetLocalizedStringResource(SR_SplitButtonSecondaryButtonName);
                AutomationProperties.SetName(m_secondaryButton, secondaryName);

                m_secondaryButton.Click += OnClickSecondary;

                this.SetBinding(SecondaryButtonIsPressedProperty, ButtonBase.IsPressedProperty, m_secondaryButton);
                this.SetBinding(SecondaryButtonIsMouseOverProperty, IsMouseOverProperty, m_secondaryButton);
                RegisterPointerEvents(m_secondaryButton);
            }

            RegisterFlyoutEvents();

            UpdateVisualStates();

            m_hasLoaded = true;
        }

        protected override AutomationPeer OnCreateAutomationPeer()
        {
            return new SplitButtonAutomationPeer(this);
        }

        private void OnFlyoutChanged()
        {
            RegisterFlyoutEvents();

            UpdateVisualStates();
        }

        private void RegisterFlyoutEvents()
        {
            if (m_registeredFlyout != null)
            {
                m_registeredFlyout.Opened -= OnFlyoutOpened;
                m_registeredFlyout.Closed -= OnFlyoutClosed;
                ClearValue(FlyoutPlacementProperty);
                m_registeredFlyout = null;
            }

            var flyout = Flyout;
            if (flyout != null)
            {
                flyout.Opened += OnFlyoutOpened;

                flyout.Closed += OnFlyoutClosed;

                this.SetBinding(FlyoutPlacementProperty, FlyoutBase.PlacementProperty, flyout);
                m_registeredFlyout = flyout;
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
            if (!IsEnabled)
            {
                VisualStateManager.GoToState(this, "Disabled", useTransitions);
                return;
            }

            var primaryButton = m_primaryButton;
            var secondaryButton = m_secondaryButton;
            if (primaryButton != null && m_secondaryButton != null)
            {
                if (m_isFlyoutOpen)
                {
                    if (InternalIsChecked)
                    {
                        VisualStateManager.GoToState(this, "CheckedFlyoutOpen", useTransitions);
                    }
                    else
                    {
                        VisualStateManager.GoToState(this, "FlyoutOpen", useTransitions);
                    }
                }
                // SplitButton and ToggleSplitButton share a template -- this section is driving the checked states for ToggleSplitButton.
                else if (InternalIsChecked)
                {
                    if (m_lastPointerDeviceType == PointerDeviceType.Touch || m_isKeyDown)
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
                    if (m_lastPointerDeviceType == PointerDeviceType.Touch || m_isKeyDown)
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
                var options = new FlyoutShowOptions
                {
                    Placement = FlyoutPlacementMode.BottomEdgeAlignedLeft
                };
                flyout.ShowAt(this, options);
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

        internal void Invoke()
        {
            bool invoked = false;

            var primaryButton = m_primaryButton;
            if (primaryButton != null &&
                FrameworkElementAutomationPeer.FromElement(primaryButton) is AutomationPeer peer &&
                peer.GetPattern(PatternInterface.Invoke) is IInvokeProvider invokeProvider)
            {
                invokeProvider.Invoke();
                invoked = true;
            }

            if (!invoked)
            {
                OnClickPrimary(null, null);
            }
        }

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

        private void ExecuteCommand()
        {
            var command = Command;
            if (command == null)
            {
                return;
            }

            var commandParameter = CommandParameter;
            if (command is RoutedCommand routedCommand)
            {
                var commandTarget = CommandTarget ?? this;
                if (routedCommand.CanExecute(commandParameter, commandTarget))
                {
                    routedCommand.Execute(commandParameter, commandTarget);
                }
            }
            else if (command.CanExecute(commandParameter))
            {
                command.Execute(commandParameter);
            }
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
                    ExecuteCommand();
                    args.Handled = true;
                }
            }
            else if (key == Key.Down)
            {
                bool menuKeyDown = Keyboard.Modifiers.HasFlag(ModifierKeys.Alt);

                if (IsEnabled && menuKeyDown)
                {
                    // Open the menu on alt-down
                    OpenFlyout();
                    args.Handled = true;
                }
            }
            else if (key == Key.F4 && IsEnabled)
            {
                // Open the menu on F4
                OpenFlyout();
                args.Handled = true;
            }
        }

        private void OnSplitButtonIsEnabledChanged(object sender, DependencyPropertyChangedEventArgs args)
        {
            UpdateVisualStates();
        }

        private void UnregisterEvents()
        {
            if (m_primaryButton != null)
            {
                m_primaryButton.Click -= OnClickPrimary;
                UnregisterPointerEvents(m_primaryButton);

                ClearValue(PrimaryButtonIsPressedProperty);
                ClearValue(PrimaryButtonIsMouseOverProperty);
            }

            if (m_secondaryButton != null)
            {
                m_secondaryButton.Click -= OnClickSecondary;
                UnregisterPointerEvents(m_secondaryButton);

                ClearValue(SecondaryButtonIsPressedProperty);
                ClearValue(SecondaryButtonIsMouseOverProperty);
            }
        }

        private void RegisterPointerEvents(Button button)
        {
            button.MouseEnter += OnMousePointerEvent;
            button.MouseLeave += OnMousePointerEvent;
            button.PreviewMouseDown += OnMouseButtonPointerEvent;
            button.PreviewMouseUp += OnMouseButtonPointerEvent;
            button.TouchDown += OnTouchPointerEvent;
            button.TouchUp += OnTouchPointerEvent;
        }

        private void UnregisterPointerEvents(Button button)
        {
            button.MouseEnter -= OnMousePointerEvent;
            button.MouseLeave -= OnMousePointerEvent;
            button.PreviewMouseDown -= OnMouseButtonPointerEvent;
            button.PreviewMouseUp -= OnMouseButtonPointerEvent;
            button.TouchDown -= OnTouchPointerEvent;
            button.TouchUp -= OnTouchPointerEvent;
        }

        private void OnMousePointerEvent(object sender, MouseEventArgs args)
        {
            SetLastPointerDeviceType(PointerDeviceType.Mouse);
        }

        private void OnMouseButtonPointerEvent(object sender, MouseButtonEventArgs args)
        {
            SetLastPointerDeviceType(PointerDeviceType.Mouse);
        }

        private void OnTouchPointerEvent(object sender, TouchEventArgs args)
        {
            SetLastPointerDeviceType(PointerDeviceType.Touch);
        }

        private void SetLastPointerDeviceType(PointerDeviceType pointerDeviceType)
        {
            if (m_lastPointerDeviceType != pointerDeviceType)
            {
                m_lastPointerDeviceType = pointerDeviceType;
                UpdateVisualStates();
            }
        }

        internal bool m_hasLoaded;

        private Button m_primaryButton;
        private Button m_secondaryButton;
        private FlyoutBase m_registeredFlyout;

        private bool m_isFlyoutOpen;
        private bool m_isKeyDown;
        private PointerDeviceType m_lastPointerDeviceType = PointerDeviceType.Mouse;

        private enum PointerDeviceType
        {
            Mouse,
            Touch,
        }
    }
}
