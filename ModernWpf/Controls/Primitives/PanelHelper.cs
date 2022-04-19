using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace ModernWpf.Controls.Primitives
{
    public static class PanelHelper
    {
        #region Spacing

        /// <summary>
        /// Identifies the Spacing dependency property.
        /// </summary>
        public static readonly DependencyProperty SpacingProperty =
            DependencyProperty.RegisterAttached(
                "Spacing",
                typeof(double),
                typeof(PanelHelper),
                new PropertyMetadata(0d, OnSpacingChanged));

        /// <summary>
        /// Gets a uniform distance (in pixels) between stacked items. It is applied in the direction of the StackPanel's Orientation.
        /// </summary>
        /// <param name="panel">The element from which to read the property value.</param>
        /// <returns>The uniform distance (in pixels) between stacked items.</returns>
        public static double GetSpacing(Panel panel)
        {
            return (double)panel.GetValue(SpacingProperty);
        }

        /// <summary>
        /// Sets a uniform distance (in pixels) between stacked items. It is applied in the direction of the StackPanel's Orientation.
        /// </summary>
        /// <param name="panel">The element from which to read the property value.</param>
        /// <param name="value">The uniform distance (in pixels) between stacked items.</param>
        public static void SetSpacing(Panel panel, double value)
        {
            panel.SetValue(SpacingProperty, value);
        }

        private static void OnSpacingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            // Make sure this is put on a panel
            var panel = d as Panel;
            if (panel == null) return;

            // Avoid duplicate registrations
            panel.Loaded -= OnPanelLoaded;
            panel.Loaded += OnPanelLoaded;

            if (panel.IsLoaded)
            {
                OnPanelLoaded(panel, null);
            }
        }

        private static void OnPanelLoaded(object sender, RoutedEventArgs e)
        {
            var panel = (Panel)sender;
            object value = panel.GetProperty("Orientation");
            double spacing = GetSpacing(panel);
            Thickness margin = value != null && value is Orientation orientation ? orientation == Orientation.Horizontal ? new Thickness(0, 0, spacing, 0) : new Thickness(0, 0, 0, spacing) : new Thickness(spacing / 2, spacing / 2, spacing / 2, spacing / 2);
            Thickness lastmargin = value == null ? margin : new Thickness(0);

            // Go over the children and set margin for them:
            for (var i = 0; i < panel.Children.Count; i++)
            {
                UIElement child = panel.Children[i];
                var fe = child as FrameworkElement;
                if (fe == null) continue;

                bool isLastItem = i == panel.Children.Count - 1;
                fe.Margin = isLastItem ? lastmargin : margin;
            }
        }

        #endregion
    }
}
