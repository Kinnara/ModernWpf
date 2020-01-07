using System;
using System.Diagnostics;
using System.Windows;

namespace ModernWpf
{
    public class ApplicationThemeResources : ResourceDictionary
    {
        private string _key;

        public string Key
        {
            get => _key;
            set
            {
                if (_key != value)
                {
                    switch (value)
                    {
                        case ThemeManager.LightKey:
                        case ThemeManager.DarkKey:
                        case ThemeManager.HighContrastKey:
                            _key = value;
                            UpdateContent();
                            break;
                        default:
                            throw new ArgumentOutOfRangeException(nameof(value));
                    }
                }
            }
        }

        private void UpdateContent()
        {
            MergedDictionaries.Clear();

            ResourceDictionary themeDictionary = null;
            ThemeResources.Current?.ThemeDictionaries.TryGetValue(Key, out themeDictionary);
            Debug.Assert(themeDictionary != null);

            if (themeDictionary == null)
            {
                themeDictionary = ThemeManager.GetDefaultThemeDictionary(Key);
            }

            if (themeDictionary != null)
            {
                MergedDictionaries.Add(themeDictionary);
            }
        }
    }
}
