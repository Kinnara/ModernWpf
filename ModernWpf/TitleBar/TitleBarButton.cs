using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
using Standard;
using static Windows.Win32.PInvoke;

namespace ModernWpf.Controls.Primitives
{
    public class TitleBarButton : Button
    {
        private HwndSource _parentHwndSource;

        static TitleBarButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(TitleBarButton),
                new FrameworkPropertyMetadata(typeof(TitleBarButton)));
        }

        public TitleBarButton()
        {
            if (OSVersionHelper.IsWindows11OrGreater)
            {
                Loaded += OnLoaded;
                Unloaded += OnUnloaded;
            }
        }

        #region IsActive

        public static readonly DependencyProperty IsActiveProperty =
            DependencyProperty.Register(
                nameof(IsActive),
                typeof(bool),
                typeof(TitleBarButton),
                new PropertyMetadata(false));

        public bool IsActive
        {
            get => (bool)GetValue(IsActiveProperty);
            set => SetValue(IsActiveProperty, value);
        }

        #endregion

        #region InactiveBackground

        public static readonly DependencyProperty InactiveBackgroundProperty =
            DependencyProperty.Register(
                nameof(InactiveBackground),
                typeof(Brush),
                typeof(TitleBarButton),
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
                typeof(TitleBarButton),
                null);

        public Brush InactiveForeground
        {
            get => (Brush)GetValue(InactiveForegroundProperty);
            set => SetValue(InactiveForegroundProperty, value);
        }

        #endregion

        #region HoverBackground

        public Brush HoverBackground
        {
            get => (Brush)GetValue(HoverBackgroundProperty);
            set => SetValue(HoverBackgroundProperty, value);
        }

        public static readonly DependencyProperty HoverBackgroundProperty = DependencyProperty.Register(
            nameof(HoverBackground),
            typeof(Brush),
            typeof(TitleBarButton),
            null);

        #endregion

        #region HoverForeground

        public Brush HoverForeground
        {
            get => (Brush)GetValue(HoverForegroundProperty);
            set => SetValue(HoverForegroundProperty, value);
        }

        public static readonly DependencyProperty HoverForegroundProperty = DependencyProperty.Register(
            nameof(HoverForeground),
            typeof(Brush),
            typeof(TitleBarButton),
            null);

        #endregion

        #region PressedBackground

        public Brush PressedBackground
        {
            get => (Brush)GetValue(PressedBackgroundProperty);
            set => SetValue(PressedBackgroundProperty, value);
        }

        public static readonly DependencyProperty PressedBackgroundProperty = DependencyProperty.Register(
            nameof(PressedBackground),
            typeof(Brush),
            typeof(TitleBarButton),
            null);

        #endregion

        #region PressedForeground

        public Brush PressedForeground
        {
            get => (Brush)GetValue(PressedForegroundProperty);
            set => SetValue(PressedForegroundProperty, value);
        }

        public static readonly DependencyProperty PressedForegroundProperty = DependencyProperty.Register(
            nameof(PressedForeground),
            typeof(Brush),
            typeof(TitleBarButton),
            null);

        #endregion

        #region IsMouseReallyOver

        private static readonly DependencyPropertyKey IsMouseReallyOverPropertyKey =
            DependencyProperty.RegisterReadOnly(
                nameof(IsMouseReallyOver),
                typeof(bool),
                typeof(TitleBarButton),
                new PropertyMetadata(false));

        public static readonly DependencyProperty IsMouseReallyOverProperty = IsMouseReallyOverPropertyKey.DependencyProperty;

        public bool IsMouseReallyOver
        {
            get => (bool)GetValue(IsMouseReallyOverProperty);
            private set => SetValue(IsMouseReallyOverPropertyKey, value);
        }

        private void UpdateIsMouseReallyOver()
        {
            IsMouseReallyOver = IsMouseOver || IsNCMouseOver;
        }

        #endregion

        #region IsReallyPressed

        private static readonly DependencyPropertyKey IsReallyPressedPropertyKey =
            DependencyProperty.RegisterReadOnly(
                nameof(IsReallyPressed),
                typeof(bool),
                typeof(TitleBarButton),
                new PropertyMetadata(false));

        public static readonly DependencyProperty IsReallyPressedProperty = IsReallyPressedPropertyKey.DependencyProperty;

        public bool IsReallyPressed
        {
            get => (bool)GetValue(IsReallyPressedProperty);
            private set => SetValue(IsReallyPressedPropertyKey, value);
        }

        private void UpdateIsReallyPressed()
        {
            IsReallyPressed = IsPressed || IsNCPressed;
        }

        #endregion

        internal uint? HitTestCode { get; set; }

        #region IsNCMouseOver

        private bool _isNCMouseOver;

        private bool IsNCMouseOver
        {
            get { return _isNCMouseOver; }
            set
            {
                if (_isNCMouseOver != value)
                {
                    _isNCMouseOver = value;
                    OnIsNCMouseOverChanged();
                }
            }
        }

        private void OnIsNCMouseOverChanged()
        {
            UpdateIsMouseReallyOver();

            if (!IsNCMouseOver)
            {
                IsNCPressed = false;
            }
        }

        #endregion

        #region IsNCPressed

        private bool _isNCPressed;

        private bool IsNCPressed
        {
            get { return _isNCPressed; }
            set
            {
                if (_isNCPressed != value)
                {
                    _isNCPressed = value;
                    OnIsNCPressedChanged();
                }
            }
        }

        private void OnIsNCPressedChanged()
        {
            UpdateIsReallyPressed();
        }

        #endregion

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);

            if (e.Property == IsMouseOverProperty)
            {
                UpdateIsMouseReallyOver();
            }
            else if (e.Property == IsPressedProperty)
            {
                UpdateIsReallyPressed();
            }
        }

        internal void DoClick()
        {
            OnClick();
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (HitTestCode.HasValue)
            {
                _parentHwndSource = PresentationSource.FromVisual(this) as HwndSource;
                Debug.Assert(_parentHwndSource != null);
                if (_parentHwndSource != null)
                {
                    _parentHwndSource.AddHook(TitleBarButtonFilterMessage);
                }
            }
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            if (_parentHwndSource != null)
            {
                _parentHwndSource.RemoveHook(TitleBarButtonFilterMessage);
                _parentHwndSource = null;
            }
        }

        private IntPtr TitleBarButtonFilterMessage(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            switch ((uint)msg)
            {
                case WM_NCHITTEST:
                    if (IsMousePositionWithin(lParam))
                    {
                        IsNCMouseOver = true;
                        handled = true;
                        return (IntPtr)HitTestCode;
                    }
                    else
                    {
                        IsNCMouseOver = false;
                    }
                    break;

                case WM_NCLBUTTONDOWN:
                    if (IsNCMouseOver)
                    {
                        IsNCPressed = true;
                        handled = true;
                    }
                    break;

                case WM_NCLBUTTONUP:
                    if (IsNCPressed)
                    {
                        IsNCPressed = false;

                        if (IsMousePositionWithin(lParam))
                        {
                            OnClick();
                        }

                        handled = true;
                    }
                    break;

                case WM_NCMOUSELEAVE:
                    IsNCMouseOver = false;
                    break;
            }

            return IntPtr.Zero;
        }

        private bool IsMousePositionWithin(IntPtr lParam)
        {
            var mousePosScreen = new Point(Utility.GET_X_LPARAM(lParam), Utility.GET_Y_LPARAM(lParam));
            var bounds = new Rect(new Point(), RenderSize);
            var mousePosRelative = PointFromScreen(mousePosScreen);
            return bounds.Contains(mousePosRelative);
        }
    }
}
