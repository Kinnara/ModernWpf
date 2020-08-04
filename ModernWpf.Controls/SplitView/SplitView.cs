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
    public partial class SplitView : Control
    {
        static SplitView()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(SplitView), new FrameworkPropertyMetadata(typeof(SplitView)));
        }

        public SplitView()
        {
            TemplateSettings = new SplitViewTemplateSettings();

            SizeChanged += OnSizeChanged;
            IsVisibleChanged += OnIsVisibleChanged;
        }

        private bool IsLightDismissible
        {
            get
            {
                var displayMode = DisplayMode;
                return displayMode == SplitViewDisplayMode.Overlay || displayMode == SplitViewDisplayMode.CompactOverlay;
            }
        }

        public event TypedEventHandler<SplitView, object> PaneOpening;
        public event TypedEventHandler<SplitView, object> PaneOpened;
        public event TypedEventHandler<SplitView, SplitViewPaneClosingEventArgs> PaneClosing;
        public event TypedEventHandler<SplitView, object> PaneClosed;

        internal event DependencyPropertyChangedCallback IsPaneOpenChanged;
        internal event DependencyPropertyChangedCallback DisplayModeChanged;
        internal event DependencyPropertyChangedCallback CompactPaneLengthChanged;

        public override void OnApplyTemplate()
        {
            if (_displayModeStates != null)
            {
                _displayModeStates.CurrentStateChanging -= OnDisplayModeStatesCurrentStateChanging;
                _displayModeStates.CurrentStateChanged -= OnDisplayModeStatesCurrentStateChanged;
            }

            base.OnApplyTemplate();

            _templateRoot = this.GetTemplateRoot();
            _paneRoot = GetTemplateChild(PaneRootName) as FrameworkElement;
            _displayModeStates = GetTemplateChild(DisplayModeStatesName) as VisualStateGroup;
            _paneClipRectangle = GetTemplateChild(PaneClipRectangleName) as RectangleGeometry;

            if (_displayModeStates != null)
            {
                _displayModeStates.CurrentStateChanging += OnDisplayModeStatesCurrentStateChanging;
                _displayModeStates.CurrentStateChanged += OnDisplayModeStatesCurrentStateChanged;
                AnimationHelper.DeferTransitions(_displayModeStates);
            }

            UpdateTemplateSettings();
            UpdatePaneClipRectangle();
            UpdateVisualState(false);

            Dispatcher.BeginInvoke(() =>
            {
                ReapplyDisplayModeState(false);
            }, DispatcherPriority.DataBind);
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdatePaneClipRectangle();
        }

        private void OnIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            UpdateIsLightDismissActive();
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
                PaneClosed?.Invoke(this, null);
            }
        }

        private void OnWindowPreviewMouseDown(object sender, MouseButtonEventArgs e)
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

            if (IsPaneOpen)
            {
                e.Handled = true;
                ClosePane();
            }
        }

        private void OpenPane()
        {
            if (_isPaneOpening)
            {
                return;
            }

            PaneOpening?.Invoke(this, null);

            if (UpdateDisplayModeState())
            {
                _isPaneOpening = true;
            }
            else
            {
                PaneOpened?.Invoke(this, null);
            }
        }

        private void ClosePane()
        {
            if (_isPaneClosing)
            {
                return;
            }

            var paneClosing = PaneClosing;
            if (paneClosing != null)
            {
                var args = new SplitViewPaneClosingEventArgs();
                paneClosing(this, args);

                if (args.Cancel && IsPaneOpen)
                {
                    return;
                }
            }

            _isPaneClosing = true;

            if (IsPaneOpen)
            {
                SetCurrentValue(IsPaneOpenProperty, false);
            }

            if (!UpdateDisplayModeState())
            {
                _isPaneClosing = false;
                PaneClosed?.Invoke(this, null);
            }
        }

        private void UpdateTemplateSettings()
        {
            var compactPaneLength = CompactPaneLength;
            var openPaneLength = OpenPaneLength;
            var openPaneLengthMinusCompactLength = openPaneLength - compactPaneLength;

            var templateSettings = TemplateSettings;
            templateSettings.CompactPaneGridLength = new GridLength(compactPaneLength);
            templateSettings.NegativeOpenPaneLength = -openPaneLength;
            templateSettings.NegativeOpenPaneLengthMinusCompactLength = -openPaneLengthMinusCompactLength;
            templateSettings.OpenPaneGridLength = new GridLength(openPaneLength);
            templateSettings.OpenPaneLength = openPaneLength;
            templateSettings.OpenPaneLengthMinusCompactLength = openPaneLengthMinusCompactLength;

            ReapplyDisplayModeState();
        }

        private void UpdatePaneClipRectangle()
        {
            if (_paneClipRectangle != null)
            {
                _paneClipRectangle.Rect = new Rect(0, 0, OpenPaneLength, ActualHeight);
            }
        }

        private void UpdateIsLightDismissActive()
        {
            bool value = IsVisible && IsPaneOpen && IsLightDismissible;

            if (_isLightDismissActive != value)
            {
                _isLightDismissActive = value;

                if (_isLightDismissActive)
                {
                    _window = Window.GetWindow(this);
                    if (_window != null)
                    {
                        _window.PreviewMouseDown += OnWindowPreviewMouseDown;
                    }
                }
                else
                {
                    if (_window != null)
                    {
                        _window.PreviewMouseDown -= OnWindowPreviewMouseDown;
                        _window = null;
                    }
                }
            }
        }

        private bool UpdateDisplayModeState(bool useTransitions = true)
        {
            string stateName = null;
            var displayMode = DisplayMode;

            if (IsPaneOpen)
            {
                if (displayMode == SplitViewDisplayMode.Overlay)
                {
                    if (PanePlacement == SplitViewPanePlacement.Left)
                    {
                        stateName = "OpenOverlayLeft";
                    }
                    else
                    {
                        stateName = "OpenOverlayRight";
                    }
                }
                else if (displayMode == SplitViewDisplayMode.Inline || displayMode == SplitViewDisplayMode.CompactInline)
                {
                    if (PanePlacement == SplitViewPanePlacement.Left)
                    {
                        stateName = "OpenInlineLeft";
                    }
                    else
                    {
                        stateName = "OpenInlineRight";
                    }
                }
                else if (displayMode == SplitViewDisplayMode.CompactOverlay)
                {
                    if (PanePlacement == SplitViewPanePlacement.Left)
                    {
                        stateName = "OpenCompactOverlayLeft";
                    }
                    else
                    {
                        stateName = "OpenCompactOverlayRight";
                    }
                }
            }
            else
            {
                if (displayMode == SplitViewDisplayMode.CompactOverlay || displayMode == SplitViewDisplayMode.CompactInline)
                {
                    if (PanePlacement == SplitViewPanePlacement.Left)
                    {
                        stateName = "ClosedCompactLeft";
                    }
                    else
                    {
                        stateName = "ClosedCompactRight";
                    }
                }
                else
                {
                    stateName = "Closed";
                }
            }

            Debug.Assert(stateName != null);
            return VisualStateManager.GoToState(this, stateName, useTransitions);
        }

        private void UpdateOverlayVisibilityState(bool useTransitions = true)
        {
            string stateName;

            if (IsPaneOpen && IsLightDismissible && LightDismissOverlayMode == LightDismissOverlayMode.On)
            {
                stateName = "OverlayVisible";
            }
            else
            {
                stateName = "OverlayNotVisible";
            }

            VisualStateManager.GoToState(this, stateName, useTransitions);
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
                            // Wait for data binding to update the storyboard
                            DispatcherHelper.DoEvents(DispatcherPriority.DataBind);
                        }

                        storyboard.Begin(_templateRoot, true);
                    }
                }
            }
        }

        private FrameworkElement _templateRoot;
        private VisualStateGroup _displayModeStates;
        private FrameworkElement _paneRoot;
        private RectangleGeometry _paneClipRectangle;

        private Window _window;

        private bool _isLightDismissActive;
        private bool _isPaneOpening;
        private bool _isPaneClosing;
        private bool _isDisplayModeStateChanging;

        private const string PaneRootName = "PaneRoot";
        private const string DisplayModeStatesName = "DisplayModeStates";
        private const string PaneClipRectangleName = "PaneClipRectangle";
    }
}
