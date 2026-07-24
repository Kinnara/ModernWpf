using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using ModernWpf.Controls.Primitives;
using ModernWpf.Automation.Peers;

namespace ModernWpf.Controls
{
    [ContentProperty(nameof(PrimaryCommands))]
    [TemplatePart(Name = PrimaryItemsControlName, Type = typeof(Panel))]
    [TemplatePart(Name = SecondaryItemsPanelName, Type = typeof(Panel))]
    [TemplatePart(Name = OverflowPopupName, Type = typeof(Popup))]
    public partial class CommandBar : Control
    {
        static CommandBar()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(CommandBar), new FrameworkPropertyMetadata(typeof(CommandBar)));

            KeyboardNavigation.DirectionalNavigationProperty.OverrideMetadata(typeof(CommandBar),
                new FrameworkPropertyMetadata(KeyboardNavigationMode.Contained));
            KeyboardNavigation.TabNavigationProperty.OverrideMetadata(typeof(CommandBar),
                new FrameworkPropertyMetadata(KeyboardNavigationMode.Continue));
        }

        public CommandBar()
        {
            SetValue(CommandBarTemplateSettingsPropertyKey, new CommandBarTemplateSettings());

            PrimaryCommands = new ObservableCollection<ICommandBarElement>();
            PrimaryCommands.CollectionChanged += PrimaryCommands_CollectionChanged;

            SecondaryCommands = new ObservableCollection<ICommandBarElement>();
            SecondaryCommands.CollectionChanged += SecondaryCommands_CollectionChanged;

            Loaded += OnLoaded;
            Unloaded += OnUnloaded;
            SizeChanged += OnSizeChanged;
        }

        private static void OnIsOpenChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((CommandBar)d).OnIsOpenChanged((bool)e.NewValue);
        }

        private void OnIsOpenChanged(bool isOpen)
        {
            if (isOpen)
            {
                OnOpening(null);
            }
            else
            {
                OnClosing(null);
            }

            if (isOpen)
            {
                UpdateInputDeviceTypeUsedToOpen();
            }
            else
            {
                m_inputModeUsedToOpen = AppBarButtonInputMode.Default;
            }

            UpdateOverflowPopupVisibility(isOpen);
            UpdateCommandOverflowStyleParams();
            UpdateTemplateSettings();
            UpdateOverflowPresenterVisualState(true);

            if (!isOpen)
            {
                CloseSubMenus(null);
                OnClosed(null);
            }

            if (FrameworkElementAutomationPeer.FromElement(this) is CommandBarAutomationPeer peer)
            {
                peer.RaiseIsOpenChanged(!isOpen, isOpen);
            }
        }

        protected override AutomationPeer OnCreateAutomationPeer()
        {
            return new CommandBarAutomationPeer(this);
        }

        #region PrimaryCommands

        public ObservableCollection<ICommandBarElement> PrimaryCommands { get; }

        private void PrimaryCommands_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems != null)
            {
                ClearParentCommandBarForCommands(e.OldItems.OfType<DependencyObject>());
            }

            if (e.NewItems != null)
            {
                SetParentCommandBarForCommands(e.NewItems.OfType<DependencyObject>());
            }

            ResetDynamicCommands();
            UpdateUI();
        }

        #endregion

        #region SecondaryCommands

        public ObservableCollection<ICommandBarElement> SecondaryCommands { get; }

        private void SecondaryCommands_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems != null)
            {
                ClearParentCommandBarForCommands(e.OldItems.OfType<DependencyObject>());
            }

            if (e.NewItems != null)
            {
                SetParentCommandBarForCommands(e.NewItems.OfType<DependencyObject>());
            }

            ResetDynamicCommands();
            UpdateUI();
        }

        #endregion

        private static void OnDefaultLabelPositionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var commandBar = (CommandBar)d;
            commandBar.PropagateDefaultLabelPosition();
            commandBar.UpdateUI();
        }

        private static void OnIsDynamicOverflowEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var commandBar = (CommandBar)d;
            commandBar.ApplyDynamicOverflow(commandBar.ActualWidth);
            commandBar.UpdateUI();
        }

        private static void OnOverflowButtonVisibilityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((CommandBar)d).UpdateUI();
        }

        public event EventHandler<object> Opening;

        public event EventHandler<object> Opened;

        public event EventHandler<object> Closing;

        public event EventHandler<object> Closed;

        public event TypedEventHandler<CommandBar, DynamicOverflowItemsChangingEventArgs> DynamicOverflowItemsChanging;

        protected virtual void OnOpening(object e)
        {
            Opening?.Invoke(this, e);
        }

        protected virtual void OnOpened(object e)
        {
            Opened?.Invoke(this, e);
        }

        protected virtual void OnClosing(object e)
        {
            Closing?.Invoke(this, e);
        }

        protected virtual void OnClosed(object e)
        {
            Closed?.Invoke(this, e);
        }

        public override void OnApplyTemplate()
        {
            DetachTemplatePartHandlers();
            ClearPanelChildren(m_primaryItemsPanel);
            ClearPanelChildren(m_secondaryItemsPanel);

            base.OnApplyTemplate();

            m_layoutRoot = GetTemplateChild("LayoutRoot") as FrameworkElement;
            m_contentControl = GetTemplateChild("ContentControl") as FrameworkElement;
            m_primaryItemsPanel = GetTemplateChild(PrimaryItemsControlName) as Panel;
            m_secondaryItemsPanel = GetTemplateChild(SecondaryItemsPanelName) as Panel;
            m_secondaryItemsControl = GetTemplateChild(SecondaryItemsControlName) as CommandBarOverflowPresenter;
            m_moreButton = GetTemplateChild("MoreButton") as ButtonBase;
            m_overflowPopup = GetTemplateChild(OverflowPopupName) as Popup;
            m_overflowContentRoot = GetTemplateChild("OverflowContentRoot") as FrameworkElement;

            if (m_moreButton != null)
            {
                AutomationProperties.SetName(m_moreButton, Strings.AppBarMoreButtonName);
                UpdateMoreButtonToolTip();

                if (m_moreButton is ToggleButton moreToggleButton)
                {
                    moreToggleButton.Checked += OnMoreButtonChecked;
                    moreToggleButton.Unchecked += OnMoreButtonUnchecked;
                }
            }

            if (m_overflowPopup != null)
            {
                m_overflowPopup.CustomPopupPlacementCallback = PositionOverflowPopup;
                m_overflowPopup.SetValue(CustomPopupPlacementHelper.PlacementProperty, CustomPlacementMode.BottomEdgeAlignedRight);
                m_overflowPopup.Opened += OnOverflowPopupOpened;
                m_overflowPopup.Closed += OnOverflowPopupClosed;
            }

            if (m_overflowContentRoot != null)
            {
                m_overflowContentRoot.PreviewKeyDown += OnOverflowContentRootPreviewKeyDown;
            }

            AttachCommandElementsToPanels();
            UpdateOverflowPopupVisibility(IsOpen);
            UpdateUI(false);
        }

        protected override Size MeasureOverride(Size constraint)
        {
            ApplyDynamicOverflow(constraint.Width);
            return base.MeasureOverride(constraint);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.Key == Key.Escape && IsOpen)
            {
                TryDismissCommandBarOverflow();
                e.Handled = true;
            }

            base.OnKeyDown(e);
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

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            ApplyDynamicOverflow(ActualWidth);
            UpdateUI(false);
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            InputManager.Current.PreProcessInput -= OnPreProcessInput;
            if (IsOpen)
            {
                SetCurrentValue(IsOpenProperty, false);
            }
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            ApplyDynamicOverflow(e.NewSize.Width);
            UpdateUI(false);
        }

        private void DetachTemplatePartHandlers()
        {
            InputManager.Current.PreProcessInput -= OnPreProcessInput;

            if (m_moreButton != null)
            {
                if (m_moreButton is ToggleButton moreToggleButton)
                {
                    moreToggleButton.Checked -= OnMoreButtonChecked;
                    moreToggleButton.Unchecked -= OnMoreButtonUnchecked;
                }

                m_moreButton.ClearValue(ToolTipProperty);
            }

            if (m_secondaryItemsPanel is CommandBarOverflowPanel overflowPanel &&
                ReferenceEquals(overflowPanel.OwnerCommandBar, this))
            {
                overflowPanel.OwnerCommandBar = null;
            }

            if (m_overflowPopup != null)
            {
                m_overflowPopup.ClearValue(Popup.CustomPopupPlacementCallbackProperty);
                m_overflowPopup.ClearValue(CustomPopupPlacementHelper.PlacementProperty);
                m_overflowPopup.Opened -= OnOverflowPopupOpened;
                m_overflowPopup.Closed -= OnOverflowPopupClosed;
            }

            if (m_overflowContentRoot != null)
            {
                m_overflowContentRoot.PreviewKeyDown -= OnOverflowContentRootPreviewKeyDown;
            }
        }

        private void AttachCommandElementsToPanels()
        {
            ClearPanelChildren(m_primaryItemsPanel);
            ClearPanelChildren(m_secondaryItemsPanel);

            if (m_secondaryItemsPanel is CommandBarOverflowPanel overflowPanel)
            {
                overflowPanel.OwnerCommandBar = this;
            }

            AddCommandsToPanel(m_primaryItemsPanel, m_dynamicPrimaryCommands);
            AddCommandsToPanel(m_secondaryItemsPanel, m_dynamicSecondaryCommands);

            PropagateDefaultLabelPosition();
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
                    AppBarElementProperties.SetUseOverflowStyle(dependencyObject, false);
                    dependencyObject.ClearValue(AppBarElementProperties.DefaultLabelPositionProperty);
                }

                if (child is IAppBarButtonElement appBarButtonElement)
                {
                    appBarButtonElement.SetOverflowStyleParams(false, false, false);
                    appBarButtonElement.SetInputMode(AppBarButtonInputMode.Default);
                    appBarButtonElement.UpdateTemplateSettings(0);
                }
            }

            panel.Children.Clear();
        }

        private static void AddCommandsToPanel(Panel panel, IEnumerable<ICommandBarElement> commands)
        {
            if (panel == null)
            {
                return;
            }

            foreach (var command in commands)
            {
                if (command is UIElement element && !HasVisualOrLogicalParent(element))
                {
                    panel.Children.Add(element);
                }
            }
        }

        private static bool HasVisualOrLogicalParent(UIElement element)
        {
            return VisualTreeHelper.GetParent(element) != null ||
                   LogicalTreeHelper.GetParent(element) != null;
        }

        private void ResetDynamicCommands()
        {
            ReplaceDynamicCommands(PrimaryCommands, SecondaryCommands);
        }

        private void ApplyDynamicOverflow(double availableWidth)
        {
            if (m_isApplyingDynamicOverflow)
            {
                return;
            }

            m_isApplyingDynamicOverflow = true;

            try
            {
                if (!IsDynamicOverflowEnabled || double.IsInfinity(availableWidth) || double.IsNaN(availableWidth) || availableWidth <= 0)
                {
                    ReplaceDynamicCommands(PrimaryCommands, SecondaryCommands);
                    return;
                }

                var primaryCommands = PrimaryCommands.ToList();
                var secondaryCommands = SecondaryCommands.ToList();

                double primaryWidth = MeasureCommandsWidth(primaryCommands);
                double contentWidth = MeasureElementWidth(m_contentControl);
                double moreButtonWidth = MeasureElementWidth(m_moreButton);
                double availablePrimaryWidth = Math.Max(0, availableWidth - contentWidth - moreButtonWidth);

                var movedPrimaryCommands = FindMovablePrimaryCommands(
                    primaryCommands,
                    availablePrimaryWidth,
                    primaryWidth);
                var movedPrimarySet = new HashSet<ICommandBarElement>(movedPrimaryCommands);
                var dynamicPrimary = primaryCommands.Where(command => !movedPrimarySet.Contains(command)).ToList();
                var dynamicSecondary = new List<ICommandBarElement>();

                if (movedPrimaryCommands.Count > 0)
                {
                    dynamicSecondary.AddRange(movedPrimaryCommands);

                    if (HasVisibleElements(secondaryCommands))
                    {
                        dynamicSecondary.Add(OverflowSeparator);
                    }
                }

                dynamicSecondary.AddRange(secondaryCommands);
                RaiseDynamicOverflowItemsChangingIfNeeded(movedPrimaryCommands);
                ReplaceDynamicCommands(dynamicPrimary, dynamicSecondary);
            }
            finally
            {
                m_isApplyingDynamicOverflow = false;
            }
        }

        private void RaiseDynamicOverflowItemsChangingIfNeeded(
            IReadOnlyCollection<ICommandBarElement> movedPrimaryCommands)
        {
            var previousMovedPrimaryCommands = new HashSet<ICommandBarElement>(
                m_dynamicSecondaryCommands.Where(command => PrimaryCommands.Contains(command)));
            var currentMovedPrimaryCommands = new HashSet<ICommandBarElement>(movedPrimaryCommands);

            if (previousMovedPrimaryCommands.SetEquals(currentMovedPrimaryCommands))
            {
                return;
            }

            bool isAdding = currentMovedPrimaryCommands.Any(
                command => !previousMovedPrimaryCommands.Contains(command));
            DynamicOverflowItemsChanging?.Invoke(
                this,
                new DynamicOverflowItemsChangingEventArgs
                {
                    Action = isAdding
                        ? CommandBarDynamicOverflowAction.AddingToOverflow
                        : CommandBarDynamicOverflowAction.RemovingFromOverflow
                });
        }

        private static List<ICommandBarElement> FindMovablePrimaryCommands(
            List<ICommandBarElement> primaryCommands,
            double availablePrimaryWidth,
            double primaryWidth)
        {
            var movedIndices = new HashSet<int>();
            var orderedValues = primaryCommands
                .Select(command => command.DynamicOverflowOrder)
                .Where(order => order > 0)
                .Distinct()
                .OrderBy(order => order)
                .ToList();

            if (orderedValues.Count > 0)
            {
                foreach (int order in orderedValues)
                {
                    for (int i = 0; i < primaryCommands.Count; i++)
                    {
                        if (primaryCommands[i].DynamicOverflowOrder == order)
                        {
                            MovePrimaryCommand(primaryCommands, i, movedIndices, ref primaryWidth);
                            MoveAdjacentSeparators(primaryCommands, i, movedIndices, ref primaryWidth);
                        }
                    }

                    if (primaryWidth < availablePrimaryWidth)
                    {
                        break;
                    }
                }

                if (primaryWidth > availablePrimaryWidth)
                {
                    for (int i = primaryCommands.Count - 1; i >= 0; i--)
                    {
                        if (primaryCommands[i].DynamicOverflowOrder == 0)
                        {
                            MovePrimaryCommand(primaryCommands, i, movedIndices, ref primaryWidth);
                            if (primaryWidth < availablePrimaryWidth)
                            {
                                break;
                            }
                        }
                    }
                }
            }
            else
            {
                for (int i = primaryCommands.Count - 1;
                    i >= 0 && primaryWidth >= availablePrimaryWidth;
                    i--)
                {
                    MovePrimaryCommand(primaryCommands, i, movedIndices, ref primaryWidth);
                }
            }

            return primaryCommands
                .Where((command, index) => movedIndices.Contains(index))
                .ToList();
        }

        private static void MoveAdjacentSeparators(
            List<ICommandBarElement> primaryCommands,
            int movingIndex,
            HashSet<int> movedIndices,
            ref double primaryWidth)
        {
            bool hasPreviousSeparator = movingIndex > 0 &&
                primaryCommands[movingIndex - 1] is AppBarSeparator;

            if (hasPreviousSeparator)
            {
                int previousNonSeparator = movingIndex - 1;
                while (previousNonSeparator >= 0 &&
                    primaryCommands[previousNonSeparator] is AppBarSeparator)
                {
                    previousNonSeparator--;
                }

                int separatorIndex = movingIndex - 1;
                if (previousNonSeparator < 0)
                {
                    while (separatorIndex >= 0)
                    {
                        MovePrimaryCommand(primaryCommands, separatorIndex--, movedIndices, ref primaryWidth);
                    }
                }
                else
                {
                    while (separatorIndex > previousNonSeparator &&
                        separatorIndex - previousNonSeparator > 1)
                    {
                        MovePrimaryCommand(primaryCommands, separatorIndex--, movedIndices, ref primaryWidth);
                    }
                }
            }

            if (movingIndex + 1 >= primaryCommands.Count ||
                primaryCommands[movingIndex + 1] is not AppBarSeparator)
            {
                return;
            }

            int nextNonSeparator = movingIndex + 1;
            while (nextNonSeparator < primaryCommands.Count &&
                primaryCommands[nextNonSeparator] is AppBarSeparator)
            {
                nextNonSeparator++;
            }

            int nextSeparatorIndex = movingIndex + 1;
            if (nextNonSeparator == primaryCommands.Count)
            {
                while (nextSeparatorIndex < primaryCommands.Count)
                {
                    MovePrimaryCommand(primaryCommands, nextSeparatorIndex++, movedIndices, ref primaryWidth);
                }
            }
            else
            {
                while (nextSeparatorIndex < nextNonSeparator &&
                    nextNonSeparator - nextSeparatorIndex > 1)
                {
                    MovePrimaryCommand(primaryCommands, nextSeparatorIndex++, movedIndices, ref primaryWidth);
                }
            }

            if (movingIndex == 0)
            {
                MovePrimaryCommand(primaryCommands, movingIndex + 1, movedIndices, ref primaryWidth);
            }
            else if (hasPreviousSeparator)
            {
                MovePrimaryCommand(primaryCommands, movingIndex + 1, movedIndices, ref primaryWidth);
                if (nextNonSeparator == primaryCommands.Count)
                {
                    MovePrimaryCommand(primaryCommands, movingIndex - 1, movedIndices, ref primaryWidth);
                }
            }
        }

        private static void MovePrimaryCommand(
            List<ICommandBarElement> primaryCommands,
            int index,
            HashSet<int> movedIndices,
            ref double primaryWidth)
        {
            if (index >= 0 && index < primaryCommands.Count && movedIndices.Add(index))
            {
                primaryWidth -= MeasureCommandWidth(primaryCommands[index]);
            }
        }

        private void ReplaceDynamicCommands(IEnumerable<ICommandBarElement> primaryCommands, IEnumerable<ICommandBarElement> secondaryCommands)
        {
            var primaryList = primaryCommands.ToList();
            var secondaryList = secondaryCommands.ToList();

            if (m_dynamicPrimaryCommands.SequenceEqual(primaryList) &&
                m_dynamicSecondaryCommands.SequenceEqual(secondaryList))
            {
                PropagateDefaultLabelPosition();
                UpdateCommandOverflowStyleParams();
                return;
            }

            ClearPanelChildren(m_primaryItemsPanel);
            ClearPanelChildren(m_secondaryItemsPanel);

            m_dynamicPrimaryCommands.Clear();
            m_dynamicSecondaryCommands.Clear();

            foreach (var command in primaryList)
            {
                m_dynamicPrimaryCommands.Add(command);
            }

            foreach (var command in secondaryList)
            {
                m_dynamicSecondaryCommands.Add(command);
            }

            AttachCommandElementsToPanels();
        }

        private double MeasureCommandsWidth(IEnumerable<ICommandBarElement> commands)
        {
            double width = 0;

            foreach (var command in commands)
            {
                width += MeasureCommandWidth(command);
            }

            return width;
        }

        private static double MeasureCommandWidth(ICommandBarElement command)
        {
            return MeasureElementWidth(command as UIElement);
        }

        private static double MeasureElementWidth(UIElement element)
        {
            if (element == null || element.Visibility == Visibility.Collapsed)
            {
                return 0;
            }

            element.Measure(InfiniteSize);
            return element.DesiredSize.Width;
        }

        private void UpdateUI(bool useTransitions = true)
        {
            UpdateTemplateSettings();
            UpdateVisualState(useTransitions);
            UpdateMoreButtonToolTip();
            UpdateOverflowPresenterVisualState(useTransitions);
        }

        internal void UpdateVisualState(bool useTransitions = true)
        {
            string stateName;

            bool hasVisiblePrimaryCommands = HasVisibleElements(m_dynamicPrimaryCommands);
            bool hasVisibleSecondaryCommands = HasVisibleElements(m_dynamicSecondaryCommands);

            if (hasVisiblePrimaryCommands && hasVisibleSecondaryCommands)
            {
                stateName = "BothCommands";
            }
            else if (hasVisibleSecondaryCommands)
            {
                stateName = "SecondaryCommandsOnly";
            }
            else
            {
                stateName = "PrimaryCommandsOnly";
            }

            VisualStateManager.GoToState(this, stateName, useTransitions);
            VisualStateManager.GoToState(
                this,
                IsDynamicOverflowEnabled ? DynamicOverflowEnabledStateName : DynamicOverflowDisabledStateName,
                useTransitions);
        }

        private void UpdateTemplateSettings()
        {
            var settings = CommandBarTemplateSettings;
            if (settings == null)
            {
                return;
            }

            double contentHeight = Math.Max(ActualHeight, m_layoutRoot?.ActualHeight ?? 0);
            settings.ContentHeight = contentHeight;
            settings.OverflowContentMaxHeight = CalculateOverflowContentMaxHeight();
            settings.OverflowContentMinWidth = GetDoubleResource("CommandBarOverflowMinWidth", 160);
            settings.OverflowContentMaxWidth = GetDoubleResource("CommandBarOverflowMaxWidth", 480);
            settings.EffectiveOverflowButtonVisibility = CalculateEffectiveOverflowButtonVisibility();
            settings.OverflowContentHorizontalOffset = 0;

            Size overflowContentSize = new();
            if (m_overflowContentRoot != null && HasVisibleElements(m_dynamicSecondaryCommands))
            {
                m_overflowContentRoot.Measure(InfiniteSize);
                overflowContentSize = m_overflowContentRoot.DesiredSize;
            }

            settings.OverflowContentHeight = overflowContentSize.Height;
            settings.NegativeOverflowContentHeight = -overflowContentSize.Height;
            settings.OverflowContentClipRect = new Rect(0, 0, overflowContentSize.Width, overflowContentSize.Height);
            settings.OverflowContentCompactYTranslation = -contentHeight;
            settings.OverflowContentMinimalYTranslation = -contentHeight;
            settings.OverflowContentHiddenYTranslation = -contentHeight;
        }

        private Visibility CalculateEffectiveOverflowButtonVisibility()
        {
            bool visible = true;

            switch (OverflowButtonVisibility)
            {
                case CommandBarOverflowButtonVisibility.Auto:
                    visible = m_dynamicSecondaryCommands.Count > 0 || HasVisiblePrimaryCommandWithBottomLabel();
                    break;
                case CommandBarOverflowButtonVisibility.Collapsed:
                    visible = false;
                    break;
            }

            return visible ? Visibility.Visible : Visibility.Collapsed;
        }

        private double GetDoubleResource(string key, double fallback)
        {
            if (TryFindResource(key) is double value)
            {
                return value;
            }

            return fallback;
        }

        private static double CalculateOverflowContentMaxHeight()
        {
            return SystemParameters.PrimaryScreenHeight / 2 + 20;
        }

        private void UpdateCommandOverflowStyleParams()
        {
            AppBarElementProperties.UpdateOverflowStyleParams(m_dynamicPrimaryCommands, false);
            AppBarElementProperties.UpdateOverflowStyleParams(
                m_dynamicSecondaryCommands,
                true,
                IsOpen ? m_inputModeUsedToOpen : AppBarButtonInputMode.Default);
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

        private void PropagateDefaultLabelPosition()
        {
            PropagateDefaultLabelPosition(PrimaryCommands);
            PropagateDefaultLabelPosition(SecondaryCommands);
        }

        private void PropagateDefaultLabelPosition(IEnumerable<ICommandBarElement> commands)
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

        private bool HasVisiblePrimaryCommandWithBottomLabel()
        {
            foreach (var command in m_dynamicPrimaryCommands)
            {
                if (command is UIElement { Visibility: Visibility.Visible } &&
                    command is IAppBarButtonElement appBarButtonElement &&
                    appBarButtonElement.GetHasBottomLabel())
                {
                    return true;
                }
            }

            return false;
        }

        private static bool HasVisibleElements(IEnumerable<ICommandBarElement> elements)
        {
            foreach (var element in elements)
            {
                if (element is UIElement uiElement &&
                    uiElement.Visibility == Visibility.Visible)
                {
                    return true;
                }
            }

            return false;
        }

        private void OnOverflowPopupOpened(object sender, EventArgs e)
        {
            InputManager.Current.PreProcessInput -= OnPreProcessInput;
            InputManager.Current.PreProcessInput += OnPreProcessInput;
            RefreshOverflowPopupPosition();
            UpdateTemplateSettings();
            UpdateOverflowPresenterVisualState(true);
            OnOpened(null);
        }

        private void RefreshOverflowPopupPosition()
        {
            if (m_overflowPopup == null)
            {
                return;
            }

            // Popup caches its native position. A CommandBar can be rendered while
            // disconnected (for example, into a bitmap) and then attached to a live
            // window; nudging the offset makes WPF recompute against the current target.
            var horizontalOffset = m_overflowPopup.HorizontalOffset;
            m_overflowPopup.SetCurrentValue(Popup.HorizontalOffsetProperty, horizontalOffset + 0.01);
            m_overflowPopup.SetCurrentValue(Popup.HorizontalOffsetProperty, horizontalOffset);
        }

        private void OnOverflowPopupClosed(object sender, EventArgs e)
        {
            InputManager.Current.PreProcessInput -= OnPreProcessInput;
            if (IsOpen)
            {
                SetCurrentValue(IsOpenProperty, false);
            }

            UpdateTemplateSettings();
            UpdateOverflowPresenterVisualState(true);
        }

        private void OnMoreButtonChecked(object sender, RoutedEventArgs e)
        {
            if (!IsOpen)
            {
                SetCurrentValue(IsOpenProperty, true);
            }
        }

        private void OnMoreButtonUnchecked(object sender, RoutedEventArgs e)
        {
            if (IsOpen)
            {
                SetCurrentValue(IsOpenProperty, false);
            }
        }

        private void OnOverflowContentRootPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape && IsOpen)
            {
                TryDismissCommandBarOverflow();
                e.Handled = true;
            }
        }

        private void OnPreProcessInput(object sender, PreProcessInputEventArgs e)
        {
            if (!IsOpen || IsSticky ||
                e.StagingItem.Input is not MouseButtonEventArgs mouseArgs ||
                mouseArgs.ButtonState != MouseButtonState.Pressed)
            {
                return;
            }

            var inputSource = Mouse.DirectlyOver as DependencyObject ?? mouseArgs.OriginalSource as DependencyObject;
            TryLightDismissForTesting(inputSource);
        }

        internal bool TryLightDismissForTesting(DependencyObject inputSource)
        {
            if (IsOpen && !IsSticky && !IsWithinCommandBarOrOverflow(inputSource))
            {
                SetCurrentValue(IsOpenProperty, false);
                return true;
            }

            return false;
        }

        private bool IsWithinCommandBarOrOverflow(DependencyObject element)
        {
            while (element != null)
            {
                if (ReferenceEquals(element, this) || ReferenceEquals(element, m_overflowContentRoot))
                {
                    return true;
                }

                DependencyObject parent = null;
                if (element is Visual || element is System.Windows.Media.Media3D.Visual3D)
                {
                    parent = VisualTreeHelper.GetParent(element);
                }

                element = parent ?? LogicalTreeHelper.GetParent(element);
            }

            return false;
        }

        private void TryDismissCommandBarOverflow()
        {
            if (!IsSticky)
            {
                SetCurrentValue(IsOpenProperty, false);
            }

            m_moreButton?.Focus();
        }

        private void UpdateOverflowPopupVisibility(bool isOpen)
        {
            if (m_overflowContentRoot != null)
            {
                m_overflowContentRoot.Visibility = isOpen ? Visibility.Visible : Visibility.Collapsed;
            }

            if (m_overflowPopup == null)
            {
                return;
            }

            if (isOpen)
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

        private CustomPopupPlacement[] PositionOverflowPopup(Size popupSize, Size targetSize, Point offset)
        {
            return CustomPopupPlacementHelper.PositionPopup(
                CustomPlacementMode.BottomEdgeAlignedRight,
                popupSize,
                targetSize,
                offset);
        }

        private void UpdateOverflowPresenterVisualState(bool useTransitions)
        {
            m_secondaryItemsControl?.UpdateVisualState(useTransitions);
        }

        internal bool IsOverflowPopupOpenDown()
        {
            if (m_overflowContentRoot != null)
            {
                var overflowPopupTop = m_overflowContentRoot.TranslatePoint(new Point(), this);
                return overflowPopupTop.Y > 0;
            }

            return true;
        }

        private void UpdateMoreButtonToolTip()
        {
            if (m_moreButton != null)
            {
                m_moreButton.ToolTip = IsOpen ? Strings.AppBarMoreButtonOpenToolTip : Strings.AppBarMoreButtonClosedToolTip;
            }
        }

        internal static void OnCommandExecutionStatic(ICommandBarElement element)
        {
            if (element is DependencyObject dependencyObject &&
                FindParentCommandBarForElement(dependencyObject) is { } commandBar)
            {
                commandBar.SetCurrentValue(IsOpenProperty, false);
            }
        }

        internal static void OnCommandBarElementVisibilityChanged(ICommandBarElement element)
        {
            if (element is DependencyObject dependencyObject &&
                FindParentCommandBarForElement(dependencyObject) is { } commandBar)
            {
                commandBar.OnCommandBarElementChanged();
            }
        }

        internal static void OnCommandBarElementDependencyPropertyChanged(DependencyObject element)
        {
            if (FindParentCommandBarForElement(element) is { } commandBar)
            {
                commandBar.OnCommandBarElementChanged();
            }
        }

        private void OnCommandBarElementChanged()
        {
            ApplyDynamicOverflow(ActualWidth);
            UpdateUI();
        }

        internal static void ClosePeerSubMenusOnPointerEntered(DependencyObject element, AppBarButton menuToLeaveOpen)
        {
            if (FindParentCommandBarForElement(element) is { } commandBar)
            {
                commandBar.CloseSubMenus(menuToLeaveOpen);
                return;
            }

            if (SharedHelpers.GetAncestorOfType<CommandBarFlyoutCommandBar>(element) is { } flyoutCommandBar)
            {
                flyoutCommandBar.CloseSubMenus(menuToLeaveOpen);
            }
        }

        internal static CommandBar FindParentCommandBarForElement(DependencyObject element)
        {
            if (GetParentCommandBar(element) is { } ownerCommandBar &&
                ownerCommandBar.ContainsCommandElement(element))
            {
                return ownerCommandBar;
            }

            var current = element;
            while (current != null)
            {
                if (current is CommandBar commandBar)
                {
                    return commandBar;
                }

                current = VisualTreeHelper.GetParent(current) ?? LogicalTreeHelper.GetParent(current);
            }

            return null;
        }

        private void CloseSubMenus(AppBarButton menuToLeaveOpen)
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

        private void SetParentCommandBarForCommands(IEnumerable<DependencyObject> elements)
        {
            foreach (var element in elements)
            {
                SetParentCommandBar(element, this);
            }
        }

        private void ClearParentCommandBarForCommands(IEnumerable<DependencyObject> elements)
        {
            foreach (var element in elements)
            {
                if (GetParentCommandBar(element) == this)
                {
                    element.ClearValue(ParentCommandBarProperty);
                }
            }
        }

        private bool ContainsCommandElement(DependencyObject element)
        {
            return element is ICommandBarElement commandBarElement &&
                   (PrimaryCommands.Contains(commandBarElement) || SecondaryCommands.Contains(commandBarElement));
        }

        private static CommandBar GetParentCommandBar(DependencyObject element)
        {
            return (CommandBar)element.GetValue(ParentCommandBarProperty);
        }

        private static void SetParentCommandBar(DependencyObject element, CommandBar commandBar)
        {
            element.SetValue(ParentCommandBarProperty, commandBar);
        }

        private AppBarSeparator OverflowSeparator
        {
            get
            {
                if (m_overflowSeparator == null)
                {
                    m_overflowSeparator = new AppBarSeparator();
                }

                return m_overflowSeparator;
            }
        }

        private readonly List<ICommandBarElement> m_dynamicPrimaryCommands = new();
        private readonly List<ICommandBarElement> m_dynamicSecondaryCommands = new();

        private FrameworkElement m_layoutRoot;
        private FrameworkElement m_contentControl;
        private Panel m_primaryItemsPanel;
        private Panel m_secondaryItemsPanel;
        private CommandBarOverflowPresenter m_secondaryItemsControl;
        private ButtonBase m_moreButton;
        private Popup m_overflowPopup;
        private FrameworkElement m_overflowContentRoot;
        private AppBarSeparator m_overflowSeparator;
        private AppBarButtonInputMode m_lastInputMode;
        private AppBarButtonInputMode m_inputModeUsedToOpen;
        private bool m_isApplyingDynamicOverflow;

        private static readonly Size InfiniteSize = new(double.PositiveInfinity, double.PositiveInfinity);

        private const string OverflowPopupName = "OverflowPopup";
        private const string PrimaryItemsControlName = "PrimaryItemsControl";
        private const string SecondaryItemsControlName = "SecondaryItemsControl";
        private const string SecondaryItemsPanelName = "SecondaryItemsPanel";
        private const string DynamicOverflowDisabledStateName = "DynamicOverflowDisabled";
        private const string DynamicOverflowEnabledStateName = "DynamicOverflowEnabled";
    }
}
