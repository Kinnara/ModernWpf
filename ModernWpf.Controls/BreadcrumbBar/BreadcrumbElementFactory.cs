using System.Windows;
using System.Windows.Controls;

namespace ModernWpf.Controls
{
    internal sealed class BreadcrumbElementFactory : ElementFactory
    {
        public void UserElementFactory(object newValue)
        {
            if (newValue is DataTemplate dataTemplate)
            {
                _itemTemplateWrapper = new ItemTemplateWrapper(dataTemplate);
            }
            else if (newValue is DataTemplateSelector dataTemplateSelector)
            {
                _itemTemplateWrapper = new ItemTemplateWrapper(dataTemplateSelector);
            }
            else if (newValue is IElementFactory customElementFactory)
            {
                _itemTemplateWrapper = customElementFactory;
            }
            else
            {
                _itemTemplateWrapper = null;
            }
        }

        protected override UIElement GetElementCore(ElementFactoryGetArgs args)
        {
            if (args.Data is BreadcrumbBarItem existingBreadcrumbItem)
            {
                return existingBreadcrumbItem;
            }

            var newBreadcrumbBarItem = new BreadcrumbBarItem
            {
                Content = args.Data
            };

            if (_itemTemplateWrapper is ItemTemplateWrapper itemTemplateWrapper)
            {
                if (itemTemplateWrapper.Template != null)
                {
                    newBreadcrumbBarItem.ContentTemplate = itemTemplateWrapper.Template;
                }
                else
                {
                    newBreadcrumbBarItem.ContentTemplateSelector = itemTemplateWrapper.TemplateSelector;
                }
            }

            return newBreadcrumbBarItem;
        }

        protected override void RecycleElementCore(ElementFactoryRecycleArgs args)
        {
            if (args.Element is BreadcrumbBarItem breadcrumbItem)
            {
                breadcrumbItem.ResetVisualProperties();
            }
        }

        private IElementFactory _itemTemplateWrapper;
    }
}
