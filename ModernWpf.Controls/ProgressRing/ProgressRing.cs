using System;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using ModernWpf.Automation.Peers;
using ModernWpf.Controls.Primitives;

namespace ModernWpf.Controls
{
    public class ProgressRing : Control
    {
        const string s_ActiveStateName = "Active";
        const string s_DeterminateActiveStateName = "DeterminateActive";
        const string s_InactiveStateName = "Inactive";
        const string s_SmallStateName = "Small";
        const string s_LargeStateName = "Large";

        static ProgressRing()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ProgressRing), new FrameworkPropertyMetadata(typeof(ProgressRing)));
        }

        public ProgressRing()
        {
            SetValue(TemplateSettingsPropertyKey, new ProgressRingTemplateSettings());

            SizeChanged += OnSizeChanged;
        }

        #region IsActive

        public bool IsActive
        {
            get => (bool)GetValue(IsActiveProperty);
            set => SetValue(IsActiveProperty, value);
        }

        public static readonly DependencyProperty IsActiveProperty =
            DependencyProperty.Register(
                nameof(IsActive),
                typeof(bool),
                typeof(ProgressRing),
                new PropertyMetadata(true, OnIsActivePropertyChanged));

        private static void OnIsActivePropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            ((ProgressRing)sender).OnIsActivePropertyChanged(args);
        }

        #endregion

        #region IsIndeterminate

        public bool IsIndeterminate
        {
            get => (bool)GetValue(IsIndeterminateProperty);
            set => SetValue(IsIndeterminateProperty, value);
        }

        public static readonly DependencyProperty IsIndeterminateProperty =
            DependencyProperty.Register(
                nameof(IsIndeterminate),
                typeof(bool),
                typeof(ProgressRing),
                new PropertyMetadata(true, OnIsActivePropertyChanged));

        #endregion

        #region Maximum

        public double Maximum
        {
            get => (double)GetValue(MaximumProperty);
            set => SetValue(MaximumProperty, value);
        }

        public static readonly DependencyProperty MaximumProperty =
            DependencyProperty.Register(
                nameof(Maximum),
                typeof(double),
                typeof(ProgressRing),
                new FrameworkPropertyMetadata(OnMaximumPropertyChanged));

        private static void OnMaximumPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            ((ProgressRing)sender).OnRangeBasePropertyChanged(args);
        }

        #endregion

        #region Minimum

        public double Minimum
        {
            get => (double)GetValue(MinimumProperty);
            set => SetValue(MinimumProperty, value);
        }

        public static readonly DependencyProperty MinimumProperty =
            DependencyProperty.Register(
                nameof(Minimum),
                typeof(double),
                typeof(ProgressRing),
                new FrameworkPropertyMetadata(OnMinimumPropertyChanged));

        private static void OnMinimumPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            ((ProgressRing)sender).OnRangeBasePropertyChanged(args);
        }

        #endregion

        #region TemplateSettings

        private static readonly DependencyPropertyKey TemplateSettingsPropertyKey =
            DependencyProperty.RegisterReadOnly(
                nameof(TemplateSettings),
                typeof(ProgressRingTemplateSettings),
                typeof(ProgressRing),
                null);

        public static readonly DependencyProperty TemplateSettingsProperty =
            TemplateSettingsPropertyKey.DependencyProperty;

        public ProgressRingTemplateSettings TemplateSettings
        {
            get => (ProgressRingTemplateSettings)GetValue(TemplateSettingsProperty);
        }

        #endregion

        #region Value

        public double Value
        {
            get => (double)GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
        }

        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register(
                nameof(Value),
                typeof(double),
                typeof(ProgressRing),
                new FrameworkPropertyMetadata(OnValuePropertyChanged));

        private static void OnValuePropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            ((ProgressRing)sender).OnRangeBasePropertyChanged(args);
        }
        
        #endregion

        #region StrokeThickness

        public double StrokeThickness
        {
            get => (double)GetValue(StrokeThicknessProperty);
            set => SetValue(StrokeThicknessProperty, value);
        }

        public static readonly DependencyProperty StrokeThicknessProperty =
            DependencyProperty.Register(
                nameof(StrokeThickness),
                typeof(double),
                typeof(ProgressRing),
                null);

        #endregion

        protected override AutomationPeer OnCreateAutomationPeer()
        {
            return new ProgressRingAutomationPeer(this);
        }

        void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            ChangeVisualState();
        }

        void OnIsActivePropertyChanged(DependencyPropertyChangedEventArgs args)
        {
            ChangeVisualState();
        }

        void OnRangeBasePropertyChanged(DependencyPropertyChangedEventArgs args)
        {
            UpdateRange();
        }

        void UpdateRange()
        {
            var templateSettings = TemplateSettings;

            double minimum = Minimum;
            double range = Maximum - minimum;
            double delta = Value - minimum;

            double normalizedRange = (range == 0.0) ? 0.0 : (delta / range);

            templateSettings.NormalizedRange = normalizedRange;
        }

        void ChangeVisualState()
        {
            VisualStateManager.GoToState(this, IsActive ? (IsIndeterminate ? s_ActiveStateName : s_DeterminateActiveStateName) : s_InactiveStateName, true);
            VisualStateManager.GoToState(this, TemplateSettings.MaxSideLength < 60 ? s_SmallStateName : s_LargeStateName, true);
        }

    }
}
