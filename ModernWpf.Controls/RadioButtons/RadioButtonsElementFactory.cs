// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ModernWpf.Controls
{
    internal class RadioButtonsElementFactory : ElementFactory
    {
        private static readonly DependencyProperty IsFactoryCreatedProperty =
            DependencyProperty.RegisterAttached("IsFactoryCreated", typeof(bool),
                typeof(RadioButtonsElementFactory), new PropertyMetadata(false));

        private readonly Stack<RadioButton> _radioButtonPool = new Stack<RadioButton>();

        public RadioButtonsElementFactory()
        {
        }

        internal void UserElementFactory(object newValue)
        {
            if (newValue is DataTemplate dataTemplate)
            {
                m_itemTemplateWrapper = new ItemTemplateWrapper(dataTemplate);
            }
            else if (newValue is DataTemplateSelector selector)
            {
                m_itemTemplateWrapper = new ItemTemplateWrapper(selector);
            }
            else if (newValue is IElementFactory customElementFactory)
            {
                m_itemTemplateWrapper = customElementFactory;
            }
            else
            {
                m_itemTemplateWrapper = null;
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

            // Reuse wrappers still owned by this repeater. A retemplate can
            // leave pooled elements attached to the old parent.
            RadioButton newRadioButton = null;
            if (_radioButtonPool.Count > 0)
            {
                if (VisualTreeHelper.GetParent(_radioButtonPool.Peek()) == args.Parent)
                {
                    newRadioButton = _radioButtonPool.Pop();
                }
                else
                {
                    _radioButtonPool.Clear();
                }
            }

            if (newRadioButton == null)
            {
                newRadioButton = new RadioButton();
                newRadioButton.SetValue(IsFactoryCreatedProperty, true);
            }
            newRadioButton.Content = args.Data;

            // If a user provided item template exists, we pass the template down to the ContentPresenter of the RadioButton.
            if (m_itemTemplateWrapper is ItemTemplateWrapper itemTemplateWrapper)
            {
                newRadioButton.ContentTemplate = itemTemplateWrapper.Template;
                newRadioButton.ContentTemplateSelector = itemTemplateWrapper.TemplateSelector;
            }
            else
            {
                newRadioButton.ContentTemplate = null;
                newRadioButton.ContentTemplateSelector = null;
            }

            return newRadioButton;
        }

        protected override void RecycleElementCore(ElementFactoryRecycleArgs args)
        {
            if (args.Element is RadioButton radioButton &&
                (bool)radioButton.GetValue(IsFactoryCreatedProperty))
            {
                radioButton.IsChecked = false;
                radioButton.Content = null;
                radioButton.ContentTemplate = null;
                radioButton.ContentTemplateSelector = null;
                _radioButtonPool.Push(radioButton);
            }
            else if (args.Element != null)
            {
                m_itemTemplateWrapper?.RecycleElement(args);
            }
        }

        IElementFactory m_itemTemplateWrapper;
    }
}
