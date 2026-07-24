using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Shapes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModernWpf.WinUI.TestInfra;

namespace ModernWpf.WinUI.Tests.VisualStateSetters;

[TestClass]
public class VisualStateSetterTests
{
    [TestMethod]
    public void VisualStateExAppliesDirectPropertySetter()
    {
        WpfTestHost.Run(() =>
        {
            var control = CreateControl(
                """
                <VisualStateGroup x:Name="CommonStates">
                    <ui:VisualStateEx x:Name="Normal" />
                    <ui:VisualStateEx x:Name="PointerOver">
                        <ui:VisualStateEx.Setters>
                            <ui:VisualStateSetter Target="TargetBorder.Background" Value="Red" />
                        </ui:VisualStateEx.Setters>
                    </ui:VisualStateEx>
                </VisualStateGroup>
                """,
                """
                <Border x:Name="TargetBorder" Background="Blue" />
                """);

            using var host = new TestWindowHost(control);
            var target = FindTemplateChild<Border>(control, "TargetBorder");

            AssertSolidColorBrush(target.Background, Colors.Blue);

            Assert.IsTrue(System.Windows.VisualStateManager.GoToState(control, "PointerOver", false));

            AssertSolidColorBrush(target.Background, Colors.Red);
        });
    }

    [TestMethod]
    public void VisualStateExAppliesAttachedPropertySetter()
    {
        WpfTestHost.Run(() =>
        {
            var control = CreateControl(
                """
                <VisualStateGroup x:Name="CommonStates">
                    <ui:VisualStateEx x:Name="Normal" />
                    <ui:VisualStateEx x:Name="PointerOver">
                        <ui:VisualStateEx.Setters>
                            <ui:VisualStateSetter Target="TargetBorder.(Grid.Column)" Value="1" />
                        </ui:VisualStateEx.Setters>
                    </ui:VisualStateEx>
                </VisualStateGroup>
                """,
                """
                <Grid>
                    <Grid.ColumnDefinitions>
                        <ColumnDefinition />
                        <ColumnDefinition />
                    </Grid.ColumnDefinitions>
                    <Border x:Name="TargetBorder" Grid.Column="0" />
                </Grid>
                """);

            using var host = new TestWindowHost(control);
            var target = FindTemplateChild<Border>(control, "TargetBorder");

            Assert.AreEqual(0, Grid.GetColumn(target));

            Assert.IsTrue(System.Windows.VisualStateManager.GoToState(control, "PointerOver", false));

            Assert.AreEqual(1, Grid.GetColumn(target));
        });
    }

    [TestMethod]
    public void VisualStateExAppliesDynamicResourceSetter()
    {
        WpfTestHost.Run(() =>
        {
            var control = CreateControl(
                """
                <VisualStateGroup x:Name="CommonStates">
                    <ui:VisualStateEx x:Name="Normal" />
                    <ui:VisualStateEx x:Name="PointerOver">
                        <ui:VisualStateEx.Setters>
                            <ui:VisualStateSetter Target="TargetBorder.Background" Value="{DynamicResource SetterBrush}" />
                        </ui:VisualStateEx.Setters>
                    </ui:VisualStateEx>
                </VisualStateGroup>
                """,
                """
                <Border x:Name="TargetBorder" Background="Blue" />
                """);
            control.Resources["SetterBrush"] = new SolidColorBrush(Colors.Red);

            using var host = new TestWindowHost(control);
            var target = FindTemplateChild<Border>(control, "TargetBorder");

            Assert.IsTrue(System.Windows.VisualStateManager.GoToState(control, "PointerOver", false));
            AssertSolidColorBrush(target.Background, Colors.Red);

            control.Resources["SetterBrush"] = new SolidColorBrush(Colors.Green);
            WpfTestHost.DoEvents();

            AssertSolidColorBrush(target.Background, Colors.Green);
        });
    }

    [TestMethod]
    public void VisualStateExAppliesBindingSetter()
    {
        WpfTestHost.Run(() =>
        {
            var control = CreateControl(
                """
                <VisualStateGroup x:Name="CommonStates">
                    <ui:VisualStateEx x:Name="Normal" />
                    <ui:VisualStateEx x:Name="PointerOver">
                        <ui:VisualStateEx.Setters>
                            <ui:VisualStateSetter Target="TargetBorder.Background" Value="{Binding ElementName=SourceBorder, Path=Background}" />
                        </ui:VisualStateEx.Setters>
                    </ui:VisualStateEx>
                </VisualStateGroup>
                """,
                """
                <Grid>
                    <Border x:Name="SourceBorder" Background="Red" />
                    <Border x:Name="TargetBorder" Background="Blue" />
                </Grid>
                """);

            using var host = new TestWindowHost(control);
            var source = FindTemplateChild<Border>(control, "SourceBorder");
            var target = FindTemplateChild<Border>(control, "TargetBorder");

            AssertSolidColorBrush(target.Background, Colors.Blue);

            Assert.IsTrue(System.Windows.VisualStateManager.GoToState(control, "PointerOver", false));
            AssertSolidColorBrush(target.Background, Colors.Red);

            source.Background = new SolidColorBrush(Colors.Green);
            WpfTestHost.DoEvents();

            AssertSolidColorBrush(target.Background, Colors.Green);

            Assert.IsTrue(System.Windows.VisualStateManager.GoToState(control, "Normal", false));
            AssertSolidColorBrush(target.Background, Colors.Blue);
        });
    }

    [TestMethod]
    public void VisualStateExRestoresPreviousDynamicResourceValueOnStateExit()
    {
        WpfTestHost.Run(() =>
        {
            var control = CreateControl(
                """
                <VisualStateGroup x:Name="CommonStates">
                    <ui:VisualStateEx x:Name="Normal" />
                    <ui:VisualStateEx x:Name="PointerOver">
                        <ui:VisualStateEx.Setters>
                            <ui:VisualStateSetter Target="TargetBorder.Background" Value="{DynamicResource SetterBrush}" />
                        </ui:VisualStateEx.Setters>
                    </ui:VisualStateEx>
                </VisualStateGroup>
                """,
                """
                <Border x:Name="TargetBorder" Background="{DynamicResource BaseBrush}" />
                """);
            control.Resources["BaseBrush"] = new SolidColorBrush(Colors.Blue);
            control.Resources["SetterBrush"] = new SolidColorBrush(Colors.Red);

            using var host = new TestWindowHost(control);
            var target = FindTemplateChild<Border>(control, "TargetBorder");

            AssertSolidColorBrush(target.Background, Colors.Blue);

            Assert.IsTrue(System.Windows.VisualStateManager.GoToState(control, "PointerOver", false));
            AssertSolidColorBrush(target.Background, Colors.Red);

            Assert.IsTrue(System.Windows.VisualStateManager.GoToState(control, "Normal", false));
            AssertSolidColorBrush(target.Background, Colors.Blue);

            control.Resources["BaseBrush"] = new SolidColorBrush(Colors.Green);
            WpfTestHost.DoEvents();

            AssertSolidColorBrush(target.Background, Colors.Green);
        });
    }

    [TestMethod]
    public void VisualStateExRestoresPreviousValueOnStateExit()
    {
        WpfTestHost.Run(() =>
        {
            var control = CreateControl(
                """
                <VisualStateGroup x:Name="CommonStates">
                    <ui:VisualStateEx x:Name="Normal" />
                    <ui:VisualStateEx x:Name="PointerOver">
                        <ui:VisualStateEx.Setters>
                            <ui:VisualStateSetter Target="TargetBorder.Background" Value="Red" />
                        </ui:VisualStateEx.Setters>
                    </ui:VisualStateEx>
                </VisualStateGroup>
                """,
                """
                <Border x:Name="TargetBorder" Background="Blue" />
                """);

            using var host = new TestWindowHost(control);
            var target = FindTemplateChild<Border>(control, "TargetBorder");

            Assert.IsTrue(System.Windows.VisualStateManager.GoToState(control, "PointerOver", false));
            AssertSolidColorBrush(target.Background, Colors.Red);

            Assert.IsTrue(System.Windows.VisualStateManager.GoToState(control, "Normal", false));

            AssertSolidColorBrush(target.Background, Colors.Blue);
        });
    }

    [TestMethod]
    public void VisualStateExPreservesOtherGroupSettersWhenOneGroupChanges()
    {
        WpfTestHost.Run(() =>
        {
            var control = CreateControl(
                """
                <VisualStateGroup x:Name="CommonStates">
                    <ui:VisualStateEx x:Name="Normal" />
                    <ui:VisualStateEx x:Name="PointerOver">
                        <ui:VisualStateEx.Setters>
                            <ui:VisualStateSetter Target="TargetBorder.Background" Value="Red" />
                        </ui:VisualStateEx.Setters>
                    </ui:VisualStateEx>
                </VisualStateGroup>
                <VisualStateGroup x:Name="SelectionStates">
                    <ui:VisualStateEx x:Name="Unselected" />
                    <ui:VisualStateEx x:Name="Selected">
                        <ui:VisualStateEx.Setters>
                            <ui:VisualStateSetter Target="TargetBorder.Background" Value="Green" />
                        </ui:VisualStateEx.Setters>
                    </ui:VisualStateEx>
                </VisualStateGroup>
                """,
                """
                <Border x:Name="TargetBorder" Background="Blue" />
                """);

            using var host = new TestWindowHost(control);
            var target = FindTemplateChild<Border>(control, "TargetBorder");

            Assert.IsTrue(System.Windows.VisualStateManager.GoToState(control, "PointerOver", false));
            AssertSolidColorBrush(target.Background, Colors.Red);

            Assert.IsTrue(System.Windows.VisualStateManager.GoToState(control, "Selected", false));
            AssertSolidColorBrush(target.Background, Colors.Green);

            Assert.IsTrue(System.Windows.VisualStateManager.GoToState(control, "Normal", false));
            AssertSolidColorBrush(target.Background, Colors.Green);

            Assert.IsTrue(System.Windows.VisualStateManager.GoToState(control, "Unselected", false));
            AssertSolidColorBrush(target.Background, Colors.Blue);
        });
    }

    [TestMethod]
    public void VisualStateExRunsSetterAndStoryboardInSameState()
    {
        WpfTestHost.Run(() =>
        {
            var control = CreateControl(
                """
                <VisualStateGroup x:Name="CommonStates">
                    <ui:VisualStateEx x:Name="Normal" />
                    <ui:VisualStateEx x:Name="PointerOver">
                        <ui:VisualStateEx.Storyboard>
                            <Storyboard>
                                <DoubleAnimation
                                    Storyboard.TargetName="TargetBorder"
                                    Storyboard.TargetProperty="Opacity"
                                    To="0.5"
                                    Duration="0:0:0" />
                            </Storyboard>
                        </ui:VisualStateEx.Storyboard>
                        <ui:VisualStateEx.Setters>
                            <ui:VisualStateSetter Target="TargetBorder.Background" Value="Red" />
                        </ui:VisualStateEx.Setters>
                    </ui:VisualStateEx>
                </VisualStateGroup>
                """,
                """
                <Border x:Name="TargetBorder" Opacity="1" Background="Blue" />
                """);

            using var host = new TestWindowHost(control);
            var target = FindTemplateChild<Border>(control, "TargetBorder");

            Assert.IsTrue(System.Windows.VisualStateManager.GoToState(control, "PointerOver", false));
            WpfTestHost.DoEvents();

            AssertSolidColorBrush(target.Background, Colors.Red);
            Assert.AreEqual(0.5, target.Opacity, 0.01);
        });
    }

    [TestMethod]
    public void VisualStateExThrowsForMissingTarget()
    {
        WpfTestHost.Run(() =>
        {
            var control = CreateControl(
                """
                <VisualStateGroup x:Name="CommonStates">
                    <ui:VisualStateEx x:Name="Broken">
                        <ui:VisualStateEx.Setters>
                            <ui:VisualStateSetter Target="MissingBorder.Background" Value="Red" />
                        </ui:VisualStateEx.Setters>
                    </ui:VisualStateEx>
                </VisualStateGroup>
                """,
                """
                <Border x:Name="TargetBorder" Background="Blue" />
                """);

            using var host = new TestWindowHost(control);

            Assert.ThrowsExactly<InvalidOperationException>(
                () => System.Windows.VisualStateManager.GoToState(control, "Broken", false));
        });
    }

    [TestMethod]
    public void VisualStateExAppliesNestedTargetPath()
    {
        WpfTestHost.Run(() =>
        {
            var control = CreateControl(
                """
                <VisualStateGroup x:Name="CommonStates">
                    <ui:VisualStateEx x:Name="Normal" />
                    <ui:VisualStateEx x:Name="PointerOver">
                        <ui:VisualStateEx.Setters>
                            <ui:VisualStateSetter Target="TargetRectangle.(Shape.Fill).(SolidColorBrush.Color)" Value="Red" />
                        </ui:VisualStateEx.Setters>
                    </ui:VisualStateEx>
                </VisualStateGroup>
                """,
                """
                <Rectangle x:Name="TargetRectangle" Fill="Blue" />
                """);

            using var host = new TestWindowHost(control);
            var target = FindTemplateChild<Rectangle>(control, "TargetRectangle");

            AssertSolidColorBrush(target.Fill, Colors.Blue);

            Assert.IsTrue(System.Windows.VisualStateManager.GoToState(control, "PointerOver", false));
            AssertSolidColorBrush(target.Fill, Colors.Red);

            Assert.IsTrue(System.Windows.VisualStateManager.GoToState(control, "Normal", false));
            AssertSolidColorBrush(target.Fill, Colors.Blue);
        });
    }

    [TestMethod]
    public void VisualStateExAppliesNestedDynamicResourceTargetPath()
    {
        WpfTestHost.Run(() =>
        {
            var control = CreateControl(
                """
                <VisualStateGroup x:Name="CommonStates">
                    <ui:VisualStateEx x:Name="Normal" />
                    <ui:VisualStateEx x:Name="PointerOver">
                        <ui:VisualStateEx.Setters>
                            <ui:VisualStateSetter Target="TargetRectangle.(Shape.Fill).(SolidColorBrush.Color)" Value="{DynamicResource SetterColor}" />
                        </ui:VisualStateEx.Setters>
                    </ui:VisualStateEx>
                </VisualStateGroup>
                """,
                """
                <Rectangle x:Name="TargetRectangle" Fill="Blue" />
                """);
            control.Resources["SetterColor"] = Colors.Red;

            using var host = new TestWindowHost(control);
            var target = FindTemplateChild<Rectangle>(control, "TargetRectangle");

            Assert.IsTrue(System.Windows.VisualStateManager.GoToState(control, "PointerOver", false));
            AssertSolidColorBrush(target.Fill, Colors.Red);

            Assert.IsTrue(System.Windows.VisualStateManager.GoToState(control, "Normal", false));
            AssertSolidColorBrush(target.Fill, Colors.Blue);
        });
    }

    private static ContentControl CreateControl(string visualStateGroups, string templateBody)
    {
        var templateXaml =
            $$"""
            <ControlTemplate
                xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
                xmlns:ui="clr-namespace:ModernWpf;assembly=ModernWpf"
                TargetType="{x:Type ContentControl}">
                <Grid x:Name="Root">
                    <VisualStateManager.CustomVisualStateManager>
                        <ui:VisualStateManagerEx />
                    </VisualStateManager.CustomVisualStateManager>
                    <VisualStateManager.VisualStateGroups>
                        {{visualStateGroups}}
                    </VisualStateManager.VisualStateGroups>
                    {{templateBody}}
                </Grid>
            </ControlTemplate>
            """;

        return new ContentControl
        {
            Template = (ControlTemplate)XamlReader.Parse(templateXaml)
        };
    }

    private static T FindTemplateChild<T>(Control control, string name)
        where T : FrameworkElement
    {
        control.ApplyTemplate();
        return control.Template?.FindName(name, control) as T
            ?? throw new AssertFailedException($"Expected template child '{name}'.");
    }

    private static void AssertSolidColorBrush(Brush brush, Color expectedColor)
    {
        var solidColorBrush = brush as SolidColorBrush
            ?? throw new AssertFailedException("Expected a SolidColorBrush.");
        Assert.AreEqual(expectedColor, solidColorBrush.Color);
    }
}
