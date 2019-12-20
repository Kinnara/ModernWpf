using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace ModernWpf.MahApps.Controls
{
    public class DateTimeComponentSelectorItemsConverter : IValueConverter
    {
        internal const int NumberPaddingItem = -1;
        internal const string StringPaddingItem = "";

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is IEnumerable<int> numbers && numbers.Any())
            {
                return new LoopingSelectorDataSource(numbers);
            }
            else if (value is IEnumerable<string> strings && strings.Any())
            {
                var items = new List<string>();
                items.AddRange(Enumerable.Repeat(StringPaddingItem, DateTimeComponentSelector.PaddingItemsCount));
                items.AddRange(strings);
                items.AddRange(Enumerable.Repeat(StringPaddingItem, DateTimeComponentSelector.PaddingItemsCount));
                return items;
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
