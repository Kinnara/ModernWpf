using System;
using System.Collections.Generic;
using System.Windows;

namespace ModernWpf
{
    internal static class ResourceDictionaryCache
    {
        private static readonly Dictionary<Uri, WeakReference<ResourceDictionary>> _cache = new Dictionary<Uri, WeakReference<ResourceDictionary>>();

        public static void Add(Uri source, ResourceDictionary value)
        {
            _cache[source] = new WeakReference<ResourceDictionary>(value);
        }

        public static ResourceDictionary GetOrCreateDictionary(Uri source)
        {
            if (!TryGetDictionary(source, out ResourceDictionary dictionary))
            {
                dictionary = new ResourceDictionary { Source = source };
                Add(source, dictionary);
            }

            return dictionary;
        }

        public static bool TryGetDictionary(Uri source, out ResourceDictionary value)
        {
            if (_cache.TryGetValue(source, out WeakReference<ResourceDictionary> wr))
            {
                if (wr.TryGetTarget(out value))
                {
                    return true;
                }
                else
                {
                    _cache.Remove(source);
                }
            }

            value = null;
            return false;
        }
    }
}
