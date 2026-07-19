using System;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using ModernWpf.Automation.Peers;
using ModernWpf.Controls.Primitives;

namespace ModernWpf.Controls
{
    public partial class TeachingTip : ContentControl
    {
        private const string PopupName = "Popup";
        private const string ContainerName = "Container";
        private const string TailOcclusionGridName = "TailOcclusionGrid";
        private const string TailOcclusionScaleTransformName = "TailOcclusionScaleTransform";
        private const string ContentRootGridShadowChromeName = "ContentRootGridShadowChrome";
        private const string ContentRootGridName = "ContentRootGrid";
        private const string HeroContentBorderName = "HeroContentBorder";
        private const string MainContentPresenterName = "MainContentPresenter";
        private const string TitleTextBlockName = "TitleTextBlock";
        private const string SubtitleTextBlockName = "SubtitleTextBlock";
        private const string IconPresenterName = "IconPresenter";
        private const string ActionButtonName = "ActionButton";
        private const string CloseButtonName = "CloseButton";
        private const string AlternateCloseButtonName = "AlternateCloseButton";
        private const string TailPolygonName = "TailPolygon";
        private const double DefaultTipHeightAndWidth = 320.0;
        private const double ContractedTipSize = 20.0;
        private const double ContentElevation = 32.0;
        private const double ContractedContentElevation = 0.01;
        private static readonly TimeSpan ExpandAnimationDuration = TimeSpan.FromMilliseconds(300);
        private static readonly TimeSpan ContractAnimationDuration = TimeSpan.FromMilliseconds(200);
        private static readonly Point ExpandAnimationEasingControlPoint1 = new Point(0.1, 0.9);
        private static readonly Point ExpandAnimationEasingControlPoint2 = new Point(0.2, 1.0);
        private static readonly Point ContractAnimationEasingControlPoint1 = new Point(0.7, 0.0);
        private static readonly Point ContractAnimationEasingControlPoint2 = new Point(1.0, 0.5);

        static TeachingTip()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(TeachingTip), new FrameworkPropertyMetadata(typeof(TeachingTip)));
        }

        public TeachingTip()
        {
            SetValue(TemplateSettingsPropertyKey, new TeachingTipTemplateSettings());
            Loaded += OnLoaded;
            Unloaded += OnUnloaded;
        }

        public event TypedEventHandler<TeachingTip, object> ActionButtonClick;

        public event TypedEventHandler<TeachingTip, object> CloseButtonClick;

        public event TypedEventHandler<TeachingTip, TeachingTipClosingEventArgs> Closing;

        public event TypedEventHandler<TeachingTip, TeachingTipClosedEventArgs> Closed;

        public event TypedEventHandler<TeachingTip, TeachingTipOpenedEventArgs> Opened;

        protected override void OnContentChanged(object oldContent, object newContent)
        {
            base.OnContentChanged(oldContent, newContent);
            UpdateVisualState();
        }

        public override void OnApplyTemplate()
        {
            StopTipAnimation();
            UnhookButtonEvents();
            UpdateLightDismissHook(null);
            UpdateRepositionHooks(null, null);

            base.OnApplyTemplate();

            _popup = GetTemplateChild(PopupName) as Popup;
            _container = GetTemplateChild(ContainerName) as FrameworkElement;
            _tailOcclusionGrid = GetTemplateChild(TailOcclusionGridName) as Grid;
            _tailOcclusionScaleTransform = GetTemplateChild(TailOcclusionScaleTransformName) as ScaleTransform;
            _contentRootGridShadowChrome = GetTemplateChild(ContentRootGridShadowChromeName) as ThemeShadowChrome;
            _contentRootGrid = GetTemplateChild(ContentRootGridName) as Border;
            _heroContentBorder = GetTemplateChild(HeroContentBorderName) as Border;
            _mainContentPresenter = GetTemplateChild(MainContentPresenterName) as ContentPresenterEx;
            _titleTextBlock = GetTemplateChild(TitleTextBlockName) as TextBlock;
            _subtitleTextBlock = GetTemplateChild(SubtitleTextBlockName) as TextBlock;
            _iconPresenter = GetTemplateChild(IconPresenterName) as FrameworkElement;
            _actionButton = GetTemplateChild(ActionButtonName) as Button;
            _closeButton = GetTemplateChild(CloseButtonName) as Button;
            _alternateCloseButton = GetTemplateChild(AlternateCloseButtonName) as Button;
            _tailPolygon = GetTemplateChild(TailPolygonName) as Polygon;

            ApplySizeResources();
            HookButtonEvents();
            ConfigurePopup();
            UpdateIcon();
            UpdateVisualState();

            if (IsOpen)
            {
                StartOpenAnimation();
            }
        }

        protected override AutomationPeer OnCreateAutomationPeer()
        {
            return new TeachingTipAutomationPeer(this);
        }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);

            if (e.Property == TitleProperty ||
                e.Property == System.Windows.Automation.AutomationProperties.NameProperty ||
                e.Property == System.Windows.Automation.AutomationProperties.AutomationIdProperty)
            {
                SetPopupAutomationProperties();
            }
        }

        private void ApplySizeResources()
        {
            if (_tailOcclusionGrid == null)
            {
                return;
            }

            _tailOcclusionGrid.MinHeight = GetDoubleResource("TeachingTipMinHeight", _tailOcclusionGrid.MinHeight);
            _tailOcclusionGrid.MinWidth = GetDoubleResource("TeachingTipMinWidth", _tailOcclusionGrid.MinWidth);
            _tailOcclusionGrid.MaxHeight = GetDoubleResource("TeachingTipMaxHeight", _tailOcclusionGrid.MaxHeight);
            _tailOcclusionGrid.MaxWidth = GetDoubleResource("TeachingTipMaxWidth", _tailOcclusionGrid.MaxWidth);
        }

        private double GetDoubleResource(string resourceKey, double fallback)
        {
            var resource = TryFindResource(resourceKey);
            if (resource is double value)
            {
                return value;
            }

            if (resource is IConvertible convertible)
            {
                try
                {
                    return convertible.ToDouble(null);
                }
                catch (FormatException)
                {
                }
                catch (InvalidCastException)
                {
                }
                catch (OverflowException)
                {
                }
            }

            return fallback;
        }

        private static void OnIsOpenPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var teachingTip = (TeachingTip)d;
            if ((bool)e.NewValue)
            {
                var suppressOpenAnimation = teachingTip._suppressNextOpenAnimation;
                teachingTip._suppressNextOpenAnimation = false;
                teachingTip._isOpeningAnimationActive = false;
                teachingTip._isClosingAnimationActive = false;
                teachingTip.StopTipAnimation();
                teachingTip._lastCloseReason = TeachingTipCloseReason.Programmatic;
                teachingTip.UpdateVisualState();

                if (suppressOpenAnimation)
                {
                    teachingTip.SetTipScale(1.0, 1.0);
                    teachingTip.SetTipShadowDepth(ContentElevation);
                }
                else
                {
                    teachingTip._hasRaisedOpenedForCurrentOpen = false;
                    teachingTip.StartOpenAnimation();
                }
            }
            else if ((bool)e.OldValue)
            {
                if (teachingTip._isCompletingClose)
                {
                    teachingTip.StartCloseAnimationOrClose(teachingTip._lastCloseReason);
                }
                else
                {
                    teachingTip.BeginClose(teachingTip._lastCloseReason);
                }
            }
            else
            {
                teachingTip.UpdateVisualState();
            }
        }

        private static void OnVisualStatePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((TeachingTip)d).UpdateVisualState();
        }

        private static void OnTargetPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var teachingTip = (TeachingTip)d;
            teachingTip.UpdateTargetUnloadHook((FrameworkElement)e.NewValue);
            teachingTip.UpdateVisualState();
        }

        private static void OnIconSourcePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var teachingTip = (TeachingTip)d;
            teachingTip.UpdateIcon();
            teachingTip.UpdateVisualState();
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            UpdateTargetUnloadHook(Target);
            UpdateVisualState();
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            StopTipAnimation();
            _isOpeningAnimationActive = false;
            _isClosingAnimationActive = false;
            UpdateTargetUnloadHook(null);
            UpdateLightDismissHook(null);
            UpdateRepositionHooks(null, null);

            if (_popup != null)
            {
                _popup.SetCurrentValue(Popup.IsOpenProperty, false);
            }
        }

        private void HookButtonEvents()
        {
            if (_actionButton != null)
            {
                _actionButton.Click += OnActionButtonClick;
            }

            if (_closeButton != null)
            {
                _closeButton.Click += OnCloseButtonClick;
            }

            if (_alternateCloseButton != null)
            {
                _alternateCloseButton.Click += OnCloseButtonClick;

                if (string.IsNullOrEmpty(System.Windows.Automation.AutomationProperties.GetName(_alternateCloseButton)))
                {
                    System.Windows.Automation.AutomationProperties.SetName(_alternateCloseButton, "Close");
                }

                if (_alternateCloseButton.ToolTip == null)
                {
                    _alternateCloseButton.ToolTip = "Close";
                }
            }
        }

        private void UnhookButtonEvents()
        {
            if (_actionButton != null)
            {
                _actionButton.Click -= OnActionButtonClick;
            }

            if (_closeButton != null)
            {
                _closeButton.Click -= OnCloseButtonClick;
            }

            if (_alternateCloseButton != null)
            {
                _alternateCloseButton.Click -= OnCloseButtonClick;
            }
        }

        private void ConfigurePopup()
        {
            if (_popup == null)
            {
                return;
            }

            _popup.StaysOpen = true;
            _popup.PopupAnimation = PopupAnimation.None;
            _popup.Placement = PlacementMode.Custom;
            _popup.CustomPopupPlacementCallback = PositionPopup;
            SetPopupAutomationProperties();
        }

        private void SetPopupAutomationProperties()
        {
            if (_popup == null)
            {
                return;
            }

            var name = System.Windows.Automation.AutomationProperties.GetName(this);
            if (string.IsNullOrEmpty(name))
            {
                name = Title;
            }

            System.Windows.Automation.AutomationProperties.SetName(_popup, name ?? string.Empty);
            System.Windows.Automation.AutomationProperties.SetAutomationId(
                _popup,
                System.Windows.Automation.AutomationProperties.GetAutomationId(this));
        }

        private void OnActionButtonClick(object sender, RoutedEventArgs e)
        {
            ExecuteCommand(ActionButtonCommand, ActionButtonCommandParameter);
            ActionButtonClick?.Invoke(this, e);
        }

        private void OnCloseButtonClick(object sender, RoutedEventArgs e)
        {
            _lastCloseReason = TeachingTipCloseReason.CloseButton;
            ExecuteCommand(CloseButtonCommand, CloseButtonCommandParameter);
            CloseButtonClick?.Invoke(this, e);
            IsOpen = false;
        }

        private static void ExecuteCommand(ICommand command, object parameter)
        {
            if (command?.CanExecute(parameter) == true)
            {
                command.Execute(parameter);
            }
        }

        private void BeginClose(TeachingTipCloseReason reason)
        {
            var args = new TeachingTipClosingEventArgs(reason);
            Closing?.Invoke(this, args);

            if (args.Cancel)
            {
                RestoreOpenState();
                return;
            }

            if (args.HasOutstandingDeferrals)
            {
                RestoreOpenState();
                args.DeferralsCompleted += OnDeferralsCompleted;
                return;
            }

            CompleteClose(reason);

            void OnDeferralsCompleted(object sender, EventArgs e)
            {
                args.DeferralsCompleted -= OnDeferralsCompleted;
                if (args.Cancel)
                {
                    RestoreOpenState();
                    return;
                }

                CompleteClose(reason);
            }
        }

        private void RestoreOpenState()
        {
            _suppressNextOpenAnimation = true;
            SetCurrentValue(IsOpenProperty, true);
            UpdateVisualState();
        }

        private void CompleteClose(TeachingTipCloseReason reason)
        {
            _lastCloseReason = reason;

            if (IsOpen)
            {
                _isCompletingClose = true;
                try
                {
                    SetCurrentValue(IsOpenProperty, false);
                }
                finally
                {
                    _isCompletingClose = false;
                }
            }
            else
            {
                StartCloseAnimationOrClose(reason);
            }
        }

        private void UpdateIcon()
        {
            TemplateSettings.IconElement = IconSource?.CreateIconElement();
        }

        private void UpdateVisualState()
        {
            if (_container != null)
            {
                _container.Visibility = ShouldKeepPopupOpen ? Visibility.Visible : Visibility.Collapsed;
                _container.Margin = default;
            }

            if (_titleTextBlock != null)
            {
                VisualStateManager.GoToState(this, string.IsNullOrEmpty(Title) ? "CollapseTitleTextBlock" : "ShowTitleTextBlock", false);
            }

            if (_subtitleTextBlock != null)
            {
                VisualStateManager.GoToState(this, string.IsNullOrEmpty(Subtitle) ? "CollapseSubtitleTextBlock" : "ShowSubtitleTextBlock", false);
            }

            if (_iconPresenter != null)
            {
                VisualStateManager.GoToState(this, IconSource == null ? "NoIcon" : "Icon", false);
            }

            UpdateButtons();
            UpdateMainContent();
            UpdateHeroContentPlacement();
            UpdateTailPlacement();
            UpdateBackgroundResources();
            UpdatePopupState();
        }

        private void UpdatePopupState()
        {
            if (_popup == null)
            {
                return;
            }

            var shouldKeepPopupOpen = ShouldKeepPopupOpen;
            var placementTarget = GetPopupPlacementTarget();
            _popup.Placement = PlacementMode.Custom;
            _popup.CustomPopupPlacementCallback = PositionPopup;

            if (!ReferenceEquals(_popup.PlacementTarget, placementTarget))
            {
                _popup.PlacementTarget = placementTarget;
            }

            if (_popup.IsOpen != shouldKeepPopupOpen)
            {
                _popup.SetCurrentValue(Popup.IsOpenProperty, shouldKeepPopupOpen);
            }

            if (shouldKeepPopupOpen)
            {
                RepositionPopup();
            }

            UpdateLightDismissHook(IsOpen && IsLightDismissEnabled ? Window.GetWindow(placementTarget) : null);
            UpdateRepositionHooks(IsOpen ? placementTarget : null, IsOpen ? Window.GetWindow(placementTarget) : null);
        }

        private void UpdateButtons()
        {
            var hasActionButton = !ControlHelper.IsNullOrEmptyString(ActionButtonContent) || ActionButtonCommand != null;
            var hasCloseButton = !ControlHelper.IsNullOrEmptyString(CloseButtonContent) || CloseButtonCommand != null;

            if (_actionButton != null || _closeButton != null)
            {
                var buttonState = hasActionButton && hasCloseButton
                    ? "BothButtonsVisible"
                    : hasActionButton
                        ? "ActionButtonVisible"
                        : hasCloseButton
                            ? "CloseButtonVisible"
                            : "NoButtonsVisible";
                VisualStateManager.GoToState(this, buttonState, false);
                VisualStateManager.GoToState(this, hasCloseButton ? "FooterCloseButton" : "HeaderCloseButton", false);
            }
        }

        private void UpdateMainContent()
        {
            if (_mainContentPresenter == null)
            {
                return;
            }

            VisualStateManager.GoToState(this, ControlHelper.IsNullOrEmptyString(Content) ? "NoContent" : "Content", false);
        }

        private void UpdateHeroContentPlacement()
        {
            if (_heroContentBorder == null)
            {
                return;
            }

            _heroContentBorder.Visibility = HeroContent == null ? Visibility.Collapsed : Visibility.Visible;
            VisualStateManager.GoToState(
                this,
                HeroContentPlacement == TeachingTipHeroContentPlacementMode.Bottom ? "HeroContentBottom" : "HeroContentTop",
                false);
        }

        private void UpdateTailPlacement()
        {
            if (_tailPolygon == null)
            {
                return;
            }

            var showTail = TailVisibility == TeachingTipTailVisibility.Visible ||
                TailVisibility == TeachingTipTailVisibility.Auto && Target != null;

            if (!showTail)
            {
                VisualStateManager.GoToState(this, "Untargeted", false);
                return;
            }

            var placement = GetEffectivePlacement();
            UpdateAnimationCenterPoint(placement);
            VisualStateManager.GoToState(this, placement.ToString(), false);
        }

        private CustomPopupPlacement[] PositionPopup(Size popupSize, Size targetSize, Point offset)
        {
            var fallbackPlacements = GetPlacementFallbacks(GetEffectivePlacement());
            var placements = new CustomPopupPlacement[ShouldConstrainToRootBounds ? fallbackPlacements.Length : 1];

            for (var i = 0; i < placements.Length; i++)
            {
                placements[i] = CreatePopupPlacement(fallbackPlacements[i], popupSize, targetSize);
            }

            if (ShouldConstrainToRootBounds && placements.Length > 1)
            {
                var fittingIndex = FindFirstPlacementWithinRootBounds(placements, popupSize);
                if (fittingIndex > 0)
                {
                    var fittingPlacement = placements[fittingIndex];
                    for (var i = fittingIndex; i > 0; i--)
                    {
                        placements[i] = placements[i - 1];
                    }

                    placements[0] = fittingPlacement;
                }
                else if (fittingIndex < 0)
                {
                    placements[0] = new CustomPopupPlacement(
                        ClampPlacementToRootBounds(placements[0].Point, popupSize),
                        placements[0].PrimaryAxis);
                }
            }

            return placements;
        }

        private CustomPopupPlacement CreatePopupPlacement(TeachingTipPlacementMode placement, Size popupSize, Size targetSize)
        {
            return new CustomPopupPlacement(
                GetPopupPoint(placement, popupSize, targetSize),
                GetPopupPrimaryAxis(placement));
        }

        private Point GetPopupPoint(TeachingTipPlacementMode placement, Size popupSize, Size targetSize)
        {
            var margin = PlacementMargin;
            var centeredX = (targetSize.Width - popupSize.Width) / 2 + margin.Left - margin.Right;
            var centeredY = (targetSize.Height - popupSize.Height) / 2 + margin.Top - margin.Bottom;
            var topY = -popupSize.Height - margin.Bottom;
            var bottomY = targetSize.Height + margin.Top;
            var leftX = -popupSize.Width - margin.Right;
            var rightX = targetSize.Width + margin.Left;

            switch (placement)
            {
                case TeachingTipPlacementMode.Top:
                    return new Point(centeredX, topY);
                case TeachingTipPlacementMode.TopLeft:
                    return new Point(margin.Left, topY);
                case TeachingTipPlacementMode.TopRight:
                    return new Point(targetSize.Width - popupSize.Width - margin.Right, topY);
                case TeachingTipPlacementMode.Left:
                    return new Point(leftX, centeredY);
                case TeachingTipPlacementMode.LeftTop:
                    return new Point(leftX, margin.Top);
                case TeachingTipPlacementMode.LeftBottom:
                    return new Point(leftX, targetSize.Height - popupSize.Height - margin.Bottom);
                case TeachingTipPlacementMode.Right:
                    return new Point(rightX, centeredY);
                case TeachingTipPlacementMode.RightTop:
                    return new Point(rightX, margin.Top);
                case TeachingTipPlacementMode.RightBottom:
                    return new Point(rightX, targetSize.Height - popupSize.Height - margin.Bottom);
                case TeachingTipPlacementMode.Center:
                    return new Point(centeredX, centeredY);
                case TeachingTipPlacementMode.BottomLeft:
                    return new Point(margin.Left, bottomY);
                case TeachingTipPlacementMode.BottomRight:
                    return new Point(targetSize.Width - popupSize.Width - margin.Right, bottomY);
                default:
                    return new Point(centeredX, bottomY);
            }
        }

        private static PopupPrimaryAxis GetPopupPrimaryAxis(TeachingTipPlacementMode placement)
        {
            switch (placement)
            {
                case TeachingTipPlacementMode.Left:
                case TeachingTipPlacementMode.LeftTop:
                case TeachingTipPlacementMode.LeftBottom:
                case TeachingTipPlacementMode.Right:
                case TeachingTipPlacementMode.RightTop:
                case TeachingTipPlacementMode.RightBottom:
                    return PopupPrimaryAxis.Horizontal;

                case TeachingTipPlacementMode.Center:
                    return PopupPrimaryAxis.None;

                default:
                    return PopupPrimaryAxis.Vertical;
            }
        }

        private TeachingTipPlacementMode GetEffectivePlacement()
        {
            if (PreferredPlacement == TeachingTipPlacementMode.Auto)
            {
                return Target == null ? TeachingTipPlacementMode.Bottom : TeachingTipPlacementMode.Top;
            }

            return PreferredPlacement;
        }

        private static TeachingTipPlacementMode[] GetPlacementFallbacks(TeachingTipPlacementMode placement)
        {
            switch (placement)
            {
                case TeachingTipPlacementMode.Top:
                    return new[] { TeachingTipPlacementMode.Top, TeachingTipPlacementMode.Bottom, TeachingTipPlacementMode.Right, TeachingTipPlacementMode.Left };
                case TeachingTipPlacementMode.TopLeft:
                    return new[] { TeachingTipPlacementMode.TopLeft, TeachingTipPlacementMode.BottomLeft, TeachingTipPlacementMode.TopRight, TeachingTipPlacementMode.BottomRight };
                case TeachingTipPlacementMode.TopRight:
                    return new[] { TeachingTipPlacementMode.TopRight, TeachingTipPlacementMode.BottomRight, TeachingTipPlacementMode.TopLeft, TeachingTipPlacementMode.BottomLeft };
                case TeachingTipPlacementMode.Left:
                    return new[] { TeachingTipPlacementMode.Left, TeachingTipPlacementMode.Right, TeachingTipPlacementMode.Bottom, TeachingTipPlacementMode.Top };
                case TeachingTipPlacementMode.LeftTop:
                    return new[] { TeachingTipPlacementMode.LeftTop, TeachingTipPlacementMode.RightTop, TeachingTipPlacementMode.LeftBottom, TeachingTipPlacementMode.RightBottom };
                case TeachingTipPlacementMode.LeftBottom:
                    return new[] { TeachingTipPlacementMode.LeftBottom, TeachingTipPlacementMode.RightBottom, TeachingTipPlacementMode.LeftTop, TeachingTipPlacementMode.RightTop };
                case TeachingTipPlacementMode.Right:
                    return new[] { TeachingTipPlacementMode.Right, TeachingTipPlacementMode.Left, TeachingTipPlacementMode.Bottom, TeachingTipPlacementMode.Top };
                case TeachingTipPlacementMode.RightTop:
                    return new[] { TeachingTipPlacementMode.RightTop, TeachingTipPlacementMode.LeftTop, TeachingTipPlacementMode.RightBottom, TeachingTipPlacementMode.LeftBottom };
                case TeachingTipPlacementMode.RightBottom:
                    return new[] { TeachingTipPlacementMode.RightBottom, TeachingTipPlacementMode.LeftBottom, TeachingTipPlacementMode.RightTop, TeachingTipPlacementMode.LeftTop };
                case TeachingTipPlacementMode.Center:
                    return new[] { TeachingTipPlacementMode.Center };
                case TeachingTipPlacementMode.BottomLeft:
                    return new[] { TeachingTipPlacementMode.BottomLeft, TeachingTipPlacementMode.TopLeft, TeachingTipPlacementMode.BottomRight, TeachingTipPlacementMode.TopRight };
                case TeachingTipPlacementMode.BottomRight:
                    return new[] { TeachingTipPlacementMode.BottomRight, TeachingTipPlacementMode.TopRight, TeachingTipPlacementMode.BottomLeft, TeachingTipPlacementMode.TopLeft };
                default:
                    return new[] { TeachingTipPlacementMode.Bottom, TeachingTipPlacementMode.Top, TeachingTipPlacementMode.Right, TeachingTipPlacementMode.Left };
            }
        }

        private int FindFirstPlacementWithinRootBounds(CustomPopupPlacement[] placements, Size popupSize)
        {
            for (var i = 0; i < placements.Length; i++)
            {
                if (IsPlacementWithinRootBounds(placements[i].Point, popupSize))
                {
                    return i;
                }
            }

            return -1;
        }

        private bool IsPlacementWithinRootBounds(Point popupPoint, Size popupSize)
        {
            if (!TryGetRootRelativePopupBounds(popupPoint, popupSize, out var popupBounds, out var rootBounds))
            {
                return false;
            }

            return popupBounds.Left >= rootBounds.Left &&
                popupBounds.Top >= rootBounds.Top &&
                popupBounds.Right <= rootBounds.Right &&
                popupBounds.Bottom <= rootBounds.Bottom;
        }

        private Point ClampPlacementToRootBounds(Point popupPoint, Size popupSize)
        {
            if (!TryGetRootRelativePopupBounds(popupPoint, popupSize, out var popupBounds, out var rootBounds))
            {
                return popupPoint;
            }

            var clampedLeft = Math.Max(rootBounds.Left, Math.Min(popupBounds.Left, Math.Max(rootBounds.Left, rootBounds.Right - popupSize.Width)));
            var clampedTop = Math.Max(rootBounds.Top, Math.Min(popupBounds.Top, Math.Max(rootBounds.Top, rootBounds.Bottom - popupSize.Height)));

            return new Point(
                popupPoint.X + clampedLeft - popupBounds.Left,
                popupPoint.Y + clampedTop - popupBounds.Top);
        }

        private bool TryGetRootRelativePopupBounds(Point popupPoint, Size popupSize, out Rect popupBounds, out Rect rootBounds)
        {
            popupBounds = default;
            rootBounds = default;

            var placementTarget = GetPopupPlacementTarget();
            var root = GetPopupRoot();
            if (placementTarget == null || root == null || root.ActualWidth <= 0 || root.ActualHeight <= 0)
            {
                return false;
            }

            try
            {
                var targetOrigin = placementTarget.TranslatePoint(new Point(), root);
                popupBounds = new Rect(
                    targetOrigin.X + popupPoint.X,
                    targetOrigin.Y + popupPoint.Y,
                    popupSize.Width,
                    popupSize.Height);
                rootBounds = new Rect(0, 0, root.ActualWidth, root.ActualHeight);
                return true;
            }
            catch (InvalidOperationException)
            {
                return false;
            }
        }

        private void UpdateBackgroundResources()
        {
            VisualStateManager.GoToState(this, IsLightDismissEnabled ? "LightDismiss" : "NormalDismiss", false);
        }

        private FrameworkElement GetPopupPlacementTarget()
        {
            if (Target != null)
            {
                return Target;
            }

            return GetPopupRoot() ?? this;
        }

        private FrameworkElement GetPopupRoot()
        {
            var window = Window.GetWindow(Target ?? this);
            return window?.Content as FrameworkElement ?? this;
        }

        private void CloseWithReason(TeachingTipCloseReason reason)
        {
            _lastCloseReason = reason;
            SetCurrentValue(IsOpenProperty, false);
        }

        private void UpdateTargetUnloadHook(FrameworkElement target)
        {
            if (ReferenceEquals(_unloadHookedTarget, target))
            {
                return;
            }

            if (_unloadHookedTarget != null)
            {
                _unloadHookedTarget.Unloaded -= OnTargetUnloaded;
            }

            _unloadHookedTarget = target;

            if (_unloadHookedTarget != null)
            {
                _unloadHookedTarget.Unloaded += OnTargetUnloaded;
            }
        }

        private void OnTargetUnloaded(object sender, RoutedEventArgs e)
        {
            if (IsOpen)
            {
                CloseWithReason(TeachingTipCloseReason.Programmatic);
            }
        }

        private void UpdateLightDismissHook(Window window)
        {
            if (ReferenceEquals(_lightDismissWindow, window))
            {
                return;
            }

            if (_lightDismissWindow != null)
            {
                _lightDismissWindow.PreviewMouseDown -= OnWindowPreviewMouseDown;
            }

            _lightDismissWindow = window;

            if (_lightDismissWindow != null)
            {
                _lightDismissWindow.PreviewMouseDown += OnWindowPreviewMouseDown;
            }
        }

        private void OnWindowPreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!IsOpen || !IsLightDismissEnabled)
            {
                return;
            }

            if (IsMouseWithinElement(e, Target))
            {
                return;
            }

            CloseWithReason(TeachingTipCloseReason.LightDismiss);
            e.Handled = true;
        }

        private static bool IsMouseWithinElement(MouseEventArgs e, FrameworkElement element)
        {
            if (element == null || !element.IsVisible)
            {
                return false;
            }

            try
            {
                var position = e.GetPosition(element);
                return position.X >= 0 &&
                    position.Y >= 0 &&
                    position.X <= element.ActualWidth &&
                    position.Y <= element.ActualHeight;
            }
            catch (InvalidOperationException)
            {
                return false;
            }
        }

        private void UpdateRepositionHooks(FrameworkElement target, Window window)
        {
            if (!ReferenceEquals(_repositionTarget, target))
            {
                if (_repositionTarget != null)
                {
                    _repositionTarget.LayoutUpdated -= OnPlacementTargetLayoutUpdated;
                }

                _repositionTarget = target;

                if (_repositionTarget != null)
                {
                    _repositionTarget.LayoutUpdated += OnPlacementTargetLayoutUpdated;
                }
            }

            if (!ReferenceEquals(_repositionWindow, window))
            {
                if (_repositionWindow != null)
                {
                    _repositionWindow.LocationChanged -= OnPlacementWindowLocationChanged;
                    _repositionWindow.SizeChanged -= OnPlacementWindowSizeChanged;
                }

                _repositionWindow = window;

                if (_repositionWindow != null)
                {
                    _repositionWindow.LocationChanged += OnPlacementWindowLocationChanged;
                    _repositionWindow.SizeChanged += OnPlacementWindowSizeChanged;
                }
            }
        }

        private void OnPlacementTargetLayoutUpdated(object sender, EventArgs e)
        {
            RepositionPopup();
        }

        private void OnPlacementWindowLocationChanged(object sender, EventArgs e)
        {
            RepositionPopup();
        }

        private void OnPlacementWindowSizeChanged(object sender, SizeChangedEventArgs e)
        {
            RepositionPopup();
        }

        private void RepositionPopup()
        {
            if (_popup?.IsOpen == true)
            {
                _popup.Reposition();
            }
        }

        private bool ShouldKeepPopupOpen => IsOpen || _isClosingAnimationActive;

        internal bool IsIdleForAutomation => !_isOpeningAnimationActive && !_isClosingAnimationActive;

        private void StartOpenAnimation()
        {
            if (_tailOcclusionScaleTransform == null)
            {
                return;
            }

            UpdateAnimationCenterPoint(GetEffectivePlacement());

            if (!SharedHelpers.IsAnimationsEnabled)
            {
                SetTipScale(1.0, 1.0);
                SetTipShadowDepth(ContentElevation);
                RaiseOpened();
                return;
            }

            _isOpeningAnimationActive = true;
            var startScale = GetExpandStartScale();
            AnimateTipScale(
                startScale.Width,
                startScale.Height,
                1.0,
                1.0,
                ExpandAnimationDuration,
                ExpandAnimationEasingControlPoint1,
                ExpandAnimationEasingControlPoint2,
                () =>
                {
                    _isOpeningAnimationActive = false;
                    SetTipScale(1.0, 1.0);
                    SetTipShadowDepth(ContentElevation);
                    RaiseOpened();
                },
                ContractedContentElevation,
                ContentElevation);
        }

        private void StartCloseAnimationOrClose(TeachingTipCloseReason reason)
        {
            if (_isClosingAnimationActive)
            {
                _lastCloseReason = reason;
                return;
            }

            _isClosingAnimationActive = true;
            UpdateVisualState();
            UpdateAnimationCenterPoint(GetEffectivePlacement());

            if (_tailOcclusionScaleTransform == null || !SharedHelpers.IsAnimationsEnabled)
            {
                FinishClose(reason);
                return;
            }

            var endScale = GetContractEndScale();
            AnimateTipScale(
                _tailOcclusionScaleTransform.ScaleX,
                _tailOcclusionScaleTransform.ScaleY,
                endScale.Width,
                endScale.Height,
                ContractAnimationDuration,
                ContractAnimationEasingControlPoint1,
                ContractAnimationEasingControlPoint2,
                () => FinishClose(reason),
                _contentRootGridShadowChrome?.Depth ?? ContentElevation,
                ContractedContentElevation);
        }

        private void FinishClose(TeachingTipCloseReason reason)
        {
            _isClosingAnimationActive = false;
            _isOpeningAnimationActive = false;
            StopTipAnimation();
            SetTipScale(1.0, 1.0);
            SetTipShadowDepth(ContractedContentElevation);
            UpdateVisualState();
            Closed?.Invoke(this, new TeachingTipClosedEventArgs(reason));
            GetAutomationPeerForEvents()?.RaiseWindowClosedEvent();
            _hasRaisedOpenedForCurrentOpen = false;
            _lastCloseReason = TeachingTipCloseReason.Programmatic;
        }

        private void RaiseOpened()
        {
            if (!IsOpen || _hasRaisedOpenedForCurrentOpen)
            {
                return;
            }

            _hasRaisedOpenedForCurrentOpen = true;
            Opened?.Invoke(this, new TeachingTipOpenedEventArgs());
            GetAutomationPeerForEvents()?.RaiseWindowOpenedEvent(GetAutomationNotificationString());
        }

        private TeachingTipAutomationPeer GetAutomationPeerForEvents()
        {
            return UIElementAutomationPeer.FromElement(this) as TeachingTipAutomationPeer
                ?? UIElementAutomationPeer.CreatePeerForElement(this) as TeachingTipAutomationPeer;
        }

        private string GetAutomationNotificationString()
        {
            var name = _popup != null
                ? System.Windows.Automation.AutomationProperties.GetName(_popup)
                : null;

            if (string.IsNullOrEmpty(name))
            {
                name = System.Windows.Automation.AutomationProperties.GetName(this);
            }

            return string.IsNullOrEmpty(name) ? Title ?? string.Empty : name;
        }

        private void AnimateTipScale(
            double fromScaleX,
            double fromScaleY,
            double toScaleX,
            double toScaleY,
            TimeSpan duration,
            Point easingControlPoint1,
            Point easingControlPoint2,
            Action completed,
            double? fromShadowDepth = null,
            double? toShadowDepth = null)
        {
            if (_tailOcclusionScaleTransform == null)
            {
                completed?.Invoke();
                return;
            }

            StopTipAnimation();
            SetTipScale(fromScaleX, fromScaleY);
            if (fromShadowDepth.HasValue)
            {
                SetTipShadowDepth(fromShadowDepth.Value);
            }

            var generation = ++_tipAnimationGeneration;
            var scaleXAnimation = CreateScaleAnimation(toScaleX, duration, easingControlPoint1, easingControlPoint2);
            var scaleYAnimation = CreateScaleAnimation(toScaleY, duration, easingControlPoint1, easingControlPoint2);
            scaleYAnimation.Completed += (sender, args) =>
            {
                if (generation == _tipAnimationGeneration)
                {
                    completed?.Invoke();
                }
            };

            _tailOcclusionScaleTransform.BeginAnimation(ScaleTransform.ScaleXProperty, scaleXAnimation, HandoffBehavior.SnapshotAndReplace);
            _tailOcclusionScaleTransform.BeginAnimation(ScaleTransform.ScaleYProperty, scaleYAnimation, HandoffBehavior.SnapshotAndReplace);

            if (_contentRootGridShadowChrome != null && toShadowDepth.HasValue)
            {
                var depthAnimation = CreateScaleAnimation(toShadowDepth.Value, duration, easingControlPoint1, easingControlPoint2);
                _contentRootGridShadowChrome.BeginAnimation(ThemeShadowChrome.DepthProperty, depthAnimation, HandoffBehavior.SnapshotAndReplace);
            }
        }

        private static DoubleAnimationUsingKeyFrames CreateScaleAnimation(
            double toValue,
            TimeSpan duration,
            Point easingControlPoint1,
            Point easingControlPoint2)
        {
            var animation = new DoubleAnimationUsingKeyFrames
            {
                Duration = new Duration(duration),
                FillBehavior = FillBehavior.HoldEnd
            };

            animation.KeyFrames.Add(new SplineDoubleKeyFrame(
                toValue,
                KeyTime.FromTimeSpan(duration),
                new KeySpline(easingControlPoint1, easingControlPoint2)));

            return animation;
        }

        private void StopTipAnimation()
        {
            _tipAnimationGeneration++;

            if (_tailOcclusionScaleTransform != null)
            {
                _tailOcclusionScaleTransform.BeginAnimation(ScaleTransform.ScaleXProperty, null);
                _tailOcclusionScaleTransform.BeginAnimation(ScaleTransform.ScaleYProperty, null);
            }

            if (_contentRootGridShadowChrome != null)
            {
                _contentRootGridShadowChrome.BeginAnimation(ThemeShadowChrome.DepthProperty, null);
            }
        }

        private void SetTipScale(double scaleX, double scaleY)
        {
            if (_tailOcclusionScaleTransform != null)
            {
                _tailOcclusionScaleTransform.ScaleX = scaleX;
                _tailOcclusionScaleTransform.ScaleY = scaleY;
            }
        }

        private void SetTipShadowDepth(double depth)
        {
            if (_contentRootGridShadowChrome != null)
            {
                _contentRootGridShadowChrome.Depth = depth;
            }
        }

        private Size GetExpandStartScale()
        {
            var width = GetAnimationWidth();
            var height = GetAnimationHeight();

            return new Size(
                Math.Min(0.01, ContractedTipSize / width),
                Math.Min(0.01, ContractedTipSize / height));
        }

        private Size GetContractEndScale()
        {
            return new Size(
                ContractedTipSize / GetAnimationWidth(),
                ContractedTipSize / GetAnimationHeight());
        }

        private double GetAnimationWidth()
        {
            return GetActualOrDefault(_tailOcclusionGrid?.ActualWidth, DefaultTipHeightAndWidth);
        }

        private double GetAnimationHeight()
        {
            return GetActualOrDefault(_tailOcclusionGrid?.ActualHeight, DefaultTipHeightAndWidth);
        }

        private static double GetActualOrDefault(double? actualValue, double defaultValue)
        {
            return actualValue.HasValue && actualValue.Value > 0.0 ? actualValue.Value : defaultValue;
        }

        private void UpdateAnimationCenterPoint(TeachingTipPlacementMode placement)
        {
            if (_tailOcclusionGrid == null || _tailOcclusionScaleTransform == null)
            {
                return;
            }

            _tailOcclusionGrid.UpdateLayout();

            var width = GetAnimationWidth();
            var height = GetAnimationHeight();
            var firstColumnWidth = GetColumnWidth(0, GetGridLengthResourceValue("TeachingTipTailShortSideLength", 8.0));
            var secondColumnWidth = GetColumnWidth(1, GetGridLengthResourceValue("TeachingTipTailMargin", 10.0));
            var nextToLastColumnWidth = GetColumnWidth(Math.Max(0, _tailOcclusionGrid.ColumnDefinitions.Count - 2), secondColumnWidth);
            var lastColumnWidth = GetColumnWidth(_tailOcclusionGrid.ColumnDefinitions.Count - 1, firstColumnWidth);
            var firstRowHeight = GetRowHeight(0, GetGridLengthResourceValue("TeachingTipTailShortSideLength", 8.0));
            var secondRowHeight = GetRowHeight(1, GetGridLengthResourceValue("TeachingTipTailMargin", 10.0));
            var nextToLastRowHeight = GetRowHeight(Math.Max(0, _tailOcclusionGrid.RowDefinitions.Count - 2), secondRowHeight);
            var lastRowHeight = GetRowHeight(_tailOcclusionGrid.RowDefinitions.Count - 1, firstRowHeight);

            var centerX = width / 2.0;
            var centerY = height / 2.0;

            switch (placement)
            {
                case TeachingTipPlacementMode.Top:
                    centerY = height - lastRowHeight;
                    break;

                case TeachingTipPlacementMode.Bottom:
                    centerY = firstRowHeight;
                    break;

                case TeachingTipPlacementMode.Left:
                    centerX = width - lastColumnWidth;
                    break;

                case TeachingTipPlacementMode.Right:
                    centerX = firstColumnWidth;
                    break;

                case TeachingTipPlacementMode.TopRight:
                    centerX = firstColumnWidth + secondColumnWidth + 1.0;
                    centerY = height - lastRowHeight;
                    break;

                case TeachingTipPlacementMode.TopLeft:
                    centerX = width - (nextToLastColumnWidth + lastColumnWidth + 1.0);
                    centerY = height - lastRowHeight;
                    break;

                case TeachingTipPlacementMode.BottomRight:
                    centerX = firstColumnWidth + secondColumnWidth + 1.0;
                    centerY = firstRowHeight;
                    break;

                case TeachingTipPlacementMode.BottomLeft:
                    centerX = width - (nextToLastColumnWidth + lastColumnWidth + 1.0);
                    centerY = firstRowHeight;
                    break;

                case TeachingTipPlacementMode.LeftTop:
                    centerX = width - lastColumnWidth;
                    centerY = height - (nextToLastRowHeight + lastRowHeight + 1.0);
                    break;

                case TeachingTipPlacementMode.LeftBottom:
                    centerX = width - lastColumnWidth;
                    centerY = firstRowHeight + secondRowHeight + 1.0;
                    break;

                case TeachingTipPlacementMode.RightTop:
                    centerX = firstColumnWidth;
                    centerY = height - (nextToLastRowHeight + lastRowHeight + 1.0);
                    break;

                case TeachingTipPlacementMode.RightBottom:
                    centerX = firstColumnWidth;
                    centerY = firstRowHeight + secondRowHeight + 1.0;
                    break;

                case TeachingTipPlacementMode.Center:
                    centerY = height - lastRowHeight;
                    break;
            }

            _tailOcclusionScaleTransform.CenterX = centerX;
            _tailOcclusionScaleTransform.CenterY = centerY;
        }

        private double GetColumnWidth(int index, double fallback)
        {
            if (_tailOcclusionGrid != null &&
                index >= 0 &&
                index < _tailOcclusionGrid.ColumnDefinitions.Count &&
                _tailOcclusionGrid.ColumnDefinitions[index].ActualWidth > 0.0)
            {
                return _tailOcclusionGrid.ColumnDefinitions[index].ActualWidth;
            }

            return fallback;
        }

        private double GetRowHeight(int index, double fallback)
        {
            if (_tailOcclusionGrid != null &&
                index >= 0 &&
                index < _tailOcclusionGrid.RowDefinitions.Count &&
                _tailOcclusionGrid.RowDefinitions[index].ActualHeight > 0.0)
            {
                return _tailOcclusionGrid.RowDefinitions[index].ActualHeight;
            }

            return fallback;
        }

        private double GetGridLengthResourceValue(object key, double fallback)
        {
            return TryFindResource(key) is GridLength gridLength ? gridLength.Value : fallback;
        }

        private TeachingTipCloseReason _lastCloseReason = TeachingTipCloseReason.Programmatic;
        private bool _isCompletingClose;
        private bool _isOpeningAnimationActive;
        private bool _isClosingAnimationActive;
        private bool _hasRaisedOpenedForCurrentOpen;
        private bool _suppressNextOpenAnimation;
        private int _tipAnimationGeneration;
        private Popup _popup;
        private FrameworkElement _unloadHookedTarget;
        private FrameworkElement _repositionTarget;
        private Window _repositionWindow;
        private Window _lightDismissWindow;
        private FrameworkElement _container;
        private Grid _tailOcclusionGrid;
        private ScaleTransform _tailOcclusionScaleTransform;
        private ThemeShadowChrome _contentRootGridShadowChrome;
        private Border _contentRootGrid;
        private Border _heroContentBorder;
        private ContentPresenterEx _mainContentPresenter;
        private TextBlock _titleTextBlock;
        private TextBlock _subtitleTextBlock;
        private FrameworkElement _iconPresenter;
        private Button _actionButton;
        private Button _closeButton;
        private Button _alternateCloseButton;
        private Polygon _tailPolygon;
    }
}
