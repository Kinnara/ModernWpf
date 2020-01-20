using ModernWpf.Controls;
using ModernWpf.SampleApp.Helpers;
using ModernWpf.SampleApp.Properties;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace ModernWpf.SampleApp
{
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();

            var rootFrame = NavigationRootPage.RootFrame;
            SetBinding(TitleBar.IsBackButtonVisibleProperty,
                new Binding { Path = new PropertyPath(Frame.CanGoBackProperty), Source = rootFrame });
            TitleBar.SetBackButtonCommand(this, NavigationCommands.BrowseBack);
            TitleBar.SetBackButtonCommandTarget(this, rootFrame);

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
