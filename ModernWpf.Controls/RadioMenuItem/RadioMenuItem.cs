// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using ModernWpf.Controls.Primitives;

namespace ModernWpf.Controls
{
    public class RadioMenuItem : MenuItem
    {
        static RadioMenuItem()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(RadioMenuItem), new FrameworkPropertyMetadata(typeof(RadioMenuItem)));
            IsCheckableProperty.OverrideMetadata(typeof(RadioMenuItem), new FrameworkPropertyMetadata(true, null, CoerceIsCheckable));
        }

        private static object CoerceIsCheckable(DependencyObject d, object baseValue)
        {
            return true;
        }

        public static readonly DependencyProperty GroupNameProperty =
            DependencyProperty.Register(
                nameof(GroupName),
                typeof(string),
                typeof(RadioMenuItem),
                new FrameworkPropertyMetadata(string.Empty, OnGroupNameChanged));

        public string GroupName
        {
            get => (string)GetValue(GroupNameProperty);
            set => SetValue(GroupNameProperty, value);
        }

        private static void OnGroupNameChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var radioItem = (RadioMenuItem)d;
            if (radioItem.IsChecked)
            {
                radioItem.RemoveCheckedItemFromGroup((string)e.OldValue);
                radioItem.UpdateCheckedItemInGroup();
            }
        }

        public static readonly DependencyProperty UseSystemFocusVisualsProperty =
            FocusVisualHelper.UseSystemFocusVisualsProperty.AddOwner(typeof(RadioMenuItem));

        public bool UseSystemFocusVisuals
        {
            get => (bool)GetValue(UseSystemFocusVisualsProperty);
            set => SetValue(UseSystemFocusVisualsProperty, value);
        }

        #region AreCheckStatesEnabled

        public static readonly DependencyProperty AreCheckStatesEnabledProperty =
            DependencyProperty.RegisterAttached(
                "AreCheckStatesEnabled",
                typeof(bool),
                typeof(RadioMenuItem),
                new PropertyMetadata(false, OnAreCheckStatesEnabledChanged));

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
                    menuItem.Loaded += OnSubMenuLoaded;
                    menuItem.Unloaded += OnSubMenuUnloaded;
                    HookSubMenu(menuItem);
                }
                else
                {
                    menuItem.Loaded -= OnSubMenuLoaded;
                    menuItem.Unloaded -= OnSubMenuUnloaded;
                    UnhookSubMenu(menuItem);
                    menuItem.SetCurrentValue(IsCheckedProperty, false);
                }
            }
        }

        #endregion

        #region CornerRadius

        public static readonly DependencyProperty CornerRadiusProperty =
            ControlHelper.CornerRadiusProperty.AddOwner(typeof(RadioMenuItem));

        public CornerRadius CornerRadius
        {
            get => (CornerRadius)GetValue(CornerRadiusProperty);
            set => SetValue(CornerRadiusProperty, value);
        }

        #endregion

        protected override void OnChecked(RoutedEventArgs e)
        {
            if (m_surpressOnChecked)
            {
                e.Handled = true;
                return;
            }

            UpdateCheckedItemInGroup();

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

        private static readonly DependencyProperty HookedRadioItemsProperty =
            DependencyProperty.RegisterAttached(
                "HookedRadioItems",
                typeof(List<RadioMenuItem>),
                typeof(RadioMenuItem),
                new PropertyMetadata(null));

        private static List<RadioMenuItem> GetHookedRadioItems(DependencyObject element)
        {
            return (List<RadioMenuItem>)element.GetValue(HookedRadioItemsProperty);
        }

        private static void SetHookedRadioItems(DependencyObject element, List<RadioMenuItem> value)
        {
            element.SetValue(HookedRadioItemsProperty, value);
        }

        private static readonly DependencyProperty CollectionChangedHandlerProperty =
            DependencyProperty.RegisterAttached(
                "CollectionChangedHandler",
                typeof(NotifyCollectionChangedEventHandler),
                typeof(RadioMenuItem),
                new PropertyMetadata(null));

        private static NotifyCollectionChangedEventHandler GetCollectionChangedHandler(DependencyObject element)
        {
            return (NotifyCollectionChangedEventHandler)element.GetValue(CollectionChangedHandlerProperty);
        }

        private static void SetCollectionChangedHandler(DependencyObject element, NotifyCollectionChangedEventHandler value)
        {
            element.SetValue(CollectionChangedHandlerProperty, value);
        }

        private static readonly Dictionary<string, WeakReference<RadioMenuItem>> s_selectionMap = new Dictionary<string, WeakReference<RadioMenuItem>>();

        private bool m_isSafeUncheck;
        private bool m_surpressOnChecked;
    }
}
