using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

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
                if (toolTip.PlacementTarget is Thumb thumb &&
                    thumb.TemplatedParent is Slider slider)
                {
                    SetOriginalCustomPopupPlacementCallback(toolTip, toolTip.CustomPopupPlacementCallback);
                    toolTip.CustomPopupPlacementCallback = (popupSize, targetSize, offset) => PositionAutoToolTip(slider, toolTip, popupSize, targetSize);
                }

                toolTip.IsVisibleChanged += OnToolTipIsVisibleChanged;
            }
            else
            {
                if (toolTip.ReadLocalValue(OriginalCustomPopupPlacementCallbackProperty) != DependencyProperty.UnsetValue)
                {
                    toolTip.CustomPopupPlacementCallback = GetOriginalCustomPopupPlacementCallback(toolTip);
                    toolTip.ClearValue(OriginalCustomPopupPlacementCallbackProperty);
                }

                toolTip.IsVisibleChanged -= OnToolTipIsVisibleChanged;
            }
        }

        #endregion

        #region OriginalCustomPopupPlacementCallback

        private static readonly DependencyProperty OriginalCustomPopupPlacementCallbackProperty =
            DependencyProperty.RegisterAttached(
                "OriginalCustomPopupPlacementCallback",
                typeof(CustomPopupPlacementCallback),
                typeof(SliderAutoToolTipHelper));

        private static CustomPopupPlacementCallback GetOriginalCustomPopupPlacementCallback(ToolTip toolTip)
        {
            return (CustomPopupPlacementCallback)toolTip.GetValue(OriginalCustomPopupPlacementCallbackProperty);
        }

        private static void SetOriginalCustomPopupPlacementCallback(ToolTip toolTip, CustomPopupPlacementCallback value)
        {
            toolTip.SetValue(OriginalCustomPopupPlacementCallbackProperty, value);
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

        private static CustomPopupPlacement[] PositionAutoToolTip(
            Slider slider,
            ToolTip autoToolTip,
            Size popupSize,
            Size targetSize)
        {
            Point point;
            PopupPrimaryAxis primaryAxis;

            switch (slider.AutoToolTipPlacement)
            {
                case AutoToolTipPlacement.TopLeft:
                    if (slider.Orientation == Orientation.Horizontal)
                    {
                        // Place popup at top of thumb
                        point = new Point((targetSize.Width - popupSize.Width) * 0.5, -popupSize.Height);
                        primaryAxis = PopupPrimaryAxis.Horizontal;
                    }
                    else
                    {
                        // Place popup at left of thumb
                        point = new Point(-popupSize.Width, (targetSize.Height - popupSize.Height) * 0.5);
                        primaryAxis = PopupPrimaryAxis.Vertical;
                    }
                    break;
                case AutoToolTipPlacement.BottomRight:
                    if (slider.Orientation == Orientation.Horizontal)
                    {
                        // Place popup at bottom of thumb
                        point = new Point((targetSize.Width - popupSize.Width) * 0.5, targetSize.Height);
                        primaryAxis = PopupPrimaryAxis.Horizontal;
                    }
                    else
                    {
                        // Place popup at right of thumb
                        point = new Point(targetSize.Width, (targetSize.Height - popupSize.Height) * 0.5);
                        primaryAxis = PopupPrimaryAxis.Vertical;
                    }
                    break;
                default:
                    return new CustomPopupPlacement[] { };
            }

            if (Helper.TryGetTransformToDevice(autoToolTip, out Matrix transformToDevice))
            {
                Vector offset = VisualTreeHelper.GetOffset(autoToolTip);
                point -= transformToDevice.Transform(offset);
            }

            return new CustomPopupPlacement[] { new CustomPopupPlacement(point, primaryAxis) };
        }
    }
}
