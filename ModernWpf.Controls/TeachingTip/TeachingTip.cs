// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Windows;
using System.Windows.Controls;

namespace ModernWpf.Controls
{
    public partial class TeachingTip : ContentControl
    {
        //        TeachingTip()
        //{
        //    __RP_Marker_ClassById(RuntimeProfiler.ProfId_TeachingTip);
        //        SetDefaultStyleKey(this);
        //        EnsureProperties();
        //        Unloaded({ this, &ClosePopupOnUnloadEvent });
        //    m_automationNameChangedRevoker = RegisterPropertyChanged(this, AutomationProperties.NameProperty(), { this, &OnAutomationNameChanged });
        //    m_automationIdChangedRevoker = RegisterPropertyChanged(this, AutomationProperties.AutomationIdProperty(), { this, &OnAutomationIdChanged });
        //    SetValue(s_TemplateSettingsProperty, make<.TeachingTipTemplateSettings>());
        //    }

        //    AutomationPeer OnCreateAutomationPeer()
        //    {
        //        return make<TeachingTipAutomationPeer>(this);
        //    }

        //    void OnApplyTemplate()
        //    {
        //        m_acceleratorKeyActivatedRevoker.revoke();
        //        m_effectiveViewportChangedRevoker.revoke();
        //        m_contentSizeChangedRevoker.revoke();
        //        m_closeButtonClickedRevoker.revoke();
        //        m_alternateCloseButtonClickedRevoker.revoke();
        //        m_actionButtonClickedRevoker.revoke();
        //        m_windowSizeChangedRevoker.revoke();

        //        IControlProtected controlProtected{ this };

        //        m_container.set(GetTemplateChildT<Border>(s_containerName, controlProtected));
        //        m_rootElement.set(m_container.get().Child());
        //        m_tailOcclusionGrid.set(GetTemplateChildT<Grid>(s_tailOcclusionGridName, controlProtected));
        //        m_contentRootGrid.set(GetTemplateChildT<Grid>(s_contentRootGridName, controlProtected));
        //        m_nonHeroContentRootGrid.set(GetTemplateChildT<Grid>(s_nonHeroContentRootGridName, controlProtected));
        //        m_heroContentBorder.set(GetTemplateChildT<Border>(s_heroContentBorderName, controlProtected));
        //        m_actionButton.set(GetTemplateChildT<Button>(s_actionButtonName, controlProtected));
        //        m_alternateCloseButton.set(GetTemplateChildT<Button>(s_alternateCloseButtonName, controlProtected));
        //        m_closeButton.set(GetTemplateChildT<Button>(s_closeButtonName, controlProtected));
        //        m_tailEdgeBorder.set(GetTemplateChildT<Grid>(s_tailEdgeBorderName, controlProtected));
        //        m_tailPolygon.set(GetTemplateChildT<Polygon>(s_tailPolygonName, controlProtected));
        //        m_titleTextBox.set(GetTemplateChildT<UIElement>(s_titleTextBoxName, controlProtected));
        //        m_subtitleTextBox.set(GetTemplateChildT<UIElement>(s_subtitleTextBoxName, controlProtected));

        //        if (auto && container = m_container.get())
        //        {
        //            container.Child(nullptr);
        //        }

        //        m_contentSizeChangedRevoker = [this]()
        //        {
        //            if (auto && tailOcclusionGrid = m_tailOcclusionGrid.get())
        //            {
        //                return tailOcclusionGrid.SizeChanged(auto_revoke, { this, &OnContentSizeChanged });
        //            }
        //            return FrameworkElement.SizeChanged_revoker{ };
        //        } ();

        //        if (auto && contentRootGrid = m_contentRootGrid.get())
        //        {
        //            AutomationProperties.SetLocalizedLandmarkType(contentRootGrid, ResourceAccessor.GetLocalizedStringResource(SR_TeachingTipCustomLandmarkName));
        //        }

        //        m_closeButtonClickedRevoker = [this]()
        //        {
        //            if (auto && closeButton = m_closeButton.get())
        //            {
        //                return closeButton.Click(auto_revoke, { this, &OnCloseButtonClicked });
        //            }
        //            return Button.Click_revoker{ };
        //        } ();

        //        m_alternateCloseButtonClickedRevoker = [this]()
        //        {
        //            if (auto && alternateCloseButton = m_alternateCloseButton.get())
        //            {
        //                AutomationProperties.SetName(alternateCloseButton, ResourceAccessor.GetLocalizedStringResource(SR_TeachingTipAlternateCloseButtonName));
        //                ToolTip tooltip = ToolTip();
        //                tooltip.Content(box_value(ResourceAccessor.GetLocalizedStringResource(SR_TeachingTipAlternateCloseButtonTooltip)));
        //                ToolTipService.SetToolTip(alternateCloseButton, tooltip);
        //                return alternateCloseButton.Click(auto_revoke, { this, &OnCloseButtonClicked });
        //            }
        //            return Button.Click_revoker{ };
        //        } ();

        //        m_actionButtonClickedRevoker = [this]()
        //        {
        //            if (auto && actionButton = m_actionButton.get())
        //            {
        //                return actionButton.Click(auto_revoke, { this, &OnActionButtonClicked });
        //            }
        //            return Button.Click_revoker{ };
        //        } ();

        //        UpdateButtonsState();
        //        OnIsLightDismissEnabledChanged();
        //        OnIconSourceChanged();
        //        OnHeroContentPlacementChanged();

        //        EstablishShadows();

        //        m_isTemplateApplied = true;
        //    }

        private void OnPropertyChanged(DependencyPropertyChangedEventArgs args)
        {
            DependencyProperty property = args.Property;

            //if (property == s_IsOpenProperty)
            //{
            //    OnIsOpenChanged();
            //}
            //else if (property == s_TargetProperty)
            //{
            //    // Unregister from old target if it exists
            //    if (args.OldValue())
            //    {
            //        m_TargetUnloadedRevoker.revoke();
            //    }

            //    // Register to new target if it exists
            //    if (const auto&value = args.NewValue()) {
            //        FrameworkElement newTarget = unbox_value<FrameworkElement>(value);
            //        m_TargetUnloadedRevoker = newTarget.Unloaded(auto_revoke, { this,&ClosePopupOnUnloadEvent });
            //    }
            //    OnTargetChanged();
            //}
            //else if (property == s_ActionButtonContentProperty ||
            //    property == s_CloseButtonContentProperty)
            //{
            //    UpdateButtonsState();
            //}
            //else if (property == s_PlacementMarginProperty)
            //{
            //    OnPlacementMarginChanged();
            //}
            //else if (property == s_IsLightDismissEnabledProperty)
            //{
            //    OnIsLightDismissEnabledChanged();
            //}
            //else if (property == s_ShouldConstrainToRootBoundsProperty)
            //{
            //    OnShouldConstrainToRootBoundsChanged();
            //}
            //else if (property == s_TailVisibilityProperty)
            //{
            //    OnTailVisibilityChanged();
            //}
            //else if (property == s_PreferredPlacementProperty)
            //{
            //    if (IsOpen())
            //    {
            //        PositionPopup();
            //    }
            //}
            //else if (property == s_HeroContentPlacementProperty)
            //{
            //    OnHeroContentPlacementChanged();
            //}
            //else if (property == s_IconSourceProperty)
            //{
            //    OnIconSourceChanged();
            //}
            //else if (property == s_TitleProperty)
            //{
            //    SetPopupAutomationProperties();
            //    if (ToggleVisibilityForEmptyContent(m_titleTextBox.get(), Title()))
            //    {
            //        TeachingTipTestHooks.NotifyTitleVisibilityChanged(this);
            //    }
            //}
            //else if (property == s_SubtitleProperty)
            //{
            //    if (ToggleVisibilityForEmptyContent(m_subtitleTextBox.get(), Subtitle()))
            //    {
            //        TeachingTipTestHooks.NotifySubtitleVisibilityChanged(this);
            //    }
            //}

        }

        //    bool ToggleVisibilityForEmptyContent(const UIElement& element, const hstring& content)
        //    {
        //        if (element)
        //        {
        //            if (content != L"")
        //        {
        //                if (element.Visibility() == Visibility.Collapsed)
        //                {
        //                    element.Visibility(Visibility.Visible);
        //                    return true;
        //                }
        //            }
        //        else
        //            {
        //                if (element.Visibility() == Visibility.Visible)
        //                {
        //                    element.Visibility(Visibility.Collapsed);
        //                    return true;
        //                }
        //            }
        //        }
        //        return false;
        //    }

        //    void OnContentChanged(const IInspectable& oldContent, const IInspectable& newContent)
        //    {
        //        if (newContent)
        //        {
        //            VisualStateManager.GoToState(this, L"Content"sv, false);
        //        }
        //        else
        //        {
        //            VisualStateManager.GoToState(this, L"NoContent"sv, false);
        //        }
        //    }

        //    void SetPopupAutomationProperties()
        //    {
        //        if (auto && popup = m_popup.get())
        //        {
        //            auto name = AutomationProperties.GetName(this);
        //            if (name.empty())
        //            {
        //                name = Title();
        //            }
        //            AutomationProperties.SetName(popup, name);

        //            AutomationProperties.SetAutomationId(popup, AutomationProperties.GetAutomationId(this));
        //        }
        //    }

        //    // Playing a closing animation when the Teaching Tip is closed via light dismiss requires this work around.
        //    // This is because there is no event that occurs when a popup is closing due to light dismiss so we have no way to intercept
        //    // the close and play our animation first. To work around this we've created a second popup which has no content and sits
        //    // underneath the teaching tip and is put into light dismiss mode instead of the primary popup. Then when this popup closes
        //    // due to light dismiss we know we are supposed to close the primary popup as well. To ensure that this popup does not block
        //    // interaction to the primary popup we need to make sure that the LightDismissIndicatorPopup is always opened first, so that
        //    // it is Z ordered underneath the primary popup.
        //    void CreateLightDismissIndicatorPopup()
        //    {
        //        auto const popup = Popup();
        //        // Set XamlRoot on the popup to handle XamlIsland/AppWindow scenarios.
        //        if (IUIElement10 uiElement10 = this)
        //    {
        //            popup.XamlRoot(uiElement10.XamlRoot());
        //        }
        //        // A Popup needs contents to open, so set a child that doesn't do anything.
        //        auto const grid = Grid();
        //        popup.Child(grid);

        //        m_lightDismissIndicatorPopup.set(popup);
        //    }

        //    bool UpdateTail()
        //    {
        //        // An effective placement of auto indicates that no tail should be shown.
        //        auto const [placement, tipDoesNotFit] = DetermineEffectivePlacement();
        //        m_currentEffectiveTailPlacementMode = placement;
        //        const auto tailVisibility = TailVisibility();
        //        if (tailVisibility == TeachingTipTailVisibility.Collapsed || (!m_target && tailVisibility != TeachingTipTailVisibility.Visible))
        //        {
        //            m_currentEffectiveTailPlacementMode = TeachingTipPlacementMode.Auto;
        //        }

        //        if (placement != m_currentEffectiveTipPlacementMode)
        //        {
        //            m_currentEffectiveTipPlacementMode = placement;
        //            TeachingTipTestHooks.NotifyEffectivePlacementChanged(this);
        //        }

        //        auto && nullableTailOcclusionGrid = m_tailOcclusionGrid.get();

        //        auto const height = nullableTailOcclusionGrid ? static_cast<float>(nullableTailOcclusionGrid.ActualHeight()) : 0;
        //        auto const width = nullableTailOcclusionGrid ? static_cast<float>(nullableTailOcclusionGrid.ActualWidth()) : 0;

        //        auto const [firstColumnWidth, secondColumnWidth, nextToLastColumnWidth, lastColumnWidth] = [this, nullableTailOcclusionGrid]()
        //        {
        //            if (auto const columnDefinitions = nullableTailOcclusionGrid ? nullableTailOcclusionGrid.ColumnDefinitions() : nullptr)
        //        {
        //                auto const firstColumnWidth = columnDefinitions.Size() > 0 ? static_cast<float>(columnDefinitions.GetAt(0).ActualWidth()) : 0.0f;
        //                auto const secondColumnWidth = columnDefinitions.Size() > 1 ? static_cast<float>(columnDefinitions.GetAt(1).ActualWidth()) : 0.0f;
        //                auto const nextToLastColumnWidth = columnDefinitions.Size() > 1 ? static_cast<float>(columnDefinitions.GetAt(columnDefinitions.Size() - 2).ActualWidth()) : 0.0f;
        //                auto const lastColumnWidth = columnDefinitions.Size() > 0 ? static_cast<float>(columnDefinitions.GetAt(columnDefinitions.Size() - 1).ActualWidth()) : 0.0f;

        //                return std.make_tuple(firstColumnWidth, secondColumnWidth, nextToLastColumnWidth, lastColumnWidth);
        //            }
        //            return std.make_tuple(0.0f, 0.0f, 0.0f, 0.0f);
        //        } ();

        //        auto const [firstRowHeight, secondRowHeight, nextToLastRowHeight, lastRowHeight] = [this, nullableTailOcclusionGrid]()
        //        {
        //            if (auto const rowDefinitions = nullableTailOcclusionGrid ? nullableTailOcclusionGrid.RowDefinitions() : nullptr)
        //        {
        //                auto const firstRowHeight = rowDefinitions.Size() > 0 ? static_cast<float>(rowDefinitions.GetAt(0).ActualHeight()) : 0.0f;
        //                auto const secondRowHeight = rowDefinitions.Size() > 1 ? static_cast<float>(rowDefinitions.GetAt(1).ActualHeight()) : 0.0f;
        //                auto const nextToLastRowHeight = rowDefinitions.Size() > 1 ? static_cast<float>(rowDefinitions.GetAt(rowDefinitions.Size() - 2).ActualHeight()) : 0.0f;
        //                auto const lastRowHeight = rowDefinitions.Size() > 0 ? static_cast<float>(rowDefinitions.GetAt(rowDefinitions.Size() - 1).ActualHeight()) : 0.0f;

        //                return std.make_tuple(firstRowHeight, secondRowHeight, nextToLastRowHeight, lastRowHeight);
        //            }
        //            return std.make_tuple(0.0f, 0.0f, 0.0f, 0.0f);
        //        } ();

        //        UpdateSizeBasedTemplateSettings();

        //        switch (m_currentEffectiveTailPlacementMode)
        //        {
        //            // An effective placement of auto means the tip should not display a tail.
        //            case TeachingTipPlacementMode.Auto:
        //                TrySetCenterPoint(nullableTailOcclusionGrid, { width / 2, height / 2, 0.0f });
        //                UpdateDynamicHeroContentPlacementToTop();
        //                VisualStateManager.GoToState(this, L"Untargeted"sv, false);
        //                break;

        //            case TeachingTipPlacementMode.Top:
        //                TrySetCenterPoint(nullableTailOcclusionGrid, { width / 2, height - lastRowHeight, 0.0f });
        //                TrySetCenterPoint(m_tailEdgeBorder.get(), { (width / 2) - firstColumnWidth, 0.0f, 0.0f });
        //                UpdateDynamicHeroContentPlacementToTop();
        //                VisualStateManager.GoToState(this, L"Top"sv, false);
        //                break;

        //            case TeachingTipPlacementMode.Bottom:
        //                TrySetCenterPoint(nullableTailOcclusionGrid, { width / 2, firstRowHeight, 0.0f });
        //                TrySetCenterPoint(m_tailEdgeBorder.get(), { (width / 2) - firstColumnWidth, 0.0f, 0.0f });
        //                UpdateDynamicHeroContentPlacementToBottom();
        //                VisualStateManager.GoToState(this, L"Bottom"sv, false);
        //                break;

        //            case TeachingTipPlacementMode.Left:
        //                TrySetCenterPoint(nullableTailOcclusionGrid, { width - lastColumnWidth, (height / 2), 0.0f });
        //                TrySetCenterPoint(m_tailEdgeBorder.get(), { 0.0f, (height / 2) - firstRowHeight, 0.0f });
        //                UpdateDynamicHeroContentPlacementToTop();
        //                VisualStateManager.GoToState(this, L"Left"sv, false);
        //                break;

        //            case TeachingTipPlacementMode.Right:
        //                TrySetCenterPoint(nullableTailOcclusionGrid, { firstColumnWidth, height / 2, 0.0f });
        //                TrySetCenterPoint(m_tailEdgeBorder.get(), { 0.0f, (height / 2) - firstRowHeight, 0.0f });
        //                UpdateDynamicHeroContentPlacementToTop();
        //                VisualStateManager.GoToState(this, L"Right"sv, false);
        //                break;

        //            case TeachingTipPlacementMode.TopRight:
        //                TrySetCenterPoint(nullableTailOcclusionGrid, { firstColumnWidth + secondColumnWidth + 1, height - lastRowHeight, 0.0f });
        //                TrySetCenterPoint(m_tailEdgeBorder.get(), { secondColumnWidth, 0.0f, 0.0f });
        //                UpdateDynamicHeroContentPlacementToTop();
        //                VisualStateManager.GoToState(this, L"TopRight"sv, false);
        //                break;

        //            case TeachingTipPlacementMode.TopLeft:
        //                TrySetCenterPoint(nullableTailOcclusionGrid, { width - (nextToLastColumnWidth + lastColumnWidth + 1), height - lastRowHeight, 0.0f });
        //                TrySetCenterPoint(m_tailEdgeBorder.get(), { width - (nextToLastColumnWidth + firstColumnWidth + lastColumnWidth), 0.0f, 0.0f });
        //                UpdateDynamicHeroContentPlacementToTop();
        //                VisualStateManager.GoToState(this, L"TopLeft"sv, false);
        //                break;

        //            case TeachingTipPlacementMode.BottomRight:
        //                TrySetCenterPoint(nullableTailOcclusionGrid, { firstColumnWidth + secondColumnWidth + 1, firstRowHeight, 0.0f });
        //                TrySetCenterPoint(m_tailEdgeBorder.get(), { secondColumnWidth, 0.0f, 0.0f });
        //                UpdateDynamicHeroContentPlacementToBottom();
        //                VisualStateManager.GoToState(this, L"BottomRight"sv, false);
        //                break;

        //            case TeachingTipPlacementMode.BottomLeft:
        //                TrySetCenterPoint(nullableTailOcclusionGrid, { width - (nextToLastColumnWidth + lastColumnWidth + 1), firstRowHeight, 0.0f });
        //                TrySetCenterPoint(m_tailEdgeBorder.get(), { width - (nextToLastColumnWidth + firstColumnWidth + lastColumnWidth), 0.0f, 0.0f });
        //                UpdateDynamicHeroContentPlacementToBottom();
        //                VisualStateManager.GoToState(this, L"BottomLeft"sv, false);
        //                break;

        //            case TeachingTipPlacementMode.LeftTop:
        //                TrySetCenterPoint(nullableTailOcclusionGrid, { width - lastColumnWidth,  height - (nextToLastRowHeight + lastRowHeight + 1), 0.0f });
        //                TrySetCenterPoint(m_tailEdgeBorder.get(), { 0.0f,  height - (nextToLastRowHeight + firstRowHeight + lastRowHeight), 0.0f });
        //                UpdateDynamicHeroContentPlacementToTop();
        //                VisualStateManager.GoToState(this, L"LeftTop"sv, false);
        //                break;

        //            case TeachingTipPlacementMode.LeftBottom:
        //                TrySetCenterPoint(nullableTailOcclusionGrid, { width - lastColumnWidth, (firstRowHeight + secondRowHeight + 1), 0.0f });
        //                TrySetCenterPoint(m_tailEdgeBorder.get(), { 0.0f, secondRowHeight, 0.0f });
        //                UpdateDynamicHeroContentPlacementToBottom();
        //                VisualStateManager.GoToState(this, L"LeftBottom"sv, false);
        //                break;

        //            case TeachingTipPlacementMode.RightTop:
        //                TrySetCenterPoint(nullableTailOcclusionGrid, { firstColumnWidth, height - (nextToLastRowHeight + lastRowHeight + 1), 0.0f });
        //                TrySetCenterPoint(m_tailEdgeBorder.get(), { 0.0f, height - (nextToLastRowHeight + firstRowHeight + lastRowHeight), 0.0f });
        //                UpdateDynamicHeroContentPlacementToTop();
        //                VisualStateManager.GoToState(this, L"RightTop"sv, false);
        //                break;

        //            case TeachingTipPlacementMode.RightBottom:
        //                TrySetCenterPoint(nullableTailOcclusionGrid, { firstColumnWidth, (firstRowHeight + secondRowHeight + 1), 0.0f });
        //                TrySetCenterPoint(m_tailEdgeBorder.get(), { 0.0f, secondRowHeight, 0.0f });
        //                UpdateDynamicHeroContentPlacementToBottom();
        //                VisualStateManager.GoToState(this, L"RightBottom"sv, false);
        //                break;

        //            case TeachingTipPlacementMode.Center:
        //                TrySetCenterPoint(nullableTailOcclusionGrid, { width / 2, height - lastRowHeight, 0.0f });
        //                TrySetCenterPoint(m_tailEdgeBorder.get(), { (width / 2) - firstColumnWidth, 0.0f, 0.0f });
        //                UpdateDynamicHeroContentPlacementToTop();
        //                VisualStateManager.GoToState(this, L"Center"sv, false);
        //                break;

        //            default:
        //                break;
        //        }

        //        return tipDoesNotFit;
        //    }

        //    void PositionPopup()
        //    {
        //        bool tipDoesNotFit = false;
        //        if (m_target)
        //        {
        //            tipDoesNotFit = PositionTargetedPopup();
        //        }
        //        else
        //        {
        //            tipDoesNotFit = PositionUntargetedPopup();
        //        }
        //        if (tipDoesNotFit)
        //        {
        //            IsOpen(false);
        //        }

        //        TeachingTipTestHooks.NotifyOffsetChanged(this);
        //    }

        //    bool PositionTargetedPopup()
        //    {
        //        const bool tipDoesNotFit = UpdateTail();
        //        auto const offset = PlacementMargin();

        //        auto const [tipHeight, tipWidth] = [this]()
        //        {
        //            if (auto && tailOcclusionGrid = m_tailOcclusionGrid.get())
        //            {
        //                auto const tipHeight = tailOcclusionGrid.ActualHeight();
        //                auto const tipWidth = tailOcclusionGrid.ActualWidth();
        //                return std.make_tuple(tipHeight, tipWidth);
        //            }
        //            return std.make_tuple(0.0, 0.0);
        //        } ();

        //        if (auto && popup = m_popup.get())
        //        {
        //            // Depending on the effective placement mode of the tip we use a combination of the tip's size, the target's position within the app, the target's
        //            // size, and the target offset property to determine the appropriate vertical and horizontal offsets of the popup that the tip is contained in.
        //            switch (m_currentEffectiveTipPlacementMode)
        //            {
        //                case TeachingTipPlacementMode.Top:
        //                    popup.VerticalOffset(m_currentTargetBoundsInCoreWindowSpace.Y - tipHeight - offset.Top);
        //                    popup.HorizontalOffset((((m_currentTargetBoundsInCoreWindowSpace.X * 2.0f) + m_currentTargetBoundsInCoreWindowSpace.Width - tipWidth) / 2.0f));
        //                    break;

        //                case TeachingTipPlacementMode.Bottom:
        //                    popup.VerticalOffset(m_currentTargetBoundsInCoreWindowSpace.Y + m_currentTargetBoundsInCoreWindowSpace.Height + static_cast<float>(offset.Bottom));
        //                    popup.HorizontalOffset((((m_currentTargetBoundsInCoreWindowSpace.X * 2.0f) + m_currentTargetBoundsInCoreWindowSpace.Width - tipWidth) / 2.0f));
        //                    break;

        //                case TeachingTipPlacementMode.Left:
        //                    popup.VerticalOffset(((m_currentTargetBoundsInCoreWindowSpace.Y * 2.0f) + m_currentTargetBoundsInCoreWindowSpace.Height - tipHeight) / 2.0f);
        //                    popup.HorizontalOffset(m_currentTargetBoundsInCoreWindowSpace.X - tipWidth - offset.Left);
        //                    break;

        //                case TeachingTipPlacementMode.Right:
        //                    popup.VerticalOffset(((m_currentTargetBoundsInCoreWindowSpace.Y * 2.0f) + m_currentTargetBoundsInCoreWindowSpace.Height - tipHeight) / 2.0f);
        //                    popup.HorizontalOffset(m_currentTargetBoundsInCoreWindowSpace.X + m_currentTargetBoundsInCoreWindowSpace.Width + static_cast<float>(offset.Right));
        //                    break;

        //                case TeachingTipPlacementMode.TopRight:
        //                    popup.VerticalOffset(m_currentTargetBoundsInCoreWindowSpace.Y - tipHeight - offset.Top);
        //                    popup.HorizontalOffset(((((m_currentTargetBoundsInCoreWindowSpace.X * 2.0f) + m_currentTargetBoundsInCoreWindowSpace.Width) / 2.0f) - MinimumTipEdgeToTailCenter()));
        //                    break;

        //                case TeachingTipPlacementMode.TopLeft:
        //                    popup.VerticalOffset(m_currentTargetBoundsInCoreWindowSpace.Y - tipHeight - offset.Top);
        //                    popup.HorizontalOffset(((((m_currentTargetBoundsInCoreWindowSpace.X * 2.0f) + m_currentTargetBoundsInCoreWindowSpace.Width) / 2.0f) - tipWidth + MinimumTipEdgeToTailCenter()));
        //                    break;

        //                case TeachingTipPlacementMode.BottomRight:
        //                    popup.VerticalOffset(m_currentTargetBoundsInCoreWindowSpace.Y + m_currentTargetBoundsInCoreWindowSpace.Height + static_cast<float>(offset.Bottom));
        //                    popup.HorizontalOffset(((((m_currentTargetBoundsInCoreWindowSpace.X * 2.0f) + m_currentTargetBoundsInCoreWindowSpace.Width) / 2.0f) - MinimumTipEdgeToTailCenter()));
        //                    break;

        //                case TeachingTipPlacementMode.BottomLeft:
        //                    popup.VerticalOffset(m_currentTargetBoundsInCoreWindowSpace.Y + m_currentTargetBoundsInCoreWindowSpace.Height + static_cast<float>(offset.Bottom));
        //                    popup.HorizontalOffset(((((m_currentTargetBoundsInCoreWindowSpace.X * 2.0f) + m_currentTargetBoundsInCoreWindowSpace.Width) / 2.0f) - tipWidth + MinimumTipEdgeToTailCenter()));
        //                    break;

        //                case TeachingTipPlacementMode.LeftTop:
        //                    popup.VerticalOffset((((m_currentTargetBoundsInCoreWindowSpace.Y * 2.0f) + m_currentTargetBoundsInCoreWindowSpace.Height) / 2.0f) - tipHeight + MinimumTipEdgeToTailCenter());
        //                    popup.HorizontalOffset(m_currentTargetBoundsInCoreWindowSpace.X - tipWidth - offset.Left);
        //                    break;

        //                case TeachingTipPlacementMode.LeftBottom:
        //                    popup.VerticalOffset((((m_currentTargetBoundsInCoreWindowSpace.Y * 2.0f) + m_currentTargetBoundsInCoreWindowSpace.Height) / 2.0f) - MinimumTipEdgeToTailCenter());
        //                    popup.HorizontalOffset(m_currentTargetBoundsInCoreWindowSpace.X - tipWidth - offset.Left);
        //                    break;

        //                case TeachingTipPlacementMode.RightTop:
        //                    popup.VerticalOffset((((m_currentTargetBoundsInCoreWindowSpace.Y * 2.0f) + m_currentTargetBoundsInCoreWindowSpace.Height) / 2.0f) - tipHeight + MinimumTipEdgeToTailCenter());
        //                    popup.HorizontalOffset(m_currentTargetBoundsInCoreWindowSpace.X + m_currentTargetBoundsInCoreWindowSpace.Width + static_cast<float>(offset.Right));
        //                    break;

        //                case TeachingTipPlacementMode.RightBottom:
        //                    popup.VerticalOffset((((m_currentTargetBoundsInCoreWindowSpace.Y * 2.0f) + m_currentTargetBoundsInCoreWindowSpace.Height) / 2.0f) - MinimumTipEdgeToTailCenter());
        //                    popup.HorizontalOffset(m_currentTargetBoundsInCoreWindowSpace.X + m_currentTargetBoundsInCoreWindowSpace.Width + static_cast<float>(offset.Right));
        //                    break;

        //                case TeachingTipPlacementMode.Center:
        //                    popup.VerticalOffset(m_currentTargetBoundsInCoreWindowSpace.Y + (m_currentTargetBoundsInCoreWindowSpace.Height / 2.0f) - tipHeight - offset.Top);
        //                    popup.HorizontalOffset((((m_currentTargetBoundsInCoreWindowSpace.X * 2.0f) + m_currentTargetBoundsInCoreWindowSpace.Width - tipWidth) / 2.0f));
        //                    break;

        //                default:
        //                    MUX_FAIL_FAST();
        //            }
        //        }
        //        return tipDoesNotFit;
        //    }

        //    bool PositionUntargetedPopup()
        //    {
        //        auto const windowBoundsInCoreWindowSpace = GetEffectiveWindowBoundsInCoreWindowSpace(GetWindowBounds());

        //        auto const [finalTipHeight, finalTipWidth] = [this]()
        //        {
        //            if (auto && tailOcclusionGrid = m_tailOcclusionGrid.get())
        //            {
        //                auto const finalTipHeight = tailOcclusionGrid.ActualHeight();
        //                auto const finalTipWidth = tailOcclusionGrid.ActualWidth();
        //                return std.make_tuple(finalTipHeight, finalTipWidth);
        //            }
        //            return std.make_tuple(0.0, 0.0);
        //        } ();

        //        const bool tipDoesNotFit = UpdateTail();

        //        auto const offset = PlacementMargin();

        //        // Depending on the effective placement mode of the tip we use a combination of the tip's size, the window's size, and the target
        //        // offset property to determine the appropriate vertical and horizontal offsets of the popup that the tip is contained in.
        //        if (auto && popup = m_popup.get())
        //        {
        //            switch (GetFlowDirectionAdjustedPlacement(PreferredPlacement()))
        //            {
        //                case TeachingTipPlacementMode.Auto:
        //                case TeachingTipPlacementMode.Bottom:
        //                    popup.VerticalOffset(UntargetedTipFarPlacementOffset(windowBoundsInCoreWindowSpace.Height, finalTipHeight, offset.Bottom));
        //                    popup.HorizontalOffset(UntargetedTipCenterPlacementOffset(windowBoundsInCoreWindowSpace.X, windowBoundsInCoreWindowSpace.Width, finalTipWidth, offset.Left, offset.Right));
        //                    break;

        //                case TeachingTipPlacementMode.Top:
        //                    popup.VerticalOffset(UntargetedTipNearPlacementOffset(windowBoundsInCoreWindowSpace.Y, offset.Top));
        //                    popup.HorizontalOffset(UntargetedTipCenterPlacementOffset(windowBoundsInCoreWindowSpace.X, windowBoundsInCoreWindowSpace.Width, finalTipWidth, offset.Left, offset.Right));
        //                    break;

        //                case TeachingTipPlacementMode.Left:
        //                    popup.VerticalOffset(UntargetedTipCenterPlacementOffset(windowBoundsInCoreWindowSpace.Y, windowBoundsInCoreWindowSpace.Height, finalTipHeight, offset.Top, offset.Bottom));
        //                    popup.HorizontalOffset(UntargetedTipNearPlacementOffset(windowBoundsInCoreWindowSpace.X, offset.Left));
        //                    break;

        //                case TeachingTipPlacementMode.Right:
        //                    popup.VerticalOffset(UntargetedTipCenterPlacementOffset(windowBoundsInCoreWindowSpace.Y, windowBoundsInCoreWindowSpace.Height, finalTipHeight, offset.Top, offset.Bottom));
        //                    popup.HorizontalOffset(UntargetedTipFarPlacementOffset(windowBoundsInCoreWindowSpace.Width, finalTipWidth, offset.Right));
        //                    break;

        //                case TeachingTipPlacementMode.TopRight:
        //                    popup.VerticalOffset(UntargetedTipNearPlacementOffset(windowBoundsInCoreWindowSpace.Y, offset.Top));
        //                    popup.HorizontalOffset(UntargetedTipFarPlacementOffset(windowBoundsInCoreWindowSpace.Width, finalTipWidth, offset.Right));
        //                    break;

        //                case TeachingTipPlacementMode.TopLeft:
        //                    popup.VerticalOffset(UntargetedTipNearPlacementOffset(windowBoundsInCoreWindowSpace.Y, offset.Top));
        //                    popup.HorizontalOffset(UntargetedTipNearPlacementOffset(windowBoundsInCoreWindowSpace.X, offset.Left));
        //                    break;

        //                case TeachingTipPlacementMode.BottomRight:
        //                    popup.VerticalOffset(UntargetedTipFarPlacementOffset(windowBoundsInCoreWindowSpace.Height, finalTipHeight, offset.Bottom));
        //                    popup.HorizontalOffset(UntargetedTipFarPlacementOffset(windowBoundsInCoreWindowSpace.Width, finalTipWidth, offset.Right));
        //                    break;

        //                case TeachingTipPlacementMode.BottomLeft:
        //                    popup.VerticalOffset(UntargetedTipFarPlacementOffset(windowBoundsInCoreWindowSpace.Height, finalTipHeight, offset.Bottom));
        //                    popup.HorizontalOffset(UntargetedTipNearPlacementOffset(windowBoundsInCoreWindowSpace.X, offset.Left));
        //                    break;

        //                case TeachingTipPlacementMode.LeftTop:
        //                    popup.VerticalOffset(UntargetedTipNearPlacementOffset(windowBoundsInCoreWindowSpace.Y, offset.Top));
        //                    popup.HorizontalOffset(UntargetedTipNearPlacementOffset(windowBoundsInCoreWindowSpace.X, offset.Left));
        //                    break;

        //                case TeachingTipPlacementMode.LeftBottom:
        //                    popup.VerticalOffset(UntargetedTipFarPlacementOffset(windowBoundsInCoreWindowSpace.Height, finalTipHeight, offset.Bottom));
        //                    popup.HorizontalOffset(UntargetedTipNearPlacementOffset(windowBoundsInCoreWindowSpace.X, offset.Left));
        //                    break;

        //                case TeachingTipPlacementMode.RightTop:
        //                    popup.VerticalOffset(UntargetedTipNearPlacementOffset(windowBoundsInCoreWindowSpace.Y, offset.Top));
        //                    popup.HorizontalOffset(UntargetedTipFarPlacementOffset(windowBoundsInCoreWindowSpace.Width, finalTipWidth, offset.Right));
        //                    break;

        //                case TeachingTipPlacementMode.RightBottom:
        //                    popup.VerticalOffset(UntargetedTipFarPlacementOffset(windowBoundsInCoreWindowSpace.Height, finalTipHeight, offset.Bottom));
        //                    popup.HorizontalOffset(UntargetedTipFarPlacementOffset(windowBoundsInCoreWindowSpace.Width, finalTipWidth, offset.Right));
        //                    break;

        //                case TeachingTipPlacementMode.Center:
        //                    popup.VerticalOffset(UntargetedTipCenterPlacementOffset(windowBoundsInCoreWindowSpace.Y, windowBoundsInCoreWindowSpace.Height, finalTipHeight, offset.Top, offset.Bottom));
        //                    popup.HorizontalOffset(UntargetedTipCenterPlacementOffset(windowBoundsInCoreWindowSpace.X, windowBoundsInCoreWindowSpace.Width, finalTipWidth, offset.Left, offset.Right));
        //                    break;

        //                default:
        //                    MUX_FAIL_FAST();
        //            }
        //        }

        //        return tipDoesNotFit;
        //    }

        //    void UpdateSizeBasedTemplateSettings()
        //    {
        //        auto const templateSettings = get_self <.TeachingTipTemplateSettings > (TemplateSettings());
        //        auto const [width, height] = [this]()
        //        {
        //            if (auto && contentRootGrid = m_contentRootGrid.get())
        //            {
        //                auto const width = contentRootGrid.ActualWidth();
        //                auto const height = contentRootGrid.ActualHeight();
        //                return std.make_tuple(width, height);
        //            }
        //            return std.make_tuple(0.0, 0.0);
        //        } ();

        //        switch (m_currentEffectiveTailPlacementMode)
        //        {
        //            case TeachingTipPlacementMode.Top:
        //                templateSettings.TopRightHighlightMargin(OtherPlacementTopRightHighlightMargin(width, height));
        //                templateSettings.TopLeftHighlightMargin(TopEdgePlacementTopLeftHighlightMargin(width, height));
        //                break;
        //            case TeachingTipPlacementMode.Bottom:
        //                templateSettings.TopRightHighlightMargin(BottomPlacementTopRightHighlightMargin(width, height));
        //                templateSettings.TopLeftHighlightMargin(BottomPlacementTopLeftHighlightMargin(width, height));
        //                break;
        //            case TeachingTipPlacementMode.Left:
        //                templateSettings.TopRightHighlightMargin(OtherPlacementTopRightHighlightMargin(width, height));
        //                templateSettings.TopLeftHighlightMargin(LeftEdgePlacementTopLeftHighlightMargin(width, height));
        //                break;
        //            case TeachingTipPlacementMode.Right:
        //                templateSettings.TopRightHighlightMargin(OtherPlacementTopRightHighlightMargin(width, height));
        //                templateSettings.TopLeftHighlightMargin(RightEdgePlacementTopLeftHighlightMargin(width, height));
        //                break;
        //            case TeachingTipPlacementMode.TopLeft:
        //                templateSettings.TopRightHighlightMargin(OtherPlacementTopRightHighlightMargin(width, height));
        //                templateSettings.TopLeftHighlightMargin(TopEdgePlacementTopLeftHighlightMargin(width, height));
        //                break;
        //            case TeachingTipPlacementMode.TopRight:
        //                templateSettings.TopRightHighlightMargin(OtherPlacementTopRightHighlightMargin(width, height));
        //                templateSettings.TopLeftHighlightMargin(TopEdgePlacementTopLeftHighlightMargin(width, height));
        //                break;
        //            case TeachingTipPlacementMode.BottomLeft:
        //                templateSettings.TopRightHighlightMargin(BottomLeftPlacementTopRightHighlightMargin(width, height));
        //                templateSettings.TopLeftHighlightMargin(BottomLeftPlacementTopLeftHighlightMargin(width, height));
        //                break;
        //            case TeachingTipPlacementMode.BottomRight:
        //                templateSettings.TopRightHighlightMargin(BottomRightPlacementTopRightHighlightMargin(width, height));
        //                templateSettings.TopLeftHighlightMargin(BottomRightPlacementTopLeftHighlightMargin(width, height));
        //                break;
        //            case TeachingTipPlacementMode.LeftTop:
        //                templateSettings.TopRightHighlightMargin(OtherPlacementTopRightHighlightMargin(width, height));
        //                templateSettings.TopLeftHighlightMargin(LeftEdgePlacementTopLeftHighlightMargin(width, height));
        //                break;
        //            case TeachingTipPlacementMode.LeftBottom:
        //                templateSettings.TopRightHighlightMargin(OtherPlacementTopRightHighlightMargin(width, height));
        //                templateSettings.TopLeftHighlightMargin(LeftEdgePlacementTopLeftHighlightMargin(width, height));
        //                break;
        //            case TeachingTipPlacementMode.RightTop:
        //                templateSettings.TopRightHighlightMargin(OtherPlacementTopRightHighlightMargin(width, height));
        //                templateSettings.TopLeftHighlightMargin(RightEdgePlacementTopLeftHighlightMargin(width, height));
        //                break;
        //            case TeachingTipPlacementMode.RightBottom:
        //                templateSettings.TopRightHighlightMargin(OtherPlacementTopRightHighlightMargin(width, height));
        //                templateSettings.TopLeftHighlightMargin(RightEdgePlacementTopLeftHighlightMargin(width, height));
        //                break;
        //            case TeachingTipPlacementMode.Auto:
        //                templateSettings.TopRightHighlightMargin(OtherPlacementTopRightHighlightMargin(width, height));
        //                templateSettings.TopLeftHighlightMargin(TopEdgePlacementTopLeftHighlightMargin(width, height));
        //                break;
        //            case TeachingTipPlacementMode.Center:
        //                templateSettings.TopRightHighlightMargin(OtherPlacementTopRightHighlightMargin(width, height));
        //                templateSettings.TopLeftHighlightMargin(TopEdgePlacementTopLeftHighlightMargin(width, height));
        //                break;
        //        }
        //    }

        //    void UpdateButtonsState()
        //    {
        //        auto const actionContent = ActionButtonContent();
        //        auto const closeContent = CloseButtonContent();
        //        const bool isLightDismiss = IsLightDismissEnabled();
        //        if (actionContent && closeContent)
        //        {
        //            VisualStateManager.GoToState(this, L"BothButtonsVisible"sv, false);
        //            VisualStateManager.GoToState(this, L"FooterCloseButton"sv, false);
        //        }
        //        else if (actionContent && isLightDismiss)
        //        {
        //            VisualStateManager.GoToState(this, L"ActionButtonVisible"sv, false);
        //            VisualStateManager.GoToState(this, L"FooterCloseButton"sv, false);
        //        }
        //        else if (actionContent)
        //        {
        //            VisualStateManager.GoToState(this, L"ActionButtonVisible"sv, false);
        //            VisualStateManager.GoToState(this, L"HeaderCloseButton"sv, false);
        //        }
        //        else if (closeContent)
        //        {
        //            VisualStateManager.GoToState(this, L"CloseButtonVisible"sv, false);
        //            VisualStateManager.GoToState(this, L"FooterCloseButton"sv, false);
        //        }
        //        else if (isLightDismiss)
        //        {
        //            VisualStateManager.GoToState(this, L"NoButtonsVisible"sv, false);
        //            VisualStateManager.GoToState(this, L"FooterCloseButton"sv, false);
        //        }
        //        else
        //        {
        //            VisualStateManager.GoToState(this, L"NoButtonsVisible"sv, false);
        //            VisualStateManager.GoToState(this, L"HeaderCloseButton"sv, false);
        //        }
        //    }

        //    void UpdateDynamicHeroContentPlacementToTop()
        //    {
        //        if (HeroContentPlacement() == TeachingTipHeroContentPlacementMode.Auto)
        //        {
        //            UpdateDynamicHeroContentPlacementToTopImpl();
        //        }
        //    }

        //    void UpdateDynamicHeroContentPlacementToTopImpl()
        //    {
        //        VisualStateManager.GoToState(this, L"HeroContentTop"sv, false);
        //        if (m_currentHeroContentEffectivePlacementMode != TeachingTipHeroContentPlacementMode.Top)
        //        {
        //            m_currentHeroContentEffectivePlacementMode = TeachingTipHeroContentPlacementMode.Top;
        //            TeachingTipTestHooks.NotifyEffectiveHeroContentPlacementChanged(this);
        //        }
        //    }

        //    void UpdateDynamicHeroContentPlacementToBottom()
        //    {
        //        if (HeroContentPlacement() == TeachingTipHeroContentPlacementMode.Auto)
        //        {
        //            UpdateDynamicHeroContentPlacementToBottomImpl();
        //        }
        //    }

        //    void UpdateDynamicHeroContentPlacementToBottomImpl()
        //    {
        //        VisualStateManager.GoToState(this, L"HeroContentBottom"sv, false);
        //        if (m_currentHeroContentEffectivePlacementMode != TeachingTipHeroContentPlacementMode.Bottom)
        //        {
        //            m_currentHeroContentEffectivePlacementMode = TeachingTipHeroContentPlacementMode.Bottom;
        //            TeachingTipTestHooks.NotifyEffectiveHeroContentPlacementChanged(this);
        //        }
        //    }

        //    void OnIsOpenChanged()
        //    {
        //        SharedHelpers.QueueCallbackForCompositionRendering([strongThis = get_strong()]()
        //        {
        //            if (strongThis.IsOpen())
        //            {
        //                strongThis.IsOpenChangedToOpen();
        //            }
        //            else
        //            {
        //                strongThis.IsOpenChangedToClose();
        //            }
        //            TeachingTipTestHooks.NotifyOpenedStatusChanged(*strongThis);
        //        });
        //    }

        //    void IsOpenChangedToOpen()
        //    {
        //        //Reset the close reason to the default value of programmatic.
        //        m_lastCloseReason = TeachingTipCloseReason.Programmatic;

        //        m_currentBoundsInCoreWindowSpace = this.TransformToVisual(nullptr).TransformBounds({
        //            0.0,
        //        0.0,
        //        static_cast<float>(this.ActualWidth()),
        //        static_cast<float>(this.ActualHeight())
        //            });

        //        m_currentTargetBoundsInCoreWindowSpace = [this]()
        //        {
        //            if (auto && target = m_target.get())
        //            {
        //                SetViewportChangedEvent(gsl.make_strict_not_null(target));
        //                return target.TransformToVisual(nullptr).TransformBounds({
        //                    0.0,
        //                0.0,
        //                static_cast<float>(target.as< FrameworkElement > ().ActualWidth()),
        //                static_cast<float>(target.as< FrameworkElement > ().ActualHeight())
        //                    });
        //            }
        //            return Rect{ 0,0,0,0 };
        //        } ();

        //        if (!m_lightDismissIndicatorPopup)
        //        {
        //            CreateLightDismissIndicatorPopup();
        //        }
        //        OnIsLightDismissEnabledChanged();

        //        if (!m_contractAnimation)
        //        {
        //            CreateContractAnimation();
        //        }
        //        if (!m_expandAnimation)
        //        {
        //            CreateExpandAnimation();
        //        }

        //        // We are about to begin the process of trying to open the teaching tip, so notify that we are no longer idle.
        //        SetIsIdle(false);

        //        //If the developer defines their TeachingTip in a resource dictionary it is possible that it's template will have never been applied
        //        if (!m_isTemplateApplied)
        //        {
        //            this.ApplyTemplate();
        //        }

        //        if (!m_popup || m_createNewPopupOnOpen)
        //        {
        //            CreateNewPopup();
        //        }

        //        // If the tip is not going to open because it does not fit we need to make sure that
        //        // the open, closing, closed life cycle still fires so that we don't cause apps to leak
        //        // that depend on this sequence.
        //        auto const [ignored, tipDoesNotFit] = DetermineEffectivePlacement();
        //        if (tipDoesNotFit)
        //        {
        //            __RP_Marker_ClassMemberById(RuntimeProfiler.ProfId_TeachingTip, RuntimeProfiler.ProfMemberId_TeachingTip_TipDidNotOpenDueToSize);
        //            RaiseClosingEvent(false);
        //            auto const closedArgs = make_self<TeachingTipClosedEventArgs>();
        //            closedArgs.Reason(m_lastCloseReason);
        //            m_closedEventSource(this, *closedArgs);
        //            IsOpen(false);
        //        }
        //        else
        //        {
        //            if (auto && popup = m_popup.get())
        //            {
        //                if (!popup.IsOpen())
        //                {
        //                    UpdatePopupRequestedTheme();
        //                    popup.Child(m_rootElement.get());
        //                    if (auto && lightDismissIndicatorPopup = m_lightDismissIndicatorPopup.get())
        //                    {
        //                        lightDismissIndicatorPopup.IsOpen(true);
        //                    }
        //                    popup.IsOpen(true);
        //                }
        //                else
        //                {
        //                    // We have become Open but our popup was already open. This can happen when a close is canceled by the closing event, so make sure the idle status is correct.
        //                    if (!m_isExpandAnimationPlaying && !m_isContractAnimationPlaying)
        //                    {
        //                        SetIsIdle(true);
        //                    }
        //                }
        //            }
        //        }

        //        m_acceleratorKeyActivatedRevoker = Dispatcher().AcceleratorKeyActivated(auto_revoke, { this, &OnF6AcceleratorKeyClicked });

        //        // Make sure we are in the correct VSM state after ApplyTemplate and moving the template content from the Control to the Popup:
        //        OnIsLightDismissEnabledChanged();
        //    }

        //    void IsOpenChangedToClose()
        //    {
        //        if (auto && popup = m_popup.get())
        //        {
        //            if (popup.IsOpen())
        //            {
        //                // We are about to begin the process of trying to close the teaching tip, so notify that we are no longer idle.
        //                SetIsIdle(false);
        //                RaiseClosingEvent(true);
        //            }
        //            else
        //            {
        //                // We have become not Open but our popup was already not open. Lets make sure the idle status is correct.
        //                if (!m_isExpandAnimationPlaying && !m_isContractAnimationPlaying)
        //                {
        //                    SetIsIdle(true);
        //                }
        //            }
        //        }

        //        m_currentEffectiveTipPlacementMode = TeachingTipPlacementMode.Auto;
        //        TeachingTipTestHooks.NotifyEffectivePlacementChanged(this);
        //    }

        //    void CreateNewPopup()
        //    {
        //        m_popupOpenedRevoker.revoke();
        //        m_popupClosedRevoker.revoke();

        //        auto const popup = Popup();
        //        // Set XamlRoot on the popup to handle XamlIsland/AppWindow scenarios.
        //        if (IUIElement10 uiElement10 = this)
        //    {
        //            popup.XamlRoot(uiElement10.XamlRoot());
        //        }

        //        m_popupOpenedRevoker = popup.Opened(auto_revoke, { this, &OnPopupOpened });
        //        m_popupClosedRevoker = popup.Closed(auto_revoke, { this, &OnPopupClosed });
        //        if (IPopup3 popup3 = popup)
        //    {
        //            popup3.ShouldConstrainToRootBounds(ShouldConstrainToRootBounds());
        //        }
        //        m_popup.set(popup);
        //        SetPopupAutomationProperties();
        //        m_createNewPopupOnOpen = false;
        //    }

        //    void OnTailVisibilityChanged()
        //    {
        //        UpdateTail();
        //    }

        //    void OnIconSourceChanged()
        //    {
        //        auto const templateSettings = get_self <.TeachingTipTemplateSettings > (TemplateSettings());
        //        if (auto const source = IconSource())
        //    {
        //            templateSettings.IconElement(SharedHelpers.MakeIconElementFrom(source));
        //            VisualStateManager.GoToState(this, L"Icon"sv, false);
        //        }
        //    else
        //        {
        //            templateSettings.IconElement(nullptr);
        //            VisualStateManager.GoToState(this, L"NoIcon"sv, false);
        //        }
        //    }

        //    void OnPlacementMarginChanged()
        //    {
        //        if (IsOpen())
        //        {
        //            PositionPopup();
        //        }
        //    }

        //    void OnIsLightDismissEnabledChanged()
        //    {
        //        if (IsLightDismissEnabled())
        //        {
        //            VisualStateManager.GoToState(this, L"LightDismiss"sv, false);
        //            if (auto && lightDismissIndicatorPopup = m_lightDismissIndicatorPopup.get())
        //            {
        //                lightDismissIndicatorPopup.IsLightDismissEnabled(true);
        //                m_lightDismissIndicatorPopupClosedRevoker = lightDismissIndicatorPopup.Closed(auto_revoke, { this, &OnLightDismissIndicatorPopupClosed });
        //            }
        //        }
        //        else
        //        {
        //            VisualStateManager.GoToState(this, L"NormalDismiss"sv, false);
        //            if (auto && lightDismissIndicatorPopup = m_lightDismissIndicatorPopup.get())
        //            {
        //                lightDismissIndicatorPopup.IsLightDismissEnabled(false);
        //            }
        //            m_lightDismissIndicatorPopupClosedRevoker.revoke();
        //        }
        //        UpdateButtonsState();
        //    }

        //    void OnShouldConstrainToRootBoundsChanged()
        //    {
        //        // ShouldConstrainToRootBounds is a property that can only be set on a popup before it is opened.
        //        // If we have opened the tip's popup and then this property changes we will need to discard the old popup
        //        // and replace it with a new popup.  This variable indicates this state.

        //        //The underlying popup api is only available on 19h1 plus, if we aren't on that no opt.
        //        if (m_popup.get().try_as<Controls.Primitives.IPopup3>())
        //        {
        //            m_createNewPopupOnOpen = true;
        //        }
        //    }

        //    void OnHeroContentPlacementChanged()
        //    {
        //        switch (HeroContentPlacement())
        //        {
        //            case TeachingTipHeroContentPlacementMode.Auto:
        //                break;
        //            case TeachingTipHeroContentPlacementMode.Top:
        //                UpdateDynamicHeroContentPlacementToTopImpl();
        //                break;
        //            case TeachingTipHeroContentPlacementMode.Bottom:
        //                UpdateDynamicHeroContentPlacementToBottomImpl();
        //                break;
        //        }

        //        // Setting m_currentEffectiveTipPlacementMode to auto ensures that the next time position popup is called we'll rerun the DetermineEffectivePlacement
        //        // algorithm. If we did not do this and the popup was opened the algorithm would maintain the current effective placement mode, which we don't want
        //        // since the hero content placement contributes to the choice of tip placement mode.
        //        m_currentEffectiveTipPlacementMode = TeachingTipPlacementMode.Auto;
        //        TeachingTipTestHooks.NotifyEffectivePlacementChanged(this);
        //        if (IsOpen())
        //        {
        //            PositionPopup();
        //        }
        //    }

        //    void OnContentSizeChanged(const IInspectable&, const SizeChangedEventArgs& args)
        //    {
        //        UpdateSizeBasedTemplateSettings();
        //        // Reset the currentEffectivePlacementMode so that the tail will be updated for the new size as well.
        //        m_currentEffectiveTipPlacementMode = TeachingTipPlacementMode.Auto;
        //        TeachingTipTestHooks.NotifyEffectivePlacementChanged(this);
        //        if (IsOpen())
        //        {
        //            PositionPopup();
        //        }
        //        if (auto && expandAnimation = m_expandAnimation.get())
        //        {
        //            expandAnimation.SetScalarParameter(L"Width", args.NewSize().Width);
        //            expandAnimation.SetScalarParameter(L"Height", args.NewSize().Height);
        //        }
        //        if (auto && contractAnimation = m_contractAnimation.get())
        //        {
        //            contractAnimation.SetScalarParameter(L"Width", args.NewSize().Width);
        //            contractAnimation.SetScalarParameter(L"Height", args.NewSize().Height);
        //        }
        //    }

        //    void OnF6AcceleratorKeyClicked(const CoreDispatcher&, const AcceleratorKeyEventArgs& args)
        //    {
        //        if (!args.Handled() &&
        //            IsOpen() &&
        //            args.VirtualKey() == VirtualKey.F6 &&
        //            args.EventType() == CoreAcceleratorKeyEventType.KeyDown)
        //        {
        //            //  Logging usage telemetry
        //            if (m_hasF6BeenInvoked)
        //            {
        //                __RP_Marker_ClassMemberById(RuntimeProfiler.ProfId_TeachingTip, RuntimeProfiler.ProfMemberId_TeachingTip_F6AccessKey_SubsequentInvocation);
        //            }
        //            else
        //            {
        //                __RP_Marker_ClassMemberById(RuntimeProfiler.ProfId_TeachingTip, RuntimeProfiler.ProfMemberId_TeachingTip_F6AccessKey_FirstInvocation);
        //                m_hasF6BeenInvoked = true;
        //            }

        //            auto const hasFocusInSubtree = [this, args]()
        //            {
        //                auto current = FocusManager.GetFocusedElement().try_as<DependencyObject>();
        //                if (auto const rootElement = m_rootElement.get())
        //            {
        //                    while (current)
        //                    {
        //                        if (current.try_as<UIElement>() == rootElement)
        //                        {
        //                            return true;
        //                        }
        //                        current = VisualTreeHelper.GetParent(current);
        //                    }
        //                }
        //                return false;
        //            } ();

        //            if (hasFocusInSubtree)
        //            {
        //                bool setFocus = SetFocus(m_previouslyFocusedElement.get(), FocusState.Programmatic);
        //                m_previouslyFocusedElement = nullptr;
        //                args.Handled(setFocus);
        //            }
        //            else
        //            {
        //                const Button f6Button = [this]().Button
        //                {
        //                    auto firstButton = m_closeButton.get();
        //                    auto secondButton = m_alternateCloseButton.get();
        //                    //Prefer the close button to the alternate, except when there is no content.
        //                    if (!CloseButtonContent())
        //                    {
        //                        std.swap(firstButton, secondButton);
        //                    }
        //                    if (firstButton && firstButton.Visibility() == Visibility.Visible)
        //                    {
        //                        return firstButton;
        //                    }
        //                    else if (secondButton && secondButton.Visibility() == Visibility.Visible)
        //                    {
        //                        return secondButton;
        //                    }
        //                    return nullptr;
        //                } ();

        //                if (f6Button)
        //                {
        //                    auto const scopedRevoker = f6Button.GettingFocus(auto_revoke, [this](auto const&, auto const&args) {
        //                        m_previouslyFocusedElement = make_weak(args.OldFocusedElement());
        //                    });
        //                    const bool setFocus = f6Button.Focus(FocusState.Keyboard);
        //                    args.Handled(setFocus);
        //                }
        //            }
        //        }
        //    }

        //    void OnAutomationNameChanged(const IInspectable&, const IInspectable&)
        //    {
        //        SetPopupAutomationProperties();
        //    }

        //    void OnAutomationIdChanged(const IInspectable&, const IInspectable&)
        //    {
        //        SetPopupAutomationProperties();
        //    }

        //    void OnCloseButtonClicked(const IInspectable&, const RoutedEventArgs&)
        //    {
        //        m_closeButtonClickEventSource(this, nullptr);
        //        m_lastCloseReason = TeachingTipCloseReason.CloseButton;
        //        IsOpen(false);
        //    }

        //    void OnActionButtonClicked(const IInspectable&, const RoutedEventArgs&)
        //    {
        //        m_actionButtonClickEventSource(this, nullptr);
        //    }

        //    void OnPopupOpened(const IInspectable&, const IInspectable&)
        //    {
        //        if (IUIElement10 uiElement10 = this)
        //    {
        //            if (auto xamlRoot = uiElement10.XamlRoot())
        //        {
        //                m_currentXamlRootSize = xamlRoot.Size();
        //                m_xamlRoot.set(xamlRoot);
        //                m_xamlRootChangedRevoker = xamlRoot.Changed(auto_revoke, { this, &XamlRootChanged });
        //            }
        //        }
        //    else
        //        {
        //            if (auto coreWindow = CoreWindow.GetForCurrentThread())
        //        {
        //                m_windowSizeChangedRevoker = coreWindow.SizeChanged(auto_revoke, { this, &WindowSizeChanged });
        //            }
        //        }

        //        // Expand animation requires IUIElement9
        //        if (this.try_as<IUIElement9>() && SharedHelpers.IsAnimationsEnabled())
        //        {
        //            StartExpandToOpen();
        //        }
        //        else
        //        {
        //            // We won't be playing an animation so we're immediately idle.
        //            SetIsIdle(true);
        //        }

        //        if (auto const teachingTipPeer = FrameworkElementAutomationPeer.FromElement(this).try_as<TeachingTipAutomationPeer>())
        //    {
        //            auto const notificationString = [this]()
        //        {
        //                auto const appName = []()
        //            {
        //                    try
        //                    {
        //                        if (const auto package = ApplicationModel.Package.Current())
        //                    {
        //                            return package.DisplayName();
        //                        }
        //                    }
        //                    catch (...) { }

        //                    return hstring{ };
        //                    } ();

        //                    if (!appName.empty())
        //                    {
        //                        return StringUtil.FormatString(
        //                            ResourceAccessor.GetLocalizedStringResource(SR_TeachingTipNotification),
        //                            appName.data(),
        //                            AutomationProperties.GetName(m_popup.get()).data());
        //                    }
        //                    else
        //                    {
        //                        return StringUtil.FormatString(
        //                            ResourceAccessor.GetLocalizedStringResource(SR_TeachingTipNotificationWithoutAppName),
        //                            AutomationProperties.GetName(m_popup.get()).data());
        //                    }
        //                } ();

        //                get_self<TeachingTipAutomationPeer>(teachingTipPeer).RaiseWindowOpenedEvent(notificationString);
        //            }
        //        }

        //        void OnPopupClosed(const IInspectable&, const IInspectable&)
        //{
        //            m_windowSizeChangedRevoker.revoke();
        //            m_xamlRootChangedRevoker.revoke();
        //            m_xamlRoot.set(nullptr);
        //            if (auto && lightDismissIndicatorPopup = m_lightDismissIndicatorPopup.get())
        //            {
        //                lightDismissIndicatorPopup.IsOpen(false);
        //            }
        //            if (auto && popup = m_popup.get())
        //            {
        //                popup.Child(nullptr);
        //            }
        //            auto const myArgs = make_self<TeachingTipClosedEventArgs>();

        //            myArgs.Reason(m_lastCloseReason);
        //            m_closedEventSource(this, *myArgs);

        //            //If we were closed by the close button and we have tracked a previously focused element because F6 was used
        //            //To give the tip focus, then we return focus when the popup closes.
        //            if (m_lastCloseReason == TeachingTipCloseReason.CloseButton)
        //            {
        //                SetFocus(m_previouslyFocusedElement.get(), FocusState.Programmatic);
        //            }
        //            m_previouslyFocusedElement = nullptr;

        //            if (auto const teachingTipPeer = FrameworkElementAutomationPeer.FromElement(this).try_as<TeachingTipAutomationPeer>())
        //    {
        //                get_self<TeachingTipAutomationPeer>(teachingTipPeer).RaiseWindowClosedEvent();
        //            }
        //        }

        //        void ClosePopupOnUnloadEvent(IInspectable const&, RoutedEventArgs const&)
        //{
        //            IsOpen(false);
        //            ClosePopup();
        //        }

        //        void OnLightDismissIndicatorPopupClosed(const IInspectable&, const IInspectable&)
        //{
        //            if (IsOpen())
        //            {
        //                m_lastCloseReason = TeachingTipCloseReason.LightDismiss;
        //            }
        //            IsOpen(false);
        //        }

        //        void RaiseClosingEvent(bool attachDeferralCompletedHandler)
        //    {
        //            auto const args = make_self<TeachingTipClosingEventArgs>();
        //            args.Reason(m_lastCloseReason);

        //            if (attachDeferralCompletedHandler)
        //            {
        //                Deferral instance{
        //                    [strongThis = get_strong(), args]()
        //                    {
        //                        strongThis.CheckThread();
        //                        if (!args.Cancel())
        //                        {
        //                            strongThis.ClosePopupWithAnimationIfAvailable();
        //                        }
        //                        else
        //                        {
        //                            // The developer has changed the Cancel property to true, indicating that they wish to Cancel the
        //                            // closing of this tip, so we need to revert the IsOpen property to true.
        //                            strongThis.IsOpen(true);
        //                        }
        //                    }
        //                };

        //                args.SetDeferral(instance);

        //                args.IncrementDeferralCount();
        //                m_closingEventSource(this, *args);
        //                args.DecrementDeferralCount();
        //            }
        //            else
        //            {
        //                m_closingEventSource(this, *args);
        //            }
        //        }

        //        void ClosePopupWithAnimationIfAvailable()
        //    {
        //            if (m_popup && m_popup.get().IsOpen())
        //            {
        //                // Contract animation requires IUIElement9
        //                if (this.try_as<IUIElement9>() && SharedHelpers.IsAnimationsEnabled())
        //                {
        //                    StartContractToClose();
        //                }
        //                else
        //                {
        //                    ClosePopup();
        //                }

        //                // Under normal circumstances we would have launched an animation just now, if we did not then we should make sure
        //                // that the idle state is correct.
        //                if (!m_isContractAnimationPlaying && !m_isExpandAnimationPlaying)
        //                {
        //                    SetIsIdle(true);
        //                }
        //            }
        //        }

        //        void ClosePopup()
        //    {
        //            if (auto && popup = m_popup.get())
        //            {
        //                popup.IsOpen(false);
        //            }
        //            if (auto && lightDismissIndicatorPopup = m_lightDismissIndicatorPopup.get())
        //            {
        //                lightDismissIndicatorPopup.IsOpen(false);
        //            }
        //            if (IUIElement9 const tailOcclusionGrid = m_tailOcclusionGrid.get())
        //    {
        //                // A previous close animation may have left the rootGrid's scale at a very small value and if this teaching tip
        //                // is shown again then its text would be rasterized at this small scale and blown up ~20x. To fix this we have to
        //                // reset the scale after the popup has closed so that if the teaching tip is re-shown the render pass does not use the
        //                // small scale.
        //                tailOcclusionGrid.Scale({ 1.0f,1.0f,1.0f });
        //            }
        //        }

        //        TeachingTipPlacementMode GetFlowDirectionAdjustedPlacement(const TeachingTipPlacementMode&placementMode)
        //{
        //            if (FlowDirection() == FlowDirection.LeftToRight)
        //            {
        //                return placementMode;
        //            }
        //            else
        //            {
        //                switch (placementMode)
        //                {
        //                    case TeachingTipPlacementMode.Auto:
        //                        return TeachingTipPlacementMode.Auto;
        //                    case TeachingTipPlacementMode.Left:
        //                        return TeachingTipPlacementMode.Right;
        //                    case TeachingTipPlacementMode.Right:
        //                        return TeachingTipPlacementMode.Left;
        //                    case TeachingTipPlacementMode.Top:
        //                        return TeachingTipPlacementMode.Top;
        //                    case TeachingTipPlacementMode.Bottom:
        //                        return TeachingTipPlacementMode.Bottom;
        //                    case TeachingTipPlacementMode.LeftBottom:
        //                        return TeachingTipPlacementMode.RightBottom;
        //                    case TeachingTipPlacementMode.LeftTop:
        //                        return TeachingTipPlacementMode.RightTop;
        //                    case TeachingTipPlacementMode.TopLeft:
        //                        return TeachingTipPlacementMode.TopRight;
        //                    case TeachingTipPlacementMode.TopRight:
        //                        return TeachingTipPlacementMode.TopLeft;
        //                    case TeachingTipPlacementMode.RightTop:
        //                        return TeachingTipPlacementMode.LeftTop;
        //                    case TeachingTipPlacementMode.RightBottom:
        //                        return TeachingTipPlacementMode.LeftBottom;
        //                    case TeachingTipPlacementMode.BottomRight:
        //                        return TeachingTipPlacementMode.BottomLeft;
        //                    case TeachingTipPlacementMode.BottomLeft:
        //                        return TeachingTipPlacementMode.BottomRight;
        //                    case TeachingTipPlacementMode.Center:
        //                        return TeachingTipPlacementMode.Center;
        //                }
        //            }
        //            return TeachingTipPlacementMode.Auto;
        //        }

        //        void OnTargetChanged()
        //    {
        //            m_targetLayoutUpdatedRevoker.revoke();
        //            m_targetEffectiveViewportChangedRevoker.revoke();
        //            m_targetLoadedRevoker.revoke();

        //            auto const target = Target();
        //            m_target.set(target);

        //            if (target)
        //            {
        //                m_targetLoadedRevoker = target.Loaded(auto_revoke, { this, &OnTargetLoaded });
        //            }

        //            if (IsOpen())
        //            {
        //                if (target)
        //                {
        //                    m_currentTargetBoundsInCoreWindowSpace = target.TransformToVisual(nullptr).TransformBounds({
        //                        0.0,
        //                0.0,
        //                static_cast<float>(target.ActualWidth()),
        //                static_cast<float>(target.ActualHeight())
        //                    });
        //                    SetViewportChangedEvent(gsl.make_strict_not_null(target));
        //                }
        //                PositionPopup();
        //            }
        //        }

        //        void SetViewportChangedEvent(const gsl.strict_not_null<FrameworkElement>&target)
        //{
        //            if (m_tipFollowsTarget)
        //            {
        //                // EffectiveViewPortChanged is only available on RS5 and higher.
        //                if (IFrameworkElement7 targetAsFE7 = target.get())
        //        {
        //                    m_targetEffectiveViewportChangedRevoker = targetAsFE7.EffectiveViewportChanged(auto_revoke, { this, &OnTargetLayoutUpdated });
        //                    m_effectiveViewportChangedRevoker = this.EffectiveViewportChanged(auto_revoke, { this, &OnTargetLayoutUpdated });
        //                }
        //        else
        //                {
        //                    m_targetLayoutUpdatedRevoker = target.get().LayoutUpdated(auto_revoke, { this, &OnTargetLayoutUpdated });
        //                }
        //            }
        //        }

        //        void RevokeViewportChangedEvent()
        //    {
        //            m_targetEffectiveViewportChangedRevoker.revoke();
        //            m_effectiveViewportChangedRevoker.revoke();
        //            m_targetLayoutUpdatedRevoker.revoke();
        //        }

        //        void WindowSizeChanged(const CoreWindow&, const WindowSizeChangedEventArgs&)
        //{
        //            // Reposition popup when target/window has finished determining sizes
        //            SharedHelpers.QueueCallbackForCompositionRendering(

        //                [strongThis = get_strong()](){
        //                strongThis.RepositionPopup();
        //            }
        //    );
        //        }

        //        void XamlRootChanged(const XamlRoot&xamlRoot, const XamlRootChangedEventArgs&)
        //{
        //            // Reposition popup when target has finished determining its own position.
        //            SharedHelpers.QueueCallbackForCompositionRendering(

        //                [strongThis = get_strong(), xamlRootSize = xamlRoot.Size()](){
        //                if (xamlRootSize != strongThis.m_currentXamlRootSize)
        //                {
        //                    strongThis.m_currentXamlRootSize = xamlRootSize;
        //                    strongThis.RepositionPopup();
        //                }
        //            }
        //    );

        //        }

        //        void RepositionPopup()
        //    {
        //            if (IsOpen())
        //            {
        //                auto const newTargetBounds = [this]()
        //                {
        //                    if (auto && target = m_target.get())
        //                    {
        //                        return target.TransformToVisual(nullptr).TransformBounds({
        //                            0.0,
        //                    0.0,
        //                    static_cast<float>(target.as< FrameworkElement > ().ActualWidth()),
        //                    static_cast<float>(target.as< FrameworkElement > ().ActualHeight())
        //                            });
        //                    }
        //                    return Rect{ };
        //                } ();

        //                auto const newCurrentBounds = this.TransformToVisual(nullptr).TransformBounds({
        //                    0.0,
        //            0.0,
        //            static_cast<float>(this.ActualWidth()),
        //            static_cast<float>(this.ActualHeight())
        //                    });

        //                if (newTargetBounds != m_currentTargetBoundsInCoreWindowSpace || newCurrentBounds != m_currentBoundsInCoreWindowSpace)
        //                {
        //                    m_currentBoundsInCoreWindowSpace = newCurrentBounds;
        //                    m_currentTargetBoundsInCoreWindowSpace = newTargetBounds;
        //                    PositionPopup();
        //                }
        //            }
        //        }

        //        void OnTargetLoaded(const IInspectable&, const IInspectable&)
        //{
        //            RepositionPopup();
        //        }

        //        void OnTargetLayoutUpdated(const IInspectable&, const IInspectable&)
        //{
        //            RepositionPopup();
        //        }

        //        void CreateExpandAnimation()
        //    {
        //            auto const compositor = Window.Current().Compositor();

        //            auto && expandEasingFunction = [this, compositor]()
        //        {
        //                if (!m_expandEasingFunction)
        //                {
        //                    auto const easingFunction = compositor.CreateCubicBezierEasingFunction(s_expandAnimationEasingCurveControlPoint1, s_expandAnimationEasingCurveControlPoint2);
        //                    m_expandEasingFunction.set(easingFunction);
        //                    return static_cast<CompositionEasingFunction>(easingFunction);
        //                }
        //                return m_expandEasingFunction.get();
        //            } ();

        //            m_expandAnimation.set([this, compositor, expandEasingFunction]()
        //        {
        //                auto const expandAnimation = compositor.CreateVector3KeyFrameAnimation();
        //                if (auto && tailOcclusionGrid = m_tailOcclusionGrid.get())
        //                {
        //                    expandAnimation.SetScalarParameter(L"Width", static_cast<float>(tailOcclusionGrid.ActualWidth()));
        //                    expandAnimation.SetScalarParameter(L"Height", static_cast<float>(tailOcclusionGrid.ActualHeight()));
        //                }
        //                else
        //                {
        //                    expandAnimation.SetScalarParameter(L"Width", s_defaultTipHeightAndWidth);
        //                    expandAnimation.SetScalarParameter(L"Height", s_defaultTipHeightAndWidth);
        //                }

        //                expandAnimation.InsertExpressionKeyFrame(0.0f, L"Vector3(Min(0.01, 20.0 / Width), Min(0.01, 20.0 / Height), 1.0)");
        //                expandAnimation.InsertKeyFrame(1.0f, { 1.0f, 1.0f, 1.0f }, expandEasingFunction);
        //                expandAnimation.Duration(m_expandAnimationDuration);
        //                expandAnimation.Target(s_scaleTargetName);
        //                return expandAnimation;
        //            } ());

        //            m_expandElevationAnimation.set([this, compositor, expandEasingFunction]()
        //        {
        //                auto const expandElevationAnimation = compositor.CreateVector3KeyFrameAnimation();
        //                expandElevationAnimation.InsertExpressionKeyFrame(1.0f, L"Vector3(this.Target.Translation.X, this.Target.Translation.Y, contentElevation)", expandEasingFunction);
        //                expandElevationAnimation.SetScalarParameter(L"contentElevation", m_contentElevation);
        //                expandElevationAnimation.Duration(m_expandAnimationDuration);
        //                expandElevationAnimation.Target(s_translationTargetName);
        //                return expandElevationAnimation;
        //            } ());
        //        }

        //        void CreateContractAnimation()
        //    {
        //            auto const compositor = Window.Current().Compositor();

        //            auto && contractEasingFunction = [this, compositor]()
        //        {
        //                if (!m_contractEasingFunction)
        //                {
        //                    auto const easingFunction = compositor.CreateCubicBezierEasingFunction(s_contractAnimationEasingCurveControlPoint1, s_contractAnimationEasingCurveControlPoint2);
        //                    m_contractEasingFunction.set(easingFunction);
        //                    return static_cast<CompositionEasingFunction>(easingFunction);
        //                }
        //                return m_contractEasingFunction.get();
        //            } ();

        //            m_contractAnimation.set([this, compositor, contractEasingFunction]()
        //        {
        //                auto const contractAnimation = compositor.CreateVector3KeyFrameAnimation();
        //                if (auto && tailOcclusionGrid = m_tailOcclusionGrid.get())
        //                {
        //                    contractAnimation.SetScalarParameter(L"Width", static_cast<float>(tailOcclusionGrid.ActualWidth()));
        //                    contractAnimation.SetScalarParameter(L"Height", static_cast<float>(tailOcclusionGrid.ActualHeight()));
        //                }
        //                else
        //                {
        //                    contractAnimation.SetScalarParameter(L"Width", s_defaultTipHeightAndWidth);
        //                    contractAnimation.SetScalarParameter(L"Height", s_defaultTipHeightAndWidth);
        //                }

        //                contractAnimation.InsertKeyFrame(0.0f, { 1.0f, 1.0f, 1.0f });
        //                contractAnimation.InsertExpressionKeyFrame(1.0f, L"Vector3(20.0 / Width, 20.0 / Height, 1.0)", contractEasingFunction);
        //                contractAnimation.Duration(m_contractAnimationDuration);
        //                contractAnimation.Target(s_scaleTargetName);
        //                return contractAnimation;
        //            } ());

        //            m_contractElevationAnimation.set([this, compositor, contractEasingFunction]()
        //        {
        //                auto const contractElevationAnimation = compositor.CreateVector3KeyFrameAnimation();
        //                // animating to 0.01f instead of 0.0f as work around to internal issue 26001712 which was causing text clipping.
        //                contractElevationAnimation.InsertExpressionKeyFrame(1.0f, L"Vector3(this.Target.Translation.X, this.Target.Translation.Y, 0.01f)", contractEasingFunction);
        //                contractElevationAnimation.Duration(m_contractAnimationDuration);
        //                contractElevationAnimation.Target(s_translationTargetName);
        //                return contractElevationAnimation;
        //            } ());
        //        }

        //        void StartExpandToOpen()
        //    {
        //            MUX_ASSERT_MSG(this.try_as<IUIElement9>(), "The contract and expand animations currently use facade's which were not available pre-RS5.");
        //            if (!m_expandAnimation)
        //            {
        //                CreateExpandAnimation();
        //            }

        //            auto const scopedBatch = [this]()
        //        {
        //                auto const scopedBatch = Window.Current().Compositor().CreateScopedBatch(CompositionBatchTypes.Animation);

        //                if (auto && expandAnimation = m_expandAnimation.get())
        //                {
        //                    if (auto && tailOcclusionGrid = m_tailOcclusionGrid.get())
        //                    {
        //                        tailOcclusionGrid.StartAnimation(expandAnimation);
        //                        m_isExpandAnimationPlaying = true;
        //                    }
        //                    if (auto && tailEdgeBorder = m_tailEdgeBorder.get())
        //                    {
        //                        tailEdgeBorder.StartAnimation(expandAnimation);
        //                        m_isExpandAnimationPlaying = true;
        //                    }
        //                }
        //                if (auto && expandElevationAnimation = m_expandElevationAnimation.get())
        //                {
        //                    if (auto && contentRootGrid = m_contentRootGrid.get())
        //                    {
        //                        contentRootGrid.StartAnimation(expandElevationAnimation);
        //                        m_isExpandAnimationPlaying = true;
        //                    }
        //                }
        //                return scopedBatch;
        //            } ();
        //            scopedBatch.End();

        //            scopedBatch.Completed([strongThis = get_strong()](auto, auto)
        //        {
        //                strongThis.m_isExpandAnimationPlaying = false;
        //                if (!strongThis.m_isContractAnimationPlaying)
        //                {
        //                    strongThis.SetIsIdle(true);
        //                }
        //            });

        //            // Under normal circumstances we would have launched an animation just now, if we did not then we should make sure that the idle state is correct
        //            if (!m_isExpandAnimationPlaying && !m_isContractAnimationPlaying)
        //            {
        //                SetIsIdle(true);
        //            }
        //        }

        //        void StartContractToClose()
        //    {
        //            MUX_ASSERT_MSG(this.try_as<IUIElement9>(), "The contract and expand animations currently use facade's which were not available pre RS5.");
        //            if (!m_contractAnimation)
        //            {
        //                CreateContractAnimation();
        //            }

        //            auto const scopedBatch = [this]()
        //        {
        //                auto const scopedBatch = Window.Current().Compositor().CreateScopedBatch(CompositionBatchTypes.Animation);
        //                if (auto && contractAnimation = m_contractAnimation.get())
        //                {
        //                    if (auto && tailOcclusionGrid = m_tailOcclusionGrid.get())
        //                    {
        //                        tailOcclusionGrid.StartAnimation(contractAnimation);
        //                        m_isContractAnimationPlaying = true;
        //                    }
        //                    if (auto && tailEdgeBorder = m_tailEdgeBorder.get())
        //                    {
        //                        tailEdgeBorder.StartAnimation(contractAnimation);
        //                        m_isContractAnimationPlaying = true;
        //                    }
        //                }
        //                if (auto && contractElevationAnimation = m_contractElevationAnimation.get())
        //                {
        //                    if (auto && contentRootGrid = m_contentRootGrid.get())
        //                    {
        //                        contentRootGrid.StartAnimation(contractElevationAnimation);
        //                        m_isContractAnimationPlaying = true;
        //                    }
        //                }
        //                return scopedBatch;
        //            } ();
        //            scopedBatch.End();

        //            scopedBatch.Completed([strongThis = get_strong()](auto, auto)
        //        {
        //                strongThis.m_isContractAnimationPlaying = false;
        //                strongThis.ClosePopup();
        //                if (!strongThis.m_isExpandAnimationPlaying)
        //                {
        //                    strongThis.SetIsIdle(true);
        //                }
        //            });
        //        }

        //        std.tuple<TeachingTipPlacementMode, bool> DetermineEffectivePlacement()
        //    {
        //            // Because we do not have access to APIs to give us details about multi monitor scenarios we do not have the ability to correctly
        //            // Place the tip in scenarios where we have an out of root bounds tip. Since this is the case we have decided to do no special
        //            // calculations and return the provided value or top if auto was set. This behavior can be removed via the
        //            // SetReturnTopForOutOfWindowBounds test hook.
        //            if (!ShouldConstrainToRootBounds() && m_returnTopForOutOfWindowPlacement)
        //            {
        //                auto const placement = GetFlowDirectionAdjustedPlacement(PreferredPlacement());
        //                if (placement == TeachingTipPlacementMode.Auto)
        //                {
        //                    return std.make_tuple(TeachingTipPlacementMode.Top, false);
        //                }
        //                return std.make_tuple(placement, false);
        //            }

        //            if (IsOpen() && m_currentEffectiveTipPlacementMode != TeachingTipPlacementMode.Auto)
        //            {
        //                return std.make_tuple(m_currentEffectiveTipPlacementMode, false);
        //            }

        //            auto const [contentHeight, contentWidth] = [this]()
        //        {
        //                if (auto && tailOcclusionGrid = m_tailOcclusionGrid.get())
        //                {
        //                    double contentHeight = tailOcclusionGrid.ActualHeight();
        //                    double contentWidth = tailOcclusionGrid.ActualWidth();
        //                    return std.make_tuple(contentHeight, contentWidth);
        //                }
        //                return std.make_tuple(0.0, 0.0);
        //            } ();

        //            if (m_target)
        //            {
        //                return DetermineEffectivePlacementTargeted(contentHeight, contentWidth);
        //            }
        //            else
        //            {
        //                return DetermineEffectivePlacementUntargeted(contentHeight, contentWidth);
        //            }
        //        }

        //        std.tuple<TeachingTipPlacementMode, bool> DetermineEffectivePlacementTargeted(double contentHeight, double contentWidth)
        //    {
        //            // These variables will track which positions the tip will fit in. They all start true and are
        //            // flipped to false when we find a display condition that is not met.
        //            enum_array < TeachingTipPlacementMode, bool, 14 > availability;
        //            availability[TeachingTipPlacementMode.Auto] = false;
        //            availability[TeachingTipPlacementMode.Top] = true;
        //            availability[TeachingTipPlacementMode.Bottom] = true;
        //            availability[TeachingTipPlacementMode.Right] = true;
        //            availability[TeachingTipPlacementMode.Left] = true;
        //            availability[TeachingTipPlacementMode.TopLeft] = true;
        //            availability[TeachingTipPlacementMode.TopRight] = true;
        //            availability[TeachingTipPlacementMode.BottomLeft] = true;
        //            availability[TeachingTipPlacementMode.BottomRight] = true;
        //            availability[TeachingTipPlacementMode.LeftTop] = true;
        //            availability[TeachingTipPlacementMode.LeftBottom] = true;
        //            availability[TeachingTipPlacementMode.RightTop] = true;
        //            availability[TeachingTipPlacementMode.RightBottom] = true;
        //            availability[TeachingTipPlacementMode.Center] = true;

        //            const double tipHeight = contentHeight + TailShortSideLength();
        //            const double tipWidth = contentWidth + TailShortSideLength();

        //            // We try to avoid having the tail touch the HeroContent so rule out positions where this would be required
        //            if (HeroContent())
        //            {
        //                if (auto && heroContentBorder = m_heroContentBorder.get())
        //                {
        //                    if (auto && nonHeroContentRootGrid = m_nonHeroContentRootGrid.get())
        //                    {
        //                        if (heroContentBorder.ActualHeight() > nonHeroContentRootGrid.ActualHeight() - TailLongSideActualLength())
        //                        {
        //                            availability[TeachingTipPlacementMode.Left] = false;
        //                            availability[TeachingTipPlacementMode.Right] = false;
        //                        }
        //                    }
        //                }

        //                switch (HeroContentPlacement())
        //                {
        //                    case TeachingTipHeroContentPlacementMode.Bottom:
        //                        availability[TeachingTipPlacementMode.Top] = false;
        //                        availability[TeachingTipPlacementMode.TopRight] = false;
        //                        availability[TeachingTipPlacementMode.TopLeft] = false;
        //                        availability[TeachingTipPlacementMode.RightTop] = false;
        //                        availability[TeachingTipPlacementMode.LeftTop] = false;
        //                        availability[TeachingTipPlacementMode.Center] = false;
        //                        break;
        //                    case TeachingTipHeroContentPlacementMode.Top:
        //                        availability[TeachingTipPlacementMode.Bottom] = false;
        //                        availability[TeachingTipPlacementMode.BottomLeft] = false;
        //                        availability[TeachingTipPlacementMode.BottomRight] = false;
        //                        availability[TeachingTipPlacementMode.RightBottom] = false;
        //                        availability[TeachingTipPlacementMode.LeftBottom] = false;
        //                        break;
        //                }
        //            }

        //            // When ShouldConstrainToRootBounds is true clippedTargetBounds == availableBoundsAroundTarget
        //            // We have to separate them because there are checks which care about both.
        //            auto const [clippedTargetBounds, availableBoundsAroundTarget] = DetermineSpaceAroundTarget();

        //            // If the edge of the target isn't in the window.
        //            if (clippedTargetBounds.Left < 0)
        //            {
        //                availability[TeachingTipPlacementMode.LeftBottom] = false;
        //                availability[TeachingTipPlacementMode.Left] = false;
        //                availability[TeachingTipPlacementMode.LeftTop] = false;
        //            }
        //            // If the right edge of the target isn't in the window.
        //            if (clippedTargetBounds.Right < 0)
        //            {
        //                availability[TeachingTipPlacementMode.RightBottom] = false;
        //                availability[TeachingTipPlacementMode.Right] = false;
        //                availability[TeachingTipPlacementMode.RightTop] = false;
        //            }
        //            // If the top edge of the target isn't in the window.
        //            if (clippedTargetBounds.Top < 0)
        //            {
        //                availability[TeachingTipPlacementMode.TopLeft] = false;
        //                availability[TeachingTipPlacementMode.Top] = false;
        //                availability[TeachingTipPlacementMode.TopRight] = false;
        //            }
        //            // If the bottom edge of the target isn't in the window
        //            if (clippedTargetBounds.Bottom < 0)
        //            {
        //                availability[TeachingTipPlacementMode.BottomLeft] = false;
        //                availability[TeachingTipPlacementMode.Bottom] = false;
        //                availability[TeachingTipPlacementMode.BottomRight] = false;
        //            }

        //            // If the horizontal midpoint is out of the window.
        //            if (clippedTargetBounds.Left < -m_currentTargetBoundsInCoreWindowSpace.Width / 2 ||
        //                clippedTargetBounds.Right < -m_currentTargetBoundsInCoreWindowSpace.Width / 2)
        //            {
        //                availability[TeachingTipPlacementMode.TopLeft] = false;
        //                availability[TeachingTipPlacementMode.Top] = false;
        //                availability[TeachingTipPlacementMode.TopRight] = false;
        //                availability[TeachingTipPlacementMode.BottomLeft] = false;
        //                availability[TeachingTipPlacementMode.Bottom] = false;
        //                availability[TeachingTipPlacementMode.BottomRight] = false;
        //                availability[TeachingTipPlacementMode.Center] = false;
        //            }

        //            // If the vertical midpoint is out of the window.
        //            if (clippedTargetBounds.Top < -m_currentTargetBoundsInCoreWindowSpace.Height / 2 ||
        //                clippedTargetBounds.Bottom < -m_currentTargetBoundsInCoreWindowSpace.Height / 2)
        //            {
        //                availability[TeachingTipPlacementMode.LeftBottom] = false;
        //                availability[TeachingTipPlacementMode.Left] = false;
        //                availability[TeachingTipPlacementMode.LeftTop] = false;
        //                availability[TeachingTipPlacementMode.RightBottom] = false;
        //                availability[TeachingTipPlacementMode.Right] = false;
        //                availability[TeachingTipPlacementMode.RightTop] = false;
        //                availability[TeachingTipPlacementMode.Center] = false;
        //            }

        //            // If the tip is too tall to fit between the top of the target and the top edge of the window or screen.
        //            if (tipHeight > availableBoundsAroundTarget.Top)
        //            {
        //                availability[TeachingTipPlacementMode.Top] = false;
        //                availability[TeachingTipPlacementMode.TopRight] = false;
        //                availability[TeachingTipPlacementMode.TopLeft] = false;
        //            }
        //            // If the total tip is too tall to fit between the center of the target and the top of the window.
        //            if (tipHeight > availableBoundsAroundTarget.Top + (m_currentTargetBoundsInCoreWindowSpace.Height / 2.0f))
        //            {
        //                availability[TeachingTipPlacementMode.Center] = false;
        //            }
        //            // If the tip is too tall to fit between the center of the target and the top edge of the window.
        //            if (contentHeight - MinimumTipEdgeToTailCenter() > availableBoundsAroundTarget.Top + (m_currentTargetBoundsInCoreWindowSpace.Height / 2.0f))
        //            {
        //                availability[TeachingTipPlacementMode.RightTop] = false;
        //                availability[TeachingTipPlacementMode.LeftTop] = false;
        //            }
        //            // If the tip is too tall to fit in the window when the tail is centered vertically on the target and the tip.
        //            if (contentHeight / 2.0f > availableBoundsAroundTarget.Top + (m_currentTargetBoundsInCoreWindowSpace.Height / 2.0f) ||
        //                contentHeight / 2.0f > availableBoundsAroundTarget.Bottom + (m_currentTargetBoundsInCoreWindowSpace.Height / 2.0f))
        //            {
        //                availability[TeachingTipPlacementMode.Right] = false;
        //                availability[TeachingTipPlacementMode.Left] = false;
        //            }
        //            // If the tip is too tall to fit between the center of the target and the bottom edge of the window.
        //            if (contentHeight - MinimumTipEdgeToTailCenter() > availableBoundsAroundTarget.Bottom + (m_currentTargetBoundsInCoreWindowSpace.Height / 2.0f))
        //            {
        //                availability[TeachingTipPlacementMode.RightBottom] = false;
        //                availability[TeachingTipPlacementMode.LeftBottom] = false;
        //            }
        //            // If the tip is too tall to fit between the bottom of the target and the bottom edge of the window.
        //            if (tipHeight > availableBoundsAroundTarget.Bottom)
        //            {
        //                availability[TeachingTipPlacementMode.Bottom] = false;
        //                availability[TeachingTipPlacementMode.BottomLeft] = false;
        //                availability[TeachingTipPlacementMode.BottomRight] = false;
        //            }

        //            // If the tip is too wide to fit between the left edge of the target and the left edge of the window.
        //            if (tipWidth > availableBoundsAroundTarget.Left)
        //            {
        //                availability[TeachingTipPlacementMode.Left] = false;
        //                availability[TeachingTipPlacementMode.LeftTop] = false;
        //                availability[TeachingTipPlacementMode.LeftBottom] = false;
        //            }
        //            // If the tip is too wide to fit between the center of the target and the left edge of the window.
        //            if (contentWidth - MinimumTipEdgeToTailCenter() > availableBoundsAroundTarget.Left + (m_currentTargetBoundsInCoreWindowSpace.Width / 2.0f))
        //            {
        //                availability[TeachingTipPlacementMode.TopLeft] = false;
        //                availability[TeachingTipPlacementMode.BottomLeft] = false;
        //            }
        //            // If the tip is too wide to fit in the window when the tail is centered horizontally on the target and the tip.
        //            if (contentWidth / 2.0f > availableBoundsAroundTarget.Left + (m_currentTargetBoundsInCoreWindowSpace.Width / 2.0f) ||
        //                contentWidth / 2.0f > availableBoundsAroundTarget.Right + (m_currentTargetBoundsInCoreWindowSpace.Width / 2.0f))
        //            {
        //                availability[TeachingTipPlacementMode.Top] = false;
        //                availability[TeachingTipPlacementMode.Bottom] = false;
        //                availability[TeachingTipPlacementMode.Center] = false;
        //            }
        //            // If the tip is too wide to fit between the center of the target and the right edge of the window.
        //            if (contentWidth - MinimumTipEdgeToTailCenter() > availableBoundsAroundTarget.Right + (m_currentTargetBoundsInCoreWindowSpace.Width / 2.0f))
        //            {
        //                availability[TeachingTipPlacementMode.TopRight] = false;
        //                availability[TeachingTipPlacementMode.BottomRight] = false;
        //            }
        //            // If the tip is too wide to fit between the right edge of the target and the right edge of the window.
        //            if (tipWidth > availableBoundsAroundTarget.Right)
        //            {
        //                availability[TeachingTipPlacementMode.Right] = false;
        //                availability[TeachingTipPlacementMode.RightTop] = false;
        //                availability[TeachingTipPlacementMode.RightBottom] = false;
        //            }

        //            auto const wantedDirection = GetFlowDirectionAdjustedPlacement(PreferredPlacement());
        //            auto const priorities = GetPlacementFallbackOrder(wantedDirection);

        //            for (auto const mode : priorities)
        //    {
        //                if (availability[mode])
        //                {
        //                    return std.make_tuple(mode, false);
        //                }
        //            }
        //            // The teaching tip wont fit anywhere, set tipDoesNotFit to indicate that we should not open.
        //            return std.make_tuple(TeachingTipPlacementMode.Top, true);
        //        }

        //        std.tuple<TeachingTipPlacementMode, bool> DetermineEffectivePlacementUntargeted(double contentHeight, double contentWidth)
        //    {
        //            auto const windowBounds = GetWindowBounds();
        //            if (!ShouldConstrainToRootBounds())
        //            {
        //                auto const screenBoundsInCoreWindowSpace = GetEffectiveScreenBoundsInCoreWindowSpace(windowBounds);
        //                if (screenBoundsInCoreWindowSpace.Height > contentHeight && screenBoundsInCoreWindowSpace.Width > contentWidth)
        //                {
        //                    return std.make_tuple(TeachingTipPlacementMode.Bottom, false);
        //                }
        //            }
        //            else
        //            {
        //                auto const windowBoundsInCoreWindowSpace = GetEffectiveWindowBoundsInCoreWindowSpace(windowBounds);
        //                if (windowBoundsInCoreWindowSpace.Height > contentHeight && windowBoundsInCoreWindowSpace.Width > contentWidth)
        //                {
        //                    return std.make_tuple(TeachingTipPlacementMode.Bottom, false);
        //                }
        //            }

        //            // The teaching tip doesn't fit in the window/screen set tipDoesNotFit to indicate that we should not open.
        //            return std.make_tuple(TeachingTipPlacementMode.Top, true);
        //        }

        //        std.tuple<Thickness, Thickness> DetermineSpaceAroundTarget()
        //    {
        //            auto const shouldConstrainToRootBounds = ShouldConstrainToRootBounds();

        //            auto const [windowBoundsInCoreWindowSpace, screenBoundsInCoreWindowSpace] = [this]()
        //        {
        //                auto const windowBounds = GetWindowBounds();
        //                return std.make_tuple(GetEffectiveWindowBoundsInCoreWindowSpace(windowBounds),
        //                                       GetEffectiveScreenBoundsInCoreWindowSpace(windowBounds));
        //            } ();


        //            const Thickness windowSpaceAroundTarget{
        //                // Target.Left - Window.Left
        //                m_currentTargetBoundsInCoreWindowSpace.X - /* 0 except with test window bounds */ windowBoundsInCoreWindowSpace.X,
        //        // Target.Top - Window.Top
        //        m_currentTargetBoundsInCoreWindowSpace.Y - /* 0 except with test window bounds */ windowBoundsInCoreWindowSpace.Y,
        //        // Window.Right - Target.Right
        //        (windowBoundsInCoreWindowSpace.X + windowBoundsInCoreWindowSpace.Width) - (m_currentTargetBoundsInCoreWindowSpace.X + m_currentTargetBoundsInCoreWindowSpace.Width),
        //        // Screen.Right - Target.Right
        //        (windowBoundsInCoreWindowSpace.Y + windowBoundsInCoreWindowSpace.Height) - (m_currentTargetBoundsInCoreWindowSpace.Y + m_currentTargetBoundsInCoreWindowSpace.Height) };


        //            const Thickness screenSpaceAroundTarget = [this, screenBoundsInCoreWindowSpace, windowSpaceAroundTarget]()
        //        {
        //                if (!ShouldConstrainToRootBounds())
        //                {
        //                    return Thickness{
        //                        // Target.Left - Screen.Left
        //                        m_currentTargetBoundsInCoreWindowSpace.X - screenBoundsInCoreWindowSpace.X,
        //                // Target.Top - Screen.Top
        //                m_currentTargetBoundsInCoreWindowSpace.Y - screenBoundsInCoreWindowSpace.Y,
        //                // Screen.Right - Target.Right
        //                (screenBoundsInCoreWindowSpace.X + screenBoundsInCoreWindowSpace.Width) - (m_currentTargetBoundsInCoreWindowSpace.X + m_currentTargetBoundsInCoreWindowSpace.Width),
        //                // Screen.Bottom - Target.Bottom
        //                (screenBoundsInCoreWindowSpace.Y + screenBoundsInCoreWindowSpace.Height) - (m_currentTargetBoundsInCoreWindowSpace.Y + m_currentTargetBoundsInCoreWindowSpace.Height) };
        //                }
        //                return windowSpaceAroundTarget;
        //            } ();

        //            return std.make_tuple(windowSpaceAroundTarget, screenSpaceAroundTarget);
        //        }

        //        Rect GetEffectiveWindowBoundsInCoreWindowSpace(const Rect&windowBounds)
        //{
        //            if (m_useTestWindowBounds)
        //            {
        //                return m_testWindowBoundsInCoreWindowSpace;
        //            }
        //            else
        //            {
        //                return Rect{ 0, 0, windowBounds.Width, windowBounds.Height };
        //            }

        //        }

        //        Rect GetEffectiveScreenBoundsInCoreWindowSpace(const Rect&windowBounds)
        //{
        //            if (!m_useTestScreenBounds && !ShouldConstrainToRootBounds())
        //            {
        //                MUX_ASSERT_MSG(!m_returnTopForOutOfWindowPlacement, "When returnTopForOutOfWindowPlacement is true we will never need to get the screen bounds");
        //                auto const displayInfo = DisplayInformation.GetForCurrentView();
        //                auto const scaleFactor = displayInfo.RawPixelsPerViewPixel();
        //                return Rect(-windowBounds.X,
        //                    -windowBounds.Y,
        //                    displayInfo.ScreenHeightInRawPixels() / static_cast<float>(scaleFactor),
        //                    displayInfo.ScreenWidthInRawPixels() / static_cast<float>(scaleFactor));
        //            }
        //            return m_testScreenBoundsInCoreWindowSpace;
        //        }

        //        Rect GetWindowBounds()
        //    {
        //            if (IUIElement10 uiElement10 = this)
        //    {
        //                if (auto const xamlRoot = uiElement10.XamlRoot())
        //        {
        //                    return Rect{ 0, 0, xamlRoot.Size().Width, xamlRoot.Size().Height };
        //                }
        //            }
        //            return Window.Current().CoreWindow().Bounds();
        //        }

        //        std.array < TeachingTipPlacementMode, 13 > GetPlacementFallbackOrder(TeachingTipPlacementMode preferredPlacement)
        //    {
        //            auto priorityList = std.array < TeachingTipPlacementMode, 13 > ();
        //            priorityList[0] = TeachingTipPlacementMode.Top;
        //            priorityList[1] = TeachingTipPlacementMode.Bottom;
        //            priorityList[2] = TeachingTipPlacementMode.Left;
        //            priorityList[3] = TeachingTipPlacementMode.Right;
        //            priorityList[4] = TeachingTipPlacementMode.TopLeft;
        //            priorityList[5] = TeachingTipPlacementMode.TopRight;
        //            priorityList[6] = TeachingTipPlacementMode.BottomLeft;
        //            priorityList[7] = TeachingTipPlacementMode.BottomRight;
        //            priorityList[8] = TeachingTipPlacementMode.LeftTop;
        //            priorityList[9] = TeachingTipPlacementMode.LeftBottom;
        //            priorityList[10] = TeachingTipPlacementMode.RightTop;
        //            priorityList[11] = TeachingTipPlacementMode.RightBottom;
        //            priorityList[12] = TeachingTipPlacementMode.Center;


        //            if (IsPlacementBottom(preferredPlacement))
        //            {
        //                // Swap to bottom > top
        //                std.swap(priorityList[0], priorityList[1]);
        //                std.swap(priorityList[4], priorityList[6]);
        //                std.swap(priorityList[5], priorityList[7]);
        //            }
        //            else if (IsPlacementLeft(preferredPlacement))
        //            {
        //                // swap to lateral > vertical
        //                std.swap(priorityList[0], priorityList[2]);
        //                std.swap(priorityList[1], priorityList[3]);
        //                std.swap(priorityList[4], priorityList[8]);
        //                std.swap(priorityList[5], priorityList[9]);
        //                std.swap(priorityList[6], priorityList[10]);
        //                std.swap(priorityList[7], priorityList[11]);
        //            }
        //            else if (IsPlacementRight(preferredPlacement))
        //            {
        //                // swap to lateral > vertical
        //                std.swap(priorityList[0], priorityList[2]);
        //                std.swap(priorityList[1], priorityList[3]);
        //                std.swap(priorityList[4], priorityList[8]);
        //                std.swap(priorityList[5], priorityList[9]);
        //                std.swap(priorityList[6], priorityList[10]);
        //                std.swap(priorityList[7], priorityList[11]);

        //                // swap to right > left
        //                std.swap(priorityList[0], priorityList[1]);
        //                std.swap(priorityList[4], priorityList[6]);
        //                std.swap(priorityList[5], priorityList[7]);
        //            }

        //            //Switch the preferred placement to first.
        //            auto const pivot = std.find_if(priorityList.begin(),
        //                priorityList.end(),

        //                [preferredPlacement](const TeachingTipPlacementMode mode) . bool {
        //                return mode == preferredPlacement;
        //            });
        //            if (pivot != priorityList.end())
        //            {
        //                std.rotate(priorityList.begin(), pivot, pivot + 1);
        //            }

        //            return priorityList;
        //        }


        //        void EstablishShadows()
        //    {
        //# ifdef TAIL_SHADOW
        //# ifdef _DEBUG
        //            if (IUIElement10 tailPolygon_uiElement10 = m_tailPolygon.get())
        //    {
        //                if (m_tipShadow)
        //                {
        //                    if (!tailPolygon_uiElement10.Shadow())
        //                    {
        //                        // This facilitates an experiment around faking a proper tail shadow, shadows are expensive though so we don't want it present for release builds.
        //                        auto const tailShadow = Windows.UI.Xaml.Media.ThemeShadow{ };
        //                        tailShadow.Receivers().Append(m_target.get());
        //                        tailPolygon_uiElement10.Shadow(tailShadow);
        //                        if (auto && tailPolygon = m_tailPolygon.get())
        //                        {
        //                            auto const tailPolygonTranslation = tailPolygon.Translation()
        //                            tailPolygon.Translation({ tailPolygonTranslation.x, tailPolygonTranslation.y, m_tailElevation });
        //                        }
        //                    }
        //                }
        //                else
        //                {
        //                    tailPolygon_uiElement10.Shadow(nullptr);
        //                }
        //            }
        //#endif
        //#endif
        //            if (IUIElement10 m_contentRootGrid_uiElement10 = m_contentRootGrid.get())
        //    {
        //                if (m_tipShouldHaveShadow)
        //                {
        //                    if (!m_contentRootGrid_uiElement10.Shadow())
        //                    {
        //                        m_contentRootGrid_uiElement10.Shadow(ThemeShadow{ });
        //                        if (auto && contentRootGrid = m_contentRootGrid.get())
        //                        {
        //                            const auto contentRootGridTranslation = contentRootGrid.Translation();
        //                            contentRootGrid.Translation({ contentRootGridTranslation.x, contentRootGridTranslation.y, m_contentElevation });
        //                        }
        //                    }
        //                }
        //                else
        //                {
        //                    m_contentRootGrid_uiElement10.Shadow(nullptr);
        //                }
        //            }
        //        }

        //        void TrySetCenterPoint(const IUIElement9&element, const float3&centerPoint)
        //{
        //            if (element)
        //            {
        //                element.CenterPoint(centerPoint);
        //            }
        //        }

        //        void OnPropertyChanged(
        //        const DependencyObject&sender,
        //    const DependencyPropertyChangedEventArgs&args)
        //{
        //            get_self<TeachingTip>(sender.as< TeachingTip > ()).OnPropertyChanged(args);
        //        }

        //        float TailLongSideActualLength()
        //    {
        //            if (auto && tailPolygon = m_tailPolygon.get())
        //            {
        //                return static_cast<float>(std.max(tailPolygon.ActualHeight(), tailPolygon.ActualWidth()));
        //            }
        //            return 0;
        //        }

        //        float TailLongSideLength()
        //    {
        //            return static_cast<float>(TailLongSideActualLength() - (2 * s_tailOcclusionAmount));
        //        }

        //        float TailShortSideLength()
        //    {
        //            if (auto && tailPolygon = m_tailPolygon.get())
        //            {
        //                return static_cast<float>(std.min(tailPolygon.ActualHeight(), tailPolygon.ActualWidth()) - s_tailOcclusionAmount);
        //            }
        //            return 0;
        //        }

        //        float MinimumTipEdgeToTailEdgeMargin()
        //    {
        //            if (auto && tailOcclusionGrid = m_tailOcclusionGrid.get())
        //            {
        //                return tailOcclusionGrid.ColumnDefinitions().Size() > 1 ?
        //                    static_cast<float>(tailOcclusionGrid.ColumnDefinitions().GetAt(1).ActualWidth() + s_tailOcclusionAmount)
        //                    : 0.0f;
        //            }
        //            return 0;
        //        }

        //        float MinimumTipEdgeToTailCenter()
        //    {
        //            if (auto && tailOcclusionGrid = m_tailOcclusionGrid.get())
        //            {
        //                if (auto && tailPolygon = m_tailPolygon.get())
        //                {
        //                    return tailOcclusionGrid.ColumnDefinitions().Size() > 1 ?
        //                        static_cast<float>(tailOcclusionGrid.ColumnDefinitions().GetAt(0).ActualWidth() +
        //                            tailOcclusionGrid.ColumnDefinitions().GetAt(1).ActualWidth() +
        //                            (std.max(tailPolygon.ActualHeight(), tailPolygon.ActualWidth()) / 2))
        //                        : 0.0f;
        //                }
        //            }
        //            return 0;
        //        }

        //        ////////////////
        //        // Test Hooks //
        //        ////////////////
        //        void SetExpandEasingFunction(const CompositionEasingFunction&easingFunction)
        //{
        //            m_expandEasingFunction.set(easingFunction);
        //            CreateExpandAnimation();
        //        }

        //        void SetContractEasingFunction(const CompositionEasingFunction&easingFunction)
        //{
        //            m_contractEasingFunction.set(easingFunction);
        //            CreateContractAnimation();
        //        }

        //        void SetTipShouldHaveShadow(bool tipShouldHaveShadow)
        //    {
        //            if (m_tipShouldHaveShadow != tipShouldHaveShadow)
        //            {
        //                m_tipShouldHaveShadow = tipShouldHaveShadow;
        //                EstablishShadows();
        //            }
        //        }

        //        void SetContentElevation(float elevation)
        //    {
        //            m_contentElevation = elevation;
        //            if (SharedHelpers.IsRS5OrHigher())
        //            {
        //                if (auto && contentRootGrid = m_contentRootGrid.get())
        //                {
        //                    auto const contentRootGridTranslation = contentRootGrid.Translation();
        //                    m_contentRootGrid.get().Translation({ contentRootGridTranslation.x, contentRootGridTranslation.y, m_contentElevation });
        //                }
        //                if (m_expandElevationAnimation)
        //                {
        //                    m_expandElevationAnimation.get().SetScalarParameter(L"contentElevation", m_contentElevation);
        //                }
        //            }
        //        }

        //        void SetTailElevation(float elevation)
        //    {
        //            m_tailElevation = elevation;
        //            if (SharedHelpers.IsRS5OrHigher() && m_tailPolygon)
        //            {
        //                if (auto && tailPolygon = m_tailPolygon.get())
        //                {
        //                    auto const tailPolygonTranslation = tailPolygon.Translation();
        //                    tailPolygon.Translation({ tailPolygonTranslation.x, tailPolygonTranslation.y, m_tailElevation });
        //                }
        //            }
        //        }

        //        void SetUseTestWindowBounds(bool useTestWindowBounds)
        //    {
        //            m_useTestWindowBounds = useTestWindowBounds;
        //        }

        //        void SetTestWindowBounds(const Rect&testWindowBounds)
        //{
        //            m_testWindowBoundsInCoreWindowSpace = testWindowBounds;
        //        }

        //        void SetUseTestScreenBounds(bool useTestScreenBounds)
        //    {
        //            m_useTestScreenBounds = useTestScreenBounds;
        //        }

        //        void SetTestScreenBounds(const Rect&testScreenBounds)
        //{
        //            m_testScreenBoundsInCoreWindowSpace = testScreenBounds;
        //        }

        //        void SetTipFollowsTarget(bool tipFollowsTarget)
        //    {
        //            if (m_tipFollowsTarget != tipFollowsTarget)
        //            {
        //                m_tipFollowsTarget = tipFollowsTarget;
        //                if (tipFollowsTarget)
        //                {
        //                    if (auto && target = m_target.get())
        //                    {
        //                        SetViewportChangedEvent(gsl.make_strict_not_null(target));
        //                    }
        //                }
        //                else
        //                {
        //                    RevokeViewportChangedEvent();
        //                }
        //            }
        //        }

        //        void SetReturnTopForOutOfWindowPlacement(bool returnTopForOutOfWindowPlacement)
        //    {
        //            m_returnTopForOutOfWindowPlacement = returnTopForOutOfWindowPlacement;
        //        }

        //        void SetExpandAnimationDuration(const TimeSpan&expandAnimationDuration)
        //{
        //            m_expandAnimationDuration = expandAnimationDuration;
        //            if (auto && expandAnimation = m_expandAnimation.get())
        //            {
        //                expandAnimation.Duration(m_expandAnimationDuration);
        //            }
        //            if (auto && expandElevationAnimation = m_expandElevationAnimation.get())
        //            {
        //                expandElevationAnimation.Duration(m_expandAnimationDuration);
        //            }
        //        }

        //        void SetContractAnimationDuration(const TimeSpan&contractAnimationDuration)
        //{
        //            m_contractAnimationDuration = contractAnimationDuration;
        //            if (auto && contractAnimation = m_contractAnimation.get())
        //            {
        //                contractAnimation.Duration(m_contractAnimationDuration);
        //            }
        //            if (auto && contractElevationAnimation = m_contractElevationAnimation.get())
        //            {
        //                contractElevationAnimation.Duration(m_contractAnimationDuration);
        //            }
        //        }

        //        bool GetIsIdle()
        //    {
        //            return m_isIdle;
        //        }

        //        void SetIsIdle(bool isIdle)
        //    {
        //            if (m_isIdle != isIdle)
        //            {
        //                m_isIdle = isIdle;
        //                TeachingTipTestHooks.NotifyIdleStatusChanged(this);
        //            }
        //        }

        //        TeachingTipPlacementMode GetEffectivePlacement()
        //    {
        //            return m_currentEffectiveTipPlacementMode;
        //        }

        //        TeachingTipHeroContentPlacementMode GetEffectiveHeroContentPlacement()
        //    {
        //            return m_currentHeroContentEffectivePlacementMode;
        //        }

        //        double GetHorizontalOffset()
        //    {
        //            if (auto && popup = m_popup.get())
        //            {
        //                return popup.HorizontalOffset();
        //            }
        //            return 0.0;
        //        }

        //        double GetVerticalOffset()
        //    {
        //            if (auto && popup = m_popup.get())
        //            {
        //                return popup.VerticalOffset();
        //            }
        //            return 0.0;
        //        }

        //        Visibility GetTitleVisibility()
        //    {
        //            if (auto && titleTextBox = m_titleTextBox.get())
        //            {
        //                return titleTextBox.Visibility();
        //            }
        //            return Visibility.Collapsed;
        //        }

        //        Visibility GetSubtitleVisibility()
        //    {
        //            if (auto && subtitleTextBox = m_subtitleTextBox.get())
        //            {
        //                return subtitleTextBox.Visibility();
        //            }
        //            return Visibility.Collapsed;
        //        }

        //        void UpdatePopupRequestedTheme()
        //    {
        //            // The way that TeachingTip reparents its content tree breaks ElementTheme calculations. Hook up a listener to
        //            // ActualTheme on the TeachingTip and then set the Popup's RequestedTheme to match when it changes.

        //            if (IFrameworkElement6 frameworkElement6 = this)
        //    {
        //                if (!m_actualThemeChangedRevoker)
        //                {
        //                    m_actualThemeChangedRevoker = frameworkElement6.ActualThemeChanged(auto_revoke,

        //                        [this](auto &&, auto &&) { UpdatePopupRequestedTheme(); });
        //                }

        //                if (auto && popup = m_popup.get())
        //                {
        //                    popup.RequestedTheme(frameworkElement6.ActualTheme());
        //                }
        //            }
        //        }

    }
}
