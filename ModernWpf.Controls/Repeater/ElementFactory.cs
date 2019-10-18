// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Windows;

namespace ModernWpf.Controls
{
    public abstract class ElementFactory : DependencyObject, IElementFactoryShim
    {
        protected ElementFactory()
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

        protected abstract UIElement GetElementCore(ElementFactoryGetArgs args);

        protected abstract void RecycleElementCore(ElementFactoryRecycleArgs args);
    }
}
