using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Security;
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
        internal const string WindowBorderColorKey = "SystemWindowBorderColor";

        private const string DwmRegistryPath = @"Software\Microsoft\Windows\DWM";
        private const string ColorPrevalenceValueName = "ColorPrevalence";

        internal static readonly Color DefaultAccentColor = Color.FromRgb(0x00, 0x78, 0xD7);
        internal static readonly Color DefaultWindowBorderColor = Color.FromRgb(0x70, 0x70, 0x70);

        private readonly ResourceDictionary _colors = new ResourceDictionary();
        private UISettings _uiSettings;

        private Color _systemBackground;
        private Color _systemAccent;
        private bool _useAccentColorForWindowBorders;

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

        internal event EventHandler SystemWindowBorderPreferenceChanged;

        [MethodImpl(MethodImplOptions.NoInlining)]
        public void FetchSystemAccentColors()
        {
            var uiSettings = new UISettings();
            if (TryApplySystemAccentPalette(
                _colors,
                uiSettings.GetColorValue(UIColorType.Accent).ToColor(),
                uiSettings.GetColorValue(UIColorType.AccentDark1).ToColor(),
                uiSettings.GetColorValue(UIColorType.AccentDark2).ToColor(),
                uiSettings.GetColorValue(UIColorType.AccentDark3).ToColor(),
                uiSettings.GetColorValue(UIColorType.AccentLight1).ToColor(),
                uiSettings.GetColorValue(UIColorType.AccentLight2).ToColor(),
                uiSettings.GetColorValue(UIColorType.AccentLight3).ToColor()))
            {
                UpdateSystemAccentResources();
            }
            else if (!_colors.Contains(AccentKey))
            {
                SetAccent(DefaultAccentColor);
            }

            UpdateWindowBorderColor();
        }

        internal static bool TryApplySystemAccentPalette(
            ResourceDictionary colors,
            Color accent,
            Color accentDark1,
            Color accentDark2,
            Color accentDark3,
            Color accentLight1,
            Color accentLight2,
            Color accentLight3)
        {
            if (!IsUsableSystemColor(accent) ||
                !IsUsableSystemColor(accentDark1) ||
                !IsUsableSystemColor(accentDark2) ||
                !IsUsableSystemColor(accentDark3) ||
                !IsUsableSystemColor(accentLight1) ||
                !IsUsableSystemColor(accentLight2) ||
                !IsUsableSystemColor(accentLight3))
            {
                return false;
            }

            colors[AccentKey] = accent;
            colors[AccentDark1Key] = accentDark1;
            colors[AccentDark2Key] = accentDark2;
            colors[AccentDark3Key] = accentDark3;
            colors[AccentLight1Key] = accentLight1;
            colors[AccentLight2Key] = accentLight2;
            colors[AccentLight3Key] = accentLight3;
            return true;
        }

        private static bool IsUsableSystemColor(Color color)
        {
            return color.A != 0;
        }

        public void SetAccent(Color accent)
        {
            Color color = accent;
            _colors[AccentKey] = color;
            UpdateShades(_colors, color);
            UpdateWindowBorderColor();
            UpdateSystemAccentResources();
        }

        internal static Color GetWindowBorderColor(Color systemAccent, bool useAccentColor)
        {
            return useAccentColor && IsUsableSystemColor(systemAccent)
                ? systemAccent
                : DefaultWindowBorderColor;
        }

        internal void RefreshSystemColorValues()
        {
            if (!SystemColorsSupported)
            {
                return;
            }

            if (Dispatcher.CheckAccess())
            {
                UpdateColorValues();
            }
            else
            {
                Dispatcher.BeginInvoke(UpdateColorValues);
            }
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

        private void UpdateSystemAccentResources()
        {
#if NET10_0_OR_GREATER
            UpdateSystemAccentResource(SystemColors.AccentColorKey, SystemColors.AccentColorBrushKey, AccentKey);
            UpdateSystemAccentResource(SystemColors.AccentColorDark1Key, SystemColors.AccentColorDark1BrushKey, AccentDark1Key);
            UpdateSystemAccentResource(SystemColors.AccentColorDark2Key, SystemColors.AccentColorDark2BrushKey, AccentDark2Key);
            UpdateSystemAccentResource(SystemColors.AccentColorDark3Key, SystemColors.AccentColorDark3BrushKey, AccentDark3Key);
            UpdateSystemAccentResource(SystemColors.AccentColorLight1Key, SystemColors.AccentColorLight1BrushKey, AccentLight1Key);
            UpdateSystemAccentResource(SystemColors.AccentColorLight2Key, SystemColors.AccentColorLight2BrushKey, AccentLight2Key);
            UpdateSystemAccentResource(SystemColors.AccentColorLight3Key, SystemColors.AccentColorLight3BrushKey, AccentLight3Key);
#endif
        }

#if NET10_0_OR_GREATER
        private void UpdateSystemAccentResource(ResourceKey colorKey, ResourceKey brushKey, string modernWpfColorKey)
        {
            if (_colors[modernWpfColorKey] is Color color)
            {
                _colors[colorKey] = color;

                if (_colors[brushKey] is SolidColorBrush brush && !brush.IsFrozen)
                {
                    brush.SetCurrentValue(SolidColorBrush.ColorProperty, color);
                }
                else
                {
                    _colors[brushKey] = new SolidColorBrush(color);
                }
            }
        }
#endif

        public static void UpdateBrushes(ResourceDictionary themeDictionary, ResourceDictionary colors)
        {
            UpdateBrushes(
                themeDictionary,
                colors,
                new HashSet<ResourceDictionary>());
        }

        private static void UpdateBrushes(
            ResourceDictionary themeDictionary,
            ResourceDictionary colors,
            HashSet<ResourceDictionary> visited)
        {
            if (!visited.Add(themeDictionary))
            {
                return;
            }

            foreach (var mergedDictionary in themeDictionary.MergedDictionaries)
            {
                UpdateBrushes(mergedDictionary, colors, visited);
            }

            foreach (DictionaryEntry entry in themeDictionary)
            {
                if (entry.Value is SolidColorBrush brush && !brush.IsFrozen)
                {
                    UpdateColor(brush, SolidColorBrush.ColorProperty, colors);
                }
                else if (entry.Value is GradientBrush gradientBrush && !gradientBrush.IsFrozen)
                {
                    foreach (GradientStop gradientStop in gradientBrush.GradientStops)
                    {
                        if (!gradientStop.IsFrozen)
                        {
                            UpdateColor(gradientStop, GradientStop.ColorProperty, colors);
                        }
                    }
                }
            }
        }

        private static void UpdateColor(
            DependencyObject target,
            DependencyProperty colorProperty,
            ResourceDictionary colors)
        {
            object colorKey = ThemeResourceHelper.GetColorKey(target);
            if (colorKey != null && colors.Contains(colorKey))
            {
                target.SetCurrentValue(colorProperty, (Color)colors[colorKey]);
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
            var systemAccent = _uiSettings.GetColorValue(UIColorType.Accent).ToColor();
            _systemAccent = IsUsableSystemColor(systemAccent)
                ? systemAccent
                : DefaultAccentColor;
            _useAccentColorForWindowBorders = ReadUseAccentColorForWindowBorders();
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
            if (e.Category == UserPreferenceCategory.Color ||
                e.Category == UserPreferenceCategory.General)
            {
                UpdateColorValues();
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private void UpdateColorValues()
        {
            var background = _uiSettings.GetColorValue(UIColorType.Background).ToColor();
            bool systemThemeChanged = _systemBackground != background;
            bool systemAccentChanged = false;
            bool windowBorderPreferenceChanged = false;

            if (systemThemeChanged)
            {
                _systemBackground = background;
                UpdateSystemAppTheme();
            }

            var accent = _uiSettings.GetColorValue(UIColorType.Accent).ToColor();
            if (IsUsableSystemColor(accent) && _systemAccent != accent)
            {
                _systemAccent = accent;
                systemAccentChanged = true;
            }

            bool useAccentColorForWindowBorders = ReadUseAccentColorForWindowBorders();
            if (_useAccentColorForWindowBorders != useAccentColorForWindowBorders)
            {
                _useAccentColorForWindowBorders = useAccentColorForWindowBorders;
                windowBorderPreferenceChanged = true;
            }

            if (systemThemeChanged)
            {
                SystemThemeChanged?.Invoke(null, EventArgs.Empty);
            }

            if (systemAccentChanged)
            {
                SystemAccentColorChanged?.Invoke(null, EventArgs.Empty);
            }

            if (windowBorderPreferenceChanged)
            {
                SystemWindowBorderPreferenceChanged?.Invoke(null, EventArgs.Empty);
            }
        }

        private void UpdateWindowBorderColor()
        {
            _colors[WindowBorderColorKey] = GetWindowBorderColor(
                _systemAccent,
                _useAccentColorForWindowBorders);
        }

        private static bool ReadUseAccentColorForWindowBorders()
        {
            try
            {
                using var key = Registry.CurrentUser.OpenSubKey(DwmRegistryPath);
                return key?.GetValue(ColorPrevalenceValueName) is int value && value != 0;
            }
            catch (IOException)
            {
                return false;
            }
            catch (SecurityException)
            {
                return false;
            }
            catch (UnauthorizedAccessException)
            {
                return false;
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
