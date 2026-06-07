using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Markup;
using System.Windows.Media;

namespace ModernWpf.Controls.Primitives
{
    [ContentProperty(nameof(Child))]
    public class WindowedPopup : FrameworkElement, IAddChild
    {
        public static readonly DependencyProperty ChildProperty =
            DependencyProperty.Register(
                nameof(Child),
                typeof(UIElement),
                typeof(WindowedPopup),
                new PropertyMetadata(OnChildChanged));

        public UIElement Child
        {
            get => (UIElement)GetValue(ChildProperty);
            set => SetValue(ChildProperty, value);
        }

        public static readonly DependencyProperty IsOpenProperty =
            DependencyProperty.Register(
                nameof(IsOpen),
                typeof(bool),
                typeof(WindowedPopup),
                new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnIsOpenChanged));

        public bool IsOpen
        {
            get => (bool)GetValue(IsOpenProperty);
            set => SetValue(IsOpenProperty, value);
        }

        public static readonly DependencyProperty PlacementTargetProperty =
            DependencyProperty.Register(
                nameof(PlacementTarget),
                typeof(FrameworkElement),
                typeof(WindowedPopup),
                new PropertyMetadata(OnPlacementPropertyChanged));

        public FrameworkElement PlacementTarget
        {
            get => (FrameworkElement)GetValue(PlacementTargetProperty);
            set => SetValue(PlacementTargetProperty, value);
        }

        public static readonly DependencyProperty DesiredPlacementProperty =
            DependencyProperty.Register(
                nameof(DesiredPlacement),
                typeof(PopupPlacementMode),
                typeof(WindowedPopup),
                new PropertyMetadata(PopupPlacementMode.Auto, OnPlacementPropertyChanged));

        public PopupPlacementMode DesiredPlacement
        {
            get => (PopupPlacementMode)GetValue(DesiredPlacementProperty);
            set => SetValue(DesiredPlacementProperty, value);
        }

        private static readonly DependencyPropertyKey ActualPlacementPropertyKey =
            DependencyProperty.RegisterReadOnly(
                nameof(ActualPlacement),
                typeof(PopupPlacementMode),
                typeof(WindowedPopup),
                new PropertyMetadata(PopupPlacementMode.Auto));

        public static readonly DependencyProperty ActualPlacementProperty = ActualPlacementPropertyKey.DependencyProperty;

        public PopupPlacementMode ActualPlacement => (PopupPlacementMode)GetValue(ActualPlacementProperty);

        public static readonly DependencyProperty HorizontalOffsetProperty =
            DependencyProperty.Register(
                nameof(HorizontalOffset),
                typeof(double),
                typeof(WindowedPopup),
                new PropertyMetadata(0d, OnPlacementPropertyChanged));

        public double HorizontalOffset
        {
            get => (double)GetValue(HorizontalOffsetProperty);
            set => SetValue(HorizontalOffsetProperty, value);
        }

        public static readonly DependencyProperty VerticalOffsetProperty =
            DependencyProperty.Register(
                nameof(VerticalOffset),
                typeof(double),
                typeof(WindowedPopup),
                new PropertyMetadata(0d, OnPlacementPropertyChanged));

        public double VerticalOffset
        {
            get => (double)GetValue(VerticalOffsetProperty);
            set => SetValue(VerticalOffsetProperty, value);
        }

        public event EventHandler Opened;
        public event EventHandler Closed;
        public event EventHandler<object> ActualPlacementChanged;

        void IAddChild.AddChild(object value)
        {
            if (value is UIElement child)
            {
                Child = child;
                return;
            }

            throw new ArgumentException("WindowedPopup child must be a UIElement.", nameof(value));
        }

        void IAddChild.AddText(string text)
        {
            if (!string.IsNullOrEmpty(text))
            {
                ((IAddChild)this).AddChild(new TextBlock { Text = text });
            }
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            return new Size();
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            return finalSize;
        }

        private static void OnChildChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var popup = (WindowedPopup)d;
            popup.OnChildChanged((UIElement)e.OldValue, (UIElement)e.NewValue);
        }

        private void OnChildChanged(UIElement oldChild, UIElement newChild)
        {
            if (_root != null)
            {
                _root.Child = newChild;
            }

            if (oldChild != null)
            {
                oldChild.RemoveHandler(SizeChangedEvent, _childSizeChangedHandler);
                RemoveLogicalChild(oldChild);
            }

            if (newChild != null)
            {
                AddLogicalChild(newChild);
                newChild.AddHandler(SizeChangedEvent, _childSizeChangedHandler);
            }

            Reposition();
        }

        private static void OnIsOpenChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var popup = (WindowedPopup)d;
            if ((bool)e.NewValue)
            {
                popup.Open();
            }
            else
            {
                popup.Close();
            }
        }

        private static void OnPlacementPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((WindowedPopup)d).OnPlacementPropertyChanged(e.Property);
        }

        private void OnPlacementPropertyChanged(DependencyProperty property)
        {
            if (property == PlacementTargetProperty)
            {
                UpdatePlacementTargetHandlers();
            }

            Reposition();
        }

        private void Open()
        {
            if (_source != null)
            {
                Reposition();
                return;
            }

            if (Child == null)
            {
                SetCurrentValue(IsOpenProperty, false);
                return;
            }

            UpdatePlacementTargetHandlers();
            var placement = CalculatePlacement();

            _root = new WindowedPopupRoot { Child = Child };
            _source = CreateSource(placement);
            _source.RootVisual = _root;
            _source.AddHook(WndProc);

            ApplyPlacement(placement, show: true);
            Opened?.Invoke(this, EventArgs.Empty);
        }

        private void Close()
        {
            ClearPlacementTargetHandlers();

            if (_root != null)
            {
                _root.Child = null;
            }

            if (_source != null)
            {
                _source.RemoveHook(WndProc);
                _source.RootVisual = null;
                _source.Dispose();
                _source = null;
                _root = null;
                Closed?.Invoke(this, EventArgs.Empty);
            }
        }

        private void Reposition()
        {
            if (!IsOpen || _source == null || _source.IsDisposed)
            {
                return;
            }

            ApplyPlacement(CalculatePlacement(), show: true);
        }

        private PlacementResult CalculatePlacement()
        {
            var child = Child;
            var placementTarget = PlacementTarget;
            var desiredPlacement = DesiredPlacement;

            if (child == null || placementTarget == null || desiredPlacement == PopupPlacementMode.Auto)
            {
                SetActualPlacement(PopupPlacementMode.Auto);
                return new PlacementResult();
            }

            child.Measure(InfiniteSize);
            var hostSize = GetMeasuredHostSize(child);
            var contentBounds = GetPlacementBounds(child, hostSize);
            var targetBounds = GetTargetScreenBounds(placementTarget);
            var transformToDevice = GetTransformToDevice(placementTarget);
            var hostSizeDevice = TransformSize(transformToDevice, hostSize);
            var contentOffsetDevice = TransformPoint(transformToDevice, contentBounds.TopLeft);
            var contentSizeDevice = TransformSize(transformToDevice, contentBounds.Size);
            var availableRect = GetAvailableScreenRect(targetBounds);
            var flowDirection = FlowDirection;

            var majorPlacement = GetMajorPlacementFromPlacement(desiredPlacement, flowDirection);
            var justification = GetJustificationFromPlacement(desiredPlacement, flowDirection);
            var contentPosition = GetPositionFromMajorPlacementAndJustification(
                contentSizeDevice,
                targetBounds,
                flowDirection,
                majorPlacement,
                justification);
            var valueChanged = FlipMajorPlacementAndJustificationIfOutOfBounds(
                ref majorPlacement,
                ref justification,
                contentPosition,
                contentSizeDevice,
                availableRect);

            if (valueChanged)
            {
                contentPosition = GetPositionFromMajorPlacementAndJustification(
                    contentSizeDevice,
                    targetBounds,
                    flowDirection,
                    majorPlacement,
                    justification);

                FlipMajorPlacementAndJustificationIfOutOfBounds(
                    ref majorPlacement,
                    ref justification,
                    contentPosition,
                    contentSizeDevice,
                    availableRect);

                contentPosition = GetPositionFromMajorPlacementAndJustification(
                    contentSizeDevice,
                    targetBounds,
                    flowDirection,
                    majorPlacement,
                    justification);
            }

            var actualPlacement = GetPlacementFromMajorPlacementAndJustification(majorPlacement, justification, flowDirection);
            SetActualPlacement(actualPlacement);

            var offsetDevice = TransformPoint(transformToDevice, new Point(HorizontalOffset, VerticalOffset));
            var hostPosition = new Point(
                contentPosition.X - contentOffsetDevice.X + offsetDevice.X,
                contentPosition.Y - contentOffsetDevice.Y + offsetDevice.Y);

            return new PlacementResult
            {
                X = DoubleToInt(hostPosition.X),
                Y = DoubleToInt(hostPosition.Y),
                Width = Math.Max(1, DoubleToInt(hostSizeDevice.Width)),
                Height = Math.Max(1, DoubleToInt(hostSizeDevice.Height))
            };
        }

        private void ApplyPlacement(PlacementResult placement, bool show)
        {
            if (_source == null || _source.IsDisposed)
            {
                return;
            }

            var flags = SWP_NOACTIVATE;
            if (show)
            {
                flags |= SWP_SHOWWINDOW;
            }

            SetWindowPos(
                _source.Handle,
                HWND_TOPMOST,
                placement.X,
                placement.Y,
                placement.Width,
                placement.Height,
                flags);
        }

        private HwndSource CreateSource(PlacementResult placement)
        {
            var parameters = new HwndSourceParameters(string.Empty)
            {
                WindowStyle = WS_POPUP | WS_CLIPSIBLINGS,
                ExtendedWindowStyle = WS_EX_TOOLWINDOW | WS_EX_NOACTIVATE | WS_EX_TOPMOST,
                UsesPerPixelOpacity = true
            };
            parameters.SetPosition(placement.X, placement.Y);
            parameters.SetSize(Math.Max(1, placement.Width), Math.Max(1, placement.Height));

            var parent = GetParentWindowHandle(PlacementTarget);
            if (parent != IntPtr.Zero)
            {
                parameters.ParentWindow = parent;
            }

            var source = new HwndSource(parameters);
            if (source.CompositionTarget != null)
            {
                source.CompositionTarget.BackgroundColor = Colors.Transparent;
            }

            return source;
        }

        private void UpdatePlacementTargetHandlers()
        {
            var placementTarget = PlacementTarget;
            if (ReferenceEquals(_trackedPlacementTarget, placementTarget))
            {
                return;
            }

            ClearPlacementTargetHandlers();

            _trackedPlacementTarget = placementTarget;
            if (_trackedPlacementTarget != null)
            {
                _trackedPlacementTarget.LayoutUpdated += OnPlacementTargetLayoutUpdated;
                _trackedPlacementTarget.Unloaded += OnPlacementTargetUnloaded;
            }
        }

        private void ClearPlacementTargetHandlers()
        {
            if (_trackedPlacementTarget != null)
            {
                _trackedPlacementTarget.LayoutUpdated -= OnPlacementTargetLayoutUpdated;
                _trackedPlacementTarget.Unloaded -= OnPlacementTargetUnloaded;
                _trackedPlacementTarget = null;
            }
        }

        private void OnPlacementTargetLayoutUpdated(object sender, EventArgs e)
        {
            Reposition();
        }

        private void OnPlacementTargetUnloaded(object sender, RoutedEventArgs e)
        {
            SetCurrentValue(IsOpenProperty, false);
        }

        private void OnChildSizeChanged(object sender, SizeChangedEventArgs e)
        {
            Reposition();
        }

        private void SetActualPlacement(PopupPlacementMode value)
        {
            if (ActualPlacement == value)
            {
                return;
            }

            SetValue(ActualPlacementPropertyKey, value);
            ActualPlacementChanged?.Invoke(this, EventArgs.Empty);
        }

        private static Size GetMeasuredHostSize(UIElement child)
        {
            var desiredSize = child.DesiredSize;
            if (desiredSize.Width <= 0 && child.RenderSize.Width > 0)
            {
                desiredSize.Width = child.RenderSize.Width;
            }
            if (desiredSize.Height <= 0 && child.RenderSize.Height > 0)
            {
                desiredSize.Height = child.RenderSize.Height;
            }
            return desiredSize;
        }

        private static Rect GetPlacementBounds(UIElement child, Size hostSize)
        {
            if (child is ThemeShadowChrome shadowChrome &&
                shadowChrome.HasWindowedPopupPlacementBounds)
            {
                return shadowChrome.GetWindowedPopupPlacementBounds(hostSize);
            }

            return new Rect(hostSize);
        }

        private static Rect GetTargetScreenBounds(FrameworkElement target)
        {
            var topLeft = target.PointToScreen(new Point());
            var bottomRight = target.PointToScreen(new Point(target.ActualWidth, target.ActualHeight));
            return new Rect(topLeft, bottomRight);
        }

        private static Rect GetAvailableScreenRect(Rect targetBounds)
        {
            var nativeBounds = new RECT
            {
                Left = DoubleToInt(targetBounds.Left),
                Top = DoubleToInt(targetBounds.Top),
                Right = DoubleToInt(targetBounds.Right),
                Bottom = DoubleToInt(targetBounds.Bottom)
            };

            var monitor = MonitorFromRect(ref nativeBounds, MONITOR_DEFAULTTONEAREST);
            if (monitor != IntPtr.Zero)
            {
                var monitorInfo = new MONITORINFO();
                if (GetMonitorInfo(monitor, monitorInfo))
                {
                    return monitorInfo.Work.ToRect();
                }
            }

            return SystemParameters.WorkArea;
        }

        private static Matrix GetTransformToDevice(Visual visual)
        {
            if (visual != null &&
                PresentationSource.FromVisual(visual) is HwndSource source &&
                source.CompositionTarget != null)
            {
                return source.CompositionTarget.TransformToDevice;
            }

            return Matrix.Identity;
        }

        private static Size TransformSize(Matrix transform, Size size)
        {
            var point = transform.Transform(new Point(size.Width, size.Height));
            return new Size(Math.Abs(point.X), Math.Abs(point.Y));
        }

        private static Point TransformPoint(Matrix transform, Point point)
        {
            return transform.Transform(point);
        }

        private static IntPtr GetParentWindowHandle(Visual visual)
        {
            return visual != null && PresentationSource.FromVisual(visual) is HwndSource source
                ? source.Handle
                : IntPtr.Zero;
        }

        private static Point GetPositionFromMajorPlacementAndJustification(
            Size childSize,
            Rect targetBounds,
            FlowDirection flowDirection,
            MajorPlacementMode majorPlacement,
            PreferredJustification justification)
        {
            var position = new Point();

            switch (majorPlacement)
            {
                case MajorPlacementMode.Top:
                    position.Y = targetBounds.Y - childSize.Height;
                    break;
                case MajorPlacementMode.Bottom:
                    position.Y = targetBounds.Y + targetBounds.Height;
                    break;
                case MajorPlacementMode.Left:
                    position.X = targetBounds.X - childSize.Width;
                    break;
                case MajorPlacementMode.Right:
                    position.X = targetBounds.X + targetBounds.Width;
                    break;
            }

            switch (justification)
            {
                case PreferredJustification.HorizontalCenter:
                    position.X = targetBounds.X + targetBounds.Width / 2 - childSize.Width / 2;
                    break;
                case PreferredJustification.Left:
                    position.X = targetBounds.X;
                    break;
                case PreferredJustification.Right:
                    position.X = targetBounds.X + targetBounds.Width - childSize.Width;
                    break;
                case PreferredJustification.VerticalCenter:
                    position.Y = targetBounds.Y + targetBounds.Height / 2 - childSize.Height / 2;
                    break;
                case PreferredJustification.Top:
                    position.Y = targetBounds.Y;
                    break;
                case PreferredJustification.Bottom:
                    position.Y = targetBounds.Y + targetBounds.Height - childSize.Height;
                    break;
            }

            if (flowDirection == FlowDirection.RightToLeft)
            {
                position.X += childSize.Width;
            }

            return position;
        }

        private static bool FlipMajorPlacementAndJustificationIfOutOfBounds(
            ref MajorPlacementMode majorPlacement,
            ref PreferredJustification justification,
            Point position,
            Size childSize,
            Rect availableRect)
        {
            var valueChanged = false;

            if (position.X + childSize.Width > availableRect.X + availableRect.Width)
            {
                if (majorPlacement == MajorPlacementMode.Right)
                {
                    majorPlacement = MajorPlacementMode.Left;
                    valueChanged = true;
                }

                if (justification == PreferredJustification.Left)
                {
                    justification = PreferredJustification.Right;
                    valueChanged = true;
                }
            }
            else if (position.X < availableRect.X)
            {
                if (majorPlacement == MajorPlacementMode.Left)
                {
                    majorPlacement = MajorPlacementMode.Right;
                    valueChanged = true;
                }

                if (justification == PreferredJustification.Right)
                {
                    justification = PreferredJustification.Left;
                    valueChanged = true;
                }
            }

            if (position.Y < availableRect.Y)
            {
                if (majorPlacement == MajorPlacementMode.Top)
                {
                    majorPlacement = MajorPlacementMode.Bottom;
                    valueChanged = true;
                }

                if (justification == PreferredJustification.Bottom)
                {
                    justification = PreferredJustification.Top;
                    valueChanged = true;
                }
            }
            else if (position.Y + childSize.Height > availableRect.Y + availableRect.Height)
            {
                if (majorPlacement == MajorPlacementMode.Bottom)
                {
                    majorPlacement = MajorPlacementMode.Top;
                    valueChanged = true;
                }

                if (justification == PreferredJustification.Top)
                {
                    justification = PreferredJustification.Bottom;
                    valueChanged = true;
                }
            }

            return valueChanged;
        }

        private static MajorPlacementMode GetMajorPlacementFromPlacement(PopupPlacementMode placement, FlowDirection flowDirection)
        {
            switch (placement)
            {
                case PopupPlacementMode.Top:
                case PopupPlacementMode.TopEdgeAlignedLeft:
                case PopupPlacementMode.TopEdgeAlignedRight:
                    return MajorPlacementMode.Top;
                case PopupPlacementMode.Bottom:
                case PopupPlacementMode.BottomEdgeAlignedLeft:
                case PopupPlacementMode.BottomEdgeAlignedRight:
                    return MajorPlacementMode.Bottom;
                case PopupPlacementMode.Left:
                case PopupPlacementMode.LeftEdgeAlignedTop:
                case PopupPlacementMode.LeftEdgeAlignedBottom:
                    return flowDirection == FlowDirection.LeftToRight ? MajorPlacementMode.Left : MajorPlacementMode.Right;
                case PopupPlacementMode.Right:
                case PopupPlacementMode.RightEdgeAlignedTop:
                case PopupPlacementMode.RightEdgeAlignedBottom:
                    return flowDirection == FlowDirection.LeftToRight ? MajorPlacementMode.Right : MajorPlacementMode.Left;
                default:
                    return MajorPlacementMode.Auto;
            }
        }

        private static PreferredJustification GetJustificationFromPlacement(PopupPlacementMode placement, FlowDirection flowDirection)
        {
            switch (placement)
            {
                case PopupPlacementMode.Top:
                case PopupPlacementMode.Bottom:
                    return PreferredJustification.HorizontalCenter;
                case PopupPlacementMode.Left:
                case PopupPlacementMode.Right:
                    return PreferredJustification.VerticalCenter;
                case PopupPlacementMode.TopEdgeAlignedLeft:
                case PopupPlacementMode.BottomEdgeAlignedLeft:
                    return flowDirection == FlowDirection.LeftToRight ? PreferredJustification.Left : PreferredJustification.Right;
                case PopupPlacementMode.TopEdgeAlignedRight:
                case PopupPlacementMode.BottomEdgeAlignedRight:
                    return flowDirection == FlowDirection.LeftToRight ? PreferredJustification.Right : PreferredJustification.Left;
                case PopupPlacementMode.LeftEdgeAlignedTop:
                case PopupPlacementMode.RightEdgeAlignedTop:
                    return PreferredJustification.Top;
                case PopupPlacementMode.LeftEdgeAlignedBottom:
                case PopupPlacementMode.RightEdgeAlignedBottom:
                    return PreferredJustification.Bottom;
                default:
                    return PreferredJustification.Auto;
            }
        }

        private static PopupPlacementMode GetPlacementFromMajorPlacementAndJustification(
            MajorPlacementMode majorPlacement,
            PreferredJustification justification,
            FlowDirection flowDirection)
        {
            switch (majorPlacement)
            {
                case MajorPlacementMode.Top:
                    switch (justification)
                    {
                        case PreferredJustification.HorizontalCenter:
                            return PopupPlacementMode.Top;
                        case PreferredJustification.Left:
                            return flowDirection == FlowDirection.LeftToRight ? PopupPlacementMode.TopEdgeAlignedLeft : PopupPlacementMode.TopEdgeAlignedRight;
                        case PreferredJustification.Right:
                            return flowDirection == FlowDirection.LeftToRight ? PopupPlacementMode.TopEdgeAlignedRight : PopupPlacementMode.TopEdgeAlignedLeft;
                    }
                    break;
                case MajorPlacementMode.Bottom:
                    switch (justification)
                    {
                        case PreferredJustification.HorizontalCenter:
                            return PopupPlacementMode.Bottom;
                        case PreferredJustification.Left:
                            return flowDirection == FlowDirection.LeftToRight ? PopupPlacementMode.BottomEdgeAlignedLeft : PopupPlacementMode.BottomEdgeAlignedRight;
                        case PreferredJustification.Right:
                            return flowDirection == FlowDirection.LeftToRight ? PopupPlacementMode.BottomEdgeAlignedRight : PopupPlacementMode.BottomEdgeAlignedLeft;
                    }
                    break;
                case MajorPlacementMode.Left:
                    if (flowDirection == FlowDirection.LeftToRight)
                    {
                        switch (justification)
                        {
                            case PreferredJustification.VerticalCenter:
                                return PopupPlacementMode.Left;
                            case PreferredJustification.Top:
                                return PopupPlacementMode.LeftEdgeAlignedTop;
                            case PreferredJustification.Bottom:
                                return PopupPlacementMode.LeftEdgeAlignedBottom;
                        }
                    }
                    else
                    {
                        switch (justification)
                        {
                            case PreferredJustification.VerticalCenter:
                                return PopupPlacementMode.Right;
                            case PreferredJustification.Top:
                                return PopupPlacementMode.RightEdgeAlignedTop;
                            case PreferredJustification.Bottom:
                                return PopupPlacementMode.RightEdgeAlignedBottom;
                        }
                    }
                    break;
                case MajorPlacementMode.Right:
                    if (flowDirection == FlowDirection.LeftToRight)
                    {
                        switch (justification)
                        {
                            case PreferredJustification.VerticalCenter:
                                return PopupPlacementMode.Right;
                            case PreferredJustification.Top:
                                return PopupPlacementMode.RightEdgeAlignedTop;
                            case PreferredJustification.Bottom:
                                return PopupPlacementMode.RightEdgeAlignedBottom;
                        }
                    }
                    else
                    {
                        switch (justification)
                        {
                            case PreferredJustification.VerticalCenter:
                                return PopupPlacementMode.Left;
                            case PreferredJustification.Top:
                                return PopupPlacementMode.LeftEdgeAlignedTop;
                            case PreferredJustification.Bottom:
                                return PopupPlacementMode.LeftEdgeAlignedBottom;
                        }
                    }
                    break;
            }

            return PopupPlacementMode.Auto;
        }

        private static int DoubleToInt(double value)
        {
            return (0 < value) ? (int)(value + 0.5) : (int)(value - 0.5);
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == WM_MOUSEACTIVATE)
            {
                handled = true;
                return new IntPtr(MA_NOACTIVATE);
            }

            return IntPtr.Zero;
        }

        private sealed class WindowedPopupRoot : FrameworkElement
        {
            public UIElement Child
            {
                get => _child;
                set
                {
                    if (_child == value)
                    {
                        return;
                    }

                    if (_child != null)
                    {
                        RemoveVisualChild(_child);
                    }

                    _child = value;

                    if (_child != null)
                    {
                        AddVisualChild(_child);
                    }

                    InvalidateMeasure();
                }
            }

            protected override int VisualChildrenCount => _child != null ? 1 : 0;

            protected override Visual GetVisualChild(int index)
            {
                if (_child == null || index != 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(index));
                }

                return _child;
            }

            protected override Size MeasureOverride(Size availableSize)
            {
                if (_child == null)
                {
                    return new Size();
                }

                _child.Measure(InfiniteSize);
                return _child.DesiredSize;
            }

            protected override Size ArrangeOverride(Size finalSize)
            {
                _child?.Arrange(new Rect(finalSize));
                return finalSize;
            }

            private UIElement _child;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;

            public Rect ToRect()
            {
                return new Rect(Left, Top, Right - Left, Bottom - Top);
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private sealed class MONITORINFO
        {
            public int Size = Marshal.SizeOf(typeof(MONITORINFO));
            public RECT Monitor;
            public RECT Work;
            public int Flags;
        }

        private struct PlacementResult
        {
            public int X;
            public int Y;
            public int Width;
            public int Height;
        }

        private enum MajorPlacementMode
        {
            Auto,
            Top,
            Bottom,
            Left,
            Right
        }

        private enum PreferredJustification
        {
            Auto,
            HorizontalCenter,
            VerticalCenter,
            Left,
            Right,
            Top,
            Bottom
        }

        private static readonly Size InfiniteSize = new Size(double.PositiveInfinity, double.PositiveInfinity);
        private readonly SizeChangedEventHandler _childSizeChangedHandler;

        private HwndSource _source;
        private WindowedPopupRoot _root;
        private FrameworkElement _trackedPlacementTarget;
        private const int WS_POPUP = unchecked((int)0x80000000);
        private const int WS_CLIPSIBLINGS = 0x04000000;
        private const int WS_EX_TOOLWINDOW = 0x00000080;
        private const int WS_EX_NOACTIVATE = 0x08000000;
        private const int WS_EX_TOPMOST = 0x00000008;
        private const int SWP_NOACTIVATE = 0x0010;
        private const int SWP_SHOWWINDOW = 0x0040;
        private const int WM_MOUSEACTIVATE = 0x0021;
        private const int MA_NOACTIVATE = 3;
        private const int MONITOR_DEFAULTTONEAREST = 0x00000002;
        private static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);

        public WindowedPopup()
        {
            _childSizeChangedHandler = OnChildSizeChanged;
        }

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, int flags);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr MonitorFromRect(ref RECT rect, int flags);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool GetMonitorInfo(IntPtr monitor, [In, Out] MONITORINFO monitorInfo);
    }
}
