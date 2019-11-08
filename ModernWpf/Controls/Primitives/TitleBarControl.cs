using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace ModernWpf.Controls.Primitives
{
    [TemplatePart(Name = BackButtonName, Type = typeof(Button))]
    public class TitleBarControl : Control
    {
        private const string BackButtonName = "BackButton";

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

        #region ButtonBackground

        public static readonly DependencyProperty ButtonBackgroundProperty =
            TitleBar.ButtonBackgroundProperty.AddOwner(typeof(TitleBarControl));

        public Brush ButtonBackground
        {
            get => (Brush)GetValue(ButtonBackgroundProperty);
            set => SetValue(ButtonBackgroundProperty, value);
        }

        #endregion

        #region ButtonForeground

        public static readonly DependencyProperty ButtonForegroundProperty =
            TitleBar.ButtonForegroundProperty.AddOwner(typeof(TitleBarControl));

        public Brush ButtonForeground
        {
            get => (Brush)GetValue(ButtonForegroundProperty);
            set => SetValue(ButtonForegroundProperty, value);
        }

        #endregion

        #region ButtonHoverBackground

        public static readonly DependencyProperty ButtonHoverBackgroundProperty =
            TitleBar.ButtonHoverBackgroundProperty.AddOwner(typeof(TitleBarControl));

        public Brush ButtonHoverBackground
        {
            get => (Brush)GetValue(ButtonHoverBackgroundProperty);
            set => SetValue(ButtonHoverBackgroundProperty, value);
        }

        #endregion

        #region ButtonHoverForeground

        public static readonly DependencyProperty ButtonHoverForegroundProperty =
            TitleBar.ButtonHoverForegroundProperty.AddOwner(typeof(TitleBarControl));

        public Brush ButtonHoverForeground
        {
            get => (Brush)GetValue(ButtonHoverForegroundProperty);
            set => SetValue(ButtonHoverForegroundProperty, value);
        }

        #endregion

        #region ButtonPressedBackground

        public static readonly DependencyProperty ButtonPressedBackgroundProperty =
            TitleBar.ButtonPressedBackgroundProperty.AddOwner(typeof(TitleBarControl));

        public Brush ButtonPressedBackground
        {
            get => (Brush)GetValue(ButtonPressedBackgroundProperty);
            set => SetValue(ButtonPressedBackgroundProperty, value);
        }

        #endregion

        #region ButtonPressedForeground

        public static readonly DependencyProperty ButtonPressedForegroundProperty =
            TitleBar.ButtonPressedForegroundProperty.AddOwner(typeof(TitleBarControl));

        public Brush ButtonPressedForeground
        {
            get => (Brush)GetValue(ButtonPressedForegroundProperty);
            set => SetValue(ButtonPressedForegroundProperty, value);
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

        #region ButtonInactiveBackground

        public static readonly DependencyProperty ButtonInactiveBackgroundProperty =
            TitleBar.ButtonInactiveBackgroundProperty.AddOwner(typeof(TitleBarControl));

        public Brush ButtonInactiveBackground
        {
            get => (Brush)GetValue(ButtonInactiveBackgroundProperty);
            set => SetValue(ButtonInactiveBackgroundProperty, value);
        }

        #endregion

        #region ButtonInactiveForeground

        public static readonly DependencyProperty ButtonInactiveForegroundProperty =
            TitleBar.ButtonInactiveForegroundProperty.AddOwner(typeof(TitleBarControl));

        public Brush ButtonInactiveForeground
        {
            get => (Brush)GetValue(ButtonInactiveForegroundProperty);
            set => SetValue(ButtonInactiveForegroundProperty, value);
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
                null);

        public ImageSource Icon
        {
            get => (ImageSource)GetValue(IconProperty);
            set => SetValue(IconProperty, value);
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
        /// <returns>true if the back button is enabled; otherwise, false. The default is false.</returns>
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

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            if (BackButton != null)
            {
                BackButton.Click -= OnBackButtonClick;
            }

            BackButton = GetTemplateChild(BackButtonName) as Button;

            if (BackButton != null)
            {
                BackButton.Click += OnBackButtonClick;
            }
        }

        private void OnBackButtonClick(object sender, RoutedEventArgs e)
        {
            if (TemplatedParent is Window window)
            {
                var internalArgs = new BackRequestedEventArgs(TitleBar.InternalBackRequestedEvent, window);
                window.RaiseEvent(internalArgs);
                if (!internalArgs.Handled)
                {
                    TitleBar.RaiseBackRequested(window);
                }
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
