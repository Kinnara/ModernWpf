using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModernWpf.Gallery.Shell;

namespace ModernWpf.Gallery.Tests
{
    [TestClass]
    public class NavigationRootPageTests
    {
        [TestMethod]
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

        [TestMethod]
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

        [TestMethod]
        [DataRow("category/Design%20Guidance", "DesignGuidance")]
        [DataRow("category/Basic%20Input", "BasicInput")]
        [DataRow("category/Date%20%26%20Calendar", "DateAndCalendar")]
        [DataRow("category/Status%20%26%20Info", "StatusAndInfo")]
        [DataRow("winui3gallery://category/Date%20%26%20Calendar", "DateAndCalendar")]
        public void ResolveNavigationTargetAcceptsOfficialWpfGalleryCategoryIds(string value, string expectedUniqueId)
        {
            var target = NavigationRootPage.ResolveNavigationTarget(value);

            Assert.IsNotNull(target);
            Assert.AreEqual(NavigationTargetKind.Group, target.Kind);
            Assert.AreEqual(expectedUniqueId, target.UniqueId);
        }

        [TestMethod]
        [DataRow("item/Colors", "Color")]
        [DataRow("item/Icons", "Iconography")]
        [DataRow("item/RichTextEdit", "RichTextBox")]
        public void ResolveNavigationTargetAcceptsOfficialWpfGalleryItemIds(string value, string expectedUniqueId)
        {
            var target = NavigationRootPage.ResolveNavigationTarget(value);

            Assert.IsNotNull(target);
            Assert.AreEqual(NavigationTargetKind.Item, target.Kind);
            Assert.AreEqual(expectedUniqueId, target.UniqueId);
        }

        [TestMethod]
        [DataRow("category/Media%20Controls")]
        [DataRow("category/Samples")]
        [DataRow("category/System")]
        [DataRow("item/File%20and%20Folder%20Dialogs")]
        [DataRow("item/User%20Dashboard")]
        [DataRow("item/MessageBox")]
        [DataRow("item/Clipboard")]
        [DataRow("item/Canvas")]
        [DataRow("item/Image")]
        public void ResolveNavigationTargetRejectsRetiredWpfGalleryRoutes(string value)
        {
            Assert.IsNull(NavigationRootPage.ResolveNavigationTarget(value));
        }

        [TestMethod]
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

        [TestMethod]
        [DataRow("Home", "Home")]
        [DataRow("All Controls", "AllControls")]
        [DataRow("item/All%20Controls", "AllControls")]
        public void ResolveNavigationTargetAcceptsOfficialWpfGalleryTopLevelIds(string value, string expectedKind)
        {
            var target = NavigationRootPage.ResolveNavigationTarget(value);

            Assert.IsNotNull(target);
            Assert.AreEqual(expectedKind, target.Kind.ToString());
        }

        [TestMethod]
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

        [TestMethod]
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
