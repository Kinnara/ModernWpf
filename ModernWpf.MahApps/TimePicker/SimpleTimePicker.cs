using MahApps.Metro.Controls;
using ModernWpf.Controls.Primitives;
using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Markup;

namespace ModernWpf.MahApps.Controls
{
    /// <summary>
    /// Represents a control that allows a user to pick a time value.
    /// </summary>
    [ContentProperty(nameof(Header))]
    public class SimpleTimePicker : TimePicker
    {
        private const string ElementAmPmSwitcher = "PART_AmPmSwitcher";
        private const string ElementHourPicker = "PART_HourPicker";
        private const string ElementMinutePicker = "PART_MinutePicker";
        private const string ElementSecondPicker = "PART_SecondPicker";

        private Selector _ampmSwitcher;
        private Selector _hourInput;
        private Selector _minuteInput;
        private Selector _secondInput;

        private TextBlock _hourTextBlock;
        private TextBlock _minuteTextBlock;
        private TextBlock _secondTextBlock;
        private TextBlock _periodTextBlock;
        private Button _acceptButton;
        private Button _dismissButton;

        static SimpleTimePicker()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(SimpleTimePicker), new FrameworkPropertyMetadata(typeof(SimpleTimePicker)));
        }

        /// <summary>
        /// Initializes a new instance of the SimpleTimePicker class.
        /// </summary>
        public SimpleTimePicker()
        {
        }

        #region CornerRadius

        /// <summary>
        /// Identifies the CornerRadius dependency property.
        /// </summary>
        public static readonly DependencyProperty CornerRadiusProperty =
            ControlHelper.CornerRadiusProperty.AddOwner(typeof(SimpleTimePicker));

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
            ControlHelper.HeaderProperty.AddOwner(typeof(SimpleTimePicker));

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
            ControlHelper.HeaderTemplateProperty.AddOwner(typeof(SimpleTimePicker));

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

        /// <summary>
        /// Called when the Template's tree has been generated.
        /// </summary>
        public override void OnApplyTemplate()
        {
            _hourInput = GetTemplateChild(ElementHourPicker) as Selector;
            _minuteInput = GetTemplateChild(ElementMinutePicker) as Selector;
            _secondInput = GetTemplateChild(ElementSecondPicker) as Selector;
            _ampmSwitcher = GetTemplateChild(ElementAmPmSwitcher) as Selector;
            _hourTextBlock = GetTemplateChild("HourTextBlock") as TextBlock;
            _minuteTextBlock = GetTemplateChild("MinuteTextBlock") as TextBlock;
            _secondTextBlock = GetTemplateChild("SecondTextBlock") as TextBlock;
            _periodTextBlock = GetTemplateChild("PeriodTextBlock") as TextBlock;
            _acceptButton = GetTemplateChild("AcceptButton") as Button;
            _dismissButton = GetTemplateChild("DismissButton") as Button;

            base.OnApplyTemplate();

            UpdateTextBlocks();
        }

        private void OnAcceptButtonClick(object sender, RoutedEventArgs e)
        {
            foreach (var selector in new[] { _hourInput, _minuteInput, _secondInput, _ampmSwitcher }.OfType<DateTimeComponentSelector>())
            {
                selector.RaiseDeferredSelectionChanged();
            }

            ClosePopup();
        }

        private void OnDismissButtonClick(object sender, RoutedEventArgs e)
        {
            ClosePopup();
        }

        protected override void OnIsKeyboardFocusWithinChanged(DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue)
            {
                base.OnIsKeyboardFocusWithinChanged(e);
            }
        }

        protected override void SubscribeEvents()
        {
            base.SubscribeEvents();

            if (Popup != null)
            {
                Popup.Opened += OnPopupOpened;
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

        protected override void UnSubscribeEvents()
        {
            base.UnSubscribeEvents();

            if (Popup != null)
            {
                Popup.Opened -= OnPopupOpened;
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

        protected override void OnSelectedTimeChanged(TimePickerBaseSelectionChangedEventArgs<DateTime?> e)
        {
            base.OnSelectedTimeChanged(e);

            UpdateTextBlocks();
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
                    "hour";
            }

            if (_minuteTextBlock != null)
            {
                _minuteTextBlock.Text = selectedTime.HasValue ?
                    selectedTime.Value.ToString("mm", SpecificCultureInfo) :
                    "minute";
            }

            if (_secondTextBlock != null)
            {
                _secondTextBlock.Text = selectedTime.HasValue ?
                    selectedTime.Value.ToString("ss", SpecificCultureInfo) :
                    "second";
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
        }

        private void ClosePopup()
        {
            SetCurrentValue(IsDropDownOpenProperty, false);
            if (Popup != null && Popup.IsOpen)
            {
                Popup.IsOpen = false;
            }
        }
    }
}
