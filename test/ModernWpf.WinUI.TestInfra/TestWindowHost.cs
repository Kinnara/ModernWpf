using System;
using System.Windows;

namespace ModernWpf.WinUI.TestInfra;

public sealed class TestWindowHost : IDisposable
{
    private readonly Window? previousMainWindow;

    public TestWindowHost(FrameworkElement content, double width = 1024, double height = 768)
    {
        Window = new Window
        {
            Width = width,
            Height = height,
            Left = -32000,
            Top = -32000,
            ShowInTaskbar = false,
            WindowStartupLocation = WindowStartupLocation.Manual,
            Content = content
        };

        var app = Application.Current;
        if (app != null)
        {
            previousMainWindow = app.MainWindow;
            app.MainWindow = Window;
        }

        Window.Show();
        WpfTestHost.DoEvents();
        Window.UpdateLayout();
        WpfTestHost.DoEvents();
    }

    public Window Window { get; }

    public void Dispose()
    {
        var app = Application.Current;
        if (app != null && ReferenceEquals(app.MainWindow, Window))
        {
            app.MainWindow = previousMainWindow;
        }

        Window.Content = null;
        Window.Close();
        WpfTestHost.DoEvents();
    }
}
