using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Controls;
using ModernWpf.Controls.Primitives;
using static ModernWpf.ResourceAccessor;

namespace ModernWpf.Controls
{
    /// <summary>
    /// Represents a control that allows the user to select a time.
    /// </summary>
    public class TimePicker : TimePickerBase
    {
        private const string ElementPopup = "PART_Popup";
        private const string ElementAmPmSwitcher = "PART_AmPmSwitcher";
        private const string ElementHourPicker = "PART_HourPicker";
        private const string ElementMinutePicker = "PART_MinutePicker";
        private const string ElementSecondPicker = "PART_SecondPicker";

        private Popup _popup;

        private DateTimeComponentSelector _ampmSwitcher;
        private DateTimeComponentSelector _hourInput;
        private DateTimeComponentSelector _minuteInput;
        private DateTimeComponentSelector _secondInput;

        private TextBlock _hourTextBlock;
        private TextBlock _minuteTextBlock;
        private TextBlock _secondTextBlock;
        private TextBlock _periodTextBlock;
        private Button _acceptButton;
        private Button _dismissButton;

        private static readonly ResourceAccessor ResourceAccessor = new ResourceAccessor(typeof(TimePicker));

        static TimePicker()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(TimePicker), new FrameworkPropertyMetadata(typeof(TimePicker)));

            SelectedDateTimeProperty.OverrideMetadata(typeof(TimePicker), new FrameworkPropertyMetadata(OnSelectedDateTimeChanged));
            IsDropDownOpenProperty.OverrideMetadata(typeof(TimePicker), new FrameworkPropertyMetadata(OnIsDropDownOpenChanged));
        }

        public TimePicker()
        {
            IsDatePickerVisible = false;
        }

        #region UseSystemFocusVisuals

        /// <summary>
        /// Identifies the UseSystemFocusVisuals dependency property.
        /// </summary>
        public static readonly DependencyProperty UseSystemFocusVisualsProperty =
           FocusVisualHelper.UseSystemFocusVisualsProperty.AddOwner(typeof(TimePicker));

        /// <summary>
        /// Gets or sets a value that indicates whether the control uses focus visuals that
        /// are drawn by the system or those defined in the control template.
        /// </summary>
        /// <returns>
        /// **true** if the control uses focus visuals drawn by the system; **false** if
        /// the control uses focus visuals defined in the ControlTemplate. The default is
        /// **false**; see Remarks.
        /// </returns>
        public bool UseSystemFocusVisuals
        {
            get => (bool)GetValue(UseSystemFocusVisualsProperty);
            set => SetValue(UseSystemFocusVisualsProperty, value);
        }

        #endregion

        #region CornerRadius

        /// <summary>
        /// Identifies the CornerRadius dependency property.
        /// </summary>
        public static readonly DependencyProperty CornerRadiusProperty =
            ControlHelper.CornerRadiusProperty.AddOwner(typeof(TimePicker));

        /// <summary>
        /// Gets or sets the radius for the corners of the control's border.
        /// </summary>
        /// <returns>
        /// The degree to which the corners are rounded, expressed as values of the CornerRadius
        /// structure.
        /// </returns>
        public CornerRadius CornerRadius
        {
            get => (CornerRadius)GetValue(CornerRadiusProperty);
            set => SetValue(CornerRadiusProperty, value);
        }

        #endregion

        #region Header

        /// <summary>
        /// Identifies the Header dependency property.
        /// </summary>
        public static readonly DependencyProperty HeaderProperty =
            ControlHelper.HeaderProperty.AddOwner(typeof(TimePicker));

        /// <summary>
        /// Gets or sets the content for the control's header.
        /// </summary>
        /// <returns>The content of the control's header. The default is **null**.</returns>
        public object Header
        {
            get => GetValue(HeaderProperty);
            set => SetValue(HeaderProperty, value);
        }

        #endregion

        #region HeaderTemplate

        /// <summary>
        /// Identifies the HeaderTemplate dependency property.
        /// </summary>
        public static readonly DependencyProperty HeaderTemplateProperty =
            ControlHelper.HeaderTemplateProperty.AddOwner(typeof(TimePicker));

        /// <summary>
        /// Gets or sets the DataTemplate used to display the content of the control's header.
        /// </summary>
        /// <returns>
        /// The template that specifies the visualization of the header object. The default
        /// is **null**.
        /// </returns>
        public DataTemplate HeaderTemplate
        {
            get => (DataTemplate)GetValue(HeaderTemplateProperty);
            set => SetValue(HeaderTemplateProperty, value);
        }

        #endregion

        #region HourPlaceholderText

        public static readonly DependencyProperty HourPlaceholderTextProperty =
            DependencyProperty.Register(
                nameof(HourPlaceholderText),
                typeof(string),
                typeof(TimePicker),
                new PropertyMetadata(string.Empty, OnPlaceholderTextChanged));

        public string HourPlaceholderText
        {
            get => (string)GetValue(HourPlaceholderTextProperty);
            set => SetValue(HourPlaceholderTextProperty, value);
        }

        #endregion

        #region MinutePlaceholderText

        public static readonly DependencyProperty MinutePlaceholderTextProperty =
            DependencyProperty.Register(
                nameof(MinutePlaceholderText),
                typeof(string),
                typeof(TimePicker),
                new PropertyMetadata(string.Empty, OnPlaceholderTextChanged));

        public string MinutePlaceholderText
        {
            get => (string)GetValue(MinutePlaceholderTextProperty);
            set => SetValue(MinutePlaceholderTextProperty, value);
        }

        #endregion

        #region SecondPlaceholderText

        public static readonly DependencyProperty SecondPlaceholderTextProperty =
            DependencyProperty.Register(
                nameof(SecondPlaceholderText),
                typeof(string),
                typeof(TimePicker),
                new PropertyMetadata(string.Empty, OnPlaceholderTextChanged));

        public string SecondPlaceholderText
        {
            get => (string)GetValue(SecondPlaceholderTextProperty);
            set => SetValue(SecondPlaceholderTextProperty, value);
        }

        #endregion

        private IEnumerable<DateTimeComponentSelector> Selectors
        {
            get
            {
                yield return _hourInput;
                yield return _minuteInput;
                yield return _secondInput;
                yield return _ampmSwitcher;
            }
        }

        /// <summary>
        /// Called when the Template's tree has been generated.
        /// </summary>
        public override void OnApplyTemplate()
        {
            UnSubscribeEvents();

            _popup = GetTemplateChild(ElementPopup) as Popup;
            _hourInput = GetTemplateChild(ElementHourPicker) as DateTimeComponentSelector;
            _minuteInput = GetTemplateChild(ElementMinutePicker) as DateTimeComponentSelector;
            _secondInput = GetTemplateChild(ElementSecondPicker) as DateTimeComponentSelector;
            _ampmSwitcher = GetTemplateChild(ElementAmPmSwitcher) as DateTimeComponentSelector;
            _hourTextBlock = GetTemplateChild("HourTextBlock") as TextBlock;
            _minuteTextBlock = GetTemplateChild("MinuteTextBlock") as TextBlock;
            _secondTextBlock = GetTemplateChild("SecondTextBlock") as TextBlock;
            _periodTextBlock = GetTemplateChild("PeriodTextBlock") as TextBlock;
            _acceptButton = GetTemplateChild("AcceptButton") as Button;
            _dismissButton = GetTemplateChild("DismissButton") as Button;

            base.OnApplyTemplate();

            SubscribeEvents();
            UpdateTextBlocks();
        }

        protected override void OnIsKeyboardFocusWithinChanged(DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue)
            {
                base.OnIsKeyboardFocusWithinChanged(e);
            }
        }

        /// <inheritdoc />
        protected override void FocusElementAfterIsDropDownOpenChanged()
        {
            if (hourInput is null)
            {
                return;
            }

            // When the popup is opened set focus to the hour input.
            // Do this asynchronously because the IsDropDownOpen could
            // have been set even before the template for the DatePicker is
            // applied. And this would mean that the visuals wouldn't be available yet.

            _ = Dispatcher.BeginInvoke(DispatcherPriority.Input, (Action)delegate
            {
                // setting the focus to the calendar will focus the correct date.
                hourInput.Focus();
            });
        }

        /// <inheritdoc />
        protected override void SetSelectedDateTime()
        {
            if (textBox is null)
            {
                return;
            }

            const DateTimeStyles dateTimeParseStyle = DateTimeStyles.AllowWhiteSpaces
                                                      & DateTimeStyles.AssumeLocal
                                                      & DateTimeStyles.NoCurrentDateDefault;

            if (DateTime.TryParse(textBox.Text, SpecificCultureInfo, dateTimeParseStyle, out var timeSpan))
            {
                SetCurrentValue(SelectedDateTimeProperty, SelectedDateTime.GetValueOrDefault().Date + timeSpan.TimeOfDay);
            }
            else
            {
                SetCurrentValue(SelectedDateTimeProperty, null);
                if (SelectedDateTime == null)
                {
                    // if already null, overwrite wrong data in TextBox
                    WriteValueToTextBox();
                }
            }
        }


        private void SubscribeEvents()
        {
            if (_popup != null)
            {
                _popup.Opened += OnPopupOpened;
            }

            if (_acceptButton != null)
            {
                _acceptButton.Click += OnAcceptButtonClick;
            }

            if (_dismissButton != null)
            {
                _dismissButton.Click += OnDismissButtonClick; ;
            }
        }

        private void UnSubscribeEvents()
        {
            if (_popup != null)
            {
                _popup.Opened -= OnPopupOpened;
            }

            if (_acceptButton != null)
            {
                _acceptButton.Click -= OnAcceptButtonClick;
            }

            if (_dismissButton != null)
            {
                _dismissButton.Click -= OnDismissButtonClick; ;
            }
        }

        protected override void ApplyCulture()
        {
            base.ApplyCulture();

            if (_ampmSwitcher != null && _ampmSwitcher.Items.Count > 0)
            {
                for (int i = 0; i < DateTimeComponentSelector.PaddingItemsCount; i++)
                {
                    _ampmSwitcher.Items.Insert(i, DateTimeComponentSelectorItemsConverter.StringPaddingItem);
                }

                for (int i = 0; i < DateTimeComponentSelector.PaddingItemsCount; i++)
                {
                    _ampmSwitcher.Items.Add(DateTimeComponentSelectorItemsConverter.StringPaddingItem);
                }
            }
        }

        private static void OnSelectedDateTimeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var timePicker = (TimePicker)d;
            timePicker.UpdateTextBlocks();
        }

        private static void OnIsDropDownOpenChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!(bool)e.NewValue)
            {
                var timePicker = (TimePicker)d;
                if (timePicker.IsKeyboardFocusWithin)
                {
                    timePicker.Focus();
                }
            }
        }

        private static void OnPlaceholderTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((TimePicker)d).UpdateTextBlocks();
        }

        private void SetHourPartValues(TimeSpan timeOfDay)
        {
            if (_hourInput != null)
            {
                if (IsMilitaryTime)
                {
                    _ampmSwitcher.SelectedValue = timeOfDay.Hours < 12 ? SpecificCultureInfo.DateTimeFormat.AMDesignator : SpecificCultureInfo.DateTimeFormat.PMDesignator;
                    if (timeOfDay.Hours == 0 || timeOfDay.Hours == 12)
                    {
                        _hourInput.SelectedValue = 12;
                    }
                    else
                    {
                        _hourInput.SelectedValue = timeOfDay.Hours % 12;
                    }
                }
                else
                {
                    _hourInput.SelectedValue = timeOfDay.Hours;
                }
            }

            if (_minuteInput != null)
            {
                _minuteInput.SelectedValue = timeOfDay.Minutes;
            }

            if (_secondInput != null)
            {
                _secondInput.SelectedValue = timeOfDay.Seconds;
            }
        }

        private void UpdateTextBlocks()
        {
            DateTime? selectedTime = SelectedDateTime;

            if (_hourTextBlock != null)
            {
                _hourTextBlock.Text = selectedTime.HasValue ?
                    selectedTime.Value.ToString(IsMilitaryTime ? "%h" : "%H", SpecificCultureInfo) :
                    HourPlaceholderText.DefaultIfNullOrEmpty(ResourceAccessor.GetLocalizedStringResource(SR_TimePickerHour));
            }

            if (_minuteTextBlock != null)
            {
                _minuteTextBlock.Text = selectedTime.HasValue ?
                    selectedTime.Value.ToString("mm", SpecificCultureInfo) :
                    MinutePlaceholderText.DefaultIfNullOrEmpty(ResourceAccessor.GetLocalizedStringResource(SR_TimePickerMinute));
            }

            if (_secondTextBlock != null)
            {
                _secondTextBlock.Text = selectedTime.HasValue ?
                    selectedTime.Value.ToString("ss", SpecificCultureInfo) :
                    SecondPlaceholderText.DefaultIfNullOrEmpty(ResourceAccessor.GetLocalizedStringResource(SR_TimePickerSecond));
            }

            if (_periodTextBlock != null)
            {
                _periodTextBlock.Text = selectedTime.HasValue ?
                    selectedTime.Value.ToString("tt", CultureInfo.InvariantCulture) :
                    SpecificCultureInfo.DateTimeFormat.AMDesignator;
            }
        }

        private void OnPopupOpened(object sender, EventArgs e)
        {
            SetHourPartValues(SelectedDateTime.GetValueOrDefault().TimeOfDay);

            var firstVisibleSelector = Selectors.FirstOrDefault(s => s.Visibility == Visibility.Visible);
            firstVisibleSelector?.Focus();
        }

        private void ClosePopup()
        {
            SetCurrentValue(IsDropDownOpenProperty, false);
            if (_popup != null && _popup.IsOpen)
            {
                _popup.IsOpen = false;
            }
        }

        private void OnAcceptButtonClick(object sender, RoutedEventArgs e)
        {
            foreach (var selector in Selectors)
            {
                selector.RaiseDeferredSelectionChanged();
            }

            ClosePopup();
        }

        private void OnDismissButtonClick(object sender, RoutedEventArgs e)
        {
            ClosePopup();
        }
    }
}
