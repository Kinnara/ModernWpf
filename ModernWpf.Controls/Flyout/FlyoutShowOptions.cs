using System.Windows;

namespace ModernWpf.Controls.Primitives
{
    public class FlyoutShowOptions
    {
        public Rect? ExclusionRect { get; set; }

        public FlyoutPlacementMode Placement { get; set; } = FlyoutPlacementMode.Auto;

        public Point? Position { get; set; }

        public FlyoutShowMode ShowMode { get; set; } = FlyoutShowMode.Auto;
    }
}
