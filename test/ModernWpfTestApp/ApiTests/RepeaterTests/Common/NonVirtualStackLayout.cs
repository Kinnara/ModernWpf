// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Windows;

using NonVirtualizingLayoutContext = ModernWpf.Controls.NonVirtualizingLayoutContext;
using NonVirtualizingLayout = ModernWpf.Controls.NonVirtualizingLayout;

namespace ModernWpf.Tests.MUXControls.ApiTests.RepeaterTests.Common
{
    public class NonVirtualStackLayout : NonVirtualizingLayout
    {
        protected override Size MeasureOverride(NonVirtualizingLayoutContext context, Size availableSize)
        {
            double extentHeight = 0.0;
            double extentWidth = 0.0;
            foreach (var element in context.Children)
            {
                element.Measure(availableSize);
                extentHeight += element.DesiredSize.Height;
                extentWidth = Math.Max(extentWidth, element.DesiredSize.Width);
            }

            return new Size(extentWidth, extentHeight);
        }

        protected override Size ArrangeOverride(NonVirtualizingLayoutContext context, Size finalSize)
        {
            double offset = 0.0;
            foreach (var element in context.Children)
            {
                element.Arrange(new Rect(0, offset, element.DesiredSize.Width, element.DesiredSize.Height));
                offset += element.DesiredSize.Height;
            }

            return finalSize;
        }
    }
}