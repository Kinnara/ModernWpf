using System.Collections.Generic;
using System.Linq;

namespace ModernWpf.Controls
{
    internal static class ListExtensions
    {
        public static void Resize<T>(this List<T> list, int size, T element = default)
        {
            int count = list.Count;

            if (size < count)
            {
                list.RemoveRange(size, count - size);
            }
            else if (size > count)
            {
                if (size > list.Capacity)   // Optimization
                    list.Capacity = size;

                list.AddRange(Enumerable.Repeat(element, size - count));
            }
        }

        public static T Last<T>(this List<T> list)
        {
            return list[list.Count - 1];
        }

        public static void RemoveLast<T>(this List<T> list)
        {
            list.RemoveAt(list.Count - 1);
        }

        public static bool Empty<T>(this List<T> list)
        {
            return list.Count == 0;
        }
    }
}
