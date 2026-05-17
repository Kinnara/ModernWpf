using System.Collections;
using System.Collections.Generic;

namespace ModernWpf.Controls
{
    internal sealed class BreadcrumbIterable : IEnumerable<object>
    {
        public BreadcrumbIterable(object itemsSource)
        {
            _itemsSource = itemsSource;
        }

        public IEnumerator<object> GetEnumerator()
        {
            yield return null;

            if (_itemsSource is IEnumerable enumerable && _itemsSource is not string)
            {
                foreach (var item in enumerable)
                {
                    yield return item;
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private readonly object _itemsSource;
    }
}
