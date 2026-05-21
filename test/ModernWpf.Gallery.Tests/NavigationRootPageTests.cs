using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModernWpf.Gallery.Shell;

namespace ModernWpf.Gallery.Tests
{
    [TestClass]
    public class NavigationRootPageTests
    {
        [DataTestMethod]
        [DataRow("TeachingTip")]
        [DataRow("item/TeachingTip")]
        [DataRow("/item/TeachingTip")]
        [DataRow("winui3gallery://item/TeachingTip")]
        [DataRow("winui3gallerydev://item/TeachingTip")]
        public void ResolveNavigationTargetAcceptsItemLinks(string value)
        {
            var target = NavigationRootPage.ResolveNavigationTarget(value);

            Assert.IsNotNull(target);
            Assert.AreEqual(NavigationTargetKind.Item, target.Kind);
            Assert.AreEqual("TeachingTip", target.UniqueId);
        }

        [DataTestMethod]
        [DataRow("DialogsAndFlyouts")]
        [DataRow("category/DialogsAndFlyouts")]
        [DataRow("/category/DialogsAndFlyouts")]
        [DataRow("winui3gallery://category/DialogsAndFlyouts")]
        [DataRow("winui3gallerydev://category/DialogsAndFlyouts")]
        public void ResolveNavigationTargetAcceptsCategoryLinks(string value)
        {
            var target = NavigationRootPage.ResolveNavigationTarget(value);

            Assert.IsNotNull(target);
            Assert.AreEqual(NavigationTargetKind.Group, target.Kind);
            Assert.AreEqual("DialogsAndFlyouts", target.UniqueId);
        }

        [DataTestMethod]
        [DataRow("")]
        [DataRow(" ")]
        [DataRow("NewControls")]
        public void ResolveNavigationTargetAcceptsHomeLinks(string value)
        {
            var target = NavigationRootPage.ResolveNavigationTarget(value);

            Assert.IsNotNull(target);
            Assert.AreEqual(NavigationTargetKind.Home, target.Kind);
        }

        [TestMethod]
        public void ResolveNavigationTargetAcceptsAllControls()
        {
            var target = NavigationRootPage.ResolveNavigationTarget("AllControls");

            Assert.IsNotNull(target);
            Assert.AreEqual(NavigationTargetKind.AllControls, target.Kind);
        }

        [DataTestMethod]
        [DataRow("WhatsNew")]
        [DataRow("What's New")]
        [DataRow("Whats New")]
        [DataRow("/WhatsNew")]
        public void ResolveNavigationTargetAcceptsWhatsNew(string value)
        {
            var target = NavigationRootPage.ResolveNavigationTarget(value);

            Assert.IsNotNull(target);
            Assert.AreEqual(NavigationTargetKind.WhatsNew, target.Kind);
        }

        [DataTestMethod]
        [DataRow("Settings")]
        [DataRow("settings")]
        [DataRow("/settings")]
        public void ResolveNavigationTargetAcceptsSettings(string value)
        {
            var target = NavigationRootPage.ResolveNavigationTarget(value);

            Assert.IsNotNull(target);
            Assert.AreEqual(NavigationTargetKind.Settings, target.Kind);
        }

        [TestMethod]
        public void ResolveNavigationTargetRejectsUnknownLinks()
        {
            var target = NavigationRootPage.ResolveNavigationTarget("item/NotAControl");

            Assert.IsNull(target);
        }
    }
}
