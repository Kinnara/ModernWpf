using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Media;

namespace ModernWpf
{
    internal static class Helper
    {
        public static bool TryGetScaleFactors(Visual visual, out double scaleX, out double scaleY)
        {
            var presentationSource = PresentationSource.FromVisual(visual);
            if (presentationSource != null)
            {
                var transformToDevice = presentationSource.CompositionTarget.TransformToDevice;
                scaleX = transformToDevice.M11;
                scaleY = transformToDevice.M22;
                return true;
            }

            scaleX = default;
            scaleY = default;
            return false;
        }
    }
}
