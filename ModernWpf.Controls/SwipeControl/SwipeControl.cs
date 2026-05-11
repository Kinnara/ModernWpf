using System;
using System.Linq;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace ModernWpf.Controls
{
    [TemplatePart(Name = LeftItemsPanelName, Type = typeof(Panel))]
    [TemplatePart(Name = RightItemsPanelName, Type = typeof(Panel))]
    [TemplatePart(Name = TopItemsPanelName, Type = typeof(Panel))]
    [TemplatePart(Name = BottomItemsPanelName, Type = typeof(Panel))]
    public class SwipeControl : ContentControl
    {
        private const string LeftItemsPanelName = "PART_LeftItemsPanel";
        private const string RightItemsPanelName = "PART_RightItemsPanel";
        private const string TopItemsPanelName = "PART_TopItemsPanel";
        private const string BottomItemsPanelName = "PART_BottomItemsPanel";

        static SwipeControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(SwipeControl),
                new FrameworkPropertyMetadata(typeof(SwipeControl)));
        }

        public SwipeControl()
        {
            IsTabStop = false;
        }

        public static readonly DependencyProperty LeftItemsProperty =
            DependencyProperty.Register(
                nameof(LeftItems),
                typeof(SwipeItems),
                typeof(SwipeControl),
                new FrameworkPropertyMetadata(null, OnItemsPropertyChanged));

        public SwipeItems LeftItems
        {
            get => (SwipeItems)GetValue(LeftItemsProperty);
            set
            {
                ValidateSwipeItemsCanSet(value, SwipeItemsPlacement.Left);
                SetValue(LeftItemsProperty, value);
            }
        }

        public static readonly DependencyProperty RightItemsProperty =
            DependencyProperty.Register(
                nameof(RightItems),
                typeof(SwipeItems),
                typeof(SwipeControl),
                new FrameworkPropertyMetadata(null, OnItemsPropertyChanged));

        public SwipeItems RightItems
        {
            get => (SwipeItems)GetValue(RightItemsProperty);
            set
            {
                ValidateSwipeItemsCanSet(value, SwipeItemsPlacement.Right);
                SetValue(RightItemsProperty, value);
            }
        }

        public static readonly DependencyProperty TopItemsProperty =
            DependencyProperty.Register(
                nameof(TopItems),
                typeof(SwipeItems),
                typeof(SwipeControl),
                new FrameworkPropertyMetadata(null, OnItemsPropertyChanged));

        public SwipeItems TopItems
        {
            get => (SwipeItems)GetValue(TopItemsProperty);
            set
            {
                ValidateSwipeItemsCanSet(value, SwipeItemsPlacement.Top);
                SetValue(TopItemsProperty, value);
            }
        }

        public static readonly DependencyProperty BottomItemsProperty =
            DependencyProperty.Register(
                nameof(BottomItems),
                typeof(SwipeItems),
                typeof(SwipeControl),
                new FrameworkPropertyMetadata(null, OnItemsPropertyChanged));

        public SwipeItems BottomItems
        {
            get => (SwipeItems)GetValue(BottomItemsProperty);
            set
            {
                ValidateSwipeItemsCanSet(value, SwipeItemsPlacement.Bottom);
                SetValue(BottomItemsProperty, value);
            }
        }

        public void Close()
        {
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _leftItemsPanel = GetTemplateChild(LeftItemsPanelName) as Panel;
            _rightItemsPanel = GetTemplateChild(RightItemsPanelName) as Panel;
            _topItemsPanel = GetTemplateChild(TopItemsPanelName) as Panel;
            _bottomItemsPanel = GetTemplateChild(BottomItemsPanelName) as Panel;

            RebuildSwipeItems();
        }

        internal void ValidateSwipeItemsCanAdd(SwipeItemsPlacement placement)
        {
            if (IsHorizontalPlacement(placement) && HasVerticalItems())
            {
                throw new ArgumentException("SwipeControl can only have horizontal or vertical items.");
            }

            if (IsVerticalPlacement(placement) && HasHorizontalItems())
            {
                throw new ArgumentException("SwipeControl can only have horizontal or vertical items.");
            }
        }

        internal void OnSwipeItemsChanged()
        {
            RebuildSwipeItems();
        }

        private static void OnItemsPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var swipeControl = (SwipeControl)d;
            var placement = GetPlacement(e.Property);

            if (e.OldValue is SwipeItems oldItems)
            {
                oldItems.DetachOwner(swipeControl);
            }

            if (e.NewValue is SwipeItems newItems)
            {
                swipeControl.ValidateSwipeItemsCanSet(newItems, placement);
                newItems.AttachOwner(swipeControl, placement);
            }

            swipeControl.RebuildSwipeItems();
        }

        private void ValidateSwipeItemsCanSet(SwipeItems items, SwipeItemsPlacement placement)
        {
            if (items == null || items.Count == 0)
            {
                return;
            }

            if (items.Mode == SwipeMode.Execute && items.Count > 1)
            {
                throw new ArgumentException("Execute items should only have one item.");
            }

            if (IsHorizontalPlacement(placement) && HasVerticalItems())
            {
                throw new ArgumentException("SwipeControl can only have horizontal or vertical items.");
            }

            if (IsVerticalPlacement(placement) && HasHorizontalItems())
            {
                throw new ArgumentException("SwipeControl can only have horizontal or vertical items.");
            }
        }

        private bool HasHorizontalItems()
        {
            return HasItems(LeftItems) || HasItems(RightItems);
        }

        private bool HasVerticalItems()
        {
            return HasItems(TopItems) || HasItems(BottomItems);
        }

        private static bool HasItems(SwipeItems items)
        {
            return items?.Count > 0;
        }

        private static bool IsHorizontalPlacement(SwipeItemsPlacement placement)
        {
            return placement == SwipeItemsPlacement.Left || placement == SwipeItemsPlacement.Right;
        }

        private static bool IsVerticalPlacement(SwipeItemsPlacement placement)
        {
            return placement == SwipeItemsPlacement.Top || placement == SwipeItemsPlacement.Bottom;
        }

        private static SwipeItemsPlacement GetPlacement(DependencyProperty property)
        {
            if (property == LeftItemsProperty)
            {
                return SwipeItemsPlacement.Left;
            }

            if (property == RightItemsProperty)
            {
                return SwipeItemsPlacement.Right;
            }

            if (property == TopItemsProperty)
            {
                return SwipeItemsPlacement.Top;
            }

            if (property == BottomItemsProperty)
            {
                return SwipeItemsPlacement.Bottom;
            }

            return SwipeItemsPlacement.None;
        }

        private void RebuildSwipeItems()
        {
            RebuildPanel(_leftItemsPanel, LeftItems);
            RebuildPanel(_rightItemsPanel, RightItems);
            RebuildPanel(_topItemsPanel, TopItems);
            RebuildPanel(_bottomItemsPanel, BottomItems);
        }

        private void RebuildPanel(Panel panel, SwipeItems items)
        {
            if (panel == null)
            {
                return;
            }

            foreach (var button in panel.Children.OfType<Button>().ToList())
            {
                button.Click -= OnSwipeItemButtonClick;
            }

            panel.Children.Clear();
            panel.Visibility = HasItems(items) ? Visibility.Visible : Visibility.Collapsed;

            if (items == null)
            {
                return;
            }

            foreach (var item in items)
            {
                var button = CreateButtonForItem(item);
                button.Click += OnSwipeItemButtonClick;
                panel.Children.Add(button);
            }
        }

        private Button CreateButtonForItem(SwipeItem item)
        {
            var button = new Button
            {
                Tag = item,
                Content = CreateButtonContent(item),
                MinWidth = 68,
                MinHeight = 44,
                Padding = new Thickness(8, 4, 8, 4),
                Background = item.Background,
                Foreground = item.Foreground
            };

            AutomationProperties.SetName(button, item.Text ?? string.Empty);

            var command = item.Command;
            if (command != null)
            {
                button.IsEnabled = command.CanExecute(item.CommandParameter);
            }

            return button;
        }

        private static object CreateButtonContent(SwipeItem item)
        {
            if (item.IconSource == null)
            {
                return item.Text;
            }

            var panel = new StackPanel
            {
                Orientation = Orientation.Horizontal
            };

            var icon = item.IconSource.CreateIconElement();
            icon.Margin = new Thickness(0, 0, 4, 0);
            panel.Children.Add(icon);
            panel.Children.Add(new TextBlock
            {
                Text = item.Text,
                VerticalAlignment = VerticalAlignment.Center
            });
            return panel;
        }

        private void OnSwipeItemButtonClick(object sender, RoutedEventArgs e)
        {
            if (sender is Button { Tag: SwipeItem item })
            {
                item.Invoke(this);
            }
        }

        private Panel _leftItemsPanel;
        private Panel _rightItemsPanel;
        private Panel _topItemsPanel;
        private Panel _bottomItemsPanel;
    }
}
