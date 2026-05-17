using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;

namespace ModernWpf.Controls.Primitives
{
    public static class DatePickerHelper
    {
        private static readonly FirstNotNullOrEmptyConverter _watermarkConverter = new FirstNotNullOrEmptyConverter();

        #region IsEnabled

        public static readonly DependencyProperty IsEnabledProperty =
            DependencyProperty.RegisterAttached(
                "IsEnabled",
                typeof(bool),
                typeof(DatePickerHelper),
                new PropertyMetadata(false, OnIsEnabledChanged));

        public static bool GetIsEnabled(DatePicker datePicker)
        {
            return (bool)datePicker.GetValue(IsEnabledProperty);
        }

        public static void SetIsEnabled(DatePicker datePicker, bool value)
        {
            datePicker.SetValue(IsEnabledProperty, value);
        }

        private static void OnIsEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var datePicker = (DatePicker)d;
            if ((bool)e.NewValue)
            {
                datePicker.Loaded -= OnLoaded;
                datePicker.SelectedDateChanged -= OnSelectedDateChanged;
                datePicker.IsEnabledChanged -= OnIsEnabledChanged;
                datePicker.MouseEnter -= OnPointerStateChanged;
                datePicker.MouseLeave -= OnPointerStateChanged;
                datePicker.PreviewMouseLeftButtonDown -= OnPreviewMouseLeftButtonDown;
                datePicker.PreviewMouseLeftButtonUp -= OnPreviewMouseLeftButtonUp;
                datePicker.LostMouseCapture -= OnLostMouseCapture;

                datePicker.Loaded += OnLoaded;
                datePicker.SelectedDateChanged += OnSelectedDateChanged;
                datePicker.IsEnabledChanged += OnIsEnabledChanged;
                datePicker.MouseEnter += OnPointerStateChanged;
                datePicker.MouseLeave += OnPointerStateChanged;
                datePicker.PreviewMouseLeftButtonDown += OnPreviewMouseLeftButtonDown;
                datePicker.PreviewMouseLeftButtonUp += OnPreviewMouseLeftButtonUp;
                datePicker.LostMouseCapture += OnLostMouseCapture;

                if (datePicker.IsLoaded)
                {
                    Initialize(datePicker);
                }
            }
            else
            {
                datePicker.Loaded -= OnLoaded;
                datePicker.SelectedDateChanged -= OnSelectedDateChanged;
                datePicker.IsEnabledChanged -= OnIsEnabledChanged;
                datePicker.MouseEnter -= OnPointerStateChanged;
                datePicker.MouseLeave -= OnPointerStateChanged;
                datePicker.PreviewMouseLeftButtonDown -= OnPreviewMouseLeftButtonDown;
                datePicker.PreviewMouseLeftButtonUp -= OnPreviewMouseLeftButtonUp;
                datePicker.LostMouseCapture -= OnLostMouseCapture;
                SetIsPressed(datePicker, false);
            }
        }

        #endregion

        #region HeaderPlacement

        public static readonly DependencyProperty HeaderPlacementProperty =
            DependencyProperty.RegisterAttached(
                "HeaderPlacement",
                typeof(DatePickerHeaderPlacement),
                typeof(DatePickerHelper),
                new FrameworkPropertyMetadata(DatePickerHeaderPlacement.Top, OnHeaderPlacementChanged));

        public static DatePickerHeaderPlacement GetHeaderPlacement(DatePicker datePicker)
        {
            return (DatePickerHeaderPlacement)datePicker.GetValue(HeaderPlacementProperty);
        }

        public static void SetHeaderPlacement(DatePicker datePicker, DatePickerHeaderPlacement value)
        {
            datePicker.SetValue(HeaderPlacementProperty, value);
        }

        private static void OnHeaderPlacementChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is DatePicker datePicker)
            {
                UpdateVisualStates(datePicker, true);
            }
        }

        #endregion

        private static void OnLoaded(object sender, RoutedEventArgs e)
        {
            var datePicker = (DatePicker)sender;
            Initialize(datePicker);
        }

        private static void Initialize(DatePicker datePicker)
        {
            ApplyPlaceholderTextBinding(datePicker);
            UpdateVisualStates(datePicker, false);
        }

        private static void ApplyPlaceholderTextBinding(DatePicker datePicker)
        {
            if (datePicker.GetTemplateChild<DatePickerTextBox>("PART_TextBox") is DatePickerTextBox textBox)
            {
                if (textBox.GetTemplateChild<ContentControl>("PART_Watermark") is ContentControl watermarkElement)
                {
                    if ((bool)watermarkElement.GetValue(IsWatermarkBindingAppliedProperty))
                    {
                        return;
                    }

                    var placeholderTextBinding = new Binding
                    {
                        Path = new PropertyPath(ControlHelper.PlaceholderTextProperty),
                        Source = datePicker
                    };

                    BindingBase newBinding;

                    var originalBE = watermarkElement.GetBindingExpression(ContentControl.ContentProperty);
                    if (originalBE != null)
                    {
                        newBinding = new MultiBinding
                        {
                            Bindings = { placeholderTextBinding, originalBE.ParentBinding },
                            Converter = _watermarkConverter
                        };
                    }
                    else
                    {
                        newBinding = placeholderTextBinding;
                    }

                    watermarkElement.SetBinding(ContentControl.ContentProperty, newBinding);
                    watermarkElement.SetValue(IsWatermarkBindingAppliedProperty, true);
                }
            }
        }

        private static readonly DependencyProperty IsWatermarkBindingAppliedProperty =
            DependencyProperty.RegisterAttached(
                "IsWatermarkBindingApplied",
                typeof(bool),
                typeof(DatePickerHelper),
                new PropertyMetadata(false));

        private static void OnSelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateVisualStates((DatePicker)sender, true);
        }

        private static void UpdateVisualStates(DatePicker datePicker, bool useTransitions)
        {
            VisualStateManager.GoToState(
                datePicker,
                GetCommonStateName(datePicker),
                useTransitions);

            VisualStateManager.GoToState(
                datePicker,
                datePicker.SelectedDate.HasValue ? "Selected" : "Unselected",
                useTransitions);

            VisualStateManager.GoToState(
                datePicker,
                GetHeaderPlacement(datePicker) == DatePickerHeaderPlacement.Left ? "LeftHeader" : "TopHeader",
                useTransitions);
        }

        private static string GetCommonStateName(DatePicker datePicker)
        {
            if (!datePicker.IsEnabled)
            {
                return "Disabled";
            }

            if (GetIsPressed(datePicker))
            {
                return "Pressed";
            }

            return datePicker.IsMouseOver ? "PointerOver" : "Normal";
        }

        private static void OnIsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var datePicker = (DatePicker)sender;
            if (!datePicker.IsEnabled)
            {
                SetIsPressed(datePicker, false);
            }

            UpdateVisualStates(datePicker, true);
        }

        private static void OnPointerStateChanged(object sender, RoutedEventArgs e)
        {
            UpdateVisualStates((DatePicker)sender, true);
        }

        private static void OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var datePicker = (DatePicker)sender;
            SetIsPressed(datePicker, true);
            UpdateVisualStates(datePicker, true);
        }

        private static void OnPreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var datePicker = (DatePicker)sender;
            SetIsPressed(datePicker, false);
            UpdateVisualStates(datePicker, true);
        }

        private static void OnLostMouseCapture(object sender, MouseEventArgs e)
        {
            var datePicker = (DatePicker)sender;
            SetIsPressed(datePicker, false);
            UpdateVisualStates(datePicker, true);
        }

        private static readonly DependencyProperty IsPressedProperty =
            DependencyProperty.RegisterAttached(
                "IsPressed",
                typeof(bool),
                typeof(DatePickerHelper),
                new PropertyMetadata(false));

        private static bool GetIsPressed(DatePicker datePicker)
        {
            return (bool)datePicker.GetValue(IsPressedProperty);
        }

        private static void SetIsPressed(DatePicker datePicker, bool value)
        {
            datePicker.SetValue(IsPressedProperty, value);
        }

        private class FirstNotNullOrEmptyConverter : IMultiValueConverter
        {
            public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
            {
                foreach (object value in values)
                {
                    if (value is string s)
                    {
                        if (!string.IsNullOrEmpty(s))
                        {
                            return s;
                        }
                    }
                    else if (value != null)
                    {
                        return value;
                    }
                }

                return null;
            }

            public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
            {
                var result = new object[targetTypes.Length];
                for (var i = 0; i < result.Length; i++)
                {
                    result[i] = Binding.DoNothing;
                }

                return result;
            }
        }
    }
}
