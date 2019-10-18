using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace ModernWpf.Controls.Primitives
{
    public static class SliderAutoToolTipHelper
    {
        #region IsEnabled

        public static bool GetIsEnabled(ToolTip toolTip)
        {
            return (bool)toolTip.GetValue(IsEnabledProperty);
        }

        public static void SetIsEnabled(ToolTip toolTip, bool value)
        {
            toolTip.SetValue(IsEnabledProperty, value);
        }

        public static readonly DependencyProperty IsEnabledProperty =
            DependencyProperty.RegisterAttached(
                "IsEnabled",
                typeof(bool),
                typeof(SliderAutoToolTipHelper),
                new PropertyMetadata(OnIsEnabledChanged));

        private static void OnIsEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var toolTip = (ToolTip)d;
            var slider = GetSlider(toolTip);
            if (slider != null)
            {
                if ((bool)e.NewValue)
                {
                    SetAutoToolTip(slider, toolTip);
                    SetSliderAutoToolTipPlacement(toolTip, slider.AutoToolTipPlacement);
                    toolTip.IsVisibleChanged += OnToolTipIsVisibleChanged;
                }
                else
                {
                    toolTip.IsVisibleChanged -= OnToolTipIsVisibleChanged;
                    UnregisterSliderPropertyChangedCallback(slider);
                    slider.ClearValue(AutoToolTipProperty);
                    toolTip.ClearValue(SliderAutoToolTipPlacementPropertyKey);
                }
            }
        }

        #endregion

        #region SliderAutoToolTipPlacement

        public static AutoToolTipPlacement GetSliderAutoToolTipPlacement(ToolTip toolTip)
        {
            return (AutoToolTipPlacement)toolTip.GetValue(SliderAutoToolTipPlacementProperty);
        }

        private static void SetSliderAutoToolTipPlacement(ToolTip toolTip, AutoToolTipPlacement value)
        {
            toolTip.SetValue(SliderAutoToolTipPlacementPropertyKey, value);
        }

        private static readonly DependencyPropertyKey SliderAutoToolTipPlacementPropertyKey =
            DependencyProperty.RegisterAttachedReadOnly(
                "SliderAutoToolTipPlacement",
                typeof(AutoToolTipPlacement),
                typeof(SliderAutoToolTipHelper),
                null);

        public static readonly DependencyProperty SliderAutoToolTipPlacementProperty =
            SliderAutoToolTipPlacementPropertyKey.DependencyProperty;

        #endregion

        #region AutoToolTip

        private static ToolTip GetAutoToolTip(Slider slider)
        {
            return (ToolTip)slider.GetValue(AutoToolTipProperty);
        }

        private static void SetAutoToolTip(Slider slider, ToolTip value)
        {
            slider.SetValue(AutoToolTipProperty, value);
        }

        private static readonly DependencyProperty AutoToolTipProperty =
            DependencyProperty.RegisterAttached(
                "AutoToolTip",
                typeof(ToolTip),
                typeof(SliderAutoToolTipHelper),
                null);

        #endregion

        private static void OnToolTipIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var slider = GetSlider((ToolTip)sender);
            Debug.Assert(slider != null);
            if (slider != null)
            {
                if ((bool)e.NewValue)
                {
                    RegisterSliderPropertyChangedCallback(slider);
                }
                else
                {
                    UnregisterSliderPropertyChangedCallback(slider);
                }
            }
        }

        private static void RegisterSliderPropertyChangedCallback(Slider slider)
        {
            GetSliderPropertyDescriptor(Slider.AutoToolTipPlacementProperty).AddValueChanged(
                slider, OnSliderAutoToolTipPlacementChanged);
        }

        private static void UnregisterSliderPropertyChangedCallback(Slider slider)
        {
            GetSliderPropertyDescriptor(Slider.AutoToolTipPlacementProperty).RemoveValueChanged(
                slider, OnSliderAutoToolTipPlacementChanged);
        }

        private static void OnSliderAutoToolTipPlacementChanged(object sender, EventArgs e)
        {
            var slider = (Slider)sender;
            var toolTip = GetAutoToolTip(slider);
            if (toolTip != null)
            {
                SetSliderAutoToolTipPlacement(toolTip, slider.AutoToolTipPlacement);
            }
        }

        private static Slider GetSlider(ToolTip toolTip)
        {
            return (toolTip.PlacementTarget as Thumb)?.TemplatedParent as Slider;
        }

        private static DependencyPropertyDescriptor GetSliderPropertyDescriptor(DependencyProperty dependencyProperty)
        {
            return DependencyPropertyDescriptor.FromProperty(dependencyProperty, typeof(Slider));
        }
    }
}
