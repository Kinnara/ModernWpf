using System;
using System.ComponentModel;
using System.Windows;

namespace ModernWpf
{
    [Obsolete]
    [EditorBrowsable(EditorBrowsableState.Never)]
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
            if (MergedDictionaries.Count > 0)
            {
                MergedDictionaries.Clear();
            }

            ResourceDictionary defaultThemeDictionary = ThemeManager.GetDefaultThemeDictionary(Key);

            if (defaultThemeDictionary != null)
            {
                MergedDictionaries.Add(defaultThemeDictionary);
            }
        }
    }
}
