using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace ModernWpf.Gallery.Pages.WpfGallery.Samples
{
    /// <summary>
    /// Converts an empty string to Visibility.Collapsed
    /// </summary>
    public sealed class EmptyToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string str)
            {
                return string.IsNullOrWhiteSpace(str) ? Visibility.Collapsed : Visibility.Visible;
            }

            return value is null ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Converts an image id to a brush
    /// </summary>
    public sealed class ImageIdToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var imageKey = value as string;
            if (string.IsNullOrEmpty(imageKey))
            {
                imageKey = "p91";
            }
            else if (imageKey[0] != 'p' && imageKey[0] != 'P')
            {
                imageKey = "p" + imageKey;
            }

            return Application.Current.Resources[imageKey];
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
