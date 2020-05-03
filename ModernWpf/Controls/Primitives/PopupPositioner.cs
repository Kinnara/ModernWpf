// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using MS.Internal;
using MS.Win32;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Threading;

namespace ModernWpf.Controls.Primitives
{
    internal class PopupPositioner : DependencyObject, IDisposable
    {
        static PopupPositioner()
        {
            IsSupported = Delegates.GetPlacementInternal != null &&
                          Delegates.GetDropOpposite != null &&
                          Delegates.GetPlacementTargetInterestPoints != null &&
                          Delegates.GetChildInterestPoints != null &&
                          Delegates.GetScreenBounds != null;
        }

        public PopupPositioner(Popup popup)
        {
            if (!IsSupported)
            {
                throw new NotSupportedException();
            }

            _popup = popup;
            _secHelper = new PopupSecurityHelper();

            SetPositioner(popup, this);

            popup.Opened += OnPopupOpened;
            popup.Closed += OnPopupClosed;

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

            _isDisposed = true;

            if (_popup != null)
            {
                _popup.Opened -= OnPopupOpened;
                _popup.Closed -= OnPopupClosed;
                _popup.ClearValue(PositionerProperty);
            }

            OnPopupClosed(null, null);
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

        internal bool DropOpposite => Delegates.GetDropOpposite(_popup);

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

        private class PositionInfo
        {
            // The position of the upper left corner of the popup after nudging
            public int X;
            public int Y;

            // The size of the popup
            public Size ChildSize;
        }

        // To position the popup, we find the InterestPoints of the placement rectangle/point
        // in the screen coordinate space.  We also find the InterestPoints of the child in
        // the popup's space.  Then we attempt all valid combinations of matching InterestPoints
        // (based on PlacementMode) to find the position that best fits on the screen.
        // NOTE: any reference to the screen implies the monitor for full trust and
        //       the browser area for partial trust
        private void UpdatePosition()
        {
            if (_popupRoot == null)
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

            Rect windowRect = _secHelper.GetWindowRect();
            _positionInfo ??= new PositionInfo();
            _positionInfo.X = (int)windowRect.X;
            _positionInfo.Y = (int)windowRect.Y;
            _positionInfo.ChildSize = windowRect.Size;

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

                    // Stop when we find a popup that is completely on screen
                    if (Math.Abs(score - childArea) < Tolerance)
                    {
                        break;
                    }
                }
            }

            // Check to see if the pop needs to be nudged onto the screen.
            // Popups are not nudged if their axes do not align with the screen axes

            // Use the size of the popupRoot in case it is clipping the popup content
            Matrix transformToDevice = _secHelper.GetTransformToDevice();
            childBounds = new Rect((Size)transformToDevice.Transform((Point)GetChildSize()));

            childBounds.Offset(bestTranslation);

            Vector childTranslation = (Vector)transformToDevice.Transform(GetChildTranslation());
            childBounds.Offset(childTranslation);

            screenBounds = GetScreenBounds(targetBounds, placementTargetInterestPoints[(int)InterestPoint.TopLeft]);
            Rect intersection = Rect.Intersect(screenBounds, childBounds);

            // See if width/height of intersection are less than child's
            if (Math.Abs(intersection.Width - childBounds.Width) > Tolerance ||
                Math.Abs(intersection.Height - childBounds.Height) > Tolerance)
            {
                // Nudge Horizontally
                Point topLeft = placementTargetInterestPoints[(int)InterestPoint.TopLeft];
                Point topRight = placementTargetInterestPoints[(int)InterestPoint.TopRight];

                // Create a vector pointing from the top of the placement target to the bottom
                // to determine which direction the popup should be nudged in.
                // If the vector is zero (NaN's after normalization), nudge horizontally
                Vector horizontalAxis = topRight - topLeft;
                horizontalAxis.Normalize();

                // See if target's horizontal axis is aligned with screen
                // (For opaque windows always translate horizontally)
                if (!IsTransparent || double.IsNaN(horizontalAxis.Y) || Math.Abs(horizontalAxis.Y) < Tolerance)
                {
                    // Nudge horizontally
                    if (childBounds.Right > screenBounds.Right)
                    {
                        bestTranslation.X = screenBounds.Right - childBounds.Width;
                        bestTranslation.X -= childTranslation.X;
                    }
                    else if (childBounds.Left < screenBounds.Left)
                    {
                        bestTranslation.X = screenBounds.Left;
                        bestTranslation.X -= childTranslation.X;
                    }
                }
                else if (IsTransparent && Math.Abs(horizontalAxis.X) < Tolerance)
                {
                    // Nudge vertically, limit horizontally
                    if (childBounds.Bottom > screenBounds.Bottom)
                    {
                        bestTranslation.Y = screenBounds.Bottom - childBounds.Height;
                        bestTranslation.Y -= childTranslation.Y;
                    }
                    else if (childBounds.Top < screenBounds.Top)
                    {
                        bestTranslation.Y = screenBounds.Top;
                        bestTranslation.Y -= childTranslation.Y;
                    }
                }

                // Nudge Vertically
                Point bottomLeft = placementTargetInterestPoints[(int)InterestPoint.BottomLeft];

                // Create a vector pointing from the top of the placement target to the bottom
                // to determine which direction the popup should be nudged in
                // If the vector is zero (NaN's after normalization), nudge vertically
                Vector verticalAxis = topLeft - bottomLeft;
                verticalAxis.Normalize();

                // Axis is aligned with screen, nudge
                if (!IsTransparent || double.IsNaN(verticalAxis.X) || Math.Abs(verticalAxis.X) < Tolerance)
                {
                    if (childBounds.Bottom > screenBounds.Bottom)
                    {
                        bestTranslation.Y = screenBounds.Bottom - childBounds.Height;
                        bestTranslation.Y -= childTranslation.Y;
                    }
                    else if (childBounds.Top < screenBounds.Top)
                    {
                        bestTranslation.Y = screenBounds.Top;
                        bestTranslation.Y -= childTranslation.Y;
                    }
                }
                else if (IsTransparent && Math.Abs(verticalAxis.Y) < Tolerance)
                {
                    if (childBounds.Right > screenBounds.Right)
                    {
                        bestTranslation.X = screenBounds.Right - childBounds.Width;
                        bestTranslation.X -= childTranslation.X;
                    }
                    else if (childBounds.Left < screenBounds.Left)
                    {
                        bestTranslation.X = screenBounds.Left;
                        bestTranslation.X -= childTranslation.X;
                    }
                }
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

            Size GetChildSize()
            {
                if (_popup.Child is { } child)
                {
                    return child.RenderSize;
                }
                return _popupRoot.RenderSize;
            }

            Point GetChildTranslation()
            {
                if (_popup.Child is { } child)
                {
                    return child.TranslatePoint(new Point(), _popupRoot);
                }
                return new Point();
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

        private bool IsTransparent => _popup.AllowsTransparency;

        internal const double Tolerance = 1.0e-2; // allow errors in double calculations

        private PositionInfo _positionInfo;

        private FrameworkElement _popupRoot;

        private PopupSecurityHelper _secHelper;

        private class PopupSecurityHelper
        {
            internal PopupSecurityHelper()
            {
            }

            internal bool AttachedToWindow => _window != null;

            internal void AttachToWindow(HwndSource window, AutoResizedEventHandler handler)
            {
                if (_window == null)
                {
                    _window = window;

                    window.AutoResized += handler;
                }
                else
                {
                    Debug.Assert(_window == window);
                }
            }

            internal void DetachFromWindow(AutoResizedEventHandler onAutoResizedEventHandler)
            {
                if (_window != null)
                {
                    HwndSource hwnd = _window;

                    _window = null;

                    hwnd.AutoResized -= onAutoResizedEventHandler;
                }
            }

            internal bool IsWindowAlive()
            {
                if (_window != null)
                {
                    HwndSource hwnd = _window;
                    return (hwnd != null) && !hwnd.IsDisposed;
                }

                return false;
            }

            internal void SetPopupPos(bool position, int x, int y, bool size, int width, int height)
            {
                int flags = NativeMethods.SWP_NOZORDER | NativeMethods.SWP_NOACTIVATE;
                if (!position)
                {
                    flags |= NativeMethods.SWP_NOMOVE;
                }
                if (!size)
                {
                    flags |= NativeMethods.SWP_NOSIZE;
                }

                UnsafeNativeMethods.SetWindowPos(new HandleRef(null, Handle), new HandleRef(null, IntPtr.Zero),
                    x, y, width, height, flags);
            }

            internal Rect GetWindowRect()
            {
                NativeMethods.RECT rect = new NativeMethods.RECT(0, 0, 0, 0);

                if (IsWindowAlive())
                {
                    SafeNativeMethods.GetWindowRect(_window.CreateHandleRef(), ref rect);
                }

                return PointUtil.ToRect(rect);
            }

            internal Matrix GetTransformToDevice()
            {
                CompositionTarget ct = _window.CompositionTarget;
                if (ct != null)
                {
                    try
                    {
                        return ct.TransformToDevice;
                    }
                    catch (ObjectDisposedException)
                    {
                    }
                }

                return Matrix.Identity;
            }

            internal Matrix GetTransformFromDevice()
            {
                CompositionTarget ct = _window.CompositionTarget;
                if (ct != null)
                {
                    try
                    {
                        return ct.TransformFromDevice;
                    }
                    catch (ObjectDisposedException)
                    {
                    }
                }

                return Matrix.Identity;
            }

            private static IntPtr GetHandle(HwndSource hwnd)
            {
                // add hook to the popup's window
                return (hwnd != null ? hwnd.Handle : IntPtr.Zero);
            }

            private IntPtr Handle
            {
                get
                {
                    return (GetHandle(_window));
                }
            }

            private HwndSource _window;
        }

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

        private void OnPopupOpened(object sender, EventArgs e)
        {
            if (!_secHelper.AttachedToWindow &&
                _popup.Child is { } child &&
                PresentationSource.FromVisual(child) is HwndSource window)
            {
                _secHelper.AttachToWindow(window, OnWindowResize);
                _popupRoot = window.RootVisual as FrameworkElement;
                Debug.Assert(_popupRoot != null && _popupRoot.GetType().Name == "PopupRoot");

                DependencyPropertyDescriptor.FromProperty(Popup.ChildProperty, typeof(Popup)).AddValueChanged(_popup, OnPopupPropertyChanged);
                DependencyPropertyDescriptor.FromProperty(Popup.PlacementProperty, typeof(Popup)).AddValueChanged(_popup, OnPopupPropertyChanged);
                DependencyPropertyDescriptor.FromProperty(Popup.HorizontalOffsetProperty, typeof(Popup)).AddValueChanged(_popup, OnPopupPropertyChanged);
                DependencyPropertyDescriptor.FromProperty(Popup.VerticalOffsetProperty, typeof(Popup)).AddValueChanged(_popup, OnPopupPropertyChanged);
                DependencyPropertyDescriptor.FromProperty(Popup.PlacementRectangleProperty, typeof(Popup)).AddValueChanged(_popup, OnPopupPropertyChanged);

                Reposition();
            }
        }

        private void OnPopupClosed(object sender, EventArgs e)
        {
            if (_secHelper.AttachedToWindow)
            {
                DependencyPropertyDescriptor.FromProperty(Popup.ChildProperty, typeof(Popup)).RemoveValueChanged(_popup, OnPopupPropertyChanged);
                DependencyPropertyDescriptor.FromProperty(Popup.PlacementProperty, typeof(Popup)).RemoveValueChanged(_popup, OnPopupPropertyChanged);
                DependencyPropertyDescriptor.FromProperty(Popup.HorizontalOffsetProperty, typeof(Popup)).RemoveValueChanged(_popup, OnPopupPropertyChanged);
                DependencyPropertyDescriptor.FromProperty(Popup.VerticalOffsetProperty, typeof(Popup)).RemoveValueChanged(_popup, OnPopupPropertyChanged);
                DependencyPropertyDescriptor.FromProperty(Popup.PlacementRectangleProperty, typeof(Popup)).RemoveValueChanged(_popup, OnPopupPropertyChanged);

                _secHelper.DetachFromWindow(OnWindowResize);
                _popupRoot = null;
                _positionInfo = null;
            }
        }

        private void OnPopupPropertyChanged(object sender, EventArgs e)
        {
            Reposition();
        }

        private readonly Popup _popup;
        private bool _isDisposed;

        private static class Delegates
        {
            static Delegates()
            {
                try
                {
                    GetPlacementInternal = DelegateHelper.CreatePropertyGetter<Popup, PlacementMode>(
                        nameof(PlacementInternal),
                        BindingFlags.Instance | BindingFlags.NonPublic,
                        true);

                    GetDropOpposite = DelegateHelper.CreatePropertyGetter<Popup, bool>(
                        nameof(DropOpposite),
                        BindingFlags.Instance | BindingFlags.NonPublic,
                        true);

                    GetPlacementTargetInterestPoints = DelegateHelper.CreateDelegate<Func<Popup, PlacementMode, Point[]>>(
                        typeof(Popup),
                        nameof(GetPlacementTargetInterestPoints),
                        BindingFlags.Instance | BindingFlags.NonPublic);

                    GetChildInterestPoints = DelegateHelper.CreateDelegate<Func<Popup, PlacementMode, Point[]>>(
                        typeof(Popup),
                        nameof(GetChildInterestPoints),
                        BindingFlags.Instance | BindingFlags.NonPublic);

                    GetScreenBounds = DelegateHelper.CreateDelegate<Func<Popup, Rect, Point, Rect>>(
                        typeof(Popup),
                        nameof(GetScreenBounds),
                        BindingFlags.Instance | BindingFlags.NonPublic);
                }
                catch { }
            }

            public static Func<Popup, PlacementMode> GetPlacementInternal { get; }

            public static Func<Popup, bool> GetDropOpposite { get; }

            public static Func<Popup, PlacementMode, Point[]> GetPlacementTargetInterestPoints { get; }
            public static Func<Popup, PlacementMode, Point[]> GetChildInterestPoints { get; }
            public static Func<Popup, Rect, Point, Rect> GetScreenBounds { get; }
        }
    }
}
