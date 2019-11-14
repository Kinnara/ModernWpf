using MahApps.Metro.Controls;
using System.Windows;

namespace ModernWpf.MahApps.Controls
{
    public class HamburgerMenuHeaderItem : HamburgerMenuItemBase
    {
        #region Label

        public static readonly DependencyProperty LabelProperty =
            DependencyProperty.Register(
                nameof(Label),
                typeof(string),
                typeof(HamburgerMenuHeaderItem),
                null);

        public string Label
        {
            get => (string)GetValue(LabelProperty);
            set => SetValue(LabelProperty, value);
        }

        #endregion

        protected override Freezable CreateInstanceCore()
        {
            return new HamburgerMenuHeaderItem();
        }
    }
}
