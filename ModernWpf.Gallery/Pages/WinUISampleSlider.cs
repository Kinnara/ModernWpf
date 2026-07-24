using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace ModernWpf.Gallery.Pages
{
    internal static class WinUISampleSlider
    {
        public static Slider ShowValueFill(Slider slider)
        {
            if (slider == null)
            {
                throw new ArgumentNullException(nameof(slider));
            }

            DispatcherOperation pendingRangeLayout = null;
            Action queueRangeLayout = delegate
            {
                if (pendingRangeLayout != null)
                {
                    return;
                }

                pendingRangeLayout = slider.Dispatcher.BeginInvoke(new Action(() =>
                {
                    pendingRangeLayout = null;
                    slider.ApplyTemplate();

                    var selectionRange = slider.Template?.FindName("PART_SelectionRange", slider) as FrameworkElement;
                    if (selectionRange?.Parent is FrameworkElement rangeCanvas)
                    {
                        selectionRange.InvalidateMeasure();
                        rangeCanvas.InvalidateMeasure();
                        rangeCanvas.InvalidateArrange();
                        ArrangeRange(selectionRange, rangeCanvas);
                    }
                }), DispatcherPriority.Loaded);
            };

            Action updateValueFill = delegate
            {
                slider.SelectionStart = slider.Minimum;
                slider.SelectionEnd = slider.Value;

                if (slider.IsLoaded)
                {
                    queueRangeLayout();
                }
            };

            slider.IsSelectionRangeEnabled = true;
            updateValueFill();
            slider.Loaded += delegate { queueRangeLayout(); };
            slider.ValueChanged += delegate { updateValueFill(); };
            return slider;
        }

        private static void ArrangeRange(FrameworkElement range, FrameworkElement rangeCanvas)
        {
            // The .NET 10 Fluent Slider sets the range size after its Canvas
            // measure pass, which can leave a nonzero Width with zero RenderSize.
            range.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            var rangeSize = range.DesiredSize;

            if (!double.IsNaN(range.Width))
            {
                rangeSize.Width = range.Width;
            }

            if (!double.IsNaN(range.Height))
            {
                rangeSize.Height = range.Height;
            }

            var left = Canvas.GetLeft(range);
            if (double.IsNaN(left))
            {
                var right = Canvas.GetRight(range);
                left = double.IsNaN(right) ? 0 : rangeCanvas.ActualWidth - rangeSize.Width - right;
            }

            var top = Canvas.GetTop(range);
            if (double.IsNaN(top))
            {
                var bottom = Canvas.GetBottom(range);
                top = double.IsNaN(bottom) ? 0 : rangeCanvas.ActualHeight - rangeSize.Height - bottom;
            }

            range.Arrange(new Rect(new Point(left, top), rangeSize));
        }
    }
}
