using System;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using ModernWpf.Automation.Peers;
using ModernWpf.Controls.Primitives;

namespace ModernWpf.Controls
{
    [ContentProperty(nameof(Header))]
    [TemplatePart(Name = nameof(HeaderContentPresenter), Type = typeof(ContentPresenter))]
    [TemplatePart(Name = nameof(SwitchCurtain), Type = typeof(FrameworkElement))]
    [TemplatePart(Name = nameof(SwitchCurtainBounds), Type = typeof(FrameworkElement))]
    [TemplatePart(Name = nameof(SwitchCurtainClip), Type = typeof(UIElement))]
    [TemplatePart(Name = nameof(SwitchKnobBounds), Type = typeof(FrameworkElement))]
    [TemplatePart(Name = nameof(SwitchKnob), Type = typeof(FrameworkElement))]
    [TemplatePart(Name = nameof(KnobTranslateTransform), Type = typeof(TranslateTransform))]
    [TemplatePart(Name = nameof(SwitchThumb), Type = typeof(Thumb))]
    [TemplateVisualState(GroupName = VisualStates.GroupCommon, Name = VisualStates.StateNormal)]
    [TemplateVisualState(GroupName = VisualStates.GroupCommon, Name = PointerOverState)]
    [TemplateVisualState(GroupName = VisualStates.GroupCommon, Name = VisualStates.StatePressed)]
    [TemplateVisualState(GroupName = VisualStates.GroupCommon, Name = VisualStates.StateDisabled)]
    [TemplateVisualState(GroupName = FocusStatesGroup, Name = PointerFocusedState)]
    [TemplateVisualState(GroupName = FocusStatesGroup, Name = FocusedState)]
    [TemplateVisualState(GroupName = FocusStatesGroup, Name = UnfocusedState)]
    [TemplateVisualState(GroupName = ContentStatesGroup, Name = OffContentState)]
    [TemplateVisualState(GroupName = ContentStatesGroup, Name = OnContentState)]
    [TemplateVisualState(GroupName = ToggleStatesGroup, Name = DraggingState)]
    [TemplateVisualState(GroupName = ToggleStatesGroup, Name = OffState)]
    [TemplateVisualState(GroupName = ToggleStatesGroup, Name = OnState)]
    [TemplateVisualState(GroupName = HeaderStatesGroup, Name = TopHeaderState)]
    [TemplateVisualState(GroupName = HeaderStatesGroup, Name = LeftHeaderState)]
    public class ToggleSwitch : Control
    {
        private const string PointerOverState = "PointerOver";
        private const string FocusStatesGroup = "FocusStates";
        private const string PointerFocusedState = "PointerFocused";
        private const string FocusedState = "Focused";
        private const string UnfocusedState = "Unfocused";
        private const string ContentStatesGroup = "ContentStates";
        private const string OffContentState = "OffContent";
        private const string OnContentState = "OnContent";
        private const string ToggleStatesGroup = "ToggleStates";
        private const string DraggingState = "Dragging";
        private const string OffState = "Off";
        private const string OnState = "On";
        private const string HeaderStatesGroup = "HeaderStates";
        private const string TopHeaderState = "TopHeader";
        private const string LeftHeaderState = "LeftHeader";

        private bool _isPointerOver;
        private bool _isPointerFocused;
        private bool _isDragging;
        private bool _wasDragged;
        private bool _handledKeyDown;

        private double _curtainTranslation;
        private double _minCurtainTranslation;
        private double _maxCurtainTranslation;

        private double _knobTranslation;
        private double _minKnobTranslation;
        private double _maxKnobTranslation;

        static ToggleSwitch()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ToggleSwitch), new FrameworkPropertyMetadata(typeof(ToggleSwitch)));
            FocusableProperty.OverrideMetadata(typeof(ToggleSwitch), new FrameworkPropertyMetadata(true));

            EventManager.RegisterClassHandler(typeof(ToggleSwitch), MouseLeftButtonDownEvent, new MouseButtonEventHandler(OnMouseLeftButtonDown), true);
        }

        public ToggleSwitch()
        {
            SetValue(TemplateSettingsPropertyKey, new ToggleSwitchTemplateSettings());

            IsEnabledChanged += OnIsEnabledChanged;
        }

        public static readonly RoutedEvent ToggledEvent = EventManager.RegisterRoutedEvent(nameof(Toggled), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(ToggleSwitch));

        public event RoutedEventHandler Toggled
        {
            add { AddHandler(ToggledEvent, value); }
            remove { RemoveHandler(ToggledEvent, value); }
        }

        #region Header

        public object Header
        {
            get => GetValue(HeaderProperty);
            set => SetValue(HeaderProperty, value);
        }

        public static readonly DependencyProperty HeaderProperty =
            ControlHelper.HeaderProperty.AddOwner(
                typeof(ToggleSwitch),
                new FrameworkPropertyMetadata(
                    null,
                    FrameworkPropertyMetadataOptions.AffectsMeasure,
                    OnHeaderChanged));

        private static void OnHeaderChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (ToggleSwitch)d;
            control.UpdateHeaderContentPresenterVisibility();
            control.OnHeaderChanged(e.OldValue, e.NewValue);
        }

        protected virtual void OnHeaderChanged(object oldContent, object newContent)
        {
        }

        #endregion

        #region HeaderTemplate

        public DataTemplate HeaderTemplate
        {
            get => (DataTemplate)GetValue(HeaderTemplateProperty);
            set => SetValue(HeaderTemplateProperty, value);
        }

        public static readonly DependencyProperty HeaderTemplateProperty =
            ControlHelper.HeaderTemplateProperty.AddOwner(
                typeof(ToggleSwitch),
                new FrameworkPropertyMetadata(
                    null,
                    FrameworkPropertyMetadataOptions.AffectsMeasure,
                    OnHeaderTemplateChanged));

        private static void OnHeaderTemplateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((ToggleSwitch)d).UpdateHeaderContentPresenterVisibility();
        }

        #endregion

        #region HeaderPlacement

        public ControlHeaderPlacement HeaderPlacement
        {
            get => (ControlHeaderPlacement)GetValue(HeaderPlacementProperty);
            set => SetValue(HeaderPlacementProperty, value);
        }

        public static readonly DependencyProperty HeaderPlacementProperty =
            DependencyProperty.Register(
                nameof(HeaderPlacement),
                typeof(ControlHeaderPlacement),
                typeof(ToggleSwitch),
                new FrameworkPropertyMetadata(
                    ControlHeaderPlacement.Top,
                    OnHeaderPlacementChanged));

        private static void OnHeaderPlacementChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((ToggleSwitch)d).UpdateVisualStates();
        }

        #endregion

        #region IsOn

        public bool IsOn
        {
            get => (bool)GetValue(IsOnProperty);
            set => SetValue(IsOnProperty, value);
        }

        public static readonly DependencyProperty IsOnProperty =
            DependencyProperty.Register(
                nameof(IsOn),
                typeof(bool),
                typeof(ToggleSwitch),
                new FrameworkPropertyMetadata(
                    false,
                    FrameworkPropertyMetadataOptions.AffectsMeasure |
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault |
                    FrameworkPropertyMetadataOptions.Journal,
                    OnIsOnChanged));

        private static void OnIsOnChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((ToggleSwitch)d).OnToggled();
        }

        #endregion

        #region OffContent

        public object OffContent
        {
            get => GetValue(OffContentProperty);
            set => SetValue(OffContentProperty, value);
        }

        public static readonly DependencyProperty OffContentProperty =
            DependencyProperty.Register(
                nameof(OffContent),
                typeof(object),
                typeof(ToggleSwitch),
                new FrameworkPropertyMetadata(
                    Strings.ToggleSwitchOff,
                    FrameworkPropertyMetadataOptions.AffectsMeasure,
                    OnOffContentChanged));

        private static void OnOffContentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((ToggleSwitch)d).OnOffContentChanged(e.OldValue, e.NewValue);
        }

        protected virtual void OnOffContentChanged(object oldContent, object newContent)
        {
        }

        #endregion

        #region OffContentTemplate

        public DataTemplate OffContentTemplate
        {
            get => (DataTemplate)GetValue(OffContentTemplateProperty);
            set => SetValue(OffContentTemplateProperty, value);
        }

        public static readonly DependencyProperty OffContentTemplateProperty =
            DependencyProperty.Register(
                nameof(OffContentTemplate),
                typeof(DataTemplate),
                typeof(ToggleSwitch),
                new FrameworkPropertyMetadata(
                    null,
                    FrameworkPropertyMetadataOptions.AffectsMeasure));

        #endregion

        #region OnContent

        public object OnContent
        {
            get => GetValue(OnContentProperty);
            set => SetValue(OnContentProperty, value);
        }

        public static readonly DependencyProperty OnContentProperty =
            DependencyProperty.Register(
                nameof(OnContent),
                typeof(object),
                typeof(ToggleSwitch),
                new FrameworkPropertyMetadata(Strings.ToggleSwitchOn, OnOnContentChanged));

        private static void OnOnContentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((ToggleSwitch)d).OnOnContentChanged(e.OldValue, e.NewValue);
        }

        protected virtual void OnOnContentChanged(object oldContent, object newContent)
        {
        }

        #endregion

        #region OnContentTemplate

        public DataTemplate OnContentTemplate
        {
            get => (DataTemplate)GetValue(OnContentTemplateProperty);
            set => SetValue(OnContentTemplateProperty, value);
        }

        public static readonly DependencyProperty OnContentTemplateProperty =
            DependencyProperty.Register(
                nameof(OnContentTemplate),
                typeof(DataTemplate),
                typeof(ToggleSwitch),
                new FrameworkPropertyMetadata(
                    null,
                    FrameworkPropertyMetadataOptions.AffectsMeasure));

        #endregion

        #region UseSystemFocusVisuals

        public static readonly DependencyProperty UseSystemFocusVisualsProperty =
            FocusVisualHelper.UseSystemFocusVisualsProperty.AddOwner(typeof(ToggleSwitch));

        public bool UseSystemFocusVisuals
        {
            get => (bool)GetValue(UseSystemFocusVisualsProperty);
            set => SetValue(UseSystemFocusVisualsProperty, value);
        }

        #endregion

        #region FocusVisualMargin

        public static readonly DependencyProperty FocusVisualMarginProperty =
            FocusVisualHelper.FocusVisualMarginProperty.AddOwner(typeof(ToggleSwitch));

        public Thickness FocusVisualMargin
        {
            get => (Thickness)GetValue(FocusVisualMarginProperty);
            set => SetValue(FocusVisualMarginProperty, value);
        }

        #endregion

        #region CornerRadius

        public static readonly DependencyProperty CornerRadiusProperty =
            ControlHelper.CornerRadiusProperty.AddOwner(typeof(ToggleSwitch));

        public CornerRadius CornerRadius
        {
            get => (CornerRadius)GetValue(CornerRadiusProperty);
            set => SetValue(CornerRadiusProperty, value);
        }

        #endregion

        #region TemplateSettings

        private static readonly DependencyPropertyKey TemplateSettingsPropertyKey =
            DependencyProperty.RegisterReadOnly(
                nameof(TemplateSettings),
                typeof(ToggleSwitchTemplateSettings),
                typeof(ToggleSwitch),
                new PropertyMetadata(null));

        public static readonly DependencyProperty TemplateSettingsProperty =
            TemplateSettingsPropertyKey.DependencyProperty;

        public ToggleSwitchTemplateSettings TemplateSettings =>
            (ToggleSwitchTemplateSettings)GetValue(TemplateSettingsProperty);

        #endregion

        private ContentPresenter HeaderContentPresenter { get; set; }

        private FrameworkElement SwitchCurtain { get; set; }

        private FrameworkElement SwitchCurtainBounds { get; set; }

        private UIElement SwitchCurtainClip { get; set; }

        private TranslateTransform CurtainTranslateTransform { get; set; }

        private FrameworkElement SwitchKnobBounds { get; set; }

        private FrameworkElement SwitchKnob { get; set; }

        private TranslateTransform KnobTranslateTransform { get; set; }

        private Thumb SwitchThumb { get; set; }

        public override void OnApplyTemplate()
        {
            if (SwitchThumb != null)
            {
                SwitchThumb.DragStarted -= OnSwitchThumbDragStarted;
                SwitchThumb.DragDelta -= OnSwitchThumbDragDelta;
                SwitchThumb.DragCompleted -= OnSwitchThumbDragCompleted;
                SwitchThumb.LostMouseCapture -= OnSwitchThumbLostMouseCapture;
                SwitchThumb.RemoveHandler(MouseLeftButtonUpEvent, new MouseButtonEventHandler(OnSwitchThumbMouseLeftButtonUp));
            }

            if (SwitchKnob != null)
            {
                SwitchKnob.SizeChanged -= OnSwitchPartSizeChanged;
            }

            if (SwitchKnobBounds != null)
            {
                SwitchKnobBounds.SizeChanged -= OnSwitchPartSizeChanged;
            }

            base.OnApplyTemplate();

            HeaderContentPresenter = GetTemplateChild(nameof(HeaderContentPresenter)) as ContentPresenter;
            SwitchCurtain = GetTemplateChild(nameof(SwitchCurtain)) as FrameworkElement;
            SwitchCurtainBounds = GetTemplateChild(nameof(SwitchCurtainBounds)) as FrameworkElement;
            SwitchCurtainClip = GetTemplateChild(nameof(SwitchCurtainClip)) as UIElement;
            SwitchKnobBounds = GetTemplateChild(nameof(SwitchKnobBounds)) as FrameworkElement;
            SwitchKnob = GetTemplateChild(nameof(SwitchKnob)) as FrameworkElement;
            KnobTranslateTransform = GetTemplateChild(nameof(KnobTranslateTransform)) as TranslateTransform;
            SwitchThumb = GetTemplateChild(nameof(SwitchThumb)) as Thumb;
            CurtainTranslateTransform = SwitchCurtain?.RenderTransform as TranslateTransform;

            if (SwitchThumb != null)
            {
                SwitchThumb.DragStarted += OnSwitchThumbDragStarted;
                SwitchThumb.DragDelta += OnSwitchThumbDragDelta;
                SwitchThumb.DragCompleted += OnSwitchThumbDragCompleted;
                SwitchThumb.LostMouseCapture += OnSwitchThumbLostMouseCapture;
                SwitchThumb.AddHandler(MouseLeftButtonUpEvent, new MouseButtonEventHandler(OnSwitchThumbMouseLeftButtonUp), true);
            }

            if (SwitchKnob != null)
            {
                SwitchKnob.SizeChanged += OnSwitchPartSizeChanged;
            }

            if (SwitchKnobBounds != null)
            {
                SwitchKnobBounds.SizeChanged += OnSwitchPartSizeChanged;
            }

            UpdateHeaderContentPresenterVisibility();
            UpdateVisualStates(false);
        }

        protected override AutomationPeer OnCreateAutomationPeer()
        {
            return new ToggleSwitchAutomationPeer(this);
        }

        protected virtual void OnToggled()
        {
            RaiseEvent(new RoutedEventArgs(ToggledEvent));

            if (UIElementAutomationPeer.FromElement(this) is { } peer)
            {
                var newValue = IsOn ? ToggleState.On : ToggleState.Off;
                var oldValue = (newValue == ToggleState.On) ? ToggleState.Off : ToggleState.On;
                peer.RaisePropertyChangedEvent(TogglePatternIdentifiers.ToggleStateProperty, oldValue, newValue);
            }

            if (!_isDragging)
            {
                UpdateVisualStates(true);
            }
        }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);

            if (e.Property == VisibilityProperty)
            {
                if (Visibility != Visibility.Visible)
                {
                    _isDragging = false;
                    _isPointerOver = false;
                }

                UpdateVisualStates();
            }
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            base.OnKeyUp(e);

            bool isHandled = e.Handled;
            ToggleSwitchKeyProcess.KeyUp(GetOriginalKey(e), this, ref isHandled);
            e.Handled = isHandled;
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            if (e.Handled || _isDragging)
            {
                return;
            }

            bool isHandled = e.Handled;
            ToggleSwitchKeyProcess.KeyDown(GetOriginalKey(e), this, ref isHandled);
            e.Handled = isHandled;
            _handledKeyDown = isHandled;
        }

        protected override void OnMouseEnter(MouseEventArgs e)
        {
            base.OnMouseEnter(e);

            _isPointerOver = true;
            UpdateVisualStates(true);
        }

        protected override void OnMouseLeave(MouseEventArgs e)
        {
            base.OnMouseLeave(e);

            _isPointerOver = false;
            UpdateVisualStates(true);
        }

        protected override void OnLostMouseCapture(MouseEventArgs e)
        {
            base.OnLostMouseCapture(e);

            OnPointerCaptureLostSubstitute();
        }

        protected override void OnManipulationStarting(ManipulationStartingEventArgs e)
        {
            base.OnManipulationStarting(e);

            e.Mode = ManipulationModes.TranslateX;
            e.ManipulationContainer = this;
        }

        protected override void OnManipulationStarted(ManipulationStartedEventArgs e)
        {
            base.OnManipulationStarted(e);

            _isDragging = true;
            _wasDragged = false;

            FocusFromPointer();

            GetTranslations();
            UpdateVisualStates(true);
            SetTranslations();
        }

        protected override void OnManipulationDelta(ManipulationDeltaEventArgs e)
        {
            base.OnManipulationDelta(e);

            double horizontalChange = e.DeltaManipulation.Translation.X;

            if (horizontalChange != 0)
            {
                _wasDragged = true;
                MoveDelta(horizontalChange);
                e.Handled = true;
            }
        }

        protected override void OnManipulationCompleted(ManipulationCompletedEventArgs e)
        {
            base.OnManipulationCompleted(e);

            if (_isDragging)
            {
                _isDragging = false;
                MoveCompleted(_wasDragged);
            }
        }

        private void OnSwitchThumbLostMouseCapture(object sender, MouseEventArgs e)
        {
            OnPointerCaptureLostSubstitute();
        }

        private void OnPointerCaptureLostSubstitute()
        {
            if (!_isDragging)
            {
                _isPointerOver = false;
            }

            UpdateVisualStates(true);
        }

        protected override void OnGotFocus(RoutedEventArgs e)
        {
            base.OnGotFocus(e);

            UpdateVisualStates(true);
        }

        protected override void OnLostFocus(RoutedEventArgs e)
        {
            base.OnLostFocus(e);

            _isPointerFocused = false;
            UpdateVisualStates(true);
        }

        private static void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var toggle = (ToggleSwitch)sender;

            if (!toggle.IsKeyboardFocused)
            {
                e.Handled = toggle.FocusFromPointer() || e.Handled;
            }
            else
            {
                toggle._isPointerFocused = true;
                toggle.UpdateVisualStates(true);
            }
        }

        private void OnSwitchThumbDragStarted(object sender, DragStartedEventArgs e)
        {
            _isDragging = true;
            _wasDragged = false;

            FocusFromPointer();

            GetTranslations();
            UpdateVisualStates(true);
            SetTranslations();
        }

        private void OnSwitchThumbDragDelta(object sender, DragDeltaEventArgs e)
        {
            if (e.HorizontalChange != 0)
            {
                _wasDragged = true;
                MoveDelta(e.HorizontalChange);
            }
        }

        private void OnSwitchThumbDragCompleted(object sender, DragCompletedEventArgs e)
        {
            if (e.Canceled)
            {
                return;
            }

            _isDragging = false;
            MoveCompleted(_wasDragged);
        }

        private void OnSwitchThumbMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            // WPF Thumb handles MouseLeftButtonUp before this instance handler runs; this is the
            // closest substitute for WinUI's Thumb.Tapped event, which runs after drag cleanup.
            if (_isDragging)
            {
                return;
            }

            if (_wasDragged)
            {
                _wasDragged = false;
                e.Handled = true;
                return;
            }

            Toggle();
            e.Handled = true;
        }

        private void OnSwitchPartSizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateTranslationBounds();
        }

        private void OnIsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (!IsEnabled)
            {
                _isDragging = false;
                _isPointerOver = false;
            }

            UpdateVisualStates();
        }

        private void GetTranslations()
        {
            if (KnobTranslateTransform != null)
            {
                _knobTranslation = KnobTranslateTransform.X;
            }

            if (CurtainTranslateTransform != null)
            {
                _curtainTranslation = CurtainTranslateTransform.X;
            }
        }

        private void SetTranslations()
        {
            var templateSettings = TemplateSettings;

            if (KnobTranslateTransform != null)
            {
                double translation = Math.Min(_knobTranslation, _maxKnobTranslation);
                translation = Math.Max(translation, _minKnobTranslation);

                StopTranslationAnimation(KnobTranslateTransform);
                KnobTranslateTransform.X = translation;

                templateSettings.KnobCurrentToOffOffset = translation - _minKnobTranslation;
                templateSettings.KnobCurrentToOnOffset = translation - _maxKnobTranslation;
            }

            if (CurtainTranslateTransform != null)
            {
                double translation = Math.Min(_curtainTranslation, _maxCurtainTranslation);
                translation = Math.Max(translation, _minCurtainTranslation);

                StopTranslationAnimation(CurtainTranslateTransform);
                CurtainTranslateTransform.X = translation;

                templateSettings.CurtainCurrentToOffOffset = translation - _minCurtainTranslation;
                templateSettings.CurtainCurrentToOnOffset = translation - _maxCurtainTranslation;
            }
        }

        private void ClearTranslations()
        {
            ClearTranslation(KnobTranslateTransform);
            ClearTranslation(CurtainTranslateTransform);
        }

        private void MoveDelta(double translationDelta)
        {
            _curtainTranslation += translationDelta;
            _knobTranslation += translationDelta;

            SetTranslations();
        }

        private void MoveCompleted(bool wasMoved)
        {
            bool wasToggled = false;

            if (wasMoved)
            {
                double halfOfTranslationRange = (_maxKnobTranslation - _minKnobTranslation) / 2;
                wasToggled = IsOn
                    ? _knobTranslation <= halfOfTranslationRange
                    : _knobTranslation >= halfOfTranslationRange;
            }

            ClearTranslations();

            if (wasToggled)
            {
                Toggle();
            }
            else
            {
                UpdateVisualStates(true);
            }
        }

        private void UpdateTranslationBounds()
        {
            double curtainBoundsWidth = 0;

            if (SwitchCurtainBounds != null)
            {
                curtainBoundsWidth = SwitchCurtainBounds.ActualWidth;

                if (SwitchCurtainClip != null)
                {
                    SwitchCurtainClip.Clip = new RectangleGeometry(
                        new Rect(0, 0, curtainBoundsWidth, SwitchCurtainBounds.ActualHeight));
                }
            }

            if (SwitchKnob != null &&
                SwitchKnobBounds != null &&
                KnobTranslateTransform != null)
            {
                double knobTranslation = KnobTranslateTransform.X;
                double knobBoundsWidth = SwitchKnobBounds.ActualWidth;
                double knobWidth = SwitchKnob.ActualWidth;

                if (IsOn)
                {
                    _maxKnobTranslation = knobTranslation;
                    _minKnobTranslation = _maxKnobTranslation - knobBoundsWidth + knobWidth;
                }
                else
                {
                    _minKnobTranslation = knobTranslation;
                    _maxKnobTranslation = _minKnobTranslation + knobBoundsWidth - knobWidth;
                }

                if (SwitchKnob.Margin.Left < 0)
                {
                    _maxKnobTranslation -= SwitchKnob.Margin.Left;
                }

                if (SwitchKnob.Margin.Right < 0)
                {
                    _maxKnobTranslation -= SwitchKnob.Margin.Right;
                }
            }

            if (SwitchCurtainBounds != null && CurtainTranslateTransform != null)
            {
                double curtainTranslation = CurtainTranslateTransform.X;

                if (IsOn)
                {
                    _maxCurtainTranslation = curtainTranslation;
                    _minCurtainTranslation = _maxCurtainTranslation - curtainBoundsWidth;
                }
                else
                {
                    _minCurtainTranslation = curtainTranslation;
                    _maxCurtainTranslation = _minCurtainTranslation + curtainBoundsWidth;
                }
            }

            var templateSettings = TemplateSettings;
            templateSettings.KnobOffToOnOffset = _minKnobTranslation - _maxKnobTranslation;
            templateSettings.KnobOnToOffOffset = _maxKnobTranslation - _minKnobTranslation;
            templateSettings.CurtainOffToOnOffset = _minCurtainTranslation - _maxCurtainTranslation;
            templateSettings.CurtainOnToOffOffset = _maxCurtainTranslation - _minCurtainTranslation;
        }

        private static void ClearTranslation(TranslateTransform transform)
        {
            if (transform != null)
            {
                StopTranslationAnimation(transform);
                transform.ClearValue(TranslateTransform.XProperty);
            }
        }

        private static void StopTranslationAnimation(TranslateTransform transform)
        {
            transform.BeginAnimation(TranslateTransform.XProperty, null);
        }

        private static bool HandlesKey(Key key)
        {
            return key == Key.Space;
        }

        private static class ToggleSwitchKeyProcess
        {
            public static void KeyDown(Key key, ToggleSwitch control, ref bool isHandled)
            {
                if (HandlesKey(key))
                {
                    isHandled = true;
                }
            }

            public static void KeyUp(Key key, ToggleSwitch control, ref bool isHandled)
            {
                bool shouldToggleOff = false;
                bool shouldToggleOn = false;
                bool handlesKey = HandlesKey(key);
                bool handledKeyDown = false;
                bool isLTR = control.FlowDirection == FlowDirection.LeftToRight;

                if (handlesKey)
                {
                    handledKeyDown = control._handledKeyDown;
                    control._handledKeyDown = false;
                }

                // WinUI also handles VirtualKey.GamepadA here. WPF exposes no
                // equivalent Key value in the target frameworks.
                if (handlesKey && handledKeyDown && !isHandled && !control._isDragging)
                {
                    if ((key == Key.Left && isLTR) ||
                        (key == Key.Right && !isLTR) ||
                        key == Key.Down ||
                        key == Key.Home)
                    {
                        shouldToggleOff = true;
                    }
                    else if ((key == Key.Right && isLTR) ||
                        (key == Key.Left && !isLTR) ||
                        key == Key.Up ||
                        key == Key.End)
                    {
                        shouldToggleOn = true;
                    }

                    if ((!control.IsOn && shouldToggleOn) ||
                        (control.IsOn && shouldToggleOff) ||
                        key == Key.Space)
                    {
                        control.Toggle();
                        isHandled = true;
                    }
                }
            }
        }

        private static Key GetOriginalKey(KeyEventArgs e)
        {
            // WinUI uses KeyRoutedEventArgs.OriginalKey; these WPF fallbacks keep
            // system/IME-processed key input on the same source-shaped path.
            if (e.Key == Key.System)
            {
                return e.SystemKey;
            }

            if (e.Key == Key.ImeProcessed)
            {
                return e.ImeProcessedKey;
            }

            if (e.Key == Key.DeadCharProcessed)
            {
                return e.DeadCharProcessedKey;
            }

            return e.Key;
        }

        private void UpdateHeaderContentPresenterVisibility()
        {
            if (HeaderContentPresenter != null)
            {
                bool showHeader = Header != null || HeaderTemplate != null;
                HeaderContentPresenter.Visibility = showHeader ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        private void UpdateVisualStates(bool useTransitions = true)
        {
            string stateName;

            if (!IsEnabled)
            {
                stateName = VisualStates.StateDisabled;
            }
            else if (_isDragging)
            {
                stateName = VisualStates.StatePressed;
            }
            else if (_isPointerOver)
            {
                stateName = PointerOverState;
            }
            else
            {
                stateName = VisualStates.StateNormal;
            }
            VisualStateManager.GoToState(this, stateName, useTransitions);

            if (IsEnabled)
            {
                if (IsKeyboardFocused)
                {
                    VisualStateManager.GoToState(this, _isPointerFocused ? PointerFocusedState : FocusedState, useTransitions);
                }
                else
                {
                    VisualStateManager.GoToState(this, UnfocusedState, useTransitions);
                }
            }
            else
            {
                VisualStateManager.GoToState(this, UnfocusedState, useTransitions);
            }

            if (_isDragging)
            {
                stateName = DraggingState;
            }
            else
            {
                stateName = IsOn ? OnState : OffState;
                VisualStateManager.GoToState(this, IsOn ? OnContentState : OffContentState, useTransitions);
            }
            VisualStateManager.GoToState(this, stateName, useTransitions);

            VisualStateManager.GoToState(
                this,
                HeaderPlacement == ControlHeaderPlacement.Left ? LeftHeaderState : TopHeaderState,
                useTransitions);
        }

        internal void Toggle()
        {
            SetCurrentValue(IsOnProperty, !IsOn);
        }

        internal void AutomationToggleSwitchOnToggle()
        {
            Toggle();
        }

        private bool FocusFromPointer()
        {
            _isPointerFocused = true;
            bool focused = Focus();

            if (!focused)
            {
                _isPointerFocused = false;
            }

            return focused;
        }

        internal UIElement GetAutomationClickableElement()
        {
            return SwitchThumb != null ? (UIElement)SwitchThumb : this;
        }
    }
}
