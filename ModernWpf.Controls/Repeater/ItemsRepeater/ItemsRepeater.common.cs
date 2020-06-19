// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Media.Media3D;

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
            if (element is Visual || element is Visual3D)
            {
                return VisualTreeHelper.GetParent(element);
            }

            if (element is FrameworkContentElement fce)
            {
                return fce.Parent;
            }

            return null;
        }
    }
}
