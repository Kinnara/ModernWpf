using System;
using System.Collections.Generic;
using System.Linq;

namespace ModernWpf.Gallery.Models
{
    internal static class GalleryCatalog
    {
        public static IReadOnlyList<GalleryGroup> Groups
        {
            get { return GalleryCatalogData.Groups; }
        }

        public static IReadOnlyList<GalleryItem> Items
        {
            get { return GalleryCatalogData.Items; }
        }

        public static IReadOnlyList<GalleryItem> NewOrUpdatedItems
        {
            get { return Items.Where(item => item.IsNew || item.IsUpdated).Take(16).ToArray(); }
        }

        public static GalleryGroup FindGroup(string uniqueId)
        {
            return Groups.FirstOrDefault(group => string.Equals(group.UniqueId, uniqueId, StringComparison.OrdinalIgnoreCase));
        }

        public static GalleryItem FindItem(string uniqueId)
        {
            return Items.FirstOrDefault(item => string.Equals(item.UniqueId, uniqueId, StringComparison.OrdinalIgnoreCase));
        }

        public static IReadOnlyList<GalleryItem> Search(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return Items;
            }

            var tokens = query.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            return Items
                .Where(item => tokens.All(token => item.Matches(token)))
                .OrderByDescending(item => item.Title.StartsWith(query, StringComparison.OrdinalIgnoreCase))
                .ThenBy(item => item.Title)
                .ToArray();
        }
    }

    public sealed class GalleryGroup
    {
        public GalleryGroup(string uniqueId, string title, string subtitle, string imagePath, bool isSpecialSection, IReadOnlyList<GalleryItem> items)
        {
            UniqueId = uniqueId;
            Title = title;
            Subtitle = subtitle;
            ImagePath = GalleryAssetUri.Normalize(imagePath);
            IsSpecialSection = isSpecialSection;
            Items = items ?? Array.Empty<GalleryItem>();
        }

        public string UniqueId { get; }
        public string Title { get; }
        public string Subtitle { get; }
        public string ImagePath { get; }
        public bool IsSpecialSection { get; }
        public IReadOnlyList<GalleryItem> Items { get; }

        public override string ToString()
        {
            return Title;
        }
    }

    public sealed class GalleryItem
    {
        public GalleryItem(
            string groupId,
            string uniqueId,
            string title,
            string subtitle,
            string imagePath,
            string description,
            string apiNamespace,
            bool isNew,
            bool isUpdated,
            IReadOnlyList<string> baseClasses,
            IReadOnlyList<GalleryDocLink> docs,
            IReadOnlyList<string> relatedControlIds)
        {
            GroupId = groupId;
            UniqueId = uniqueId;
            Title = title;
            Subtitle = subtitle;
            ImagePath = GalleryAssetUri.Normalize(imagePath);
            Description = description;
            ApiNamespace = apiNamespace;
            IsNew = isNew;
            IsUpdated = isUpdated;
            BaseClasses = baseClasses ?? Array.Empty<string>();
            Docs = docs ?? Array.Empty<GalleryDocLink>();
            RelatedControlIds = relatedControlIds ?? Array.Empty<string>();
        }

        public string GroupId { get; }
        public string UniqueId { get; }
        public string Title { get; }
        public string Subtitle { get; }
        public string ImagePath { get; }
        public string Description { get; }
        public string ApiNamespace { get; }
        public bool IsNew { get; }
        public bool IsUpdated { get; }
        public IReadOnlyList<string> BaseClasses { get; }
        public IReadOnlyList<GalleryDocLink> Docs { get; }
        public IReadOnlyList<string> RelatedControlIds { get; }

        public string Badge
        {
            get
            {
                if (IsNew)
                {
                    return "New";
                }

                return IsUpdated ? "Updated" : string.Empty;
            }
        }

        public bool HasBadge
        {
            get { return !string.IsNullOrEmpty(Badge); }
        }

        public string BaseClassText
        {
            get { return BaseClasses.Count == 0 ? string.Empty : string.Join(" > ", BaseClasses); }
        }

        public string GroupTitle
        {
            get
            {
                var group = GalleryCatalog.FindGroup(GroupId);
                return group == null ? string.Empty : group.Title;
            }
        }

        public bool Matches(string token)
        {
            return Contains(Title, token) ||
                Contains(Subtitle, token) ||
                Contains(Description, token) ||
                Contains(UniqueId, token) ||
                Contains(ApiNamespace, token);
        }

        public override string ToString()
        {
            return Title;
        }

        private static bool Contains(string value, string token)
        {
            return !string.IsNullOrEmpty(value) && value.IndexOf(token, StringComparison.OrdinalIgnoreCase) >= 0;
        }
    }

    internal static class GalleryAssetUri
    {
        private const string ApplicationPackPrefix = "pack://application:,,,/";
        private const string GalleryPackPrefix = "pack://application:,,,/ModernWpf.Gallery;component/";
        private const string MsAppxPrefix = "ms-appx:///";

        public static string Normalize(string uri)
        {
            if (string.IsNullOrWhiteSpace(uri))
            {
                return uri;
            }

            if (uri.StartsWith(GalleryPackPrefix, StringComparison.OrdinalIgnoreCase))
            {
                return uri;
            }

            if (uri.StartsWith(MsAppxPrefix, StringComparison.OrdinalIgnoreCase))
            {
                return GalleryPackPrefix + uri.Substring(MsAppxPrefix.Length);
            }

            if (uri.StartsWith(ApplicationPackPrefix, StringComparison.OrdinalIgnoreCase))
            {
                var path = uri.Substring(ApplicationPackPrefix.Length);
                if (path.IndexOf(";component/", StringComparison.OrdinalIgnoreCase) < 0)
                {
                    return GalleryPackPrefix + path;
                }
            }

            return uri;
        }
    }

    public sealed class GalleryDocLink
    {
        public GalleryDocLink(string title, string uri)
        {
            Title = title;
            Uri = uri;
        }

        public string Title { get; }
        public string Uri { get; }

        public override string ToString()
        {
            return Title;
        }
    }

    public sealed class SampleSnippet
    {
        public SampleSnippet(string title, string text)
        {
            Title = title;
            Text = text;
        }

        public string Title { get; }
        public string Text { get; }
    }
}
