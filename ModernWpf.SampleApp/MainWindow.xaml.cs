using ModernWpf.Controls;
using ModernWpf.SampleApp.Helpers;
using ModernWpf.SampleApp.Properties;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Windows;
using System.Windows.Data;

namespace ModernWpf.SampleApp
{
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();

            SubscribeToResourcesChanged();
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            this.SetPlacement(Settings.Default.MainWindowPlacement);
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            if (!e.Cancel)
            {
                Settings.Default.MainWindowPlacement = this.GetPlacement();
                Settings.Default.Save();
            }
        }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);

            if (e.Property == TitleBar.ExtendViewIntoTitleBarProperty)
            {
                UpdateTitleBar();
            }
        }

        private void UpdateTitleBar()
        {
            bool useCustomTitleBar = TitleBar.GetExtendViewIntoTitleBar(this);
            if (useCustomTitleBar)
            {
                CustomTitleBar.Visibility = Visibility.Visible;
                TitleBar.SetBackButtonStyle(this, (Style)Resources["CustomTitleBarBackButtonStyle"]);
                TitleBar.SetButtonStyle(this, (Style)Resources["CustomTitleBarButtonStyle"]);
                TitleBar.SetIsBackButtonVisible(this, true);
            }
            else
            {
                CustomTitleBar.Visibility = Visibility.Collapsed;
                ClearValue(TitleBar.BackButtonStyleProperty);
                ClearValue(TitleBar.ButtonStyleProperty);
                SetBinding(TitleBar.IsBackButtonVisibleProperty, new Binding("CanGoBack") { Source = RootFrame });
            }
        }

        private void OnBackRequested(object sender, BackRequestedEventArgs e)
        {
            GoBack();
        }

        private void GoBack()
        {
            if (RootFrame.CanGoBack)
            {
                RootFrame.GoBack();
                //RootFrame.RemoveBackEntry();
            }
        }

        private void ForwardButton_Click(object sender, RoutedEventArgs e)
        {
            if (RootFrame.CanGoForward)
            {
                RootFrame.GoForward();
            }
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            if (RootFrame.Content is MainPage mainPage)
            {
                mainPage.Frame.Navigate(new Uri("ControlPages/WindowPage.xaml", UriKind.Relative));
            }
        }

        [Conditional("DEBUG")]
        private void SubscribeToResourcesChanged()
        {
            Type t = typeof(FrameworkElement);
            EventInfo ei = t.GetEvent("ResourcesChanged", BindingFlags.NonPublic | BindingFlags.Instance);
            Type tDelegate = ei.EventHandlerType;
            MethodInfo h = GetType().GetMethod(nameof(OnResourcesChanged), BindingFlags.NonPublic | BindingFlags.Instance);
            Delegate d = Delegate.CreateDelegate(tDelegate, this, h);
            MethodInfo addHandler = ei.GetAddMethod(true);
            object[] addHandlerArgs = { d };
            addHandler.Invoke(this, addHandlerArgs);
        }

        private void OnResourcesChanged(object sender, EventArgs e)
        {
        }
    }
}
