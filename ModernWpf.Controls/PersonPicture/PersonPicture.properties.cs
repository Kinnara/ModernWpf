using System.Windows;

namespace ModernWpf.Controls
{
    partial class PersonPicture
    {
        private static object CoerceStringProperty(DependencyObject d, object baseValue)
        {
            return baseValue ?? string.Empty;
        }
    }
}
