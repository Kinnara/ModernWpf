using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace ModernWpf.Controls
{
    internal sealed class ItemsViewElementFactory : IElementFactory
    {
        private readonly Stack<ItemContainer> _recyclePool = new Stack<ItemContainer>();

        public UIElement GetElement(ElementFactoryGetArgs args)
        {
            if (_recyclePool.Count > 0)
            {
                return _recyclePool.Pop();
            }

            var textBlock = new TextBlock
            {
                Margin = new Thickness(8, 6, 8, 6),
                TextTrimming = TextTrimming.CharacterEllipsis
            };
            textBlock.SetBinding(TextBlock.TextProperty, new Binding());

            return new ItemContainer
            {
                Child = textBlock,
                HorizontalContentAlignment = HorizontalAlignment.Stretch
            };
        }

        public void RecycleElement(ElementFactoryRecycleArgs args)
        {
            if (args.Element is ItemContainer itemContainer)
            {
                _recyclePool.Push(itemContainer);
            }
        }
    }
}
