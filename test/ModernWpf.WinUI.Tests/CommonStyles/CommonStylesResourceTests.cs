using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
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
}
