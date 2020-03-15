using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;

namespace ModernWpf.Controls.Primitives
{
    public static class TiltEffect
    {

        #region IsPressed
        internal static bool GetIsPressed(FrameworkElement frameworkElement)
        {
            return (bool)frameworkElement.GetValue(IsPressedProperty);
        }

        internal static void SetIsPressed(FrameworkElement frameworkElement, bool value)
        {
            frameworkElement.SetValue(IsPressedProperty, value);
        }

        internal static readonly DependencyProperty IsPressedProperty =
            DependencyProperty.RegisterAttached(
                "IsPressed",
                typeof(bool),
                typeof(TiltEffect), new PropertyMetadata(false, OnIsPressedPropertyChanged));
        #endregion

        #region IsEnabled
        public static bool GetIsEnabled(FrameworkElement frameworkElement)
        {
            return (bool)frameworkElement.GetValue(IsEnabledProperty);
        }

        public static void SetIsEnabled(FrameworkElement frameworkElement, bool value)
        {
            frameworkElement.SetValue(IsEnabledProperty, value);
        }

        public static readonly DependencyProperty IsEnabledProperty =
            DependencyProperty.RegisterAttached(
                "IsEnabled",
                typeof(bool),
                typeof(TiltEffect), new PropertyMetadata(false));
        #endregion

        private static Duration DefaultDuration = TimeSpan.FromMilliseconds(300.0);

        private static double TiltFactor = 3.0;

        private static double Depth = 5.0;

        private static void OnIsPressedPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var fe = d as FrameworkElement;

            bool newValue = (bool)e.NewValue;
            bool oldValue = (bool)e.OldValue;

            if (fe == null) { return; }

            if (oldValue && !newValue)
            {
                var RP = PlaneratorHelper.GetRotatorParent(fe);
                if (RP == null) { return; }
                double yrot = 0;
                double xrot = 0;
                PrepareForCompletion(fe, DefaultDuration.TimeSpan);
                SetAnim(RP, Planerator.RotationYProperty, yrot);
                SetAnim(RP, Planerator.RotationXProperty, xrot);
                SetAnim(RP, Planerator.DepthProperty, 0);
            }

            if (!oldValue && newValue)
            {
                bool IsEnabled = GetIsEnabled(fe);

                if (IsEnabled)
                {
                    fe.BeginAnimation(PlaneratorHelper.PlaceIn3DProperty, null);
                    PlaneratorHelper.SetPlaceIn3D(fe, true);
                    var RP = PlaneratorHelper.GetRotatorParent(fe);
                    if (RP == null) { return; }
                    Point current = Mouse.GetPosition(RP);
                    bool IsPressed = Mouse.LeftButton == MouseButtonState.Pressed;
                    bool IsEnter = current.X > 0 && current.X < fe.ActualWidth && current.Y > 0 && current.Y < fe.ActualHeight;
                    if (IsEnter && IsPressed)
                    {
                        double yrot = -1 * TiltFactor + current.X * 2 * TiltFactor / fe.ActualWidth;
                        double xrot = -1 * TiltFactor + current.Y * 2 * TiltFactor / fe.ActualHeight;
                        SetAnim(RP, Planerator.RotationYProperty, yrot);
                        SetAnim(RP, Planerator.RotationXProperty, xrot);
                    }
                    SetAnim(RP, Planerator.DepthProperty, Depth);
                }
            }
        }

        private static void PrepareForCompletion(FrameworkElement fe, TimeSpan timeSpan)
        {
            var bauk = new BooleanAnimationUsingKeyFrames() { BeginTime = timeSpan, Duration = TimeSpan.FromMilliseconds(10) };
            bauk.KeyFrames.Add(new DiscreteBooleanKeyFrame(false, KeyTime.FromTimeSpan(TimeSpan.Zero)));
            fe.BeginAnimation(PlaneratorHelper.PlaceIn3DProperty, bauk);
        }

        private static void SetAnim(Planerator pl, DependencyProperty dp, double value)
        {
            DoubleAnimation da = new DoubleAnimation(value, DefaultDuration);
            da.EasingFunction = new CircleEase() { EasingMode = EasingMode.EaseOut };
            pl.BeginAnimation(dp, da);
        }
    }
}
