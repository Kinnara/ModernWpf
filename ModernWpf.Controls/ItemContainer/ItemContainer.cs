// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using ModernWpf.Automation.Peers;

namespace ModernWpf.Controls
{
    [ContentProperty(nameof(Child))]
    public partial class ItemContainer : Control
    {
        static ItemContainer()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(ItemContainer),
                new FrameworkPropertyMetadata(typeof(ItemContainer)));
            IsEnabledProperty.OverrideMetadata(
                typeof(ItemContainer),
                new FrameworkPropertyMetadata(OnIsEnabledChanged));
        }

        public ItemContainer()
        {
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            UpdateVisualState(false);
            UpdateMultiSelectState(false);
        }

        internal ItemContainerUserSelectMode CanUserSelectInternal
        {
            get => (ItemContainerUserSelectMode)GetValue(CanUserSelectInternalProperty);
            set => SetValue(CanUserSelectInternalProperty, value);
        }

        internal ItemContainerUserInvokeMode CanUserInvokeInternal
        {
            get => (ItemContainerUserInvokeMode)GetValue(CanUserInvokeInternalProperty);
            set => SetValue(CanUserInvokeInternalProperty, value);
        }

        internal ItemContainerMultiSelectMode MultiSelectModeInternal
        {
            get => (ItemContainerMultiSelectMode)GetValue(MultiSelectModeInternalProperty);
            set => SetValue(MultiSelectModeInternalProperty, value);
        }

        internal event TypedEventHandler<ItemContainer, ItemContainerInvokedEventArgs> ItemInvoked;

        internal bool CanRaiseItemInvoked => CanUserInvoke || CanUserSelect;

        internal bool CanUserSelect =>
            (CanUserSelectInternal & (ItemContainerUserSelectMode.Auto | ItemContainerUserSelectMode.UserCanSelect)) != 0;

        internal bool CanUserInvoke =>
            (CanUserInvokeInternal & ItemContainerUserInvokeMode.UserCanInvoke) != 0;

        internal bool RaiseItemInvoked(ItemContainerInteractionTrigger interactionTrigger, object originalSource)
        {
            var handler = ItemInvoked;
            if (handler == null)
            {
                return false;
            }

            var args = new ItemContainerInvokedEventArgs(interactionTrigger, originalSource);
            handler(this, args);
            return args.Handled;
        }

        protected override AutomationPeer OnCreateAutomationPeer()
        {
            return new ItemContainerAutomationPeer(this);
        }

        protected override void OnGotKeyboardFocus(KeyboardFocusChangedEventArgs e)
        {
            base.OnGotKeyboardFocus(e);
            UpdateVisualState();
        }

        protected override void OnLostKeyboardFocus(KeyboardFocusChangedEventArgs e)
        {
            base.OnLostKeyboardFocus(e);
            UpdateVisualState();
        }

        protected override void OnLostMouseCapture(MouseEventArgs e)
        {
            base.OnLostMouseCapture(e);

            if (_isPressed)
            {
                _isPressed = false;
                UpdateVisualState();
            }
        }

        protected override void OnMouseEnter(MouseEventArgs e)
        {
            base.OnMouseEnter(e);
            UpdateVisualState();
        }

        protected override void OnMouseLeave(MouseEventArgs e)
        {
            base.OnMouseLeave(e);
            UpdateVisualState();
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);

            if (!e.Handled && IsEnabled && CanRaiseItemInvoked)
            {
                Focus();
                _isPressed = true;
                CaptureMouse();
                e.Handled = RaiseItemInvoked(ItemContainerInteractionTrigger.PointerPressed, e.OriginalSource);
                UpdateVisualState();
            }
        }

        protected override void OnMouseDoubleClick(MouseButtonEventArgs e)
        {
            base.OnMouseDoubleClick(e);

            if (!e.Handled && IsEnabled && CanRaiseItemInvoked)
            {
                e.Handled = RaiseItemInvoked(ItemContainerInteractionTrigger.DoubleTap, e.OriginalSource);
            }
        }

        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonUp(e);

            if (!_isPressed)
            {
                return;
            }

            var shouldInvoke = IsMouseOver;
            _isPressed = false;
            if (IsMouseCaptured)
            {
                ReleaseMouseCapture();
            }

            if (shouldInvoke && CanRaiseItemInvoked)
            {
                e.Handled = RaiseItemInvoked(ItemContainerInteractionTrigger.PointerReleased, e.OriginalSource);
            }

            UpdateVisualState();
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            if (e.Handled || !IsEnabled || !CanRaiseItemInvoked)
            {
                return;
            }

            if (e.Key == Key.Enter)
            {
                e.Handled = RaiseItemInvoked(ItemContainerInteractionTrigger.EnterKey, e.OriginalSource);
            }
            else if (e.Key == Key.Space)
            {
                e.Handled = RaiseItemInvoked(ItemContainerInteractionTrigger.SpaceKey, e.OriginalSource);
            }
        }

        private static void OnIsSelectedPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var item = (ItemContainer)d;
            item.UpdateVisualState();
            item.UpdateMultiSelectState();

            if (UIElementAutomationPeer.FromElement(item) is ItemContainerAutomationPeer peer)
            {
                peer.RaisePropertyChangedEvent(
                    SelectionItemPatternIdentifiers.IsSelectedProperty,
                    e.OldValue,
                    e.NewValue);

                var isSelected = (bool)e.NewValue;
                var isMultiple = (item.MultiSelectModeInternal &
                    (ItemContainerMultiSelectMode.Multiple | ItemContainerMultiSelectMode.Extended)) != 0;
                if (isSelected)
                {
                    peer.RaiseAutomationEvent(
                        isMultiple
                            ? AutomationEvents.SelectionItemPatternOnElementAddedToSelection
                            : AutomationEvents.SelectionItemPatternOnElementSelected);
                }
                else if (isMultiple)
                {
                    peer.RaiseAutomationEvent(AutomationEvents.SelectionItemPatternOnElementRemovedFromSelection);
                }
            }
        }

        private static void OnInternalSelectionPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var item = (ItemContainer)d;
            item.UpdateVisualState();
            item.UpdateMultiSelectState();
        }

        private static void OnIsEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var item = (ItemContainer)d;
            if (!item.IsEnabled)
            {
                item._isPressed = false;
                if (item.IsMouseCaptured)
                {
                    item.ReleaseMouseCapture();
                }
            }

            item.UpdateVisualState();
        }

        private void UpdateVisualState(bool useTransitions = true)
        {
            var interactionState = !IsEnabled
                ? "Normal"
                : _isPressed
                ? "Pressed"
                : IsMouseOver
                    ? "PointerOver"
                    : "Normal";
            VisualStateManager.GoToState(
                this,
                (IsSelected ? "Selected" : "Unselected") + interactionState,
                useTransitions);
            VisualStateManager.GoToState(this, IsEnabled ? "Enabled" : "Disabled", useTransitions);
        }

        private void UpdateMultiSelectState(bool useTransitions = true)
        {
            var isMultiple = (MultiSelectModeInternal & ItemContainerMultiSelectMode.Multiple) != 0;
            VisualStateManager.GoToState(this, isMultiple ? "Multiple" : "Single", useTransitions);
        }

        internal static readonly DependencyProperty CanUserSelectInternalProperty =
            DependencyProperty.Register(
                nameof(CanUserSelectInternal),
                typeof(ItemContainerUserSelectMode),
                typeof(ItemContainer),
                new FrameworkPropertyMetadata(ItemContainerUserSelectMode.Auto, OnInternalSelectionPropertyChanged));

        internal static readonly DependencyProperty CanUserInvokeInternalProperty =
            DependencyProperty.Register(
                nameof(CanUserInvokeInternal),
                typeof(ItemContainerUserInvokeMode),
                typeof(ItemContainer),
                new FrameworkPropertyMetadata(ItemContainerUserInvokeMode.Auto));

        internal static readonly DependencyProperty MultiSelectModeInternalProperty =
            DependencyProperty.Register(
                nameof(MultiSelectModeInternal),
                typeof(ItemContainerMultiSelectMode),
                typeof(ItemContainer),
                new FrameworkPropertyMetadata(ItemContainerMultiSelectMode.Auto, OnInternalSelectionPropertyChanged));

        private bool _isPressed;
    }

    [Flags]
    internal enum ItemContainerMultiSelectMode
    {
        Auto = 1,
        Single = 2,
        Extended = 4,
        Multiple = 8
    }

    internal enum ItemContainerInteractionTrigger
    {
        PointerPressed,
        PointerReleased,
        Tap,
        DoubleTap,
        EnterKey,
        SpaceKey,
        AutomationInvoke
    }

    [Flags]
    internal enum ItemContainerUserInvokeMode
    {
        Auto = 1,
        UserCanInvoke = 2,
        UserCannotInvoke = 4
    }

    [Flags]
    internal enum ItemContainerUserSelectMode
    {
        Auto = 1,
        UserCanSelect = 2,
        UserCannotSelect = 4
    }

    internal sealed class ItemContainerInvokedEventArgs : EventArgs
    {
        internal ItemContainerInvokedEventArgs(
            ItemContainerInteractionTrigger interactionTrigger,
            object originalSource)
        {
            InteractionTrigger = interactionTrigger;
            OriginalSource = originalSource;
        }

        public object OriginalSource { get; }

        public ItemContainerInteractionTrigger InteractionTrigger { get; }

        public bool Handled { get; set; }
    }
}
