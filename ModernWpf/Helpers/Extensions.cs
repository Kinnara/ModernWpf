using System.Windows.Media;

namespace ModernWpf
{
    internal static class Extensions
    {
        public static GeneralTransform SafeTransformToVisual(this Visual self, Visual visual)
        {
            if (self.FindCommonVisualAncestor(visual) != null)
            {
                return self.TransformToVisual(visual);
            }
            return Transform.Identity;
        }
    }
}
