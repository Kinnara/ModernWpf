// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

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
            ((RadioMenuItem)d).UpdateSiblings();
        }

        public static readonly DependencyProperty UseSystemFocusVisualsProperty =
            FocusVisualHelper.UseSystemFocusVisualsProperty.AddOwner(typeof(RadioMenuItem));

        public bool UseSystemFocusVisuals
        {
            get => (bool)GetValue(UseSystemFocusVisualsProperty);
            set => SetValue(UseSystemFocusVisualsProperty, value);
        }

        protected override void OnChecked(RoutedEventArgs e)
        {
            if (m_surpressOnChecked)
            {
                e.Handled = true;
                return;
            }

            UpdateSiblings();

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

        private void UpdateSiblings()
        {
            if (IsChecked)
            {
                // Since this item is checked, uncheck all siblings
                if (ItemsControlFromItemContainer(this) is { } parent)
                {
                    int childrenCount = parent.Items.Count;
                    for (int i = 0; i < childrenCount; i++)
                    {
                        var child = parent.Items[i];
                        if (child is RadioMenuItem radioItem)
                        {
                            if (radioItem != this
                                && radioItem.GroupName == GroupName)
                            {
                                radioItem.m_isSafeUncheck = true;
                                radioItem.SetCurrentValue(IsCheckedProperty, false);
                                radioItem.m_isSafeUncheck = false;
                            }
                        }
                    }
                }
            }
        }

        private bool m_isSafeUncheck;
        private bool m_surpressOnChecked;
    }
}
