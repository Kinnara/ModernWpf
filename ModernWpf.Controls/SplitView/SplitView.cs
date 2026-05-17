using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Threading;
using ModernWpf.Controls.Primitives;

namespace ModernWpf.Controls
{
    [ContentProperty(nameof(Content))]
    public partial class SplitView : ControlEx
    {
        private static readonly string[,,] s_visualStateTable =
        {
            {
                { "Closed", "OpenOverlayLeft" },
                { "Closed", "OpenOverlayRight" }
            },
            {
                { "Closed", "OpenInlineLeft" },
                { "Closed", "OpenInlineRight" }
            },
            {
                { "ClosedCompactLeft", "OpenCompactOverlayLeft" },
                { "ClosedCompactRight", "OpenCompactOverlayRight" }
            },
            {
                { "ClosedCompactLeft", "OpenInlineLeft" },
                { "ClosedCompactRight", "OpenInlineRight" }
            }
        };

        static SplitView()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(SplitView), new FrameworkPropertyMetadata(typeof(SplitView)));
        }

        public SplitView()
        {
            TemplateSettings = new SplitViewTemplateSettings();

            Loaded += OnLoaded;
            Unloaded += OnUnloaded;
            SizeChanged += OnSizeChanged;
            IsVisibleChanged += OnIsVisibleChanged;
        }

        public event TypedEventHandler<SplitView, object> PaneOpening;
        public event TypedEventHandler<SplitView, object> PaneOpened;
        public event TypedEventHandler<SplitView, SplitViewPaneClosingEventArgs> PaneClosing;
        public event TypedEventHandler<SplitView, object> PaneClosed;

        internal event DependencyPropertyChangedCallback IsPaneOpenChanged;
        internal event DependencyPropertyChangedCallback DisplayModeChanged;
        internal event DependencyPropertyChangedCallback CompactPaneLengthChanged;

        private bool IsLightDismissible()
        {
            var displayMode = DisplayMode;
            return displayMode != SplitViewDisplayMode.Inline &&
                displayMode != SplitViewDisplayMode.CompactInline;
        }

        private bool CanLightDismiss()
        {
            return IsPaneOpen && !_isPaneClosingByLightDismiss && IsLightDismissible();
        }

        public override void OnApplyTemplate()
        {
            UnregisterDisplayModeStateHandler();
            UnregisterLightDismissLayerHandler();
            TeardownOuterDismissLayer();

            _templateRoot = null;
            _paneRoot = null;
            _contentRoot = null;
            _lightDismissLayer = null;
            _paneClipRectangle = null;

            base.OnApplyTemplate();

            _templateRoot = this.GetTemplateRoot();
            _paneRoot = GetTemplateChild(PaneRootName) as FrameworkElement;
            _contentRoot = GetTemplateChild(ContentRootName) as FrameworkElement;
            _lightDismissLayer = GetTemplateChild(LightDismissLayerName) as FrameworkElement;
            _paneClipRectangle = GetTemplateChild(PaneClipRectangleName) as RectangleGeometry;
            _displayModeStates = GetTemplateChild(DisplayModeStatesName) as VisualStateGroup;

            RegisterDisplayModeStateHandler();
            RegisterLightDismissLayerHandler();

            UpdateTemplateSettings(false);
            UpdatePaneClipRectangle();
            UpdateVisualState(false);
            SetupOuterDismissLayer();

            Dispatcher.BeginInvoke(() =>
            {
                ReapplyDisplayModeState(false);
            }, DispatcherPriority.DataBind);
        }

        protected override Size MeasureOverride(Size constraint)
        {
            if (Pane != null)
            {
                Pane.Measure(constraint);
                _paneMeasuredLength = Pane.DesiredSize.Width;
            }
            else
            {
                _paneMeasuredLength = 0d;
            }

            var desiredSize = base.MeasureOverride(constraint);
            UpdateTemplateSettings(false);
            return desiredSize;
        }

        protected override Size ArrangeOverride(Size arrangeBounds)
        {
            var finalSize = base.ArrangeOverride(arrangeBounds);
            UpdatePaneClipRectangle(finalSize.Height);
            return finalSize;
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            if (!e.Handled && e.Key == Key.Escape && CanLightDismiss())
            {
                e.Handled = TryCloseLightDismissiblePane();
            }
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            SetupOuterDismissLayer();
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            TeardownOuterDismissLayer();
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdatePaneClipRectangle();

            if (!_hasCompletedInitialSize)
            {
                if (e.NewSize.Width != 0 || e.NewSize.Height != 0)
                {
                    Dispatcher.BeginInvoke(() =>
                    {
                        _hasCompletedInitialSize = true;
                    }, DispatcherPriority.Loaded);
                }

                return;
            }

            if ((e.PreviousSize.Width != 0 || e.PreviousSize.Height != 0) && CanLightDismiss())
            {
                TryCloseLightDismissiblePane();
            }
        }

        private void OnIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (IsVisible)
            {
                SetupOuterDismissLayer();
            }
            else
            {
                TeardownOuterDismissLayer();
            }
        }

        private void RegisterDisplayModeStateHandler()
        {
            if (_displayModeStates != null)
            {
                _displayModeStates.CurrentStateChanging += OnDisplayModeStatesCurrentStateChanging;
                _displayModeStates.CurrentStateChanged += OnDisplayModeStatesCurrentStateChanged;
                AnimationHelper.DeferTransitions(_displayModeStates);
            }
        }

        private void UnregisterDisplayModeStateHandler()
        {
            if (_displayModeStates != null)
            {
                _displayModeStates.CurrentStateChanging -= OnDisplayModeStatesCurrentStateChanging;
                _displayModeStates.CurrentStateChanged -= OnDisplayModeStatesCurrentStateChanged;
                _displayModeStates = null;
            }
        }

        private void RegisterLightDismissLayerHandler()
        {
            if (_lightDismissLayer != null)
            {
                _lightDismissLayer.MouseLeftButtonUp += OnLightDismissLayerMouseLeftButtonUp;
            }
        }

        private void UnregisterLightDismissLayerHandler()
        {
            if (_lightDismissLayer != null)
            {
                _lightDismissLayer.MouseLeftButtonUp -= OnLightDismissLayerMouseLeftButtonUp;
            }
        }

        private void OnDisplayModeStatesCurrentStateChanging(object sender, VisualStateChangedEventArgs e)
        {
            _isDisplayModeStateChanging = true;
        }

        private void OnDisplayModeStatesCurrentStateChanged(object sender, VisualStateChangedEventArgs e)
        {
            _isDisplayModeStateChanging = false;

            if (_isPaneOpening)
            {
                _isPaneOpening = false;
                PaneOpened?.Invoke(this, null);
            }
            else if (_isPaneClosing)
            {
                _isPaneClosing = false;
                OnPaneClosed();
            }
        }

        private void OnOuterDismissElementPreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (_paneRoot != null)
            {
                var pos = e.GetPosition(_paneRoot);
                if ((pos.X >= 0) && (pos.X <= _paneRoot.ActualWidth) && (pos.Y >= 0) && (pos.Y <= _paneRoot.ActualHeight))
                {
                    return;
                }
            }

            if (e.OriginalSource is UIElement originalElement &&
                TitleBarControl.GetInsideTitleBar(originalElement))
            {
                return;
            }

            if (CanLightDismiss())
            {
                TryCloseLightDismissiblePane();
                e.Handled = true;
            }
        }

        private void OnLightDismissLayerMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (CanLightDismiss())
            {
                TryCloseLightDismissiblePane();
                e.Handled = true;
            }
        }

        private void OpenPane()
        {
            if (_isPaneOpening)
            {
                return;
            }

            _isPaneClosingByLightDismiss = false;

            PaneOpening?.Invoke(this, null);
            OnPaneOpening();

            if (UpdateDisplayModeState())
            {
                _isPaneOpening = true;
            }
            else
            {
                PaneOpened?.Invoke(this, null);
            }

            SetupOuterDismissLayer();
        }

        private void ClosePane()
        {
            if (_isPaneClosing)
            {
                return;
            }

            OnPaneClosing();

            if (UpdateDisplayModeState())
            {
                _isPaneClosing = true;
            }
            else
            {
                OnPaneClosed();
            }

            TeardownOuterDismissLayer();
        }

        private bool TryCloseLightDismissiblePane()
        {
            if (!CanLightDismiss())
            {
                return false;
            }

            var args = new SplitViewPaneClosingEventArgs();
            PaneClosing?.Invoke(this, args);

            if (args.Cancel)
            {
                OnCancelClosing();
                return false;
            }

            _isPaneClosingByLightDismiss = true;
            SetCurrentValue(IsPaneOpenProperty, false);
            return true;
        }

        private void OnPaneOpening()
        {
            if (IsLightDismissible())
            {
                SetFocusToPane();
            }
        }

        private void OnPaneClosing()
        {
            if (!_isPaneClosingByLightDismiss)
            {
                PaneClosing?.Invoke(this, new SplitViewPaneClosingEventArgs());
            }

            if (IsLightDismissible())
            {
                RestoreSavedFocusElement();
            }
        }

        private void OnPaneClosed()
        {
            _isPaneClosingByLightDismiss = false;
            PaneClosed?.Invoke(this, null);
        }

        private void OnCancelClosing()
        {
            _isPaneClosingByLightDismiss = false;
        }

        private void UpdateTemplateSettings(bool reapplyDisplayModeState = true)
        {
            var compactPaneLength = CompactPaneLength;
            var openPaneLength = GetOpenPaneLength();
            var openPaneLengthMinusCompactLength = openPaneLength - compactPaneLength;

            var templateSettings = TemplateSettings;
            templateSettings.CompactPaneGridLength = new GridLength(compactPaneLength);
            templateSettings.NegativeOpenPaneLength = -openPaneLength;
            templateSettings.NegativeOpenPaneLengthMinusCompactLength = -openPaneLengthMinusCompactLength;
            templateSettings.OpenPaneGridLength = new GridLength(openPaneLength);
            templateSettings.OpenPaneLength = openPaneLength;
            templateSettings.OpenPaneLengthMinusCompactLength = openPaneLengthMinusCompactLength;

            if (reapplyDisplayModeState)
            {
                ReapplyDisplayModeState();
            }
        }

        private double GetOpenPaneLength()
        {
            var openPaneLength = OpenPaneLength;
            return double.IsNaN(openPaneLength) ? _paneMeasuredLength : openPaneLength;
        }

        private void UpdatePaneClipRectangle()
        {
            UpdatePaneClipRectangle(ActualHeight);
        }

        private void UpdatePaneClipRectangle(double height)
        {
            if (_paneClipRectangle != null)
            {
                _paneClipRectangle.Rect = new Rect(0, 0, GetOpenPaneLength(), height);
            }
        }

        private bool UpdateDisplayModeState(bool useTransitions = true)
        {
            var displayMode = (int)DisplayMode;
            var panePlacement = (int)PanePlacement;

            Debug.Assert(displayMode >= 0 && displayMode < 4);
            Debug.Assert(panePlacement >= 0 && panePlacement < 2);

            if (displayMode < 0 || displayMode >= 4 || panePlacement < 0 || panePlacement >= 2)
            {
                return false;
            }

            var stateName = s_visualStateTable[displayMode, panePlacement, IsPaneOpen ? 1 : 0];
            return VisualStateManager.GoToState(this, stateName, useTransitions);
        }

        private void UpdateOverlayVisibilityState(bool useTransitions = true)
        {
            var isOverlayVisible = LightDismissOverlayMode == LightDismissOverlayMode.On;
            VisualStateManager.GoToState(this, isOverlayVisible ? "OverlayVisible" : "OverlayNotVisible", useTransitions);
        }

        private void UpdateVisualState(bool useTransitions = true)
        {
            UpdateDisplayModeState(useTransitions);
            UpdateOverlayVisibilityState(useTransitions);
        }

        private void ReapplyDisplayModeState(bool waitForDataBinding = true)
        {
            if (!_isDisplayModeStateChanging)
            {
                var storyboard = _displayModeStates?.CurrentState?.Storyboard;
                if (storyboard != null && _templateRoot != null)
                {
                    if (!storyboard.CanFreeze)
                    {
                        if (waitForDataBinding)
                        {
                            DispatcherHelper.DoEvents(DispatcherPriority.DataBind);
                        }

                        storyboard.Begin(_templateRoot, true);
                    }
                }
            }
        }

        private void SetupOuterDismissLayer()
        {
            if (_isOuterDismissLayerActive || !IsVisible || !CanLightDismiss())
            {
                return;
            }

            _window = Window.GetWindow(this);
            if (_window != null)
            {
                _window.PreviewMouseDown += OnOuterDismissElementPreviewMouseDown;
                _isOuterDismissLayerActive = true;
            }
        }

        private void TeardownOuterDismissLayer()
        {
            if (_window != null)
            {
                _window.PreviewMouseDown -= OnOuterDismissElementPreviewMouseDown;
                _window = null;
            }

            _isOuterDismissLayerActive = false;
        }

        private void SetFocusToPane()
        {
            _previousFocusedElement = Keyboard.FocusedElement;

            if (_paneRoot != null)
            {
                if (TryFocusFirstElement(_paneRoot))
                {
                    return;
                }

                _paneRoot.Focus();
            }
        }

        private void RestoreSavedFocusElement()
        {
            if (_previousFocusedElement is UIElement element && element.IsVisible && element.IsEnabled && element.Focusable)
            {
                element.Focus();
            }
            else if (_contentRoot != null)
            {
                TryFocusFirstElement(_contentRoot);
            }

            _previousFocusedElement = null;
        }

        private static bool TryFocusFirstElement(DependencyObject root)
        {
            foreach (var child in EnumerateVisualDescendants(root))
            {
                if (child is UIElement element && element.Focusable && element.IsVisible && element.IsEnabled)
                {
                    return element.Focus();
                }
            }

            return false;
        }

        private static IEnumerable<DependencyObject> EnumerateVisualDescendants(DependencyObject root)
        {
            if (!(root is Visual) && !(root is System.Windows.Media.Media3D.Visual3D))
            {
                yield break;
            }

            var childCount = VisualTreeHelper.GetChildrenCount(root);
            for (var i = 0; i < childCount; i++)
            {
                var child = VisualTreeHelper.GetChild(root, i);
                yield return child;

                foreach (var descendant in EnumerateVisualDescendants(child))
                {
                    yield return descendant;
                }
            }
        }

        private FrameworkElement _templateRoot;
        private VisualStateGroup _displayModeStates;
        private FrameworkElement _paneRoot;
        private FrameworkElement _contentRoot;
        private FrameworkElement _lightDismissLayer;
        private RectangleGeometry _paneClipRectangle;

        private Window _window;
        private IInputElement _previousFocusedElement;

        private bool _isOuterDismissLayerActive;
        private bool _hasCompletedInitialSize;
        private bool _isPaneOpening;
        private bool _isPaneClosing;
        private bool _isPaneClosingByLightDismiss;
        private bool _isDisplayModeStateChanging;
        private double _paneMeasuredLength;

        private const string PaneRootName = "PaneRoot";
        private const string ContentRootName = "ContentRoot";
        private const string LightDismissLayerName = "LightDismissLayer";
        private const string DisplayModeStatesName = "DisplayModeStates";
        private const string PaneClipRectangleName = "PaneClipRectangle";
    }
}
