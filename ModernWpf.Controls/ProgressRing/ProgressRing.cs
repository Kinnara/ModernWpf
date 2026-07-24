using System;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using ModernWpf.Automation.Peers;
using ModernWpf.Controls.Primitives;

namespace ModernWpf.Controls
{
    public partial class ProgressRing : Control
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
