using System;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using ModernWpf.Automation.Peers;
using ModernWpf.Controls.Primitives;

namespace ModernWpf.Controls
{
    public class ProgressRing : Control
    {
        const string s_LayoutRootName = "LayoutRoot";
        const string s_ActiveStateName = "Active";
        const string s_DeterminateActiveStateName = "DeterminateActive";
        const string s_InactiveStateName = "Inactive";

        static ProgressRing()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ProgressRing), new FrameworkPropertyMetadata(typeof(ProgressRing)));
        }

        public ProgressRing()
        {
            SetValue(TemplateSettingsPropertyKey, new ProgressRingTemplateSettings());

            SizeChanged += OnSizeChanged;
            Loaded += OnLoaded;
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
                new FrameworkPropertyMetadata(true, OnIsActivePropertyChanged));

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
                new FrameworkPropertyMetadata(true, OnIsIndeterminatePropertyChanged));

        private static void OnIsIndeterminatePropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            ((ProgressRing)sender).OnIsIndeterminatePropertyChanged(args);
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
                new FrameworkPropertyMetadata(0.0, OnValuePropertyChanged));

        private static void OnValuePropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            ((ProgressRing)sender).OnValuePropertyChanged(args);
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
                new FrameworkPropertyMetadata(0.0, OnMinimumPropertyChanged));

        private static void OnMinimumPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            ((ProgressRing)sender).OnMinimumPropertyChanged(args);
        }

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
                new FrameworkPropertyMetadata(100.0, OnMaximumPropertyChanged));

        private static void OnMaximumPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            ((ProgressRing)sender).OnMaximumPropertyChanged(args);
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

        protected override AutomationPeer OnCreateAutomationPeer()
        {
            return new ProgressRingAutomationPeer(this);
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            m_layoutRoot = GetTemplateChild(s_LayoutRootName) as FrameworkElement;

            UpdateLottieProgress();
            UpdateStates();
        }

        void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            ApplyTemplateSettings();
        }

        void OnLoaded(object sender, RoutedEventArgs e)
        {
            UpdateStates();
        }

        void OnIsActivePropertyChanged(DependencyPropertyChangedEventArgs args)
        {
            UpdateStates();
        }

        void OnIsIndeterminatePropertyChanged(DependencyPropertyChangedEventArgs args)
        {
            UpdateStates();
        }

        void OnValuePropertyChanged(DependencyPropertyChangedEventArgs args)
        {
            if (!m_rangePropertyUpdating)
            {
                m_rangePropertyUpdating = true;
                CoerceValue();
                m_rangePropertyUpdating = false;

                if (!IsIndeterminate)
                {
                    UpdateLottieProgress();
                }
            }
        }

        void OnMinimumPropertyChanged(DependencyPropertyChangedEventArgs args)
        {
            if (!m_rangePropertyUpdating)
            {
                m_rangePropertyUpdating = true;
                CoerceMaximum();
                CoerceValue();
                m_rangePropertyUpdating = false;

                if (!IsIndeterminate)
                {
                    UpdateLottieProgress();
                }
            }
        }

        void OnMaximumPropertyChanged(DependencyPropertyChangedEventArgs args)
        {
            if (!m_rangePropertyUpdating)
            {
                m_rangePropertyUpdating = true;
                CoerceMinimum();
                CoerceValue();
                m_rangePropertyUpdating = false;

                if (!IsIndeterminate)
                {
                    UpdateLottieProgress();
                }
            }
        }

        void UpdateStates()
        {
            if (m_layoutRoot != null)
            {
                m_layoutRoot.Opacity = IsActive ? 1.0 : 0.0;
            }

            if (IsActive)
            {
                VisualStateManager.GoToState(this, IsIndeterminate ? s_ActiveStateName : s_DeterminateActiveStateName, true);
            }
            else
            {
                VisualStateManager.GoToState(this, s_InactiveStateName, true);
            }
        }

        void ApplyTemplateSettings()
        {
            // TemplateSetting properties from WUXC for backwards compatibility.
            var templateSettings = TemplateSettings;

            var (width, diameterValue, anchorPoint) = calcSettings();
            (double, double, double) calcSettings()
            {
                if (ActualWidth != 0)
                {
                    double width = ActualWidth;

                    double diameterAdditive;
                    {
                        double init()
                        {
                            if (width <= 40.0)
                            {
                                return 1.0;
                            }
                            return 0.0;
                        }
                        diameterAdditive = init();
                    }

                    double diamaterValue = (width * 0.1) + diameterAdditive;
                    double anchorPoint = (width * 0.5) - diamaterValue;
                    return (width, diamaterValue, anchorPoint);
                }

                return (0.0, 0.0, 0.0);
            };

            templateSettings.EllipseDiameter = diameterValue;

            Thickness thicknessEllipseOffset = new Thickness(0, anchorPoint, 0, 0);

            templateSettings.EllipseOffset = thicknessEllipseOffset;
            templateSettings.MaxSideLength = width;
        }

        void CoerceMinimum()
        {
            if (Minimum > Maximum)
            {
                Minimum = Maximum;
            }
        }

        void CoerceMaximum()
        {
            if (Maximum < Minimum)
            {
                Maximum = Minimum;
            }
        }

        void CoerceValue()
        {
            var value = Value;
            if (!double.IsNaN(value) && !IsInBounds(value))
            {
                Value = value > Maximum ? Maximum : Minimum;
            }
        }

        bool IsInBounds(double value)
        {
            return value >= Minimum && value <= Maximum;
        }

        void UpdateLottieProgress()
        {
            // WPF has no WinUI AnimatedVisualPlayer/Lottie pipeline in this control.
            // The source state flow is kept, while the template provides a storyboard substitute.
            if (m_layoutRoot == null)
            {
                return;
            }
        }

        bool m_rangePropertyUpdating;
        FrameworkElement m_layoutRoot;
    }
}
