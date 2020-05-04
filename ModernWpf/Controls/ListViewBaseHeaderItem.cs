using ModernWpf.Controls.Primitives;
using System.Windows;
using System.Windows.Controls;

namespace ModernWpf.Controls
{
    /// <summary>
    /// Provides the infrastructure for the ListViewHeaderItem and GridViewHeaderItem
    /// classes.
    /// </summary>
    public class ListViewBaseHeaderItem : ContentControl
    {
        internal ListViewBaseHeaderItem()
        {
        }

        #region UseSystemFocusVisuals

        /// <summary>
        /// Identifies the UseSystemFocusVisuals dependency property.
        /// </summary>
        public static readonly DependencyProperty UseSystemFocusVisualsProperty =
            FocusVisualHelper.UseSystemFocusVisualsProperty.AddOwner(typeof(ListViewBaseHeaderItem));

        /// <summary>
        /// Gets or sets a value that indicates whether the control uses focus visuals that
        /// are drawn by the system or those defined in the control template.
        /// </summary>
        /// <returns>
        /// **true** if the control uses focus visuals drawn by the system; **false** if
        /// the control uses focus visuals defined in the ControlTemplate. The default is
        /// **false**; see Remarks.
        /// </returns>
        public bool UseSystemFocusVisuals
        {
            get => (bool)GetValue(UseSystemFocusVisualsProperty);
            set => SetValue(UseSystemFocusVisualsProperty, value);
        }

        #endregion

        #region CornerRadius

        /// <summary>
        /// Identifies the CornerRadius dependency property.
        /// </summary>
        public static readonly DependencyProperty CornerRadiusProperty =
            ControlHelper.CornerRadiusProperty.AddOwner(typeof(ListViewBaseHeaderItem));

        /// <summary>
        /// Gets or sets the radius for the corners of the control's border.
        /// </summary>
        /// <returns>
        /// The degree to which the corners are rounded, expressed as values of the CornerRadius
        /// structure.
        /// </returns>
        public CornerRadius CornerRadius
        {
            get => (CornerRadius)GetValue(CornerRadiusProperty);
            set => SetValue(CornerRadiusProperty, value);
        }

        #endregion
    }
}
