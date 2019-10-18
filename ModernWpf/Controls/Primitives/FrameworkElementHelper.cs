using System.Windows;
using System.Windows.Media;

namespace ModernWpf.Controls.Primitives
{
    public static class FrameworkElementHelper
    {
        #region FocusVisualPrimaryBrush

        public static Brush GetFocusVisualPrimaryBrush(FrameworkElement element)
        {
            return (Brush)element.GetValue(FocusVisualPrimaryBrushProperty);
        }

        public static void SetFocusVisualPrimaryBrush(FrameworkElement element, Brush value)
        {
            element.SetValue(FocusVisualPrimaryBrushProperty, value);
        }

        public static readonly DependencyProperty FocusVisualPrimaryBrushProperty =
            DependencyProperty.RegisterAttached(
                "FocusVisualPrimaryBrush",
                typeof(Brush),
                typeof(FrameworkElementHelper),
                null);

        #endregion

        #region FocusVisualSecondaryBrush

        public static Brush GetFocusVisualSecondaryBrush(FrameworkElement element)
        {
            return (Brush)element.GetValue(FocusVisualSecondaryBrushProperty);
        }

        public static void SetFocusVisualSecondaryBrush(FrameworkElement element, Brush value)
        {
            element.SetValue(FocusVisualSecondaryBrushProperty, value);
        }

        public static readonly DependencyProperty FocusVisualSecondaryBrushProperty =
            DependencyProperty.RegisterAttached(
                "FocusVisualSecondaryBrush",
                typeof(Brush),
                typeof(FrameworkElementHelper),
                null);

        #endregion

        #region FocusVisualPrimaryThickness

        public static Thickness GetFocusVisualPrimaryThickness(FrameworkElement element)
        {
            return (Thickness)element.GetValue(FocusVisualPrimaryThicknessProperty);
        }

        public static void SetFocusVisualPrimaryThickness(FrameworkElement element, Thickness value)
        {
            element.SetValue(FocusVisualPrimaryThicknessProperty, value);
        }

        public static readonly DependencyProperty FocusVisualPrimaryThicknessProperty =
            DependencyProperty.RegisterAttached(
                "FocusVisualPrimaryThickness",
                typeof(Thickness),
                typeof(FrameworkElementHelper),
                new FrameworkPropertyMetadata(new Thickness(1)));

        #endregion

        #region FocusVisualSecondaryThickness

        public static Thickness GetFocusVisualSecondaryThickness(FrameworkElement element)
        {
            return (Thickness)element.GetValue(FocusVisualSecondaryThicknessProperty);
        }

        public static void SetFocusVisualSecondaryThickness(FrameworkElement element, Thickness value)
        {
            element.SetValue(FocusVisualSecondaryThicknessProperty, value);
        }

        public static readonly DependencyProperty FocusVisualSecondaryThicknessProperty =
            DependencyProperty.RegisterAttached(
                "FocusVisualSecondaryThickness",
                typeof(Thickness),
                typeof(FrameworkElementHelper),
                new FrameworkPropertyMetadata(new Thickness(1)));

        #endregion

        #region FocusVisualMargin

        /// <summary>
        /// Gets the outer margin of the focus visual for a FrameworkElement.
        /// </summary>
        /// <param name="element">The element from which to read the property value.</param>
        /// <returns>
        /// Provides margin values for the focus visual. The default value is a default Thickness
        /// with all properties (dimensions) equal to 0.
        /// </returns>
        public static Thickness GetFocusVisualMargin(FrameworkElement element)
        {
            return (Thickness)element.GetValue(FocusVisualMarginProperty);
        }

        /// <summary>
        /// Sets the outer margin of the focus visual for a FrameworkElement.
        /// </summary>
        /// <param name="element">The element on which to set the attached property.</param>
        /// <param name="value">The property value to set.</param>
        public static void SetFocusVisualMargin(FrameworkElement element, Thickness value)
        {
            element.SetValue(FocusVisualMarginProperty, value);
        }

        /// <summary>
        /// Identifies the FocusVisualMargin dependency property.
        /// </summary>
        public static readonly DependencyProperty FocusVisualMarginProperty =
            DependencyProperty.RegisterAttached(
                "FocusVisualMargin",
                typeof(Thickness),
                typeof(FrameworkElementHelper),
                null);

        #endregion
    }
}
