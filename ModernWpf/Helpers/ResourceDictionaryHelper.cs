using System.Windows;
using System.Collections;
using System.Collections.Generic;

namespace ModernWpf
{
    internal static class ResourceDictionaryHelper
    {
        public static void SealValues(this ResourceDictionary dictionary)
        {
            foreach (var md in dictionary.MergedDictionaries)
            {
                SealValues(md);
            }

            foreach (var value in dictionary.Values)
            {
                if (value is Freezable freezable)
                {
                    if (!freezable.CanFreeze)
                    {
                        SnapshotExpressions(freezable, new HashSet<Freezable>());
                    }

                    if (!freezable.IsFrozen)
                    {
                        freezable.Freeze();
                    }
                }
                else if (value is Style style)
                {
                    if (!style.IsSealed)
                    {
                        style.Seal();
                    }
                }
            }

            if (dictionary is ResourceDictionaryEx rdEx)
            {
                foreach (var td in rdEx.ThemeDictionaries.Values)
                {
                    SealValues(td);
                }
            }
        }

        private static void SnapshotExpressions(
            Freezable value,
            HashSet<Freezable> visited)
        {
            if (!visited.Add(value))
            {
                return;
            }

            var localProperties = new List<DependencyProperty>();
            var enumerator = value.GetLocalValueEnumerator();
            while (enumerator.MoveNext())
            {
                localProperties.Add(enumerator.Current.Property);
            }

            foreach (var property in localProperties)
            {
                if (DependencyPropertyHelper.GetValueSource(value, property).IsExpression)
                {
                    value.SetValue(property, value.GetValue(property));
                }

                SnapshotChild(value.GetValue(property), visited);
            }

            if (value is IEnumerable children)
            {
                foreach (var child in children)
                {
                    SnapshotChild(child, visited);
                }
            }
        }

        private static void SnapshotChild(
            object value,
            HashSet<Freezable> visited)
        {
            if (value is Freezable freezable)
            {
                SnapshotExpressions(freezable, visited);
            }
        }
    }
}
