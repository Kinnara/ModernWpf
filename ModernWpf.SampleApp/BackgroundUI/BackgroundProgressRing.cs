using ModernWpf.Controls;
using System.Windows;
using System.Windows.Media;

namespace ModernWpf.SampleApp.Controls
{
    public partial class BackgroundProgressRing : ControlWrapperBase<ProgressRing>
    {
        private readonly ProgressRingPropertyValues _propertyValues = new ProgressRingPropertyValues();

        public BackgroundProgressRing() : base(BackgroundUIHelper.Dispatcher)
        {
        }

        #region Foreground

        public static readonly DependencyProperty ForegroundProperty =
            ProgressRing.ForegroundProperty.AddOwner(typeof(BackgroundProgressRing));

        public Brush Foreground
        {
            get => (Brush)GetValue(ForegroundProperty);
            set => SetValue(ForegroundProperty, value);
        }

        #endregion

        #region IsActive

        public static readonly DependencyProperty IsActiveProperty =
            ProgressRing.IsActiveProperty.AddOwner(typeof(BackgroundProgressRing));

        public bool IsActive
        {
            get => (bool)GetValue(IsActiveProperty);
            set => SetValue(IsActiveProperty, value);
        }

        #endregion

        protected override void BindProperties()
        {
            base.BindProperties();
            Bind(ProgressRing.ForegroundProperty);
            Bind(ProgressRing.IsActiveProperty);
        }

        protected override ControlPropertyValues PropertyValues => _propertyValues;

        private class ProgressRingPropertyValues : ControlPropertyValues
        {
            public Brush Foreground => GetValue<Brush>();
            public bool IsActive => GetValue<bool>();
        }
    }
}
