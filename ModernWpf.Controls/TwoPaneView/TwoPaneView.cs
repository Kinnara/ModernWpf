// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Windows;
using System.Windows.Controls;

namespace ModernWpf.Controls
{
    public enum TwoPaneViewPriority
    {
        Pane1 = 0,
        Pane2 = 1
    }

    public enum TwoPaneViewMode
    {
        SinglePane = 0,
        Wide = 1,
        Tall = 2
    }

    public enum TwoPaneViewWideModeConfiguration
    {
        SinglePane = 0,
        LeftRight = 1,
        RightLeft = 2
    }

    public enum TwoPaneViewTallModeConfiguration
    {
        SinglePane = 0,
        TopBottom = 1,
        BottomTop = 2
    }

    [TemplatePart(Name = Pane1ScrollViewerName, Type = typeof(ScrollViewer))]
    [TemplatePart(Name = Pane2ScrollViewerName, Type = typeof(ScrollViewer))]
    [TemplatePart(Name = ColumnLeftName, Type = typeof(ColumnDefinition))]
    [TemplatePart(Name = ColumnMiddleName, Type = typeof(ColumnDefinition))]
    [TemplatePart(Name = ColumnRightName, Type = typeof(ColumnDefinition))]
    [TemplatePart(Name = RowTopName, Type = typeof(RowDefinition))]
    [TemplatePart(Name = RowMiddleName, Type = typeof(RowDefinition))]
    [TemplatePart(Name = RowBottomName, Type = typeof(RowDefinition))]
    public class TwoPaneView : Control
    {
        private const string Pane1ScrollViewerName = "PART_Pane1ScrollViewer";
        private const string Pane2ScrollViewerName = "PART_Pane2ScrollViewer";
        private const string ColumnLeftName = "PART_ColumnLeft";
        private const string ColumnMiddleName = "PART_ColumnMiddle";
        private const string ColumnRightName = "PART_ColumnRight";
        private const string RowTopName = "PART_RowTop";
        private const string RowMiddleName = "PART_RowMiddle";
        private const string RowBottomName = "PART_RowBottom";

        private enum ViewMode
        {
            Pane1Only,
            Pane2Only,
            LeftRight,
            RightLeft,
            TopBottom,
            BottomTop,
            None
        }

        private static readonly DependencyPropertyKey ModePropertyKey =
            DependencyProperty.RegisterReadOnly(
                nameof(Mode),
                typeof(TwoPaneViewMode),
                typeof(TwoPaneView),
                new PropertyMetadata(TwoPaneViewMode.SinglePane));

        public static readonly DependencyProperty Pane1Property =
            DependencyProperty.Register(
                nameof(Pane1),
                typeof(UIElement),
                typeof(TwoPaneView),
                new PropertyMetadata(null));

        public static readonly DependencyProperty Pane2Property =
            DependencyProperty.Register(
                nameof(Pane2),
                typeof(UIElement),
                typeof(TwoPaneView),
                new PropertyMetadata(null));

        public static readonly DependencyProperty Pane1LengthProperty =
            DependencyProperty.Register(
                nameof(Pane1Length),
                typeof(GridLength),
                typeof(TwoPaneView),
                new PropertyMetadata(GridLength.Auto, OnLayoutPropertyChanged));

        public static readonly DependencyProperty Pane2LengthProperty =
            DependencyProperty.Register(
                nameof(Pane2Length),
                typeof(GridLength),
                typeof(TwoPaneView),
                new PropertyMetadata(new GridLength(1, GridUnitType.Star), OnLayoutPropertyChanged));

        public static readonly DependencyProperty PanePriorityProperty =
            DependencyProperty.Register(
                nameof(PanePriority),
                typeof(TwoPaneViewPriority),
                typeof(TwoPaneView),
                new PropertyMetadata(TwoPaneViewPriority.Pane1, OnLayoutPropertyChanged));

        public static readonly DependencyProperty ModeProperty = ModePropertyKey.DependencyProperty;

        public static readonly DependencyProperty WideModeConfigurationProperty =
            DependencyProperty.Register(
                nameof(WideModeConfiguration),
                typeof(TwoPaneViewWideModeConfiguration),
                typeof(TwoPaneView),
                new PropertyMetadata(TwoPaneViewWideModeConfiguration.LeftRight, OnLayoutPropertyChanged));

        public static readonly DependencyProperty TallModeConfigurationProperty =
            DependencyProperty.Register(
                nameof(TallModeConfiguration),
                typeof(TwoPaneViewTallModeConfiguration),
                typeof(TwoPaneView),
                new PropertyMetadata(TwoPaneViewTallModeConfiguration.TopBottom, OnLayoutPropertyChanged));

        public static readonly DependencyProperty MinWideModeWidthProperty =
            DependencyProperty.Register(
                nameof(MinWideModeWidth),
                typeof(double),
                typeof(TwoPaneView),
                new PropertyMetadata(641d, OnLayoutPropertyChanged, CoerceMinimum));

        public static readonly DependencyProperty MinTallModeHeightProperty =
            DependencyProperty.Register(
                nameof(MinTallModeHeight),
                typeof(double),
                typeof(TwoPaneView),
                new PropertyMetadata(641d, OnLayoutPropertyChanged, CoerceMinimum));

        private ColumnDefinition _columnLeft;
        private ColumnDefinition _columnMiddle;
        private ColumnDefinition _columnRight;
        private RowDefinition _rowTop;
        private RowDefinition _rowMiddle;
        private RowDefinition _rowBottom;
        private ViewMode _currentMode = ViewMode.None;
        private bool _templateApplied;

        static TwoPaneView()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(TwoPaneView), new FrameworkPropertyMetadata(typeof(TwoPaneView)));
        }

        public TwoPaneView()
        {
            SizeChanged += OnSizeChanged;
        }

        public UIElement Pane1
        {
            get => (UIElement)GetValue(Pane1Property);
            set => SetValue(Pane1Property, value);
        }

        public UIElement Pane2
        {
            get => (UIElement)GetValue(Pane2Property);
            set => SetValue(Pane2Property, value);
        }

        public GridLength Pane1Length
        {
            get => (GridLength)GetValue(Pane1LengthProperty);
            set => SetValue(Pane1LengthProperty, value);
        }

        public GridLength Pane2Length
        {
            get => (GridLength)GetValue(Pane2LengthProperty);
            set => SetValue(Pane2LengthProperty, value);
        }

        public TwoPaneViewPriority PanePriority
        {
            get => (TwoPaneViewPriority)GetValue(PanePriorityProperty);
            set => SetValue(PanePriorityProperty, value);
        }

        public TwoPaneViewMode Mode => (TwoPaneViewMode)GetValue(ModeProperty);

        public TwoPaneViewWideModeConfiguration WideModeConfiguration
        {
            get => (TwoPaneViewWideModeConfiguration)GetValue(WideModeConfigurationProperty);
            set => SetValue(WideModeConfigurationProperty, value);
        }

        public TwoPaneViewTallModeConfiguration TallModeConfiguration
        {
            get => (TwoPaneViewTallModeConfiguration)GetValue(TallModeConfigurationProperty);
            set => SetValue(TallModeConfigurationProperty, value);
        }

        public double MinWideModeWidth
        {
            get => (double)GetValue(MinWideModeWidthProperty);
            set => SetValue(MinWideModeWidthProperty, value);
        }

        public double MinTallModeHeight
        {
            get => (double)GetValue(MinTallModeHeightProperty);
            set => SetValue(MinTallModeHeightProperty, value);
        }

        public event TypedEventHandler<TwoPaneView, object> ModeChanged;

        public override void OnApplyTemplate()
        {
            _templateApplied = false;
            _columnLeft = null;
            _columnMiddle = null;
            _columnRight = null;
            _rowTop = null;
            _rowMiddle = null;
            _rowBottom = null;

            base.OnApplyTemplate();

            _columnLeft = GetTemplateChild(ColumnLeftName) as ColumnDefinition;
            _columnMiddle = GetTemplateChild(ColumnMiddleName) as ColumnDefinition;
            _columnRight = GetTemplateChild(ColumnRightName) as ColumnDefinition;
            _rowTop = GetTemplateChild(RowTopName) as RowDefinition;
            _rowMiddle = GetTemplateChild(RowMiddleName) as RowDefinition;
            _rowBottom = GetTemplateChild(RowBottomName) as RowDefinition;
            _templateApplied = true;

            UpdateMode();
        }

        private static object CoerceMinimum(DependencyObject dependencyObject, object baseValue)
        {
            var value = (double)baseValue;
            return double.IsNaN(value) || value < 0 ? 0d : value;
        }

        private static void OnLayoutPropertyChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs args)
        {
            ((TwoPaneView)dependencyObject).UpdateMode();
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs args)
        {
            UpdateMode();
        }

        private void UpdateMode()
        {
            if (!_templateApplied)
            {
                return;
            }

            var newMode = PanePriority == TwoPaneViewPriority.Pane1 ? ViewMode.Pane1Only : ViewMode.Pane2Only;

            if (ActualWidth > MinWideModeWidth && WideModeConfiguration != TwoPaneViewWideModeConfiguration.SinglePane)
            {
                newMode = WideModeConfiguration == TwoPaneViewWideModeConfiguration.LeftRight
                    ? ViewMode.LeftRight
                    : ViewMode.RightLeft;
            }
            else if (ActualHeight > MinTallModeHeight && TallModeConfiguration != TwoPaneViewTallModeConfiguration.SinglePane)
            {
                newMode = TallModeConfiguration == TwoPaneViewTallModeConfiguration.TopBottom
                    ? ViewMode.TopBottom
                    : ViewMode.BottomTop;
            }

            UpdateRowsAndColumns(newMode);

            if (newMode == _currentMode)
            {
                return;
            }

            _currentMode = newMode;
            var publicMode = TwoPaneViewMode.SinglePane;
            string stateName;

            switch (newMode)
            {
                case ViewMode.Pane1Only:
                    stateName = "ViewMode_OneOnly";
                    break;
                case ViewMode.Pane2Only:
                    stateName = "ViewMode_TwoOnly";
                    break;
                case ViewMode.LeftRight:
                    stateName = "ViewMode_LeftRight";
                    publicMode = TwoPaneViewMode.Wide;
                    break;
                case ViewMode.RightLeft:
                    stateName = "ViewMode_RightLeft";
                    publicMode = TwoPaneViewMode.Wide;
                    break;
                case ViewMode.TopBottom:
                    stateName = "ViewMode_TopBottom";
                    publicMode = TwoPaneViewMode.Tall;
                    break;
                case ViewMode.BottomTop:
                    stateName = "ViewMode_BottomTop";
                    publicMode = TwoPaneViewMode.Tall;
                    break;
                default:
                    return;
            }

            VisualStateManager.GoToState(this, stateName, true);

            if (publicMode != Mode)
            {
                SetValue(ModePropertyKey, publicMode);
                ModeChanged?.Invoke(this, this);
            }
        }

        private void UpdateRowsAndColumns(ViewMode newMode)
        {
            if (_columnLeft == null || _columnMiddle == null || _columnRight == null ||
                _rowTop == null || _rowMiddle == null || _rowBottom == null)
            {
                return;
            }

            _columnMiddle.Width = new GridLength(0);
            _rowMiddle.Height = new GridLength(0);

            if (newMode == ViewMode.LeftRight || newMode == ViewMode.RightLeft)
            {
                _columnLeft.Width = newMode == ViewMode.LeftRight ? Pane1Length : Pane2Length;
                _columnRight.Width = newMode == ViewMode.LeftRight ? Pane2Length : Pane1Length;
            }
            else
            {
                _columnLeft.Width = new GridLength(1, GridUnitType.Star);
                _columnRight.Width = new GridLength(0);
            }

            if (newMode == ViewMode.TopBottom || newMode == ViewMode.BottomTop)
            {
                _rowTop.Height = newMode == ViewMode.TopBottom ? Pane1Length : Pane2Length;
                _rowBottom.Height = newMode == ViewMode.TopBottom ? Pane2Length : Pane1Length;
            }
            else
            {
                _rowTop.Height = new GridLength(1, GridUnitType.Star);
                _rowBottom.Height = new GridLength(0);
            }
        }
    }
}
