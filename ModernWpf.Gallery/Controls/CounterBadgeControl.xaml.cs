using System.Windows;

namespace ModernWpf.Gallery.Controls
{
    public partial class CounterBadgeControl
    {
        public CounterBadgeControl()
        {
            InitializeComponent();
            UpdateCountLabel();
        }

        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register(
                nameof(Title),
                typeof(string),
                typeof(CounterBadgeControl),
                new PropertyMetadata("Reusable counter"));

        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }

        public static readonly DependencyProperty CountProperty =
            DependencyProperty.Register(
                nameof(Count),
                typeof(int),
                typeof(CounterBadgeControl),
                new PropertyMetadata(0, OnCountChanged));

        public int Count
        {
            get { return (int)GetValue(CountProperty); }
            set { SetValue(CountProperty, value); }
        }

        public static readonly DependencyProperty CountLabelProperty =
            DependencyProperty.Register(
                nameof(CountLabel),
                typeof(string),
                typeof(CounterBadgeControl),
                new PropertyMetadata(string.Empty));

        public string CountLabel
        {
            get { return (string)GetValue(CountLabelProperty); }
            private set { SetValue(CountLabelProperty, value); }
        }

        private static void OnCountChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((CounterBadgeControl)d).UpdateCountLabel();
        }

        private void OnIncrementClick(object sender, RoutedEventArgs e)
        {
            Count++;
        }

        private void UpdateCountLabel()
        {
            CountLabel = "Clicked " + Count + " times";
        }
    }
}
