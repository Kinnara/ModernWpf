using System.Windows;
using System.Windows.Controls.Primitives;

namespace ModernWpf.Controls.Primitives
{
    public static class DataGridColumnHeaderHelper
    {
        #region IsEnabled

        public static bool GetIsEnabled(DataGridColumnHeader header)
        {
            return (bool)header.GetValue(IsEnabledProperty);
        }

        public static void SetIsEnabled(DataGridColumnHeader header, bool value)
        {
            header.SetValue(IsEnabledProperty, value);
        }

        public static readonly DependencyProperty IsEnabledProperty =
            DependencyProperty.RegisterAttached(
                "IsEnabled",
                typeof(bool),
                typeof(DataGridColumnHeaderHelper));

        #endregion

        #region ComputedSeparatorVisibility

        public static Visibility GetComputedSeparatorVisibility(DataGridColumnHeader header)
        {
            return (Visibility)header.GetValue(ComputedSeparatorVisibilityProperty);
        }

        public static void SetComputedSeparatorVisibility(DataGridColumnHeader header, Visibility value)
        {
            header.SetValue(ComputedSeparatorVisibilityProperty, value);
        }

        public static readonly DependencyProperty ComputedSeparatorVisibilityProperty =
            DependencyProperty.RegisterAttached(
                "ComputedSeparatorVisibility",
                typeof(Visibility),
                typeof(DataGridColumnHeaderHelper),
                new PropertyMetadata(Visibility.Visible));

        #endregion

        #region IsLastVisibleColumnHeader

        public static bool GetIsLastVisibleColumnHeader(DataGridColumnHeader columnHeader)
        {
            return (bool)columnHeader.GetValue(IsLastVisibleColumnHeaderProperty);
        }

        public static void SetIsLastVisibleColumnHeader(DataGridColumnHeader columnHeader, bool value)
        {
            columnHeader.SetValue(IsLastVisibleColumnHeaderProperty, value);
        }

        public static readonly DependencyProperty IsLastVisibleColumnHeaderProperty =
            DependencyProperty.RegisterAttached(
                "IsLastVisibleColumnHeader",
                typeof(bool),
                typeof(DataGridColumnHeaderHelper));

        #endregion
    }
}
