// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Common;
using MUXControlsTestApp.Utilities;
using System.Linq;
using System.Threading;
using Windows.System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using PlatformConfiguration = Common.PlatformConfiguration;
using OSVersion = Common.OSVersion;
using System.Collections.Generic;
using XamlControlsResources = ModernWpf.Controls.XamlControlsResources;
using System.Windows.Markup;
using System;
using ModernWpf.Controls;
using System.Text;
using System.Collections;

#if USING_TAEF
using WEX.TestExecution;
using WEX.TestExecution.Markup;
using WEX.Logging.Interop;
#else
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.VisualStudio.TestTools.UnitTesting.Logging;
#endif

namespace ModernWpf.Tests.MUXControls.ApiTests
{
    [TestClass]
    public class CommonStylesApiTests : ApiTestBase
    {
        [TestMethod]
        public void VerifyAllThemesContainSameResourceKeys()
        {
            bool dictionariesContainSameElements = true;
            RunOnUIThread.Execute(() =>
            {
                var resourceDictionaries = new ResourceDictionaryEx();
                resourceDictionaries.ThemeDictionaries.Add("Default", ThemeResources.Current.GetThemeDictionary("Dark"));
                resourceDictionaries.ThemeDictionaries.Add("Light", ThemeResources.Current.GetThemeDictionary("Light"));
                resourceDictionaries.ThemeDictionaries.Add("HighContrast", ThemeResources.Current.GetThemeDictionary("HighContrast"));
                Log.Comment("ThemeDictionaries");

                var defaultThemeDictionary = resourceDictionaries.ThemeDictionaries["Default"] as ResourceDictionary;

                foreach (var dictionaryName in resourceDictionaries.ThemeDictionaries.Keys)
                {
                    // Skip the Default theme dictionary
                    if (dictionaryName.ToString() == "Default")
                    {
                        continue;
                    }

                    Log.Comment("Comparing against " + dictionaryName.ToString());
                    var themeDictionary = resourceDictionaries.ThemeDictionaries[dictionaryName] as ResourceDictionary;

                    bool allKeysInDefaultExistInDictionary = AreKeysFromExpectedInActualDictionary(defaultThemeDictionary, "Default", themeDictionary, dictionaryName.ToString());
                    bool allKeysInDictionaryExistInDefault = AreKeysFromExpectedInActualDictionary(themeDictionary, dictionaryName.ToString(), defaultThemeDictionary, "Default");

                    dictionariesContainSameElements &= (allKeysInDefaultExistInDictionary && allKeysInDictionaryExistInDefault);
                }

                Verify.AreEqual(0, resourceDictionaries.MergedDictionaries.Count, "MergedDictionaries is not empty, Verify if you really wanted to update the merged dictionary. If so, update the test");
            });

            Verify.IsTrue(dictionariesContainSameElements, "Resource Keys you have added are missing in one of the theme dictionaries. This is trouble since we might end up crashing when trying to resolve the key in that Theme.");
            if (!dictionariesContainSameElements)
            {
                Log.Error("Resource Keys you have added are missing in one of the theme dictionaries. This is trouble since we might end up crashing when trying to resolve the key in that Theme.");
            }
        }

        [TestMethod]
        public void VerifyNoResourceKeysWereRemovedFromPreviousStableReleaseInV2Styles()
        {
            /*
            if (PlatformConfiguration.IsOSVersionLessThan(OSVersion.Redstone5))
            {
                // https://github.com/microsoft/microsoft-ui-xaml/issues/4674
                Log.Comment("Skipping validation below RS5.");
                return;
            }
            */

            RunOnUIThread.Execute(() =>
            {
                EnsureNoMissingThemeResources(
                BaselineResources.BaselineResourcesList2dot5Stable,
                ThemeResources.Current);
            });
        }

        private bool AreKeysFromExpectedInActualDictionary(ResourceDictionary expectedDictionary, string expectedDictionaryName, ResourceDictionary actualDictionary, string actualDictionaryName)
        {
            List<string> missingKeysInActualDictionary = new List<string>();
            foreach (DictionaryEntry entry in expectedDictionary)
            {
                if (!actualDictionary.Contains(entry.Key))
                {
                    missingKeysInActualDictionary.Add(entry.Key.ToString());
                }
            }

            if (missingKeysInActualDictionary.Count > 0)
            {
                Log.Comment("Keys found in " + expectedDictionaryName + " but not in " + actualDictionaryName);
                foreach (var missingKey in missingKeysInActualDictionary)
                {
                    Log.Error("* " + missingKey);
                }
            }

            return (missingKeysInActualDictionary.Count == 0);
        }

        private void EnsureNoMissingThemeResources(IList<string> baseline, ThemeResources dictionaryToVerify)
        {
            var actualResourcesKeys = new HashSet<string>();
            var resourceDictionaries = dictionaryToVerify;

            foreach (var dictionaryName in resourceDictionaries.ThemeDictionaries.Keys)
            {
                var themeDictionary = resourceDictionaries.ThemeDictionaries[dictionaryName] as ResourceDictionary;

                foreach (DictionaryEntry entry in themeDictionary)
                {
                    string entryKey = entry.Key as string;
                    if (!actualResourcesKeys.Contains(entryKey))
                    {
                        actualResourcesKeys.Add(entryKey);
                    }
                }
            }

            foreach (DictionaryEntry entry in resourceDictionaries)
            {
                string entryKey = entry.Key as string;
                if (!actualResourcesKeys.Contains(entryKey))
                {
                    actualResourcesKeys.Add(entryKey);
                }
            }

            StringBuilder missingKeysList = new StringBuilder();

            bool allBaselineResourceKeysExist = true;
            foreach (var baselineResourceKey in baseline)
            {
                if (!actualResourcesKeys.Contains(baselineResourceKey))
                {
                    missingKeysList.Append(baselineResourceKey + ", ");
                    allBaselineResourceKeysExist = false;
                }
            }

            Verify.IsTrue(allBaselineResourceKeysExist, "List of missing resource keys: " + missingKeysList.ToString());
            if (!allBaselineResourceKeysExist)
            {
                Log.Error("List of missing resource keys: " + missingKeysList.ToString());
            }
        }

        public void DumpThemeResources()
        {
            RunOnUIThread.Execute(() =>
            {
                var resourceDictionary = ThemeResources.Current;

                Log.Comment("ThemeDictionaries");
                foreach (var key in resourceDictionary.ThemeDictionaries.Keys)
                {
                    Log.Comment("* " + key.ToString());

                    var themeDictionary = resourceDictionary.ThemeDictionaries[key] as ResourceDictionary;
                    foreach (var entry in themeDictionary)
                    {
                        Log.Comment("\t*" + entry.ToString());
                    }
                }

                Log.Comment("Entries in Resource Dictionary");
                foreach (var entry in resourceDictionary)
                {
                    Log.Comment("* " + entry.ToString());
                }
            });
        }

        [TestMethod]
        public void VerifyUseCompactResourcesAPI()
        {
            //Verify there is no crash and TreeViewItemMinHeight is not the same when changing UseCompactResources.
            RunOnUIThread.Execute(() =>
            {
                var dict = new XamlControlsResources();
                var height = dict["TreeViewItemMinHeight"].ToString();

                dict.UseCompactResources = true;
                var compactHeight = dict["TreeViewItemMinHeight"].ToString();
                Verify.AreNotEqual(height, compactHeight, "Height in Compact is not the same as default");
                Verify.AreEqual("24", compactHeight, "Height in 24 in Compact");

                dict.UseCompactResources = false;
                var height2 = dict["TreeViewItemMinHeight"].ToString();
                Verify.AreEqual(height, height2, "Height are the same after disabled compact");
            });

            MUXControlsTestApp.Utilities.IdleSynchronizer.Wait();
        }

        [TestMethod]
        public void CornerRadiusFilterConverterTest()
        {
            /*
            if (!PlatformConfiguration.IsOsVersionGreaterThan(OSVersion.Redstone4))
            {
                Log.Comment("Corner radius is only available on RS5+");
                return;
            }
            */

            RunOnUIThread.Execute(() =>
            {
                var root = (StackPanel)XamlReader.Parse(
                    @"<StackPanel xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation' 
                             xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'
                             xmlns:primitives='http://schemas.modernwpf.com/2019'> 
                            <StackPanel.Resources>
                                <primitives:CornerRadiusFilterConverter x:Key='TopCornerRadiusFilterConverter' Filter='Top' Scale='2'/>
                                <primitives:CornerRadiusFilterConverter x:Key='RightCornerRadiusFilterConverter' Filter='Right'/>
                                <primitives:CornerRadiusFilterConverter x:Key='BottomCornerRadiusFilterConverter' Filter='Bottom'/>
                                <primitives:CornerRadiusFilterConverter x:Key='LeftCornerRadiusFilterConverter' Filter='Left'/>
                                <CornerRadius x:Key='testCornerRadius'>6,6,6,6</CornerRadius>
                            </StackPanel.Resources>
                            <Border x:Name='TopRadiusGrid'
                                CornerRadius='{Binding Source={StaticResource testCornerRadius}, Converter={StaticResource TopCornerRadiusFilterConverter}}'>
                            </Border>
                            <Border x:Name='RightRadiusGrid'
                                CornerRadius='{Binding Source={StaticResource testCornerRadius}, Converter={StaticResource RightCornerRadiusFilterConverter}}'>
                            </Border>
                            <Border x:Name='BottomRadiusGrid'
                                CornerRadius='{Binding Source={StaticResource testCornerRadius}, Converter={StaticResource BottomCornerRadiusFilterConverter}}'>
                            </Border>
                            <Border x:Name='LeftRadiusGrid'
                                CornerRadius='{Binding Source={StaticResource testCornerRadius}, Converter={StaticResource LeftCornerRadiusFilterConverter}}'>
                            </Border>
                       </StackPanel>");

                var topRadiusGrid = (Border)root.FindName("TopRadiusGrid");
                var rightRadiusGrid = (Border)root.FindName("RightRadiusGrid");
                var bottomRadiusGrid = (Border)root.FindName("BottomRadiusGrid");
                var leftRadiusGrid = (Border)root.FindName("LeftRadiusGrid");

                Verify.AreEqual(new CornerRadius(12, 12, 0, 0), topRadiusGrid.CornerRadius);
                Verify.AreEqual(new CornerRadius(0, 6, 6, 0), rightRadiusGrid.CornerRadius);
                Verify.AreEqual(new CornerRadius(0, 0, 6, 6), bottomRadiusGrid.CornerRadius);
                Verify.AreEqual(new CornerRadius(6, 0, 0, 6), leftRadiusGrid.CornerRadius);
            });
        }
    }
}
