using System;
using System.ComponentModel;
using System.Windows;

namespace ModernWpf
{
    public class DesignTimeThemeResources : ResourceDictionary
    {
        private static DesignTimeThemeResources _current;

        private ApplicationTheme _theme;
        private Uri _lightSource = ThemeManager.DefaultLightSource;
        private Uri _darkSource = ThemeManager.DefaultDarkSource;
        private Uri _highContrastSource = ThemeManager.DefaultHighContrastSource;

        public DesignTimeThemeResources()
        {
            if (!DesignMode.DesignModeEnabled)
            {
                return;
            }

            UpdateSource();
            Current = this;
        }

        internal static DesignTimeThemeResources Current
        {
            get => _current;
            set
            {
                if (_current != null)
                {
                    SystemParameters.StaticPropertyChanged -= _current.OnSystemParametersPropertyChanged;
                }

                _current = value;

                if (_current != null)
                {
                    SystemParameters.StaticPropertyChanged += _current.OnSystemParametersPropertyChanged;
                }
            }
        }

        public ApplicationTheme Theme
        {
            get => _theme;
            set
            {
                _theme = value;
                UpdateSource();
            }
        }

        public Uri LightSource
        {
            get => _lightSource;
            set
            {
                _lightSource = value;
                UpdateSource();
            }
        }

        public Uri DarkSource
        {
            get => _darkSource;
            set
            {
                _darkSource = value;
                UpdateSource();
            }
        }

        public Uri HighContrastSource
        {
            get => _highContrastSource;
            set
            {
                _highContrastSource = value;
                UpdateSource();
            }
        }

        private void UpdateSource()
        {
            if (!DesignMode.DesignModeEnabled)
            {
                return;
            }

            Source = SystemParameters.HighContrast ?
                HighContrastSource :
                Theme == ApplicationTheme.Dark ?
                    DarkSource :
                    LightSource;
        }

        private void OnSystemParametersPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(SystemParameters.HighContrast))
            {
                UpdateSource();
            }
        }
    }
}
