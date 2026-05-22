using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ModernWpf.Gallery.Pages.WpfGallery.Samples
{
    public sealed class EmptyToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var text = value as string;
            return string.IsNullOrEmpty(text) ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    public sealed class ImageIdToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var imageKey = value as string;
            if (string.IsNullOrEmpty(imageKey))
            {
                imageKey = "91";
            }

            if (imageKey[0] == 'p' || imageKey[0] == 'P')
            {
                imageKey = imageKey.Substring(1);
            }

            return new ImageBrush
            {
                ImageSource = new BitmapImage(new Uri("pack://application:,,,/ModernWpf.Gallery;component/Assets/UserDashboard/" + imageKey + "-100x100.jpg")),
                Stretch = Stretch.UniformToFill
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
