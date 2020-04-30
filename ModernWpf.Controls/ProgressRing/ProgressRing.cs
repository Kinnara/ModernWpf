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
        const string s_ActiveStateName = "Active";
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
                new FrameworkPropertyMetadata(OnIsActivePropertyChanged));

        private static void OnIsActivePropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            ((ProgressRing)sender).OnIsActivePropertyChanged(args);
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

            ChangeVisualState();
        }

        void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            ApplyTemplateSettings();
            ChangeVisualState();
        }

        void OnIsActivePropertyChanged(DependencyPropertyChangedEventArgs args)
        {
            ChangeVisualState();
        }

        void ChangeVisualState()
        {
            VisualStateManager.GoToState(this, IsActive ? s_ActiveStateName : s_InactiveStateName, true);
            VisualStateManager.GoToState(this, TemplateSettings.MaxSideLength < 60 ? s_SmallStateName : s_LargeStateName, true);
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
                    double width = Math.Min(ActualWidth, ActualHeight);

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
    }
}
