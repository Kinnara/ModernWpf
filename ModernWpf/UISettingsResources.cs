using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Threading;
using Windows.Foundation.Metadata;
using Windows.UI.ViewManagement;

namespace ModernWpf
{
    internal class UISettingsResources : ResourceDictionary
    {
        private const string UniversalApiContractName = "Windows.Foundation.UniversalApiContract";
        private const string AutoHideScrollBarsKey = "AutoHideScrollBars";

        private object _uiSettings;

        public UISettingsResources()
        {
            if (DesignMode.DesignModeEnabled)
            {
                return;
            }

            if (OSVersionHelper.IsWindows10)
            {
                Initialize();
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private void Initialize()
        {
            var uiSettings = new UISettings();

            if (ApiInformation.IsApiContractPresent(UniversalApiContractName, 4))
            {
                InitializeForContract4(uiSettings);
            }

            if (ApiInformation.IsApiContractPresent(UniversalApiContractName, 8))
            {
                InitializeForContract8(uiSettings);
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

        [MethodImpl(MethodImplOptions.NoInlining)]
        private void InitializeForContract8(UISettings settings)
        {
            settings.AutoHideScrollBarsChanged += (sender, args) =>
            {
                Application.Current.Dispatcher.BeginInvoke(() =>
                {
                    ApplyAutoHideScrollBars(sender.AutoHideScrollBars);
                });
            };
            ApplyAutoHideScrollBars(settings.AutoHideScrollBars);
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

        private void ApplyAutoHideScrollBars(bool value)
        {
            this[AutoHideScrollBarsKey] = value;
        }
    }
}
