using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;

namespace ModernWpf.Controls
{
    [TemplatePart(Name = LabelsHostName, Type = typeof(ItemsControl))]
    public class AnnotatedScrollBar : Control
    {
        private const string LabelsHostName = "PART_LabelsHost";

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
            base.OnApplyTemplate();

            _labelsHost = GetTemplateChild(LabelsHostName) as ItemsControl;
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
            DetailLabelRequested?.Invoke(this, args);
            return args;
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

        private ItemsControl _labelsHost;
    }
}
