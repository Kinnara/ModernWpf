using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace ModernWpf.Controls
{
    internal sealed class TabViewListView : ItemsControl
    {
        protected override bool IsItemItsOwnContainerOverride(object item)
        {
            return item is TabViewItem;
        }

        protected override DependencyObject GetContainerForItemOverride()
        {
            return new ContentPresenter();
        }

        protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
        {
            base.PrepareContainerForItemOverride(element, item);

            if (element is ContentPresenter presenter)
            {
                presenter.HorizontalAlignment = HorizontalAlignment.Stretch;
                presenter.VerticalAlignment = VerticalAlignment.Stretch;
            }

            Dispatcher.BeginInvoke(
                DispatcherPriority.Loaded,
                new Action(() => Owner?.PrepareRealizedContainers()));
        }

        protected override void ClearContainerForItemOverride(DependencyObject element, object item)
        {
            Owner?.ClearRealizedContainer(element, item);
            base.ClearContainerForItemOverride(element, item);
        }

        internal TabView Owner { get; set; }
    }
}
