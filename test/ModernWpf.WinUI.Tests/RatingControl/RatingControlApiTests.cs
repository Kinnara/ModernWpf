using System;
using System.Windows;
using System.Windows.Media.Imaging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModernWpf.Controls;
using ModernWpf.WinUI.TestApp;
using ModernWpf.WinUI.TestInfra;

namespace ModernWpf.WinUI.Tests.RatingControl;

[TestClass]
public class RatingControlApiTests
{
    private const string FontSizeForRenderingResourceKey = "RatingControlFontSizeForRendering";
    private const string ItemSpacingResourceKey = "RatingControlItemSpacing";

    [TestMethod]
    public void VerifyDefaultsAndBasicSetting()
    {
        WpfTestHost.Run(() =>
        {
            var ratingControl = new ModernWpf.Controls.RatingControl();
            Assert.IsNotNull(ratingControl);

            Assert.AreEqual(string.Empty, ratingControl.Caption);
            Assert.AreEqual(1, ratingControl.InitialSetValue);
            Assert.IsTrue(ratingControl.IsClearEnabled);
            Assert.IsFalse(ratingControl.IsReadOnly);
            Assert.AreEqual(5, ratingControl.MaxRating);
            Assert.AreEqual(-1.0, ratingControl.PlaceholderValue);
            Assert.AreEqual(-1.0, ratingControl.Value);

            ratingControl.Caption = "Rating API Test Caption";
            ratingControl.InitialSetValue = 2;
            ratingControl.IsClearEnabled = false;
            ratingControl.IsReadOnly = true;
            ratingControl.MaxRating = 10;
            ratingControl.PlaceholderValue = 3.0;
            ratingControl.Value = 2.0;

            var imageUri = new Uri("pack://application:,,,/ModernWpf.WinUI.Tests;component/Assets/rating_set.png", UriKind.Absolute);
            var imageInfo = new RatingItemImageInfo
            {
                Image = new BitmapImage(imageUri)
            };
            ratingControl.ItemInfo = imageInfo;

            WpfTestHost.DoEvents();

            Assert.AreEqual("Rating API Test Caption", ratingControl.Caption);
            Assert.AreEqual(2, ratingControl.InitialSetValue);
            Assert.IsFalse(ratingControl.IsClearEnabled);
            Assert.IsTrue(ratingControl.IsReadOnly);
            Assert.AreEqual(10, ratingControl.MaxRating);
            Assert.AreEqual(3.0, ratingControl.PlaceholderValue);
            Assert.AreEqual(2.0, ratingControl.Value);
            Assert.IsInstanceOfType(ratingControl.ItemInfo, typeof(RatingItemImageInfo));
            var image = ((RatingItemImageInfo)ratingControl.ItemInfo).Image as BitmapImage;
            Assert.IsNotNull(image);
            Assert.AreEqual(imageUri, image!.UriSource);
        });
    }

    [TestMethod]
    public void VerifyDontCrashWhenCollapsedAndValueSet()
    {
        WpfTestHost.Run(() =>
        {
            var ratingControl = new ModernWpf.Controls.RatingControl
            {
                Visibility = Visibility.Collapsed,
                Value = 3.3
            };

            Assert.AreEqual(3.3, ratingControl.Value);
        });
    }

    [TestMethod]
    public void VerifyValuesCoercion()
    {
        WpfTestHost.Run(() =>
        {
            var ratingControl = new ModernWpf.Controls.RatingControl();
            Assert.IsNotNull(ratingControl);
            Assert.AreEqual(-1.0, ratingControl.PlaceholderValue);
            Assert.AreEqual(-1.0, ratingControl.Value);

            ratingControl.PlaceholderValue = 0.1;
            ratingControl.Value = 0.1;
            Assert.AreEqual(1.0, ratingControl.PlaceholderValue, "Should coerce small PlaceholderValue values to 1.0");
            Assert.AreEqual(1.0, ratingControl.Value, "Should coerce small Value values to 1.0");

            ratingControl.PlaceholderValue = 6.0;
            ratingControl.Value = 6.0;
            Assert.AreEqual(5.0, ratingControl.PlaceholderValue, "Should coerce PlaceholderValue above MaxRating back to MaxRating");
            Assert.AreEqual(5.0, ratingControl.Value, "Should coerce Value above MaxRating back to MaxRating");

            ratingControl.MaxRating = -2;
            Assert.AreEqual(1, ratingControl.MaxRating, "Should coerce MaxRating below 1 back up to 1.");
            Assert.AreEqual(1.0, ratingControl.PlaceholderValue, "Should auto-coerce now outdated PlaceholderValue above MaxRating back to MaxRating [2]");
            Assert.AreEqual(1.0, ratingControl.Value, "Should auto-coerce now outdated Value above MaxRating back to MaxRating [2]");

            ratingControl.PlaceholderValue = 6.0;
            ratingControl.Value = 6.0;
            Assert.AreEqual(1.0, ratingControl.PlaceholderValue, "Should coerce set PlaceholderValue above MaxRating back to MaxRating");
            Assert.AreEqual(1.0, ratingControl.Value, "Should coerce set Value above MaxRating back to MaxRating");
        });
    }

    [TestMethod]
    public void VerifySizeIsChangeableFromResource()
    {
        WpfTestHost.Run(() =>
        {
            var appResources = TestApplication.EnsureInitialized().Resources;
            var hadFontSizeOverride = appResources.Contains(FontSizeForRenderingResourceKey);
            var hadItemSpacingOverride = appResources.Contains(ItemSpacingResourceKey);
            var originalFontSizeOverride = hadFontSizeOverride ? appResources[FontSizeForRenderingResourceKey] : null;
            var originalItemSpacingOverride = hadItemSpacingOverride ? appResources[ItemSpacingResourceKey] : null;

            try
            {
                appResources.Remove(FontSizeForRenderingResourceKey);
                appResources.Remove(ItemSpacingResourceKey);
                var originalWidth = MeasureRatingWidth();

                appResources[FontSizeForRenderingResourceKey] = 20.0;
                var smallerFontWidth = MeasureRatingWidth();
                Assert.IsTrue(
                    smallerFontWidth < originalWidth,
                    $"Expected a smaller font rendering resource to reduce width. Original={originalWidth}, new={smallerFontWidth}");

                appResources[ItemSpacingResourceKey] = 20.0;
                var widerSpacingWidth = MeasureRatingWidth();
                Assert.IsTrue(
                    widerSpacingWidth > smallerFontWidth,
                    $"Expected a larger item spacing resource to increase width. Previous={smallerFontWidth}, new={widerSpacingWidth}");

                appResources[FontSizeForRenderingResourceKey] = 48.0;
                appResources.Remove(ItemSpacingResourceKey);
                var largerFontWidth = MeasureRatingWidth();
                Assert.IsTrue(
                    largerFontWidth > originalWidth,
                    $"Expected a larger font rendering resource to exceed default width. Original={originalWidth}, new={largerFontWidth}");
                Assert.IsTrue(
                    largerFontWidth > widerSpacingWidth,
                    $"Expected the larger font rendering resource to exceed the spacing-only width. Previous={widerSpacingWidth}, new={largerFontWidth}");
            }
            finally
            {
                RestoreResource(appResources, FontSizeForRenderingResourceKey, hadFontSizeOverride, originalFontSizeOverride);
                RestoreResource(appResources, ItemSpacingResourceKey, hadItemSpacingOverride, originalItemSpacingOverride);
            }
        });
    }

    private static double MeasureRatingWidth()
    {
        var ratingControl = new ModernWpf.Controls.RatingControl();

        using var host = new TestWindowHost(ratingControl, width: 420, height: 180);
        host.UpdateLayout();
        return ratingControl.ActualWidth;
    }

    private static void RestoreResource(ResourceDictionary resources, string key, bool hadOriginalValue, object? originalValue)
    {
        if (hadOriginalValue)
        {
            resources[key] = originalValue;
        }
        else
        {
            resources.Remove(key);
        }
    }
}
