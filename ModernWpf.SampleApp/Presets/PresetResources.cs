using System;
using System.Windows;

namespace ModernWpf.SampleApp.Presets
{
    public class PresetResources : ResourceDictionary
    {
        private ApplicationTheme _targetTheme;

        public PresetResources()
        {
            PresetManager.Current.CurrentPresetChanged += OnCurrentPresetChanged; // TODO: Prevent memory leak
            ApplyCurrentPreset();
        }

        public ApplicationTheme TargetTheme
        {
            get => _targetTheme;
            set
            {
                if (_targetTheme != value)
                {
                    _targetTheme = value;
                    ApplyCurrentPreset();
                }
            }
        }

        private void OnCurrentPresetChanged(object sender, EventArgs e)
        {
            ApplyCurrentPreset();
        }

        private void ApplyCurrentPreset()
        {
            MergedDictionaries.Clear();
            string assemblyName = GetType().Assembly.GetName().Name;
            string currentPreset = PresetManager.Current.CurrentPreset;
            var source = new Uri($"pack://application:,,,/{assemblyName};component/Presets/{currentPreset}/{TargetTheme}.xaml");
            var rd = new ResourceDictionary { Source = source };
            MergedDictionaries.Add(rd);
        }
    }
}
