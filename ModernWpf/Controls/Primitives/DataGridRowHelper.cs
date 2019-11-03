using System.Windows;
using System.Windows.Controls;

namespace ModernWpf.Controls.Primitives
{
    public static class DataGridRowHelper
    {
        #region AreRowDetailsFrozen

        private static readonly DependencyPropertyKey AreRowDetailsFrozenPropertyKey =
            DependencyProperty.RegisterAttachedReadOnly(
                "AreRowDetailsFrozen",
                typeof(bool),
                typeof(DataGridRowHelper),
                new PropertyMetadata(false));

        public static readonly DependencyProperty AreRowDetailsFrozenProperty =
            AreRowDetailsFrozenPropertyKey.DependencyProperty;

        public static bool GetAreRowDetailsFrozen(DataGridRow row)
        {
            return (bool)row.GetValue(AreRowDetailsFrozenProperty);
        }

        private static void SetAreRowDetailsFrozen(DataGridRow row, bool value)
        {
            row.SetValue(AreRowDetailsFrozenPropertyKey, value);
        }

        internal static readonly DependencyProperty AreRowDetailsFrozenInternalProperty =
            DependencyProperty.RegisterAttached(
                "AreRowDetailsFrozenInternal",
                typeof(bool),
                typeof(DataGridRowHelper),
                new PropertyMetadata(false, OnAreRowDetailsFrozenInternalChanged));

        private static void OnAreRowDetailsFrozenInternalChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            SetAreRowDetailsFrozen((DataGridRow)d, (bool)e.NewValue);
        }

        #endregion

        #region HeadersVisibility

        private static readonly DependencyPropertyKey HeadersVisibilityPropertyKey =
            DependencyProperty.RegisterAttachedReadOnly(
                "HeadersVisibility",
                typeof(DataGridHeadersVisibility),
                typeof(DataGridRowHelper),
                new PropertyMetadata(DataGridHeadersVisibility.All));

        public static readonly DependencyProperty HeadersVisibilityProperty =
            HeadersVisibilityPropertyKey.DependencyProperty;

        public static DataGridHeadersVisibility GetHeadersVisibility(DataGridRow row)
        {
            return (DataGridHeadersVisibility)row.GetValue(HeadersVisibilityProperty);
        }

        private static void SetHeadersVisibility(DataGridRow row, DataGridHeadersVisibility value)
        {
            row.SetValue(HeadersVisibilityPropertyKey, value);
        }

        internal static readonly DependencyProperty HeadersVisibilityInternalProperty =
            DependencyProperty.RegisterAttached(
                "HeadersVisibilityInternal",
                typeof(DataGridHeadersVisibility),
                typeof(DataGridRowHelper),
                new PropertyMetadata(DataGridHeadersVisibility.All, OnHeadersVisibilityInternalChanged));

        private static void OnHeadersVisibilityInternalChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            SetHeadersVisibility((DataGridRow)d, (DataGridHeadersVisibility)e.NewValue);
        }

        #endregion
    }
}
