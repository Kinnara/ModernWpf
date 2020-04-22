using System.Windows;

namespace ModernWpf
{
    internal sealed class CoreApplicationViewTitleBar
    {
        internal CoreApplicationViewTitleBar()
        {
        }

        public bool ExtendViewIntoTitleBar { get; set; }
        public double Height { get; } = 32;
        public bool IsVisible { get; } = true;
        public double SystemOverlayLeftInset { get; }
        public double SystemOverlayRightInset { get; }

        public event TypedEventHandler<CoreApplicationViewTitleBar, object> IsVisibleChanged;
        public event TypedEventHandler<CoreApplicationViewTitleBar, object> LayoutMetricsChanged;

        internal static CoreApplicationViewTitleBar GetTitleBar(DependencyObject dependencyObject)
        {
            var window = Window.GetWindow(dependencyObject) ?? Application.Current.MainWindow;
            return new CoreApplicationViewTitleBar();
        }
    }
}
