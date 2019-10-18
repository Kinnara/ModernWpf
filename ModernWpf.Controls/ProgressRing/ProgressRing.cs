using ModernWpf.Controls.Primitives;
using System;
using System.Windows;
using System.Windows.Controls;

namespace ModernWpf.Controls
{
    [TemplateVisualState(GroupName = SizeStatesGroup, Name = LargeState)]
    [TemplateVisualState(GroupName = SizeStatesGroup, Name = SmallState)]
    [TemplateVisualState(GroupName = ActiveStatesGroup, Name = InactiveState)]
    [TemplateVisualState(GroupName = ActiveStatesGroup, Name = ActiveState)]
    public class ProgressRing : Control
    {
        private const string SizeStatesGroup = "SizeStates";
        private const string LargeState = "Large";
        private const string SmallState = "Small";

        private const string ActiveStatesGroup = "ActiveStates";
        private const string InactiveState = "Inactive";
        private const string ActiveState = "Active";

        static ProgressRing()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ProgressRing),
                new FrameworkPropertyMetadata(typeof(ProgressRing)));
        }

        public ProgressRing()
        {
            TemplateSettings = new ProgressRingTemplateSettings();
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
                new FrameworkPropertyMetadata(OnIsActiveChanged));

        private static void OnIsActiveChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((ProgressRing)d).UpdateVisualStates(true);
        }

        #endregion

        public ProgressRingTemplateSettings TemplateSettings { get; }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            UpdateVisualStates(false);
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);

            double maxSideLength = Math.Min(sizeInfo.NewSize.Width, sizeInfo.NewSize.Height);
            double ellipseDiameter = 0.1 * maxSideLength;
            if (maxSideLength <= 40)
            {
                ellipseDiameter += 1;
            }

            var templateSettings = TemplateSettings;
            templateSettings.EllipseDiameter = ellipseDiameter;
            templateSettings.EllipseOffset = new Thickness(0, maxSideLength / 2 - ellipseDiameter, 0, 0);
            templateSettings.MaxSideLength = maxSideLength;

            UpdateVisualStates(true);
        }

        private void UpdateVisualStates(bool useTransitions)
        {
            VisualStateManager.GoToState(this, IsActive ? ActiveState : InactiveState, useTransitions);
            VisualStateManager.GoToState(this, TemplateSettings.MaxSideLength < 60 ? SmallState : LargeState, useTransitions);
        }
    }
}
