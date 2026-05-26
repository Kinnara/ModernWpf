using System.Windows;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModernWpf.Markup;

namespace ModernWpf.Theme.Tests;

[TestClass]
public class ThemeResourceExtensionTests
{
    [TestMethod]
    public void SystemColorBindingsRefreshWhenHighContrastToggles()
    {
        Assert.IsTrue(ThemeResourceExtension.ShouldRefreshSystemColorBindings(
            nameof(SystemParameters.HighContrast),
            isHighContrast: true));
        Assert.IsTrue(ThemeResourceExtension.ShouldRefreshSystemColorBindings(
            nameof(SystemParameters.HighContrast),
            isHighContrast: false));
    }

    [TestMethod]
    public void SystemColorBindingsRefreshForSystemParameterChangesWhileHighContrastIsActive()
    {
        Assert.IsTrue(ThemeResourceExtension.ShouldRefreshSystemColorBindings(
            nameof(SystemParameters.WindowGlassColor),
            isHighContrast: true));
        Assert.IsFalse(ThemeResourceExtension.ShouldRefreshSystemColorBindings(
            nameof(SystemParameters.WindowGlassColor),
            isHighContrast: false));
    }
}
