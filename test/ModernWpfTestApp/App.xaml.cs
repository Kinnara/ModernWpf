using ModernWpf.Controls;
using System;
using System.Diagnostics;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace ModernWpfTestApp
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private string _currentLanguage = "en-US";

        public static string LanguageOverride
        {
            get
            {
                return ((App)Current)._currentLanguage;
            }
            set
            {
                var culture = new CultureInfo(value);
                CultureInfo.CurrentCulture = culture;
                CultureInfo.CurrentUICulture = culture;
                CultureInfo.DefaultThreadCurrentCulture = culture;
                CultureInfo.DefaultThreadCurrentUICulture = culture;

                Debug.Assert(Current.MainWindow != null);
                Current.MainWindow.Language = XmlLanguage.GetLanguage(value);

                ((App)Current)._currentLanguage = value;
            }
        }

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
