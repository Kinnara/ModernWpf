// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace ModernWpf.Controls
{
    internal class CachedVisualTreeHelpers
    {
        public static Rect GetLayoutSlot(FrameworkElement element)
        {
            return LayoutInformation.GetLayoutSlot(element);
        }

        public static DependencyObject GetParent(DependencyObject element)
        {
            return VisualTreeHelper.GetParent(element);
        }
    }
}
