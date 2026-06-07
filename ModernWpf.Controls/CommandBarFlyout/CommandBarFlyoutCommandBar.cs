using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace ModernWpf.Controls.Primitives
{
    [TemplatePart(Name = PrimaryItemsPanelName, Type = typeof(Panel))]
    [TemplatePart(Name = SecondaryItemsPanelName, Type = typeof(Panel))]
    [TemplatePart(Name = OverflowPopupName, Type = typeof(Popup))]
    public partial class CommandBarFlyoutCommandBar : Control
    {
        static CommandBarFlyoutCommandBar()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(CommandBarFlyoutCommandBar),
                new FrameworkPropertyMetadata(typeof(CommandBarFlyoutCommandBar)));
        }

        public CommandBarFlyoutCommandBar()
        {
            SetValue(FlyoutTemplateSettingsPropertyKey, new CommandBarFlyoutCommandBarTemplateSettings());

            PrimaryCommands = new ObservableCollection<ICommandBarElement>();
            PrimaryCommands.CollectionChanged += delegate
            {
                AttachCommandElementsToPanels();
                AttachItemEventHandlers();
                UpdateHasOverflowItems();
                UpdateFlowsFromAndFlowsTo();
                UpdateUI();
            };

            SecondaryCommands = new ObservableCollection<ICommandBarElement>();
            SecondaryCommands.CollectionChanged += delegate
            {
                m_secondaryItemsRootSized = false;
                AttachCommandElementsToPanels();
                AttachItemEventHandlers();
                UpdateHasOverflowItems();
                UpdateFlowsFromAndFlowsTo();
                UpdateUI();
            };

            Loaded += delegate
            {
                UpdateUI();

                if (TryGetOwningFlyout(out var owningFlyout) &&
                    owningFlyout.ShowMode == FlyoutShowMode.Standard)
                {
                    var commands = PrimaryCommands.Count > 0 ? PrimaryCommands : (SecondaryCommands.Count > 0 ? SecondaryCommands : null);

                    if (commands != null)
                    {
                        bool usingPrimaryCommands = commands == PrimaryCommands;
                        bool ensureTabStopUniqueness = usingPrimaryCommands;
                        var firstCommandAsFrameworkElement = commands[0] as FrameworkElement;

                        if (firstCommandAsFrameworkElement != null)
                        {
                            if (SharedHelpers.IsFrameworkElementLoaded(firstCommandAsFrameworkElement))
                            {
                                FocusCommand(
                                    commands,
                                    usingPrimaryCommands ? m_moreButton : null,
                                    true,
                                    ensureTabStopUniqueness);
                            }
                            else
                            {
                                m_firstItemLoadedRevoker = new RoutedEventHandlerRevoker(
                                    firstCommandAsFrameworkElement,
                                    LoadedEvent,
                                    new RoutedEventHandler(delegate
                                    {
                                        FocusCommand(
                                            commands,
                                            usingPrimaryCommands ? m_moreButton : null,
                                            true,
                                            ensureTabStopUniqueness);
                                        m_firstItemLoadedRevoker?.Revoke();
                                    }));
                            }
                        }
                    }
                }
            };

            Unloaded += delegate
            {
                CancelAsyncSizeChangeUpdate();
                StopCloseAnimation();
                StopOpenAnimation();
                SetOpacity(1);
            };

            SizeChanged += delegate
            {
                UpdateUI(true, true);
            };

            AddHandler(MouseDownEvent, new MouseButtonEventHandler(OnMouseDown), true);
        }

        public ObservableCollection<ICommandBarElement> PrimaryCommands { get; }

        public ObservableCollection<ICommandBarElement> SecondaryCommands { get; }

        public static readonly DependencyProperty SystemBackdropProperty =
            DependencyProperty.Register(
                nameof(SystemBackdrop),
                typeof(Brush),
                typeof(CommandBarFlyoutCommandBar),
                new PropertyMetadata(null));

        public Brush SystemBackdrop
        {
            get => (Brush)GetValue(SystemBackdropProperty);
            set => SetValue(SystemBackdropProperty, value);
        }

        private static void OnDefaultLabelPositionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((CommandBarFlyoutCommandBar)d).UpdateCommandDefaultLabelPositions();
        }

        private static void OnIsOpenChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((CommandBarFlyoutCommandBar)d).OnIsOpenChanged((bool)e.NewValue);
        }

        private void OnIsOpenChanged(bool isOpen)
        {
            UpdateOverflowPopupVisibility(isOpen);

            if (isOpen)
            {
                StopCloseAnimation();
                SetOpacity(1);
                UpdateInputDeviceTypeUsedToOpen();
                UpdateCommandOverflowStyleParams();
                Opening?.Invoke(this, null);
            }
            else
            {
                Closing?.Invoke(this, null);
                m_inputModeUsedToOpen = AppBarButtonInputMode.Default;
                UpdateCommandOverflowStyleParams();
                m_secondaryItemsRootSized = false;
                StopCloseAnimation();
                StopOpenAnimation();

                if (PrimaryCommands.Count > 0)
                {
                    EnsureFocusedPrimaryCommand();
                }
            }

            UpdateFlowsFromAndFlowsTo();
            UpdateUI();
        }

        private static void OnOverflowButtonVisibilityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((CommandBarFlyoutCommandBar)d).UpdateEffectiveOverflowButtonVisibility();
        }

        private static void OnCommandBarOverflowPresenterStyleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((CommandBarFlyoutCommandBar)d).SetCurrentValue(OverflowPresenterStyleProperty, e.NewValue);
        }

        internal WeakReference<CommandBarFlyout> OwningFlyout => m_owningFlyout;

        internal void SetOwningFlyout(CommandBarFlyout owningFlyout)
        {
            m_owningFlyout = new WeakReference<CommandBarFlyout>(owningFlyout);
        }

        public event EventHandler<object> Opened;

        internal event EventHandler<object> Opening;

        internal event EventHandler<object> Closing;

        public event EventHandler<object> Closed;

        public override void OnApplyTemplate()
        {
            DetachEventHandlers();
            ClearPanelChildren(m_primaryItemsPanel);
            ClearPanelChildren(m_secondaryItemsPanel);

            base.OnApplyTemplate();

            m_layoutRoot = GetTemplateChild("LayoutRoot") as FrameworkElement;
            m_primaryItemsRoot = GetTemplateChild("PrimaryItemsRoot") as FrameworkElement;
            m_secondaryItemsRoot = GetTemplateChild("OverflowContentRoot") as FrameworkElement;
            m_primaryItemsPanel = GetTemplateChild(PrimaryItemsPanelName) as Panel;
            m_secondaryItemsPanel = GetTemplateChild(SecondaryItemsPanelName) as Panel;
            m_moreButton = GetTemplateChild("MoreButton") as ButtonBase;
            m_overflowPopup = GetTemplateChild(OverflowPopupName) as Popup;
            m_outerOverflowContentRootShadowChrome = GetTemplateChild("OuterOverflowContentRootShadowChrome") as ThemeShadowChrome;

            if (m_layoutRoot != null)
            {
                m_openingStoryboard = m_layoutRoot.Resources["OpeningStoryboard"] as Storyboard;
                m_closingStoryboard = m_layoutRoot.Resources["ClosingStoryboard"] as Storyboard;
                m_collapsedToExpandedUpStoryboard = m_layoutRoot.Resources["CollapsedToExpandedUpStoryboard"] as Storyboard;
                m_collapsedToExpandedDownStoryboard = m_layoutRoot.Resources["CollapsedToExpandedDownStoryboard"] as Storyboard;
                m_expandedUpToCollapsedStoryboard = m_layoutRoot.Resources["ExpandedUpToCollapsedStoryboard"] as Storyboard;
                m_expandedDownToCollapsedStoryboard = m_layoutRoot.Resources["ExpandedDownToCollapsedStoryboard"] as Storyboard;
            }

            if (m_moreButton != null && m_moreButton.IsTabStop)
            {
                m_moreButton.IsTabStop = false;
            }

            if (m_moreButton != null)
            {
                m_moreButton.SetBinding(System.Windows.Controls.Border.CornerRadiusProperty, new Binding(nameof(CornerRadius)) { Source = this, Mode = BindingMode.OneWay });

                if (m_moreButton is ToggleButton moreToggleButton)
                {
                    moreToggleButton.Checked += MoreButtonChecked;
                    moreToggleButton.Unchecked += MoreButtonUnchecked;
                }
            }

            if (m_overflowPopup is PopupEx popupEx)
            {
                popupEx.SuppressFadeAnimation = true;
            }

            BindOwningFlyoutPresenterToCornerRadius();
            AttachEventHandlers();
            AttachCommandElementsToPanels();
            AttachItemEventHandlers();
            UpdateHasOverflowItems();
            UpdateFlowsFromAndFlowsTo();
            UpdateOverflowPopupVisibility(IsOpen);
            UpdateUI(false);
        }

        protected override void OnIsKeyboardFocusWithinChanged(DependencyPropertyChangedEventArgs e)
        {
            if (!(bool)e.NewValue &&
                TryGetOwningFlyout(out var owningFlyout) &&
                owningFlyout.IsOpen)
            {
                MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
            }

            base.OnIsKeyboardFocusWithinChanged(e);
        }

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            m_lastInputMode = AppBarButtonInputMode.Default;
            base.OnPreviewKeyDown(e);
        }

        protected override void OnPreviewMouseDown(MouseButtonEventArgs e)
        {
            if (e.StylusDevice == null)
            {
                m_lastInputMode = AppBarButtonInputMode.Default;
            }

            base.OnPreviewMouseDown(e);
        }

        protected override void OnPreviewTouchDown(TouchEventArgs e)
        {
            m_lastInputMode = AppBarButtonInputMode.Touch;
            base.OnPreviewTouchDown(e);
        }

        private void AttachEventHandlers()
        {
            if (m_secondaryItemsRoot != null)
            {
                m_secondaryItemsRoot.SizeChanged += SecondaryItemsRootSizeChanged;
                m_secondaryItemsRoot.PreviewKeyDown += SecondaryItemsRootPreviewKeyDown;
            }

            if (m_overflowPopup != null)
            {
                m_overflowPopup.Opened += OverflowPopupOpened;
                m_overflowPopup.Closed += OverflowPopupClosed;
            }

            if (m_openingStoryboard != null)
            {
                m_openingStoryboard.Completed += OpeningStoryboardCompleted;
                m_openingStoryboard.CurrentStateInvalidated += OpeningStoryboardCurrentStateInvalidated;
            }

            if (m_closingStoryboard != null)
            {
                m_closingStoryboard.Completed += ClosingStoryboardCompleted;
                m_closingStoryboard.CurrentStateInvalidated += ClosingStoryboardCurrentStateInvalidated;
            }

            AttachEventsToSecondaryStoryboards();
        }

        private void DetachEventHandlers()
        {
            CancelAsyncSizeChangeUpdate();
            DetachItemEventHandlers();

            if (m_secondaryItemsRoot != null)
            {
                m_secondaryItemsRoot.PreviewKeyDown -= SecondaryItemsRootPreviewKeyDown;
                m_secondaryItemsRoot.SizeChanged -= SecondaryItemsRootSizeChanged;
            }

            if (m_moreButton is ToggleButton moreToggleButton)
            {
                moreToggleButton.Checked -= MoreButtonChecked;
                moreToggleButton.Unchecked -= MoreButtonUnchecked;
            }

            if (m_secondaryItemsPanel is CommandBarFlyoutOverflowPanel overflowPanel &&
                ReferenceEquals(overflowPanel.OwnerCommandBar, this))
            {
                overflowPanel.OwnerCommandBar = null;
            }

            if (m_overflowPopup != null)
            {
                m_overflowPopup.Opened -= OverflowPopupOpened;
                m_overflowPopup.Closed -= OverflowPopupClosed;
            }

            m_firstItemLoadedRevoker?.Revoke();
            m_firstItemLoadedRevoker = null;

            if (m_openingStoryboard != null)
            {
                m_openingStoryboard.Completed -= OpeningStoryboardCompleted;
                m_openingStoryboard.CurrentStateInvalidated -= OpeningStoryboardCurrentStateInvalidated;
                m_openingStoryboardState = null;
            }

            if (m_closingStoryboard != null)
            {
                StopCloseAnimation();
                m_closingStoryboard.Completed -= ClosingStoryboardCompleted;
                m_closingStoryboard.CurrentStateInvalidated -= ClosingStoryboardCurrentStateInvalidated;
                m_closingStoryboardState = null;
            }

            DetachEventsFromSecondaryStoryboards();
        }

        private void AttachCommandElementsToPanels()
        {
            ClearPanelChildren(m_primaryItemsPanel);
            ClearPanelChildren(m_secondaryItemsPanel);

            if (m_secondaryItemsPanel is CommandBarFlyoutOverflowPanel overflowPanel)
            {
                overflowPanel.OwnerCommandBar = this;
            }

            AddCommandsToPanel(m_primaryItemsPanel, PrimaryCommands, false);
            AddCommandsToPanel(m_secondaryItemsPanel, SecondaryCommands, true);
            UpdateCommandDefaultLabelPositions();
            UpdateCommandOverflowStyleParams();
        }

        private static void ClearPanelChildren(Panel panel)
        {
            if (panel == null)
            {
                return;
            }

            foreach (UIElement child in panel.Children)
            {
                if (child is DependencyObject dependencyObject)
                {
                    AppBarElementProperties.SetIsInOverflow(dependencyObject, false);
                    dependencyObject.ClearValue(AppBarElementProperties.DefaultLabelPositionProperty);
                }
            }

            panel.Children.Clear();
        }

        private static void AddCommandsToPanel(Panel panel, IEnumerable<ICommandBarElement> commands, bool isInOverflow)
        {
            if (panel == null || commands == null)
            {
                return;
            }

            foreach (var command in commands)
            {
                if (command is UIElement element)
                {
                    panel.Children.Add(element);
                    AppBarElementProperties.SetIsInOverflow(element, isInOverflow);
                }
            }
        }

        internal void ReleaseCommandElements()
        {
            ClearPanelChildren(m_primaryItemsPanel);
            ClearPanelChildren(m_secondaryItemsPanel);
        }

        private void UpdateCommandDefaultLabelPositions()
        {
            UpdateCommandDefaultLabelPositions(PrimaryCommands);
            UpdateCommandDefaultLabelPositions(SecondaryCommands);
        }

        private void UpdateCommandOverflowStyleParams()
        {
            AppBarElementProperties.UpdateOverflowStyleParams(PrimaryCommands, false);
            AppBarElementProperties.UpdateOverflowStyleParams(
                SecondaryCommands,
                true,
                GetInputModeForOverflowCommands());
        }

        internal void SetLastInputModeForTesting(AppBarButtonInputMode inputMode)
        {
            m_lastInputMode = inputMode;
        }

        internal AppBarButtonInputMode GetInputModeForOverflowCommands()
        {
            return IsOpen ? m_inputModeUsedToOpen : AppBarButtonInputMode.Default;
        }

        private void UpdateInputDeviceTypeUsedToOpen()
        {
            m_inputModeUsedToOpen = m_lastInputMode;
        }

        private void UpdateCommandDefaultLabelPositions(IEnumerable<ICommandBarElement> commands)
        {
            foreach (var command in commands)
            {
                if (command is DependencyObject dependencyObject)
                {
                    dependencyObject.SetValue(AppBarElementProperties.DefaultLabelPositionProperty, DefaultLabelPosition);
                    (command as IAppBarElement)?.UpdateApplicationViewState();
                }
            }
        }

        private void AttachItemEventHandlers()
        {
            DetachItemEventHandlers();

            AttachItemEventHandlers(PrimaryCommands, true);
            AttachItemEventHandlers(SecondaryCommands, false);
        }

        private void AttachItemEventHandlers(IEnumerable<ICommandBarElement> commands, bool isPrimaryItem)
        {
            if (commands == null)
            {
                return;
            }

            foreach (var command in commands)
            {
                if (command is FrameworkElement commandAsElement)
                {
                    RoutedEventHandler loadedHandler = (sender, args) =>
                    {
                        UpdateItemVisualState(sender as Control, isPrimaryItem);
                        UpdateTemplateSettings();
                    };
                    m_itemLoadedRevokers.Add(new RoutedEventHandlerRevoker(commandAsElement, LoadedEvent, loadedHandler));

                    SizeChangedEventHandler sizeChangedHandler = (sender, args) =>
                    {
                        UpdateItemVisualState(sender as Control, isPrimaryItem);
                        UpdateTemplateSettings();
                    };
                    commandAsElement.SizeChanged += sizeChangedHandler;
                    m_itemSizeChangedHandlers.Add((commandAsElement, sizeChangedHandler));
                }
            }
        }

        private void DetachItemEventHandlers()
        {
            foreach (var revoker in m_itemLoadedRevokers)
            {
                revoker.Revoke();
            }
            m_itemLoadedRevokers.Clear();

            foreach (var handler in m_itemSizeChangedHandlers)
            {
                handler.Element.SizeChanged -= handler.Handler;
            }
            m_itemSizeChangedHandlers.Clear();
        }

        private void UpdateItemVisualState(Control item, bool isPrimaryItem)
        {
            (item as IAppBarElement)?.UpdateApplicationViewState();
        }

        internal void OnCommandBarElementDependencyPropertyChanged()
        {
            UpdateCommandOverflowStyleParams();

            if (IsOpen)
            {
                UpdateUI(true, true);
                QueueSizeChangeUpdate();
            }
        }

        internal void CloseSubMenus(AppBarButton menuToLeaveOpen)
        {
            CloseSubMenus(PrimaryCommands, menuToLeaveOpen);
            CloseSubMenus(SecondaryCommands, menuToLeaveOpen);
        }

        private static void CloseSubMenus(IEnumerable<ICommandBarElement> commands, AppBarButton menuToLeaveOpen)
        {
            foreach (var command in commands)
            {
                if (command is AppBarButton appBarButton && !ReferenceEquals(appBarButton, menuToLeaveOpen))
                {
                    appBarButton.CloseSubMenuTree();
                }
            }
        }

        internal bool HasOpenAnimation()
        {
            return m_openingStoryboard != null && AreCommandBarFlyoutAnimationsEnabled();
        }

        internal void PlayOpenAnimation()
        {
            StopCloseAnimation();
            StopOpenAnimation();
            SetOpacity(1);

            if (m_openingStoryboard != null && m_openingStoryboardState != ClockState.Active)
            {
                if (IsOpen)
                {
                    m_openAnimationPending = true;
                }
                else
                {
                    m_openAnimationPending = false;
                    SetOpacity(0);
                    DispatcherHelper.DoEvents(DispatcherPriority.DataBind);
                    SetOpacity(1);
                    m_openingStoryboard.Begin(m_layoutRoot, true);
                }
            }
        }

        internal bool HasCloseAnimation()
        {
            return m_closingStoryboard != null && AreCommandBarFlyoutAnimationsEnabled();
        }

        internal bool HasSecondaryOpenCloseAnimations()
        {
            return AreCommandBarFlyoutAnimationsEnabled() &&
                   (m_collapsedToExpandedUpStoryboard != null ||
                    m_collapsedToExpandedDownStoryboard != null ||
                    m_expandedUpToCollapsedStoryboard != null ||
                    m_expandedDownToCollapsedStoryboard != null);
        }

        internal void PlayCloseAnimation(Action onCompleteFunc)
        {
            StopOpenAnimation();
            StopCloseAnimation();

            if (m_closingStoryboard != null)
            {
                m_closingStoryboardCompletedCallback = closingStoryboardCompletedCallback;
                m_closingStoryboard.Completed += m_closingStoryboardCompletedCallback;

                void closingStoryboardCompletedCallback(object sender, EventArgs e)
                {
                    if (m_closingStoryboardCompletedCallback != null)
                    {
                        m_closingStoryboard.Completed -= m_closingStoryboardCompletedCallback;
                        m_closingStoryboardCompletedCallback = null;
                    }

                    onCompleteFunc();
                }

                UpdateTemplateSettings();
                m_closingStoryboard.Begin(m_layoutRoot, true);
            }
            else
            {
                onCompleteFunc();
            }
        }

        internal void ClearShadow()
        {
            VisualStateManager.GoToState(this, "NoOuterOverflowContentRootShadow", true);
        }

        internal bool IsOverflowPopupOpenDown()
        {
            if (m_secondaryItemsRoot != null)
            {
                var popupTop = m_secondaryItemsRoot.TranslatePoint(new Point(0, 0), this);
                return popupTop.Y >= 0;
            }

            return true;
        }

        private void UpdateFlowsFromAndFlowsTo()
        {
            var moreButton = m_moreButton;

            EnsureTabStopUniqueness(PrimaryCommands, moreButton);
            EnsureTabStopUniqueness(SecondaryCommands, null);

#if NET48_OR_NEWER
            EnsureAutomationSetCountAndPosition();
#endif

            m_currentPrimaryItemsEndElement = null;
            m_currentSecondaryItemsStartElement = null;

            if (IsOpen)
            {
                bool isElementFocusable(ICommandBarElement element, bool checkTabStop)
                {
                    Control primaryCommandAsControl = element as Control;
                    return IsControlFocusable(primaryCommandAsControl, checkTabStop);
                }

                for (int i = PrimaryCommands.Count - 1; i >= 0; i--)
                {
                    var primaryCommand = PrimaryCommands[i];
                    if (isElementFocusable(primaryCommand, false))
                    {
                        m_currentPrimaryItemsEndElement = primaryCommand as FrameworkElement;
                        break;
                    }
                }

                if (moreButton != null && m_currentPrimaryItemsEndElement != null)
                {
                    m_currentPrimaryItemsEndElement = moreButton;
                }

                foreach (var secondaryCommand in SecondaryCommands)
                {
                    if (isElementFocusable(secondaryCommand, false))
                    {
                        m_currentSecondaryItemsStartElement = secondaryCommand as FrameworkElement;
                        break;
                    }
                }
            }
        }

        private void UpdateUI(bool useTransitions = true, bool isForSizeChange = false)
        {
            UpdateTemplateSettings();
            UpdateVisualState(useTransitions, isForSizeChange);
            UpdateShadow();
        }

        private void QueueSizeChangeUpdate()
        {
            if (m_asyncSizeChangeUpdate != null)
            {
                return;
            }

            m_asyncSizeChangeUpdate = Dispatcher.BeginInvoke(() =>
            {
                m_asyncSizeChangeUpdate = null;
                UpdateUI(false, true);
            }, DispatcherPriority.Render);
        }

        private void UpdateVisualState(bool useTransitions, bool isForSizeChange)
        {
            useTransitions = useTransitions && AreCommandBarFlyoutAnimationsEnabled();

            if (IsOpen)
            {
                if (!m_secondaryItemsRootSized)
                {
                    return;
                }

                bool shouldExpandUp = false;

                if (m_secondaryItemsRoot != null && IsVisible && m_secondaryItemsRoot.IsVisible)
                {
                    UpdateLayout();

                    var overflowPopupTop = m_secondaryItemsRoot.TranslatePoint(new Point(), this);
                    shouldExpandUp = overflowPopupTop.Y < 0;
                }

                if (isForSizeChange)
                {
                    VisualStateManager.GoToState(this, "Collapsed", false);
                }

                void updateExpansionStates()
                {
                    VisualStateManager.GoToState(
                        this,
                        shouldExpandUp ? "ExpandedUp" : "ExpandedDown",
                        useTransitions && !isForSizeChange);
                }

                if (m_openAnimationPending)
                {
                    m_openAnimationPending = false;
                    CancelAsyncOpenAnimation();
                    m_asyncOpenAnimation = Dispatcher.BeginInvoke(() =>
                    {
                        m_asyncOpenAnimation = null;
                        SetOpacity(1);
                        m_openingStoryboard.Begin(m_layoutRoot, true);
                        updateExpansionStates();
                    }, DispatcherPriority.Render);
                }
                else if (m_asyncOpenAnimation == null)
                {
                    updateExpansionStates();
                }

                if (PrimaryCommands.Count != 0)
                {
                    VisualStateManager.GoToState(
                        this,
                        shouldExpandUp ? "ExpandedUpWithPrimaryCommands" : "ExpandedDownWithPrimaryCommands",
                        useTransitions);
                }
                else
                {
                    VisualStateManager.GoToState(
                        this,
                        shouldExpandUp ? "ExpandedUpWithoutPrimaryCommands" : "ExpandedDownWithoutPrimaryCommands",
                        useTransitions);
                }
            }
            else
            {
                StopOpenAnimation();
                VisualStateManager.GoToState(this, "Default", useTransitions);
                VisualStateManager.GoToState(this, "Collapsed", useTransitions);
            }

            UpdatePrimaryLabelStates(useTransitions);
            UpdateAvailableCommandsState(useTransitions);
        }

        private void UpdateAvailableCommandsState(bool useTransitions)
        {
            string stateName;

            if (PrimaryCommands.Count > 0 && SecondaryCommands.Count > 0)
            {
                stateName = "BothCommands";
            }
            else if (SecondaryCommands.Count > 0)
            {
                stateName = "SecondaryCommandsOnly";
            }
            else
            {
                stateName = "PrimaryCommandsOnly";
            }

            VisualStateManager.GoToState(this, stateName, useTransitions);
        }

        private void UpdatePrimaryLabelStates(bool useTransitions)
        {
            bool hasPrimaryCommandLabels = false;

            foreach (var primaryCommand in PrimaryCommands)
            {
                if (HasVisibleLabel(primaryCommand as AppBarButton) ||
                    HasVisibleLabel(primaryCommand as AppBarToggleButton))
                {
                    hasPrimaryCommandLabels = true;
                    break;
                }
            }

            foreach (var command in PrimaryCommands)
            {
                if (command is Control commandControl)
                {
                    VisualStateManager.GoToState(commandControl, hasPrimaryCommandLabels ? "HasPrimaryLabels" : "NoPrimaryLabels", useTransitions);
                }
            }

            foreach (var command in SecondaryCommands)
            {
                if (command is Control commandControl)
                {
                    VisualStateManager.GoToState(commandControl, "NoPrimaryLabels", useTransitions);
                }
            }

            VisualStateManager.GoToState(this, hasPrimaryCommandLabels ? "HasPrimaryLabels" : "NoPrimaryLabels", useTransitions);
        }

        private static bool HasVisibleLabel(AppBarButton button)
        {
            return button != null &&
                button.LabelPosition != CommandBarLabelPosition.Collapsed &&
                !string.IsNullOrEmpty(button.Label);
        }

        private static bool HasVisibleLabel(AppBarToggleButton button)
        {
            return button != null &&
                button.LabelPosition != CommandBarLabelPosition.Collapsed &&
                !string.IsNullOrEmpty(button.Label);
        }

        private void UpdateTemplateSettings()
        {
            if (m_primaryItemsRoot != null && m_secondaryItemsRoot != null)
            {
                var flyoutTemplateSettings = FlyoutTemplateSettings;
                if (flyoutTemplateSettings == null)
                {
                    return;
                }

                double maxWidth = MaxWidth;

                Size infiniteSize = new Size(double.PositiveInfinity, double.PositiveInfinity);
                m_primaryItemsRoot.Measure(infiniteSize);
                Size primaryItemsRootDesiredSize = m_primaryItemsRoot.DesiredSize;
                double collapsedWidth = Math.Min(maxWidth, primaryItemsRootDesiredSize.Width);

                if (m_secondaryItemsRoot != null)
                {
                    m_secondaryItemsRoot.Measure(infiniteSize);
                    var overflowPopupSize = m_secondaryItemsRoot.DesiredSize;

                    flyoutTemplateSettings.ExpandedWidth = Math.Min(maxWidth, Math.Max(collapsedWidth, overflowPopupSize.Width));
                    flyoutTemplateSettings.ExpandUpOverflowVerticalPosition = -overflowPopupSize.Height;
                    flyoutTemplateSettings.ExpandUpAnimationStartPosition = overflowPopupSize.Height / 2;
                    flyoutTemplateSettings.ExpandUpAnimationEndPosition = 0;
                    flyoutTemplateSettings.ExpandUpAnimationHoldPosition = overflowPopupSize.Height;
                    flyoutTemplateSettings.ExpandDownAnimationStartPosition = -overflowPopupSize.Height / 2;
                    flyoutTemplateSettings.ExpandDownAnimationEndPosition = 0;
                    flyoutTemplateSettings.ExpandDownAnimationHoldPosition = -overflowPopupSize.Height;
                    flyoutTemplateSettings.OverflowContentClipRect = new Rect(0, 0, flyoutTemplateSettings.ExpandedWidth, overflowPopupSize.Height + 2);
                }
                else
                {
                    flyoutTemplateSettings.ExpandedWidth = collapsedWidth;
                    flyoutTemplateSettings.ExpandUpOverflowVerticalPosition = 0;
                    flyoutTemplateSettings.ExpandUpAnimationStartPosition = 0;
                    flyoutTemplateSettings.ExpandUpAnimationEndPosition = 0;
                    flyoutTemplateSettings.ExpandUpAnimationHoldPosition = 0;
                    flyoutTemplateSettings.ExpandDownAnimationStartPosition = 0;
                    flyoutTemplateSettings.ExpandDownAnimationEndPosition = 0;
                    flyoutTemplateSettings.ExpandDownAnimationHoldPosition = 0;
                    flyoutTemplateSettings.OverflowContentClipRect = new Rect(0, 0, 0, 0);
                }

                double expandedWidth = flyoutTemplateSettings.ExpandedWidth;

                if (collapsedWidth == 0)
                {
                    collapsedWidth = expandedWidth;
                }

                flyoutTemplateSettings.WidthExpansionDelta = collapsedWidth - expandedWidth;
                flyoutTemplateSettings.WidthExpansionAnimationStartPosition = -flyoutTemplateSettings.WidthExpansionDelta / 2.0;
                flyoutTemplateSettings.WidthExpansionAnimationEndPosition = -flyoutTemplateSettings.WidthExpansionDelta;
                flyoutTemplateSettings.ContentClipRect = new Rect(0, 0, expandedWidth, primaryItemsRootDesiredSize.Height);
                flyoutTemplateSettings.CurrentWidth = IsOpen ? expandedWidth : collapsedWidth;
                UpdateOverflowPopupOffset(collapsedWidth, expandedWidth);

                bool isPlayingCloseAnimation = m_closingStoryboard != null && m_closingStoryboardState == ClockState.Active;

                if (!isPlayingCloseAnimation)
                {
                    if (IsOpen)
                    {
                        flyoutTemplateSettings.OpenAnimationStartPosition = -expandedWidth / 2;
                        flyoutTemplateSettings.OpenAnimationEndPosition = 0;
                    }
                    else
                    {
                        flyoutTemplateSettings.OpenAnimationStartPosition = flyoutTemplateSettings.WidthExpansionDelta - collapsedWidth / 2;
                        flyoutTemplateSettings.OpenAnimationEndPosition = flyoutTemplateSettings.WidthExpansionDelta;
                    }

                    flyoutTemplateSettings.CloseAnimationEndPosition = -expandedWidth;
                }

                flyoutTemplateSettings.WidthExpansionMoreButtonAnimationStartPosition = flyoutTemplateSettings.WidthExpansionDelta / 2;
                flyoutTemplateSettings.WidthExpansionMoreButtonAnimationEndPosition = flyoutTemplateSettings.WidthExpansionDelta;

                if (PrimaryCommands.Count > 0)
                {
                    Height = primaryItemsRootDesiredSize.Height;
                    flyoutTemplateSettings.ExpandDownOverflowVerticalPosition = primaryItemsRootDesiredSize.Height;
                }
                else
                {
                    flyoutTemplateSettings.ExpandDownOverflowVerticalPosition = 0;
                }
            }
        }

        private void UpdateOverflowPopupOffset(double collapsedWidth, double expandedWidth)
        {
            if (m_overflowPopup == null)
            {
                return;
            }

            if (PrimaryCommands.Count > 0)
            {
                m_overflowPopup.HorizontalOffset = (collapsedWidth - expandedWidth) / 2.0 - GetOverflowPopupAlignmentCorrection();
                m_overflowPopup.VerticalOffset = -GetOverflowPopupVerticalAlignmentCorrection();
            }
            else
            {
                m_overflowPopup.HorizontalOffset = 0;
                m_overflowPopup.VerticalOffset = 0;
            }
        }

        private double GetOverflowPopupAlignmentCorrection()
        {
            // The WPF shadow host reserves popup space that WinUI's render-only shadow does not include
            // in layout. Compensate for that reserved inset plus the source ellipsis margin so the
            // visible overflow surface lines up with the primary command surface.
            var popupShadowInset = m_outerOverflowContentRootShadowChrome?.ReservesShadowSpace == true
                ? m_outerOverflowContentRootShadowChrome.PopupPositionShadowPadding.Left
                : 0;
            return popupShadowInset + BorderThickness.Left + BorderThickness.Right + BorderThickness.Right + BorderThickness.Left + GetOverflowPopupEllipsisInset();
        }

        private double GetOverflowPopupVerticalAlignmentCorrection()
        {
            var popupShadowInset = m_outerOverflowContentRootShadowChrome?.ReservesShadowSpace == true
                ? m_outerOverflowContentRootShadowChrome.PopupPositionShadowPadding.Top
                : 0;
            return popupShadowInset + BorderThickness.Bottom + BorderThickness.Bottom;
        }

        private double GetOverflowPopupEllipsisInset()
        {
            if (TryFindResource("CommandBarFlyoutAppBarEllipsisButtonInnerBorderMargin") is Thickness margin)
            {
                return margin.Right;
            }

            return 6;
        }

#if NET48_OR_NEWER
        private void EnsureAutomationSetCountAndPosition()
        {
            var moreButton = m_moreButton;
            int sizeOfSet = 0;

            foreach (var command in PrimaryCommands)
            {
                if (command is UIElement commandAsUIElement)
                {
                    if (commandAsUIElement is AppBarSeparator separator)
                    {
                        if (!separator.IsTabStop)
                        {
                            continue;
                        }
                    }
                    else if (commandAsUIElement.Visibility == Visibility.Visible)
                    {
                        sizeOfSet++;
                    }
                }
            }

            if (moreButton != null && moreButton.Visibility == Visibility.Visible)
            {
                sizeOfSet++;
            }

            int position = 1;

            foreach (var command in PrimaryCommands)
            {
                if (command is UIElement commandAsUIElement)
                {
                    if (commandAsUIElement is AppBarSeparator separator)
                    {
                        if (!separator.IsTabStop)
                        {
                            continue;
                        }
                    }
                    else if (commandAsUIElement.Visibility != Visibility.Visible)
                    {
                        continue;
                    }

                    AutomationProperties.SetSizeOfSet(commandAsUIElement, sizeOfSet);
                    AutomationProperties.SetPositionInSet(commandAsUIElement, position);
                    position++;
                }
            }

            if (moreButton != null)
            {
                AutomationProperties.SetSizeOfSet(moreButton, sizeOfSet);
                AutomationProperties.SetPositionInSet(moreButton, position);
            }
        }
#endif

        private void EnsureFocusedPrimaryCommand()
        {
            foreach (var primaryCommand in PrimaryCommands)
            {
                if (primaryCommand is Control control && control.IsKeyboardFocusWithin)
                {
                    return;
                }
            }

            if (PrimaryCommands.Count > 0)
            {
                FocusCommand(PrimaryCommands, m_moreButton, true, true);
            }
        }

        protected override void OnKeyDown(KeyEventArgs args)
        {
            if (args.Handled)
            {
                return;
            }

            switch (args.Key)
            {
                case Key.Escape:
                    SetCurrentValue(IsOpenProperty, false);

                    if (TryGetOwningFlyout(out var owningFlyout))
                    {
                        owningFlyout.Hide();
                    }

                    args.Handled = true;
                    break;

                case Key.Enter:
                case Key.Space:
                    if (m_moreButton != null && m_moreButton.IsFocused)
                    {
                        IsOpen = true;
                        FocusCommand(SecondaryCommands, null, true, true);
                        args.Handled = true;
                    }
                    break;

                case Key.Down:
                case Key.Up:
                    if (IsOpen && SecondaryCommands.Count > 0 && FocusCommand(SecondaryCommands, null, args.Key == Key.Down, true))
                    {
                        args.Handled = true;
                    }
                    break;
            }

            base.OnKeyDown(args);
        }

        private bool IsControlFocusable(Control control, bool checkTabStop)
        {
            return control != null &&
                control.Visibility == Visibility.Visible &&
                control.IsEnabled &&
                (control.IsTabStop || (!checkTabStop && !(control is AppBarSeparator)));
        }

        private Control GetFirstTabStopControl(IList<ICommandBarElement> commands)
        {
            foreach (var command in commands)
            {
                if (command is Control commandAsControl && commandAsControl.IsTabStop)
                {
                    return commandAsControl;
                }
            }

            return null;
        }

        private bool FocusControl(Control newFocus, Control oldFocus, bool updateTabStop)
        {
            Debug.Assert(newFocus != null);

            if (updateTabStop)
            {
                newFocus.IsTabStop = true;
            }

            if (newFocus.Focus())
            {
                if (oldFocus != null && updateTabStop)
                {
                    oldFocus.IsTabStop = false;
                }

                return true;
            }

            return false;
        }

        private bool FocusCommand(IList<ICommandBarElement> commands, Control moreButton, bool firstCommand, bool ensureTabStopUniqueness)
        {
            Debug.Assert(commands != null);

            Control focusedControl = null;
            int startIndex = 0;
            int endIndex = commands.Count;
            int deltaIndex = 1;

            if (!firstCommand)
            {
                deltaIndex = -1;
                startIndex = endIndex - 1;
                endIndex = -1;
            }

            for (int index = startIndex; index != endIndex; index += deltaIndex)
            {
                var command = commands[index];

                if (command is Control commandAsControl &&
                    IsControlFocusable(commandAsControl, !ensureTabStopUniqueness))
                {
                    if (focusedControl == null)
                    {
                        if (FocusControl(commandAsControl, null, ensureTabStopUniqueness))
                        {
                            if (ensureTabStopUniqueness && moreButton != null && moreButton.IsTabStop)
                            {
                                moreButton.IsTabStop = false;
                            }

                            focusedControl = commandAsControl;

                            if (!ensureTabStopUniqueness)
                            {
                                break;
                            }
                        }
                    }
                    else if (commandAsControl.IsTabStop)
                    {
                        commandAsControl.IsTabStop = false;
                    }
                }
            }

            return focusedControl != null;
        }

        private void EnsureTabStopUniqueness(IList<ICommandBarElement> commands, Control moreButton)
        {
            Debug.Assert(commands != null);

            bool tabStopSeen = moreButton != null && moreButton.IsTabStop;

            if (tabStopSeen || GetFirstTabStopControl(commands) != null)
            {
                foreach (var command in commands)
                {
                    if (command is Control commandAsControl &&
                        IsControlFocusable(commandAsControl, false) &&
                        commandAsControl.IsTabStop)
                    {
                        if (!tabStopSeen)
                        {
                            tabStopSeen = true;
                        }
                        else
                        {
                            commandAsControl.IsTabStop = false;
                        }
                    }
                }
            }
            else
            {
                foreach (var command in commands)
                {
                    if (command is Control commandAsControl && IsControlFocusable(commandAsControl, false))
                    {
                        commandAsControl.IsTabStop = true;
                        break;
                    }
                }
            }
        }

        private void UpdateShadow()
        {
            if (!IsOpen || !m_secondaryItemsRootSized)
            {
                VisualStateManager.GoToState(this, "NoOuterOverflowContentRootShadow", true);
                return;
            }

            var shouldUseOverflowShadow = PrimaryCommands.Count == 0 || IsOverflowPopupOpenDown();
            VisualStateManager.GoToState(
                this,
                shouldUseOverflowShadow ? "OuterOverflowContentRootShadow" : "NoOuterOverflowContentRootShadow",
                true);
        }

        private void AttachEventsToSecondaryStoryboards()
        {
            if (m_collapsedToExpandedUpStoryboard != null)
            {
                m_collapsedToExpandedUpStoryboard.Completed += SecondaryOpenCloseStoryboardCompleted;
            }

            if (m_collapsedToExpandedDownStoryboard != null)
            {
                m_collapsedToExpandedDownStoryboard.Completed += SecondaryOpenCloseStoryboardCompleted;
            }

            if (m_expandedUpToCollapsedStoryboard != null)
            {
                m_expandedUpToCollapsedStoryboard.Completed += SecondaryOpenCloseStoryboardCompleted;
            }

            if (m_expandedDownToCollapsedStoryboard != null)
            {
                m_expandedDownToCollapsedStoryboard.Completed += SecondaryOpenCloseStoryboardCompleted;
            }
        }

        private void DetachEventsFromSecondaryStoryboards()
        {
            if (m_collapsedToExpandedUpStoryboard != null)
            {
                m_collapsedToExpandedUpStoryboard.Completed -= SecondaryOpenCloseStoryboardCompleted;
                m_collapsedToExpandedUpStoryboard = null;
            }

            if (m_collapsedToExpandedDownStoryboard != null)
            {
                m_collapsedToExpandedDownStoryboard.Completed -= SecondaryOpenCloseStoryboardCompleted;
                m_collapsedToExpandedDownStoryboard = null;
            }

            if (m_expandedUpToCollapsedStoryboard != null)
            {
                m_expandedUpToCollapsedStoryboard.Completed -= SecondaryOpenCloseStoryboardCompleted;
                m_expandedUpToCollapsedStoryboard = null;
            }

            if (m_expandedDownToCollapsedStoryboard != null)
            {
                m_expandedDownToCollapsedStoryboard.Completed -= SecondaryOpenCloseStoryboardCompleted;
                m_expandedDownToCollapsedStoryboard = null;
            }
        }

        private void SecondaryOpenCloseStoryboardCompleted(object sender, EventArgs e)
        {
            if (SharedHelpers.IsAnimationsEnabled &&
                TryGetOwningFlyout(out var owningFlyout) &&
                owningFlyout.IsOpen)
            {
                owningFlyout.AddDropShadow();
            }
        }

        private void BindOwningFlyoutPresenterToCornerRadius()
        {
            if (TryGetOwningFlyout(out var actualFlyout) &&
                GetTemplateChild("LayoutRoot") is Border root)
            {
                Binding binding = new();
                binding.Source = root;
                binding.Path = new PropertyPath("CornerRadius");
                binding.Mode = BindingMode.OneWay;
                if (actualFlyout.GetPresenter() is { } presenter)
                {
                    presenter.SetBinding(System.Windows.Controls.Border.CornerRadiusProperty, binding);
                }
            }
        }

        private void SecondaryItemsRootSizeChanged(object sender, SizeChangedEventArgs e)
        {
            m_secondaryItemsRootSized = true;
            UpdateUI(true, true);
        }

        private void SecondaryItemsRootPreviewKeyDown(object sender, KeyEventArgs args)
        {
            if (args.Handled)
            {
                return;
            }

            switch (args.Key)
            {
                case Key.Escape:
                    SetCurrentValue(IsOpenProperty, false);

                    if (TryGetOwningFlyout(out var owningFlyout))
                    {
                        owningFlyout.Hide();
                    }
                    args.Handled = true;
                    break;
            }
        }

        private void OverflowPopupOpened(object sender, EventArgs e)
        {
            m_secondaryItemsRootSized = true;
            UpdateFlowsFromAndFlowsTo();
            UpdateUI();
            Opened?.Invoke(this, null);
        }

        private void OverflowPopupClosed(object sender, EventArgs e)
        {
            if (IsOpen)
            {
                SetCurrentValue(IsOpenProperty, false);
            }

            m_secondaryItemsRootSized = false;
            UpdateFlowsFromAndFlowsTo();
            UpdateUI();
            Closed?.Invoke(this, null);
        }

        private void MoreButtonChecked(object sender, RoutedEventArgs e)
        {
            if (!IsOpen)
            {
                SetCurrentValue(IsOpenProperty, true);
            }
        }

        private void MoreButtonUnchecked(object sender, RoutedEventArgs e)
        {
            if (IsOpen)
            {
                SetCurrentValue(IsOpenProperty, false);
            }
        }

        private void OpeningStoryboardCompleted(object sender, EventArgs e)
        {
            m_openingStoryboard.Stop(m_layoutRoot);
            m_openingStoryboardState = null;
            SetOpacity(1);
        }

        private void ClosingStoryboardCompleted(object sender, EventArgs e)
        {
            m_closingStoryboard.Stop(m_layoutRoot);
            m_closingStoryboardState = null;
            SetOpacity(1);
        }

        private void OpeningStoryboardCurrentStateInvalidated(object sender, EventArgs e)
        {
            var clock = (Clock)sender;
            m_openingStoryboardState = clock.CurrentState;
        }

        private void ClosingStoryboardCurrentStateInvalidated(object sender, EventArgs e)
        {
            var clock = (Clock)sender;
            m_closingStoryboardState = clock.CurrentState;
        }

        private void CancelAsyncOpenAnimation()
        {
            if (m_asyncOpenAnimation != null)
            {
                m_asyncOpenAnimation.Abort();
                m_asyncOpenAnimation = null;
            }
        }

        private void CancelAsyncSizeChangeUpdate()
        {
            if (m_asyncSizeChangeUpdate != null)
            {
                m_asyncSizeChangeUpdate.Abort();
                m_asyncSizeChangeUpdate = null;
            }
        }

        private void StopOpenAnimation()
        {
            CancelAsyncOpenAnimation();

            if (m_openAnimationPending)
            {
                m_openAnimationPending = false;
                SetOpacity(1);
            }

            if (m_openingStoryboard != null && m_openingStoryboardState == ClockState.Active)
            {
                m_openingStoryboard.Stop(m_layoutRoot);
            }
        }

        private void StopCloseAnimation()
        {
            if (m_closingStoryboardCompletedCallback != null && m_closingStoryboard != null)
            {
                m_closingStoryboard.Completed -= m_closingStoryboardCompletedCallback;
                m_closingStoryboardCompletedCallback = null;
            }

            if (m_closingStoryboard != null && m_closingStoryboardState == ClockState.Active)
            {
                m_closingStoryboard.Stop(m_layoutRoot);
            }

            m_closingStoryboardState = null;
        }

        private void OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!IsOpen && e.Handled && e.OriginalSource == this)
            {
                if (TryGetOwningFlyout(out var owningFlyout))
                {
                    owningFlyout.Hide();
                }
            }
        }

        private bool TryGetOwningFlyout(out CommandBarFlyout flyout)
        {
            var reference = OwningFlyout;
            if (reference != null)
            {
                return reference.TryGetTarget(out flyout);
            }

            flyout = null;
            return false;
        }

        private bool AreCommandBarFlyoutAnimationsEnabled()
        {
            return SharedHelpers.IsAnimationsEnabled;
        }

        private void UpdateOverflowPopupVisibility(bool isOpen)
        {
            bool shouldShowOverflow = isOpen && SecondaryCommands.Count > 0;

            if (m_secondaryItemsRoot != null)
            {
                m_secondaryItemsRoot.Visibility = shouldShowOverflow ? Visibility.Visible : Visibility.Collapsed;
            }

            if (m_overflowPopup == null)
            {
                return;
            }

            if (shouldShowOverflow)
            {
                if (!m_overflowPopup.IsOpen)
                {
                    m_overflowPopup.SetCurrentValue(Popup.IsOpenProperty, true);
                }
            }
            else if (m_overflowPopup.IsOpen)
            {
                m_overflowPopup.SetCurrentValue(Popup.IsOpenProperty, false);
            }
        }

        private void SetOpacity(double value)
        {
            if (m_layoutRoot != null)
            {
                m_layoutRoot.Opacity = value;
            }

            if (m_secondaryItemsRoot != null)
            {
                m_secondaryItemsRoot.Opacity = value;
            }
        }

        private void UpdateHasOverflowItems()
        {
            HasOverflowItems = SecondaryCommands.Count > 0;
            UpdateEffectiveOverflowButtonVisibility();
        }

        private void UpdateEffectiveOverflowButtonVisibility()
        {
            bool visible = true;

            switch (OverflowButtonVisibility)
            {
                case CommandBarOverflowButtonVisibility.Auto:
                    visible = HasOverflowItems;
                    break;
                case CommandBarOverflowButtonVisibility.Collapsed:
                    visible = false;
                    break;
            }

            EffectiveOverflowButtonVisibility = visible ? Visibility.Visible : Visibility.Collapsed;
        }

        private WeakReference<CommandBarFlyout> m_owningFlyout;

        private FrameworkElement m_layoutRoot;
        private FrameworkElement m_primaryItemsRoot;
        private FrameworkElement m_secondaryItemsRoot;
        private Panel m_primaryItemsPanel;
        private Panel m_secondaryItemsPanel;
        private ButtonBase m_moreButton;
        private Popup m_overflowPopup;
        private ThemeShadowChrome m_outerOverflowContentRootShadowChrome;
        private RoutedEventHandlerRevoker m_firstItemLoadedRevoker;
        private readonly List<RoutedEventHandlerRevoker> m_itemLoadedRevokers = new();
        private readonly List<(FrameworkElement Element, SizeChangedEventHandler Handler)> m_itemSizeChangedHandlers = new();
        private AppBarButtonInputMode m_lastInputMode;
        private AppBarButtonInputMode m_inputModeUsedToOpen;

        private FrameworkElement m_currentPrimaryItemsEndElement;
        private FrameworkElement m_currentSecondaryItemsStartElement;

        private Storyboard m_openingStoryboard;
        private Storyboard m_closingStoryboard;
        private ClockState? m_openingStoryboardState;
        private ClockState? m_closingStoryboardState;
        private EventHandler m_closingStoryboardCompletedCallback;
        private Storyboard m_collapsedToExpandedUpStoryboard;
        private Storyboard m_collapsedToExpandedDownStoryboard;
        private Storyboard m_expandedUpToCollapsedStoryboard;
        private Storyboard m_expandedDownToCollapsedStoryboard;

        private bool m_secondaryItemsRootSized;
        private bool m_openAnimationPending;
        private DispatcherOperation m_asyncOpenAnimation;
        private DispatcherOperation m_asyncSizeChangeUpdate;

        private const string PrimaryItemsPanelName = "PrimaryItemsPanel";
        private const string SecondaryItemsPanelName = "SecondaryItemsPanel";
        private const string OverflowPopupName = "OverflowPopup";
    }
}
