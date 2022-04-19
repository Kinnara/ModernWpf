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

        public static object GetProperty(this object item, string name) => item.GetType()?.GetProperty(name)?.GetValue(item, null);

        public static void SetProperty(this object item, string name, object value) => item.GetType()?.GetProperty(name)?.SetValue(item, value);
    }
}
