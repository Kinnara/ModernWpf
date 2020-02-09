using ModernWpf.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace MUXControlsTestApp
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private string _currentLanguage = "en-US";

        public App()
        {
            Activated += OnFirstActivated;
        }

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

        // Home for arbitrary test content so people don't muck with Window.Current.Content
        public static UIElement TestContentRoot
        {
            get
            {
                var rootFrame = Current.MainWindow.Content as TestFrame;
                return rootFrame.Content as UIElement;
            }
            set
            {
                var rootFrame = Current.MainWindow.Content as TestFrame;
                if (value != null)
                {
                    rootFrame.Content = value;
                }
                else
                {
                    rootFrame.NavigateWithoutAnimation(typeof(MainPage));
                }
            }
        }

        private bool _isSplashScreenDismissed;
        private bool _isRootCreated = false;
        private List<Action> _actionsToRunAfterSplashScreenDismissedAndRootIsCreated = new List<Action>();

        public static void RunAfterSplashScreenDismissed(Action action)
        {
            var app = Application.Current as App;
            lock (app._actionsToRunAfterSplashScreenDismissedAndRootIsCreated)
            {
                if (app._isSplashScreenDismissed && app._isRootCreated)
                {
                    action();
                }
                else
                {
                    app._actionsToRunAfterSplashScreenDismissedAndRootIsCreated.Add(action);
                }
            }
        }

        private void SplashScreenDismissedAndRootCreated()
        {
            lock (_actionsToRunAfterSplashScreenDismissedAndRootIsCreated)
            {
                foreach (var action in _actionsToRunAfterSplashScreenDismissedAndRootIsCreated)
                {
                    action();
                }
                _actionsToRunAfterSplashScreenDismissedAndRootIsCreated.Clear();
            }
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var frame = new TestFrame
            {
                Source = new Uri("MainPage.xaml", UriKind.Relative),
            };

            var window = new Window
            {
                Content = frame,
                WindowState = WindowState.Maximized,
                UseLayoutRounding = true
            };
            window.SetBinding(TitleBar.IsBackButtonVisibleProperty, new Binding("CanGoBack") { Source = frame });
            TitleBar.AddBackRequestedHandler(window, delegate { frame.GoBack(); });
            MainWindow = window;
            _isRootCreated = true;

            window.Show();
        }

        private void OnFirstActivated(object sender, EventArgs e)
        {
            Activated -= OnFirstActivated;

            _isSplashScreenDismissed = true;
            SplashScreenDismissedAndRootCreated();
        }
    }
}
