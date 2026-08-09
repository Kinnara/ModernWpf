using System.Windows;

namespace ModernWpf.Controls
{
    public enum TabViewWidthMode
    {
        Equal = 0,
        SizeToContent = 1,
        Compact = 2
    }

    public enum TabViewCloseButtonOverlayMode
    {
        Auto = 0,
        OnPointerOver = 1,
        Always = 2
    }

    public class TabViewTabCloseRequestedEventArgs
    {
        internal TabViewTabCloseRequestedEventArgs(object item, TabViewItem tab)
        {
            Item = item;
            Tab = tab;
        }

        public object Item { get; }

        public TabViewItem Tab { get; }
    }

    public class TabViewTabDroppedOutsideEventArgs
    {
        internal TabViewTabDroppedOutsideEventArgs(object item, TabViewItem tab)
        {
            Item = item;
            Tab = tab;
        }

        public object Item { get; }

        public TabViewItem Tab { get; }
    }

    public class TabViewTabDragStartingEventArgs
    {
        internal TabViewTabDragStartingEventArgs(IDataObject data, object item, TabViewItem tab)
        {
            Data = data;
            Item = item;
            Tab = tab;
        }

        public bool Cancel { get; set; }

        public IDataObject Data { get; }

        public object Item { get; }

        public TabViewItem Tab { get; }
    }

    public class TabViewTabDragCompletedEventArgs
    {
        internal TabViewTabDragCompletedEventArgs(DragDropEffects dropResult, object item, TabViewItem tab)
        {
            DropResult = dropResult;
            Item = item;
            Tab = tab;
        }

        public DragDropEffects DropResult { get; }

        public object Item { get; }

        public TabViewItem Tab { get; }
    }

    public class TabViewTabTearOutWindowRequestedEventArgs
    {
        internal TabViewTabTearOutWindowRequestedEventArgs(object[] items, UIElement[] tabs)
        {
            Items = items;
            Tabs = tabs;
        }

        public object[] Items { get; }

        public UIElement[] Tabs { get; }

        public Window NewWindow { get; set; }
    }

    public class TabViewTabTearOutRequestedEventArgs
    {
        internal TabViewTabTearOutRequestedEventArgs(object[] items, UIElement[] tabs, Window newWindow)
        {
            Items = items;
            Tabs = tabs;
            NewWindow = newWindow;
        }

        public object[] Items { get; }

        public UIElement[] Tabs { get; }

        public Window NewWindow { get; }
    }

    public class TabViewExternalTornOutTabsDroppingEventArgs
    {
        internal TabViewExternalTornOutTabsDroppingEventArgs(object[] items, UIElement[] tabs, int dropIndex)
        {
            Items = items;
            Tabs = tabs;
            DropIndex = dropIndex;
        }

        public object[] Items { get; }

        public UIElement[] Tabs { get; }

        public int DropIndex { get; }

        public bool AllowDrop { get; set; }
    }

    public class TabViewExternalTornOutTabsDroppedEventArgs
    {
        internal TabViewExternalTornOutTabsDroppedEventArgs(object[] items, UIElement[] tabs, int dropIndex)
        {
            Items = items;
            Tabs = tabs;
            DropIndex = dropIndex;
        }

        public object[] Items { get; }

        public UIElement[] Tabs { get; }

        public int DropIndex { get; }
    }
}
