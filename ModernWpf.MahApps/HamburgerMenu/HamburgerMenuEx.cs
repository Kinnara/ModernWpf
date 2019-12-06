using MahApps.Metro.Controls;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ModernWpf.MahApps.Controls
{
    /// <summary>
    /// Represents a container that enables navigation of app content. It has a header,
    /// a view for the main content, and a menu pane for navigation commands.
    /// </summary>
    [TemplatePart(Name = c_navViewBackButton, Type = typeof(Button))]
    public class HamburgerMenuEx : HamburgerMenu
    {
        private const string c_navViewBackButton = "NavigationViewBackButton";

        static HamburgerMenuEx()
        {
            DisplayModeProperty.OverrideMetadata(typeof(HamburgerMenuEx), new FrameworkPropertyMetadata(OnDisplayModePropertyChanged));
            IsPaneOpenProperty.OverrideMetadata(typeof(HamburgerMenuEx), new FrameworkPropertyMetadata(OnIsPaneOpenPropertyChanged));
            CompactPaneLengthProperty.OverrideMetadata(typeof(HamburgerMenuEx), new FrameworkPropertyMetadata(OnPaneLengthPropertyChanged));
            OpenPaneLengthProperty.OverrideMetadata(typeof(HamburgerMenuEx), new FrameworkPropertyMetadata(OnPaneLengthPropertyChanged));
        }

        /// <summary>
        /// Initializes a new instance of the HamburgerMenuEx class.
        /// </summary>
        public HamburgerMenuEx()
        {
            DefaultStyleKey = typeof(HamburgerMenuEx);
            SetResourceReference(DefaultItemFocusVisualStyleProperty, SystemParameters.FocusVisualStyleKey);
        }

        #region IsBackButtonVisible

        /// <summary>
        /// Identifies the IsBackButtonVisible dependency property.
        /// </summary>
        public static readonly DependencyProperty IsBackButtonVisibleProperty =
            DependencyProperty.Register(
                nameof(IsBackButtonVisible),
                typeof(bool),
                typeof(HamburgerMenuEx),
                new PropertyMetadata(false));

        /// <summary>
        /// Gets or sets a value that indicates whether the back button is visible or collapsed.
        /// </summary>
        /// <returns>true if the back button is visible; otherwise, false. The default is false.</returns>
        public bool IsBackButtonVisible
        {
            get => (bool)GetValue(IsBackButtonVisibleProperty);
            set => SetValue(IsBackButtonVisibleProperty, value);
        }

        #endregion

        #region IsBackEnabled

        /// <summary>
        /// Identifies the IsBackEnabled dependency property.
        /// </summary>
        public static readonly DependencyProperty IsBackEnabledProperty =
            DependencyProperty.Register(
                nameof(IsBackEnabled),
                typeof(bool),
                typeof(HamburgerMenuEx),
                new PropertyMetadata(true));

        /// <summary>
        /// Gets or sets a value that indicates whether the back button is enabled or disabled.
        /// </summary>
        /// <returns>true if the back button is enabled; otherwise, false. The default is true.</returns>
        public bool IsBackEnabled
        {
            get => (bool)GetValue(IsBackEnabledProperty);
            set => SetValue(IsBackEnabledProperty, value);
        }

        #endregion

        #region BackButtonCommand

        public static readonly DependencyProperty BackButtonCommandProperty =
            DependencyProperty.Register(
                nameof(BackButtonCommand),
                typeof(ICommand),
                typeof(HamburgerMenuEx),
                null);

        public ICommand BackButtonCommand
        {
            get => (ICommand)GetValue(BackButtonCommandProperty);
            set => SetValue(BackButtonCommandProperty, value);
        }

        #endregion

        #region BackButtonCommandParameter

        public static readonly DependencyProperty BackButtonCommandParameterProperty =
            DependencyProperty.Register(
                nameof(BackButtonCommandParameter),
                typeof(object),
                typeof(HamburgerMenuEx),
                null);

        public object BackButtonCommandParameter
        {
            get => GetValue(BackButtonCommandParameterProperty);
            set => SetValue(BackButtonCommandParameterProperty, value);
        }

        #endregion

        #region PaneLength

        private static readonly DependencyPropertyKey PaneLengthPropertyKey =
            DependencyProperty.RegisterReadOnly(
                nameof(PaneLength),
                typeof(double),
                typeof(HamburgerMenuEx),
                new PropertyMetadata(0.0));

        public static readonly DependencyProperty PaneLengthProperty =
            PaneLengthPropertyKey.DependencyProperty;

        public double PaneLength
        {
            get => (double)GetValue(PaneLengthProperty);
            private set => SetValue(PaneLengthPropertyKey, value);
        }

        private void UpdatePaneLength()
        {
            PaneLength = IsPaneOpen ? OpenPaneLength : CompactPaneLength;
        }

        #endregion

        #region Header

        /// <summary>
        /// Identifies the Header dependency property.
        /// </summary>
        public static readonly DependencyProperty HeaderProperty =
            DependencyProperty.Register(
                nameof(Header),
                typeof(object),
                typeof(HamburgerMenuEx));

        /// <summary>
        /// Gets or sets the header content.
        /// </summary>
        /// <returns>The header content.</returns>
        public object Header
        {
            get => GetValue(HeaderProperty);
            set => SetValue(HeaderProperty, value);
        }

        #endregion

        #region HeaderTemplate

        /// <summary>
        /// Identifies the HeaderTemplate dependency property.
        /// </summary>
        public static readonly DependencyProperty HeaderTemplateProperty =
            DependencyProperty.Register(
                nameof(HeaderTemplate),
                typeof(DataTemplate),
                typeof(HamburgerMenuEx));

        /// <summary>
        /// Gets or sets the DataTemplate used to display the control's header.
        /// </summary>
        /// <returns>The DataTemplate used to display the control's header.</returns>
        public DataTemplate HeaderTemplate
        {
            get => (DataTemplate)GetValue(HeaderTemplateProperty);
            set => SetValue(HeaderTemplateProperty, value);
        }

        #endregion

        #region DefaultItemFocusVisualStyle

        private static readonly DependencyProperty DefaultItemFocusVisualStyleProperty =
            DependencyProperty.Register(
                nameof(DefaultItemFocusVisualStyle),
                typeof(Style),
                typeof(HamburgerMenuEx),
                new PropertyMetadata(OnDefaultItemFocusVisualStyleChanged));

        private Style DefaultItemFocusVisualStyle
        {
            get => (Style)GetValue(DefaultItemFocusVisualStyleProperty);
            set => SetValue(DefaultItemFocusVisualStyleProperty, value);
        }

        private static void OnDefaultItemFocusVisualStyleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((HamburgerMenuEx)d).ChangeItemFocusVisualStyle();
        }

        #endregion

        #region PaneTitle

        /// <summary>
        /// Identifies the PaneTitle dependency property.
        /// </summary>
        public static readonly DependencyProperty PaneTitleProperty =
            DependencyProperty.Register(
                nameof(PaneTitle),
                typeof(string),
                typeof(HamburgerMenuEx),
                new PropertyMetadata(OnPaneTitleChanged));

        /// <summary>
        /// Gets or sets the label adjacent to the menu icon when the pane is open.
        /// </summary>
        /// <returns>
        /// The label adjacent to the menu icon when the pane is open. The default is an
        /// empty string.
        /// </returns>
        public string PaneTitle
        {
            get => (string)GetValue(PaneTitleProperty);
            set => SetValue(PaneTitleProperty, value);
        }

        private static void OnPaneTitleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((HamburgerMenuEx)d).OnPaneTitleChanged(e);
        }

        private void OnPaneTitleChanged(DependencyPropertyChangedEventArgs e)
        {
            HasPaneTitle = !string.IsNullOrEmpty((string)e.NewValue);
        }

        #endregion

        #region HasPaneTitle

        private static readonly DependencyPropertyKey HasPaneTitlePropertyKey =
            DependencyProperty.RegisterReadOnly(
                nameof(HasPaneTitle),
                typeof(bool),
                typeof(HamburgerMenuEx),
                new PropertyMetadata(false));

        public static readonly DependencyProperty HasPaneTitleProperty =
            HasPaneTitlePropertyKey.DependencyProperty;

        public bool HasPaneTitle
        {
            get => (bool)GetValue(HasPaneTitleProperty);
            private set => SetValue(HasPaneTitlePropertyKey, value);
        }

        #endregion

        /// <summary>
        /// Occurs when the back button receives an interaction such as a click or tap.
        /// </summary>
        public event EventHandler<HamburgerMenuBackRequestedEventArgs> BackRequested;

        /// <summary>
        /// Occurs when the DisplayMode property changes.
        /// </summary>
        public event EventHandler<HamburgerMenuDisplayModeChangedEventArgs> DisplayModeChanged;

        /// <summary>
        /// Occurs when the pane is opened.
        /// </summary>
        public event EventHandler PaneOpened;

        /// <summary>
        /// Occurs when the pane is closed.
        /// </summary>
        public event EventHandler PaneClosed;

        /// <summary>
        /// Called when the Template's tree has been generated.
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            if (_backButton != null)
            {
                _backButton.Click -= OnBackButtonClicked;
            }

            _backButton = GetTemplateChild(c_navViewBackButton) as Button;

            if (_backButton != null)
            {
                _backButton.Click += OnBackButtonClicked;
            }

            ChangeItemFocusVisualStyle();
        }

        private static void OnDisplayModePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((HamburgerMenuEx)d).OnDisplayModeChanged(e);
        }

        private static void OnIsPaneOpenPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((HamburgerMenuEx)d).OnIsPaneOpenChanged(e);
        }

        private static void OnPaneLengthPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((HamburgerMenuEx)d).OnPaneLengthPropertyChanged(e);
        }

        private void OnBackButtonClicked(object sender, RoutedEventArgs e)
        {
            BackRequested?.Invoke(this, new HamburgerMenuBackRequestedEventArgs());
        }

        private void OnDisplayModeChanged(DependencyPropertyChangedEventArgs e)
        {
            DisplayModeChanged?.Invoke(this, new HamburgerMenuDisplayModeChangedEventArgs((SplitViewDisplayMode)e.NewValue));
        }

        private void OnIsPaneOpenChanged(DependencyPropertyChangedEventArgs e)
        {
            UpdatePaneLength();
            ChangeItemFocusVisualStyle();

            if ((bool)e.NewValue)
            {
                PaneOpened?.Invoke(this, EventArgs.Empty);
            }
            else
            {
                PaneClosed?.Invoke(this, EventArgs.Empty);
            }
        }

        private void OnPaneLengthPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            UpdatePaneLength();
            ChangeItemFocusVisualStyle();
        }

        private void ChangeItemFocusVisualStyle()
        {
            if (DefaultItemFocusVisualStyle != null)
            {
                var focusVisualStyle = new Style(typeof(Control), DefaultItemFocusVisualStyle);
                focusVisualStyle.Setters.Add(new Setter(Control.WidthProperty, IsPaneOpen ? OpenPaneLength : CompactPaneLength));
                focusVisualStyle.Setters.Add(new Setter(Control.HorizontalAlignmentProperty, HorizontalAlignment.Left));
                focusVisualStyle.Seal();

                SetValue(ItemFocusVisualStylePropertyKey, focusVisualStyle);
            }
        }

        private Button _backButton;
    }
}
