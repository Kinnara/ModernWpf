using System.Windows;

namespace ModernWpf.WinUI.TestApp;

public static class TestApplication
{
    public static Application EnsureInitialized()
    {
        if (Application.Current == null)
        {
            var app = new App
            {
                ShutdownMode = ShutdownMode.OnExplicitShutdown
            };

            app.InitializeComponent();
        }

        return Application.Current!;
    }
}
