// Ported from https://github.com/MahApps/MahApps.Metro/blob/develop/src/MahApps.Metro/Controls/TimePicker/TimePickerBase.cs

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Markup;

namespace ModernWpf.Controls
{

    /// <summary>
    /// Represents a base-class for time picking.
    /// </summary>
    [TemplatePart(Name = ElementButton, Type = typeof(Button))]
    [TemplatePart(Name = ElementHourHand, Type = typeof(UIElement))]
    [TemplatePart(Name = ElementHourPicker, Type = typeof(Selector))]
    [TemplatePart(Name = ElementMinuteHand, Type = typeof(UIElement))]
    [TemplatePart(Name = ElementSecondHand, Type = typeof(UIElement))]
    [TemplatePart(Name = ElementSecondPicker, Type = typeof(Selector))]
    [TemplatePart(Name = ElementMinutePicker, Type = typeof(Selector))]
    [TemplatePart(Name = ElementAmPmSwitcher, Type = typeof(Selector))]
    [TemplatePart(Name = ElementTextBox, Type = typeof(DatePickerTextBox))]
    [TemplatePart(Name = ElementPopup, Type = typeof(Popup))]
    [DefaultEvent("SelectedDateTimeChanged")]
    public abstract class TimePickerBase : Control
    {
        private const string ElementAmPmSwitcher = "PART_AmPmSwitcher";
        private const string ElementButton = "PART_Button";
        private const string ElementHourHand = "PART_HourHand";
        private const string ElementHourPicker = "PART_HourPicker";
        private const string ElementMinuteHand = "PART_MinuteHand";
        private const string ElementMinutePicker = "PART_MinutePicker";
        private const string ElementPopup = "PART_Popup";
        private const string ElementSecondHand = "PART_SecondHand";
        private const string ElementSecondPicker = "PART_SecondPicker";
        private const string ElementTextBox = "PART_TextBox";

        private Selector ampmSwitcher;
        private Button dropDownButton;
        private bool deactivateRangeBaseEvent;
        private bool deactivateTextChangedEvent;
        private bool textInputChanged;
        private UIElement hourHand;
        protected Selector hourInput;
        private UIElement minuteHand;
        private Selector minuteInput;
        private Popup popUp;
        private bool disablePopupReopen;
        private UIElement secondHand;
        private Selector secondInput;
        protected DatePickerTextBox textBox;
        protected DateTime? originalSelectedDateTime;

        /// <summary>
        /// This list contains values from 0 to 55 with an interval of 5. It can be used to bind to <see cref="SourceMinutes"/> and <see cref="SourceSeconds"/>.
        /// </summary>
        /// <example>
        /// <code>&lt;MahApps:TimePicker SourceSeconds="{x:Static MahApps:TimePickerBase.IntervalOf5}" /&gt;</code>
        /// <code>&lt;MahApps:DateTimePicker SourceSeconds="{x:Static MahApps:TimePickerBase.IntervalOf5}" /&gt;</code>
        /// </example>
        /// <returns>
        /// Returns a list containing {0, 5, 10, 15, 20, 25, 30, 35, 40, 45, 50, 55}.
        /// </returns>
        public static readonly IEnumerable<int> IntervalOf5 = CreateValueList(5);

        /// <summary>
        /// This list contains values from 0 to 50 with an interval of 10. It can be used to bind to <see cref="SourceMinutes"/> and <see cref="SourceSeconds"/>.
        /// </summary>
        /// <example>
        /// <code>&lt;MahApps:TimePicker SourceSeconds="{x:Static MahApps:TimePickerBase.IntervalOf10}" /&gt;</code>
        /// <code>&lt;MahApps:DateTimePicker SourceSeconds="{x:Static MahApps:TimePickerBase.IntervalOf10}" /&gt;</code>
        /// </example>
        /// <returns>
        /// Returns a list containing {0, 10, 20, 30, 40, 50}.
        /// </returns>
        public static readonly IEnumerable<int> IntervalOf10 = CreateValueList(10);

        /// <summary>
        /// This list contains values from 0 to 45 with an interval of 15. It can be used to bind to <see cref="SourceMinutes"/> and <see cref="SourceSeconds"/>.
        /// </summary>
        /// <example>
        /// <code>&lt;MahApps:TimePicker SourceSeconds="{x:Static MahApps:TimePickerBase.IntervalOf15}" /&gt;</code>
        /// <code>&lt;MahApps:DateTimePicker SourceSeconds="{x:Static MahApps:TimePickerBase.IntervalOf15}" /&gt;</code>
        /// </example>
        /// <returns>
        /// Returns a list containing {0, 15, 30, 45}.
        /// </returns>
        public static readonly IEnumerable<int> IntervalOf15 = CreateValueList(15);

        /// <summary>Identifies the <see cref="SourceHours"/> dependency property.</summary>
        public static readonly DependencyProperty SourceHoursProperty
            = DependencyProperty.Register(nameof(SourceHours),
                                          typeof(IEnumerable<int>),
                                          typeof(TimePickerBase),
                                          new FrameworkPropertyMetadata(Enumerable.Range(0, 24), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, null, CoerceSourceHours));

        private static object CoerceSourceHours(DependencyObject d, object basevalue)
        {
            if (d is TimePickerBase timePicker && basevalue is IEnumerable<int> hourList)
            {
                if (timePicker.IsMilitaryTime)
                {
                    if (timePicker.SourceHoursAmPmComparer is not null)
                    {
                        return hourList.Where(i => i > 0 && i <= 12).OrderBy(i => i, timePicker.SourceHoursAmPmComparer);
                    }

                    return hourList.Where(i => i > 0 && i <= 12);
                }

                return hourList.Where(i => i >= 0 && i < 24);
            }

            return Enumerable.Empty<int>();
        }

        /// <summary>
        /// Gets or sets a collection used to generate the content for selecting the hours.
        /// </summary>
        /// <returns>
        /// A collection that is used to generate the content for selecting the hours. The default is a list of integer from 0
        /// to 23 if <see cref="IsMilitaryTime" /> is false or a list of integer from
        /// 1 to 12 otherwise.
        /// </returns>
        [Category("Common")]
        public IEnumerable<int> SourceHours
        {
            get => (IEnumerable<int>)GetValue(SourceHoursProperty);
            set => SetValue(SourceHoursProperty, value);
        }

        /// <summary>Identifies the <see cref="SourceHoursAmPmComparer"/> dependency property.</summary>
        public static readonly DependencyProperty SourceHoursAmPmComparerProperty
            = DependencyProperty.Register(nameof(SourceHoursAmPmComparer),
                                          typeof(IComparer<int>),
                                          typeof(TimePickerBase),
                                          new FrameworkPropertyMetadata(null, OnSourceHoursAmPmComparerPropertyChanged));

        private static void OnSourceHoursAmPmComparerPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TimePickerBase timePicker && e.OldValue != e.NewValue)
            {
                timePicker.CoerceValue(SourceHoursProperty);
            }
        }

        /// <summary>
        /// Gets or sets a comparer for the Am/Pm collection used to generate the content for selecting the hours.
        /// </summary>
        [Category("Common")]
        public IComparer<int> SourceHoursAmPmComparer
        {
            get => (IComparer<int>)GetValue(SourceHoursAmPmComparerProperty);
            set => SetValue(SourceHoursAmPmComparerProperty, value);
        }

        /// <summary>Identifies the <see cref="SourceMinutes"/> dependency property.</summary>
        public static readonly DependencyProperty SourceMinutesProperty
            = DependencyProperty.Register(nameof(SourceMinutes),
                                          typeof(IEnumerable<int>),
                                          typeof(TimePickerBase),
                                          new FrameworkPropertyMetadata(Enumerable.Range(0, 60), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, null, CoerceSource60));

        private static object CoerceSource60(DependencyObject d, object basevalue)
        {
            if (basevalue is IEnumerable<int> list)
            {
                return list.Where(i => i >= 0 && i < 60);
            }

            return Enumerable.Empty<int>();
        }

        /// <summary>
        /// Gets or sets a collection used to generate the content for selecting the minutes.
        /// </summary>
        /// <returns>
        /// A collection that is used to generate the content for selecting the minutes. The default is a list of int from
        /// 0 to 59.
        /// </returns>
        [Category("Common")]
        public IEnumerable<int> SourceMinutes
        {
            get => (IEnumerable<int>)GetValue(SourceMinutesProperty);
            set => SetValue(SourceMinutesProperty, value);
        }

        /// <summary>Identifies the <see cref="SourceSeconds"/> dependency property.</summary>
        public static readonly DependencyProperty SourceSecondsProperty
            = DependencyProperty.Register(nameof(SourceSeconds),
                                          typeof(IEnumerable<int>),
                                          typeof(TimePickerBase),
                                          new FrameworkPropertyMetadata(Enumerable.Range(0, 60), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, null, CoerceSource60));

        /// <summary>
        /// Gets or sets a collection used to generate the content for selecting the seconds.
        /// </summary>
        /// <returns>
        /// A collection that is used to generate the content for selecting the minutes. The default is a list of int from
        /// 0 to 59.
        /// </returns>
        [Category("Common")]
        public IEnumerable<int> SourceSeconds
        {
            get => (IEnumerable<int>)GetValue(SourceSecondsProperty);
            set => SetValue(SourceSecondsProperty, value);
        }

        /// <summary>Identifies the <see cref="IsDropDownOpen"/> dependency property.</summary>
        public static readonly DependencyProperty IsDropDownOpenProperty
            = DatePicker.IsDropDownOpenProperty.AddOwner(typeof(TimePickerBase),
                                                         new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnIsDropDownOpenChanged, OnCoerceIsDropDownOpen));

        private static object OnCoerceIsDropDownOpen(DependencyObject d, object baseValue)
        {
            if (d is TimePickerBase tp && !tp.IsEnabled)
            {
                return false;
            }

            return baseValue;
        }

        /// <summary>
        /// IsDropDownOpenProperty property changed handler.
        /// </summary>
        /// <param name="d">DatePicker that changed its IsDropDownOpen.</param>
        /// <param name="e">DependencyPropertyChangedEventArgs.</param>
        private static void OnIsDropDownOpenChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not TimePickerBase tp)
            {
                return;
            }

            bool newValue = (bool)e.NewValue;
            if (tp.popUp != null && tp.popUp.IsOpen != newValue)
            {
                tp.popUp.IsOpen = newValue;
                if (newValue)
                {
                    tp.originalSelectedDateTime = tp.SelectedDateTime;

                    tp.FocusElementAfterIsDropDownOpenChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the drop-down for a <see cref="TimePickerBase"/> box is currently open.
        /// </summary>
        /// <returns>true if the drop-down is open; otherwise, false. The default is false.</returns>
        public bool IsDropDownOpen
        {
            get => (bool)GetValue(IsDropDownOpenProperty);
            set => SetValue(IsDropDownOpenProperty, value);
        }

        /// <summary>
        /// This method is invoked when the <see cref="IsDropDownOpenProperty"/> changes.
        /// </summary>
        protected virtual void FocusElementAfterIsDropDownOpenChanged()
        {
            // noting here
        }

        /// <summary>Identifies the <see cref="IsClockVisible"/> dependency property.</summary>
        public static readonly DependencyProperty IsClockVisibleProperty
            = DependencyProperty.Register(nameof(IsClockVisible),
                                          typeof(bool),
                                          typeof(TimePickerBase),
                                          new PropertyMetadata(true));

        /// <summary>
        /// Gets or sets a value indicating whether the clock of this control is visible in the user interface (UI). This is a
        /// dependency property.
        /// </summary>
        /// <remarks>
        /// If this value is set to false then <see cref="Orientation" /> is set to
        /// <see cref="Orientation.Vertical" />
        /// </remarks>
        /// <returns>
        /// true if the clock is visible; otherwise, false. The default value is true.
        /// </returns>
        [Category("Appearance")]
        public bool IsClockVisible
        {
            get => (bool)GetValue(IsClockVisibleProperty);
            set => SetValue(IsClockVisibleProperty, value);
        }

        /// <summary>Identifies the <see cref="IsReadOnly"/> dependency property.</summary>
        public static readonly DependencyProperty IsReadOnlyProperty
            = DependencyProperty.Register(nameof(IsReadOnly),
                                          typeof(bool),
                                          typeof(TimePickerBase),
                                          new PropertyMetadata(false));

        /// <summary>
        /// Gets or sets a value indicating whether the contents of the <see cref="TimePickerBase" /> are not editable.
        /// </summary>
        /// <returns>
        /// true if the <see cref="TimePickerBase" /> is read-only; otherwise, false. The default is false.
        /// </returns>
        public bool IsReadOnly
        {
            get => (bool)GetValue(IsReadOnlyProperty);
            set => SetValue(IsReadOnlyProperty, value);
        }

        /// <summary>Identifies the <see cref="HandVisibility"/> dependency property.</summary>
        public static readonly DependencyProperty HandVisibilityProperty
            = DependencyProperty.Register(nameof(HandVisibility),
                                          typeof(TimePartVisibility),
                                          typeof(TimePickerBase),
                                          new PropertyMetadata(TimePartVisibility.All, OnHandVisibilityChanged));

        private static void OnHandVisibilityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((TimePickerBase)d).SetHandVisibility((TimePartVisibility)e.NewValue);
        }

        /// <summary>
        /// Gets or sets a value indicating the visibility of the clock hands in the user interface (UI).
        /// </summary>
        /// <returns>
        /// The visibility definition of the clock hands. The default is <see cref="TimePartVisibility.All" />.
        /// </returns>
        [Category("Appearance")]
        [DefaultValue(TimePartVisibility.All)]
        public TimePartVisibility HandVisibility
        {
            get => (TimePartVisibility)GetValue(HandVisibilityProperty);
            set => SetValue(HandVisibilityProperty, value);
        }

        /// <summary>Identifies the <see cref="Culture"/> dependency property.</summary>
        public static readonly DependencyProperty CultureProperty
            = DependencyProperty.Register(nameof(Culture),
                                          typeof(CultureInfo),
                                          typeof(TimePickerBase),
                                          new PropertyMetadata(null, OnCultureChanged));

        private static void OnCultureChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var timePartPickerBase = (TimePickerBase)d;

            timePartPickerBase.Language = e.NewValue is CultureInfo info ? XmlLanguage.GetLanguage(info.IetfLanguageTag) : XmlLanguage.Empty;

            timePartPickerBase.ApplyCulture();
        }

        /// <summary>
        /// Gets or sets a value indicating the culture to be used in string formatting operations.
        /// </summary>
        [Category("Behavior")]
        [DefaultValue(null)]
        public CultureInfo Culture
        {
            get => (CultureInfo)GetValue(CultureProperty);
            set => SetValue(CultureProperty, value);
        }

        /// <summary>Identifies the <see cref="PickerVisibility"/> dependency property.</summary>
        public static readonly DependencyProperty PickerVisibilityProperty
            = DependencyProperty.Register(nameof(PickerVisibility),
                                          typeof(TimePartVisibility),
                                          typeof(TimePickerBase),
                                          new PropertyMetadata(TimePartVisibility.All, OnPickerVisibilityChanged));

        private static void OnPickerVisibilityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((TimePickerBase)d).SetPickerVisibility((TimePartVisibility)e.NewValue);
        }

        /// <summary>
        /// Gets or sets a value indicating the visibility of the selectable date-time-parts in the user interface (UI).
        /// </summary>
        /// <returns>
        /// visibility definition of the selectable date-time-parts. The default is <see cref="TimePartVisibility.All" />.
        /// </returns>
        [Category("Appearance")]
        [DefaultValue(TimePartVisibility.All)]
        public TimePartVisibility PickerVisibility
        {
            get => (TimePartVisibility)GetValue(PickerVisibilityProperty);
            set => SetValue(PickerVisibilityProperty, value);
        }

        public static readonly RoutedEvent SelectedDateTimeChangedEvent
            = EventManager.RegisterRoutedEvent(nameof(SelectedDateTimeChanged),
                                               RoutingStrategy.Bubble,
                                               typeof(RoutedPropertyChangedEventHandler<DateTime?>),
                                               typeof(TimePickerBase));

        /// <summary>
        /// Occurs when the <see cref="SelectedDateTime" /> property is changed.
        /// </summary>
        public event RoutedPropertyChangedEventHandler<DateTime?> SelectedDateTimeChanged
        {
            add => AddHandler(SelectedDateTimeChangedEvent, value);
            remove => RemoveHandler(SelectedDateTimeChangedEvent, value);
        }

        /// <summary>Identifies the <see cref="SelectedDateTime"/> dependency property.</summary>
        public static readonly DependencyProperty SelectedDateTimeProperty
            = DependencyProperty.Register(nameof(SelectedDateTime),
                                          typeof(DateTime?),
                                          typeof(TimePickerBase),
                                          new FrameworkPropertyMetadata(default(DateTime?), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnSelectedDateTimeChanged));

        private static void OnSelectedDateTimeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var timePartPickerBase = (TimePickerBase)d;

            if (timePartPickerBase.deactivateRangeBaseEvent)
            {
                return;
            }

            timePartPickerBase.OnSelectedDateTimeChanged((DateTime?)e.OldValue, (DateTime?)e.NewValue);

            timePartPickerBase.WriteValueToTextBox();

            timePartPickerBase.RaiseSelectedDateTimeChangedEvent((DateTime?)e.OldValue, (DateTime?)e.NewValue);
        }

        /// <summary>
        /// Gets or sets the currently selected date and time.
        /// </summary>
        /// <returns>
        /// The date and time which is currently selected. The default is null.
        /// </returns>
        public DateTime? SelectedDateTime
        {
            get => (DateTime?)GetValue(SelectedDateTimeProperty);
            set => SetValue(SelectedDateTimeProperty, value);
        }

        /// <summary>Identifies the <see cref="SelectedTimeFormat"/> dependency property.</summary>
        public static readonly DependencyProperty SelectedTimeFormatProperty
            = DependencyProperty.Register(nameof(SelectedTimeFormat),
                                          typeof(TimePickerFormat),
                                          typeof(TimePickerBase),
                                          new PropertyMetadata(TimePickerFormat.Long, OnSelectedTimeFormatChanged));

        /// <summary>
        /// Gets or sets the format that is used to display the selected time.
        /// </summary>
        [Category("Appearance")]
        [DefaultValue(TimePickerFormat.Long)]
        public TimePickerFormat SelectedTimeFormat
        {
            get => (TimePickerFormat)GetValue(SelectedTimeFormatProperty);
            set => SetValue(SelectedTimeFormatProperty, value);
        }

        /// <summary>Identifies the <see cref="HoursItemStringFormat"/> dependency property.</summary>
        public static readonly DependencyProperty HoursItemStringFormatProperty
            = DependencyProperty.Register(nameof(HoursItemStringFormat),
                                          typeof(string),
                                          typeof(TimePickerBase),
                                          new FrameworkPropertyMetadata(null));

        /// <summary>
        /// Gets or sets a composite string that specifies how to format the hour items.
        /// </summary>
        public string HoursItemStringFormat
        {
            get => (string)GetValue(HoursItemStringFormatProperty);
            set => SetValue(HoursItemStringFormatProperty, value);
        }

        /// <summary>Identifies the <see cref="MinutesItemStringFormat"/> dependency property.</summary>
        public static readonly DependencyProperty MinutesItemStringFormatProperty
            = DependencyProperty.Register(nameof(MinutesItemStringFormat),
                                          typeof(string),
                                          typeof(TimePickerBase),
                                          new FrameworkPropertyMetadata(null));

        /// <summary>
        /// Gets or sets a composite string that specifies how to format the minute items.
        /// </summary>
        public string MinutesItemStringFormat
        {
            get => (string)GetValue(MinutesItemStringFormatProperty);
            set => SetValue(MinutesItemStringFormatProperty, value);
        }

        /// <summary>Identifies the <see cref="SecondsItemStringFormat"/> dependency property.</summary>
        public static readonly DependencyProperty SecondsItemStringFormatProperty
            = DependencyProperty.Register(nameof(SecondsItemStringFormat),
                                          typeof(string),
                                          typeof(TimePickerBase),
                                          new FrameworkPropertyMetadata(null));

        /// <summary>
        /// Gets or sets a composite string that specifies how to format the second items.
        /// </summary>
        public string SecondsItemStringFormat
        {
            get => (string)GetValue(SecondsItemStringFormatProperty);
            set => SetValue(SecondsItemStringFormatProperty, value);
        }

        #region Do not change order of fields inside this region

        /// <summary>
        /// This readonly dependency property is to control whether to show the date-picker (in case of <see cref="DateTimePicker"/>) or hide it (in case of <see cref="TimePicker"/>.
        /// </summary>
        private static readonly DependencyPropertyKey IsDatePickerVisiblePropertyKey
            = DependencyProperty.RegisterReadOnly(nameof(IsDatePickerVisible),
                                                  typeof(bool),
                                                  typeof(TimePickerBase),
                                                  new PropertyMetadata(true));

        /// <summary>Identifies the <see cref="IsDatePickerVisible"/> dependency property.</summary>
        [SuppressMessage("StyleCop.CSharp.OrderingRules", "SA1202:ElementsMustBeOrderedByAccess", Justification = "Otherwise we have \"Static member initializer refers to static member below or in other type part\" and thus resulting in having \"null\" as value")]
        public static readonly DependencyProperty IsDatePickerVisibleProperty = IsDatePickerVisiblePropertyKey.DependencyProperty;

        /// <summary>
        /// Gets or sets a value indicating whether the date can be selected or not. This property is read-only.
        /// </summary>
        public bool IsDatePickerVisible
        {
            get => (bool)GetValue(IsDatePickerVisibleProperty);
            protected set => SetValue(IsDatePickerVisiblePropertyKey, value);
        }

        #endregion

        public void Clear()
        {
            this.SetCurrentValue(SelectedDateTimeProperty, null);
            WriteValueToTextBox();
        }

        static TimePickerBase()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(TimePickerBase), new FrameworkPropertyMetadata(typeof(TimePickerBase)));
            EventManager.RegisterClassHandler(typeof(TimePickerBase), GotFocusEvent, new RoutedEventHandler(OnGotFocus));
            VerticalContentAlignmentProperty.OverrideMetadata(typeof(TimePickerBase), new FrameworkPropertyMetadata(VerticalAlignment.Center));
            LanguageProperty.OverrideMetadata(typeof(TimePickerBase), new FrameworkPropertyMetadata(OnLanguageChanged));
            IsEnabledProperty.OverrideMetadata(typeof(TimePickerBase), new UIPropertyMetadata(OnIsEnabledChanged));
        }

        protected TimePickerBase()
        {
            this.SetCurrentValue(SourceHoursAmPmComparerProperty, new AmPmComparer());

            Mouse.AddPreviewMouseDownOutsideCapturedElementHandler(this, OutsideCapturedElementHandler);
        }

        private static void OnLanguageChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var timePartPickerBase = (TimePickerBase)d;

            timePartPickerBase.Language = e.NewValue as XmlLanguage ?? XmlLanguage.Empty;

            timePartPickerBase.ApplyCulture();
        }

        private static void OnIsEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TimePickerBase tp)
            {
                tp.CoerceValue(IsDropDownOpenProperty);
            }
        }

        /// <summary>
        /// Gets a value indicating whether the <see cref="DateTimeFormatInfo.AMDesignator" /> that is specified by the
        /// <see cref="CultureInfo" />
        /// set by the <see cref="Culture" /> (<see cref="FrameworkElement.Language" /> if null) has not a value.
        /// </summary>
        public bool IsMilitaryTime
        {
            get
            {
                var dateTimeFormat = SpecificCultureInfo.DateTimeFormat;
                return !string.IsNullOrEmpty(dateTimeFormat.AMDesignator) && (dateTimeFormat.ShortTimePattern.Contains("h") || dateTimeFormat.LongTimePattern.Contains("h"));
            }
        }

        protected CultureInfo SpecificCultureInfo => Culture ?? Language.GetSpecificCulture();

        /// <summary>
        /// When overridden in a derived class, is invoked whenever application code or internal processes call
        /// <see cref="M:System.Windows.FrameworkElement.ApplyTemplate" />.
        /// </summary>
        public override void OnApplyTemplate()
        {
            UnSubscribeEvents();

            base.OnApplyTemplate();

            popUp = GetTemplateChild(ElementPopup) as Popup;

            dropDownButton = GetTemplateChild(ElementButton) as Button;
            hourInput = GetTemplateChild(ElementHourPicker) as Selector;
            minuteInput = GetTemplateChild(ElementMinutePicker) as Selector;
            secondInput = GetTemplateChild(ElementSecondPicker) as Selector;
            hourHand = GetTemplateChild(ElementHourHand) as FrameworkElement;
            ampmSwitcher = GetTemplateChild(ElementAmPmSwitcher) as Selector;
            minuteHand = GetTemplateChild(ElementMinuteHand) as FrameworkElement;
            secondHand = GetTemplateChild(ElementSecondHand) as FrameworkElement;
            textBox = GetTemplateChild(ElementTextBox) as DatePickerTextBox;

            SetHandVisibility(HandVisibility);
            SetPickerVisibility(PickerVisibility);

            SetHourPartValues(SelectedDateTime.GetValueOrDefault().TimeOfDay);
            WriteValueToTextBox();

            SetDefaultTimeOfDayValues();
            SubscribeEvents();
            ApplyCulture();
        }

        protected virtual void ApplyCulture()
        {
            deactivateRangeBaseEvent = true;
            try
            {
                if (ampmSwitcher != null)
                {
                    ampmSwitcher.Items.Clear();
                    if (!string.IsNullOrEmpty(SpecificCultureInfo.DateTimeFormat.AMDesignator))
                    {
                        ampmSwitcher.Items.Add(SpecificCultureInfo.DateTimeFormat.AMDesignator);
                    }

                    if (!string.IsNullOrEmpty(SpecificCultureInfo.DateTimeFormat.PMDesignator))
                    {
                        ampmSwitcher.Items.Add(SpecificCultureInfo.DateTimeFormat.PMDesignator);
                    }
                }

                SetAmPmVisibility();

                CoerceValue(SourceHoursProperty);

                if (SelectedDateTime != null)
                {
                    SetHourPartValues(SelectedDateTime.Value.TimeOfDay);
                }

                SetDefaultTimeOfDayValues();
            }
            finally
            {
                deactivateRangeBaseEvent = false;
            }

            WriteValueToTextBox();
        }

        protected Binding GetBinding(DependencyProperty property, BindingMode bindingMode = BindingMode.Default)
        {
            return new Binding(property.Name) { Source = this, Mode = bindingMode };
        }

        protected virtual string GetValueForTextBox()
        {
            var format = SelectedTimeFormat == TimePickerFormat.Long ? string.Intern(SpecificCultureInfo.DateTimeFormat.LongTimePattern) : string.Intern(SpecificCultureInfo.DateTimeFormat.ShortTimePattern);
            var valueForTextBox = SelectedDateTime?.ToString(string.Intern(format), SpecificCultureInfo);
            return valueForTextBox;
        }

        protected virtual void ClockSelectedTimeChanged()
        {
            var time = GetSelectedTimeFromGUI() ?? TimeSpan.Zero;
            var date = SelectedDateTime ?? DateTime.Today;

            this.SetCurrentValue(SelectedDateTimeProperty, date.Date + time);
        }

        protected void RaiseSelectedDateTimeChangedEvent(DateTime? oldValue, DateTime? newValue)
        {
            var args = new RoutedPropertyChangedEventArgs<DateTime?>(oldValue, newValue) { RoutedEvent = SelectedDateTimeChangedEvent };
            RaiseEvent(args);
        }

        private static void OnSelectedTimeFormatChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TimePickerBase tp)
            {
                tp.WriteValueToTextBox();
            }
        }

        protected void SetDefaultTimeOfDayValues()
        {
            SetDefaultTimeOfDayValue(hourInput, hourInput?.Items.IndexOf(IsMilitaryTime ? 12 : 0));
            SetDefaultTimeOfDayValue(minuteInput, 0);
            SetDefaultTimeOfDayValue(secondInput, 0);
            SetDefaultTimeOfDayValue(ampmSwitcher, 0);
        }

        private void SubscribeEvents()
        {
            if (popUp != null)
            {
                popUp.AddHandler(PreviewMouseLeftButtonDownEvent, new MouseButtonEventHandler(PopUp_PreviewMouseLeftButtonDown));
                popUp.Opened += PopUp_Opened;
                popUp.Closed += PopUp_Closed;

                if (IsDropDownOpen)
                {
                    popUp.IsOpen = true;
                }
            }

            SubscribeTimePickerEvents(hourInput, minuteInput, secondInput, ampmSwitcher);

            if (dropDownButton != null)
            {
                dropDownButton.Click += OnDropDownButtonClicked;
                dropDownButton.AddHandler(MouseLeaveEvent, new MouseEventHandler(DropDownButton_MouseLeave), true);
            }

            if (textBox != null)
            {
                textBox.AddHandler(KeyDownEvent, new KeyEventHandler(TextBox_KeyDown), true);
                textBox.TextChanged += TextBox_TextChanged;
                textBox.LostFocus += TextBox_LostFocus;
            }
        }

        private void UnSubscribeEvents()
        {
            if (popUp != null)
            {
                popUp.RemoveHandler(PreviewMouseLeftButtonDownEvent, new MouseButtonEventHandler(PopUp_PreviewMouseLeftButtonDown));
                popUp.Opened -= PopUp_Opened;
                popUp.Closed -= PopUp_Closed;
            }

            UnsubscribeTimePickerEvents(hourInput, minuteInput, secondInput, ampmSwitcher);

            if (dropDownButton != null)
            {
                dropDownButton.Click -= OnDropDownButtonClicked;
                dropDownButton.RemoveHandler(MouseLeaveEvent, new MouseEventHandler(DropDownButton_MouseLeave));
            }

            if (textBox != null)
            {
                textBox.RemoveHandler(KeyDownEvent, new KeyEventHandler(TextBox_KeyDown));
                textBox.TextChanged -= TextBox_TextChanged;
                textBox.LostFocus -= TextBox_LostFocus;
            }
        }

        private void OutsideCapturedElementHandler(object sender, MouseButtonEventArgs e)
        {
            if (IsDropDownOpen)
            {
                if (!(dropDownButton?.InputHitTest(e.GetPosition(dropDownButton)) is null))
                {
                    return;
                }

                SetCurrentValue(IsDropDownOpenProperty, false);
            }
        }

        private void PopUp_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is Popup popup && !popup.StaysOpen)
            {
                if (dropDownButton?.InputHitTest(e.GetPosition(dropDownButton)) != null)
                {
                    // This popup is being closed by a mouse press on the drop down button
                    // The following mouse release will cause the closed popup to immediately reopen.
                    // Raise a flag to block reopeneing the popup
                    disablePopupReopen = true;
                }
            }
        }

        private void PopUp_Opened(object sender, EventArgs e)
        {
            if (!IsDropDownOpen)
            {
                SetCurrentValue(IsDropDownOpenProperty, true);
            }

            OnPopUpOpened();
        }

        protected virtual void OnPopUpOpened()
        {
            // nothing here
        }

        private void PopUp_Closed(object sender, EventArgs e)
        {
            if (IsDropDownOpen)
            {
                SetCurrentValue(IsDropDownOpenProperty, false);
            }

            OnPopUpClosed();
        }

        protected virtual void OnPopUpClosed()
        {
            // nothing here
        }

        protected virtual void WriteValueToTextBox()
        {
            if (textBox != null)
            {
                deactivateTextChangedEvent = true;
                textBox.Text = GetValueForTextBox();
                deactivateTextChangedEvent = false;
            }
        }

        private static IList<int> CreateValueList(int interval)
        {
            return Enumerable.Repeat(interval, 60 / interval)
                             .Select((value, index) => value * index)
                             .ToList();
        }

        private void TimePickerPreviewKeyDown(object sender, RoutedEventArgs e)
        {
            if (e.OriginalSource is not Selector)
            {
                return;
            }

            var selector = sender as Selector;
            var keyEventArgs = (KeyEventArgs)e;

            Debug.Assert(selector != null);
            Debug.Assert(keyEventArgs != null);

            if (keyEventArgs is not null && (keyEventArgs.Key == Key.Escape || keyEventArgs.Key == Key.Enter || keyEventArgs.Key == Key.Space))
            {
                this.SetCurrentValue(IsDropDownOpenProperty, false);
                if (keyEventArgs.Key == Key.Escape)
                {
                    this.SetCurrentValue(SelectedDateTimeProperty, originalSelectedDateTime);
                }
            }
        }

        private void TimePickerSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (deactivateRangeBaseEvent)
            {
                return;
            }

            ClockSelectedTimeChanged();
        }

        private static void OnGotFocus(object sender, RoutedEventArgs e)
        {
            TimePickerBase picker = (TimePickerBase)sender;
            if (!e.Handled && picker.Focusable)
            {
                if (Equals(e.OriginalSource, picker))
                {
                    // MoveFocus takes a TraversalRequest as its argument.
                    var request = new TraversalRequest((Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift ? FocusNavigationDirection.Previous : FocusNavigationDirection.Next);
                    // Gets the element with keyboard focus.
                    // Change keyboard focus.
                    if (Keyboard.FocusedElement is UIElement elementWithFocus)
                    {
                        elementWithFocus.MoveFocus(request);
                    }
                    else
                    {
                        picker.Focus();
                    }

                    e.Handled = true;
                }
                else if (picker.textBox != null && Equals(e.OriginalSource, picker.textBox))
                {
                    picker.textBox.SelectAll();
                    e.Handled = true;
                }
            }
        }

        protected virtual void OnSelectedDateTimeChanged(DateTime? oldValue, DateTime? newValue)
        {
            SetHourPartValues(newValue.GetValueOrDefault().TimeOfDay);
        }

        private static void SetVisibility(UIElement partHours, UIElement partMinutes, UIElement partSeconds, TimePartVisibility visibility)
        {
            if (partHours != null)
            {
                partHours.Visibility = visibility.HasFlag(TimePartVisibility.Hour) ? Visibility.Visible : Visibility.Collapsed;
            }

            if (partMinutes != null)
            {
                partMinutes.Visibility = visibility.HasFlag(TimePartVisibility.Minute) ? Visibility.Visible : Visibility.Collapsed;
            }

            if (partSeconds != null)
            {
                partSeconds.Visibility = visibility.HasFlag(TimePartVisibility.Second) ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        private static bool IsValueSelected(Selector selector)
        {
            return selector?.SelectedItem is not null;
        }

        private static void SetDefaultTimeOfDayValue(Selector selector, int? defaultIndex)
        {
            if (selector is not null && selector.SelectedValue is null)
            {
                selector.SelectedIndex = defaultIndex.GetValueOrDefault(0);
            }
        }

        protected TimeSpan? GetSelectedTimeFromGUI()
        {
            if (IsValueSelected(hourInput) &&
                IsValueSelected(minuteInput) &&
                IsValueSelected(secondInput))
            {
                var hours = (int)hourInput!.SelectedItem;
                var minutes = (int)minuteInput!.SelectedItem;
                var seconds = (int)secondInput!.SelectedItem;

                hours += GetAmPmOffset(hours);

                return new TimeSpan(hours, minutes, seconds);
            }

            return null;
        }

        /// <summary>
        /// Gets the offset from the selected <paramref name="currentHour" /> to use it in <see cref="TimeSpan" /> as hour
        /// parameter.
        /// </summary>
        /// <param name="currentHour">The current hour.</param>
        /// <returns>
        /// An integer representing the offset to add to the hour that is selected in the hour-picker for setting the correct
        /// <see cref="DateTime.TimeOfDay" />. The offset is determined as follows:
        /// <list type="table">
        ///     <listheader>
        ///         <term>Condition</term><description>Offset</description>
        ///     </listheader>
        ///     <item>
        ///         <term><see cref="IsMilitaryTime" /> is false</term><description>0</description>
        ///     </item>
        ///     <item>
        ///         <term>Selected hour is between 1 AM and 11 AM</term><description>0</description>
        ///     </item>
        ///     <item>
        ///         <term>Selected hour is 12 AM</term><description>-12h</description>
        ///     </item>
        ///     <item>
        ///         <term>Selected hour is between 12 PM and 11 PM</term><description>+12h</description>
        ///     </item>
        /// </list>
        /// </returns>
        private int GetAmPmOffset(int currentHour)
        {
            if (IsMilitaryTime
                && ampmSwitcher is not null)
            {
                if (currentHour == 12)
                {
                    if (Equals(ampmSwitcher.SelectedItem, SpecificCultureInfo.DateTimeFormat.AMDesignator))
                    {
                        return -12;
                    }
                }
                else if (Equals(ampmSwitcher.SelectedItem, SpecificCultureInfo.DateTimeFormat.PMDesignator))
                {
                    return 12;
                }
            }

            return 0;
        }

        private void OnDropDownButtonClicked(object sender, RoutedEventArgs e)
        {
            TogglePopUp();
        }

        private void DropDownButton_MouseLeave(object sender, MouseEventArgs e)
        {
            disablePopupReopen = false;
        }

        private void TogglePopUp()
        {
            if (IsDropDownOpen)
            {
                SetCurrentValue(IsDropDownOpenProperty, false);
            }
            else
            {
                if (disablePopupReopen)
                {
                    disablePopupReopen = false;
                }
                else
                {
                    SetSelectedDateTime();
                    SetCurrentValue(IsDropDownOpenProperty, true);
                }
            }
        }

        private void SetAmPmVisibility()
        {
            if (ampmSwitcher != null)
            {
                if (!PickerVisibility.HasFlag(TimePartVisibility.Hour))
                {
                    ampmSwitcher.Visibility = Visibility.Collapsed;
                }
                else
                {
                    ampmSwitcher.Visibility = IsMilitaryTime ? Visibility.Visible : Visibility.Collapsed;
                }
            }
        }

        private void SetHandVisibility(TimePartVisibility visibility)
        {
            SetVisibility(hourHand, minuteHand, secondHand, visibility);
        }

        private void SetHourPartValues(TimeSpan timeOfDay)
        {
            if (deactivateRangeBaseEvent)
            {
                return;
            }

            deactivateRangeBaseEvent = true;
            try
            {
                if (hourInput != null)
                {
                    if (IsMilitaryTime)
                    {
                        if (ampmSwitcher is not null)
                        {
                            ampmSwitcher.SelectedValue = timeOfDay.Hours < 12 ? SpecificCultureInfo.DateTimeFormat.AMDesignator : SpecificCultureInfo.DateTimeFormat.PMDesignator;
                            if (timeOfDay.Hours == 0 || timeOfDay.Hours == 12)
                            {
                                hourInput.SelectedValue = 12;
                            }
                            else
                            {
                                hourInput.SelectedValue = timeOfDay.Hours % 12;
                            }
                        }
                    }
                    else
                    {
                        hourInput.SelectedValue = timeOfDay.Hours;
                    }
                }

                if (minuteInput != null)
                {
                    minuteInput.SelectedValue = timeOfDay.Minutes;
                }

                if (secondInput != null)
                {
                    secondInput.SelectedValue = timeOfDay.Seconds;
                }
            }
            finally
            {
                deactivateRangeBaseEvent = false;
            }
        }

        private void SetPickerVisibility(TimePartVisibility visibility)
        {
            SetVisibility(hourInput, minuteInput, secondInput, visibility);
            SetAmPmVisibility();
        }

        private void UnsubscribeTimePickerEvents(params Selector[] selectors)
        {
            foreach (var selector in selectors)
            {
                if (selector is null)
                {
                    continue;
                }

                selector.PreviewKeyDown -= TimePickerPreviewKeyDown;
                selector.SelectionChanged -= TimePickerSelectionChanged;
            }
        }

        private void SubscribeTimePickerEvents(params Selector[] selectors)
        {
            foreach (var selector in selectors)
            {
                if (selector is null)
                {
                    continue;
                }

                selector.PreviewKeyDown += TimePickerPreviewKeyDown;
                selector.SelectionChanged += TimePickerSelectionChanged;
            }
        }

        private bool ProcessKey(KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.System:
                    {
                        switch (e.SystemKey)
                        {
                            case Key.Down:
                                {
                                    if ((Keyboard.Modifiers & ModifierKeys.Alt) == ModifierKeys.Alt)
                                    {
                                        TogglePopUp();
                                        return true;
                                    }

                                    break;
                                }
                        }

                        break;
                    }

                case Key.Enter:
                    {
                        SetSelectedDateTime();
                        return true;
                    }
            }

            return false;
        }

        protected abstract void SetSelectedDateTime();

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (textInputChanged)
            {
                textInputChanged = false;

                SetSelectedDateTime();
            }
        }

        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            e.Handled = ProcessKey(e) || e.Handled;
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!deactivateTextChangedEvent)
            {
                textInputChanged = true;
            }
        }
    }
}
