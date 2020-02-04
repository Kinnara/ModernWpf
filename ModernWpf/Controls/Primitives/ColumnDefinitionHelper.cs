using System.Windows;
using System.Windows.Controls;

namespace ModernWpf.Controls.Primitives
{
    public static class ColumnDefinitionHelper
    {
        #region PixelWidth

        public static readonly DependencyProperty PixelWidthProperty =
            DependencyProperty.RegisterAttached(
                "PixelWidth",
                typeof(double),
                typeof(ColumnDefinitionHelper),
                new PropertyMetadata(double.NaN, OnPixelWidthChanged));

        public static double GetPixelWidth(ColumnDefinition columnDefinition)
        {
            return (double)columnDefinition.GetValue(PixelWidthProperty);
        }

        public static void SetPixelWidth(ColumnDefinition columnDefinition, double value)
        {
            columnDefinition.SetValue(PixelWidthProperty, value);
        }

        private static void OnPixelWidthChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var columnDefinition = (ColumnDefinition)d;
            var pixels = (double)e.NewValue;
            if (double.IsNaN(pixels) || double.IsInfinity(pixels))
            {
                columnDefinition.ClearValue(ColumnDefinition.WidthProperty);
            }
            else
            {
                columnDefinition.Width = new GridLength(pixels);
            }
        }

        #endregion
    }
}
