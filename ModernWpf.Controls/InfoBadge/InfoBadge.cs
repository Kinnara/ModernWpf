using System.Windows;
using System.Windows.Controls;
using ModernWpf.Controls.Primitives;

namespace ModernWpf.Controls
{
    public class InfoBadge : Control
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

        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register(
                nameof(Value),
                typeof(int),
                typeof(InfoBadge),
                new PropertyMetadata(-1, OnDisplayKindPropertyChanged),
                value => (int)value >= -1);

        public int Value
        {
            get => (int)GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
        }

        public static readonly DependencyProperty IconSourceProperty =
            DependencyProperty.Register(
                nameof(IconSource),
                typeof(IconSource),
                typeof(InfoBadge),
                new PropertyMetadata(null, OnDisplayKindPropertyChanged));

        public IconSource IconSource
        {
            get => (IconSource)GetValue(IconSourceProperty);
            set => SetValue(IconSourceProperty, value);
        }

        public static readonly DependencyProperty CornerRadiusProperty =
            ControlHelper.CornerRadiusProperty.AddOwner(
                typeof(InfoBadge),
                new FrameworkPropertyMetadata(default(CornerRadius), OnCornerRadiusChanged));

        public CornerRadius CornerRadius
        {
            get => (CornerRadius)GetValue(CornerRadiusProperty);
            set => SetValue(CornerRadiusProperty, value);
        }

        private static readonly DependencyPropertyKey TemplateSettingsPropertyKey =
            DependencyProperty.RegisterReadOnly(
                nameof(TemplateSettings),
                typeof(InfoBadgeTemplateSettings),
                typeof(InfoBadge),
                new PropertyMetadata(null));

        public static readonly DependencyProperty TemplateSettingsProperty =
            TemplateSettingsPropertyKey.DependencyProperty;

        public InfoBadgeTemplateSettings TemplateSettings =>
            (InfoBadgeTemplateSettings)GetValue(TemplateSettingsProperty);

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
