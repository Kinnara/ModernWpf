using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Markup;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModernWpf.Controls;
using ModernWpf.Controls.Primitives;
using ModernWpf.WinUI.TestApp;
using ModernWpf.WinUI.TestInfra;

namespace ModernWpf.WinUI.Tests.CommonStyles;

[TestClass]
public class ScrollViewerVisualStateTests
{
    [TestMethod]
    public void DefaultScrollViewerStyleUsesOfficialWpfFluentTemplateShape()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var defaultStyle = (Style)Application.Current.FindResource("DefaultScrollViewerStyle");
            var implicitStyle = (Style)Application.Current.FindResource(typeof(ScrollViewer));
            Assert.AreEqual(typeof(ScrollViewer), defaultStyle.TargetType);
            Assert.AreEqual(typeof(ScrollViewer), implicitStyle.TargetType);
            Assert.AreSame(defaultStyle, implicitStyle.BasedOn);

            var setters = defaultStyle.Setters.OfType<Setter>().ToArray();
            AssertSetter(setters, FrameworkElement.MarginProperty, new Thickness(0));
            AssertSetter(setters, Control.PaddingProperty, new Thickness(0));
            AssertSetter(setters, UIElement.SnapsToDevicePixelsProperty, true);
            AssertSetter(setters, Control.OverridesDefaultStyleProperty, true);
            AssertNoSetter(setters, UIElement.FocusableProperty);
            AssertNoSetter(setters, Control.BorderBrushProperty);
            AssertNoSetter(setters, Control.BorderThicknessProperty);
            AssertNoSetter(setters, Control.BackgroundProperty);
            AssertNoSetter(setters, Control.FocusVisualStyleProperty);
            AssertNoSetter(setters, FocusVisualHelper.UseSystemFocusVisualsProperty);

            var template = (ControlTemplate)setters.Single(item => item.Property == Control.TemplateProperty).Value;
            Assert.AreEqual(typeof(ScrollViewer), template.TargetType);
            Assert.IsInstanceOfType(template.LoadContent(), typeof(Grid));
        });
    }

    [TestMethod]
    public void StockScrollViewerAppliesOfficialWpfTemplateParts()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var scrollViewer = new ScrollViewer
            {
                Content = new Border { Width = 400, Height = 400 },
                HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto
            };

            using var host = new TestWindowHost(scrollViewer, width: 120, height: 120);
            host.UpdateLayout();

            Assert.IsNotNull(GetTemplateChild<ScrollContentPresenter>(scrollViewer, "PART_ScrollContentPresenter"));
            Assert.IsNotNull(GetTemplateChild<ScrollBar>(scrollViewer, "PART_VerticalScrollBar"));
            Assert.IsNotNull(GetTemplateChild<ScrollBar>(scrollViewer, "PART_HorizontalScrollBar"));
            Assert.IsNull(VisualTreeTestHelper.FindDescendant<ContentPresenterEx>(scrollViewer));
        });
    }

    [TestMethod]
    public void TextControlContentHostStyleRetainsModernWpfSupportTemplate()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var defaultStyle = (Style)Application.Current.FindResource("DefaultScrollViewerStyle");
            var textHostStyle = (Style)Application.Current.FindResource("TextControlContentHostStyle");
            Assert.AreEqual(typeof(ScrollViewer), textHostStyle.TargetType);
            Assert.AreSame(defaultStyle, textHostStyle.BasedOn);

            var setters = textHostStyle.Setters.OfType<Setter>().ToArray();
            AssertSetter(setters, UIElement.FocusableProperty, false);
            AssertSetter(setters, TextControlContentHostHelper.ContentPresenterMarginProperty, new Thickness(-2, 0, -2, 0));

            var textHost = new ScrollViewer
            {
                Style = textHostStyle,
                Content = new TextBlock { Text = "Text" }
            };

            using var host = new TestWindowHost(textHost, width: 120, height: 40);
            host.UpdateLayout();

            Assert.IsNotNull(GetTemplateChild<ScrollContentPresenter>(textHost, "PART_ScrollContentPresenter"));
            Assert.IsNotNull(GetTemplateChild<ScrollBar>(textHost, "PART_VerticalScrollBar"));
            Assert.IsNotNull(GetTemplateChild<ScrollBar>(textHost, "PART_HorizontalScrollBar"));
        });
    }

    [TestMethod]
    public void ScrollViewerFileDeletesOldDefaultStyleGuesses()
    {
        var repoRoot = FindRepoRoot();
        var text = System.IO.File.ReadAllText(System.IO.Path.Combine(repoRoot, "ModernWpf", "Styles", "ScrollViewer.xaml"));
        var defaultStyleText = text[..text.IndexOf("TextControlContentHostStyle", System.StringComparison.Ordinal)];

        Assert.IsFalse(defaultStyleText.Contains("ScrollViewerSeparator", System.StringComparison.Ordinal));
        Assert.IsFalse(defaultStyleText.Contains("FocusVisualHelper", System.StringComparison.Ordinal));
        Assert.IsFalse(defaultStyleText.Contains("ControlHelper.CornerRadius", System.StringComparison.Ordinal));
        Assert.IsFalse(defaultStyleText.Contains("AutomationProperties.AutomationId", System.StringComparison.Ordinal));
        Assert.IsFalse(defaultStyleText.Contains("CanHorizontallyScroll=\"False\"", System.StringComparison.Ordinal));
        Assert.IsFalse(defaultStyleText.Contains("CanVerticallyScroll=\"False\"", System.StringComparison.Ordinal));
        Assert.IsFalse(text.Contains("VisualStateEx", System.StringComparison.Ordinal));
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

    private static T GetTemplateChild<T>(Control control, string name)
        where T : DependencyObject
    {
        return control.Template.FindName(name, control) as T
            ?? throw new AssertFailedException($"Expected {control.GetType().Name} template child '{name}' to be a {typeof(T).Name}.");
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
