using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Threading;

namespace ModernWpf.Controls.Primitives
{
    public abstract partial class FlyoutBase : DependencyObject
    {
        protected FlyoutBase()
        {
        }

        #region AreOpenCloseAnimationsEnabled

        private static void OnAreOpenCloseAnimationsEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((FlyoutBase)d).OnAreOpenCloseAnimationsEnabledChanged(e);
        }

        internal virtual void OnAreOpenCloseAnimationsEnabledChanged(DependencyPropertyChangedEventArgs e)
        {
            UpdatePopupAnimation();
        }

        #endregion

        public bool IsConstrainedToRootBounds => ShouldConstrainToRootBounds;

        #region IsOpen

        private static void OnIsOpenChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((FlyoutBase)d).OnIsOpenChanged();
        }

        internal virtual void OnIsOpenChanged()
        {
            if (IsOpen)
            {
                if (m_shouldTakeFocus)
                {
                    if (Keyboard.FocusedElement != null)
                    {
                        m_weakRefToPreviousFocus = new WeakReference<IInputElement>(Keyboard.FocusedElement);
                    }

                    FocusTarget?.Focus();
                }

                UpdatePointerMoveAwayTracking();
            }
            else
            {
                RemoveRootPointerMovedHandler();

                if (m_weakRefToPreviousFocus != null)
                {
                    if (m_weakRefToPreviousFocus.TryGetTarget(out IInputElement previousFocus))
                    {
                        previousFocus.Focus();
                    }

                    m_weakRefToPreviousFocus = null;
                }
            }
        }

        internal virtual void UpdateIsOpen()
        {
            IsOpen = m_popup != null && m_popup.IsOpen;
        }

        #endregion

        #region ShowMode

        private static void OnShowModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((FlyoutBase)d).UpdateStateToShowMode((FlyoutShowMode)e.NewValue);
        }

        #endregion

        #region AttachedFlyout

        public static FlyoutBase GetAttachedFlyout(FrameworkElement element)
        {
            return (FlyoutBase)element.GetValue(AttachedFlyoutProperty);
        }

        public static void SetAttachedFlyout(FrameworkElement element, FlyoutBase value)
        {
            element.SetValue(AttachedFlyoutProperty, value);
        }

        public static void ShowAttachedFlyout(FrameworkElement flyoutOwner)
        {
            var flyout = GetAttachedFlyout(flyoutOwner);
            if (flyout != null)
            {
                flyout.ShowAt(flyoutOwner);
            }
        }

        #endregion

        internal virtual PopupAnimation DesiredPopupAnimation => PopupAnimation.Fade;

        internal PopupEx InternalPopup => m_popup;

        internal double Offset { get; set; } = s_offset;

        public event EventHandler<object> Opening;
        public event EventHandler<object> Opened;
        public event EventHandler<object> Closed;
        public event TypedEventHandler<FlyoutBase, FlyoutBaseClosingEventArgs> Closing;

        public void ShowAt(FrameworkElement placementTarget)
        {
            if (placementTarget is null)
            {
                throw new ArgumentNullException(nameof(placementTarget));
            }

            ShowAtCore(placementTarget, false);
        }

        public void ShowAt(FrameworkElement placementTarget, FlyoutShowOptions showOptions)
        {
            if (placementTarget is null)
            {
                placementTarget = GetRootPlacementTargetForPosition(showOptions);
            }

            ShowAtCore(placementTarget, false, showOptions);
        }

        public void Hide()
        {
            CancelAsyncShow();
            HideCore();
        }

        protected abstract Control CreatePresenter();

        protected virtual Control FocusTarget => m_presenter;

        protected virtual FrameworkElement PointerMoveAwayBoundsElement => m_presenter;

        protected virtual bool ShouldRecreatePresenterAfterClose => false;

        protected virtual void OnPresenterReleased()
        {
        }

        internal void ShowAsContextFlyout(FrameworkElement placementTarget)
        {
            if (placementTarget is null)
            {
                throw new ArgumentNullException(nameof(placementTarget));
            }

            ShowAtCore(placementTarget, true);
        }

        internal virtual void ShowAtCore(FrameworkElement placementTarget, bool showAsContextFlyout, FlyoutShowOptions showOptions = null)
        {
            showOptions = CloneShowOptions(showOptions);
            CancelAsyncShow();
            ApplyShowOptions(showOptions, showAsContextFlyout);

            if (m_popup != null &&
                m_popup.IsOpen &&
                Target == placementTarget &&
                m_showingAsContextFlyout == showAsContextFlyout &&
                IsSameTargetPosition(showOptions, showAsContextFlyout))
            {
                return;
            }

            if (m_closing)
            {
                m_pendingShow = () => ShowAtCore(placementTarget, showAsContextFlyout, showOptions);
                return;
            }

            if (TryStageLatestShowUntilOpenFlyoutCloses(placementTarget, showAsContextFlyout, showOptions))
            {
                return;
            }

            PreparePopup(placementTarget, showAsContextFlyout);
            Debug.Assert(m_popup.HasLocalValue(Popup.PlacementProperty));
            Debug.Assert(m_popup.HasLocalValue(Popup.PlacementTargetProperty));

            Target = placementTarget;
            m_showingAsContextFlyout = showAsContextFlyout;
            TrackPlacementTarget(placementTarget);
            OnOpening();
            SetOpenFlyout(this);
            m_popup.IsOpen = true;
        }

        internal virtual void HideCore()
        {
            if (m_popup != null && m_popup.IsOpen)
            {
                m_popup.IsOpen = false;
            }
        }

        internal virtual void OnOpening()
        {
            Opening?.Invoke(this, null);
        }

        internal virtual void OnOpened()
        {
            Opened?.Invoke(this, null);
        }

        internal virtual bool OnClosing()
        {
            var args = new FlyoutBaseClosingEventArgs();
            Closing?.Invoke(this, args);
            return args.Cancel;
        }

        internal virtual void OnClosed()
        {
            m_isTargetPositionSet = false;
            m_exclusionRect = null;
            m_hasPlacementOverride = false;

            ClearOpenFlyout(this);
            Closed?.Invoke(this, null);

            var pendingShow = m_pendingShow;
            CancelAsyncShow();
            if (pendingShow != null)
            {
                m_asyncShow = Dispatcher.BeginInvoke(pendingShow);
            }
            else if (TryTakePendingFlyoutShow(this, out var pendingFlyoutShow))
            {
                m_asyncShow = Dispatcher.BeginInvoke(new Action(pendingFlyoutShow.Show));
            }
        }

        internal void BindPlacement(Control presenter)
        {
            presenter.SetBinding(
                CustomPopupPlacementHelper.PlacementProperty,
                new Binding
                {
                    Path = new PropertyPath(PlacementProperty),
                    Source = this,
                    Converter = s_placementConverter
                });
        }

        private void EnsurePresenter()
        {
            if (m_presenter == null)
            {
                m_presenter = CreatePresenter();
                BindPlacement(m_presenter);
            }
        }

        private void EnsurePopup()
        {
            if (m_popup == null)
            {
                EnsurePresenter();

                m_popup = new PopupEx
                {
                    Child = m_presenter,
                    StaysOpen = false,
                    AllowsTransparency = true,
                    CustomPopupPlacementCallback = PositionPopup
                };
                m_popup.Opened += OnPopupOpened;
                m_popup.Closing += OnPopupClosing;
                m_popup.Closed += OnPopupClosed;
                m_popup.IsOpenChanged += OnPopupIsOpenChanged;
            }
        }

        private void PreparePopup(FrameworkElement placementTarget, bool showAsContextFlyout)
        {
            EnsurePopup();

            if (m_popup.IsOpen)
            {
                m_popup.IsOpen = false;
            }

            UpdatePopupAnimation();

            var effectivePlacement = GetEffectivePlacement();
            m_presenter.SetCurrentValue(CustomPopupPlacementHelper.PlacementProperty, (CustomPlacementMode)effectivePlacement);

            if (showAsContextFlyout)
            {
                m_absolutePopupPlacementPoint = null;
                m_presenter.ClearValue(FrameworkElement.WidthProperty);
                m_presenter.ClearValue(FrameworkElement.HeightProperty);
                m_popup.Placement = PlacementMode.MousePoint;
                m_popup.PlacementTarget = placementTarget;
                m_popup.ClearValue(Popup.PlacementRectangleProperty);
            }
            else if (!m_isTargetPositionSet &&
                effectivePlacement == FlyoutPlacementMode.Full &&
                Window.GetWindow(placementTarget) is Window window)
            {
                var adornerDecorator = window.FindDescendant<AdornerDecorator>();
                if (adornerDecorator != null)
                {
                    placementTarget = adornerDecorator;

                    m_presenter.SetBinding(
                        FrameworkElement.WidthProperty,
                        new Binding
                        {
                            Path = new PropertyPath(FrameworkElement.ActualWidthProperty),
                            Source = adornerDecorator
                        });

                    m_presenter.SetBinding(
                        FrameworkElement.HeightProperty,
                        new Binding
                        {
                            Path = new PropertyPath(FrameworkElement.ActualHeightProperty),
                            Source = adornerDecorator
                        });
                }
                else
                {
                    placementTarget = window;

                    m_presenter.SetBinding(
                        FrameworkElement.WidthProperty,
                        new MultiBinding
                        {
                            Converter = s_fullPlacementWidthConverter,
                            Bindings =
                            {
                                new Binding { Path = new PropertyPath(FrameworkElement.ActualWidthProperty), Source = window },
                                new Binding { Path = new PropertyPath(Control.BorderThicknessProperty), Source = window },
                            }
                        });

                    m_presenter.SetBinding(
                        FrameworkElement.HeightProperty,
                        new MultiBinding
                        {
                            Converter = s_fullPlacementHeightConverter,
                            Bindings =
                            {
                                new Binding { Path = new PropertyPath(FrameworkElement.ActualHeightProperty), Source = window },
                                new Binding { Path = new PropertyPath(Control.BorderThicknessProperty), Source = window },
                            }
                        });
                }

                m_popup.Placement = PlacementMode.Center;
                m_popup.PlacementTarget = placementTarget;
                m_popup.ClearValue(Popup.PlacementRectangleProperty);
                m_absolutePopupPlacementPoint = null;
            }
            else
            {
                m_presenter.ClearValue(FrameworkElement.WidthProperty);
                m_presenter.ClearValue(FrameworkElement.HeightProperty);
                m_popup.Placement = PlacementMode.Custom;
                m_popup.PlacementTarget = placementTarget;
                m_popup.PlacementRectangle = GetPlacementRectangle(placementTarget, effectivePlacement);
                m_absolutePopupPlacementPoint = TryGetAbsolutePlacementPoint(placementTarget, effectivePlacement, out var absolutePlacementPoint)
                    ? absolutePlacementPoint
                    : null;
            }
        }

        private void UpdatePopupAnimation()
        {
            if (m_popup != null)
            {
                m_popup.PopupAnimation = AreOpenCloseAnimationsEnabled && SharedHelpers.IsAnimationsEnabled ?
                    DesiredPopupAnimation : PopupAnimation.None;
            }
        }

        internal Rect GetPlacementRectangle(UIElement target)
        {
            return GetPlacementRectangle(target, GetEffectivePlacement());
        }

        internal Rect GetPlacementRectangle(UIElement target, FlyoutPlacementMode placement)
        {
            Rect value = Rect.Empty;

            if (target != null)
            {
                Size targetSize = target.RenderSize;

                if (m_isTargetPositionSet)
                {
                    return new Rect(m_targetPosition, new Size());
                }

                switch (placement)
                {
                    case FlyoutPlacementMode.Top:
                    case FlyoutPlacementMode.Bottom:
                    case FlyoutPlacementMode.TopEdgeAlignedLeft:
                    case FlyoutPlacementMode.TopEdgeAlignedRight:
                    case FlyoutPlacementMode.BottomEdgeAlignedLeft:
                    case FlyoutPlacementMode.BottomEdgeAlignedRight:
                        value = new Rect(
                            new Point(0, -Offset),
                            new Point(targetSize.Width, targetSize.Height + Offset));
                        break;
                    case FlyoutPlacementMode.Left:
                    case FlyoutPlacementMode.Right:
                    case FlyoutPlacementMode.LeftEdgeAlignedTop:
                    case FlyoutPlacementMode.LeftEdgeAlignedBottom:
                    case FlyoutPlacementMode.RightEdgeAlignedTop:
                    case FlyoutPlacementMode.RightEdgeAlignedBottom:
                        value = new Rect(
                            new Point(-Offset, 0),
                            new Point(targetSize.Width + Offset, targetSize.Height));
                        break;
                }
            }

            return value;
        }

        private void OnPopupOpened(object sender, EventArgs e)
        {
            MovePopupWindowToAbsolutePlacementPoint();
            Dispatcher.BeginInvoke(new Action(MovePopupWindowToAbsolutePlacementPoint), DispatcherPriority.Loaded);
            Dispatcher.BeginInvoke(new Action(MovePopupWindowToAbsolutePlacementPoint), DispatcherPriority.ApplicationIdle);
            UpdatePointerMoveAwayTracking();

            if (m_suppressNextOpened)
            {
                m_suppressNextOpened = false;
                return;
            }

            OnOpened();
        }

        private void OnPopupClosing(object sender, CancelEventArgs e)
        {
            e.Cancel = OnClosing();
            if (e.Cancel)
            {
                m_suppressNextOpened = true;
            }
            else
            {
                m_closing = true;
            }
        }

        private void OnPopupClosed(object sender, EventArgs e)
        {
            m_closing = false;

            if (m_popup.IsOpen)
            {
                return;
            }

            m_suppressNextOpened = false;
            m_popup.ClearValue(Popup.PlacementProperty);
            m_popup.ClearValue(Popup.PlacementTargetProperty);
            m_popup.ClearValue(Popup.PlacementRectangleProperty);
            m_popup.ClearValue(FrameworkElement.WidthProperty);
            m_popup.ClearValue(FrameworkElement.HeightProperty);
            m_absolutePopupPlacementPoint = null;
            ClearPlacementTargetTracking();
            Target = null;
            m_showingAsContextFlyout = false;
            ClearOpenFlyout(this);
            RemoveRootPointerMovedHandler();

            OnClosed();

            if (ShouldRecreatePresenterAfterClose)
            {
                ReleasePopupAndPresenter();
            }
        }

        private void OnPopupIsOpenChanged(object sender, EventArgs e)
        {
            UpdateIsOpen();
        }

        private void ReleasePopupAndPresenter()
        {
            if (m_popup != null)
            {
                m_popup.Opened -= OnPopupOpened;
                m_popup.Closing -= OnPopupClosing;
                m_popup.Closed -= OnPopupClosed;
                m_popup.IsOpenChanged -= OnPopupIsOpenChanged;
                m_popup.CustomPopupPlacementCallback = null;
                m_popup.Child = null;
                m_popup = null;
            }

            m_presenter = null;
            OnPresenterReleased();
        }

        private CustomPopupPlacement[] PositionPopup(Size popupSize, Size targetSize, Point offset)
        {
            return PositionPopup(popupSize, targetSize, offset, m_presenter);
        }

        internal CustomPopupPlacement[] PositionPopup(Size popupSize, Size targetSize, Point offset, FrameworkElement child)
        {
            return CustomPopupPlacementHelper.PositionPopup(
                (CustomPlacementMode)GetEffectivePlacement(),
                popupSize,
                targetSize,
                offset,
                child,
                GetTargetPositionRelativeExclusionRect());
        }

        private bool TryGetAbsolutePlacementPoint(
            FrameworkElement placementTarget,
            FlyoutPlacementMode effectivePlacement,
            out Point point)
        {
            point = default;

            var placementRect = GetPlacementRectangle(placementTarget, effectivePlacement);
            if (placementRect.IsEmpty)
            {
                return false;
            }

            var topLeft = placementTarget.PointToScreen(placementRect.TopLeft);
            var bottomRight = placementTarget.PointToScreen(placementRect.BottomRight);
            var targetRect = new Rect(topLeft, bottomRight);
            var popupSize = GetPresenterDesiredScreenSize(placementTarget);

            switch (effectivePlacement)
            {
                case FlyoutPlacementMode.Top:
                case FlyoutPlacementMode.TopEdgeAlignedLeft:
                    point = new Point(targetRect.Left, targetRect.Top - popupSize.Height);
                    return true;
                case FlyoutPlacementMode.TopEdgeAlignedRight:
                    point = new Point(targetRect.Right - popupSize.Width, targetRect.Top - popupSize.Height);
                    return true;
                case FlyoutPlacementMode.Bottom:
                case FlyoutPlacementMode.BottomEdgeAlignedLeft:
                    point = new Point(targetRect.Left, targetRect.Bottom);
                    return true;
                case FlyoutPlacementMode.BottomEdgeAlignedRight:
                    point = new Point(targetRect.Right - popupSize.Width, targetRect.Bottom);
                    return true;
                case FlyoutPlacementMode.Left:
                case FlyoutPlacementMode.LeftEdgeAlignedTop:
                    point = new Point(targetRect.Left - popupSize.Width, targetRect.Top);
                    point.Y = GetSidePlacementVerticalOffset(targetRect, popupSize.Height, point.Y);
                    return true;
                case FlyoutPlacementMode.LeftEdgeAlignedBottom:
                    point = new Point(targetRect.Left - popupSize.Width, targetRect.Bottom - popupSize.Height);
                    return true;
                case FlyoutPlacementMode.Right:
                case FlyoutPlacementMode.RightEdgeAlignedTop:
                    point = new Point(targetRect.Right, targetRect.Top);
                    point.Y = GetSidePlacementVerticalOffset(targetRect, popupSize.Height, point.Y);
                    return true;
                case FlyoutPlacementMode.RightEdgeAlignedBottom:
                    point = new Point(targetRect.Right, targetRect.Bottom - popupSize.Height);
                    return true;
                default:
                    return false;
            }
        }

        private static double GetSidePlacementVerticalOffset(Rect targetRect, double presenterHeight, double verticalOffset)
        {
            if (TryGetMonitorWorkArea(targetRect, out var workArea))
            {
                return ClampSidePlacementVerticalOffset(
                    verticalOffset,
                    targetRect.Bottom,
                    presenterHeight,
                    workArea.Top,
                    workArea.Bottom);
            }

            return verticalOffset;
        }

        internal static double ClampSidePlacementVerticalOffset(
            double verticalOffset,
            double anchorBottom,
            double presenterHeight,
            double availableTop,
            double availableBottom)
        {
            if (verticalOffset + presenterHeight <= availableBottom)
            {
                return verticalOffset;
            }

            if (verticalOffset <= availableTop)
            {
                return availableTop;
            }

            return anchorBottom - Math.Min(
                presenterHeight,
                Math.Max(0, anchorBottom - availableTop));
        }

        private static bool TryGetMonitorWorkArea(Rect targetRect, out Rect workArea)
        {
            var nativeTargetRect = new NativeRect
            {
                Left = (int)Math.Floor(targetRect.Left),
                Top = (int)Math.Floor(targetRect.Top),
                Right = (int)Math.Ceiling(targetRect.Right),
                Bottom = (int)Math.Ceiling(targetRect.Bottom)
            };
            var monitor = MonitorFromRect(ref nativeTargetRect, MONITOR_DEFAULTTONEAREST);
            if (monitor != IntPtr.Zero)
            {
                var monitorInfo = new MonitorInfo();
                if (GetMonitorInfo(monitor, monitorInfo))
                {
                    workArea = new Rect(
                        monitorInfo.Work.Left,
                        monitorInfo.Work.Top,
                        monitorInfo.Work.Right - monitorInfo.Work.Left,
                        monitorInfo.Work.Bottom - monitorInfo.Work.Top);
                    return true;
                }
            }

            workArea = Rect.Empty;
            return false;
        }

        private Size GetPresenterDesiredScreenSize(FrameworkElement placementTarget)
        {
            m_presenter.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            var desiredSize = m_presenter.DesiredSize;
            var source = PresentationSource.FromVisual(placementTarget);
            if (source?.CompositionTarget != null)
            {
                var transformed = source.CompositionTarget.TransformToDevice.Transform(
                    new Vector(desiredSize.Width, desiredSize.Height));
                return new Size(transformed.X, transformed.Y);
            }

            return desiredSize;
        }

        private void MovePopupWindowToAbsolutePlacementPoint()
        {
            if (!m_absolutePopupPlacementPoint.HasValue ||
                m_popup?.Child == null ||
                !(PresentationSource.FromVisual(m_popup.Child) is HwndSource source) ||
                source.Handle == IntPtr.Zero)
            {
                return;
            }

            var point = m_absolutePopupPlacementPoint.Value;
            SetWindowPos(
                source.Handle,
                IntPtr.Zero,
                (int)Math.Round(point.X),
                (int)Math.Round(point.Y),
                0,
                0,
                SWP_NOSIZE | SWP_NOZORDER | SWP_NOACTIVATE | SWP_SHOWWINDOW);
        }

        protected void TrackPlacementTarget(FrameworkElement placementTarget)
        {
            if (m_trackedPlacementTarget == placementTarget)
            {
                return;
            }

            ClearPlacementTargetTracking();

            m_trackedPlacementTarget = placementTarget;
            m_trackedPlacementTarget.Unloaded += OnPlacementTargetUnloaded;
        }

        protected void ClearPlacementTargetTracking()
        {
            if (m_trackedPlacementTarget != null)
            {
                m_trackedPlacementTarget.Unloaded -= OnPlacementTargetUnloaded;
                m_trackedPlacementTarget = null;
            }
        }

        private void OnPlacementTargetUnloaded(object sender, RoutedEventArgs e)
        {
            Hide();
        }

        internal void UpdateStateToShowMode(FlyoutShowMode showMode)
        {
            if (showMode == FlyoutShowMode.Auto)
            {
                SetCurrentValue(ShowModeProperty, FlyoutShowMode.Standard);
                showMode = FlyoutShowMode.Standard;
            }
            else if (ShowMode != showMode)
            {
                SetCurrentValue(ShowModeProperty, showMode);
            }

            switch (showMode)
            {
                case FlyoutShowMode.Standard:
                    m_shouldTakeFocus = true;
                    m_shouldHideIfPointerMovesAway = false;
                    break;
                case FlyoutShowMode.Transient:
                    m_shouldTakeFocus = false;
                    m_shouldHideIfPointerMovesAway = false;
                    break;
                case FlyoutShowMode.TransientWithDismissOnPointerMoveAway:
                    m_shouldTakeFocus = false;
                    m_shouldHideIfPointerMovesAway = true;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(showMode), showMode, null);
            }

            UpdatePointerMoveAwayTracking();
        }

        internal FlyoutPlacementMode GetEffectivePlacement()
        {
            var placement = m_hasPlacementOverride ? m_placementOverride : Placement;
            return placement == FlyoutPlacementMode.Auto ? FlyoutPlacementMode.Top : placement;
        }

        internal bool IsSameTargetPosition(FlyoutShowOptions showOptions, bool showAsContextFlyout)
        {
            var hasTargetPosition = !showAsContextFlyout && showOptions?.Position != null;
            if (hasTargetPosition != m_isTargetPositionSet)
            {
                return false;
            }

            if (!hasTargetPosition)
            {
                return true;
            }

            var position = showOptions.Position.Value;
            return position.X == m_targetPosition.X &&
                   position.Y == m_targetPosition.Y;
        }

        internal void ApplyShowOptions(FlyoutShowOptions showOptions, bool showAsContextFlyout)
        {
            m_hasPlacementOverride = false;
            m_isTargetPositionSet = false;
            m_exclusionRect = null;

            var showMode = ShowMode;

            if (!showAsContextFlyout && showOptions != null)
            {
                showMode = showOptions.ShowMode;

                if (showOptions.Position.HasValue)
                {
                    SetTargetPosition(showOptions.Position.Value);
                }

                m_exclusionRect = showOptions.ExclusionRect;

                if (showOptions.Placement != FlyoutPlacementMode.Auto)
                {
                    m_hasPlacementOverride = true;
                    m_placementOverride = showOptions.Placement;
                }
            }

            UpdateStateToShowMode(showMode);
        }

        internal static FlyoutShowOptions CloneShowOptions(FlyoutShowOptions showOptions)
        {
            if (showOptions == null)
            {
                return null;
            }

            return new FlyoutShowOptions
            {
                ExclusionRect = showOptions.ExclusionRect,
                Placement = showOptions.Placement,
                Position = showOptions.Position,
                ShowMode = showOptions.ShowMode
            };
        }

        private static void ValidateTargetPosition(Point targetPosition)
        {
            if (double.IsNaN(targetPosition.X) || double.IsNaN(targetPosition.Y))
            {
                throw new ArgumentException("Position cannot contain NaN values.", nameof(targetPosition));
            }
        }

        private void SetTargetPosition(Point targetPosition)
        {
            ValidateTargetPosition(targetPosition);
            m_isTargetPositionSet = true;
            m_targetPosition = targetPosition;
        }

        private static FrameworkElement GetRootPlacementTargetForPosition(FlyoutShowOptions showOptions)
        {
            if (showOptions?.Position == null)
            {
                throw new ArgumentException("A placement target or show-options position is required.", nameof(showOptions));
            }

            if (Application.Current?.MainWindow?.Content is FrameworkElement content)
            {
                return content;
            }

            if (Application.Current?.MainWindow is Window window)
            {
                return window;
            }

            throw new InvalidOperationException("A root element is required to show a flyout at a position without a placement target.");
        }

        private Rect? GetTargetPositionRelativeExclusionRect()
        {
            if (!m_isTargetPositionSet || !m_exclusionRect.HasValue)
            {
                return null;
            }

            Rect exclusionRect = m_exclusionRect.Value;
            exclusionRect.Offset(-m_targetPosition.X, -m_targetPosition.Y);
            return exclusionRect;
        }

        private void UpdatePointerMoveAwayTracking()
        {
            if (IsOpen && m_shouldHideIfPointerMovesAway)
            {
                AddRootPointerMovedHandler();
            }
            else
            {
                RemoveRootPointerMovedHandler();
            }
        }

        private void AddRootPointerMovedHandler()
        {
            var root = GetRootElement();
            if (root == null || ReferenceEquals(root, m_pointerMoveAwayRoot))
            {
                return;
            }

            RemoveRootPointerMovedHandler();

            m_pointerMoveAwayRoot = root;
            m_pointerMoveAwayRoot.MouseMove += OnRootPointerMoved;
        }

        private void RemoveRootPointerMovedHandler()
        {
            if (m_pointerMoveAwayRoot != null)
            {
                m_pointerMoveAwayRoot.MouseMove -= OnRootPointerMoved;
                m_pointerMoveAwayRoot = null;
            }
        }

        private UIElement GetRootElement()
        {
            if (Target == null)
            {
                return null;
            }

            return Window.GetWindow(Target) ??
                   PresentationSource.FromVisual(Target)?.RootVisual as UIElement;
        }

        private void OnRootPointerMoved(object sender, MouseEventArgs e)
        {
            if (!IsOpen || !m_shouldHideIfPointerMovesAway)
            {
                return;
            }

            var root = (UIElement)sender;
            var pointerPosition = root.PointToScreen(e.GetPosition(root));

            if (ShouldHideForPointerMoveAway(pointerPosition))
            {
                Hide();
            }
        }

        internal bool ShouldHideForPointerMoveAway(Point pointerScreenPosition)
        {
            var boundsElement = PointerMoveAwayBoundsElement;
            if (boundsElement == null ||
                !boundsElement.IsVisible ||
                boundsElement.ActualWidth <= 0 ||
                boundsElement.ActualHeight <= 0)
            {
                return false;
            }

            var topLeft = boundsElement.PointToScreen(new Point(0, 0));
            var bottomRight = boundsElement.PointToScreen(new Point(boundsElement.ActualWidth, boundsElement.ActualHeight));
            var bounds = new Rect(topLeft, bottomRight);

            return IsPointerBeyondMoveAwayThreshold(bounds, pointerScreenPosition);
        }

        internal static bool IsPointerBeyondMoveAwayThreshold(Rect presenterBounds, Point pointerPosition)
        {
            double xDistance = 0;
            double yDistance = 0;

            if (pointerPosition.X < presenterBounds.Left)
            {
                xDistance = presenterBounds.Left - pointerPosition.X;
            }
            else if (pointerPosition.X > presenterBounds.Right)
            {
                xDistance = pointerPosition.X - presenterBounds.Right;
            }

            if (pointerPosition.Y < presenterBounds.Top)
            {
                yDistance = presenterBounds.Top - pointerPosition.Y;
            }
            else if (pointerPosition.Y > presenterBounds.Bottom)
            {
                yDistance = pointerPosition.Y - presenterBounds.Bottom;
            }

            return xDistance * xDistance + yDistance * yDistance > s_pointerMoveAwayThresholdSquared;
        }

        private void CancelAsyncShow()
        {
            m_pendingShow = null;

            if (m_asyncShow != null)
            {
                m_asyncShow.Abort();
                m_asyncShow = null;
            }
        }

        internal bool TryStageLatestShowUntilOpenFlyoutCloses(FrameworkElement placementTarget, bool showAsContextFlyout, FlyoutShowOptions showOptions = null)
        {
            if (s_openFlyout != null &&
                s_openFlyout != this &&
                s_openFlyout.IsOpen)
            {
                s_pendingFlyoutShow = new PendingFlyoutShow(this, placementTarget, showAsContextFlyout, showOptions);
                s_openFlyout.Hide();
                return true;
            }

            return false;
        }

        internal static void SetOpenFlyout(FlyoutBase flyout)
        {
            s_openFlyout = flyout;
        }

        private static void ClearOpenFlyout(FlyoutBase flyout)
        {
            if (s_openFlyout == flyout)
            {
                s_openFlyout = null;
            }
        }

        private static bool TryTakePendingFlyoutShow(FlyoutBase closingFlyout, out PendingFlyoutShow pendingFlyoutShow)
        {
            pendingFlyoutShow = null;

            if (s_openFlyout != null && s_openFlyout != closingFlyout)
            {
                return false;
            }

            if (s_pendingFlyoutShow != null)
            {
                pendingFlyoutShow = s_pendingFlyoutShow;
                s_pendingFlyoutShow = null;
                return true;
            }

            return false;
        }

        private static readonly IMultiValueConverter s_fullPlacementWidthConverter = new FullPlacementWidthConverter();
        private static readonly IMultiValueConverter s_fullPlacementHeightConverter = new FullPlacementHeightConverter();
        private static readonly IValueConverter s_placementConverter = new PlacementConverter();

        private const double s_offset = 4;
        private const double s_pointerMoveAwayThresholdSquared = 80 * 80;

        private Control m_presenter;
        private PopupEx m_popup;
        private bool m_showingAsContextFlyout;
        private bool m_isTargetPositionSet;
        private Point m_targetPosition;
        private Rect? m_exclusionRect;
        private Point? m_absolutePopupPlacementPoint;
        private bool m_hasPlacementOverride;
        private FlyoutPlacementMode m_placementOverride = FlyoutPlacementMode.Top;
        private WeakReference<IInputElement> m_weakRefToPreviousFocus;
        private FrameworkElement m_trackedPlacementTarget;
        private bool m_closing;
        private bool m_suppressNextOpened;
        private bool m_shouldTakeFocus = true;
        private bool m_shouldHideIfPointerMovesAway;
        private UIElement m_pointerMoveAwayRoot;
        private Action m_pendingShow;
        private DispatcherOperation m_asyncShow;
        private static FlyoutBase s_openFlyout;
        private static PendingFlyoutShow s_pendingFlyoutShow;

        [DllImport("user32.dll")]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr insertAfter, int x, int y, int cx, int cy, uint flags);

        [DllImport("user32.dll")]
        private static extern IntPtr MonitorFromRect(ref NativeRect rect, uint flags);

        [DllImport("user32.dll")]
        private static extern bool GetMonitorInfo(IntPtr monitor, [In, Out] MonitorInfo monitorInfo);

        private const uint SWP_NOSIZE = 0x0001;
        private const uint SWP_NOZORDER = 0x0004;
        private const uint SWP_NOACTIVATE = 0x0010;
        private const uint SWP_SHOWWINDOW = 0x0040;
        private const uint MONITOR_DEFAULTTONEAREST = 0x00000002;

        [StructLayout(LayoutKind.Sequential)]
        private struct NativeRect
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        [StructLayout(LayoutKind.Sequential)]
        private sealed class MonitorInfo
        {
            public int Size = Marshal.SizeOf(typeof(MonitorInfo));
            public NativeRect Monitor;
            public NativeRect Work;
            public int Flags;
        }

        private sealed class PendingFlyoutShow
        {
            public PendingFlyoutShow(FlyoutBase flyout, FrameworkElement placementTarget, bool showAsContextFlyout, FlyoutShowOptions showOptions)
            {
                _flyout = flyout;
                _placementTarget = placementTarget;
                _showAsContextFlyout = showAsContextFlyout;
                _showOptions = CloneShowOptions(showOptions);
            }

            public void Show()
            {
                _flyout.ShowAtCore(_placementTarget, _showAsContextFlyout, _showOptions);
            }

            private readonly FlyoutBase _flyout;
            private readonly FrameworkElement _placementTarget;
            private readonly bool _showAsContextFlyout;
            private readonly FlyoutShowOptions _showOptions;
        }

        private class FullPlacementWidthConverter : IMultiValueConverter
        {
            public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
            {
                double windowWidth = (double)values[0];
                Thickness border = (Thickness)values[1];
                return windowWidth - border.Left - border.Right;
            }

            public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
            {
                return CreateDoNothingValues(targetTypes);
            }
        }

        private class FullPlacementHeightConverter : IMultiValueConverter
        {
            public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
            {
                double windowHeight = (double)values[0];
                Thickness border = (Thickness)values[1];
                return windowHeight - border.Top - border.Bottom;
            }

            public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
            {
                return CreateDoNothingValues(targetTypes);
            }
        }

        private class PlacementConverter : IValueConverter
        {
            public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            {
                var placement = (FlyoutPlacementMode)value;
                if (placement == FlyoutPlacementMode.Auto)
                {
                    placement = FlyoutPlacementMode.Top;
                }

                return (CustomPlacementMode)placement;
            }

            public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            {
                return Binding.DoNothing;
            }
        }

        private static object[] CreateDoNothingValues(Type[] targetTypes)
        {
            var values = new object[targetTypes.Length];
            for (int i = 0; i < values.Length; i++)
            {
                values[i] = Binding.DoNothing;
            }

            return values;
        }
    }
}
