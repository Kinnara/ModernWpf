// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Windows;

namespace ModernWpf.Controls
{
    public abstract class LayoutContext : DependencyObject
    {
        protected internal LayoutContext()
        {
        }

        public object LayoutState
        {
            get => LayoutStateCore;
            set => LayoutStateCore = value;
        }

        protected internal abstract object LayoutStateCore { get; set; }
    }
}