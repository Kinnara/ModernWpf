using System;
using System.Windows.Media;
using System.Windows.Threading;

namespace ModernWpf
{
    internal static class Extensions
    {
        private static readonly Action NoOpCallback = delegate { };

        public static GeneralTransform SafeTransformToVisual(this Visual self, Visual visual)
        {
            if (self.FindCommonVisualAncestor(visual) != null)
            {
                return self.TransformToVisual(visual);
            }
            return Transform.Identity;
        }

        public static void Wait(this Dispatcher dispatcher, DispatcherPriority priority)
        {
            dispatcher.Invoke(NoOpCallback, priority);
        }
    }
}
