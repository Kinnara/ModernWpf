using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ModernWpf.Controls
{
    public partial class TwoPaneView : Control
    {
        private const double DefaultMinWideModeWidth = 641.0;
        private const double DefaultMinTallModeHeight = 641.0;
        private const string Pane1ScrollViewerName = "PART_Pane1ScrollViewer";
        private const string Pane2ScrollViewerName = "PART_Pane2ScrollViewer";
        private const string Pane1HostName = "PART_Pane1Host";
        private const string Pane2HostName = "PART_Pane2Host";
        private const string ColumnLeftName = "PART_ColumnLeft";
        private const string ColumnMiddleName = "PART_ColumnMiddle";
        private const string ColumnRightName = "PART_ColumnRight";
        private const string RowTopName = "PART_RowTop";
        private const string RowMiddleName = "PART_RowMiddle";
        private const string RowBottomName = "PART_RowBottom";

        static TwoPaneView()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(TwoPaneView), new FrameworkPropertyMetadata(typeof(TwoPaneView)));
        }

        public TwoPaneView()
        {
            Loaded += OnLoaded;
            SizeChanged += OnSizeChanged;
        }

        public event TypedEventHandler<TwoPaneView, object> ModeChanged;

        public override void OnApplyTemplate()
        {
            ClearTemplatePartHandlers();

            base.OnApplyTemplate();

            _loaded = true;

            _pane1ScrollViewer = SetScrollViewerProperties(Pane1ScrollViewerName);
            _pane2ScrollViewer = SetScrollViewerProperties(Pane2ScrollViewerName);
            _pane1Host = GetTemplateChild(Pane1HostName) as Border;
            _pane2Host = GetTemplateChild(Pane2HostName) as Border;
            _columnLeft = GetTemplateChild(ColumnLeftName) as ColumnDefinition;
            _columnMiddle = GetTemplateChild(ColumnMiddleName) as ColumnDefinition;
            _columnRight = GetTemplateChild(ColumnRightName) as ColumnDefinition;
            _rowTop = GetTemplateChild(RowTopName) as RowDefinition;
            _rowMiddle = GetTemplateChild(RowMiddleName) as RowDefinition;
            _rowBottom = GetTemplateChild(RowBottomName) as RowDefinition;

            UpdatePaneHosts();
            UpdateMode();
        }

        private ScrollViewer SetScrollViewerProperties(string scrollViewerName)
        {
            if (GetTemplateChild(scrollViewerName) is ScrollViewer scrollViewer)
            {
                scrollViewer.Loaded += OnScrollViewerLoaded;
                return scrollViewer;
            }

            return null;
        }

        private void OnScrollViewerLoaded(object sender, RoutedEventArgs args)
        {
            if (sender is ScrollViewer scrollViewer &&
                FindInVisualTreeByName(scrollViewer, "PART_ScrollContentPresenter") is ScrollContentPresenter scrollContentPresenter)
            {
                scrollContentPresenter.HorizontalAlignment = HorizontalAlignment.Stretch;
                scrollContentPresenter.VerticalAlignment = VerticalAlignment.Stretch;
            }
        }

        private static void OnLayoutPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((TwoPaneView)d).UpdateMode();
        }

        private static void OnPanePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((TwoPaneView)d).UpdatePaneHosts();
        }

        private static object CoerceMinModeLength(DependencyObject d, object baseValue)
        {
            return Math.Max(0.0, (double)baseValue);
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateMode();
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            UpdateMode();
        }

        private void UpdateMode()
        {
            if (!_loaded)
            {
                return;
            }

            var controlWidth = ActualWidth;
            var controlHeight = ActualHeight;
            var newMode = PanePriority == TwoPaneViewPriority.Pane1 ? ViewMode.Pane1Only : ViewMode.Pane2Only;
            var info = DisplayRegionHelper.GetRegionInfo();
            var rcControl = GetControlRect();
            var isInMultipleRegions = IsInMultipleRegions(info, rcControl);

            if (isInMultipleRegions)
            {
                if (info.Mode == TwoPaneViewMode.Wide)
                {
                    if (WideModeConfiguration != TwoPaneViewWideModeConfiguration.SinglePane)
                    {
                        newMode = WideModeConfiguration == TwoPaneViewWideModeConfiguration.LeftRight
                            ? ViewMode.LeftRight
                            : ViewMode.RightLeft;
                    }
                }
                else if (info.Mode == TwoPaneViewMode.Tall)
                {
                    if (TallModeConfiguration != TwoPaneViewTallModeConfiguration.SinglePane)
                    {
                        newMode = TallModeConfiguration == TwoPaneViewTallModeConfiguration.TopBottom
                            ? ViewMode.TopBottom
                            : ViewMode.BottomTop;
                    }
                }
            }
            else
            {
                if (controlWidth > MinWideModeWidth && WideModeConfiguration != TwoPaneViewWideModeConfiguration.SinglePane)
                {
                    newMode = WideModeConfiguration == TwoPaneViewWideModeConfiguration.LeftRight
                        ? ViewMode.LeftRight
                        : ViewMode.RightLeft;
                }
                else if (controlHeight > MinTallModeHeight && TallModeConfiguration != TwoPaneViewTallModeConfiguration.SinglePane)
                {
                    newMode = TallModeConfiguration == TwoPaneViewTallModeConfiguration.TopBottom
                        ? ViewMode.TopBottom
                        : ViewMode.BottomTop;
                }
            }

            UpdateRowsColumns(newMode, info, rcControl);

            if (newMode != _currentMode)
            {
                _currentMode = newMode;

                var newViewMode = TwoPaneViewMode.SinglePane;

                switch (_currentMode)
                {
                    case ViewMode.Pane1Only:
                        GoToLayoutState("ViewMode_OneOnly");
                        break;

                    case ViewMode.Pane2Only:
                        GoToLayoutState("ViewMode_TwoOnly");
                        break;

                    case ViewMode.LeftRight:
                        GoToLayoutState("ViewMode_LeftRight");
                        newViewMode = TwoPaneViewMode.Wide;
                        break;

                    case ViewMode.RightLeft:
                        GoToLayoutState("ViewMode_RightLeft");
                        newViewMode = TwoPaneViewMode.Wide;
                        break;

                    case ViewMode.TopBottom:
                        GoToLayoutState("ViewMode_TopBottom");
                        newViewMode = TwoPaneViewMode.Tall;
                        break;

                    case ViewMode.BottomTop:
                        GoToLayoutState("ViewMode_BottomTop");
                        newViewMode = TwoPaneViewMode.Tall;
                        break;
                }

                if (newViewMode != Mode)
                {
                    SetValue(ModePropertyKey, newViewMode);
                    ModeChanged?.Invoke(this, null);
                }
            }
        }

        private void UpdateRowsColumns(ViewMode newMode, DisplayRegionHelperInfo info, Rect rcControl)
        {
            if (_columnLeft != null &&
                _columnMiddle != null &&
                _columnRight != null &&
                _rowTop != null &&
                _rowMiddle != null &&
                _rowBottom != null)
            {
                _columnMiddle.Width = new GridLength(0, GridUnitType.Pixel);
                _rowMiddle.Height = new GridLength(0, GridUnitType.Pixel);

                if (newMode == ViewMode.LeftRight || newMode == ViewMode.RightLeft)
                {
                    _columnLeft.Width = newMode == ViewMode.LeftRight ? Pane1Length : Pane2Length;
                    _columnRight.Width = newMode == ViewMode.LeftRight ? Pane2Length : Pane1Length;
                }
                else
                {
                    _columnLeft.Width = new GridLength(1, GridUnitType.Star);
                    _columnRight.Width = new GridLength(0, GridUnitType.Pixel);
                }

                if (newMode == ViewMode.TopBottom || newMode == ViewMode.BottomTop)
                {
                    _rowTop.Height = newMode == ViewMode.TopBottom ? Pane1Length : Pane2Length;
                    _rowBottom.Height = newMode == ViewMode.TopBottom ? Pane2Length : Pane1Length;
                }
                else
                {
                    _rowTop.Height = new GridLength(1, GridUnitType.Star);
                    _rowBottom.Height = new GridLength(0, GridUnitType.Pixel);
                }

                if (IsInMultipleRegions(info, rcControl) &&
                    newMode != ViewMode.Pane1Only &&
                    newMode != ViewMode.Pane2Only)
                {
                    var rc1 = info.Regions[0];
                    var rc2 = info.Regions[1];
                    var rcWindow = DisplayRegionHelper.WindowRect(this);

                    if (info.Mode == TwoPaneViewMode.Wide)
                    {
                        _columnMiddle.Width = new GridLength(Math.Max(0, rc2.X - rc1.Width), GridUnitType.Pixel);
                        _columnLeft.Width = new GridLength(Math.Max(0, rc1.Width - rcControl.X), GridUnitType.Pixel);
                        _columnRight.Width = new GridLength(Math.Max(0, rc2.Width - ((rcWindow.Width - rcControl.Width) - rcControl.X)), GridUnitType.Pixel);
                    }
                    else
                    {
                        _rowMiddle.Height = new GridLength(Math.Max(0, rc2.Y - rc1.Height), GridUnitType.Pixel);
                        _rowTop.Height = new GridLength(Math.Max(0, rc1.Height - rcControl.Y), GridUnitType.Pixel);
                        _rowBottom.Height = new GridLength(Math.Max(0, rc2.Height - ((rcWindow.Height - rcControl.Height) - rcControl.Y)), GridUnitType.Pixel);
                    }
                }
            }
        }

        private Rect GetControlRect()
        {
            var bounds = new Rect(0, 0, ActualWidth, ActualHeight);
            var windowElement = DisplayRegionHelper.WindowElement(this);
            if (windowElement != null && !ReferenceEquals(windowElement, this))
            {
                try
                {
                    return TransformToVisual(windowElement).TransformBounds(bounds);
                }
                catch (InvalidOperationException)
                {
                }
            }

            return bounds;
        }

        private bool IsInMultipleRegions(DisplayRegionHelperInfo info, Rect rcControl)
        {
            if (info.Mode == TwoPaneViewMode.SinglePane)
            {
                return false;
            }

            var rc1 = info.Regions[0];
            var rc2 = info.Regions[1];

            if (info.Mode == TwoPaneViewMode.Wide)
            {
                return rcControl.X < rc1.Width && rcControl.X + rcControl.Width > rc2.X;
            }

            if (info.Mode == TwoPaneViewMode.Tall)
            {
                return rcControl.Y < rc1.Height && rcControl.Y + rcControl.Height > rc2.Y;
            }

            return false;
        }

        private void GoToLayoutState(string stateName)
        {
            VisualStateManager.GoToState(this, stateName, true);
        }

        private void UpdatePaneHosts()
        {
            UpdatePaneHost(_pane1Host, Pane1);
            UpdatePaneHost(_pane2Host, Pane2);
        }

        private static void UpdatePaneHost(Border host, UIElement pane)
        {
            if (host == null || ReferenceEquals(host.Child, pane))
            {
                return;
            }

            host.Child = null;
            if (pane != null)
            {
                host.Child = pane;
            }
        }

        private void ClearTemplatePartHandlers()
        {
            if (_pane1Host != null && ReferenceEquals(_pane1Host.Child, Pane1))
            {
                _pane1Host.Child = null;
            }

            if (_pane2Host != null && ReferenceEquals(_pane2Host.Child, Pane2))
            {
                _pane2Host.Child = null;
            }

            if (_pane1ScrollViewer != null)
            {
                _pane1ScrollViewer.Loaded -= OnScrollViewerLoaded;
                _pane1ScrollViewer = null;
            }

            if (_pane2ScrollViewer != null)
            {
                _pane2ScrollViewer.Loaded -= OnScrollViewerLoaded;
                _pane2ScrollViewer = null;
            }

            _pane1Host = null;
            _pane2Host = null;
        }

        private static FrameworkElement FindInVisualTreeByName(DependencyObject root, string name)
        {
            if (root == null)
            {
                return null;
            }

            var childrenCount = VisualTreeHelper.GetChildrenCount(root);
            for (var i = 0; i < childrenCount; i++)
            {
                var child = VisualTreeHelper.GetChild(root, i);
                if (child is FrameworkElement element && element.Name == name)
                {
                    return element;
                }

                var match = FindInVisualTreeByName(child, name);
                if (match != null)
                {
                    return match;
                }
            }

            return null;
        }

        private bool _loaded;
        private ViewMode _currentMode = ViewMode.None;
        private ScrollViewer _pane1ScrollViewer;
        private ScrollViewer _pane2ScrollViewer;
        private Border _pane1Host;
        private Border _pane2Host;
        private ColumnDefinition _columnLeft;
        private ColumnDefinition _columnMiddle;
        private ColumnDefinition _columnRight;
        private RowDefinition _rowTop;
        private RowDefinition _rowMiddle;
        private RowDefinition _rowBottom;

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
    }

    internal sealed class DisplayRegionHelperInfo
    {
        public TwoPaneViewMode Mode { get; set; } = TwoPaneViewMode.SinglePane;

        public Rect[] Regions { get; } = new Rect[2];
    }

    internal static class DisplayRegionHelper
    {
        private static readonly Rect SimulateWide0 = new Rect(0, 0, 300, 400);
        private static readonly Rect SimulateWide1 = new Rect(312, 0, 300, 400);
        private static readonly Rect SimulateTall0 = new Rect(0, 0, 400, 300);
        private static readonly Rect SimulateTall1 = new Rect(0, 312, 400, 300);

        public static DisplayRegionHelperInfo GetRegionInfo()
        {
            var info = new DisplayRegionHelperInfo();

            if (SimulateDisplayRegions)
            {
                if (SimulateMode == TwoPaneViewMode.Wide)
                {
                    info.Regions[0] = SimulateWide0;
                    info.Regions[1] = SimulateWide1;
                    info.Mode = TwoPaneViewMode.Wide;
                }
                else if (SimulateMode == TwoPaneViewMode.Tall)
                {
                    info.Regions[0] = SimulateTall0;
                    info.Regions[1] = SimulateTall1;
                    info.Mode = TwoPaneViewMode.Tall;
                }
                else
                {
                    info.Regions[0] = SimulateWide0;
                }
            }

            return info;
        }

        public static FrameworkElement WindowElement(TwoPaneView owner)
        {
            var window = Window.GetWindow(owner);
            var content = window?.Content as FrameworkElement;

            if (SimulateDisplayRegions)
            {
                return FindInVisualTreeByName(content, "SimulatedWindow");
            }

            return content ?? owner;
        }

        public static Rect WindowRect(TwoPaneView owner)
        {
            if (SimulateDisplayRegions)
            {
                var windowElement = WindowElement(owner);
                if (windowElement != null)
                {
                    return new Rect(0, 0, windowElement.ActualWidth, windowElement.ActualHeight);
                }
            }

            var window = Window.GetWindow(owner);
            if (window != null)
            {
                return new Rect(0, 0, window.ActualWidth, window.ActualHeight);
            }

            return Rect.Empty;
        }

        public static bool SimulateDisplayRegions { get; set; }

        public static TwoPaneViewMode SimulateMode { get; set; } = TwoPaneViewMode.SinglePane;

        private static FrameworkElement FindInVisualTreeByName(DependencyObject root, string name)
        {
            if (root == null)
            {
                return null;
            }

            if (root is FrameworkElement element && element.Name == name)
            {
                return element;
            }

            var childrenCount = VisualTreeHelper.GetChildrenCount(root);
            for (var i = 0; i < childrenCount; i++)
            {
                var child = VisualTreeHelper.GetChild(root, i);
                var match = FindInVisualTreeByName(child, name);
                if (match != null)
                {
                    return match;
                }
            }

            return null;
        }
    }

    internal static class DisplayRegionHelperTestApi
    {
        public static bool SimulateDisplayRegions
        {
            get => DisplayRegionHelper.SimulateDisplayRegions;
            set => DisplayRegionHelper.SimulateDisplayRegions = value;
        }

        public static TwoPaneViewMode SimulateMode
        {
            get => DisplayRegionHelper.SimulateMode;
            set => DisplayRegionHelper.SimulateMode = value;
        }
    }
}
