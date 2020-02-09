// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Windows;

namespace ModernWpf.Controls
{
    public class ElementFactory : DependencyObject, IElementFactoryShim
    {
        public ElementFactory()
        {
        }

        #region IElementFactory

        public UIElement GetElement(ElementFactoryGetArgs args)
        {
            return GetElementCore(args);
        }

        public void RecycleElement(ElementFactoryRecycleArgs args)
        {
            RecycleElementCore(args);
        }

        #endregion

        protected virtual UIElement GetElementCore(ElementFactoryGetArgs args)
        {
            throw new NotImplementedException();
        }

        protected virtual void RecycleElementCore(ElementFactoryRecycleArgs args)
        {
            throw new NotImplementedException();
        }
    }
}
