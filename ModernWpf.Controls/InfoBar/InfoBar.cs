using System;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using ModernWpf.Automation.Peers;
using ModernWpf.Controls.Primitives;

namespace ModernWpf.Controls
{
    public partial class InfoBar : Control
    {
        private const string CloseButtonName = "CloseButton";
        private const string StandardIconName = "StandardIcon";

        static InfoBar()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(InfoBar), new FrameworkPropertyMetadata(typeof(InfoBar)));
        }

        public InfoBar()
        {
            SetValue(TemplateSettingsPropertyKey, new InfoBarTemplateSettings());
        }

        public event RoutedEventHandler CloseButtonClick;

        public event EventHandler<InfoBarClosingEventArgs> Closing;

        public event EventHandler<InfoBarClosedEventArgs> Closed;

        public event EventHandler<InfoBarOpenedEventArgs> Opened;

        public override void OnApplyTemplate()
        {
            _applyTemplateCalled = true;

            if (_closeButton != null)
            {
                _closeButton.Click -= OnCloseButtonClick;
            }

            base.OnApplyTemplate();

            _closeButton = GetTemplateChild(CloseButtonName) as Button;
            _standardIconTextBlock = GetTemplateChild(StandardIconName) as FrameworkElement;

            if (_closeButton != null)
            {
                _closeButton.Click += OnCloseButtonClick;

                if (string.IsNullOrEmpty(AutomationProperties.GetName(_closeButton)))
                {
                    AutomationProperties.SetName(_closeButton, Strings.InfoBarCloseButtonName);
                }

                _closeButton.ToolTip = new ToolTip { Content = Strings.InfoBarCloseButtonTooltip };
            }

            if (_standardIconTextBlock != null)
            {
                AutomationProperties.SetName(_standardIconTextBlock, GetIconSeverityLevelName(Severity));
            }

            UpdateVisibility(_notifyOpen, true);
            _notifyOpen = false;

            UpdateSeverity();
            UpdateIcon();
            UpdateIconVisibility();
            UpdateCloseButton();
            UpdateForeground();
            UpdateContentPosition();
        }

        protected override AutomationPeer OnCreateAutomationPeer()
        {
            return new InfoBarAutomationPeer(this);
        }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);

            if (e.Property == ForegroundProperty)
            {
                UpdateForeground();
            }
        }

        private static void OnIsOpenPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var infoBar = (InfoBar)d;
            if ((bool)e.NewValue)
            {
                infoBar._lastCloseReason = InfoBarCloseReason.Programmatic;
                infoBar.UpdateVisibility();
                infoBar.RaiseOpenedEvent();
            }
            else
            {
                infoBar.RaiseClosingEvent();
            }
        }

        private static void OnSeverityPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((InfoBar)d).UpdateSeverity();
        }

        private static void OnIconSourcePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var infoBar = (InfoBar)d;
            infoBar.UpdateIcon();
            infoBar.UpdateIconVisibility();
        }

        private static void OnIsIconVisiblePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((InfoBar)d).UpdateIconVisibility();
        }

        private static void OnIsClosablePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((InfoBar)d).UpdateCloseButton();
        }

        private static void OnContentPositionPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((InfoBar)d).UpdateContentPosition();
        }

        private void OnCloseButtonClick(object sender, RoutedEventArgs e)
        {
            CloseButtonClick?.Invoke(this, e);
            _lastCloseReason = InfoBarCloseReason.CloseButton;
            IsOpen = false;
        }

        private void RaiseClosingEvent()
        {
            var args = new InfoBarClosingEventArgs(_lastCloseReason);
            Closing?.Invoke(this, args);

            if (!args.Cancel)
            {
                UpdateVisibility();
                RaiseClosedEvent();
            }
            else
            {
                IsOpen = true;
            }
        }

        private void RaiseClosedEvent()
        {
            Closed?.Invoke(this, new InfoBarClosedEventArgs(_lastCloseReason));
        }

        private void RaiseOpenedEvent()
        {
            Opened?.Invoke(this, new InfoBarOpenedEventArgs());
        }

        private void UpdateVisibility(bool notify = true, bool force = false)
        {
            var peer = FrameworkElementAutomationPeer.FromElement(this) as InfoBarAutomationPeer;

            if (!_applyTemplateCalled)
            {
                _notifyOpen = true;
            }
            else if (force || IsOpen != _isVisible)
            {
                if (IsOpen)
                {
                    if (notify && peer != null)
                    {
                        var notificationString = string.Format(
                            Strings.InfoBarOpenedNotification,
                            GetSeverityLevelName(Severity),
                            Title,
                            Message);
                        peer.RaiseOpenedEvent(Severity, notificationString);
                    }

                    VisualStateManager.GoToState(this, "InfoBarVisible", false);
                    _isVisible = true;
                }
                else
                {
                    if (notify && peer != null)
                    {
                        peer.RaiseClosedEvent(Severity, Strings.InfoBarClosedNotification);
                    }

                    VisualStateManager.GoToState(this, "InfoBarCollapsed", false);
                    _isVisible = false;
                }

                peer?.InvalidatePeer();
            }
        }

        private void UpdateSeverity()
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
            }

            if (_standardIconTextBlock != null)
            {
                AutomationProperties.SetName(_standardIconTextBlock, GetIconSeverityLevelName(Severity));
            }

            VisualStateManager.GoToState(this, severityState, false);
        }

        private void UpdateIcon()
        {
            TemplateSettings.IconElement = IconSource?.CreateIconElement();
        }

        private void UpdateIconVisibility()
        {
            string stateName = !IsIconVisible
                ? "NoIconVisible"
                : IconSource != null
                    ? "UserIconVisible"
                    : "StandardIconVisible";
            VisualStateManager.GoToState(this, stateName, false);
        }

        private void UpdateCloseButton()
        {
            VisualStateManager.GoToState(this, IsClosable ? "CloseButtonVisible" : "CloseButtonCollapsed", false);
        }

        private void UpdateForeground()
        {
            VisualStateManager.GoToState(
                this,
                ReadLocalValue(ForegroundProperty) == DependencyProperty.UnsetValue ? "ForegroundNotSet" : "ForegroundSet",
                false);
        }

        private void UpdateContentPosition()
        {
            VisualStateManager.GoToState(
                this,
                string.IsNullOrEmpty(Title) && string.IsNullOrEmpty(Message) && ActionButton == null ? "NoBannerContent" : "BannerContent",
                false);
        }

        private static string GetSeverityLevelName(InfoBarSeverity severity)
        {
            switch (severity)
            {
                case InfoBarSeverity.Success:
                    return Strings.InfoBarSeveritySuccessName;
                case InfoBarSeverity.Warning:
                    return Strings.InfoBarSeverityWarningName;
                case InfoBarSeverity.Error:
                    return Strings.InfoBarSeverityErrorName;
                default:
                    return Strings.InfoBarSeverityInformationalName;
            }
        }

        private static string GetIconSeverityLevelName(InfoBarSeverity severity)
        {
            switch (severity)
            {
                case InfoBarSeverity.Success:
                    return Strings.InfoBarIconSeveritySuccessName;
                case InfoBarSeverity.Warning:
                    return Strings.InfoBarIconSeverityWarningName;
                case InfoBarSeverity.Error:
                    return Strings.InfoBarIconSeverityErrorName;
                default:
                    return Strings.InfoBarIconSeverityInformationalName;
            }
        }

        private InfoBarCloseReason _lastCloseReason = InfoBarCloseReason.Programmatic;
        private Button _closeButton;
        private FrameworkElement _standardIconTextBlock;
        private bool _applyTemplateCalled;
        private bool _notifyOpen;
        private bool _isVisible;
    }
}
