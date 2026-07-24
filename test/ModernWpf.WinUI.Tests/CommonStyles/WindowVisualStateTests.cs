using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Shell;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModernWpf.Controls.Primitives;
using ModernWpf.WinUI.TestApp;
using ModernWpf.WinUI.TestInfra;

namespace ModernWpf.WinUI.Tests.CommonStyles;

[TestClass]
public class WindowVisualStateTests
{
    [TestMethod]
    public void WindowStyleUsesOfficialWpfFluentResourceSurfaceWithModernWpfChrome()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var baseStyle = AssertStyle("BaseWindowStyle");
            var defaultStyle = AssertStyle("DefaultWindowStyle");
            Assert.AreSame(baseStyle, defaultStyle.BasedOn);

            AssertDynamicResourceSetter(baseStyle, Control.ForegroundProperty, "WindowForeground");
            AssertDynamicResourceSetter(baseStyle, Control.BackgroundProperty, "WindowBackground");
            AssertDynamicResourceSetter(baseStyle, Control.BorderBrushProperty, "WindowBorder");
            AssertDynamicResourceSetter(baseStyle, WindowChrome.WindowChromeProperty, "DefaultWindowChrome");
            AssertStyleSetter(baseStyle, Control.BorderThicknessProperty, new Thickness(1));
            AssertStyleSetter(baseStyle, WindowHelper.FixMaximizedWindowProperty, true);
            AssertStyleSetter(defaultStyle, FrameworkElement.OverridesDefaultStyleProperty, true);
        });
    }

    [TestMethod]
    public void WindowTemplateUsesWpfContentPresenterAndKeepsCustomTitleBar()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var content = new TextBlock { Text = "Window content" };
            using var host = new TestWindowHost(content, width: 320, height: 240);
            host.UpdateLayout();

            var window = host.Window;
            Assert.AreSame(window.TryFindResource("WindowForeground"), window.Foreground);
            Assert.AreSame(window.TryFindResource("WindowBackground"), window.Background);
            Assert.IsNotNull(WindowChrome.GetWindowChrome(window));
            Assert.IsTrue(WindowHelper.GetFixMaximizedWindow(window));

            Assert.IsNotNull(VisualTreeTestHelper.FindDescendant<TitleBarControl>(window));
            Assert.IsNotNull(VisualTreeTestHelper.FindDescendant<ResizeGrip>(window));

            var presenter = VisualTreeTestHelper.EnumerateDescendants(window)
                .OfType<ContentPresenter>()
                .FirstOrDefault(item => ReferenceEquals(item.Content, content))
                ?? throw new AssertFailedException("Expected the Window content host to be a WPF ContentPresenter.");

            Assert.AreEqual(typeof(ContentPresenter), presenter.GetType());
        });
    }

    [TestMethod]
    public void WindowStyleDocumentsOfficialWpfFluentSubstitutions()
    {
        var repoRoot = FindRepoRoot();
        var stylePath = Path.Combine(repoRoot, "ModernWpf", "Styles", "Window.xaml");
        var text = File.ReadAllText(stylePath);

        Assert.IsTrue(text.Contains("WindowForeground", System.StringComparison.Ordinal));
        Assert.IsTrue(text.Contains("WindowBackground", System.StringComparison.Ordinal));
        Assert.IsTrue(text.Contains("<ContentPresenter x:Name=\"ContentPresenter\"", System.StringComparison.Ordinal));
        Assert.IsTrue(text.Contains("TitleBarControl", System.StringComparison.Ordinal));
        Assert.IsTrue(text.Contains("WindowChrome.WindowChrome", System.StringComparison.Ordinal));
        Assert.IsTrue(text.Contains("WindowHelper.FixMaximizedWindow", System.StringComparison.Ordinal));
        Assert.IsFalse(text.Contains("ContentPresenterEx", System.StringComparison.Ordinal));
        Assert.IsFalse(text.Contains("MS.Internal", System.StringComparison.Ordinal));
        Assert.IsFalse(text.Contains("Fluent.Controls", System.StringComparison.Ordinal));
        Assert.IsFalse(text.Contains("System.Runtime", System.StringComparison.Ordinal));
    }

    private static Style AssertStyle(object resourceKey)
    {
        var style = Application.Current.FindResource(resourceKey) as Style;
        Assert.IsNotNull(style, $"Expected style resource {resourceKey}.");
        return style!;
    }

    private static void AssertDynamicResourceSetter(Style style, DependencyProperty property, object expectedResourceKey)
    {
        var setter = style.Setters.OfType<Setter>().SingleOrDefault(setter => setter.Property == property);
        Assert.IsNotNull(setter, $"Expected local setter for {property.Name}.");

        var resource = setter!.Value as DynamicResourceExtension;
        Assert.IsNotNull(resource, $"Expected {property.Name} to use DynamicResource.");
        Assert.AreEqual(expectedResourceKey, resource!.ResourceKey);
    }

    private static void AssertStyleSetter(Style style, DependencyProperty property, object expectedValue)
    {
        var setter = style.Setters.OfType<Setter>().SingleOrDefault(setter => setter.Property == property);
        Assert.IsNotNull(setter, $"Expected local setter for {property.Name}.");
        Assert.AreEqual(expectedValue, setter!.Value);
    }

    private static string FindRepoRoot()
    {
        var directory = new DirectoryInfo(System.AppContext.BaseDirectory);

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
