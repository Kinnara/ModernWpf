using System.Windows;
using System.Windows.Controls;
using ModernWpf.Controls.Primitives;

namespace ModernWpf.Controls
{
    public class InfoBadge : Control
    {
        private static readonly Thickness s_iconMargin = new(4, 4, 4, 4);
        private static readonly Thickness s_fontIconMargin = new(4, 0, 4, 2);
        private static readonly Thickness s_valueMargin = new(4, 0, 4, 2);

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

            _valueTextBlock = GetTemplateChild("ValueTextBlock") as TextBlock;
            _iconPresenter = GetTemplateChild("IconPresenter") as FrameworkElement;

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
            if (_valueTextBlock == null || _iconPresenter == null)
            {
                return;
            }

            if (Value >= 0)
            {
                _valueTextBlock.Visibility = Visibility.Visible;
                _valueTextBlock.Margin = s_valueMargin;
                _iconPresenter.Visibility = Visibility.Collapsed;
            }
            else if (IconSource is { } iconSource)
            {
                TemplateSettings.IconElement = iconSource.CreateIconElement();
                _valueTextBlock.Visibility = Visibility.Collapsed;
                _iconPresenter.Visibility = Visibility.Visible;
                _iconPresenter.Margin = iconSource is FontIconSource ? s_fontIconMargin : s_iconMargin;
            }
            else
            {
                TemplateSettings.IconElement = null;
                _valueTextBlock.Visibility = Visibility.Collapsed;
                _iconPresenter.Visibility = Visibility.Collapsed;
            }
        }

        private void UpdateCornerRadius()
        {
            var cornerRadius = ReadLocalValue(CornerRadiusProperty) == DependencyProperty.UnsetValue
                ? new CornerRadius(ActualHeight / 2)
                : CornerRadius;

            TemplateSettings.InfoBadgeCornerRadius = cornerRadius;
        }

        private TextBlock _valueTextBlock;
        private FrameworkElement _iconPresenter;
    }
}
