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
            if ((bool)e.NewValue)
            {
                toolTip.IsVisibleChanged += OnToolTipIsVisibleChanged;
            }
            else
            {
                toolTip.IsVisibleChanged -= OnToolTipIsVisibleChanged;
            }
        }

        #endregion

        private static void OnToolTipIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var toolTip = (ToolTip)sender;
            Debug.Assert(toolTip.PlacementTarget is Thumb);
            if (toolTip.PlacementTarget is Thumb thumb)
            {
                if ((bool)e.NewValue)
                {
                    thumb.SizeChanged += OnThumbSizeChanged;
                    UpdatePlacementRectangle(toolTip, thumb.RenderSize);
                }
                else
                {
                    thumb.SizeChanged -= OnThumbSizeChanged;
                    toolTip.ClearValue(ToolTip.PlacementRectangleProperty);
                }
            }
        }

        private static void OnThumbSizeChanged(object sender, SizeChangedEventArgs e)
        {
            var thumb = (Thumb)sender;
            if (thumb.ToolTip is ToolTip toolTip)
            {
                UpdatePlacementRectangle(toolTip, e.NewSize);
            }
        }

        private static void UpdatePlacementRectangle(ToolTip toolTip, Size targetSize)
        {
            toolTip.PlacementRectangle = new Rect(
                new Point(-20, -20),
                new Point(targetSize.Width + 20, targetSize.Height + 20));
        }
    }
}
