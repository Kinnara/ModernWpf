// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Automation.Peers;
using System.Windows.Automation;
using System.Windows.Controls.Primitives;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows;
using ModernWpf.Media.Animation;
using static CppWinRTHelpers;
using static ModernWpf.ResourceAccessor;

namespace ModernWpf.Controls
{
    public partial class TeachingTip : ContentControl, IControlProtected
    {
        public FrameworkElement m_target;
        public bool m_isIdle = true;

        private const string c_TitleTextBlockVisibleStateName = "ShowTitleTextBlock";
        private const string c_TitleTextBlockCollapsedStateName = "CollapseTitleTextBlock";
        private const string c_SubtitleTextBlockVisibleStateName = "ShowSubtitleTextBlock";
        private const string c_SubtitleTextBlockCollapsedStateName = "CollapseSubtitleTextBlock";

        private const string c_OverlayCornerRadiusName = "OverlayCornerRadius";

        private static readonly ResourceAccessor ResourceAccessor = new ResourceAccessor(typeof(TeachingTip));

        static TeachingTip()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(TeachingTip), new FrameworkPropertyMetadata(typeof(TeachingTip)));
        }

        public TeachingTip()
        {
            Unloaded += ClosePopupOnUnloadEvent;
            SetValue(TemplateSettingsProperty, new TeachingTipTemplateSettings());
        }

        protected override AutomationPeer OnCreateAutomationPeer()
        {
            return new TeachingTipAutomationPeer(this);
        }

        public override void OnApplyTemplate()
        {
            PreviewKeyDown -= OnF6AcceleratorKeyClicked;
            //m_acceleratorKeyActivatedRevoker.revoke();            
            //m_effectiveViewportChangedRevoker.revoke();

            if (m_tailOcclusionGrid != null)
            {
                m_tailOcclusionGrid.SizeChanged -= OnContentSizeChanged;
            }
            if (m_closeButton != null)
            {
                m_closeButton.Click -= OnCloseButtonClicked;
            }
            if (m_alternateCloseButton != null)
            {
                m_alternateCloseButton.Click -= OnCloseButtonClicked;
            }
            if (m_actionButton != null)
            {
                m_actionButton.Click -= OnActionButtonClicked;
            }

            var window = Window.GetWindow(this);
            if (window != null)
            {
                window.SizeChanged -= WindowSizeChanged;
            }

            IControlProtected controlProtected = this;

            m_container = GetTemplateChildT<Border>(s_containerName, controlProtected);
            m_rootElement = m_container.Child;
            m_tailOcclusionGrid = GetTemplateChildT<Grid>(s_tailOcclusionGridName, controlProtected);
            m_contentRootGrid = GetTemplateChildT<Grid>(s_contentRootGridName, controlProtected);
            m_nonHeroContentRootGrid = GetTemplateChildT<Grid>(s_nonHeroContentRootGridName, controlProtected);
            m_heroContentBorder = GetTemplateChildT<Border>(s_heroContentBorderName, controlProtected);
            m_actionButton = GetTemplateChildT<Button>(s_actionButtonName, controlProtected);
            m_alternateCloseButton = GetTemplateChildT<Button>(s_alternateCloseButtonName, controlProtected);
            m_closeButton = GetTemplateChildT<Button>(s_closeButtonName, controlProtected);
            m_tailEdgeBorder = GetTemplateChildT<Grid>(s_tailEdgeBorderName, controlProtected);
            m_tailPolygon = GetTemplateChildT<Polygon>(s_tailPolygonName, controlProtected);
            ToggleVisibilityForEmptyContent(c_TitleTextBlockVisibleStateName, c_TitleTextBlockCollapsedStateName, Title);
            ToggleVisibilityForEmptyContent(c_SubtitleTextBlockVisibleStateName, c_SubtitleTextBlockCollapsedStateName, Subtitle);

            var container = m_container;
            if (container != null)
            {
                container.Child = null;
            }

            var tailOcclusionGrid = m_tailOcclusionGrid;
            if (tailOcclusionGrid != null)
            {
                tailOcclusionGrid.SizeChanged += OnContentSizeChanged;
            }

            var contentRootGrid = m_contentRootGrid;
            if (contentRootGrid != null)
            {
                //AutomationProperties.SetLocalizedLandmarkType(contentRootGrid, ResourceAccessor.GetLocalizedStringResource(SR_TeachingTipCustomLandmarkName));
            }

            var closeButton = m_closeButton;
            if (closeButton != null)
            {
                closeButton.Click += OnCloseButtonClicked;
            }

            var alternateCloseButton = m_alternateCloseButton;
            if (alternateCloseButton != null)
            {
                AutomationProperties.SetName(alternateCloseButton, ResourceAccessor.GetLocalizedStringResource(SR_TeachingTipAlternateCloseButtonName));
                ToolTip tooltip = new ToolTip();
                tooltip.Content = ResourceAccessor.GetLocalizedStringResource(SR_TeachingTipAlternateCloseButtonTooltip);
                ToolTipService.SetToolTip(alternateCloseButton, tooltip);
                alternateCloseButton.Click += OnCloseButtonClicked;
            }

            var actionButton = m_actionButton;
            if (actionButton != null)
            {
                actionButton.Click += OnActionButtonClicked;
            }

            UpdateButtonsState();
            OnIsLightDismissEnabledChanged();
            OnIconSourceChanged();
            OnHeroContentPlacementChanged();

            EstablishShadows();

            m_isTemplateApplied = true;
        }

        private void OnPropertyChanged(DependencyPropertyChangedEventArgs args)
        {
            DependencyProperty property = args.Property;

            if (property == IsOpenProperty)
            {
                OnIsOpenChanged();
            }
            else if (property == TargetProperty)
            {
                // Unregister from old target if it exists
                var value = args.OldValue;
                if (value != null)
                {
                    FrameworkElement newTarget = (FrameworkElement)value;
                    newTarget.Unloaded -= ClosePopupOnUnloadEvent;
                }

                // Register to new target if it exists
                value = args.NewValue;
                if (value != null)
                {
                    FrameworkElement newTarget = (FrameworkElement)value;
                    newTarget.Unloaded += ClosePopupOnUnloadEvent;
                }
                OnTargetChanged();
            }
            else if (property == PlacementMarginProperty)
            {
                OnPlacementMarginChanged();
            }
            else if (property == IsLightDismissEnabledProperty)
            {
                OnIsLightDismissEnabledChanged();
            }
            else if (property == ShouldConstrainToRootBoundsProperty)
            {
                OnShouldConstrainToRootBoundsChanged();
            }
            else if (property == TailVisibilityProperty)
            {
                OnTailVisibilityChanged();
            }
            else if (property == PreferredPlacementProperty)
            {
                if (IsOpen)
                {
                    PositionPopup();
                }
            }
            else if (property == HeroContentPlacementProperty)
            {
                OnHeroContentPlacementChanged();
            }
            else if (property == IconSourceProperty)
            {
                OnIconSourceChanged();
            }
            else if (property == TitleProperty)
            {
                SetPopupAutomationProperties();
                if (ToggleVisibilityForEmptyContent(c_TitleTextBlockVisibleStateName, c_TitleTextBlockCollapsedStateName, Title))
                {
                    TeachingTipTestHooks.NotifyTitleVisibilityChanged(this);
                }
            }
            else if (property == SubtitleProperty)
            {
                if (ToggleVisibilityForEmptyContent(c_SubtitleTextBlockVisibleStateName, c_SubtitleTextBlockCollapsedStateName, Subtitle))
                {
                    TeachingTipTestHooks.NotifySubtitleVisibilityChanged(this);
                }
            }
            else if (property == ActionButtonContentProperty)
            {
                UpdateButtonsState();
                object value = args.NewValue;
                UpdateButtonAutomationProperties(m_actionButton, value);
            }
            else if (property == CloseButtonContentProperty)
            {
                UpdateButtonsState();
                object value = args.NewValue;
                UpdateButtonAutomationProperties(m_closeButton, value);
            }
        }

        public void UpdateButtonAutomationProperties(Button button, object content)
        {
            if (button != null)
            {
                string nameHString = SharedHelpers.TryGetStringRepresentationFromObject(content);
                AutomationProperties.SetName(button, nameHString);
            }
        }

        public bool ToggleVisibilityForEmptyContent(string visibleStateName, string collapsedStateName, string content)
        {
            if (content != "")
            {
                VisualStateManager.GoToState(this, visibleStateName, false);
                return true;
            }
            else
            {
                VisualStateManager.GoToState(this, collapsedStateName, false);
                return true;
            }
        }

        protected override void OnContentChanged(object oldContent, object newContent)
        {
            if (newContent != null)
            {
                VisualStateManager.GoToState(this, "Content", false);
            }
            else
            {
                VisualStateManager.GoToState(this, "NoContent", false);
            }
        }

        private void SetPopupAutomationProperties()
        {
            var popup = m_popup;
            if (popup != null)
            {
                var name = AutomationProperties.GetName(this);
                if (string.IsNullOrEmpty(name))
                {
                    name = Title ?? string.Empty;
                }
                AutomationProperties.SetName(popup, name);

                AutomationProperties.SetAutomationId(popup, AutomationProperties.GetAutomationId(this));
            }
        }

        // Playing a closing animation when the Teaching Tip is closed via light dismiss requires this work around.
        // This is because there is no event that occurs when a popup is closing due to light dismiss so we have no way to intercept
        // the close and play our animation first. To work around this we've created a second popup which has no content and sits
        // underneath the teaching tip and is put into light dismiss mode instead of the primary popup. Then when this popup closes
        // due to light dismiss we know we are supposed to close the primary popup as well. To ensure that this popup does not block
        // interaction to the primary popup we need to make sure that the LightDismissIndicatorPopup is always opened first, so that
        // it is Z ordered underneath the primary popup.
        private void CreateLightDismissIndicatorPopup()
        {
            var popup = new Popup { AllowsTransparency = true };
            //// Set XamlRoot on the popup to handle XamlIsland/AppWindow scenarios.
            //UIElement uiElement10 = this
            //if (uiElement10 != null)
            //{
            //    popup.XamlRoot = uiElement10.XamlRoot;
            //}

            // A Popup needs contents to open, so set a child that doesn't do anything.
            var grid = new Grid();
            popup.Child = grid;
            if (Target != null)
            {
                popup.PlacementTarget = Target;
            }

            m_lightDismissIndicatorPopup = popup;
        }

        private bool UpdateTail()
        {
            // An effective placement of var indicates that no tail should be shown.
            var (placement, tipDoesNotFit) = DetermineEffectivePlacement();
            m_currentEffectiveTailPlacementMode = placement;
            var tailVisibility = TailVisibility;
            if (tailVisibility == TeachingTipTailVisibility.Collapsed || (m_target == null && tailVisibility != TeachingTipTailVisibility.Visible))
            {
                m_currentEffectiveTailPlacementMode = TeachingTipPlacementMode.Auto;
            }

            if (placement != m_currentEffectiveTipPlacementMode)
            {
                m_currentEffectiveTipPlacementMode = placement;
                TeachingTipTestHooks.NotifyEffectivePlacementChanged(this);
            }

            var nullableTailOcclusionGrid = m_tailOcclusionGrid;

            var height = nullableTailOcclusionGrid?.ActualHeight ?? 0;
            var width = nullableTailOcclusionGrid?.ActualWidth ?? 0;

            (double firstColumnWidth, double secondColumnWidth, double nextToLastColumnWidth, double lastColumnWidth) GetColumnWidths()
            {
                var columnDefinitions = nullableTailOcclusionGrid?.ColumnDefinitions;
                if (columnDefinitions != null)
                {
                    var firstColumnWidth = columnDefinitions.Count > 0 ? columnDefinitions[0].ActualWidth : 0.0;
                    var secondColumnWidth = columnDefinitions.Count > 1 ? columnDefinitions[1].ActualWidth : 0.0;
                    var nextToLastColumnWidth = columnDefinitions.Count > 1 ? columnDefinitions[columnDefinitions.Count - 2].ActualWidth : 0.0;
                    var lastColumnWidth = columnDefinitions.Count > 0 ? columnDefinitions[columnDefinitions.Count - 1].ActualWidth : 0.0;

                    return (firstColumnWidth, secondColumnWidth, nextToLastColumnWidth, lastColumnWidth);
                }
                return (0.0, 0.0, 0.0, 0.0);
            }
            var (firstColumnWidth, secondColumnWidth, nextToLastColumnWidth, lastColumnWidth) = GetColumnWidths();

            (double firstRowHeight, double secondRowHeight, double nextToLastRowHeight, double lastRowHeight) GetRowHeights()
            {
                var rowDefinitions = nullableTailOcclusionGrid?.RowDefinitions;
                if (rowDefinitions != null)
                {
                    var firstRowHeight = rowDefinitions.Count > 0 ? rowDefinitions[0].ActualHeight : 0.0;
                    var secondRowHeight = rowDefinitions.Count > 1 ? rowDefinitions[1].ActualHeight : 0.0;
                    var nextToLastRowHeight = rowDefinitions.Count > 1 ? rowDefinitions[rowDefinitions.Count - 2].ActualHeight : 0.0;
                    var lastRowHeight = rowDefinitions.Count > 0 ? rowDefinitions[rowDefinitions.Count - 1].ActualHeight : 0.0;

                    return (firstRowHeight, secondRowHeight, nextToLastRowHeight, lastRowHeight);
                }
                return (0.0, 0.0, 0.0, 0.0);
            }

            var (firstRowHeight, secondRowHeight, nextToLastRowHeight, lastRowHeight) = GetRowHeights();

            UpdateSizeBasedTemplateSettings();

            switch (m_currentEffectiveTailPlacementMode)
            {
                // An effective placement of var means the tip should not display a tail.
                case TeachingTipPlacementMode.Auto:
                    TrySetCenterPoint(nullableTailOcclusionGrid, new Point(width / 2, height / 2));
                    UpdateDynamicHeroContentPlacementToTop();
                    VisualStateManager.GoToState(this, "Untargeted", false);
                    break;

                case TeachingTipPlacementMode.Top:
                    TrySetCenterPoint(nullableTailOcclusionGrid, new Point(width / 2, height - lastRowHeight));
                    TrySetCenterPoint(m_tailEdgeBorder, new Point((width / 2) - firstColumnWidth, 0.0));
                    UpdateDynamicHeroContentPlacementToTop();
                    VisualStateManager.GoToState(this, "Top", false);
                    break;

                case TeachingTipPlacementMode.Bottom:
                    TrySetCenterPoint(nullableTailOcclusionGrid, new Point(width / 2, firstRowHeight));
                    TrySetCenterPoint(m_tailEdgeBorder, new Point((width / 2) - firstColumnWidth, 0.0));
                    UpdateDynamicHeroContentPlacementToBottom();
                    VisualStateManager.GoToState(this, "Bottom", false);
                    break;

                case TeachingTipPlacementMode.Left:
                    TrySetCenterPoint(nullableTailOcclusionGrid, new Point(width - lastColumnWidth, height / 2));
                    TrySetCenterPoint(m_tailEdgeBorder, new Point(0.0, (height / 2) - firstRowHeight));
                    UpdateDynamicHeroContentPlacementToTop();
                    VisualStateManager.GoToState(this, "Left", false);
                    break;

                case TeachingTipPlacementMode.Right:
                    TrySetCenterPoint(nullableTailOcclusionGrid, new Point(firstColumnWidth, height / 2));
                    TrySetCenterPoint(m_tailEdgeBorder, new Point(0.0, (height / 2) - firstRowHeight));
                    UpdateDynamicHeroContentPlacementToTop();
                    VisualStateManager.GoToState(this, "Right", false);
                    break;

                case TeachingTipPlacementMode.TopRight:
                    TrySetCenterPoint(nullableTailOcclusionGrid, new Point(firstColumnWidth + secondColumnWidth + 1, height - lastRowHeight));
                    TrySetCenterPoint(m_tailEdgeBorder, new Point(secondColumnWidth, 0.0));
                    UpdateDynamicHeroContentPlacementToTop();
                    VisualStateManager.GoToState(this, "TopRight", false);
                    break;

                case TeachingTipPlacementMode.TopLeft:
                    TrySetCenterPoint(nullableTailOcclusionGrid, new Point(width - (nextToLastColumnWidth + lastColumnWidth + 1), height - lastRowHeight));
                    TrySetCenterPoint(m_tailEdgeBorder, new Point(width - (nextToLastColumnWidth + firstColumnWidth + lastColumnWidth), 0.0));
                    UpdateDynamicHeroContentPlacementToTop();
                    VisualStateManager.GoToState(this, "TopLeft", false);
                    break;

                case TeachingTipPlacementMode.BottomRight:
                    TrySetCenterPoint(nullableTailOcclusionGrid, new Point(firstColumnWidth + secondColumnWidth + 1, firstRowHeight));
                    TrySetCenterPoint(m_tailEdgeBorder, new Point(secondColumnWidth, 0.0));
                    UpdateDynamicHeroContentPlacementToBottom();
                    VisualStateManager.GoToState(this, "BottomRight", false);
                    break;

                case TeachingTipPlacementMode.BottomLeft:
                    TrySetCenterPoint(nullableTailOcclusionGrid, new Point(width - (nextToLastColumnWidth + lastColumnWidth + 1), firstRowHeight));
                    TrySetCenterPoint(m_tailEdgeBorder, new Point(width - (nextToLastColumnWidth + firstColumnWidth + lastColumnWidth), 0.0));
                    UpdateDynamicHeroContentPlacementToBottom();
                    VisualStateManager.GoToState(this, "BottomLeft", false);
                    break;

                case TeachingTipPlacementMode.LeftTop:
                    TrySetCenterPoint(nullableTailOcclusionGrid, new Point(width - lastColumnWidth, height - (nextToLastRowHeight + lastRowHeight + 1)));
                    TrySetCenterPoint(m_tailEdgeBorder, new Point(0.0, height - (nextToLastRowHeight + firstRowHeight + lastRowHeight)));
                    UpdateDynamicHeroContentPlacementToTop();
                    VisualStateManager.GoToState(this, "LeftTop", false);
                    break;

                case TeachingTipPlacementMode.LeftBottom:
                    TrySetCenterPoint(nullableTailOcclusionGrid, new Point(width - lastColumnWidth, firstRowHeight + secondRowHeight + 1));
                    TrySetCenterPoint(m_tailEdgeBorder, new Point(0.0, secondRowHeight));
                    UpdateDynamicHeroContentPlacementToBottom();
                    VisualStateManager.GoToState(this, "LeftBottom", false);
                    break;

                case TeachingTipPlacementMode.RightTop:
                    TrySetCenterPoint(nullableTailOcclusionGrid, new Point(firstColumnWidth, height - (nextToLastRowHeight + lastRowHeight + 1)));
                    TrySetCenterPoint(m_tailEdgeBorder, new Point(0.0, height - (nextToLastRowHeight + firstRowHeight + lastRowHeight)));
                    UpdateDynamicHeroContentPlacementToTop();
                    VisualStateManager.GoToState(this, "RightTop", false);
                    break;

                case TeachingTipPlacementMode.RightBottom:
                    TrySetCenterPoint(nullableTailOcclusionGrid, new Point(firstColumnWidth, firstRowHeight + secondRowHeight + 1));
                    TrySetCenterPoint(m_tailEdgeBorder, new Point(0.0, secondRowHeight));
                    UpdateDynamicHeroContentPlacementToBottom();
                    VisualStateManager.GoToState(this, "RightBottom", false);
                    break;

                case TeachingTipPlacementMode.Center:
                    TrySetCenterPoint(nullableTailOcclusionGrid, new Point(width / 2, height - lastRowHeight));
                    TrySetCenterPoint(m_tailEdgeBorder, new Point((width / 2) - firstColumnWidth, 0.0));
                    UpdateDynamicHeroContentPlacementToTop();
                    VisualStateManager.GoToState(this, "Center", false);
                    break;

                default:
                    break;
            }

            return tipDoesNotFit;
        }

        private void PositionPopup()
        {
            bool tipDoesNotFit = false;
            if (m_target != null)
            {
                tipDoesNotFit = PositionTargetedPopup();
            }
            else
            {
                tipDoesNotFit = PositionUntargetedPopup();
            }
            if (tipDoesNotFit)
            {
                IsOpen = false;
            }

            TeachingTipTestHooks.NotifyOffsetChanged(this);
        }

        private bool PositionTargetedPopup()
        {
            bool tipDoesNotFit = UpdateTail();
            //var offset = PlacementMargin;

            //(double tipHeight, double tipWidth) GetTipSize()
            //{
            //    var tailOcclusionGrid = m_tailOcclusionGrid;
            //    if (tailOcclusionGrid != null)
            //    {
            //        var tipHeight = tailOcclusionGrid.ActualHeight;
            //        var tipWidth = tailOcclusionGrid.ActualWidth;
            //        return (tipHeight, tipWidth);
            //    }
            //    return (0.0, 0.0);
            //}

            //var (tipHeight, tipWidth) = GetTipSize();

            //var popup = m_popup;
            //if (popup != null)
            //{
            //    // Depending on the effective placement mode of the tip we use a combination of the tip's size, the target's position within the app, the target's
            //    // size, and the target offset property to determine the appropriate vertical and horizontal offsets of the popup that the tip is contained in.
            //    switch (m_currentEffectiveTipPlacementMode)
            //    {
            //        case TeachingTipPlacementMode.Top:
            //            popup.VerticalOffset = m_currentTargetBoundsInCoreWindowSpace.Y - tipHeight - offset.Top;
            //            popup.HorizontalOffset = ((m_currentTargetBoundsInCoreWindowSpace.X * 2.0f) + m_currentTargetBoundsInCoreWindowSpace.Width - tipWidth) / 2.0f;
            //            break;

            //        case TeachingTipPlacementMode.Bottom:
            //            popup.VerticalOffset = m_currentTargetBoundsInCoreWindowSpace.Y + m_currentTargetBoundsInCoreWindowSpace.Height + (float)offset.Bottom;
            //            popup.HorizontalOffset = ((m_currentTargetBoundsInCoreWindowSpace.X * 2.0f) + m_currentTargetBoundsInCoreWindowSpace.Width - tipWidth) / 2.0f;
            //            break;

            //        case TeachingTipPlacementMode.Left:
            //            popup.VerticalOffset = ((m_currentTargetBoundsInCoreWindowSpace.Y * 2.0f) + m_currentTargetBoundsInCoreWindowSpace.Height - tipHeight) / 2.0f;
            //            popup.HorizontalOffset = m_currentTargetBoundsInCoreWindowSpace.X - tipWidth - offset.Left;
            //            break;

            //        case TeachingTipPlacementMode.Right:
            //            popup.VerticalOffset = ((m_currentTargetBoundsInCoreWindowSpace.Y * 2.0f) + m_currentTargetBoundsInCoreWindowSpace.Height - tipHeight) / 2.0f;
            //            popup.HorizontalOffset = m_currentTargetBoundsInCoreWindowSpace.X + m_currentTargetBoundsInCoreWindowSpace.Width + (float)offset.Right;
            //            break;

            //        case TeachingTipPlacementMode.TopRight:
            //            popup.VerticalOffset = m_currentTargetBoundsInCoreWindowSpace.Y - tipHeight - offset.Top;
            //            popup.HorizontalOffset = (((m_currentTargetBoundsInCoreWindowSpace.X * 2.0f) + m_currentTargetBoundsInCoreWindowSpace.Width) / 2.0f) - MinimumTipEdgeToTailCenter();
            //            break;

            //        case TeachingTipPlacementMode.TopLeft:
            //            popup.VerticalOffset = m_currentTargetBoundsInCoreWindowSpace.Y - tipHeight - offset.Top;
            //            popup.HorizontalOffset = (((m_currentTargetBoundsInCoreWindowSpace.X * 2.0f) + m_currentTargetBoundsInCoreWindowSpace.Width) / 2.0f) - tipWidth + MinimumTipEdgeToTailCenter();
            //            break;

            //        case TeachingTipPlacementMode.BottomRight:
            //            popup.VerticalOffset = m_currentTargetBoundsInCoreWindowSpace.Y + m_currentTargetBoundsInCoreWindowSpace.Height + (float)offset.Bottom;
            //            popup.HorizontalOffset = (((m_currentTargetBoundsInCoreWindowSpace.X * 2.0f) + m_currentTargetBoundsInCoreWindowSpace.Width) / 2.0f) - MinimumTipEdgeToTailCenter();
            //            break;

            //        case TeachingTipPlacementMode.BottomLeft:
            //            popup.VerticalOffset = m_currentTargetBoundsInCoreWindowSpace.Y + m_currentTargetBoundsInCoreWindowSpace.Height + (float)offset.Bottom;
            //            popup.HorizontalOffset = (((m_currentTargetBoundsInCoreWindowSpace.X * 2.0f) + m_currentTargetBoundsInCoreWindowSpace.Width) / 2.0f) - tipWidth + MinimumTipEdgeToTailCenter();
            //            break;

            //        case TeachingTipPlacementMode.LeftTop:
            //            popup.VerticalOffset = (((m_currentTargetBoundsInCoreWindowSpace.Y * 2.0f) + m_currentTargetBoundsInCoreWindowSpace.Height) / 2.0f) - tipHeight + MinimumTipEdgeToTailCenter();
            //            popup.HorizontalOffset = m_currentTargetBoundsInCoreWindowSpace.X - tipWidth - offset.Left;
            //            break;

            //        case TeachingTipPlacementMode.LeftBottom:
            //            popup.VerticalOffset = (((m_currentTargetBoundsInCoreWindowSpace.Y * 2.0f) + m_currentTargetBoundsInCoreWindowSpace.Height) / 2.0f) - MinimumTipEdgeToTailCenter();
            //            popup.HorizontalOffset = m_currentTargetBoundsInCoreWindowSpace.X - tipWidth - offset.Left;
            //            break;

            //        case TeachingTipPlacementMode.RightTop:
            //            popup.VerticalOffset = (((m_currentTargetBoundsInCoreWindowSpace.Y * 2.0f) + m_currentTargetBoundsInCoreWindowSpace.Height) / 2.0f) - tipHeight + MinimumTipEdgeToTailCenter();
            //            popup.HorizontalOffset = m_currentTargetBoundsInCoreWindowSpace.X + m_currentTargetBoundsInCoreWindowSpace.Width + (float)offset.Right;
            //            break;

            //        case TeachingTipPlacementMode.RightBottom:
            //            popup.VerticalOffset = (((m_currentTargetBoundsInCoreWindowSpace.Y * 2.0f) + m_currentTargetBoundsInCoreWindowSpace.Height) / 2.0f) - MinimumTipEdgeToTailCenter();
            //            popup.HorizontalOffset = m_currentTargetBoundsInCoreWindowSpace.X + m_currentTargetBoundsInCoreWindowSpace.Width + (float)offset.Right;
            //            break;

            //        case TeachingTipPlacementMode.Center:
            //            popup.VerticalOffset = m_currentTargetBoundsInCoreWindowSpace.Y + (m_currentTargetBoundsInCoreWindowSpace.Height / 2.0f) - tipHeight - offset.Top;
            //            popup.HorizontalOffset = ((m_currentTargetBoundsInCoreWindowSpace.X * 2.0f) + m_currentTargetBoundsInCoreWindowSpace.Width - tipWidth) / 2.0f;
            //            break;

            //        default:
            //            throw new InvalidOperationException("Provided placement is not supported");
            //    }
            //}
            return tipDoesNotFit;
        }

        private bool PositionUntargetedPopup()
        {
            var windowBoundsInCoreWindowSpace = GetEffectiveWindowBoundsInCoreWindowSpace(GetWindowBounds());

            (double finalTipHeight, double finalTipWidth) GetFinalTipSize()
            {
                var tailOcclusionGrid = m_tailOcclusionGrid;
                if (tailOcclusionGrid != null)
                {
                    var finalTipHeight = tailOcclusionGrid.ActualHeight;
                    var finalTipWidth = tailOcclusionGrid.ActualWidth;
                    return (finalTipHeight, finalTipWidth);
                }
                return (0.0, 0.0);
            }

            var (finalTipHeight, finalTipWidth) = GetFinalTipSize();

            bool tipDoesNotFit = UpdateTail();

            var offset = PlacementMargin;

            // Depending on the effective placement mode of the tip we use a combination of the tip's size, the window's size, and the target
            // offset property to determine the appropriate vertical and horizontal offsets of the popup that the tip is contained in.
            var popup = m_popup;
            if (popup != null)
            {
                switch (GetFlowDirectionAdjustedPlacement(PreferredPlacement))
                {
                    case TeachingTipPlacementMode.Auto:
                    case TeachingTipPlacementMode.Bottom:
                        popup.VerticalOffset = UntargetedTipFarPlacementOffset(windowBoundsInCoreWindowSpace.Height, finalTipHeight, offset.Bottom);
                        popup.HorizontalOffset = UntargetedTipCenterPlacementOffset(windowBoundsInCoreWindowSpace.X, windowBoundsInCoreWindowSpace.Width, finalTipWidth, offset.Left, offset.Right);
                        break;

                    case TeachingTipPlacementMode.Top:
                        popup.VerticalOffset = UntargetedTipNearPlacementOffset(windowBoundsInCoreWindowSpace.Y, offset.Top);
                        popup.HorizontalOffset = UntargetedTipCenterPlacementOffset(windowBoundsInCoreWindowSpace.X, windowBoundsInCoreWindowSpace.Width, finalTipWidth, offset.Left, offset.Right);
                        break;

                    case TeachingTipPlacementMode.Left:
                        popup.VerticalOffset = UntargetedTipCenterPlacementOffset(windowBoundsInCoreWindowSpace.Y, windowBoundsInCoreWindowSpace.Height, finalTipHeight, offset.Top, offset.Bottom);
                        popup.HorizontalOffset = UntargetedTipNearPlacementOffset(windowBoundsInCoreWindowSpace.X, offset.Left);
                        break;

                    case TeachingTipPlacementMode.Right:
                        popup.VerticalOffset = UntargetedTipCenterPlacementOffset(windowBoundsInCoreWindowSpace.Y, windowBoundsInCoreWindowSpace.Height, finalTipHeight, offset.Top, offset.Bottom);
                        popup.HorizontalOffset = UntargetedTipFarPlacementOffset(windowBoundsInCoreWindowSpace.Width, finalTipWidth, offset.Right);
                        break;

                    case TeachingTipPlacementMode.TopRight:
                        popup.VerticalOffset = UntargetedTipNearPlacementOffset(windowBoundsInCoreWindowSpace.Y, offset.Top);
                        popup.HorizontalOffset = UntargetedTipFarPlacementOffset(windowBoundsInCoreWindowSpace.Width, finalTipWidth, offset.Right);
                        break;

                    case TeachingTipPlacementMode.TopLeft:
                        popup.VerticalOffset = UntargetedTipNearPlacementOffset(windowBoundsInCoreWindowSpace.Y, offset.Top);
                        popup.HorizontalOffset = UntargetedTipNearPlacementOffset(windowBoundsInCoreWindowSpace.X, offset.Left);
                        break;

                    case TeachingTipPlacementMode.BottomRight:
                        popup.VerticalOffset = UntargetedTipFarPlacementOffset(windowBoundsInCoreWindowSpace.Height, finalTipHeight, offset.Bottom);
                        popup.HorizontalOffset = UntargetedTipFarPlacementOffset(windowBoundsInCoreWindowSpace.Width, finalTipWidth, offset.Right);
                        break;

                    case TeachingTipPlacementMode.BottomLeft:
                        popup.VerticalOffset = UntargetedTipFarPlacementOffset(windowBoundsInCoreWindowSpace.Height, finalTipHeight, offset.Bottom);
                        popup.HorizontalOffset = UntargetedTipNearPlacementOffset(windowBoundsInCoreWindowSpace.X, offset.Left);
                        break;

                    case TeachingTipPlacementMode.LeftTop:
                        popup.VerticalOffset = UntargetedTipNearPlacementOffset(windowBoundsInCoreWindowSpace.Y, offset.Top);
                        popup.HorizontalOffset = UntargetedTipNearPlacementOffset(windowBoundsInCoreWindowSpace.X, offset.Left);
                        break;

                    case TeachingTipPlacementMode.LeftBottom:
                        popup.VerticalOffset = UntargetedTipFarPlacementOffset(windowBoundsInCoreWindowSpace.Height, finalTipHeight, offset.Bottom);
                        popup.HorizontalOffset = UntargetedTipNearPlacementOffset(windowBoundsInCoreWindowSpace.X, offset.Left);
                        break;

                    case TeachingTipPlacementMode.RightTop:
                        popup.VerticalOffset = UntargetedTipNearPlacementOffset(windowBoundsInCoreWindowSpace.Y, offset.Top);
                        popup.HorizontalOffset = UntargetedTipFarPlacementOffset(windowBoundsInCoreWindowSpace.Width, finalTipWidth, offset.Right);
                        break;

                    case TeachingTipPlacementMode.RightBottom:
                        popup.VerticalOffset = UntargetedTipFarPlacementOffset(windowBoundsInCoreWindowSpace.Height, finalTipHeight, offset.Bottom);
                        popup.HorizontalOffset = UntargetedTipFarPlacementOffset(windowBoundsInCoreWindowSpace.Width, finalTipWidth, offset.Right);
                        break;

                    case TeachingTipPlacementMode.Center:
                        popup.VerticalOffset = UntargetedTipCenterPlacementOffset(windowBoundsInCoreWindowSpace.Y, windowBoundsInCoreWindowSpace.Height, finalTipHeight, offset.Top, offset.Bottom);
                        popup.HorizontalOffset = UntargetedTipCenterPlacementOffset(windowBoundsInCoreWindowSpace.X, windowBoundsInCoreWindowSpace.Width, finalTipWidth, offset.Left, offset.Right);
                        break;

                    default:
                        throw new InvalidOperationException("Provided placement is not supported");
                }
            }

            return tipDoesNotFit;
        }

        private void UpdateSizeBasedTemplateSettings()
        {
            var templateSettings = TemplateSettings;

            (double width, double height) GetSize()
            {
                var contentRootGrid = m_contentRootGrid;
                if (contentRootGrid != null)
                {
                    var width = contentRootGrid.ActualWidth;
                    var height = contentRootGrid.ActualHeight;

                    return (width, height);
                }

                return (0.0, 0.0);
            }

            var (width, height) = GetSize();

            switch (m_currentEffectiveTailPlacementMode)
            {
                case TeachingTipPlacementMode.Top:
                    templateSettings.TopRightHighlightMargin = OtherPlacementTopRightHighlightMargin(width, height);
                    templateSettings.TopLeftHighlightMargin = TopEdgePlacementTopLeftHighlightMargin(width, height);
                    break;
                case TeachingTipPlacementMode.Bottom:
                    templateSettings.TopRightHighlightMargin = BottomPlacementTopRightHighlightMargin(width, height);
                    templateSettings.TopLeftHighlightMargin = BottomPlacementTopLeftHighlightMargin(width, height);
                    break;
                case TeachingTipPlacementMode.Left:
                    templateSettings.TopRightHighlightMargin = OtherPlacementTopRightHighlightMargin(width, height);
                    templateSettings.TopLeftHighlightMargin = LeftEdgePlacementTopLeftHighlightMargin(width, height);
                    break;
                case TeachingTipPlacementMode.Right:
                    templateSettings.TopRightHighlightMargin = OtherPlacementTopRightHighlightMargin(width, height);
                    templateSettings.TopLeftHighlightMargin = RightEdgePlacementTopLeftHighlightMargin(width, height);
                    break;
                case TeachingTipPlacementMode.TopLeft:
                    templateSettings.TopRightHighlightMargin = OtherPlacementTopRightHighlightMargin(width, height);
                    templateSettings.TopLeftHighlightMargin = TopEdgePlacementTopLeftHighlightMargin(width, height);
                    break;
                case TeachingTipPlacementMode.TopRight:
                    templateSettings.TopRightHighlightMargin = OtherPlacementTopRightHighlightMargin(width, height);
                    templateSettings.TopLeftHighlightMargin = TopEdgePlacementTopLeftHighlightMargin(width, height);
                    break;
                case TeachingTipPlacementMode.BottomLeft:
                    templateSettings.TopRightHighlightMargin = BottomLeftPlacementTopRightHighlightMargin(width, height);
                    templateSettings.TopLeftHighlightMargin = BottomLeftPlacementTopLeftHighlightMargin(width, height);
                    break;
                case TeachingTipPlacementMode.BottomRight:
                    templateSettings.TopRightHighlightMargin = BottomRightPlacementTopRightHighlightMargin(width, height);
                    templateSettings.TopLeftHighlightMargin = BottomRightPlacementTopLeftHighlightMargin(width, height);
                    break;
                case TeachingTipPlacementMode.LeftTop:
                    templateSettings.TopRightHighlightMargin = OtherPlacementTopRightHighlightMargin(width, height);
                    templateSettings.TopLeftHighlightMargin = LeftEdgePlacementTopLeftHighlightMargin(width, height);
                    break;
                case TeachingTipPlacementMode.LeftBottom:
                    templateSettings.TopRightHighlightMargin = OtherPlacementTopRightHighlightMargin(width, height);
                    templateSettings.TopLeftHighlightMargin = LeftEdgePlacementTopLeftHighlightMargin(width, height);
                    break;
                case TeachingTipPlacementMode.RightTop:
                    templateSettings.TopRightHighlightMargin = OtherPlacementTopRightHighlightMargin(width, height);
                    templateSettings.TopLeftHighlightMargin = RightEdgePlacementTopLeftHighlightMargin(width, height);
                    break;
                case TeachingTipPlacementMode.RightBottom:
                    templateSettings.TopRightHighlightMargin = OtherPlacementTopRightHighlightMargin(width, height);
                    templateSettings.TopLeftHighlightMargin = RightEdgePlacementTopLeftHighlightMargin(width, height);
                    break;
                case TeachingTipPlacementMode.Auto:
                    templateSettings.TopRightHighlightMargin = OtherPlacementTopRightHighlightMargin(width, height);
                    templateSettings.TopLeftHighlightMargin = TopEdgePlacementTopLeftHighlightMargin(width, height);
                    break;
                case TeachingTipPlacementMode.Center:
                    templateSettings.TopRightHighlightMargin = OtherPlacementTopRightHighlightMargin(width, height);
                    templateSettings.TopLeftHighlightMargin = TopEdgePlacementTopLeftHighlightMargin(width, height);
                    break;
            }
        }

        private void UpdateButtonsState()
        {
            var actionContent = ActionButtonContent;
            var closeContent = CloseButtonContent;
            bool isLightDismiss = IsLightDismissEnabled;

            if (actionContent != null && closeContent != null)
            {
                VisualStateManager.GoToState(this, "BothButtonsVisible", false);
                VisualStateManager.GoToState(this, "FooterCloseButton", false);
            }
            else if (actionContent != null && isLightDismiss)
            {
                VisualStateManager.GoToState(this, "ActionButtonVisible", false);
                VisualStateManager.GoToState(this, "FooterCloseButton", false);
            }
            else if (actionContent != null)
            {
                VisualStateManager.GoToState(this, "ActionButtonVisible", false);
                VisualStateManager.GoToState(this, "HeaderCloseButton", false);
            }
            else if (closeContent != null)
            {
                VisualStateManager.GoToState(this, "CloseButtonVisible", false);
                VisualStateManager.GoToState(this, "FooterCloseButton", false);
            }
            else if (isLightDismiss)
            {
                VisualStateManager.GoToState(this, "NoButtonsVisible", false);
                VisualStateManager.GoToState(this, "FooterCloseButton", false);
            }
            else
            {
                VisualStateManager.GoToState(this, "NoButtonsVisible", false);
                VisualStateManager.GoToState(this, "HeaderCloseButton", false);
            }
        }

        private void UpdateDynamicHeroContentPlacementToTop()
        {
            if (HeroContentPlacement == TeachingTipHeroContentPlacementMode.Auto)
            {
                UpdateDynamicHeroContentPlacementToTopImpl();
            }
        }

        private void UpdateDynamicHeroContentPlacementToTopImpl()
        {
            VisualStateManager.GoToState(this, "HeroContentTop", false);
            if (m_currentHeroContentEffectivePlacementMode != TeachingTipHeroContentPlacementMode.Top)
            {
                m_currentHeroContentEffectivePlacementMode = TeachingTipHeroContentPlacementMode.Top;
                TeachingTipTestHooks.NotifyEffectiveHeroContentPlacementChanged(this);
            }
        }

        private void UpdateDynamicHeroContentPlacementToBottom()
        {
            if (HeroContentPlacement == TeachingTipHeroContentPlacementMode.Auto)
            {
                UpdateDynamicHeroContentPlacementToBottomImpl();
            }
        }

        private void UpdateDynamicHeroContentPlacementToBottomImpl()
        {
            VisualStateManager.GoToState(this, "HeroContentBottom", false);
            if (m_currentHeroContentEffectivePlacementMode != TeachingTipHeroContentPlacementMode.Bottom)
            {
                m_currentHeroContentEffectivePlacementMode = TeachingTipHeroContentPlacementMode.Bottom;
                TeachingTipTestHooks.NotifyEffectiveHeroContentPlacementChanged(this);
            }
        }

        private void OnIsOpenChanged()
        {
            SharedHelpers.QueueCallbackForCompositionRendering(() =>
            {
                if (IsOpen)
                {
                    IsOpenChangedToOpen();
                }
                else
                {
                    IsOpenChangedToClose();
                }
                TeachingTipTestHooks.NotifyOpenedStatusChanged(this);
            });
        }

        void IsOpenChangedToOpen()
        {
            //Reset the close reason to the default value of programmatic.
            m_lastCloseReason = TeachingTipCloseReason.Programmatic;

            m_currentBoundsInCoreWindowSpace = TransformToVisual(Window.GetWindow(this)).TransformBounds(
                new Rect(
                    0.0,
                    0.0,
                    (float)ActualWidth,
                    (float)ActualHeight));

            Rect GetCurrentTargetBoundsInCoreWindowSpace()
            {
                var target = m_target;
                if (target != null)
                {
                    SetViewportChangedEvent(target);
                    return target.TransformToVisual(Window.GetWindow(target)).TransformBounds(
                        new Rect(
                            0.0,
                            0.0,
                            (float)target.ActualWidth,
                            (float)target.ActualHeight));
                }
                return new Rect(0, 0, 0, 0);
            }
            m_currentTargetBoundsInCoreWindowSpace = GetCurrentTargetBoundsInCoreWindowSpace();

            if (m_lightDismissIndicatorPopup == null)
            {
                CreateLightDismissIndicatorPopup();
            }
            OnIsLightDismissEnabledChanged();

            if (m_contractAnimation == null)
            {
                CreateContractAnimation();
            }
            if (m_expandAnimation == null)
            {
                CreateExpandAnimation();
            }

            //If the developer defines their TeachingTip in a resource dictionary it is possible that it's template will have never been applied
            if (!m_isTemplateApplied)
            {
                ApplyTemplate();
            }

            if (m_popup == null || m_createNewPopupOnOpen)
            {
                CreateNewPopup();
            }

            // If the tip is not going to open because it does not fit we need to make sure that
            // the open, closing, closed life cycle still fires so that we don't cause apps to leak
            // that depend on this sequence.
            var (ignored, tipDoesNotFit) = DetermineEffectivePlacement();
            if (tipDoesNotFit)
            {
                //__RP_Marker_ClassMemberById(RuntimeProfiler.ProfId_TeachingTip, RuntimeProfiler.ProfMemberId_TeachingTip_TipDidNotOpenDueToSize);
                RaiseClosingEvent(false);
                var closedArgs = new TeachingTipClosedEventArgs(m_lastCloseReason);
                Closed?.Invoke(this, closedArgs);
                IsOpen = false;
            }
            else
            {
                var popup = m_popup;
                if (popup != null)
                {
                    if (!popup.IsOpen)
                    {
                        // We are about to begin the process of trying to open the teaching tip, so notify that we are no longer idle.
                        SetIsIdle(false);
                        UpdatePopupRequestedTheme();
                        popup.Child = m_rootElement;
                        var lightDismissIndicatorPopup = m_lightDismissIndicatorPopup;
                        if (lightDismissIndicatorPopup != null)
                        {
                            lightDismissIndicatorPopup.IsOpen = true;
                        }
                        popup.IsOpen = true;
                    }
                    else
                    {
                        // We have become Open but our popup was already open. This can happen when a close is canceled by the closing event, so make sure the idle status is correct.
                        if (!m_isExpandAnimationPlaying && !m_isContractAnimationPlaying)
                        {
                            SetIsIdle(true);
                        }
                    }
                }
            }

            PreviewKeyDown += OnF6AcceleratorKeyClicked;
            //m_acceleratorKeyActivatedRevoker = Dispatcher().AcceleratorKeyActivated(auto_revoke, { this, &OnF6AcceleratorKeyClicked });

            // Make sure we are in the correct VSM state after ApplyTemplate and moving the template content from the Control to the Popup:
            OnIsLightDismissEnabledChanged();
        }

        void IsOpenChangedToClose()
        {
            var popup = m_popup;
            if (popup != null)
            {
                if (popup.IsOpen)
                {
                    // We are about to begin the process of trying to close the teaching tip, so notify that we are no longer idle.
                    SetIsIdle(false);
                    popup.IsOpen = false;
                    RaiseClosingEvent(true);
                }
                else
                {
                    // We have become not Open but our popup was already not open. Lets make sure the idle status is correct.
                    if (!m_isExpandAnimationPlaying && !m_isContractAnimationPlaying)
                    {
                        SetIsIdle(true);
                    }
                }
            }

            m_currentEffectiveTipPlacementMode = TeachingTipPlacementMode.Auto;
            TeachingTipTestHooks.NotifyEffectivePlacementChanged(this);
        }

        void CreateNewPopup()
        {
            if (m_popup != null)
            {
                m_popup.Opened -= OnPopupOpened;
                m_popup.Closed -= OnPopupClosed;
            }

            var popup = new Popup { AllowsTransparency = true };
            // Set XamlRoot on the popup to handle XamlIsland/AppWindow scenarios.
            /*
            UIElement uiElement10 = this;
            if (uiElement10 != null)
            {
                popup.XamlRoot = uiElement10.XamlRoot;
            }
            */

            popup.Opened += OnPopupOpened;
            popup.Closed += OnPopupClosed;
            if (Target != null)
            {
                popup.PlacementTarget = Target;
            }

            //popup.ShouldConstrainToRootBounds = ShouldConstrainToRootBounds;

            m_popup = popup;
            SetPopupAutomationProperties();
            m_createNewPopupOnOpen = false;
        }

        private void OnTailVisibilityChanged()
        {
            UpdateTail();
        }

        private void OnIconSourceChanged()
        {
            var templateSettings = TemplateSettings;
            var source = IconSource;
            if (source != null)
            {
                templateSettings.IconElement = SharedHelpers.MakeIconElementFrom(source);
                VisualStateManager.GoToState(this, "Icon", false);
            }
            else
            {
                templateSettings.IconElement = null;
                VisualStateManager.GoToState(this, "NoIcon", false);
            }
        }

        private void OnPlacementMarginChanged()
        {
            if (IsOpen)
            {
                PositionPopup();
            }
        }

        private void OnIsLightDismissEnabledChanged()
        {
            if (IsLightDismissEnabled)
            {
                VisualStateManager.GoToState(this, "LightDismiss", false);

                var lightDismissIndicatorPopup = m_lightDismissIndicatorPopup;
                if (lightDismissIndicatorPopup != null)
                {
                    lightDismissIndicatorPopup.StaysOpen = false;
                    lightDismissIndicatorPopup.Closed += OnLightDismissIndicatorPopupClosed;
                }
            }
            else
            {
                VisualStateManager.GoToState(this, "NormalDismiss", false);

                var lightDismissIndicatorPopup = m_lightDismissIndicatorPopup;
                if (lightDismissIndicatorPopup != null)
                {
                    lightDismissIndicatorPopup.StaysOpen = true;
                    lightDismissIndicatorPopup.Closed -= OnLightDismissIndicatorPopupClosed;
                }
            }
            UpdateButtonsState();
        }

        private void OnShouldConstrainToRootBoundsChanged()
        {
            // ShouldConstrainToRootBounds is a property that can only be set on a popup before it is opened.
            // If we have opened the tip's popup and then this property changes we will need to discard the old popup
            // and replace it with a new popup.  This variable indicates this state.

            //The underlying popup api is only available on 19h1 plus, if we aren't on that no opt.
            if (m_popup != null)
            {
                m_createNewPopupOnOpen = true;
            }
        }

        private void OnHeroContentPlacementChanged()
        {
            switch (HeroContentPlacement)
            {
                case TeachingTipHeroContentPlacementMode.Auto:
                    break;
                case TeachingTipHeroContentPlacementMode.Top:
                    UpdateDynamicHeroContentPlacementToTopImpl();
                    break;
                case TeachingTipHeroContentPlacementMode.Bottom:
                    UpdateDynamicHeroContentPlacementToBottomImpl();
                    break;
            }

            // Setting m_currentEffectiveTipPlacementMode to var ensures that the next time position popup is called we'll rerun the DetermineEffectivePlacement
            // algorithm. If we did not do this and the popup was opened the algorithm would maintain the current effective placement mode, which we don't want
            // since the hero content placement contributes to the choice of tip placement mode.
            m_currentEffectiveTipPlacementMode = TeachingTipPlacementMode.Auto;
            TeachingTipTestHooks.NotifyEffectivePlacementChanged(this);
            if (IsOpen)
            {
                PositionPopup();
            }
        }

        private void OnContentSizeChanged(object sender, SizeChangedEventArgs args)
        {
            UpdateSizeBasedTemplateSettings();
            // Reset the currentEffectivePlacementMode so that the tail will be updated for the new size as well.
            m_currentEffectiveTipPlacementMode = TeachingTipPlacementMode.Auto;
            TeachingTipTestHooks.NotifyEffectivePlacementChanged(this);
            if (IsOpen)
            {
                PositionPopup();
            }

            var expandAnimation = m_expandAnimation;
            if (expandAnimation != null)
            {
                //expandAnimation.SetScalarParameter("Width", args.NewSize.Width);
                //expandAnimation.SetScalarParameter("Height", args.NewSize.Height);
            }
            var contractAnimation = m_contractAnimation;
            if (contractAnimation != null)
            {
                //contractAnimation.SetScalarParameter("Width", args.NewSize.Width);
                //contractAnimation.SetScalarParameter("Height", args.NewSize.Height);
            }
        }

        private void OnF6AcceleratorKeyClicked(object sender, KeyEventArgs args)
        {
            if (!args.Handled &&
                IsOpen &&
                args.Key == Key.F6 &&
                args.IsDown)
            {
                bool GetHasFocusInSubTree()
                {
                    var current = FocusManager.GetFocusedElement(this) as DependencyObject;
                    var rootElement = m_rootElement;
                    if (rootElement != null)
                    {
                        while (current != null)
                        {
                            if ((current as UIElement) == rootElement)
                            {
                                return true;
                            }
                            current = VisualTreeHelper.GetParent(current);
                        }
                    }

                    return false;
                }

                bool hasFocusInSubTree = GetHasFocusInSubTree();

                if (hasFocusInSubTree)
                {
                    if (m_previouslyFocusedElement.TryGetTarget(out var previouslyFocusedElement))
                    {
                        args.Handled = previouslyFocusedElement.Focus();
                        m_previouslyFocusedElement = null;
                    }
                }
                else
                {
                    Button GetF6Button()
                    {
                        var firstButton = m_closeButton;
                        var secondButton = m_alternateCloseButton;
                        //Prefer the close button to the alternate, except when there is no content.
                        if (CloseButtonContent == null)
                        {
                            (secondButton, firstButton) = (firstButton, secondButton);
                        }
                        if (firstButton != null && firstButton.Visibility == Visibility.Visible)
                        {
                            return firstButton;
                        }
                        else if (secondButton != null && secondButton.Visibility == Visibility.Visible)
                        {
                            return secondButton;
                        }
                        return null;
                    }

                    Button f6Button = GetF6Button();
                    var window = Window.GetWindow(this);
                    if (f6Button != null && window != null)
                    {
                        m_previouslyFocusedElement = new WeakReference<IInputElement>(FocusManager.GetFocusedElement(window));
                        args.Handled = f6Button.Focus();
                    }
                }
            }
        }

        private void OnAutomationNameChanged(object sender, object args)
        {
            SetPopupAutomationProperties();
        }

        private void OnAutomationIdChanged(object sender, object args)
        {
            SetPopupAutomationProperties();
        }

        private void OnCloseButtonClicked(object sender, RoutedEventArgs args)
        {
            CloseButtonClick?.Invoke(this, null);
            m_lastCloseReason = TeachingTipCloseReason.CloseButton;
            IsOpen = false;
        }

        private void OnActionButtonClicked(object sender, RoutedEventArgs args)
        {
            ActionButtonClick?.Invoke(this, null);
        }

        private void OnPopupOpened(object sender, object args)
        {
            UIElement uiElement10 = this;
            if (uiElement10 != null)
            {
                var window = Window.GetWindow(this);
                if (window != null)
                {
                    window.SizeChanged += WindowSizeChanged;
                }
            }

            // Expand animation requires IUIElement9
            if (this is UIElement && SharedHelpers.IsAnimationsEnabled)
            {
                StartExpandToOpen();
            }
            else
            {
                // We won't be playing an animation so we're immediately idle.
                SetIsIdle(true);
            }

            #region WPF specific

            // This part is not necessary as TeachingTipAutomationPeer.RaiseWindowOpenedEvent() is an empty method

            /*
            var teachingTipPeer = FrameworkElementAutomationPeer.FromElement(this) as TeachingTipAutomationPeer;
            if (teachingTipPeer != null)
            {
                string GetNotificationString()
                {
                    string GetAppName()
                    {
                        try
                        {
                            var package = Package.Current;
                            if (package != null)
                            {
                                return package.DisplayName;
                            }
                        }
                        catch { }
                        return string.Empty;
                    }
                    var appName = GetAppName();
                    if (!string.IsNullOrEmpty(appName))
                    {
                        return string.Format(
                            Strings.TeachingTipNotification,
                            appName,
                            AutomationProperties.GetName(m_popup));
                    }
                    else
                    {
                        return string.Format(
                            Strings.TeachingTipNotificationWithoutAppName,
                            AutomationProperties.GetName(m_popup));
                    }
                }
                var notificationString = GetNotificationString();
                teachingTipPeer.RaiseWindowOpenedEvent(notificationString);
            }
            */

            #endregion
        }

        private void OnPopupClosed(object sender, object args)
        {
            var window = Window.GetWindow(this);
            if (window != null)
            {
                window.SizeChanged -= WindowSizeChanged;
            }
            //if (m_xamlRoot != null)
            //{
            //    m_xamlRoot.SizeChanged -= XamlRootChanged;
            //}
            //m_xamlRoot = null;

            var lightDismissIndicatorPopup = m_lightDismissIndicatorPopup;
            if (lightDismissIndicatorPopup != null)
            {
                lightDismissIndicatorPopup.IsOpen = false;
            }
            var popup = m_popup;
            if (popup != null)
            {
                popup.Child = null;
            }
            var myArgs = new TeachingTipClosedEventArgs(m_lastCloseReason);

            Closed?.Invoke(this, myArgs);

            //If we were closed by the close button and we have tracked a previously focused element because F6 was used
            //To give the tip focus, then we return focus when the popup closes.
            if (m_lastCloseReason == TeachingTipCloseReason.CloseButton)
            {
                if (m_previouslyFocusedElement != null && m_previouslyFocusedElement.TryGetTarget(out var previouslyFocusedElement))
                {
                    previouslyFocusedElement.Focus();
                }
            }
            m_previouslyFocusedElement = null;

            var teachingTipPeer = FrameworkElementAutomationPeer.FromElement(this) as TeachingTipAutomationPeer;
            if (teachingTipPeer != null)
            {
                teachingTipPeer.RaiseWindowClosedEvent();
            }
        }

        private void ClosePopupOnUnloadEvent(object sender, RoutedEventArgs args)
        {
            IsOpen = false;
            ClosePopup();
        }

        private void OnLightDismissIndicatorPopupClosed(object sender, object args)
        {
            if (IsOpen)
            {
                m_lastCloseReason = TeachingTipCloseReason.LightDismiss;
            }
            IsOpen = false;
        }

        private void RaiseClosingEvent(bool attachDeferralCompletedHandler)
        {
            var args = new TeachingTipClosingEventArgs(m_lastCloseReason);

            if (attachDeferralCompletedHandler)
            {
                Deferral instance = new Deferral(() =>
                {
                    if (!args.Cancel)
                    {
                        ClosePopupWithAnimationIfAvailable();
                    }
                    else
                    {
                        // The developer has changed the Cancel property to true, indicating that they wish to Cancel the
                        // closing of this tip, so we need to revert the IsOpen property to true.
                        IsOpen = true;
                    }
                });

                args.SetDeferral(instance);

                args.IncrementDeferralCount();
                Closing?.Invoke(this, args);
                args.DecrementDeferralCount();
            }
            else
            {
                Closing?.Invoke(this, args);
            }
        }

        private void ClosePopupWithAnimationIfAvailable()
        {
            if (m_popup != null && m_popup.IsOpen)
            {
                // Contract animation requires IUIElement9
                if (this is UIElement && SharedHelpers.IsAnimationsEnabled)
                {
                    StartContractToClose();
                }
                else
                {
                    ClosePopup();
                }

                // Under normal circumstances we would have launched an animation just now, if we did not then we should make sure
                // that the idle state is correct.
                if (!m_isContractAnimationPlaying && !m_isExpandAnimationPlaying)
                {
                    SetIsIdle(true);
                }
            }
        }

        private void ClosePopup()
        {
            var popup = m_popup;
            if (popup != null)
            {
                popup.IsOpen = false;
            }

            var lightDismissIndicatorPopup = m_lightDismissIndicatorPopup;
            if (lightDismissIndicatorPopup != null)
            {
                lightDismissIndicatorPopup.IsOpen = false;
            }

            var tailOcclusionGrid = m_tailOcclusionGrid;
            if (tailOcclusionGrid != null)
            {
                // A previous close animation may have left the rootGrid's scale at a very small value and if this teaching tip
                // is shown again then its text would be rasterized at this small scale and blown up ~20x. To fix this we have to
                // reset the scale after the popup has closed so that if the teaching tip is re-shown the render pass does not use the
                // small scale.
                //tailOcclusionGrid.Scale({ 1.0f,1.0f,1.0f });
            }
        }

        private TeachingTipPlacementMode GetFlowDirectionAdjustedPlacement(TeachingTipPlacementMode placementMode)
        {
            if (FlowDirection == FlowDirection.LeftToRight)
            {
                return placementMode;
            }
            else
            {
                switch (placementMode)
                {
                    case TeachingTipPlacementMode.Auto:
                        return TeachingTipPlacementMode.Auto;
                    case TeachingTipPlacementMode.Left:
                        return TeachingTipPlacementMode.Right;
                    case TeachingTipPlacementMode.Right:
                        return TeachingTipPlacementMode.Left;
                    case TeachingTipPlacementMode.Top:
                        return TeachingTipPlacementMode.Top;
                    case TeachingTipPlacementMode.Bottom:
                        return TeachingTipPlacementMode.Bottom;
                    case TeachingTipPlacementMode.LeftBottom:
                        return TeachingTipPlacementMode.RightBottom;
                    case TeachingTipPlacementMode.LeftTop:
                        return TeachingTipPlacementMode.RightTop;
                    case TeachingTipPlacementMode.TopLeft:
                        return TeachingTipPlacementMode.TopRight;
                    case TeachingTipPlacementMode.TopRight:
                        return TeachingTipPlacementMode.TopLeft;
                    case TeachingTipPlacementMode.RightTop:
                        return TeachingTipPlacementMode.LeftTop;
                    case TeachingTipPlacementMode.RightBottom:
                        return TeachingTipPlacementMode.LeftBottom;
                    case TeachingTipPlacementMode.BottomRight:
                        return TeachingTipPlacementMode.BottomLeft;
                    case TeachingTipPlacementMode.BottomLeft:
                        return TeachingTipPlacementMode.BottomRight;
                    case TeachingTipPlacementMode.Center:
                        return TeachingTipPlacementMode.Center;
                }
            }
            return TeachingTipPlacementMode.Auto;
        }

        private void OnTargetChanged()
        {
            if (m_target != null)
            {
                m_target.LayoutUpdated -= OnTargetLayoutUpdated;
                m_target.Loaded -= OnTargetLoaded;
            }
            //m_targetEffectiveViewportChangedRevoker.revoke();

            var target = Target;
            m_target = target;

            if (target != null)
            {
                target.Loaded += OnTargetLoaded;
            }

            if (IsOpen)
            {
                if (target != null)
                {
                    m_currentTargetBoundsInCoreWindowSpace = target.TransformToVisual(null).TransformBounds(
                        new Rect(
                            0.0,
                            0.0,
                            (float)target.ActualWidth,
                            (float)target.ActualHeight));

                    SetViewportChangedEvent(target);
                }
                PositionPopup();
            }
        }

        private void SetViewportChangedEvent(FrameworkElement target)
        {
            if (m_tipFollowsTarget)
            {
                // EffectiveViewPortChanged is only available on RS5 and higher.
                FrameworkElement targetAsFE7 = target;
                if (targetAsFE7 != null)
                {
                    target.LayoutUpdated += OnTargetLayoutUpdated;
                    //targetAsFE7.EffectiveViewportChanged += OnTargetLayoutUpdated;
                    //EffectiveViewportChanged += OnTargetLayoutUpdated;
                }
                else
                {
                    //m_targetLayoutUpdatedRevoker = target.LayoutUpdated(auto_revoke, { this, &OnTargetLayoutUpdated });
                }
            }
        }

        private void RevokeViewportChangedEvent()
        {
            if (m_target != null)
            {
                m_target.LayoutUpdated -= OnTargetLayoutUpdated;
            }
            //m_targetEffectiveViewportChangedRevoker.revoke();
            //m_effectiveViewportChangedRevoker.revoke();
        }

        private void WindowSizeChanged(object sender, SizeChangedEventArgs args)
        {
            // Reposition popup when target/window has finished determining sizes
            SharedHelpers.QueueCallbackForCompositionRendering(() => RepositionPopup());
        }

        //private void XamlRootChanged(XamlRoot xamlRoot, XamlRootChangedEventArgs args)
        //{
        //    // Reposition popup when target has finished determining its own position.
        //    SharedHelpers.QueueCallbackForCompositionRendering(() =>
        //    {
        //        if (xamlRoot.Size != m_currentXamlRootSize)
        //        {
        //            m_currentXamlRootSize = xamlRootSize;
        //            RepositionPopup();
        //        }
        //    });
        //}

        private void RepositionPopup()
        {
            if (IsOpen)
            {
                Rect GetNewTargetBounds()
                {
                    var target = m_target;
                    if (target != null)
                    {
                        return target.TransformToVisual(null).TransformBounds(
                            new Rect(
                                0.0,
                                0.0,
                                (float)target.ActualWidth,
                                (float)target.ActualHeight));
                    }
                    return Rect.Empty;
                }

                var newTargetBounds = GetNewTargetBounds();

                var newCurrentBounds = TransformToVisual(null).TransformBounds(
                    new Rect(
                        0.0,
                        0.0,
                        (float)ActualWidth,
                        (float)ActualHeight));

                if (newTargetBounds != m_currentTargetBoundsInCoreWindowSpace || newCurrentBounds != m_currentBoundsInCoreWindowSpace)
                {
                    m_currentBoundsInCoreWindowSpace = newCurrentBounds;
                    m_currentTargetBoundsInCoreWindowSpace = newTargetBounds;
                    PositionPopup();
                }
            }
        }

        private void OnTargetLoaded(object sender, object args)
        {
            RepositionPopup();
        }

        private void OnTargetLayoutUpdated(object sender, object args)
        {
            RepositionPopup();
        }

        private void CreateExpandAnimation()
        {
            EasingFunctionBase CreateExpandEasingFunction()
            {
                if (m_expandEasingFunction == null)
                {
                    m_expandEasingFunction = new CubicBezierEase()
                    {
                        ControlPoint1 = s_expandAnimationEasingCurveControlPoint1,
                        ControlPoint2 = s_expandAnimationEasingCurveControlPoint2
                    };
                }

                return m_expandEasingFunction;
            }

            var expandEasingFunction = CreateExpandEasingFunction();

            AnimationTimeline GetExpandAnimation()
            {
                var expandAnimation = new DoubleAnimationUsingKeyFrames()
                {
                    KeyFrames =
                    {
                        new DiscreteDoubleKeyFrame(0.0, TimeSpan.Zero),
                        new EasingDoubleKeyFrame(1.0, m_expandAnimationDuration, expandEasingFunction)
                    }
                };

                return expandAnimation;
            }

            m_expandAnimation = GetExpandAnimation();

            AnimationTimeline GetExpandElevationAnimation()
            {
                var expandElevationAnimation = new DoubleAnimation()
                {
                    EasingFunction = expandEasingFunction,
                    Duration = m_contractAnimationDuration
                };

                return expandElevationAnimation;
            }

            m_expandElevationAnimation = GetExpandElevationAnimation();
        }

        void CreateContractAnimation()
        {
            EasingFunctionBase GetContractEasingFunction()
            {
                if (m_contractEasingFunction == null)
                {
                    m_contractEasingFunction = new CubicBezierEase()
                    {
                        ControlPoint1 = s_contractAnimationEasingCurveControlPoint1,
                        ControlPoint2 = s_contractAnimationEasingCurveControlPoint2
                    };
                }

                return m_contractEasingFunction;
            }

            var contractEasingFunction = GetContractEasingFunction();

            AnimationTimeline GetContractAnimation()
            {
                var contractAnimation = new DoubleAnimationUsingKeyFrames()
                {
                    KeyFrames =
                    {
                        new DiscreteDoubleKeyFrame(1.0, TimeSpan.Zero),
                        new EasingDoubleKeyFrame(0, m_contractAnimationDuration, contractEasingFunction)
                    }
                };

                return contractAnimation;
            }

            m_contractAnimation = GetContractAnimation();

            AnimationTimeline GetContractElevationAnimation()
            {
                var contractElevationAnimation = new DoubleAnimation()
                {
                    EasingFunction = contractEasingFunction,
                    Duration = m_contractAnimationDuration
                };

                return contractElevationAnimation;
            }

            m_contractElevationAnimation = GetContractElevationAnimation();
        }

        private void StartExpandToOpen()
        {
            Debug.Assert(this is UIElement, "The contract and expand animations currently use facade's which were not available pre RS5.");
            if (m_expandAnimation == null)
            {
                CreateExpandAnimation();
            }

            Storyboard CreateStoryboard()
            {
                var storyboard = new Storyboard();
                var expandAnimation = m_expandAnimation;
                if (expandAnimation != null)
                {
                    var tailOcclusionGrid = m_tailOcclusionGrid;
                    if (tailOcclusionGrid != null)
                    {
                        Storyboard.SetTarget(expandAnimation, tailOcclusionGrid);
                    }
                    var tailEdgeBorder = m_tailEdgeBorder;
                    if (tailEdgeBorder != null)
                    {
                        Storyboard.SetTarget(expandAnimation, tailEdgeBorder);
                    }
                }

                var expandElevationAnimation = m_expandElevationAnimation;
                if (expandElevationAnimation != null)
                {
                    var contentRootGrid = m_contentRootGrid;
                    if (contentRootGrid != null)
                    {
                        Storyboard.SetTarget(expandElevationAnimation, contentRootGrid);
                    }
                }

                storyboard.Children.Add(expandAnimation);
                storyboard.Children.Add(expandElevationAnimation);

                return storyboard;
            }

            var storyboard = CreateStoryboard();
            //storyboard.Begin(this);
            m_isExpandAnimationPlaying = true;

            storyboard.Completed += (_, __) =>
            {
                m_isContractAnimationPlaying = false;

                if (!m_isContractAnimationPlaying)
                {
                    SetIsIdle(true);
                }
            };

            // Under normal circumstances we would have launched an animation just now, if we did not then we should make sure that the idle state is correct
            if (!m_isExpandAnimationPlaying && !m_isContractAnimationPlaying)
            {
                SetIsIdle(true);
            }
        }

        private void StartContractToClose()
        {
            Debug.Assert(this is UIElement, "The contract and expand animations currently use facade's which were not available pre RS5.");
            if (m_contractAnimation == null)
            {
                CreateContractAnimation();
            }

            Storyboard CreateStoryboard()
            {
                var storyboard = new Storyboard();
                var contractAnimation = m_contractAnimation;
                if (contractAnimation != null)
                {
                    var tailOcclusionGrid = m_tailOcclusionGrid;
                    if (tailOcclusionGrid != null)
                    {
                        Storyboard.SetTarget(contractAnimation, tailOcclusionGrid);
                    }
                    var tailEdgeBorder = m_tailEdgeBorder;
                    if (tailEdgeBorder != null)
                    {
                        Storyboard.SetTarget(contractAnimation, tailEdgeBorder);
                    }
                }

                var contractElevationAnimation = m_contractElevationAnimation;
                if (contractElevationAnimation != null)
                {
                    var contentRootGrid = m_contentRootGrid;
                    if (contentRootGrid != null)
                    {
                        Storyboard.SetTarget(contractElevationAnimation, contentRootGrid);
                    }
                }

                storyboard.Children.Add(contractAnimation);
                storyboard.Children.Add(contractElevationAnimation);

                return storyboard;
            }

            var storyboard = CreateStoryboard();
            //storyboard.Begin(this);
            m_isContractAnimationPlaying = true;

            storyboard.Completed += (_, __) =>
            {
                m_isContractAnimationPlaying = false;
                ClosePopup();

                if (!m_isExpandAnimationPlaying)
                {
                    SetIsIdle(true);
                }
            };
        }

        private Tuple<TeachingTipPlacementMode, bool> DetermineEffectivePlacement()
        {
            // Because we do not have access to APIs to give us details about multi monitor scenarios we do not have the ability to correctly
            // Place the tip in scenarios where we have an out of root bounds tip. Since this is the case we have decided to do no special
            // calculations and return the provided value or top if var was set. This behavior can be removed via the
            // SetReturnTopForOutOfWindowBounds test hook.
            if (!ShouldConstrainToRootBounds && m_returnTopForOutOfWindowPlacement)
            {
                var placement = GetFlowDirectionAdjustedPlacement(PreferredPlacement);
                if (placement == TeachingTipPlacementMode.Auto)
                {
                    return Tuple.Create(TeachingTipPlacementMode.Top, false);
                }
                return Tuple.Create(placement, false);
            }

            if (IsOpen && m_currentEffectiveTipPlacementMode != TeachingTipPlacementMode.Auto)
            {
                return Tuple.Create(m_currentEffectiveTipPlacementMode, false);
            }

            (double contentHeight, double contentWidth) GetContentSize()
            {
                var tailOcclusionGrid = m_tailOcclusionGrid;
                if (tailOcclusionGrid != null)
                {
                    double contentHeight = tailOcclusionGrid.ActualHeight;
                    double contentWidth = tailOcclusionGrid.ActualWidth;
                    return (contentHeight, contentWidth);
                }
                return (0.0, 0.0);
            }
            var (contentHeight, contentWidth) = GetContentSize();

            if (m_target != null)
            {
                return DetermineEffectivePlacementTargeted(contentHeight, contentWidth);
            }
            else
            {
                return DetermineEffectivePlacementUntargeted(contentHeight, contentWidth);
            }
        }

        private Tuple<TeachingTipPlacementMode, bool> DetermineEffectivePlacementTargeted(double contentHeight, double contentWidth)
        {
            // These variables will track which positions the tip will fit in. They all start true and are
            // flipped to false when we find a display condition that is not met.
            var availability = new Dictionary<TeachingTipPlacementMode, bool>();
            availability[TeachingTipPlacementMode.Auto] = false;
            availability[TeachingTipPlacementMode.Top] = true;
            availability[TeachingTipPlacementMode.Bottom] = true;
            availability[TeachingTipPlacementMode.Right] = true;
            availability[TeachingTipPlacementMode.Left] = true;
            availability[TeachingTipPlacementMode.TopLeft] = true;
            availability[TeachingTipPlacementMode.TopRight] = true;
            availability[TeachingTipPlacementMode.BottomLeft] = true;
            availability[TeachingTipPlacementMode.BottomRight] = true;
            availability[TeachingTipPlacementMode.LeftTop] = true;
            availability[TeachingTipPlacementMode.LeftBottom] = true;
            availability[TeachingTipPlacementMode.RightTop] = true;
            availability[TeachingTipPlacementMode.RightBottom] = true;
            availability[TeachingTipPlacementMode.Center] = true;

            double tipHeight = contentHeight + TailShortSideLength();
            double tipWidth = contentWidth + TailShortSideLength();

            // We try to avoid having the tail touch the HeroContent so rule out positions where this would be required
            if (HeroContent != null)
            {
                var heroContentBorder = m_heroContentBorder;
                if (heroContentBorder != null)
                {
                    var nonHeroContentRootGrid = m_nonHeroContentRootGrid;
                    if (nonHeroContentRootGrid != null)
                    {
                        if (heroContentBorder.ActualHeight > nonHeroContentRootGrid.ActualHeight - TailLongSideActualLength())
                        {
                            availability[TeachingTipPlacementMode.Left] = false;
                            availability[TeachingTipPlacementMode.Right] = false;
                        }
                    }
                }

                switch (HeroContentPlacement)
                {
                    case TeachingTipHeroContentPlacementMode.Bottom:
                        availability[TeachingTipPlacementMode.Top] = false;
                        availability[TeachingTipPlacementMode.TopRight] = false;
                        availability[TeachingTipPlacementMode.TopLeft] = false;
                        availability[TeachingTipPlacementMode.RightTop] = false;
                        availability[TeachingTipPlacementMode.LeftTop] = false;
                        availability[TeachingTipPlacementMode.Center] = false;
                        break;
                    case TeachingTipHeroContentPlacementMode.Top:
                        availability[TeachingTipPlacementMode.Bottom] = false;
                        availability[TeachingTipPlacementMode.BottomLeft] = false;
                        availability[TeachingTipPlacementMode.BottomRight] = false;
                        availability[TeachingTipPlacementMode.RightBottom] = false;
                        availability[TeachingTipPlacementMode.LeftBottom] = false;
                        break;
                }
            }

            // When ShouldConstrainToRootBounds is true clippedTargetBounds == availableBoundsAroundTarget
            // We have to separate them because there are checks which care about both.
            var (clippedTargetBounds, availableBoundsAroundTarget) = DetermineSpaceAroundTarget();

            // If the edge of the target isn't in the window.
            if (clippedTargetBounds.Left < 0)
            {
                availability[TeachingTipPlacementMode.LeftBottom] = false;
                availability[TeachingTipPlacementMode.Left] = false;
                availability[TeachingTipPlacementMode.LeftTop] = false;
            }
            // If the right edge of the target isn't in the window.
            if (clippedTargetBounds.Right < 0)
            {
                availability[TeachingTipPlacementMode.RightBottom] = false;
                availability[TeachingTipPlacementMode.Right] = false;
                availability[TeachingTipPlacementMode.RightTop] = false;
            }
            // If the top edge of the target isn't in the window.
            if (clippedTargetBounds.Top < 0)
            {
                availability[TeachingTipPlacementMode.TopLeft] = false;
                availability[TeachingTipPlacementMode.Top] = false;
                availability[TeachingTipPlacementMode.TopRight] = false;
            }
            // If the bottom edge of the target isn't in the window
            if (clippedTargetBounds.Bottom < 0)
            {
                availability[TeachingTipPlacementMode.BottomLeft] = false;
                availability[TeachingTipPlacementMode.Bottom] = false;
                availability[TeachingTipPlacementMode.BottomRight] = false;
            }

            // If the horizontal midpoint is out of the window.
            if (clippedTargetBounds.Left < -m_currentTargetBoundsInCoreWindowSpace.Width / 2 ||
                clippedTargetBounds.Right < -m_currentTargetBoundsInCoreWindowSpace.Width / 2)
            {
                availability[TeachingTipPlacementMode.TopLeft] = false;
                availability[TeachingTipPlacementMode.Top] = false;
                availability[TeachingTipPlacementMode.TopRight] = false;
                availability[TeachingTipPlacementMode.BottomLeft] = false;
                availability[TeachingTipPlacementMode.Bottom] = false;
                availability[TeachingTipPlacementMode.BottomRight] = false;
                availability[TeachingTipPlacementMode.Center] = false;
            }

            // If the vertical midpoint is out of the window.
            if (clippedTargetBounds.Top < -m_currentTargetBoundsInCoreWindowSpace.Height / 2 ||
                clippedTargetBounds.Bottom < -m_currentTargetBoundsInCoreWindowSpace.Height / 2)
            {
                availability[TeachingTipPlacementMode.LeftBottom] = false;
                availability[TeachingTipPlacementMode.Left] = false;
                availability[TeachingTipPlacementMode.LeftTop] = false;
                availability[TeachingTipPlacementMode.RightBottom] = false;
                availability[TeachingTipPlacementMode.Right] = false;
                availability[TeachingTipPlacementMode.RightTop] = false;
                availability[TeachingTipPlacementMode.Center] = false;
            }

            // If the tip is too tall to fit between the top of the target and the top edge of the window or screen.
            if (tipHeight > availableBoundsAroundTarget.Top)
            {
                availability[TeachingTipPlacementMode.Top] = false;
                availability[TeachingTipPlacementMode.TopRight] = false;
                availability[TeachingTipPlacementMode.TopLeft] = false;
            }
            // If the total tip is too tall to fit between the center of the target and the top of the window.
            if (tipHeight > availableBoundsAroundTarget.Top + (m_currentTargetBoundsInCoreWindowSpace.Height / 2.0f))
            {
                availability[TeachingTipPlacementMode.Center] = false;
            }
            // If the tip is too tall to fit between the center of the target and the top edge of the window.
            if (contentHeight - MinimumTipEdgeToTailCenter() > availableBoundsAroundTarget.Top + (m_currentTargetBoundsInCoreWindowSpace.Height / 2.0f))
            {
                availability[TeachingTipPlacementMode.RightTop] = false;
                availability[TeachingTipPlacementMode.LeftTop] = false;
            }
            // If the tip is too tall to fit in the window when the tail is centered vertically on the target and the tip.
            if (contentHeight / 2.0f > availableBoundsAroundTarget.Top + (m_currentTargetBoundsInCoreWindowSpace.Height / 2.0f) ||
                contentHeight / 2.0f > availableBoundsAroundTarget.Bottom + (m_currentTargetBoundsInCoreWindowSpace.Height / 2.0f))
            {
                availability[TeachingTipPlacementMode.Right] = false;
                availability[TeachingTipPlacementMode.Left] = false;
            }
            // If the tip is too tall to fit between the center of the target and the bottom edge of the window.
            if (contentHeight - MinimumTipEdgeToTailCenter() > availableBoundsAroundTarget.Bottom + (m_currentTargetBoundsInCoreWindowSpace.Height / 2.0f))
            {
                availability[TeachingTipPlacementMode.RightBottom] = false;
                availability[TeachingTipPlacementMode.LeftBottom] = false;
            }
            // If the tip is too tall to fit between the bottom of the target and the bottom edge of the window.
            if (tipHeight > availableBoundsAroundTarget.Bottom)
            {
                availability[TeachingTipPlacementMode.Bottom] = false;
                availability[TeachingTipPlacementMode.BottomLeft] = false;
                availability[TeachingTipPlacementMode.BottomRight] = false;
            }

            // If the tip is too wide to fit between the left edge of the target and the left edge of the window.
            if (tipWidth > availableBoundsAroundTarget.Left)
            {
                availability[TeachingTipPlacementMode.Left] = false;
                availability[TeachingTipPlacementMode.LeftTop] = false;
                availability[TeachingTipPlacementMode.LeftBottom] = false;
            }
            // If the tip is too wide to fit between the center of the target and the left edge of the window.
            if (contentWidth - MinimumTipEdgeToTailCenter() > availableBoundsAroundTarget.Left + (m_currentTargetBoundsInCoreWindowSpace.Width / 2.0f))
            {
                availability[TeachingTipPlacementMode.TopLeft] = false;
                availability[TeachingTipPlacementMode.BottomLeft] = false;
            }
            // If the tip is too wide to fit in the window when the tail is centered horizontally on the target and the tip.
            if (contentWidth / 2.0f > availableBoundsAroundTarget.Left + (m_currentTargetBoundsInCoreWindowSpace.Width / 2.0f) ||
                contentWidth / 2.0f > availableBoundsAroundTarget.Right + (m_currentTargetBoundsInCoreWindowSpace.Width / 2.0f))
            {
                availability[TeachingTipPlacementMode.Top] = false;
                availability[TeachingTipPlacementMode.Bottom] = false;
                availability[TeachingTipPlacementMode.Center] = false;
            }
            // If the tip is too wide to fit between the center of the target and the right edge of the window.
            if (contentWidth - MinimumTipEdgeToTailCenter() > availableBoundsAroundTarget.Right + (m_currentTargetBoundsInCoreWindowSpace.Width / 2.0f))
            {
                availability[TeachingTipPlacementMode.TopRight] = false;
                availability[TeachingTipPlacementMode.BottomRight] = false;
            }
            // If the tip is too wide to fit between the right edge of the target and the right edge of the window.
            if (tipWidth > availableBoundsAroundTarget.Right)
            {
                availability[TeachingTipPlacementMode.Right] = false;
                availability[TeachingTipPlacementMode.RightTop] = false;
                availability[TeachingTipPlacementMode.RightBottom] = false;
            }

            var wantedDirection = GetFlowDirectionAdjustedPlacement(PreferredPlacement);
            var priorities = GetPlacementFallbackOrder(wantedDirection);

            foreach (var mode in priorities)
            {
                if (availability[mode])
                {
                    return Tuple.Create(mode, false);
                }
            }
            // The teaching tip wont fit anywhere, set tipDoesNotFit to indicate that we should not open.
            return Tuple.Create(TeachingTipPlacementMode.Top, true);
        }

        private Tuple<TeachingTipPlacementMode, bool> DetermineEffectivePlacementUntargeted(double contentHeight, double contentWidth)
        {
            var windowBounds = GetWindowBounds();
            if (!ShouldConstrainToRootBounds)
            {
                var screenBoundsInCoreWindowSpace = GetEffectiveScreenBoundsInCoreWindowSpace(windowBounds);
                if (screenBoundsInCoreWindowSpace.Height > contentHeight && screenBoundsInCoreWindowSpace.Width > contentWidth)
                {
                    return Tuple.Create(TeachingTipPlacementMode.Bottom, false);
                }
            }
            else
            {
                var windowBoundsInCoreWindowSpace = GetEffectiveWindowBoundsInCoreWindowSpace(windowBounds);
                if (windowBoundsInCoreWindowSpace.Height > contentHeight && windowBoundsInCoreWindowSpace.Width > contentWidth)
                {
                    return Tuple.Create(TeachingTipPlacementMode.Bottom, false);
                }
            }

            // The teaching tip doesn't fit in the window/screen set tipDoesNotFit to indicate that we should not open.
            return Tuple.Create(TeachingTipPlacementMode.Top, true);
        }

        private Tuple<Thickness, Thickness> DetermineSpaceAroundTarget()
        {
            var shouldConstrainToRootBounds = ShouldConstrainToRootBounds;

            (Rect windowBoundsInCoreWindowSpace, Rect screenBoundsInCoreWindowSpace) GetBoundsInCoreWindowSpace()
            {
                var windowBounds = GetWindowBounds();
                return
                    (GetEffectiveWindowBoundsInCoreWindowSpace(windowBounds),
                    GetEffectiveScreenBoundsInCoreWindowSpace(windowBounds));
            }

            var (windowBoundsInCoreWindowSpace, screenBoundsInCoreWindowSpace) = GetBoundsInCoreWindowSpace();


            Thickness windowSpaceAroundTarget = new Thickness(
                // Target.Left - Window.Left
                m_currentTargetBoundsInCoreWindowSpace.X - /* 0 except with test window bounds */ windowBoundsInCoreWindowSpace.X,
                // Target.Top - Window.Top
                m_currentTargetBoundsInCoreWindowSpace.Y - /* 0 except with test window bounds */ windowBoundsInCoreWindowSpace.Y,
                // Window.Right - Target.Right
                windowBoundsInCoreWindowSpace.X + windowBoundsInCoreWindowSpace.Width - (m_currentTargetBoundsInCoreWindowSpace.X + m_currentTargetBoundsInCoreWindowSpace.Width),
                // Screen.Right - Target.Right
                windowBoundsInCoreWindowSpace.Y + windowBoundsInCoreWindowSpace.Height - (m_currentTargetBoundsInCoreWindowSpace.Y + m_currentTargetBoundsInCoreWindowSpace.Height));


            Thickness GetScreenSpaceAroundTarget()
            {
                if (!ShouldConstrainToRootBounds)
                {
                    return new Thickness(
                        // Target.Left - Screen.Left
                        m_currentTargetBoundsInCoreWindowSpace.X - screenBoundsInCoreWindowSpace.X,
                        // Target.Top - Screen.Top
                        m_currentTargetBoundsInCoreWindowSpace.Y - screenBoundsInCoreWindowSpace.Y,
                        // Screen.Right - Target.Right
                        screenBoundsInCoreWindowSpace.X + screenBoundsInCoreWindowSpace.Width - (m_currentTargetBoundsInCoreWindowSpace.X + m_currentTargetBoundsInCoreWindowSpace.Width),
                        // Screen.Bottom - Target.Bottom
                        screenBoundsInCoreWindowSpace.Y + screenBoundsInCoreWindowSpace.Height - (m_currentTargetBoundsInCoreWindowSpace.Y + m_currentTargetBoundsInCoreWindowSpace.Height));
                }
                return windowSpaceAroundTarget;
            }
            Thickness screenSpaceAroundTarget = GetScreenSpaceAroundTarget();

            return Tuple.Create(windowSpaceAroundTarget, screenSpaceAroundTarget);
        }

        private Rect GetEffectiveWindowBoundsInCoreWindowSpace(Rect windowBounds)
        {
            if (m_useTestWindowBounds)
            {
                return m_testWindowBoundsInCoreWindowSpace;
            }
            else
            {
                return new Rect(0, 0, windowBounds.Width, windowBounds.Height);
            }

        }

        private Rect GetEffectiveScreenBoundsInCoreWindowSpace(Rect windowBounds)
        {
            if (!m_useTestScreenBounds && !ShouldConstrainToRootBounds)
            {
                Debug.Assert(!m_returnTopForOutOfWindowPlacement, "When returnTopForOutOfWindowPlacement is true we will never need to get the screen bounds");

                double dpiScaleX, dpiScaleY;
#if NET462_OR_NEWER
                DpiScale dpi = VisualTreeHelper.GetDpi(this);
                dpiScaleX = dpi.DpiScaleX;
                dpiScaleY = dpi.DpiScaleY;
#else
                Matrix transformToDevice = PresentationSource.FromVisual(this).CompositionTarget.TransformToDevice;
                dpiScaleX = transformToDevice.M11;
                dpiScaleY = transformToDevice.M22;
#endif

                return new Rect(-windowBounds.X,
                    -windowBounds.Y,
                    SystemParameters.VirtualScreenHeight / dpiScaleY,
                    SystemParameters.VirtualScreenWidth / dpiScaleX);
            }
            return m_testScreenBoundsInCoreWindowSpace;
        }

        private Rect GetWindowBounds()
        {
            //UIElement uiElement10 = this;
            //if (uiElement10 != null)
            //{
            //    var xamlRoot = uiElement10.XamlRoot;
            //    if (xamlRoot != null)
            //    {
            //        return new Rect(0, 0, xamlRoot.Size.Width, xamlRoot.Size.Height);
            //    }
            //}
            var win = Window.GetWindow(this);
            return win != null ? new Rect(win.Left, win.Top, win.ActualWidth, win.ActualHeight) : Rect.Empty;
        }

        private TeachingTipPlacementMode[] GetPlacementFallbackOrder(TeachingTipPlacementMode preferredPlacement)
        {
            var priorityList = new TeachingTipPlacementMode[13];
            priorityList[0] = TeachingTipPlacementMode.Top;
            priorityList[1] = TeachingTipPlacementMode.Bottom;
            priorityList[2] = TeachingTipPlacementMode.Left;
            priorityList[3] = TeachingTipPlacementMode.Right;
            priorityList[4] = TeachingTipPlacementMode.TopLeft;
            priorityList[5] = TeachingTipPlacementMode.TopRight;
            priorityList[6] = TeachingTipPlacementMode.BottomLeft;
            priorityList[7] = TeachingTipPlacementMode.BottomRight;
            priorityList[8] = TeachingTipPlacementMode.LeftTop;
            priorityList[9] = TeachingTipPlacementMode.LeftBottom;
            priorityList[10] = TeachingTipPlacementMode.RightTop;
            priorityList[11] = TeachingTipPlacementMode.RightBottom;
            priorityList[12] = TeachingTipPlacementMode.Center;


            if (IsPlacementBottom(preferredPlacement))
            {
                // Swap to bottom > top
                (priorityList[1], priorityList[0]) = (priorityList[0], priorityList[1]);
                (priorityList[6], priorityList[4]) = (priorityList[4], priorityList[6]);
                (priorityList[7], priorityList[5]) = (priorityList[5], priorityList[7]);
            }
            else if (IsPlacementLeft(preferredPlacement))
            {
                // swap to lateral > vertical
                (priorityList[2], priorityList[0]) = (priorityList[0], priorityList[2]);
                (priorityList[3], priorityList[1]) = (priorityList[1], priorityList[3]);
                (priorityList[8], priorityList[4]) = (priorityList[4], priorityList[8]);
                (priorityList[9], priorityList[5]) = (priorityList[5], priorityList[9]);
                (priorityList[10], priorityList[6]) = (priorityList[6], priorityList[10]);
                (priorityList[11], priorityList[7]) = (priorityList[7], priorityList[11]);
            }
            else if (IsPlacementRight(preferredPlacement))
            {
                // swap to lateral > vertical
                (priorityList[2], priorityList[0]) = (priorityList[0], priorityList[2]);
                (priorityList[3], priorityList[1]) = (priorityList[1], priorityList[3]);
                (priorityList[8], priorityList[4]) = (priorityList[4], priorityList[8]);
                (priorityList[9], priorityList[5]) = (priorityList[5], priorityList[9]);
                (priorityList[10], priorityList[6]) = (priorityList[6], priorityList[10]);
                (priorityList[11], priorityList[7]) = (priorityList[7], priorityList[11]);

                // swap to right > left
                (priorityList[1], priorityList[0]) = (priorityList[0], priorityList[1]);
                (priorityList[6], priorityList[4]) = (priorityList[4], priorityList[6]);
                (priorityList[7], priorityList[5]) = (priorityList[5], priorityList[7]);
            }

            //Switch the preferred placement to first.
            var pivotPosition = Array.IndexOf(priorityList, preferredPlacement);

            if (pivotPosition > 0)
            {
                priorityList = priorityList
                    .Skip(pivotPosition)
                    .Concat(priorityList.Take(pivotPosition))
                    .ToArray();
            }

            return priorityList;
        }


        private void EstablishShadows()
        {
            //if (IUIElement10 tailPolygon_uiElement10 = m_tailPolygon)
            //{
            //    if (m_tipShadow)
            //    {
            //        if (!tailPolygon_uiElement10.Shadow())
            //        {
            //            // This facilitates an experiment around faking a proper tail shadow, shadows are expensive though so we don't want it present for release builds.
            //            var const tailShadow = Windows.UI.Xaml.Media.ThemeShadow{ };
            //            tailShadow.Receivers().Append(m_target);
            //            tailPolygon_uiElement10.Shadow(tailShadow);
            //            if (var && tailPolygon = m_tailPolygon)
            //            {
            //                var const tailPolygonTranslation = tailPolygon.Translation()
            //                        tailPolygon.Translation({ tailPolygonTranslation.x, tailPolygonTranslation.y, m_tailElevation });
            //            }
            //        }
            //    }
            //    else
            //    {
            //        tailPolygon_uiElement10.Shadow(null);
            //    }
            //}

            var m_contentRootGrid_uiElement10 = m_contentRootGrid;
            if (m_contentRootGrid_uiElement10 != null)
            {
                if (m_tipShouldHaveShadow)
                {
                    //if (m_contentRootGrid_uiElement10.Shadow == null)
                    //{
                    //    m_contentRootGrid_uiElement10.Shadow(ThemeShadow{ });
                    //    if (var && contentRootGrid = m_contentRootGrid)
                    //    {
                    //        const var contentRootGridTranslation = contentRootGrid.Translation();
                    //        contentRootGrid.Translation({ contentRootGridTranslation.x, contentRootGridTranslation.y, m_contentElevation });
                    //    }
                    //}
                }
                else
                {
                    //m_contentRootGrid_uiElement10.Shadow = null;
                }
            }
        }

        private void TrySetCenterPoint(UIElement element, Point centerPoint)
        {
            if (element != null)
            {
                element.RenderTransformOrigin = centerPoint;
            }
        }

        private float TailLongSideActualLength()
        {
            var tailPolygon = m_tailPolygon;
            if (tailPolygon != null)
            {
                return (float)Math.Max(tailPolygon.ActualHeight, tailPolygon.ActualWidth);
            }
            return 0;
        }

        private float TailLongSideLength()
        {
            return (float)(TailLongSideActualLength() - (2 * s_tailOcclusionAmount));
        }

        private float TailShortSideLength()
        {
            var tailPolygon = m_tailPolygon;
            if (tailPolygon != null)
            {
                return (float)(Math.Min(tailPolygon.ActualHeight, tailPolygon.ActualWidth) - s_tailOcclusionAmount);
            }
            return 0;
        }

        private float MinimumTipEdgeToTailEdgeMargin()
        {
            var tailOcclusionGrid = m_tailOcclusionGrid;
            if (tailOcclusionGrid != null)
            {
                return tailOcclusionGrid.ColumnDefinitions.Count > 1 ?
                    (float)(tailOcclusionGrid.ColumnDefinitions[1].ActualWidth + s_tailOcclusionAmount)
                    : 0.0f;
            }
            return 0;
        }

        private float MinimumTipEdgeToTailCenter()
        {
            var tailOcclusionGrid = m_tailOcclusionGrid;
            if (tailOcclusionGrid != null)
            {
                var tailPolygon = m_tailPolygon;
                if (tailPolygon != null)
                {
                    return tailOcclusionGrid.ColumnDefinitions.Count > 1 ?
                        (float)(tailOcclusionGrid.ColumnDefinitions[0].ActualWidth +
                            tailOcclusionGrid.ColumnDefinitions[1].ActualWidth +
                            (Math.Max(tailPolygon.ActualHeight, tailPolygon.ActualWidth) / 2))
                        : 0.0f;
                }
            }
            return 0;
        }

        public CornerRadius GetTeachingTipCornerRadius() => CornerRadius;

        public string GetIconSeverityLevelResourceName(InfoBarSeverity severity)
        {
            switch (severity)
            {
                case InfoBarSeverity.Success:
                    return SR_InfoBarIconSeveritySuccessName;
                case InfoBarSeverity.Warning:
                    return SR_InfoBarIconSeverityWarningName;
                case InfoBarSeverity.Error:
                    return SR_InfoBarIconSeverityErrorName;
            };
            return SR_InfoBarIconSeverityInformationalName;
        }

        DependencyObject IControlProtected.GetTemplateChild(string childName)
        {
            return GetTemplateChild(childName);
        }

        #region Test Hooks

        internal void SetExpandEasingFunction(EasingFunctionBase easingFunction)
        {
            m_expandEasingFunction = easingFunction;
            CreateExpandAnimation();
        }

        internal void SetContractEasingFunction(EasingFunctionBase easingFunction)
        {
            m_contractEasingFunction = easingFunction;
            CreateContractAnimation();
        }

        internal void SetTipShouldHaveShadow(bool tipShouldHaveShadow)
        {
            if (m_tipShouldHaveShadow != tipShouldHaveShadow)
            {
                m_tipShouldHaveShadow = tipShouldHaveShadow;
                EstablishShadows();
            }
        }

        //void SetContentElevation(float elevation)
        //{
        //    m_contentElevation = elevation;
        //    if (SharedHelpers.IsRS5OrHigher())
        //    {
        //        if (var && contentRootGrid = m_contentRootGrid)
        //        {
        //            var const contentRootGridTranslation = contentRootGrid.Translation();
        //            m_contentRootGrid.Translation({ contentRootGridTranslation.x, contentRootGridTranslation.y, m_contentElevation });
        //        }
        //        if (m_expandElevationAnimation)
        //        {
        //            m_expandElevationAnimation.SetScalarParameter("contentElevation", m_contentElevation);
        //        }
        //    }
        //}

        //void SetTailElevation(float elevation)
        //{
        //    m_tailElevation = elevation;
        //    if (SharedHelpers.IsRS5OrHigher() && m_tailPolygon)
        //    {
        //        if (var && tailPolygon = m_tailPolygon)
        //        {
        //            var const tailPolygonTranslation = tailPolygon.Translation();
        //            tailPolygon.Translation({ tailPolygonTranslation.x, tailPolygonTranslation.y, m_tailElevation });
        //        }
        //    }
        //}

        internal void SetUseTestWindowBounds(bool useTestWindowBounds)
        {
            m_useTestWindowBounds = useTestWindowBounds;
        }

        internal void SetTestWindowBounds(Rect testWindowBounds)
        {
            m_testWindowBoundsInCoreWindowSpace = testWindowBounds;
        }

        internal void SetUseTestScreenBounds(bool useTestScreenBounds)
        {
            m_useTestScreenBounds = useTestScreenBounds;
        }

        internal void SetTestScreenBounds(Rect testScreenBounds)
        {
            m_testScreenBoundsInCoreWindowSpace = testScreenBounds;
        }

        internal void SetTipFollowsTarget(bool tipFollowsTarget)
        {
            if (m_tipFollowsTarget != tipFollowsTarget)
            {
                m_tipFollowsTarget = tipFollowsTarget;
                if (tipFollowsTarget)
                {
                    var target = m_target;
                    if (target != null)
                    {
                        SetViewportChangedEvent(target);
                    }
                }
                else
                {
                    RevokeViewportChangedEvent();
                }
            }
        }

        internal void SetReturnTopForOutOfWindowPlacement(bool returnTopForOutOfWindowPlacement)
        {
            m_returnTopForOutOfWindowPlacement = returnTopForOutOfWindowPlacement;
        }

        internal void SetExpandAnimationDuration(TimeSpan expandAnimationDuration)
        {
            m_expandAnimationDuration = expandAnimationDuration;
            var expandAnimation = m_expandAnimation;
            if (expandAnimation != null)
            {
                expandAnimation.Duration = m_expandAnimationDuration;
            }
            var expandElevationAnimation = m_expandElevationAnimation;
            if (expandElevationAnimation != null)
            {
                expandElevationAnimation.Duration = m_expandAnimationDuration;
            }
        }

        internal void SetContractAnimationDuration(TimeSpan contractAnimationDuration)
        {
            m_contractAnimationDuration = contractAnimationDuration;
            var contractAnimation = m_contractAnimation;
            if (contractAnimation != null)
            {
                contractAnimation.Duration = m_contractAnimationDuration;
            }
            var contractElevationAnimation = m_contractElevationAnimation;
            if (contractElevationAnimation != null)
            {
                contractElevationAnimation.Duration = m_contractAnimationDuration;
            }
        }

        internal bool GetIsIdle()
        {
            return m_isIdle;
        }

        internal void SetIsIdle(bool isIdle)
        {
            if (m_isIdle != isIdle)
            {
                m_isIdle = isIdle;
                TeachingTipTestHooks.NotifyIdleStatusChanged(this);
            }
        }

        internal TeachingTipPlacementMode GetEffectivePlacement()
        {
            return m_currentEffectiveTipPlacementMode;
        }

        internal TeachingTipHeroContentPlacementMode GetEffectiveHeroContentPlacement()
        {
            return m_currentHeroContentEffectivePlacementMode;
        }

        internal double GetHorizontalOffset()
        {
            var popup = m_popup;
            if (popup != null)
            {
                return popup.HorizontalOffset;
            }
            return 0.0;
        }

        internal double GetVerticalOffset()
        {
            var popup = m_popup;
            if (popup != null)
            {
                return popup.VerticalOffset;
            }
            return 0.0;
        }

        internal Visibility GetTitleVisibility()
        {
            var titleTextBox = GetTemplateChildT<UIElement>(s_titleTextBoxName, this);
            if (titleTextBox != null)
            {
                return titleTextBox.Visibility;
            }
            return Visibility.Collapsed;
        }

        internal Visibility GetSubtitleVisibility()
        {
            var subtitleTextBox = GetTemplateChildT<UIElement>(s_subtitleTextBoxName, this);
            if (subtitleTextBox != null)
            {
                return subtitleTextBox.Visibility;
            }
            return Visibility.Collapsed;
        }

        #endregion

        private bool m_actualThemeChangedSubscribed;
        private void UpdatePopupRequestedTheme()
        {
            // The way that TeachingTip reparents its content tree breaks ElementTheme calculations. Hook up a listener to
            // ActualTheme on the TeachingTip and then set the Popup's RequestedTheme to match when it changes.

            FrameworkElement frameworkElement6 = this;
            if (frameworkElement6 != null)
            {
                // TODO: Unsubscribe ActualThemeChanged while unloading
                if (!m_actualThemeChangedSubscribed)
                {
                    ThemeManager.AddActualThemeChangedHandler(frameworkElement6, (s, e) => UpdatePopupRequestedTheme());
                    m_actualThemeChangedSubscribed = true;
                }

                var popup = m_popup;
                if (popup != null)
                {
                    ThemeManager.SetRequestedTheme(popup, ThemeManager.GetActualTheme(frameworkElement6));
                }
            }
        }

        private double TopLeftCornerRadius()
        {
            return GetTeachingTipCornerRadius().TopLeft;
        }
        private double TopRightCornerRadius()
        {
            return GetTeachingTipCornerRadius().TopRight;
        }

        private Border m_container;

        internal Popup m_popup;
        private Popup m_lightDismissIndicatorPopup;
        private ContentControl m_popupContentControl;

        private UIElement m_rootElement;
        private Grid m_tailOcclusionGrid;
        private Grid m_contentRootGrid;
        private Grid m_nonHeroContentRootGrid;
        private Border m_heroContentBorder;
        private Button m_actionButton;
        private Button m_alternateCloseButton;
        private Button m_closeButton;
        private Polygon m_tailPolygon;
        private Grid m_tailEdgeBorder;
        private UIElement m_titleTextBlock;
        private UIElement m_subtitleTextBlock;

        private WeakReference<IInputElement> m_previouslyFocusedElement;

        private AnimationTimeline m_expandAnimation;
        private AnimationTimeline m_contractAnimation;
        private AnimationTimeline m_expandElevationAnimation;
        private AnimationTimeline m_contractElevationAnimation;
        private EasingFunctionBase m_expandEasingFunction;
        private EasingFunctionBase m_contractEasingFunction;

        private TeachingTipPlacementMode m_currentEffectiveTipPlacementMode = TeachingTipPlacementMode.Auto;
        private TeachingTipPlacementMode m_currentEffectiveTailPlacementMode = TeachingTipPlacementMode.Auto;
        private TeachingTipHeroContentPlacementMode m_currentHeroContentEffectivePlacementMode = TeachingTipHeroContentPlacementMode.Auto;

        private Rect m_currentBoundsInCoreWindowSpace = new Rect(0, 0, 0, 0);
        private Rect m_currentTargetBoundsInCoreWindowSpace = new Rect(0, 0, 0, 0);

        private Size m_currentXamlRootSize = new Size(0, 0);

        private bool m_ignoreNextIsOpenChanged = false;
        private bool m_isTemplateApplied = false;
        private bool m_createNewPopupOnOpen = false;

        private bool m_isExpandAnimationPlaying = false;
        private bool m_isContractAnimationPlaying = false;

        private bool m_hasF6BeenInvoked = false;

        private bool m_useTestWindowBounds = false;
        private Rect m_testWindowBoundsInCoreWindowSpace = new Rect(0, 0, 0, 0);
        private bool m_useTestScreenBounds = false;
        private Rect m_testScreenBoundsInCoreWindowSpace = new Rect(0, 0, 0, 0);

        private bool m_tipShouldHaveShadow = true;

        private bool m_tipFollowsTarget = false;
        private bool m_returnTopForOutOfWindowPlacement = true;

        private float m_contentElevation = 32.0f;
        private float m_tailElevation = 0.0f;
        private bool m_tailShadowTargetsShadowTarget = false;

        private TimeSpan m_expandAnimationDuration = TimeSpan.FromMilliseconds(300);
        private TimeSpan m_contractAnimationDuration = TimeSpan.FromMilliseconds(200);

        private TeachingTipCloseReason m_lastCloseReason = TeachingTipCloseReason.Programmatic;

        private static bool IsPlacementTop(TeachingTipPlacementMode placement)
        {
            return placement == TeachingTipPlacementMode.Top || placement == TeachingTipPlacementMode.TopLeft || placement == TeachingTipPlacementMode.TopRight;
        }
        private static bool IsPlacementBottom(TeachingTipPlacementMode placement)
        {
            return placement == TeachingTipPlacementMode.Bottom || placement == TeachingTipPlacementMode.BottomLeft || placement == TeachingTipPlacementMode.BottomRight;
        }
        private static bool IsPlacementLeft(TeachingTipPlacementMode placement)
        {
            return placement == TeachingTipPlacementMode.Left || placement == TeachingTipPlacementMode.LeftTop || placement == TeachingTipPlacementMode.LeftBottom;
        }
        private static bool IsPlacementRight(TeachingTipPlacementMode placement)
        {
            return placement == TeachingTipPlacementMode.Right || placement == TeachingTipPlacementMode.RightTop || placement == TeachingTipPlacementMode.RightBottom;
        }

        // These values are shifted by one because this is the 1px highlight that sits adjacent to the tip border.
        private Thickness BottomPlacementTopRightHighlightMargin(double width, double height)
        {
            return new Thickness((width / 2) + (TailShortSideLength() - 1.0), 0, TopRightCornerRadius() - 1.0, 0);
        }
        private Thickness BottomRightPlacementTopRightHighlightMargin(double width, double height)
        {
            return new Thickness(MinimumTipEdgeToTailEdgeMargin() + TailLongSideLength() - 1.0, 0, TopRightCornerRadius() - 1.0, 0);
        }
        private Thickness BottomLeftPlacementTopRightHighlightMargin(double width, double height)
        {
            return new Thickness(width - (MinimumTipEdgeToTailEdgeMargin() + 1.0), 0, TopRightCornerRadius() - 1.0, 0);
        }
        private static Thickness OtherPlacementTopRightHighlightMargin(double width, double height)
        {
            return new Thickness(0, 0, 0, 0);
        }

        private Thickness BottomPlacementTopLeftHighlightMargin(double width, double height)
        {
            return new Thickness(TopLeftCornerRadius() - 1.0, 0, (width / 2) + (TailShortSideLength() - 1.0), 0);
        }
        private Thickness BottomRightPlacementTopLeftHighlightMargin(double width, double height)
        {
            return new Thickness(TopLeftCornerRadius() - 1.0, 0, width - (MinimumTipEdgeToTailEdgeMargin() + 1.0), 0);
        }
        private Thickness BottomLeftPlacementTopLeftHighlightMargin(double width, double height)
        {
            return new Thickness(TopLeftCornerRadius() - 1.0, 0, MinimumTipEdgeToTailEdgeMargin() + TailLongSideLength() - 1.0, 0);
        }
        private Thickness TopEdgePlacementTopLeftHighlightMargin(double width, double height)
        {
            return new Thickness(TopLeftCornerRadius() - 1.0, 1, TopRightCornerRadius() - 1.0, 0);
        }
        // Shifted by one since the tail edge's border is not accounted for automatically.
        private Thickness LeftEdgePlacementTopLeftHighlightMargin(double width, double height)
        {
            return new Thickness(TopLeftCornerRadius() - 1.0, 1, TopRightCornerRadius() - 2.0, 0);
        }
        private Thickness RightEdgePlacementTopLeftHighlightMargin(double width, double height)
        {
            return new Thickness(TopLeftCornerRadius() - 2.0, 1, TopRightCornerRadius() - 1.0, 0);
        }

        private static double UntargetedTipFarPlacementOffset(double farWindowCoordinateInCoreWindowSpace, double tipSize, double offset)
        {
            return farWindowCoordinateInCoreWindowSpace - (tipSize + s_untargetedTipWindowEdgeMargin + offset);
        }
        private static double UntargetedTipCenterPlacementOffset(double nearWindowCoordinateInCoreWindowSpace, double farWindowCoordinateInCoreWindowSpace, double tipSize, double nearOffset, double farOffset)
        {
            return ((nearWindowCoordinateInCoreWindowSpace + farWindowCoordinateInCoreWindowSpace) / 2) - (tipSize / 2) + nearOffset - farOffset;
        }
        private static double UntargetedTipNearPlacementOffset(double nearWindowCoordinateInCoreWindowSpace, double offset)
        {
            return s_untargetedTipWindowEdgeMargin + nearWindowCoordinateInCoreWindowSpace + offset;
        }

        private const string s_scaleTargetName = "Scale";
        private const string s_translationTargetName = "Translation";

        private const string s_containerName = "Container";
        private const string s_popupName = "Popup";
        private const string s_tailOcclusionGridName = "TailOcclusionGrid";
        private const string s_contentRootGridName = "ContentRootGrid";
        private const string s_nonHeroContentRootGridName = "NonHeroContentRootGrid";
        private const string s_shadowTargetName = "ShadowTarget";
        private const string s_heroContentBorderName = "HeroContentBorder";
        private const string s_titlesStackPanelName = "TitlesStackPanel";
        private const string s_titleTextBoxName = "TitleTextBlock";
        private const string s_subtitleTextBoxName = "SubtitleTextBlock";
        private const string s_alternateCloseButtonName = "AlternateCloseButton";
        private const string s_mainContentPresenterName = "MainContentPresenter";
        private const string s_actionButtonName = "ActionButton";
        private const string s_closeButtonName = "CloseButton";
        private const string s_tailPolygonName = "TailPolygon";
        private const string s_tailEdgeBorderName = "TailEdgeBorder";
        private const string s_topTailPolygonHighlightName = "TopTailPolygonHighlight";
        private const string s_topHighlightLeftName = "TopHighlightLeft";
        private const string s_topHighlightRightName = "TopHighlightRight";

        private const string s_accentButtonStyleName = "AccentButtonStyle";
        private const string s_teachingTipTopHighlightBrushName = "TeachingTipTopHighlightBrush";

        private static readonly Point s_expandAnimationEasingCurveControlPoint1 = new Point(0.1, 0.9);
        private static readonly Point s_expandAnimationEasingCurveControlPoint2 = new Point(0.2, 1.0);
        private static readonly Point s_contractAnimationEasingCurveControlPoint1 = new Point(0.7, 0.0);
        private static readonly Point s_contractAnimationEasingCurveControlPoint2 = new Point(1.0, 0.5);

        //It is possible this should be exposed as a property, but you can adjust what it does with margin.
        private const float s_untargetedTipWindowEdgeMargin = 24F;
        private const float s_defaultTipHeightAndWidth = 320F;

        //Ideally this would be computed from layout but it is difficult to do.
        private const float s_tailOcclusionAmount = 2F;
    }
}
