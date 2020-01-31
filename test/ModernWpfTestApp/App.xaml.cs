using ModernWpf.Controls;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Navigation;

namespace ModernWpfTestApp
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var frame = new ThemeAwareFrame
            {
                Source = new Uri("MainPage.xaml", UriKind.Relative),
            };

            var window = new Window
            {
                Content = frame,
                WindowState = WindowState.Maximized
            };
            window.SetBinding(TitleBar.IsBackButtonVisibleProperty, new Binding("CanGoBack") { Source = frame });
            TitleBar.AddBackRequestedHandler(window, delegate { frame.GoBack(); });
            window.Show();
        }
    }
}
