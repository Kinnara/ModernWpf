// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Windows;
using System.Windows.Controls;

namespace ModernWpf.Tests.MUXControls.ApiTests.RepeaterTests.Common
{
    using ModernWpf.Controls;
    using ElementFactory = ModernWpf.Controls.ElementFactory;

    class DataAsElementElementFactory : ElementFactory
    {
        protected override UIElement GetElementCore(ElementFactoryGetArgs args)
        {
            return args.Data as UIElement;
        }

        protected override void RecycleElementCore(ElementFactoryRecycleArgs args)
        {
        }
    }
}
