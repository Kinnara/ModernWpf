using System.Windows;
using ModernWpf;

namespace ModernWpf.Gallery.Pages.WpfGallery.DesignGuidance
{
    internal static class DesignImageTheme
    {
        public static ElementTheme Resolve(FrameworkElement element)
        {
            var applicationTheme = ThemeManager.Current.ApplicationTheme;
            if (applicationTheme == ApplicationTheme.Dark)
            {
                return ElementTheme.Dark;
            }

            if (applicationTheme == ApplicationTheme.Light)
            {
                return ElementTheme.Light;
            }

            return ThemeManager.GetActualTheme(element) == ElementTheme.Dark
                ? ElementTheme.Dark
                : ElementTheme.Light;
        }
    }
}
