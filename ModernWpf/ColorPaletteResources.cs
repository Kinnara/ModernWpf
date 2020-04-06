using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media;

namespace ModernWpf
{
    /// <summary>
    /// Represents a specialized resource dictionary that contains color resources used
    /// by XAML elements.
    /// </summary>
    public class ColorPaletteResources : ResourceDictionary, ISupportInitialize
    {
        /// <summary>
        /// Initializes a new instance of the ColorPaletteResources class.
        /// </summary>
        public ColorPaletteResources()
        {
        }

        private ApplicationTheme? _targetTheme;
        public ApplicationTheme? TargetTheme
        {
            get => _targetTheme;
            set
            {
                if (_targetTheme.HasValue)
                {
                    throw new InvalidOperationException(nameof(TargetTheme) + " cannot be changed after it's set.");
                }

                if (_targetTheme != value)
                {
                    _targetTheme = value;
                    UpdateBrushes();
                }
            }
        }

        private Color? _accent;
        /// <summary>
        /// Gets or sets the Accent color value.
        /// </summary>
        /// <returns>The Accent color value.</returns>
        public Color? Accent
        {
            get => _accent;
            set
            {
                if (Set(ref _accent, value, updateBrushes: false))
                {
                    UpdateAccentShades();

                    if (TargetTheme.HasValue)
                    {
                        UpdateBrushes();
                    }
                };
            }
        }

        private Color? _altHigh;
        /// <summary>
        /// Gets or sets the AltHigh color value.
        /// </summary>
        /// <returns>The AltHigh color value.</returns>
        public Color? AltHigh
        {
            get => _altHigh;
            set => Set(ref _altHigh, value);
        }

        private Color? _altLow;
        /// <summary>
        /// Gets or sets the AltLow color value.
        /// </summary>
        /// <returns>The AltLow color value.</returns>
        public Color? AltLow
        {
            get => _altLow;
            set => Set(ref _altLow, value);
        }

        private Color? _altMedium;
        /// <summary>
        /// Gets or sets the AltMedium color value.
        /// </summary>
        /// <returns>The AltMedium color value.</returns>
        public Color? AltMedium
        {
            get => _altMedium;
            set => Set(ref _altMedium, value);
        }

        private Color? _altMediumHigh;
        /// <summary>
        /// Gets or sets the AltMediumHigh color value.
        /// </summary>
        /// <returns>The AltMediumHigh color value.</returns>
        public Color? AltMediumHigh
        {
            get => _altMediumHigh;
            set => Set(ref _altMediumHigh, value);
        }

        private Color? _altMediumLow;
        /// <summary>
        /// Gets or sets the AltMediumLow color value.
        /// </summary>
        /// <returns>The AltMediumLow color value.</returns>
        public Color? AltMediumLow
        {
            get => _altMediumLow;
            set => Set(ref _altMediumLow, value);
        }

        private Color? _baseHigh;
        /// <summary>
        /// Gets or sets the BaseHigh color value.
        /// </summary>
        /// <returns>The BaseHigh color value.</returns>
        public Color? BaseHigh
        {
            get => _baseHigh;
            set => Set(ref _baseHigh, value);
        }

        private Color? _baseLow;
        /// <summary>
        /// Gets or sets the BaseLow color value.
        /// </summary>
        /// <returns>The BaseLow color value.</returns>
        public Color? BaseLow
        {
            get => _baseLow;
            set => Set(ref _baseLow, value);
        }

        private Color? _baseMedium;
        /// <summary>
        /// Gets or sets the BaseMedium color value.
        /// </summary>
        /// <returns>The BaseMedium color value.</returns>
        public Color? BaseMedium
        {
            get => _baseMedium;
            set => Set(ref _baseMedium, value);
        }

        private Color? _baseMediumHigh;
        /// <summary>
        /// Gets or sets the BaseMediumHigh color value.
        /// </summary>
        /// <returns>The BaseMediumHigh color value.</returns>
        public Color? BaseMediumHigh
        {
            get => _baseMediumHigh;
            set => Set(ref _baseMediumHigh, value);
        }

        private Color? _baseMediumLow;
        /// <summary>
        /// Gets or sets the BaseMediumLow color value.
        /// </summary>
        /// <returns>The BaseMediumLow color value.</returns>
        public Color? BaseMediumLow
        {
            get => _baseMediumLow;
            set => Set(ref _baseMediumLow, value);
        }

        private Color? _chromeAltLow;
        /// <summary>
        /// Gets or sets the ChromeAltLow color value.
        /// </summary>
        /// <returns>The ChromeAltLow color value.</returns>
        public Color? ChromeAltLow
        {
            get => _chromeAltLow;
            set => Set(ref _chromeAltLow, value);
        }

        private Color? _chromeBlackHigh;
        /// <summary>
        /// Gets or sets the ChromeBlackHigh color value.
        /// </summary>
        /// <returns>The ChromeBlackHigh color value.</returns>
        public Color? ChromeBlackHigh
        {
            get => _chromeBlackHigh;
            set => Set(ref _chromeBlackHigh, value);
        }

        private Color? _chromeBlackLow;
        /// <summary>
        /// Gets or sets the ChromeBlackLow color value.
        /// </summary>
        /// <returns>The ChromeBlackLow color value.</returns>
        public Color? ChromeBlackLow
        {
            get => _chromeBlackLow;
            set => Set(ref _chromeBlackLow, value);
        }

        private Color? _chromeBlackMedium;
        /// <summary>
        /// Gets or sets the ChromeBlackMedium color value.
        /// </summary>
        /// <returns>The ChromeBlackMedium color value.</returns>
        public Color? ChromeBlackMedium
        {
            get => _chromeBlackMedium;
            set => Set(ref _chromeBlackMedium, value);
        }

        private Color? _chromeBlackMediumLow;
        /// <summary>
        /// Gets or sets the ChromeBlackMediumLow color value.
        /// </summary>
        /// <returns>The ChromeBlackMediumLow color value.</returns>
        public Color? ChromeBlackMediumLow
        {
            get => _chromeBlackMediumLow;
            set => Set(ref _chromeBlackMediumLow, value);
        }

        private Color? _chromeDisabledHigh;
        /// <summary>
        /// Gets or sets the ChromeDisabledHigh color value.
        /// </summary>
        /// <returns>The ChromeDisabledHigh color value.</returns>
        public Color? ChromeDisabledHigh
        {
            get => _chromeDisabledHigh;
            set => Set(ref _chromeDisabledHigh, value);
        }

        private Color? _chromeDisabledLow;
        /// <summary>
        /// Gets or sets the ChromeDisabledLow color value.
        /// </summary>
        /// <returns>The ChromeDisabledLow color value.</returns>
        public Color? ChromeDisabledLow
        {
            get => _chromeDisabledLow;
            set => Set(ref _chromeDisabledLow, value);
        }

        private Color? _chromeGray;
        /// <summary>
        /// Gets or sets the ChromeGray color value.
        /// </summary>
        /// <returns>The ChromeGray color value.</returns>
        public Color? ChromeGray
        {
            get => _chromeGray;
            set => Set(ref _chromeGray, value);
        }

        private Color? _chromeHigh;
        /// <summary>
        /// Gets or sets the ChromeHigh color value.
        /// </summary>
        /// <returns>The ChromeHigh color value.</returns>
        public Color? ChromeHigh
        {
            get => _chromeHigh;
            set => Set(ref _chromeHigh, value);
        }

        private Color? _chromeLow;
        /// <summary>
        /// Gets or sets the ChromeLow color value.
        /// </summary>
        /// <returns>The ChromeLow color value.</returns>
        public Color? ChromeLow
        {
            get => _chromeLow;
            set => Set(ref _chromeLow, value);
        }

        private Color? _chromeMedium;
        /// <summary>
        /// Gets or sets the ChromeMedium color value.
        /// </summary>
        /// <returns>The ChromeMedium color value.</returns>
        public Color? ChromeMedium
        {
            get => _chromeMedium;
            set => Set(ref _chromeMedium, value);
        }

        private Color? _chromeMediumLow;
        /// <summary>
        /// Gets or sets the ChromeMediumLow color value.
        /// </summary>
        /// <returns>The ChromeMediumLow color value.</returns>
        public Color? ChromeMediumLow
        {
            get => _chromeMediumLow;
            set => Set(ref _chromeMediumLow, value);
        }

        private Color? _chromeWhite;
        /// <summary>
        /// Gets or sets the ChromeWhite color value.
        /// </summary>
        /// <returns>The ChromeWhite color value.</returns>
        public Color? ChromeWhite
        {
            get => _chromeWhite;
            set => Set(ref _chromeWhite, value);
        }

        private Color? _errorText;
        /// <summary>
        /// Gets or sets the ErrorText color value.
        /// </summary>
        /// <returns>The ErrorText color value.</returns>
        public Color? ErrorText
        {
            get => _errorText;
            set => Set(ref _errorText, value);
        }

        private Color? _listLow;
        /// <summary>
        /// Gets or sets the ListLow color value.
        /// </summary>
        /// <returns>The ListLow color value.</returns>
        public Color? ListLow
        {
            get => _listLow;
            set => Set(ref _listLow, value);
        }

        private Color? _listMedium;
        /// <summary>
        /// Gets or sets the ListMedium color value.
        /// </summary>
        /// <returns>The ListMedium color value.</returns>
        public Color? ListMedium
        {
            get => _listMedium;
            set => Set(ref _listMedium, value);
        }

        private bool Set(ref Color? storage, Color? value, bool updateBrushes = true, [CallerMemberName]string propertyName = null)
        {
            if (storage != value)
            {
                string key = "System" + propertyName + "Color";

                if (storage.HasValue)
                {
                    Remove(key);
                }

                storage = value;

                if (storage.HasValue)
                {
                    Add(key, storage.Value);
                }

                if (TargetTheme.HasValue && updateBrushes)
                {
                    UpdateBrushes();
                }

                return true;
            }

            return false;
        }

        private void UpdateAccentShades()
        {
            if (IsInitializePending)
            {
                return;
            }

            if (Accent.HasValue)
            {
                ColorsHelper.UpdateShades(this, Accent.Value);
            }
            else
            {
                ColorsHelper.RemoveShades(this);
            }
        }

        private void UpdateBrushes()
        {
            if (IsInitializePending)
            {
                return;
            }

            if (MergedDictionaries.Count > 0)
            {
                MergedDictionaries.Clear();
            }

            if (TargetTheme == null || Count == 0)
            {
                return;
            }

            var originals = ThemeManager.GetDefaultThemeDictionary(TargetTheme.Value.ToString());
            var overrides = new ResourceDictionary();
            var originalsToOverrides = new Dictionary<SolidColorBrush, SolidColorBrush>();

            // TODO: recursive
            foreach (DictionaryEntry entry in originals)
            {
                if (entry.Value is SolidColorBrush originalBrush)
                {
                    object colorKey = ThemeResourceHelper.GetColorKey(originalBrush);
                    if (colorKey != null && Contains(colorKey))
                    {
                        if (!originalsToOverrides.TryGetValue(originalBrush, out SolidColorBrush overrideBrush))
                        {
                            overrideBrush = originalBrush.CloneCurrentValue();
                            overrideBrush.Color = (Color)this[colorKey];
                            originalsToOverrides[originalBrush] = overrideBrush;
                        }
                        overrides.Add(entry.Key, overrideBrush);
                    }
                }
            }

            MergedDictionaries.Add(overrides);
        }

        #region ISupportInitialize

        private bool IsInitializePending { get; set; }

        public new void BeginInit()
        {
            base.BeginInit();

            IsInitializePending = true;
        }

        public new void EndInit()
        {
            base.EndInit();

            IsInitializePending = false;

            if (Accent.HasValue)
            {
                UpdateAccentShades();
            }

            UpdateBrushes();
        }

        void ISupportInitialize.BeginInit()
        {
            BeginInit();
        }

        void ISupportInitialize.EndInit()
        {
            EndInit();
        }

        #endregion
    }
}
