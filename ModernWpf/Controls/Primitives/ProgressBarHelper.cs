using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace ModernWpf.Controls.Primitives
{
    /// <summary>
    /// Provides calculated values that can be referenced as **TemplatedParent** sources
    /// when defining templates for a ProgressBar control. Not intended for general use.
    /// </summary>
    public static class ProgressBarHelper
    {
        #region ContainerAnimationStartPosition

        /// <summary>
        /// Gets the "From" point of the container animation that animates the ProgressBar.
        /// </summary>
        /// <param name="progressBar">The element from which to read the property value.</param>
        /// <returns>
        /// A double that represents the orientation-specific x- or y-value that is the "From"
        /// point of the animation.
        /// </returns>
        public static double GetContainerAnimationStartPosition(ProgressBar progressBar)
        {
            return (double)progressBar.GetValue(ContainerAnimationStartPositionProperty);
        }

        private static void SetContainerAnimationStartPosition(ProgressBar progressBar, double value)
        {
            progressBar.SetValue(ContainerAnimationStartPositionPropertyKey, value);
        }

        private static readonly DependencyPropertyKey ContainerAnimationStartPositionPropertyKey =
            DependencyProperty.RegisterAttachedReadOnly(
                "ContainerAnimationStartPosition",
                typeof(double),
                typeof(ProgressBarHelper),
                null);

        /// <summary>
        /// Identifies the ContainerAnimationStartPosition dependency property.
        /// </summary>
        public static readonly DependencyProperty ContainerAnimationStartPositionProperty = ContainerAnimationStartPositionPropertyKey.DependencyProperty;

        #endregion

        #region ContainerAnimationEndPosition

        /// <summary>
        /// Gets the target "To" point of the container animation that animates the ProgressBar.
        /// </summary>
        /// <param name="progressBar">The element from which to read the property value.</param>
        /// <returns>
        /// A double that represents the orientation-specific x- or y-value that is the target
        /// "To" point of the animation.
        /// </returns>
        public static double GetContainerAnimationEndPosition(ProgressBar progressBar)
        {
            return (double)progressBar.GetValue(ContainerAnimationEndPositionProperty);
        }

        private static void SetContainerAnimationEndPosition(ProgressBar progressBar, double value)
        {
            progressBar.SetValue(ContainerAnimationEndPositionPropertyKey, value);
        }

        private static readonly DependencyPropertyKey ContainerAnimationEndPositionPropertyKey =
            DependencyProperty.RegisterAttachedReadOnly(
                "ContainerAnimationEndPosition",
                typeof(double),
                typeof(ProgressBarHelper),
                null);

        /// <summary>
        /// Identifies the ContainerAnimationEndPosition dependency property.
        /// </summary>
        public static readonly DependencyProperty ContainerAnimationEndPositionProperty = ContainerAnimationEndPositionPropertyKey.DependencyProperty;

        #endregion

        #region EllipseAnimationWellPosition

        /// <summary>
        /// Gets the stopped point of the "Ellipse" animation that animates the ProgressBar.
        /// </summary>
        /// <param name="progressBar">The element from which to read the property value.</param>
        /// <returns>
        /// The stopped point of the Ellipse animation that animates the ProgressBar. This
        /// is internally calculated as 1/3 of the ActualWidth of the control.
        /// </returns>
        public static double GetEllipseAnimationWellPosition(ProgressBar progressBar)
        {
            return (double)progressBar.GetValue(EllipseAnimationWellPositionProperty);
        }

        private static void SetEllipseAnimationWellPosition(ProgressBar progressBar, double value)
        {
            progressBar.SetValue(EllipseAnimationWellPositionPropertyKey, value);
        }

        private static readonly DependencyPropertyKey EllipseAnimationWellPositionPropertyKey =
            DependencyProperty.RegisterAttachedReadOnly(
                "EllipseAnimationWellPosition",
                typeof(double),
                typeof(ProgressBarHelper),
                null);

        /// <summary>
        /// Identifies the EllipseAnimationWellPosition dependency property.
        /// </summary>
        public static readonly DependencyProperty EllipseAnimationWellPositionProperty = EllipseAnimationWellPositionPropertyKey.DependencyProperty;

        #endregion

        #region EllipseAnimationEndPosition

        /// <summary>
        /// Gets the "To" point of the "Ellipse" animation that animates the ProgressBar.
        /// </summary>
        /// <param name="progressBar">The element from which to read the property value.</param>
        /// <returns>
        /// The "To" point of the "Ellipse" animation that animates the ProgressBar. This
        /// is internally calculated as 2/3 of the ActualWidth of the control.
        /// </returns>
        public static double GetEllipseAnimationEndPosition(ProgressBar progressBar)
        {
            return (double)progressBar.GetValue(EllipseAnimationEndPositionProperty);
        }

        private static void SetEllipseAnimationEndPosition(ProgressBar progressBar, double value)
        {
            progressBar.SetValue(EllipseAnimationEndPositionPropertyKey, value);
        }

        private static readonly DependencyPropertyKey EllipseAnimationEndPositionPropertyKey =
            DependencyProperty.RegisterAttachedReadOnly(
                "EllipseAnimationEndPosition",
                typeof(double),
                typeof(ProgressBarHelper),
                null);

        /// <summary>
        /// Identifies the EllipseAnimationEndPosition dependency property.
        /// </summary>
        public static readonly DependencyProperty EllipseAnimationEndPositionProperty = EllipseAnimationEndPositionPropertyKey.DependencyProperty;

        #endregion

        #region EllipseDiameter

        /// <summary>
        /// Gets the template-defined diameter of the "Ellipse" element that is animated
        /// in a templated ProgressBar.
        /// </summary>
        /// <param name="progressBar">The element from which to read the property value.</param>
        /// <returns>The "Ellipse" element width in pixels.</returns>
        public static double GetEllipseDiameter(ProgressBar progressBar)
        {
            return (double)progressBar.GetValue(EllipseDiameterProperty);
        }

        private static void SetEllipseDiameter(ProgressBar progressBar, double value)
        {
            progressBar.SetValue(EllipseDiameterPropertyKey, value);
        }

        private static readonly DependencyPropertyKey EllipseDiameterPropertyKey =
            DependencyProperty.RegisterAttachedReadOnly(
                "EllipseDiameter",
                typeof(double),
                typeof(ProgressBarHelper),
                null);

        /// <summary>
        /// Identifies the EllipseDiameter dependency property.
        /// </summary>
        public static readonly DependencyProperty EllipseDiameterProperty = EllipseDiameterPropertyKey.DependencyProperty;

        #endregion

        #region EllipseOffset

        /// <summary>
        /// Gets the template-defined offset position of the "Ellipse" element that is animated
        /// in a templated ProgressBar.
        /// </summary>
        /// <param name="progressBar">The element from which to read the property value.</param>
        /// <returns>The offset in pixels.</returns>
        public static double GetEllipseOffset(ProgressBar progressBar)
        {
            return (double)progressBar.GetValue(EllipseOffsetProperty);
        }

        private static void SetEllipseOffset(ProgressBar progressBar, double value)
        {
            progressBar.SetValue(EllipseOffsetPropertyKey, value);
        }

        private static readonly DependencyPropertyKey EllipseOffsetPropertyKey =
            DependencyProperty.RegisterAttachedReadOnly(
                "EllipseOffset",
                typeof(double),
                typeof(ProgressBarHelper),
                null);

        /// <summary>
        /// Identifies the EllipseOffset dependency property.
        /// </summary>
        public static readonly DependencyProperty EllipseOffsetProperty = EllipseOffsetPropertyKey.DependencyProperty;

        #endregion

        #region IndicatorLengthDelta

        /// <summary>
        /// Gets the indicator length delta, which is useful for repositioning transitions.
        /// </summary>
        /// <param name="progressBar">The element from which to read the property value.</param>
        /// <returns>The delta in pixels.</returns>
        public static double GetIndicatorLengthDelta(ProgressBar progressBar)
        {
            return (double)progressBar.GetValue(IndicatorLengthDeltaProperty);
        }

        private static void SetIndicatorLengthDelta(ProgressBar progressBar, double value)
        {
            progressBar.SetValue(IndicatorLengthDeltaPropertyKey, value);
        }

        private static readonly DependencyPropertyKey IndicatorLengthDeltaPropertyKey =
            DependencyProperty.RegisterAttachedReadOnly(
                "IndicatorLengthDelta",
                typeof(double),
                typeof(ProgressBarHelper),
                null);

        /// <summary>
        /// Identifies the IndicatorLengthDelta dependency property.
        /// </summary>
        public static readonly DependencyProperty IndicatorLengthDeltaProperty = IndicatorLengthDeltaPropertyKey.DependencyProperty;

        #endregion

        #region IsEnabled

        public static bool GetIsEnabled(ProgressBar progressBar)
        {
            return (bool)progressBar.GetValue(IsEnabledProperty);
        }

        public static void SetIsEnabled(ProgressBar progressBar, bool value)
        {
            progressBar.SetValue(IsEnabledProperty, value);
        }

        public static readonly DependencyProperty IsEnabledProperty =
            DependencyProperty.RegisterAttached(
                "IsEnabled",
                typeof(bool),
                typeof(ProgressBarHelper),
                new FrameworkPropertyMetadata(OnIsEnabledChanged));

        private static void OnIsEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var progressBar = (ProgressBar)d;

            if ((bool)e.NewValue)
            {
                SetContainerAnimationStartPosition(progressBar, -34);
                SetEllipseDiameter(progressBar, 4);
                SetEllipseOffset(progressBar, 4);
                progressBar.SizeChanged += OnSizeChanged;
            }
            else
            {
                progressBar.SizeChanged -= OnSizeChanged;
                progressBar.ClearValue(ContainerAnimationStartPositionPropertyKey);
                progressBar.ClearValue(ContainerAnimationEndPositionPropertyKey);
                progressBar.ClearValue(EllipseAnimationWellPositionPropertyKey);
                progressBar.ClearValue(EllipseAnimationEndPositionPropertyKey);
                progressBar.ClearValue(EllipseDiameterPropertyKey);
                progressBar.ClearValue(EllipseOffsetPropertyKey);
                progressBar.ClearValue(IndicatorLengthDeltaPropertyKey);
            }
        }

        #endregion

        private static void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            var progressBar = (ProgressBar)sender;

            double width = e.NewSize.Width;
            SetContainerAnimationEndPosition(progressBar, 0.4352 * width - 34);
            SetEllipseAnimationWellPosition(progressBar, width * 1.0 / 3.0);
            SetEllipseAnimationEndPosition(progressBar, width * 2.0 / 3.0);

            progressBar.Dispatcher.BeginInvoke(DispatcherPriority.Render,
                (Action)(() => RestartIndeterminateStoryboard(progressBar)));
        }

        private static void RestartIndeterminateStoryboard(ProgressBar progressBar)
        {
            if (progressBar.IsIndeterminate)
            {
                if (progressBar.Template?.FindName("Indeterminate", progressBar) is VisualState indeterminate)
                {
                    var templateRoot = progressBar.GetTemplateRoot();
                    if (templateRoot != null)
                    {
                        var storyboard = indeterminate.Storyboard;
                        if (storyboard != null)
                        {
                            RestartStoryboard(storyboard, templateRoot);
                        }
                    }
                }
            }
        }

        private static void RestartStoryboard(Storyboard storyboard, FrameworkElement containingObject)
        {
            storyboard.Remove(containingObject);
            storyboard.Begin(containingObject, HandoffBehavior.SnapshotAndReplace, true);
        }
    }
}
