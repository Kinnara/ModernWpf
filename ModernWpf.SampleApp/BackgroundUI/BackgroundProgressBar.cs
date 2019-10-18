using ModernWpf.Controls.Primitives;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ModernWpf.SampleApp.Controls
{
    public class BackgroundProgressBar : ControlWrapperBase<ProgressBar>
    {
        private readonly ProgressBarPropertyValues _propertyValues = new ProgressBarPropertyValues();

        public BackgroundProgressBar() : base(BackgroundUIHelper.Dispatcher)
        {
        }

        #region Foreground

        public static readonly DependencyProperty ForegroundProperty =
            ProgressBar.ForegroundProperty.AddOwner(typeof(BackgroundProgressBar));

        public Brush Foreground
        {
            get => (Brush)GetValue(ForegroundProperty);
            set => SetValue(ForegroundProperty, value);
        }

        #endregion

        #region Background

        public static readonly DependencyProperty BackgroundProperty =
            ProgressBar.BackgroundProperty.AddOwner(typeof(BackgroundProgressBar));

        public Brush Background
        {
            get => (Brush)GetValue(BackgroundProperty);
            set => SetValue(BackgroundProperty, value);
        }

        #endregion

        #region BorderBrush

        public static readonly DependencyProperty BorderBrushProperty =
            ProgressBar.BorderBrushProperty.AddOwner(typeof(BackgroundProgressBar));

        public Brush BorderBrush
        {
            get => (Brush)GetValue(BorderBrushProperty);
            set => SetValue(BorderBrushProperty, value);
        }

        #endregion

        #region BorderThickness

        public static readonly DependencyProperty BorderThicknessProperty =
            ProgressBar.BorderThicknessProperty.AddOwner(typeof(BackgroundProgressBar));

        public Thickness BorderThickness
        {
            get => (Thickness)GetValue(BorderThicknessProperty);
            set => SetValue(BorderThicknessProperty, value);
        }

        #endregion

        #region IsIndeterminate

        public static readonly DependencyProperty IsIndeterminateProperty =
            ProgressBar.IsIndeterminateProperty.AddOwner(typeof(BackgroundProgressBar));

        public bool IsIndeterminate
        {
            get => (bool)GetValue(IsIndeterminateProperty);
            set => SetValue(IsIndeterminateProperty, value);
        }

        #endregion

        #region CornerRadius

        public static readonly DependencyProperty CornerRadiusProperty =
            ControlHelper.CornerRadiusProperty.AddOwner(typeof(BackgroundProgressBar));

        public CornerRadius CornerRadius
        {
            get => (CornerRadius)GetValue(CornerRadiusProperty);
            set => SetValue(CornerRadiusProperty, value);
        }

        #endregion

        protected override ControlPropertyValues PropertyValues => _propertyValues;

        protected override void BindProperties()
        {
            base.BindProperties();
            Bind(ProgressBar.ForegroundProperty);
            Bind(ProgressBar.BackgroundProperty);
            Bind(ProgressBar.BorderBrushProperty);
            Bind(ProgressBar.BorderThicknessProperty);
            Bind(ProgressBar.IsIndeterminateProperty);
            Bind(ControlHelper.CornerRadiusProperty);
        }

        private class ProgressBarPropertyValues : ControlPropertyValues
        {
            public Brush Foreground => GetValue<Brush>();
            public Brush Background => GetValue<Brush>();
            public Brush BorderBrush => GetValue<Brush>();
            public Thickness BorderThickness => GetValue<Thickness>();
            public bool IsIndeterminate => GetValue<bool>();
            public CornerRadius CornerRadius => GetValue<CornerRadius>();
        }
    }
}
