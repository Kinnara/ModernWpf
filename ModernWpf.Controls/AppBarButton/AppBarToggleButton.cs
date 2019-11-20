using System.Windows;
using System.Windows.Controls.Primitives;
using ModernWpf.Controls.Primitives;

namespace ModernWpf.Controls
{
    public class AppBarToggleButton : ToggleButton, ICommandBarElement
    {
        static AppBarToggleButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(AppBarToggleButton), new FrameworkPropertyMetadata(typeof(AppBarToggleButton)));
            IsCheckedProperty.OverrideMetadata(typeof(AppBarToggleButton), new FrameworkPropertyMetadata(OnIsCheckedChanged));
        }

        public AppBarToggleButton()
        {
            UpdateIsCheckedOrIndeterminate();
        }

        #region Icon

        public static readonly DependencyProperty IconProperty =
            DependencyProperty.Register(
                nameof(Icon),
                typeof(IconElement),
                typeof(AppBarToggleButton),
                null);

        public IconElement Icon
        {
            get => (IconElement)GetValue(IconProperty);
            set => SetValue(IconProperty, value);
        }

        #endregion

        #region Label

        public static readonly DependencyProperty LabelProperty =
            DependencyProperty.Register(
                nameof(Label),
                typeof(string),
                typeof(AppBarToggleButton),
                new PropertyMetadata(string.Empty));

        public string Label
        {
            get => (string)GetValue(LabelProperty);
            set => SetValue(LabelProperty, value);
        }

        #endregion

        #region IsCompact

        public static readonly DependencyProperty IsCompactProperty =
            DependencyProperty.Register(
                nameof(IsCompact),
                typeof(bool),
                typeof(AppBarToggleButton),
                new PropertyMetadata(false));

        public bool IsCompact
        {
            get => (bool)GetValue(IsCompactProperty);
            set => SetValue(IsCompactProperty, value);
        }

        #endregion

        #region CornerRadius

        public static readonly DependencyProperty CornerRadiusProperty =
            ControlHelper.CornerRadiusProperty.AddOwner(typeof(AppBarToggleButton));

        public CornerRadius CornerRadius
        {
            get => (CornerRadius)GetValue(CornerRadiusProperty);
            set => SetValue(CornerRadiusProperty, value);
        }

        #endregion

        #region IsCheckedOrIndeterminate

        private static readonly DependencyPropertyKey IsCheckedOrIndeterminatePropertyKey =
            DependencyProperty.RegisterReadOnly(
                nameof(IsCheckedOrIndeterminate),
                typeof(bool),
                typeof(AppBarToggleButton),
                new PropertyMetadata(false));

        public static readonly DependencyProperty IsCheckedOrIndeterminateProperty =
            IsCheckedOrIndeterminatePropertyKey.DependencyProperty;

        public bool IsCheckedOrIndeterminate
        {
            get => (bool)GetValue(IsCheckedOrIndeterminateProperty);
            private set => SetValue(IsCheckedOrIndeterminatePropertyKey, value);
        }

        private void UpdateIsCheckedOrIndeterminate()
        {
            IsCheckedOrIndeterminate = IsChecked != false;
        }

        #endregion

        private static void OnIsCheckedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((AppBarToggleButton)d).UpdateIsCheckedOrIndeterminate();
        }
    }
}
