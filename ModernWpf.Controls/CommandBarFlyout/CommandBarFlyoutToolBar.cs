// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace ModernWpf.Controls.Primitives
{
    public class CommandBarFlyoutToolBar : CommandBarToolBar
    {
        static CommandBarFlyoutToolBar()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(CommandBarFlyoutToolBar),
                new FrameworkPropertyMetadata(typeof(CommandBarFlyoutToolBar)));

            IsOverflowOpenProperty.OverrideMetadata(typeof(CommandBarFlyoutToolBar),
                new FrameworkPropertyMetadata(OnOverflowOpenChanged));
        }

        public CommandBarFlyoutToolBar()
        {
            SetValue(FlyoutTemplateSettingsPropertyKey, new CommandBarFlyoutCommandBarTemplateSettings());

            Loaded += delegate
            {
                UpdateUI();

                // Programmatically focus the first primary command if any, else programmatically focus the first secondary command if any.
                var commands = PrimaryCommands.Count > 0 ? PrimaryCommands : (SecondaryCommands.Count > 0 ? SecondaryCommands : null);

                if (commands != null)
                {
                    bool usingPrimaryCommands = commands == PrimaryCommands;
                    bool ensureTabStopUniqueness = usingPrimaryCommands || true;
                    var firstCommandAsFrameworkElement = commands[0] as FrameworkElement;

                    if (firstCommandAsFrameworkElement != null)
                    {
                        if (SharedHelpers.IsFrameworkElementLoaded(firstCommandAsFrameworkElement))
                        {
                            FocusCommand(
                                commands,
                                usingPrimaryCommands ? m_moreButton : null /*moreButton*/,
                                true /*firstCommand*/,
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
                                        usingPrimaryCommands ? m_moreButton : null /*moreButton*/,
                                        true /*firstCommand*/,
                                        ensureTabStopUniqueness);
                                    m_firstItemLoadedRevoker?.Revoke();
                                }));
                        }
                    }
                }
            };

            Unloaded += delegate
            {
                StopOpenAnimation();
                SetOpacity(1);
            };

            SizeChanged += delegate
            {
                UpdateUI();
            };

            OverflowOpened += delegate
            {
                m_secondaryItemsRootSized = true;

                UpdateFlowsFromAndFlowsTo();
                UpdateUI();
            };

            OverflowClosed += delegate
            {
                m_secondaryItemsRootSized = false;

                if (PrimaryCommands.Count > 0)
                {
                    // Before RS3, ensure the focus goes to a primary command when
                    // the secondary commands are closed.
                    EnsureFocusedPrimaryCommand();
                }
            };

            AddHandler(MouseDownEvent, new MouseButtonEventHandler(OnMouseDown), true);
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            PrimaryCommands.CollectionChanged += delegate
            {
                UpdateFlowsFromAndFlowsTo();
                UpdateUI();
            };

            SecondaryCommands.CollectionChanged += delegate
            {
                m_secondaryItemsRootSized = false;
                UpdateFlowsFromAndFlowsTo();
                UpdateUI();
            };
        }

        #region FlyoutTemplateSettings

        private static readonly DependencyPropertyKey FlyoutTemplateSettingsPropertyKey =
            DependencyProperty.RegisterReadOnly(
                nameof(FlyoutTemplateSettings),
                typeof(CommandBarFlyoutCommandBarTemplateSettings),
                typeof(CommandBarFlyoutToolBar),
                null);

        public static readonly DependencyProperty FlyoutTemplateSettingsProperty =
            FlyoutTemplateSettingsPropertyKey.DependencyProperty;

        public CommandBarFlyoutCommandBarTemplateSettings FlyoutTemplateSettings =>
            (CommandBarFlyoutCommandBarTemplateSettings)GetValue(FlyoutTemplateSettingsProperty);

        #endregion

        private ObservableCollection<ICommandBarElement> PrimaryCommands => (TemplatedParent as CommandBar)?.PrimaryCommands;

        private ObservableCollection<ICommandBarElement> SecondaryCommands => (TemplatedParent as CommandBar)?.SecondaryCommands;

        private bool IsOpen
        {
            get => IsOverflowOpen;
            set => IsOverflowOpen = value;
        }

        private WeakReference<CommandBarFlyout> OwningFlyout => (TemplatedParent as CommandBarFlyoutCommandBar)?.OwningFlyout;

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            DetachEventHandlers();

            m_layoutRoot = GetTemplateChild("LayoutRoot") as FrameworkElement;
            m_primaryItemsRoot = GetTemplateChild("PrimaryItemsRoot") as FrameworkElement;
            m_secondaryItemsRoot = GetTemplateChild("OverflowContentRoot") as FrameworkElement;
            m_moreButton = GetTemplateChild("MoreButton") as ButtonBase;

            if (m_layoutRoot != null)
            {
                m_openingStoryboard = m_layoutRoot.Resources["OpeningStoryboard"] as Storyboard;
                m_closingStoryboard = m_layoutRoot.Resources["ClosingStoryboard"] as Storyboard;
            }

            if (m_moreButton != null)
            {
                // Initially only the first focusable primary and secondary commands
                // keep their IsTabStop set to True.
                if (m_moreButton.IsTabStop)
                {
                    m_moreButton.IsTabStop = false;
                }
            }

            if (OverflowPopup is PopupEx popupEx)
            {
                popupEx.SuppressFadeAnimation = true;
            }

            AttachEventHandlers();
            UpdateFlowsFromAndFlowsTo();
            UpdateUI(false /* useTransitions */);
        }

        protected override void OnIsKeyboardFocusWithinChanged(DependencyPropertyChangedEventArgs e)
        {
            if (!(bool)e.NewValue)
            {
                if (TryGetOwningFlyout(out var owningFlyout))
                {
                    if (owningFlyout.IsOpen)
                    {
                        MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
                    }
                }
            }

            base.OnIsKeyboardFocusWithinChanged(e);
        }

        private static void OnOverflowOpenChanged(DependencyObject element, DependencyPropertyChangedEventArgs e)
        {
            ((CommandBarFlyoutToolBar)element).OnOverflowOpenChanged(e);
        }

        private void OnOverflowOpenChanged(DependencyPropertyChangedEventArgs e)
        {
            UpdateFlowsFromAndFlowsTo();
            UpdateUI();
        }

        void AttachEventHandlers()
        {
            if (m_secondaryItemsRoot != null)
            {
                m_secondaryItemsRoot.SizeChanged += SecondaryItemsRootSizeChanged;
                m_secondaryItemsRoot.PreviewKeyDown += SecondaryItemsRootPreviewKeyDown;
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
        }

        void DetachEventHandlers()
        {
            if (m_secondaryItemsRoot != null)
            {
                m_secondaryItemsRoot.PreviewKeyDown -= SecondaryItemsRootPreviewKeyDown;
                m_secondaryItemsRoot.SizeChanged -= SecondaryItemsRootSizeChanged;
            }

            m_firstItemLoadedRevoker?.Revoke();

            if (m_openingStoryboard != null)
            {
                m_openingStoryboard.Completed -= OpeningStoryboardCompleted;
                m_openingStoryboard.CurrentStateInvalidated -= OpeningStoryboardCurrentStateInvalidated;
                m_openingStoryboardState = null;
            }

            if (m_closingStoryboard != null)
            {
                m_closingStoryboard.Completed -= ClosingStoryboardCompleted;
                m_closingStoryboard.CurrentStateInvalidated -= ClosingStoryboardCurrentStateInvalidated;
                m_closingStoryboardState = null;
            }
        }

        internal bool HasOpenAnimation()
        {
            return m_openingStoryboard != null && SharedHelpers.IsAnimationsEnabled;
        }

        internal void PlayOpenAnimation()
        {
            StopOpenAnimation();

            if (m_openingStoryboard != null)
            {
                if (m_openingStoryboardState != ClockState.Active)
                {
                    if (TemplatedParent is CommandBar commandBar && commandBar.IsOpen)
                    {
                        m_openAnimationPending = true;
                        SetOpacity(0);
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
        }

        internal bool HasCloseAnimation()
        {
            return m_closingStoryboard != null && SharedHelpers.IsAnimationsEnabled;
        }

        internal void PlayCloseAnimation(
            Action onCompleteFunc)
        {
            StopOpenAnimation();

            if (m_closingStoryboard != null)
            {
                if (m_closingStoryboardState != ClockState.Active)
                {
                    m_closingStoryboard.Completed += closingStoryboardCompletedCallback;
                    void closingStoryboardCompletedCallback(object sender, EventArgs e)
                    {
                        m_closingStoryboard.Completed -= closingStoryboardCompletedCallback;
                        onCompleteFunc();
                    }

                    UpdateTemplateSettings();
                    m_closingStoryboard.Begin(m_layoutRoot, true);
                }
            }
            else
            {
                onCompleteFunc();
            }
        }

        void UpdateFlowsFromAndFlowsTo()
        {
            var moreButton = m_moreButton;

            // Ensure there is only one focusable command with IsTabStop set to True
            // to enable tabbing from primary to secondary commands and vice-versa
            // with a single Tab keystroke.
            EnsureTabStopUniqueness(PrimaryCommands, moreButton);
            EnsureTabStopUniqueness(SecondaryCommands, null);

            // Ensure the SizeOfSet and PositionInSet automation properties
            // for the primary commands and the MoreButton account for the
            // potential MoreButton.
#if NET48_OR_NEWER
            EnsureAutomationSetCountAndPosition();
#endif

            if (m_currentPrimaryItemsEndElement != null)
            {
                //AutomationProperties.GetFlowsTo(m_currentPrimaryItemsEndElement).Clear();
                m_currentPrimaryItemsEndElement = null;
            }

            if (m_currentSecondaryItemsStartElement != null)
            {
                //AutomationProperties.GetFlowsFrom(m_currentSecondaryItemsStartElement).Clear();
                m_currentSecondaryItemsStartElement = null;
            }

            // If we're not open, then we don't want to do anything special - the only time we do need to do something special
            // is when the secondary commands are showing, in which case we want to connect the primary and secondary command lists.
            if (IsOpen)
            {
                bool isElementFocusable(ICommandBarElement element, bool checkTabStop)
                {
                    Control primaryCommandAsControl = element as Control;
                    return IsControlFocusable(primaryCommandAsControl, checkTabStop);
                };

                var primaryCommands = PrimaryCommands;
                for (int i = primaryCommands.Count - 1; i >= 0; i--)
                {
                    var primaryCommand = primaryCommands[i];
                    if (isElementFocusable(primaryCommand, false /*checkTabStop*/))
                    {
                        m_currentPrimaryItemsEndElement = primaryCommand as FrameworkElement;
                        break;
                    }
                }

                // If we have a more button and at least one focusable primary item, then
                // we'll use the more button as the last element in our primary items list.
                if (moreButton != null && m_currentPrimaryItemsEndElement != null)
                {
                    m_currentPrimaryItemsEndElement = moreButton;
                }

                foreach (var secondaryCommand in SecondaryCommands)
                {
                    if (isElementFocusable(secondaryCommand, false /*checkTabStop*/))
                    {
                        m_currentSecondaryItemsStartElement = secondaryCommand as FrameworkElement;
                        break;
                    }
                }

                /*if (m_currentPrimaryItemsEndElement && m_currentSecondaryItemsStartElement)
                {
                    AutomationProperties.GetFlowsTo(m_currentPrimaryItemsEndElement).Append(m_currentSecondaryItemsStartElement);
                    AutomationProperties.GetFlowsFrom(m_currentSecondaryItemsStartElement).Append(m_currentPrimaryItemsEndElement);
                }*/
            }
        }

        void UpdateUI(
            bool useTransitions = true)
        {
            UpdateTemplateSettings();
            UpdateVisualState(useTransitions);

            UpdateShadow();

            /*if (OverflowPopup != null && OverflowPopup.IsOpen)
            {
                OverflowPopup.Reposition();
            }*/
        }

        void UpdateVisualState(
            bool useTransitions)
        {
            if (IsOpen)
            {
                // If we're currently open, have overflow items, and haven't yet sized our overflow item root,
                // then we want to wait until then to update visual state - otherwise, we'll be animating
                // to incorrect values.  Animations only retrieve values from bindings when they begin,
                // so if we begin an animation and then update a bound template setting, that won't take effect.
                if (!m_secondaryItemsRootSized)
                {
                    return;
                }

                bool shouldExpandUp = false;

                // If there isn't enough space to display the overflow below the command bar,
                // and if there is enough space above, then we'll display it above instead.
                if (m_secondaryItemsRoot != null)
                {
                    if (IsVisible && m_secondaryItemsRoot.IsVisible)
                    {
                        UpdateLayout();

                        var overflowPopupTop = m_secondaryItemsRoot.TranslatePoint(new Point(), this);

                        shouldExpandUp = overflowPopupTop.Y < 0;
                    }
                }

                void updateExpansionStates()
                {
                    if (shouldExpandUp)
                    {
                        VisualStateManager.GoToState(this, "ExpandedUp", useTransitions);
                    }
                    else
                    {
                        VisualStateManager.GoToState(this, "ExpandedDown", useTransitions);
                    }
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

                // Union of AvailableCommandsStates and ExpansionStates
                bool hasPrimaryCommands = PrimaryCommands.Count != 0;
                if (hasPrimaryCommands)
                {
                    if (shouldExpandUp)
                    {
                        VisualStateManager.GoToState(this, "ExpandedUpWithPrimaryCommands", useTransitions);
                    }
                    else
                    {
                        VisualStateManager.GoToState(this, "ExpandedDownWithPrimaryCommands", useTransitions);
                    }
                }
                else
                {
                    if (shouldExpandUp)
                    {
                        VisualStateManager.GoToState(this, "ExpandedUpWithoutPrimaryCommands", useTransitions);
                    }
                    else
                    {
                        VisualStateManager.GoToState(this, "ExpandedDownWithoutPrimaryCommands", useTransitions);
                    }
                }
            }
            else
            {
                VisualStateManager.GoToState(this, "Default", useTransitions);
                VisualStateManager.GoToState(this, "Collapsed", useTransitions);
            }
        }

        void UpdateTemplateSettings()
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
                    // This clip needs to cover the border at the bottom of the overflow otherwise it'll 
                    // clip the border. The measure size seems slightly off from what we eventually require
                    // so we're going to compensate just a bit to make sure there's room for any borders.
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

                // If collapsedWidth is 0, then we'll never be showing in collapsed mode,
                // so we'll set it equal to expandedWidth to ensure that our open/close animations are correct.
                if (collapsedWidth == 0)
                {
                    collapsedWidth = expandedWidth;
                }

                flyoutTemplateSettings.WidthExpansionDelta = collapsedWidth - expandedWidth;
                flyoutTemplateSettings.WidthExpansionAnimationStartPosition = -flyoutTemplateSettings.WidthExpansionDelta / 2.0;
                flyoutTemplateSettings.WidthExpansionAnimationEndPosition = -flyoutTemplateSettings.WidthExpansionDelta;
                flyoutTemplateSettings.ContentClipRect = new Rect(0, 0, expandedWidth, primaryItemsRootDesiredSize.Height);

                if (IsOpen)
                {
                    flyoutTemplateSettings.CurrentWidth = expandedWidth;
                }
                else
                {
                    flyoutTemplateSettings.CurrentWidth = collapsedWidth;
                }

                // If we're currently playing the close animation, don't update these properties -
                // the animation is expecting them not to change out from under it.
                // After the close animation has completed, the flyout will close and no further
                // visual updates will occur, so there's no need to update these values in that case.
                bool isPlayingCloseAnimation = false;

                var closingStoryboard = m_closingStoryboard;
                if (closingStoryboard != null)
                {
                    isPlayingCloseAnimation = m_closingStoryboardState == ClockState.Active;
                }

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
                    flyoutTemplateSettings.ExpandDownOverflowVerticalPosition = Height;
                }
                else
                {
                    flyoutTemplateSettings.ExpandDownOverflowVerticalPosition = 0;
                }
            }
        }

#if NET48_OR_NEWER
        void EnsureAutomationSetCountAndPosition()
        {
            var moreButton = m_moreButton;
            int sizeOfSet = 0;

            foreach (var command in PrimaryCommands)
            {
                if (command is UIElement commandAsUIElement)
                {
                    if (commandAsUIElement.Visibility == Visibility.Visible)
                    {
                        sizeOfSet++;
                    }
                }
            }

            if (moreButton != null)
            {
                // Accounting for the MoreButton
                sizeOfSet++;
            }

            foreach (var command in PrimaryCommands)
            {
                if (command is UIElement commandAsUIElement)
                {
                    if (commandAsUIElement.Visibility == Visibility.Visible)
                    {
                        AutomationProperties.SetSizeOfSet(commandAsUIElement, sizeOfSet);
                    }
                }
            }

            if (moreButton != null)
            {
                AutomationProperties.SetSizeOfSet(moreButton, sizeOfSet);
                AutomationProperties.SetPositionInSet(moreButton, sizeOfSet);
            }
        }
#endif

        void EnsureFocusedPrimaryCommand()
        {
            var moreButton = m_moreButton;
            var tabStopControl = GetFirstTabStopControl(PrimaryCommands);

            if (tabStopControl == null)
            {
                if (moreButton != null && moreButton.IsTabStop)
                {
                    tabStopControl = moreButton;
                }
            }

            if (tabStopControl != null)
            {
                if (!tabStopControl.IsFocused)
                {
                    FocusControl(
                        tabStopControl /*newFocus*/,
                        null /*oldFocus*/,
                        false /*updateTabStop*/);
                }
            }
            else
            {
                FocusCommand(
                    PrimaryCommands /*commands*/,
                    moreButton /*moreButton*/,
                    true /*firstCommand*/,
                    true /*ensureTabStopUniqueness*/);
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
                case Key.Tab:
                    {
                        if (SecondaryCommands.Count > 0 && !IsOpen)
                        {
                            // Ensure the secondary commands flyout is open ...
                            IsOpen = true;

                            // ... and focus the first focusable command
                            FocusCommand(
                                SecondaryCommands /*commands*/,
                                null /*moreButton*/,
                                true /*firstCommand*/,
                                true /*ensureTabStopUniqueness*/);
                        }
                        break;
                    }

                case Key.Escape:
                    {
                        if (TryGetOwningFlyout(out var owningFlyout))
                        {
                            owningFlyout.Hide();
                            args.Handled = true;
                        }
                        break;
                    }

                case Key.Right:
                case Key.Left:
                case Key.Down:
                case Key.Up:
                    {
                        bool isRightToLeft = m_primaryItemsRoot != null && m_primaryItemsRoot.FlowDirection == FlowDirection.RightToLeft;
                        bool isLeft = (args.Key == Key.Left && !isRightToLeft) || (args.Key == Key.Right && isRightToLeft);
                        bool isRight = (args.Key == Key.Right && !isRightToLeft) || (args.Key == Key.Left && isRightToLeft);
                        bool isDown = args.Key == Key.Down;
                        bool isUp = args.Key == Key.Up;

                        var moreButton = m_moreButton;

                        if (isDown &&
                            moreButton != null &&
                            moreButton.IsFocused &&
                            SecondaryCommands.Count > 0)
                        {
                            // When on the MoreButton, give keyboard focus to the first focusable secondary command
                            // First ensure the secondary commands flyout is open
                            if (!IsOpen)
                            {
                                IsOpen = true;
                            }

                            if (FocusCommand(
                                    SecondaryCommands /*commands*/,
                                    null /*moreButton*/,
                                    true /*firstCommand*/,
                                    true /*ensureTabStopUniqueness*/))
                            {
                                args.Handled = true;
                            }
                        }

                        if (!args.Handled && PrimaryCommands.Count > 0)
                        {
                            Control focusedControl = null;
                            int startIndex = 0;
                            int endIndex = PrimaryCommands.Count;
                            int deltaIndex = 1;

                            if (isLeft || isUp)
                            {
                                deltaIndex = -1;
                                startIndex = endIndex - 1;
                                endIndex = -1;

                                if (moreButton != null && moreButton.IsFocused)
                                {
                                    focusedControl = moreButton;
                                }
                            }

                            // Give focus to the previous or next command if possible
                            for (int index = startIndex; index != endIndex; index += deltaIndex)
                            {
                                var primaryCommand = PrimaryCommands[index];

                                if (primaryCommand is Control primaryCommandAsControl)
                                {
                                    if (primaryCommandAsControl.IsFocused)
                                    {
                                        focusedControl = primaryCommandAsControl;
                                    }
                                    else if (focusedControl != null &&
                                        IsControlFocusable(primaryCommandAsControl, false /*checkTabStop*/) &&
                                        FocusControl(
                                            primaryCommandAsControl /*newFocus*/,
                                            focusedControl /*oldFocus*/,
                                            true /*updateTabStop*/))
                                    {
                                        args.Handled = true;
                                        break;
                                    }
                                }
                            }

                            if (!args.Handled)
                            {
                                if ((isRight || isDown) &&
                                    focusedControl != null &&
                                    moreButton != null &&
                                    IsControlFocusable(moreButton, false /*checkTabStop*/))
                                {
                                    // When on last primary command, give keyboard focus to the MoreButton
                                    if (FocusControl(
                                            moreButton /*newFocus*/,
                                            focusedControl /*oldFocus*/,
                                            true /*updateTabStop*/))
                                    {
                                        args.Handled = true;
                                    }
                                }
                                else if (isUp && SecondaryCommands.Count > 0)
                                {
                                    // When on first primary command, give keyboard focus to the last focusable secondary command
                                    // First ensure the secondary commands flyout is open
                                    if (!IsOpen)
                                    {
                                        IsOpen = true;
                                    }

                                    if (FocusCommand(
                                            SecondaryCommands /*commands*/,
                                            null /*moreButton*/,
                                            false /*firstCommand*/,
                                            true /*ensureTabStopUniqueness*/))
                                    {
                                        args.Handled = true;
                                    }
                                }
                            }
                        }

                        if (!args.Handled)
                        {
                            // Occurs for example with Right key while MoreButton has focus. Stay on that MoreButton.
                            args.Handled = true;
                        }
                        break;
                    }
            }

            base.OnKeyDown(args);
        }

        bool IsControlFocusable(
            Control control,
            bool checkTabStop)
        {
            return control != null &&
                control.Visibility == Visibility.Visible &&
                control.IsEnabled &&
                (!checkTabStop || control.IsTabStop);
        }

        Control GetFirstTabStopControl(
            IList<ICommandBarElement> commands)
        {
            foreach (var command in commands)
            {
                if (command is Control commandAsControl)
                {
                    if (commandAsControl.IsTabStop)
                    {
                        return commandAsControl;
                    }
                }
            }
            return null;
        }

        bool FocusControl(
            Control newFocus,
            Control oldFocus,
            bool updateTabStop)
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

        bool FocusCommand(
            IList<ICommandBarElement> commands,
            Control moreButton,
            bool firstCommand,
            bool ensureTabStopUniqueness)
        {
            Debug.Assert(commands != null);

            // Give focus to the first or last focusable command
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

                if (command is Control commandAsControl)
                {
                    if (IsControlFocusable(commandAsControl, !ensureTabStopUniqueness /*checkTabStop*/))
                    {
                        if (focusedControl == null)
                        {
                            if (FocusControl(
                                    commandAsControl /*newFocus*/,
                                    null /*oldFocus*/,
                                    ensureTabStopUniqueness /*updateTabStop*/))
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
                        else if (focusedControl != null && commandAsControl.IsTabStop)
                        {
                            commandAsControl.IsTabStop = false;
                        }
                    }
                }
            }

            return focusedControl != null;
        }

        void EnsureTabStopUniqueness(
                IList<ICommandBarElement> commands,
                Control moreButton)
        {
            Debug.Assert(commands != null);

            bool tabStopSeen = moreButton != null && moreButton.IsTabStop;

            if (tabStopSeen || GetFirstTabStopControl(commands) != null)
            {
                // Make sure only one command or the MoreButton has IsTabStop set
                foreach (var command in commands)
                {
                    if (command is Control commandAsControl)
                    {
                        if (IsControlFocusable(commandAsControl, false /*checkTabStop*/) && commandAsControl.IsTabStop)
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
            }
            else
            {
                // Set IsTabStop to first focusable command
                foreach (var command in commands)
                {
                    if (command is Control commandAsControl)
                    {
                        if (IsControlFocusable(commandAsControl, false /*checkTabStop*/))
                        {
                            commandAsControl.IsTabStop = true;
                            break;
                        }
                    }
                }
            }
        }

        void UpdateShadow()
        {
            if (PrimaryCommands.Count > 0)
            {
                AddShadow();
            }
            else if (PrimaryCommands.Count == 0)
            {
                ClearShadow();
            }
        }

        void AddShadow()
        {
        }

        internal void ClearShadow()
        {
        }

        private void SecondaryItemsRootSizeChanged(object sender, SizeChangedEventArgs e)
        {
            //m_secondaryItemsRootSized = true;
            UpdateUI();
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
                    {
                        // In addition to closing the CommandBar if someone hits the escape key,
                        // we also want to close the whole flyout.
                        if (TryGetOwningFlyout(out var owningFlyout))
                        {
                            owningFlyout.Hide();
                        }
                        break;
                    }

                case Key.Down:
                case Key.Up:
                    {
                        if (SecondaryCommands.Count > 1)
                        {
                            Control focusedControl = null;
                            int startIndex = 0;
                            int endIndex = SecondaryCommands.Count;
                            int deltaIndex = 1;
                            int loopCount = 0;

                            if (args.Key == Key.Up)
                            {
                                deltaIndex = -1;
                                startIndex = endIndex - 1;
                                endIndex = -1;
                            }

                            do
                            {
                                // Give keyboard focus to the previous or next secondary command if possible
                                for (int index = startIndex; index != endIndex; index += deltaIndex)
                                {
                                    var secondaryCommand = SecondaryCommands[index];

                                    if (secondaryCommand is Control secondaryCommandAsControl)
                                    {
                                        if (secondaryCommandAsControl.IsFocused)
                                        {
                                            focusedControl = secondaryCommandAsControl;
                                        }
                                        else if (focusedControl != null && IsControlFocusable(secondaryCommandAsControl, false /*checkTabStop*/) &&
                                                 focusedControl != secondaryCommandAsControl)
                                        {
                                            if (FocusControl(
                                                    secondaryCommandAsControl /*newFocus*/,
                                                    focusedControl /*oldFocus*/,
                                                    true /*updateTabStop*/))
                                            {
                                                args.Handled = true;
                                                return;
                                            }
                                        }
                                    }
                                }

                                if (loopCount == 0 && PrimaryCommands.Count > 0)
                                {
                                    var moreButton = m_moreButton;

                                    if (deltaIndex == 1 &&
                                        FocusCommand(
                                            PrimaryCommands /*commands*/,
                                            moreButton /*moreButton*/,
                                            true /*firstCommand*/,
                                            true /*ensureTabStopUniqueness*/))
                                    {
                                        // Being on the last secondary command, keyboard focus was given to the first primary command
                                        args.Handled = true;
                                        return;
                                    }
                                    else if (deltaIndex == -1 &&
                                        focusedControl != null &&
                                        moreButton != null &&
                                        IsControlFocusable(moreButton, false /*checkTabStop*/) &&
                                        FocusControl(
                                            moreButton /*newFocus*/,
                                            focusedControl /*oldFocus*/,
                                            true /*updateTabStop*/))
                                    {
                                        // Being on the first secondary command, keyboard focus was given to the MoreButton
                                        args.Handled = true;
                                        return;
                                    }
                                }

                                loopCount++; // Looping again when focus could not be given to a MoreButton going up or primary command going down.
                            }
                            while (loopCount < 2 && focusedControl != null);
                        }
                        args.Handled = true;
                        break;
                    }
            }
        }

        private void OpeningStoryboardCompleted(object sender, EventArgs e)
        {
            m_openingStoryboard.Stop(m_layoutRoot);
        }

        private void ClosingStoryboardCompleted(object sender, EventArgs e)
        {
            m_closingStoryboard.Stop(m_layoutRoot);
            SetOpacity(0);
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

        private void OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            // Close the owning flyout if the overflow was closed by MouseDown
            if (!IsOverflowOpen && e.Handled && e.OriginalSource == this)
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
            else
            {
                flyout = null;
                return false;
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

        FrameworkElement m_layoutRoot;
        FrameworkElement m_primaryItemsRoot;
        FrameworkElement m_secondaryItemsRoot;
        ButtonBase m_moreButton;
        RoutedEventHandlerRevoker m_firstItemLoadedRevoker;

        // We need to manually connect the end element of the primary items to the start element of the secondary items
        // for the purposes of UIA items navigation. To ensure that we only have the current start and end elements registered
        // (e.g., if the app adds a new start element to the secondary commands, we want to unregister the previous start element),
        // we'll save references to those elements.
        FrameworkElement m_currentPrimaryItemsEndElement;
        FrameworkElement m_currentSecondaryItemsStartElement;

        Storyboard m_openingStoryboard;
        Storyboard m_closingStoryboard;
        ClockState? m_openingStoryboardState;
        ClockState? m_closingStoryboardState;

        bool m_secondaryItemsRootSized;

        bool m_openAnimationPending;
        DispatcherOperation m_asyncOpenAnimation;
    }
}
