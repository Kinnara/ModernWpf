using System.Windows;
using System.Windows.Controls;
using ModernWpf.Controls.Primitives;

namespace ModernWpf.Controls
{
    public partial class InfoBadge : Control
    {
        static InfoBadge()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(InfoBadge), new FrameworkPropertyMetadata(typeof(InfoBadge)));
        }

        public InfoBadge()
        {
            SetValue(TemplateSettingsPropertyKey, new InfoBadgeTemplateSettings());
            SizeChanged += OnSizeChanged;
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            UpdateCornerRadius();
            UpdateDisplayKind();
        }

        protected override Size MeasureOverride(Size constraint)
        {
            var desiredSize = base.MeasureOverride(constraint);
            if (desiredSize.Width < desiredSize.Height)
            {
                return new Size(desiredSize.Height, desiredSize.Height);
            }

            return desiredSize;
        }

        private static void OnDisplayKindPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((InfoBadge)d).UpdateDisplayKind();
        }

        private static void OnCornerRadiusChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((InfoBadge)d).UpdateCornerRadius();
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateCornerRadius();
        }

        private void UpdateDisplayKind()
        {
            string stateName;
            if (Value >= 0)
            {
                stateName = "Value";
            }
            else if (IconSource is { } iconSource)
            {
                TemplateSettings.IconElement = iconSource.CreateIconElement();
                stateName = iconSource is FontIconSource ? "FontIcon" : "Icon";
            }
            else
            {
                TemplateSettings.IconElement = null;
                stateName = "Dot";
            }

            VisualStateManager.GoToState(this, stateName, true);
        }

        private void UpdateCornerRadius()
        {
            var cornerRadius = ReadLocalValue(CornerRadiusProperty) == DependencyProperty.UnsetValue
                ? new CornerRadius(ActualHeight / 2)
                : CornerRadius;

            TemplateSettings.InfoBadgeCornerRadius = cornerRadius;
        }
    }
}
