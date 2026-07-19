using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModernWpf;
using ModernWpf.WinUI.TestApp;
using ModernWpf.WinUI.TestInfra;

namespace ModernWpf.WinUI.Tests;

[TestClass]
public static class TestAssembly
{
    [AssemblyInitialize]
    public static void Initialize(TestContext context)
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            // Theme dictionaries are lazy. Load every variant on the shared STA so a
            // source-only test cannot accidentally give HighContrast resources a
            // different dispatcher owner before a later rendered-control test uses them.
            _ = ThemeResources.Current.GetThemeDictionary("Light");
            _ = ThemeResources.Current.GetThemeDictionary("Dark");
            _ = ThemeResources.Current.GetThemeDictionary("HighContrast");
        });
    }

    [AssemblyCleanup]
    public static void Cleanup()
    {
        WpfTestHost.Shutdown();
    }
}
