using ModernWpf.Controls.Primitives;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Threading;

namespace ModernWpf.MahApps.Controls
{
    public class DateTimeComponentSelector : ListBox
    {
        internal const int PaddingItemsCount = 4;

        static DateTimeComponentSelector()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(DateTimeComponentSelector), new FrameworkPropertyMetadata(typeof(DateTimeComponentSelector)));

            IsTabStopProperty.OverrideMetadata(typeof(DateTimeComponentSelector), new FrameworkPropertyMetadata(true));
            KeyboardNavigation.TabNavigationProperty.OverrideMetadata(typeof(DateTimeComponentSelector), new FrameworkPropertyMetadata(KeyboardNavigationMode.Cycle));
        }

        public DateTimeComponentSelector()
        {
            SetResourceReference(ItemHeightProperty, "TimePickerFlyoutPresenterItemHeight");
            UpdateHeight();
            IsVisibleChanged += OnIsVisibleChanged;
        }

        #region UseSystemFocusVisuals

        /// <summary>
        /// Identifies the UseSystemFocusVisuals dependency property.
        /// </summary>
        public static readonly DependencyProperty UseSystemFocusVisualsProperty =
            FocusVisualHelper.UseSystemFocusVisualsProperty.AddOwner(typeof(DateTimeComponentSelector));

        /// <summary>
        /// Gets or sets a value that indicates whether the control uses focus visuals that
        /// are drawn by the system or those defined in the control template.
        /// </summary>
        /// <returns>
        /// **true** if the control uses focus visuals drawn by the system; **false** if
        /// the control uses focus visuals defined in the ControlTemplate. The default is
        /// **false**; see Remarks.
        /// </returns>
        public bool UseSystemFocusVisuals
        {
            get => (bool)GetValue(UseSystemFocusVisualsProperty);
            set => SetValue(UseSystemFocusVisualsProperty, value);
        }

        #endregion

        #region SuppressItemMouseOver

        private static readonly DependencyPropertyKey SuppressItemMouseOverPropertyKey =
            DependencyProperty.RegisterReadOnly(
                nameof(SuppressItemMouseOver),
                typeof(bool),
                typeof(DateTimeComponentSelector),
                new PropertyMetadata(false));

        public static readonly DependencyProperty SuppressItemMouseOverProperty =
            SuppressItemMouseOverPropertyKey.DependencyProperty;

        public bool SuppressItemMouseOver
        {
            get => (bool)GetValue(SuppressItemMouseOverProperty);
            private set => SetValue(SuppressItemMouseOverPropertyKey, value);
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

            base.OnApplyTemplate();

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

            ScrollToSelection();
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

        protected override void OnKeyDown(KeyEventArgs e)
        {
            bool handled = true;
            Key key = e.Key;
            switch (key)
            {
                case Key.Up:
                    _scrollViewer?.LineUp();
                    break;

                case Key.Down:
                    _scrollViewer?.LineDown();
                    break;

                case Key.Left:
                case Key.Right:
                    if (IsKeyboardFocusWithin)
                    {
                        var direction = key == Key.Left ? FocusNavigationDirection.Left : FocusNavigationDirection.Right;
                        MoveFocus(new TraversalRequest(direction));
                    }
                    break;

                case Key.Home:
                case Key.End:
                    break;

                case Key.PageUp:
                    _scrollViewer?.PageUp();
                    break;

                case Key.PageDown:
                    _scrollViewer?.PageDown();
                    break;

                default:
                    handled = false;
                    break;
            }
            if (handled)
            {
                e.Handled = true;
            }
            else
            {
                base.OnKeyDown(e);
            }
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
            else if (SuppressItemMouseOver)
            {
                ClearValue(SuppressItemMouseOverPropertyKey);
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

        private void OnIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue)
            {
                Opacity = 0;
                Dispatcher.BeginInvoke(() =>
                {
                    if (IsVisible)
                    {
                        ClearValue(OpacityProperty);
                    }
                });
            }
            else
            {
                ClearValue(OpacityProperty);
            }
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

            if (!SuppressItemMouseOver)
            {
                SuppressItemMouseOver = true;
            }
        }

        private ScrollViewer _scrollViewer;
        private RepeatButton _upButton;
        private RepeatButton _downButton;

        private SelectionChangedEventArgs _deferredSelectionChangedEventArgs;
        private bool _ignoreNextMouseMove;
    }
}
