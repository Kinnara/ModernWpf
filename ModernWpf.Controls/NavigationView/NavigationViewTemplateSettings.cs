// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Windows;

namespace ModernWpf.Controls
{
    public class NavigationViewTemplateSettings : DependencyObject
    {
        public NavigationViewTemplateSettings()
        {
        }

        #region TopPadding

        private static readonly DependencyPropertyKey TopPaddingPropertyKey =
            DependencyProperty.RegisterReadOnly(
                nameof(TopPadding),
                typeof(double),
                typeof(NavigationViewTemplateSettings),
                new PropertyMetadata(0.0));

        public static readonly DependencyProperty TopPaddingProperty =
            TopPaddingPropertyKey.DependencyProperty;

        public double TopPadding
        {
            get => (double)GetValue(TopPaddingProperty);
            internal set => SetValue(TopPaddingPropertyKey, value);
        }

        #endregion

        #region OverflowButtonVisibility

        private static readonly DependencyPropertyKey OverflowButtonVisibilityPropertyKey =
            DependencyProperty.RegisterReadOnly(
                nameof(OverflowButtonVisibility),
                typeof(Visibility),
                typeof(NavigationViewTemplateSettings),
                new PropertyMetadata(Visibility.Collapsed));

        public static readonly DependencyProperty OverflowButtonVisibilityProperty =
            OverflowButtonVisibilityPropertyKey.DependencyProperty;

        public Visibility OverflowButtonVisibility
        {
            get => (Visibility)GetValue(OverflowButtonVisibilityProperty);
            internal set => SetValue(OverflowButtonVisibilityPropertyKey, value);
        }

        #endregion

        #region PaneToggleButtonVisibility

        private static readonly DependencyPropertyKey PaneToggleButtonVisibilityPropertyKey =
            DependencyProperty.RegisterReadOnly(
                nameof(PaneToggleButtonVisibility),
                typeof(Visibility),
                typeof(NavigationViewTemplateSettings),
                new PropertyMetadata(Visibility.Visible));

        public static readonly DependencyProperty PaneToggleButtonVisibilityProperty =
            PaneToggleButtonVisibilityPropertyKey.DependencyProperty;

        public Visibility PaneToggleButtonVisibility
        {
            get => (Visibility)GetValue(PaneToggleButtonVisibilityProperty);
            internal set => SetValue(PaneToggleButtonVisibilityPropertyKey, value);
        }

        #endregion

        #region BackButtonVisibility

        private static readonly DependencyPropertyKey BackButtonVisibilityPropertyKey =
            DependencyProperty.RegisterReadOnly(
                nameof(BackButtonVisibility),
                typeof(Visibility),
                typeof(NavigationViewTemplateSettings),
                new PropertyMetadata(Visibility.Collapsed));

        public static readonly DependencyProperty BackButtonVisibilityProperty =
            BackButtonVisibilityPropertyKey.DependencyProperty;

        public Visibility BackButtonVisibility
        {
            get => (Visibility)GetValue(BackButtonVisibilityProperty);
            internal set => SetValue(BackButtonVisibilityPropertyKey, value);
        }

        #endregion

        #region TopPaneVisibility

        private static readonly DependencyPropertyKey TopPaneVisibilityPropertyKey =
            DependencyProperty.RegisterReadOnly(
                nameof(TopPaneVisibility),
                typeof(Visibility),
                typeof(NavigationViewTemplateSettings),
                new PropertyMetadata(Visibility.Collapsed));

        public static readonly DependencyProperty TopPaneVisibilityProperty =
            TopPaneVisibilityPropertyKey.DependencyProperty;

        public Visibility TopPaneVisibility
        {
            get => (Visibility)GetValue(TopPaneVisibilityProperty);
            internal set => SetValue(TopPaneVisibilityPropertyKey, value);
        }

        #endregion

        #region LeftPaneVisibility

        private static readonly DependencyPropertyKey LeftPaneVisibilityPropertyKey =
            DependencyProperty.RegisterReadOnly(
                nameof(LeftPaneVisibility),
                typeof(Visibility),
                typeof(NavigationViewTemplateSettings),
                new PropertyMetadata(Visibility.Visible));

        public static readonly DependencyProperty LeftPaneVisibilityProperty =
            LeftPaneVisibilityPropertyKey.DependencyProperty;

        public Visibility LeftPaneVisibility
        {
            get => (Visibility)GetValue(LeftPaneVisibilityProperty);
            internal set => SetValue(LeftPaneVisibilityPropertyKey, value);
        }

        #endregion

        #region SingleSelectionFollowsFocus

        private static readonly DependencyPropertyKey SingleSelectionFollowsFocusPropertyKey =
            DependencyProperty.RegisterReadOnly(
                nameof(SingleSelectionFollowsFocus),
                typeof(bool),
                typeof(NavigationViewTemplateSettings),
                null);

        public static readonly DependencyProperty SingleSelectionFollowsFocusProperty =
            SingleSelectionFollowsFocusPropertyKey.DependencyProperty;

        public bool SingleSelectionFollowsFocus
        {
            get => (bool)GetValue(SingleSelectionFollowsFocusProperty);
            internal set => SetValue(SingleSelectionFollowsFocusPropertyKey, value);
        }

        #endregion

        #region LatestPaneButtonWidth

        private static readonly DependencyPropertyKey LatestPaneButtonWidthPropertyKey =
            DependencyProperty.RegisterReadOnly(
                nameof(LatestPaneButtonWidth),
                typeof(double),
                typeof(NavigationViewTemplateSettings),
                null);

        public static readonly DependencyProperty LatestPaneButtonWidthProperty =
            LatestPaneButtonWidthPropertyKey.DependencyProperty;

        public double LatestPaneButtonWidth
        {
            get => (double)GetValue(LatestPaneButtonWidthProperty);
            internal set => SetValue(LatestPaneButtonWidthPropertyKey, value);
        }

        #endregion

        #region PaneButtonWidth

        private static readonly DependencyPropertyKey PaneButtonWidthPropertyKey =
            DependencyProperty.RegisterReadOnly(
                nameof(PaneButtonWidth),
                typeof(double),
                typeof(NavigationViewTemplateSettings),
                null);

        public static readonly DependencyProperty PaneButtonWidthProperty =
            PaneButtonWidthPropertyKey.DependencyProperty;

        public double PaneButtonWidth
        {
            get => (double)GetValue(PaneButtonWidthProperty);
            internal set => SetValue(PaneButtonWidthPropertyKey, value);
        }

        #endregion
    }
}
