// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Windows;
using System.Windows.Controls;

namespace ModernWpf.Controls
{
    internal class RadioButtonsElementFactory : ElementFactory
    {
        public RadioButtonsElementFactory()
        {
        }

        internal void UserElementFactory(object newValue)
        {
            m_itemTemplateWrapper = newValue as IElementFactoryShim;
            if (m_itemTemplateWrapper is null)
            {
                // ItemTemplate set does not implement IElementFactoryShim. We also want to support DataTemplate.
                if (newValue is DataTemplate dataTemplate)
                {
                    m_itemTemplateWrapper = new ItemTemplateWrapper(dataTemplate);
                }
            }
        }

        protected override UIElement GetElementCore(ElementFactoryGetArgs args)
        {
            object newContent;
            if (m_itemTemplateWrapper != null)
            {
                newContent = m_itemTemplateWrapper.GetElement(args);
            }
            else
            {
                newContent = args.Data;
            }

            // Element is already a RadioButton, so we just return it.
            if (newContent is RadioButton radioButton)
            {
                return radioButton;
            }

            // Element is not a RadioButton. We'll wrap it in a RadioButton now.
            var newRadioButton = new RadioButton();
            newRadioButton.Content = args.Data;

            // If a user provided item template exists, we pass the template down to the ContentPresenter of the RadioButton.
            if (m_itemTemplateWrapper is ItemTemplateWrapper itemTemplateWrapper)
            {
                newRadioButton.ContentTemplate = itemTemplateWrapper.Template;
            }

            return newRadioButton;
        }

        protected override void RecycleElementCore(ElementFactoryRecycleArgs args)
        {
        }

        IElementFactoryShim m_itemTemplateWrapper;
    }
}
