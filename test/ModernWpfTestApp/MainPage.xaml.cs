// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Navigation;

namespace MUXControlsTestApp
{
    public sealed partial class MainPage : TestPage
    {
        List<string> locales = new List<string>
        {
            "af-ZA",
            "am-ET",
            "ar-SA",
            "az-Latn-AZ",
            "be-BY",
            "bg-BG",
            "bn-BD",
            "bs-Latn-BA",
            "ca-ES",
            "cs-CZ",
            "da-DK",
            "de-DE",
            "el-GR",
            "en-GB",
            "en-US",
            "es-ES",
            "es-MX",
            "et-EE",
            "eu-ES",
            "fa-IR",
            "fi-FI",
            "fil-PH",
            "fr-CA",
            "fr-FR",
            "gl-ES",
            "ha-Latn-NG",
            "he-IL",
            "hi-IN",
            "hr-HR",
            "hu-HU",
            "id-ID",
            "is-IS",
            "it-IT",
            "ja-JP",
            "ka-GE",
            "kk-KZ",
            "km-KH",
            "kn-IN",
            "ko-KR",
            "lo-LA",
            "lt-LT",
            "lv-LV",
            "mk-MK",
            "ml-IN",
            "ms-MY",
            "nb-NO",
            "nl-NL",
            "nn-NO",
            "pl-PL",
            "pt-BR",
            "pt-PT",
            "ro-RO",
            "ru-RU",
            "sk-SK",
            "sl-SI",
            "sq-AL",
            "sr-Latn-RS",
            "sv-SE",
            "sw-KE",
            "ta-IN",
            "te-IN",
            "th-TH",
            "tr-TR",
            "uk-UA",
            "uz-Latn-UZ",
            "vi-VN",
            "zh-CN",
            "zh-TW"
        };

        public List<FlowDirection> FlowDirections
        {
            get;
        } = new List<FlowDirection>() { FlowDirection.LeftToRight, FlowDirection.RightToLeft };

        public MainPage()
        {
            InitializeComponent();

            AutomationProperties.SetName(this, "MainPage");
            Loaded += OnLoaded;

            foreach (var locale in locales)
            {
                var item = new ComboBoxItem { Content = locale };
                LanguageChooser.Items.Add(item);
                AutomationProperties.SetAutomationId(item, locale);
            }

            foreach (var flowDirection in FlowDirections)
            {
                var item = new ComboBoxItem { Content = flowDirection.ToString(), Tag = flowDirection };
                FlowDirectionChooser.Items.Add(item);
                AutomationProperties.SetAutomationId(item, flowDirection.ToString());
            }

            // This setting is persisted across multiple openings of an app, so we always want to initialize it to en-US
            // in case the app crashed while in a different language or otherwise was not able to set it back.
            App.LanguageOverride = "en-US";

            // We'll additionally make sure that the combo box begins on the right element to reflect the current value.
            LanguageChooser.SelectedIndex = locales.IndexOf("en-US");
            //LongAnimationsDisabled.IsChecked = MUXControlsTestApp.App.DisableLongAnimations;
            FlowDirectionChooser.SelectedIndex = FlowDirections.IndexOf(GetRootFlowDirection());

            // App remembers ExtendViewIntoTitleBar and the value persists true if test case aborted and didn't change it back
            // Always set it to false when app restarted
            //CoreApplicationViewTitleBar titleBar = CoreApplication.GetCurrentView().TitleBar;
            //titleBar.ExtendViewIntoTitleBar = false;

            DataContext = this;
        }

        protected override AutomationPeer OnCreateAutomationPeer()
        {
            return new FrameworkElementAutomationPeer(this);
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            var automationPeer = new FrameworkElementAutomationPeer(this);
            automationPeer.RaiseAutomationEvent(AutomationEvents.AsyncContentLoaded);

            ((NavigateToTestCommand)Resources["NavigateToTestCommand"]).Frame = Frame;
        }

        public List<TestDeclaration> Tests
        {
            get { return TestInventory.Tests; }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            DependencyObject checkBox = SearchVisualTree(this.Frame, "ViewScalingCheckBox");
            CheckBox cb = checkBox as CheckBox;
            cb.IsChecked = true;
        }

        DependencyObject SearchVisualTree(DependencyObject root, string name)
        {
            int size = VisualTreeHelper.GetChildrenCount(root);
            DependencyObject child = null;

            for (int i = 0; i < size && child == null; i++)
            {
                DependencyObject depObj = VisualTreeHelper.GetChild(root, i);
                FrameworkElement fe = depObj as FrameworkElement;

                if (fe.Name.Equals(name))
                {
                    child = fe;
                }
                else
                {
                    child = SearchVisualTree(fe, name);
                }
            }

            return child;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            LanguageChooser.SelectedItem = App.LanguageOverride;

            /*var testContentLoadedCheckBox = SearchVisualTree(this.Frame, "TestContentLoadedCheckBox") as CheckBox;
            if (testContentLoadedCheckBox != null)
            {
                testContentLoadedCheckBox.IsChecked = false;
            }
            else
            {
                Log.Warning("Warning: Couldn't find the TestContentLoadedCheckBox to uncheck in OnNavigatedTo");
            }*/
        }

        private void LanguageChooser_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            App.LanguageOverride = (string)((ComboBoxItem)LanguageChooser.SelectedItem).Content;
        }

        private void LongAnimationsDisabled_Checked(object sender, RoutedEventArgs e)
        {
            //MUXControlsTestApp.App.DisableLongAnimations = true;
        }

        private void LongAnimationsDisabled_Unchecked(object sender, RoutedEventArgs e)
        {
            //MUXControlsTestApp.App.DisableLongAnimations = false;
        }

        private void FlowDirectionChooser_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            FrameworkElement root = Application.Current.MainWindow as FrameworkElement;
            if (root != null)
            {
                root.FlowDirection = (FlowDirection)((ComboBoxItem)FlowDirectionChooser.SelectedItem).Tag;
            }
        }

        private FlowDirection GetRootFlowDirection()
        {
            return (Application.Current.MainWindow as FrameworkElement)?.FlowDirection ?? FlowDirection.LeftToRight;
        }
    }
}
