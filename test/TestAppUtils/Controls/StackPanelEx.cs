using ModernWpf.Controls;
using System.Collections;
using System.Windows.Markup;
using System.Windows.Media;

namespace System.Windows.Controls
{
    [ContentProperty(nameof(Children))]
    public class StackPanelEx : FrameworkElement
    {
        public StackPanelEx()
        {
            _itemsHost = new SimpleStackPanel();
            _border = new Border { Child = _itemsHost };
            AddLogicalChild(_border);
            AddVisualChild(_border);
        }

        public UIElementCollection Children => _itemsHost.Children;

        #region Background

        public static readonly DependencyProperty BackgroundProperty =
                Panel.BackgroundProperty.AddOwner(typeof(StackPanelEx),
                    new FrameworkPropertyMetadata(
                        Panel.BackgroundProperty.DefaultMetadata.DefaultValue,
                        FrameworkPropertyMetadataOptions.None,
                        OnBackgroundPropertyChanged));

        public Brush Background
        {
            get => (Brush)GetValue(BackgroundProperty);
            set => SetValue(BackgroundProperty, value);
        }

        private static void OnBackgroundPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            ((StackPanelEx)sender).OnBackgroundPropertyChanged(args);
        }

        private void OnBackgroundPropertyChanged(DependencyPropertyChangedEventArgs args)
        {
            _border.Background = (Brush)args.NewValue;
        }

        #endregion

        #region BorderBrush

        public static readonly DependencyProperty BorderBrushProperty
                = Border.BorderBrushProperty.AddOwner(typeof(StackPanelEx),
                    new FrameworkPropertyMetadata(
                        Border.BorderBrushProperty.DefaultMetadata.DefaultValue,
                        FrameworkPropertyMetadataOptions.None,
                        OnBorderBrushPropertyChanged));

        public Brush BorderBrush
        {
            get => (Brush)GetValue(BorderBrushProperty);
            set => SetValue(BorderBrushProperty, value);
        }

        private static void OnBorderBrushPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            ((StackPanelEx)sender).OnBorderBrushPropertyChanged(args);
        }

        private void OnBorderBrushPropertyChanged(DependencyPropertyChangedEventArgs args)
        {
            _border.BorderBrush = (Brush)args.NewValue;
        }

        #endregion

        #region BorderThickness

        public static readonly DependencyProperty BorderThicknessProperty
                = Border.BorderThicknessProperty.AddOwner(typeof(StackPanelEx),
                    new FrameworkPropertyMetadata(
                        Border.BorderThicknessProperty.DefaultMetadata.DefaultValue,
                        FrameworkPropertyMetadataOptions.None,
                        OnBorderThicknessPropertyChanged));

        public Thickness BorderThickness
        {
            get => (Thickness)GetValue(BorderThicknessProperty);
            set => SetValue(BorderThicknessProperty, value);
        }

        private static void OnBorderThicknessPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            ((StackPanelEx)sender).OnBorderThicknessPropertyChanged(args);
        }

        private void OnBorderThicknessPropertyChanged(DependencyPropertyChangedEventArgs args)
        {
            _border.BorderThickness = (Thickness)args.NewValue;
        }

        #endregion

        #region Padding

        public static readonly DependencyProperty PaddingProperty
                = Border.PaddingProperty.AddOwner(typeof(StackPanelEx),
                    new FrameworkPropertyMetadata(
                        Border.PaddingProperty.DefaultMetadata.DefaultValue,
                        FrameworkPropertyMetadataOptions.None,
                        OnPaddingPropertyChanged));

        public Thickness Padding
        {
            get => (Thickness)GetValue(PaddingProperty);
            set => SetValue(PaddingProperty, value);
        }

        private static void OnPaddingPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            ((StackPanelEx)sender).OnPaddingPropertyChanged(args);
        }

        private void OnPaddingPropertyChanged(DependencyPropertyChangedEventArgs args)
        {
            _border.Padding = (Thickness)args.NewValue;
        }

        #endregion

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
            _itemsHost.Orientation = (Orientation)args.NewValue;
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
            _itemsHost.Spacing = (double)args.NewValue;
        }

        #endregion

        protected override IEnumerator LogicalChildren
        {
            get { yield return _border; }
        }

        protected override int VisualChildrenCount => 1;

        protected override Visual GetVisualChild(int index)
        {
            if (index != 0)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            return _border;
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            _border.Measure(availableSize);
            return _border.DesiredSize;
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            _border.Arrange(new Rect(finalSize));
            return finalSize;
        }

        Border _border;
        SimpleStackPanel _itemsHost;
    }
}
