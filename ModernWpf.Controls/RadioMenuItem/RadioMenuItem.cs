// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
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

        private static readonly Dictionary<string, WeakReference<RadioMenuItem>> s_selectionMap = new Dictionary<string, WeakReference<RadioMenuItem>>();

        private bool m_isSafeUncheck;
        private bool m_surpressOnChecked;
    }
}
