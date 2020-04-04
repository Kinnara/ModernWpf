using System;
using System.Windows;

namespace ModernWpf.SampleApp.ThreadedUI
{
    public class ThreadedVisualHost : ThreadedVisualHostBase
    {
        public ThreadedVisualHost()
        {
            DataContextChanged += OnDataContextChanged;
            ThemeManager.AddActualThemeChangedHandler(this, OnActualThemeChanged);
        }

        #region ChildType

        public static readonly DependencyProperty ChildTypeProperty =
            DependencyProperty.Register(
                nameof(ChildType),
                typeof(Type),
                typeof(ThreadedVisualHost),
                new PropertyMetadata(OnChildTypeChanged));

        public Type ChildType
        {
            get => (Type)GetValue(ChildTypeProperty);
            set => SetValue(ChildTypeProperty, value);
        }

        private static void OnChildTypeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = (ThreadedVisualHost)d;
            ctrl._childType = (Type)e.NewValue;
            ctrl.InvalidateChild();
        }

        private Type _childType;

        #endregion

        public UIElement Child => ChildInternal;

        protected override UIElement CreateChild()
        {
            UIElement child = null;

            if (_childType != null)
            {
                child = Activator.CreateInstance(_childType) as UIElement;

                if (child is FrameworkElement fe)
                {
                    fe.DataContext = _dataContext;
                }
            }

            return child;
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            _dataContext = e.NewValue;

            if (ChildInternal is FrameworkElement fe)
            {
                fe.Dispatcher.Invoke(() => fe.DataContext = _dataContext);
            }
        }

        private void OnActualThemeChanged(object sender, RoutedEventArgs e)
        {
            if (Child is FrameworkElement fe)
            {
                fe.Dispatcher.Invoke(() =>
                {
                    // Invalidates all the properties on the nodes in the given sub-tree
                    var resources = fe.Resources;
                    if (resources.MergedDictionaries.Count == 0)
                    {
                        resources.MergedDictionaries.Clear();
                    }
                    else
                    {
                        var rd = new ResourceDictionary();
                        resources.MergedDictionaries.Add(rd);
                        rd.MergedDictionaries.Clear();
                        resources.MergedDictionaries.Remove(rd);
                    }
                });
            }
        }

        private object _dataContext;
    }
}
