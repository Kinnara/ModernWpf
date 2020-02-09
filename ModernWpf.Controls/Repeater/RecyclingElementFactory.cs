// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Markup;

namespace ModernWpf.Controls
{
    //[ContentProperty(nameof(Templates))]
    public class RecyclingElementFactory : ElementFactory
    {
        public RecyclingElementFactory()
        {
            m_templates = new Dictionary<string, DataTemplate>();
        }

        public RecyclePool RecyclePool { get; set; }

        public Dictionary<string, DataTemplate> Templates
        {
            get => m_templates;
            set => m_templates = value;
        }

        public event TypedEventHandler<RecyclingElementFactory, SelectTemplateEventArgs> SelectTemplateKey;

        protected virtual string OnSelectTemplateKeyCore(
            object dataContext,
            UIElement owner)
        {
            if (m_args == null)
            {
                m_args = new SelectTemplateEventArgs();
            }

            var args = m_args;
            args.TemplateKey = string.Empty;
            args.DataContext = dataContext;
            args.Owner = owner;

            SelectTemplateKey?.Invoke(this, args);

            var templateKey = args.TemplateKey;
            if (string.IsNullOrEmpty(templateKey))
            {
                throw new InvalidOperationException("Please provide a valid template identifier in the handler for the SelectTemplateKey event.");
            }

            return templateKey;
        }

        protected override UIElement GetElementCore(ElementFactoryGetArgs args)
        {
            if (m_templates == null || m_templates.Count == 0)
            {
                throw new InvalidOperationException("Templates property cannot be null or empty.");
            }

            var winrtOwner = args.Parent;
            var templateKey =
               m_templates.Count == 1 ?
               m_templates.First().Key :
               OnSelectTemplateKeyCore(args.Data, winrtOwner);

            if (string.IsNullOrEmpty(templateKey))
            {
                // Note: We could allow null/whitespace, which would work as long as
                // the recycle pool is not shared. in order to make this work in all cases
                // currently we validate that a valid template key is provided.
                throw new InvalidOperationException("Template key cannot be empty or null.");
            }

            // Get an element from the Recycle Pool or create one
            var element = RecyclePool.TryGetElement(templateKey, winrtOwner) as FrameworkElement;

            if (element == null)
            {
                // No need to call HasKey if there is only one template.
                if (m_templates.Count > 1 && !m_templates.ContainsKey(templateKey))
                {
                    string message = "No templates of key " + templateKey + " were found in the templates collection.";
                    throw new InvalidOperationException(message);
                }

                var dataTemplate = m_templates[templateKey];
                element = dataTemplate.LoadContent() as FrameworkElement;

                // Associate ReuseKey with element
                RecyclePool.SetReuseKey(element, templateKey);
            }

            return element;
        }

        protected override void RecycleElementCore(ElementFactoryRecycleArgs args)
        {
            var element = args.Element;
            var key = RecyclePool.GetReuseKey(element);
            RecyclePool.PutElement(element, key, args.Parent);
        }

        private Dictionary<string, DataTemplate> m_templates;
        private SelectTemplateEventArgs m_args;
    }
}
