using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace ModernWpf.Controls.Primitives
{
    public sealed class ClipHelper
    {
        #region CornerRadius

        /// <summary>
        /// Gets the radius for the corners of the control's border.
        /// </summary>
        /// <param name="control">The element from which to read the property value.</param>
        /// <returns>
        /// The degree to which the corners are rounded, expressed as values of the CornerRadius
        /// structure.
        /// </returns>
        public static CornerRadius GetCornerRadius(FrameworkElement control)
        {
            return (CornerRadius)control.GetValue(CornerRadiusProperty);
        }

        /// <summary>
        /// Sets the radius for the corners of the control's border.
        /// </summary>
        /// <param name="control">The element on which to set the attached property.</param>
        /// <param name="value">The property value to set.</param>
        public static void SetCornerRadius(FrameworkElement control, CornerRadius value)
        {
            control.SetValue(CornerRadiusProperty, value);
        }

        /// <summary>
        /// Identifies the CornerRadius dependency property.
        /// </summary>
        public static readonly DependencyProperty CornerRadiusProperty =
            DependencyProperty.RegisterAttached(
                "CornerRadius",
                typeof(CornerRadius),
                typeof(ClipHelper),
                new PropertyMetadata(OnCornerRadiusChanged));

        private static void OnCornerRadiusChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            FrameworkElement element = d as FrameworkElement;
            double Radius = GetCornerRadius(element).TopRight;
            RectangleGeometry geometry = new RectangleGeometry { RadiusX = Radius, RadiusY = Radius };
            MultiBinding binding = new MultiBinding { Converter = new SizeToRectConverter() };
            binding.Bindings.Add(new Binding { Source = 0, });
            binding.Bindings.Add(new Binding { Source = 0, });
            binding.Bindings.Add(new Binding
            {
                Source = element,
                Path = new PropertyPath(nameof(element.ActualWidth))
            });
            binding.Bindings.Add(new Binding
            {
                Source = element,
                Path = new PropertyPath(nameof(element.ActualHeight))
            });
            BindingOperations.SetBinding(geometry, RectangleGeometry.RectProperty, binding);
            element.Clip = geometry;
        }

        #endregion
    }
}
