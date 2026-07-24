using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModernWpf.WinUI.TestInfra;

namespace ModernWpf.Theme.Tests;

[TestClass]
public static class TestAssembly
{
    [AssemblyInitialize]
    public static void Initialize(TestContext context)
    {
        WpfTestHost.Run(() => ThemeTestApplication.EnsureInitialized());
    }

    [AssemblyCleanup]
    public static void Cleanup()
    {
        WpfTestHost.Shutdown();
    }
}
