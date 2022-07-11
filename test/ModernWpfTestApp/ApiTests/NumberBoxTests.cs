﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Common;
using MUXControlsTestApp.Utilities;
using System.Windows.Controls;
using System.Windows.Media;
using System.Linq;
using System.Collections.Generic;
using MUXControlsTestApp;
using ModernWpf.Controls;
using System.Windows.Controls.Primitives;
using System.Windows;
using System.Windows.Input;
using System.Windows.Automation.Peers;
using System.Windows.Automation;

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
    public class NumberBoxTests : ApiTestBase
    {
        [TestMethod]
        public void VerifyTextAlignmentPropogates()
        {
            var numberBox = SetupNumberBox();
            TextBox textBox = null;

            RunOnUIThread.Execute(() =>
            {
                Content.UpdateLayout();

                textBox = TestUtilities.FindDescendents<TextBox>(numberBox).Where(e => e.Name == "InputBox").Single();
                Verify.AreEqual(TextAlignment.Left, textBox.TextAlignment, "The default TextAlignment should be left.");

                numberBox.TextAlignment = TextAlignment.Right;
                Content.UpdateLayout();

                Verify.AreEqual(TextAlignment.Right, textBox.TextAlignment, "The TextAlignment should have been updated to Right.");
            });
        }

        [TestMethod]
        public void VerifyNumberBoxCornerRadius()
        {
            /*
            if (PlatformConfiguration.IsOSVersionLessThan(OSVersion.Redstone5))
            {
                Log.Warning("NumberBox CornerRadius property is not available pre-rs5");
                return;
            }
            */

            var numberBox = SetupNumberBox();

            RepeatButton spinButtonDown = null;
            TextBox textBox = null;
            RunOnUIThread.Execute(() =>
            {
                // first test: Uniform corner radius of '2' with no spin buttons shown
                numberBox.SpinButtonPlacementMode = NumberBoxSpinButtonPlacementMode.Hidden;
                numberBox.CornerRadius = new CornerRadius(2);

                Content.UpdateLayout();

                textBox = TestUtilities.FindDescendents<TextBox>(numberBox).Where(e => e.Name == "InputBox").Single();
                Verify.AreEqual(new CornerRadius(2, 2, 2, 2), textBox.GetCornerRadius());

                // second test: Uniform corner radius of '2' with spin buttons in inline mode (T-rule applies now)
                numberBox.SpinButtonPlacementMode = NumberBoxSpinButtonPlacementMode.Inline;
                Content.UpdateLayout();

                spinButtonDown = TestUtilities.FindDescendents<RepeatButton>(numberBox).Where(e => e.Name == "DownSpinButton").Single();

                Verify.AreEqual(new CornerRadius(2, 0, 0, 2), textBox.GetCornerRadius());
                Verify.AreEqual(new CornerRadius(0, 2, 2, 0), spinButtonDown.GetCornerRadius());

                // third test: Set uniform corner radius to '4' with spin buttons in inline mode
                numberBox.CornerRadius = new CornerRadius(4);
            });
            IdleSynchronizer.Wait();

            RunOnUIThread.Execute(() =>
            {
                // This check makes sure that updating the CornerRadius values of the numberbox in inline mode
                // does not break the T-rule.
                Verify.AreEqual(new CornerRadius(4, 0, 0, 4), textBox.GetCornerRadius());
                Verify.AreEqual(new CornerRadius(0, 4, 4, 0), spinButtonDown.GetCornerRadius());

                // fourth test: Update the spin button placement mode to 'compact' and verify that all corners
                // of the textbox are now rounded again
                numberBox.SpinButtonPlacementMode = NumberBoxSpinButtonPlacementMode.Compact;
                Content.UpdateLayout();

                Verify.AreEqual(new CornerRadius(4), textBox.GetCornerRadius());

                // fifth test: Check corner radius of 0 in compact mode.
                numberBox.CornerRadius = new CornerRadius(0);
                Content.UpdateLayout();

                Verify.AreEqual(new CornerRadius(0), textBox.GetCornerRadius());
            });
        }

        [TestMethod]
        public void VerifyInputScopePropogates()
        {
            var numberBox = SetupNumberBox();

            RunOnUIThread.Execute(() =>
            {
                Content.UpdateLayout();
                var inputTextBox = TestUtilities.FindDescendents<TextBox>(numberBox).Where(e => e.Name == "InputBox").Single();

                Verify.AreEqual(1, inputTextBox.InputScope.Names.Count);
                Verify.AreEqual(InputScopeNameValue.Number, ((InputScopeName)inputTextBox.InputScope.Names[0]).NameValue, "The default InputScope should be 'Number'.");

                var scopeName = new InputScopeName();
                scopeName.NameValue = InputScopeNameValue.CurrencyAmountAndSymbol;
                var scope = new InputScope();
                scope.Names.Add(scopeName);

                numberBox.InputScope = scope;
                Content.UpdateLayout();

                Verify.AreEqual(1, inputTextBox.InputScope.Names.Count);
                Verify.AreEqual(InputScopeNameValue.CurrencyAmountAndSymbol, ((InputScopeName)inputTextBox.InputScope.Names[0]).NameValue, "The InputScope should be 'CurrencyAmountAndSymbol'.");
            });

            return;
        }

        [TestMethod]
        public void VerifyIsEnabledChangeUpdatesVisualState()
        {
            var numberBox = SetupNumberBox();

            VisualStateGroup commonStatesGroup = null;
            RunOnUIThread.Execute(() =>
            {
                // Check 1: Set IsEnabled to true.
                numberBox.IsEnabled = true;
                Content.UpdateLayout();

                var numberBoxLayoutRoot = (FrameworkElement)VisualTreeHelper.GetChild(numberBox, 0);
                commonStatesGroup = VisualStateManager.GetVisualStateGroups(numberBoxLayoutRoot).Cast<VisualStateGroup>().First(vsg => vsg.Name.Equals("CommonStates"));

                Verify.AreEqual("Normal", commonStatesGroup.CurrentState.Name);

                // Check 2: Set IsEnabled to false.
                numberBox.IsEnabled = false;
            });
            IdleSynchronizer.Wait();

            RunOnUIThread.Execute(() =>
            {
                Verify.AreEqual("Disabled", commonStatesGroup.CurrentState.Name);

                // Check 3: Set IsEnabled back to true.
                numberBox.IsEnabled = true;
            });
            IdleSynchronizer.Wait();

            RunOnUIThread.Execute(() =>
            {
                Verify.AreEqual("Normal", commonStatesGroup.CurrentState.Name);
            });
        }

        [TestMethod]
        public void VerifyUIANameBehavior()
        {
            NumberBox numberBox = null;
            TextBox textBox = null;

            RunOnUIThread.Execute(() =>
            {
                numberBox = new NumberBox();
                Content = numberBox;
                Content.UpdateLayout();

                textBox = TestPage.FindVisualChildrenByType<TextBox>(numberBox)[0];
                Verify.IsNotNull(textBox);
                numberBox.Header = "Some header";
            });

            IdleSynchronizer.Wait();

            RunOnUIThread.Execute(() =>
            {
                VerifyUIAName("Some header");
                numberBox.Header = new Button();
                AutomationProperties.SetName(numberBox, "Some UIA name");
            });

            IdleSynchronizer.Wait();

            RunOnUIThread.Execute(() =>
            {
                VerifyUIAName("Some UIA name");
                numberBox.Header = new Button();
            });

            IdleSynchronizer.Wait();

            RunOnUIThread.Execute(() =>
            {
                VerifyUIAName("Some UIA name");
            });

            void VerifyUIAName(string value)
            {
                Verify.AreEqual(value, FrameworkElementAutomationPeer.CreatePeerForElement(textBox).GetName());
            }
        }

        private NumberBox SetupNumberBox()
        {
            NumberBox numberBox = null;
            RunOnUIThread.Execute(() =>
            {
                numberBox = new NumberBox();
                Content = numberBox;
            });

            return numberBox;
        }
    }
}
