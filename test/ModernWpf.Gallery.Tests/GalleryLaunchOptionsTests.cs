using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModernWpf.Gallery.Testing;

namespace ModernWpf.Gallery.Tests
{
    [TestClass]
    public class GalleryLaunchOptionsTests
    {
        [TestMethod]
        public void ParseAcceptsPositionalRoute()
        {
            var options = GalleryLaunchOptions.Parse(new[] { "item/TeachingTip" });

            Assert.AreEqual("item/TeachingTip", options.InitialRoute);
            Assert.IsFalse(options.VisualTestMode);
        }

        [TestMethod]
        public void ParseAcceptsVisualTestArguments()
        {
            var options = GalleryLaunchOptions.Parse(new[]
            {
                "--visual-test",
                "--route", "category/DialogsAndFlyouts",
                "--theme=Dark",
                "--visual-artifact-dir", "artifacts/visual-checks/run"
            });

            Assert.IsTrue(options.VisualTestMode);
            Assert.AreEqual("category/DialogsAndFlyouts", options.InitialRoute);
            Assert.AreEqual("Dark", options.Theme);
            Assert.AreEqual("artifacts/visual-checks/run", options.ArtifactDirectory);
        }
    }
}
