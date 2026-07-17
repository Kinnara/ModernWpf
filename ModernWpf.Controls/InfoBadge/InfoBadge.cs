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

        protected override Size ArrangeOverride(Size arrangeBounds)
        {
            // WPF raises SizeChanged after the arrange pass. Seed the source-equivalent
            // size-derived radius before arranging the template so its first frame is rounded.
            UpdateCornerRadius(arrangeBounds.Height);
            return base.ArrangeOverride(arrangeBounds);
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
            UpdateCornerRadius(e.NewSize.Height);
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
            UpdateCornerRadius(ActualHeight);
        }

        private void UpdateCornerRadius(double actualHeight)
        {
            var cornerRadius = IsDefaultCornerRadiusValue()
                ? new CornerRadius(actualHeight / 2)
                : CornerRadius;

            TemplateSettings.InfoBadgeCornerRadius = cornerRadius;
        }

        private bool IsDefaultCornerRadiusValue()
        {
            var valueSource = DependencyPropertyHelper.GetValueSource(this, CornerRadiusProperty);
            return valueSource.BaseValueSource == BaseValueSource.Default &&
                !valueSource.IsAnimated &&
                !valueSource.IsCoerced &&
                !valueSource.IsExpression;
        }
    }
}
