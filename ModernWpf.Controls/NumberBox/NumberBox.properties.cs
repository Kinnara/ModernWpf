using System.Windows;
using System.Windows.Media;
using ModernWpf.Controls.Primitives;

namespace ModernWpf.Controls
{
    partial class NumberBox
    {
        #region Minimum

        public static readonly DependencyProperty MinimumProperty =
            DependencyProperty.Register(
                nameof(Minimum),
                typeof(double),
                typeof(NumberBox),
                new PropertyMetadata(double.MinValue, OnMinimumPropertyChanged));

        public double Minimum
        {
            get => (double)GetValue(MinimumProperty);
            set => SetValue(MinimumProperty, value);
        }

        private static void OnMinimumPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            ((NumberBox)sender).OnMinimumPropertyChanged(args);
        }

        #endregion

        #region Maximum

        public static readonly DependencyProperty MaximumProperty =
            DependencyProperty.Register(
                nameof(Maximum),
                typeof(double),
                typeof(NumberBox),
                new PropertyMetadata(double.MaxValue, OnMaximumPropertyChanged));

        public double Maximum
        {
            get => (double)GetValue(MaximumProperty);
            set => SetValue(MaximumProperty, value);
        }

        private static void OnMaximumPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            ((NumberBox)sender).OnMaximumPropertyChanged(args);
        }

        #endregion

        #region Value

        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register(
                nameof(Value),
                typeof(double),
                typeof(NumberBox),
                new FrameworkPropertyMetadata(
                    double.NaN,
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault | FrameworkPropertyMetadataOptions.Journal,
                    OnValuePropertyChanged));

        public double Value
        {
            get => (double)GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
        }

        private static void OnValuePropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            ((NumberBox)sender).OnValuePropertyChanged(args);
        }

        #endregion

        #region SmallChange

        public static readonly DependencyProperty SmallChangeProperty =
            DependencyProperty.Register(
                nameof(SmallChange),
                typeof(double),
                typeof(NumberBox),
                new PropertyMetadata(1d, OnSmallChangePropertyChanged));

        public double SmallChange
        {
            get => (double)GetValue(SmallChangeProperty);
            set => SetValue(SmallChangeProperty, value);
        }

        private static void OnSmallChangePropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            ((NumberBox)sender).OnSmallChangePropertyChanged(args);
        }

        #endregion

        #region LargeChange

        public static readonly DependencyProperty LargeChangeProperty =
            DependencyProperty.Register(
                nameof(LargeChange),
                typeof(double),
                typeof(NumberBox),
                new PropertyMetadata(10d));

        public double LargeChange
        {
            get => (double)GetValue(LargeChangeProperty);
            set => SetValue(LargeChangeProperty, value);
        }

        #endregion

        #region Text

        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register(
                nameof(Text),
                typeof(string),
                typeof(NumberBox),
                new FrameworkPropertyMetadata(
                    string.Empty,
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault | FrameworkPropertyMetadataOptions.Journal,
                    OnTextPropertyChanged));

        public string Text
        {
            get => (string)GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        private static void OnTextPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            ((NumberBox)sender).OnTextPropertyChanged(args);
        }

        #endregion

        #region Header

        public static readonly DependencyProperty HeaderProperty =
            ControlHelper.HeaderProperty.AddOwner(
                typeof(NumberBox),
                new FrameworkPropertyMetadata(OnHeaderPropertyChanged));

        public object Header
        {
            get => GetValue(HeaderProperty);
            set => SetValue(HeaderProperty, value);
        }

        private static void OnHeaderPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            ((NumberBox)sender).OnHeaderPropertyChanged(args);
        }

        #endregion

        #region HeaderTemplate

        public static readonly DependencyProperty HeaderTemplateProperty =
            ControlHelper.HeaderTemplateProperty.AddOwner(
                typeof(NumberBox),
                new FrameworkPropertyMetadata(OnHeaderTemplatePropertyChanged));

        public DataTemplate HeaderTemplate
        {
            get => (DataTemplate)GetValue(HeaderTemplateProperty);
            set => SetValue(HeaderTemplateProperty, value);
        }

        private static void OnHeaderTemplatePropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            ((NumberBox)sender).OnHeaderTemplatePropertyChanged(args);
        }

        #endregion

        #region PlaceholderText

        public static readonly DependencyProperty PlaceholderTextProperty =
            ControlHelper.PlaceholderTextProperty.AddOwner(typeof(NumberBox));

        public string PlaceholderText
        {
            get => (string)GetValue(PlaceholderTextProperty);
            set => SetValue(PlaceholderTextProperty, value);
        }

        #endregion

        #region SelectionBrush

        public static readonly DependencyProperty SelectionBrushProperty =
            DependencyProperty.Register(
                nameof(SelectionBrush),
                typeof(Brush),
                typeof(NumberBox));

        public Brush SelectionBrush
        {
            get => (Brush)GetValue(SelectionBrushProperty);
            set => SetValue(SelectionBrushProperty, value);
        }

        #endregion

        #region TextAlignment

        public static readonly DependencyProperty TextAlignmentProperty =
            DependencyProperty.Register(
                nameof(TextAlignment),
                typeof(TextAlignment),
                typeof(NumberBox),
                new PropertyMetadata(TextAlignment.Left));

        public TextAlignment TextAlignment
        {
            get => (TextAlignment)GetValue(TextAlignmentProperty);
            set => SetValue(TextAlignmentProperty, value);
        }

        #endregion

        #region Description

        public static readonly DependencyProperty DescriptionProperty =
            ControlHelper.DescriptionProperty.AddOwner(typeof(NumberBox));

        public object Description
        {
            get => GetValue(DescriptionProperty);
            set => SetValue(DescriptionProperty, value);
        }

        #endregion

        #region ValidationMode

        public static readonly DependencyProperty ValidationModeProperty =
            DependencyProperty.Register(
                nameof(ValidationMode),
                typeof(NumberBoxValidationMode),
                typeof(NumberBox),
                new PropertyMetadata(OnValidationModePropertyChanged));

        public NumberBoxValidationMode ValidationMode
        {
            get => (NumberBoxValidationMode)GetValue(ValidationModeProperty);
            set => SetValue(ValidationModeProperty, value);
        }

        private static void OnValidationModePropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            ((NumberBox)sender).OnValidationModePropertyChanged(args);
        }

        #endregion

        #region SpinButtonPlacementMode

        public static readonly DependencyProperty SpinButtonPlacementModeProperty =
            DependencyProperty.Register(
                nameof(SpinButtonPlacementMode),
                typeof(NumberBoxSpinButtonPlacementMode),
                typeof(NumberBox),
                new PropertyMetadata(NumberBoxSpinButtonPlacementMode.Hidden, OnSpinButtonPlacementModePropertyChanged));

        public NumberBoxSpinButtonPlacementMode SpinButtonPlacementMode
        {
            get => (NumberBoxSpinButtonPlacementMode)GetValue(SpinButtonPlacementModeProperty);
            set => SetValue(SpinButtonPlacementModeProperty, value);
        }

        private static void OnSpinButtonPlacementModePropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            ((NumberBox)sender).OnSpinButtonPlacementModePropertyChanged(args);
        }

        #endregion

        #region IsWrapEnabled

        public static readonly DependencyProperty IsWrapEnabledProperty =
            DependencyProperty.Register(
                nameof(IsWrapEnabled),
                typeof(bool),
                typeof(NumberBox),
                new PropertyMetadata(false, OnIsWrapEnabledPropertyChanged));

        public bool IsWrapEnabled
        {
            get => (bool)GetValue(IsWrapEnabledProperty);
            set => SetValue(IsWrapEnabledProperty, value);
        }

        private static void OnIsWrapEnabledPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            ((NumberBox)sender).OnIsWrapEnabledPropertyChanged(args);
        }

        #endregion

        #region AcceptsExpression

        public static readonly DependencyProperty AcceptsExpressionProperty =
            DependencyProperty.Register(
                nameof(AcceptsExpression),
                typeof(bool),
                typeof(NumberBox),
                new PropertyMetadata(false));

        public bool AcceptsExpression
        {
            get => (bool)GetValue(AcceptsExpressionProperty);
            set => SetValue(AcceptsExpressionProperty, value);
        }

        #endregion

        #region NumberFormatter

        public static readonly DependencyProperty NumberFormatterProperty =
            DependencyProperty.Register(
                nameof(NumberFormatter),
                typeof(INumberBoxNumberFormatter),
                typeof(NumberBox),
                new PropertyMetadata(OnNumberFormatterPropertyChanged));

        public INumberBoxNumberFormatter NumberFormatter
        {
            get => (INumberBoxNumberFormatter)GetValue(NumberFormatterProperty);
            set
            {
                INumberBoxNumberFormatter coercedValue = value;
                ValidateNumberFormatter(coercedValue);
                SetValue(NumberFormatterProperty, coercedValue);
            }
        }

        private static void OnNumberFormatterPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            var owner = (NumberBox)sender;

            var value = (INumberBoxNumberFormatter)args.NewValue;
            var coercedValue = value;
            owner.ValidateNumberFormatter(coercedValue);
            if (value != coercedValue)
            {
                sender.SetCurrentValue(args.Property, coercedValue);
                return;
            }

            owner.OnNumberFormatterPropertyChanged(args);
        }

        #endregion

        #region CornerRadius

        public static readonly DependencyProperty CornerRadiusProperty =
            ControlHelper.CornerRadiusProperty.AddOwner(typeof(NumberBox));

        public CornerRadius CornerRadius
        {
            get => (CornerRadius)GetValue(CornerRadiusProperty);
            set => SetValue(CornerRadiusProperty, value);
        }

        #endregion

        public event TypedEventHandler<NumberBox, NumberBoxValueChangedEventArgs> ValueChanged;
    }
}
