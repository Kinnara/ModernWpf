using System.Linq;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModernWpf.Controls;
using ModernWpf.Controls.Primitives;
using ModernWpf.WinUI.TestApp;
using ModernWpf.WinUI.TestInfra;

namespace ModernWpf.WinUI.Tests.CommonStyles;

[TestClass]
public class MenuFamilyVisualStateTests
{
    [TestMethod]
    public void DefaultMenuStyleUsesOfficialWpfFluentTemplateShape()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var defaultStyle = (Style)Application.Current.FindResource("DefaultMenuStyle");
            var implicitStyle = (Style)Application.Current.FindResource(typeof(Menu));
            Assert.AreEqual(typeof(Menu), defaultStyle.TargetType);
            Assert.AreEqual(typeof(Menu), implicitStyle.TargetType);
            Assert.AreSame(defaultStyle, implicitStyle.BasedOn);

            var setters = defaultStyle.Setters.OfType<Setter>().ToArray();
            AssertDynamicResourceSetter(setters, Control.BackgroundProperty, "MenuBarBackground");
            AssertSetter(setters, UIElement.FocusableProperty, false);
            AssertSetter(setters, Control.OverridesDefaultStyleProperty, true);
            AssertSetter(setters, UIElement.SnapsToDevicePixelsProperty, true);
            Assert.IsTrue(setters.Any(item => item.Property == Control.TemplateProperty));

            var template = (ControlTemplate)setters.Single(item => item.Property == Control.TemplateProperty).Value;
            var border = (Border)template.LoadContent();
            var presenter = VisualTreeTestHelper.EnumerateDescendants(border).OfType<ItemsPresenter>().Single();
            Assert.IsTrue(presenter.ClipToBounds);
        });
    }

    [TestMethod]
    public void DefaultContextMenuStyleUsesOfficialWpfFluentTemplateShape()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var defaultStyle = (Style)Application.Current.FindResource("DefaultContextMenuStyle");
            var implicitStyle = (Style)Application.Current.FindResource(typeof(ContextMenu));
            Assert.AreEqual(typeof(ContextMenu), defaultStyle.TargetType);
            Assert.AreEqual(typeof(ContextMenu), implicitStyle.TargetType);
            Assert.AreSame(defaultStyle, implicitStyle.BasedOn?.BasedOn);

            var setters = defaultStyle.Setters.OfType<Setter>().ToArray();
            AssertDynamicResourceSetter(setters, TextElement.ForegroundProperty, "ContextMenuForeground");
            AssertDynamicResourceSetter(setters, Control.ForegroundProperty, "ContextMenuForeground");
            AssertDynamicResourceSetter(setters, Control.BackgroundProperty, "ContextMenuBackground");
            AssertDynamicResourceSetter(setters, Control.BorderBrushProperty, "ContextMenuBorderBrush");
            AssertSetter(setters, Control.BorderThicknessProperty, new Thickness(1));
            AssertSetter(setters, FrameworkElement.MinWidthProperty, 140.0);
            AssertSetter(setters, Control.PaddingProperty, new Thickness(0));
            AssertSetter(setters, FrameworkElement.MarginProperty, new Thickness(0));
            AssertSetter(setters, ContextMenu.HasDropShadowProperty, false);
            AssertSetter(setters, Grid.IsSharedSizeScopeProperty, true);
            AssertSetter(setters, Popup.PopupAnimationProperty, PopupAnimation.None);
            AssertSetter(setters, UIElement.SnapsToDevicePixelsProperty, true);
            Assert.IsTrue(setters.Any(item => item.Property == Control.TemplateProperty));

            var template = (ControlTemplate)setters.Single(item => item.Property == Control.TemplateProperty).Value;
            var border = (Border)template.LoadContent();
            Assert.AreEqual(new CornerRadius(8), border.CornerRadius);
            Assert.AreEqual(new Thickness(0, 3, 0, 3), border.Padding);
            Assert.IsNull(VisualTreeTestHelper.FindDescendant<ThemeShadowChrome>(border));
        });
    }

    [TestMethod]
    public void DefaultMenuItemStyleUsesOfficialWpfFluentTemplateShape()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var defaultStyle = (Style)Application.Current.FindResource("DefaultMenuItemStyle");
            var implicitStyle = (Style)Application.Current.FindResource(typeof(MenuItem));
            Assert.AreEqual(typeof(MenuItem), defaultStyle.TargetType);
            Assert.AreEqual(typeof(MenuItem), implicitStyle.TargetType);
            Assert.AreSame(defaultStyle, implicitStyle.BasedOn);

            var setters = defaultStyle.Setters.OfType<Setter>().ToArray();
            AssertAncestorAlignmentBinding(setters, Control.HorizontalContentAlignmentProperty);
            AssertAncestorAlignmentBinding(setters, Control.VerticalContentAlignmentProperty);
            AssertDynamicResourceSetter(setters, Control.FocusVisualStyleProperty, "DefaultCollectionFocusVisualStyle");
            AssertSetter(setters, KeyboardNavigation.IsTabStopProperty, true);
            AssertBrushSetter(setters, Control.BackgroundProperty, Colors.Transparent);
            AssertBrushSetter(setters, Control.BorderBrushProperty, Colors.Transparent);
            AssertSetter(setters, Control.BorderThicknessProperty, new Thickness(1));
            AssertSetter(setters, UIElement.FocusableProperty, true);
            AssertSetter(setters, Control.OverridesDefaultStyleProperty, true);

            AssertMenuItemRoleTemplateTrigger(defaultStyle, MenuItemRole.TopLevelHeader, MenuItem.TopLevelHeaderTemplateKey);
            AssertMenuItemRoleTemplateTrigger(defaultStyle, MenuItemRole.TopLevelItem, MenuItem.TopLevelItemTemplateKey);
            AssertMenuItemRoleTemplateTrigger(defaultStyle, MenuItemRole.SubmenuHeader, MenuItem.SubmenuHeaderTemplateKey);
            AssertMenuItemRoleTemplateTrigger(defaultStyle, MenuItemRole.SubmenuItem, MenuItem.SubmenuItemTemplateKey);

            AssertTemplateKey(MenuItem.TopLevelHeaderTemplateKey);
            AssertTemplateKey(MenuItem.TopLevelItemTemplateKey);
            AssertTemplateKey(MenuItem.SubmenuHeaderTemplateKey);
            AssertTemplateKey(MenuItem.SubmenuItemTemplateKey);

            var separatorStyle = (Style)Application.Current.FindResource(MenuItem.SeparatorStyleKey);
            Assert.AreEqual(typeof(Separator), separatorStyle.TargetType);
            Assert.IsTrue(separatorStyle.Setters.OfType<Setter>()
                .Any(item => item.Property == Control.OverridesDefaultStyleProperty && Equals(item.Value, true)));
        });
    }

    [TestMethod]
    public void StyledWpfMenuItemCanOpenTopLevelSubmenuThroughAutomation()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var root = new StackPanel();
            root.Resources.Add(
                typeof(MenuItem),
                new Style(typeof(MenuItem), (Style)Application.Current.FindResource("DefaultMenuItemStyle"))
                {
                    Setters =
                    {
                        new EventSetter(MenuItem.ClickEvent, new RoutedEventHandler(FocusClickedGalleryMenuItem))
                    }
                });

            var menu = new Menu();
            var fileItem = new MenuItem { Header = "File" };
            var newItem = new MenuItem { Header = "New" };
            fileItem.Items.Add(newItem);
            menu.Items.Add(fileItem);
            root.Children.Add(menu);

            using var host = new TestWindowHost(root, width: 320, height: 160);
            host.UpdateLayout();

            Assert.AreEqual(MenuItemRole.TopLevelHeader, fileItem.Role);

            var peer = FrameworkElementAutomationPeer.CreatePeerForElement(fileItem);
            Assert.IsNotNull(peer);

            var provider = peer.GetPattern(PatternInterface.ExpandCollapse) as IExpandCollapseProvider;
            Assert.IsNotNull(provider);

            provider!.Expand();
            WpfTestHost.DoEvents();

            Assert.IsTrue(fileItem.IsSubmenuOpen, "The styled top-level MenuItem must open through UIA Expand.");
            Assert.AreEqual(ExpandCollapseState.Expanded, provider.ExpandCollapseState);

            provider.Collapse();
            WpfTestHost.DoEvents();

            Assert.IsFalse(fileItem.IsSubmenuOpen);
            Assert.AreEqual(ExpandCollapseState.Collapsed, provider.ExpandCollapseState);
        });
    }

    private static void FocusClickedGalleryMenuItem(object sender, RoutedEventArgs e)
    {
        if (sender is MenuItem menuItem)
        {
            if (menuItem.Parent is MenuItem parentMenuItem)
            {
                parentMenuItem.Focus();
            }
            else
            {
                menuItem.Focus();
            }
        }
    }

    [TestMethod]
    public void MenuFamilyDeletesModernWpfSpecificTemplateGuesses()
    {
        var repoRoot = FindRepoRoot();
        var menuFiles = new[]
        {
            "Menu.xaml",
            "ContextMenu.xaml",
            "MenuItem.xaml"
        };

        var text = string.Join(
            "\n",
            menuFiles.Select(file => System.IO.File.ReadAllText(System.IO.Path.Combine(repoRoot, "ModernWpf", "Styles", file))));

        Assert.IsFalse(text.Contains("VisualStateEx", System.StringComparison.Ordinal));
        Assert.IsFalse(text.Contains("ContentPresenterEx", System.StringComparison.Ordinal));
        Assert.IsFalse(text.Contains("BorderEx", System.StringComparison.Ordinal));
        Assert.IsFalse(text.Contains("ThemeShadowChrome", System.StringComparison.Ordinal));
        Assert.IsFalse(text.Contains("MenuPopup", System.StringComparison.Ordinal));
        Assert.IsFalse(text.Contains("ControlHelper.CornerRadius", System.StringComparison.Ordinal));
        Assert.IsFalse(text.Contains("MenuItemHelper.VisualStateSettersEnabled", System.StringComparison.Ordinal));
    }

    [TestMethod]
    public void ThemeDictionariesExposeOfficialMenuFamilyAliases()
    {
        WpfTestHost.Run(() =>
        {
            foreach (var themeName in new[] { "Light", "Dark" })
            {
                AssertThemeResourceReference(themeName, "MenuBarForeground", "TextFillColorPrimaryBrush");
                AssertThemeResourceReference(themeName, "ContextMenuBackground", "AcrylicBackgroundFillColorDefaultBrush");
                AssertThemeResourceReference(themeName, "ContextMenuBorderBrush", "SurfaceStrokeColorFlyoutBrush");
                AssertThemeResourceReference(themeName, "ContextMenuForeground", "TextFillColorPrimaryBrush");
                AssertThemeResourceReference(themeName, "FlyoutBackground", "AcrylicBackgroundFillColorDefaultBrush");
                AssertThemeResourceReference(themeName, "FlyoutBorderBrush", "SurfaceStrokeColorFlyoutBrush");
                AssertThemeResourceReference(themeName, "CheckBoxBackground", "SubtleFillColorTransparentBrush");
                AssertThemeResourceReference(themeName, "CheckBoxBorderBrush", "SubtleFillColorTransparentBrush");
            }

            AssertThemeResourceReference("HighContrast", "MenuBarForeground", "SystemColorWindowTextColorBrush");
            AssertThemeResourceReference("HighContrast", "ContextMenuBackground", "SystemColorWindowColorBrush");
            AssertThemeResourceReference("HighContrast", "ContextMenuBorderBrush", "SystemColorWindowTextColorBrush");
            AssertThemeResourceReference("HighContrast", "ContextMenuForeground", "SystemColorWindowTextColorBrush");
            AssertThemeResourceReference("HighContrast", "FlyoutBackground", "SystemColorWindowColorBrush");
            AssertThemeResourceReference("HighContrast", "FlyoutBorderBrush", "SystemColorWindowTextColorBrush");
            AssertThemeResourceReference("HighContrast", "CheckBoxBackground", "SystemColorWindowColorBrush");
            AssertThemeResourceReference("HighContrast", "CheckBoxBorderBrush", "SystemColorWindowColorBrush");
        });
    }

    private static void AssertTemplateKey(object templateKey)
    {
        var template = (ControlTemplate)Application.Current.FindResource(templateKey);
        Assert.AreEqual(typeof(MenuItem), template.TargetType);
    }

    private static void AssertMenuItemRoleTemplateTrigger(Style style, MenuItemRole role, object templateKey)
    {
        var trigger = style.Triggers.OfType<Trigger>()
            .Single(item => item.Property == MenuItem.RoleProperty && Equals(item.Value, role));
        var setter = trigger.Setters.OfType<Setter>()
            .Single(item => item.Property == Control.TemplateProperty);
        if (setter.Value is StaticResourceExtension staticResource)
        {
            Assert.AreEqual(templateKey, staticResource.ResourceKey);
        }
        else
        {
            Assert.AreSame(Application.Current.FindResource(templateKey), setter.Value);
        }
    }

    private static void AssertAncestorAlignmentBinding(Setter[] setters, DependencyProperty property)
    {
        var setter = setters.Single(item => item.Property == property);
        Assert.IsInstanceOfType(setter.Value, typeof(Binding));
        var binding = (Binding)setter.Value;
        Assert.AreEqual(property.Name, binding.Path.Path);
        Assert.AreEqual(typeof(ItemsControl), binding.RelativeSource.AncestorType);
    }

    private static void AssertSetter(Setter[] setters, DependencyProperty property, object value)
    {
        var setter = setters.Single(item => item.Property == property);
        Assert.AreEqual(value, setter.Value);
    }

    private static void AssertBrushSetter(Setter[] setters, DependencyProperty property, Color color)
    {
        var setter = setters.Single(item => item.Property == property);
        Assert.IsInstanceOfType(setter.Value, typeof(SolidColorBrush));
        Assert.AreEqual(color, ((SolidColorBrush)setter.Value).Color);
    }

    private static void AssertDynamicResourceSetter(Setter[] setters, DependencyProperty property, object resourceKey)
    {
        var setter = setters.FirstOrDefault(item =>
            (item.Property == property || item.Property.Name == property.Name) &&
            item.Value is DynamicResourceExtension dynamicResource &&
            Equals(dynamicResource.ResourceKey, resourceKey));
        Assert.IsNotNull(setter, $"Expected a dynamic resource setter for {property.Name} -> {resourceKey}.");
        Assert.IsInstanceOfType(setter!.Value, typeof(DynamicResourceExtension));
        var dynamicResource = (DynamicResourceExtension)setter.Value;
        Assert.AreEqual(resourceKey, dynamicResource.ResourceKey);
    }

    private static T GetTemplateChild<T>(Control control, string name, bool required = true)
        where T : DependencyObject
    {
        var child = control.Template.FindName(name, control) as T;
        if (child == null && required)
        {
            throw new AssertFailedException($"Expected {control.GetType().Name} template child '{name}' to be a {typeof(T).Name}.");
        }
        return child!;
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
