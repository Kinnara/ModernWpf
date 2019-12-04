using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace ModernWpf.Controls.Primitives
{
    public static class FocusVisualHelper
    {
        #region FocusVisualPrimaryBrush

        /// <summary>
        /// Gets the brush used to draw the outer border of a HighVisibility focus
        /// visual for a FrameworkElement.
        /// </summary>
        /// <param name="element">The element from which to read the property value.</param>
        /// <returns>The brush used to draw the outer border of a HighVisibility focus visual.</returns>
        public static Brush GetFocusVisualPrimaryBrush(FrameworkElement element)
        {
            return (Brush)element.GetValue(FocusVisualPrimaryBrushProperty);
        }

        /// <summary>
        /// Sets the brush used to draw the outer border of a HighVisibility focus
        /// visual for a FrameworkElement.
        /// </summary>
        /// <param name="element">The element on which to set the attached property.</param>
        /// <param name="value">The property value to set.</param>
        public static void SetFocusVisualPrimaryBrush(FrameworkElement element, Brush value)
        {
            element.SetValue(FocusVisualPrimaryBrushProperty, value);
        }

        /// <summary>
        /// Identifies the FocusVisualPrimaryBrush dependency property.
        /// </summary>
        public static readonly DependencyProperty FocusVisualPrimaryBrushProperty =
            DependencyProperty.RegisterAttached(
                "FocusVisualPrimaryBrush",
                typeof(Brush),
                typeof(FocusVisualHelper));

        #endregion

        #region FocusVisualSecondaryBrush

        /// <summary>
        /// Gets the brush used to draw the inner border of a HighVisibility focus
        /// visual for a FrameworkElement.
        /// </summary>
        /// <param name="element">The element from which to read the property value.</param>
        /// <returns>The brush used to draw the inner border of a HighVisibility focus visual.</returns>
        public static Brush GetFocusVisualSecondaryBrush(FrameworkElement element)
        {
            return (Brush)element.GetValue(FocusVisualSecondaryBrushProperty);
        }

        /// <summary>
        /// Sets the brush used to draw the inner border of a HighVisibility focus
        /// visual for a FrameworkElement.
        /// </summary>
        /// <param name="element">The element on which to set the attached property.</param>
        /// <param name="value">The property value to set.</param>
        public static void SetFocusVisualSecondaryBrush(FrameworkElement element, Brush value)
        {
            element.SetValue(FocusVisualSecondaryBrushProperty, value);
        }

        /// <summary>
        /// Identifies the FocusVisualSecondaryBrush dependency property.
        /// </summary>
        public static readonly DependencyProperty FocusVisualSecondaryBrushProperty =
            DependencyProperty.RegisterAttached(
                "FocusVisualSecondaryBrush",
                typeof(Brush),
                typeof(FocusVisualHelper));

        #endregion

        #region FocusVisualPrimaryThickness

        /// <summary>
        /// Gets the thickness of the outer border of a HighVisibility focus visual
        /// for a FrameworkElement.
        /// </summary>
        /// <param name="element">The element from which to read the property value.</param>
        /// <returns>
        /// The thickness of the outer border of a HighVisibility focus visual. The default
        /// value is 2.
        /// </returns>
        public static Thickness GetFocusVisualPrimaryThickness(FrameworkElement element)
        {
            return (Thickness)element.GetValue(FocusVisualPrimaryThicknessProperty);
        }

        /// <summary>
        /// Sets the thickness of the outer border of a HighVisibility focus visual
        /// for a FrameworkElement.
        /// </summary>
        /// <param name="element">The element on which to set the attached property.</param>
        /// <param name="value">The property value to set.</param>
        public static void SetFocusVisualPrimaryThickness(FrameworkElement element, Thickness value)
        {
            element.SetValue(FocusVisualPrimaryThicknessProperty, value);
        }

        /// <summary>
        /// Identifies the FocusVisualPrimaryThickness dependency property.
        /// </summary>
        public static readonly DependencyProperty FocusVisualPrimaryThicknessProperty =
            DependencyProperty.RegisterAttached(
                "FocusVisualPrimaryThickness",
                typeof(Thickness),
                typeof(FocusVisualHelper),
                new FrameworkPropertyMetadata(new Thickness(2)));

        #endregion

        #region FocusVisualSecondaryThickness

        /// <summary>
        /// Gets the thickness of the inner border of a HighVisibility focus visual
        /// for a FrameworkElement.
        /// </summary>
        /// <param name="element">The element from which to read the property value.</param>
        /// <returns>
        /// The thickness of the inner border of a HighVisibility focus visual. The default
        /// value is 1.
        /// </returns>
        public static Thickness GetFocusVisualSecondaryThickness(FrameworkElement element)
        {
            return (Thickness)element.GetValue(FocusVisualSecondaryThicknessProperty);
        }

        /// <summary>
        /// Sets the thickness of the inner border of a HighVisibility focus visual
        /// for a FrameworkElement.
        /// </summary>
        /// <param name="element">The element on which to set the attached property.</param>
        /// <param name="value">The property value to set.</param>
        public static void SetFocusVisualSecondaryThickness(FrameworkElement element, Thickness value)
        {
            element.SetValue(FocusVisualSecondaryThicknessProperty, value);
        }

        /// <summary>
        /// Identifies the FocusVisualSecondaryThickness dependency property.
        /// </summary>
        public static readonly DependencyProperty FocusVisualSecondaryThicknessProperty =
            DependencyProperty.RegisterAttached(
                "FocusVisualSecondaryThickness",
                typeof(Thickness),
                typeof(FocusVisualHelper),
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
                typeof(FocusVisualHelper),
                new FrameworkPropertyMetadata(new Thickness()));

        #endregion

        #region IsSystemFocusVisual

        public static bool GetIsSystemFocusVisual(Control control)
        {
            return (bool)control.GetValue(IsSystemFocusVisualProperty);
        }

        public static void SetIsSystemFocusVisual(Control control, bool value)
        {
            control.SetValue(IsSystemFocusVisualProperty, value);
        }

        public static readonly DependencyProperty IsSystemFocusVisualProperty =
            DependencyProperty.RegisterAttached(
                "IsSystemFocusVisual",
                typeof(bool),
                typeof(FocusVisualHelper),
                new PropertyMetadata(OnIsSystemFocusVisualChanged));

        private static void OnIsSystemFocusVisualChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (Control)d;
            if ((bool)e.NewValue)
            {
                control.IsVisibleChanged += OnFocusVisualIsVisibleChanged;
            }
            else
            {
                control.IsVisibleChanged -= OnFocusVisualIsVisibleChanged;
                control.ClearValue(FrameworkElement.MarginProperty);
            }
        }

        #endregion

        #region IsSystemFocusVisualVisible

        public static bool GetIsSystemFocusVisualVisible(FrameworkElement element)
        {
            return (bool)element.GetValue(IsSystemFocusVisualVisibleProperty);
        }

        private static void SetIsSystemFocusVisualVisible(FrameworkElement element, bool value)
        {
            element.SetValue(IsSystemFocusVisualVisiblePropertyKey, value);
        }

        private static readonly DependencyPropertyKey IsSystemFocusVisualVisiblePropertyKey =
            DependencyProperty.RegisterAttachedReadOnly(
                "IsSystemFocusVisualVisible",
                typeof(bool),
                typeof(FocusVisualHelper),
                null);

        public static readonly DependencyProperty IsSystemFocusVisualVisibleProperty =
            IsSystemFocusVisualVisiblePropertyKey.DependencyProperty;

        #endregion

        #region FocusedElement

        private static FrameworkElement GetFocusedElement(Control control)
        {
            return (FrameworkElement)control.GetValue(FocusedElementProperty);
        }

        private static void SetFocusedElement(Control control, FrameworkElement value)
        {
            control.SetValue(FocusedElementProperty, value);
        }

        private static readonly DependencyProperty FocusedElementProperty =
            DependencyProperty.RegisterAttached(
                "FocusedElement",
                typeof(FrameworkElement),
                typeof(FocusVisualHelper));

        #endregion

        private static void OnFocusVisualIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var focusVisual = (Control)sender;
            if ((bool)e.NewValue)
            {
                if ((VisualTreeHelper.GetParent(focusVisual) as Adorner)?.AdornedElement is FrameworkElement focusedElement)
                {
                    SetIsSystemFocusVisualVisible(focusedElement, true);

                    TransferValue(focusedElement, focusVisual, FocusVisualPrimaryBrushProperty);
                    TransferValue(focusedElement, focusVisual, FocusVisualPrimaryThicknessProperty);
                    TransferValue(focusedElement, focusVisual, FocusVisualSecondaryBrushProperty);
                    TransferValue(focusedElement, focusVisual, FocusVisualSecondaryThicknessProperty);                    
                    focusVisual.Margin = GetFocusVisualMargin(focusedElement);

                    SetFocusedElement(focusVisual, focusedElement);
                }
            }
            else
            {
                FrameworkElement focusedElement = GetFocusedElement(focusVisual);
                if (focusedElement != null)
                {
                    focusedElement.ClearValue(IsSystemFocusVisualVisiblePropertyKey);
                    focusVisual.ClearValue(FocusVisualPrimaryBrushProperty);
                    focusVisual.ClearValue(FocusVisualPrimaryThicknessProperty);
                    focusVisual.ClearValue(FocusVisualSecondaryBrushProperty);
                    focusVisual.ClearValue(FocusVisualSecondaryThicknessProperty);
                    focusVisual.ClearValue(FrameworkElement.MarginProperty);
                    focusVisual.ClearValue(FocusedElementProperty);
                }
            }
        }

        private static void TransferValue(DependencyObject source, DependencyObject target, DependencyProperty dp)
        {
            if (!Helper.HasDefaultValue(source, dp))
            {
                target.SetValue(dp, source.GetValue(dp));
            }
        }
    }
}
