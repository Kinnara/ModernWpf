using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Threading;

namespace ModernWpf.Controls.Primitives
{
    public static class ToggleButtonHelper
    {
        #region VisualStateSettersEnabled

        public static readonly DependencyProperty VisualStateSettersEnabledProperty =
            DependencyProperty.RegisterAttached(
                "VisualStateSettersEnabled",
                typeof(bool),
                typeof(ToggleButtonHelper),
                new PropertyMetadata(false, OnVisualStateSettersEnabledChanged));

        public static bool GetVisualStateSettersEnabled(ToggleButton button)
        {
            return (bool)button.GetValue(VisualStateSettersEnabledProperty);
        }

        public static void SetVisualStateSettersEnabled(ToggleButton button, bool value)
        {
            button.SetValue(VisualStateSettersEnabledProperty, value);
        }

        private static void OnVisualStateSettersEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var button = (ToggleButton)d;
            if ((bool)e.NewValue)
            {
                Attach(button);

                if (button.IsLoaded)
                {
                    UpdateVisualState(button, false);
                }
            }
            else
            {
                Detach(button);
            }
        }

        #endregion

        private static void Attach(ToggleButton button)
        {
            button.Loaded += OnLoaded;
            button.IsEnabledChanged += OnDependencyStateChanged;
            button.Checked += OnCheckedChanged;
            button.Unchecked += OnCheckedChanged;
            button.Indeterminate += OnCheckedChanged;
            button.MouseEnter += OnMouseStateChanged;
            button.MouseLeave += OnMouseStateChanged;
            IsPressedPropertyDescriptor.AddValueChanged(button, OnVisualStatePropertyChanged);
        }

        private static void Detach(ToggleButton button)
        {
            button.Loaded -= OnLoaded;
            button.IsEnabledChanged -= OnDependencyStateChanged;
            button.Checked -= OnCheckedChanged;
            button.Unchecked -= OnCheckedChanged;
            button.Indeterminate -= OnCheckedChanged;
            button.MouseEnter -= OnMouseStateChanged;
            button.MouseLeave -= OnMouseStateChanged;
            IsPressedPropertyDescriptor.RemoveValueChanged(button, OnVisualStatePropertyChanged);
        }

        private static void OnLoaded(object sender, RoutedEventArgs e)
        {
            UpdateVisualState((ToggleButton)sender, false);
        }

        private static void OnDependencyStateChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            ScheduleVisualStateUpdate((ToggleButton)sender);
        }

        private static void OnCheckedChanged(object sender, RoutedEventArgs e)
        {
            ScheduleVisualStateUpdate((ToggleButton)sender);
        }

        private static void OnMouseStateChanged(object sender, RoutedEventArgs e)
        {
            ScheduleVisualStateUpdate((ToggleButton)sender);
        }

        private static void OnVisualStatePropertyChanged(object sender, EventArgs e)
        {
            ScheduleVisualStateUpdate((ToggleButton)sender);
        }

        private static void ScheduleVisualStateUpdate(ToggleButton button)
        {
            UpdateVisualState(button, true);
            button.Dispatcher.BeginInvoke(
                (Action)(() => UpdateVisualState(button, true)),
                DispatcherPriority.Input);
        }

        private static void UpdateVisualState(ToggleButton button, bool useTransitions)
        {
            GoToState(button, GetCommonStateName(button), useTransitions);
            GoToState(button, GetCheckStateName(button), useTransitions);
        }

        private static void GoToState(ToggleButton button, string stateName, bool useTransitions)
        {
            if (!VisualStateManager.GoToState(button, stateName, useTransitions) &&
                button.GetTemplateRoot() is { } templateRoot)
            {
                VisualStateManager.GoToElementState(templateRoot, stateName, useTransitions);
            }
        }

        private static string GetCommonStateName(ToggleButton button)
        {
            if (!button.IsEnabled)
            {
                return "Disabled";
            }

            if (button.IsPressed)
            {
                return "Pressed";
            }

            if (button.IsMouseOver)
            {
                return "PointerOver";
            }

            return "Normal";
        }

        private static string GetCheckStateName(ToggleButton button)
        {
            if (button.IsChecked == true)
            {
                if (!button.IsEnabled)
                {
                    return "CheckedDisabled";
                }

                if (button.IsPressed)
                {
                    return "CheckedPressed";
                }

                if (button.IsMouseOver)
                {
                    return "CheckedPointerOver";
                }

                return "Checked";
            }

            if (button.IsChecked == null)
            {
                if (!button.IsEnabled)
                {
                    return "IndeterminateDisabled";
                }

                if (button.IsPressed)
                {
                    return "IndeterminatePressed";
                }

                if (button.IsMouseOver)
                {
                    return "IndeterminatePointerOver";
                }

                return "Indeterminate";
            }

            return "Unchecked";
        }

        private static readonly DependencyPropertyDescriptor IsPressedPropertyDescriptor =
            DependencyPropertyDescriptor.FromProperty(ButtonBase.IsPressedProperty, typeof(ToggleButton));
    }
}
