// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Dragablz;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;

namespace DragablzSample
{
    public class TabablzControlEx : TabablzControl
    {
        static TabablzControlEx()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(TabablzControlEx), new FrameworkPropertyMetadata(typeof(TabablzControlEx)));
        }

        public override void OnApplyTemplate()
        {
            if (_itemsPresenter != null)
            {
                _itemsPresenter.SizeChanged -= OnItemsPresenterSizeChanged;
                _itemsPresenter = null;
            }

            base.OnApplyTemplate();

            _rightContentPresenter = GetTemplateChild("RightContentPresenter") as ContentPresenter;

            _leftContentColumn = GetTemplateChild("LeftContentColumn") as ColumnDefinition;
            _tabColumn = GetTemplateChild("TabColumn") as ColumnDefinition;
            _addButtonColumn = GetTemplateChild("AddButtonColumn") as ColumnDefinition;
            _rightContentColumn = GetTemplateChild("RightContentColumn") as ColumnDefinition;

            _tabContainerGrid = GetTemplateChild("TabContainerGrid") as Grid;

            _itemsControl = GetTemplateChild(HeaderItemsControlPartName) as DragablzItemsControlEx;
            if (_itemsControl != null)
            {
                _itemsControl.ApplyTemplate();

                _itemsPresenter = _itemsControl.Template?.FindName("TabsItemsPresenter", _itemsControl) as ItemsPresenter;
                if (_itemsPresenter != null)
                {
                    _itemsPresenter.SizeChanged += OnItemsPresenterSizeChanged;
                }
            }
        }

        protected override Size MeasureOverride(Size constraint)
        {
            if (_previousAvailableSize.Width != constraint.Width)
            {
                _previousAvailableSize = constraint;
                UpdateTabWidths();
            }

            return base.MeasureOverride(constraint);
        }

        protected override void OnItemsChanged(NotifyCollectionChangedEventArgs e)
        {
            base.OnItemsChanged(e);

            UpdateTabWidths();
        }

        private void OnItemsPresenterSizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateTabWidths();
            _itemsControl?.UpdateScrollViewerDecreaseAndIncreaseButtonsViewState();
        }

        private void UpdateTabWidths()
        {
            if (_tabContainerGrid != null)
            {
                // Add up width taken by custom content and + button
                double widthTaken = 0.0;
                if (_leftContentColumn != null)
                {
                    widthTaken += _leftContentColumn.ActualWidth;
                }
                if (_addButtonColumn != null)
                {
                    widthTaken += _addButtonColumn.ActualWidth;
                }
                if (_rightContentColumn != null)
                {
                    if (_rightContentPresenter != null)
                    {
                        Size rightContentSize = _rightContentPresenter.DesiredSize;
                        _rightContentPresenter.MinWidth = rightContentSize.Width;
                        widthTaken += rightContentSize.Width;
                    }
                }

                if (_tabColumn != null)
                {
                    // Note: can be infinite
                    var availableWidth = _previousAvailableSize.Width - widthTaken;

                    // Size can be 0 when window is first created; in that case, skip calculations; we'll get a new size soon
                    if (availableWidth > 0)
                    {
                        _tabColumn.MaxWidth = availableWidth;
                        _tabColumn.Width = GridLength.Auto;
                        if (_itemsControl != null)
                        {
                            _itemsControl.MaxWidth = availableWidth;

                            // Calculate if the scroll buttons should be visible.
                            if (_itemsPresenter != null)
                            {
                                var visible = _itemsPresenter.ActualWidth > availableWidth;
                                ScrollViewer.SetHorizontalScrollBarVisibility(_itemsControl, visible
                                    ? ScrollBarVisibility.Visible
                                    : ScrollBarVisibility.Hidden);
                                if (visible)
                                {
                                    _itemsControl.UpdateScrollViewerDecreaseAndIncreaseButtonsViewState();
                                }
                            }
                        }
                    }
                }
            }
        }

        private ColumnDefinition _leftContentColumn;
        private ColumnDefinition _tabColumn;
        private ColumnDefinition _addButtonColumn;
        private ColumnDefinition _rightContentColumn;

        private DragablzItemsControlEx _itemsControl;
        private ContentPresenter _rightContentPresenter;
        private Grid _tabContainerGrid;
        private ItemsPresenter _itemsPresenter;

        private Size _previousAvailableSize;
    }
}
