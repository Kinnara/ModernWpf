using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Input;
using ModernWpf.Automation.Peers;

namespace ModernWpf.Controls
{
    [TemplatePart(Name = RootPanelName, Type = typeof(Panel))]
    public class BreadcrumbBar : Control
    {
        private const string RootPanelName = "PART_RootPanel";

        static BreadcrumbBar()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(BreadcrumbBar), new FrameworkPropertyMetadata(typeof(BreadcrumbBar)));
        }

        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register(
                nameof(ItemsSource),
                typeof(object),
                typeof(BreadcrumbBar),
                new FrameworkPropertyMetadata(null, OnItemsSourcePropertyChanged));

        public object ItemsSource
        {
            get => GetValue(ItemsSourceProperty);
            set => SetValue(ItemsSourceProperty, value);
        }

        public static readonly DependencyProperty ItemTemplateProperty =
            DependencyProperty.Register(
                nameof(ItemTemplate),
                typeof(DataTemplate),
                typeof(BreadcrumbBar),
                new FrameworkPropertyMetadata(null, OnItemTemplatePropertyChanged));

        public DataTemplate ItemTemplate
        {
            get => (DataTemplate)GetValue(ItemTemplateProperty);
            set => SetValue(ItemTemplateProperty, value);
        }

        public event TypedEventHandler<BreadcrumbBar, BreadcrumbBarItemClickedEventArgs> ItemClicked;

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _rootPanel = GetTemplateChild(RootPanelName) as Panel;
            RebuildItems();
        }

        internal IReadOnlyList<BreadcrumbBarItem> Containers => new ReadOnlyCollection<BreadcrumbBarItem>(_containers);

        internal BreadcrumbBarItem ContainerFromIndex(int index)
        {
            return index >= 0 && index < _containers.Count ? _containers[index] : null;
        }

        internal void RaiseItemClicked(object item, int index)
        {
            ItemClicked?.Invoke(this, new BreadcrumbBarItemClickedEventArgs(item, index));
        }

        internal bool MoveFocusFrom(BreadcrumbBarItem item, Key key)
        {
            var index = _containers.IndexOf(item);
            if (index < 0)
            {
                return false;
            }

            var flowDirectionIsLeftToRight = FlowDirection == FlowDirection.LeftToRight;
            var delta = 0;
            if ((flowDirectionIsLeftToRight && key == Key.Right) ||
                (!flowDirectionIsLeftToRight && key == Key.Left))
            {
                delta = 1;
            }
            else if ((flowDirectionIsLeftToRight && key == Key.Left) ||
                (!flowDirectionIsLeftToRight && key == Key.Right))
            {
                delta = -1;
            }

            var nextIndex = index + delta;
            if (delta == 0 || nextIndex < 0 || nextIndex >= _containers.Count)
            {
                return false;
            }

            return _containers[nextIndex].Focus();
        }

        protected override AutomationPeer OnCreateAutomationPeer()
        {
            return new BreadcrumbBarAutomationPeer(this);
        }

        private static void OnItemsSourcePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var breadcrumbBar = (BreadcrumbBar)d;
            breadcrumbBar.UpdateCollectionChangedSubscription(e.OldValue, e.NewValue);
            breadcrumbBar.RebuildItems();
        }

        private static void OnItemTemplatePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((BreadcrumbBar)d).RebuildItems();
        }

        private void UpdateCollectionChangedSubscription(object oldValue, object newValue)
        {
            if (oldValue is INotifyCollectionChanged oldObservable)
            {
                oldObservable.CollectionChanged -= OnItemsSourceCollectionChanged;
            }

            if (newValue is INotifyCollectionChanged newObservable)
            {
                newObservable.CollectionChanged += OnItemsSourceCollectionChanged;
            }
        }

        private void OnItemsSourceCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            RebuildItems();
        }

        private void RebuildItems()
        {
            _containers.Clear();

            if (_rootPanel == null)
            {
                return;
            }

            _rootPanel.Children.Clear();

            var items = EnumerateItems(ItemsSource).ToList();
            for (var index = 0; index < items.Count; index++)
            {
                var isCurrentItem = index == items.Count - 1;
                var item = CreateContainer(items[index], index, isCurrentItem, items.Count);
                _containers.Add(item);
                _rootPanel.Children.Add(item);
            }
        }

        private BreadcrumbBarItem CreateContainer(object item, int index, bool isCurrentItem, int itemCount)
        {
            var container = item as BreadcrumbBarItem ?? new BreadcrumbBarItem
            {
                Content = item,
                ContentTemplate = ItemTemplate
            };

            if (!(item is BreadcrumbBarItem))
            {
                container.Content = item;
                container.ContentTemplate = ItemTemplate;
            }

            container.Owner = this;
            container.Index = index;
            container.SourceItem = item is BreadcrumbBarItem breadcrumbBarItem ? breadcrumbBarItem.Content : item;
            container.IsCurrentItem = isCurrentItem;

            AutomationProperties.SetName(container, GetAutomationName(container.SourceItem));
#if NET48_OR_NEWER
            AutomationProperties.SetPositionInSet(container, index + 1);
            AutomationProperties.SetSizeOfSet(container, itemCount);
#endif

            return container;
        }

        private static IEnumerable<object> EnumerateItems(object source)
        {
            if (source == null)
            {
                yield break;
            }

            if (source is IEnumerable enumerable && !(source is string))
            {
                foreach (var item in enumerable)
                {
                    yield return item;
                }
            }
            else
            {
                yield return source;
            }
        }

        private static string GetAutomationName(object item)
        {
            return item?.ToString() ?? string.Empty;
        }

        private Panel _rootPanel;
        private readonly List<BreadcrumbBarItem> _containers = new List<BreadcrumbBarItem>();
    }
}
