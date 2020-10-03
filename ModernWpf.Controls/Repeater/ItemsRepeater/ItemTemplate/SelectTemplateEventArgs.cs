// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Windows;

namespace ModernWpf.Controls
{
    public sealed class SelectTemplateEventArgs : EventArgs
    {
        internal SelectTemplateEventArgs()
        {
        }

        public string TemplateKey { get; set; }

        public object DataContext { get; internal set; }

        public UIElement Owner { get; internal set; }
    }
}
