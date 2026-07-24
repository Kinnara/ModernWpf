using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls.Primitives;

namespace ModernWpf.Controls.Primitives
{
    public static class ButtonHelper
    {
        #region VisualStateSettersEnabled

        public static readonly DependencyProperty VisualStateSettersEnabledProperty =
            DependencyProperty.RegisterAttached(
                "VisualStateSettersEnabled",
                typeof(bool),
                typeof(ButtonHelper),
                new PropertyMetadata(false, OnVisualStateSettersEnabledChanged));

        public static bool GetVisualStateSettersEnabled(ButtonBase button)
        {
            return (bool)button.GetValue(VisualStateSettersEnabledProperty);
        }

        public static void SetVisualStateSettersEnabled(ButtonBase button, bool value)
        {
            button.SetValue(VisualStateSettersEnabledProperty, value);
        }

        private static void OnVisualStateSettersEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var button = (ButtonBase)d;
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

        private static void Attach(ButtonBase button)
        {
            button.Loaded += OnLoaded;
            button.IsEnabledChanged += OnDependencyStateChanged;
            button.MouseEnter += OnMouseStateChanged;
            button.MouseLeave += OnMouseStateChanged;
            IsPressedPropertyDescriptor.AddValueChanged(button, OnVisualStatePropertyChanged);
        }

        private static void Detach(ButtonBase button)
        {
            button.Loaded -= OnLoaded;
            button.IsEnabledChanged -= OnDependencyStateChanged;
            button.MouseEnter -= OnMouseStateChanged;
            button.MouseLeave -= OnMouseStateChanged;
            IsPressedPropertyDescriptor.RemoveValueChanged(button, OnVisualStatePropertyChanged);
        }

        private static void OnLoaded(object sender, RoutedEventArgs e)
        {
            UpdateVisualState((ButtonBase)sender, false);
        }

        private static void OnDependencyStateChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            UpdateVisualState((ButtonBase)sender, true);
        }

        private static void OnMouseStateChanged(object sender, RoutedEventArgs e)
        {
            UpdateVisualState((ButtonBase)sender, true);
        }

        private static void OnVisualStatePropertyChanged(object sender, EventArgs e)
        {
            UpdateVisualState((ButtonBase)sender, true);
        }

        private static void UpdateVisualState(ButtonBase button, bool useTransitions)
        {
            string stateName = GetCommonStateName(button);
            if (!VisualStateManager.GoToState(button, stateName, useTransitions) &&
                button.GetTemplateRoot() is { } templateRoot)
            {
                VisualStateManager.GoToElementState(templateRoot, stateName, useTransitions);
            }
        }

        private static string GetCommonStateName(ButtonBase button)
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

        private static readonly DependencyPropertyDescriptor IsPressedPropertyDescriptor =
            DependencyPropertyDescriptor.FromProperty(ButtonBase.IsPressedProperty, typeof(ButtonBase));
    }
}
