using System.Windows;
using System.Windows.Controls;
using ModernWpf.Controls.Primitives;

namespace ModernWpf.Controls
{
    public class FlyoutPresenter : ContentControl
    {
        static FlyoutPresenter()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(FlyoutPresenter), new FrameworkPropertyMetadata(typeof(FlyoutPresenter)));
        }

        public FlyoutPresenter()
        {
        }

        #region CornerRadius

        public static readonly DependencyProperty CornerRadiusProperty =
            ControlHelper.CornerRadiusProperty.AddOwner(typeof(FlyoutPresenter));

        public CornerRadius CornerRadius
        {
            get => (CornerRadius)GetValue(CornerRadiusProperty);
            set => SetValue(CornerRadiusProperty, value);
        }

        #endregion
    }
}
