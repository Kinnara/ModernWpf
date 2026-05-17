using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Threading;

namespace ModernWpf.Controls.Primitives
{
    public static class CheckBoxHelper
    {
        #region IsEnabled

        public static readonly DependencyProperty IsEnabledProperty =
            DependencyProperty.RegisterAttached(
                "IsEnabled",
                typeof(bool),
                typeof(CheckBoxHelper),
                new PropertyMetadata(false, OnIsEnabledChanged));

        public static bool GetIsEnabled(CheckBox checkBox)
        {
            return (bool)checkBox.GetValue(IsEnabledProperty);
        }

        public static void SetIsEnabled(CheckBox checkBox, bool value)
        {
            checkBox.SetValue(IsEnabledProperty, value);
        }

        private static void OnIsEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var checkBox = (CheckBox)d;
            if ((bool)e.NewValue)
            {
                Detach(checkBox);
                Attach(checkBox);

                if (checkBox.IsLoaded)
                {
                    UpdateVisualState(checkBox, false);
                }
            }
            else
            {
                Detach(checkBox);
            }
        }

        #endregion

        private static void Attach(CheckBox checkBox)
        {
            checkBox.Loaded += OnLoaded;
            checkBox.Checked += OnRoutedStateChanged;
            checkBox.Unchecked += OnRoutedStateChanged;
            checkBox.Indeterminate += OnRoutedStateChanged;
            checkBox.IsEnabledChanged += OnDependencyStateChanged;
            checkBox.KeyDown += OnKeyDown;
            checkBox.MouseEnter += OnMouseStateChanged;
            checkBox.MouseLeave += OnMouseStateChanged;
            checkBox.PreviewMouseDown += OnMouseButtonStateChanged;
            checkBox.PreviewMouseUp += OnMouseButtonStateChanged;
            checkBox.LostMouseCapture += OnMouseStateChanged;
        }

        private static void Detach(CheckBox checkBox)
        {
            checkBox.Loaded -= OnLoaded;
            checkBox.Checked -= OnRoutedStateChanged;
            checkBox.Unchecked -= OnRoutedStateChanged;
            checkBox.Indeterminate -= OnRoutedStateChanged;
            checkBox.IsEnabledChanged -= OnDependencyStateChanged;
            checkBox.KeyDown -= OnKeyDown;
            checkBox.MouseEnter -= OnMouseStateChanged;
            checkBox.MouseLeave -= OnMouseStateChanged;
            checkBox.PreviewMouseDown -= OnMouseButtonStateChanged;
            checkBox.PreviewMouseUp -= OnMouseButtonStateChanged;
            checkBox.LostMouseCapture -= OnMouseStateChanged;
        }

        private static void OnLoaded(object sender, RoutedEventArgs e)
        {
            UpdateVisualState((CheckBox)sender, false);
        }

        private static void OnRoutedStateChanged(object sender, RoutedEventArgs e)
        {
            UpdateVisualState((CheckBox)sender, true);
        }

        private static void OnDependencyStateChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            UpdateVisualState((CheckBox)sender, true);
        }

        private static void OnKeyDown(object sender, KeyEventArgs e)
        {
            var checkBox = (CheckBox)sender;
            if (checkBox.IsThreeState || !checkBox.IsEnabled)
            {
                return;
            }

            if (e.Key == Key.Add)
            {
                e.Handled = true;
                checkBox.IsChecked = true;
                UpdateVisualState(checkBox, true);
            }
            else if (e.Key == Key.Subtract)
            {
                e.Handled = true;
                checkBox.IsChecked = false;
                UpdateVisualState(checkBox, true);
            }
        }

        private static void OnMouseStateChanged(object sender, MouseEventArgs e)
        {
            ScheduleVisualStateUpdate((CheckBox)sender);
        }

        private static void OnMouseButtonStateChanged(object sender, MouseButtonEventArgs e)
        {
            ScheduleVisualStateUpdate((CheckBox)sender);
        }

        private static void ScheduleVisualStateUpdate(CheckBox checkBox)
        {
            UpdateVisualState(checkBox, true);
            checkBox.Dispatcher.BeginInvoke(
                (Action)(() => UpdateVisualState(checkBox, true)),
                DispatcherPriority.Input);
        }

        private static void UpdateVisualState(CheckBox checkBox, bool useTransitions)
        {
            VisualStateManager.GoToState(checkBox, GetStateName(checkBox), useTransitions);
        }

        private static string GetStateName(CheckBox checkBox)
        {
            string prefix;
            if (checkBox.IsChecked == true)
            {
                prefix = "Checked";
            }
            else if (checkBox.IsChecked == null)
            {
                prefix = "Indeterminate";
            }
            else
            {
                prefix = "Unchecked";
            }

            if (!checkBox.IsEnabled)
            {
                return prefix + "Disabled";
            }

            if (checkBox.IsPressed)
            {
                return prefix + "Pressed";
            }

            if (checkBox.IsMouseOver)
            {
                return prefix + "PointerOver";
            }

            return prefix + "Normal";
        }
    }
}
