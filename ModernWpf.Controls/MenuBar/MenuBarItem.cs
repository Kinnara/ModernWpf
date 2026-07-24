using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Controls.Primitives;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using ModernWpf.Automation.Peers;
using ModernWpf.Controls.Primitives;

namespace ModernWpf.Controls
{
    [ContentProperty(nameof(Items))]
    public partial class MenuBarItem : Control
    {
        static MenuBarItem()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(MenuBarItem),
                new FrameworkPropertyMetadata(typeof(MenuBarItem)));

            IsEnabledProperty.OverrideMetadata(
                typeof(MenuBarItem),
                new FrameworkPropertyMetadata(true, OnIsEnabledChanged));
        }

        public MenuBarItem()
        {
            Items = new ObservableCollection<object>();
            Items.CollectionChanged += OnItemsVectorChanged;
        }

        public ObservableCollection<object> Items { get; }

        internal bool IsFlyoutOpen => _isFlyoutOpen;

        internal MenuBarItemFlyout Flyout => _flyout;

        internal Button ContentButton => _button;

        internal FrameworkElement PassThroughElement => _passThroughElement;

        public override void OnApplyTemplate()
        {
            DetachTemplateHandlers();

            base.OnApplyTemplate();

            _button = GetTemplateChild("ContentButton") as Button;
            _parentMenuBar = FindParentMenuBar();
            _parentMenuBar?.RequestPassThroughElement(this);

            PopulateContent();
            AttachTemplateHandlers();
            UpdateVisualStates(false);
        }

        internal void AddPassThroughElement(FrameworkElement element)
        {
            _passThroughElement = element;
        }

        internal void ShowMenuFlyout()
        {
            if (Items.Count == 0)
            {
                return;
            }

            if (_button == null)
            {
                ApplyTemplate();
            }

            if (_button == null || _flyout == null)
            {
                return;
            }

            Focus();

            var options = new FlyoutShowOptions
            {
                Placement = FlyoutPlacementMode.Bottom,
                Position = new Point(0, Math.Max(0, _button.ActualHeight)),
                ExclusionRect = new Rect(0, 0, Math.Max(0, _button.ActualWidth), Math.Max(0, _button.ActualHeight))
            };

            _flyout.ShowAt(_button, options);
        }

        internal void CloseMenuFlyout()
        {
            _flyout?.Hide();
        }

        internal void Invoke()
        {
            if (IsFlyoutOpen)
            {
                CloseMenuFlyout();
            }
            else
            {
                ShowMenuFlyout();
            }
        }

        protected override AutomationPeer OnCreateAutomationPeer()
        {
            return new MenuBarItemAutomationPeer(this);
        }

        protected override void OnMouseEnter(MouseEventArgs e)
        {
            base.OnMouseEnter(e);

            if (_parentMenuBar?.IsFlyoutOpen == true)
            {
                ShowMenuFlyout();
            }

            UpdateVisualStates();
        }

        protected override void OnMouseLeave(MouseEventArgs e)
        {
            base.OnMouseLeave(e);
            UpdateVisualStates();
        }

        protected override void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnPreviewMouseLeftButtonDown(e);

            if (_parentMenuBar?.IsFlyoutOpen != true && Items.Count > 0)
            {
                _openFlyoutOnMouseLeftButtonUp = true;
            }

            UpdateVisualStates();
        }

        protected override void OnPreviewMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            base.OnPreviewMouseLeftButtonUp(e);

            if (_openFlyoutOnMouseLeftButtonUp)
            {
                _openFlyoutOnMouseLeftButtonUp = false;
                BeginOpenMenuFlyout();
            }

            UpdateVisualStates();
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            if ((Keyboard.Modifiers & ModifierKeys.Alt) == ModifierKeys.Alt)
            {
                return;
            }

            switch (e.Key)
            {
                case Key.Down:
                case Key.Enter:
                case Key.Space:
                    ShowMenuFlyout();
                    e.Handled = true;
                    break;

                case Key.Left:
                    MoveFocusTo(GetDirectionalStep(-1), openFlyout: false);
                    e.Handled = true;
                    break;

                case Key.Right:
                    MoveFocusTo(GetDirectionalStep(1), openFlyout: false);
                    e.Handled = true;
                    break;
            }
        }

        protected override void OnAccessKey(AccessKeyEventArgs e)
        {
            base.OnAccessKey(e);

            ShowMenuFlyout();
        }

        private static void OnIsEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((MenuBarItem)d).UpdateVisualStates();
        }

        protected override void OnVisualParentChanged(DependencyObject oldParent)
        {
            base.OnVisualParentChanged(oldParent);

            _parentMenuBar = FindParentMenuBar();
            _parentMenuBar?.RequestPassThroughElement(this);
        }

        private void PopulateContent()
        {
            UnhookFlyout();

            _flyout = new MenuBarItemFlyout
            {
                Placement = FlyoutPlacementMode.Bottom
            };

            SyncFlyoutItems();

            _flyout.Opening += OnFlyoutOpening;
            _flyout.Closed += OnFlyoutClosed;
            _flyout.Presenter.PreviewKeyDown += OnFlyoutPresenterKeyDown;
            _flyout.Presenter.AddHandler(MenuItem.ClickEvent, new RoutedEventHandler(OnFlyoutItemClick));
        }

        private void OnItemsVectorChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            SyncFlyoutItems();
        }

        private void SyncFlyoutItems()
        {
            if (_flyout == null)
            {
                return;
            }

            _flyout.Items.Clear();

            foreach (var item in Items)
            {
                _flyout.Items.Add(item);
            }
        }

        private void AttachTemplateHandlers()
        {
            if (_button != null)
            {
                _button.Click += OnButtonClick;
                _button.PreviewMouseLeftButtonDown += OnButtonMouseStateChanged;
                _button.PreviewMouseLeftButtonUp += OnButtonMouseStateChanged;
                _button.MouseEnter += OnButtonMouseStateChanged;
                _button.MouseLeave += OnButtonMouseStateChanged;
            }
        }

        private void DetachTemplateHandlers()
        {
            if (_button != null)
            {
                _button.Click -= OnButtonClick;
                _button.PreviewMouseLeftButtonDown -= OnButtonMouseStateChanged;
                _button.PreviewMouseLeftButtonUp -= OnButtonMouseStateChanged;
                _button.MouseEnter -= OnButtonMouseStateChanged;
                _button.MouseLeave -= OnButtonMouseStateChanged;
                _button = null;
            }

            UnhookFlyout();
        }

        private void UnhookFlyout()
        {
            if (_flyout != null)
            {
                _flyout.Opening -= OnFlyoutOpening;
                _flyout.Closed -= OnFlyoutClosed;
                _flyout.Presenter.PreviewKeyDown -= OnFlyoutPresenterKeyDown;
                _flyout.Presenter.RemoveHandler(MenuItem.ClickEvent, new RoutedEventHandler(OnFlyoutItemClick));
            }
        }

        private void OnFlyoutItemClick(object sender, RoutedEventArgs e)
        {
            if (e.OriginalSource is MenuItem item &&
                !item.HasItems &&
                !item.StaysOpenOnClick)
            {
                // WinUI MenuFlyoutItem invocation dismisses the owning
                // MenuBarItemFlyout. WPF's stock MenuItem does not reliably
                // close this custom ContextMenu host, so complete the source
                // behavior after the item's own Click handlers run.
                Dispatcher.BeginInvoke((Action)CloseMenuFlyout);
            }
        }

        private void OnButtonClick(object sender, RoutedEventArgs e)
        {
            if (!IsFlyoutOpen)
            {
                BeginOpenMenuFlyout();
            }
        }

        private void BeginOpenMenuFlyout()
        {
            Dispatcher.BeginInvoke((Action)(() =>
            {
                if (!IsFlyoutOpen)
                {
                    ShowMenuFlyout();
                }
            }));
        }

        private void OnButtonMouseStateChanged(object sender, RoutedEventArgs e)
        {
            UpdateVisualStates();
        }

        private void OnFlyoutOpening(object sender, object e)
        {
            Focus();
            _isFlyoutOpen = true;

            if (_parentMenuBar != null)
            {
                _parentMenuBar.IsFlyoutOpen = true;
            }

            UpdateVisualStates();
        }

        private void OnFlyoutClosed(object sender, object e)
        {
            _isFlyoutOpen = false;

            if (_parentMenuBar != null)
            {
                _parentMenuBar.IsFlyoutOpen = false;
            }

            UpdateVisualStates();
        }

        private void OnFlyoutPresenterKeyDown(object sender, KeyEventArgs e)
        {
            if (e.OriginalSource is MenuItem { HasItems: true })
            {
                return;
            }

            switch (e.Key)
            {
                case Key.Left:
                    MoveFocusTo(GetDirectionalStep(-1), openFlyout: true);
                    e.Handled = true;
                    break;

                case Key.Right:
                    MoveFocusTo(GetDirectionalStep(1), openFlyout: true);
                    e.Handled = true;
                    break;
            }
        }

        private int GetDirectionalStep(int logicalStep)
        {
            return FlowDirection == FlowDirection.RightToLeft ? -logicalStep : logicalStep;
        }

        private void MoveFocusTo(int step, bool openFlyout)
        {
            if (_parentMenuBar == null || _parentMenuBar.Items.Count == 0)
            {
                return;
            }

            int index = _parentMenuBar.Items.IndexOf(this);
            if (index < 0)
            {
                return;
            }

            for (int i = 0; i < _parentMenuBar.Items.Count - 1; i++)
            {
                index = (index + step + _parentMenuBar.Items.Count) % _parentMenuBar.Items.Count;
                var item = _parentMenuBar.Items[index];

                if (item.IsEnabled && item.Visibility == Visibility.Visible)
                {
                    item.Focus();

                    if (openFlyout)
                    {
                        item.ShowMenuFlyout();
                    }

                    return;
                }
            }
        }

        private void UpdateVisualStates(bool useTransitions = true)
        {
            string stateName;

            if (!IsEnabled)
            {
                stateName = "Disabled";
            }
            else if (_button?.IsPressed == true)
            {
                stateName = "Pressed";
            }
            else if (_isFlyoutOpen)
            {
                stateName = "Selected";
            }
            else if (IsMouseOver || _button?.IsMouseOver == true)
            {
                stateName = "PointerOver";
            }
            else
            {
                stateName = "Normal";
            }

            VisualStateManager.GoToState(this, stateName, useTransitions);
        }

        private MenuBar FindParentMenuBar()
        {
            DependencyObject current = this;

            while (current != null)
            {
                current = GetParent(current);

                if (current is MenuBar menuBar)
                {
                    return menuBar;
                }
            }

            return null;
        }

        private static DependencyObject GetParent(DependencyObject element)
        {
            DependencyObject visualParent = null;

            if (element is Visual || element is Visual3D)
            {
                visualParent = VisualTreeHelper.GetParent(element);
            }

            return visualParent ?? LogicalTreeHelper.GetParent(element);
        }

        private Button _button;
        private MenuBarItemFlyout _flyout;
        private MenuBar _parentMenuBar;
        private FrameworkElement _passThroughElement;
        private bool _isFlyoutOpen;
        private bool _openFlyoutOnMouseLeftButtonUp;
    }
}
