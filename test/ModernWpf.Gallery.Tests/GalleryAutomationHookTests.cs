using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModernWpf.Gallery.Models;
using ModernWpf.Gallery.Pages;
using ModernWpf.Gallery.Testing;
using TeachingTipControl = ModernWpf.Controls.TeachingTip;

namespace ModernWpf.Gallery.Tests
{
    [TestClass]
    public class GalleryAutomationHookTests
    {
        public static IEnumerable<object[]> CuratedSampleAutomationIds()
        {
            yield return new object[] { "TeachingTip", "GallerySample_TeachingTip_Root", "GallerySample_TeachingTip_ShowButton" };
            yield return new object[] { "Button", "GallerySample_Button_Root", "GallerySample_Button_PrimaryButton" };
            yield return new object[] { "ComboBox", "GallerySample_ComboBox_Root", "GallerySample_ComboBox_ComboBox" };
            yield return new object[] { "InfoBar", "GallerySample_InfoBar_Root", "GallerySample_InfoBar_ShowButton" };
            yield return new object[] { "InfoBar", "GallerySample_InfoBar_Root", "GallerySample_InfoBar_InfoBar" };
            yield return new object[] { "NavigationView", "GallerySample_NavigationView_Root", "GallerySample_NavigationView_NavigationView" };
            yield return new object[] { "ContentDialog", "GallerySample_ContentDialog_Root", "GallerySample_ContentDialog_ShowButton" };
        }

        [TestMethod]
        [DynamicData(nameof(CuratedSampleAutomationIds), DynamicDataSourceType.Method)]
        public void CuratedSamplesExposeStableAutomationIds(string uniqueId, string rootAutomationId, string primaryAutomationId)
        {
            WpfTestHost.Run(() =>
            {
                var page = new ItemPage(GalleryCatalog.FindItem(uniqueId));
                var window = new Window
                {
                    Width = 1024,
                    Height = 768,
                    Left = -32000,
                    Top = -32000,
                    ShowInTaskbar = false,
                    WindowStartupLocation = WindowStartupLocation.Manual,
                    Content = page
                };

                try
                {
                    window.Show();
                    WpfTestHost.DoEvents();
                    window.UpdateLayout();
                    WpfTestHost.DoEvents();

                    Assert.IsNotNull(FindByAutomationId(page, "GalleryItemPageTitle"), "Item page title AutomationId is missing.");
                    Assert.IsNotNull(FindByAutomationId(page, "GallerySampleHost"), "Sample host AutomationId is missing.");
                    var sampleRoot = FindByAutomationId(page, rootAutomationId) as UIElement;
                    Assert.IsNotNull(sampleRoot, rootAutomationId + " is missing.");
                    var sampleRootPeer = UIElementAutomationPeer.CreatePeerForElement(sampleRoot);
                    Assert.IsNotNull(sampleRootPeer, rootAutomationId + " has no automation peer.");
                    Assert.IsTrue(sampleRootPeer.IsControlElement(), rootAutomationId + " is not exposed in the UI Automation control view.");
                    Assert.AreEqual(AutomationControlType.Group, sampleRootPeer.GetAutomationControlType());
                    Assert.IsNotNull(FindByAutomationId(page, primaryAutomationId), primaryAutomationId + " is missing.");
                }
                finally
                {
                    window.Content = null;
                    window.Close();
                    WpfTestHost.DoEvents();
                }
            });
        }

        [TestMethod]
        public void TeachingTipSampleButtonOpensTip()
        {
            WpfTestHost.Run(() =>
            {
                var page = new ItemPage(GalleryCatalog.FindItem("TeachingTip"));
                var window = new Window
                {
                    Width = 1024,
                    Height = 768,
                    Left = -32000,
                    Top = -32000,
                    ShowInTaskbar = false,
                    WindowStartupLocation = WindowStartupLocation.Manual,
                    Content = page
                };

                try
                {
                    window.Show();
                    WpfTestHost.DoEvents();
                    window.UpdateLayout();
                    WpfTestHost.DoEvents();

                    var button = (ButtonBase)FindByAutomationId(page, "GallerySample_TeachingTip_ShowButton");
                    var teachingTip = (TeachingTipControl)FindByAutomationId(page, "GallerySample_TeachingTip_TeachingTip");

                    Assert.IsNotNull(button);
                    Assert.IsNotNull(teachingTip);
                    Assert.AreEqual(48.0, teachingTip.TryFindResource("TeachingTipMinWidth"));

                    button.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
                    WpfTestHost.DoEvents();

                    Assert.IsTrue(teachingTip.IsOpen);
                    var popupRoot = teachingTip.Template.FindName("TailOcclusionGrid", teachingTip) as FrameworkElement;
                    Assert.IsNotNull(popupRoot);
                    Assert.AreEqual(48.0, popupRoot.MinWidth);
                }
                finally
                {
                    window.Content = null;
                    window.Close();
                    WpfTestHost.DoEvents();
                }
            });
        }

        [TestMethod]
        public void InfoBarSampleUsesVisibleOpenInfoBarTemplate()
        {
            WpfTestHost.Run(() =>
            {
                var page = new ItemPage(GalleryCatalog.FindItem("InfoBar"));
                var window = new Window
                {
                    Width = 1024,
                    Height = 768,
                    Left = -32000,
                    Top = -32000,
                    ShowInTaskbar = false,
                    WindowStartupLocation = WindowStartupLocation.Manual,
                    Content = page
                };

                try
                {
                    window.Show();
                    WpfTestHost.DoEvents();
                    window.UpdateLayout();
                    WpfTestHost.DoEvents();

                    var infoBar = (ModernWpf.Controls.InfoBar)FindByAutomationId(page, "GallerySample_InfoBar_InfoBar");
                    Assert.IsNotNull(infoBar);
                    Assert.IsTrue(infoBar.IsOpen);

                    var contentRoot = FindNamedDescendant<Border>(infoBar, "ContentRoot");
                    Assert.IsNotNull(contentRoot);
                    Assert.AreEqual(Visibility.Visible, contentRoot.Visibility);
                    Assert.IsNotNull(FindNamedDescendant<TextBlock>(infoBar, "Title"));
                    Assert.IsNotNull(FindNamedDescendant<TextBlock>(infoBar, "Message"));
                }
                finally
                {
                    window.Content = null;
                    window.Close();
                    WpfTestHost.DoEvents();
                }
            });
        }

        [TestMethod]
        public void InfoBarSampleWritesRenderedVisualArtifacts()
        {
            WpfTestHost.Run(() =>
            {
                var artifactDirectory = Path.Combine(Path.GetTempPath(), "ModernWpfGalleryTests", Path.GetRandomFileName());
                GalleryDiagnostics.Configure(GalleryLaunchOptions.Parse(new[] { "--visual-test", "--visual-artifact-dir", artifactDirectory }));

                var page = new ItemPage(GalleryCatalog.FindItem("InfoBar"));
                var window = new Window
                {
                    Width = 1024,
                    Height = 768,
                    Left = -32000,
                    Top = -32000,
                    ShowInTaskbar = false,
                    WindowStartupLocation = WindowStartupLocation.Manual,
                    Content = page
                };

                try
                {
                    window.Show();
                    WpfTestHost.DoEvents();
                    window.UpdateLayout();
                    WpfTestHost.DoEvents();

                    GalleryDiagnostics.WriteVisualArtifacts(page);

                    var infoBarArtifact = Path.Combine(artifactDirectory, "GallerySample_InfoBar_InfoBar.png");
                    var rootArtifact = Path.Combine(artifactDirectory, "GallerySample_InfoBar_Root.png");
                    Assert.IsTrue(File.Exists(infoBarArtifact), infoBarArtifact + " was not written.");
                    Assert.IsTrue(File.Exists(rootArtifact), rootArtifact + " was not written.");
                    Assert.IsTrue(new FileInfo(infoBarArtifact).Length > 0);
                    Assert.IsTrue(new FileInfo(rootArtifact).Length > 0);
                    Assert.IsTrue(HasVisibleRgbPixels(infoBarArtifact), infoBarArtifact + " has no visible RGB content.");
                    Assert.IsTrue(HasVisibleRgbPixels(rootArtifact), rootArtifact + " has no visible RGB content.");
                }
                finally
                {
                    window.Content = null;
                    window.Close();
                    GalleryDiagnostics.ResetForTests();
                    if (Directory.Exists(artifactDirectory))
                    {
                        Directory.Delete(artifactDirectory, recursive: true);
                    }
                    WpfTestHost.DoEvents();
                }
            });
        }

        private static DependencyObject FindByAutomationId(DependencyObject root, string automationId)
        {
            if (root == null)
            {
                return null;
            }

            var element = root as UIElement;
            if (element != null && AutomationProperties.GetAutomationId(element) == automationId)
            {
                return root;
            }

            var childCount = VisualTreeHelper.GetChildrenCount(root);
            for (var i = 0; i < childCount; i++)
            {
                var result = FindByAutomationId(VisualTreeHelper.GetChild(root, i), automationId);
                if (result != null)
                {
                    return result;
                }
            }

            return null;
        }

        private static bool HasVisibleRgbPixels(string path)
        {
            using (var stream = File.OpenRead(path))
            {
                var decoder = new PngBitmapDecoder(stream, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.OnLoad);
                var frame = decoder.Frames[0];
                BitmapSource bitmap = frame.Format == PixelFormats.Bgra32
                    ? frame
                    : new FormatConvertedBitmap(frame, PixelFormats.Bgra32, null, 0);
                var stride = bitmap.PixelWidth * 4;
                var pixels = new byte[stride * bitmap.PixelHeight];
                bitmap.CopyPixels(pixels, stride, 0);

                for (var i = 0; i < pixels.Length; i += 4)
                {
                    var blue = pixels[i];
                    var green = pixels[i + 1];
                    var red = pixels[i + 2];
                    var alpha = pixels[i + 3];
                    if (alpha > 16 && red + green + blue > 36)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private static T FindNamedDescendant<T>(DependencyObject root, string name)
            where T : FrameworkElement
        {
            if (root == null)
            {
                return null;
            }

            var element = root as T;
            if (element != null && element.Name == name)
            {
                return element;
            }

            var childCount = VisualTreeHelper.GetChildrenCount(root);
            for (var i = 0; i < childCount; i++)
            {
                var result = FindNamedDescendant<T>(VisualTreeHelper.GetChild(root, i), name);
                if (result != null)
                {
                    return result;
                }
            }

            return null;
        }
    }
}
