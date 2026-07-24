using System.Windows;
using System.Windows.Media;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModernWpf.WinUI.TestApp;
using ModernWpf.WinUI.TestInfra;

namespace ModernWpf.WinUI.Tests.CommonStyles;

[TestClass]
public class ThemeResourcesTests
{
    [TestMethod]
    public void VerifyOverrides()
    {
        WpfTestHost.Run(() =>
        {
            var appResources = TestApplication.EnsureInitialized().Resources;
            appResources["RatingControlCaptionForeground"] = new SolidColorBrush(Colors.Orange);

            try
            {
                var ratingControl = new ModernWpf.Controls.RatingControl { Value = 2 };

                using var host = new TestWindowHost(ratingControl, width: 360, height: 180);
                host.UpdateLayout();

                var foreground = ratingControl.Foreground as SolidColorBrush;
                Assert.IsNotNull(foreground, "RatingControl foreground should resolve to the overridden brush.");
                Assert.AreEqual(
                    Colors.Orange,
                    foreground!.Color,
                    "RatingControlCaptionForeground override in Application.Resources should be picked up by RatingControl.");
            }
            finally
            {
                appResources.Remove("RatingControlCaptionForeground");
            }
        });
    }
}
