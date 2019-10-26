using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Threading;
using ModernWpf.Controls.Primitives;

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
        static ContentDialog()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ContentDialog),
                new FrameworkPropertyMetadata(typeof(ContentDialog)));
        }

        public ContentDialog()
        {
            m_closeTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(0.6)
            };
            m_closeTimer.Tick += OnCloseTimerTick;
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

        #region OpenDialog

        private static readonly DependencyProperty OpenDialogProperty =
            DependencyProperty.RegisterAttached(
                "OpenDialog",
                typeof(ContentDialog),
                typeof(ContentDialog),
                new PropertyMetadata(default(ContentDialog), OnOpenDialogChanged));

        private static ContentDialog GetOpenDialog(Window window)
        {
            return (ContentDialog)window.GetValue(OpenDialogProperty);
        }

        private static void SetOpenDialog(Window window, ContentDialog value)
        {
            window.SetValue(OpenDialogProperty, value);
        }

        private static void OnOpenDialogChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var owner = (Window)d;

            var oldValue = (ContentDialog)e.OldValue;
            if (oldValue != null)
            {
                oldValue.UnsubscribeToBackRequested(owner);
            }

            var newValue = (ContentDialog)e.NewValue;
            if (newValue != null)
            {
                newValue.SubscribeToBackRequested(owner);
            }
        }

        #endregion

        public Window Owner { get; set; }

        private Window ActualOwner => Owner ?? GetActiveWindow();

        private Border Container { get; set; }

        private FrameworkElement LayoutRoot { get; set; }

        private Button PrimaryButton { get; set; }

        private Button SecondaryButton { get; set; }

        private Button CloseButton { get; set; }

        private bool IsShowing
        {
            get => m_isShowing;
            set
            {
                if (m_isShowing != value)
                {
                    m_isShowing = value;
                    m_opening = m_isShowing;

                    if (!m_isShowing)
                    {
                        if (m_isShowingInPlace)
                        {
                            m_isShowingInPlace = false;
                        }
                        else if (m_openDialogOwner != null)
                        {
                            m_openDialogOwner.ClearValue(OpenDialogProperty);
                            m_openDialogOwner = null;
                        }

                        m_closeTimer.Start();
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

        public async Task<ContentDialogResult> ShowAsync()
        {
            var owner = ActualOwner;
            if (owner == null)
            {
                await WaitUntilApplicationActivated();
                owner = ActualOwner;
            }

            if (owner == null)
            {
                throw new InvalidOperationException("Could not find an owner window for this ContentDialog.");
            }

            ThrowIfHasOpenDialog(owner);

            var cp = FindContentPresenter(owner);
            if (cp == null)
            {
                if (!owner.IsActive)
                {
                    await WaitUntilOwnerActivated(owner);
                    cp = FindContentPresenter(owner);
                }
            }

            if (cp == null)
            {
                throw new InvalidOperationException("Cound not find the ContentPresenter in the owner window.");
            }

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
            m_adornerLayer.Add(m_adorner);
            DisableKeyboardNavigation(cp);

            IsShowing = true;
            m_openDialogOwner = owner;
            SetOpenDialog(owner, this);

            return await CreateAsyncOperation();
        }

        public Task<ContentDialogResult> ShowAsync(ContentDialogPlacement placement)
        {
            if (placement == ContentDialogPlacement.InPlace && Parent != null)
            {
                if (IsShowing)
                {
                    ThrowAlreadyOpenException();
                }
                RemovePopup();
                IsShowing = true;
                m_isShowingInPlace = true;
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
            if (GetTemplateChild(DialogShowingStatesGroup) is VisualStateGroup dialogShowingStates)
            {
                dialogShowingStates.CurrentStateChanging += DialogShowingStates_CurrentStateChanging;
                dialogShowingStates.CurrentStateChanged += DialogShowingStates_CurrentStateChanged;
            }
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
                            m_result = result;
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
                    m_result = result;
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
                m_closeTimer.Stop();
                OnClosed();
            }
        }

        private void OnCloseTimerTick(object sender, EventArgs e)
        {
            m_closeTimer.Stop();
            UpdateVisualStates(false);
            OnClosed();
        }

        private void OnOpened()
        {
            if (m_opening)
            {
                m_opening = false;
                Opened?.Invoke(this, new ContentDialogOpenedEventArgs());
            }
        }

        private void OnClosed()
        {
            if (m_adornerLayer != null)
            {
                RestoreKeyboardNavigation(m_adorner.AdornedElement);
                m_adornerLayer.Remove(m_adorner);
                m_adornerLayer = null;
            }

            if (m_showTcs != null)
            {
                Closed?.Invoke(this, new ContentDialogClosedEventArgs(m_result));
                m_showTcs.TrySetResult(m_result);
                m_showTcs = null;
                m_result = ContentDialogResult.None;
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
            m_adornerLayer = AdornerLayer.GetAdornerLayer(contentPresenter);
            if (m_adornerLayer == null)
            {
                throw new InvalidOperationException("AdornerLayer not found.");
            }
        }

        private void DisconnectAdornerChild()
        {
            if (m_adorner != null)
            {
                m_adorner.Child = null;
            }
        }

        private void EnsureAdornerChild(ContentPresenter cp, UIElement child)
        {
            if (m_adorner == null)
            {
                m_adorner = new ContentDialogAdorner(cp, child);
            }
            else
            {
                m_adorner.Child = child;
            }
        }

        private void AddPopup()
        {
            if (m_popup == null && Container != null && LayoutRoot != null)
            {
                Container.Child = null;
                m_popup = new Popup { Child = LayoutRoot };
                Container.Child = m_popup;
            }
        }

        private void RemovePopup()
        {
            if (m_popup != null && Container != null && LayoutRoot != null)
            {
                m_popup.Child = null;
                m_popup = null;
                DisconnectAdornerChild();
                Container.Child = LayoutRoot;
            }
        }

        private void SubscribeToBackRequested(Window owner)
        {
            var handler = new EventHandler<BackRequestedEventArgs>(OnBackRequested);
            owner.AddHandler(WindowHelper.InternalBackRequestedEvent, handler);
            m_backRequestedHandler = handler;
        }

        private void UnsubscribeToBackRequested(Window owner)
        {
            if (m_backRequestedHandler != null)
            {
                owner.RemoveHandler(WindowHelper.InternalBackRequestedEvent, m_backRequestedHandler);
                m_backRequestedHandler = null;
            }
        }

        private void OnBackRequested(object sender, BackRequestedEventArgs e)
        {
            e.Handled = true;
            Hide();
        }

        private void OnApplicationActivated(object sender, EventArgs e)
        {
            Application.Current.Activated -= OnApplicationActivated;
            if (m_activatedTcs != null)
            {
                m_activatedTcs.TrySetResult(true);
                m_activatedTcs = null;
            }
        }

        private void OnOwnerActivated(object sender, EventArgs e)
        {
            var owner = (Window)sender;
            owner.Activated -= OnOwnerActivated;
            if (m_activatedTcs != null)
            {
                m_activatedTcs.TrySetResult(true);
                m_activatedTcs = null;
            }
        }

        private Task WaitUntilApplicationActivated()
        {
            m_activatedTcs = new TaskCompletionSource<bool>();
            Application.Current.Activated += OnApplicationActivated;
            return m_activatedTcs.Task;
        }

        private Task WaitUntilOwnerActivated(Window owner)
        {
            m_activatedTcs = new TaskCompletionSource<bool>();
            owner.Activated += OnOwnerActivated;
            return m_activatedTcs.Task;
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

        private void ThrowIfHasOpenDialog(Window owner)
        {
            if (GetOpenDialog(owner) != null)
            {
                ThrowAlreadyOpenException();
            }
        }

        private static void ThrowAlreadyOpenException()
        {
            throw new InvalidOperationException("Only a single ContentDialog can be open at any time.");
        }

        private static Window GetActiveWindow()
        {
            var active = UnsafeNativeMethods.GetActiveWindow();
            foreach (Window window in Application.Current.Windows)
            {
                if (new WindowInteropHelper(window).Handle == active)
                {
                    return window;
                }
            }
            return null;
        }

        private static ContentPresenter FindContentPresenter(Window window)
        {
            ContentPresenter cp = null;

            if (window.Content is UIElement windowContent)
            {
                cp = VisualTreeHelper.GetParent(windowContent) as ContentPresenter;
            }

            if (cp == null)
            {
                var ad = window.FindDescendant<AdornerDecorator>();
                if (ad != null)
                {
                    cp = ad.FindDescendant<ContentPresenter>();
                }
            }

            return cp;
        }

        private Task<ContentDialogResult> CreateAsyncOperation()
        {
            m_showTcs = new TaskCompletionSource<ContentDialogResult>();
            return m_showTcs.Task;
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
                Dispatcher.BeginInvoke(() =>
                {
                    var tabNavigation = KeyboardNavigation.GetTabNavigation(GetTemplateChild("BackgroundElement"));
                    Debug.Assert(tabNavigation == KeyboardNavigationMode.Cycle);
                });
            }
        }
#endif

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

        private TaskCompletionSource<ContentDialogResult> m_showTcs;
        private TaskCompletionSource<bool> m_activatedTcs;
        private ContentDialogAdorner m_adorner;
        private AdornerLayer m_adornerLayer;
        private Popup m_popup;
        private bool m_opening;
        private bool m_isShowing;
        private bool m_isShowingInPlace;
        private Window m_openDialogOwner;
        private ContentDialogResult m_result;
        private readonly DispatcherTimer m_closeTimer;
        private EventHandler<BackRequestedEventArgs> m_backRequestedHandler;
    }
}
