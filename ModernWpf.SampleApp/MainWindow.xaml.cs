using ModernWpf.Controls;
using ModernWpf.SampleApp.Common;
using ModernWpf.SampleApp.DataModel;
using ModernWpf.SampleApp.Helper;
using ModernWpf.SampleApp.Helpers;
using ModernWpf.SampleApp.Properties;
using SamplesCommon;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Interop;

namespace ModernWpf.SampleApp
{
    public partial class MainWindow
    {
        public static MainWindow Current;

        public MainWindow()
        {
            Current = this;
            InitializeComponent();
            InitialzeApp();
        }

        private async void InitialzeApp()
        {
            await Task.Delay(1);
            WindowHelper.TrackWindow(this);
            ThemeHelper.Initialize();
            await ControlInfoDataSource.Instance.GetGroupsAsync();
            SubscribeToResourcesChanged();
            NavigationRootPage NavigationRootPage = new NavigationRootPage();
            Content = NavigationRootPage;
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            DispatcherHelper.RunOnMainThread(() =>
            {
                if (this == Application.Current.MainWindow)
                {
                    this.SetPlacement(Settings.Default.MainWindowPlacement);
                }
            });
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);

            if (!e.Cancel)
            {
                DispatcherHelper.RunOnMainThread(() =>
                {
                    if (this == Application.Current.MainWindow)
                    {
                        Settings.Default.MainWindowPlacement = this.GetPlacement();
                        Settings.Default.Save();
                    }
                });
            }
        }

        /*protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);

            if (e.Property == FocusManager.FocusedElementProperty)
            {
                Debug.WriteLine("FocusedElement: " + e.NewValue);
            }
        }*/

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
