using System.Collections.Generic;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModernWpf.Gallery.Models;
using ModernWpf.Gallery.Pages;
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
                    Assert.IsNotNull(FindByAutomationId(page, rootAutomationId), rootAutomationId + " is missing.");
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
    }
}
