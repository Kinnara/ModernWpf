using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Xml.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModernWpf;
using ModernWpf.Controls;
using ModernWpf.WinUI.TestApp;
using ModernWpf.WinUI.TestInfra;
using XamlControlsResources = ModernWpf.Controls.XamlControlsResources;

namespace ModernWpf.WinUI.Tests.CommonStyles;

[TestClass]
public class CommonStylesResourceTests
{
    [TestMethod]
    public void VerifyAllThemesContainSameResourceKeys()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var resourceDictionaries = CreateThemeResourceSnapshot();
            var defaultThemeDictionary = resourceDictionaries.ThemeDictionaries["Default"];
            var missingKeys = new List<string>();

            foreach (var dictionaryName in resourceDictionaries.ThemeDictionaries.Keys)
            {
                if (Equals(dictionaryName, "Default"))
                {
                    continue;
                }

                var themeDictionary = resourceDictionaries.ThemeDictionaries[dictionaryName];
                missingKeys.AddRange(FindMissingKeys(defaultThemeDictionary, "Default", themeDictionary, dictionaryName.ToString()!));
                missingKeys.AddRange(FindMissingKeys(themeDictionary, dictionaryName.ToString()!, defaultThemeDictionary, "Default"));
            }

            Assert.AreEqual(0, resourceDictionaries.MergedDictionaries.Count);
            AssertMissingKeys(missingKeys);
        });
    }

    [TestMethod]
    public void VerifyUseCompactResourcesAPI()
    {
        WpfTestHost.Run(() =>
        {
            var resources = new XamlControlsResources();
            var height = resources["TreeViewItemMinHeight"].ToString();

            resources.UseCompactResources = true;
            var compactHeight = resources["TreeViewItemMinHeight"].ToString();
            Assert.AreNotEqual(height, compactHeight);
            Assert.AreEqual("24", compactHeight);

            resources.UseCompactResources = false;
            Assert.AreEqual(height, resources["TreeViewItemMinHeight"].ToString());
        });
    }

    [TestMethod]
    public void VerifyFinalWinUI2CompactResourceValues()
    {
        WpfTestHost.Run(() =>
        {
            var resources = new XamlControlsResources
            {
                UseCompactResources = true
            };

            AssertResource(resources, "ControlContentThemeFontSize", 14.0);
            AssertResource(resources, "ContentControlFontSize", 14.0);
            AssertResource(resources, "TextControlThemeMinHeight", 24.0);
            AssertResource(resources, "TextControlThemePadding", new Thickness(2, 2, 6, 1));
            AssertResource(resources, "PopupCornerRadius", new CornerRadius(8));
            AssertResource(resources, "ListViewItemMinHeight", 32.0);
            AssertResource(resources, "TreeViewItemMinHeight", 24.0);
            AssertResource(resources, "TreeViewItemMultiSelectCheckBoxMinHeight", 24.0);
            AssertResource(resources, "TreeViewItemPresenterMargin", 0.0);
            AssertResource(resources, "TreeViewItemPresenterPadding", 0.0);
            AssertResource(resources, "TimePickerHostPadding", new Thickness(0, 1, 0, 2));
            AssertResource(resources, "DatePickerHostPadding", new Thickness(0, 1, 0, 2));
            AssertResource(resources, "DatePickerHostMonthPadding", new Thickness(9, 0, 0, 1));
            AssertResource(resources, "ComboBoxEditableTextPadding", new Thickness(10, 0, 30, 0));
            AssertResource(resources, "ComboBoxMinHeight", 24.0);
            AssertResource(resources, "ComboBoxPadding", new Thickness(12, 1, 0, 3));
            AssertResource(resources, "NavigationViewItemOnLeftMinHeight", 32.0);
        });
    }

    [TestMethod]
    public void ThemeResourcesUseWinUI2DateTimeAndFlipViewHighContrastTokens()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            foreach (var themeName in new[] { "Light", "Dark", "HighContrast" })
            {
                AssertThemeResourceValue(themeName, "DatePickerHeaderThemeMargin", new Thickness(0, 0, 0, 4));
                AssertThemeResourceValue(themeName, "DateTimeFlyoutBorderThickness", new Thickness(1));
                AssertThemeResourceValue(themeName, "DateTimeFlyoutBorderPadding", new Thickness(0));
                AssertThemeResourceValue(themeName, "DateTimeFlyoutButtonBorderThickness", new Thickness(0));
                AssertThemeResourceValue(themeName, "FlipViewButtonBorderThemeThickness", new Thickness(0));
                AssertThemeResourceReference(themeName, "FlipViewNextPreviousButtonBackground", "AcrylicInAppFillColorDefaultBrush");
                AssertThemeResourceReference(themeName, "FlipViewNextPreviousButtonBackgroundPointerOver", "AcrylicInAppFillColorDefaultBrush");
                AssertThemeResourceReference(themeName, "FlipViewNextPreviousButtonBackgroundPressed", "AcrylicInAppFillColorDefaultBrush");
            }

            foreach (var themeName in new[] { "Light", "Dark" })
            {
                AssertThemeResourceReference(themeName, "DateTimePickerFlyoutButtonBackground", "SubtleFillColorTransparentBrush");
                AssertThemeResourceReference(themeName, "DateTimePickerFlyoutButtonBackgroundPointerOver", "SubtleFillColorSecondaryBrush");
                AssertThemeResourceReference(themeName, "DateTimePickerFlyoutButtonBackgroundPressed", "SubtleFillColorTertiaryBrush");
                AssertThemeResourceReference(themeName, "DateTimePickerFlyoutButtonBorderBrush", "ControlStrokeColorDefaultBrush");
                AssertThemeResourceReference(themeName, "DateTimePickerFlyoutButtonBorderBrushPointerOver", "ControlStrokeColorDefaultBrush");
                AssertThemeResourceReference(themeName, "DateTimePickerFlyoutButtonBorderBrushPressed", "ControlStrokeColorDefaultBrush");
                AssertThemeResourceReference(themeName, "DateTimePickerFlyoutButtonForegroundPointerOver", "TextFillColorPrimaryBrush");
                AssertThemeResourceReference(themeName, "DateTimePickerFlyoutButtonForegroundPressed", "TextFillColorPrimaryBrush");
                AssertThemeResourceReference(themeName, "FlipViewBackground", "SolidBackgroundFillColorBaseBrush");
                AssertThemeResourceReference(themeName, "FlipViewNextPreviousArrowForeground", "ControlStrongFillColorDefaultBrush");
                AssertThemeResourceReference(themeName, "FlipViewNextPreviousArrowForegroundPointerOver", "TextFillColorSecondaryBrush");
                AssertThemeResourceReference(themeName, "FlipViewNextPreviousArrowForegroundPressed", "TextFillColorSecondaryBrush");
                AssertThemeResourceReference(themeName, "FlipViewNextPreviousButtonBorderBrush", "ControlStrokeColorDefaultBrush");
                AssertThemeResourceReference(themeName, "FlipViewNextPreviousButtonBorderBrushPointerOver", "ControlStrokeColorDefaultBrush");
                AssertThemeResourceReference(themeName, "FlipViewNextPreviousButtonBorderBrushPressed", "ControlStrokeColorDefaultBrush");
                AssertThemeResourceReference(themeName, "FlipViewItemBackground", "SubtleFillColorTransparentBrush");
            }

            AssertThemeResourceReference("HighContrast", "DateTimePickerFlyoutButtonBackground", "SystemControlTransparentBrush");
            AssertThemeResourceReference("HighContrast", "DateTimePickerFlyoutButtonBackgroundPointerOver", "SystemControlHighlightListLowBrush");
            AssertThemeResourceReference("HighContrast", "DateTimePickerFlyoutButtonBackgroundPressed", "SystemControlHighlightListMediumBrush");
            AssertThemeResourceReference("HighContrast", "DateTimePickerFlyoutButtonBorderBrush", "SystemControlForegroundTransparentBrush");
            AssertThemeResourceReference("HighContrast", "DateTimePickerFlyoutButtonBorderBrushPointerOver", "SystemControlHighlightTransparentBrush");
            AssertThemeResourceReference("HighContrast", "DateTimePickerFlyoutButtonBorderBrushPressed", "SystemControlHighlightTransparentBrush");
            AssertThemeResourceReference("HighContrast", "DateTimePickerFlyoutButtonForegroundPointerOver", "SystemControlHighlightAltBaseHighBrush");
            AssertThemeResourceReference("HighContrast", "DateTimePickerFlyoutButtonForegroundPressed", "SystemControlHighlightAltBaseHighBrush");
            AssertThemeResourceReference("HighContrast", "FlipViewBackground", "SystemControlPageBackgroundListLowBrush");
            AssertThemeResourceReference("HighContrast", "FlipViewNextPreviousArrowForeground", "SystemControlForegroundAltMediumHighBrush");
            AssertThemeResourceReference("HighContrast", "FlipViewNextPreviousArrowForegroundPointerOver", "SystemControlHighlightAltAltMediumHighBrush");
            AssertThemeResourceReference("HighContrast", "FlipViewNextPreviousArrowForegroundPressed", "SystemControlHighlightAltAltMediumHighBrush");
            AssertThemeResourceReference("HighContrast", "FlipViewNextPreviousButtonBorderBrush", "SystemControlForegroundTransparentBrush");
            AssertThemeResourceReference("HighContrast", "FlipViewNextPreviousButtonBorderBrushPointerOver", "SystemControlForegroundTransparentBrush");
            AssertThemeResourceReference("HighContrast", "FlipViewNextPreviousButtonBorderBrushPressed", "SystemControlForegroundTransparentBrush");
            AssertThemeResourceReference("HighContrast", "FlipViewItemBackground", "SystemControlTransparentBrush");
        });
    }

    [TestMethod]
    public void ThemeResourcesUseWinUI2CalendarPickerHighContrastTokens()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            foreach (var themeName in new[] { "Light", "Dark" })
            {
                AssertThemeResourceValue(themeName, "CalendarDatePickerBorderThemeThickness", new Thickness(1));
                AssertThemeResourceReference(themeName, "CalendarDatePickerForeground", "TextFillColorPrimaryBrush");
                AssertThemeResourceReference(themeName, "CalendarDatePickerForegroundDisabled", "TextFillColorDisabledBrush");
                AssertThemeResourceReference(themeName, "CalendarDatePickerCalendarGlyphForeground", "TextFillColorSecondaryBrush");
                AssertThemeResourceReference(themeName, "CalendarDatePickerCalendarGlyphForegroundPointerOver", "TextFillColorSecondaryBrush");
                AssertThemeResourceReference(themeName, "CalendarDatePickerCalendarGlyphForegroundPressed", "TextFillColorSecondaryBrush");
                AssertThemeResourceReference(themeName, "CalendarDatePickerCalendarGlyphForegroundDisabled", "TextFillColorDisabledBrush");
                AssertThemeResourceReference(themeName, "CalendarDatePickerTextForeground", "TextFillColorPrimaryBrush");
                AssertThemeResourceReference(themeName, "CalendarDatePickerTextForegroundPointerOver", "TextFillColorPrimaryBrush");
                AssertThemeResourceReference(themeName, "CalendarDatePickerTextForegroundPressed", "TextFillColorSecondaryBrush");
                AssertThemeResourceReference(themeName, "CalendarDatePickerTextForegroundDisabled", "TextFillColorDisabledBrush");
                AssertThemeResourceReference(themeName, "CalendarDatePickerTextForegroundSelected", "TextFillColorPrimaryBrush");
                AssertThemeResourceReference(themeName, "CalendarDatePickerHeaderForeground", "TextFillColorPrimaryBrush");
                AssertThemeResourceReference(themeName, "CalendarDatePickerHeaderForegroundDisabled", "TextFillColorDisabledBrush");
                AssertThemeResourceReference(themeName, "CalendarDatePickerBackground", "ControlFillColorDefaultBrush");
                AssertThemeResourceReference(themeName, "CalendarDatePickerBackgroundPointerOver", "ControlFillColorSecondaryBrush");
                AssertThemeResourceReference(themeName, "CalendarDatePickerBackgroundPressed", "ControlFillColorTertiaryBrush");
                AssertThemeResourceReference(themeName, "CalendarDatePickerBackgroundDisabled", "ControlFillColorDisabledBrush");
                AssertThemeResourceReference(themeName, "CalendarDatePickerBackgroundFocused", "ControlFillColorDefaultBrush");
                AssertThemeResourceReference(themeName, "CalendarDatePickerBorderBrush", "ControlElevationBorderBrush");
                AssertThemeResourceReference(themeName, "CalendarDatePickerBorderBrushPointerOver", "ControlElevationBorderBrush");
                AssertThemeResourceReference(themeName, "CalendarDatePickerBorderBrushPressed", "ControlStrokeColorDefaultBrush");
                AssertThemeResourceReference(themeName, "CalendarDatePickerBorderBrushDisabled", "ControlStrokeColorDefaultBrush");
                AssertThemeResourceReference(themeName, "CalendarDatePickerLightDismissOverlayBackground", "SystemControlPageBackgroundMediumAltMediumBrush");

                AssertThemeResourceReference(themeName, "DatePickerForeground", "TextFillColorPrimaryBrush");
                AssertThemeResourceReference(themeName, "DatePickerBackground", "ControlFillColorDefaultBrush");
                AssertThemeResourceReference(themeName, "DatePickerBackgroundFocused", "ControlFillColorInputActiveBrush");
                AssertThemeResourceReference(themeName, "DatePickerBackgroundPointerOver", "ControlFillColorSecondaryBrush");
                AssertThemeResourceReference(themeName, "DatePickerPopupBackground", "AcrylicBackgroundFillColorDefaultBrush");
                AssertThemeResourceReference(themeName, "DatePickerTextBoxCaretBrush", "TextFillColorPrimaryBrush");

                AssertThemeResourceReference(themeName, "CalendarViewSelectedHoverBorderBrush", "AccentFillColorSecondaryBrush");
                AssertThemeResourceReference(themeName, "CalendarViewSelectedPressedBorderBrush", "SubtleFillColorTertiaryBrush");
                AssertThemeResourceReference(themeName, "CalendarViewSelectedBorderBrush", "AccentFillColorDefaultBrush");
                AssertThemeResourceReference(themeName, "CalendarViewHoverBorderBrush", "ControlStrokeColorDefaultBrush");
                AssertThemeResourceReference(themeName, "CalendarViewPressedBorderBrush", "ControlStrokeColorDefaultBrush");
                AssertThemeResourceReference(themeName, "CalendarViewTodayForeground", "TextOnAccentFillColorPrimaryBrush");
                AssertThemeResourceReference(themeName, "CalendarViewBlackoutForeground", "TextFillColorDisabledBrush");
                AssertThemeResourceReference(themeName, "CalendarViewSelectedForeground", "TextFillColorPrimaryBrush");
                AssertThemeResourceReference(themeName, "CalendarViewPressedForeground", "TextFillColorPrimaryBrush");
                AssertThemeResourceReference(themeName, "CalendarViewOutOfScopeForeground", "SystemControlHyperlinkBaseHighBrush");
                AssertThemeResourceReference(themeName, "CalendarViewCalendarItemForeground", "TextFillColorPrimaryBrush");
                AssertThemeResourceReference(themeName, "CalendarViewOutOfScopeBackground", "SystemControlDisabledChromeMediumLowBrush");
                AssertThemeResourceReference(themeName, "CalendarViewCalendarItemBackground", "ControlFillColorInputActiveBrush");
                AssertThemeResourceReference(themeName, "CalendarViewForeground", "TextFillColorPrimaryBrush");
                AssertThemeResourceReference(themeName, "CalendarViewBackground", "ControlFillColorInputActiveBrush");
                AssertThemeResourceReference(themeName, "CalendarViewBorderBrush", "ControlStrokeColorDefaultBrush");
                AssertThemeResourceReference(themeName, "CalendarViewWeekDayForegroundDisabled", "TextFillColorDisabledBrush");
                AssertThemeResourceReference(themeName, "CalendarViewItemBackgroundPointerOver", "SubtleFillColorSecondaryBrush");
                AssertThemeResourceReference(themeName, "CalendarViewNavigationButtonBackground", "SubtleFillColorTransparentBrush");
                AssertThemeResourceReference(themeName, "CalendarViewNavigationButtonForeground", "TextFillColorSecondaryBrush");
                AssertThemeResourceReference(themeName, "CalendarViewNavigationButtonForegroundPointerOver", "TextFillColorPrimaryBrush");
                AssertThemeResourceReference(themeName, "CalendarViewNavigationButtonForegroundPressed", "ControlAltFillColorTertiaryBrush");
                AssertThemeResourceReference(themeName, "CalendarViewNavigationButtonForegroundDisabled", "TextFillColorDisabledBrush");
                AssertThemeResourceReference(themeName, "CalendarViewCalendarItemRevealBackground", "SystemControlTransparentRevealBackgroundBrush");
                AssertThemeResourceReference(themeName, "CalendarViewCalendarItemRevealBorderBrush", "SystemControlTransparentRevealBorderBrush");
                AssertThemeResourceReference(themeName, "CalendarViewNavigationButtonBorderBrushPointerOver", "ControlStrokeColorDefaultBrush");
                AssertThemeResourceReference(themeName, "CalendarViewNavigationButtonBorderBrush", "SubtleFillColorTransparentBrush");
            }

            AssertThemeResourceReference("Light", "CalendarViewFocusBorderBrush", "TextFillColorPrimaryBrush");
            AssertThemeResourceReference("Light", "CalendarViewSelectedBackground", "SystemAccentColorDark1Brush");
            AssertThemeResourceReference("Light", "CalendarViewTodayBackground", "SystemAccentColorDark1Brush");
            AssertThemeResourceReference("Dark", "CalendarViewFocusBorderBrush", "AccentFillColorSecondaryBrush");
            AssertThemeResourceReference("Dark", "CalendarViewSelectedBackground", "SystemAccentColorLight2Brush");
            AssertThemeResourceReference("Dark", "CalendarViewTodayBackground", "SystemAccentColorLight2Brush");

            AssertThemeResourceValue("HighContrast", "CalendarDatePickerBorderThemeThickness", new Thickness(1));
            AssertThemeResourceReference("HighContrast", "CalendarDatePickerForeground", "SystemColorButtonTextColorBrush");
            AssertThemeResourceReference("HighContrast", "CalendarDatePickerForegroundDisabled", "SystemColorGrayTextColorBrush");
            AssertThemeResourceReference("HighContrast", "CalendarDatePickerCalendarGlyphForeground", "SystemColorButtonTextColorBrush");
            AssertThemeResourceReference("HighContrast", "CalendarDatePickerCalendarGlyphForegroundPointerOver", "SystemColorHighlightColorBrush");
            AssertThemeResourceReference("HighContrast", "CalendarDatePickerCalendarGlyphForegroundPressed", "SystemColorHighlightColorBrush");
            AssertThemeResourceReference("HighContrast", "CalendarDatePickerCalendarGlyphForegroundDisabled", "SystemColorGrayTextColorBrush");
            AssertThemeResourceReference("HighContrast", "CalendarDatePickerTextForeground", "SystemColorButtonTextColorBrush");
            AssertThemeResourceReference("HighContrast", "CalendarDatePickerTextForegroundPointerOver", "SystemColorHighlightColorBrush");
            AssertThemeResourceReference("HighContrast", "CalendarDatePickerTextForegroundPressed", "SystemColorHighlightColorBrush");
            AssertThemeResourceReference("HighContrast", "CalendarDatePickerTextForegroundDisabled", "SystemColorGrayTextColorBrush");
            AssertThemeResourceReference("HighContrast", "CalendarDatePickerTextForegroundSelected", "SystemColorHighlightColorBrush");
            AssertThemeResourceReference("HighContrast", "CalendarDatePickerHeaderForeground", "SystemColorButtonTextColorBrush");
            AssertThemeResourceReference("HighContrast", "CalendarDatePickerHeaderForegroundDisabled", "SystemColorGrayTextColorBrush");
            AssertThemeResourceReference("HighContrast", "CalendarDatePickerBackground", "SystemColorButtonFaceColorBrush");
            AssertThemeResourceReference("HighContrast", "CalendarDatePickerBackgroundPointerOver", "SystemColorHighlightTextColorBrush");
            AssertThemeResourceReference("HighContrast", "CalendarDatePickerBackgroundPressed", "SystemColorWindowColorBrush");
            AssertThemeResourceReference("HighContrast", "CalendarDatePickerBackgroundDisabled", "SystemColorWindowColorBrush");
            AssertThemeResourceReference("HighContrast", "CalendarDatePickerBackgroundFocused", "SystemColorHighlightColorBrush");
            AssertThemeResourceReference("HighContrast", "CalendarDatePickerBorderBrush", "SystemColorButtonTextColorBrush");
            AssertThemeResourceReference("HighContrast", "CalendarDatePickerBorderBrushPointerOver", "SystemColorHighlightColorBrush");
            AssertThemeResourceReference("HighContrast", "CalendarDatePickerBorderBrushPressed", "SystemColorHighlightColorBrush");
            AssertThemeResourceReference("HighContrast", "CalendarDatePickerBorderBrushDisabled", "SystemColorGrayTextColorBrush");
            AssertThemeResourceReference("HighContrast", "CalendarDatePickerLightDismissOverlayBackground", "SystemControlPageBackgroundMediumAltMediumBrush");

            AssertThemeResourceReference("HighContrast", "DatePickerForeground", "SystemColorWindowTextColorBrush");
            AssertThemeResourceReference("HighContrast", "DatePickerBackground", "SystemColorWindowColorBrush");
            AssertThemeResourceReference("HighContrast", "DatePickerBackgroundFocused", "SystemColorHighlightTextColorBrush");
            AssertThemeResourceReference("HighContrast", "DatePickerBackgroundPointerOver", "SystemColorHighlightTextColorBrush");
            AssertThemeResourceReference("HighContrast", "DatePickerPopupBackground", "SystemColorWindowColorBrush");
            AssertThemeResourceReference("HighContrast", "DatePickerTextBoxCaretBrush", "SystemColorButtonTextColorBrush");

            AssertThemeResourceReference("HighContrast", "CalendarViewFocusBorderBrush", "SystemControlForegroundBaseHighBrush");
            AssertThemeResourceReference("HighContrast", "CalendarViewSelectedHoverBorderBrush", "SystemControlHighlightListAccentMediumBrush");
            AssertThemeResourceReference("HighContrast", "CalendarViewSelectedPressedBorderBrush", "SystemControlHighlightListAccentHighBrush");
            AssertThemeResourceReference("HighContrast", "CalendarViewSelectedBorderBrush", "SystemControlHighlightAccentBrush");
            AssertThemeResourceReference("HighContrast", "CalendarViewHoverBorderBrush", "SystemControlHighlightBaseMediumLowBrush");
            AssertThemeResourceReference("HighContrast", "CalendarViewPressedBorderBrush", "SystemControlHighlightBaseMediumBrush");
            AssertThemeResourceReference("HighContrast", "CalendarViewTodayForeground", "SystemControlHighlightAltChromeWhiteBrush");
            AssertThemeResourceReference("HighContrast", "CalendarViewBlackoutForeground", "SystemControlDisabledBaseMediumLowBrush");
            AssertThemeResourceReference("HighContrast", "CalendarViewSelectedForeground", "SystemControlHighlightBaseHighBrush");
            AssertThemeResourceReference("HighContrast", "CalendarViewPressedForeground", "SystemControlHighlightBaseHighBrush");
            AssertThemeResourceReference("HighContrast", "CalendarViewOutOfScopeForeground", "SystemControlHyperlinkBaseHighBrush");
            AssertThemeResourceReference("HighContrast", "CalendarViewCalendarItemForeground", "SystemControlForegroundBaseHighBrush");
            AssertThemeResourceReference("HighContrast", "CalendarViewOutOfScopeBackground", "SystemControlDisabledChromeMediumLowBrush");
            AssertThemeResourceReference("HighContrast", "CalendarViewCalendarItemBackground", "SystemControlBackgroundAltHighBrush");
            AssertThemeResourceReference("HighContrast", "CalendarViewForeground", "SystemControlHyperlinkBaseMediumHighBrush");
            AssertThemeResourceReference("HighContrast", "CalendarViewBackground", "SystemControlBackgroundAltHighBrush");
            AssertThemeResourceReference("HighContrast", "CalendarViewBorderBrush", "SystemControlForegroundChromeMediumBrush");
            AssertThemeResourceReference("HighContrast", "CalendarViewWeekDayForegroundDisabled", "SystemControlDisabledBaseMediumLowBrush");
            AssertThemeResourceReference("HighContrast", "CalendarViewItemBackgroundPointerOver", "SystemColorHighlightTextColorBrush");
            AssertThemeResourceReference("HighContrast", "CalendarViewSelectedBackground", "SystemColorHighlightColorBrush");
            AssertThemeResourceReference("HighContrast", "CalendarViewTodayBackground", "SystemColorHighlightColorBrush");
            AssertThemeResourceReference("HighContrast", "CalendarViewNavigationButtonBackground", "SystemControlTransparentBrush");
            AssertThemeResourceReference("HighContrast", "CalendarViewNavigationButtonForeground", "SystemColorButtonTextColorBrush");
            AssertThemeResourceReference("HighContrast", "CalendarViewNavigationButtonForegroundPointerOver", "SystemControlForegroundBaseHighBrush");
            AssertThemeResourceReference("HighContrast", "CalendarViewNavigationButtonForegroundPressed", "SystemControlForegroundBaseMediumBrush");
            AssertThemeResourceReference("HighContrast", "CalendarViewNavigationButtonForegroundDisabled", "SystemControlDisabledBaseMediumLowBrush");
            AssertThemeResourceReference("HighContrast", "CalendarViewCalendarItemRevealBackground", "SystemControlBackgroundAltHighBrush");
            AssertThemeResourceReference("HighContrast", "CalendarViewCalendarItemRevealBorderBrush", "SystemControlTransparentRevealBorderBrush");
            AssertThemeResourceReference("HighContrast", "CalendarViewNavigationButtonBorderBrushPointerOver", "SystemControlHighlightTransparentBrush");
            AssertThemeResourceReference("HighContrast", "CalendarViewNavigationButtonBorderBrush", "SystemControlTransparentBrush");
        });
    }

    [TestMethod]
    public void ProductXamlStyleResourceReferencesResolveToDeclaredKeys()
    {
        var repoRoot = FindRepoRoot();
        var xamlFiles = GetProductXamlFiles(repoRoot).ToArray();
        var declaredKeys = new HashSet<string>();
        var resourceReferenceRegex = new Regex(@"\{(?:DynamicResource|StaticResource)\s+([^},]+)", RegexOptions.CultureInvariant);

        foreach (var path in xamlFiles)
        {
            var document = XDocument.Load(path);
            foreach (var keyAttribute in document.Descendants().Attributes().Where(IsXamlKeyAttribute))
            {
                declaredKeys.Add(keyAttribute.Value);
            }
        }

        var missingKeys = new SortedSet<string>();
        foreach (var path in xamlFiles)
        {
            var relativePath = Path.GetRelativePath(repoRoot, path);
            var document = XDocument.Load(path);
            foreach (var attribute in document.Descendants().Attributes())
            {
                foreach (Match match in resourceReferenceRegex.Matches(attribute.Value))
                {
                    var resourceKey = match.Groups[1].Value.Trim();
                    if (!IsSimpleStringResourceKey(resourceKey) || declaredKeys.Contains(resourceKey))
                    {
                        continue;
                    }

                    missingKeys.Add($"{resourceKey} referenced by {relativePath}");
                }
            }
        }

        AssertMissingKeys(missingKeys);
    }

    [TestMethod]
    public void ThemeResourcesUseWinUI2TimePickerAndLoopingSelectorHighContrastTokens()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            foreach (var themeName in new[] { "Light", "Dark", "HighContrast" })
            {
                AssertThemeResourceValue(themeName, "TimePickerSelectorThemeMinWidth", 80.0);
                AssertThemeResourceValue(themeName, "TimePickerSpacerThemeWidth", 1.0);
                AssertThemeResourceValue(themeName, "TimePickerBorderThemeThickness", new Thickness(1));
                AssertThemeResourceValue(themeName, "TimePickerHeaderThemeMargin", new Thickness(0, 0, 0, 4));
                AssertThemeResourceValue(themeName, "TimePickerFirstHostThemeMargin", new Thickness(0, 0, 20, 0));
                AssertThemeResourceValue(themeName, "TimePickerThirdHostThemeMargin", new Thickness(20, 0, 0, 0));
                AssertThemeResourceValue(themeName, "TimePickerHeaderThemeFontWeight", FontWeights.Normal);
                AssertThemeResourceReference(themeName, "TimePickerLightDismissOverlayBackground", "SystemControlPageBackgroundMediumAltMediumBrush");

                AssertThemeResourceReference(themeName, "LoopingSelectorButtonBackground", "SystemControlBackgroundChromeMediumBrush");
                AssertThemeResourceReference(themeName, "LoopingSelectorItemForeground", "SystemControlForegroundBaseHighBrush");
                AssertThemeResourceReference(themeName, "LoopingSelectorItemForegroundSelected", "SystemControlHighlightAltBaseHighBrush");
                AssertThemeResourceReference(themeName, "LoopingSelectorItemForegroundPointerOver", "SystemControlHighlightAltBaseHighBrush");
                AssertThemeResourceReference(themeName, "LoopingSelectorItemForegroundPressed", "SystemControlHighlightAltBaseHighBrush");
                AssertThemeResourceReference(themeName, "LoopingSelectorItemBackgroundPointerOver", "SystemControlHighlightListLowBrush");
                AssertThemeResourceReference(themeName, "LoopingSelectorItemBackgroundPressed", "SystemControlHighlightListMediumBrush");
            }

            foreach (var themeName in new[] { "Light", "Dark" })
            {
                AssertThemeResourceReference(themeName, "TimePickerSpacerFill", "DividerStrokeColorDefaultBrush");
                AssertThemeResourceReference(themeName, "TimePickerSpacerFillDisabled", "AccentFillColorDisabledBrush");
                AssertThemeResourceReference(themeName, "TimePickerHeaderForeground", "TextFillColorPrimaryBrush");
                AssertThemeResourceReference(themeName, "TimePickerHeaderForegroundDisabled", "TextFillColorDisabledBrush");
                AssertThemeResourceReference(themeName, "TimePickerButtonBorderBrush", "ControlAltFillColorTertiaryBrush");
                AssertThemeResourceReference(themeName, "TimePickerButtonBorderBrushPointerOver", "ControlStrongStrokeColorDefaultBrush");
                AssertThemeResourceReference(themeName, "TimePickerButtonBorderBrushPressed", "ControlStrokeColorDefaultBrush");
                AssertThemeResourceReference(themeName, "TimePickerButtonBorderBrushDisabled", "AccentFillColorDisabledBrush");
                AssertThemeResourceReference(themeName, "TimePickerButtonBackground", "ControlFillColorDefaultBrush");
                AssertThemeResourceReference(themeName, "TimePickerButtonBackgroundPointerOver", "SubtleFillColorSecondaryBrush");
                AssertThemeResourceReference(themeName, "TimePickerButtonBackgroundPressed", "ControlFillColorTertiaryBrush");
                AssertThemeResourceReference(themeName, "TimePickerButtonBackgroundDisabled", "ControlFillColorTertiaryBrush");
                AssertThemeResourceReference(themeName, "TimePickerButtonBackgroundFocused", "SubtleFillColorTertiaryBrush");
                AssertThemeResourceReference(themeName, "TimePickerButtonForeground", "TextFillColorPrimaryBrush");
                AssertThemeResourceReference(themeName, "TimePickerButtonForegroundPointerOver", "TextFillColorPrimaryBrush");
                AssertThemeResourceReference(themeName, "TimePickerButtonForegroundPressed", "TextFillColorPrimaryBrush");
                AssertThemeResourceReference(themeName, "TimePickerButtonForegroundDisabled", "TextFillColorDisabledBrush");
                AssertThemeResourceReference(themeName, "TimePickerButtonForegroundFocused", "TextFillColorPrimaryBrush");
                AssertThemeResourceReference(themeName, "TimePickerFlyoutPresenterBackground", "AcrylicBackgroundFillColorDefaultBrush");
                AssertThemeResourceReference(themeName, "TimePickerFlyoutPresenterBorderBrush", "SurfaceStrokeColorFlyoutBrush");
                AssertThemeResourceReference(themeName, "TimePickerFlyoutPresenterSpacerFill", "DividerStrokeColorDefaultBrush");
                AssertThemeResourceReference(themeName, "TimePickerFlyoutPresenterHighlightFill", "SubtleFillColorTertiaryBrush");
            }

            AssertThemeResourceReference("HighContrast", "TimePickerSpacerFill", "SystemControlForegroundBaseLowBrush");
            AssertThemeResourceReference("HighContrast", "TimePickerSpacerFillDisabled", "SystemControlDisabledBaseLowBrush");
            AssertThemeResourceReference("HighContrast", "TimePickerHeaderForeground", "SystemControlForegroundBaseHighBrush");
            AssertThemeResourceReference("HighContrast", "TimePickerHeaderForegroundDisabled", "SystemControlDisabledBaseMediumLowBrush");
            AssertThemeResourceReference("HighContrast", "TimePickerButtonBorderBrush", "SystemControlForegroundBaseMediumBrush");
            AssertThemeResourceReference("HighContrast", "TimePickerButtonBorderBrushPointerOver", "SystemControlHighlightBaseMediumHighBrush");
            AssertThemeResourceReference("HighContrast", "TimePickerButtonBorderBrushPressed", "SystemControlHighlightBaseMediumBrush");
            AssertThemeResourceReference("HighContrast", "TimePickerButtonBorderBrushDisabled", "SystemControlDisabledBaseLowBrush");
            AssertThemeResourceReference("HighContrast", "TimePickerButtonBackground", "SystemControlBackgroundAltMediumLowBrush");
            AssertThemeResourceReference("HighContrast", "TimePickerButtonBackgroundPointerOver", "SystemControlPageBackgroundAltMediumBrush");
            AssertThemeResourceReference("HighContrast", "TimePickerButtonBackgroundPressed", "SystemControlBackgroundBaseLowBrush");
            AssertThemeResourceReference("HighContrast", "TimePickerButtonBackgroundDisabled", "SystemControlBackgroundBaseLowBrush");
            AssertThemeResourceReference("HighContrast", "TimePickerButtonBackgroundFocused", "SystemControlHighlightListAccentLowBrush");
            AssertThemeResourceReference("HighContrast", "TimePickerButtonForeground", "SystemControlForegroundBaseHighBrush");
            AssertThemeResourceReference("HighContrast", "TimePickerButtonForegroundPointerOver", "SystemControlHighlightBaseHighBrush");
            AssertThemeResourceReference("HighContrast", "TimePickerButtonForegroundPressed", "SystemControlHighlightBaseHighBrush");
            AssertThemeResourceReference("HighContrast", "TimePickerButtonForegroundDisabled", "SystemControlDisabledBaseMediumLowBrush");
            AssertThemeResourceReference("HighContrast", "TimePickerButtonForegroundFocused", "SystemControlHighlightAltBaseHighBrush");
            AssertThemeResourceReference("HighContrast", "TimePickerFlyoutPresenterBackground", "SystemControlBackgroundChromeMediumLowBrush");
            AssertThemeResourceReference("HighContrast", "TimePickerFlyoutPresenterBorderBrush", "SystemControlTransientBorderBrush");
            AssertThemeResourceReference("HighContrast", "TimePickerFlyoutPresenterSpacerFill", "SystemControlForegroundBaseLowBrush");
            AssertThemeResourceReference("HighContrast", "TimePickerFlyoutPresenterHighlightFill", "SystemControlHighlightListAccentLowBrush");
        });
    }

    [TestMethod]
    public void CornerRadiusFilterConverterTest()
    {
        WpfTestHost.Run(() =>
        {
            var root = (StackPanel)XamlReader.Parse(
                @"<StackPanel xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation'
                        xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'
                        xmlns:ui='http://schemas.modernwpf.com/2019'>
                    <StackPanel.Resources>
                        <ui:CornerRadiusFilterConverter x:Key='TopCornerRadiusFilterConverter' Filter='Top' Scale='2'/>
                        <ui:CornerRadiusFilterConverter x:Key='RightCornerRadiusFilterConverter' Filter='Right'/>
                        <ui:CornerRadiusFilterConverter x:Key='BottomCornerRadiusFilterConverter' Filter='Bottom'/>
                        <ui:CornerRadiusFilterConverter x:Key='LeftCornerRadiusFilterConverter' Filter='Left'/>
                        <CornerRadius x:Key='testCornerRadius'>6,6,6,6</CornerRadius>
                    </StackPanel.Resources>
                    <Border x:Name='TopRadiusBorder'
                        CornerRadius='{Binding Source={StaticResource testCornerRadius}, Converter={StaticResource TopCornerRadiusFilterConverter}}' />
                    <Border x:Name='RightRadiusBorder'
                        CornerRadius='{Binding Source={StaticResource testCornerRadius}, Converter={StaticResource RightCornerRadiusFilterConverter}}' />
                    <Border x:Name='BottomRadiusBorder'
                        CornerRadius='{Binding Source={StaticResource testCornerRadius}, Converter={StaticResource BottomCornerRadiusFilterConverter}}' />
                    <Border x:Name='LeftRadiusBorder'
                        CornerRadius='{Binding Source={StaticResource testCornerRadius}, Converter={StaticResource LeftCornerRadiusFilterConverter}}' />
                </StackPanel>");

            Assert.AreEqual(new CornerRadius(12, 12, 0, 0), ((Border)root.FindName("TopRadiusBorder")).CornerRadius);
            Assert.AreEqual(new CornerRadius(0, 6, 6, 0), ((Border)root.FindName("RightRadiusBorder")).CornerRadius);
            Assert.AreEqual(new CornerRadius(0, 0, 6, 6), ((Border)root.FindName("BottomRadiusBorder")).CornerRadius);
            Assert.AreEqual(new CornerRadius(6, 0, 0, 6), ((Border)root.FindName("LeftRadiusBorder")).CornerRadius);
        });
    }

    private static void AssertResource(ResourceDictionary resources, string key, object expected)
    {
        Assert.IsTrue(resources.Contains(key), $"Expected compact resource '{key}' to exist.");
        Assert.AreEqual(expected, resources[key], $"Unexpected compact resource value for '{key}'.");
    }

    private static void AssertThemeResourceReference(string themeName, object resourceKey, object expectedResourceKey)
    {
        var themeDictionary = ThemeResources.Current.GetThemeDictionary(themeName);
        Assert.IsTrue(themeDictionary.Contains(resourceKey), $"{themeName} is missing {resourceKey}.");
        Assert.IsTrue(themeDictionary.Contains(expectedResourceKey), $"{themeName} is missing {expectedResourceKey}.");
        Assert.AreSame(themeDictionary[expectedResourceKey], themeDictionary[resourceKey], $"{themeName}:{resourceKey}");
    }

    private static void AssertThemeResourceValue<T>(string themeName, object resourceKey, T expectedValue)
    {
        var themeDictionary = ThemeResources.Current.GetThemeDictionary(themeName);
        Assert.IsTrue(themeDictionary.Contains(resourceKey), $"{themeName} is missing {resourceKey}.");
        Assert.AreEqual(expectedValue, themeDictionary[resourceKey]);
    }

    private static ResourceDictionaryEx CreateThemeResourceSnapshot()
    {
        var resourceDictionaries = new ResourceDictionaryEx();
        resourceDictionaries.ThemeDictionaries.Add("Default", ThemeResources.Current.GetThemeDictionary("Dark"));
        resourceDictionaries.ThemeDictionaries.Add("Light", ThemeResources.Current.GetThemeDictionary("Light"));
        resourceDictionaries.ThemeDictionaries.Add("HighContrast", ThemeResources.Current.GetThemeDictionary("HighContrast"));
        return resourceDictionaries;
    }

    private static IEnumerable<string> FindMissingKeys(
        ResourceDictionary expectedDictionary,
        string expectedDictionaryName,
        ResourceDictionary actualDictionary,
        string actualDictionaryName)
    {
        foreach (DictionaryEntry entry in expectedDictionary)
        {
            if (!actualDictionary.Contains(entry.Key))
            {
                yield return $"{entry.Key} exists in {expectedDictionaryName} but not {actualDictionaryName}";
            }
        }
    }

    private static void AssertMissingKeys(IReadOnlyCollection<string> missingKeys)
    {
        if (missingKeys.Count > 0)
        {
            Assert.Fail("Resource key mismatch:" + System.Environment.NewLine + string.Join(System.Environment.NewLine, missingKeys.OrderBy(static key => key)));
        }
    }

    private static IEnumerable<string> GetProductXamlFiles(string repoRoot)
    {
        foreach (var productDirectory in new[] { "ModernWpf", "ModernWpf.Controls" })
        {
            var root = Path.Combine(repoRoot, productDirectory);
            foreach (var path in Directory.EnumerateFiles(root, "*.xaml", SearchOption.AllDirectories))
            {
                if (IsGeneratedOutputPath(path))
                {
                    continue;
                }

                yield return path;
            }
        }
    }

    private static bool IsGeneratedOutputPath(string path)
    {
        return path.IndexOf($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase) >= 0 ||
            path.IndexOf($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase) >= 0;
    }

    private static bool IsXamlKeyAttribute(XAttribute attribute)
    {
        return attribute.Name.LocalName == "Key" &&
            attribute.Name.NamespaceName == "http://schemas.microsoft.com/winfx/2006/xaml";
    }

    private static bool IsSimpleStringResourceKey(string resourceKey)
    {
        return resourceKey.Length > 0 &&
            !resourceKey.StartsWith("{", StringComparison.Ordinal) &&
            !resourceKey.StartsWith("x:Static", StringComparison.Ordinal) &&
            resourceKey.IndexOf(':') < 0;
    }

    private static string FindRepoRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);

        while (directory != null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "ModernWpf.sln")))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        Assert.Fail("Could not locate ModernWpf.sln from the test output directory.");
        return string.Empty;
    }
}
