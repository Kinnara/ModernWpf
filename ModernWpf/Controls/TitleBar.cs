using ModernWpf.Controls.Primitives;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace ModernWpf.Controls
{
    [TemplatePart(Name = BackButtonName, Type = typeof(Button))]
    [StyleTypedProperty(Property = nameof(ButtonStyle), StyleTargetType = typeof(TitleBarButton))]
    public class TitleBar : Control
    {
        private const string BackButtonName = "BackButton";

        static TitleBar()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(TitleBar),
                new FrameworkPropertyMetadata(typeof(TitleBar)));
        }

        public TitleBar()
        {
            CommandBindings.Add(new CommandBinding(SystemCommands.MinimizeWindowCommand, MinimizeWindow));
            CommandBindings.Add(new CommandBinding(SystemCommands.MaximizeWindowCommand, MaximizeWindow));
            CommandBindings.Add(new CommandBinding(SystemCommands.RestoreWindowCommand, RestoreWindow));
            CommandBindings.Add(new CommandBinding(SystemCommands.CloseWindowCommand, CloseWindow));
        }

        #region InactiveBackground

        public static readonly DependencyProperty InactiveBackgroundProperty =
            DependencyProperty.Register(
                nameof(InactiveBackground),
                typeof(Brush),
                typeof(TitleBar),
                null);

        public Brush InactiveBackground
        {
            get => (Brush)GetValue(InactiveBackgroundProperty);
            set => SetValue(InactiveBackgroundProperty, value);
        }

        #endregion

        #region InactiveForeground

        public static readonly DependencyProperty InactiveForegroundProperty =
            DependencyProperty.Register(
                nameof(InactiveForeground),
                typeof(Brush),
                typeof(TitleBar),
                null);

        public Brush InactiveForeground
        {
            get => (Brush)GetValue(InactiveForegroundProperty);
            set => SetValue(InactiveForegroundProperty, value);
        }

        #endregion

        #region Title

        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register(
                nameof(Title),
                typeof(string),
                typeof(TitleBar),
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
                typeof(TitleBar),
                null);

        public ImageSource Icon
        {
            get => (ImageSource)GetValue(IconProperty);
            set => SetValue(IconProperty, value);
        }

        #endregion

        #region IsIconVisible

        public static readonly DependencyProperty IsIconVisibleProperty =
            DependencyProperty.RegisterAttached(
                nameof(IsIconVisible),
                typeof(bool),
                typeof(TitleBar),
                new PropertyMetadata(false));

        public bool IsIconVisible
        {
            get => (bool)GetValue(IsIconVisibleProperty);
            set => SetValue(IsIconVisibleProperty, value);
        }

        public static bool GetIsIconVisible(Window window)
        {
            return (bool)window.GetValue(IsIconVisibleProperty);
        }

        public static void SetIsIconVisible(Window window, bool value)
        {
            window.SetValue(IsIconVisibleProperty, value);
        }

        #endregion

        #region IsBackButtonVisible

        public static readonly DependencyProperty IsBackButtonVisibleProperty =
            DependencyProperty.RegisterAttached(
                nameof(IsBackButtonVisible),
                typeof(bool),
                typeof(TitleBar),
                null);

        public bool IsBackButtonVisible
        {
            get => (bool)GetValue(IsBackButtonVisibleProperty);
            set => SetValue(IsBackButtonVisibleProperty, value);
        }

        public static bool GetIsBackButtonVisible(Window window)
        {
            return (bool)window.GetValue(IsBackButtonVisibleProperty);
        }

        public static void SetIsBackButtonVisible(Window window, bool value)
        {
            window.SetValue(IsBackButtonVisibleProperty, value);
        }

        #endregion

        #region IsBackEnabled

        /// <summary>
        /// Identifies the IsBackEnabled attached property.
        /// </summary>
        public static readonly DependencyProperty IsBackEnabledProperty =
            DependencyProperty.RegisterAttached(
                nameof(IsBackEnabled),
                typeof(bool),
                typeof(TitleBar),
                null);

        /// <summary>
        /// Gets or sets a value that indicates whether the back button is enabled or disabled.
        /// </summary>
        /// <returns>true if the back button is enabled; otherwise, false. The default is false.</returns>
        public bool IsBackEnabled
        {
            get => (bool)GetValue(IsBackEnabledProperty);
            set => SetValue(IsBackEnabledProperty, value);
        }

        /// <summary>
        /// Gets a value that indicates whether the back button is enabled or disabled.
        /// </summary>
        /// <param name="window">The element from which to read the property value.</param>
        /// <returns>true if the back button is enabled; otherwise, false. The default is false.</returns>
        public static bool GetIsBackEnabled(Window window)
        {
            return (bool)window.GetValue(IsBackEnabledProperty);
        }

        /// <summary>
        /// Sets a value that indicates whether the back button is enabled or disabled.
        /// </summary>
        /// <param name="window">The element on which to set the attached property.</param>
        /// <param name="value">The property value to set.</param>
        public static void SetIsBackEnabled(Window window, bool value)
        {
            window.SetValue(IsBackEnabledProperty, value);
        }

        #endregion

        #region BackButtonCommand

        public static readonly DependencyProperty BackButtonCommandProperty =
            DependencyProperty.RegisterAttached(
                "BackButtonCommand",
                typeof(ICommand),
                typeof(TitleBar));

        public ICommand BackButtonCommand
        {
            get => (ICommand)GetValue(BackButtonCommandProperty);
            set => SetValue(BackButtonCommandProperty, value);
        }

        public static ICommand GetBackButtonCommand(Window window)
        {
            return (ICommand)window.GetValue(BackButtonCommandProperty);
        }

        public static void SetBackButtonCommand(Window window, ICommand value)
        {
            window.SetValue(BackButtonCommandProperty, value);
        }

        #endregion

        #region BackButtonCommandParameter

        public static readonly DependencyProperty BackButtonCommandParameterProperty =
            DependencyProperty.RegisterAttached(
                "BackButtonCommandParameter",
                typeof(object),
                typeof(TitleBar));

        public object BackButtonCommandParameter
        {
            get => GetValue(BackButtonCommandParameterProperty);
            set => SetValue(BackButtonCommandParameterProperty, value);
        }

        public static object GetBackButtonCommandParameter(Window window)
        {
            return window.GetValue(BackButtonCommandParameterProperty);
        }

        public static void SetBackButtonCommandParameter(Window window, object value)
        {
            window.SetValue(BackButtonCommandParameterProperty, value);
        }

        #endregion

        #region ButtonStyle

        public static readonly DependencyProperty ButtonStyleProperty =
            DependencyProperty.RegisterAttached(
                nameof(ButtonStyle),
                typeof(Style),
                typeof(TitleBar),
                null);

        public Style ButtonStyle
        {
            get => (Style)GetValue(ButtonStyleProperty);
            set => SetValue(ButtonStyleProperty, value);
        }

        public static Style GetButtonStyle(Window window)
        {
            return (Style)window.GetValue(ButtonStyleProperty);
        }

        public static void SetButtonStyle(Window window, Style value)
        {
            window.SetValue(ButtonStyleProperty, value);
        }

        #endregion

        #region TitleBarStyle

        public static readonly DependencyProperty TitleBarStyleProperty =
            DependencyProperty.RegisterAttached(
                "TitleBarStyle",
                typeof(Style),
                typeof(TitleBar));

        public static Style GetTitleBarStyle(Window window)
        {
            return (Style)window.GetValue(TitleBarStyleProperty);
        }

        public static void SetTitleBarStyle(Window window, Style value)
        {
            window.SetValue(TitleBarStyleProperty, value);
        }

        #endregion

        #region BackRequested

        /// <summary>
        /// Identifies the BackRequested routed event.
        /// </summary>
        public static readonly RoutedEvent BackRequestedEvent =
            EventManager.RegisterRoutedEvent(
                "BackRequested",
                RoutingStrategy.Bubble,
                typeof(EventHandler<BackRequestedEventArgs>),
                typeof(TitleBar));

        public static void AddBackRequestedHandler(Window window, EventHandler<BackRequestedEventArgs> handler)
        {
            window.AddHandler(BackRequestedEvent, handler);
        }

        public static void RemoveBackRequestedHandler(Window window, EventHandler<BackRequestedEventArgs> handler)
        {
            window.RemoveHandler(BackRequestedEvent, handler);
        }

        internal static void RaiseBackRequested(Window window)
        {
            window.RaiseEvent(new BackRequestedEventArgs(window));
        }

        #endregion

        #region InternalBackRequested

        internal static readonly RoutedEvent InternalBackRequestedEvent =
            EventManager.RegisterRoutedEvent(
                "InternalBackRequested",
                RoutingStrategy.Tunnel,
                typeof(EventHandler<BackRequestedEventArgs>),
                typeof(TitleBar));

        internal static void AddInternalBackRequestedHandler(Window window, EventHandler<BackRequestedEventArgs> handler)
        {
            window.AddHandler(InternalBackRequestedEvent, handler);
        }

        internal static void RemoveInternalBackRequestedHandler(Window window, EventHandler<BackRequestedEventArgs> handler)
        {
            window.RemoveHandler(InternalBackRequestedEvent, handler);
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
                var internalArgs = new BackRequestedEventArgs(InternalBackRequestedEvent, window);
                window.RaiseEvent(internalArgs);
                if (!internalArgs.Handled)
                {
                    RaiseBackRequested(window);
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
