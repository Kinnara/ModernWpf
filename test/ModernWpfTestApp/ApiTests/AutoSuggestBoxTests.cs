﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Common;
using MUXControlsTestApp.Utilities;
using System.Windows.Controls;
using System.Windows.Media;
using System.Linq;
using System.Collections.Generic;
using MUXControlsTestApp;
using System.Windows;
using ModernWpf.Controls;

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
    public class AutoSuggestBoxTests : ApiTestBase
    {
        [TestMethod]
        public void VerifyAutoSuggestBoxCornerRadius()
        {
            /*
            if (PlatformConfiguration.IsOSVersionLessThan(OSVersion.Redstone5))
            {
                Log.Warning("AutoSuggestBox CornerRadius property is not available pre-rs5");
                return;
            }
            */

            var autoSuggestBox = SetupAutoSuggestBox();

            RunOnUIThread.Execute(() =>
            {
                autoSuggestBox.CornerRadius = new CornerRadius(2);
                autoSuggestBox.Focus(/*FocusState.Keyboard*/);
                autoSuggestBox.Text = "123";
                autoSuggestBox.MaxHeight = 32;
            });
            IdleSynchronizer.Wait();

            RunOnUIThread.Execute(() =>
            {
                var textBox = TestUtilities.FindDescendents<TextBox>(autoSuggestBox).Where(e => e.Name == "TextBox").Single();

                // Flyout might open differently and as such flip corner radii values
                Verify.IsTrue(new CornerRadius(2, 2, 0, 0) == textBox.GetCornerRadius() || new CornerRadius(0, 0, 2, 2) == textBox.GetCornerRadius());

                var overlayCornerRadius = new CornerRadius(0, 0, 0, 0);
                var radius = App.Current.Resources["OverlayCornerRadius"];
                if (radius != null)
                {
                    overlayCornerRadius = (CornerRadius)radius;
                }
                var popup = VisualTreeHelperEx.GetOpenPopups(WindowEx.Current).Last();
                var popupBorder = TestUtilities.FindDescendents<Border>(popup).First();

                // Flyout might open differently and as such flip corner radii values
                Verify.IsTrue(new CornerRadius(0, 0, overlayCornerRadius.BottomRight, overlayCornerRadius.BottomLeft) == popupBorder.CornerRadius
                    || new CornerRadius(overlayCornerRadius.TopRight, overlayCornerRadius.TopLeft, 0, 0) == popupBorder.CornerRadius);
            });
        }

        /*
        [TestMethod]
        public void VerifyVisualTree()
        {
            var autoSuggestBox = SetupAutoSuggestBox();
            VisualTreeTestHelper.VerifyVisualTree(root: autoSuggestBox, masterFilePrefix: "AutoSuggestBox");
        }
        */

        private AutoSuggestBox SetupAutoSuggestBox()
        {
            AutoSuggestBox autoSuggestBox = null;
            RunOnUIThread.Execute(() =>
            {
                autoSuggestBox = new AutoSuggestBox();
                List<string> suggestions = new List<string>
                {
                    "Item 1", "Item 2", "Item 3"
                };
                autoSuggestBox.ItemsSource = suggestions;
                autoSuggestBox.Width = 400;
            });
            TestUtilities.SetAsVisualTreeRoot(autoSuggestBox);
            Verify.IsNotNull(autoSuggestBox);
            return autoSuggestBox;
        }

    }
}
