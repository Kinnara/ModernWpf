using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using ModernWpf;
using ModernWpf.Gallery.Testing;

namespace ModernWpf.Gallery
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var options = GalleryLaunchOptions.Parse(e.Args);
            GalleryDiagnostics.Configure(options);
            if (options.VisualTestMode)
            {
                System.Windows.Media.RenderOptions.ProcessRenderMode =
                    System.Windows.Interop.RenderMode.SoftwareOnly;
                AttachVisualTestExceptionLogging();
            }

            ApplyTheme(options.Theme);

            var window = new MainWindow();
            MainWindow = window;
            window.Show();

            if (!string.IsNullOrWhiteSpace(options.InitialRoute))
            {
                window.NavigateTo(options.InitialRoute);
            }
        }

        private void AttachVisualTestExceptionLogging()
        {
            DispatcherUnhandledException += OnDispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
            TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;
        }

        private static void ApplyTheme(string theme)
        {
            if (string.Equals(theme, "Light", StringComparison.OrdinalIgnoreCase))
            {
                ThemeManager.Current.ApplicationTheme = ApplicationTheme.Light;
            }
            else if (string.Equals(theme, "Dark", StringComparison.OrdinalIgnoreCase))
            {
                ThemeManager.Current.ApplicationTheme = ApplicationTheme.Dark;
            }
        }

        private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            GalleryDiagnostics.RecordException(e.Exception);
            e.Handled = true;
        }

        private void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            GalleryDiagnostics.RecordException(e.ExceptionObject as Exception);
        }

        private void OnUnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            GalleryDiagnostics.RecordException(e.Exception);
            e.SetObserved();
        }
    }
}
