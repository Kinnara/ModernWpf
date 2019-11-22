using ModernWpf.Media.ColorPalette;
using System;
using System.Collections;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media;
using Windows.UI.ViewManagement;

namespace ModernWpf
{
    internal class ColorsHelper
    {
        private const string AccentKey = "SystemAccentColor";
        private const string AccentDark1Key = "SystemAccentColorDark1";
        private const string AccentDark2Key = "SystemAccentColorDark2";
        private const string AccentDark3Key = "SystemAccentColorDark3";
        private const string AccentLight1Key = "SystemAccentColorLight1";
        private const string AccentLight2Key = "SystemAccentColorLight2";
        private const string AccentLight3Key = "SystemAccentColorLight3";

        internal static readonly Color DefaultAccentColor = Color.FromRgb(0x00, 0x78, 0xD7);

        private readonly ResourceDictionary _colors = new ResourceDictionary();
        private object _uiSettings;

        private Color _systemBackground;
        private Color _systemAccent;

        private ColorsHelper()
        {
            if (SystemColorsSupported)
            {
                ListenToSystemColorChanges();
            }
        }

        public static bool SystemColorsSupported { get; } = Environment.OSVersion.Version.Major >= 10;

        public static ColorsHelper Current { get; } = new ColorsHelper();

        public ResourceDictionary Colors => _colors;

        public ApplicationTheme? SystemTheme { get; private set; }

        public event EventHandler SystemThemeChanged;

        public event EventHandler AccentColorChanged;

        [MethodImpl(MethodImplOptions.NoInlining)]
        public void FetchSystemAccentColors()
        {
            var uiSettings = new UISettings();
            _colors[AccentKey] = uiSettings.GetColorValue(UIColorType.Accent).ToColor();
            _colors[AccentDark1Key] = uiSettings.GetColorValue(UIColorType.AccentDark1).ToColor();
            _colors[AccentDark2Key] = uiSettings.GetColorValue(UIColorType.AccentDark2).ToColor();
            _colors[AccentDark3Key] = uiSettings.GetColorValue(UIColorType.AccentDark3).ToColor();
            _colors[AccentLight1Key] = uiSettings.GetColorValue(UIColorType.AccentLight1).ToColor();
            _colors[AccentLight2Key] = uiSettings.GetColorValue(UIColorType.AccentLight2).ToColor();
            _colors[AccentLight3Key] = uiSettings.GetColorValue(UIColorType.AccentLight3).ToColor();
        }

        public void SetAccent(Color accent)
        {
            Color color = accent;
            _colors[AccentKey] = color;
            UpdateShades(_colors, color);
        }

        public static void UpdateShades(ResourceDictionary colors, Color accent)
        {
            var palette = new ColorPalette(11, accent);
            colors[AccentDark1Key] = palette.Palette[6].ActiveColor;
            colors[AccentDark2Key] = palette.Palette[7].ActiveColor;
            colors[AccentDark3Key] = palette.Palette[8].ActiveColor;
            colors[AccentLight1Key] = palette.Palette[4].ActiveColor;
            colors[AccentLight2Key] = palette.Palette[3].ActiveColor;
            colors[AccentLight3Key] = palette.Palette[2].ActiveColor;
        }

        public static void RemoveShades(ResourceDictionary colors)
        {
            colors.Remove(AccentDark3Key);
            colors.Remove(AccentDark2Key);
            colors.Remove(AccentDark1Key);
            colors.Remove(AccentLight1Key);
            colors.Remove(AccentLight2Key);
            colors.Remove(AccentLight3Key);
        }

        public void UpdateBrushes(ResourceDictionary themeDictionary)
        {
            int count = 0;

            foreach (DictionaryEntry entry in themeDictionary)
            {
                if (entry.Value is SolidColorBrush brush)
                {
                    object colorKey = ThemeResourceHelper.GetColorKey(brush);
                    if (colorKey != null && _colors.Contains(colorKey))
                    {
                        brush.SetCurrentValue(SolidColorBrush.ColorProperty, (Color)_colors[colorKey]);
                        count++;
                    }
                }
            }

            Debug.WriteLine($"{count} brushes updated");
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static Color GetSystemAccentColor()
        {
            var uiSettings = new UISettings();
            return uiSettings.GetColorValue(UIColorType.Accent).ToColor();
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private void ListenToSystemColorChanges()
        {
            var uiSettings = new UISettings();

            _systemBackground = uiSettings.GetColorValue(UIColorType.Background).ToColor();
            _systemAccent = uiSettings.GetColorValue(UIColorType.Accent).ToColor();

            uiSettings.ColorValuesChanged += (sender, args) =>
            {
#if DEBUG
                //for (int i = 0; i <= 8; i++)
                //{
                //    var colorType = (UIColorType)i;
                //    Debug.WriteLine(colorType + " = " + sender.GetColorValue(colorType));
                //}
#endif
                var background = sender.GetColorValue(UIColorType.Background).ToColor();
                if (_systemBackground != background)
                {
                    _systemBackground = background;
                    UpdateSystemAppTheme();
                    SystemThemeChanged?.Invoke(null, EventArgs.Empty);
                }

                var accent = sender.GetColorValue(UIColorType.Accent).ToColor();
                if (_systemAccent != accent)
                {
                    _systemAccent = accent;
                    AccentColorChanged?.Invoke(null, EventArgs.Empty);
                }
            };

            _uiSettings = uiSettings;

            UpdateSystemAppTheme();
        }

        private void UpdateSystemAppTheme()
        {
            SystemTheme = IsDarkBackground(_systemBackground) ? ApplicationTheme.Dark : ApplicationTheme.Light;
        }

        private static bool IsDarkBackground(Color color)
        {
            return color.R + color.G + color.B < (255 * 3 - color.R - color.G - color.B);
        }
    }
}
