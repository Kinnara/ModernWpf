using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace ModernWpf.Controls.Primitives
{
    [TemplatePart(Name = s_LayoutRootName, Type = typeof(Grid))]
    [TemplatePart(Name = s_ProgressBarIndicatorName, Type = typeof(Rectangle))]
    [TemplateVisualState(GroupName = VisualStates.GroupCommon, Name = s_DeterminateStateName)]
    [TemplateVisualState(GroupName = VisualStates.GroupCommon, Name = s_IndeterminateStateName)]
    public class ProgressBarPresenter : RangeBase
    {
        static ProgressBarPresenter()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ProgressBarPresenter), new FrameworkPropertyMetadata(typeof(ProgressBarPresenter)));
        }

        public ProgressBarPresenter()
        {
        }

        #region IsIndeterminate

        public static readonly DependencyProperty IsIndeterminateProperty =
            DependencyProperty.Register(
                nameof(IsIndeterminate),
                typeof(bool),
                typeof(ProgressBarPresenter),
                new PropertyMetadata(false, OnIsIndeterminatePropertyChanged));

        public bool IsIndeterminate
        {
            get => (bool)GetValue(IsIndeterminateProperty);
            set => SetValue(IsIndeterminateProperty, value);
        }

        private static void OnIsIndeterminatePropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            ((ProgressBarPresenter)sender).OnIsIndeterminatePropertyChanged(args);
        }

        #endregion

        #region ContainerAnimationStartPosition

        private static readonly DependencyPropertyKey ContainerAnimationStartPositionPropertyKey =
            DependencyProperty.RegisterReadOnly(
                nameof(ContainerAnimationStartPosition),
                typeof(double),
                typeof(ProgressBarPresenter),
                null);

        public static readonly DependencyProperty ContainerAnimationStartPositionProperty =
            ContainerAnimationStartPositionPropertyKey.DependencyProperty;

        public double ContainerAnimationStartPosition
        {
            get => (double)GetValue(ContainerAnimationStartPositionProperty);
            private set => SetValue(ContainerAnimationStartPositionPropertyKey, value);
        }

        #endregion

        #region ContainerAnimationEndPosition

        private static readonly DependencyPropertyKey ContainerAnimationEndPositionPropertyKey =
            DependencyProperty.RegisterReadOnly(
                nameof(ContainerAnimationEndPosition),
                typeof(double),
                typeof(ProgressBarPresenter),
                null);

        public static readonly DependencyProperty ContainerAnimationEndPositionProperty =
            ContainerAnimationEndPositionPropertyKey.DependencyProperty;

        public double ContainerAnimationEndPosition
        {
            get => (double)GetValue(ContainerAnimationEndPositionProperty);
            private set => SetValue(ContainerAnimationEndPositionPropertyKey, value);
        }

        #endregion

        #region IndicatorLengthDelta

        private static readonly DependencyPropertyKey IndicatorLengthDeltaPropertyKey =
            DependencyProperty.RegisterReadOnly(
                nameof(IndicatorLengthDelta),
                typeof(double),
                typeof(ProgressBarPresenter),
                null);

        public static readonly DependencyProperty IndicatorLengthDeltaProperty =
            IndicatorLengthDeltaPropertyKey.DependencyProperty;

        public double IndicatorLengthDelta
        {
            get => (double)GetValue(IndicatorLengthDeltaProperty);
            private set => SetValue(IndicatorLengthDeltaPropertyKey, value);
        }

        #endregion

        #region ClipRect

        private static readonly DependencyPropertyKey ClipRectPropertyKey =
            DependencyProperty.RegisterReadOnly(
                nameof(ClipRect),
                typeof(RectangleGeometry),
                typeof(ProgressBarPresenter),
                null);

        public static readonly DependencyProperty ClipRectProperty =
            ClipRectPropertyKey.DependencyProperty;

        public RectangleGeometry ClipRect
        {
            get => (RectangleGeometry)GetValue(ClipRectProperty);
            private set => SetValue(ClipRectPropertyKey, value);
        }

        #endregion

        #region CornerRadius

        public static readonly DependencyProperty CornerRadiusProperty =
            ControlHelper.CornerRadiusProperty.AddOwner(typeof(ProgressBarPresenter));

        public CornerRadius CornerRadius
        {
            get => (CornerRadius)GetValue(CornerRadiusProperty);
            set => SetValue(CornerRadiusProperty, value);
        }

        #endregion

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            m_layoutRoot = GetTemplateChild(s_LayoutRootName) as Grid;
            m_progressBarIndicator = GetTemplateChild(s_ProgressBarIndicatorName) as Rectangle;

            UpdateStates();
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);

            SetProgressBarIndicatorWidth();
            UpdateWidthBasedTemplateSettings();

            Dispatcher.BeginInvoke(
                () => RestartIndeterminateStoryboard(),
                DispatcherPriority.Render);
        }

        protected override void OnValueChanged(double oldValue, double newValue)
        {
            base.OnValueChanged(oldValue, newValue);
            OnRangeBasePropertyChanged();
        }

        protected override void OnMinimumChanged(double oldMinimum, double newMinimum)
        {
            base.OnMinimumChanged(oldMinimum, newMinimum);
            OnRangeBasePropertyChanged();
        }

        protected override void OnMaximumChanged(double oldMaximum, double newMaximum)
        {
            base.OnMaximumChanged(oldMaximum, newMaximum);
            OnRangeBasePropertyChanged();
        }

        private void OnRangeBasePropertyChanged()
        {
            SetProgressBarIndicatorWidth();
        }

        private void OnIsIndeterminatePropertyChanged(DependencyPropertyChangedEventArgs args)
        {
            SetProgressBarIndicatorWidth();
            UpdateStates();
        }

        void UpdateStates()
        {
            if (IsIndeterminate)
            {
                UpdateWidthBasedTemplateSettings();
                VisualStateManager.GoToState(this, s_IndeterminateStateName, true);
            }
            else if (!IsIndeterminate)
            {
                VisualStateManager.GoToState(this, s_DeterminateStateName, true);
            }
        }

        void SetProgressBarIndicatorWidth()
        {
            if (m_layoutRoot != null)
            {
                if (m_progressBarIndicator != null)
                {
                    double progressBarWidth = m_layoutRoot.ActualWidth;
                    double prevIndicatorWidth = m_progressBarIndicator.ActualWidth;
                    double maximum = Maximum;
                    double minimum = Minimum;
                    var padding = Padding;

                    if (IsIndeterminate)
                    {
                        m_progressBarIndicator.Width = progressBarWidth * 0.4;
                    }
                    else if (Math.Abs(maximum - minimum) > double.Epsilon)
                    {
                        double maxIndicatorWidth = progressBarWidth - (padding.Left + padding.Right);
                        double increment = maxIndicatorWidth / (maximum - minimum);
                        double indicatorWidth = increment * (Value - minimum);
                        double widthDelta = indicatorWidth - prevIndicatorWidth;
                        IndicatorLengthDelta = -widthDelta;
                        m_progressBarIndicator.Width = indicatorWidth;
                    }
                    else
                    {
                        m_progressBarIndicator.Width = 0; // Error
                    }

                    UpdateStates(); // Reverts back to previous state
                }
            }
        }

        void UpdateWidthBasedTemplateSettings()
        {
            if (m_progressBarIndicator != null)
            {
                double width;
                double height;
                if (m_layoutRoot != null)
                {
                    width = m_layoutRoot.ActualWidth;
                    height = m_layoutRoot.ActualHeight;
                }
                else
                {
                    width = 0;
                    height = 0;
                }

                double indicatorWidthMultiplier = -0.4;

                ContainerAnimationStartPosition = width * indicatorWidthMultiplier;
                ContainerAnimationEndPosition = width;

                var padding = Padding;
                var rectangle = new RectangleGeometry(
                    new Rect(
                        padding.Left,
                        padding.Top,
                        width - (padding.Right + padding.Left),
                        height - (padding.Bottom + padding.Top)
                        ),
                    m_progressBarIndicator.RadiusX,
                    m_progressBarIndicator.RadiusY);

                ClipRect = rectangle;
            }
        }

        private void RestartIndeterminateStoryboard()
        {
            if (IsIndeterminate)
            {
                if (GetTemplateChild(s_IndeterminateStateName) is VisualState indeterminate)
                {
                    var templateRoot = this.GetTemplateRoot();
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

        Grid m_layoutRoot;
        Rectangle m_progressBarIndicator;

        const string s_LayoutRootName = "LayoutRoot";
        const string s_ProgressBarIndicatorName = "ProgressBarIndicator";
        const string s_IndeterminateStateName = "Indeterminate";
        const string s_DeterminateStateName = "Determinate";
    }
}
