using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ModernWpf.Controls
{
    [TemplatePart(Name = LabelsHostName, Type = typeof(ItemsControl))]
    [TemplatePart(Name = RailName, Type = typeof(FrameworkElement))]
    public class AnnotatedScrollBar : Control
    {
        private const string LabelsHostName = "PART_LabelsHost";
        private const string RailName = "PART_Rail";

        static AnnotatedScrollBar()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(AnnotatedScrollBar),
                new FrameworkPropertyMetadata(typeof(AnnotatedScrollBar)));
        }

        public AnnotatedScrollBar()
        {
            SetCurrentValue(LabelsProperty, new ObservableCollection<AnnotatedScrollBarLabel>());
        }

        public object ScrollController => this;

        public static readonly DependencyProperty LabelsProperty =
            DependencyProperty.Register(
                nameof(Labels),
                typeof(IList<AnnotatedScrollBarLabel>),
                typeof(AnnotatedScrollBar),
                new FrameworkPropertyMetadata(null, OnLabelsPropertyChanged));

        public IList<AnnotatedScrollBarLabel> Labels
        {
            get => (IList<AnnotatedScrollBarLabel>)GetValue(LabelsProperty);
            set => SetValue(LabelsProperty, value);
        }

        public static readonly DependencyProperty LabelTemplateProperty =
            DependencyProperty.Register(
                nameof(LabelTemplate),
                typeof(DataTemplate),
                typeof(AnnotatedScrollBar),
                new FrameworkPropertyMetadata(null));

        public DataTemplate LabelTemplate
        {
            get => (DataTemplate)GetValue(LabelTemplateProperty);
            set => SetValue(LabelTemplateProperty, value);
        }

        public static readonly DependencyProperty DetailLabelTemplateProperty =
            DependencyProperty.Register(
                nameof(DetailLabelTemplate),
                typeof(DataTemplate),
                typeof(AnnotatedScrollBar),
                new FrameworkPropertyMetadata(null));

        public DataTemplate DetailLabelTemplate
        {
            get => (DataTemplate)GetValue(DetailLabelTemplateProperty);
            set => SetValue(DetailLabelTemplateProperty, value);
        }

        public static readonly DependencyProperty SmallChangeProperty =
            DependencyProperty.Register(
                nameof(SmallChange),
                typeof(double),
                typeof(AnnotatedScrollBar),
                new FrameworkPropertyMetadata(0d));

        public double SmallChange
        {
            get => (double)GetValue(SmallChangeProperty);
            set => SetValue(SmallChangeProperty, value);
        }

        public event TypedEventHandler<AnnotatedScrollBar, AnnotatedScrollBarScrollingEventArgs> Scrolling;

        public event TypedEventHandler<AnnotatedScrollBar, AnnotatedScrollBarDetailLabelRequestedEventArgs> DetailLabelRequested;

        public override void OnApplyTemplate()
        {
            if (_rail != null)
            {
                _rail.MouseLeftButtonDown -= OnRailMouseLeftButtonDown;
                _rail.MouseMove -= OnRailMouseMove;
            }

            base.OnApplyTemplate();

            _labelsHost = GetTemplateChild(LabelsHostName) as ItemsControl;
            _rail = GetTemplateChild(RailName) as FrameworkElement;

            if (_rail != null)
            {
                _rail.MouseLeftButtonDown += OnRailMouseLeftButtonDown;
                _rail.MouseMove += OnRailMouseMove;
            }

            UpdateLabelsHost();
        }

        internal AnnotatedScrollBarScrollingEventArgs RaiseScrolling(
            double scrollOffset,
            AnnotatedScrollBarScrollingEventKind scrollingEventKind)
        {
            var args = new AnnotatedScrollBarScrollingEventArgs(scrollOffset, scrollingEventKind);
            Scrolling?.Invoke(this, args);
            return args;
        }

        internal AnnotatedScrollBarDetailLabelRequestedEventArgs RaiseDetailLabelRequested(double scrollOffset)
        {
            var args = new AnnotatedScrollBarDetailLabelRequestedEventArgs(scrollOffset);
            args.Content = GetNearestLabelContent(scrollOffset);
            DetailLabelRequested?.Invoke(this, args);
            return args;
        }

        internal AnnotatedScrollBarScrollingEventArgs ScrollToRatioForTesting(
            double ratio,
            AnnotatedScrollBarScrollingEventKind eventKind)
        {
            return RaiseScrolling(MapRatioToOffset(ratio), eventKind);
        }

        internal AnnotatedScrollBarDetailLabelRequestedEventArgs RequestDetailLabelForRatioForTesting(double ratio)
        {
            return RaiseDetailLabelRequested(MapRatioToOffset(ratio));
        }

        private static void OnLabelsPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var annotatedScrollBar = (AnnotatedScrollBar)d;
            annotatedScrollBar.UpdateCollectionChangedSubscription(e.OldValue, e.NewValue);
            annotatedScrollBar.UpdateLabelsHost();
        }

        private void UpdateCollectionChangedSubscription(object oldValue, object newValue)
        {
            if (oldValue is INotifyCollectionChanged oldObservable)
            {
                oldObservable.CollectionChanged -= OnLabelsCollectionChanged;
            }

            if (newValue is INotifyCollectionChanged newObservable)
            {
                newObservable.CollectionChanged += OnLabelsCollectionChanged;
            }
        }

        private void OnLabelsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            UpdateLabelsHost();
        }

        private void UpdateLabelsHost()
        {
            if (_labelsHost != null)
            {
                _labelsHost.ItemsSource = Labels;
            }
        }

        private void OnRailMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (_rail == null)
            {
                return;
            }

            var ratio = GetRailRatio(e.GetPosition(_rail));
            RaiseScrolling(MapRatioToOffset(ratio), AnnotatedScrollBarScrollingEventKind.Click);
            e.Handled = true;
        }

        private void OnRailMouseMove(object sender, MouseEventArgs e)
        {
            if (_rail == null)
            {
                return;
            }

            var ratio = GetRailRatio(e.GetPosition(_rail));
            RaiseDetailLabelRequested(MapRatioToOffset(ratio));
        }

        private double GetRailRatio(Point point)
        {
            var height = Math.Max(1, _rail?.ActualHeight ?? ActualHeight);
            return Math.Max(0, Math.Min(1, point.Y / height));
        }

        private double MapRatioToOffset(double ratio)
        {
            ratio = Math.Max(0, Math.Min(1, ratio));

            if (Labels == null || Labels.Count == 0)
            {
                return ratio * 100;
            }

            var min = Labels.Min(label => label.ScrollOffset);
            var max = Labels.Max(label => label.ScrollOffset);
            return min + ratio * Math.Max(0, max - min);
        }

        private object GetNearestLabelContent(double scrollOffset)
        {
            if (Labels == null || Labels.Count == 0)
            {
                return null;
            }

            return Labels
                .OrderBy(label => Math.Abs(label.ScrollOffset - scrollOffset))
                .First()
                .Content;
        }

        private ItemsControl _labelsHost;
        private FrameworkElement _rail;
    }
}
