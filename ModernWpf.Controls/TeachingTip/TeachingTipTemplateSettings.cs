using System.Windows;

namespace ModernWpf.Controls
{
    public sealed class TeachingTipTemplateSettings : DependencyObject
    {
        public static readonly DependencyProperty TopRightHighlightMarginProperty =
            DependencyProperty.Register(
                nameof(TopRightHighlightMargin),
                typeof(Thickness),
                typeof(TeachingTipTemplateSettings),
                new PropertyMetadata(new Thickness()));

        public Thickness TopRightHighlightMargin
        {
            get => (Thickness)GetValue(TopRightHighlightMarginProperty);
            internal set => SetValue(TopRightHighlightMarginProperty, value);
        }

        public static readonly DependencyProperty TopLeftHighlightMarginProperty =
            DependencyProperty.Register(
                nameof(TopLeftHighlightMargin),
                typeof(Thickness),
                typeof(TeachingTipTemplateSettings),
                new PropertyMetadata(new Thickness()));

        public Thickness TopLeftHighlightMargin
        {
            get => (Thickness)GetValue(TopLeftHighlightMarginProperty);
            internal set => SetValue(TopLeftHighlightMarginProperty, value);
        }

        public static readonly DependencyProperty IconElementProperty =
            DependencyProperty.Register(
                nameof(IconElement),
                typeof(IconElement),
                typeof(TeachingTipTemplateSettings));

        public IconElement IconElement
        {
            get => (IconElement)GetValue(IconElementProperty);
            internal set => SetValue(IconElementProperty, value);
        }
    }
}
