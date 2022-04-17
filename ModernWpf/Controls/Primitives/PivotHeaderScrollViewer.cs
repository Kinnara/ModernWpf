using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ModernWpf.Controls.Primitives
{
    public class PivotHeaderScrollViewer : ScrollViewer
    {
        private TabControl _tabControl;

        static PivotHeaderScrollViewer()
        {
            FlowDirectionProperty.OverrideMetadata(typeof(PivotHeaderScrollViewer), new FrameworkPropertyMetadata(OnFlowDirectionChanged));
            HorizontalScrollBarVisibilityProperty.OverrideMetadata(typeof(PivotHeaderScrollViewer), new FrameworkPropertyMetadata(OnHorizontalScrollBarVisibilityChanged));
        }

        public PivotHeaderScrollViewer()
        {
            Loaded += OnLoaded;
        }

        #region CanScroll

        private static readonly DependencyPropertyKey CanScrollPropertyKey =
            DependencyProperty.RegisterReadOnly(
                nameof(CanScroll),
                typeof(bool),
                typeof(PivotHeaderScrollViewer),
                new PropertyMetadata(false));

        public static readonly DependencyProperty CanScrollProperty =
            CanScrollPropertyKey.DependencyProperty;

        public bool CanScroll
        {
            get => (bool)GetValue(CanScrollProperty);
            private set => SetValue(CanScrollPropertyKey, value);
        }

        #endregion

        #region CanScrollUp

        private static readonly DependencyPropertyKey CanScrollUpPropertyKey =
            DependencyProperty.RegisterReadOnly(
                nameof(CanScrollUp),
                typeof(bool),
                typeof(PivotHeaderScrollViewer),
                new PropertyMetadata(false));

        public static readonly DependencyProperty CanScrollUpProperty =
            CanScrollUpPropertyKey.DependencyProperty;

        public bool CanScrollUp
        {
            get => (bool)GetValue(CanScrollUpProperty);
            private set => SetValue(CanScrollUpPropertyKey, value);
        }

        #endregion

        #region CanScrollDown

        private static readonly DependencyPropertyKey CanScrollDownPropertyKey =
            DependencyProperty.RegisterReadOnly(
                nameof(CanScrollDown),
                typeof(bool),
                typeof(PivotHeaderScrollViewer),
                new PropertyMetadata(false));

        public static readonly DependencyProperty CanScrollDownProperty =
            CanScrollDownPropertyKey.DependencyProperty;

        public bool CanScrollDown
        {
            get => (bool)GetValue(CanScrollDownProperty);
            private set => SetValue(CanScrollDownPropertyKey, value);
        }

        #endregion

        #region CanScrollLeft

        private static readonly DependencyPropertyKey CanScrollLeftPropertyKey =
            DependencyProperty.RegisterReadOnly(
                nameof(CanScrollLeft),
                typeof(bool),
                typeof(PivotHeaderScrollViewer),
                new PropertyMetadata(false));

        public static readonly DependencyProperty CanScrollLeftProperty =
            CanScrollLeftPropertyKey.DependencyProperty;

        public bool CanScrollLeft
        {
            get => (bool)GetValue(CanScrollLeftProperty);
            private set => SetValue(CanScrollLeftPropertyKey, value);
        }

        #endregion

        #region CanScrollRight

        private static readonly DependencyPropertyKey CanScrollRightPropertyKey =
            DependencyProperty.RegisterReadOnly(
                nameof(CanScrollRight),
                typeof(bool),
                typeof(PivotHeaderScrollViewer),
                new PropertyMetadata(false));

        public static readonly DependencyProperty CanScrollRightProperty =
            CanScrollRightPropertyKey.DependencyProperty;

        public bool CanScrollRight
        {
            get => (bool)GetValue(CanScrollRightProperty);
            private set => SetValue(CanScrollRightPropertyKey, value);
        }

        #endregion

        #region CanScrollVertically

        private static readonly DependencyPropertyKey CanScrollVerticallyPropertyKey =
            DependencyProperty.RegisterReadOnly(
                nameof(CanScrollVertically),
                typeof(bool),
                typeof(PivotHeaderScrollViewer),
                new PropertyMetadata(false));

        public static readonly DependencyProperty CanScrollVerticallyProperty =
            CanScrollVerticallyPropertyKey.DependencyProperty;

        public bool CanScrollVertically
        {
            get => (bool)GetValue(CanScrollVerticallyProperty);
            private set => SetValue(CanScrollVerticallyPropertyKey, value);
        }

        #endregion

        #region CanScrollHorizontally

        private static readonly DependencyPropertyKey CanScrollHorizontallyPropertyKey =
            DependencyProperty.RegisterReadOnly(
                nameof(CanScrollHorizontally),
                typeof(bool),
                typeof(PivotHeaderScrollViewer),
                new PropertyMetadata(false));

        public static readonly DependencyProperty CanScrollHorizontallyProperty =
            CanScrollHorizontallyPropertyKey.DependencyProperty;

        public bool CanScrollHorizontally
        {
            get => (bool)GetValue(CanScrollHorizontallyProperty);
            private set => SetValue(CanScrollHorizontallyPropertyKey, value);
        }

        #endregion

        protected override void OnVisualParentChanged(DependencyObject oldParent)
        {
            if (_tabControl != null)
            {
                _tabControl.SelectionChanged -= OnTabControlSelectionChanged;
            }

            base.OnVisualParentChanged(oldParent);

            _tabControl = TemplatedParent as TabControl;

            if (_tabControl != null)
            {
                _tabControl.SelectionChanged += OnTabControlSelectionChanged;
            }
        }

        protected override void OnScrollChanged(ScrollChangedEventArgs e)
        {
            base.OnScrollChanged(e);

            if (e.VerticalChange != 0 ||
                e.ExtentHeightChange != 0 ||
                e.ViewportHeightChange != 0)
            {
                UpdateCanScrollVertically();
            }

            if (e.HorizontalChange != 0 ||
                e.ExtentWidthChange != 0 ||
                e.ViewportWidthChange != 0)
            {
                UpdateCanScrollHorizontally();
            }
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
        }

        private static void OnFlowDirectionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var sv = (PivotHeaderScrollViewer)d;
            sv.UpdateCanScrollHorizontally();
        }

        private static void OnHorizontalScrollBarVisibilityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var sv = (PivotHeaderScrollViewer)d;
            sv.UpdateCanScrollHorizontally();
        }

        private void UpdateCanScrollVertically()
        {
            bool canScrollUp = CanScrollVerticallyInDirection(false);
            if (CanScrollUp != canScrollUp)
            {
                CanScrollUp = canScrollUp;
            }

            bool canScrollDown = CanScrollVerticallyInDirection(true);
            if (CanScrollDown != canScrollDown)
            {
                CanScrollDown = canScrollDown;
            }

            CanScrollVertically = CanScrollUp || CanScrollDown;
            CanScroll = CanScrollVertically || CanScrollHorizontally;
        }

        private void UpdateCanScrollHorizontally()
        {
            bool canScrollLeft = CanScrollHorizontallyInDirection(false);
            if (CanScrollLeft != canScrollLeft)
            {
                CanScrollLeft = canScrollLeft;
            }

            bool canScrollRight = CanScrollHorizontallyInDirection(true);
            if (CanScrollRight != canScrollRight)
            {
                CanScrollRight = canScrollRight;
            }

            CanScrollHorizontally = CanScrollLeft || CanScrollRight;
            CanScroll = CanScrollVertically || CanScrollHorizontally;
        }

        private bool CanScrollVerticallyInDirection(bool inPositiveDirection)
        {
            bool canScrollInDirection = false;

            if (FlowDirection == FlowDirection.RightToLeft)
            {
                inPositiveDirection = !inPositiveDirection;
            }

            if (VerticalScrollBarVisibility != ScrollBarVisibility.Disabled)
            {
                var extentHeight = ExtentHeight;
                var viewportHeight = ViewportHeight;
                if (extentHeight > viewportHeight)
                {
                    if (inPositiveDirection)
                    {
                        var maxVerticalOffset = extentHeight - viewportHeight;
                        if (VerticalOffset < maxVerticalOffset)
                        {
                            canScrollInDirection = true;
                        }
                    }
                    else
                    {
                        if (VerticalOffset > 0)
                        {
                            canScrollInDirection = true;
                        }
                    }
                }
            }

            return canScrollInDirection;
        }

        private bool CanScrollHorizontallyInDirection(bool inPositiveDirection)
        {
            bool canScrollInDirection = false;

            if (FlowDirection == FlowDirection.RightToLeft)
            {
                inPositiveDirection = !inPositiveDirection;
            }

            if (HorizontalScrollBarVisibility != ScrollBarVisibility.Disabled)
            {
                var extentWidth = ExtentWidth;
                var viewportWidth = ViewportWidth;
                if (extentWidth > viewportWidth)
                {
                    if (inPositiveDirection)
                    {
                        var maxHorizontalOffset = extentWidth - viewportWidth;
                        if (HorizontalOffset < maxHorizontalOffset)
                        {
                            canScrollInDirection = true;
                        }
                    }
                    else
                    {
                        if (HorizontalOffset > 0)
                        {
                            canScrollInDirection = true;
                        }
                    }
                }
            }

            return canScrollInDirection;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            BringSelectedTabItemIntoView();
        }

        private void OnTabControlSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            BringSelectedTabItemIntoView();
        }

        private void BringSelectedTabItemIntoView()
        {
            if (_tabControl != null)
            {
                var item = GetSelectedTabItem(_tabControl);
                if (item != null)
                {
                    item.BringIntoView();
                }
            }
        }

        private static TabItem GetSelectedTabItem(TabControl tabControl)
        {
            object selectedItem = tabControl.SelectedItem;
            if (selectedItem != null)
            {
                // Check if the selected item is a TabItem
                TabItem tabItem = selectedItem as TabItem;
                if (tabItem == null)
                {
                    // It is a data item, get its TabItem container
                    tabItem = tabControl.ItemContainerGenerator.ContainerFromIndex(tabControl.SelectedIndex) as TabItem;

                    // Due to event leapfrogging, we may have the wrong container.
                    // If so, re-fetch the right container using a more expensive method.
                    // (BTW, the previous line will cause a debug assert in this case) 
                    if (tabItem == null ||
                        !EqualsEx(selectedItem, tabControl.ItemContainerGenerator.ItemFromContainer(tabItem)))
                    {
                        tabItem = tabControl.ItemContainerGenerator.ContainerFromItem(selectedItem) as TabItem;
                    }
                }

                return tabItem;
            }

            return null;
        }

        private static bool EqualsEx(object o1, object o2)
        {
            try
            {
                return Equals(o1, o2);
            }
            catch (InvalidCastException)
            {
                return false;
            }
        }
    }
}
