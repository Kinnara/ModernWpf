using System.ComponentModel;
using System.Windows;
using ModernWpf.Controls;

namespace ModernWpf.Theme.Tests;

internal static class ThemeTestApplication
{
    public static Application EnsureInitialized()
    {
        if (Application.Current == null)
        {
            var app = new Application
            {
                ShutdownMode = ShutdownMode.OnExplicitShutdown,
                Resources = new ResourceDictionary()
            };

            // Exercise the frozen resource path here; the other test applications use
            // the default mutable resource path.
            var themeResources = new ThemeResources
            {
                CanBeAccessedAcrossThreads = true
            };
            ((ISupportInitialize)themeResources).BeginInit();
            ((ISupportInitialize)themeResources).EndInit();

            app.Resources.MergedDictionaries.Add(themeResources);
            app.Resources.MergedDictionaries.Add(new FluentControlsResources());
        }

        return Application.Current!;
    }
}
