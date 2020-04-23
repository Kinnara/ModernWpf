using System.Windows;

namespace MUXControlsTestApp
{
    public static class NameHelper
    {
        #region Name

        public static readonly DependencyProperty NameProperty =
            DependencyProperty.RegisterAttached(
                "Name",
                typeof(string),
                typeof(NameHelper),
                new PropertyMetadata(string.Empty, OnNameChanged));

        public static string GetName(FrameworkElement element)
        {
            return (string)element.GetValue(NameProperty);
        }

        public static void SetName(FrameworkElement element, string value)
        {
            element.SetValue(NameProperty, value);
        }

        private static void OnNameChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var element = (FrameworkElement)d;
            element.Name = (string)e.NewValue;
        }

        #endregion
    }
}
