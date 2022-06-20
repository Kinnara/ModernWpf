using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace ModernWpf.Controls
{
    public class MediaTransportControlsTemplateSettings : DependencyObject
    {
        public MediaTransportControlsTemplateSettings()
        {
        }

        #region AcrylicBrush

        private static readonly DependencyPropertyKey AcrylicBrushPropertyKey =
            DependencyProperty.RegisterReadOnly(
                nameof(AcrylicBrush),
                typeof(Brush),
                typeof(MediaTransportControlsTemplateSettings),
                null);

        public static readonly DependencyProperty AcrylicBrushProperty =
            AcrylicBrushPropertyKey.DependencyProperty;

        public Brush AcrylicBrush
        {
            get => (Brush)GetValue(AcrylicBrushProperty);
            set => SetValue(AcrylicBrushPropertyKey, value);
        }

        #endregion
    }
}
