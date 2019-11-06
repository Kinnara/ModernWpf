using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace ModernWpf.Controls.Primitives
{
    /// <summary>
    /// Provides calculated values that can be referenced as **TemplatedParent** sources
    /// when defining templates for a ProgressBar control. Not intended for general use.
    /// </summary>
    public class ProgressBarHelper : DependencyObject
    {
        private ProgressBarHelper(ProgressBar owner)
        {
            m_owner = owner;
        }

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
                new PropertyMetadata(OnContainerAnimationEndPositionPropertyChanged));

        /// <summary>
        /// Identifies the ContainerAnimationEndPosition dependency property.
        /// </summary>
        public static readonly DependencyProperty ContainerAnimationEndPositionProperty =
            ContainerAnimationEndPositionPropertyKey.DependencyProperty;

        private static void OnContainerAnimationEndPositionPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var helper = GetHelperInstance((ProgressBar)d);
            if (helper != null)
            {
                helper.Dispatcher.BeginInvoke(
                    () => helper.RestartIndeterminateStoryboard(),
                    DispatcherPriority.Render);
            }
        }

        #endregion

        #region ClipRect

        private static readonly DependencyPropertyKey ClipRectPropertyKey =
            DependencyProperty.RegisterAttachedReadOnly(
                "ClipRect",
                typeof(RectangleGeometry),
                typeof(ProgressBarHelper),
                null);

        public static readonly DependencyProperty ClipRectProperty =
            ClipRectPropertyKey.DependencyProperty;

        public static RectangleGeometry GetClipRect(ProgressBar progressBar)
        {
            return (RectangleGeometry)progressBar.GetValue(ClipRectProperty);
        }

        private static void SetClipRect(ProgressBar progressBar, RectangleGeometry value)
        {
            progressBar.SetValue(ClipRectPropertyKey, value);
        }

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
                SetHelperInstance(progressBar, new ProgressBarHelper(progressBar));
            }
            else
            {
                progressBar.ClearValue(HelperInstanceProperty);
            }
        }

        #endregion

        #region HelperInstance

        private static readonly DependencyProperty HelperInstanceProperty =
            DependencyProperty.RegisterAttached(
                "HelperInstance",
                typeof(ProgressBarHelper),
                typeof(ProgressBarHelper),
                new PropertyMetadata(default(ProgressBarHelper), OnHelperInstanceChanged));

        private static ProgressBarHelper GetHelperInstance(ProgressBar progressBar)
        {
            return (ProgressBarHelper)progressBar.GetValue(HelperInstanceProperty);
        }

        private static void SetHelperInstance(ProgressBar progressBar, ProgressBarHelper value)
        {
            progressBar.SetValue(HelperInstanceProperty, value);
        }

        private static void OnHelperInstanceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue is ProgressBarHelper oldValue)
            {
                oldValue.Detach();
            }

            if (e.NewValue is ProgressBarHelper newValue)
            {
                newValue.Attach();
            }
        }

        #endregion

        private static readonly DependencyProperty MinimumProperty =
            RangeBase.MinimumProperty.AddOwner(typeof(ProgressBarHelper), new FrameworkPropertyMetadata(OnRangeBasePropertyChanged));

        private static readonly DependencyProperty MaximumProperty =
            RangeBase.MaximumProperty.AddOwner(typeof(ProgressBarHelper), new FrameworkPropertyMetadata(OnRangeBasePropertyChanged));

        private static readonly DependencyProperty IsIndeterminateProperty =
            ProgressBar.IsIndeterminateProperty.AddOwner(typeof(ProgressBarHelper), new FrameworkPropertyMetadata(OnIsIndeterminatePropertyChanged));

        private void Attach()
        {
            m_owner.SizeChanged += OnSizeChanged;
            m_owner.ValueChanged += OnValueChanged;
            BindingOperations.SetBinding(this, MinimumProperty,
                new Binding { Path = new PropertyPath(RangeBase.MinimumProperty), Source = m_owner });
            BindingOperations.SetBinding(this, MaximumProperty,
                new Binding { Path = new PropertyPath(RangeBase.MaximumProperty), Source = m_owner });
            BindingOperations.SetBinding(this, IsIndeterminateProperty,
                new Binding { Path = new PropertyPath(ProgressBar.IsIndeterminateProperty), Source = m_owner });

            if (m_owner.IsLoaded)
            {
                OnApplyTemplate();
            }
            else
            {
                m_owner.Loaded += OnLoaded;
            }
        }

        private void Detach()
        {
            m_layoutRoot = null;
            m_progressBarIndicator = null;

            m_owner.Loaded -= OnLoaded;
            m_owner.SizeChanged -= OnSizeChanged;
            m_owner.ValueChanged -= OnValueChanged;
            ClearValue(MinimumProperty);
            ClearValue(MaximumProperty);
            ClearValue(IsIndeterminateProperty);

            m_owner.ClearValue(ContainerAnimationEndPositionPropertyKey);
            m_owner.ClearValue(ClipRectPropertyKey);
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            m_owner.Loaded -= OnLoaded;
            OnApplyTemplate();
        }

        private void OnApplyTemplate()
        {
            m_layoutRoot = m_owner.GetTemplateChild<Grid>(s_LayoutRootName);
            m_progressBarIndicator = m_owner.GetTemplateChild<Rectangle>(s_ProgressBarIndicatorName);

            UpdateStates();
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            SetProgressBarIndicatorWidth();
            if (m_shouldUpdateWidthBasedTemplateSettings)
            {
                UpdateWidthBasedTemplateSettings();
            }
        }

        private void OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            SetProgressBarIndicatorWidth();
        }

        private static void OnRangeBasePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((ProgressBarHelper)d).SetProgressBarIndicatorWidth();
        }

        private static void OnIsIndeterminatePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((ProgressBarHelper)d).UpdateStates();
        }

        private void UpdateStates()
        {
            m_shouldUpdateWidthBasedTemplateSettings = false;
            if (m_owner.IsIndeterminate)
            {
                m_shouldUpdateWidthBasedTemplateSettings = true;
                UpdateWidthBasedTemplateSettings();

            }
            else if (!m_owner.IsIndeterminate)
            {
                SetProgressBarIndicatorWidth();
            }
        }

        private void SetProgressBarIndicatorWidth()
        {
            var progressBar = m_layoutRoot;
            if (progressBar != null)
            {
                var progressBarIndicator = m_progressBarIndicator;
                if (progressBarIndicator != null)
                {
                    double progressBarWidth = progressBar.ActualWidth;
                    double maximum = m_owner.Maximum;
                    double minimum = m_owner.Minimum;
                    var padding = m_owner.Padding;

                    if (Math.Abs(maximum - minimum) > double.Epsilon)
                    {
                        double maxIndicatorWidth = progressBarWidth - (padding.Left + padding.Right);
                        double increment = maxIndicatorWidth / (maximum - minimum);
                        progressBarIndicator.Width = increment * (m_owner.Value - minimum);
                    }
                    else
                    {
                        progressBarIndicator.Width = 0; // Error
                    }
                }
            }
        }

        private void UpdateWidthBasedTemplateSettings()
        {
            var progressBarIndicator = m_progressBarIndicator;
            if (progressBarIndicator != null)
            {
                double width, height;

                var progressBar = m_layoutRoot;
                if (progressBar != null)
                {
                    width = progressBar.ActualWidth;
                    height = progressBar.ActualHeight;
                }
                else
                {
                    width = 0.0;
                    height = 0.0;
                }

                progressBarIndicator.Width = width / 3;

                SetContainerAnimationEndPosition(m_owner, width);

                var padding = m_owner.Padding;
                var rectangle = new RectangleGeometry();
                rectangle.Rect = new Rect(
                    padding.Left,
                    padding.Top,
                    width - (padding.Right + padding.Left),
                    height - (padding.Bottom + padding.Top)
                    );

                SetClipRect(m_owner, rectangle);
            }
        }

        private void RestartIndeterminateStoryboard()
        {
            if (m_owner.IsIndeterminate)
            {
                var indeterminate = m_owner.GetTemplateChild<VisualState>(s_IndeterminateStateName);
                if (indeterminate != null)
                {
                    var templateRoot = m_owner.GetTemplateRoot();
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

        private readonly ProgressBar m_owner;

        private Grid m_layoutRoot;
        private Rectangle m_progressBarIndicator;

        private bool m_shouldUpdateWidthBasedTemplateSettings = false;

        private const string s_LayoutRootName = "LayoutRoot";
        private const string s_ProgressBarIndicatorName = "ProgressBarIndicator";
        private const string s_IndeterminateStateName = "Indeterminate";
}
}
