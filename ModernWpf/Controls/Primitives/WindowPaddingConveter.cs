using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace ModernWpf.Controls.Primitives
{
    public class WindowPaddingConveter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length == 4)
            {
                if (values[0] is WindowState windowState)
                {
                    if (windowState == WindowState.Maximized)
                    {
                        if (values[1] is Thickness windowNonClientFrameThickness &&
                            values[2] is Thickness windowResizeBorderThickness &&
                            values[3] is double windowCaptionHeight)
                        {
                            var padding = new Thickness(
                                windowNonClientFrameThickness.Left + windowResizeBorderThickness.Left,
                                windowNonClientFrameThickness.Top + windowResizeBorderThickness.Top - windowCaptionHeight,
                                windowNonClientFrameThickness.Right + windowResizeBorderThickness.Right,
                                windowNonClientFrameThickness.Bottom + windowResizeBorderThickness.Bottom);
                            //Debug.WriteLine(padding);
                            return padding;
                        }
                    }
                }
            }

            return new Thickness();
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
