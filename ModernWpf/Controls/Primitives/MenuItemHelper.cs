using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace ModernWpf.Controls.Primitives
{
    public static class MenuItemHelper
    {
        #region VisualStateSettersEnabled

        public static bool GetVisualStateSettersEnabled(MenuItem menuItem)
        {
            return (bool)menuItem.GetValue(VisualStateSettersEnabledProperty);
        }

        public static void SetVisualStateSettersEnabled(MenuItem menuItem, bool value)
        {
            menuItem.SetValue(VisualStateSettersEnabledProperty, value);
        }

        public static readonly DependencyProperty VisualStateSettersEnabledProperty =
            DependencyProperty.RegisterAttached(
                "VisualStateSettersEnabled",
                typeof(bool),
                typeof(MenuItemHelper),
                new PropertyMetadata(false, OnVisualStateSettersEnabledChanged));

        private static void OnVisualStateSettersEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is MenuItem menuItem)
            {
                if ((bool)e.NewValue)
                {
                    GetOrCreateVisualStateTracker(menuItem).Attach();
                }
                else
                {
                    GetVisualStateTracker(menuItem)?.Detach();
                }
            }
        }

        #endregion

        private static VisualStateTracker GetOrCreateVisualStateTracker(MenuItem menuItem)
        {
            var tracker = GetVisualStateTracker(menuItem);
            if (tracker == null)
            {
                tracker = new VisualStateTracker(menuItem);
                menuItem.SetValue(VisualStateTrackerProperty, tracker);
            }

            return tracker;
        }

        private static VisualStateTracker GetVisualStateTracker(MenuItem menuItem)
        {
            return (VisualStateTracker)menuItem.GetValue(VisualStateTrackerProperty);
        }

        private static readonly DependencyProperty VisualStateTrackerProperty =
            DependencyProperty.RegisterAttached(
                "VisualStateTracker",
                typeof(VisualStateTracker),
                typeof(MenuItemHelper));

        private sealed class VisualStateTracker
        {
            private static readonly DependencyPropertyDescriptor IsHighlightedPropertyDescriptor =
                DependencyPropertyDescriptor.FromProperty(MenuItem.IsHighlightedProperty, typeof(MenuItem));

            private static readonly DependencyPropertyDescriptor IsPressedPropertyDescriptor =
                DependencyPropertyDescriptor.FromProperty(MenuItem.IsPressedProperty, typeof(MenuItem));

            private static readonly DependencyPropertyDescriptor IsSubmenuOpenPropertyDescriptor =
                DependencyPropertyDescriptor.FromProperty(MenuItem.IsSubmenuOpenProperty, typeof(MenuItem));

            private static readonly DependencyPropertyDescriptor IsEnabledPropertyDescriptor =
                DependencyPropertyDescriptor.FromProperty(UIElement.IsEnabledProperty, typeof(MenuItem));

            private static readonly DependencyPropertyDescriptor IsCheckedPropertyDescriptor =
                DependencyPropertyDescriptor.FromProperty(MenuItem.IsCheckedProperty, typeof(MenuItem));

            private static readonly DependencyPropertyDescriptor InputGestureTextPropertyDescriptor =
                DependencyPropertyDescriptor.FromProperty(MenuItem.InputGestureTextProperty, typeof(MenuItem));

            public VisualStateTracker(MenuItem menuItem)
            {
                _menuItem = menuItem;
            }

            public void Attach()
            {
                if (_isAttached)
                {
                    return;
                }

                _isAttached = true;
                _menuItem.Loaded += OnLoaded;
                AttachPropertyListeners();
                UpdateVisualStates(false);
            }

            public void Detach()
            {
                if (!_isAttached)
                {
                    return;
                }

                DetachPropertyListeners();
                _menuItem.Loaded -= OnLoaded;
                _isAttached = false;
            }

            private void OnLoaded(object sender, RoutedEventArgs e)
            {
                UpdateVisualStates(false);
            }

            private void AttachPropertyListeners()
            {
                IsHighlightedPropertyDescriptor.AddValueChanged(_menuItem, OnVisualStatePropertyChanged);
                IsPressedPropertyDescriptor.AddValueChanged(_menuItem, OnVisualStatePropertyChanged);
                IsSubmenuOpenPropertyDescriptor.AddValueChanged(_menuItem, OnVisualStatePropertyChanged);
                IsEnabledPropertyDescriptor.AddValueChanged(_menuItem, OnVisualStatePropertyChanged);
                IsCheckedPropertyDescriptor.AddValueChanged(_menuItem, OnVisualStatePropertyChanged);
                InputGestureTextPropertyDescriptor.AddValueChanged(_menuItem, OnVisualStatePropertyChanged);
            }

            private void DetachPropertyListeners()
            {
                InputGestureTextPropertyDescriptor.RemoveValueChanged(_menuItem, OnVisualStatePropertyChanged);
                IsCheckedPropertyDescriptor.RemoveValueChanged(_menuItem, OnVisualStatePropertyChanged);
                IsEnabledPropertyDescriptor.RemoveValueChanged(_menuItem, OnVisualStatePropertyChanged);
                IsSubmenuOpenPropertyDescriptor.RemoveValueChanged(_menuItem, OnVisualStatePropertyChanged);
                IsPressedPropertyDescriptor.RemoveValueChanged(_menuItem, OnVisualStatePropertyChanged);
                IsHighlightedPropertyDescriptor.RemoveValueChanged(_menuItem, OnVisualStatePropertyChanged);
            }

            private void OnVisualStatePropertyChanged(object sender, EventArgs e)
            {
                UpdateVisualStates(true);
            }

            private void UpdateVisualStates(bool useTransitions)
            {
                VisualStateManager.GoToState(_menuItem, GetCommonStateName(), useTransitions);
                VisualStateManager.GoToState(_menuItem, _menuItem.IsChecked ? "Checked" : "Unchecked", useTransitions);
                VisualStateManager.GoToState(
                    _menuItem,
                    string.IsNullOrEmpty(_menuItem.InputGestureText) ? "KeyboardAcceleratorTextCollapsed" : "KeyboardAcceleratorTextVisible",
                    useTransitions);
            }

            private string GetCommonStateName()
            {
                if (!_menuItem.IsEnabled)
                {
                    return "Disabled";
                }

                if (_menuItem.IsSubmenuOpen)
                {
                    return "SubMenuOpened";
                }

                if (_menuItem.IsPressed)
                {
                    return "Pressed";
                }

                if (_menuItem.IsHighlighted)
                {
                    return "PointerOver";
                }

                return "Normal";
            }

            private readonly MenuItem _menuItem;
            private bool _isAttached;
        }
    }
}
