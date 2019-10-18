using ModernWpf.SampleApp.Common;
using System;

namespace ModernWpf.SampleApp.Presets
{
    public class PresetManager : BindableBase
    {
        internal const string DefaultPreset = "Default";

        private string _currentPreset = DefaultPreset;

        private PresetManager()
        {
        }

        public static PresetManager Current { get; } = new PresetManager();

        public string CurrentPreset
        {
            get => _currentPreset;
            set
            {
                if (_currentPreset != value)
                {
                    _currentPreset = value;
                    RaisePropertyChanged();
                    OnCurrentPresetChanged();
                }
            }
        }

        public event EventHandler CurrentPresetChanged;

        private void OnCurrentPresetChanged()
        {
            CurrentPresetChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
