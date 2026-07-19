// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using ModernWpf.Controls.Primitives;

namespace ModernWpf.Controls
{
    public partial class RadioMenuItem : MenuItem
    {
        static RadioMenuItem()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(RadioMenuItem), new FrameworkPropertyMetadata(typeof(RadioMenuItem)));
            IsCheckableProperty.OverrideMetadata(typeof(RadioMenuItem), new FrameworkPropertyMetadata(true, null, CoerceIsCheckable));
        }

        public RadioMenuItem()
        {
            Loaded += OnLoaded;
            Unloaded += OnUnloaded;
        }

        private static object CoerceIsCheckable(DependencyObject d, object baseValue)
        {
            return true;
        }

        private static void OnGroupNameChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var radioItem = (RadioMenuItem)d;
            radioItem.m_groupName = (string)e.NewValue ?? string.Empty;
        }

        #region AreCheckStatesEnabled

        public static bool GetAreCheckStatesEnabled(MenuItem element)
        {
            if (element == null)
            {
                throw new ArgumentNullException(nameof(element));
            }

            return (bool)element.GetValue(AreCheckStatesEnabledProperty);
        }

        public static void SetAreCheckStatesEnabled(MenuItem element, bool value)
        {
            if (element == null)
            {
                throw new ArgumentNullException(nameof(element));
            }

            element.SetValue(AreCheckStatesEnabledProperty, value);
        }

        private static void OnAreCheckStatesEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is MenuItem menuItem)
            {
                if ((bool)e.NewValue)
                {
                    // WPF MenuItem has no MenuFlyoutSubItem style selector.
                    // IsCheckable selects the radio-submenu placeholder in the
                    // shared WPF template while IsChecked drives its opacity.
                    menuItem.SetCurrentValue(IsCheckableProperty, true);
                    menuItem.Loaded += OnSubMenuLoaded;
                    menuItem.Unloaded += OnSubMenuUnloaded;
                    HookSubMenu(menuItem);
                }
                else
                {
                    menuItem.Loaded -= OnSubMenuLoaded;
                    menuItem.Unloaded -= OnSubMenuUnloaded;
                    UnhookSubMenu(menuItem);
                    menuItem.SetCurrentValue(IsCheckableProperty, false);
                    menuItem.SetCurrentValue(IsCheckedProperty, false);
                }
            }
        }

        #endregion

        #region CornerRadius

        #endregion

        protected override void OnChecked(RoutedEventArgs e)
        {
            if (m_surpressOnChecked)
            {
                e.Handled = true;
                return;
            }

            UpdateCheckedItemInGroup();
            UpdateVisualStates(true);
            m_isChecked = true;

            base.OnChecked(e);
        }

        protected override void OnUnchecked(RoutedEventArgs e)
        {
            if (!m_isSafeUncheck)
            {
                m_surpressOnChecked = true;
                SetCurrentValue(IsCheckedProperty, true);
                m_surpressOnChecked = false;
                e.Handled = true;
                return;
            }

            base.OnUnchecked(e);
            RemoveCheckedItemFromGroup(GroupName);
            UpdateVisualStates(true);
            m_isChecked = false;
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            UpdateVisualStates(false);
        }

        private void UpdateCheckedItemInGroup()
        {
            if (IsChecked)
            {
                string groupName = GroupName ?? string.Empty;
                if (s_selectionMap.TryGetValue(groupName, out var previousCheckedItemWeak) &&
                    previousCheckedItemWeak.TryGetTarget(out var previousCheckedItem) &&
                    !ReferenceEquals(previousCheckedItem, this))
                {
                    previousCheckedItem.m_isSafeUncheck = true;
                    try
                    {
                        previousCheckedItem.SetCurrentValue(IsCheckedProperty, false);
                    }
                    finally
                    {
                        previousCheckedItem.m_isSafeUncheck = false;
                    }
                }

                s_selectionMap[groupName] = new WeakReference<RadioMenuItem>(this);
            }
        }

        private void RemoveCheckedItemFromGroup(string groupName)
        {
            groupName ??= string.Empty;

            if (s_selectionMap.TryGetValue(groupName, out var checkedItemWeak) &&
                (!checkedItemWeak.TryGetTarget(out var checkedItem) || ReferenceEquals(checkedItem, this)))
            {
                s_selectionMap.Remove(groupName);
            }
        }

        private static void OnSubMenuLoaded(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem menuItem)
            {
                HookSubMenu(menuItem);
                UpdateSubMenuCheckState(menuItem);
            }
        }

        private static void OnSubMenuUnloaded(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem menuItem)
            {
                UnhookSubMenu(menuItem);
                menuItem.SetCurrentValue(IsCheckedProperty, false);
            }
        }

        private static void HookSubMenu(MenuItem menuItem)
        {
            UnhookSubMenu(menuItem);

            var hookedItems = new List<RadioMenuItem>();
            foreach (var radioItem in menuItem.Items.OfType<RadioMenuItem>())
            {
                radioItem.Checked += OnSubMenuRadioItemCheckedChanged;
                radioItem.Unchecked += OnSubMenuRadioItemCheckedChanged;
                hookedItems.Add(radioItem);
            }

            SetHookedRadioItems(menuItem, hookedItems);

            if (menuItem.Items is INotifyCollectionChanged notifyCollectionChanged)
            {
                NotifyCollectionChangedEventHandler handler = (sender, args) =>
                {
                    HookSubMenu(menuItem);
                    UpdateSubMenuCheckState(menuItem);
                };

                notifyCollectionChanged.CollectionChanged += handler;
                SetCollectionChangedHandler(menuItem, handler);
            }

            UpdateSubMenuCheckState(menuItem);
        }

        private static void UnhookSubMenu(MenuItem menuItem)
        {
            if (GetHookedRadioItems(menuItem) is List<RadioMenuItem> hookedItems)
            {
                foreach (var radioItem in hookedItems)
                {
                    radioItem.Checked -= OnSubMenuRadioItemCheckedChanged;
                    radioItem.Unchecked -= OnSubMenuRadioItemCheckedChanged;
                }

                menuItem.ClearValue(HookedRadioItemsProperty);
            }

            if (GetCollectionChangedHandler(menuItem) is NotifyCollectionChangedEventHandler handler &&
                menuItem.Items is INotifyCollectionChanged notifyCollectionChanged)
            {
                notifyCollectionChanged.CollectionChanged -= handler;
                menuItem.ClearValue(CollectionChangedHandlerProperty);
            }
        }

        private static void OnSubMenuRadioItemCheckedChanged(object sender, RoutedEventArgs e)
        {
            if (sender is RadioMenuItem radioItem &&
                ItemsControl.ItemsControlFromItemContainer(radioItem) is MenuItem menuItem &&
                GetAreCheckStatesEnabled(menuItem))
            {
                UpdateSubMenuCheckState(menuItem);
            }
        }

        private static void UpdateSubMenuCheckState(MenuItem menuItem)
        {
            bool isAnyItemChecked = menuItem.Items
                .OfType<RadioMenuItem>()
                .Any(item => item.IsChecked);

            menuItem.SetCurrentValue(IsCheckedProperty, isAnyItemChecked);
        }

        private static List<RadioMenuItem> GetHookedRadioItems(DependencyObject element)
        {
            return (List<RadioMenuItem>)element.GetValue(HookedRadioItemsProperty);
        }

        private static void SetHookedRadioItems(DependencyObject element, List<RadioMenuItem> value)
        {
            element.SetValue(HookedRadioItemsProperty, value);
        }

        private static NotifyCollectionChangedEventHandler GetCollectionChangedHandler(DependencyObject element)
        {
            return (NotifyCollectionChangedEventHandler)element.GetValue(CollectionChangedHandlerProperty);
        }

        private static void SetCollectionChangedHandler(DependencyObject element, NotifyCollectionChangedEventHandler value)
        {
            element.SetValue(CollectionChangedHandlerProperty, value);
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            m_groupName = GroupName ?? string.Empty;
            UpdateCheckedItemInGroup();
            m_isChecked = IsChecked;
            AttachVisualStatePropertyListeners();
            UpdateVisualStates(false);
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            if (m_isChecked)
            {
                RemoveCheckedItemFromGroup(m_groupName);
            }

            DetachVisualStatePropertyListeners();
        }

        private void AttachVisualStatePropertyListeners()
        {
            if (m_areVisualStatePropertyListenersAttached)
            {
                return;
            }

            m_areVisualStatePropertyListenersAttached = true;
            IsHighlightedPropertyDescriptor.AddValueChanged(this, OnVisualStatePropertyChanged);
            IsPressedPropertyDescriptor.AddValueChanged(this, OnVisualStatePropertyChanged);
            IsEnabledPropertyDescriptor.AddValueChanged(this, OnVisualStatePropertyChanged);
            IconPropertyDescriptor.AddValueChanged(this, OnVisualStatePropertyChanged);
            InputGestureTextPropertyDescriptor.AddValueChanged(this, OnVisualStatePropertyChanged);
        }

        private void DetachVisualStatePropertyListeners()
        {
            if (!m_areVisualStatePropertyListenersAttached)
            {
                return;
            }

            InputGestureTextPropertyDescriptor.RemoveValueChanged(this, OnVisualStatePropertyChanged);
            IconPropertyDescriptor.RemoveValueChanged(this, OnVisualStatePropertyChanged);
            IsEnabledPropertyDescriptor.RemoveValueChanged(this, OnVisualStatePropertyChanged);
            IsPressedPropertyDescriptor.RemoveValueChanged(this, OnVisualStatePropertyChanged);
            IsHighlightedPropertyDescriptor.RemoveValueChanged(this, OnVisualStatePropertyChanged);
            m_areVisualStatePropertyListenersAttached = false;
        }

        private void OnVisualStatePropertyChanged(object sender, EventArgs e)
        {
            UpdateVisualStates(true);
        }

        private void UpdateVisualStates(bool useTransitions)
        {
            string commonStateName;
            if (!IsEnabled)
            {
                commonStateName = "Disabled";
            }
            else if (IsPressed)
            {
                commonStateName = "Pressed";
            }
            else if (IsHighlighted)
            {
                commonStateName = "PointerOver";
            }
            else
            {
                commonStateName = "Normal";
            }

            VisualStateManager.GoToState(this, commonStateName, useTransitions);

            bool hasIcon = Icon != null;
            string checkStateName = IsChecked
                ? (hasIcon ? "CheckedWithIcon" : "Checked")
                : (hasIcon ? "UncheckedWithIcon" : "Unchecked");
            VisualStateManager.GoToState(this, checkStateName, useTransitions);

            string keyboardAcceleratorTextStateName = string.IsNullOrEmpty(InputGestureText)
                ? "KeyboardAcceleratorTextCollapsed"
                : "KeyboardAcceleratorTextVisible";
            VisualStateManager.GoToState(this, keyboardAcceleratorTextStateName, useTransitions);
        }

        private static readonly DependencyPropertyDescriptor IsHighlightedPropertyDescriptor =
            DependencyPropertyDescriptor.FromProperty(IsHighlightedProperty, typeof(RadioMenuItem));

        private static readonly DependencyPropertyDescriptor IsPressedPropertyDescriptor =
            DependencyPropertyDescriptor.FromProperty(IsPressedProperty, typeof(RadioMenuItem));

        private static readonly DependencyPropertyDescriptor IsEnabledPropertyDescriptor =
            DependencyPropertyDescriptor.FromProperty(IsEnabledProperty, typeof(RadioMenuItem));

        private static readonly DependencyPropertyDescriptor IconPropertyDescriptor =
            DependencyPropertyDescriptor.FromProperty(IconProperty, typeof(RadioMenuItem));

        private static readonly DependencyPropertyDescriptor InputGestureTextPropertyDescriptor =
            DependencyPropertyDescriptor.FromProperty(InputGestureTextProperty, typeof(RadioMenuItem));

        private static readonly Dictionary<string, WeakReference<RadioMenuItem>> s_selectionMap = new Dictionary<string, WeakReference<RadioMenuItem>>();

        private bool m_isSafeUncheck;
        private bool m_surpressOnChecked;
        private bool m_areVisualStatePropertyListenersAttached;
        private bool m_isChecked;
        private string m_groupName = string.Empty;
    }
}
