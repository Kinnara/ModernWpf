using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace ModernWpf.Controls.Primitives
{
    internal static class PlaneratorHelper
    {

        #region Dependency Properties
        #region RotationX
        public static double GetRotationX(FrameworkElement frameworkElement)
        {
            return (double)frameworkElement.GetValue(RotationXProperty);
        }

        public static void SetRotationX(FrameworkElement frameworkElement, double value)
        {
            frameworkElement.SetValue(RotationXProperty, value);
        }

        public static readonly DependencyProperty RotationXProperty =
            DependencyProperty.RegisterAttached(
                "RotationX",
                typeof(double),
                typeof(PlaneratorHelper), new PropertyMetadata(0.0));
        #endregion
        #region RotationY
        public static double GetRotationY(FrameworkElement frameworkElement)
        {
            return (double)frameworkElement.GetValue(RotationYProperty);
        }

        public static void SetRotationY(FrameworkElement frameworkElement, double value)
        {
            frameworkElement.SetValue(RotationYProperty, value);
        }

        public static readonly DependencyProperty RotationYProperty =
            DependencyProperty.RegisterAttached(
                "RotationY",
                typeof(double),
                typeof(PlaneratorHelper), new PropertyMetadata(0.0));
        #endregion
        #region RotationZ
        public static double GetRotationZ(FrameworkElement frameworkElement)
        {
            return (double)frameworkElement.GetValue(RotationZProperty);
        }

        public static void SetRotationZ(FrameworkElement frameworkElement, double value)
        {
            frameworkElement.SetValue(RotationZProperty, value);
        }

        public static readonly DependencyProperty RotationZProperty =
            DependencyProperty.RegisterAttached(
                "RotationZ",
                typeof(double),
                typeof(PlaneratorHelper), new PropertyMetadata(0.0));
        #endregion
        #region FieldOfView
        public static double GetFieldOfView(FrameworkElement frameworkElement)
        {
            return (double)frameworkElement.GetValue(FieldOfViewProperty);
        }

        public static void SetFieldOfView(FrameworkElement frameworkElement, double value)
        {
            frameworkElement.SetValue(FieldOfViewProperty, value);
        }

        public static readonly DependencyProperty FieldOfViewProperty =
            DependencyProperty.RegisterAttached(
                "FieldOfView",
                typeof(double),
                typeof(PlaneratorHelper), new PropertyMetadata(45.0));
        #endregion
        #region OriginX
        public static double GetOriginX(FrameworkElement frameworkElement)
        {
            return (double)frameworkElement.GetValue(OriginXProperty);
        }

        public static void SetOriginX(FrameworkElement frameworkElement, double value)
        {
            frameworkElement.SetValue(OriginXProperty, value);
        }

        public static readonly DependencyProperty OriginXProperty =
            DependencyProperty.RegisterAttached(
                "OriginX",
                typeof(double),
                typeof(PlaneratorHelper), new PropertyMetadata(0.5));
        #endregion
        #region OriginY
        public static double GetOriginY(FrameworkElement frameworkElement)
        {
            return (double)frameworkElement.GetValue(OriginYProperty);
        }

        public static void SetOriginY(FrameworkElement frameworkElement, double value)
        {
            frameworkElement.SetValue(OriginYProperty, value);
        }

        public static readonly DependencyProperty OriginYProperty =
            DependencyProperty.RegisterAttached(
                "OriginY",
                typeof(double),
                typeof(PlaneratorHelper), new PropertyMetadata(0.5));
        #endregion
        #region Depth
        public static double GetDepth(FrameworkElement frameworkElement)
        {
            return (double)frameworkElement.GetValue(DepthProperty);
        }

        public static void SetDepth(FrameworkElement frameworkElement, double value)
        {
            frameworkElement.SetValue(DepthProperty, value);
        }

        public static readonly DependencyProperty DepthProperty =
            DependencyProperty.RegisterAttached(
                "Depth",
                typeof(double),
                typeof(PlaneratorHelper), new PropertyMetadata(0.0));
        #endregion
        #region PlaceIn3D
        public static bool GetPlaceIn3D(FrameworkElement frameworkElement)
        {
            return (bool)frameworkElement.GetValue(PlaceIn3DProperty);
        }

        public static void SetPlaceIn3D(FrameworkElement frameworkElement, bool value)
        {
            frameworkElement.SetValue(PlaceIn3DProperty, value);
        }

        public static readonly DependencyProperty PlaceIn3DProperty =
            DependencyProperty.RegisterAttached(
                "PlaceIn3D",
                typeof(bool),
                typeof(PlaneratorHelper), new PropertyMetadata(false, OnPlaceIn3dPropertyChanged));
        #endregion
        #region RotatorParent
        public static Planerator GetRotatorParent(FrameworkElement frameworkElement)
        {
            return frameworkElement.GetValue(RotatorParentProperty) as Planerator;
        }

        public static void SetRotatorParent(FrameworkElement frameworkElement, Planerator value)
        {
            frameworkElement.SetValue(RotatorParentProperty, value);
        }

        public static readonly DependencyProperty RotatorParentProperty =
            DependencyProperty.RegisterAttached("RotatorParent", typeof(Planerator), typeof(PlaneratorHelper), new PropertyMetadata(null));
        #endregion
        #endregion

        private static void OnPlaceIn3dPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var fe = d as FrameworkElement;

            bool newValue = (bool)e.NewValue;
            bool oldValue = (bool)e.OldValue;

            if (fe == null) { return; }

            if (oldValue && !newValue)
            {
                GetItOut(fe);
            }
            if (!oldValue && newValue)
            {
                PlaceItIn3D(fe);
            }
        }

        private static void GetItOut(FrameworkElement fe)
        {
            Planerator RotatorParent = GetRotatorParent(fe);
            if (RotatorParent == null) { return; }
            Panel OriginalPanel = RotatorParent.Parent as Panel;
            Thickness OriginalMargin = RotatorParent.Margin;
            Size OriginalSize = new Size(RotatorParent.Width, RotatorParent.Height);
            BindingOperations.ClearBinding(RotatorParent, Planerator.RotationXProperty);
            BindingOperations.ClearBinding(RotatorParent, Planerator.RotationYProperty);
            BindingOperations.ClearBinding(RotatorParent, Planerator.RotationZProperty);
            BindingOperations.ClearBinding(RotatorParent, Planerator.FieldOfViewProperty);
            BindingOperations.ClearBinding(RotatorParent, Planerator.OriginXProperty);
            BindingOperations.ClearBinding(RotatorParent, Planerator.OriginYProperty);
            BindingOperations.ClearBinding(RotatorParent, Planerator.DepthProperty);

            RotatorParent.ClearChild();
            OriginalPanel.Children.Remove(RotatorParent);
            RotatorParent = null;

            fe.ClearValue(RotatorParentProperty);

            //fe.Width = OriginalSize.Width;
            //fe.Height = OriginalSize.Height;
            fe.Margin = OriginalMargin;

            OriginalPanel.Children.Add(fe);
        }

        private static void PlaceItIn3D(FrameworkElement fe)
        {

            Panel OriginalPanel = fe.Parent as Panel;
            Thickness OriginalMargin = fe.Margin;
            Size OriginalSize = new Size(fe.Width, fe.Height);
            double left = Canvas.GetLeft(fe);
            double right = Canvas.GetRight(fe);
            double top = Canvas.GetTop(fe);
            double bottom = Canvas.GetBottom(fe);
            int z = Canvas.GetZIndex(fe);
            VerticalAlignment va = fe.VerticalAlignment;
            HorizontalAlignment ha = fe.HorizontalAlignment;

            Planerator RotatorParent = new Planerator();
            RotatorParent.Width = OriginalSize.Width;
            RotatorParent.Height = OriginalSize.Height;
            RotatorParent.Margin = OriginalMargin;
            RotatorParent.VerticalAlignment = va;
            RotatorParent.HorizontalAlignment = ha;
            RotatorParent.SetValue(Canvas.LeftProperty, left);
            RotatorParent.SetValue(Canvas.RightProperty, right);
            RotatorParent.SetValue(Canvas.TopProperty, top);
            RotatorParent.SetValue(Canvas.BottomProperty, bottom);
            RotatorParent.SetValue(Canvas.ZIndexProperty, z);

            //fe.Width = double.NaN;
            //fe.Height = double.NaN;

            BindingOperations.SetBinding(RotatorParent, Planerator.RotationXProperty, new Binding() { Source = fe, Path = new PropertyPath(RotationXProperty) });
            BindingOperations.SetBinding(RotatorParent, Planerator.RotationYProperty, new Binding() { Source = fe, Path = new PropertyPath(RotationYProperty) });
            BindingOperations.SetBinding(RotatorParent, Planerator.RotationZProperty, new Binding() { Source = fe, Path = new PropertyPath(RotationZProperty) });
            BindingOperations.SetBinding(RotatorParent, Planerator.FieldOfViewProperty, new Binding() { Source = fe, Path = new PropertyPath(FieldOfViewProperty) });
            BindingOperations.SetBinding(RotatorParent, Planerator.OriginXProperty, new Binding() { Source = fe, Path = new PropertyPath(OriginXProperty) });
            BindingOperations.SetBinding(RotatorParent, Planerator.OriginYProperty, new Binding() { Source = fe, Path = new PropertyPath(OriginYProperty) });
            BindingOperations.SetBinding(RotatorParent, Planerator.DepthProperty, new Binding() { Source = fe, Path = new PropertyPath(DepthProperty) });

            OriginalPanel.Children.Remove(fe);
            fe.Margin = new Thickness();

            OriginalPanel.Children.Add(RotatorParent);
            SetRotatorParent(fe, RotatorParent);
            RotatorParent.Child = fe;
        }
    }
}
