using System;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace ModernWpf.Controls.Primitives
{
    internal enum CustomPlacementMode
    {
        Top = 0,
        Bottom = 1,
        Left = 2,
        Right = 3,
        Full = 4,
        TopEdgeAlignedLeft = 5,
        TopEdgeAlignedRight = 6,
        BottomEdgeAlignedLeft = 7,
        BottomEdgeAlignedRight = 8,
        LeftEdgeAlignedTop = 9,
        LeftEdgeAlignedBottom = 10,
        RightEdgeAlignedTop = 11,
        RightEdgeAlignedBottom = 12,
        //Auto = 13
    }

    internal static class CustomPopupPlacementHelper
    {
        #region Placement

        public static readonly DependencyProperty PlacementProperty =
            DependencyProperty.RegisterAttached(
                "Placement",
                typeof(CustomPlacementMode),
                typeof(CustomPopupPlacementHelper),
                new PropertyMetadata(CustomPlacementMode.Top));

        public static CustomPlacementMode GetPlacement(DependencyObject element)
        {
            return (CustomPlacementMode)element.GetValue(PlacementProperty);
        }

        public static void SetPlacement(DependencyObject element, CustomPlacementMode value)
        {
            element.SetValue(PlacementProperty, value);
        }

        #endregion

        internal static CustomPopupPlacement[] PositionPopup(
            CustomPlacementMode placement,
            Size popupSize,
            Size targetSize,
            Point offset,
            FrameworkElement child = null,
            Rect? exclusionRect = null)
        {
            Matrix transformToDevice = default;
            if (child != null)
            {
                Helper.TryGetTransformToDevice(child, out transformToDevice);
            }

            var fallbackOrder = GetPlacementFallbackOrder(placement);
            var placements = new CustomPopupPlacement[fallbackOrder.Length];

            for (int i = 0; i < fallbackOrder.Length; i++)
            {
                placements[i] = CalculatePopupPlacement(
                    fallbackOrder[i],
                    popupSize,
                    targetSize,
                    offset,
                    child,
                    transformToDevice);

                if (exclusionRect.HasValue)
                {
                    placements[i] = AccountForExclusionRect(
                        fallbackOrder[i],
                        placements[i],
                        popupSize,
                        exclusionRect.Value);
                }
            }

            return placements;
        }

        private static CustomPopupPlacement CalculatePopupPlacement(
            CustomPlacementMode placement,
            Size popupSize,
            Size targetSize,
            Point offset,
            FrameworkElement child = null,
            Matrix transformToDevice = default)
        {
            Point point;
            PopupPrimaryAxis primaryAxis;

            switch (placement)
            {
                case CustomPlacementMode.Top:
                    point = new Point((targetSize.Width - popupSize.Width) / 2, -popupSize.Height);
                    primaryAxis = PopupPrimaryAxis.Horizontal;
                    break;
                case CustomPlacementMode.Bottom:
                    point = new Point((targetSize.Width - popupSize.Width) / 2, targetSize.Height);
                    primaryAxis = PopupPrimaryAxis.Horizontal;
                    break;
                case CustomPlacementMode.Left:
                    point = new Point(-popupSize.Width, (targetSize.Height - popupSize.Height) / 2);
                    primaryAxis = PopupPrimaryAxis.Vertical;
                    break;
                case CustomPlacementMode.Right:
                    point = new Point(targetSize.Width, (targetSize.Height - popupSize.Height) / 2);
                    primaryAxis = PopupPrimaryAxis.Vertical;
                    break;
                case CustomPlacementMode.Full:
                    point = new Point((targetSize.Width - popupSize.Width) / 2, (targetSize.Height - popupSize.Height) / 2);
                    primaryAxis = PopupPrimaryAxis.None;
                    break;
                case CustomPlacementMode.TopEdgeAlignedLeft:
                    point = new Point(0, -popupSize.Height);
                    primaryAxis = PopupPrimaryAxis.Horizontal;
                    break;
                case CustomPlacementMode.TopEdgeAlignedRight:
                    point = new Point(targetSize.Width - popupSize.Width, -popupSize.Height);
                    primaryAxis = PopupPrimaryAxis.Horizontal;
                    break;
                case CustomPlacementMode.BottomEdgeAlignedLeft:
                    point = new Point(0, targetSize.Height);
                    primaryAxis = PopupPrimaryAxis.Horizontal;
                    break;
                case CustomPlacementMode.BottomEdgeAlignedRight:
                    point = new Point(targetSize.Width - popupSize.Width, targetSize.Height);
                    primaryAxis = PopupPrimaryAxis.Horizontal;
                    break;
                case CustomPlacementMode.LeftEdgeAlignedTop:
                    point = new Point(-popupSize.Width, 0);
                    primaryAxis = PopupPrimaryAxis.Vertical;
                    break;
                case CustomPlacementMode.LeftEdgeAlignedBottom:
                    point = new Point(-popupSize.Width, targetSize.Height - popupSize.Height);
                    primaryAxis = PopupPrimaryAxis.Vertical;
                    break;
                case CustomPlacementMode.RightEdgeAlignedTop:
                    point = new Point(targetSize.Width, 0);
                    primaryAxis = PopupPrimaryAxis.Vertical;
                    break;
                case CustomPlacementMode.RightEdgeAlignedBottom:
                    point = new Point(targetSize.Width, targetSize.Height - popupSize.Height);
                    primaryAxis = PopupPrimaryAxis.Vertical;
                    break;
                //case CustomPopupPlacementMode.Auto:
                default:
                    throw new ArgumentOutOfRangeException(nameof(placement));
            }

            if (child != null)
            {
                Vector childOffset = VisualTreeHelper.GetOffset(child);
                if (transformToDevice != default)
                {
                    childOffset = transformToDevice.Transform(childOffset);
                }
                point -= childOffset;
            }

            return new CustomPopupPlacement(point, primaryAxis);
        }

        private static CustomPopupPlacement AccountForExclusionRect(
            CustomPlacementMode placement,
            CustomPopupPlacement popupPlacement,
            Size popupSize,
            Rect exclusionRect)
        {
            var popupRect = new Rect(popupPlacement.Point, popupSize);
            if (!popupRect.IntersectsWith(exclusionRect))
            {
                return popupPlacement;
            }

            var point = popupPlacement.Point;
            switch (GetMajorPlacement(placement))
            {
                case CustomPlacementMode.Top:
                    point.Y = exclusionRect.Y - popupSize.Height;
                    break;
                case CustomPlacementMode.Bottom:
                    point.Y = exclusionRect.Y + exclusionRect.Height;
                    break;
                case CustomPlacementMode.Left:
                    point.X = exclusionRect.X - popupSize.Width;
                    break;
                case CustomPlacementMode.Right:
                    point.X = exclusionRect.X + exclusionRect.Width;
                    break;
                case CustomPlacementMode.Full:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(placement));
            }

            return new CustomPopupPlacement(point, popupPlacement.PrimaryAxis);
        }

        private static CustomPlacementMode GetMajorPlacement(CustomPlacementMode placement)
        {
            switch (placement)
            {
                case CustomPlacementMode.Top:
                case CustomPlacementMode.TopEdgeAlignedLeft:
                case CustomPlacementMode.TopEdgeAlignedRight:
                    return CustomPlacementMode.Top;
                case CustomPlacementMode.Bottom:
                case CustomPlacementMode.BottomEdgeAlignedLeft:
                case CustomPlacementMode.BottomEdgeAlignedRight:
                    return CustomPlacementMode.Bottom;
                case CustomPlacementMode.Left:
                case CustomPlacementMode.LeftEdgeAlignedTop:
                case CustomPlacementMode.LeftEdgeAlignedBottom:
                    return CustomPlacementMode.Left;
                case CustomPlacementMode.Right:
                case CustomPlacementMode.RightEdgeAlignedTop:
                case CustomPlacementMode.RightEdgeAlignedBottom:
                    return CustomPlacementMode.Right;
                case CustomPlacementMode.Full:
                    return CustomPlacementMode.Full;
                default:
                    throw new ArgumentOutOfRangeException(nameof(placement));
            }
        }

        private static CustomPlacementMode[] GetPlacementFallbackOrder(CustomPlacementMode placement)
        {
            switch (placement)
            {
                case CustomPlacementMode.Top:
                    return new[]
                    {
                        CustomPlacementMode.Top,
                        CustomPlacementMode.Bottom,
                        CustomPlacementMode.Left,
                        CustomPlacementMode.Right
                    };
                case CustomPlacementMode.Bottom:
                    return new[]
                    {
                        CustomPlacementMode.Bottom,
                        CustomPlacementMode.Top,
                        CustomPlacementMode.Left,
                        CustomPlacementMode.Right
                    };
                case CustomPlacementMode.Left:
                    return new[]
                    {
                        CustomPlacementMode.Left,
                        CustomPlacementMode.Right,
                        CustomPlacementMode.Top,
                        CustomPlacementMode.Bottom
                    };
                case CustomPlacementMode.Right:
                    return new[]
                    {
                        CustomPlacementMode.Right,
                        CustomPlacementMode.Left,
                        CustomPlacementMode.Top,
                        CustomPlacementMode.Bottom
                    };
                case CustomPlacementMode.Full:
                    return new[] { CustomPlacementMode.Full };
                case CustomPlacementMode.TopEdgeAlignedLeft:
                    return new[]
                    {
                        CustomPlacementMode.TopEdgeAlignedLeft,
                        CustomPlacementMode.BottomEdgeAlignedLeft,
                        CustomPlacementMode.LeftEdgeAlignedTop,
                        CustomPlacementMode.RightEdgeAlignedTop
                    };
                case CustomPlacementMode.TopEdgeAlignedRight:
                    return new[]
                    {
                        CustomPlacementMode.TopEdgeAlignedRight,
                        CustomPlacementMode.BottomEdgeAlignedRight,
                        CustomPlacementMode.LeftEdgeAlignedBottom,
                        CustomPlacementMode.RightEdgeAlignedBottom
                    };
                case CustomPlacementMode.BottomEdgeAlignedLeft:
                    return new[]
                    {
                        CustomPlacementMode.BottomEdgeAlignedLeft,
                        CustomPlacementMode.TopEdgeAlignedLeft,
                        CustomPlacementMode.LeftEdgeAlignedTop,
                        CustomPlacementMode.RightEdgeAlignedTop
                    };
                case CustomPlacementMode.BottomEdgeAlignedRight:
                    return new[]
                    {
                        CustomPlacementMode.BottomEdgeAlignedRight,
                        CustomPlacementMode.TopEdgeAlignedRight,
                        CustomPlacementMode.LeftEdgeAlignedBottom,
                        CustomPlacementMode.RightEdgeAlignedBottom
                    };
                case CustomPlacementMode.LeftEdgeAlignedTop:
                    return new[]
                    {
                        CustomPlacementMode.LeftEdgeAlignedTop,
                        CustomPlacementMode.RightEdgeAlignedTop,
                        CustomPlacementMode.TopEdgeAlignedLeft,
                        CustomPlacementMode.BottomEdgeAlignedLeft
                    };
                case CustomPlacementMode.LeftEdgeAlignedBottom:
                    return new[]
                    {
                        CustomPlacementMode.LeftEdgeAlignedBottom,
                        CustomPlacementMode.RightEdgeAlignedBottom,
                        CustomPlacementMode.TopEdgeAlignedRight,
                        CustomPlacementMode.BottomEdgeAlignedRight
                    };
                case CustomPlacementMode.RightEdgeAlignedTop:
                    return new[]
                    {
                        CustomPlacementMode.RightEdgeAlignedTop,
                        CustomPlacementMode.LeftEdgeAlignedTop,
                        CustomPlacementMode.TopEdgeAlignedLeft,
                        CustomPlacementMode.BottomEdgeAlignedLeft
                    };
                case CustomPlacementMode.RightEdgeAlignedBottom:
                    return new[]
                    {
                        CustomPlacementMode.RightEdgeAlignedBottom,
                        CustomPlacementMode.LeftEdgeAlignedBottom,
                        CustomPlacementMode.TopEdgeAlignedRight,
                        CustomPlacementMode.BottomEdgeAlignedRight
                    };
                //case CustomPopupPlacementMode.Auto:
                default:
                    throw new ArgumentOutOfRangeException(nameof(placement));
            }
        }
    }
}
