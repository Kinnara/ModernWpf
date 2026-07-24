using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Input;
using ModernWpf.Automation.Peers;

namespace ModernWpf.Controls
{
    [ContentProperty(nameof(Items))]
    public class MenuBar : Control
    {
        static MenuBar()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(MenuBar),
                new FrameworkPropertyMetadata(typeof(MenuBar)));

            FocusableProperty.OverrideMetadata(
                typeof(MenuBar),
                new FrameworkPropertyMetadata(false));
        }

        public MenuBar()
        {
            Items = new ObservableCollection<MenuBarItem>();
            Items.CollectionChanged += OnItemsVectorChanged;
        }

        public ObservableCollection<MenuBarItem> Items { get; }

        internal bool IsFlyoutOpen { get; set; }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _layoutRoot = GetTemplateChild("LayoutRoot") as FrameworkElement;
            _contentRoot = GetTemplateChild("ContentRoot") as ItemsControl;

            if (_contentRoot != null)
            {
                _contentRoot.ItemsSource = Items;
                KeyboardNavigation.SetDirectionalNavigation(_contentRoot, KeyboardNavigationMode.Cycle);
            }

            UpdateAutomationSizeAndPosition();
        }

        internal void RequestPassThroughElement(MenuBarItem menuBarItem)
        {
            if (_layoutRoot != null)
            {
                menuBarItem.AddPassThroughElement(_layoutRoot);
            }
        }

        internal void UpdateAutomationSizeAndPosition()
        {
            int visibleItemCount = 0;

            foreach (var item in Items)
            {
                if (item.Visibility == Visibility.Visible)
                {
                    visibleItemCount++;
                }
            }

            int position = 1;
            foreach (var item in Items)
            {
                if (item.Visibility == Visibility.Visible)
                {
#if NET48_OR_NEWER
                    AutomationProperties.SetPositionInSet(item, position++);
                    AutomationProperties.SetSizeOfSet(item, visibleItemCount);
#else
                    position++;
#endif
                }
            }
        }

        protected override AutomationPeer OnCreateAutomationPeer()
        {
            return new MenuBarAutomationPeer(this);
        }

        private void OnItemsVectorChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            UpdateAutomationSizeAndPosition();
        }

        private FrameworkElement _layoutRoot;
        private ItemsControl _contentRoot;
    }
}
