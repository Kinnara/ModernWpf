using ModernWpf.Controls;
using System.Windows;
using System.Windows.Data;

namespace ModernWpf
{
    internal sealed class CoreApplicationViewTitleBar
    {
        private CoreApplicationViewTitleBar(Window owner)
        {
            _owner = owner;
            _listener = new Listener(this);
        }

        public bool ExtendViewIntoTitleBar
        {
            get => TitleBar.GetExtendViewIntoTitleBar(_owner);
            set => TitleBar.SetExtendViewIntoTitleBar(_owner, value);
        }

        public double Height => TitleBar.GetHeight(_owner);

        public bool IsVisible => true;

        public double SystemOverlayLeftInset => TitleBar.GetSystemOverlayLeftInset(_owner);

        public double SystemOverlayRightInset => TitleBar.GetSystemOverlayRightInset(_owner);

        public event TypedEventHandler<CoreApplicationViewTitleBar, object> IsVisibleChanged;
        public event TypedEventHandler<CoreApplicationViewTitleBar, object> LayoutMetricsChanged;

        private void RaiseIsVisibleChanged()
        {
            IsVisibleChanged?.Invoke(this, null);
        }

        private void RaiseLayoutMetricsChanged()
        {
            LayoutMetricsChanged?.Invoke(this, null);
        }

        #region TitleBar

        private static readonly DependencyProperty TitleBarProperty =
            DependencyProperty.RegisterAttached(
                "TitleBar",
                typeof(CoreApplicationViewTitleBar),
                typeof(CoreApplicationViewTitleBar));

        internal static CoreApplicationViewTitleBar GetTitleBar(Window window)
        {
            var value = (CoreApplicationViewTitleBar)window.GetValue(TitleBarProperty);
            if (value == null)
            {
                value = new CoreApplicationViewTitleBar(window);
                SetTitleBar(window, value);
            }
            return value;
        }

        internal static CoreApplicationViewTitleBar GetTitleBar(DependencyObject dependencyObject)
        {
            var window = Window.GetWindow(dependencyObject);
            if (window != null)
            {
                return GetTitleBar(window);
            }
            return null;
        }

        private static void SetTitleBar(Window window, CoreApplicationViewTitleBar value)
        {
            window.SetValue(TitleBarProperty, value);
        }

        #endregion

        private readonly Window _owner;
        private readonly Listener _listener;

        private class Listener : DependencyObject
        {
            public Listener(CoreApplicationViewTitleBar owner)
            {
                _owner = owner;

                var window = _owner._owner;
                BindingOperations.SetBinding(this, ExtendViewIntoTitleBarProperty,
                    new Binding { Path = new PropertyPath(TitleBar.ExtendViewIntoTitleBarProperty), Source = window });
                BindingOperations.SetBinding(this, HeightProperty,
                    new Binding { Path = new PropertyPath(TitleBar.HeightProperty), Source = window });
                BindingOperations.SetBinding(this, SystemOverlayLeftInsetProperty,
                    new Binding { Path = new PropertyPath(TitleBar.SystemOverlayLeftInsetProperty), Source = window });
                BindingOperations.SetBinding(this, SystemOverlayRightInsetProperty,
                    new Binding { Path = new PropertyPath(TitleBar.SystemOverlayRightInsetProperty), Source = window });
            }

            #region ExtendViewIntoTitleBar

            public static readonly DependencyProperty ExtendViewIntoTitleBarProperty =
                DependencyProperty.Register(
                    nameof(ExtendViewIntoTitleBar),
                    typeof(bool),
                    typeof(Listener),
                    new PropertyMetadata(OnExtendViewIntoTitleBarPropertyChanged));

            public bool ExtendViewIntoTitleBar
            {
                get => (bool)GetValue(ExtendViewIntoTitleBarProperty);
                set => SetValue(ExtendViewIntoTitleBarProperty, value);
            }

            private static void OnExtendViewIntoTitleBarPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
            {
                ((Listener)sender).OnExtendViewIntoTitleBarPropertyChanged(args);
            }

            private void OnExtendViewIntoTitleBarPropertyChanged(DependencyPropertyChangedEventArgs args)
            {
                _owner.RaiseLayoutMetricsChanged();
                _owner.RaiseIsVisibleChanged();
            }

            #endregion

            #region Height

            public static readonly DependencyProperty HeightProperty =
                DependencyProperty.Register(
                    nameof(Height),
                    typeof(double),
                    typeof(Listener),
                    new PropertyMetadata(OnHeightPropertyChanged));

            public double Height
            {
                get => (double)GetValue(HeightProperty);
                set => SetValue(HeightProperty, value);
            }

            private static void OnHeightPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
            {
                ((Listener)sender).OnHeightPropertyChanged(args);
            }

            private void OnHeightPropertyChanged(DependencyPropertyChangedEventArgs args)
            {
                _owner.RaiseLayoutMetricsChanged();
            }

            #endregion

            #region SystemOverlayLeftInset

            public static readonly DependencyProperty SystemOverlayLeftInsetProperty =
                DependencyProperty.Register(
                    nameof(SystemOverlayLeftInset),
                    typeof(double),
                    typeof(Listener),
                    new PropertyMetadata(OnSystemOverlayLeftInsetPropertyChanged));

            public double SystemOverlayLeftInset
            {
                get => (double)GetValue(SystemOverlayLeftInsetProperty);
                set => SetValue(SystemOverlayLeftInsetProperty, value);
            }

            private static void OnSystemOverlayLeftInsetPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
            {
                ((Listener)sender).OnSystemOverlayLeftInsetPropertyChanged(args);
            }

            private void OnSystemOverlayLeftInsetPropertyChanged(DependencyPropertyChangedEventArgs args)
            {
                _owner.RaiseLayoutMetricsChanged();
            }

            #endregion

            #region SystemOverlayRightInset

            public static readonly DependencyProperty SystemOverlayRightInsetProperty =
                DependencyProperty.Register(
                    nameof(SystemOverlayRightInset),
                    typeof(double),
                    typeof(Listener),
                    new PropertyMetadata(OnSystemOverlayRightInsetPropertyChanged));

            public double SystemOverlayRightInset
            {
                get => (double)GetValue(SystemOverlayRightInsetProperty);
                set => SetValue(SystemOverlayRightInsetProperty, value);
            }

            private static void OnSystemOverlayRightInsetPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
            {
                ((Listener)sender).OnSystemOverlayRightInsetPropertyChanged(args);
            }

            private void OnSystemOverlayRightInsetPropertyChanged(DependencyPropertyChangedEventArgs args)
            {
                _owner.RaiseLayoutMetricsChanged();
            }

            #endregion

            private readonly CoreApplicationViewTitleBar _owner;
        }
    }
}
