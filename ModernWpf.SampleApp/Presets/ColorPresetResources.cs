using System;
using System.Windows;

namespace ModernWpf.SampleApp.Presets
{
    public class ColorPresetResources : ResourceDictionary
    {
        private ApplicationTheme _targetTheme;

        public ColorPresetResources()
        {
            WeakEventManager<PresetManager, EventArgs>.AddHandler(
                PresetManager.Current,
                nameof(PresetManager.ColorPresetChanged),
                OnCurrentPresetChanged);

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
            if (MergedDictionaries.Count > 0)
            {
                MergedDictionaries.Clear();
            }
            
            string assemblyName = GetType().Assembly.GetName().Name;
            string currentPreset = PresetManager.Current.ColorPreset;
            var source = new Uri($"pack://application:,,,/{assemblyName};component/Presets/{currentPreset}/{TargetTheme}.xaml");
            var rd = new ResourceDictionary { Source = source };
            MergedDictionaries.Add(rd);
        }
    }
}
