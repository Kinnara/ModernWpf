// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;

namespace ModernWpf.Controls
{
    internal class ItemTemplateWrapper : IElementFactoryShim
    {
        public ItemTemplateWrapper(DataTemplate dataTemplate)
        {
            Template = dataTemplate;
        }

        public ItemTemplateWrapper(DataTemplateSelector dataTemplateSelector)
        {
            TemplateSelector = dataTemplateSelector;
        }

        public DataTemplate Template { get; set; }

        public DataTemplateSelector TemplateSelector { get; set; }

        #region IElementFactory

        public UIElement GetElement(ElementFactoryGetArgs args)
        {
            var selectedTemplate = Template ?? TemplateSelector.SelectTemplate(args.Data, null);
            // Check if selected template we got is valid
            if (selectedTemplate == null)
            {
                selectedTemplate = TemplateSelector.SelectTemplate(args.Data, null);

                if (selectedTemplate == null)
                {
                    // Still nullptr, fail with a reasonable message now.
                    throw new InvalidOperationException("Null encountered as data template. That is not a valid value for a data template, and can not be used.");
                }
            }
            var recyclePool = RecyclePool.GetPoolInstance(selectedTemplate);
            UIElement element = null;

            if (recyclePool != null)
            {
                // try to get an element from the recycle pool.
                element = recyclePool.TryGetElement(string.Empty /* key */, args.Parent as FrameworkElement);
            }

            if (element == null)
            {
                // no element was found in recycle pool, create a new element
                element = selectedTemplate.LoadContent() as FrameworkElement;

                // Template returned null, so insert empty element to render nothing
                if (element == null)
                {
                    element = new Rectangle {
                        Width = 0,
                        Height = 0
                    };
                }

                // Associate template with element
                element.SetValue(RecyclePool.OriginTemplateProperty, selectedTemplate);
            }

            return element;
        }

        public void RecycleElement(ElementFactoryRecycleArgs args)
        {
            var element = args.Element;
            DataTemplate selectedTemplate = Template ??
                element.GetValue(RecyclePool.OriginTemplateProperty) as DataTemplate;
            var recyclePool = RecyclePool.GetPoolInstance(selectedTemplate);
            if (recyclePool == null)
            {
                // No Recycle pool in the template, create one.
                recyclePool = new RecyclePool();
                RecyclePool.SetPoolInstance(selectedTemplate, recyclePool);
            }

            recyclePool.PutElement(args.Element, string.Empty /* key */, args.Parent);
        }

        #endregion
    }
}
