using ModernWpf.SampleApp.ThreadedUI;
using SamplesCommon;
using System;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace ModernWpf.SampleApp.ControlPages
{
    public partial class ThreadedUIPage
    {
        public ThreadedUIPage()
        {
            InitializeComponent();
            DataContext = ViewModel;
        }

        public ThreadedUIPageViewModel ViewModel { get; } = new ThreadedUIPageViewModel();

        private void ProgressControlHost_ChildChanged(object sender, EventArgs e)
        {
            var host = (ThreadedVisualHost)sender;
            if (host.Child is ThreadedProgressBar progressBar)
            {
                progressBar.SetBinding(ThreadedProgressBar.IsIndeterminateProperty, new Binding(nameof(ViewModel.IsBusy)) { Source = ViewModel });
            }
            else if (host.Child is ThreadedProgressRing progressRing)
            {
                progressRing.SetBinding(ThreadedProgressRing.IsActiveProperty, new Binding(nameof(ViewModel.IsBusy)) { Source = ViewModel });
            }
        }

        private void BlockMainThread(object sender, RoutedEventArgs e)
        {
            Thread.Sleep(3000);
        }

        private void LoadOrUnload(object sender, RoutedEventArgs e)
        {
            if (ProgressBarExample.Example != null)
            {
                ProgressBarExample.Example = null;
                ((Button)sender).Content = "Load";
            }
            else
            {
                ProgressBarExample.Example = ProgressControlHost;
                ((Button)sender).Content = "Unload";
            }
        }

        private void ToggleChildType(object sender, RoutedEventArgs e)
        {
            if (ProgressControlHost.ChildType == typeof(ThreadedProgressBar))
            {
                ProgressControlHost.ChildType = typeof(ThreadedProgressRing);
            }
            else
            {
                ProgressControlHost.ChildType = typeof(ThreadedProgressBar);
            }
        }
    }

    public class ThreadedUIPageViewModel : BindableBase
    {
        private bool _isBusy = true;
        public bool IsBusy
        {
            get => _isBusy;
            set => Set(ref _isBusy, value);
        }

        private bool _isVisbile = true;
        public bool IsVisible
        {
            get => _isVisbile;
            set => Set(ref _isVisbile, value);
        }
    }
}
