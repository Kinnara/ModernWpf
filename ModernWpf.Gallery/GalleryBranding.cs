using System;
using System.Reflection;

namespace ModernWpf.Gallery
{
    public static class GalleryBranding
    {
        private const string GallerySuffix = " Gallery";

        public const string RepositoryUrl = "https://github.com/Kinnara/ModernWpf";

        public const string QuickStartUrl = RepositoryUrl + "#quick-start";

        public const string ControlsReferenceUrl = RepositoryUrl + "/wiki/Controls";

        public const string IssuesUrl = RepositoryUrl + "/issues";

        public const string NewIssueUrl = RepositoryUrl + "/issues/new";

        public const string LicenseUrl = RepositoryUrl + "/blob/master/LICENSE";

        public const string NuGetPackageUrl = "https://www.nuget.org/packages/ModernWpfUI/";

        public const string BehaviorsPackageUrl = "https://www.nuget.org/packages/Microsoft.Xaml.Behaviors.Wpf/";

        public static string DisplayName { get; } = GetAssemblyTitle();

        public static string BrandName { get; } = GetBrandName(DisplayName);

        public static string PreviewDisplayName { get; } = DisplayName + " Preview";

        public static string Version { get; } = GetAssemblyVersion();

        public static string VersionDisplay { get; } = "Version " + Version;

        public static string CopyrightNotice { get; } = GetAssemblyCopyright() + " · MIT License";

        public static string CloneCommand { get; } = "git clone " + RepositoryUrl + ".git";

        public static string ControlsGroupTitle { get; } = BrandName + " controls";

        public static string WhatsNewPageTitle { get; } = "What's New in " + BrandName;

        public static string WhatsNewTitle { get; } = "What's new in " + BrandName;

        public static string WhatsNewDescription { get; } =
            "See the current " + BrandName + " direction, supported targets, and gallery improvements.";

        public static string NewSamplesAutomationName { get; } =
            "New and updated " + BrandName + " samples";

        private static string GetAssemblyTitle()
        {
            var title = typeof(GalleryBranding).Assembly.GetCustomAttribute<AssemblyTitleAttribute>()?.Title;
            return string.IsNullOrWhiteSpace(title)
                ? typeof(GalleryBranding).Assembly.GetName().Name
                : title;
        }

        private static string GetAssemblyVersion()
        {
            var assembly = typeof(GalleryBranding).Assembly;
            var version = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
            if (string.IsNullOrWhiteSpace(version))
            {
                version = assembly.GetName().Version?.ToString() ?? "unknown";
            }

            var metadataSeparator = version.IndexOf('+');
            return metadataSeparator < 0 ? version : version.Substring(0, metadataSeparator);
        }

        private static string GetAssemblyCopyright()
        {
            var copyright = typeof(GalleryBranding).Assembly
                .GetCustomAttribute<AssemblyCopyrightAttribute>()?.Copyright;
            return string.IsNullOrWhiteSpace(copyright)
                ? "Copyright © 2019–2026 Yimeng Wu"
                : copyright;
        }

        private static string GetBrandName(string displayName)
        {
            return displayName.EndsWith(GallerySuffix, StringComparison.Ordinal)
                ? displayName.Substring(0, displayName.Length - GallerySuffix.Length)
                : displayName;
        }
    }
}
