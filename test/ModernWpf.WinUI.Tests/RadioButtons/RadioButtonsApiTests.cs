using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModernWpf.Automation.Peers;
using ModernWpf.Controls;
using ModernWpf.Controls.Primitives;
using ModernWpf.WinUI.TestApp;
using ModernWpf.WinUI.TestInfra;

namespace ModernWpf.WinUI.Tests.RadioButtons;

[TestClass]
public class RadioButtonsApiTests
{
    [TestMethod]
    public void VerifyHeaderPresenterMatchesWinUITemplate()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var headerTemplate = (DataTemplate)XamlReader.Parse(
                @"<DataTemplate xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation'>
                    <TextBlock Text='{Binding}'/>
                </DataTemplate>");
            var radioButtons = new ModernWpf.Controls.RadioButtons
            {
                Header = "RadioButtons header",
                HeaderTemplate = headerTemplate,
                ItemsSource = new List<string> { "Option 1", "Option 2" }
            };

            using var host = new TestWindowHost(radioButtons);
            host.UpdateLayout();

            var headerPresenter = FindNamedDescendant<ContentPresenterEx>(radioButtons, "HeaderContentPresenter");
            Assert.AreEqual("RadioButtons header", headerPresenter.Content);
            Assert.AreSame(headerTemplate, headerPresenter.ContentTemplate);
            AssertBrushEquals((Brush)headerPresenter.TryFindResource("RadioButtonsHeaderForeground"), headerPresenter.Foreground);

            radioButtons.IsEnabled = false;
            host.UpdateLayout();

            AssertBrushEquals((Brush)headerPresenter.TryFindResource("RadioButtonsHeaderForegroundDisabled"), headerPresenter.Foreground);
        });
    }

    [TestMethod]
    public void VerifyCustomItemTemplate()
    {
        WpfTestHost.Run(() =>
        {
            var radioButtons = new ModernWpf.Controls.RadioButtons
            {
                ItemsSource = new List<string> { "Option 1", "Option 2" },
                ItemTemplate = (DataTemplate)XamlReader.Parse(
                    @"<DataTemplate xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation'>
                        <TextBlock Text='{Binding}'/>
                    </DataTemplate>")
            };

            var radioButtons2 = new ModernWpf.Controls.RadioButtons
            {
                ItemsSource = new List<string> { "Option 1", "Option 2" },
                ItemTemplate = (DataTemplate)XamlReader.Parse(
                    @"<DataTemplate xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation'>
                        <RadioButton Foreground='Blue'>
                            <TextBlock Text='{Binding}'/>
                        </RadioButton>
                    </DataTemplate>")
            };

            var stackPanel = new StackPanel();
            stackPanel.Children.Add(radioButtons);
            stackPanel.Children.Add(radioButtons2);

            using var host = new TestWindowHost(stackPanel);

            var radioButton1 = radioButtons.ContainerFromIndex(0) as RadioButton;
            var radioButton2 = radioButtons2.ContainerFromIndex(0) as RadioButton;

            Assert.IsNotNull(radioButton1, "Our custom ItemTemplate should have been wrapped in a RadioButton.");
            Assert.IsNotNull(radioButton2, "Our custom ItemTemplate should have been wrapped in a RadioButton.");
            Assert.IsFalse(IsBlue(radioButton1!.Foreground), "Default foreground color of the RadioButton should not have been [blue].");
            Assert.IsTrue(IsBlue(radioButton2!.Foreground), "The foreground color of the RadioButton should have been [blue].");
        });
    }

    [TestMethod]
    public void VerifyCustomItemTemplateSelector()
    {
        WpfTestHost.Run(() =>
        {
            var itemTemplate = (DataTemplate)XamlReader.Parse(
                @"<DataTemplate xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation'>
                    <TextBlock Text='{Binding}'/>
                </DataTemplate>");
            var selector = new ConstantTemplateSelector(itemTemplate);
            var radioButtons = new ModernWpf.Controls.RadioButtons
            {
                ItemsSource = new List<string> { "Option 1", "Option 2" },
                ItemTemplate = selector
            };

            using var host = new TestWindowHost(radioButtons);

            var radioButton = radioButtons.ContainerFromIndex(0) as RadioButton;
            Assert.IsNotNull(radioButton, "The selected template content should have been wrapped in a RadioButton.");
            Assert.AreSame(selector, radioButton!.ContentTemplateSelector);
            Assert.AreEqual("Option 1", radioButton.Content);
        });
    }

    [TestMethod]
    public void VerifyAutomationPeerMatchesWinUISource()
    {
        WpfTestHost.Run(() =>
        {
            var radioButtons = new ModernWpf.Controls.RadioButtons
            {
                Header = "RadioButtons header",
                ItemsSource = new List<string> { "Option 1", "Option 2" }
            };

            using var host = new TestWindowHost(radioButtons);

            var peer = UIElementAutomationPeer.CreatePeerForElement(radioButtons);
            Assert.IsInstanceOfType(peer, typeof(RadioButtonsAutomationPeer));
            Assert.AreEqual(nameof(ModernWpf.Controls.RadioButtons), peer.GetClassName());
            Assert.AreEqual(AutomationControlType.Group, peer.GetAutomationControlType());
            Assert.AreEqual("RadioButtons header", peer.GetName());

            AutomationProperties.SetName(radioButtons, "Explicit name");
            Assert.AreEqual("Explicit name", peer.GetName());
        });
    }

    [TestMethod]
    public void VerifyMaxColumnsBindingMatchesWinUITemplate()
    {
        WpfTestHost.Run(() =>
        {
            var radioButtons = new ModernWpf.Controls.RadioButtons
            {
                ItemsSource = new List<string> { "Option 1", "Option 2" }
            };

            using var host = new TestWindowHost(radioButtons);

            var repeater = FindNamedDescendant<ItemsRepeater>(radioButtons, "InnerRepeater");
            var layout = repeater.Layout as ColumnMajorUniformToLargestGridLayout;
            Assert.IsNotNull(layout);
            var actualLayout = layout!;
            Assert.IsNotNull(BindingOperations.GetBindingExpression(
                actualLayout,
                ColumnMajorUniformToLargestGridLayout.MaxColumnsProperty));
            Assert.AreEqual(1, actualLayout.MaxColumns);

            radioButtons.MaxColumns = 4;
            host.UpdateLayout();

            Assert.AreEqual(4, actualLayout.MaxColumns);
        });
    }

    [TestMethod]
    public void VerifyMaxColumnsValidationMatchesWinUISource()
    {
        WpfTestHost.Run(() =>
        {
            var radioButtons = new ModernWpf.Controls.RadioButtons();

            Assert.ThrowsExactly<ArgumentException>(() => radioButtons.MaxColumns = 0);
            Assert.ThrowsExactly<ArgumentException>(() => radioButtons.MaxColumns = -1);
            radioButtons.MaxColumns = 1;
        });
    }

    [TestMethod]
    public void VerifyIsEnabledChangeUpdatesVisualState()
    {
        WpfTestHost.Run(() =>
        {
            var radioButtons = new ModernWpf.Controls.RadioButtons
            {
                IsEnabled = true
            };

            using var host = new TestWindowHost(radioButtons);

            var layoutRoot = (FrameworkElement)VisualTreeHelper.GetChild(radioButtons, 0);
            var commonStatesGroup = VisualStateManager.GetVisualStateGroups(layoutRoot)
                .OfType<VisualStateGroup>()
                .Single(group => group.Name == "CommonStates");

            Assert.AreEqual("Normal", commonStatesGroup.CurrentState.Name);

            radioButtons.IsEnabled = false;
            host.UpdateLayout();
            Assert.AreEqual("Disabled", commonStatesGroup.CurrentState.Name);

            radioButtons.IsEnabled = true;
            host.UpdateLayout();
            Assert.AreEqual("Normal", commonStatesGroup.CurrentState.Name);
        });
    }

    [TestMethod]
    public void VerifySelectionChangedArgsDoNotContainNullItems()
    {
        WpfTestHost.Run(() =>
        {
            var radioButtons = new ModernWpf.Controls.RadioButtons
            {
                ItemsSource = new List<string> { "0", "1", "2", "3" }
            };
            var selectionChangedArgs = new List<SelectionChangedEventArgs>();
            radioButtons.SelectionChanged += (sender, args) => selectionChangedArgs.Add(args);

            using var host = new TestWindowHost(radioButtons);
            selectionChangedArgs.Clear();

            radioButtons.SelectedIndex = 0;
            host.UpdateLayout();

            Assert.IsTrue(selectionChangedArgs.Count > 0);
            var firstSelection = selectionChangedArgs.Last();
            Assert.AreEqual(0, firstSelection.RemovedItems.Count);
            Assert.AreEqual(1, firstSelection.AddedItems.Count);
            Assert.AreEqual("0", firstSelection.AddedItems[0]);
            AssertNoNullItems(selectionChangedArgs);

            selectionChangedArgs.Clear();
            radioButtons.SelectedIndex = 2;
            host.UpdateLayout();

            Assert.AreEqual(1, selectionChangedArgs.Last().RemovedItems.Count);
            Assert.AreEqual(1, selectionChangedArgs.Last().AddedItems.Count);
            AssertNoNullItems(selectionChangedArgs);

            selectionChangedArgs.Clear();
            radioButtons.SelectedIndex = 99;
            host.UpdateLayout();

            Assert.AreEqual(1, selectionChangedArgs.Last().RemovedItems.Count);
            Assert.AreEqual(0, selectionChangedArgs.Last().AddedItems.Count);
            AssertNoNullItems(selectionChangedArgs);

            radioButtons.SelectedIndex = 0;
            host.UpdateLayout();
            selectionChangedArgs.Clear();

            radioButtons.SelectedIndex = -1;
            host.UpdateLayout();

            Assert.AreEqual(1, selectionChangedArgs.Last().RemovedItems.Count);
            Assert.AreEqual(0, selectionChangedArgs.Last().AddedItems.Count);
            AssertNoNullItems(selectionChangedArgs);
        });
    }

    private static bool IsBlue(Brush brush)
    {
        return brush is SolidColorBrush solidColorBrush && solidColorBrush.Color == Colors.Blue;
    }

    private static void AssertBrushEquals(Brush expected, Brush actual)
    {
        Assert.IsNotNull(expected);
        Assert.IsNotNull(actual);

        if (expected is SolidColorBrush expectedSolid && actual is SolidColorBrush actualSolid)
        {
            Assert.AreEqual(expectedSolid.Color, actualSolid.Color);
            Assert.AreEqual(expectedSolid.Opacity, actualSolid.Opacity);
            return;
        }

        Assert.AreEqual(expected.ToString(), actual.ToString());
    }

    private static void AssertNoNullItems(IEnumerable<SelectionChangedEventArgs> selectionChangedArgs)
    {
        foreach (var args in selectionChangedArgs)
        {
            Assert.IsFalse(args.AddedItems.Cast<object>().Any(item => item is null));
            Assert.IsFalse(args.RemovedItems.Cast<object>().Any(item => item is null));
        }
    }

    private static T FindNamedDescendant<T>(DependencyObject root, string name)
        where T : FrameworkElement
    {
        foreach (var descendant in VisualTreeTestHelper.EnumerateDescendants(root))
        {
            if (descendant is T element && element.Name == name)
            {
                return element;
            }
        }

        throw new AssertFailedException($"Could not find descendant named '{name}'.");
    }

    private sealed class ConstantTemplateSelector : DataTemplateSelector
    {
        public ConstantTemplateSelector(DataTemplate template)
        {
            Template = template;
        }

        public DataTemplate Template { get; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            return Template;
        }
    }
}
