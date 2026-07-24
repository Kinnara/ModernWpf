// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using ModernWpf.Automation.Peers;
using ModernWpf.Controls.Primitives;

namespace ModernWpf.Controls
{
    [ContentProperty(nameof(Items))]
    [TemplatePart(Name = s_repeaterName, Type = typeof(ItemsRepeater))]
    public partial class RadioButtons : Control
    {
        static RadioButtons()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(RadioButtons),
                new FrameworkPropertyMetadata(typeof(RadioButtons)));
        }

        public RadioButtons()
        {
            var items = new ObservableCollection<object>();
            SetValue(ItemsProperty, items);

            // Override normal up/down/left/right behavior -- down should always go to the next item and up to the previous.
            // left and right should be spacial but contained to the RadioButtons control. We have to attach to PreviewKeyDown
            // because RadioButton has a key down handler for up and down that gets called before we can intercept. Issue #1634.
            PreviewKeyDown += OnChildPreviewKeyDown;

            m_radioButtonsElementFactory = new RadioButtonsElementFactory();

            IsEnabledChanged += OnIsEnabledChanged;
        }

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

        protected override AutomationPeer OnCreateAutomationPeer()
        {
            return new RadioButtonsAutomationPeer(this);
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
            }

            m_repeater = GetTemplateChild(s_repeaterName) as ItemsRepeater;

            if (m_repeater != null)
            {
                m_repeater.ItemTemplate = m_radioButtonsElementFactory;

                m_repeater.ElementPrepared += OnRepeaterElementPrepared;
                m_repeater.ElementClearing += OnRepeaterElementClearing;
                m_repeater.ElementIndexChanged += OnRepeaterElementIndexChanged;
                m_repeater.Loaded += OnRepeaterLoaded;
            }

            UpdateItemsSource();
            UpdateVisualStateForIsEnabledChange();
        }

        private static bool ValidateMaxColumns(object value)
        {
            return (int)value > 0;
        }

        // When focus comes from outside the RadioButtons control we will put focus on the selected radio button.
        protected override void OnPreviewGotKeyboardFocus(KeyboardFocusChangedEventArgs args)
        {
            if (!m_currentlySettingFocus)
            {
                var repeater = m_repeater;
                if (repeater != null)
                {
                    var oldFocusedElement = args.OldFocus;
                    if (oldFocusedElement != null)
                    {
                        if (oldFocusedElement is UIElement oldFocusedElementAsUIElement)
                        {
                            var oldElementParent = VisualTreeHelper.GetParent(oldFocusedElementAsUIElement);
                            // If focus is coming from outside the repeater, put focus on the selected item.
                            if (repeater != oldElementParent)
                            {
                                var selectedItem = repeater.TryGetElement(m_selectedIndex);
                                if (selectedItem != null)
                                {
                                    try
                                    {
                                        m_currentlySettingFocus = true;

                                        if (selectedItem.Focus())
                                        {
                                            args.Handled = true;
                                        }
                                    }
                                    finally
                                    {
                                        m_currentlySettingFocus = false;
                                    }
                                }
                            }

                            // On RS3+ Selection follows focus unless control is held down.
                            else if ((RadioButtonsTestHooks.GetKeyboardModifiers(args.KeyboardDevice) & ModifierKeys.Control) != ModifierKeys.Control)
                            {
                                if (args.NewFocus is UIElement newFocusedElementAsUIE)
                                {
                                    Select(repeater.GetElementIndex(newFocusedElementAsUIE));
                                }
                            }
                        }
                    }
                }
            }

            base.OnPreviewGotKeyboardFocus(args);
        }

        void OnRepeaterLoaded(object sender, RoutedEventArgs args)
        {
            var repeater = m_repeater;
            if (repeater != null)
            {
                if (m_testHooksEnabled)
                {
                    AttachToLayoutChanged();
                }

                m_blockSelecting = false;
                if (SelectedIndex == -1 && SelectedItem != null)
                {
                    UpdateSelectedItem();
                }
                else
                {
                    UpdateSelectedIndex();
                }

                OnRepeaterCollectionChanged(null, null);
            }
        }

        void OnChildPreviewKeyDown(object sender, KeyEventArgs args)
        {
            var modifiers = RadioButtonsTestHooks.GetKeyboardModifiers(args.KeyboardDevice);

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
                        if (MoveFocusHorizontally(1, modifiers))
                        {
                            args.Handled = true;
                            return;
                        }

                        if (args.OriginalSource is UIElement)
                        {
                            args.Handled = HandleEdgeCaseFocus(false, args.OriginalSource);
                        }
                    }
                    break;
                case Key.Left:
                    {
                        if (MoveFocusHorizontally(-1, modifiers))
                        {
                            args.Handled = true;
                            return;
                        }

                        if (args.OriginalSource is UIElement)
                        {
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

                    // If the developer adds a checked toggle button to the collection, update selection to this item.
                    if (toggleButton.IsChecked == true)
                    {
                        m_blockSelecting = false; // WPF-specific fix to ensure IsChecked is honored
                        Select(args.Index);
                    }
                }
#if NET48_OR_NEWER
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

                // If the removed element was the selected one, update selection to -1
                if (element is ToggleButton elementAsToggle)
                {
                    if (elementAsToggle.IsChecked == true)
                    {
                        Select(-1);
                    }
                }
            }
        }

        void OnRepeaterElementIndexChanged(ItemsRepeater sender, ItemsRepeaterElementIndexChangedEventArgs args)
        {
            var element = args.Element;
            if (element != null)
            {
#if NET48_OR_NEWER
                element.SetValue(AutomationProperties.PositionInSetProperty, args.NewIndex + 1);
#endif
                // When the selected item's index changes, update selection to match
                if (element is ToggleButton elementAsToggle)
                {
                    if (elementAsToggle.IsChecked == true)
                    {
                        Select(args.NewIndex);
                    }
                }
            }
        }

        void OnRepeaterCollectionChanged(object sender, object args)
        {
#if NET48_OR_NEWER
            var repeater = m_repeater;
            if (repeater != null)
            {
                var itemSourceView = repeater.ItemsSourceView;
                if (itemSourceView != null)
                {
                    var count = itemSourceView.Count;
                    for (var index = 0; index < count; index++)
                    {
                        var element = repeater.TryGetElement(index);
                        if (element != null)
                        {
                            element.SetValue(AutomationProperties.SizeOfSetProperty, count);
                        }
                    }
                }
            }
#endif
        }

        void Select(int index)
        {
            if (!m_blockSelecting && !m_currentlySelecting && m_selectedIndex != index)
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

                    SetCurrentValue(SelectedIndexProperty, m_selectedIndex);
                    SetCurrentValue(SelectedItemProperty, newSelectedItem);

                    // A missing selection is represented by an empty collection,
                    // not by a collection containing a null item.
                    var removedItems = previousSelectedItem != null
                        ? new[] { previousSelectedItem }
                        : Array.Empty<object>();
                    var addedItems = newSelectedItem != null
                        ? new[] { newSelectedItem }
                        : Array.Empty<object>();
                    RaiseEvent(new SelectionChangedEventArgs(SelectionChangedEvent, removedItems, addedItems));
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
                        itemAsToggleButton.SetCurrentValue(ToggleButton.IsCheckedProperty, containerIsChecked);
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
                                    if (itemAsControl.Focus())
                                    {
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

        bool MoveFocusHorizontally(int direction, ModifierKeys modifiers)
        {
            var repeater = m_repeater;
            if (repeater != null &&
                Keyboard.FocusedElement is UIElement focusedElement)
            {
                var focusedIndex = repeater.GetElementIndex(focusedElement);
                if (focusedIndex < 0)
                {
                    return false;
                }

                var focusedCenter = GetElementCenter(focusedElement);
                Control bestControl = null;
                double bestVerticalDistance = double.PositiveInfinity;
                double bestHorizontalDistance = double.PositiveInfinity;

                var itemCount = repeater.ItemsSourceView.Count;
                for (int index = 0; index < itemCount; index++)
                {
                    if (index == focusedIndex)
                    {
                        continue;
                    }

                    if (repeater.TryGetElement(index) is not Control candidate ||
                        !CanFocusCandidate(candidate))
                    {
                        continue;
                    }

                    var candidateCenter = GetElementCenter(candidate);
                    var horizontalDelta = candidateCenter.X - focusedCenter.X;
                    if ((direction > 0 && horizontalDelta <= 0) ||
                        (direction < 0 && horizontalDelta >= 0))
                    {
                        continue;
                    }

                    var verticalDistance = Math.Abs(candidateCenter.Y - focusedCenter.Y);
                    var horizontalDistance = Math.Abs(horizontalDelta);

                    if (verticalDistance < bestVerticalDistance ||
                        (Math.Abs(verticalDistance - bestVerticalDistance) < 0.5 &&
                         horizontalDistance < bestHorizontalDistance))
                    {
                        bestControl = candidate;
                        bestVerticalDistance = verticalDistance;
                        bestHorizontalDistance = horizontalDistance;
                    }
                }

                if (bestControl != null)
                {
                    if (bestControl.Focus())
                    {
                        if ((modifiers & ModifierKeys.Control) != ModifierKeys.Control &&
                            bestControl is ToggleButton toggleButton)
                        {
                            toggleButton.SetCurrentValue(ToggleButton.IsCheckedProperty, true);
                        }

                        return true;
                    }
                }
            }

            return false;
        }

        private Point GetElementCenter(UIElement element)
        {
            var elementAsFrameworkElement = element as FrameworkElement;
            var width = elementAsFrameworkElement?.ActualWidth ?? 0;
            var height = elementAsFrameworkElement?.ActualHeight ?? 0;
            return element.TranslatePoint(new Point(width / 2, height / 2), this);
        }

        private bool CanFocusCandidate(Control control)
        {
            return control.IsEnabled &&
                control.IsVisible &&
                control.Focusable;
        }

        private void OnIsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            UpdateVisualStateForIsEnabledChange();
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

        private void UpdateItemsSource()
        {
            Select(-1);
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
                    itemsSourceView.CollectionChanged += OnRepeaterCollectionChanged;
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

        private void UpdateSelectedItem()
        {
            if (!m_currentlySelecting)
            {
                var repeater = m_repeater;
                if (repeater != null)
                {
                    var itemsSourceView = repeater.ItemsSourceView;
                    if (itemsSourceView != null)
                    {
                        Select(itemsSourceView.IndexOf(SelectedItem));
                    }
                }
            }
        }

        private void UpdateItemTemplate()
        {
            m_radioButtonsElementFactory.UserElementFactory(ItemTemplate);
        }

        private void UpdateVisualStateForIsEnabledChange()
        {
            VisualStateManager.GoToState(this, IsEnabled ? "Normal" : "Disabled", false);
        }

        // Test Hooks helpers, only function when m_testHooksEnabled == true
        internal void SetTestHooksEnabled(bool enabled)
        {
            if (m_testHooksEnabled != enabled)
            {
                m_testHooksEnabled = enabled;
                if (enabled)
                {
                    AttachToLayoutChanged();
                }
                else
                {
                    DetatchFromLayoutChanged();
                }
            }
        }

        void OnLayoutChanged(ColumnMajorUniformToLargestGridLayout sender, object args)
        {
            RadioButtonsTestHooks.NotifyLayoutChanged(this);
        }

        internal int GetRows()
        {
            var layout = GetLayout();
            if (layout != null)
            {
                return layout.GetRows();
            }
            return -1;
        }

        internal int GetColumns()
        {
            var layout = GetLayout();
            if (layout != null)
            {
                return layout.GetColumns();
            }
            return -1;
        }

        internal int GetLargerColumns()
        {
            var layout = GetLayout();
            if (layout != null)
            {
                return layout.GetLargerColumns();
            }
            return -1;
        }

        void AttachToLayoutChanged()
        {
            var layout = GetLayout();
            if (layout != null)
            {
                layout.SetTestHooksEnabled(true);
                layout.LayoutChanged += OnLayoutChanged;
            }
        }

        void DetatchFromLayoutChanged()
        {
            var layout = GetLayout();
            if (layout != null)
            {
                layout.SetTestHooksEnabled(false);
                layout.LayoutChanged -= OnLayoutChanged;
            }
        }

        ColumnMajorUniformToLargestGridLayout GetLayout()
        {
            if (m_repeater != null)
            {
                if (m_repeater.Layout is ColumnMajorUniformToLargestGridLayout customLayout)
                {
                    return customLayout;
                }
            }
            return null;
        }

        int m_selectedIndex = -1;
        // This is used to guard against reentrency when calling select, since select changes
        // the Selected Index/Item which in turn calls select.
        bool m_currentlySelecting = false;
        // We block selection before the control has loaded.
        // This is to ensure that we do not overwrite a provided Selected Index/Item value.
        bool m_blockSelecting = true;

        bool m_currentlySettingFocus = false;

        ItemsRepeater m_repeater;

        RadioButtonsElementFactory m_radioButtonsElementFactory;

        //Test hooks helpers, only function while m_testHooksEnabled == true
        bool m_testHooksEnabled = false;

        const string s_repeaterName = "InnerRepeater";
    }
}
