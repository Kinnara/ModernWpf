using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModernWpf.Gallery.Models;

namespace ModernWpf.Gallery.Tests
{
    [TestClass]
    public class GalleryCatalogTests
    {
        [TestMethod]
        public void GroupsMatchWinUIGalleryOrder()
        {
            var expected = new[]
            {
                "Fundamentals",
                "Design",
                "Accessibility",
                "Menus & toolbars",
                "Collections",
                "Date & time",
                "Basic input",
                "Status & info",
                "Dialogs & flyouts",
                "Scrolling",
                "Layout",
                "Navigation",
                "Media",
                "Styles",
                "Text",
                "Motion",
                "Windowing",
                "System",
                "Shell"
            };

            var actual = GalleryCatalog.Groups.Select(group => group.Title).ToArray();

            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void SearchForAFindsEveryGroup()
        {
            var resultGroupIds = GalleryCatalog.Search("a")
                .Select(item => item.GroupId)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();

            foreach (var group in GalleryCatalog.Groups)
            {
                Assert.IsTrue(
                    resultGroupIds.Contains(group.UniqueId, StringComparer.OrdinalIgnoreCase),
                    "Expected query 'a' to return at least one result from group '{0}'.",
                    group.Title);
            }
        }

        [TestMethod]
        public void CatalogItemsHaveUniqueIds()
        {
            var duplicates = GalleryCatalog.Items
                .GroupBy(item => item.UniqueId, StringComparer.OrdinalIgnoreCase)
                .Where(group => group.Count() > 1)
                .Select(group => group.Key)
                .ToArray();

            CollectionAssert.AreEqual(Array.Empty<string>(), duplicates);
        }

        [TestMethod]
        public void CatalogRelationshipsResolve()
        {
            foreach (var item in GalleryCatalog.Items)
            {
                Assert.IsNotNull(GalleryCatalog.FindGroup(item.GroupId), "Missing group '{0}' for '{1}'.", item.GroupId, item.UniqueId);

                foreach (var relatedControlId in item.RelatedControlIds)
                {
                    Assert.IsNotNull(
                        GalleryCatalog.FindItem(relatedControlId),
                        "Missing related item '{0}' referenced by '{1}'.",
                        relatedControlId,
                        item.UniqueId);
                }
            }
        }

        [TestMethod]
        public void CatalogContainsPortedWinUIGallerySurface()
        {
            Assert.AreEqual(19, GalleryCatalog.Groups.Count);
            Assert.AreEqual(121, GalleryCatalog.Items.Count);
        }
    }
}
