using ModernWpf.Media.ColorPalette;
using System;
using System.Collections;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using Windows.UI.ViewManagement;

namespace ModernWpf
{
    internal static class ColorsHelper
    {
        private const string AccentKey = "SystemAccentColor";
        private const string AccentDark1Key = "SystemAccentColorDark1";
        private const string AccentDark2Key = "SystemAccentColorDark2";
        private const string AccentDark3Key = "SystemAccentColorDark3";
        private const string AccentLight1Key = "SystemAccentColorLight1";
        private const string AccentLight2Key = "SystemAccentColorLight2";
        private const string AccentLight3Key = "SystemAccentColorLight3";

        internal static readonly Color DefaultAccentColor = Color.FromRgb(0x00, 0x78, 0xD7);

        private static readonly ResourceDictionary _colors = new ResourceDictionary();
        private static object _uiSettings;

        private static Color _systemBackground;
        private static Color _systemAccent;

        public static bool SystemColorsSupported { get; } = Environment.OSVersion.Version.Major >= 10;

        public static bool IsInitialized { get; private set; }

        public static event EventHandler BackgroundColorChanged;

        public static event EventHandler AccentColorChanged;

        public static void Initialize()
        {
            if (IsInitialized)
            {
                return;
            }

            if (SystemColorsSupported)
            {
                ListenToSystemColorChanges();
            }

            Application.Current.Resources.MergedDictionaries.Insert(0, _colors);
            IsInitialized = true;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void FetchSystemAccentColors()
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

        public static void SetAccent(Color accent)
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

        public static void UpdateBrushes(ResourceDictionary themeDictionary)
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
        private static Color ToColor(this Windows.UI.Color color)
        {
            return Color.FromArgb(color.A, color.R, color.G, color.B);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void ListenToSystemColorChanges()
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
                    Application.Current?.Dispatcher.BeginInvoke((Action)(() =>
                    {
                        BackgroundColorChanged?.Invoke(null, EventArgs.Empty);
                    }));
                }

                var accent = sender.GetColorValue(UIColorType.Accent).ToColor();
                if (_systemAccent != accent)
                {
                    _systemAccent = accent;
                    Application.Current?.Dispatcher.BeginInvoke((Action)(() =>
                    {
                        AccentColorChanged?.Invoke(null, EventArgs.Empty);
                    }));
                }
            };

            _uiSettings = uiSettings;
        }
    }
}
