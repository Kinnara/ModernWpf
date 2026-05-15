using System;
using System.Windows;
using System.Windows.Controls;

namespace ModernWpf.Controls
{
    public class TwoPaneView : Control
    {
        private const double DefaultMinWideModeWidth = 641.0;
        private const double DefaultMinTallModeHeight = 641.0;

        static TwoPaneView()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(TwoPaneView), new FrameworkPropertyMetadata(typeof(TwoPaneView)));
        }

        public TwoPaneView()
        {
            SizeChanged += OnSizeChanged;
        }

        public static readonly DependencyProperty Pane1Property =
            DependencyProperty.Register(
                nameof(Pane1),
                typeof(UIElement),
                typeof(TwoPaneView));

        public UIElement Pane1
        {
            get => (UIElement)GetValue(Pane1Property);
            set => SetValue(Pane1Property, value);
        }

        public static readonly DependencyProperty Pane2Property =
            DependencyProperty.Register(
                nameof(Pane2),
                typeof(UIElement),
                typeof(TwoPaneView));

        public UIElement Pane2
        {
            get => (UIElement)GetValue(Pane2Property);
            set => SetValue(Pane2Property, value);
        }

        public static readonly DependencyProperty Pane1LengthProperty =
            DependencyProperty.Register(
                nameof(Pane1Length),
                typeof(GridLength),
                typeof(TwoPaneView),
                new PropertyMetadata(GridLength.Auto, OnLayoutPropertyChanged));

        public GridLength Pane1Length
        {
            get => (GridLength)GetValue(Pane1LengthProperty);
            set => SetValue(Pane1LengthProperty, value);
        }

        public static readonly DependencyProperty Pane2LengthProperty =
            DependencyProperty.Register(
                nameof(Pane2Length),
                typeof(GridLength),
                typeof(TwoPaneView),
                new PropertyMetadata(new GridLength(1, GridUnitType.Star), OnLayoutPropertyChanged));

        public GridLength Pane2Length
        {
            get => (GridLength)GetValue(Pane2LengthProperty);
            set => SetValue(Pane2LengthProperty, value);
        }

        public static readonly DependencyProperty PanePriorityProperty =
            DependencyProperty.Register(
                nameof(PanePriority),
                typeof(TwoPaneViewPriority),
                typeof(TwoPaneView),
                new PropertyMetadata(TwoPaneViewPriority.Pane1, OnLayoutPropertyChanged));

        public TwoPaneViewPriority PanePriority
        {
            get => (TwoPaneViewPriority)GetValue(PanePriorityProperty);
            set => SetValue(PanePriorityProperty, value);
        }

        private static readonly DependencyPropertyKey ModePropertyKey =
            DependencyProperty.RegisterReadOnly(
                nameof(Mode),
                typeof(TwoPaneViewMode),
                typeof(TwoPaneView),
                new PropertyMetadata(TwoPaneViewMode.SinglePane));

        public static readonly DependencyProperty ModeProperty =
            ModePropertyKey.DependencyProperty;

        public TwoPaneViewMode Mode => (TwoPaneViewMode)GetValue(ModeProperty);

        public static readonly DependencyProperty WideModeConfigurationProperty =
            DependencyProperty.Register(
                nameof(WideModeConfiguration),
                typeof(TwoPaneViewWideModeConfiguration),
                typeof(TwoPaneView),
                new PropertyMetadata(TwoPaneViewWideModeConfiguration.LeftRight, OnLayoutPropertyChanged));

        public TwoPaneViewWideModeConfiguration WideModeConfiguration
        {
            get => (TwoPaneViewWideModeConfiguration)GetValue(WideModeConfigurationProperty);
            set => SetValue(WideModeConfigurationProperty, value);
        }

        public static readonly DependencyProperty TallModeConfigurationProperty =
            DependencyProperty.Register(
                nameof(TallModeConfiguration),
                typeof(TwoPaneViewTallModeConfiguration),
                typeof(TwoPaneView),
                new PropertyMetadata(TwoPaneViewTallModeConfiguration.TopBottom, OnLayoutPropertyChanged));

        public TwoPaneViewTallModeConfiguration TallModeConfiguration
        {
            get => (TwoPaneViewTallModeConfiguration)GetValue(TallModeConfigurationProperty);
            set => SetValue(TallModeConfigurationProperty, value);
        }

        public static readonly DependencyProperty MinWideModeWidthProperty =
            DependencyProperty.Register(
                nameof(MinWideModeWidth),
                typeof(double),
                typeof(TwoPaneView),
                new PropertyMetadata(DefaultMinWideModeWidth, OnLayoutPropertyChanged, CoerceMinModeLength));

        public double MinWideModeWidth
        {
            get => (double)GetValue(MinWideModeWidthProperty);
            set => SetValue(MinWideModeWidthProperty, value);
        }

        public static readonly DependencyProperty MinTallModeHeightProperty =
            DependencyProperty.Register(
                nameof(MinTallModeHeight),
                typeof(double),
                typeof(TwoPaneView),
                new PropertyMetadata(DefaultMinTallModeHeight, OnLayoutPropertyChanged, CoerceMinModeLength));

        public double MinTallModeHeight
        {
            get => (double)GetValue(MinTallModeHeightProperty);
            set => SetValue(MinTallModeHeightProperty, value);
        }

        public event EventHandler ModeChanged;

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _pane1ScrollViewer = GetTemplateChild("PART_Pane1ScrollViewer") as FrameworkElement;
            _pane2ScrollViewer = GetTemplateChild("PART_Pane2ScrollViewer") as FrameworkElement;
            _columnLeft = GetTemplateChild("PART_ColumnLeft") as ColumnDefinition;
            _columnMiddle = GetTemplateChild("PART_ColumnMiddle") as ColumnDefinition;
            _columnRight = GetTemplateChild("PART_ColumnRight") as ColumnDefinition;
            _rowTop = GetTemplateChild("PART_RowTop") as RowDefinition;
            _rowMiddle = GetTemplateChild("PART_RowMiddle") as RowDefinition;
            _rowBottom = GetTemplateChild("PART_RowBottom") as RowDefinition;

            UpdateMode();
        }

        private static void OnLayoutPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((TwoPaneView)d).UpdateMode();
        }

        private static object CoerceMinModeLength(DependencyObject d, object baseValue)
        {
            return Math.Max(0.0, (double)baseValue);
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateMode();
        }

        private void UpdateMode()
        {
            if (_pane1ScrollViewer == null || _pane2ScrollViewer == null ||
                _columnLeft == null || _columnMiddle == null || _columnRight == null ||
                _rowTop == null || _rowMiddle == null || _rowBottom == null)
            {
                return;
            }

            var newMode = CalculateMode();
            ApplyLayout(newMode);

            if (newMode != Mode)
            {
                SetValue(ModePropertyKey, newMode);
                ModeChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        private TwoPaneViewMode CalculateMode()
        {
            if (ActualWidth > MinWideModeWidth && WideModeConfiguration != TwoPaneViewWideModeConfiguration.SinglePane)
            {
                return TwoPaneViewMode.Wide;
            }

            if (ActualHeight > MinTallModeHeight && TallModeConfiguration != TwoPaneViewTallModeConfiguration.SinglePane)
            {
                return TwoPaneViewMode.Tall;
            }

            return TwoPaneViewMode.SinglePane;
        }

        private void ApplyLayout(TwoPaneViewMode mode)
        {
            ResetGrid();

            switch (mode)
            {
                case TwoPaneViewMode.Wide:
                    ApplyWideLayout();
                    break;

                case TwoPaneViewMode.Tall:
                    ApplyTallLayout();
                    break;

                default:
                    ApplySinglePaneLayout();
                    break;
            }
        }

        private void ResetGrid()
        {
            _columnLeft.Width = new GridLength(1, GridUnitType.Star);
            _columnMiddle.Width = new GridLength(0);
            _columnRight.Width = new GridLength(0);
            _rowTop.Height = new GridLength(1, GridUnitType.Star);
            _rowMiddle.Height = new GridLength(0);
            _rowBottom.Height = new GridLength(0);

            Grid.SetColumn(_pane1ScrollViewer, 0);
            Grid.SetColumn(_pane2ScrollViewer, 2);
            Grid.SetRow(_pane1ScrollViewer, 0);
            Grid.SetRow(_pane2ScrollViewer, 0);
            _pane1ScrollViewer.Visibility = Visibility.Visible;
            _pane2ScrollViewer.Visibility = Visibility.Visible;
        }

        private void ApplyWideLayout()
        {
            _rowTop.Height = new GridLength(1, GridUnitType.Star);
            _rowBottom.Height = new GridLength(0);

            if (WideModeConfiguration == TwoPaneViewWideModeConfiguration.RightLeft)
            {
                Grid.SetColumn(_pane1ScrollViewer, 2);
                Grid.SetColumn(_pane2ScrollViewer, 0);
                _columnLeft.Width = Pane2Length;
                _columnRight.Width = Pane1Length;
            }
            else
            {
                Grid.SetColumn(_pane1ScrollViewer, 0);
                Grid.SetColumn(_pane2ScrollViewer, 2);
                _columnLeft.Width = Pane1Length;
                _columnRight.Width = Pane2Length;
            }
        }

        private void ApplyTallLayout()
        {
            _columnLeft.Width = new GridLength(1, GridUnitType.Star);
            _columnRight.Width = new GridLength(0);

            if (TallModeConfiguration == TwoPaneViewTallModeConfiguration.BottomTop)
            {
                Grid.SetColumn(_pane1ScrollViewer, 0);
                Grid.SetColumn(_pane2ScrollViewer, 0);
                Grid.SetRow(_pane1ScrollViewer, 2);
                Grid.SetRow(_pane2ScrollViewer, 0);
                _rowTop.Height = Pane2Length;
                _rowBottom.Height = Pane1Length;
            }
            else
            {
                Grid.SetColumn(_pane1ScrollViewer, 0);
                Grid.SetColumn(_pane2ScrollViewer, 0);
                Grid.SetRow(_pane1ScrollViewer, 0);
                Grid.SetRow(_pane2ScrollViewer, 2);
                _rowTop.Height = Pane1Length;
                _rowBottom.Height = Pane2Length;
            }
        }

        private void ApplySinglePaneLayout()
        {
            _columnLeft.Width = new GridLength(1, GridUnitType.Star);
            _columnRight.Width = new GridLength(0);
            _rowTop.Height = new GridLength(1, GridUnitType.Star);
            _rowBottom.Height = new GridLength(0);

            Grid.SetColumn(_pane1ScrollViewer, 0);
            Grid.SetColumn(_pane2ScrollViewer, 0);
            Grid.SetRow(_pane1ScrollViewer, 0);
            Grid.SetRow(_pane2ScrollViewer, 0);

            if (PanePriority == TwoPaneViewPriority.Pane2)
            {
                _pane1ScrollViewer.Visibility = Visibility.Collapsed;
                _pane2ScrollViewer.Visibility = Visibility.Visible;
            }
            else
            {
                _pane1ScrollViewer.Visibility = Visibility.Visible;
                _pane2ScrollViewer.Visibility = Visibility.Collapsed;
            }
        }

        private FrameworkElement _pane1ScrollViewer;
        private FrameworkElement _pane2ScrollViewer;
        private ColumnDefinition _columnLeft;
        private ColumnDefinition _columnMiddle;
        private ColumnDefinition _columnRight;
        private RowDefinition _rowTop;
        private RowDefinition _rowMiddle;
        private RowDefinition _rowBottom;
    }
}
