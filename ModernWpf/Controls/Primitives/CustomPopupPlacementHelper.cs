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
            FrameworkElement child = null)
        {
            Matrix transformToDevice = default;
            if (child != null)
            {
                Helper.TryGetTransformToDevice(child, out transformToDevice);
            }

            CustomPopupPlacement preferredPlacement = CalculatePopupPlacement(placement, popupSize, targetSize, offset, child, transformToDevice);

            CustomPopupPlacement? alternativePlacement = null;
            var alternativePlacementMode = GetAlternativePlacementMode(placement);
            if (alternativePlacementMode.HasValue)
            {
                alternativePlacement = CalculatePopupPlacement(alternativePlacementMode.Value, popupSize, targetSize, offset, child, transformToDevice);
            }

            if (alternativePlacement.HasValue)
            {
                return new[] { preferredPlacement, alternativePlacement.Value };
            }
            else
            {
                return new[] { preferredPlacement };
            }
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

        private static CustomPlacementMode? GetAlternativePlacementMode(CustomPlacementMode placement)
        {
            switch (placement)
            {
                case CustomPlacementMode.Top:
                    return CustomPlacementMode.Bottom;
                case CustomPlacementMode.Bottom:
                    return CustomPlacementMode.Top;
                case CustomPlacementMode.Left:
                    return CustomPlacementMode.Right;
                case CustomPlacementMode.Right:
                    return CustomPlacementMode.Left;
                case CustomPlacementMode.Full:
                    return null;
                case CustomPlacementMode.TopEdgeAlignedLeft:
                    return CustomPlacementMode.BottomEdgeAlignedLeft;
                case CustomPlacementMode.TopEdgeAlignedRight:
                    return CustomPlacementMode.BottomEdgeAlignedRight;
                case CustomPlacementMode.BottomEdgeAlignedLeft:
                    return CustomPlacementMode.TopEdgeAlignedLeft;
                case CustomPlacementMode.BottomEdgeAlignedRight:
                    return CustomPlacementMode.TopEdgeAlignedRight;
                case CustomPlacementMode.LeftEdgeAlignedTop:
                    return CustomPlacementMode.RightEdgeAlignedTop;
                case CustomPlacementMode.LeftEdgeAlignedBottom:
                    return CustomPlacementMode.RightEdgeAlignedBottom;
                case CustomPlacementMode.RightEdgeAlignedTop:
                    return CustomPlacementMode.RightEdgeAlignedTop;
                case CustomPlacementMode.RightEdgeAlignedBottom:
                    return CustomPlacementMode.LeftEdgeAlignedBottom;
                //case CustomPopupPlacementMode.Auto:
                default:
                    return null;
            }
        }
    }
}
