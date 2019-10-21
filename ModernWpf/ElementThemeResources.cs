using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;

namespace ModernWpf
{
    public class ElementThemeResources : ResourceDictionary, ISupportInitialize
    {
        /// <summary>
        /// Gets a collection of merged resource dictionaries that are specifically keyed
        /// and composed to address theme scenarios, for example supplying theme values for
        /// "HighContrast".
        /// </summary>
        /// <returns>
        /// A dictionary of ResourceDictionary theme dictionaries. Each must be keyed with
        /// **x:Key**.
        /// </returns>
        public Dictionary<object, ResourceDictionary> ThemeDictionaries { get; } = new Dictionary<object, ResourceDictionary>();

        internal bool ContainsApplicationThemeDictionary { get; set; }

        internal void Update(string themeKey)
        {
            if (ThemeDictionaries.TryGetValue(themeKey, out ResourceDictionary themeDictionary))
            {
                MergedDictionaries.InsertOrReplace(ContainsApplicationThemeDictionary ? 1 : 0, themeDictionary);
            }
            else
            {
                if (ContainsApplicationThemeDictionary)
                {
                    Debug.Assert(MergedDictionaries.Count >= 1 && MergedDictionaries.Count <= 2);
                    if (MergedDictionaries.Count == 2)
                    {
                        MergedDictionaries.RemoveAt(1);
                    }
                }
                else
                {
                    MergedDictionaries.Clear();
                }
            }
        }

        void ISupportInitialize.EndInit()
        {
            EndInit();

            for (int i = MergedDictionaries.Count - 1; i >= 0; i--)
            {
                if (MergedDictionaries[i] is ThemeDictionary td)
                {
                    ThemeDictionaries[td.Key] = td;
                    MergedDictionaries.RemoveAt(i);
                }
            }
        }
    }
}
