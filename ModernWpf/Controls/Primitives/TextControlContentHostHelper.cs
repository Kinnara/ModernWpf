using System.Windows;
using System.Windows.Controls;

namespace ModernWpf.Controls.Primitives
{
    public static class TextControlContentHostHelper
    {
        #region ContentPresenterMargin

        public static readonly DependencyProperty ContentPresenterMarginProperty =
            DependencyProperty.RegisterAttached(
                "ContentPresenterMargin",
                typeof(Thickness),
                typeof(TextControlContentHostHelper));

        public static Thickness GetContentPresenterMargin(ScrollViewer contentHost)
        {
            return (Thickness)contentHost.GetValue(ContentPresenterMarginProperty);
        }

        public static void SetContentPresenterMargin(ScrollViewer contentHost, Thickness value)
        {
            contentHost.SetValue(ContentPresenterMarginProperty, value);
        }

        #endregion
    }
}
