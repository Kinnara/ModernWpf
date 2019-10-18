using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace ModernWpf.Controls.Primitives
{
    public class DataGridColumnHeadersPresenterEx : DataGridColumnHeadersPresenter
    {
        private ScrollViewer _scrollViewer;

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _scrollViewer = TemplatedParent as ScrollViewer;
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            var items = Items;

            bool isFillerColumnActive = _scrollViewer != null && _scrollViewer.ComputedHorizontalScrollBarVisibility == Visibility.Collapsed;
            if (isFillerColumnActive)
            {
                for (int index = 0; index < items.Count; index++)
                {
                    if (ItemContainerGenerator.ContainerFromIndex(index) is DataGridColumnHeader columnHeader)
                    {
                        DataGridColumnHeaderHelper.SetIsLastVisibleColumnHeader(columnHeader, false);
                    }
                }
            }
            else
            {
                var lastVisibleColumnHeader = GetLastVisibleColumnHeader();
                if (lastVisibleColumnHeader != null)
                {
                    for (int index = 0; index < items.Count; index++)
                    {
                        if (ItemContainerGenerator.ContainerFromIndex(index) is DataGridColumnHeader columnHeader)
                        {
                            DataGridColumnHeaderHelper.SetIsLastVisibleColumnHeader(columnHeader, columnHeader == lastVisibleColumnHeader);
                        }
                    }
                }
            }

            return base.MeasureOverride(availableSize);
        }

        internal DataGridColumnHeader GetLastVisibleColumnHeader()
        {
            DataGridColumnHeader lastVisibleColumnHeader = null;

            var items = Items;
            for (int index = items.Count - 1; index >= 0; index--)
            {
                var columnHeader = ItemContainerGenerator.ContainerFromIndex(index) as DataGridColumnHeader;
                if (columnHeader != null &&
                    columnHeader.Visibility == Visibility.Visible)
                {
                    if (lastVisibleColumnHeader == null)
                    {
                        lastVisibleColumnHeader = columnHeader;
                    }
                    else if (lastVisibleColumnHeader.DisplayIndex < columnHeader.DisplayIndex)
                    {
                        lastVisibleColumnHeader = columnHeader;
                    }
                }
            }

            return lastVisibleColumnHeader;
        }

        private static DataGrid GetOwningGrid(FrameworkElement element)
        {
            var templatedParent = element.TemplatedParent;
            if (templatedParent is DataGrid dataGrid)
            {
                return dataGrid;
            }
            if (templatedParent is FrameworkElement frameworkElement)
            {
                return GetOwningGrid(frameworkElement);
            }
            return null;
        }
    }
}
