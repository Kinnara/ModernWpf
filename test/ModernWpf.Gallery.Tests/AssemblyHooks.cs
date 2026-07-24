using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ModernWpf.Gallery.Tests
{
    [TestClass]
    public sealed class AssemblyHooks
    {
        [AssemblyInitialize]
        public static void Initialize(TestContext context)
        {
            WpfTestHost.EnsureStarted();
        }

        [AssemblyCleanup]
        public static void Cleanup()
        {
            WpfTestHost.Shutdown();
        }
    }
}
