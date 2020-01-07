using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Threading;
using Windows.Foundation.Metadata;
using Windows.UI.ViewManagement;

namespace SamplesCommon
{
    public class UISettingsResources : ResourceDictionary
    {
        private object _uiSettings;

        public UISettingsResources()
        {
            if (!DesignerProperties.GetIsInDesignMode(new DependencyObject()))
            {
                if (Environment.OSVersion.Version.Major >= 10)
                {
                    Initialize();
                }
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private void Initialize()
        {
            if (_uiSettings != null)
            {
                return;
            }

            var uiSettings = new UISettings();

            if (ApiInformation.IsApiContractPresent("Windows.Foundation.UniversalApiContract", 4))
            {
                InitializeForContract4(uiSettings);
            }

            _uiSettings = uiSettings;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private void InitializeForContract4(UISettings settings)
        {
            settings.AdvancedEffectsEnabledChanged += (sender, args) =>
            {
                Application.Current.Dispatcher.BeginInvoke(() =>
                {
                    ApplyAdvancedEffectsEnabled(sender.AdvancedEffectsEnabled);
                });
            };
            ApplyAdvancedEffectsEnabled(settings.AdvancedEffectsEnabled);
        }

        private void ApplyAdvancedEffectsEnabled(bool value)
        {
            var key = SystemParameters.DropShadowKey;
            if (value)
            {
                Remove(key);
            }
            else
            {
                this[key] = false;
            }
        }
    }
}
