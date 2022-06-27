using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using ModernWpf.Controls.Primitives;

namespace ModernWpf.Controls
{
    public partial class InfoBar
    {
        #region ActionButton

        public ButtonBase ActionButton
        {
            get => (ButtonBase)GetValue(ActionButtonProperty);
            set => SetValue(ActionButtonProperty, value);
        }

        public static readonly DependencyProperty ActionButtonProperty =
            DependencyProperty.Register(
                nameof(ActionButton),
                typeof(ButtonBase),
                typeof(InfoBar),
                null);

        #endregion

        #region CloseButtonCommand

        public ICommand CloseButtonCommand
        {
            get => (ICommand)GetValue(CloseButtonCommandProperty);
            set => SetValue(CloseButtonCommandProperty, value);
        }

        public static readonly DependencyProperty CloseButtonCommandProperty =
            DependencyProperty.Register(
                nameof(CloseButtonCommand),
                typeof(ICommand),
                typeof(InfoBar),
                null);

        #endregion

        #region CloseButtonCommandParameter

        public object CloseButtonCommandParameter
        {
            get => GetValue(CloseButtonCommandParameterProperty);
            set => SetValue(CloseButtonCommandParameterProperty, value);
        }

        public static readonly DependencyProperty CloseButtonCommandParameterProperty =
            DependencyProperty.Register(
                nameof(CloseButtonCommandParameter),
                typeof(object),
                typeof(InfoBar),
                null);

        #endregion

        #region CloseButtonStyle

        public Style CloseButtonStyle
        {
            get => (Style)GetValue(CloseButtonStyleProperty);
            set => SetValue(CloseButtonStyleProperty, value);
        }

        public static readonly DependencyProperty CloseButtonStyleProperty =
            DependencyProperty.Register(
                nameof(CloseButtonStyle),
                typeof(Style),
                typeof(InfoBar),
                null);

        #endregion

        #region Content

        public object Content
        {
            get => GetValue(ContentProperty);
            set => SetValue(ContentProperty, value);
        }

        public static readonly DependencyProperty ContentProperty =
            DependencyProperty.Register(
                nameof(Content),
                typeof(object),
                typeof(InfoBar),
                null);

        #endregion

        #region ContentTemplate

        public DataTemplate ContentTemplate
        {
            get => (DataTemplate)GetValue(ContentTemplateProperty);
            set => SetValue(ContentTemplateProperty, value);
        }

        public static readonly DependencyProperty ContentTemplateProperty =
            DependencyProperty.Register(
                nameof(ContentTemplate),
                typeof(DataTemplate),
                typeof(InfoBar),
                null);

        #endregion

        #region IconSource

        public IconSource IconSource
        {
            get => (IconSource)GetValue(IconSourceProperty);
            set => SetValue(IconSourceProperty, value);
        }

        public static readonly DependencyProperty IconSourceProperty =
            DependencyProperty.Register(
                nameof(IconSource),
                typeof(IconSource),
                typeof(InfoBar),
                new PropertyMetadata(OnIconSourcePropertyChanged));

        private static void OnIconSourcePropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            ((InfoBar)sender).OnIconSourcePropertyChanged(args);
        }

        #endregion

        #region IsClosable

        public bool IsClosable
        {
            get => (bool)GetValue(IsClosableProperty);
            set => SetValue(IsClosableProperty, value);
        }

        public static readonly DependencyProperty IsClosableProperty =
            DependencyProperty.Register(
                nameof(IsClosable),
                typeof(bool),
                typeof(InfoBar),
                new PropertyMetadata(true, OnIsClosablePropertyChanged));

        private static void OnIsClosablePropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            ((InfoBar)sender).OnIsClosablePropertyChanged(args);
        }

        #endregion

        #region IsIconVisible

        public bool IsIconVisible
        {
            get => (bool)GetValue(IsIconVisibleProperty);
            set => SetValue(IsIconVisibleProperty, value);
        }

        public static readonly DependencyProperty IsIconVisibleProperty =
            DependencyProperty.Register(
                nameof(IsIconVisible),
                typeof(bool),
                typeof(InfoBar),
                new PropertyMetadata(true, OnIsIconVisiblePropertyChanged));

        private static void OnIsIconVisiblePropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            ((InfoBar)sender).OnIsIconVisiblePropertyChanged(args);
        }

        #endregion

        #region IsOpen

        public bool IsOpen
        {
            get => (bool)GetValue(IsOpenProperty);
            set => SetValue(IsOpenProperty, value);
        }

        public static readonly DependencyProperty IsOpenProperty =
            DependencyProperty.Register(
                nameof(IsOpen),
                typeof(bool),
                typeof(InfoBar),
                new PropertyMetadata(false, OnIsOpenPropertyChanged));

        private static void OnIsOpenPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            ((InfoBar)sender).OnIsOpenPropertyChanged(args);
        }

        #endregion

        #region Message

        public string Message
        {
            get => (string)GetValue(MessageProperty);
            set => SetValue(MessageProperty, value);
        }

        public static readonly DependencyProperty MessageProperty =
            DependencyProperty.Register(
                nameof(Message),
                typeof(string),
                typeof(InfoBar),
                null);

        #endregion

        #region Severity

        public InfoBarSeverity Severity
        {
            get => (InfoBarSeverity)GetValue(SeverityProperty);
            set => SetValue(SeverityProperty, value);
        }

        public static readonly DependencyProperty SeverityProperty =
            DependencyProperty.Register(
                nameof(Severity),
                typeof(InfoBarSeverity),
                typeof(InfoBar),
                new PropertyMetadata(InfoBarSeverity.Informational, OnSeverityPropertyChanged));

        private static void OnSeverityPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            ((InfoBar)sender).OnSeverityPropertyChanged(args);
        }

        #endregion

        #region TemplateSettings

        private static readonly DependencyPropertyKey TemplateSettingsPropertyKey =
            DependencyProperty.RegisterReadOnly(
                nameof(TemplateSettings),
                typeof(InfoBarTemplateSettings),
                typeof(InfoBar),
                null);

        public static readonly DependencyProperty TemplateSettingsProperty =
            TemplateSettingsPropertyKey.DependencyProperty;

        public InfoBarTemplateSettings TemplateSettings
        {
            get => (InfoBarTemplateSettings)GetValue(TemplateSettingsProperty);
        }

        #endregion

        #region Title

        public string Title
        {
            get => (string)GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }

        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register(
                nameof(Title),
                typeof(string),
                typeof(InfoBar),
                null);

        #endregion

        #region CornerRadius

        /// <summary>
        /// Identifies the CornerRadius dependency property.
        /// </summary>
        public static readonly DependencyProperty CornerRadiusProperty =
            ControlHelper.CornerRadiusProperty.AddOwner(typeof(InfoBar));

        /// <summary>
        /// Gets or sets the radius for the corners of the control's border.
        /// </summary>
        /// <returns>
        /// The degree to which the corners are rounded, expressed as values of the CornerRadius
        /// structure.
        /// </returns>
        public CornerRadius CornerRadius
        {
            get => (CornerRadius)GetValue(CornerRadiusProperty);
            set => SetValue(CornerRadiusProperty, value);
        }

        #endregion

        public event TypedEventHandler<InfoBar, object> CloseButtonClick;

        public event TypedEventHandler<InfoBar, InfoBarClosedEventArgs> Closed;

        public event TypedEventHandler<InfoBar, InfoBarClosingEventArgs> Closing;
    }
}
