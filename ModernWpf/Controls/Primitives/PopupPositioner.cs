using MS.Internal;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Interop;
using System.Windows.Threading;

namespace ModernWpf.Controls.Primitives
{
    internal class PopupPositioner : DependencyObject, IDisposable
    {
        static PopupPositioner()
        {
            IsSupported = Delegates.GetPlacementInternal != null &&
                          Delegates.GetDropOpposite != null &&
                          Delegates.SetDropOpposite != null &&
                          Delegates.GetAnimateFromRight != null &&
                          Delegates.SetAnimateFromRight != null &&
                          Delegates.GetAnimateFromBottom != null &&
                          Delegates.SetAnimateFromBottom != null &&
                          Delegates.GetPlacementTargetInterestPoints != null &&
                          Delegates.GetChildInterestPoints != null &&
                          Delegates.GetScreenBounds != null &&
                          Fields._positionInfo != null &&
                          Fields._secHelper != null &&
                          PopupSecurityHelper.IsSupported &&
                          PositionInfo.IsSupported;
        }

        public PopupPositioner(Popup popup)
        {
            if (!IsSupported)
            {
                throw new NotSupportedException();
            }

            _popup = popup;
            _secHelper = new PopupSecurityHelper(Fields._secHelper.GetValue(_popup));

            SetPositioner(popup, this);

            BindingOperations.SetBinding(this, IsOpenProperty, new Binding { Path = new PropertyPath(Popup.IsOpenProperty), Source = popup });

            popup.Opened += OnPopupOpened;

            if (popup.IsOpen)
            {
                OnPopupOpened(null, null);
            }
        }

        public void Dispose()
        {
            if (_isDisposed)
            {
                return;
            }

            if (_popup != null)
            {
                _popup.Opened -= OnPopupOpened;
                _popup.ClearValue(PositionerProperty);
            }

            ClearValue(IsOpenProperty);

            if (_window != null)
            {
                _window.AutoResized -= OnWindowResize;
                _window = null;
            }

            _secHelper = null;
            _positionInfo = null;

            _isDisposed = true;
        }

        public static bool IsSupported { get; }

        #region Popup Members

        public bool IsOpen => _popup.IsOpen;

        public PlacementMode Placement => _popup.Placement;

        /// <summary>
        ///     Tooltips should show on Keyboard focus.
        ///     Chooses the behavior of where the Popup should be placed on screen.
        ///     Takes into account TreatMousePlacementAsBottom to place tooltips correctly on keyboard focus.
        /// </summary>
        internal PlacementMode PlacementInternal => Delegates.GetPlacementInternal(_popup);

        public CustomPopupPlacementCallback CustomPopupPlacementCallback => _popup.CustomPopupPlacementCallback;

        public double HorizontalOffset => _popup.HorizontalOffset;
        public double VerticalOffset => _popup.VerticalOffset;

        internal bool DropOpposite
        {
            get => Delegates.GetDropOpposite(_popup);
            set => Delegates.SetDropOpposite(_popup, value);
        }

        private void OnWindowResize(object sender, AutoResizedEventArgs e)
        {
            if (_positionInfo == null)
            {
                return;
            }

            if (e.Size != _positionInfo.ChildSize)
            {
                _positionInfo.ChildSize = e.Size;

                // Reposition the popup
                Reposition();
            }
        }

        internal void Reposition()
        {
            if (IsOpen && _secHelper.IsWindowAlive())
            {
                if (CheckAccess())
                {
                    UpdatePosition();
                }
                else
                {
                    Dispatcher.BeginInvoke(DispatcherPriority.Normal, new DispatcherOperationCallback(delegate (object param)
                    {
                        Debug.Assert(CheckAccess(), "AsyncReposition not called on the dispatcher thread.");

                        Reposition();

                        return null;
                    }), null);
                }
            }
        }

        // This struct is returned by GetPointCombination to indicate
        // which points on the target can align with points on the child
        private struct PointCombination
        {
            public PointCombination(InterestPoint targetInterestPoint, InterestPoint childInterestPoint)
            {
                TargetInterestPoint = targetInterestPoint;
                ChildInterestPoint = childInterestPoint;
            }

            public InterestPoint TargetInterestPoint;
            public InterestPoint ChildInterestPoint;
        }

        // To position the popup, we find the InterestPoints of the placement rectangle/point
        // in the screen coordinate space.  We also find the InterestPoints of the child in
        // the popup's space.  Then we attempt all valid combinations of matching InterestPoints
        // (based on PlacementMode) to find the position that best fits on the screen.
        // NOTE: any reference to the screen implies the monitor for full trust and
        //       the browser area for partial trust
        private void UpdatePosition()
        {
            if (_window == null || _window.RootVisual == null)
                return;

            PlacementMode placement = PlacementInternal;

            // Get a list of the corners of the target/child in screen space
            Point[] placementTargetInterestPoints = GetPlacementTargetInterestPoints(placement);
            Point[] childInterestPoints = GetChildInterestPoints(placement);

            // Find bounds of screen and child in screen space
            Rect targetBounds = GetBounds(placementTargetInterestPoints);
            Rect screenBounds;
            Rect childBounds = GetBounds(childInterestPoints);

            double childArea = childBounds.Width * childBounds.Height;

            _positionInfo ??= new PositionInfo(Fields._positionInfo.GetValue(_popup));

            // Rank possible positions
            int bestIndex = -1;
            Vector bestTranslation = new Vector(_positionInfo.X, _positionInfo.Y);
            double bestScore = -1;
            PopupPrimaryAxis bestAxis = PopupPrimaryAxis.None;

            int positions;

            CustomPopupPlacement[] customPlacements = null;

            // Find the number of possible positions
            if (placement == PlacementMode.Custom)
            {
                CustomPopupPlacementCallback customCallback = CustomPopupPlacementCallback;
                if (customCallback != null)
                {
                    customPlacements = customCallback(childBounds.Size, targetBounds.Size, new Point(HorizontalOffset, VerticalOffset));
                }
                positions = customPlacements == null ? 0 : customPlacements.Length;

                // Return if callback closed the popup
                if (!IsOpen)
                    return;
            }
            else
            {
                positions = GetNumberOfCombinations(placement);
            }

            // Try each position until the best one is found
            for (int i = 0; i < positions; i++)
            {
                Vector popupTranslation;

                bool animateFromRight = false;
                bool animateFromBottom = false;

                PopupPrimaryAxis axis;

                // Get the ith Position to rank
                if (placement == PlacementMode.Custom)
                {
                    // The custom callback only calculates relative to 0,0
                    // so the placementTarget's top/left need to be re-applied.
                    popupTranslation = (Vector)placementTargetInterestPoints[(int)InterestPoint.TopLeft]
                                      + (Vector)customPlacements[i].Point;  // vector from origin

                    axis = customPlacements[i].PrimaryAxis;
                }
                else
                {
                    PointCombination pointCombination = GetPointCombination(placement, i, out axis);

                    InterestPoint targetInterestPoint = pointCombination.TargetInterestPoint;
                    InterestPoint childInterestPoint = pointCombination.ChildInterestPoint;

                    // Compute the vector from the screen origin to the top left corner of the popup
                    // that will cause the the two interest points to overlap
                    popupTranslation = placementTargetInterestPoints[(int)targetInterestPoint]
                                       - childInterestPoints[(int)childInterestPoint];

                    // Check the matching points to see which direction to animate
                    animateFromRight = childInterestPoint == InterestPoint.TopRight || childInterestPoint == InterestPoint.BottomRight;
                    animateFromBottom = childInterestPoint == InterestPoint.BottomLeft || childInterestPoint == InterestPoint.BottomRight;
                }

                // Find percent of popup on screen by translating the popup bounds
                // and calculating the percent of the bounds that is on screen
                // Note: this score is based on the percent of the popup that is on screen
                //       not the percent of the child that is on screen.  For certain
                //       scenarios, this may produce in counter-intuitive results.
                //       If this is a problem, more complex scoring is needed
                Rect tranlsatedChildBounds = Rect.Offset(childBounds, popupTranslation);
                screenBounds = GetScreenBounds(targetBounds, placementTargetInterestPoints[(int)InterestPoint.TopLeft]);
                Rect currentIntersection = Rect.Intersect(screenBounds, tranlsatedChildBounds);

                // Calculate area of intersection
                double score = currentIntersection != Rect.Empty ? currentIntersection.Width * currentIntersection.Height : 0;

                // If current score is better than the best score so far, save the position info
                if (score - bestScore > Tolerance)
                {
                    bestIndex = i;
                    bestTranslation = popupTranslation;
                    bestScore = score;
                    bestAxis = axis;

                    AnimateFromRight = animateFromRight;
                    AnimateFromBottom = animateFromBottom;

                    // Stop when we find a popup that is completely on screen
                    if (Math.Abs(score - childArea) < Tolerance)
                    {
                        break;
                    }
                }
            }

            // When going left/right, if the edge of the monitor is hit
            // the next popup going left/right must also go in the opposite direction
            if (bestIndex >= 2 && (placement == PlacementMode.Right || placement == PlacementMode.Left))
            {
                // We switched sides, so flip the DropOpposite flag
                DropOpposite = !DropOpposite;
            }

            // Finally, take the best position and apply it to the popup
            int bestX = DoubleUtil.DoubleToInt(bestTranslation.X);
            int bestY = DoubleUtil.DoubleToInt(bestTranslation.Y);
            if (bestX != _positionInfo.X || bestY != _positionInfo.Y)
            {
                _positionInfo.X = bestX;
                _positionInfo.Y = bestY;
                _secHelper.SetPopupPos(true, bestX, bestY, false, 0, 0);
            }
        }

        private Point[] GetPlacementTargetInterestPoints(PlacementMode placement)
        {
            return Delegates.GetPlacementTargetInterestPoints(_popup, placement);
        }

        // Returns the ith possible alignment for the given PlacementMode
        private PointCombination GetPointCombination(PlacementMode placement, int i, out PopupPrimaryAxis axis)
        {
            Debug.Assert(i >= 0 && i < GetNumberOfCombinations(placement));

            bool dropFromRight = SystemParameters.MenuDropAlignment;

            switch (placement)
            {
                case PlacementMode.Bottom:
                case PlacementMode.Mouse:
                    axis = PopupPrimaryAxis.Horizontal;
                    if (dropFromRight)
                    {
                        if (i == 0) return new PointCombination(InterestPoint.BottomRight, InterestPoint.TopRight);
                        if (i == 1) return new PointCombination(InterestPoint.TopRight, InterestPoint.BottomRight);
                    }
                    else
                    {
                        if (i == 0) return new PointCombination(InterestPoint.BottomLeft, InterestPoint.TopLeft);
                        if (i == 1) return new PointCombination(InterestPoint.TopLeft, InterestPoint.BottomLeft);
                    }
                    break;


                case PlacementMode.Top:
                    axis = PopupPrimaryAxis.Horizontal;
                    if (dropFromRight)
                    {
                        if (i == 0) return new PointCombination(InterestPoint.TopRight, InterestPoint.BottomRight);
                        if (i == 1) return new PointCombination(InterestPoint.BottomRight, InterestPoint.TopRight);
                    }
                    else
                    {
                        if (i == 0) return new PointCombination(InterestPoint.TopLeft, InterestPoint.BottomLeft);
                        if (i == 1) return new PointCombination(InterestPoint.BottomLeft, InterestPoint.TopLeft);
                    }
                    break;


                case PlacementMode.Right:
                case PlacementMode.Left:
                    axis = PopupPrimaryAxis.Vertical;
                    dropFromRight |= DropOpposite;

                    if (dropFromRight && placement == PlacementMode.Right ||
                        !dropFromRight && placement == PlacementMode.Left)
                    {
                        if (i == 0) return new PointCombination(InterestPoint.TopLeft, InterestPoint.TopRight);
                        if (i == 1) return new PointCombination(InterestPoint.BottomLeft, InterestPoint.BottomRight);
                        if (i == 2) return new PointCombination(InterestPoint.TopRight, InterestPoint.TopLeft);
                        if (i == 3) return new PointCombination(InterestPoint.BottomRight, InterestPoint.BottomLeft);
                    }
                    else
                    {
                        if (i == 0) return new PointCombination(InterestPoint.TopRight, InterestPoint.TopLeft);
                        if (i == 1) return new PointCombination(InterestPoint.BottomRight, InterestPoint.BottomLeft);
                        if (i == 2) return new PointCombination(InterestPoint.TopLeft, InterestPoint.TopRight);
                        if (i == 3) return new PointCombination(InterestPoint.BottomLeft, InterestPoint.BottomRight);
                    }
                    break;

                case PlacementMode.Relative:
                case PlacementMode.RelativePoint:
                case PlacementMode.MousePoint:
                case PlacementMode.AbsolutePoint:
                    axis = PopupPrimaryAxis.Horizontal;
                    if (dropFromRight)
                    {
                        if (i == 0) return new PointCombination(InterestPoint.TopLeft, InterestPoint.TopRight);
                        if (i == 1) return new PointCombination(InterestPoint.TopLeft, InterestPoint.TopLeft);
                        if (i == 2) return new PointCombination(InterestPoint.TopLeft, InterestPoint.BottomRight);
                        if (i == 3) return new PointCombination(InterestPoint.TopLeft, InterestPoint.BottomLeft);
                    }
                    else
                    {
                        if (i == 0) return new PointCombination(InterestPoint.TopLeft, InterestPoint.TopLeft);
                        if (i == 1) return new PointCombination(InterestPoint.TopLeft, InterestPoint.TopRight);
                        if (i == 2) return new PointCombination(InterestPoint.TopLeft, InterestPoint.BottomLeft);
                        if (i == 3) return new PointCombination(InterestPoint.TopLeft, InterestPoint.BottomRight);
                    }
                    break;

                case PlacementMode.Center:
                    axis = PopupPrimaryAxis.None;
                    return new PointCombination(InterestPoint.Center, InterestPoint.Center);

                case PlacementMode.Absolute:
                case PlacementMode.Custom:
                default:
                    axis = PopupPrimaryAxis.None;
                    return new PointCombination(InterestPoint.TopLeft, InterestPoint.TopLeft);
            }

            return new PointCombination(InterestPoint.TopLeft, InterestPoint.TopRight);
        }

        // Retrieves a list of the interesting points of the popups child in the popup window space
        private Point[] GetChildInterestPoints(PlacementMode placement)
        {
            return Delegates.GetChildInterestPoints(_popup, placement);
        }

        // Gets the smallest rectangle that contains all points in the list
        private Rect GetBounds(Point[] interestPoints)
        {
            double left, right, top, bottom;

            left = right = interestPoints[0].X;
            top = bottom = interestPoints[0].Y;

            for (int i = 1; i < interestPoints.Length; i++)
            {
                double x = interestPoints[i].X;
                double y = interestPoints[i].Y;
                if (x < left) left = x;
                if (x > right) right = x;
                if (y < top) top = y;
                if (y > bottom) bottom = y;
            }
            return new Rect(left, top, right - left, bottom - top);
        }

        // Gets the number of InterestPoint combinations for the given placement
        private static int GetNumberOfCombinations(PlacementMode placement)
        {
            switch (placement)
            {
                case PlacementMode.Bottom:
                case PlacementMode.Top:
                case PlacementMode.Mouse:
                    return 2;

                case PlacementMode.Right:
                case PlacementMode.Left:
                case PlacementMode.RelativePoint:
                case PlacementMode.MousePoint:
                case PlacementMode.AbsolutePoint:
                    return 4;

                case PlacementMode.Custom:
                    return 0;

                case PlacementMode.Absolute:
                case PlacementMode.Relative:
                case PlacementMode.Center:
                default:
                    return 1;
            }
        }

        private Rect GetScreenBounds(Rect boundingBox, Point p)
        {
            return Delegates.GetScreenBounds(_popup, boundingBox, p);
        }

        private bool AnimateFromRight
        {
            get => Delegates.GetAnimateFromRight(_popup);
            set => Delegates.SetAnimateFromRight(_popup, value);
        }

        private bool AnimateFromBottom
        {
            get => Delegates.GetAnimateFromBottom(_popup);
            set => Delegates.SetAnimateFromBottom(_popup, value);
        }

        internal const double Tolerance = 1.0e-2; // allow errors in double calculations

        #endregion

        #region Positioner

        private static readonly DependencyProperty PositionerProperty =
            DependencyProperty.RegisterAttached(
                "Positioner",
                typeof(PopupPositioner),
                typeof(PopupPositioner),
                new PropertyMetadata(OnPositionerChanged));

        internal static PopupPositioner GetPositioner(Popup popup)
        {
            return (PopupPositioner)popup.GetValue(PositionerProperty);
        }

        private static void SetPositioner(Popup popup, PopupPositioner value)
        {
            popup.SetValue(PositionerProperty, value);
        }

        private static void OnPositionerChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue is PopupPositioner oldValue)
            {
                oldValue.Dispose();
            }
        }

        #endregion

        #region IsOpen

        private static readonly DependencyProperty IsOpenProperty =
            DependencyProperty.Register(
                nameof(IsOpen),
                typeof(bool),
                typeof(PopupPositioner),
                new PropertyMetadata(false, OnIsOpenChanged));

        private static void OnIsOpenChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((PopupPositioner)d).OnIsOpenChanged(e);
        }

        private void OnIsOpenChanged(DependencyPropertyChangedEventArgs e)
        {
            if (_popup is null)
            {
                return;
            }

            if ((bool)e.NewValue)
            {
                DependencyPropertyDescriptor.FromProperty(Popup.ChildProperty, typeof(Popup)).AddValueChanged(_popup, OnPopupPropertyChanged);
                DependencyPropertyDescriptor.FromProperty(Popup.PlacementProperty, typeof(Popup)).AddValueChanged(_popup, OnPopupPropertyChanged);
                DependencyPropertyDescriptor.FromProperty(Popup.HorizontalOffsetProperty, typeof(Popup)).AddValueChanged(_popup, OnPopupPropertyChanged);
                DependencyPropertyDescriptor.FromProperty(Popup.VerticalOffsetProperty, typeof(Popup)).AddValueChanged(_popup, OnPopupPropertyChanged);
                DependencyPropertyDescriptor.FromProperty(Popup.PlacementRectangleProperty, typeof(Popup)).AddValueChanged(_popup, OnPopupPropertyChanged);
            }
            else
            {
                DependencyPropertyDescriptor.FromProperty(Popup.ChildProperty, typeof(Popup)).RemoveValueChanged(_popup, OnPopupPropertyChanged);
                DependencyPropertyDescriptor.FromProperty(Popup.PlacementProperty, typeof(Popup)).RemoveValueChanged(_popup, OnPopupPropertyChanged);
                DependencyPropertyDescriptor.FromProperty(Popup.HorizontalOffsetProperty, typeof(Popup)).RemoveValueChanged(_popup, OnPopupPropertyChanged);
                DependencyPropertyDescriptor.FromProperty(Popup.VerticalOffsetProperty, typeof(Popup)).RemoveValueChanged(_popup, OnPopupPropertyChanged);
                DependencyPropertyDescriptor.FromProperty(Popup.PlacementRectangleProperty, typeof(Popup)).RemoveValueChanged(_popup, OnPopupPropertyChanged);

                if (_window != null)
                {
                    _window.AutoResized -= OnWindowResize;
                    _window = null;
                }

                _positionInfo = null;
            }
        }

        #endregion

        private void OnPopupOpened(object sender, EventArgs e)
        {
            if (_window == null && _popup.Child is { } child)
            {
                _window = PresentationSource.FromVisual(child) as HwndSource;
                if (_window != null)
                {
                    _window.AutoResized += OnWindowResize;
                }
            }

            Reposition();
        }

        private void OnPopupPropertyChanged(object sender, EventArgs e)
        {
            Reposition();
        }

        private readonly Popup _popup;
        private PopupSecurityHelper _secHelper;
        private PositionInfo _positionInfo;
        private HwndSource _window;
        private bool _isDisposed;

        private class PositionInfo
        {
            static PositionInfo()
            {
                IsSupported = Fields.X != null &&
                              Fields.Y != null &&
                              Fields.ChildSize != null;
            }

            public PositionInfo(object obj)
            {
                _obj = obj;
            }

            public static bool IsSupported { get; }

            public int X
            {
                get => (int)Fields.X.GetValue(_obj);
                set => Fields.X.SetValue(_obj, value);
            }

            public int Y
            {
                get => (int)Fields.Y.GetValue(_obj);
                set => Fields.Y.SetValue(_obj, value);
            }

            public Size ChildSize
            {
                get => (Size)Fields.ChildSize.GetValue(_obj);
                set => Fields.ChildSize.SetValue(_obj, value);
            }

            private readonly object _obj;

            private static class Fields
            {
                static Fields()
                {
                    try
                    {
                        var type = typeof(Popup).Assembly.GetType("System.Windows.Controls.Primitives.Popup+PositionInfo");
                        if (type != null)
                        {
                            X = type.GetField(nameof(X));
                            Y = type.GetField(nameof(Y));
                            ChildSize = type.GetField(nameof(ChildSize));
                        }
                    }
                    catch { }
                }

                public static FieldInfo X { get; }
                public static FieldInfo Y { get; }
                public static FieldInfo ChildSize { get; }
            }
        }

        private class PopupSecurityHelper
        {
            static PopupSecurityHelper()
            {
                IsSupported = Methods.IsWindowAlive != null &&
                              Methods.SetPopupPos != null;
            }

            public PopupSecurityHelper(object obj)
            {
                _isWindowAlive = DelegateHelper.CreateDelegate<Func<bool>>(obj, Methods.IsWindowAlive);
                _setPopupPos = DelegateHelper.CreateDelegate<Action<bool, int, int, bool, int, int>>(obj, Methods.SetPopupPos);
            }

            public static bool IsSupported { get; }

            internal bool IsWindowAlive()
            {
                return _isWindowAlive();
            }

            internal void SetPopupPos(bool position, int x, int y, bool size, int width, int height)
            {
                _setPopupPos(position, x, y, size, width, height);
            }

            private Func<bool> _isWindowAlive;
            private Action<bool, int, int, bool, int, int> _setPopupPos;

            private static class Methods
            {
                static Methods()
                {
                    try
                    {
                        var type = typeof(Popup).Assembly.GetType("System.Windows.Controls.Primitives.Popup+PopupSecurityHelper");
                        IsWindowAlive = type.GetMethod(nameof(IsWindowAlive), BindingFlags.Instance | BindingFlags.NonPublic);
                        SetPopupPos = type.GetMethod(nameof(SetPopupPos), BindingFlags.Instance | BindingFlags.NonPublic);
                    }
                    catch { }
                }

                public static MethodInfo IsWindowAlive { get; }
                public static MethodInfo SetPopupPos { get; }
            }
        }

        private static class Delegates
        {
            static Delegates()
            {
                try
                {
                    GetPlacementInternal = DelegateHelper.CreatePropertyGetter<Popup, PlacementMode>(nameof(PlacementInternal), true);

                    GetDropOpposite = DelegateHelper.CreatePropertyGetter<Popup, bool>(nameof(DropOpposite), true);
                    SetDropOpposite = DelegateHelper.CreatePropertySetter<Popup, bool>(nameof(DropOpposite), true);

                    GetAnimateFromRight = DelegateHelper.CreatePropertyGetter<Popup, bool>(nameof(AnimateFromRight), true);
                    SetAnimateFromRight = DelegateHelper.CreatePropertySetter<Popup, bool>(nameof(AnimateFromRight), true);

                    GetAnimateFromBottom = DelegateHelper.CreatePropertyGetter<Popup, bool>(nameof(AnimateFromBottom), true);
                    SetAnimateFromBottom = DelegateHelper.CreatePropertySetter<Popup, bool>(nameof(AnimateFromBottom), true);

                    GetPlacementTargetInterestPoints = DelegateHelper.CreateDelegate<Func<Popup, PlacementMode, Point[]>>(
                        typeof(Popup), nameof(GetPlacementTargetInterestPoints), true);

                    GetChildInterestPoints = DelegateHelper.CreateDelegate<Func<Popup, PlacementMode, Point[]>>(
                        typeof(Popup), nameof(GetChildInterestPoints), true);

                    GetScreenBounds = DelegateHelper.CreateDelegate<Func<Popup, Rect, Point, Rect>>(
                        typeof(Popup), nameof(GetScreenBounds), true);
                }
                catch { }
            }

            public static Func<Popup, PlacementMode> GetPlacementInternal { get; }

            public static Func<Popup, bool> GetDropOpposite { get; }
            public static Action<Popup, bool> SetDropOpposite { get; }

            public static Func<Popup, bool> GetAnimateFromRight { get; }
            public static Action<Popup, bool> SetAnimateFromRight { get; }

            public static Func<Popup, bool> GetAnimateFromBottom { get; }
            public static Action<Popup, bool> SetAnimateFromBottom { get; }

            public static Func<Popup, PlacementMode, Point[]> GetPlacementTargetInterestPoints { get; }
            public static Func<Popup, PlacementMode, Point[]> GetChildInterestPoints { get; }
            public static Func<Popup, Rect, Point, Rect> GetScreenBounds { get; }
        }

        private static class Fields
        {
            static Fields()
            {
                try
                {
                    _secHelper = typeof(Popup).GetField(nameof(_secHelper), BindingFlags.Instance | BindingFlags.NonPublic);
                    _positionInfo = typeof(Popup).GetField(nameof(_positionInfo), BindingFlags.Instance | BindingFlags.NonPublic);
                }
                catch { }
            }

            public static FieldInfo _secHelper { get; }

            public static FieldInfo _positionInfo { get; }
        }
    }
}
