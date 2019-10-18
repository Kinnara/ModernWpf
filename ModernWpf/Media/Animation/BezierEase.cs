// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Media.Animation;

namespace ModernWpf.Media.Animation
{
    public class BezierEase : EasingFunctionBase
    {
        static BezierEase()
        {
            EasingModeProperty.OverrideMetadata(typeof(BezierEase), new PropertyMetadata(EasingMode.EaseIn));
        }

        public static readonly DependencyProperty ControlPoint1Property =
            DependencyProperty.Register(
                nameof(ControlPoint1),
                typeof(Point),
                typeof(BezierEase),
                new PropertyMetadata(new Point(0, 0), OnControlPointChanged));

        public Point ControlPoint1
        {
            get => (Point)GetValue(ControlPoint1Property);
            set => SetValue(ControlPoint1Property, value);
        }

        public static readonly DependencyProperty ControlPoint2Property =
            DependencyProperty.Register(
                nameof(ControlPoint2),
                typeof(Point),
                typeof(BezierEase),
                new PropertyMetadata(new Point(1, 1), OnControlPointChanged));

        public Point ControlPoint2
        {
            get => (Point)GetValue(ControlPoint2Property);
            set => SetValue(ControlPoint2Property, value);
        }

        protected override Freezable CreateInstanceCore()
        {
            return new BezierEase();
        }

        protected override double EaseInCore(double normalizedTime)
        {
            return GetSplineProgress(normalizedTime);
        }

        protected override void OnChanged()
        {
            _isDirty = true;

            base.OnChanged();
        }

        /// <summary>
        /// Calculates spline progress from a linear progress.
        /// </summary>
        /// <param name="linearProgress">the linear progress</param>
        /// <returns>the spline progress</returns>
        private double GetSplineProgress(double linearProgress)
        {
            ReadPreamble();

            if (_isDirty)
            {
                Build();
            }

            if (!_isSpecified)
            {
                return linearProgress;
            }
            else
            {
                SetParameterFromX(linearProgress);

                return GetBezierValue(_By, _Cy, _parameter);
            }
        }

        /// <summary>
        /// Compute cached coefficients.
        /// </summary>
        private void Build()
        {
            Debug.Assert(_isDirty);

            var controlPoint1 = ControlPoint1;
            var controlPoint2 = ControlPoint2;

            if (controlPoint1 == new Point(0, 0)
                && controlPoint2 == new Point(1, 1))
            {
                // This KeySpline would have no effect on the progress.

                _isSpecified = false;
            }
            else
            {
                _isSpecified = true;

                _parameter = 0;

                // X coefficients
                _Bx = 3 * controlPoint1.X;
                _Cx = 3 * controlPoint2.X;
                _Cx_Bx = 2 * (_Cx - _Bx);
                _three_Cx = 3 - _Cx;

                // Y coefficients
                _By = 3 * controlPoint1.Y;
                _Cy = 3 * controlPoint2.Y;
            }

            _isDirty = false;
        }

        /// <summary>
        /// Get an X or Y value with the Bezier formula.
        /// </summary>
        /// <param name="b">the second Bezier coefficient</param>
        /// <param name="c">the third Bezier coefficient</param>
        /// <param name="t">the parameter value to evaluate at</param>
        /// <returns>the value of the Bezier function at the given parameter</returns>
        static private double GetBezierValue(double b, double c, double t)
        {
            double s = 1.0 - t;
            double t2 = t * t;

            return b * t * s * s + c * t2 * s + t2 * t;
        }

        /// <summary>
        /// Get X and dX/dt at a given parameter
        /// </summary>
        /// <param name="t">the parameter value to evaluate at</param>
        /// <param name="x">the value of x there</param>
        /// <param name="dx">the value of dx/dt there</param>
        private void GetXAndDx(double t, out double x, out double dx)
        {
            Debug.Assert(_isSpecified);

            double s = 1.0 - t;
            double t2 = t * t;
            double s2 = s * s;

            x = _Bx * t * s2 + _Cx * t2 * s + t2 * t;
            dx = _Bx * s2 + _Cx_Bx * s * t + _three_Cx * t2;
        }

        /// <summary>
        /// Compute the parameter value that corresponds to a given X value, using a modified
        /// clamped Newton-Raphson algorithm to solve the equation X(t) - time = 0. We make 
        /// use of some known properties of this particular function:
        /// * We are only interested in solutions in the interval [0,1]
        /// * X(t) is increasing, so we can assume that if X(t) > time t > solution.  We use
        ///   that to clamp down the search interval with every probe.
        /// * The derivative of X and Y are between 0 and 3.
        /// </summary>
        /// <param name="time">the time, scaled to fit in [0,1]</param>
        private void SetParameterFromX(double time)
        {
            Debug.Assert(_isSpecified);

            // Dynamic search interval to clamp with
            double bottom = 0;
            double top = 1;

            if (time == 0)
            {
                _parameter = 0;
            }
            else if (time == 1)
            {
                _parameter = 1;
            }
            else
            {
                // Loop while improving the guess
                while (top - bottom > fuzz)
                {
                    double x, dx, absdx;

                    // Get x and dx/dt at the current parameter
                    GetXAndDx(_parameter, out x, out dx);
                    absdx = Math.Abs(dx);

                    // Clamp down the search interval, relying on the monotonicity of X(t)
                    if (x > time)
                    {
                        top = _parameter;      // because parameter > solution
                    }
                    else
                    {
                        bottom = _parameter;  // because parameter < solution
                    }

                    // The desired accuracy is in ultimately in y, not in x, so the
                    // accuracy needs to be multiplied by dx/dy = (dx/dt) / (dy/dt).
                    // But dy/dt <=3, so we omit that
                    if (Math.Abs(x - time) < accuracy * absdx)
                    {
                        break; // We're there
                    }

                    if (absdx > fuzz)
                    {
                        // Nonzero derivative, use Newton-Raphson to obtain the next guess
                        double next = _parameter - (x - time) / dx;

                        // If next guess is out of the search interval then clamp it in
                        if (next >= top)
                        {
                            _parameter = (_parameter + top) / 2;
                        }
                        else if (next <= bottom)
                        {
                            _parameter = (_parameter + bottom) / 2;
                        }
                        else
                        {
                            // Next guess is inside the search interval, accept it
                            _parameter = next;
                        }
                    }
                    else    // Zero derivative, halve the search interval
                    {
                        _parameter = (bottom + top) / 2;
                    }
                }
            }
        }

        private static void OnControlPointChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
        }

        #region Data

        // This structure is way to large for, well, a structure.  I think the
        // animation class can allocate some data structures for calculation
        // purposes only when needed that this class can just hold the two
        // points and the bool.

        // Control points
        private bool _isSpecified;
        private bool _isDirty;

        // The parameter that corresponds to the most recent time
        private double _parameter;

        // Cached coefficients
        private double _Bx;        // 3*points[0].X
        private double _Cx;        // 3*points[1].X
        private double _Cx_Bx;     // 2*(Cx - Bx)
        private double _three_Cx;  // 3 - Cx

        private double _By;        // 3*points[0].Y
        private double _Cy;        // 3*points[1].Y

        // constants
        private const double accuracy = .001;   // 1/3 the desired accuracy in X
        private const double fuzz = .000001;    // computational zero

        #endregion
    }
}
