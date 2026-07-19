using System;
using System.Windows.Controls;

namespace ModernWpf.Gallery.Pages
{
    internal static class WinUISampleSlider
    {
        public static Slider ShowValueFill(Slider slider)
        {
            if (slider == null)
            {
                throw new ArgumentNullException(nameof(slider));
            }

            slider.IsSelectionRangeEnabled = true;
            slider.SelectionStart = slider.Minimum;
            slider.SelectionEnd = slider.Value;

            Action updateValueFill = delegate
            {
                slider.SelectionStart = slider.Minimum;
                slider.SelectionEnd = slider.Value;
            };
            slider.ValueChanged += delegate { updateValueFill(); };
            return slider;
        }
    }
}
