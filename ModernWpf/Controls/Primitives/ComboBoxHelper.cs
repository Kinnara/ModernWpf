// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Threading;

namespace ModernWpf.Controls.Primitives
{
    public sealed class ComboBoxHelper
    {
        private const string c_popupBorderName = "PopupBorder";
        private const string c_editableTextName = "PART_EditableTextBox";
        //private const string c_editableTextBorderName = "BorderElement";
        private const string c_backgroundName = "Background";
        private const string c_highlightBackgroundName = "HighlightBackground";
        private const string c_toggleButtonName = "ToggleButton";
        private const string c_dropDownOverlayName = "DropDownOverlay";
        //private const string c_controlCornerRadiusKey = "ControlCornerRadius";
        private const string c_overlayCornerRadiusKey = "OverlayCornerRadius";

        internal ComboBoxHelper()
        {
        }

        /// <summary>
        /// Identifies the TextBoxStyle dependency property.
        /// </summary>
        public static readonly DependencyProperty TextBoxStyleProperty =
            DependencyProperty.RegisterAttached(
                "TextBoxStyle",
                typeof(Style),
                typeof(ComboBoxHelper),
                null);

        /// <summary>
        /// Gets the style of the TextBox in the ComboBox when the ComboBox is editable.
        /// </summary>
        /// <param name="comboBox">The element from which to read the property value.</param>
        /// <returns>The style of the TextBox in the ComboBox when the ComboBox is editable.</returns>
        public static Style GetTextBoxStyle(ComboBox comboBox)
        {
            return (Style)comboBox.GetValue(TextBoxStyleProperty);
        }

        /// <summary>
        /// Sets the style of the TextBox in the ComboBox when the ComboBox is editable.
        /// </summary>
        /// <param name="comboBox">The element on which to set the attached property.</param>
        /// <param name="value">The property value to set.</param>
        public static void SetTextBoxStyle(ComboBox comboBox, Style value)
        {
            comboBox.SetValue(TextBoxStyleProperty, value);
        }

        public static readonly DependencyProperty KeepInteriorCornersSquareProperty =
            DependencyProperty.RegisterAttached(
                "KeepInteriorCornersSquare",
                typeof(bool),
                typeof(ComboBoxHelper),
                new PropertyMetadata(false, OnKeepInteriorCornersSquareChanged));

        public static bool GetKeepInteriorCornersSquare(ComboBox comboBox)
        {
            return (bool)comboBox.GetValue(KeepInteriorCornersSquareProperty);
        }

        public static void SetKeepInteriorCornersSquare(ComboBox comboBox, bool value)
        {
            comboBox.SetValue(KeepInteriorCornersSquareProperty, value);
        }

        private static void OnKeepInteriorCornersSquareChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            if (sender is ComboBox comboBox)
            {
                bool shouldMonitorDropDownState = (bool)args.NewValue;
                if (shouldMonitorDropDownState)
                {
                    comboBox.DropDownOpened += OnDropDownOpened;
                    comboBox.DropDownClosed += OnDropDownClosed;
                }
                else
                {
                    comboBox.DropDownOpened -= OnDropDownOpened;
                    comboBox.DropDownClosed -= OnDropDownClosed;
                }
            }
        }

        public static readonly DependencyProperty VisualStateSettersEnabledProperty =
            DependencyProperty.RegisterAttached(
                "VisualStateSettersEnabled",
                typeof(bool),
                typeof(ComboBoxHelper),
                new PropertyMetadata(false, OnVisualStateSettersEnabledChanged));

        public static bool GetVisualStateSettersEnabled(ComboBox comboBox)
        {
            return (bool)comboBox.GetValue(VisualStateSettersEnabledProperty);
        }

        public static void SetVisualStateSettersEnabled(ComboBox comboBox, bool value)
        {
            comboBox.SetValue(VisualStateSettersEnabledProperty, value);
        }

        private static void OnVisualStateSettersEnabledChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            if (sender is ComboBox comboBox)
            {
                if ((bool)args.NewValue)
                {
                    GetOrCreateVisualStateTracker(comboBox).Attach();
                }
                else
                {
                    GetVisualStateTracker(comboBox)?.Detach();
                }
            }
        }

        private static void OnDropDownOpened(object sender, object args)
        {
            var comboBox = (ComboBox)sender;
            // We need to know whether the dropDown opens above or below the ComboBox in order to update corner radius correctly.
            // Sometimes TransformToPoint value is incorrect because popup is not fully opened when this function gets called.
            // Use dispatcher to make sure we get correct VerticalOffset.
            comboBox.Dispatcher.BeginInvoke(() =>
                {
                    UpdateCornerRadius(comboBox, /*IsDropDownOpen=*/true);
                });
        }

        private static void OnDropDownClosed(object sender, object args)
        {
            var comboBox = (ComboBox)sender;
            UpdateCornerRadius(comboBox, /*IsDropDownOpen=*/false);
        }

        private static void UpdateCornerRadius(ComboBox comboBox, bool isDropDownOpen)
        {
            var textBoxRadius = ControlHelper.GetCornerRadius(comboBox);
            var popupRadius = (CornerRadius)ResourceLookup(comboBox, c_overlayCornerRadiusKey);

            if (isDropDownOpen)
            {
                bool isOpenDown = IsPopupOpenDown(comboBox);
                var cornerRadiusConverter = new CornerRadiusFilterConverter();

                var popupRadiusFilter = isOpenDown ? CornerRadiusFilterKind.Bottom : CornerRadiusFilterKind.Top;
                popupRadius = cornerRadiusConverter.Convert(popupRadius, popupRadiusFilter);

                var textBoxRadiusFilter = isOpenDown ? CornerRadiusFilterKind.Top : CornerRadiusFilterKind.Bottom;
                textBoxRadius = cornerRadiusConverter.Convert(textBoxRadius, textBoxRadiusFilter);
            }

            if (GetTemplateChild<Border>(c_popupBorderName, comboBox) is Border popupBorder)
            {
                popupBorder.CornerRadius = popupRadius;
            }

            if (comboBox.IsEditable)
            {
                if (GetTemplateChild<TextBox>(c_editableTextName, comboBox) is TextBox textBox)
                {
                    ControlHelper.SetCornerRadius(textBox, textBoxRadius);
                }
            }
            else
            {
                if (GetTemplateChild<Border>(c_backgroundName, comboBox) is Border background)
                {
                    background.CornerRadius = textBoxRadius;
                }

                if (GetTemplateChild<Border>(c_highlightBackgroundName, comboBox) is Border highlightBackground)
                {
                    highlightBackground.CornerRadius = textBoxRadius;
                }
            }
        }

        private static bool IsPopupOpenDown(ComboBox comboBox)
        {
            double verticalOffset = 0;
            if (GetTemplateChild<Border>(c_popupBorderName, comboBox) is Border popupBorder)
            {
                if (GetTemplateChild<TextBox>(c_editableTextName, comboBox) is TextBox textBox)
                {
                    var popupTop = popupBorder.TranslatePoint(new Point(0,0), textBox);
                    verticalOffset = popupTop.Y;
                }
            }
            return verticalOffset > 0;
        }

        private static object ResourceLookup(Control control, object key)
        {
            return control.TryFindResource(key);
        }

        private static T GetTemplateChild<T>(string childName, Control control) where T : DependencyObject
        {
            return control.Template?.FindName(childName, control) as T;
        }

        private static readonly DependencyProperty VisualStateTrackerProperty =
            DependencyProperty.RegisterAttached(
                "VisualStateTracker",
                typeof(ComboBoxVisualStateTracker),
                typeof(ComboBoxHelper),
                new PropertyMetadata(null));

        private static ComboBoxVisualStateTracker GetOrCreateVisualStateTracker(ComboBox comboBox)
        {
            var tracker = GetVisualStateTracker(comboBox);
            if (tracker == null)
            {
                tracker = new ComboBoxVisualStateTracker(comboBox);
                comboBox.SetValue(VisualStateTrackerProperty, tracker);
            }

            return tracker;
        }

        private static ComboBoxVisualStateTracker GetVisualStateTracker(ComboBox comboBox)
        {
            return (ComboBoxVisualStateTracker)comboBox.GetValue(VisualStateTrackerProperty);
        }

        private sealed class ComboBoxVisualStateTracker
        {
            private static readonly DependencyPropertyDescriptor IsEditablePropertyDescriptor =
                DependencyPropertyDescriptor.FromProperty(ComboBox.IsEditableProperty, typeof(ComboBox));

            private static readonly DependencyPropertyDescriptor IsSelectionActivePropertyDescriptor =
                DependencyPropertyDescriptor.FromProperty(TextBoxBase.IsSelectionActiveProperty, typeof(TextBox));

            private static readonly DependencyPropertyDescriptor ToggleButtonIsPressedPropertyDescriptor =
                DependencyPropertyDescriptor.FromProperty(ButtonBase.IsPressedProperty, typeof(ToggleButton));

            public ComboBoxVisualStateTracker(ComboBox comboBox)
            {
                _comboBox = comboBox;
            }

            public void Attach()
            {
                if (_isAttached)
                {
                    return;
                }

                _isAttached = true;
                _comboBox.Loaded += OnLoaded;
                _comboBox.Unloaded += OnUnloaded;
                _comboBox.IsEnabledChanged += OnComboBoxStateChanged;
                _comboBox.MouseEnter += OnInputStateChanged;
                _comboBox.MouseLeave += OnInputStateChanged;
                _comboBox.DropDownOpened += OnComboBoxDropDownChanged;
                _comboBox.DropDownClosed += OnComboBoxDropDownChanged;
                IsEditablePropertyDescriptor.AddValueChanged(_comboBox, OnDependencyStateChanged);

                AttachTemplateParts();
                UpdateVisualStates(false);
            }

            public void Detach()
            {
                if (!_isAttached)
                {
                    return;
                }

                DetachTemplateParts();
                IsEditablePropertyDescriptor.RemoveValueChanged(_comboBox, OnDependencyStateChanged);
                _comboBox.DropDownClosed -= OnComboBoxDropDownChanged;
                _comboBox.DropDownOpened -= OnComboBoxDropDownChanged;
                _comboBox.MouseLeave -= OnInputStateChanged;
                _comboBox.MouseEnter -= OnInputStateChanged;
                _comboBox.IsEnabledChanged -= OnComboBoxStateChanged;
                _comboBox.Unloaded -= OnUnloaded;
                _comboBox.Loaded -= OnLoaded;
                _isAttached = false;
            }

            private void OnLoaded(object sender, RoutedEventArgs e)
            {
                AttachTemplateParts();
                UpdateVisualStates(false);
            }

            private void OnUnloaded(object sender, RoutedEventArgs e)
            {
                DetachTemplateParts();
            }

            private void AttachTemplateParts()
            {
                DetachTemplateParts();

                _editableTextBox = GetTemplateChild<TextBox>(c_editableTextName, _comboBox);
                _toggleButton = GetTemplateChild<ToggleButton>(c_toggleButtonName, _comboBox);
                _dropDownOverlay = GetTemplateChild<ToggleButton>(c_dropDownOverlayName, _comboBox);

                if (_editableTextBox != null)
                {
                    _editableTextBox.GotKeyboardFocus += OnInputStateChanged;
                    _editableTextBox.LostKeyboardFocus += OnInputStateChanged;
                    _editableTextBox.MouseEnter += OnInputStateChanged;
                    _editableTextBox.MouseLeave += OnInputStateChanged;
                    IsSelectionActivePropertyDescriptor.AddValueChanged(_editableTextBox, OnDependencyStateChanged);
                }

                if (_toggleButton != null)
                {
                    _toggleButton.MouseEnter += OnInputStateChanged;
                    _toggleButton.MouseLeave += OnInputStateChanged;
                    _toggleButton.PreviewMouseDown += OnInputButtonStateChanged;
                    _toggleButton.PreviewMouseUp += OnInputButtonStateChanged;
                    _toggleButton.LostMouseCapture += OnInputStateChanged;
                    ToggleButtonIsPressedPropertyDescriptor.AddValueChanged(_toggleButton, OnDependencyStateChanged);
                }

                if (_dropDownOverlay != null)
                {
                    _dropDownOverlay.MouseEnter += OnInputStateChanged;
                    _dropDownOverlay.MouseLeave += OnInputStateChanged;
                    _dropDownOverlay.PreviewMouseDown += OnInputButtonStateChanged;
                    _dropDownOverlay.PreviewMouseUp += OnInputButtonStateChanged;
                    _dropDownOverlay.LostMouseCapture += OnInputStateChanged;
                    ToggleButtonIsPressedPropertyDescriptor.AddValueChanged(_dropDownOverlay, OnDependencyStateChanged);
                }
            }

            private void DetachTemplateParts()
            {
                if (_editableTextBox != null)
                {
                    IsSelectionActivePropertyDescriptor.RemoveValueChanged(_editableTextBox, OnDependencyStateChanged);
                    _editableTextBox.MouseLeave -= OnInputStateChanged;
                    _editableTextBox.MouseEnter -= OnInputStateChanged;
                    _editableTextBox.LostKeyboardFocus -= OnInputStateChanged;
                    _editableTextBox.GotKeyboardFocus -= OnInputStateChanged;
                    _editableTextBox = null;
                }

                if (_toggleButton != null)
                {
                    ToggleButtonIsPressedPropertyDescriptor.RemoveValueChanged(_toggleButton, OnDependencyStateChanged);
                    _toggleButton.LostMouseCapture -= OnInputStateChanged;
                    _toggleButton.PreviewMouseUp -= OnInputButtonStateChanged;
                    _toggleButton.PreviewMouseDown -= OnInputButtonStateChanged;
                    _toggleButton.MouseLeave -= OnInputStateChanged;
                    _toggleButton.MouseEnter -= OnInputStateChanged;
                    _toggleButton = null;
                }

                if (_dropDownOverlay != null)
                {
                    ToggleButtonIsPressedPropertyDescriptor.RemoveValueChanged(_dropDownOverlay, OnDependencyStateChanged);
                    _dropDownOverlay.LostMouseCapture -= OnInputStateChanged;
                    _dropDownOverlay.PreviewMouseUp -= OnInputButtonStateChanged;
                    _dropDownOverlay.PreviewMouseDown -= OnInputButtonStateChanged;
                    _dropDownOverlay.MouseLeave -= OnInputStateChanged;
                    _dropDownOverlay.MouseEnter -= OnInputStateChanged;
                    _dropDownOverlay = null;
                }
            }

            private void OnComboBoxStateChanged(object sender, DependencyPropertyChangedEventArgs e)
            {
                ScheduleVisualStateUpdate();
            }

            private void OnComboBoxDropDownChanged(object sender, EventArgs e)
            {
                ScheduleVisualStateUpdate();
            }

            private void OnDependencyStateChanged(object sender, EventArgs e)
            {
                ScheduleVisualStateUpdate();
            }

            private void OnInputStateChanged(object sender, MouseEventArgs e)
            {
                ScheduleVisualStateUpdate();
            }

            private void OnInputStateChanged(object sender, KeyboardFocusChangedEventArgs e)
            {
                ScheduleVisualStateUpdate();
            }

            private void OnInputButtonStateChanged(object sender, MouseButtonEventArgs e)
            {
                ScheduleVisualStateUpdate();
            }

            private void ScheduleVisualStateUpdate()
            {
                UpdateVisualStates(true);
                _comboBox.Dispatcher.BeginInvoke(
                    (Action)(() => UpdateVisualStates(true)),
                    DispatcherPriority.Input);
            }

            private void UpdateVisualStates(bool useTransitions)
            {
                VisualStateManager.GoToState(_comboBox, GetCommonStateName(), useTransitions);
                VisualStateManager.GoToState(_comboBox, GetEditableModeStateName(), useTransitions);
            }

            private string GetCommonStateName()
            {
                if (!_comboBox.IsEnabled)
                {
                    return "Disabled";
                }

                if (_toggleButton?.IsPressed == true ||
                    _dropDownOverlay?.IsPressed == true)
                {
                    return "Pressed";
                }

                if (_comboBox.IsMouseOver ||
                    _toggleButton?.IsMouseOver == true ||
                    _dropDownOverlay?.IsMouseOver == true)
                {
                    return "PointerOver";
                }

                return "Normal";
            }

            private string GetEditableModeStateName()
            {
                if (!_comboBox.IsEditable)
                {
                    return "TextBoxUnfocused";
                }

                bool isTextBoxFocused = _editableTextBox?.IsSelectionActive == true;
                bool isOverlayPressed = _dropDownOverlay?.IsPressed == true;
                bool isOverlayPointerOver = _dropDownOverlay?.IsMouseOver == true;

                if (isTextBoxFocused)
                {
                    if (isOverlayPressed)
                    {
                        return "TextBoxFocusedOverlayPressed";
                    }

                    if (isOverlayPointerOver)
                    {
                        return "TextBoxFocusedOverlayPointerOver";
                    }

                    return "TextBoxFocused";
                }

                if (isOverlayPressed)
                {
                    return "TextBoxOverlayPressed";
                }

                if (isOverlayPointerOver)
                {
                    return "TextBoxOverlayPointerOver";
                }

                return "TextBoxUnfocused";
            }

            private readonly ComboBox _comboBox;
            private bool _isAttached;
            private TextBox _editableTextBox;
            private ToggleButton _toggleButton;
            private ToggleButton _dropDownOverlay;
        }
    }
}
