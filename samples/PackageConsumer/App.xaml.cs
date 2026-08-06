using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using ModernWpf.Controls;

namespace ModernWpf.PackageConsumer
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            if (Array.IndexOf(e.Args, "--smoke-test") < 0)
            {
                return;
            }

            try
            {
                if (!Resources.Contains("SystemControlBackgroundChromeMediumLowBrush"))
                {
                    throw new InvalidOperationException(
                        "Expected ModernWpf theme resources were not resolved.");
                }

                var button = new Button { Content = "Styled button" };
                var navigationView = new NavigationView
                {
                    Content = new TextBlock { Text = "Packaged control content" }
                };
                navigationView.MenuItems.Add(new NavigationViewItem { Content = "Home" });

                var root = new StackPanel();
                root.Children.Add(button);
                root.Children.Add(navigationView);

                var window = new Window
                {
                    Content = root,
                    Width = 320,
                    Height = 200,
                    WindowStyle = WindowStyle.None,
                    ShowInTaskbar = false,
                    ShowActivated = false,
                    Opacity = 0
                };

                MainWindow = window;
                window.Show();
                window.UpdateLayout();
                Dispatcher.Invoke(() => { }, DispatcherPriority.ContextIdle);

                if (button.Template == null || navigationView.Template == null)
                {
                    throw new InvalidOperationException(
                        "Packaged WPF and ModernWpf control templates were not applied.");
                }

                window.Close();
                Shutdown(0);
            }
            catch (Exception exception)
            {
                Console.Error.WriteLine(exception);
                Shutdown(1);
            }
        }
    }
}
