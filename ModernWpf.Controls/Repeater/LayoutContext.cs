// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Windows;

namespace ModernWpf.Controls
{
    public class LayoutContext : DependencyObject, ILayoutContextOverrides
    {
        internal LayoutContext()
        {
        }

        public object LayoutState
        {
            get => LayoutStateCore;
            set => LayoutStateCore = value;
        }

        protected virtual object LayoutStateCore
        {
            get => throw new NotImplementedException();
            set => throw new NotImplementedException();
        }

        object ILayoutContextOverrides.LayoutStateCore
        {
            get => LayoutStateCore;
            set => LayoutStateCore = value;
        }
    }

    internal interface ILayoutContextOverrides
    {
        object LayoutStateCore { get; set; }
    }
}