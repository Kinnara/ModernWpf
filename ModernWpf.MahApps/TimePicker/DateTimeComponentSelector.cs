using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

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
            Loaded += OnLoaded;
        }

        #region IsItemMouseOverEnabled

        private static readonly DependencyPropertyKey IsItemMouseOverEnabledPropertyKey =
            DependencyProperty.RegisterReadOnly(
                nameof(IsItemMouseOverEnabled),
                typeof(bool),
                typeof(DateTimeComponentSelector),
                new PropertyMetadata(true));

        public static readonly DependencyProperty IsItemMouseOverEnabledProperty =
            IsItemMouseOverEnabledPropertyKey.DependencyProperty;

        public bool IsItemMouseOverEnabled
        {
            get => (bool)GetValue(IsItemMouseOverEnabledProperty);
            private set => SetValue(IsItemMouseOverEnabledPropertyKey, value);
        }

        #endregion

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

            if (ItemsSource is LoopingSelectorDataSource items)
            {
                int index = SelectedIndex;
                if (index >= 0 && index < items.SourceCount)
                {
                    SetCurrentValue(SelectedValueProperty, items.IndexOf(items[index]));
                }
            }

            ScrollToSelection();
        }

        protected override void OnPreviewMouseWheel(MouseWheelEventArgs e)
        {
            base.OnPreviewMouseWheel(e);

            DisableItemMouseOver();
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (_ignoreNextMouseMove)
            {
                _ignoreNextMouseMove = false;
            }
            else if (!IsItemMouseOverEnabled)
            {
                ClearValue(IsItemMouseOverEnabledPropertyKey);
            }
        }

        private static double IndexToOffset(int index)
        {
            return index - PaddingItemsCount;
        }

        private static int OffsetToIndex(double offset)
        {
            return (int)offset + PaddingItemsCount;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            ScrollToSelection();
        }

        private void ScrollToSelection()
        {
            if (_scrollViewer != null)
            {
                int selectedIndex = SelectedIndex;
                if (selectedIndex >= 0)
                {
                    double offset = IndexToOffset(selectedIndex);
                    _scrollViewer.ScrollToVerticalOffset(offset);
                }
            }
        }

        private void OnScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (e.VerticalChange != 0)
            {
                int selectedIndex = OffsetToIndex(e.VerticalOffset);
                if (selectedIndex >= 0 && selectedIndex < Items.Count)
                {
                    SetCurrentValue(SelectedIndexProperty, selectedIndex);
                }

                DisableItemMouseOver();
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

        private void DisableItemMouseOver()
        {
            if (_ignoreNextMouseMove)
            {
                return;
            }

            _ignoreNextMouseMove = true;

            if (IsItemMouseOverEnabled)
            {
                IsItemMouseOverEnabled = false;
            }
        }

        private ScrollViewer _scrollViewer;
        private RepeatButton _upButton;
        private RepeatButton _downButton;

        private SelectionChangedEventArgs _deferredSelectionChangedEventArgs;
        private bool _ignoreNextMouseMove;
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
