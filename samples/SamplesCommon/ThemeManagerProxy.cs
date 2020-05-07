using ModernWpf;
using System.ComponentModel;
using System.Windows.Media;

namespace SamplesCommon
{
    public class ThemeManagerProxy : BindableBase
    {
        private ThemeManagerProxy()
        {
            DispatcherHelper.RunOnMainThread(() =>
            {
                DependencyPropertyDescriptor.FromProperty(ThemeManager.ApplicationThemeProperty, typeof(ThemeManager))
                .AddValueChanged(ThemeManager.Current, delegate { UpdateApplicationTheme(); });

                DependencyPropertyDescriptor.FromProperty(ThemeManager.ActualApplicationThemeProperty, typeof(ThemeManager))
                .AddValueChanged(ThemeManager.Current, delegate { UpdateActualApplicationTheme(); });

                DependencyPropertyDescriptor.FromProperty(ThemeManager.AccentColorProperty, typeof(ThemeManager))
                .AddValueChanged(ThemeManager.Current, delegate { UpdateAccentColor(); });

                DependencyPropertyDescriptor.FromProperty(ThemeManager.ActualAccentColorProperty, typeof(ThemeManager))
                .AddValueChanged(ThemeManager.Current, delegate { UpdateActualAccentColor(); });

                UpdateApplicationTheme();
                UpdateActualApplicationTheme();
                UpdateAccentColor();
                UpdateActualAccentColor();
            });
        }

        public static ThemeManagerProxy Current { get; } = new ThemeManagerProxy();

        #region ApplicationTheme

        public ApplicationTheme? ApplicationTheme
        {
            get => _applicationTheme;
            set
            {
                if (_applicationTheme != value)
                {
                    Set(ref _applicationTheme, value);

                    if (!_updatingApplicationTheme)
                    {
                        DispatcherHelper.RunOnMainThread(() =>
                        {
                            ThemeManager.Current.ApplicationTheme = _applicationTheme;
                        });
                    }
                }
            }
        }

        private void UpdateApplicationTheme()
        {
            _updatingApplicationTheme = true;
            ApplicationTheme = ThemeManager.Current.ApplicationTheme;
            _updatingApplicationTheme = false;
        }

        private ApplicationTheme? _applicationTheme;
        private bool _updatingApplicationTheme;

        #endregion

        #region ActualApplicationTheme

        public ApplicationTheme ActualApplicationTheme
        {
            get => _actualApplicationTheme;
            private set => Set(ref _actualApplicationTheme, value);
        }

        private void UpdateActualApplicationTheme()
        {
            ActualApplicationTheme = ThemeManager.Current.ActualApplicationTheme;
        }

        private ApplicationTheme _actualApplicationTheme;

        #endregion

        #region AccentColor

        public Color? AccentColor
        {
            get => _accentColor;
            set
            {
                if (_accentColor != value)
                {
                    Set(ref _accentColor, value);

                    if (!_updatingAccentColor)
                    {
                        DispatcherHelper.RunOnMainThread(() =>
                        {
                            ThemeManager.Current.AccentColor = _accentColor;
                        });
                    }
                }
            }
        }

        private void UpdateAccentColor()
        {
            _updatingAccentColor = true;
            AccentColor = ThemeManager.Current.AccentColor;
            _updatingAccentColor = false;
        }

        private Color? _accentColor;
        private bool _updatingAccentColor;

        #endregion

        #region ActualAccentColor

        public Color ActualAccentColor
        {
            get => _actualAccentColor;
            private set => Set(ref _actualAccentColor, value);
        }

        private void UpdateActualAccentColor()
        {
            ActualAccentColor = ThemeManager.Current.ActualAccentColor;
        }

        private Color _actualAccentColor;

        #endregion
    }
}
