// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Markup;
using ModernWpf.Controls.Primitives;

namespace ModernWpf.Controls
{
    [ContentProperty(nameof(PrimaryCommands))]
    public class CommandBarFlyout : FlyoutBase
    {
        public CommandBarFlyout()
        {
            ShouldConstrainToRootBounds = false;
            AreOpenCloseAnimationsEnabled = false;

            PrimaryCommands = new ObservableCollection<ICommandBarElement>();
            SecondaryCommands = new ObservableCollection<ICommandBarElement>();

            PrimaryCommands.CollectionChanged += (object sender, NotifyCollectionChangedEventArgs args) =>
            {
                if (m_commandBar != null)
                {
                    SharedHelpers.ForwardCollectionChange((ObservableCollection<ICommandBarElement>)sender, m_commandBar.PrimaryCommands, args);
                }
            };

            SecondaryCommands.CollectionChanged += (object sender, NotifyCollectionChangedEventArgs args) =>
            {
                var commandBar = m_commandBar;
                if (commandBar != null)
                {
                    var source = (ObservableCollection<ICommandBarElement>)sender;
                    SharedHelpers.ForwardCollectionChange(source, commandBar.SecondaryCommands, args);

                    // We want to ensure that any interaction with secondary items causes the CommandBarFlyout
                    // to close, so we'll attach a Click handler to any buttons and Checked/Unchecked handlers
                    // to any toggle buttons that we get and close the flyout when they're invoked.
                    // The only exception is buttons with flyouts - in that case, clicking on the button
                    // will just open the flyout rather than executing an action, so we don't want that to
                    // do anything.
                    RoutedEventHandler closeFlyoutFunc = delegate { Hide(); };

                    switch (args.Action)
                    {
                        case NotifyCollectionChangedAction.Replace:
                            {
                                foreach (ICommandBarElement oldElement in args.OldItems)
                                {
                                    UnhookCommandBarElementDependencyPropertyChanges(oldElement);
                                    RevokeAndRemove(m_secondaryButtonClickRevokerByElementMap, oldElement);
                                    RevokeAndRemove(m_secondaryToggleButtonCheckedRevokerByElementMap, oldElement);
                                    RevokeAndRemove(m_secondaryToggleButtonUncheckedRevokerByElementMap, oldElement);
                                }

                                foreach (ICommandBarElement element in args.NewItems)
                                {
                                    HookCommandBarElementDependencyPropertyChanges(element);
                                    HookSecondaryCommandCloseHandlers(element, closeFlyoutFunc);
                                }
                                break;
                            }
                        case NotifyCollectionChangedAction.Add:
                            {
                                foreach (ICommandBarElement element in args.NewItems)
                                {
                                    HookCommandBarElementDependencyPropertyChanges(element);
                                    HookSecondaryCommandCloseHandlers(element, closeFlyoutFunc);
                                }
                                break;
                            }
                        case NotifyCollectionChangedAction.Remove:
                            {
                                foreach (ICommandBarElement element in args.OldItems)
                                {
                                    UnhookCommandBarElementDependencyPropertyChanges(element);
                                    RevokeAndRemove(m_secondaryButtonClickRevokerByElementMap, element);
                                    RevokeAndRemove(m_secondaryToggleButtonCheckedRevokerByElementMap, element);
                                    RevokeAndRemove(m_secondaryToggleButtonUncheckedRevokerByElementMap, element);
                                }
                                break;
                            }
                        case NotifyCollectionChangedAction.Move:
                            break;
                        case NotifyCollectionChangedAction.Reset:
                            SetSecondaryCommandsToCloseWhenExecuted();
                            HookAllCommandBarElementDependencyPropertyChanges();
                            break;
                        default:
                            Debug.Assert(false);
                            break;
                    }
                }
            };

            Opening += delegate
            {
                InternalPopup.SuppressFadeAnimation = true;
                // The expanded command list is hosted in a child HWND. WPF's
                // built-in Popup light-dismiss capture treats that HWND as an
                // outside surface, starving both primary and secondary command
                // buttons of their normal pointer states. Keep the outer popup
                // open and light-dismiss the two related surfaces as one tree.
                InternalPopup.StaysOpen = true;
                StartLightDismissTracking();

                if (m_commandBar is { } commandBar)
                {
                    if (ShowMode == FlyoutShowMode.Standard)
                    {
                        m_commandBar.IsOpen = true;
                    }

                    // When CommandBarFlyout is in AlwaysOpen state, don't show the overflow button
                    if (AlwaysExpanded)
                    {
                        SetCurrentValue(ShowModeProperty, FlyoutShowMode.Standard);
                        commandBar.IsOpen = true;
                        commandBar.OverflowButtonVisibility = CommandBarOverflowButtonVisibility.Collapsed;
                    }
                    else
                    {
                        commandBar.OverflowButtonVisibility = CommandBarOverflowButtonVisibility.Auto;
                    }
                }

                if (PrimaryCommands.Count > 0)
                {
                    AddDropShadow();
                }

                // Prime the custom opacity animation before WPF shows the popup
                // HWND. Starting at zero from Opened is one frame too late and
                // produces an opaque/transparent/opaque flash on fast displays.
                if (m_commandBar?.HasOpenAnimation() == true)
                {
                    m_commandBar.PrepareOpenAnimation();
                }
            };

            Opened += delegate
            {
                if (m_commandBar != null)
                {
                    if (m_commandBar.HasOpenAnimation())
                    {
                        m_commandBar.PlayOpenAnimation();
                    }
                }
            };

            Closed += delegate
            {
                m_isCloseAnimationRunning = false;
                StopLightDismissTracking();

                if (m_commandBar != null)
                {
                    if (m_commandBar.IsOpen)
                    {
                        m_commandBar.IsOpen = false;
                    }

                    // Shadow changes alter the WPF popup's reserved bounds. Do
                    // that cleanup only after the popup is hidden so open/close
                    // and overflow transitions cannot move the visible surface.
                    RemoveDropShadow();
                    m_commandBar.ClearShadow();
                }
            };
        }

        public bool AlwaysExpanded { get; set; }

        public ObservableCollection<ICommandBarElement> PrimaryCommands { get; }

        public ObservableCollection<ICommandBarElement> SecondaryCommands { get; }

        internal override PopupAnimation DesiredPopupAnimation => PopupAnimation.Fade;

        internal override void HideCore()
        {
            if (!IsOpen || m_isClosingAfterCloseAnimation)
            {
                base.HideCore();
                return;
            }

            if (m_isCloseAnimationRunning || m_isRaisingClosing)
            {
                return;
            }

            // PopupEx raises its cancellable Closing event only after WPF has
            // already torn down the popup HWND. Cancelling at that point to run
            // the close animation closes and recreates the native window, which
            // is visible as a several-frame flash. Raise Closing while the HWND
            // is still open, then animate in place and perform one final,
            // uncancelled native close.
            bool cancel;
            m_isRaisingClosing = true;
            try
            {
                cancel = OnClosing();
            }
            finally
            {
                m_isRaisingClosing = false;
            }

            if (cancel)
            {
                return;
            }

            var commandBar = m_commandBar;
            if (commandBar?.HasCloseAnimation() == true)
            {
                m_isCloseAnimationRunning = true;
                commandBar.PlayCloseAnimation(CompleteCloseAfterAnimation);
            }
            else
            {
                CompleteCloseAfterAnimation();
            }
        }

        internal override bool OnClosing()
        {
            // The public cancellable event was raised before starting the
            // animation. Suppress PopupEx's post-teardown duplicate event so it
            // cannot reopen the native popup.
            return m_isClosingAfterCloseAnimation ? false : base.OnClosing();
        }

        private void CompleteCloseAfterAnimation()
        {
            m_isCloseAnimationRunning = false;
            m_isClosingAfterCloseAnimation = true;

            try
            {
                // Close the overflow HWND before the primary HWND.
                if (m_commandBar?.IsOpen == true)
                {
                    m_commandBar.IsOpen = false;
                }

                base.HideCore();
            }
            finally
            {
                m_isClosingAfterCloseAnimation = false;
            }
        }

        protected override Control CreatePresenter()
        {
            var commandBar = new CommandBarFlyoutCommandBar();
            m_commandBar = commandBar;

            SharedHelpers.CopyList(PrimaryCommands, commandBar.PrimaryCommands);
            SharedHelpers.CopyList(SecondaryCommands, commandBar.SecondaryCommands);

            SetSecondaryCommandsToCloseWhenExecuted();
            HookAllCommandBarElementDependencyPropertyChanges();

            FlyoutPresenter presenter = new FlyoutPresenter
            {
                Background = null,
                Foreground = null,
                BorderBrush = null,
                MinWidth = 0,
                MaxWidth = double.PositiveInfinity,
                MinHeight = 0,
                MaxHeight = double.PositiveInfinity,
                BorderThickness = new Thickness(0),
                Padding = new Thickness(0),
                Content = commandBar,
                CornerRadius = new CornerRadius(0),
                // WPF measures a Popup as soon as its child is assigned. If the
                // shadow is enabled only from Opening, the first HWND keeps the
                // unshadowed 221x60 size and clips the reserved 10/2/10/18
                // shadow insets (including the right side of the More button).
                // WinUI's compositor shadow does not participate in layout, so
                // prime the WPF substitute before that first popup measure.
                IsDefaultShadowEnabled = PrimaryCommands.Count > 0
            };

            m_presenter = presenter;

            commandBar.Opened += delegate
            {
                SetCurrentValue(ShowModeProperty, FlyoutShowMode.Standard);
            };
            commandBar.Closing += delegate
            {
                if (AlwaysExpanded && IsOpen && !m_isClosingAfterCloseAnimation)
                {
                    // Match AppBar::Closing in the WinUI flyout command bar: the
                    // overflow list cannot be collapsed while AlwaysExpanded owns
                    // an open outer flyout.
                    commandBar.SetCurrentValue(CommandBarFlyoutCommandBar.IsOpenProperty, true);
                }
            };

            commandBar.SetOwningFlyout(this);
            return presenter;
        }

        protected override bool ShouldRecreatePresenterAfterClose => true;

        protected override void OnPresenterReleased()
        {
            StopLightDismissTracking();
            m_commandBar?.ReleaseCommandElements();
            m_commandBar = null;
            m_presenter = null;
        }

        private void SetSecondaryCommandsToCloseWhenExecuted()
        {
            RevokeAndClear(m_secondaryButtonClickRevokerByElementMap);
            RevokeAndClear(m_secondaryToggleButtonCheckedRevokerByElementMap);
            RevokeAndClear(m_secondaryToggleButtonUncheckedRevokerByElementMap);

            RoutedEventHandler closeFlyoutFunc = delegate { Hide(); };

            for (int i = 0; i < SecondaryCommands.Count; i++)
            {
                var element = SecondaryCommands[i];
                var button = element as AppBarButton;
                var toggleButton = element as AppBarToggleButton;

                if (button != null && button.Flyout == null)
                {
                    m_secondaryButtonClickRevokerByElementMap[element] = new RoutedEventHandlerRevoker(
                        button, ButtonBase.ClickEvent, closeFlyoutFunc);
                }
                else if (toggleButton != null)
                {
                    m_secondaryToggleButtonCheckedRevokerByElementMap[element] = new RoutedEventHandlerRevoker(
                        toggleButton, ToggleButton.CheckedEvent, closeFlyoutFunc);
                    m_secondaryToggleButtonUncheckedRevokerByElementMap[element] = new RoutedEventHandlerRevoker(
                        toggleButton, ToggleButton.UncheckedEvent, closeFlyoutFunc);
                }
            }
        }

        private void HookSecondaryCommandCloseHandlers(
            ICommandBarElement element,
            RoutedEventHandler closeFlyoutFunc)
        {
            var button = element as AppBarButton;
            var toggleButton = element as AppBarToggleButton;

            if (button != null && button.Flyout == null)
            {
                m_secondaryButtonClickRevokerByElementMap[element] = new RoutedEventHandlerRevoker(
                    button, ButtonBase.ClickEvent, closeFlyoutFunc);
                RevokeAndRemove(m_secondaryToggleButtonCheckedRevokerByElementMap, element);
                RevokeAndRemove(m_secondaryToggleButtonUncheckedRevokerByElementMap, element);
            }
            else if (toggleButton != null)
            {
                RevokeAndRemove(m_secondaryButtonClickRevokerByElementMap, element);
                m_secondaryToggleButtonCheckedRevokerByElementMap[element] = new RoutedEventHandlerRevoker(
                    toggleButton, ToggleButton.CheckedEvent, closeFlyoutFunc);
                m_secondaryToggleButtonUncheckedRevokerByElementMap[element] = new RoutedEventHandlerRevoker(
                    toggleButton, ToggleButton.UncheckedEvent, closeFlyoutFunc);
            }
            else
            {
                RevokeAndRemove(m_secondaryButtonClickRevokerByElementMap, element);
                RevokeAndRemove(m_secondaryToggleButtonCheckedRevokerByElementMap, element);
                RevokeAndRemove(m_secondaryToggleButtonUncheckedRevokerByElementMap, element);
            }
        }

        internal FlyoutPresenter GetPresenter()
        {
            return m_presenter;
        }

        internal void AddDropShadow()
        {
            if (m_presenter != null)
            {
                m_presenter.IsDefaultShadowEnabled = true;
            }
        }

        internal void RemoveDropShadow()
        {
            if (m_presenter != null)
            {
                m_presenter.IsDefaultShadowEnabled = false;
            }
        }

        private void HookAllCommandBarElementDependencyPropertyChanges()
        {
            UnhookAllCommandBarElementDependencyPropertyChanges();

            foreach (var element in SecondaryCommands)
            {
                HookCommandBarElementDependencyPropertyChanges(element);
            }
        }

        private void HookCommandBarElementDependencyPropertyChanges(ICommandBarElement element)
        {
            if (m_commandBar == null)
            {
                return;
            }

            UnhookCommandBarElementDependencyPropertyChanges(element);

            if (element is AppBarButton button)
            {
                m_propertyChangedRevokersByElementMap[element] = new List<DependencyPropertyChangedRevoker>
                {
                    new(button, AppBarButton.IconProperty, OnCommandBarElementDependencyPropertyChanged),
                    new(button, AppBarButton.LabelProperty, OnCommandBarElementDependencyPropertyChanged),
                    new(button, AppBarButton.KeyboardAcceleratorTextOverrideProperty, OnCommandBarElementDependencyPropertyChanged)
                };
            }
            else if (element is AppBarToggleButton toggleButton)
            {
                m_propertyChangedRevokersByElementMap[element] = new List<DependencyPropertyChangedRevoker>
                {
                    new(toggleButton, AppBarToggleButton.IconProperty, OnCommandBarElementDependencyPropertyChanged),
                    new(toggleButton, AppBarToggleButton.LabelProperty, OnCommandBarElementDependencyPropertyChanged),
                    new(toggleButton, AppBarToggleButton.KeyboardAcceleratorTextOverrideProperty, OnCommandBarElementDependencyPropertyChanged)
                };
            }
        }

        private void UnhookCommandBarElementDependencyPropertyChanges(ICommandBarElement element)
        {
            if (m_propertyChangedRevokersByElementMap.TryGetValue(element, out var revokers))
            {
                foreach (var revoker in revokers)
                {
                    revoker.Revoke();
                }

                m_propertyChangedRevokersByElementMap.Remove(element);
            }
        }

        private void UnhookAllCommandBarElementDependencyPropertyChanges()
        {
            foreach (var revokers in m_propertyChangedRevokersByElementMap.Values)
            {
                foreach (var revoker in revokers)
                {
                    revoker.Revoke();
                }
            }

            m_propertyChangedRevokersByElementMap.Clear();
        }

        private void OnCommandBarElementDependencyPropertyChanged(object sender, EventArgs args)
        {
            m_commandBar?.OnCommandBarElementDependencyPropertyChanged();
        }

        private void StartLightDismissTracking()
        {
            InputManager.Current.PreProcessInput -= OnPreProcessInput;
            InputManager.Current.PreProcessInput += OnPreProcessInput;

            var ownerWindow = Window.GetWindow(Target);
            if (!ReferenceEquals(m_lightDismissOwnerWindow, ownerWindow))
            {
                if (m_lightDismissOwnerWindow != null)
                {
                    m_lightDismissOwnerWindow.Deactivated -= OnLightDismissOwnerDeactivated;
                }

                m_lightDismissOwnerWindow = ownerWindow;
                if (m_lightDismissOwnerWindow != null)
                {
                    m_lightDismissOwnerWindow.Deactivated += OnLightDismissOwnerDeactivated;
                }
            }
        }

        private void StopLightDismissTracking()
        {
            InputManager.Current.PreProcessInput -= OnPreProcessInput;

            if (m_lightDismissOwnerWindow != null)
            {
                m_lightDismissOwnerWindow.Deactivated -= OnLightDismissOwnerDeactivated;
                m_lightDismissOwnerWindow = null;
            }

            m_isLightDismissing = false;
        }

        private void OnPreProcessInput(object sender, PreProcessInputEventArgs args)
        {
            if (!IsOpen || m_isLightDismissing || !IsPointerDown(args.StagingItem.Input))
            {
                return;
            }

            if (IsPointerInsideFlyoutSurface())
            {
                return;
            }

            m_isLightDismissing = true;
            try
            {
                Hide();
            }
            finally
            {
                m_isLightDismissing = false;
            }
        }

        private void OnLightDismissOwnerDeactivated(object sender, EventArgs args)
        {
            if (IsOpen && !m_isLightDismissing)
            {
                m_isLightDismissing = true;
                try
                {
                    Hide();
                }
                finally
                {
                    m_isLightDismissing = false;
                }
            }
        }

        private bool IsPointerInsideFlyoutSurface()
        {
            return IsPointerInsideElement(m_presenter) ||
                   m_commandBar?.IsPointerInsideOverflowPopup() == true;
        }

        private static bool IsPointerDown(InputEventArgs args)
        {
            return (args is MouseButtonEventArgs mouseArgs &&
                    mouseArgs.ButtonState == MouseButtonState.Pressed) ||
                   (args is TouchEventArgs touchArgs &&
                    touchArgs.RoutedEvent == UIElement.TouchDownEvent) ||
                   (args is StylusEventArgs stylusArgs &&
                    stylusArgs.RoutedEvent == UIElement.StylusDownEvent);
        }

        private static bool IsPointerInsideElement(FrameworkElement element)
        {
            if (element == null || !element.IsVisible || !GetCursorPos(out var point))
            {
                return false;
            }

            try
            {
                var topLeft = element.PointToScreen(new Point());
                var bottomRight = element.PointToScreen(new Point(element.ActualWidth, element.ActualHeight));
                return point.X >= topLeft.X && point.X < bottomRight.X &&
                       point.Y >= topLeft.Y && point.Y < bottomRight.Y;
            }
            catch (InvalidOperationException)
            {
                return false;
            }
        }

        private static void RevokeAndRemove(IDictionary<ICommandBarElement, RoutedEventHandlerRevoker> map, ICommandBarElement element)
        {
            if (map.TryGetValue(element, out var revoker))
            {
                revoker.Revoke();
                map.Remove(element);
            }
        }

        private static void RevokeAndClear(IDictionary<ICommandBarElement, RoutedEventHandlerRevoker> map)
        {
            foreach (var value in map.Values)
            {
                value.Revoke();
            }
            map.Clear();
        }

        CommandBarFlyoutCommandBar m_commandBar;

        Dictionary<ICommandBarElement, RoutedEventHandlerRevoker> m_secondaryButtonClickRevokerByElementMap =
            new Dictionary<ICommandBarElement, RoutedEventHandlerRevoker>();
        Dictionary<ICommandBarElement, RoutedEventHandlerRevoker> m_secondaryToggleButtonCheckedRevokerByElementMap =
            new Dictionary<ICommandBarElement, RoutedEventHandlerRevoker>();
        Dictionary<ICommandBarElement, RoutedEventHandlerRevoker> m_secondaryToggleButtonUncheckedRevokerByElementMap =
            new Dictionary<ICommandBarElement, RoutedEventHandlerRevoker>();
        Dictionary<ICommandBarElement, List<DependencyPropertyChangedRevoker>> m_propertyChangedRevokersByElementMap =
            new Dictionary<ICommandBarElement, List<DependencyPropertyChangedRevoker>>();

        FlyoutPresenter m_presenter;

        Window m_lightDismissOwnerWindow;

        bool m_isLightDismissing;

        bool m_isCloseAnimationRunning;
        bool m_isRaisingClosing;
        bool m_isClosingAfterCloseAnimation;

        [StructLayout(LayoutKind.Sequential)]
        private struct NativePoint
        {
            public int X;
            public int Y;
        }

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool GetCursorPos(out NativePoint point);

        private sealed class DependencyPropertyChangedRevoker
        {
            public DependencyPropertyChangedRevoker(
                DependencyObject source,
                DependencyProperty property,
                EventHandler handler)
            {
                _source = source;
                _handler = handler;
                _descriptor = DependencyPropertyDescriptor.FromProperty(property, source.GetType());
                _descriptor?.AddValueChanged(source, handler);
            }

            public void Revoke()
            {
                if (_descriptor != null && _source != null && _handler != null)
                {
                    _descriptor.RemoveValueChanged(_source, _handler);
                    _source = null;
                    _handler = null;
                    _descriptor = null;
                }
            }

            private DependencyObject _source;
            private EventHandler _handler;
            private DependencyPropertyDescriptor _descriptor;
        }
    }
}
