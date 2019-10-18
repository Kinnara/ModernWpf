using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace ModernWpf.Controls
{
    [TemplatePart(Name = nameof(Container), Type = typeof(Border))]
    [TemplatePart(Name = nameof(LayoutRoot), Type = typeof(FrameworkElement))]
    [TemplatePart(Name = nameof(PrimaryButton), Type = typeof(Button))]
    [TemplatePart(Name = nameof(SecondaryButton), Type = typeof(Button))]
    [TemplatePart(Name = nameof(CloseButton), Type = typeof(Button))]
    [TemplateVisualState(GroupName = DialogShowingStatesGroup, Name = DialogHiddenState)]
    [TemplateVisualState(GroupName = DialogShowingStatesGroup, Name = DialogShowingState)]
    [TemplateVisualState(GroupName = DialogShowingStatesGroup, Name = DialogShowingWithoutSmokeLayerState)]
    [TemplateVisualState(GroupName = DialogSizingStatesGroup, Name = DefaultDialogSizingState)]
    [TemplateVisualState(GroupName = DialogSizingStatesGroup, Name = FullDialogSizingState)]
    [TemplateVisualState(GroupName = ButtonsVisibilityStatesGroup, Name = AllVisibleState)]
    [TemplateVisualState(GroupName = ButtonsVisibilityStatesGroup, Name = NoneVisibleState)]
    [TemplateVisualState(GroupName = ButtonsVisibilityStatesGroup, Name = PrimaryVisibleState)]
    [TemplateVisualState(GroupName = ButtonsVisibilityStatesGroup, Name = SecondaryVisibleState)]
    [TemplateVisualState(GroupName = ButtonsVisibilityStatesGroup, Name = CloseVisibleState)]
    [TemplateVisualState(GroupName = ButtonsVisibilityStatesGroup, Name = PrimaryAndSecondaryVisibleState)]
    [TemplateVisualState(GroupName = ButtonsVisibilityStatesGroup, Name = PrimaryAndCloseVisibleState)]
    [TemplateVisualState(GroupName = ButtonsVisibilityStatesGroup, Name = SecondaryAndCloseVisibleState)]
    [TemplateVisualState(GroupName = DefaultButtonStatesGroup, Name = NoDefaultButtonState)]
    [TemplateVisualState(GroupName = DefaultButtonStatesGroup, Name = PrimaryAsDefaultButtonState)]
    [TemplateVisualState(GroupName = DefaultButtonStatesGroup, Name = SecondaryAsDefaultButtonState)]
    [TemplateVisualState(GroupName = DefaultButtonStatesGroup, Name = CloseAsDefaultButtonState)]
    [TemplateVisualState(GroupName = DialogBorderStatesGroup, Name = NoBorderState)]
    [TemplateVisualState(GroupName = DialogBorderStatesGroup, Name = AccentColorBorderState)]
    [StyleTypedProperty(Property = nameof(PrimaryButtonStyle), StyleTargetType = typeof(Button))]
    [StyleTypedProperty(Property = nameof(SecondaryButtonStyle), StyleTargetType = typeof(Button))]
    [StyleTypedProperty(Property = nameof(CloseButtonStyle), StyleTargetType = typeof(Button))]
    public class ContentDialog : ContentControl
    {
        private const string DialogShowingStatesGroup = "DialogShowingStates";
        private const string DialogHiddenState = "DialogHidden";
        private const string DialogShowingState = "DialogShowing";
        private const string DialogShowingWithoutSmokeLayerState = "DialogShowingWithoutSmokeLayer";

        private const string DialogSizingStatesGroup = "DialogSizingStates";
        private const string DefaultDialogSizingState = "DefaultDialogSizing";
        private const string FullDialogSizingState = "FullDialogSizing";

        private const string ButtonsVisibilityStatesGroup = "ButtonsVisibilityStates";
        private const string AllVisibleState = "AllVisible";
        private const string NoneVisibleState = "NoneVisible";
        private const string PrimaryVisibleState = "PrimaryVisible";
        private const string SecondaryVisibleState = "SecondaryVisible";
        private const string CloseVisibleState = "CloseVisible";
        private const string PrimaryAndSecondaryVisibleState = "PrimaryAndSecondaryVisible";
        private const string PrimaryAndCloseVisibleState = "PrimaryAndCloseVisible";
        private const string SecondaryAndCloseVisibleState = "SecondaryAndCloseVisible";

        private const string DefaultButtonStatesGroup = "DefaultButtonStates";
        private const string NoDefaultButtonState = "NoDefaultButton";
        private const string PrimaryAsDefaultButtonState = "PrimaryAsDefaultButton";
        private const string SecondaryAsDefaultButtonState = "SecondaryAsDefaultButton";
        private const string CloseAsDefaultButtonState = "CloseAsDefaultButton";

        private const string DialogBorderStatesGroup = "DialogBorderStates";
        private const string NoBorderState = "NoBorder";
        private const string AccentColorBorderState = "AccentColorBorder";

        private static TaskCompletionSource<ContentDialogResult> _tcs;

        private ContentDialogAdorner _adorner;
        private AdornerLayer _adornerLayer;
        private Popup _popup;
        private bool _opening;
        private bool _isShowing;
        private ContentDialogResult _result;
        private readonly DispatcherTimer _closeTimer;

        static ContentDialog()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ContentDialog),
                new FrameworkPropertyMetadata(typeof(ContentDialog)));
        }

        public ContentDialog()
        {
            _closeTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(0.6)
            };
            _closeTimer.Tick += OnCloseTimerTick;
        }

        #region Title

        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register(
                nameof(Title),
                typeof(object),
                typeof(ContentDialog),
                null);

        public object Title
        {
            get => GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }

        #endregion

        #region TitleTemplate

        public static readonly DependencyProperty TitleTemplateProperty =
            DependencyProperty.Register(
                nameof(TitleTemplate),
                typeof(DataTemplate),
                typeof(ContentDialog),
                null);

        public DataTemplate TitleTemplate
        {
            get => (DataTemplate)GetValue(TitleTemplateProperty);
            set => SetValue(TitleTemplateProperty, value);
        }

        #endregion

        #region PrimaryButtonText

        public static readonly DependencyProperty PrimaryButtonTextProperty =
            DependencyProperty.Register(
                nameof(PrimaryButtonText),
                typeof(string),
                typeof(ContentDialog),
                new PropertyMetadata(string.Empty, OnButtonTextChanged));

        public string PrimaryButtonText
        {
            get => (string)GetValue(PrimaryButtonTextProperty);
            set => SetValue(PrimaryButtonTextProperty, value);
        }

        #endregion

        #region PrimaryButtonCommand

        public static readonly DependencyProperty PrimaryButtonCommandProperty =
            DependencyProperty.Register(
                nameof(PrimaryButtonCommand),
                typeof(ICommand),
                typeof(ContentDialog),
                null);

        public ICommand PrimaryButtonCommand
        {
            get => (ICommand)GetValue(PrimaryButtonCommandProperty);
            set => SetValue(PrimaryButtonCommandProperty, value);
        }

        #endregion

        #region PrimaryButtonCommandParameter

        public static readonly DependencyProperty PrimaryButtonCommandParameterProperty =
            DependencyProperty.Register(
                nameof(PrimaryButtonCommandParameter),
                typeof(object),
                typeof(ContentDialog),
                null);

        public object PrimaryButtonCommandParameter
        {
            get => GetValue(PrimaryButtonCommandParameterProperty);
            set => SetValue(PrimaryButtonCommandParameterProperty, value);
        }

        #endregion

        #region PrimaryButtonStyle

        public static readonly DependencyProperty PrimaryButtonStyleProperty =
            DependencyProperty.Register(
                nameof(PrimaryButtonStyle),
                typeof(Style),
                typeof(ContentDialog),
                null);

        public Style PrimaryButtonStyle
        {
            get => (Style)GetValue(PrimaryButtonStyleProperty);
            set => SetValue(PrimaryButtonStyleProperty, value);
        }

        #endregion

        #region IsPrimaryButtonEnabled

        public static readonly DependencyProperty IsPrimaryButtonEnabledProperty =
            DependencyProperty.Register(
                nameof(IsPrimaryButtonEnabled),
                typeof(bool),
                typeof(ContentDialog),
                new PropertyMetadata(true));

        public bool IsPrimaryButtonEnabled
        {
            get => (bool)GetValue(IsPrimaryButtonEnabledProperty);
            set => SetValue(IsPrimaryButtonEnabledProperty, value);
        }

        #endregion

        #region SecondaryButtonText

        public static readonly DependencyProperty SecondaryButtonTextProperty =
            DependencyProperty.Register(
                nameof(SecondaryButtonText),
                typeof(string),
                typeof(ContentDialog),
                new PropertyMetadata(string.Empty, OnButtonTextChanged));

        public string SecondaryButtonText
        {
            get => (string)GetValue(SecondaryButtonTextProperty);
            set => SetValue(SecondaryButtonTextProperty, value);
        }

        #endregion

        #region SecondaryButtonCommand

        public static readonly DependencyProperty SecondaryButtonCommandProperty =
            DependencyProperty.Register(
                nameof(SecondaryButtonCommand),
                typeof(ICommand),
                typeof(ContentDialog),
                null);

        public ICommand SecondaryButtonCommand
        {
            get => (ICommand)GetValue(SecondaryButtonCommandProperty);
            set => SetValue(SecondaryButtonCommandProperty, value);
        }

        #endregion

        #region SecondaryButtonCommandParameter

        public static readonly DependencyProperty SecondaryButtonCommandParameterProperty =
            DependencyProperty.Register(
                nameof(SecondaryButtonCommandParameter),
                typeof(object),
                typeof(ContentDialog),
                null);

        public object SecondaryButtonCommandParameter
        {
            get => GetValue(SecondaryButtonCommandParameterProperty);
            set => SetValue(SecondaryButtonCommandParameterProperty, value);
        }

        #endregion

        #region SecondaryButtonStyle

        public static readonly DependencyProperty SecondaryButtonStyleProperty =
            DependencyProperty.Register(
                nameof(SecondaryButtonStyle),
                typeof(Style),
                typeof(ContentDialog),
                null);

        public Style SecondaryButtonStyle
        {
            get => (Style)GetValue(SecondaryButtonStyleProperty);
            set => SetValue(SecondaryButtonStyleProperty, value);
        }

        #endregion

        #region IsSecondaryButtonEnabled

        public static readonly DependencyProperty IsSecondaryButtonEnabledProperty =
            DependencyProperty.Register(
                nameof(IsSecondaryButtonEnabled),
                typeof(bool),
                typeof(ContentDialog),
                new PropertyMetadata(true));

        public bool IsSecondaryButtonEnabled
        {
            get => (bool)GetValue(IsSecondaryButtonEnabledProperty);
            set => SetValue(IsSecondaryButtonEnabledProperty, value);
        }

        #endregion

        #region CloseButtonText

        public static readonly DependencyProperty CloseButtonTextProperty =
            DependencyProperty.Register(
                nameof(CloseButtonText),
                typeof(string),
                typeof(ContentDialog),
                new PropertyMetadata(string.Empty, OnButtonTextChanged));

        public string CloseButtonText
        {
            get => (string)GetValue(CloseButtonTextProperty);
            set => SetValue(CloseButtonTextProperty, value);
        }

        #endregion

        #region CloseButtonCommand

        public static readonly DependencyProperty CloseButtonCommandProperty =
            DependencyProperty.Register(
                nameof(CloseButtonCommand),
                typeof(ICommand),
                typeof(ContentDialog),
                null);

        public ICommand CloseButtonCommand
        {
            get => (ICommand)GetValue(CloseButtonCommandProperty);
            set => SetValue(CloseButtonCommandProperty, value);
        }

        #endregion

        #region CloseButtonCommandParameter

        public static readonly DependencyProperty CloseButtonCommandParameterProperty =
            DependencyProperty.Register(
                nameof(CloseButtonCommandParameter),
                typeof(object),
                typeof(ContentDialog),
                null);

        public object CloseButtonCommandParameter
        {
            get => GetValue(CloseButtonCommandParameterProperty);
            set => SetValue(CloseButtonCommandParameterProperty, value);
        }

        #endregion

        #region CloseButtonStyle

        public static readonly DependencyProperty CloseButtonStyleProperty =
            DependencyProperty.Register(
                nameof(CloseButtonStyle),
                typeof(Style),
                typeof(ContentDialog),
                null);

        public Style CloseButtonStyle
        {
            get => (Style)GetValue(CloseButtonStyleProperty);
            set => SetValue(CloseButtonStyleProperty, value);
        }

        #endregion

        #region DefaultButton

        public static readonly DependencyProperty DefaultButtonProperty =
            DependencyProperty.Register(
                nameof(DefaultButton),
                typeof(ContentDialogButton),
                typeof(ContentDialog),
                new PropertyMetadata(OnDefaultButtonChanged));

        public ContentDialogButton DefaultButton
        {
            get => (ContentDialogButton)GetValue(DefaultButtonProperty);
            set => SetValue(DefaultButtonProperty, value);
        }

        private static void OnDefaultButtonChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((ContentDialog)d).UpdateDefaultButtonStates(true);
        }

        #endregion

        #region FullSizeDesired

        public static readonly DependencyProperty FullSizeDesiredProperty =
            DependencyProperty.Register(
                nameof(FullSizeDesired),
                typeof(bool),
                typeof(ContentDialog),
                new PropertyMetadata(OnFullSizeDesiredChanged));

        public bool FullSizeDesired
        {
            get => (bool)GetValue(FullSizeDesiredProperty);
            set => SetValue(FullSizeDesiredProperty, value);
        }

        private static void OnFullSizeDesiredChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((ContentDialog)d).UpdateVisualStates(true);
        }

        #endregion

        #region CornerRadius

        public static readonly DependencyProperty CornerRadiusProperty =
            DependencyProperty.Register(
                nameof(CornerRadius),
                typeof(CornerRadius),
                typeof(ContentDialog),
                null);

        public CornerRadius CornerRadius
        {
            get => (CornerRadius)GetValue(CornerRadiusProperty);
            set => SetValue(CornerRadiusProperty, value);
        }

        #endregion

        #region IsShadowEnabled

        public static readonly DependencyProperty IsShadowEnabledProperty =
            DependencyProperty.Register(
                nameof(IsShadowEnabled),
                typeof(bool),
                typeof(ContentDialog),
                new FrameworkPropertyMetadata(true));

        public bool IsShadowEnabled
        {
            get => (bool)GetValue(IsShadowEnabledProperty);
            set => SetValue(IsShadowEnabledProperty, value);
        }

        #endregion

        private Border Container { get; set; }

        private FrameworkElement LayoutRoot { get; set; }

        private Button PrimaryButton { get; set; }

        private Button SecondaryButton { get; set; }

        private Button CloseButton { get; set; }

        private bool IsShowing
        {
            get => _isShowing;
            set
            {
                if (_isShowing != value)
                {
                    _isShowing = value;
                    _opening = _isShowing;

                    if (!_isShowing)
                    {
                        _closeTimer.Start();
                    }

                    UpdateDialogShowingStates(true);
                }
            }
        }

        public event TypedEventHandler<ContentDialog, ContentDialogOpenedEventArgs> Opened;

        public event TypedEventHandler<ContentDialog, ContentDialogClosingEventArgs> Closing;

        public event TypedEventHandler<ContentDialog, ContentDialogClosedEventArgs> Closed;

        public event TypedEventHandler<ContentDialog, ContentDialogButtonClickEventArgs> PrimaryButtonClick;

        public event TypedEventHandler<ContentDialog, ContentDialogButtonClickEventArgs> SecondaryButtonClick;

        public event TypedEventHandler<ContentDialog, ContentDialogButtonClickEventArgs> CloseButtonClick;

        public Task<ContentDialogResult> ShowAsync()
        {
            ThorwIfAlreadyOpen();

            var cp = FindWindowContentPresenter() ?? throw new InvalidOperationException("Window ContentPresenter not found.");

            UIElement dialogRoot;
            if (Parent != null)
            {
                AddPopup();
                dialogRoot = LayoutRoot;
            }
            else
            {
                RemovePopup();
                dialogRoot = this;
            }

            EnsureAdornerLayer(cp);
            EnsureAdornerChild(cp, dialogRoot);
            _adornerLayer.Add(_adorner);
            DisableKeyboardNavigation(cp);

            IsShowing = true;
            return CreateAsyncOperation();
        }

        public Task<ContentDialogResult> ShowAsync(ContentDialogPlacement placement)
        {
            ThorwIfAlreadyOpen();

            if (placement == ContentDialogPlacement.InPlace && Parent != null)
            {
                RemovePopup();
                IsShowing = true;
                return CreateAsyncOperation();
            }
            else
            {
                return ShowAsync();
            }
        }

        public void Hide()
        {
            Hide(ContentDialogResult.None);
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            if (LayoutRoot != null)
            {
                LayoutRoot.IsVisibleChanged -= OnLayoutRootIsVisibleChanged;
                LayoutRoot.Loaded -= OnLayoutRootLoaded;
            }

            if (PrimaryButton != null)
            {
                PrimaryButton.Click -= OnPrimaryButtonClick;
            }

            if (SecondaryButton != null)
            {
                SecondaryButton.Click -= OnSecondaryButtonClick;
            }

            if (CloseButton != null)
            {
                CloseButton.Click -= OnCloseButtonClick;
            }

            Container = GetTemplateChild(nameof(Container)) as Border;
            LayoutRoot = GetTemplateChild(nameof(LayoutRoot)) as FrameworkElement;
            PrimaryButton = GetTemplateChild(nameof(PrimaryButton)) as Button;
            SecondaryButton = GetTemplateChild(nameof(SecondaryButton)) as Button;
            CloseButton = GetTemplateChild(nameof(CloseButton)) as Button;

            if (LayoutRoot != null)
            {
                LayoutRoot.IsVisibleChanged += OnLayoutRootIsVisibleChanged;
                LayoutRoot.Loaded += OnLayoutRootLoaded;
            }

            if (PrimaryButton != null)
            {
                PrimaryButton.Click += OnPrimaryButtonClick;
            }

            if (SecondaryButton != null)
            {
                SecondaryButton.Click += OnSecondaryButtonClick;
            }

            if (CloseButton != null)
            {
                CloseButton.Click += OnCloseButtonClick;
            }

#if DEBUG
            var dialogShowingStates = GetTemplateChild(DialogShowingStatesGroup) as VisualStateGroup;
            dialogShowingStates.CurrentStateChanging += DialogShowingStates_CurrentStateChanging;
            dialogShowingStates.CurrentStateChanged += DialogShowingStates_CurrentStateChanged;
#endif
            UpdateVisualStates(false);

            if (DesignerProperties.GetIsInDesignMode(this))
            {
                VisualStateManager.GoToState(this, DialogShowingState, false);
            }
        }

        private void Hide(ContentDialogResult result)
        {
            if (IsShowing)
            {
                OnOpened();

                var closing = Closing;
                if (closing != null)
                {
                    var args = new ContentDialogClosingEventArgs(result);

                    var deferral = new ContentDialogClosingDeferral(() =>
                    {
                        if (!args.Cancel)
                        {
                            _result = result;
                            IsShowing = false;
                        }
                    });

                    args.SetDeferral(deferral);

                    args.IncrementDeferralCount();
                    closing(this, args);
                    args.DecrementDeferralCount();
                }
                else
                {
                    _result = result;
                    IsShowing = false;
                }
            }
        }

        private void OnPrimaryButtonClick(object sender, RoutedEventArgs e)
        {
            OnButtonClick(
                PrimaryButtonClick,
                PrimaryButtonCommand,
                PrimaryButtonCommandParameter,
                ContentDialogResult.Primary);
        }

        private void OnSecondaryButtonClick(object sender, RoutedEventArgs e)
        {
            OnButtonClick(
                SecondaryButtonClick,
                SecondaryButtonCommand,
                SecondaryButtonCommandParameter,
                ContentDialogResult.Secondary);
        }

        private void OnCloseButtonClick(object sender, RoutedEventArgs e)
        {
            OnButtonClick(
                CloseButtonClick,
                CloseButtonCommand,
                CloseButtonCommandParameter,
                ContentDialogResult.None);
        }

        private void OnButtonClick(
            TypedEventHandler<ContentDialog, ContentDialogButtonClickEventArgs> handler,
            ICommand command,
            object commandParameter,
            ContentDialogResult result)
        {
            if (handler != null)
            {
                var args = new ContentDialogButtonClickEventArgs();

                var deferral = new ContentDialogButtonClickDeferral(() =>
                {
                    if (!args.Cancel)
                    {
                        TryExecuteCommand(command, commandParameter);
                        Hide(result);
                    }
                });

                args.SetDeferral(deferral);

                args.IncrementDeferralCount();
                handler(this, args);
                args.DecrementDeferralCount();
            }
            else
            {
                TryExecuteCommand(command, commandParameter);
                Hide(result);
            }
        }

        private void OnLayoutRootLoaded(object sender, RoutedEventArgs e)
        {
            UpdateVisualStates(true);
        }

        private void OnLayoutRootIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue)
            {
                OnOpened();

                if (LayoutRoot.Parent is Popup)
                {
                    LayoutRoot.Focusable = true;
                    LayoutRoot.Focus();
                }
                else
                {
                    LayoutRoot.Focusable = false;
                    Focus();
                }
            }
            else
            {
                _closeTimer.Stop();
                OnClosed();
            }
        }

        private void OnCloseTimerTick(object sender, EventArgs e)
        {
            _closeTimer.Stop();
            UpdateVisualStates(false);
            OnClosed();
        }

        private void OnOpened()
        {
            if (_opening)
            {
                _opening = false;
                Opened?.Invoke(this, new ContentDialogOpenedEventArgs());
            }
        }

        private void OnClosed()
        {
            if (_adornerLayer != null)
            {
                RestoreKeyboardNavigation(_adorner.AdornedElement);
                _adornerLayer.Remove(_adorner);
                _adornerLayer = null;
            }

            if (_tcs != null)
            {
                Closed?.Invoke(this, new ContentDialogClosedEventArgs(_result));
                _tcs.TrySetResult(_result);
                _tcs = null;
                _result = ContentDialogResult.None;
            }
        }

        private void UpdateVisualStates(bool useTransitions)
        {
            UpdateDialogShowingStates(useTransitions);
            VisualStateManager.GoToState(this, FullSizeDesired ? FullDialogSizingState : DefaultDialogSizingState, useTransitions);
            UpdateButtonsVisibilityStates(useTransitions);
            UpdateDefaultButtonStates(useTransitions);
        }

        private void UpdateDialogShowingStates(bool useTransitions)
        {
            string stateName = IsShowing && IsLoaded ? DialogShowingState : DialogHiddenState;
#if DEBUG
            if (DesignerProperties.GetIsInDesignMode(this))
            {
                stateName = DialogShowingState;
            }
#endif
            VisualStateManager.GoToState(this, stateName, useTransitions);
        }

        private void UpdateButtonsVisibilityStates(bool useTransitions)
        {
            string stateName;

            bool primaryVisible = !string.IsNullOrEmpty(PrimaryButtonText);
            bool secondaryVisible = !string.IsNullOrEmpty(SecondaryButtonText);
            bool closeVisible = !string.IsNullOrEmpty(CloseButtonText);

            if (primaryVisible && secondaryVisible && closeVisible)
            {
                stateName = AllVisibleState;
            }
            else if (!primaryVisible && !secondaryVisible && !closeVisible)
            {
                stateName = NoneVisibleState;
            }
            else if (primaryVisible && secondaryVisible)
            {
                stateName = PrimaryAndSecondaryVisibleState;
            }
            else if (primaryVisible && closeVisible)
            {
                stateName = PrimaryAndCloseVisibleState;
            }
            else if (secondaryVisible && closeVisible)
            {
                stateName = SecondaryAndCloseVisibleState;
            }
            else if (primaryVisible)
            {
                stateName = PrimaryVisibleState;
            }
            else if (secondaryVisible)
            {
                stateName = SecondaryVisibleState;
            }
            else if (closeVisible)
            {
                stateName = CloseVisibleState;
            }
            else
            {
                stateName = AllVisibleState;
            }

            VisualStateManager.GoToState(this, stateName, useTransitions);
        }

        private void UpdateDefaultButtonStates(bool useTransitions)
        {
            string stateName = NoDefaultButtonState;

            switch (DefaultButton)
            {
                case ContentDialogButton.Primary:
                    stateName = PrimaryAsDefaultButtonState;
                    break;
                case ContentDialogButton.Secondary:
                    stateName = SecondaryAsDefaultButtonState;
                    break;
                case ContentDialogButton.Close:
                    stateName = CloseAsDefaultButtonState;
                    break;
            }

            VisualStateManager.GoToState(this, stateName, useTransitions);
        }

        private void EnsureAdornerLayer(ContentPresenter contentPresenter)
        {
            _adornerLayer = AdornerLayer.GetAdornerLayer(contentPresenter);
            if (_adornerLayer == null)
            {
                throw new InvalidOperationException("AdornerLayer not found.");
            }
        }

        private void DisconnectAdornerChild()
        {
            if (_adorner != null)
            {
                _adorner.Child = null;
            }
        }

        private void EnsureAdornerChild(ContentPresenter cp, UIElement child)
        {
            if (_adorner == null)
            {
                _adorner = new ContentDialogAdorner(cp, child);
            }
            else
            {
                _adorner.Child = child;
            }
        }

        private void AddPopup()
        {
            if (_popup == null && Container != null && LayoutRoot != null)
            {
                Container.Child = null;
                _popup = new Popup { Child = LayoutRoot };
                Container.Child = _popup;
            }
        }

        private void RemovePopup()
        {
            if (_popup != null && Container != null && LayoutRoot != null)
            {
                _popup.Child = null;
                _popup = null;
                DisconnectAdornerChild();
                Container.Child = LayoutRoot;
            }
        }

        private static void OnButtonTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((ContentDialog)d).UpdateButtonsVisibilityStates(true);
        }

        private static void TryExecuteCommand(ICommand command, object parameter)
        {
            if (command != null && command.CanExecute(parameter))
            {
                command.Execute(parameter);
            }
        }

        private static void ThorwIfAlreadyOpen()
        {
            if (_tcs != null)
            {
                throw new InvalidOperationException("Only a single ContentDialog can be open at any time.");
            }
        }

        private static ContentPresenter FindWindowContentPresenter()
        {
            if (Application.Current?.MainWindow?.Content is UIElement windowContent)
            {
                return VisualTreeHelper.GetParent(windowContent) as ContentPresenter;
            }
            return null;
        }

        private static Task<ContentDialogResult> CreateAsyncOperation()
        {
            _tcs = new TaskCompletionSource<ContentDialogResult>();
            return _tcs.Task;
        }

        private static void DisableKeyboardNavigation(DependencyObject element)
        {
            KeyboardNavigation.SetDirectionalNavigation(element, KeyboardNavigationMode.None);
            KeyboardNavigation.SetTabNavigation(element, KeyboardNavigationMode.None);
            KeyboardNavigation.SetControlTabNavigation(element, KeyboardNavigationMode.None);
        }

        private static void RestoreKeyboardNavigation(UIElement element)
        {
            element.ClearValue(KeyboardNavigation.DirectionalNavigationProperty);
            element.ClearValue(KeyboardNavigation.TabNavigationProperty);
            element.ClearValue(KeyboardNavigation.ControlTabNavigationProperty);
        }

        private class ContentDialogAdorner : Adorner
        {
            private UIElement _child;

            public ContentDialogAdorner(UIElement adornedElement, UIElement child) : base(adornedElement)
            {
                Child = child ?? throw new ArgumentNullException(nameof(child));
            }

            public UIElement Child
            {
                get => _child;
                set
                {
                    if (_child != value)
                    {
                        if (_child != null)
                        {
                            RemoveVisualChild(_child);
                        }

                        _child = value;

                        if (_child != null)
                        {
                            AddVisualChild(_child);
                        }
                    }
                }
            }

            protected override int VisualChildrenCount => _child != null ? 1 : 0;

            protected override Visual GetVisualChild(int index)
            {
                if (index == 0 && _child != null)
                {
                    return _child;
                }
                else
                {
                    throw new ArgumentOutOfRangeException(nameof(index));
                }
            }

            protected override Size MeasureOverride(Size constraint)
            {
                Child?.Measure(constraint);
                return constraint;
            }

            protected override Size ArrangeOverride(Size finalSize)
            {
                Child?.Arrange(new Rect(new Point(), finalSize));
                return finalSize;
            }
        }

#if DEBUG
        private void DialogShowingStates_CurrentStateChanging(object sender, VisualStateChangedEventArgs e)
        {
            Debug.WriteLine($"CurrentState changing to {e.NewState.Name}");
            if (e.NewState.Name == DialogShowingState)
            {
            }
        }

        private void DialogShowingStates_CurrentStateChanged(object sender, VisualStateChangedEventArgs e)
        {
            Debug.WriteLine($"CurrentState changed to {e.NewState.Name}");
            if (e.NewState.Name == DialogShowingState)
            {
                Dispatcher.InvokeAsync(() =>
                {
                    var tabNavigation = KeyboardNavigation.GetTabNavigation(GetTemplateChild("BackgroundElement"));
                    Debug.Assert(tabNavigation == KeyboardNavigationMode.Cycle);
                });
            }
        }
#endif
    }
}
