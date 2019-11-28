using System.Windows;

namespace ModernWpf.Controls.Primitives
{
    internal enum CustomPopupPlacementMode
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

    internal static class PopupPlacementHelper
    {
        #region Placement

        public static readonly DependencyProperty PlacementProperty =
            DependencyProperty.RegisterAttached(
                "Placement",
                typeof(CustomPopupPlacementMode),
                typeof(PopupPlacementHelper),
                new PropertyMetadata(CustomPopupPlacementMode.Top));

        public static CustomPopupPlacementMode GetPlacement(DependencyObject element)
        {
            return (CustomPopupPlacementMode)element.GetValue(PlacementProperty);
        }

        public static void SetPlacement(DependencyObject element, CustomPopupPlacementMode value)
        {
            element.SetValue(PlacementProperty, value);
        }

        #endregion
    }
}
