using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModernWpf;
using ModernWpf.WinUI.TestApp;
using ModernWpf.WinUI.TestInfra;

namespace ModernWpf.WinUI.Tests.CommonStyles;

[TestClass]
public class StockProgressBarVisualStateTests
{
    [TestMethod]
    public void DefaultProgressBarStyleUsesOfficialWpfFluentTemplateShape()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var defaultStyle = (Style)Application.Current.FindResource("DefaultProgressBarStyle");
            var implicitStyle = (Style)Application.Current.FindResource(typeof(ProgressBar));
            Assert.AreEqual(typeof(ProgressBar), defaultStyle.TargetType);
            Assert.AreEqual(typeof(ProgressBar), implicitStyle.TargetType);
            Assert.AreSame(defaultStyle, implicitStyle.BasedOn);

            var setters = defaultStyle.Setters.OfType<Setter>().ToArray();
            AssertDynamicResourceSetter(setters, Control.ForegroundProperty, "ProgressBarForeground");
            AssertDynamicResourceSetter(setters, Control.BackgroundProperty, "ProgressBarBackground");
            AssertDynamicResourceSetter(setters, Control.BorderBrushProperty, "ProgressBarBorderBrush");
            AssertSetter(setters, FrameworkElement.HeightProperty, 4.0);
            AssertSetter(setters, UIElement.SnapsToDevicePixelsProperty, true);
            AssertSetter(setters, Control.OverridesDefaultStyleProperty, true);
            AssertNoSetter(setters, Control.BorderThicknessProperty);
            AssertNoSetter(setters, FrameworkElement.MinHeightProperty);
            AssertNoSetter(setters, Control.IsTabStopProperty);

            var template = (ControlTemplate)setters.Single(item => item.Property == Control.TemplateProperty).Value;
            Assert.AreEqual(typeof(ProgressBar), template.TargetType);

            var root = (FrameworkElement)template.LoadContent();
            Assert.IsInstanceOfType(root, typeof(Grid));

            var group = VisualStateManager.GetVisualStateGroups(root)
                .OfType<VisualStateGroup>()
                .Single(item => item.Name == "CommonStates");
            CollectionAssert.AreEqual(new[] { "Determinate", "Indeterminate" }, group.States.OfType<VisualState>().Select(item => item.Name).ToArray());
            Assert.IsFalse(group.States.OfType<VisualState>().Any(item => item.GetType().Name == "VisualStateEx"));

            var indeterminate = group.States.OfType<VisualState>().Single(item => item.Name == "Indeterminate");
            Assert.AreEqual(RepeatBehavior.Forever, indeterminate.Storyboard.RepeatBehavior);
            Assert.IsTrue(indeterminate.Storyboard.Children.OfType<DoubleAnimationUsingKeyFrames>().Any());
            Assert.IsTrue(indeterminate.Storyboard.Children.OfType<PointAnimationUsingKeyFrames>().Any());
        });
    }

    [TestMethod]
    public void StockProgressBarAppliesOfficialWpfTemplatePartsAndTriggers()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var progressBar = new ProgressBar
            {
                Minimum = 0,
                Maximum = 100,
                Value = 50
            };

            using var host = new TestWindowHost(progressBar, width: 160, height: 40);
            host.UpdateLayout();

            Assert.AreEqual(4.0, progressBar.Height);
            Assert.IsInstanceOfType(GetTemplateChild<Grid>(progressBar, "TemplateRoot"), typeof(Grid));
            Assert.IsInstanceOfType(GetTemplateChild<Border>(progressBar, "TrackBorder"), typeof(Border));
            Assert.IsInstanceOfType(GetTemplateChild<Rectangle>(progressBar, "PART_Track"), typeof(Rectangle));
            Assert.IsInstanceOfType(GetTemplateChild<Grid>(progressBar, "PART_Indicator"), typeof(Grid));
            Assert.IsInstanceOfType(GetTemplateChild<Rectangle>(progressBar, "Indicator"), typeof(Rectangle));
            Assert.IsInstanceOfType(GetTemplateChild<Rectangle>(progressBar, "Animation"), typeof(Rectangle));

            progressBar.IsIndeterminate = true;
            host.UpdateLayout();

            var indicator = GetTemplateChild<Rectangle>(progressBar, "Indicator");
            Assert.AreEqual(Visibility.Collapsed, indicator.Visibility);
            Assert.AreSame(progressBar.TryFindResource("ProgressBarIndeterminateBackground"), progressBar.Background);
            Assert.AreSame(progressBar.TryFindResource("ProgressBarIndeterminateBorderBrush"), progressBar.BorderBrush);
        });
    }

    [TestMethod]
    public void StockProgressBarVerticalOrientationUsesOfficialLayoutTransform()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var progressBar = new ProgressBar
            {
                Orientation = Orientation.Vertical
            };

            using var host = new TestWindowHost(progressBar, width: 40, height: 160);
            host.UpdateLayout();

            var root = GetTemplateChild<Grid>(progressBar, "TemplateRoot");
            Assert.IsInstanceOfType(root.LayoutTransform, typeof(RotateTransform));
            Assert.AreEqual(-90.0, ((RotateTransform)root.LayoutTransform).Angle);
        });
    }

    [TestMethod]
    public void StockProgressBarDeletesModernWpfWrapperGuess()
    {
        var repoRoot = FindRepoRoot();
        var text = System.IO.File.ReadAllText(System.IO.Path.Combine(repoRoot, "ModernWpf", "Styles", "ProgressBar.xaml"));

        Assert.IsFalse(text.Contains("local:ProgressBar", System.StringComparison.Ordinal));
        Assert.IsFalse(text.Contains("ModernWpf.Controls", System.StringComparison.Ordinal));
        Assert.IsFalse(text.Contains("ControlHelper.CornerRadius", System.StringComparison.Ordinal));
        Assert.IsFalse(text.Contains("ProgressBarThemeMinHeight", System.StringComparison.Ordinal));
        Assert.IsFalse(text.Contains("ProgressBarBorderThemeThickness", System.StringComparison.Ordinal));
        Assert.IsFalse(text.Contains("VisualStateEx", System.StringComparison.Ordinal));
        Assert.IsTrue(text.Contains("PART_Indicator", System.StringComparison.Ordinal));
        Assert.IsTrue(text.Contains("ProgressBarIndeterminateBackground", System.StringComparison.Ordinal));
    }

    [TestMethod]
    public void ThemeDictionariesExposeOfficialProgressBarAliases()
    {
        WpfTestHost.Run(() =>
        {
            foreach (var themeName in new[] { "Light", "Dark" })
            {
                AssertThemeResourceReference(themeName, "ProgressBarIndeterminateBackground", "ControlFillColorTransparentBrush");
                AssertThemeResourceReference(themeName, "ProgressBarIndeterminateBorderBrush", "ControlFillColorTransparentBrush");
            }

            AssertThemeResourceReference("HighContrast", "ProgressBarIndeterminateBackground", "SystemControlTransparentBrush");
            AssertThemeResourceReference("HighContrast", "ProgressBarIndeterminateBorderBrush", "SystemControlTransparentBrush");
        });
    }

    private static void AssertSetter(Setter[] setters, DependencyProperty property, object value)
    {
        var setter = setters.Single(item => item.Property == property);
        Assert.AreEqual(value, setter.Value);
    }

    private static void AssertNoSetter(Setter[] setters, DependencyProperty property)
    {
        Assert.IsFalse(setters.Any(item => item.Property == property), $"Unexpected setter for {property.Name}.");
    }

    private static void AssertDynamicResourceSetter(Setter[] setters, DependencyProperty property, object resourceKey)
    {
        var setter = setters.Single(item => item.Property == property);
        Assert.IsInstanceOfType(setter.Value, typeof(DynamicResourceExtension));
        var dynamicResource = (DynamicResourceExtension)setter.Value;
        Assert.AreEqual(resourceKey, dynamicResource.ResourceKey);
    }

    private static T GetTemplateChild<T>(Control control, string name)
        where T : DependencyObject
    {
        return control.Template.FindName(name, control) as T
            ?? throw new AssertFailedException($"Expected {control.GetType().Name} template child '{name}' to be a {typeof(T).Name}.");
    }

    private static void AssertThemeResourceReference(string themeName, string key, object expectedResourceKey)
    {
        var theme = ThemeResources.Current.GetThemeDictionary(themeName);
        Assert.IsTrue(theme.Contains(expectedResourceKey), $"Theme is missing {expectedResourceKey}.");
        Assert.AreSame(theme[expectedResourceKey], theme[key], key);
    }

    private static string FindRepoRoot()
    {
        var directory = new System.IO.DirectoryInfo(System.AppContext.BaseDirectory);

        while (directory != null)
        {
            if (System.IO.File.Exists(System.IO.Path.Combine(directory.FullName, "ModernWpf.sln")))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        Assert.Fail("Could not locate ModernWpf.sln from the test output directory.");
        return string.Empty;
    }
}
