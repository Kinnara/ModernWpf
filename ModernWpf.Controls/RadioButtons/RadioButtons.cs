using System.Collections;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Markup;
using ModernWpf.Controls.Primitives;

namespace ModernWpf.Controls
{
    [ContentProperty(nameof(Items))]
    [TemplatePart(Name = s_repeaterName, Type = typeof(ItemsRepeater))]
    public class RadioButtons : Control
    {
        static RadioButtons()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(RadioButtons),
                new FrameworkPropertyMetadata(typeof(RadioButtons)));
        }

        public RadioButtons()
        {
            ItemTemplate = new RadioButtonsElementFactory();

            var items = new List<object>();
            SetValue(ItemsProperty, items);

            PreviewKeyDown += OnChildPreviewKeyDown;
        }

        #region ItemsSource

        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register(
                nameof(ItemsSource),
                typeof(IEnumerable),
                typeof(RadioButtons),
                new FrameworkPropertyMetadata(OnItemsSourcePropertyChanged));

        public IEnumerable ItemsSource
        {
            get => (IEnumerable)GetValue(ItemsSourceProperty);
            set => SetValue(ItemsSourceProperty, value);
        }

        private static void OnItemsSourcePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((RadioButtons)d).UpdateItemsSource();
        }

        #endregion

        #region Items

        public static readonly DependencyProperty ItemsProperty =
            DependencyProperty.Register(
                nameof(Items),
                typeof(IList),
                typeof(RadioButtons),
                new FrameworkPropertyMetadata(OnItemsPropertyChanged));

        public IList Items
        {
            get => (IList)GetValue(ItemsProperty);
        }

        private static void OnItemsPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((RadioButtons)d).UpdateItemsSource();
        }

        #endregion

        #region ItemTemplate

        public static readonly DependencyProperty ItemTemplateProperty =
            DependencyProperty.Register(
                nameof(ItemTemplate),
                typeof(object),
                typeof(RadioButtons));

        public object ItemTemplate
        {
            get => GetValue(ItemTemplateProperty);
            set => SetValue(ItemTemplateProperty, value);
        }

        #endregion

        #region SelectedIndex

        public static readonly DependencyProperty SelectedIndexProperty =
            DependencyProperty.Register(
                nameof(SelectedIndex),
                typeof(int),
                typeof(RadioButtons),
                new FrameworkPropertyMetadata(-1, OnSelectedIndexPropertyChanged));

        public int SelectedIndex
        {
            get => (int)GetValue(SelectedIndexProperty);
            set => SetValue(SelectedIndexProperty, value);
        }

        private static void OnSelectedIndexPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((RadioButtons)d).UpdateSelectedIndex();
        }

        #endregion

        #region SelectedItem

        public static readonly DependencyProperty SelectedItemProperty =
            DependencyProperty.Register(
                nameof(SelectedItem),
                typeof(object),
                typeof(RadioButtons));

        public object SelectedItem
        {
            get => GetValue(SelectedItemProperty);
            set => SetValue(SelectedItemProperty, value);
        }

        #endregion

        #region MaximumColumns

        public static readonly DependencyProperty MaximumColumnsProperty =
            DependencyProperty.Register(
                nameof(MaximumColumns),
                typeof(int),
                typeof(RadioButtons),
                new FrameworkPropertyMetadata(1));

        public int MaximumColumns
        {
            get => (int)GetValue(MaximumColumnsProperty);
            set => SetValue(MaximumColumnsProperty, value);
        }

        #endregion

        #region Header

        public static readonly DependencyProperty HeaderProperty =
            ControlHelper.HeaderProperty.AddOwner(typeof(RadioButtons));

        public object Header
        {
            get => GetValue(HeaderProperty);
            set => SetValue(HeaderProperty, value);
        }

        #endregion

        public static readonly RoutedEvent SelectionChangedEvent =
            EventManager.RegisterRoutedEvent(
                nameof(SelectionChanged),
                RoutingStrategy.Direct,
                typeof(SelectionChangedEventHandler),
                typeof(RadioButtons));

        public event SelectionChangedEventHandler SelectionChanged
        {
            add { AddHandler(SelectionChangedEvent, value); }
            remove { RemoveHandler(SelectionChangedEvent, value); }
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            if (m_repeater != null)
            {
                m_repeater.ElementPrepared -= OnRepeaterElementPrepared;
                m_repeater.ElementClearing -= OnRepeaterElementClearing;
                m_repeater.ElementIndexChanged -= OnRepeaterElementIndexChanged;
                m_repeater.Loaded -= OnRepeaterLoaded;

                if (m_repeater.Layout is ColumnMajorUniformToLargestGridLayout layout)
                {
                    layout.ClearValue(ColumnMajorUniformToLargestGridLayout.MaximumColumnsProperty);
                }
            }

            m_repeater = GetTemplateChild(s_repeaterName) as ItemsRepeater;

            if (m_repeater != null)
            {
                m_repeater.ElementPrepared += OnRepeaterElementPrepared;
                m_repeater.ElementClearing += OnRepeaterElementClearing;
                m_repeater.ElementIndexChanged += OnRepeaterElementIndexChanged;
                m_repeater.Loaded += OnRepeaterLoaded;

                if (m_repeater.Layout is ColumnMajorUniformToLargestGridLayout layout)
                {
                    BindingOperations.SetBinding(layout,
                        ColumnMajorUniformToLargestGridLayout.MaximumColumnsProperty,
                        new Binding { Path = new PropertyPath(MaximumColumnsProperty), Source = this });
                }
            }

            UpdateItemsSource();
        }

        // void OnGettingFocus(object sender, GettingFocusEventArgs args);
        protected override void OnGotKeyboardFocus(KeyboardFocusChangedEventArgs args)
        {
            var repeater = m_repeater;
            if (repeater != null)
            {
                if (m_selectedIndex >= 0)
                {
                    var oldFocusedElement = args.OldFocus;
                    if (oldFocusedElement != null)
                    {
                        if (oldFocusedElement is UIElement oldFocusedElementAsUIElement)
                        {
                            // If focus is coming from outside the repeater, put focus on the selected item.
                            if (repeater.GetElementIndex(oldFocusedElementAsUIElement) < 0)
                            {
                                var selectedItem = repeater.TryGetElement(m_selectedIndex);
                                if (selectedItem != null)
                                {
                                    // TODO: TrySetNewFocusedElement
                                    //var argsAsIGettingFocusEventArgs2 = args as IGettingFocusEventArgs2;
                                    //if (argsAsIGettingFocusEventArgs2 != null)
                                    //{
                                    //    if (args.TrySetNewFocusedElement(selectedItem))
                                    //    {
                                    //        args.Handled(true);
                                    //    }
                                    //}
                                }
                            }
                        }
                    }
                }

                // On RS3+ Selection follows focus unless control is held down.
                if ((args.KeyboardDevice.Modifiers & ModifierKeys.Control) != ModifierKeys.Control)
                {
                    if (args.NewFocus is UIElement newFocusedElementAsUIE)
                    {
                        Select(repeater.GetElementIndex(newFocusedElementAsUIE));
                        args.Handled = true;
                    }
                }
            }

            base.OnGotKeyboardFocus(args);
        }

        void OnRepeaterLoaded(object sender, RoutedEventArgs args)
        {
            var repeater = m_repeater;
            if (repeater != null)
            {
                UpdateSelectedIndex();
                OnRepeaterCollectionChanged(null, null);
            }
        }

        void OnChildPreviewKeyDown(object sender, KeyEventArgs args)
        {
            switch (args.Key)
            {
                case Key.Down:
                    if (MoveFocusNext())
                    {
                        args.Handled = true;
                        return;
                    }
                    args.Handled = HandleEdgeCaseFocus(false, args.OriginalSource);
                    break;
                case Key.Up:
                    if (MoveFocusPrevious())
                    {
                        args.Handled = true;
                        return;
                    }
                    args.Handled = HandleEdgeCaseFocus(true, args.OriginalSource);
                    break;
                case Key.Right:
                    {
                        if (args.OriginalSource is UIElement sourceElement)
                        {
                            if (sourceElement.MoveFocus(new TraversalRequest(FocusNavigationDirection.Right)))
                            {
                                args.Handled = true;
                                return;
                            }
                            args.Handled = HandleEdgeCaseFocus(false, args.OriginalSource);
                        }
                    }
                    break;
                case Key.Left:
                    {
                        if (args.OriginalSource is UIElement sourceElement)
                        {
                            if (sourceElement.MoveFocus(new TraversalRequest(FocusNavigationDirection.Left)))
                            {
                                args.Handled = true;
                                return;
                            }
                            args.Handled = HandleEdgeCaseFocus(true, args.OriginalSource);
                        }
                    }
                    break;
            }
        }

        // If we haven't handled the key yet and the original source was the first(for up and left)
        // or last(for down and right) element in the repeater we need to handle the key so
        // RadioButton doesn't, which would result in the behavior.
        bool HandleEdgeCaseFocus(bool first, object source)
        {
            var repeater = m_repeater;
            if (repeater != null)
            {
                if (source is UIElement sourceAsUIElement)
                {
                    int calculateIndex()
                    {
                        if (first)
                        {
                            return 0;
                        }
                        var itemsSourceView = repeater.ItemsSourceView;
                        if (itemsSourceView != null)
                        {
                            return itemsSourceView.Count - 1;
                        }
                        return -1;
                    };
                    var index = calculateIndex();

                    if (repeater.GetElementIndex(sourceAsUIElement) == index)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        void OnRepeaterElementPrepared(ItemsRepeater sender, ItemsRepeaterElementPreparedEventArgs args)
        {
            var element = args.Element;
            if (element != null)
            {
                if (element is ToggleButton toggleButton)
                {
                    toggleButton.Checked += OnChildChecked;
                    toggleButton.Unchecked += OnChildUnchecked;
                    //var childHandlers = <ChildHandlers>();
                    //childHandlers.checkedRevoker = toggleButton.Checked(auto_revoke, { this, RadioButtons.OnChildChecked });
                    //childHandlers.uncheckedRevoker = toggleButton.Unchecked(auto_revoke, { this, RadioButtons.OnChildUnchecked });

                    //toggleButton.SetValue(s_childHandlersProperty, childHandlers.as< object > ());
                }
#if NETCOREAPP3_0
                var repeater = m_repeater;
                if (repeater != null)
                {
                    var itemSourceView = repeater.ItemsSourceView;
                    if (itemSourceView != null)
                    {
                        element.SetValue(AutomationProperties.PositionInSetProperty, args.Index + 1);
                        element.SetValue(AutomationProperties.SizeOfSetProperty, itemSourceView.Count);
                    }
                }
#endif
            }
        }

        void OnRepeaterElementClearing(ItemsRepeater sender, ItemsRepeaterElementClearingEventArgs args)
        {
            var element = args.Element;
            if (element != null)
            {
                if (element is ToggleButton toggleButton)
                {
                    toggleButton.Checked -= OnChildChecked;
                    toggleButton.Unchecked -= OnChildUnchecked;
                }
            }
        }

        void OnRepeaterElementIndexChanged(ItemsRepeater sender, ItemsRepeaterElementIndexChangedEventArgs args)
        {
#if NETCOREAPP3_0
            var element = args.Element;
            if (element != null)
            {
                element.SetValue(AutomationProperties.PositionInSetProperty, args.NewIndex + 1);
            }
#endif
            if (args.OldIndex == m_selectedIndex)
            {
                Select(args.NewIndex);
            }
        }

        void OnRepeaterCollectionChanged(object sender, object args)
        {
#if NETCOREAPP3_0
            var repeater = m_repeater;
            if (repeater != null)
            {
                var itemSourceView = repeater.ItemsSourceView;
                if (itemSourceView != null)
                {
                    var count = itemSourceView.Count;
                    for (var index = 0; index < count; index++)
                    {
                        var radioButton = repeater.TryGetElement(index);
                        if (radioButton != null)
                        {
                            radioButton.SetValue(AutomationProperties.SizeOfSetProperty, count);
                        }
                    }
                }
            }
#endif
        }

        void Select(int index)
        {
            if (!m_currentlySelecting && m_selectedIndex != index)
            {
                // Calling Select updates the checked state on the radio button being selected
                // and the radio button being unselected, as well as updates the SelectedIndex
                // and SelectedItem DP. All of these things would cause Select to be called so
                // we'll prevent reentrency with this m_currentlySelecting boolean.
                try
                {
                    m_currentlySelecting = true;

                    var previousSelectedIndex = m_selectedIndex;
                    m_selectedIndex = index;

                    var newSelectedItem = GetDataAtIndex(m_selectedIndex, true);
                    var previousSelectedItem = GetDataAtIndex(previousSelectedIndex, false);

                    SelectedIndex = m_selectedIndex;
                    SelectedItem = newSelectedItem;
                    RaiseEvent(new SelectionChangedEventArgs(SelectionChangedEvent, new[] { previousSelectedItem }, new[] { newSelectedItem }));
                }
                finally
                {
                    m_currentlySelecting = false;
                }
            }
        }

        object GetDataAtIndex(int index, bool containerIsChecked)
        {
            var repeater = m_repeater;
            if (repeater != null)
            {
                var item = repeater.TryGetElement(index);
                if (item != null)
                {
                    if (item is ToggleButton itemAsToggleButton)
                    {
                        itemAsToggleButton.IsChecked = containerIsChecked;
                    }
                }
                if (index >= 0)
                {
                    var itemsSourceView = repeater.ItemsSourceView;
                    if (itemsSourceView != null)
                    {
                        if (index < itemsSourceView.Count)
                        {
                            return itemsSourceView.GetAt(index);
                        }
                    }
                }
            }
            return null;
        }

        void OnChildChecked(object sender, RoutedEventArgs args)
        {
            if (!m_currentlySelecting)
            {
                var repeater = m_repeater;
                if (repeater != null)
                {
                    if (sender is UIElement senderAsUIE)
                    {
                        Select(repeater.GetElementIndex(senderAsUIE));
                    }
                }
            }
        }

        void OnChildUnchecked(object sender, RoutedEventArgs args)
        {
            if (!m_currentlySelecting)
            {
                var repeater = m_repeater;
                if (repeater != null)
                {
                    if (sender is UIElement senderAsUIE)
                    {
                        if (m_selectedIndex == repeater.GetElementIndex(senderAsUIE))
                        {
                            Select(-1);
                        }
                    }
                }
            }
        }

        bool MoveFocusNext()
        {
            return MoveFocus(1);
        }

        bool MoveFocusPrevious()
        {
            return MoveFocus(-1);
        }

        bool MoveFocus(int indexIncrement)
        {
            var repeater = m_repeater;
            if (repeater != null)
            {
                if (Keyboard.FocusedElement is UIElement focusedElement)
                {
                    var focusedIndex = repeater.GetElementIndex(focusedElement);

                    if (focusedIndex >= 0)
                    {
                        focusedIndex += indexIncrement;
                        var itemCount = repeater.ItemsSourceView.Count;
                        while (focusedIndex >= 0 && focusedIndex < itemCount)
                        {
                            var item = repeater.TryGetElement(focusedIndex);
                            if (item != null)
                            {
                                if (item is Control itemAsControl)
                                {
                                    if (itemAsControl.IsEnabled && itemAsControl.IsTabStop && itemAsControl.Focusable)
                                    {
                                        itemAsControl.Focus();
                                        return true;
                                    }
                                }
                            }
                            focusedIndex += indexIncrement;
                        }
                    }
                }
            }
            return false;
        }

        private void UpdateItemsSource()
        {
            Select(-1);
            // TODO: m_itemsSourceChanged.revoke();
            var repeater = m_repeater;
            if (repeater != null)
            {
                var oldItemsSourceView = repeater.ItemsSourceView;
                if (oldItemsSourceView != null)
                {
                    oldItemsSourceView.CollectionChanged -= OnRepeaterCollectionChanged;
                }

                repeater.ItemsSource = GetItemsSource();

                var itemsSourceView = repeater.ItemsSourceView;
                if (itemsSourceView != null)
                {
                    itemsSourceView.CollectionChanged -= OnRepeaterCollectionChanged;
                }
            }
        }

        private object GetItemsSource()
        {
            var itemsSource = ItemsSource;
            if (itemsSource != null)
            {
                return itemsSource;
            }
            else
            {
                return Items;
            }
        }

        private void UpdateSelectedIndex()
        {
            if (!m_currentlySelecting)
            {
                Select(SelectedIndex);
            }
        }

        public UIElement ContainerFromIndex(int index)
        {
            var repeater = m_repeater;
            if (repeater != null)
            {
                return repeater.TryGetElement(index);
            }
            return null;
        }

        int m_selectedIndex = -1;
        bool m_currentlySelecting;

        ItemsRepeater m_repeater;

        const string s_repeaterName = "InnerRepeater";
    }
}
