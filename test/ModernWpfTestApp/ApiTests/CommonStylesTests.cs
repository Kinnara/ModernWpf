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
