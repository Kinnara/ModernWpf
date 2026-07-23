using System;
using System.Windows;
using System.Windows.Media;

namespace ModernWpf.Controls.Primitives
{
    internal sealed class ProgressBarIndicatorRasterOverlay : FrameworkElement
    {
        public static readonly DependencyProperty FillProperty =
            DependencyProperty.Register(
                nameof(Fill),
                typeof(Brush),
                typeof(ProgressBarIndicatorRasterOverlay),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

        public Brush Fill
        {
            get => (Brush)GetValue(FillProperty);
            set => SetValue(FillProperty, value);
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            var dpiScaleY = VisualTreeHelper.GetDpi(this).DpiScaleY;
            var physicalPixelHeight = dpiScaleY > 0 ? 1.0 / dpiScaleY : 1.0;
            return new Size(0, physicalPixelHeight);
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);

            if (Fill == null || ActualWidth <= 0 || ActualHeight <= 0)
            {
                return;
            }

            var dpiScaleX = VisualTreeHelper.GetDpi(this).DpiScaleX;
            var physicalPixelWidth = dpiScaleX > 0 ? 1.0 / dpiScaleX : 1.0;
            var inset = 2.0 * physicalPixelWidth;
            var width = Math.Max(ActualWidth - (2.0 * inset), 0);
            if (width <= 0)
            {
                return;
            }

            var guidelines = new GuidelineSet();
            guidelines.GuidelinesX.Add(inset);
            guidelines.GuidelinesX.Add(inset + width);
            guidelines.GuidelinesY.Add(0);
            guidelines.GuidelinesY.Add(ActualHeight);

            drawingContext.PushGuidelineSet(guidelines);
            drawingContext.DrawRectangle(Fill, null, new Rect(inset, 0, width, ActualHeight));
            drawingContext.Pop();
        }
    }
}
