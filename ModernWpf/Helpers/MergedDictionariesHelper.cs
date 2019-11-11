using System;
using System.Collections.ObjectModel;
using System.Windows;

namespace ModernWpf
{
    internal static class MergedDictionariesHelper
    {
        //public static void Add(this Collection<ResourceDictionary> mergedDictionaries, Uri source, bool useCache = true)
        //{
        //    var dictionary = GetDictionary(source, useCache);
        //    if (!mergedDictionaries.Contains(dictionary))
        //    {
        //        mergedDictionaries.Add(dictionary);
        //    }
        //}

        //public static void AddFirst(this Collection<ResourceDictionary> mergedDictionaries, Uri source, bool useCache = true)
        //{
        //    mergedDictionaries.Insert(0, source, useCache);
        //}

        //public static void Insert(this Collection<ResourceDictionary> mergedDictionaries, int index, Uri source, bool useCache = true)
        //{
        //    var dictionary = GetDictionary(source, useCache);
        //    if (!mergedDictionaries.Contains(dictionary))
        //    {
        //        mergedDictionaries.Insert(index, dictionary);
        //    }
        //}

        //public static void Remove(this Collection<ResourceDictionary> mergedDictionaries, Uri source)
        //{
        //    bool removed = false;

        //    if (ResourceDictionaryCache.TryGetDictionary(source, out ResourceDictionary dictionary))
        //    {
        //        removed = mergedDictionaries.Remove(dictionary);
        //    }

        //    if (!removed)
        //    {
        //        for (int i = mergedDictionaries.Count - 1; i >= 0; i--)
        //        {
        //            dictionary = mergedDictionaries[i];
        //            if (dictionary != null && dictionary.Source == source)
        //            {
        //                mergedDictionaries.RemoveAt(i);
        //            }
        //        }
        //    }
        //}

        public static void AddIfNotNull(this Collection<ResourceDictionary> mergedDictionaries, ResourceDictionary item)
        {
            if (item != null)
            {
                mergedDictionaries.Add(item);
            }
        }

        public static void RemoveIfNotNull(this Collection<ResourceDictionary> mergedDictionaries, ResourceDictionary item)
        {
            if (item != null)
            {
                mergedDictionaries.Remove(item);
            }
        }

        public static void InsertOrReplace(this Collection<ResourceDictionary> mergedDictionaries, int index, ResourceDictionary item)
        {
            if (mergedDictionaries.Count > index)
            {
                mergedDictionaries[index] = item;
            }
            else
            {
                mergedDictionaries.Insert(index, item);
            }
        }

        public static void RemoveAll<T>(this Collection<ResourceDictionary> mergedDictionaries) where T : ResourceDictionary
        {
            for (int i = mergedDictionaries.Count - 1; i >= 0; i--)
            {
                if (mergedDictionaries[i] is T)
                {
                    mergedDictionaries.RemoveAt(i);
                }
            }
        }

        public static void InsertIfNotExists(this Collection<ResourceDictionary> mergedDictionaries, int index, ResourceDictionary item)
        {
            if (!mergedDictionaries.Contains(item))
            {
                mergedDictionaries.Insert(index, item);
            }
        }

        public static void Swap(this Collection<ResourceDictionary> mergedDictionaries, int index1, int index2)
        {
            if (index1 == index2)
            {
                return;
            }

            var smallIndex = Math.Min(index1, index2);
            var largeIndex = Math.Max(index1, index2);
            var tmp = mergedDictionaries[smallIndex];
            mergedDictionaries.RemoveAt(smallIndex);
            mergedDictionaries.Insert(largeIndex, tmp);
        }

        //private static ResourceDictionary GetDictionary(Uri source, bool useCache)
        //{
        //    if (useCache)
        //    {
        //        return ResourceDictionaryCache.GetOrCreateDictionary(source);
        //    }
        //    else
        //    {
        //        return new ResourceDictionary { Source = source };
        //    }
        //}
    }
}
