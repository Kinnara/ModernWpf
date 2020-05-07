using SamplesCommon;
using System;

namespace ModernWpf.SampleApp.Presets
{
    public class PresetManager : BindableBase
    {
        internal const string DefaultPreset = "Default";

        private string _colorPreset = DefaultPreset;
        private string _shapePreset = DefaultPreset;

        private PresetManager()
        {
        }

        public static PresetManager Current { get; } = new PresetManager();

        public string ColorPreset
        {
            get => _colorPreset;
            set
            {
                if (_colorPreset != value)
                {
                    _colorPreset = value;
                    RaisePropertyChanged();
                    ColorPresetChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        public string ShapePreset
        {
            get => _shapePreset;
            set
            {
                if (_shapePreset != value)
                {
                    _shapePreset = value;
                    RaisePropertyChanged();
                    ShapePresetChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        public event EventHandler ColorPresetChanged;
        public event EventHandler ShapePresetChanged;
    }
}
