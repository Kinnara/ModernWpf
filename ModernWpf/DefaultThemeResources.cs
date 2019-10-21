using System;
using System.Windows;

namespace ModernWpf
{
    public class DefaultThemeResources : ResourceDictionary
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

            ResourceDictionary defaultThemeDictionary = ThemeManager.GetDefaultThemeDictionary(Key);

            if (defaultThemeDictionary != null)
            {
                MergedDictionaries.Add(defaultThemeDictionary);
            }
        }
    }
}
