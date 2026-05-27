using System.Windows;
using System.Windows.Controls;

namespace ModernWpf.Gallery.Controls
{
    public class PageHeader : Control
    {
        static PageHeader()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(PageHeader), new FrameworkPropertyMetadata(typeof(PageHeader)));
        }

        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register(
                nameof(Title),
                typeof(string),
                typeof(PageHeader),
                new PropertyMetadata(null));

        public static readonly DependencyProperty DescriptionProperty =
            DependencyProperty.Register(
                nameof(Description),
                typeof(string),
                typeof(PageHeader),
                new PropertyMetadata(null));

        public static readonly DependencyProperty ShowDescriptionProperty =
            DependencyProperty.Register(
                nameof(ShowDescription),
                typeof(bool),
                typeof(PageHeader),
                new PropertyMetadata(true));

        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }

        public string Description
        {
            get { return (string)GetValue(DescriptionProperty); }
            set { SetValue(DescriptionProperty, value); }
        }

        public bool ShowDescription
        {
            get { return (bool)GetValue(ShowDescriptionProperty); }
            set { SetValue(ShowDescriptionProperty, value); }
        }
    }
}
