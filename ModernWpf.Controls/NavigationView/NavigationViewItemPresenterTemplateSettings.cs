// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Windows;

namespace ModernWpf.Controls.Primitives
{
    public class NavigationViewItemPresenterTemplateSettings : DependencyObject
    {
        public NavigationViewItemPresenterTemplateSettings()
        {                
        }

        #region IconColumnWidth

        private static readonly DependencyPropertyKey IconColumnWidthPropertyKey =
            DependencyProperty.RegisterReadOnly(
                nameof(IconColumnWidth),
                typeof(double),
                typeof(NavigationViewItemPresenterTemplateSettings),
                null);

        public static readonly DependencyProperty IconColumnWidthProperty =
            IconColumnWidthPropertyKey.DependencyProperty;

        public double IconColumnWidth
        {
            get => (double)GetValue(IconColumnWidthProperty);
            internal set => SetValue(IconColumnWidthPropertyKey, value);
        }

        #endregion

        #region LatestIconColumnWidth

        private static readonly DependencyPropertyKey LatestIconColumnWidthPropertyKey =
            DependencyProperty.RegisterReadOnly(
                nameof(LatestIconColumnWidth),
                typeof(double),
                typeof(NavigationViewItemPresenterTemplateSettings),
                null);

        public static readonly DependencyProperty LatestIconColumnWidthProperty =
            LatestIconColumnWidthPropertyKey.DependencyProperty;

        public double LatestIconColumnWidth
        {
            get => (double)GetValue(LatestIconColumnWidthProperty);
            internal set => SetValue(LatestIconColumnWidthPropertyKey, value);
        }

        #endregion
    }
}