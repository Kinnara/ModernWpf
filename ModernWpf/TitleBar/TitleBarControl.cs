using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ModernWpf.Controls.Primitives
{
    [TemplatePart(Name = BackButtonName, Type = typeof(Button))]
    [TemplatePart(Name = LeftSystemOverlayName, Type = typeof(FrameworkElement))]
    [TemplatePart(Name = RightSystemOverlayName, Type = typeof(FrameworkElement))]
    [StyleTypedProperty(Property = nameof(ButtonStyle), StyleTargetType = typeof(TitleBarButton))]
    [StyleTypedProperty(Property = nameof(BackButtonStyle), StyleTargetType = typeof(TitleBarButton))]
    public class TitleBarControl : Control
    {
        private const string BackButtonName = "PART_BackButton";
        private const string LeftSystemOverlayName = "PART_LeftSystemOverlay";
        private const string RightSystemOverlayName = "PART_RightSystemOverlay";

        static TitleBarControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(TitleBarControl),
                new FrameworkPropertyMetadata(typeof(TitleBarControl)));
        }

        public TitleBarControl()
        {
            CommandBindings.Add(new CommandBinding(SystemCommands.MinimizeWindowCommand, MinimizeWindow));
            CommandBindings.Add(new CommandBinding(SystemCommands.MaximizeWindowCommand, MaximizeWindow));
            CommandBindings.Add(new CommandBinding(SystemCommands.RestoreWindowCommand, RestoreWindow));
            CommandBindings.Add(new CommandBinding(SystemCommands.CloseWindowCommand, CloseWindow));
        }

        #region IsActive

        public static readonly DependencyProperty IsActiveProperty =
            DependencyProperty.Register(
                nameof(IsActive),
                typeof(bool),
                typeof(TitleBarControl),
                new PropertyMetadata(false));

        public bool IsActive
        {
            get => (bool)GetValue(IsActiveProperty);
            set => SetValue(IsActiveProperty, value);
        }

        #endregion

        #region InactiveBackground

        public static readonly DependencyProperty InactiveBackgroundProperty =
            TitleBar.InactiveBackgroundProperty.AddOwner(typeof(TitleBarControl));

        public Brush InactiveBackground
        {
            get => (Brush)GetValue(InactiveBackgroundProperty);
            set => SetValue(InactiveBackgroundProperty, value);
        }

        #endregion

        #region InactiveForeground

        public static readonly DependencyProperty InactiveForegroundProperty =
            TitleBar.InactiveForegroundProperty.AddOwner(typeof(TitleBarControl));

        public Brush InactiveForeground
        {
            get => (Brush)GetValue(InactiveForegroundProperty);
            set => SetValue(InactiveForegroundProperty, value);
        }

        #endregion

        #region ButtonStyle

        public static readonly DependencyProperty ButtonStyleProperty =
            TitleBar.ButtonStyleProperty.AddOwner(typeof(TitleBarControl));

        public Style ButtonStyle
        {
            get => (Style)GetValue(ButtonStyleProperty);
            set => SetValue(ButtonStyleProperty, value);
        }

        #endregion

        #region Title

        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register(
                nameof(Title),
                typeof(string),
                typeof(TitleBarControl),
                new PropertyMetadata(string.Empty));

        public string Title
        {
            get => (string)GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }

        #endregion

        #region Icon

        public static readonly DependencyProperty IconProperty =
            DependencyProperty.Register(
                nameof(Icon),
                typeof(ImageSource),
                typeof(TitleBarControl),
                new PropertyMetadata(OnIconChanged));

        public ImageSource Icon
        {
            get => (ImageSource)GetValue(IconProperty);
            set => SetValue(IconProperty, value);
        }

        private static void OnIconChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((TitleBarControl)d).UpdateActualIcon();
        }

        #endregion

        #region ActualIcon

        private static readonly DependencyPropertyKey ActualIconPropertyKey =
            DependencyProperty.RegisterReadOnly(
                nameof(ActualIcon),
                typeof(ImageSource),
                typeof(TitleBarControl),
                null);

        public static readonly DependencyProperty ActualIconProperty =
            ActualIconPropertyKey.DependencyProperty;

        public ImageSource ActualIcon
        {
            get => (ImageSource)GetValue(ActualIconProperty);
            private set => SetValue(ActualIconPropertyKey, value);
        }

        private void UpdateActualIcon()
        {
            if (Icon != null)
            {
                ActualIcon = Icon;
            }
            else
            {
                ImageSource actualIcon = null;

                var smallIconHandle = new IntPtr[1];
                IconHelper.GetDefaultIconHandles(null, smallIconHandle);
                var smallIcon = smallIconHandle[0];
                if (smallIcon != IntPtr.Zero)
                {
                    try
                    {
                        actualIcon = Imaging.CreateBitmapSourceFromHIcon(smallIcon, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                    }
                    finally
                    {
                        IconHelper.DestroyIcon(smallIcon);
                    }
                }

                ActualIcon = actualIcon;
            }
        }

        #endregion

        #region IsIconVisible

        public static readonly DependencyProperty IsIconVisibleProperty =
            TitleBar.IsIconVisibleProperty.AddOwner(typeof(TitleBarControl));

        public bool IsIconVisible
        {
            get => (bool)GetValue(IsIconVisibleProperty);
            set => SetValue(IsIconVisibleProperty, value);
        }

        #endregion

        #region IsBackButtonVisible

        public static readonly DependencyProperty IsBackButtonVisibleProperty =
            TitleBar.IsBackButtonVisibleProperty.AddOwner(typeof(TitleBarControl));

        public bool IsBackButtonVisible
        {
            get => (bool)GetValue(IsBackButtonVisibleProperty);
            set => SetValue(IsBackButtonVisibleProperty, value);
        }

        #endregion

        #region IsBackEnabled

        /// <summary>
        /// Identifies the IsBackEnabled attached property.
        /// </summary>
        public static readonly DependencyProperty IsBackEnabledProperty =
            TitleBar.IsBackEnabledProperty.AddOwner(typeof(TitleBarControl));

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
            TitleBar.BackButtonCommandProperty.AddOwner(typeof(TitleBarControl));

        public ICommand BackButtonCommand
        {
            get => (ICommand)GetValue(BackButtonCommandProperty);
            set => SetValue(BackButtonCommandProperty, value);
        }

        #endregion

        #region BackButtonCommandParameter

        public static readonly DependencyProperty BackButtonCommandParameterProperty =
            TitleBar.BackButtonCommandParameterProperty.AddOwner(typeof(TitleBarControl));

        public object BackButtonCommandParameter
        {
            get => GetValue(BackButtonCommandParameterProperty);
            set => SetValue(BackButtonCommandParameterProperty, value);
        }

        #endregion

        #region BackButtonCommandTarget

        public static readonly DependencyProperty BackButtonCommandTargetProperty =
            TitleBar.BackButtonCommandTargetProperty.AddOwner(typeof(TitleBarControl));

        public IInputElement BackButtonCommandTarget
        {
            get => (IInputElement)GetValue(BackButtonCommandTargetProperty);
            set => SetValue(BackButtonCommandTargetProperty, value);
        }

        #endregion

        #region BackButtonStyle

        public static readonly DependencyProperty BackButtonStyleProperty =
            TitleBar.BackButtonStyleProperty.AddOwner(typeof(TitleBarControl));

        public Style BackButtonStyle
        {
            get => (Style)GetValue(BackButtonStyleProperty);
            set => SetValue(BackButtonStyleProperty, value);
        }

        #endregion

        #region ExtendViewIntoTitleBar

        public static readonly DependencyProperty ExtendViewIntoTitleBarProperty =
            TitleBar.ExtendViewIntoTitleBarProperty.AddOwner(typeof(TitleBarControl));

        public bool ExtendViewIntoTitleBar
        {
            get => (bool)GetValue(ExtendViewIntoTitleBarProperty);
            set => SetValue(ExtendViewIntoTitleBarProperty, value);
        }

        #endregion

        private Button BackButton { get; set; }

        private FrameworkElement LeftSystemOverlay { get; set; }

        private FrameworkElement RightSystemOverlay { get; set; }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            if (BackButton != null)
            {
                BackButton.Click -= OnBackButtonClick;
            }

            if (LeftSystemOverlay != null)
            {
                LeftSystemOverlay.SizeChanged -= OnLeftSystemOverlaySizeChanged;
            }

            if (RightSystemOverlay != null)
            {
                RightSystemOverlay.SizeChanged -= OnRightSystemOverlaySizeChanged;
            }

            BackButton = GetTemplateChild(BackButtonName) as Button;
            LeftSystemOverlay = GetTemplateChild(LeftSystemOverlayName) as FrameworkElement;
            RightSystemOverlay = GetTemplateChild(RightSystemOverlayName) as FrameworkElement;

            if (BackButton != null)
            {
                BackButton.Click += OnBackButtonClick;
            }

            if (LeftSystemOverlay != null)
            {
                LeftSystemOverlay.SizeChanged += OnLeftSystemOverlaySizeChanged;
                UpdateSystemOverlayLeftInset(LeftSystemOverlay.ActualWidth);
            }

            if (RightSystemOverlay != null)
            {
                RightSystemOverlay.SizeChanged += OnRightSystemOverlaySizeChanged;
                UpdateSystemOverlayRightInset(RightSystemOverlay.ActualWidth);
            }
        }

        protected override void OnInitialized(EventArgs e)
        {
            UpdateActualIcon();
            base.OnInitialized(e);
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);

            Debug.Assert(TemplatedParent is Window);
            if (TemplatedParent is Window window)
            {
                TitleBar.SetHeight(window, sizeInfo.NewSize.Height);
            }
        }

        private void OnBackButtonClick(object sender, RoutedEventArgs e)
        {
            if (TemplatedParent is Window window)
            {
                TitleBar.RaiseBackRequested(window);
            }
        }

        private void OnLeftSystemOverlaySizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateSystemOverlayLeftInset(e.NewSize.Width);
        }

        private void OnRightSystemOverlaySizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateSystemOverlayRightInset(e.NewSize.Width);
        }

        private void UpdateSystemOverlayLeftInset(double value)
        {
            Debug.Assert(TemplatedParent is Window);
            if (TemplatedParent is Window window)
            {
                TitleBar.SetSystemOverlayLeftInset(window, value);
            }
        }

        private void UpdateSystemOverlayRightInset(double value)
        {
            Debug.Assert(TemplatedParent is Window);
            if (TemplatedParent is Window window)
            {
                TitleBar.SetSystemOverlayRightInset(window, value);
            }
        }

        private void MinimizeWindow(object sender, ExecutedRoutedEventArgs e)
        {
            if (TemplatedParent is Window window)
            {
                SystemCommands.MinimizeWindow(window);
            }
        }

        private void MaximizeWindow(object sender, ExecutedRoutedEventArgs e)
        {
            if (TemplatedParent is Window window)
            {
                SystemCommands.MaximizeWindow(window);
            }
        }

        private void RestoreWindow(object sender, ExecutedRoutedEventArgs e)
        {
            if (TemplatedParent is Window window)
            {
                SystemCommands.RestoreWindow(window);
            }
        }

        private void CloseWindow(object sender, ExecutedRoutedEventArgs e)
        {
            if (TemplatedParent is Window window)
            {
                SystemCommands.CloseWindow(window);
            }
        }
    }
}
