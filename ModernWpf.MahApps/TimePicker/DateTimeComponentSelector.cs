using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace ModernWpf.MahApps.Controls
{
    public class DateTimeComponentSelector : ListBox
    {
        internal const int PaddingItemsCount = 4;

        static DateTimeComponentSelector()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(DateTimeComponentSelector),
                new FrameworkPropertyMetadata(typeof(DateTimeComponentSelector)));
        }

        public DateTimeComponentSelector()
        {
            SetResourceReference(ItemHeightProperty, "TimePickerFlyoutPresenterItemHeight");
            UpdateHeight();
        }

        #region ItemHeight

        private static readonly DependencyProperty ItemHeightProperty =
            DependencyProperty.Register(
                nameof(ItemHeight),
                typeof(double),
                typeof(DateTimeComponentSelector),
                new FrameworkPropertyMetadata(40.0, OnItemHeightChanged));

        private double ItemHeight
        {
            get => (double)GetValue(ItemHeightProperty);
            set => SetValue(ItemHeightProperty, value);
        }

        private static void OnItemHeightChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var selector = (DateTimeComponentSelector)d;
            selector.UpdateHeight();
        }

        #endregion

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            if (_scrollViewer != null)
            {
                _scrollViewer.ScrollChanged -= OnScrollChanged;
            }

            if (_upButton != null)
            {
                _upButton.Click -= OnUpButtonClick;
            }

            if (_downButton != null)
            {
                _downButton.Click -= OnDownButtonClick;
            }

            _scrollViewer = GetTemplateChild("ScrollViewer") as ScrollViewer;
            _upButton = GetTemplateChild("UpButton") as RepeatButton;
            _downButton = GetTemplateChild("DownButton") as RepeatButton;

            if (_scrollViewer != null)
            {
                _scrollViewer.ScrollChanged += OnScrollChanged;
            }

            if (_upButton != null)
            {
                _upButton.Click += OnUpButtonClick;
            }

            if (_downButton != null)
            {
                _downButton.Click += OnDownButtonClick;
            }
        }

        internal void RaiseDeferredSelectionChanged()
        {
            var e = _deferredSelectionChangedEventArgs;
            _deferredSelectionChangedEventArgs = null;

            if (e != null)
            {
                e.Handled = false;
                RaiseEvent(e);
            }
        }

        protected override DependencyObject GetContainerForItemOverride()
        {
            return new DateTimeComponentSelectorItem();
        }

        protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
        {
            base.PrepareContainerForItemOverride(element, item);

            if (element is DateTimeComponentSelectorItem container)
            {
                if (item is int i && i < 0 ||
                    item is string s && string.IsNullOrEmpty(s))
                {
                    container.IsEnabled = false;
                    container.Visibility = Visibility.Hidden;
                }
            }
        }

        protected override void ClearContainerForItemOverride(DependencyObject element, object item)
        {
            if (element is DateTimeComponentSelectorItem container)
            {
                container.ClearValue(IsEnabledProperty);
                container.ClearValue(VisibilityProperty);
            }

            base.ClearContainerForItemOverride(element, item);
        }

        protected override bool IsItemItsOwnContainerOverride(object item)
        {
            return item is DateTimeComponentSelectorItem;
        }

        protected override void OnSelectionChanged(SelectionChangedEventArgs e)
        {
            e.Handled = true;
            _deferredSelectionChangedEventArgs = e;

            base.OnSelectionChanged(e);

            int selectedIndex = SelectedIndex;
            if (selectedIndex >= 0)
            {
                _scrollViewer?.ScrollToVerticalOffset(selectedIndex - PaddingItemsCount);
            }
        }

        private void OnScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (e.VerticalChange != 0)
            {
                int selectedIndex = (int)e.VerticalOffset + PaddingItemsCount;
                if (selectedIndex >= 0 && selectedIndex < Items.Count)
                {
                    SetCurrentValue(SelectedIndexProperty, (int)e.VerticalOffset + PaddingItemsCount);
                }
            }
        }

        private void OnUpButtonClick(object sender, RoutedEventArgs e)
        {
            _scrollViewer?.LineUp();
        }

        private void OnDownButtonClick(object sender, RoutedEventArgs e)
        {
            _scrollViewer?.LineDown();
        }

        private void UpdateHeight()
        {
            Height = ItemHeight * (PaddingItemsCount * 2 + 1);
        }

        private ScrollViewer _scrollViewer;
        private RepeatButton _upButton;
        private RepeatButton _downButton;

        private SelectionChangedEventArgs _deferredSelectionChangedEventArgs;
    }

    public class DateTimeComponentSelectorItem : ListBoxItem
    {
        static DateTimeComponentSelectorItem()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(DateTimeComponentSelectorItem),
                new FrameworkPropertyMetadata(typeof(DateTimeComponentSelectorItem)));
        }
    }
}
