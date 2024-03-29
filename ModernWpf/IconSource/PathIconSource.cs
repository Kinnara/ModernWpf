﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Windows;
using System.Windows.Media;

namespace ModernWpf.Controls
{
    /// <summary>
    /// Represents an icon source that uses a vector path as its content.
    /// </summary>
    public class PathIconSource : IconSource
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PathIconSource"/> class.
        /// </summary>
        public PathIconSource()
        {
        }

        /// <summary>
        /// Identifies the <see cref="Data"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty DataProperty =
            DependencyProperty.Register(
                nameof(Data),
                typeof(Geometry),
                typeof(PathIconSource));

        /// <summary>
        /// Gets or sets a <see cref="Geometry"/>.
        /// </summary>
        /// <returns>
        /// A description of the shape to be drawn.
        /// </returns>
        public Geometry Data
        {
            get => (Geometry)GetValue(DataProperty);
            set => SetValue(DataProperty, value);
        }

        protected override IconElement CreateIconElementCore()
        {
            PathIcon pathIcon = new();

            if (Data is { } data)
            {
                pathIcon.Data = data;
            }
            if (Foreground is { } newForeground)
            {
                pathIcon.Foreground = newForeground;
            }
            return pathIcon;
        }

        protected override DependencyProperty GetIconElementPropertyCore(DependencyProperty sourceProperty)
        {
            if (sourceProperty == DataProperty)
            {
                return PathIcon.DataProperty;
            }

            return base.GetIconElementPropertyCore(sourceProperty);
        }
    }
}
