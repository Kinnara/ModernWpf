using System.ComponentModel;
using System.Windows;

namespace ModernWpf.Controls
{
    /// <summary>
    /// Provides the WinUI-compatible AnimatedIcon.State attached property.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class AnimatedIcon
    {
        public static readonly DependencyProperty StateProperty =
            DependencyProperty.RegisterAttached(
                "State",
                typeof(string),
                typeof(AnimatedIcon),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.Inherits));

        public static string GetState(DependencyObject element)
        {
            return (string)element.GetValue(StateProperty);
        }

        public static void SetState(DependencyObject element, string value)
        {
            element.SetValue(StateProperty, value);
        }
    }
}
