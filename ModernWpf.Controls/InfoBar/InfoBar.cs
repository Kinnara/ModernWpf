using System;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using ModernWpf.Automation.Peers;
using ModernWpf.Controls.Primitives;

namespace ModernWpf.Controls
{
    public class InfoBar : Control
    {
        private const string CloseButtonName = "CloseButton";
        private const string StandardIconName = "StandardIcon";
        private const string IconBackgroundName = "IconBackground";
        private const string ContentRootName = "ContentRoot";
        private const string UserIconBoxName = "UserIconBox";
        private const string StandardIconAreaName = "StandardIconArea";
        private const string TitleName = "Title";
        private const string MessageName = "Message";
        private const string ContentAreaName = "ContentArea";

        static InfoBar()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(InfoBar), new FrameworkPropertyMetadata(typeof(InfoBar)));
        }

        public InfoBar()
        {
            SetValue(TemplateSettingsPropertyKey, new InfoBarTemplateSettings());
        }

        public static readonly DependencyProperty IsOpenProperty =
            DependencyProperty.Register(
                nameof(IsOpen),
                typeof(bool),
                typeof(InfoBar),
                new PropertyMetadata(false, OnIsOpenPropertyChanged));

        public bool IsOpen
        {
            get => (bool)GetValue(IsOpenProperty);
            set => SetValue(IsOpenProperty, value);
        }

        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register(
                nameof(Title),
                typeof(string),
                typeof(InfoBar),
                new PropertyMetadata(string.Empty, OnContentPositionPropertyChanged));

        public string Title
        {
            get => (string)GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }

        public static readonly DependencyProperty MessageProperty =
            DependencyProperty.Register(
                nameof(Message),
                typeof(string),
                typeof(InfoBar),
                new PropertyMetadata(string.Empty, OnContentPositionPropertyChanged));

        public string Message
        {
            get => (string)GetValue(MessageProperty);
            set => SetValue(MessageProperty, value);
        }

        public static readonly DependencyProperty SeverityProperty =
            DependencyProperty.Register(
                nameof(Severity),
                typeof(InfoBarSeverity),
                typeof(InfoBar),
                new PropertyMetadata(InfoBarSeverity.Informational, OnSeverityPropertyChanged));

        public InfoBarSeverity Severity
        {
            get => (InfoBarSeverity)GetValue(SeverityProperty);
            set => SetValue(SeverityProperty, value);
        }

        public static readonly DependencyProperty IconSourceProperty =
            DependencyProperty.Register(
                nameof(IconSource),
                typeof(IconSource),
                typeof(InfoBar),
                new PropertyMetadata(null, OnIconSourcePropertyChanged));

        public IconSource IconSource
        {
            get => (IconSource)GetValue(IconSourceProperty);
            set => SetValue(IconSourceProperty, value);
        }

        public static readonly DependencyProperty IsIconVisibleProperty =
            DependencyProperty.Register(
                nameof(IsIconVisible),
                typeof(bool),
                typeof(InfoBar),
                new PropertyMetadata(true, OnIsIconVisiblePropertyChanged));

        public bool IsIconVisible
        {
            get => (bool)GetValue(IsIconVisibleProperty);
            set => SetValue(IsIconVisibleProperty, value);
        }

        public static readonly DependencyProperty IsClosableProperty =
            DependencyProperty.Register(
                nameof(IsClosable),
                typeof(bool),
                typeof(InfoBar),
                new PropertyMetadata(true, OnIsClosablePropertyChanged));

        public bool IsClosable
        {
            get => (bool)GetValue(IsClosableProperty);
            set => SetValue(IsClosableProperty, value);
        }

        public static readonly DependencyProperty CloseButtonStyleProperty =
            DependencyProperty.Register(
                nameof(CloseButtonStyle),
                typeof(Style),
                typeof(InfoBar));

        public Style CloseButtonStyle
        {
            get => (Style)GetValue(CloseButtonStyleProperty);
            set => SetValue(CloseButtonStyleProperty, value);
        }

        public static readonly DependencyProperty CloseButtonCommandProperty =
            DependencyProperty.Register(
                nameof(CloseButtonCommand),
                typeof(ICommand),
                typeof(InfoBar));

        public ICommand CloseButtonCommand
        {
            get => (ICommand)GetValue(CloseButtonCommandProperty);
            set => SetValue(CloseButtonCommandProperty, value);
        }

        public static readonly DependencyProperty CloseButtonCommandParameterProperty =
            DependencyProperty.Register(
                nameof(CloseButtonCommandParameter),
                typeof(object),
                typeof(InfoBar));

        public object CloseButtonCommandParameter
        {
            get => GetValue(CloseButtonCommandParameterProperty);
            set => SetValue(CloseButtonCommandParameterProperty, value);
        }

        public static readonly DependencyProperty ActionButtonProperty =
            DependencyProperty.Register(
                nameof(ActionButton),
                typeof(ButtonBase),
                typeof(InfoBar),
                new PropertyMetadata(null, OnContentPositionPropertyChanged));

        public ButtonBase ActionButton
        {
            get => (ButtonBase)GetValue(ActionButtonProperty);
            set => SetValue(ActionButtonProperty, value);
        }

        public static readonly DependencyProperty ContentProperty =
            DependencyProperty.Register(
                nameof(Content),
                typeof(object),
                typeof(InfoBar));

        public object Content
        {
            get => GetValue(ContentProperty);
            set => SetValue(ContentProperty, value);
        }

        public static readonly DependencyProperty ContentTemplateProperty =
            DependencyProperty.Register(
                nameof(ContentTemplate),
                typeof(DataTemplate),
                typeof(InfoBar));

        public DataTemplate ContentTemplate
        {
            get => (DataTemplate)GetValue(ContentTemplateProperty);
            set => SetValue(ContentTemplateProperty, value);
        }

        public static readonly DependencyProperty CornerRadiusProperty =
            ControlHelper.CornerRadiusProperty.AddOwner(typeof(InfoBar));

        public CornerRadius CornerRadius
        {
            get => (CornerRadius)GetValue(CornerRadiusProperty);
            set => SetValue(CornerRadiusProperty, value);
        }

        private static readonly DependencyPropertyKey TemplateSettingsPropertyKey =
            DependencyProperty.RegisterReadOnly(
                nameof(TemplateSettings),
                typeof(InfoBarTemplateSettings),
                typeof(InfoBar),
                new PropertyMetadata(null));

        public static readonly DependencyProperty TemplateSettingsProperty =
            TemplateSettingsPropertyKey.DependencyProperty;

        public InfoBarTemplateSettings TemplateSettings =>
            (InfoBarTemplateSettings)GetValue(TemplateSettingsProperty);

        public event RoutedEventHandler CloseButtonClick;

        public event EventHandler<InfoBarClosingEventArgs> Closing;

        public event EventHandler<InfoBarClosedEventArgs> Closed;

        public event EventHandler<InfoBarOpenedEventArgs> Opened;

        public override void OnApplyTemplate()
        {
            if (_closeButton != null)
            {
                _closeButton.Click -= OnCloseButtonClick;
            }

            base.OnApplyTemplate();

            _contentRoot = GetTemplateChild(ContentRootName) as Border;
            _closeButton = GetTemplateChild(CloseButtonName) as Button;
            _standardIcon = GetTemplateChild(StandardIconName) as TextBlock;
            _iconBackground = GetTemplateChild(IconBackgroundName) as TextBlock;
            _standardIconArea = GetTemplateChild(StandardIconAreaName) as FrameworkElement;
            _userIconBox = GetTemplateChild(UserIconBoxName) as FrameworkElement;
            _titleTextBlock = GetTemplateChild(TitleName) as TextBlock;
            _messageTextBlock = GetTemplateChild(MessageName) as TextBlock;
            _contentArea = GetTemplateChild(ContentAreaName) as FrameworkElement;

            if (_closeButton != null)
            {
                _closeButton.Click += OnCloseButtonClick;

                if (string.IsNullOrEmpty(System.Windows.Automation.AutomationProperties.GetName(_closeButton)))
                {
                    System.Windows.Automation.AutomationProperties.SetName(_closeButton, "Close");
                }

                if (_closeButton.ToolTip == null)
                {
                    _closeButton.ToolTip = "Close";
                }
            }

            UpdateVisibility(force: true);
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
                infoBar.Opened?.Invoke(infoBar, new InfoBarOpenedEventArgs());
            }
            else if ((bool)e.OldValue)
            {
                infoBar.RaiseClosingEvent();
            }
            else
            {
                infoBar.UpdateVisibility();
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

            if (args.Cancel)
            {
                IsOpen = true;
                return;
            }

            UpdateVisibility();
            Closed?.Invoke(this, new InfoBarClosedEventArgs(_lastCloseReason));
        }

        private void UpdateVisibility(bool force = false)
        {
            if (_contentRoot != null && (force || _contentRoot.Visibility != (IsOpen ? Visibility.Visible : Visibility.Collapsed)))
            {
                _contentRoot.Visibility = IsOpen ? Visibility.Visible : Visibility.Collapsed;
                var peer = FrameworkElementAutomationPeer.FromElement(this);
                peer?.InvalidatePeer();
            }
        }

        private void UpdateSeverity()
        {
            if (_contentRoot == null)
            {
                return;
            }

            var backgroundKey = "InfoBarInformationalSeverityBackgroundBrush";
            var iconBackgroundKey = "InfoBarInformationalSeverityIconBackground";
            var iconForegroundKey = "InfoBarInformationalSeverityIconForeground";
            var iconGlyph = "\uF13F";

            switch (Severity)
            {
                case InfoBarSeverity.Success:
                    backgroundKey = "InfoBarSuccessSeverityBackgroundBrush";
                    iconBackgroundKey = "InfoBarSuccessSeverityIconBackground";
                    iconForegroundKey = "InfoBarSuccessSeverityIconForeground";
                    iconGlyph = "\uF13E";
                    break;

                case InfoBarSeverity.Warning:
                    backgroundKey = "InfoBarWarningSeverityBackgroundBrush";
                    iconBackgroundKey = "InfoBarWarningSeverityIconBackground";
                    iconForegroundKey = "InfoBarWarningSeverityIconForeground";
                    iconGlyph = "\uF13C";
                    break;

                case InfoBarSeverity.Error:
                    backgroundKey = "InfoBarErrorSeverityBackgroundBrush";
                    iconBackgroundKey = "InfoBarErrorSeverityIconBackground";
                    iconForegroundKey = "InfoBarErrorSeverityIconForeground";
                    iconGlyph = "\uF13D";
                    break;
            }

            SetResourceReferenceOn(_contentRoot, Border.BackgroundProperty, backgroundKey);
            SetResourceReferenceOn(_iconBackground, TextBlock.ForegroundProperty, iconBackgroundKey);
            SetResourceReferenceOn(_standardIcon, TextBlock.ForegroundProperty, iconForegroundKey);

            if (_standardIcon != null)
            {
                _standardIcon.Text = iconGlyph;
            }
        }

        private void UpdateIcon()
        {
            TemplateSettings.IconElement = IconSource?.CreateIconElement();
        }

        private void UpdateIconVisibility()
        {
            if (_standardIconArea == null || _userIconBox == null)
            {
                return;
            }

            if (!IsIconVisible)
            {
                _standardIconArea.Visibility = Visibility.Collapsed;
                _userIconBox.Visibility = Visibility.Collapsed;
            }
            else if (IconSource != null)
            {
                _standardIconArea.Visibility = Visibility.Collapsed;
                _userIconBox.Visibility = Visibility.Visible;
            }
            else
            {
                _standardIconArea.Visibility = Visibility.Visible;
                _userIconBox.Visibility = Visibility.Collapsed;
            }
        }

        private void UpdateCloseButton()
        {
            if (_closeButton != null)
            {
                _closeButton.Visibility = IsClosable ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        private void UpdateForeground()
        {
            if (_titleTextBlock == null || _messageTextBlock == null)
            {
                return;
            }

            if (ReadLocalValue(ForegroundProperty) == DependencyProperty.UnsetValue)
            {
                SetResourceReferenceOn(_titleTextBlock, TextBlock.ForegroundProperty, "InfoBarTitleForeground");
                SetResourceReferenceOn(_messageTextBlock, TextBlock.ForegroundProperty, "InfoBarMessageForeground");
            }
            else
            {
                _titleTextBlock.Foreground = Foreground;
                _messageTextBlock.Foreground = Foreground;
            }
        }

        private void UpdateContentPosition()
        {
            if (_contentArea != null)
            {
                var row = string.IsNullOrEmpty(Title) && string.IsNullOrEmpty(Message) && ActionButton == null ? 0 : 1;
                Grid.SetRow(_contentArea, row);
            }
        }

        private static void SetResourceReferenceOn(FrameworkElement element, DependencyProperty property, object resourceKey)
        {
            element?.SetResourceReference(property, resourceKey);
        }

        private InfoBarCloseReason _lastCloseReason = InfoBarCloseReason.Programmatic;
        private Border _contentRoot;
        private Button _closeButton;
        private TextBlock _standardIcon;
        private TextBlock _iconBackground;
        private FrameworkElement _standardIconArea;
        private FrameworkElement _userIconBox;
        private TextBlock _titleTextBlock;
        private TextBlock _messageTextBlock;
        private FrameworkElement _contentArea;
    }
}
