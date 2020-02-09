using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Interop;

namespace MUXControlsTestApp
{
    internal static class VisualTreeHelperEx
    {
        public static IReadOnlyList<Popup> GetOpenPopups()
        {
            return PresentationSource.CurrentSources
                .OfType<HwndSource>()
                .Select(h => h.RootVisual)
                .OfType<FrameworkElement>()
                .Select(f => f.Parent)
                .OfType<Popup>()
                .Where(p => p.IsOpen)
                .ToList()
                .AsReadOnly();
        }
    }
}
