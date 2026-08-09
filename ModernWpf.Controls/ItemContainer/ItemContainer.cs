// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using ModernWpf.Automation.Peers;

namespace ModernWpf.Controls
{
    [ContentProperty(nameof(Child))]
    [TemplatePart(Name = SelectionCheckBoxPartName, Type = typeof(CheckBox))]
    public class ItemContainer : Control
    {
        internal const string SelectionCheckBoxPartName = "PART_SelectionCheckbox";

        private CheckBox _selectionCheckBox;

        static ItemContainer()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(ItemContainer),
                new FrameworkPropertyMetadata(typeof(ItemContainer)));
            FocusableProperty.OverrideMetadata(
                typeof(ItemContainer),
                new FrameworkPropertyMetadata(true));
        }

        public static readonly DependencyProperty ChildProperty =
            DependencyProperty.Register(
                nameof(Child),
                typeof(UIElement),
                typeof(ItemContainer),
                new FrameworkPropertyMetadata(null));

        public UIElement Child
        {
            get => (UIElement)GetValue(ChildProperty);
            set => SetValue(ChildProperty, value);
        }

        public static readonly DependencyProperty IsSelectedProperty =
            DependencyProperty.Register(
                nameof(IsSelected),
                typeof(bool),
                typeof(ItemContainer),
                new FrameworkPropertyMetadata(false, OnIsSelectedChanged));

        public bool IsSelected
        {
            get => (bool)GetValue(IsSelectedProperty);
            set => SetValue(IsSelectedProperty, value);
        }

        // WinUI Control exposes CornerRadius. WPF Control does not, so the
        // WPF port owns the equivalent dependency property directly.
        public static readonly DependencyProperty CornerRadiusProperty =
            Border.CornerRadiusProperty.AddOwner(
                typeof(ItemContainer),
                new FrameworkPropertyMetadata(new CornerRadius(4.0)));

        public CornerRadius CornerRadius
        {
            get => (CornerRadius)GetValue(CornerRadiusProperty);
            set => SetValue(CornerRadiusProperty, value);
        }

        internal static readonly DependencyProperty CanUserSelectProperty =
            DependencyProperty.Register(
                nameof(CanUserSelect),
                typeof(ItemContainerUserSelectMode),
                typeof(ItemContainer),
                new FrameworkPropertyMetadata(ItemContainerUserSelectMode.Auto));

        internal ItemContainerUserSelectMode CanUserSelect
        {
            get => (ItemContainerUserSelectMode)GetValue(CanUserSelectProperty);
            set => SetValue(CanUserSelectProperty, value);
        }

        internal static readonly DependencyProperty CanUserInvokeProperty =
            DependencyProperty.Register(
                nameof(CanUserInvoke),
                typeof(ItemContainerUserInvokeMode),
                typeof(ItemContainer),
                new FrameworkPropertyMetadata(ItemContainerUserInvokeMode.Auto));

        internal ItemContainerUserInvokeMode CanUserInvoke
        {
            get => (ItemContainerUserInvokeMode)GetValue(CanUserInvokeProperty);
            set => SetValue(CanUserInvokeProperty, value);
        }

        internal static readonly DependencyProperty MultiSelectModeProperty =
            DependencyProperty.Register(
                nameof(MultiSelectMode),
                typeof(ItemContainerMultiSelectMode),
                typeof(ItemContainer),
                new FrameworkPropertyMetadata(
                    ItemContainerMultiSelectMode.Auto,
                    OnMultiSelectModeChanged));

        internal ItemContainerMultiSelectMode MultiSelectMode
        {
            get => (ItemContainerMultiSelectMode)GetValue(MultiSelectModeProperty);
            set => SetValue(MultiSelectModeProperty, value);
        }

        internal event TypedEventHandler<ItemContainer, ItemContainerInvokedEventArgs> ItemInvoked;

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            _selectionCheckBox = GetTemplateChild(SelectionCheckBoxPartName) as CheckBox;
            UpdateSelectionCheckBox();
        }

        protected override AutomationPeer OnCreateAutomationPeer()
        {
            return new ItemContainerAutomationPeer(this);
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);

            if (e.Handled || !CanRaiseItemInvoked())
            {
                return;
            }

            Focus();
            CaptureMouse();
        }

        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonUp(e);

            bool wasCaptured = IsMouseCaptured;
            if (wasCaptured)
            {
                ReleaseMouseCapture();
            }

            if (!e.Handled && wasCaptured && IsMouseOver && CanRaiseItemInvoked())
            {
                var trigger = e.ClickCount > 1
                    ? ItemContainerInteractionTrigger.DoubleClick
                    : ItemContainerInteractionTrigger.MouseReleased;
                e.Handled = RaiseItemInvoked(trigger, e.OriginalSource);
            }
        }

        protected override void OnLostMouseCapture(MouseEventArgs e)
        {
            base.OnLostMouseCapture(e);
            InvalidateVisual();
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            if (e.Handled || !CanRaiseItemInvoked())
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

        internal bool GetEffectiveCanUserSelect()
        {
            return (CanUserSelect & (ItemContainerUserSelectMode.Auto | ItemContainerUserSelectMode.UserCanSelect)) != 0;
        }

        internal bool GetEffectiveCanUserInvoke()
        {
            return (CanUserInvoke & ItemContainerUserInvokeMode.UserCanInvoke) != 0;
        }

        internal bool RaiseItemInvoked(ItemContainerInteractionTrigger trigger, object originalSource)
        {
            var args = new ItemContainerInvokedEventArgs(trigger, originalSource);
            ItemInvoked?.Invoke(this, args);
            return args.Handled;
        }

        private bool CanRaiseItemInvoked()
        {
            return IsEnabled && (GetEffectiveCanUserSelect() || GetEffectiveCanUserInvoke());
        }

        private static void OnIsSelectedChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            var owner = (ItemContainer)sender;
            owner.UpdateSelectionCheckBox();

            if (UIElementAutomationPeer.FromElement(owner) is ItemContainerAutomationPeer peer)
            {
                peer.RaiseIsSelectedChanged((bool)args.OldValue, (bool)args.NewValue);
            }
        }

        private static void OnMultiSelectModeChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            ((ItemContainer)sender).UpdateSelectionCheckBox();
        }

        private void UpdateSelectionCheckBox()
        {
            if (_selectionCheckBox == null)
            {
                return;
            }

            bool show = (MultiSelectMode & ItemContainerMultiSelectMode.Multiple) != 0;
            _selectionCheckBox.Visibility = show ? Visibility.Visible : Visibility.Collapsed;
            _selectionCheckBox.IsChecked = IsSelected;
        }
    }

    [Flags]
    internal enum ItemContainerMultiSelectMode
    {
        Auto = 1,
        Single = 2,
        Extended = 4,
        Multiple = 8
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

    internal enum ItemContainerInteractionTrigger
    {
        MouseReleased,
        DoubleClick,
        EnterKey,
        SpaceKey,
        AutomationInvoke
    }

    internal sealed class ItemContainerInvokedEventArgs
    {
        internal ItemContainerInvokedEventArgs(ItemContainerInteractionTrigger interactionTrigger, object originalSource)
        {
            InteractionTrigger = interactionTrigger;
            OriginalSource = originalSource;
        }

        internal ItemContainerInteractionTrigger InteractionTrigger { get; }

        internal object OriginalSource { get; }

        internal bool Handled { get; set; }
    }
}
