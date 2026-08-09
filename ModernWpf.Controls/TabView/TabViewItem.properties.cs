using System.Windows;
using System.Windows.Controls;

namespace ModernWpf.Controls
{
    public partial class TabViewItem
    {
        private static readonly DependencyPropertyKey TabViewTemplateSettingsPropertyKey =
            DependencyProperty.RegisterReadOnly(
                nameof(TabViewTemplateSettings),
                typeof(TabViewItemTemplateSettings),
                typeof(TabViewItem),
                new PropertyMetadata(null));

        public static readonly DependencyProperty HeaderProperty =
            DependencyProperty.Register(nameof(Header), typeof(object), typeof(TabViewItem), new PropertyMetadata(null, OnHeaderPropertyChanged));

        public static readonly DependencyProperty HeaderTemplateProperty =
            DependencyProperty.Register(nameof(HeaderTemplate), typeof(DataTemplate), typeof(TabViewItem));

        public static readonly DependencyProperty IconSourceProperty =
            DependencyProperty.Register(nameof(IconSource), typeof(IconSource), typeof(TabViewItem), new PropertyMetadata(null, OnIconSourcePropertyChanged));

        public static readonly DependencyProperty IsClosableProperty =
            DependencyProperty.Register(nameof(IsClosable), typeof(bool), typeof(TabViewItem), new PropertyMetadata(true, OnIsClosablePropertyChanged));

        public static readonly DependencyProperty TabViewTemplateSettingsProperty = TabViewTemplateSettingsPropertyKey.DependencyProperty;

        public object Header
        {
            get => GetValue(HeaderProperty);
            set => SetValue(HeaderProperty, value);
        }

        public DataTemplate HeaderTemplate
        {
            get => (DataTemplate)GetValue(HeaderTemplateProperty);
            set => SetValue(HeaderTemplateProperty, value);
        }

        public IconSource IconSource
        {
            get => (IconSource)GetValue(IconSourceProperty);
            set => SetValue(IconSourceProperty, value);
        }

        public bool IsClosable
        {
            get => (bool)GetValue(IsClosableProperty);
            set => SetValue(IsClosableProperty, value);
        }

        public TabViewItemTemplateSettings TabViewTemplateSettings =>
            (TabViewItemTemplateSettings)GetValue(TabViewTemplateSettingsProperty);
    }
}
