using System.Windows;
using System.Windows.Controls;

namespace ModernWpf.SampleApp.ThreadedUI
{
    public partial class ThreadedProgressBar : UserControl
    {
        public ThreadedProgressBar()
        {
            InitializeComponent();
        }

        #region IsIndeterminate

        public static readonly DependencyProperty IsIndeterminateProperty =
            DependencyProperty.Register(
                nameof(IsIndeterminate),
                typeof(bool),
                typeof(ThreadedProgressBar),
                null);

        public bool IsIndeterminate
        {
            get => (bool)GetValue(IsIndeterminateProperty);
            set => SetValue(IsIndeterminateProperty, value);
        }

        #endregion
    }
}
