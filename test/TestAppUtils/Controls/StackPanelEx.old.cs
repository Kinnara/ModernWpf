using ModernWpf.Controls;

namespace System.Windows.Controls
{
    public class StackPanelEx : PanelEx<SimpleStackPanel>
    {
        #region Orientation

        public static readonly DependencyProperty OrientationProperty =
                SimpleStackPanel.OrientationProperty.AddOwner(typeof(StackPanelEx),
                    new FrameworkPropertyMetadata(
                        SimpleStackPanel.OrientationProperty.DefaultMetadata.DefaultValue,
                        FrameworkPropertyMetadataOptions.None,
                        OnOrientationPropertyChanged));

        public Orientation Orientation
        {
            get => (Orientation)GetValue(OrientationProperty);
            set => SetValue(OrientationProperty, value);
        }

        private static void OnOrientationPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            ((StackPanelEx)sender).OnOrientationPropertyChanged(args);
        }

        private void OnOrientationPropertyChanged(DependencyPropertyChangedEventArgs args)
        {
            ItemsHost.Orientation = (Orientation)args.NewValue;
        }

        #endregion

        #region Spacing

        public static readonly DependencyProperty SpacingProperty =
                SimpleStackPanel.SpacingProperty.AddOwner(typeof(StackPanelEx),
                    new FrameworkPropertyMetadata(
                        SimpleStackPanel.SpacingProperty.DefaultMetadata.DefaultValue,
                        FrameworkPropertyMetadataOptions.None,
                        OnSpacingPropertyChanged));

        public double Spacing
        {
            get => (double)GetValue(SpacingProperty);
            set => SetValue(SpacingProperty, value);
        }

        private static void OnSpacingPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            ((StackPanelEx)sender).OnSpacingPropertyChanged(args);
        }

        private void OnSpacingPropertyChanged(DependencyPropertyChangedEventArgs args)
        {
            ItemsHost.Spacing = (double)args.NewValue;
        }

        #endregion
    }
}
