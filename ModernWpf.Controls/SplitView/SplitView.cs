using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;

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

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _templateRoot = this.GetTemplateRoot();
            _paneRoot = GetTemplateChild("PaneRoot") as FrameworkElement;
            _displayModeStates = GetTemplateChild("DisplayModeStates") as VisualStateGroup;
            _paneClipRectangle = GetTemplateChild("PaneClipRectangle") as RectangleGeometry;

            if (_displayModeStates != null)
            {
                _displayModeStates.CurrentStateChanging += OnDisplayModeStatesCurrentStateChanging;
                _displayModeStates.CurrentStateChanged += OnDisplayModeStatesCurrentStateChanged;
            }

            UpdateTemplateSettings();
            UpdatePaneClipRectangle();
            UpdateVisualState(false);
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdatePaneClipRectangle();
        }

        private void OnIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            UpdateLightDismiss();
        }

        private void OnDisplayModeStatesCurrentStateChanging(object sender, VisualStateChangedEventArgs e)
        {
            _displayModeStatesChanging = true;
        }

        private void OnDisplayModeStatesCurrentStateChanged(object sender, VisualStateChangedEventArgs e)
        {
            _displayModeStatesChanging = false;

            if (_paneOpening)
            {
                _paneOpening = false;
                PaneOpened?.Invoke(this, null);
            }
            else if (_paneClosing)
            {
                _paneClosing = false;
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

            if (IsPaneOpen)
            {
                e.Handled = true;
                ClosePane();
            }
        }

        private void OpenPane()
        {
            if (_paneOpening)
                return;

            PaneOpening?.Invoke(this, null);

            if (UpdateDisplayModeState())
            {
                _paneOpening = true;
            }
            else
            {
                PaneOpened?.Invoke(this, null);
            }
        }

        private void ClosePane()
        {
            if (_paneClosing)
                return;

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

            _paneClosing = true;

            if (IsPaneOpen)
            {
                SetCurrentValue(IsPaneOpenProperty, false);
            }

            if (!UpdateDisplayModeState())
            {
                _paneClosing = false;
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

            RefreshDisplayModeStates();
        }

        private void UpdatePaneClipRectangle()
        {
            if (_paneClipRectangle != null)
            {
                _paneClipRectangle.Rect = new Rect(0, 0, OpenPaneLength, ActualHeight);
            }
        }

        private void UpdateLightDismiss()
        {
            bool value = IsVisible && IsPaneOpen && IsLightDismissible;

            if (_lightDismiss != value)
            {
                _lightDismiss = value;
                if (_lightDismiss)
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

        private void RefreshDisplayModeStates()
        {
            if (!_displayModeStatesChanging)
            {
                var storyboard = _displayModeStates?.CurrentState?.Storyboard;
                if (storyboard != null && _templateRoot != null)
                {
                    storyboard.Remove(_templateRoot);
                    storyboard.Begin(_templateRoot, HandoffBehavior.SnapshotAndReplace, true);
                }
            }
        }

        private FrameworkElement _templateRoot;
        private VisualStateGroup _displayModeStates;
        private FrameworkElement _paneRoot;
        private RectangleGeometry _paneClipRectangle;

        private Window _window;

        private bool _lightDismiss;
        private bool _paneOpening;
        private bool _paneClosing;
        private bool _displayModeStatesChanging;
    }
}
