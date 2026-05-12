using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModernWpf.Gallery.Models;
using ModernWpf.Gallery.Shell;
using ModernWpf.Gallery.Testing;

namespace ModernWpf.Gallery.Tests
{
    [TestClass]
    public class GalleryNavigationRuntimeTests
    {
        [TestMethod]
        public void ShellCanNavigateEveryCatalogRouteWithoutExceptions()
        {
            WpfTestHost.Run(() =>
            {
                GalleryDiagnostics.Configure(GalleryLaunchOptions.Parse(new[] { "--visual-test" }));
                var failures = new List<string>();
                var page = new NavigationRootPage();
                var window = new Window
                {
                    Width = 1180,
                    Height = 820,
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

                    foreach (var route in CatalogRoutes())
                    {
                        try
                        {
                            page.NavigateTo(route);
                            WpfTestHost.DoEvents();
                            window.UpdateLayout();
                            WpfTestHost.DoEvents();

                            if (!string.IsNullOrWhiteSpace(GalleryDiagnostics.LastException))
                            {
                                failures.Add(route + ": " + GalleryDiagnostics.LastException);
                            }
                        }
                        catch (Exception ex)
                        {
                            failures.Add(route + ": " + ex.GetType().Name + ": " + ex.Message);
                        }
                    }
                }
                finally
                {
                    window.Content = null;
                    window.Close();
                    WpfTestHost.DoEvents();
                    GalleryDiagnostics.ResetForTests();
                }

                Assert.AreEqual(0, failures.Count, string.Join(Environment.NewLine, failures.Take(20)));
            });
        }

        private static IEnumerable<string> CatalogRoutes()
        {
            yield return "home";
            yield return "AllControls";

            foreach (var group in GalleryCatalog.Groups)
            {
                yield return "category/" + group.UniqueId;
            }

            foreach (var item in GalleryCatalog.Items)
            {
                yield return "item/" + item.UniqueId;
            }
        }
    }
}
