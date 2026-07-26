using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shell;
using Standard;
using static Windows.Win32.PInvoke;

namespace ModernWpf.Controls.Primitives
{
    [TemplatePart(Name = BackButtonName, Type = typeof(Button))]
    [TemplatePart(Name = MaximizeRestoreButtonName, Type = typeof(TitleBarButton))]
    [TemplatePart(Name = LeftSystemOverlayName, Type = typeof(FrameworkElement))]
    [TemplatePart(Name = RightSystemOverlayName, Type = typeof(FrameworkElement))]
    [StyleTypedProperty(Property = nameof(ButtonStyle), StyleTargetType = typeof(TitleBarButton))]
    [StyleTypedProperty(Property = nameof(BackButtonStyle), StyleTargetType = typeof(TitleBarButton))]
    public class TitleBarControl : Control
    {
        private const string BackButtonName = "PART_BackButton";
        private const string MaximizeRestoreButtonName = "PART_MaximizeRestoreButton";
        private const string LeftSystemOverlayName = "PART_LeftSystemOverlay";
        private const string RightSystemOverlayName = "PART_RightSystemOverlay";

        private static readonly DependencyPropertyDescriptor WindowChromePropertyDescriptor =
            DependencyPropertyDescriptor.FromProperty(WindowChrome.WindowChromeProperty, typeof(Window));

        private Window _parentWindow;
        private HwndSource _parentHwndSource;
        private KeyBinding _altLeftBinding;

        static TitleBarControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(TitleBarControl),
                new FrameworkPropertyMetadata(typeof(TitleBarControl)));
        }

        public TitleBarControl()
        {
            CommandBindings.Add(new CommandBinding(
                SystemCommands.MinimizeWindowCommand,
                MinimizeWindow,
                CanMinimizeWindow));
            CommandBindings.Add(new CommandBinding(SystemCommands.MaximizeWindowCommand, MaximizeWindow));
            CommandBindings.Add(new CommandBinding(SystemCommands.RestoreWindowCommand, RestoreWindow));
            CommandBindings.Add(new CommandBinding(SystemCommands.CloseWindowCommand, CloseWindow));

            SetInsideTitleBar(this, true);

            Loaded += OnLoaded;
            Unloaded += OnUnloaded;
        }

        #region IsActive

        public static readonly DependencyProperty IsActiveProperty =
            DependencyProperty.Register(
                nameof(IsActive),
                typeof(bool),
                typeof(TitleBarControl),
                new PropertyMetadata(false, OnVisualStatePropertyChanged));

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
                new PropertyMetadata(string.Empty, OnVisualStatePropertyChanged));

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
            TitleBar.IsIconVisibleProperty.AddOwner(
                typeof(TitleBarControl),
                new PropertyMetadata(false, OnVisualStatePropertyChanged));

        public bool IsIconVisible
        {
            get => (bool)GetValue(IsIconVisibleProperty);
            set => SetValue(IsIconVisibleProperty, value);
        }

        #endregion

        #region IsBackButtonVisible

        public static readonly DependencyProperty IsBackButtonVisibleProperty =
            TitleBar.IsBackButtonVisibleProperty.AddOwner(
                typeof(TitleBarControl),
                new PropertyMetadata(false, OnVisualStatePropertyChanged));

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
            TitleBar.ExtendViewIntoTitleBarProperty.AddOwner(
                typeof(TitleBarControl),
                new PropertyMetadata(false, OnVisualStatePropertyChanged));

        public bool ExtendViewIntoTitleBar
        {
            get => (bool)GetValue(ExtendViewIntoTitleBarProperty);
            set => SetValue(ExtendViewIntoTitleBarProperty, value);
        }

        #endregion

        #region InsideTitleBar

        internal static readonly DependencyProperty InsideTitleBarProperty =
            DependencyProperty.RegisterAttached(
                "InsideTitleBar",
                typeof(bool),
                typeof(TitleBarControl),
                new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.Inherits));

        internal static bool GetInsideTitleBar(UIElement element)
        {
            return (bool)element.GetValue(InsideTitleBarProperty);
        }

        internal static void SetInsideTitleBar(UIElement element, bool value)
        {
            element.SetValue(InsideTitleBarProperty, value);
        }

        #endregion

        private Button BackButton { get; set; }

        private TitleBarButton MaximizeRestoreButton { get; set; }

        private FrameworkElement LeftSystemOverlay { get; set; }

        private FrameworkElement RightSystemOverlay { get; set; }

        public override void OnApplyTemplate()
        {
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

            base.OnApplyTemplate();

            BackButton = GetTemplateChild(BackButtonName) as Button;
            MaximizeRestoreButton = GetTemplateChild(MaximizeRestoreButtonName) as TitleBarButton;
            LeftSystemOverlay = GetTemplateChild(LeftSystemOverlayName) as FrameworkElement;
            RightSystemOverlay = GetTemplateChild(RightSystemOverlayName) as FrameworkElement;

            if (BackButton != null)
            {
                BackButton.Click += OnBackButtonClick;
            }

            if (MaximizeRestoreButton != null)
            {
                MaximizeRestoreButton.HitTestCode = HTMAXBUTTON;
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

            UpdateVisualStates(false);
        }

        protected override void OnInitialized(EventArgs e)
        {
            UpdateActualIcon();
            base.OnInitialized(e);
        }

        protected override AutomationPeer OnCreateAutomationPeer()
        {
            return new TitleBarControlAutomationPeer(this);
        }

        protected override void OnVisualParentChanged(DependencyObject oldParent)
        {
            if (_parentWindow != null)
            {
                WindowChromePropertyDescriptor.RemoveValueChanged(
                    _parentWindow,
                    OnWindowChromeChanged);

                if (_altLeftBinding != null)
                {
                    _parentWindow.InputBindings.Remove(_altLeftBinding);
                    _altLeftBinding = null;
                }
            }

            base.OnVisualParentChanged(oldParent);

            _parentWindow = TemplatedParent as Window;

            if (_parentWindow != null)
            {
                UpdateWindowChromeCaptionHeight();

                _altLeftBinding = new KeyBinding(new GoBackCommand(this), Key.Left, ModifierKeys.Alt);
                _parentWindow.InputBindings.Add(_altLeftBinding);
            }
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);

            if (TemplatedParent is Window window)
            {
                TitleBar.SetHeight(window, sizeInfo.NewSize.Height);
                UpdateWindowChromeCaptionHeight();
            }
        }

        private void OnWindowChromeChanged(object sender, EventArgs e)
        {
            Dispatcher.BeginInvoke(new Action(UpdateWindowChromeCaptionHeight));
        }

        private void UpdateWindowChromeCaptionHeight()
        {
            if (_parentWindow != null &&
                WindowChrome.GetWindowChrome(_parentWindow) is { } chrome)
            {
                double height = TitleBar.GetHeight(_parentWindow);
                if (chrome.CaptionHeight != height)
                {
                    var valueSource = DependencyPropertyHelper.GetValueSource(
                        _parentWindow,
                        WindowChrome.WindowChromeProperty);
                    if (valueSource.BaseValueSource == BaseValueSource.Local &&
                        !valueSource.IsExpression)
                    {
                        chrome.CaptionHeight = height;
                    }
                    else
                    {
                        var updatedChrome = (WindowChrome)chrome.CloneCurrentValue();
                        updatedChrome.CaptionHeight = height;
                        _parentWindow.SetCurrentValue(
                            WindowChrome.WindowChromeProperty,
                            updatedChrome);
                    }
                }
            }
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (_parentWindow == null)
            {
                return;
            }

            WindowChromePropertyDescriptor.RemoveValueChanged(
                _parentWindow,
                OnWindowChromeChanged);
            WindowChromePropertyDescriptor.AddValueChanged(
                _parentWindow,
                OnWindowChromeChanged);
            UpdateWindowChromeCaptionHeight();

            var hwndSource = PresentationSource.FromVisual(this) as HwndSource;
            if (!ReferenceEquals(hwndSource, _parentHwndSource))
            {
                RemoveWindowHook();
                _parentHwndSource = hwndSource;
                _parentHwndSource?.AddHook(FilterWindowMessage);
            }
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            if (_parentWindow != null)
            {
                WindowChromePropertyDescriptor.RemoveValueChanged(
                    _parentWindow,
                    OnWindowChromeChanged);
            }

            RemoveWindowHook();
        }

        private void RemoveWindowHook()
        {
            if (_parentHwndSource != null)
            {
                _parentHwndSource.RemoveHook(FilterWindowMessage);
                _parentHwndSource = null;
            }
        }

        private IntPtr FilterWindowMessage(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == WmNcHitTest &&
                _parentWindow != null &&
                IsVisible &&
                ActualHeight > 0)
            {
                var mousePosition = _parentWindow.PointFromScreen(
                    new Point(Utility.GET_X_LPARAM(lParam), Utility.GET_Y_LPARAM(lParam)));
                var titleBarBottom = TranslatePoint(new Point(0, ActualHeight), _parentWindow).Y;
                var chrome = WindowChrome.GetWindowChrome(_parentWindow);
                var resizeBorder = chrome?.ResizeBorderThickness ?? default;

                if (mousePosition.Y >= titleBarBottom &&
                    mousePosition.X >= resizeBorder.Left &&
                    mousePosition.X < _parentWindow.ActualWidth - resizeBorder.Right &&
                    mousePosition.Y < _parentWindow.ActualHeight - resizeBorder.Bottom)
                {
                    var inputElement = _parentWindow.InputHitTest(mousePosition);
                    if (inputElement == null ||
                        WindowChrome.GetResizeGripDirection(inputElement) == ResizeGripDirection.None)
                    {
                        handled = true;
                        return (IntPtr)HtClient;
                    }
                }
            }

            return IntPtr.Zero;
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
            if (TemplatedParent is Window window)
            {
                TitleBar.SetSystemOverlayLeftInset(window, value);
            }
        }

        private void UpdateSystemOverlayRightInset(double value)
        {
            if (TemplatedParent is Window window)
            {
                TitleBar.SetSystemOverlayRightInset(window, value);
            }
        }

        private static void OnVisualStatePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((TitleBarControl)d).UpdateVisualStates(false);
        }

        private void UpdateVisualStates(bool useTransitions)
        {
            VisualStateManager.GoToState(this, IsActive ? "Activated" : "Deactivated", useTransitions);
            VisualStateManager.GoToState(this, IsBackButtonVisible ? "BackButtonVisible" : "BackButtonCollapsed", useTransitions);
            VisualStateManager.GoToState(this, IsIconVisible ? "IconVisible" : "IconCollapsed", useTransitions);
            VisualStateManager.GoToState(this, string.IsNullOrEmpty(Title) ? "TitleTextCollapsed" : "TitleTextVisible", useTransitions);
            VisualStateManager.GoToState(this, ExtendViewIntoTitleBar ? "TitleContentCollapsed" : "TitleContentVisible", useTransitions);
        }

        private void MinimizeWindow(object sender, ExecutedRoutedEventArgs e)
        {
            if (TemplatedParent is Window window && CanMinimizeWindow(window))
            {
                SystemCommands.MinimizeWindow(window);
            }
        }

        private void CanMinimizeWindow(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = TemplatedParent is Window window && CanMinimizeWindow(window);
            e.Handled = true;
        }

        private static bool CanMinimizeWindow(Window window)
        {
            var handle = new WindowInteropHelper(window).Handle;
            if (handle == IntPtr.Zero)
            {
                return window.ResizeMode != ResizeMode.NoResize;
            }

            return (GetWindowLong(handle, GwlStyle) & WsMinimizeBox) != 0;
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

        private void InvokeBack()
        {
            InvokeButton(BackButton);
        }

        private static void InvokeButton(Button button)
        {
            if (button != null && button.IsEnabled)
            {
                if (button is TitleBarButton titleBarButton)
                {
                    titleBarButton.DoClick();
                }
                else
                {
                    if (UIElementAutomationPeer.CreatePeerForElement(button) is { } peer
                        && peer.GetPattern(PatternInterface.Invoke) is IInvokeProvider invokeProvider)
                    {
                        invokeProvider.Invoke();
                    }
                }
            }
        }

        private const int GwlStyle = -16;
        private const int WsMinimizeBox = 0x00020000;
        private const int WmNcHitTest = 0x0084;
        private const int HtClient = 1;

        [DllImport("user32.dll", EntryPoint = "GetWindowLongW")]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        private class GoBackCommand : ICommand
        {
            private readonly TitleBarControl _owner;

            public GoBackCommand(TitleBarControl owner)
            {
                _owner = owner;
            }

            public event EventHandler CanExecuteChanged
            {
                add { }
                remove { }
            }

            public bool CanExecute(object parameter)
            {
                return true;
            }

            public void Execute(object parameter)
            {
                _owner.InvokeBack();
            }
        }
    }
}
