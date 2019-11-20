using System.Windows;
using System.Windows.Controls;

namespace ModernWpf.Controls
{
    public class AppBarSeparator : Control, ICommandBarElement
    {
        static AppBarSeparator()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(AppBarSeparator), new FrameworkPropertyMetadata(typeof(AppBarSeparator)));
        }

        public AppBarSeparator()
        {
        }

        #region IsCompact

        public static readonly DependencyProperty IsCompactProperty =
            DependencyProperty.Register(
                nameof(IsCompact),
                typeof(bool),
                typeof(AppBarSeparator),
                new PropertyMetadata(false));

        public bool IsCompact
        {
            get => (bool)GetValue(IsCompactProperty);
            set => SetValue(IsCompactProperty, value);
        }

        #endregion
    }
}
