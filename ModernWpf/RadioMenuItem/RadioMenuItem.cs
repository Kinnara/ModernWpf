// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Threading;
using System;
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

        public RadioMenuItem()
        {
            if (s_selectionMap.Value is null)
            {
                // Ensure that this object exists
                s_selectionMap.Value = new Dictionary<string, WeakReference<RadioMenuItem>>();
            }
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
            ((RadioMenuItem)d).UpdateCheckedItemInGroup();
        }

        public static readonly DependencyProperty UseSystemFocusVisualsProperty =
            FocusVisualHelper.UseSystemFocusVisualsProperty.AddOwner(typeof(RadioMenuItem));

        public bool UseSystemFocusVisuals
        {
            get => (bool)GetValue(UseSystemFocusVisualsProperty);
            set => SetValue(UseSystemFocusVisualsProperty, value);
        }

        #region CornerRadius

        public static readonly DependencyProperty CornerRadiusProperty =
            ControlHelper.CornerRadiusProperty.AddOwner(typeof(RadioMenuItem));

        public CornerRadius CornerRadius
        {
            get => (CornerRadius)GetValue(CornerRadiusProperty);
            set => SetValue(CornerRadiusProperty, value);
        }

        #endregion

        #region AreCheckStatesEnabled

        public static readonly DependencyProperty AreCheckStatesEnabledProperty =
            DependencyProperty.RegisterAttached(
                "AreCheckStatesEnabled",
                typeof(bool),
                typeof(RadioMenuItem),
                new PropertyMetadata(false, OnAreCheckStatesEnabledPropertyChanged));

        public static bool GetAreCheckStatesEnabled(MenuItem target)
        {
            return (bool)target.GetValue(AreCheckStatesEnabledProperty);
        }

        public static void SetAreCheckStatesEnabled(MenuItem target, bool value)
        {
            target.SetValue(AreCheckStatesEnabledProperty, value);
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
        }

        private void UpdateCheckedItemInGroup()
        {
            if (IsChecked)
            {
                var groupName = GroupName;

                if (s_selectionMap.Value.TryGetValue(groupName, out var previousCheckedItemWeak))
                {
                    if (previousCheckedItemWeak.TryGetTarget(out var previousCheckedItem))
                    {
                        // Uncheck the previously checked item
                        previousCheckedItem.IsChecked = false;
                    }
                }
                s_selectionMap.Value[groupName] = new(this);
            }
        }

        static void OnAreCheckStatesEnabledPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            if ((bool)args.NewValue)
            {
                if (sender is MenuItem subMenu)
                {
                    // Every time the submenu is loaded, see if it contains a checked RadioMenuFlyoutItem or not.
                    subMenu.Loaded += (s, e) =>
                    {
                        bool isAnyItemChecked = false;
                        foreach (var item in subMenu.Items)
                        {
                            if (item is RadioMenuItem radioItem)
                            {
                                isAnyItemChecked = isAnyItemChecked || radioItem.IsChecked;
                            }
                        }

                        VisualStateManager.GoToState(subMenu, isAnyItemChecked ? "Checked" : "Unchecked", false);
                    };
                }
            }
        }

        private bool m_isSafeUncheck;
        private bool m_surpressOnChecked;

        static readonly ThreadLocal<Dictionary<string, WeakReference<RadioMenuItem>>> s_selectionMap = new();
    }
}
