// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Windows.Media;
using ModernWpf.Media.Utils;

namespace ModernWpf.Media.ColorPalette
{
    // These classes are not intended to be viewmodels.
    // They deal with the data about an editable palette and are passed to special purpose controls for editing
    internal class ColorPalette
    {
        public ColorPalette(int steps, Color baseColor)
            : this(steps, new ColorPaletteEntry(baseColor))
        { }

        public ColorPalette(int steps, IColorPaletteEntry baseColor)
        {
            if (baseColor == null)
            {
                throw new ArgumentNullException("baseColor");
            }
            if (_steps <= 0)
            {
                throw new ArgumentException("steps must > 0");
            }

            _steps = steps;
            _baseColor = baseColor;

            var scale = GetPaletteScale();

            _palette = new List<ColorPaletteEntry>(_steps);
            for (int i = 0; i < _steps; i++)
            {
                var c = scale.GetColor((double)i / (double)(_steps - 1), _interpolationMode);
                _palette.Add(new ColorPaletteEntry(c));
            }
        }

        protected readonly IColorPaletteEntry _baseColor;

        protected readonly int _steps = 11;

        protected readonly List<ColorPaletteEntry> _palette;
        public IReadOnlyList<ColorPaletteEntry> Palette => _palette;

        protected ColorScaleInterpolationMode _interpolationMode = ColorScaleInterpolationMode.RGB;

        protected Color _scaleColorLight = Color.FromArgb(0xFF, 0xFF, 0xFF, 0xFF);

        protected Color _scaleColorDark = Color.FromArgb(0xFF, 0x00, 0x00, 0x00);

        protected double _clipLight = 0.185;

        protected double _clipDark = 0.160;

        protected double _saturationAdjustmentCutoff = 0.05;

        protected double _saturationLight = 0.35;

        protected double _saturationDark = 1.25;

        protected double _overlayLight = 0.0;

        protected double _overlayDark = 0.25;

        protected double _multiplyLight = 0.0;

        protected double _multiplyDark = 0.0;

        public ColorScale GetPaletteScale()
        {
            var baseColorRGB = _baseColor.ActiveColor;
            var baseColorHSL = ColorUtils.RGBToHSL(baseColorRGB);
            var baseColorNormalized = new NormalizedRGB(baseColorRGB);

            var baseScale = new ColorScale(new Color[] { _scaleColorLight, baseColorRGB, _scaleColorDark, });

            var trimmedScale = baseScale.Trim(_clipLight, 1.0 - _clipDark);
            var trimmedLight = new NormalizedRGB(trimmedScale.GetColor(0, _interpolationMode));
            var trimmedDark = new NormalizedRGB(trimmedScale.GetColor(1, _interpolationMode));

            var adjustedLight = trimmedLight;
            var adjustedDark = trimmedDark;

            if (baseColorHSL.S >= _saturationAdjustmentCutoff)
            {

                adjustedLight = ColorBlending.SaturateViaLCH(adjustedLight, _saturationLight);
                adjustedDark = ColorBlending.SaturateViaLCH(adjustedDark, _saturationDark);
            }

            if (_multiplyLight != 0)
            {
                var multiply = ColorBlending.Blend(baseColorNormalized, adjustedLight, ColorBlendMode.Multiply);
                adjustedLight = ColorUtils.InterpolateColor(adjustedLight, multiply, _multiplyLight, _interpolationMode);
            }

            if (_multiplyDark != 0)
            {
                var multiply = ColorBlending.Blend(baseColorNormalized, adjustedDark, ColorBlendMode.Multiply);
                adjustedDark = ColorUtils.InterpolateColor(adjustedDark, multiply, _multiplyDark, _interpolationMode);
            }

            if (_overlayLight != 0)
            {
                var overlay = ColorBlending.Blend(baseColorNormalized, adjustedLight, ColorBlendMode.Overlay);
                adjustedLight = ColorUtils.InterpolateColor(adjustedLight, overlay, _overlayLight, _interpolationMode);
            }

            if (_overlayDark != 0)
            {
                var overlay = ColorBlending.Blend(baseColorNormalized, adjustedDark, ColorBlendMode.Overlay);
                adjustedDark = ColorUtils.InterpolateColor(adjustedDark, overlay, _overlayDark, _interpolationMode);
            }

            var finalScale = new ColorScale(new Color[] { adjustedLight.Denormalize(), baseColorRGB, adjustedDark.Denormalize() });
            return finalScale;
        }
    }
}
