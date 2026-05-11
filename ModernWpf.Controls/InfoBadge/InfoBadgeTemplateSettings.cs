using System.Windows;

namespace ModernWpf.Controls
{
    public sealed class InfoBadgeTemplateSettings : DependencyObject
    {
        internal InfoBadgeTemplateSettings()
        {
        }

        public static readonly DependencyProperty InfoBadgeCornerRadiusProperty =
            DependencyProperty.Register(
                nameof(InfoBadgeCornerRadius),
                typeof(CornerRadius),
                typeof(InfoBadgeTemplateSettings));

        public CornerRadius InfoBadgeCornerRadius
        {
            get => (CornerRadius)GetValue(InfoBadgeCornerRadiusProperty);
            internal set => SetValue(InfoBadgeCornerRadiusProperty, value);
        }

        public static readonly DependencyProperty IconElementProperty =
            DependencyProperty.Register(
                nameof(IconElement),
                typeof(IconElement),
                typeof(InfoBadgeTemplateSettings));

        public IconElement IconElement
        {
            get => (IconElement)GetValue(IconElementProperty);
            internal set => SetValue(IconElementProperty, value);
        }
    }
}
