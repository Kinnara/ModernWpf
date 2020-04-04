using System.Windows;
using System.Windows.Controls;

namespace ModernWpf.SampleApp.ThreadedUI
{
    public partial class ThreadedProgressRing : UserControl
    {
        public ThreadedProgressRing()
        {
            InitializeComponent();
        }

        #region IsActive

        public static readonly DependencyProperty IsActiveProperty =
            DependencyProperty.Register(
                nameof(IsActive),
                typeof(bool),
                typeof(ThreadedProgressRing),
                null);

        public bool IsActive
        {
            get => (bool)GetValue(IsActiveProperty);
            set => SetValue(IsActiveProperty, value);
        }

        #endregion

        protected override Size MeasureOverride(Size constraint)
        {
            return base.MeasureOverride(constraint);
        }
    }
}
