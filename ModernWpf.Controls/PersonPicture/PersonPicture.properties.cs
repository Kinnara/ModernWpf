// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Windows;
using System.Windows.Media;

namespace ModernWpf.Controls
{
    partial class PersonPicture
    {
        #region BadgeGlyph

        public static readonly DependencyProperty BadgeGlyphProperty =
            DependencyProperty.Register(
                nameof(BadgeGlyph),
                typeof(string),
                typeof(PersonPicture),
                new PropertyMetadata(string.Empty, OnBadgeGlyphPropertyChanged, CoerceStringProperty));

        public string BadgeGlyph
        {
            get => (string)GetValue(BadgeGlyphProperty);
            set => SetValue(BadgeGlyphProperty, value);
        }

        private static void OnBadgeGlyphPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            var owner = (PersonPicture)sender;
            owner.PrivateOnPropertyChanged(args);
        }

        #endregion

        #region BadgeImageSource

        public static readonly DependencyProperty BadgeImageSourceProperty =
            DependencyProperty.Register(
                nameof(BadgeImageSource),
                typeof(ImageSource),
                typeof(PersonPicture),
                new PropertyMetadata(null, OnBadgeImageSourcePropertyChanged));

        public ImageSource BadgeImageSource
        {
            get => (ImageSource)GetValue(BadgeImageSourceProperty);
            set => SetValue(BadgeImageSourceProperty, value);
        }

        private static void OnBadgeImageSourcePropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            var owner = (PersonPicture)sender;
            owner.PrivateOnPropertyChanged(args);
        }

        #endregion

        #region BadgeNumber

        public static readonly DependencyProperty BadgeNumberProperty =
            DependencyProperty.Register(
                nameof(BadgeNumber),
                typeof(int),
                typeof(PersonPicture),
                new PropertyMetadata(0, OnBadgeNumberPropertyChanged));

        public int BadgeNumber
        {
            get => (int)GetValue(BadgeNumberProperty);
            set => SetValue(BadgeNumberProperty, value);
        }

        private static void OnBadgeNumberPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            var owner = (PersonPicture)sender;
            owner.PrivateOnPropertyChanged(args);
        }

        #endregion

        #region BadgeText

        public static readonly DependencyProperty BadgeTextProperty =
            DependencyProperty.Register(
                nameof(BadgeText),
                typeof(string),
                typeof(PersonPicture),
                new PropertyMetadata(string.Empty, OnBadgeTextPropertyChanged, CoerceStringProperty));

        public string BadgeText
        {
            get => (string)GetValue(BadgeTextProperty);
            set => SetValue(BadgeTextProperty, value);
        }

        private static void OnBadgeTextPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            var owner = (PersonPicture)sender;
            owner.PrivateOnPropertyChanged(args);
        }

        #endregion

        #region DisplayName

        public static readonly DependencyProperty DisplayNameProperty =
            DependencyProperty.Register(
                nameof(DisplayName),
                typeof(string),
                typeof(PersonPicture),
                new PropertyMetadata(string.Empty, OnDisplayNamePropertyChanged, CoerceStringProperty));

        public string DisplayName
        {
            get => (string)GetValue(DisplayNameProperty);
            set => SetValue(DisplayNameProperty, value);
        }

        private static void OnDisplayNamePropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            var owner = (PersonPicture)sender;
            owner.PrivateOnPropertyChanged(args);
        }

        #endregion

        #region Initials

        public static readonly DependencyProperty InitialsProperty =
            DependencyProperty.Register(
                nameof(Initials),
                typeof(string),
                typeof(PersonPicture),
                new PropertyMetadata(string.Empty, OnInitialsPropertyChanged, CoerceStringProperty));

        public string Initials
        {
            get => (string)GetValue(InitialsProperty);
            set => SetValue(InitialsProperty, value);
        }

        private static void OnInitialsPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            var owner = (PersonPicture)sender;
            owner.PrivateOnPropertyChanged(args);
        }

        #endregion

        #region IsGroup

        public static readonly DependencyProperty IsGroupProperty =
            DependencyProperty.Register(
                nameof(IsGroup),
                typeof(bool),
                typeof(PersonPicture),
                new PropertyMetadata(false, OnIsGroupPropertyChanged));

        public bool IsGroup
        {
            get => (bool)GetValue(IsGroupProperty);
            set => SetValue(IsGroupProperty, value);
        }

        private static void OnIsGroupPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            var owner = (PersonPicture)sender;
            owner.PrivateOnPropertyChanged(args);
        }

        #endregion

        #region ProfilePicture

        public static readonly DependencyProperty ProfilePictureProperty =
            DependencyProperty.Register(
                nameof(ProfilePicture),
                typeof(ImageSource),
                typeof(PersonPicture),
                new PropertyMetadata(null, OnProfilePicturePropertyChanged));

        public ImageSource ProfilePicture
        {
            get => (ImageSource)GetValue(ProfilePictureProperty);
            set => SetValue(ProfilePictureProperty, value);
        }

        private static void OnProfilePicturePropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            var owner = (PersonPicture)sender;
            owner.PrivateOnPropertyChanged(args);
        }

        #endregion

        #region TemplateSettings

        private static readonly DependencyPropertyKey TemplateSettingsPropertyKey =
            DependencyProperty.RegisterReadOnly(
                nameof(TemplateSettings),
                typeof(PersonPictureTemplateSettings),
                typeof(PersonPicture),
                new PropertyMetadata(null, OnTemplateSettingsPropertyChanged));

        public static readonly DependencyProperty TemplateSettingsProperty =
            TemplateSettingsPropertyKey.DependencyProperty;

        public PersonPictureTemplateSettings TemplateSettings
        {
            get => (PersonPictureTemplateSettings)GetValue(TemplateSettingsProperty);
            private set => SetValue(TemplateSettingsPropertyKey, value);
        }

        private static void OnTemplateSettingsPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            var owner = (PersonPicture)sender;
            owner.PrivateOnPropertyChanged(args);
        }

        #endregion

        private static object CoerceStringProperty(DependencyObject d, object baseValue)
        {
            return baseValue ?? string.Empty;
        }
    }
}
