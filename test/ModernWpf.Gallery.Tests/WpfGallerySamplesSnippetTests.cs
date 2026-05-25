using Microsoft.VisualStudio.TestTools.UnitTesting;
using static ModernWpf.Gallery.Tests.WpfGallerySnippetTestHelpers;

namespace ModernWpf.Gallery.Tests
{
    [TestClass]
    public class WpfGallerySamplesSnippetTests
    {
        [TestMethod]
        public void UserDashboardCodeBehindUsesOfficialCommandAndNotificationShape()
        {
            var pageSource = ReadRepoFile(
                "ModernWpf.Gallery",
                "Pages",
                "WpfGallery",
                "Samples",
                "UserDashboardPage.xaml.cs");

            StringAssert.Contains(pageSource, "var command = (sender as Button)?.Command;");
            StringAssert.Contains(pageSource, "var commandParameter = (sender as Button)?.CommandParameter;");
            StringAssert.Contains(pageSource, "var currentUserName = ViewModel.EditableUser?.Name ?? string.Empty;");
            StringAssert.Contains(pageSource, "RaiseNotification(sender as Button, $\"User {currentUserName} saved\", \"ButtonClickedActivity\");");
            StringAssert.Contains(pageSource, "RaiseNotification(sender as Button, $\"User {ViewModel.DeletedName} deleted\", \"ButtonClickedActivity\");");
            StringAssert.Contains(pageSource, "if (!(sender is Slider slider))");
            StringAssert.Contains(pageSource, "RaiseNotification(slider, $\"New age {newAge}\", \"SliderValueChangedActivity\");");
            Assert.IsFalse(
                pageSource.Contains("ExecuteButtonCommand"),
                "The copied User Dashboard page should keep the official WPF Gallery per-handler command execution shape.");
        }
    }
}
