// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Dragablz;
using ModernWpf.Controls.Primitives;
using System;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;

namespace DragablzSample
{
    public class DragablzItemsControlEx : DragablzItemsControl
    {
        private const double c_scrollAmount = 50.0;

        static DragablzItemsControlEx()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(DragablzItemsControlEx), new FrameworkPropertyMetadata(typeof(DragablzItemsControlEx)));
        }

        public override void OnApplyTemplate()
        {
            if (_scrollViewer != null)
            {
                _scrollViewer.ScrollChanged -= OnScrollViewerScrollChanged;
            }

            if (_scrollDecreaseButton != null)
            {
                _scrollDecreaseButton.IsVisibleChanged -= OnScrollButtonIsVisibleChanged;
                _scrollDecreaseButton.Click -= OnScrollDecreaseClick;
            }

            if (_scrollIncreaseButton != null)
            {
                _scrollIncreaseButton.IsVisibleChanged -= OnScrollButtonIsVisibleChanged;
                _scrollIncreaseButton.Click -= OnScrollIncreaseClick;
            }

            base.OnApplyTemplate();

            _scrollViewer = GetTemplateChild("ScrollViewer") as ScrollViewer;
            if (_scrollViewer != null)
            {
                _scrollViewer.ApplyTemplate();
                _scrollViewer.ScrollChanged += OnScrollViewerScrollChanged;

                _scrollDecreaseButton = _scrollViewer.Template?.FindName("ScrollDecreaseButton", _scrollViewer) as RepeatButton;
                if (_scrollDecreaseButton != null)
                {
                    _scrollDecreaseButton.IsVisibleChanged += OnScrollButtonIsVisibleChanged;
                    _scrollDecreaseButton.Click += OnScrollDecreaseClick;
                }

                _scrollIncreaseButton = _scrollViewer.Template?.FindName("ScrollIncreaseButton", _scrollViewer) as RepeatButton;
                if (_scrollIncreaseButton != null)
                {
                    _scrollIncreaseButton.IsVisibleChanged += OnScrollButtonIsVisibleChanged;
                    _scrollIncreaseButton.Click += OnScrollIncreaseClick;
                }
            }
        }

        protected override Size MeasureOverride(Size constraint)
        {
            var desiredSize = base.MeasureOverride(constraint);

            if (ItemsOrganiser != null)
            {
                var padding = Padding;
                var width = ItemsPresenterWidth + padding.Left + padding.Right;
                var height = ItemsPresenterHeight + padding.Top + padding.Bottom;

                if (_scrollDecreaseButton != null && _scrollDecreaseButton.IsVisible)
                {
                    width += _scrollDecreaseButton.ActualWidth;
                }
                if (_scrollIncreaseButton != null && _scrollIncreaseButton.IsVisible)
                {
                    width += _scrollIncreaseButton.ActualWidth;
                }

                desiredSize.Width = Math.Min(constraint.Width, width);
                desiredSize.Height = Math.Min(constraint.Height, height);
            }

            return desiredSize;
        }

        protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
        {
            base.PrepareContainerForItemOverride(element, item);

            if (element is DragablzItem dragablzItem && item is TabItem tabItem)
            {
                dragablzItem.SetBinding(DragablzItemHelper.IconProperty,
                    new Binding { Path = new PropertyPath(TabItemHelper.IconProperty), Source = tabItem });
            }
        }

        protected override void ClearContainerForItemOverride(DependencyObject element, object item)
        {
            base.ClearContainerForItemOverride(element, item);

            if (element is DragablzItem dragablzItem && item is TabItem)
            {
                dragablzItem.ClearValue(DragablzItemHelper.IconProperty);
            }
        }

        protected override void OnItemsChanged(NotifyCollectionChangedEventArgs e)
        {
            base.OnItemsChanged(e);
            InvalidateMeasure();
        }

        private void OnScrollButtonIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (!(bool)e.NewValue)
            {
                InvalidateMeasure();
            }
        }

        private void OnScrollDecreaseClick(object sender, RoutedEventArgs e)
        {
            if (_scrollViewer != null)
            {
                _scrollViewer.ScrollToHorizontalOffset(Math.Max(0, _scrollViewer.HorizontalOffset - c_scrollAmount));
            }
        }

        private void OnScrollIncreaseClick(object sender, RoutedEventArgs e)
        {
            if (_scrollViewer != null)
            {
                _scrollViewer.ScrollToHorizontalOffset(Math.Min(_scrollViewer.ScrollableWidth, _scrollViewer.HorizontalOffset + c_scrollAmount));
            }
        }

        private void OnScrollViewerScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            UpdateScrollViewerDecreaseAndIncreaseButtonsViewState();
        }

        internal void UpdateScrollViewerDecreaseAndIncreaseButtonsViewState()
        {
            if (_scrollViewer != null && _scrollDecreaseButton != null && _scrollIncreaseButton != null)
            {
                const double minThreshold = 0.1;
                var horizontalOffset = _scrollViewer.HorizontalOffset;
                var scrollableWidth = _scrollViewer.ScrollableWidth;

                if (Math.Abs(horizontalOffset - scrollableWidth) < minThreshold)
                {
                    _scrollDecreaseButton.IsEnabled = true;
                    _scrollIncreaseButton.IsEnabled = false;
                }
                else if (Math.Abs(horizontalOffset) < minThreshold)
                {
                    _scrollDecreaseButton.IsEnabled = false;
                    _scrollIncreaseButton.IsEnabled = true;
                }
                else
                {
                    _scrollDecreaseButton.IsEnabled = true;
                    _scrollIncreaseButton.IsEnabled = true;
                }
            }
        }

        private ScrollViewer _scrollViewer;
        private RepeatButton _scrollDecreaseButton;
        private RepeatButton _scrollIncreaseButton;
    }
}
