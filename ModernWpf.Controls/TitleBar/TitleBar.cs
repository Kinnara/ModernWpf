// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Shell;
using System.Windows.Threading;
using ModernWpf.Automation.Peers;
using static ModernWpf.ResourceAccessor;

namespace ModernWpf.Controls
{
    [ContentProperty(nameof(Content))]
    [TemplatePart(Name = BackButtonName, Type = typeof(Button))]
    [TemplatePart(Name = PaneToggleButtonName, Type = typeof(Button))]
    [TemplatePart(Name = LayoutRootName, Type = typeof(Grid))]
    [TemplatePart(Name = ContentPresenterName, Type = typeof(ContentPresenter))]
    [TemplatePart(Name = ContentPresenterGridName, Type = typeof(Grid))]
    public class TitleBar : Control
    {
        private const string BackButtonName = "PART_BackButton";
        private const string PaneToggleButtonName = "PART_PaneToggleButton";
        private const string LayoutRootName = "PART_LayoutRoot";
        private const string ContentPresenterName = "PART_ContentPresenter";
        private const string ContentPresenterGridName = "PART_ContentPresenterGrid";
        private const string LeftHeaderPresenterName = "PART_LeftHeaderPresenter";
        private const string RightHeaderPresenterName = "PART_RightHeaderPresenter";

        private static readonly ResourceAccessor ResourceAccessor = new ResourceAccessor(typeof(TitleBar));

        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register(
                nameof(Title),
                typeof(string),
                typeof(TitleBar),
                new PropertyMetadata(string.Empty, OnTitlePropertyChanged));

        public static readonly DependencyProperty SubtitleProperty =
            DependencyProperty.Register(
                nameof(Subtitle),
                typeof(string),
                typeof(TitleBar),
                new PropertyMetadata(string.Empty, OnVisualPropertyChanged));

        public static readonly DependencyProperty IconSourceProperty =
            DependencyProperty.Register(
                nameof(IconSource),
                typeof(IconSource),
                typeof(TitleBar),
                new PropertyMetadata(null, OnIconSourcePropertyChanged));

        public static readonly DependencyProperty LeftHeaderProperty =
            DependencyProperty.Register(
                nameof(LeftHeader),
                typeof(UIElement),
                typeof(TitleBar),
                new PropertyMetadata(null, OnLayoutPropertyChanged));

        public static readonly DependencyProperty ContentProperty =
            DependencyProperty.Register(
                nameof(Content),
                typeof(UIElement),
                typeof(TitleBar),
                new PropertyMetadata(null, OnContentPropertyChanged));

        public static readonly DependencyProperty RightHeaderProperty =
            DependencyProperty.Register(
                nameof(RightHeader),
                typeof(UIElement),
                typeof(TitleBar),
                new PropertyMetadata(null, OnLayoutPropertyChanged));

        public static readonly DependencyProperty IsBackButtonVisibleProperty =
            DependencyProperty.Register(
                nameof(IsBackButtonVisible),
                typeof(bool),
                typeof(TitleBar),
                new PropertyMetadata(false, OnVisualPropertyChanged));

        public static readonly DependencyProperty IsBackButtonEnabledProperty =
            DependencyProperty.Register(
                nameof(IsBackButtonEnabled),
                typeof(bool),
                typeof(TitleBar),
                new PropertyMetadata(true, OnVisualPropertyChanged));

        public static readonly DependencyProperty IsPaneToggleButtonVisibleProperty =
            DependencyProperty.Register(
                nameof(IsPaneToggleButtonVisible),
                typeof(bool),
                typeof(TitleBar),
                new PropertyMetadata(false, OnVisualPropertyChanged));

        public static readonly DependencyProperty AutoRefreshDragRegionsProperty =
            DependencyProperty.Register(
                nameof(AutoRefreshDragRegions),
                typeof(bool),
                typeof(TitleBar),
                new PropertyMetadata(false, OnAutoRefreshDragRegionsPropertyChanged));

        public static readonly DependencyProperty IsDragRegionProperty =
            DependencyProperty.RegisterAttached(
                "IsDragRegion",
                typeof(bool?),
                typeof(TitleBar),
                new FrameworkPropertyMetadata(null, OnIsDragRegionPropertyChanged));

        private static readonly DependencyPropertyKey TemplateSettingsPropertyKey =
            DependencyProperty.RegisterReadOnly(
                nameof(TemplateSettings),
                typeof(TitleBarTemplateSettings),
                typeof(TitleBar),
                new PropertyMetadata(null));

        public static readonly DependencyProperty TemplateSettingsProperty =
            TemplateSettingsPropertyKey.DependencyProperty;

        private Button _backButton;
        private Button _paneToggleButton;
        private Grid _layoutRoot;
        private Grid _contentPresenterGrid;
        private ContentPresenter _contentPresenter;
        private ContentPresenter _leftHeaderPresenter;
        private ContentPresenter _rightHeaderPresenter;
        private FrameworkElement _observedContent;
        private Window _window;
        private string _defaultWindowTitle;
        private string _lastAppliedWindowTitle;
        private double _compactModeThresholdWidth;
        private bool _isCompact;

        static TitleBar()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(TitleBar),
                new FrameworkPropertyMetadata(typeof(TitleBar)));
        }

        public TitleBar()
        {
            SetValue(TemplateSettingsPropertyKey, new TitleBarTemplateSettings());
            WindowChrome.SetIsHitTestVisibleInChrome(this, true);
            Loaded += OnLoaded;
            Unloaded += OnUnloaded;
            SizeChanged += OnSizeChanged;
        }

        public string Title
        {
            get => (string)GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }

        public string Subtitle
        {
            get => (string)GetValue(SubtitleProperty);
            set => SetValue(SubtitleProperty, value);
        }

        public IconSource IconSource
        {
            get => (IconSource)GetValue(IconSourceProperty);
            set => SetValue(IconSourceProperty, value);
        }

        public UIElement LeftHeader
        {
            get => (UIElement)GetValue(LeftHeaderProperty);
            set => SetValue(LeftHeaderProperty, value);
        }

        public UIElement Content
        {
            get => (UIElement)GetValue(ContentProperty);
            set => SetValue(ContentProperty, value);
        }

        public UIElement RightHeader
        {
            get => (UIElement)GetValue(RightHeaderProperty);
            set => SetValue(RightHeaderProperty, value);
        }

        public bool IsBackButtonVisible
        {
            get => (bool)GetValue(IsBackButtonVisibleProperty);
            set => SetValue(IsBackButtonVisibleProperty, value);
        }

        public bool IsBackButtonEnabled
        {
            get => (bool)GetValue(IsBackButtonEnabledProperty);
            set => SetValue(IsBackButtonEnabledProperty, value);
        }

        public bool IsPaneToggleButtonVisible
        {
            get => (bool)GetValue(IsPaneToggleButtonVisibleProperty);
            set => SetValue(IsPaneToggleButtonVisibleProperty, value);
        }

        public bool AutoRefreshDragRegions
        {
            get => (bool)GetValue(AutoRefreshDragRegionsProperty);
            set => SetValue(AutoRefreshDragRegionsProperty, value);
        }

        public TitleBarTemplateSettings TemplateSettings =>
            (TitleBarTemplateSettings)GetValue(TemplateSettingsProperty);

        public event TypedEventHandler<TitleBar, object> BackRequested;

        public event TypedEventHandler<TitleBar, object> PaneToggleRequested;

        public static bool? GetIsDragRegion(UIElement element)
        {
            if (element == null)
            {
                throw new ArgumentNullException(nameof(element));
            }

            return (bool?)element.GetValue(IsDragRegionProperty);
        }

        public static void SetIsDragRegion(UIElement element, bool? value)
        {
            if (element == null)
            {
                throw new ArgumentNullException(nameof(element));
            }

            if (value.HasValue)
            {
                element.SetValue(IsDragRegionProperty, value);
            }
            else
            {
                element.ClearValue(IsDragRegionProperty);
            }
        }

        public void RecomputeDragRegions()
        {
            UpdateLayout();
            InvalidateVisual();
        }

        public override void OnApplyTemplate()
        {
            UnhookTemplateParts();
            base.OnApplyTemplate();

            _backButton = GetTemplateChild(BackButtonName) as Button;
            _paneToggleButton = GetTemplateChild(PaneToggleButtonName) as Button;
            _layoutRoot = GetTemplateChild(LayoutRootName) as Grid;
            _contentPresenter = GetTemplateChild(ContentPresenterName) as ContentPresenter;
            _contentPresenterGrid = GetTemplateChild(ContentPresenterGridName) as Grid;
            _leftHeaderPresenter = GetTemplateChild(LeftHeaderPresenterName) as ContentPresenter;
            _rightHeaderPresenter = GetTemplateChild(RightHeaderPresenterName) as ContentPresenter;

            if (_backButton != null)
            {
                _backButton.Click += OnBackButtonClick;
            }

            if (_paneToggleButton != null)
            {
                _paneToggleButton.Click += OnPaneToggleButtonClick;
            }

            InitializeButtonAccessibility();

            UpdateIcon();
            UpdateAllStates();
            Dispatcher.BeginInvoke(
                DispatcherPriority.Loaded,
                new Action(UpdateDisplayMode));
        }

        protected override AutomationPeer OnCreateAutomationPeer()
        {
            return new TitleBarAutomationPeer(this);
        }

        protected override void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnPreviewMouseLeftButtonDown(e);

            if (e.Handled ||
                e.ChangedButton != MouseButton.Left ||
                e.ButtonState != MouseButtonState.Pressed ||
                !IsDragTarget(e.OriginalSource as DependencyObject))
            {
                return;
            }

            var window = Window.GetWindow(this);
            if (window == null)
            {
                return;
            }

            if (e.ClickCount == 2 && window.ResizeMode != ResizeMode.NoResize)
            {
                window.WindowState = window.WindowState == WindowState.Maximized
                    ? WindowState.Normal
                    : WindowState.Maximized;
                e.Handled = true;
                return;
            }

            try
            {
                window.DragMove();
                e.Handled = true;
            }
            catch (InvalidOperationException)
            {
                // DragMove requires a live primary-button press. Synthetic input and
                // design surfaces can reach this path without one.
            }
        }

        private static void OnTitlePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var titleBar = (TitleBar)d;
            titleBar.ApplyWindowTitle(e.OldValue as string);
            titleBar.UpdateAllStates();
        }

        private static void OnVisualPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((TitleBar)d).UpdateAllStates();
        }

        private static void OnLayoutPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var titleBar = (TitleBar)d;
            titleBar._compactModeThresholdWidth = 0;
            titleBar._isCompact = false;
            titleBar.UpdateAllStates();
            titleBar.RecomputeDragRegions();
        }

        private static void OnContentPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var titleBar = (TitleBar)d;
            titleBar.UpdateObservedContent(e.OldValue as FrameworkElement, e.NewValue as FrameworkElement);
            OnLayoutPropertyChanged(d, e);
        }

        private static void OnIconSourcePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var titleBar = (TitleBar)d;
            titleBar.UpdateIcon();
            titleBar.UpdateAllStates();
            titleBar.RecomputeDragRegions();
        }

        private static void OnAutoRefreshDragRegionsPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var titleBar = (TitleBar)d;
            titleBar.UpdateObservedContent(titleBar._observedContent, titleBar.Content as FrameworkElement);
            titleBar.RecomputeDragRegions();
        }

        private static void OnIsDragRegionPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var current = d;
            while (current != null)
            {
                if (current is TitleBar titleBar)
                {
                    titleBar.RecomputeDragRegions();
                    return;
                }

                current = GetParent(current);
            }
        }

        private static DependencyObject GetParent(DependencyObject element)
        {
            if (element is Visual || element is System.Windows.Media.Media3D.Visual3D)
            {
                return VisualTreeHelper.GetParent(element);
            }

            return LogicalTreeHelper.GetParent(element);
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            AttachWindow(Window.GetWindow(this));
            UpdateObservedContent(null, Content as FrameworkElement);
            UpdateAllStates();
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            UpdateObservedContent(_observedContent, null);
            DetachWindow(true);
        }

        private void AttachWindow(Window window)
        {
            if (ReferenceEquals(_window, window))
            {
                return;
            }

            DetachWindow(true);
            _window = window;
            if (_window == null)
            {
                return;
            }

            _defaultWindowTitle = _window.Title;
            _window.Activated += OnWindowActivationChanged;
            _window.Deactivated += OnWindowActivationChanged;
            ApplyWindowTitle(null);
        }

        private void DetachWindow(bool restoreTitle)
        {
            if (_window == null)
            {
                return;
            }

            _window.Activated -= OnWindowActivationChanged;
            _window.Deactivated -= OnWindowActivationChanged;
            if (restoreTitle &&
                _lastAppliedWindowTitle != null &&
                string.Equals(_window.Title, _lastAppliedWindowTitle, StringComparison.Ordinal))
            {
                _window.Title = _defaultWindowTitle ?? string.Empty;
            }

            _window = null;
            _defaultWindowTitle = null;
            _lastAppliedWindowTitle = null;
        }

        private void ApplyWindowTitle(string oldTitle)
        {
            if (_window == null)
            {
                return;
            }

            if (string.IsNullOrEmpty(Title))
            {
                if (!string.IsNullOrEmpty(oldTitle) &&
                    string.Equals(_window.Title, oldTitle, StringComparison.Ordinal))
                {
                    _window.Title = _defaultWindowTitle ?? string.Empty;
                }

                _lastAppliedWindowTitle = null;
            }
            else
            {
                _window.Title = Title;
                _lastAppliedWindowTitle = Title;
            }
        }

        private void OnWindowActivationChanged(object sender, EventArgs e)
        {
            UpdateAllStates();
        }

        private void OnBackButtonClick(object sender, RoutedEventArgs e)
        {
            BackRequested?.Invoke(this, null);
        }

        private void OnPaneToggleButtonClick(object sender, RoutedEventArgs e)
        {
            PaneToggleRequested?.Invoke(this, null);
        }

        private void InitializeButtonAccessibility()
        {
            if (_backButton != null)
            {
                if (string.IsNullOrEmpty(AutomationProperties.GetName(_backButton)))
                {
                    AutomationProperties.SetName(
                        _backButton,
                        ResourceAccessor.GetLocalizedStringResource(SR_NavigationBackButtonName));
                }

                _backButton.ToolTip = new ToolTip
                {
                    Content = ResourceAccessor.GetLocalizedStringResource(SR_NavigationBackButtonToolTip)
                };
            }

            if (_paneToggleButton != null)
            {
                if (string.IsNullOrEmpty(AutomationProperties.GetName(_paneToggleButton)))
                {
                    AutomationProperties.SetName(
                        _paneToggleButton,
                        ResourceAccessor.GetLocalizedStringResource(SR_NavigationButtonToggleName));
                }

                _paneToggleButton.ToolTip = new ToolTip
                {
                    Content = AutomationProperties.GetName(_paneToggleButton)
                };
            }
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateDisplayMode();
        }

        private void OnObservedContentLayoutUpdated(object sender, EventArgs e)
        {
            RecomputeDragRegions();
        }

        private void UpdateObservedContent(FrameworkElement oldContent, FrameworkElement newContent)
        {
            if (_observedContent != null)
            {
                _observedContent.LayoutUpdated -= OnObservedContentLayoutUpdated;
            }

            _observedContent = newContent;
            if (AutoRefreshDragRegions && _observedContent != null)
            {
                _observedContent.LayoutUpdated += OnObservedContentLayoutUpdated;
            }
        }

        private void UpdateIcon()
        {
            TemplateSettings.IconElement = IconSource?.CreateIconElement();
        }

        private void UpdateAllStates()
        {
            var isActive = _window == null || _window.IsActive;

            GoToState(
                !IsBackButtonVisible
                    ? "BackButtonCollapsed"
                    : isActive ? "BackButtonVisible" : "BackButtonDeactivated");
            GoToState(
                !IsPaneToggleButtonVisible
                    ? "PaneToggleButtonCollapsed"
                    : isActive ? "PaneToggleButtonVisible" : "PaneToggleButtonDeactivated");
            GoToState(
                LeftHeader == null
                    ? "LeftHeaderCollapsed"
                    : isActive ? "LeftHeaderVisible" : "LeftHeaderDeactivated");
            GoToState(
                IconSource == null
                    ? "IconCollapsed"
                    : isActive ? "IconVisible" : "IconDeactivated");
            GoToState(
                string.IsNullOrEmpty(Title)
                    ? "TitleTextCollapsed"
                    : isActive ? "TitleTextVisible" : "TitleTextDeactivated");
            GoToState(
                string.IsNullOrEmpty(Subtitle)
                    ? "SubtitleTextCollapsed"
                    : isActive ? "SubtitleTextVisible" : "SubtitleTextDeactivated");
            GoToState(
                Content == null
                    ? "ContentCollapsed"
                    : isActive ? "ContentVisible" : "ContentDeactivated");
            GoToState(
                RightHeader == null
                    ? "RightHeaderCollapsed"
                    : isActive ? "RightHeaderVisible" : "RightHeaderDeactivated");
            GoToState(
                Content == null && LeftHeader == null && RightHeader == null
                    ? "CompactHeight"
                    : "ExpandedHeight");
            GoToState(_isCompact ? "Compact" : "Expanded");
            GoToState(
                IsBackButtonVisible == IsPaneToggleButtonVisible
                    ? "DefaultSpacing"
                    : "NegativeInsetSpacing");
        }

        private void UpdateDisplayMode()
        {
            if (Content == null || _contentPresenter == null || _contentPresenterGrid == null)
            {
                _compactModeThresholdWidth = 0;
                _isCompact = false;
                GoToState("Expanded");
                return;
            }

            if (!_isCompact &&
                _contentPresenterGrid.ActualWidth > 0 &&
                _contentPresenter.DesiredSize.Width >= _contentPresenterGrid.ActualWidth)
            {
                _compactModeThresholdWidth = ActualWidth;
                _isCompact = true;
                GoToState("Compact");
            }
            else if (_isCompact && ActualWidth >= _compactModeThresholdWidth)
            {
                _compactModeThresholdWidth = 0;
                _isCompact = false;
                GoToState("Expanded");
                UpdateAllStates();
            }
        }

        private bool IsDragTarget(DependencyObject originalSource)
        {
            var current = originalSource;
            var hasInteractiveElement = false;

            while (current != null && !ReferenceEquals(current, this))
            {
                var localValue = current.ReadLocalValue(IsDragRegionProperty);
                if (localValue != DependencyProperty.UnsetValue)
                {
                    return (bool?)localValue == true;
                }

                if (current is Hyperlink hyperlink && hyperlink.IsEnabled)
                {
                    hasInteractiveElement = true;
                }
                else if (current is Control control && control.IsEnabled)
                {
                    hasInteractiveElement = true;
                }
                else if (ReferenceEquals(current, _leftHeaderPresenter) ||
                    ReferenceEquals(current, _rightHeaderPresenter))
                {
                    hasInteractiveElement = true;
                }

                current = GetParent(current);
            }

            return !hasInteractiveElement;
        }

        private void GoToState(string stateName)
        {
            VisualStateManager.GoToState(this, stateName, false);
        }

        private void UnhookTemplateParts()
        {
            if (_backButton != null)
            {
                _backButton.Click -= OnBackButtonClick;
            }

            if (_paneToggleButton != null)
            {
                _paneToggleButton.Click -= OnPaneToggleButtonClick;
            }

            _backButton = null;
            _paneToggleButton = null;
            _layoutRoot = null;
            _contentPresenter = null;
            _contentPresenterGrid = null;
            _leftHeaderPresenter = null;
            _rightHeaderPresenter = null;
        }
    }
}
