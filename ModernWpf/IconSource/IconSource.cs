// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace ModernWpf.Controls
{
    /// <summary>
    /// Represents the base class for an icon source.
    /// </summary>
    public abstract class IconSource : DependencyObject
    {
        /// <summary>
        /// Identifies the <see cref="Foreground"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ForegroundProperty =
            DependencyProperty.Register(
                nameof(Foreground),
                typeof(Brush),
                typeof(IconSource));

        /// <summary>
        /// Gets or sets a brush that describes the foreground color.
        /// </summary>
        /// <returns>
        /// The brush that paints the foreground of the control. The default is <see langword="null"/>, (a null brush) which is
        /// evaluated as Transparent for rendering.
        /// </returns>
        public Brush Foreground
        {
            get => (Brush)GetValue(ForegroundProperty);
            set => SetValue(ForegroundProperty, value);
        }

        public IconElement CreateIconElement()
        {
            var element = CreateIconElementCore();
            m_createdIconElements.Add(new WeakReference<IconElement>(element));
            return element;
        }

        protected abstract IconElement CreateIconElementCore();

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs args)
        {
            base.OnPropertyChanged(args);

            var iconProp = GetIconElementPropertyCore(args.Property);
            if (iconProp != null)
            {
                m_createdIconElements.RemoveAll(weakElement =>
                {
                    if (weakElement.TryGetTarget(out var element))
                    {
                        element.SetValue(iconProp, args.NewValue);
                        return false;
                    }
                    return true;
                });
            }
        }

        protected virtual DependencyProperty GetIconElementPropertyCore(DependencyProperty sourceProperty)
        {
            if (sourceProperty == ForegroundProperty)
            {
                return IconElement.ForegroundProperty;
            }

            return null;
        }

        private readonly List<WeakReference<IconElement>> m_createdIconElements = new();
    }
}
