// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Windows;
using System.Windows.Controls;

namespace ModernWpf.Controls
{
    class NavigationViewItemsFactory : ElementFactory
    {
        public void UserElementFactory(object newValue)
        {
            m_itemTemplateWrapper = newValue as IElementFactoryShim;
            if (m_itemTemplateWrapper == null)
            {
                // ItemTemplate set does not implement IElementFactoryShim. We also 
                // want to support DataTemplate and DataTemplateSelectors automagically.
                if (newValue is DataTemplate dataTemplate)
                {
                    m_itemTemplateWrapper = new ItemTemplateWrapper(dataTemplate);
                }
                else if (newValue is DataTemplateSelector selector)
                {
                    m_itemTemplateWrapper = new ItemTemplateWrapper(selector);
                }
            }
        }

        protected override UIElement GetElementCore(ElementFactoryGetArgs args)
        {
            object newContent;
            {
                object init()
                {
                    if (m_itemTemplateWrapper != null)
                    {
                        return m_itemTemplateWrapper.GetElement(args);
                    }
                    return args.Data;
                }
                newContent = init();
            }

            if (newContent is NavigationViewItemBase newItem)
            {
                return newItem;
            }

            // Create a wrapping container for the data
            var nvi = new NavigationViewItem();
            nvi.Content = newContent;
            return nvi;
        }

        protected override void RecycleElementCore(ElementFactoryRecycleArgs args)
        {
            if (m_itemTemplateWrapper != null)
            {
                m_itemTemplateWrapper.RecycleElement(args);
            }
            else
            {
                // We want to unlink the containers from the parent repeater
                // in case we are required to move it to a different repeater.
                if (args.Parent is Panel panel)
                {
                    var children = panel.Children;
                    int childIndex = 0;
                    if (children.IndexOf(args.Element, out childIndex))
                    {
                        children.RemoveAt(childIndex);
                    }
                }
            }
        }

        IElementFactoryShim m_itemTemplateWrapper;
    }
}
