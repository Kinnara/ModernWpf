using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Threading;
using Microsoft.Win32;
using Windows.Foundation.Metadata;
using Windows.UI.ViewManagement;

namespace ModernWpf
{
    internal class UISettingsResources : ResourceDictionary
    {
        private const string UniversalApiContractName = "Windows.Foundation.UniversalApiContract";
        private const string AutoHideScrollBarsKey = "AutoHideScrollBars";

        private readonly Dispatcher _dispatcher = Dispatcher.CurrentDispatcher;
        private UISettings _uiSettings;

        public UISettingsResources()
        {
            if (DesignMode.DesignModeEnabled)
            {
                return;
            }

            if (OSVersionHelper.IsWindows10OrGreater)
            {
                Initialize();
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private void Initialize()
        {
            _uiSettings = new UISettings();

            if (ApiInformation.IsApiContractPresent(UniversalApiContractName, 4))
            {
                InitializeForContract4();
            }

            if (ApiInformation.IsApiContractPresent(UniversalApiContractName, 8))
            {
                InitializeForContract8();
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private void InitializeForContract4()
        {
            _uiSettings.AdvancedEffectsEnabledChanged += (sender, args) =>
            {
                _dispatcher.BeginInvoke(ApplyAdvancedEffectsEnabled);
            };

            if (PackagedAppHelper.IsPackagedApp)
            {
                SystemEvents.UserPreferenceChanged += (sender, args) =>
                {
                    if (args.Category == UserPreferenceCategory.General)
                    {
                        ApplyAdvancedEffectsEnabled();
                    }
                };
            }

            ApplyAdvancedEffectsEnabled();
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private void InitializeForContract8()
        {
            _uiSettings.AutoHideScrollBarsChanged += (sender, args) =>
            {
                _dispatcher.BeginInvoke(ApplyAutoHideScrollBars);
            };
            ApplyAutoHideScrollBars();
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private void ApplyAdvancedEffectsEnabled()
        {
            var key = SystemParameters.DropShadowKey;
            if (_uiSettings.AdvancedEffectsEnabled)
            {
                Remove(key);
            }
            else
            {
                this[key] = false;
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private void ApplyAutoHideScrollBars()
        {
            this[AutoHideScrollBarsKey] = _uiSettings.AutoHideScrollBars;
        }
    }
}
