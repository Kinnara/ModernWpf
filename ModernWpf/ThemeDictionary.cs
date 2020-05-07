using System.Windows;

namespace ModernWpf
{
    public static class ThemeDictionary
    {
        public static void SetKey(ResourceDictionary themeDictionary, string key)
        {
            var baseThemeDictionary = GetBaseThemeDictionary(key);
            themeDictionary.MergedDictionaries.Insert(0, baseThemeDictionary);
        }

        private static ResourceDictionary GetBaseThemeDictionary(string key)
        {
            ResourceDictionary themeDictionary = ThemeResources.Current?.TryGetThemeDictionary(key);
            return themeDictionary ?? ThemeManager.GetDefaultThemeDictionary(key);
        }
    }
}
