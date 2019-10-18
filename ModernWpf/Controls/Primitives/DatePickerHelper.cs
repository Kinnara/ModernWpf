using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;

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
                new PropertyMetadata(OnIsEnabledChanged));

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
                datePicker.Loaded += OnLoaded;
            }
            else
            {
                datePicker.Loaded -= OnLoaded;
            }
        }

        #endregion

        private static void OnLoaded(object sender, RoutedEventArgs e)
        {
            var datePicker = (DatePicker)sender;
            datePicker.Loaded -= OnLoaded;
            if (datePicker.GetTemplateChild<DatePickerTextBox>("PART_TextBox") is DatePickerTextBox textBox)
            {
                if (textBox.GetTemplateChild<ContentControl>("PART_Watermark") is ContentControl watermarkElement)
                {
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
                }
            }
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
                throw new NotImplementedException();
            }
        }
    }
}
