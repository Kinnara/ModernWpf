using System.Collections.Generic;
using System.Resources;
using System.Runtime.CompilerServices;

namespace ModernWpf
{
    internal class ResourceAccessor
    {
        #region Resource Keys

        public const string SR_BasicRatingString = "BasicRatingString";
        public const string SR_CommunityRatingString = "CommunityRatingString";
        public const string SR_RatingsControlName = "RatingsControlName";
        public const string SR_RatingControlName = "RatingControlName";
        public const string SR_RatingUnset = "RatingUnset";
        public const string SR_NavigationButtonClosedName = "NavigationButtonClosedName";
        public const string SR_NavigationButtonOpenName = "NavigationButtonOpenName";
        public const string SR_NavigationViewItemDefaultControlName = "NavigationViewItemDefaultControlName";
        public const string SR_NavigationBackButtonName = "NavigationBackButtonName";
        public const string SR_NavigationBackButtonToolTip = "NavigationBackButtonToolTip";
        public const string SR_NavigationCloseButtonName = "NavigationCloseButtonName";
        public const string SR_NavigationOverflowButtonName = "NavigationOverflowButtonName";
        public const string SR_NavigationOverflowButtonText = "NavigationOverflowButtonText";
        public const string SR_NavigationOverflowButtonToolTip = "NavigationOverflowButtonToolTip";
        public const string SR_SettingsButtonName = "SettingsButtonName";
        public const string SR_NavigationViewSearchButtonName = "NavigationViewSearchButtonName";
        public const string SR_TextAlphaLabel = "TextAlphaLabel";
        public const string SR_AutomationNameAlphaSlider = "AutomationNameAlphaSlider";
        public const string SR_AutomationNameAlphaTextBox = "AutomationNameAlphaTextBox";
        public const string SR_AutomationNameHueSlider = "AutomationNameHueSlider";
        public const string SR_AutomationNameSaturationSlider = "AutomationNameSaturationSlider";
        public const string SR_AutomationNameValueSlider = "AutomationNameValueSlider";
        public const string SR_TextBlueLabel = "TextBlueLabel";
        public const string SR_AutomationNameBlueTextBox = "AutomationNameBlueTextBox";
        public const string SR_AutomationNameColorModelComboBox = "AutomationNameColorModelComboBox";
        public const string SR_AutomationNameColorSpectrum = "AutomationNameColorSpectrum";
        public const string SR_TextGreenLabel = "TextGreenLabel";
        public const string SR_AutomationNameGreenTextBox = "AutomationNameGreenTextBox";
        public const string SR_HelpTextColorSpectrum = "HelpTextColorSpectrum";
        public const string SR_AutomationNameHexTextBox = "AutomationNameHexTextBox";
        public const string SR_ContentHSVComboBoxItem = "ContentHSVComboBoxItem";
        public const string SR_TextHueLabel = "TextHueLabel";
        public const string SR_AutomationNameHueTextBox = "AutomationNameHueTextBox";
        public const string SR_LocalizedControlTypeColorSpectrum = "LocalizedControlTypeColorSpectrum";
        public const string SR_TextRedLabel = "TextRedLabel";
        public const string SR_AutomationNameRedTextBox = "AutomationNameRedTextBox";
        public const string SR_ContentRGBComboBoxItem = "ContentRGBComboBoxItem";
        public const string SR_TextSaturationLabel = "TextSaturationLabel";
        public const string SR_AutomationNameSaturationTextBox = "AutomationNameSaturationTextBox";
        public const string SR_TextValueLabel = "TextValueLabel";
        public const string SR_ValueStringColorSpectrumWithColorName = "ValueStringColorSpectrumWithColorName";
        public const string SR_ValueStringColorSpectrumWithoutColorName = "ValueStringColorSpectrumWithoutColorName";
        public const string SR_ValueStringHueSliderWithColorName = "ValueStringHueSliderWithColorName";
        public const string SR_ValueStringHueSliderWithoutColorName = "ValueStringHueSliderWithoutColorName";
        public const string SR_ValueStringSaturationSliderWithColorName = "ValueStringSaturationSliderWithColorName";
        public const string SR_ValueStringSaturationSliderWithoutColorName = "ValueStringSaturationSliderWithoutColorName";
        public const string SR_ValueStringValueSliderWithColorName = "ValueStringValueSliderWithColorName";
        public const string SR_ValueStringValueSliderWithoutColorName = "ValueStringValueSliderWithoutColorName";
        public const string SR_AutomationNameValueTextBox = "AutomationNameValueTextBox";
        public const string SR_ToolTipStringAlphaSlider = "ToolTipStringAlphaSlider";
        public const string SR_ToolTipStringHueSliderWithColorName = "ToolTipStringHueSliderWithColorName";
        public const string SR_ToolTipStringHueSliderWithoutColorName = "ToolTipStringHueSliderWithoutColorName";
        public const string SR_ToolTipStringSaturationSliderWithColorName = "ToolTipStringSaturationSliderWithColorName";
        public const string SR_ToolTipStringSaturationSliderWithoutColorName = "ToolTipStringSaturationSliderWithoutColorName";
        public const string SR_ToolTipStringValueSliderWithColorName = "ToolTipStringValueSliderWithColorName";
        public const string SR_ToolTipStringValueSliderWithoutColorName = "ToolTipStringValueSliderWithoutColorName";
        public const string SR_AutomationNameMoreButtonCollapsed = "AutomationNameMoreButtonCollapsed";
        public const string SR_AutomationNameMoreButtonExpanded = "AutomationNameMoreButtonExpanded";
        public const string SR_HelpTextMoreButton = "HelpTextMoreButton";
        public const string SR_TextMoreButtonLabelCollapsed = "TextMoreButtonLabelCollapsed";
        public const string SR_TextMoreButtonLabelExpanded = "TextMoreButtonLabelExpanded";
        public const string SR_BadgeItemPlural1 = "BadgeItemPlural1";
        public const string SR_BadgeItemPlural2 = "BadgeItemPlural2";
        public const string SR_BadgeItemPlural3 = "BadgeItemPlural3";
        public const string SR_BadgeItemPlural4 = "BadgeItemPlural4";
        public const string SR_BadgeItemPlural5 = "BadgeItemPlural5";
        public const string SR_BadgeItemPlural6 = "BadgeItemPlural6";
        public const string SR_BadgeItemPlural7 = "BadgeItemPlural7";
        public const string SR_BadgeItemSingular = "BadgeItemSingular";
        public const string SR_BadgeItemTextOverride = "BadgeItemTextOverride";
        public const string SR_BadgeIcon = "BadgeIcon";
        public const string SR_BadgeIconTextOverride = "BadgeIconTextOverride";
        public const string SR_PersonName = "PersonName";
        public const string SR_GroupName = "GroupName";
        public const string SR_CancelDraggingString = "CancelDraggingString";
        public const string SR_DefaultItemString = "DefaultItemString";
        public const string SR_DropIntoNodeString = "DropIntoNodeString";
        public const string SR_FallBackPlaceString = "FallBackPlaceString";
        public const string SR_PagerControlPageTextName = "PagerControlPageText";
        public const string SR_PagerControlPrefixTextName = "PagerControlPrefixText";
        public const string SR_PagerControlSuffixTextName = "PagerControlPrefixText";
        public const string SR_PagerControlFirstPageButtonTextName = "PagerControlFirstPageButtonText";
        public const string SR_PagerControlPreviousPageButtonTextName = "PagerControlPreviousPageButtonText";
        public const string SR_PagerControlNextPageButtonTextName = "PagerControlNextPageButtonText";
        public const string SR_PagerControlLastPageButtonTextName = "PagerControlLastPageButtonText";
        public const string SR_PipsPagerNameText = "PipsPagerNameText";
        public const string SR_PipsPagerNextPageButtonText = "PipsPagerNextPageButtonText";
        public const string SR_PipsPagerPreviousPageButtonText = "PipsPagerPreviousPageButtonText";
        public const string SR_PipsPagerPageText = "PipsPagerPageText";
        public const string SR_PlaceAfterString = "PlaceAfterString";
        public const string SR_PlaceBeforeString = "PlaceBeforeString";
        public const string SR_PlaceBetweenString = "PlaceBetweenString";
        public const string SR_ProgressRingName = "ProgressRingName";
        public const string SR_ProgressRingIndeterminateStatus = "ProgressRingIndeterminateStatus";
        public const string SR_ProgressBarIndeterminateStatus = "ProgressBarIndeterminateStatus";
        public const string SR_ProgressBarPausedStatus = "ProgressBarPausedStatus";
        public const string SR_ProgressBarErrorStatus = "ProgressBarErrorStatus";
        public const string SR_RatingLocalizedControlType = "RatingLocalizedControlType";
        public const string SR_SplitButtonSecondaryButtonName = "SplitButtonSecondaryButtonName";
        public const string SR_ProofingMenuItemLabel = "ProofingMenuItemLabel";
        public const string SR_TextCommandLabelCut = "TextCommandLabelCut";
        public const string SR_TextCommandLabelCopy = "TextCommandLabelCopy";
        public const string SR_TextCommandLabelPaste = "TextCommandLabelPaste";
        public const string SR_TextCommandLabelSelectAll = "TextCommandLabelSelectAll";
        public const string SR_TextCommandLabelBold = "TextCommandLabelBold";
        public const string SR_TextCommandLabelItalic = "TextCommandLabelItalic";
        public const string SR_TextCommandLabelUnderline = "TextCommandLabelUnderline";
        public const string SR_TextCommandLabelUndo = "TextCommandLabelUndo";
        public const string SR_TextCommandLabelRedo = "TextCommandLabelRedo";
        public const string SR_TextCommandDescriptionCut = "TextCommandDescriptionCut";
        public const string SR_TextCommandDescriptionCopy = "TextCommandDescriptionCopy";
        public const string SR_TextCommandDescriptionPaste = "TextCommandDescriptionPaste";
        public const string SR_TextCommandDescriptionSelectAll = "TextCommandDescriptionSelectAll";
        public const string SR_TextCommandDescriptionBold = "TextCommandDescriptionBold";
        public const string SR_TextCommandDescriptionItalic = "TextCommandDescriptionItalic";
        public const string SR_TextCommandDescriptionUnderline = "TextCommandDescriptionUnderline";
        public const string SR_TextCommandDescriptionUndo = "TextCommandDescriptionUndo";
        public const string SR_TextCommandDescriptionRedo = "TextCommandDescriptionRedo";
        public const string SR_TextCommandKeyboardAcceleratorKeyCut = "TextCommandKeyboardAcceleratorKeyCut";
        public const string SR_TextCommandKeyboardAcceleratorKeyCopy = "TextCommandKeyboardAcceleratorKeyCopy";
        public const string SR_TextCommandKeyboardAcceleratorKeyPaste = "TextCommandKeyboardAcceleratorKeyPaste";
        public const string SR_TextCommandKeyboardAcceleratorKeySelectAll = "TextCommandKeyboardAcceleratorKeySelectAll";
        public const string SR_TextCommandKeyboardAcceleratorKeyBold = "TextCommandKeyboardAcceleratorKeyBold";
        public const string SR_TextCommandKeyboardAcceleratorKeyItalic = "TextCommandKeyboardAcceleratorKeyItalic";
        public const string SR_TextCommandKeyboardAcceleratorKeyUnderline = "TextCommandKeyboardAcceleratorKeyUnderline";
        public const string SR_TextCommandKeyboardAcceleratorKeyUndo = "TextCommandKeyboardAcceleratorKeyUndo";
        public const string SR_TextCommandKeyboardAcceleratorKeyRedo = "TextCommandKeyboardAcceleratorKeyRedo";
        public const string SR_TeachingTipAlternateCloseButtonName = "TeachingTipAlternateCloseButtonName";
        public const string SR_TeachingTipAlternateCloseButtonTooltip = "TeachingTipAlternateCloseButtonTooltip";
        public const string SR_TeachingTipCustomLandmarkName = "TeachingTipCustomLandmarkName";
        public const string SR_TeachingTipNotification = "TeachingTipNotification";
        public const string SR_TeachingTipNotificationWithoutAppName = "TeachingTipNotificationWithoutAppName";
        public const string SR_TabViewAddButtonName = "TabViewAddButtonName";
        public const string SR_TabViewAddButtonTooltip = "TabViewAddButtonTooltip";
        public const string SR_TabViewCloseButtonName = "TabViewCloseButtonName";
        public const string SR_TabViewCloseButtonTooltip = "TabViewCloseButtonTooltip";
        public const string SR_TabViewCloseButtonTooltipWithKA = "TabViewCloseButtonTooltipWithKA";
        public const string SR_TabViewScrollDecreaseButtonTooltip = "TabViewScrollDecreaseButtonTooltip";
        public const string SR_TabViewScrollIncreaseButtonTooltip = "TabViewScrollIncreaseButtonTooltip";
        public const string SR_NumberBoxUpSpinButtonName = "NumberBoxUpSpinButtonName";
        public const string SR_NumberBoxDownSpinButtonName = "NumberBoxDownSpinButtonName";
        public const string SR_ExpanderDefaultControlName = "ExpanderDefaultControlName";

        public const string SR_InfoBarCloseButtonName = "InfoBarCloseButtonName";
        public const string SR_InfoBarOpenedNotification = "InfoBarOpenedNotification";
        public const string SR_InfoBarClosedNotification = "InfoBarClosedNotification";
        public const string SR_InfoBarCustomLandmarkName = "InfoBarCustomLandmarkName";
        public const string SR_InfoBarCloseButtonTooltip = "InfoBarCloseButtonTooltip";

        public const string IR_NoiseAsset_256X256_PNG = "NoiseAsset_256X256_PNG";

        #endregion

        #region RESX Specific workarounds

        private const string NavigationViewResourcesName = "ModernWpf.Controls.NavigationView.Strings.Resources";
        private const string NumberBoxResourcesName = "ModernWpf.Controls.NumberBox.Strings.Resources";
        private const string PersonPictureResourcesName = "ModernWpf.Controls.PersonPicture.Strings.Resources";
        private const string RatingControlResourcesName = "ModernWpf.Controls.RatingControl.Strings.Resources";
        private const string SplitButtonResourcesName = "ModernWpf.Controls.SplitButton.Strings.Resources";

        /// <summary>
        /// Used to map each resource keys to their respective <strong>*.resources files</strong>.
        /// </summary>
        /// <remarks>
        /// Update this map everytime the resources are modified.
        /// </remarks>
        private static readonly Dictionary<string, string> resourceMaps = new()
        {
            // NavigationView resources
            { SR_NavigationButtonClosedName, NavigationViewResourcesName },
            { SR_NavigationButtonOpenName, NavigationViewResourcesName },
            { SR_NavigationViewItemDefaultControlName, NavigationViewResourcesName },
            { SR_SettingsButtonName, NavigationViewResourcesName },
            { SR_NavigationViewSearchButtonName, NavigationViewResourcesName },
            { SR_NavigationBackButtonName, NavigationViewResourcesName },
            { SR_NavigationBackButtonToolTip, NavigationViewResourcesName },
            { SR_NavigationCloseButtonName, NavigationViewResourcesName },
            { SR_NavigationOverflowButtonName, NavigationViewResourcesName },
            { SR_NavigationOverflowButtonText, NavigationViewResourcesName },
            { SR_NavigationOverflowButtonToolTip, NavigationViewResourcesName },

            // NumberBox resources
            { SR_NumberBoxDownSpinButtonName, NumberBoxResourcesName },
            { SR_NumberBoxUpSpinButtonName, NumberBoxResourcesName },

            // PersonPicture resources
            { SR_BadgeItemPlural1, PersonPictureResourcesName },
            { SR_BadgeItemPlural2, PersonPictureResourcesName },
            { SR_BadgeItemPlural3, PersonPictureResourcesName },
            { SR_BadgeItemPlural4, PersonPictureResourcesName },
            { SR_BadgeItemPlural5, PersonPictureResourcesName },
            { SR_BadgeItemPlural6, PersonPictureResourcesName },
            { SR_BadgeItemPlural7, PersonPictureResourcesName },
            { SR_BadgeItemSingular, PersonPictureResourcesName },
            { SR_BadgeItemTextOverride, PersonPictureResourcesName },
            { SR_BadgeIcon, PersonPictureResourcesName },
            { SR_BadgeIconTextOverride, PersonPictureResourcesName },
            { SR_PersonName, PersonPictureResourcesName },
            { SR_GroupName, PersonPictureResourcesName },

            // RatingControl resources
            { SR_BasicRatingString, RatingControlResourcesName },
            { SR_CommunityRatingString, RatingControlResourcesName },
            { SR_RatingsControlName, RatingControlResourcesName },
            { SR_RatingControlName, RatingControlResourcesName },
            { SR_RatingUnset, RatingControlResourcesName },
            { SR_RatingLocalizedControlType, RatingControlResourcesName },

            // SplitButton resources
            { SR_SplitButtonSecondaryButtonName, SplitButtonResourcesName },
        };

        /// <summary>
        /// Used to cache <see cref="ResourceManager"/> instances associated with a particular
        /// <strong>*.resources file</strong> (generated from a *.resx file)
        /// </summary>
        private static readonly Dictionary<string, ResourceManager> resourceManagers = new();

        #endregion

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static string GetLocalizedStringResource(string resourceName)
        {
            if (resourceMaps.TryGetValue(resourceName, out string resourcesFilePath))
            {
                if (!resourceManagers.TryGetValue(resourcesFilePath, out ResourceManager resourceManager))
                {
                    resourceManager = new(resourcesFilePath, typeof(ResourceAccessor).Assembly);
                    resourceManagers.Add(resourcesFilePath, resourceManager);
                }

                if (resourceManager != null)
                {
                    return resourceManager.GetString(resourceName);
                }
            }

            return string.Empty;
        }
    }
}
