using System.Windows;
using System.Windows.Controls;
using ModernWpf.Controls.Primitives;

namespace ModernWpf.Controls
{
    public class ControlEx : Control
    {
        #region CornerRadius

        public static readonly DependencyProperty CornerRadiusProperty =
            System.Windows.Controls.Border.CornerRadiusProperty.AddOwner(typeof(ControlEx));

        public CornerRadius CornerRadius
        {
            get => (CornerRadius)GetValue(CornerRadiusProperty);
            set => SetValue(CornerRadiusProperty, value);
        }

        #endregion
    }
}
