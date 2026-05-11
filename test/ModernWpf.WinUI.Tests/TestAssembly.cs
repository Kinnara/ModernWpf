using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModernWpf.WinUI.TestApp;
using ModernWpf.WinUI.TestInfra;

namespace ModernWpf.WinUI.Tests;

[TestClass]
public static class TestAssembly
{
    [AssemblyInitialize]
    public static void Initialize(TestContext context)
    {
        WpfTestHost.Run(() => TestApplication.EnsureInitialized());
    }

    [AssemblyCleanup]
    public static void Cleanup()
    {
        WpfTestHost.Shutdown();
    }
}
