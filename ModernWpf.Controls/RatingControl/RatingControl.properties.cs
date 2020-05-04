// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Windows;
using ModernWpf.Controls.Primitives;

namespace ModernWpf.Controls
{
    partial class RatingControl
    {
        #region Caption

        public static readonly DependencyProperty CaptionProperty =
            DependencyProperty.Register(
                nameof(Caption),
                typeof(string),
                typeof(RatingControl),
                new PropertyMetadata(string.Empty, OnCaptionPropertyChanged));

        public string Caption
        {
            get => (string)GetValue(CaptionProperty);
            set => SetValue(CaptionProperty, value);
        }

        private static void OnCaptionPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            ((RatingControl)sender).PrivateOnPropertyChanged(args);
        }

        #endregion

        #region InitialSetValue

        public static readonly DependencyProperty InitialSetValueProperty =
            DependencyProperty.Register(
                nameof(InitialSetValue),
                typeof(int),
                typeof(RatingControl),
                new PropertyMetadata(1, OnInitialSetValuePropertyChanged));

        public int InitialSetValue
        {
            get => (int)GetValue(InitialSetValueProperty);
            set => SetValue(InitialSetValueProperty, value);
        }

        private static void OnInitialSetValuePropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            ((RatingControl)sender).PrivateOnPropertyChanged(args);
        }

        #endregion

        #region IsClearEnabled

        public static readonly DependencyProperty IsClearEnabledProperty =
            DependencyProperty.Register(
                nameof(IsClearEnabled),
                typeof(bool),
                typeof(RatingControl),
                new PropertyMetadata(true, OnIsClearEnabledPropertyChanged));

        public bool IsClearEnabled
        {
            get => (bool)GetValue(IsClearEnabledProperty);
            set => SetValue(IsClearEnabledProperty, value);
        }

        private static void OnIsClearEnabledPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            ((RatingControl)sender).PrivateOnPropertyChanged(args);
        }

        #endregion

        #region IsReadOnly

        public static readonly DependencyProperty IsReadOnlyProperty =
            DependencyProperty.Register(
                nameof(IsReadOnly),
                typeof(bool),
                typeof(RatingControl),
                new PropertyMetadata(false, OnIsReadOnlyPropertyChanged));

        public bool IsReadOnly
        {
            get => (bool)GetValue(IsReadOnlyProperty);
            set => SetValue(IsReadOnlyProperty, value);
        }

        private static void OnIsReadOnlyPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            ((RatingControl)sender).PrivateOnPropertyChanged(args);
        }

        #endregion

        #region ItemInfo

        public static readonly DependencyProperty ItemInfoProperty =
            DependencyProperty.Register(
                nameof(ItemInfo),
                typeof(RatingItemInfo),
                typeof(RatingControl),
                new PropertyMetadata(OnItemInfoPropertyChanged));

        public RatingItemInfo ItemInfo
        {
            get => (RatingItemInfo)GetValue(ItemInfoProperty);
            set => SetValue(ItemInfoProperty, value);
        }

        private static void OnItemInfoPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            ((RatingControl)sender).PrivateOnPropertyChanged(args);
        }

        #endregion

        #region MaxRating

        public static readonly DependencyProperty MaxRatingProperty =
            DependencyProperty.Register(
                nameof(MaxRating),
                typeof(int),
                typeof(RatingControl),
                new PropertyMetadata(5, OnMaxRatingPropertyChanged));

        public int MaxRating
        {
            get => (int)GetValue(MaxRatingProperty);
            set => SetValue(MaxRatingProperty, value);
        }

        private static void OnMaxRatingPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            ((RatingControl)sender).PrivateOnPropertyChanged(args);
        }

        #endregion

        #region PlaceholderValue

        public static readonly DependencyProperty PlaceholderValueProperty =
            DependencyProperty.Register(
                nameof(PlaceholderValue),
                typeof(double),
                typeof(RatingControl),
                new PropertyMetadata(-1d, OnPlaceholderValuePropertyChanged));

        public double PlaceholderValue
        {
            get => (double)GetValue(PlaceholderValueProperty);
            set => SetValue(PlaceholderValueProperty, value);
        }

        private static void OnPlaceholderValuePropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            ((RatingControl)sender).PrivateOnPropertyChanged(args);
        }

        #endregion

        #region Value

        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register(
                nameof(Value),
                typeof(double),
                typeof(RatingControl),
                new PropertyMetadata(-1d, OnValuePropertyChanged));

        public double Value
        {
            get => (double)GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
        }

        private static void OnValuePropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            ((RatingControl)sender).PrivateOnPropertyChanged(args);
        }

        #endregion

        #region UseSystemFocusVisuals

        public static readonly DependencyProperty UseSystemFocusVisualsProperty =
            FocusVisualHelper.UseSystemFocusVisualsProperty.AddOwner(typeof(RatingControl));

        public bool UseSystemFocusVisuals
        {
            get => (bool)GetValue(UseSystemFocusVisualsProperty);
            set => SetValue(UseSystemFocusVisualsProperty, value);
        }

        #endregion

        public event TypedEventHandler<RatingControl, object> ValueChanged;
    }
}
