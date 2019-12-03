using System;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using ModernWpf.Automation.Peers;
using ModernWpf.Controls.Primitives;

namespace ModernWpf.Controls
{
    public class NumberBoxValueChangedEventArgs
    {
        public NumberBoxValueChangedEventArgs(double oldValue, double newValue)
        {
            OldValue = oldValue;
            NewValue = newValue;
        }

        public double OldValue { get; }
        public double NewValue { get; }
    }

    public class NumberBox : Control
    {
        const string c_numberBoxDownButtonName = "DownSpinButton";
        const string c_numberBoxUpButtonName = "UpSpinButton";
        const string c_numberBoxTextBoxName = "InputBox";
        const string c_numberBoxPopupButtonName = "PopupButton";
        const string c_numberBoxPopupName = "UpDownPopup";
        const string c_numberBoxPopupDownButtonName = "PopupDownSpinButton";
        const string c_numberBoxPopupUpButtonName = "PopupUpSpinButton";
        const string c_numberBoxPopupContentRootName = "PopupContentRoot";

        const double c_popupShadowDepth = 16.0;
        const string c_numberBoxPopupShadowDepthName = "NumberBoxPopupShadowDepth";

        static NumberBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(NumberBox), new FrameworkPropertyMetadata(typeof(NumberBox)));
        }

        public NumberBox()
        {
            MouseWheel += OnNumberBoxScroll;

            GotFocus += OnNumberBoxGotFocus;
            LostFocus += OnNumberBoxLostFocus;
        }

        protected override AutomationPeer OnCreateAutomationPeer()
        {
            return new NumberBoxAutomationPeer(this);
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            var spinDownName = "Decrease";
            var spinUpName = "Increase";

            if (GetTemplateChild(c_numberBoxDownButtonName) is RepeatButton spinDown)
            {
                spinDown.Click += OnSpinDownClick;

                // Do localization for the down button
                if (string.IsNullOrEmpty(AutomationProperties.GetName(spinDown)))
                {
                    AutomationProperties.SetName(spinDown, spinDownName);
                }
            }

            if (GetTemplateChild(c_numberBoxUpButtonName) is RepeatButton spinUp)
            {
                spinUp.Click += OnSpinUpClick;

                // Do localization for the up button
                if (string.IsNullOrEmpty(AutomationProperties.GetName(spinUp)))
                {
                    AutomationProperties.SetName(spinUp, spinUpName);
                }
            }

            if (GetTemplateChild(c_numberBoxTextBoxName) is TextBox textBox)
            {
                textBox.KeyDown += OnNumberBoxKeyDown;
                textBox.KeyUp += OnNumberBoxKeyUp;
                m_textBox = textBox;
            }

            m_popup = GetTemplateChild(c_numberBoxPopupName) as Popup;
            if (m_popup != null)
            {
                m_popup.Opened += OnPopupOpened;
                m_popup.Closed += OnPopupClosed;
            }

            if (GetTemplateChild(c_numberBoxPopupDownButtonName) is RepeatButton popupSpinDown)
            {
                popupSpinDown.Click += OnSpinDownClick;
            }

            if (GetTemplateChild(c_numberBoxPopupUpButtonName) is RepeatButton popupSpinUp)
            {
                popupSpinUp.Click += OnSpinUpClick;
            }

            // .NET rounds to 12 significant digits when displaying doubles, so we will do the same.
            //m_displayRounder.SignificantDigits(12);

            UpdateSpinButtonPlacement();
            UpdateSpinButtonEnabled();

            if (ReadLocalValue(ValueProperty) == DependencyProperty.UnsetValue
                && ReadLocalValue(TextProperty) != DependencyProperty.UnsetValue)
            {
                // If Text has been set, but Value hasn't, update Value based on Text.
                UpdateValueToText();
            }
            else
            {
                UpdateTextToValue();
            }
        }

        #region Minimum

        public static readonly DependencyProperty MinimumProperty =
            DependencyProperty.Register(
                nameof(Minimum),
                typeof(double),
                typeof(NumberBox),
                new PropertyMetadata(double.MinValue, OnMinimumPropertyChanged));

        public double Minimum
        {
            get => (double)GetValue(MinimumProperty);
            set => SetValue(MinimumProperty, value);
        }

        private static void OnMinimumPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            ((NumberBox)sender).OnMinimumPropertyChanged(args);
        }

        private void OnMinimumPropertyChanged(DependencyPropertyChangedEventArgs args)
        {
            CoerceMaximum();
            CoerceValue();

            UpdateSpinButtonEnabled();
        }

        #endregion

        #region Maximum

        public static readonly DependencyProperty MaximumProperty =
            DependencyProperty.Register(
                nameof(Maximum),
                typeof(double),
                typeof(NumberBox),
                new PropertyMetadata(double.MaxValue, OnMaximumPropertyChanged));

        public double Maximum
        {
            get => (double)GetValue(MaximumProperty);
            set => SetValue(MaximumProperty, value);
        }

        private static void OnMaximumPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            ((NumberBox)sender).OnMaximumPropertyChanged(args);
        }

        private void OnMaximumPropertyChanged(DependencyPropertyChangedEventArgs args)
        {
            CoerceMinimum();
            CoerceValue();

            UpdateSpinButtonEnabled();
        }

        #endregion

        #region Value

        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register(
                nameof(Value),
                typeof(double),
                typeof(NumberBox),
                new PropertyMetadata(0d, OnValuePropertyChanged));

        public double Value
        {
            get => (double)GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
        }

        private static void OnValuePropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            ((NumberBox)sender).OnValuePropertyChanged(args);
        }

        private void OnValuePropertyChanged(DependencyPropertyChangedEventArgs args)
        {
            // This handler may change Value; don't send extra events in that case.
            if (!m_valueUpdating)
            {
                var oldValue = (double)args.OldValue;

                try
                {
                    m_valueUpdating = true;

                    CoerceValue();

                    var newValue = Value;
                    if (newValue != oldValue && !(double.IsNaN(newValue) && double.IsNaN(oldValue)))
                    {
                        // Fire ValueChanged event
                        var valueChangedArgs = new NumberBoxValueChangedEventArgs(oldValue, newValue);
                        ValueChanged?.Invoke(this, valueChangedArgs);

                        // Fire value property change for UIA
                        if (FrameworkElementAutomationPeer.FromElement(this) is NumberBoxAutomationPeer peer)
                        {
                            peer.RaiseValueChangedEvent(oldValue, newValue);
                        }
                    }

                    UpdateTextToValue();
                    UpdateSpinButtonEnabled();
                }
                finally
                {
                    m_valueUpdating = false;
                }
            }
        }

        #endregion

        #region SmallChange

        public static readonly DependencyProperty SmallChangeProperty =
            DependencyProperty.Register(
                nameof(SmallChange),
                typeof(double),
                typeof(NumberBox),
                new PropertyMetadata(1d, OnSmallChangePropertyChanged));

        public double SmallChange
        {
            get => (double)GetValue(SmallChangeProperty);
            set => SetValue(SmallChangeProperty, value);
        }

        private static void OnSmallChangePropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            ((NumberBox)sender).OnSmallChangePropertyChanged(args);
        }

        private void OnSmallChangePropertyChanged(DependencyPropertyChangedEventArgs args)
        {
            UpdateSpinButtonEnabled();
        }

        #endregion

        #region LargeChange

        public static readonly DependencyProperty LargeChangeProperty =
            DependencyProperty.Register(
                nameof(LargeChange),
                typeof(double),
                typeof(NumberBox),
                new PropertyMetadata(10d, OnLargeChangePropertyChanged));

        public double LargeChange
        {
            get => (double)GetValue(LargeChangeProperty);
            set => SetValue(LargeChangeProperty, value);
        }

        private static void OnLargeChangePropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            ((NumberBox)sender).OnLargeChangePropertyChanged(args);
        }

        private void OnLargeChangePropertyChanged(DependencyPropertyChangedEventArgs args)
        {

        }

        #endregion

        #region Text

        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register(
                nameof(Text),
                typeof(string),
                typeof(NumberBox),
                new PropertyMetadata(string.Empty, OnTextPropertyChanged));

        public string Text
        {
            get => (string)GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        private static void OnTextPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            ((NumberBox)sender).OnTextPropertyChanged(args);
        }

        private void OnTextPropertyChanged(DependencyPropertyChangedEventArgs args)
        {
            if (!m_textUpdating)
            {
                UpdateValueToText();
            }
        }

        #endregion

        #region Header

        public static readonly DependencyProperty HeaderProperty =
            ControlHelper.HeaderProperty.AddOwner(typeof(NumberBox));

        public object Header
        {
            get => GetValue(HeaderProperty);
            set => SetValue(HeaderProperty, value);
        }

        #endregion

        #region HeaderTemplate

        public static readonly DependencyProperty HeaderTemplateProperty =
            ControlHelper.HeaderTemplateProperty.AddOwner(typeof(NumberBox));

        public DataTemplate HeaderTemplate
        {
            get => (DataTemplate)GetValue(HeaderTemplateProperty);
            set => SetValue(HeaderTemplateProperty, value);
        }

        #endregion

        #region PlaceholderText

        public static readonly DependencyProperty PlaceholderTextProperty =
            ControlHelper.PlaceholderTextProperty.AddOwner(typeof(NumberBox));

        public string PlaceholderText
        {
            get => (string)GetValue(PlaceholderTextProperty);
            set => SetValue(PlaceholderTextProperty, value);
        }

        #endregion

        #region Description

        public static readonly DependencyProperty DescriptionProperty =
            ControlHelper.DescriptionProperty.AddOwner(typeof(NumberBox));

        public object Description
        {
            get => GetValue(DescriptionProperty);
            set => SetValue(DescriptionProperty, value);
        }

        #endregion

        #region ValidationMode

        public static readonly DependencyProperty ValidationModeProperty =
            DependencyProperty.Register(
                nameof(ValidationMode),
                typeof(NumberBoxValidationMode),
                typeof(NumberBox),
                new PropertyMetadata(OnValidationModePropertyChanged));

        public NumberBoxValidationMode ValidationMode
        {
            get => (NumberBoxValidationMode)GetValue(ValidationModeProperty);
            set => SetValue(ValidationModeProperty, value);
        }

        private static void OnValidationModePropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            ((NumberBox)sender).OnValidationModePropertyChanged(args);
        }

        private void OnValidationModePropertyChanged(DependencyPropertyChangedEventArgs args)
        {
            ValidateInput();
            UpdateSpinButtonEnabled();
        }

        #endregion

        #region SpinButtonPlacementMode

        public static readonly DependencyProperty SpinButtonPlacementModeProperty =
            DependencyProperty.Register(
                nameof(SpinButtonPlacementMode),
                typeof(NumberBoxSpinButtonPlacementMode),
                typeof(NumberBox),
                new PropertyMetadata(NumberBoxSpinButtonPlacementMode.Hidden, OnSpinButtonPlacementModePropertyChanged));

        public NumberBoxSpinButtonPlacementMode SpinButtonPlacementMode
        {
            get => (NumberBoxSpinButtonPlacementMode)GetValue(SpinButtonPlacementModeProperty);
            set => SetValue(SpinButtonPlacementModeProperty, value);
        }

        private static void OnSpinButtonPlacementModePropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            ((NumberBox)sender).OnSpinButtonPlacementModePropertyChanged(args);
        }

        private void OnSpinButtonPlacementModePropertyChanged(DependencyPropertyChangedEventArgs args)
        {
            UpdateSpinButtonPlacement();
        }

        #endregion

        #region IsWrapEnabled

        public static readonly DependencyProperty IsWrapEnabledProperty =
            DependencyProperty.Register(
                nameof(IsWrapEnabled),
                typeof(bool),
                typeof(NumberBox),
                new PropertyMetadata(false, OnIsWrapEnabledPropertyChanged));

        public bool IsWrapEnabled
        {
            get => (bool)GetValue(IsWrapEnabledProperty);
            set => SetValue(IsWrapEnabledProperty, value);
        }

        private static void OnIsWrapEnabledPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            ((NumberBox)sender).OnIsWrapEnabledPropertyChanged(args);
        }

        private void OnIsWrapEnabledPropertyChanged(DependencyPropertyChangedEventArgs args)
        {
            UpdateSpinButtonEnabled();
        }

        #endregion

        #region AcceptsExpression

        public static readonly DependencyProperty AcceptsExpressionProperty =
            DependencyProperty.Register(
                nameof(AcceptsExpression),
                typeof(bool),
                typeof(NumberBox),
                new PropertyMetadata(false, OnAcceptsExpressionPropertyChanged));

        public bool AcceptsExpression
        {
            get => (bool)GetValue(AcceptsExpressionProperty);
            set => SetValue(AcceptsExpressionProperty, value);
        }

        private static void OnAcceptsExpressionPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            ((NumberBox)sender).OnAcceptsExpressionPropertyChanged(args);
        }

        private void OnAcceptsExpressionPropertyChanged(DependencyPropertyChangedEventArgs args)
        {

        }

        #endregion

        #region NumberFormatter

        public static readonly DependencyProperty NumberFormatterProperty =
            DependencyProperty.Register(
                nameof(NumberFormatter),
                typeof(INumberBoxNumberFormatter),
                typeof(NumberBox),
                new PropertyMetadata(
                    new DefaultNumberBoxNumberFormatter(),
                    OnNumberFormatterPropertyChanged),
                ValidateNumberFormatter);

        public INumberBoxNumberFormatter NumberFormatter
        {
            get => (INumberBoxNumberFormatter)GetValue(NumberFormatterProperty);
            set => SetValue(NumberFormatterProperty, value);
        }

        private static void OnNumberFormatterPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            ((NumberBox)sender).OnNumberFormatterPropertyChanged(args);
        }

        private void OnNumberFormatterPropertyChanged(DependencyPropertyChangedEventArgs args)
        {
            // Update text with new formatting
            UpdateTextToValue();
        }

        private static bool ValidateNumberFormatter(object value)
        {
            return value is INumberBoxNumberFormatter;
        }

        #endregion

        #region CornerRadius

        public static readonly DependencyProperty CornerRadiusProperty =
            ControlHelper.CornerRadiusProperty.AddOwner(typeof(NumberBox));

        public CornerRadius CornerRadius
        {
            get => (CornerRadius)GetValue(CornerRadiusProperty);
            set => SetValue(CornerRadiusProperty, value);
        }

        #endregion

        public event TypedEventHandler<NumberBox, NumberBoxValueChangedEventArgs> ValueChanged;

        void UpdateValueToText()
        {
            if (m_textBox != null)
            {
                m_textBox.Text = Text;
                ValidateInput();
            }
        }

        void OnNumberBoxGotFocus(object sender, RoutedEventArgs e)
        {
            // When the control receives focus, select the text
            if (m_textBox != null)
            {
                m_textBox.SelectAll();
            }

            if (SpinButtonPlacementMode == NumberBoxSpinButtonPlacementMode.Compact)
            {
                if (m_popup != null)
                {
                    m_popup.IsOpen = true;
                }
            }
        }

        void OnNumberBoxLostFocus(object sender, RoutedEventArgs e)
        {
            ValidateInput();

            if (m_popup != null)
            {
                m_popup.IsOpen = false;
            }
        }

        void CoerceMinimum()
        {
            var max = Maximum;
            if (Minimum > max)
            {
                Minimum = max;
            }
        }

        void CoerceMaximum()
        {
            var min = Minimum;
            if (Maximum < min)
            {
                Maximum = min;
            }
        }

        void CoerceValue()
        {
            // Validate that the value is in bounds
            var value = Value;
            if (!double.IsNaN(value) && !IsInBounds(value) && ValidationMode == NumberBoxValidationMode.InvalidInputOverwritten)
            {
                // Coerce value to be within range
                var max = Maximum;
                if (value > max)
                {
                    Value = max;
                }
                else
                {
                    Value = Minimum;
                }
            }
        }

        void ValidateInput()
        {
            // Validate the content of the inner textbox
            if (m_textBox != null)
            {
                var text = m_textBox.Text.Trim();

                // Handles empty TextBox case, set text to current value
                if (string.IsNullOrEmpty(text))
                {
                    Value = double.NaN;
                }
                else
                {
                    // Setting NumberFormatter to something that isn't an INumberParser will throw an exception, so this should be safe
                    var numberParser = NumberFormatter;

                    double? value = AcceptsExpression
                       ? NumberBoxParser.Compute(text, numberParser)
                       : numberParser.ParseDouble(text);

                    if (!value.HasValue)
                    {
                        if (ValidationMode == NumberBoxValidationMode.InvalidInputOverwritten)
                        {
                            // Override text to current value
                            UpdateTextToValue();
                        }
                    }
                    else
                    {
                        if (value.Value == Value)
                        {
                            // Even if the value hasn't changed, we still want to update the text (e.g. Value is 3, user types 1 + 2, we want to replace the text with 3)
                            UpdateTextToValue();
                        }
                        else
                        {
                            Value = value.Value;
                        }
                    }
                }
            }
        }

        void OnSpinDownClick(object sender, RoutedEventArgs args)
        {
            StepValue(-SmallChange);
        }

        void OnSpinUpClick(object sender, RoutedEventArgs args)
        {
            StepValue(SmallChange);
        }

        void OnNumberBoxKeyDown(object sender, KeyEventArgs args)
        {
            // Handle these on key down so that we get repeat behavior.
            switch (args.Key)
            {
                case Key.Up:
                    StepValue(SmallChange);
                    args.Handled = true;
                    break;

                case Key.Down:
                    StepValue(-SmallChange);
                    args.Handled = true;
                    break;

                case Key.PageUp:
                    StepValue(LargeChange);
                    args.Handled = true;
                    break;

                case Key.PageDown:
                    StepValue(-LargeChange);
                    args.Handled = true;
                    break;
            }
        }

        void OnNumberBoxKeyUp(object sender, KeyEventArgs args)
        {
            switch (args.Key)
            {
                case Key.Enter:
                    ValidateInput();
                    args.Handled = true;
                    break;

                case Key.Escape:
                    UpdateTextToValue();
                    args.Handled = true;
                    break;
            }
        }

        void OnNumberBoxScroll(object sender, MouseWheelEventArgs args)
        {
            if (m_textBox != null)
            {
                if (m_textBox.IsFocused)
                {
                    var delta = args.Delta;
                    if (delta > 0)
                    {
                        StepValue(SmallChange);
                    }
                    else if (delta < 0)
                    {
                        StepValue(-SmallChange);
                    }
                }
            }
        }

        void StepValue(double change)
        {
            // Before adjusting the value, validate the contents of the textbox so we don't override it.
            ValidateInput();

            var newVal = Value;
            if (!double.IsNaN(newVal))
            {
                newVal += change;

                if (IsWrapEnabled)
                {
                    var max = Maximum;
                    var min = Minimum;

                    if (newVal > max)
                    {
                        newVal = min;
                    }
                    else if (newVal < min)
                    {
                        newVal = max;
                    }
                }

                Value = newVal;
            }
        }

        // Updates TextBox.Text with the formatted Value
        void UpdateTextToValue()
        {
            if (m_textBox != null)
            {
                string newText = string.Empty;

                var value = Value;
                if (!double.IsNaN(value))
                {
                    // Rounding the value here will prevent displaying digits caused by floating point imprecision.
                    var roundedValue = m_displayRounder.RoundDouble(value);
                    newText = NumberFormatter.FormatDouble(roundedValue);
                }

                m_textBox.Text = newText;

                try
                {
                    m_textUpdating = true;

                    Text = newText;

                    // This places the caret at the end of the text.
                    m_textBox.Select(newText.Length, 0);
                }
                finally
                {
                    m_textUpdating = false;
                }
            }
        }

        void UpdateSpinButtonPlacement()
        {
            var spinButtonMode = SpinButtonPlacementMode;

            if (spinButtonMode == NumberBoxSpinButtonPlacementMode.Inline)
            {
                VisualStateManager.GoToState(this, "SpinButtonsVisible", false);
            }
            else if (spinButtonMode == NumberBoxSpinButtonPlacementMode.Compact)
            {
                VisualStateManager.GoToState(this, "SpinButtonsPopup", false);
            }
            else
            {
                VisualStateManager.GoToState(this, "SpinButtonsCollapsed", false);
            }
        }

        void UpdateSpinButtonEnabled()
        {
            var value = Value;
            bool isUpButtonEnabled = false;
            bool isDownButtonEnabled = false;

            if (!double.IsNaN(value))
            {
                if (IsWrapEnabled || ValidationMode != NumberBoxValidationMode.InvalidInputOverwritten)
                {
                    // If wrapping is enabled, or invalid values are allowed, then the buttons should be enabled
                    isUpButtonEnabled = true;
                    isDownButtonEnabled = true;
                }
                else
                {
                    if (value < Maximum)
                    {
                        isUpButtonEnabled = true;
                    }
                    if (value > Minimum)
                    {
                        isDownButtonEnabled = true;
                    }
                }
            }

            VisualStateManager.GoToState(this, isUpButtonEnabled ? "UpSpinButtonEnabled" : "UpSpinButtonDisabled", false);
            VisualStateManager.GoToState(this, isDownButtonEnabled ? "DownSpinButtonEnabled" : "DownSpinButtonDisabled", false);
        }

        bool IsInBounds(double value)
        {
            return value >= Minimum && value <= Maximum;
        }

        private void OnPopupOpened(object sender, EventArgs e)
        {
            if (m_isPopupOpen)
            {
                return;
            }

            m_isPopupOpen = true;

            LayoutUpdated += OnNumberBoxLayoutUpdated;

            m_hostWindow = Window.GetWindow(this);
            if (m_hostWindow != null)
            {
                m_hostWindow.LocationChanged += OnHostWindowLocationChanged;
            }
        }

        private void OnPopupClosed(object sender, EventArgs e)
        {
            LayoutUpdated -= OnNumberBoxLayoutUpdated;

            if (m_hostWindow != null)
            {
                m_hostWindow.LocationChanged -= OnHostWindowLocationChanged;
                m_hostWindow = null;
            }

            m_isPopupOpen = false;
            m_popupPosition = default;
        }

        private void OnNumberBoxLayoutUpdated(object sender, EventArgs e)
        {
            RepositionPopup();
        }

        private void OnHostWindowLocationChanged(object sender, EventArgs e)
        {
            RepositionPopup();
        }

        private void RepositionPopup()
        {
            if (m_popup != null)
            {
                var child = m_popup.Child;
                if (child != null)
                {
                    if (child.IsVisible && IsVisible)
                    {
                        var position = child.TranslatePoint(new Point(), this);
                        if (m_popupPosition != position)
                        {
                            m_popupPosition = position;
                            m_popup.Reposition();
                        }
                    }
                }
            }
        }

        bool m_valueUpdating = false;
        bool m_textUpdating = false;

        DefaultNumberRounder m_displayRounder = new DefaultNumberRounder();

        TextBox m_textBox;
        Popup m_popup;
        bool m_isPopupOpen;
        Point m_popupPosition;
        Window m_hostWindow;
    }
}
