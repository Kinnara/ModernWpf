using System;
using System.Collections;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using Microsoft.Win32;
using ModernWpf.Media.ColorPalette;
using Windows.UI.ViewManagement;

namespace ModernWpf
{
    internal class ColorsHelper : DispatcherObject
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
        private UISettings _uiSettings;

        private Color _systemBackground;
        private Color _systemAccent;

        private ColorsHelper()
        {
            if (SystemColorsSupported)
            {
                ListenToSystemColorChanges();
            }
        }

        public static bool SystemColorsSupported { get; } = OSVersionHelper.IsWindows10OrGreater;

        public static ColorsHelper Current { get; } = new ColorsHelper();

        public ResourceDictionary Colors => _colors;

        public ApplicationTheme? SystemTheme { get; private set; }

        public Color SystemAccentColor => _systemAccent;

        public event EventHandler SystemThemeChanged;

        public event EventHandler SystemAccentColorChanged;

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
            UpdateBrushes(themeDictionary, _colors);
        }

        public static void UpdateBrushes(ResourceDictionary themeDictionary, ResourceDictionary colors)
        {
            foreach (DictionaryEntry entry in themeDictionary)
            {
                if (entry.Value is SolidColorBrush brush && !brush.IsFrozen)
                {
                    object colorKey = ThemeResourceHelper.GetColorKey(brush);
                    if (colorKey != null && colors.Contains(colorKey))
                    {
                        brush.SetCurrentValue(SolidColorBrush.ColorProperty, (Color)colors[colorKey]);
                    }
                }
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private void ListenToSystemColorChanges()
        {
            _uiSettings = new UISettings();
            _uiSettings.ColorValuesChanged += OnColorValuesChanged;

            if (PackagedAppHelper.IsPackagedApp)
            {
                SystemEvents.UserPreferenceChanged += OnUserPreferenceChanged;
            }

            _systemBackground = _uiSettings.GetColorValue(UIColorType.Background).ToColor();
            _systemAccent = _uiSettings.GetColorValue(UIColorType.Accent).ToColor();
            UpdateSystemAppTheme();
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private void OnColorValuesChanged(UISettings sender, object args)
        {
            Dispatcher.BeginInvoke(UpdateColorValues);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private void OnUserPreferenceChanged(object sender, UserPreferenceChangedEventArgs e)
        {
            if (e.Category == UserPreferenceCategory.General)
            {
                UpdateColorValues();
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private void UpdateColorValues()
        {
            var background = _uiSettings.GetColorValue(UIColorType.Background).ToColor();
            if (_systemBackground != background)
            {
                _systemBackground = background;
                UpdateSystemAppTheme();
                SystemThemeChanged?.Invoke(null, EventArgs.Empty);
            }

            var accent = _uiSettings.GetColorValue(UIColorType.Accent).ToColor();
            if (_systemAccent != accent)
            {
                _systemAccent = accent;
                SystemAccentColorChanged?.Invoke(null, EventArgs.Empty);
            }
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
