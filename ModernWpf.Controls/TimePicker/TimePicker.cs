// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Markup;
using ModernWpf.Automation.Peers;
using ModernWpf.Controls.Primitives;
using static ModernWpf.ResourceAccessor;

namespace ModernWpf.Controls
{
    [ContentProperty(nameof(Header))]
    [TemplatePart(Name = FlyoutButtonName, Type = typeof(Button))]
    [TemplatePart(Name = PopupName, Type = typeof(Popup))]
    [TemplatePart(Name = HourPickerName, Type = typeof(ListBox))]
    [TemplatePart(Name = MinutePickerName, Type = typeof(ListBox))]
    [TemplatePart(Name = PeriodPickerName, Type = typeof(ListBox))]
    public class TimePicker : Control
    {
        internal const string TwelveHourClockIdentifier = "12HourClock";
        internal const string TwentyFourHourClockIdentifier = "24HourClock";

        private const string FlyoutButtonName = "FlyoutButton";
        private const string PopupName = "PART_Popup";
        private const string HourPickerName = "HourPicker";
        private const string MinutePickerName = "MinutePicker";
        private const string PeriodPickerName = "PeriodPicker";
        private const long NullTimeSentinelTicks = -1;

        private static readonly TimeSpan NullTimeSentinel = TimeSpan.FromTicks(NullTimeSentinelTicks);
        private static readonly ResourceAccessor ResourceAccessor = new ResourceAccessor(typeof(TimePicker));

        public static readonly DependencyProperty HeaderProperty =
            ControlHelper.HeaderProperty.AddOwner(
                typeof(TimePicker),
                new FrameworkPropertyMetadata(OnHeaderPropertyChanged));

        public static readonly DependencyProperty HeaderTemplateProperty =
            ControlHelper.HeaderTemplateProperty.AddOwner(
                typeof(TimePicker),
                new FrameworkPropertyMetadata(OnHeaderPropertyChanged));

        public static readonly DependencyProperty ClockIdentifierProperty =
            DependencyProperty.Register(
                nameof(ClockIdentifier),
                typeof(string),
                typeof(TimePicker),
                new PropertyMetadata(GetDefaultClockIdentifier(), OnClockIdentifierPropertyChanged),
                IsValidClockIdentifier);

        public static readonly DependencyProperty MinuteIncrementProperty =
            DependencyProperty.Register(
                nameof(MinuteIncrement),
                typeof(int),
                typeof(TimePicker),
                new PropertyMetadata(1, OnMinuteIncrementPropertyChanged),
                IsValidMinuteIncrement);

        public static readonly DependencyProperty TimeProperty =
            DependencyProperty.Register(
                nameof(Time),
                typeof(TimeSpan),
                typeof(TimePicker),
                new FrameworkPropertyMetadata(
                    NullTimeSentinel,
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault | FrameworkPropertyMetadataOptions.Journal,
                    OnTimePropertyChanged,
                    ValidateTime));

        public static readonly DependencyProperty LightDismissOverlayModeProperty =
            DependencyProperty.Register(
                nameof(LightDismissOverlayMode),
                typeof(LightDismissOverlayMode),
                typeof(TimePicker),
                new PropertyMetadata(LightDismissOverlayMode.Auto));

        public static readonly DependencyProperty SelectedTimeProperty =
            DependencyProperty.Register(
                nameof(SelectedTime),
                typeof(TimeSpan?),
                typeof(TimePicker),
                new FrameworkPropertyMetadata(
                    null,
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault | FrameworkPropertyMetadataOptions.Journal,
                    OnSelectedTimePropertyChanged));

        public static readonly DependencyProperty HeaderPlacementProperty =
            DependencyProperty.Register(
                nameof(HeaderPlacement),
                typeof(ControlHeaderPlacement),
                typeof(TimePicker),
                new PropertyMetadata(ControlHeaderPlacement.Top, OnHeaderPropertyChanged));

        public static readonly DependencyProperty CornerRadiusProperty =
            Border.CornerRadiusProperty.AddOwner(typeof(TimePicker));

        private Button _flyoutButton;
        private Popup _popup;
        private ListBox _hourPicker;
        private ListBox _minutePicker;
        private ListBox _periodPicker;
        private Button _acceptButton;
        private Button _dismissButton;
        private ContentPresenter _headerPresenter;
        private Grid _layoutRoot;
        private RowDefinition _headerRow;
        private RowDefinition _buttonRow;
        private ColumnDefinition _headerColumn;
        private ColumnDefinition _buttonColumn;
        private TextBlock _hourTextBlock;
        private TextBlock _minuteTextBlock;
        private TextBlock _periodTextBlock;
        private FrameworkElement _hourHost;
        private FrameworkElement _minuteHost;
        private FrameworkElement _periodHost;
        private FrameworkElement _firstColumnDivider;
        private FrameworkElement _secondColumnDivider;
        private ColumnDefinition _firstTextBlockColumn;
        private ColumnDefinition _secondTextBlockColumn;
        private ColumnDefinition _thirdTextBlockColumn;
        private bool _isSynchronizingTime;
        private bool _isClosingPopup;

        static TimePicker()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(TimePicker), new FrameworkPropertyMetadata(typeof(TimePicker)));
        }

        public TimePicker()
        {
            AddHandler(Keyboard.PreviewKeyDownEvent, new KeyEventHandler(OnPreviewKeyDown), true);
            IsEnabledChanged += OnIsEnabledChanged;
        }

        public object Header
        {
            get => GetValue(HeaderProperty);
            set => SetValue(HeaderProperty, value);
        }

        public DataTemplate HeaderTemplate
        {
            get => (DataTemplate)GetValue(HeaderTemplateProperty);
            set => SetValue(HeaderTemplateProperty, value);
        }

        public string ClockIdentifier
        {
            get => (string)GetValue(ClockIdentifierProperty);
            set => SetValue(ClockIdentifierProperty, value);
        }

        public int MinuteIncrement
        {
            get => (int)GetValue(MinuteIncrementProperty);
            set => SetValue(MinuteIncrementProperty, value);
        }

        public TimeSpan Time
        {
            get => (TimeSpan)GetValue(TimeProperty);
            set => SetValue(TimeProperty, value);
        }

        public LightDismissOverlayMode LightDismissOverlayMode
        {
            get => (LightDismissOverlayMode)GetValue(LightDismissOverlayModeProperty);
            set => SetValue(LightDismissOverlayModeProperty, value);
        }

        public TimeSpan? SelectedTime
        {
            get => (TimeSpan?)GetValue(SelectedTimeProperty);
            set => SetValue(SelectedTimeProperty, value);
        }

        public ControlHeaderPlacement HeaderPlacement
        {
            get => (ControlHeaderPlacement)GetValue(HeaderPlacementProperty);
            set => SetValue(HeaderPlacementProperty, value);
        }

        public CornerRadius CornerRadius
        {
            get => (CornerRadius)GetValue(CornerRadiusProperty);
            set => SetValue(CornerRadiusProperty, value);
        }

        public event EventHandler<TimePickerValueChangedEventArgs> TimeChanged;

        public event TypedEventHandler<TimePicker, TimePickerSelectedValueChangedEventArgs> SelectedTimeChanged;

        public override void OnApplyTemplate()
        {
            UnhookTemplateParts();
            base.OnApplyTemplate();

            _flyoutButton = GetTemplateChild(FlyoutButtonName) as Button;
            _popup = GetTemplateChild(PopupName) as Popup;
            _hourPicker = GetTemplateChild(HourPickerName) as ListBox;
            _minutePicker = GetTemplateChild(MinutePickerName) as ListBox;
            _periodPicker = GetTemplateChild(PeriodPickerName) as ListBox;
            _acceptButton = GetTemplateChild("AcceptButton") as Button;
            _dismissButton = GetTemplateChild("DismissButton") as Button;
            _headerPresenter = GetTemplateChild("HeaderContentPresenter") as ContentPresenter;
            _layoutRoot = GetTemplateChild("LayoutRoot") as Grid;
            _headerRow = GetTemplateChild("PART_HeaderRow") as RowDefinition;
            _buttonRow = GetTemplateChild("PART_ButtonRow") as RowDefinition;
            _headerColumn = GetTemplateChild("PART_HeaderColumn") as ColumnDefinition;
            _buttonColumn = GetTemplateChild("PART_ButtonColumn") as ColumnDefinition;
            _hourTextBlock = GetTemplateChild("HourTextBlock") as TextBlock;
            _minuteTextBlock = GetTemplateChild("MinuteTextBlock") as TextBlock;
            _periodTextBlock = GetTemplateChild("PeriodTextBlock") as TextBlock;
            _hourHost = GetTemplateChild("HourPickerHost") as FrameworkElement;
            _minuteHost = GetTemplateChild("MinutePickerHost") as FrameworkElement;
            _periodHost = GetTemplateChild("PeriodPickerHost") as FrameworkElement;
            _firstColumnDivider = GetTemplateChild("FirstColumnDivider") as FrameworkElement;
            _secondColumnDivider = GetTemplateChild("SecondColumnDivider") as FrameworkElement;
            _firstTextBlockColumn = GetTemplateChild("FirstTextBlockColumn") as ColumnDefinition;
            _secondTextBlockColumn = GetTemplateChild("SecondTextBlockColumn") as ColumnDefinition;
            _thirdTextBlockColumn = GetTemplateChild("ThirdTextBlockColumn") as ColumnDefinition;

            HookTemplateParts();
            ApplyLocalizedAutomationNames();
            UpdateHeaderLayout();
            UpdateOrderAndLayout();
            UpdateDisplay();
        }

        protected override AutomationPeer OnCreateAutomationPeer()
        {
            return new TimePickerAutomationPeer(this);
        }

        private void OnIsEnabledChanged(object sender, DependencyPropertyChangedEventArgs args)
        {
            UpdateDisplay();
        }

        internal void OpenFlyoutForAutomation()
        {
            OpenFlyout();
        }

        internal bool IsFlyoutOpen => _popup?.IsOpen == true;

        private static string GetDefaultClockIdentifier()
        {
            var pattern = CultureInfo.CurrentCulture.DateTimeFormat.ShortTimePattern;
            return IndexOfUnquoted(pattern, 'H') >= 0
                ? TwentyFourHourClockIdentifier
                : TwelveHourClockIdentifier;
        }

        private static bool IsValidClockIdentifier(object value)
        {
            var clockIdentifier = value as string;
            return string.Equals(clockIdentifier, TwelveHourClockIdentifier, StringComparison.Ordinal) ||
                string.Equals(clockIdentifier, TwentyFourHourClockIdentifier, StringComparison.Ordinal);
        }

        private static bool IsValidMinuteIncrement(object value)
        {
            var increment = (int)value;
            return increment >= 0 && increment <= 59;
        }

        private static object ValidateTime(DependencyObject dependencyObject, object baseValue)
        {
            var value = (TimeSpan)baseValue;
            if (value.Ticks == NullTimeSentinelTicks)
            {
                return value;
            }

            if (value < TimeSpan.Zero)
            {
                throw new ArgumentOutOfRangeException(nameof(value), "Time must be non-negative or the null sentinel value.");
            }

            return value;
        }

        private TimeSpan NormalizeTimeValue(TimeSpan value)
        {
            if (value.Ticks == NullTimeSentinelTicks)
            {
                return value;
            }

            var ticks = value.Ticks % TimeSpan.TicksPerDay;
            var time = TimeSpan.FromTicks(ticks);
            var increment = MinuteIncrement == 0 ? 60 : MinuteIncrement;
            var minute = time.Minutes - (time.Minutes % increment);
            return new TimeSpan(time.Hours, minute, 0);
        }

        private static void OnTimePropertyChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs args)
        {
            ((TimePicker)dependencyObject).OnTimeChanged((TimeSpan)args.OldValue, (TimeSpan)args.NewValue);
        }

        private static void OnSelectedTimePropertyChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs args)
        {
            ((TimePicker)dependencyObject).OnSelectedTimeChanged((TimeSpan?)args.NewValue);
        }

        private static void OnMinuteIncrementPropertyChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs args)
        {
            var timePicker = (TimePicker)dependencyObject;
            var normalizedTime = timePicker.NormalizeTimeValue(timePicker.Time);
            if (normalizedTime != timePicker.Time)
            {
                timePicker.SetCurrentValue(TimeProperty, normalizedTime);
            }
            timePicker.RefreshPickerItemsIfOpen();
            timePicker.UpdateDisplay();
        }

        private static void OnClockIdentifierPropertyChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs args)
        {
            var timePicker = (TimePicker)dependencyObject;
            timePicker.UpdateOrderAndLayout();
            timePicker.RefreshPickerItemsIfOpen();
            timePicker.UpdateDisplay();
        }

        private static void OnHeaderPropertyChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs args)
        {
            var timePicker = (TimePicker)dependencyObject;
            timePicker.UpdateHeaderLayout();
            timePicker.UpdateFlyoutButtonAutomationName();
        }

        private void OnTimeChanged(TimeSpan oldTime, TimeSpan newTime)
        {
            var normalizedTime = NormalizeTimeValue(newTime);
            if (normalizedTime != newTime)
            {
                SetCurrentValue(TimeProperty, normalizedTime);
                return;
            }

            if (!_isSynchronizingTime)
            {
                try
                {
                    _isSynchronizingTime = true;
                    SetCurrentValue(SelectedTimeProperty, ToNullableTime(newTime));
                }
                finally
                {
                    _isSynchronizingTime = false;
                }
            }

            UpdateDisplay();
            TimeChanged?.Invoke(this, new TimePickerValueChangedEventArgs(oldTime, newTime));
            SelectedTimeChanged?.Invoke(
                this,
                new TimePickerSelectedValueChangedEventArgs(ToNullableTime(oldTime), ToNullableTime(newTime)));
        }

        private void OnSelectedTimeChanged(TimeSpan? newTime)
        {
            if (_isSynchronizingTime)
            {
                return;
            }

            try
            {
                _isSynchronizingTime = true;
                SetCurrentValue(TimeProperty, newTime ?? NullTimeSentinel);
            }
            finally
            {
                _isSynchronizingTime = false;
            }
        }

        private static TimeSpan? ToNullableTime(TimeSpan value)
        {
            return value.Ticks == NullTimeSentinelTicks ? (TimeSpan?)null : value;
        }

        private void HookTemplateParts()
        {
            if (_flyoutButton != null)
            {
                _flyoutButton.Click += OnFlyoutButtonClick;
            }

            if (_popup != null)
            {
                _popup.Closed += OnPopupClosed;
                if (_popup.Child is UIElement popupChild)
                {
                    popupChild.PreviewKeyDown += OnPreviewKeyDown;
                }
            }

            if (_acceptButton != null)
            {
                _acceptButton.Click += OnAcceptButtonClick;
            }

            if (_dismissButton != null)
            {
                _dismissButton.Click += OnDismissButtonClick;
            }
        }

        private void UnhookTemplateParts()
        {
            if (_flyoutButton != null)
            {
                _flyoutButton.Click -= OnFlyoutButtonClick;
            }

            if (_popup != null)
            {
                if (_popup.Child is UIElement popupChild)
                {
                    popupChild.PreviewKeyDown -= OnPreviewKeyDown;
                }

                _popup.Closed -= OnPopupClosed;
                _popup.IsOpen = false;
            }

            if (_acceptButton != null)
            {
                _acceptButton.Click -= OnAcceptButtonClick;
            }

            if (_dismissButton != null)
            {
                _dismissButton.Click -= OnDismissButtonClick;
            }
        }

        private void OnFlyoutButtonClick(object sender, RoutedEventArgs args)
        {
            OpenFlyout();
        }

        private void ApplyLocalizedAutomationNames()
        {
            if (_hourPicker != null)
            {
                AutomationProperties.SetName(_hourPicker, ResourceAccessor.GetLocalizedStringResource(SR_TimePickerHourSelectorName));
            }

            if (_minutePicker != null)
            {
                AutomationProperties.SetName(_minutePicker, ResourceAccessor.GetLocalizedStringResource(SR_TimePickerMinuteSelectorName));
            }

            if (_periodPicker != null)
            {
                AutomationProperties.SetName(_periodPicker, ResourceAccessor.GetLocalizedStringResource(SR_TimePickerPeriodSelectorName));
            }

            if (_acceptButton != null)
            {
                AutomationProperties.SetName(_acceptButton, ResourceAccessor.GetLocalizedStringResource(SR_TimePickerAcceptButtonName));
            }

            if (_dismissButton != null)
            {
                AutomationProperties.SetName(_dismissButton, ResourceAccessor.GetLocalizedStringResource(SR_TimePickerDismissButtonName));
            }
        }

        private void OpenFlyout()
        {
            if (_popup == null || _popup.IsOpen || !IsEnabled)
            {
                return;
            }

            PopulatePickerItems();
            _popup.IsOpen = true;
            Dispatcher.BeginInvoke(new Action(() => _hourPicker?.Focus()));
        }

        private void CloseFlyout(bool accept)
        {
            if (_popup == null || !_popup.IsOpen || _isClosingPopup)
            {
                return;
            }

            _isClosingPopup = true;
            try
            {
                if (accept)
                {
                    SetCurrentValue(SelectedTimeProperty, GetPickerTime());
                }

                _popup.IsOpen = false;
            }
            finally
            {
                _isClosingPopup = false;
            }

            _flyoutButton?.Focus();
        }

        private void OnAcceptButtonClick(object sender, RoutedEventArgs args)
        {
            CloseFlyout(true);
        }

        private void OnDismissButtonClick(object sender, RoutedEventArgs args)
        {
            CloseFlyout(false);
        }

        private void OnPopupClosed(object sender, EventArgs args)
        {
            _flyoutButton?.Focus();
        }

        private void OnPreviewKeyDown(object sender, KeyEventArgs args)
        {
            var key = args.Key == Key.System ? args.SystemKey : args.Key;
            if ((Keyboard.Modifiers & ModifierKeys.Alt) != 0 && (key == Key.Down || key == Key.Up))
            {
                if (_popup?.IsOpen == true)
                {
                    CloseFlyout(true);
                }
                else
                {
                    OpenFlyout();
                }

                args.Handled = true;
                return;
            }

            if (_popup?.IsOpen == true)
            {
                if (key == Key.Escape)
                {
                    CloseFlyout(false);
                    args.Handled = true;
                }
                else if (key == Key.Enter)
                {
                    CloseFlyout(true);
                    args.Handled = true;
                }
            }
        }

        private void PopulatePickerItems()
        {
            if (_hourPicker == null || _minutePicker == null || _periodPicker == null)
            {
                return;
            }

            var culture = CultureInfo.CurrentCulture;
            var isTwelveHour = IsTwelveHourClock;
            var hours = new List<string>();
            if (isTwelveHour)
            {
                hours.Add(12.ToString(culture));
                for (var hour = 1; hour < 12; hour++)
                {
                    hours.Add(hour.ToString(culture));
                }
            }
            else
            {
                for (var hour = 0; hour < 24; hour++)
                {
                    hours.Add(hour.ToString(culture));
                }
            }

            var increment = MinuteIncrement == 0 ? 60 : MinuteIncrement;
            var minutes = new List<string>();
            for (var minute = 0; minute < 60; minute += increment)
            {
                minutes.Add(minute.ToString("00", culture));
            }

            var am = GetPeriodDesignator(false);
            var pm = GetPeriodDesignator(true);
            _hourPicker.ItemsSource = hours;
            _minutePicker.ItemsSource = minutes;
            _periodPicker.ItemsSource = new[] { am, pm };

            var time = SelectedTime ?? DateTime.Now.TimeOfDay;
            time = NormalizeTimeValue(time);
            _hourPicker.SelectedIndex = isTwelveHour ? time.Hours % 12 : time.Hours;
            _minutePicker.SelectedIndex = time.Minutes / increment;
            _periodPicker.SelectedIndex = time.Hours < 12 ? 0 : 1;
            _periodPicker.Visibility = isTwelveHour ? Visibility.Visible : Visibility.Collapsed;
        }

        private TimeSpan GetPickerTime()
        {
            var increment = MinuteIncrement == 0 ? 60 : MinuteIncrement;
            var minute = Math.Max(0, _minutePicker?.SelectedIndex ?? 0) * increment;
            var hourIndex = Math.Max(0, _hourPicker?.SelectedIndex ?? 0);
            int hour;

            if (IsTwelveHourClock)
            {
                var isPm = (_periodPicker?.SelectedIndex ?? 0) == 1;
                hour = hourIndex == 0 ? 0 : hourIndex;
                if (isPm)
                {
                    hour += 12;
                }
            }
            else
            {
                hour = hourIndex;
            }

            return new TimeSpan(hour, minute, 0);
        }

        private void RefreshPickerItemsIfOpen()
        {
            if (_popup?.IsOpen == true)
            {
                PopulatePickerItems();
            }
        }

        private bool IsTwelveHourClock => string.Equals(ClockIdentifier, TwelveHourClockIdentifier, StringComparison.Ordinal);

        private void UpdateDisplay()
        {
            if (_hourTextBlock == null || _minuteTextBlock == null || _periodTextBlock == null)
            {
                return;
            }

            if (SelectedTime is TimeSpan selectedTime)
            {
                _hourTextBlock.Text = IsTwelveHourClock
                    ? (selectedTime.Hours % 12 == 0 ? 12 : selectedTime.Hours % 12).ToString(CultureInfo.CurrentCulture)
                    : selectedTime.Hours.ToString(CultureInfo.CurrentCulture);
                _minuteTextBlock.Text = selectedTime.Minutes.ToString("00", CultureInfo.CurrentCulture);
                _periodTextBlock.Text = GetPeriodDesignator(selectedTime.Hours >= 12);
            }
            else
            {
                _hourTextBlock.Text = ResourceAccessor.GetLocalizedStringResource(SR_TimePickerHourPlaceholder);
                _minuteTextBlock.Text = ResourceAccessor.GetLocalizedStringResource(SR_TimePickerMinutePlaceholder);
                _periodTextBlock.Text = GetPeriodDesignator(false);
            }

            _periodTextBlock.Visibility = IsTwelveHourClock ? Visibility.Visible : Visibility.Collapsed;
            UpdateFlyoutButtonAutomationName();
            VisualStateManager.GoToState(this, SelectedTime.HasValue ? "HasTime" : "HasNoTime", false);
            VisualStateManager.GoToState(this, IsEnabled ? "Normal" : "Disabled", false);
        }

        private string GetPeriodDesignator(bool isPm)
        {
            var designator = isPm
                ? CultureInfo.CurrentCulture.DateTimeFormat.PMDesignator
                : CultureInfo.CurrentCulture.DateTimeFormat.AMDesignator;
            if (!string.IsNullOrEmpty(designator))
            {
                return designator;
            }

            return isPm ? "PM" : "AM";
        }

        private string GetSelectedTimeAsString()
        {
            if (!(SelectedTime is TimeSpan selectedTime))
            {
                return string.Empty;
            }

            return DateTime.Today
                .Add(selectedTime)
                .ToString(GetSelectedTimeFormatPattern(), CultureInfo.CurrentCulture)
                .Trim();
        }

        private string GetSelectedTimeFormatPattern()
        {
            var sourcePattern = CultureInfo.CurrentCulture.DateTimeFormat.ShortTimePattern;
            var pattern = new List<char>(sourcePattern.Length + 3);
            var quote = '\0';
            var escaped = false;
            var hasPeriod = false;

            foreach (var character in sourcePattern)
            {
                if (escaped)
                {
                    pattern.Add(character);
                    escaped = false;
                    continue;
                }

                if (character == '\\')
                {
                    pattern.Add(character);
                    escaped = true;
                    continue;
                }

                if (character == '\'' || character == '"')
                {
                    quote = quote == '\0' ? character : quote == character ? '\0' : quote;
                    pattern.Add(character);
                    continue;
                }

                if (quote == '\0' && (character == 'h' || character == 'H'))
                {
                    pattern.Add(IsTwelveHourClock ? 'h' : 'H');
                }
                else if (quote == '\0' && character == 't')
                {
                    if (IsTwelveHourClock)
                    {
                        pattern.Add(character);
                        hasPeriod = true;
                    }
                }
                else
                {
                    pattern.Add(character);
                }
            }

            var normalizedPattern = new string(pattern.ToArray()).Trim(' ', '\u00A0', '\u202F', ',', '\u060C');
            return IsTwelveHourClock && !hasPeriod
                ? normalizedPattern + " tt"
                : normalizedPattern;
        }

        private void UpdateFlyoutButtonAutomationName()
        {
            if (_flyoutButton == null)
            {
                return;
            }

            var parentName = AutomationProperties.GetName(this);
            if (string.IsNullOrEmpty(parentName))
            {
                parentName = Header?.ToString();
            }

            var selectedValue = GetSelectedTimeAsString();
            var format = ResourceAccessor.GetLocalizedStringResource(SR_TimePickerFlyoutButtonAutomationName);
            var automationName = string.Format(
                CultureInfo.CurrentCulture,
                format,
                parentName ?? string.Empty,
                selectedValue);
            AutomationProperties.SetName(
                _flyoutButton,
                string.Join(" ", automationName.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)));
        }

        private void UpdateHeaderLayout()
        {
            if (_headerPresenter == null || _layoutRoot == null || _headerRow == null || _buttonRow == null ||
                _headerColumn == null || _buttonColumn == null || _flyoutButton == null)
            {
                return;
            }

            var hasHeader = Header != null;
            _headerPresenter.Visibility = hasHeader ? Visibility.Visible : Visibility.Collapsed;

            if (HeaderPlacement == ControlHeaderPlacement.Left && hasHeader)
            {
                _headerRow.Height = GridLength.Auto;
                _buttonRow.Height = new GridLength(0);
                _headerColumn.Width = GridLength.Auto;
                _buttonColumn.Width = new GridLength(1, GridUnitType.Star);
                Grid.SetRow(_headerPresenter, 0);
                Grid.SetColumn(_headerPresenter, 0);
                Grid.SetRow(_flyoutButton, 0);
                Grid.SetColumn(_flyoutButton, 1);
                _headerPresenter.Margin = new Thickness(0, 0, 8, 0);
            }
            else
            {
                _headerRow.Height = hasHeader ? GridLength.Auto : new GridLength(0);
                _buttonRow.Height = GridLength.Auto;
                _headerColumn.Width = new GridLength(1, GridUnitType.Star);
                _buttonColumn.Width = new GridLength(0);
                Grid.SetRow(_headerPresenter, 0);
                Grid.SetColumn(_headerPresenter, 0);
                Grid.SetRow(_flyoutButton, 1);
                Grid.SetColumn(_flyoutButton, 0);
                _headerPresenter.Margin = hasHeader ? new Thickness(0, 0, 0, 4) : new Thickness(0);
            }
        }

        private void UpdateOrderAndLayout()
        {
            if (_hourHost == null || _minuteHost == null || _periodHost == null ||
                _firstTextBlockColumn == null || _secondTextBlockColumn == null || _thirdTextBlockColumn == null)
            {
                return;
            }

            var order = GetTimeFieldOrder();
            for (var index = 0; index < order.Count; index++)
            {
                var column = index * 2;
                var field = order[index];
                var displayHost = field == 'h' ? _hourHost : field == 'm' ? _minuteHost : _periodHost;
                Grid.SetColumn(displayHost, column);

                var picker = field == 'h' ? (FrameworkElement)_hourPicker : field == 'm' ? _minutePicker : _periodPicker;
                if (picker != null)
                {
                    Grid.SetColumn(picker, column);
                }
            }

            var count = order.Count;
            _firstTextBlockColumn.Width = count > 0 ? new GridLength(1, GridUnitType.Star) : new GridLength(0);
            _secondTextBlockColumn.Width = count > 1 ? new GridLength(1, GridUnitType.Star) : new GridLength(0);
            _thirdTextBlockColumn.Width = count > 2 ? new GridLength(1, GridUnitType.Star) : new GridLength(0);
            _periodHost.Visibility = IsTwelveHourClock ? Visibility.Visible : Visibility.Collapsed;
            _firstColumnDivider.Visibility = count > 1 ? Visibility.Visible : Visibility.Collapsed;
            _secondColumnDivider.Visibility = count > 2 ? Visibility.Visible : Visibility.Collapsed;
            if (_periodPicker != null)
            {
                _periodPicker.Visibility = IsTwelveHourClock ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        private IReadOnlyList<char> GetTimeFieldOrder()
        {
            var pattern = CultureInfo.CurrentCulture.DateTimeFormat.ShortTimePattern;
            var fields = new List<Tuple<int, char>>
            {
                Tuple.Create(IndexOfUnquoted(pattern, 'h', 'H'), 'h'),
                Tuple.Create(IndexOfUnquoted(pattern, 'm'), 'm')
            };

            if (IsTwelveHourClock)
            {
                fields.Add(Tuple.Create(IndexOfUnquoted(pattern, 't'), 't'));
            }

            return fields
                .OrderBy(field => field.Item1 < 0 ? int.MaxValue : field.Item1)
                .Select(field => field.Item2)
                .ToArray();
        }

        private static int IndexOfUnquoted(string pattern, params char[] candidates)
        {
            var inQuote = false;
            for (var index = 0; index < pattern.Length; index++)
            {
                var character = pattern[index];
                if (character == '\'' || character == '"')
                {
                    inQuote = !inQuote;
                }
                else if (!inQuote && candidates.Contains(character))
                {
                    return index;
                }
            }

            return -1;
        }
    }
}
