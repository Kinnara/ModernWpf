using System.Windows;
using System.Windows.Shell;

namespace ModernWpf.Controls.Primitives
{
    internal sealed class ModernWindowChrome : WindowChrome
    {
        public ModernWindowChrome()
        {
            // High Contrast swaps in an explicit None-edge chrome resource.
            // Keep the normal resource OS-only so it restores correctly.
            NonClientFrameEdges = GetPreferredNonClientFrameEdges(
                isHighContrast: false,
                OSVersionHelper.IsWindows11OrGreater);
        }

        internal static NonClientFrameEdges GetPreferredNonClientFrameEdges(
            bool isHighContrast,
            bool isWindows11OrGreater)
        {
            if (isHighContrast || !isWindows11OrGreater)
            {
                return NonClientFrameEdges.None;
            }

            return NonClientFrameEdges.Left |
                NonClientFrameEdges.Right |
                NonClientFrameEdges.Bottom;
        }

        protected override Freezable CreateInstanceCore()
        {
            return new ModernWindowChrome();
        }
    }
}
