// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using static CppWinRTHelpers;
using static ModernWpf.ResourceAccessor;

namespace ModernWpf.Controls
{
    /// <summary>
    /// An <see cref="InfoBar"/> is an inline notification for essential app-wide messages. The <see cref="InfoBar"/> will take up space in a layout and will not cover up other content or float on top of it. It supports rich content (including titles, messages, icons, and buttons) and can be configured to be user-dismissable or persistent.
    /// </summary>
    public partial class InfoBar : Control, IControlProtected
    {
        bool m_applyTemplateCalled = false;
        bool m_notifyOpen = false;
        bool m_isVisible = false;

        InfoBarCloseReason m_lastCloseReason = InfoBarCloseReason.Programmatic;

        FrameworkElement m_standardIconTextBlock;

        private const string c_closeButtonName ="CloseButton";
        private const string c_iconTextBlockName ="StandardIcon";
        private const string c_contentRootName = "ContentRoot";

        private static readonly ResourceAccessor ResourceAccessor = new ResourceAccessor(typeof(InfoBar));

        static InfoBar()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(InfoBar), new FrameworkPropertyMetadata(typeof(InfoBar)));
        }

        public InfoBar()
        {
            SetValue(TemplateSettingsPropertyKey, new InfoBarTemplateSettings());
            DependencyPropertyDescriptor descriptor = DependencyPropertyDescriptor.FromProperty(ForegroundProperty, typeof(InfoBar));
            descriptor.AddValueChanged(this, (sender, e) => UpdateForeground());
        }

        protected override AutomationPeer OnCreateAutomationPeer()
        {
            return new InfoBarAutomationPeer(this);
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            
            m_applyTemplateCalled = true;

            IControlProtected controlProtected = this;

            var closeButton = GetTemplateChildT<Button>(c_closeButtonName, controlProtected);
            if (closeButton != null)
            {
                closeButton.Click += OnCloseButtonClick;

                // Do localization for the close button
                if (string.IsNullOrEmpty(AutomationProperties.GetName(closeButton)))
                {
                    var closeButtonName = ResourceAccessor.GetLocalizedStringResource(SR_InfoBarCloseButtonName);
                    AutomationProperties.SetName(closeButton, closeButtonName);
                }
                // Setup the tooltip for the close button
                var tooltip = new ToolTip();
                var closeButtonTooltipText = ResourceAccessor.GetLocalizedStringResource(SR_InfoBarCloseButtonTooltip);
                tooltip.Content = closeButtonTooltipText;
                ToolTipService.SetToolTip(closeButton, tooltip);
            }

            var iconTextblock = GetTemplateChildT<FrameworkElement>(c_iconTextBlockName, controlProtected);
            if (iconTextblock != null)
            {
                m_standardIconTextBlock = iconTextblock;
                AutomationProperties.SetName(iconTextblock, ResourceAccessor.GetLocalizedStringResource(GetIconSeverityLevelResourceName(Severity)));
            }

            //AutomationProperties.SetLocalizedLandmarkType(this, ResourceAccessor.GetLocalizedStringResource(SR_InfoBarCustomLandmarkName));

            UpdateVisibility(m_notifyOpen, true);
            m_notifyOpen = false;

            UpdateSeverity();
            UpdateIcon();
            UpdateIconVisibility();
            UpdateCloseButton();
            UpdateForeground();
        }

        void OnCloseButtonClick(object sender, RoutedEventArgs args)
        {
            CloseButtonClick?.Invoke(this, null);
            m_lastCloseReason = InfoBarCloseReason.CloseButton;
            IsOpen = false;
        }

        void RaiseClosingEvent()
        {
            var args = new InfoBarClosingEventArgs();
            args.Reason = m_lastCloseReason;

            Closing?.Invoke(this, args);

            if (!args.Cancel)
            {
                UpdateVisibility();
                RaiseClosedEvent();
            }
            else
            {
                // The developer has changed the Cancel property to true,
                // so we need to revert the IsOpen property to true.
                IsOpen = true;
            }
        }

        void RaiseClosedEvent()
        {
            var args = new InfoBarClosedEventArgs();
            args.Reason = m_lastCloseReason;
            Closed?.Invoke(this, args);
        }

        void OnIsOpenPropertyChanged(DependencyPropertyChangedEventArgs args)
        {
            if (IsOpen)
            {
                //Reset the close reason to the default value of programmatic.
                m_lastCloseReason = InfoBarCloseReason.Programmatic;

                UpdateVisibility();
            }
            else
            {
                RaiseClosingEvent();
            }
        }

        void OnSeverityPropertyChanged(DependencyPropertyChangedEventArgs args)
        {
            UpdateSeverity();
        }

        void OnIconSourcePropertyChanged(DependencyPropertyChangedEventArgs args)
        {
            UpdateIcon();
            UpdateIconVisibility();
        }

        void OnIsIconVisiblePropertyChanged(DependencyPropertyChangedEventArgs args)
        {
            UpdateIconVisibility();
        }

        void OnIsClosablePropertyChanged(DependencyPropertyChangedEventArgs args)
        {
            UpdateCloseButton();
        }

        public void UpdateVisibility(bool notify = false, bool force = false)
        {
            var peer = FrameworkElementAutomationPeer.FromElement(this) as InfoBarAutomationPeer;
            if (!m_applyTemplateCalled)
            {
                // ApplyTemplate() hasn't been called yet but IsOpen has already been set.
                // Since this method will be called again shortly from ApplyTemplate, we'll just wait and send a notification then.
                m_notifyOpen = true;
            }
            else
            {
                // Don't do any work if nothing has changed (unless we are forcing a update)
                if (force || IsOpen != m_isVisible)
                {
                    if (IsOpen)
                    {
                        if (notify && peer != null)
                        {
                            var notificationString = string.Format(ResourceAccessor.GetLocalizedStringResource(SR_InfoBarOpenedNotification), ResourceAccessor.GetLocalizedStringResource(GetIconSeverityLevelResourceName(Severity)), Title, Message);

                            peer.RaiseOpenedEvent(Severity, notificationString);
                        }

                        VisualStateManager.GoToState(this, "InfoBarVisible", false);
                        //AutomationProperties.SetAccessibilityView(this, AccessibilityView.Control);
                        m_isVisible = true;
                    }
                    else
                    {
                        if (notify && peer != null)
                        {
                            var notificationString = ResourceAccessor.GetLocalizedStringResource(SR_InfoBarClosedNotification);

                            peer.RaiseClosedEvent(Severity, notificationString);
                        }

                        VisualStateManager.GoToState(this, "InfoBarCollapsed", false);
                        //AutomationProperties.SetAccessibilityView(this, AccessibilityView.Raw);
                        m_isVisible = false;
                    }
                }
            }
        }

        public void UpdateSeverity()
        {
            var severityState = "Informational";

            switch (Severity)
            {
                case InfoBarSeverity.Success:
                    severityState = "Success";
                    break;
                case InfoBarSeverity.Warning:
                    severityState = "Warning";
                    break;
                case InfoBarSeverity.Error:
                    severityState = "Error";
                    break;
            };

            var iconTextblock = m_standardIconTextBlock;
            if (iconTextblock != null)
            {
                AutomationProperties.SetName(iconTextblock, ResourceAccessor.GetLocalizedStringResource(GetIconSeverityLevelResourceName(Severity)));
            }

            VisualStateManager.GoToState(this, severityState, false);
        }

        public void UpdateIcon()
        {
            var templateSettings = TemplateSettings;
            var source = IconSource;
            if (source != null)
            {
                templateSettings.IconElement = SharedHelpers.MakeIconElementFrom(source);
            }

            else
            {
                templateSettings.IconElement = null;
            }
        }

        public void UpdateIconVisibility()
        {
            VisualStateManager.GoToState(this, IsIconVisible ? (IconSource!=null ? "UserIconVisible" : "StandardIconVisible") : "NoIconVisible", false);
        }

        public void UpdateCloseButton()
        {
            VisualStateManager.GoToState(this, IsClosable ? "CloseButtonVisible" : "CloseButtonCollapsed", false);
        }

        public void UpdateForeground()
        {
            // If Foreground is set, then change Title and Message Foreground to match.
            VisualStateManager.GoToState(this, ReadLocalValue(Control.ForegroundProperty) == DependencyProperty.UnsetValue ? "ForegroundNotSet" : "ForegroundSet", false);
        }

        public string GetSeverityLevelResourceName(InfoBarSeverity severity)
        {
            switch (severity)
            {
                case InfoBarSeverity.Success:
                    return SR_InfoBarSeveritySuccessName;
                case InfoBarSeverity.Warning:
                    return SR_InfoBarSeverityWarningName;
                case InfoBarSeverity.Error:
                    return SR_InfoBarSeverityErrorName;
            };
            return SR_InfoBarSeverityInformationalName;
        }

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
    }
}
