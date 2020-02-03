using System.Windows;

namespace ModernWpf.SampleApp
{
    public static class Extensions
    {
        public static void ToggleTheme(this FrameworkElement element)
        {
            ElementTheme newTheme;
            if (ThemeManager.GetActualTheme(element) == ElementTheme.Dark)
            {
                newTheme = ElementTheme.Light;
            }
            else
            {
                newTheme = ElementTheme.Dark;
            }
            ThemeManager.SetRequestedTheme(element, newTheme);
        }
    }
}
