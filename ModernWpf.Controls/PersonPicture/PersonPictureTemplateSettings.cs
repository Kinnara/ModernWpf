// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Windows;
using System.Windows.Media;

namespace ModernWpf.Controls
{
    public sealed class PersonPictureTemplateSettings : DependencyObject
    {
        #region ActualImageBrush

        private static readonly DependencyPropertyKey ActualImageBrushPropertyKey =
            DependencyProperty.RegisterReadOnly(
                nameof(ActualImageBrush),
                typeof(ImageBrush),
                typeof(PersonPictureTemplateSettings),
                null);

        public static readonly DependencyProperty ActualImageBrushProperty =
            ActualImageBrushPropertyKey.DependencyProperty;

        public ImageBrush ActualImageBrush
        {
            get => (ImageBrush)GetValue(ActualImageBrushProperty);
            internal set => SetValue(ActualImageBrushPropertyKey, value);
        }

        #endregion

        #region ActualInitials

        private static readonly DependencyPropertyKey ActualInitialsPropertyKey =
            DependencyProperty.RegisterReadOnly(
                nameof(ActualInitials),
                typeof(string),
                typeof(PersonPictureTemplateSettings),
                new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty ActualInitialsProperty =
            ActualInitialsPropertyKey.DependencyProperty;

        public string ActualInitials
        {
            get => (string)GetValue(ActualInitialsProperty);
            internal set => SetValue(ActualInitialsPropertyKey, value);
        }

        #endregion
    }
}
