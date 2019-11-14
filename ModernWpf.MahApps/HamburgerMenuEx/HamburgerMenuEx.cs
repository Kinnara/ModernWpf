using MahApps.Metro.Controls;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ModernWpf.MahApps.Controls
{
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

        public HamburgerMenuEx()
        {
            DefaultStyleKey = typeof(HamburgerMenuEx);
        }

        #region IsBackButtonVisible

        public static readonly DependencyProperty IsBackButtonVisibleProperty =
            DependencyProperty.Register(
                nameof(IsBackButtonVisible),
                typeof(bool),
                typeof(HamburgerMenuEx),
                new PropertyMetadata(false));

        public bool IsBackButtonVisible
        {
            get => (bool)GetValue(IsBackButtonVisibleProperty);
            set => SetValue(IsBackButtonVisibleProperty, value);
        }

        #endregion

        #region IsBackEnabled

        public static readonly DependencyProperty IsBackEnabledProperty =
            DependencyProperty.Register(
                nameof(IsBackEnabled),
                typeof(bool),
                typeof(HamburgerMenuEx),
                new PropertyMetadata(true));

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

        public static readonly DependencyProperty HeaderProperty =
            DependencyProperty.Register(
                nameof(Header),
                typeof(object),
                typeof(HamburgerMenuEx));

        public object Header
        {
            get => GetValue(HeaderProperty);
            set => SetValue(HeaderProperty, value);
        }

        #endregion

        #region HeaderTemplate

        public static readonly DependencyProperty HeaderTemplateProperty =
            DependencyProperty.Register(
                nameof(HeaderTemplate),
                typeof(DataTemplate),
                typeof(HamburgerMenuEx));

        public DataTemplate HeaderTemplate
        {
            get => (DataTemplate)GetValue(HeaderTemplateProperty);
            set => SetValue(HeaderTemplateProperty, value);
        }

        #endregion

        public event EventHandler<HamburgerMenuBackRequestedEventArgs> BackRequested;

        public event EventHandler<HamburgerMenuDisplayModeChangedEventArgs> DisplayModeChanged;

        public event EventHandler PaneOpened;

        public event EventHandler PaneClosed;

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            if (m_backButton != null)
            {
                m_backButton.Click -= OnBackButtonClicked;
            }

            m_backButton = GetTemplateChild(c_navViewBackButton) as Button;

            if (m_backButton != null)
            {
                m_backButton.Click += OnBackButtonClicked;
            }
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
            ((HamburgerMenuEx)d).UpdatePaneLength();
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

            if ((bool)e.NewValue)
            {
                PaneOpened?.Invoke(this, EventArgs.Empty);
            }
            else
            {
                PaneClosed?.Invoke(this, EventArgs.Empty);
            }
        }

        private Button m_backButton;
    }
}
