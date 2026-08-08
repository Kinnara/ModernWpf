using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using static ModernWpf.Gallery.Tests.WpfGallerySnippetTestHelpers;

namespace ModernWpf.Gallery.Tests
{
    [TestClass]
    public class WpfGallerySourceShapeTests
    {
        [TestMethod]
        public void ActiveMappedXamlKeepsOfficialAutomationNameAndHookTokensFromLocalSource()
        {
            var repoRoot = GetRepoRoot();
            var officialViewsRoot = @"D:\repos\WPF-Samples\Sample Applications\WPFGallery\Views";
            if (!Directory.Exists(officialViewsRoot))
            {
                Assert.Inconclusive("Local official WPF Gallery source was not found at " + officialViewsRoot + ".");
            }

            var localPagesRoot = Path.Combine(repoRoot, "ModernWpf.Gallery", "Pages");
            var localXamlByFileName = Directory
                .EnumerateFiles(localPagesRoot, "*.xaml", SearchOption.AllDirectories)
                .ToDictionary(Path.GetFileName, path => path, StringComparer.OrdinalIgnoreCase);
            var missingTokens = new List<string>();
            var mappedFileCount = 0;

            foreach (var officialPath in Directory.EnumerateFiles(officialViewsRoot, "*.xaml", SearchOption.AllDirectories))
            {
                var officialFileName = Path.GetFileName(officialPath);
                if (!localXamlByFileName.TryGetValue(officialFileName, out var localPath))
                {
                    continue;
                }

                mappedFileCount++;
                var localSource = NormalizeXamlTokenSource(File.ReadAllText(localPath));
                var officialSource = File.ReadAllText(officialPath);
                var officialRelativePath = Path.GetRelativePath(officialViewsRoot, officialPath);
                var officialTokens = Regex
                    .Matches(
                        officialSource,
                        "AutomationProperties\\.Name\\s*=\\s*\"[^\"]+\"|x:Name\\s*=\\s*\"[^\"]+\"|Name\\s*=\\s*\"[^\"]+\"|\\b(?:Click|Checked|Unchecked|SelectionChanged|TextChanged|RequestNavigate|Loaded|Unloaded|SizeChanged|ValueChanged|Navigated|Selected|MouseLeftButtonDown|PreviewKeyDown|KeyDown)\\s*=\\s*\"[^\"]+\"|Command\\s*=\\s*\"[^\"]+\"")
                    .Cast<Match>()
                    .Select(match => NormalizeXamlTokenSource(match.Value))
                    .Distinct(StringComparer.Ordinal)
                    .OrderBy(token => token, StringComparer.Ordinal);

                foreach (var token in officialTokens)
                {
                    if (!localSource.Contains(token, StringComparison.Ordinal) &&
                        !IsOfficialXamlHookTokenAdaptedAway(officialRelativePath, token))
                    {
                        missingTokens.Add(officialRelativePath + " :: " + token);
                    }
                }
            }

            Assert.AreEqual(54, mappedFileCount, "The active WPF Gallery XAML mapping count changed; update the 5.3 scan deliberately.");
            Assert.AreEqual(
                0,
                missingTokens.Count,
                "Missing official 5.3 XAML tokens from local official source:\n" + string.Join("\n", missingTokens));
        }

        [TestMethod]
        public void ActiveMappedXamlKeepsOfficialSamplePaneCodeTokensFromLocalSource()
        {
            var repoRoot = GetRepoRoot();
            var officialViewsRoot = @"D:\repos\WPF-Samples\Sample Applications\WPFGallery\Views";
            if (!Directory.Exists(officialViewsRoot))
            {
                Assert.Inconclusive("Local official WPF Gallery source was not found at " + officialViewsRoot + ".");
            }

            var localPagesRoot = Path.Combine(repoRoot, "ModernWpf.Gallery", "Pages");
            var localXamlByFileName = Directory
                .EnumerateFiles(localPagesRoot, "*.xaml", SearchOption.AllDirectories)
                .ToDictionary(Path.GetFileName, path => path, StringComparer.OrdinalIgnoreCase);
            var missingTokens = new List<string>();
            var mappedFileCount = 0;

            foreach (var officialPath in Directory.EnumerateFiles(officialViewsRoot, "*.xaml", SearchOption.AllDirectories))
            {
                var officialFileName = Path.GetFileName(officialPath);
                if (!localXamlByFileName.TryGetValue(officialFileName, out var localPath))
                {
                    continue;
                }

                mappedFileCount++;
                var localSource = NormalizeXamlTokenSource(File.ReadAllText(localPath));
                var officialSource = File.ReadAllText(officialPath);
                var officialRelativePath = Path.GetRelativePath(officialViewsRoot, officialPath);
                var officialTokens = Regex
                    .Matches(
                        officialSource,
                        "\\b(?:HeaderText|XamlCode|CSharpCode)\\s*=\\s*\"[^\"]+\"")
                    .Cast<Match>()
                    .Select(match => NormalizeXamlTokenSource(match.Value))
                    .Distinct(StringComparer.Ordinal)
                    .OrderBy(token => token, StringComparer.Ordinal);

                foreach (var token in officialTokens)
                {
                    if (!localSource.Contains(token, StringComparison.Ordinal) &&
                        !IsOfficialSamplePaneTokenAdaptedAway(officialRelativePath, token))
                    {
                        missingTokens.Add(officialRelativePath + " :: " + token);
                    }
                }
            }

            Assert.AreEqual(54, mappedFileCount, "The active WPF Gallery XAML mapping count changed; update the 5.1 sample-pane scan deliberately.");
            Assert.AreEqual(
                0,
                missingTokens.Count,
                "Missing official 5.1 sample-pane tokens from local official source:\n" + string.Join("\n", missingTokens));
        }

        [TestMethod]
        public void MappedWpfGalleryViewModelsKeepOfficialObservableFieldNamesFromLocalSource()
        {
            var repoRoot = GetRepoRoot();
            var officialViewModelsRoot = @"D:\repos\WPF-Samples\Sample Applications\WPFGallery\ViewModels";
            if (!Directory.Exists(officialViewModelsRoot))
            {
                Assert.Inconclusive("Local official WPF Gallery source was not found at " + officialViewModelsRoot + ".");
            }

            var localSources = Directory
                .EnumerateFiles(Path.Combine(repoRoot, "ModernWpf.Gallery"), "*.cs", SearchOption.AllDirectories)
                .Where(path => !path.Contains(Path.DirectorySeparatorChar + "bin" + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
                .Where(path => !path.Contains(Path.DirectorySeparatorChar + "obj" + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
                .Select(path => new
                {
                    Path = path,
                    Source = File.ReadAllText(path)
                })
                .ToList();
            var missingFields = new List<string>();
            var mappedClassCount = 0;
            var officialFieldCount = 0;

            foreach (var officialPath in Directory.EnumerateFiles(officialViewModelsRoot, "*.cs", SearchOption.AllDirectories))
            {
                var officialSource = File.ReadAllText(officialPath);
                var classMatch = Regex.Match(officialSource, "public\\s+partial\\s+class\\s+(\\w+)");
                if (!classMatch.Success)
                {
                    continue;
                }

                var className = classMatch.Groups[1].Value;
                var localSource = localSources.FirstOrDefault(file =>
                    Regex.IsMatch(file.Source, "\\bclass\\s+" + Regex.Escape(className) + "\\b"));
                if (localSource == null)
                {
                    missingFields.Add(Path.GetRelativePath(officialViewModelsRoot, officialPath) + " :: missing local class " + className);
                    continue;
                }

                mappedClassCount++;
                var officialRelativePath = Path.GetRelativePath(officialViewModelsRoot, officialPath);
                var officialFields = Regex
                    .Matches(
                        officialSource,
                        "\\[ObservableProperty\\]\\s*(?:\\r?\\n\\s*)private\\s+[^;]*\\s+(_\\w+)\\b[^;]*;")
                    .Cast<Match>()
                    .Select(match => match.Groups[1].Value)
                    .Where(fieldName => !IsOfficialObservableFieldAdaptedAway(className, fieldName))
                    .Distinct(StringComparer.Ordinal)
                    .OrderBy(fieldName => fieldName, StringComparer.Ordinal);

                foreach (var fieldName in officialFields)
                {
                    officialFieldCount++;
                    if (!localSource.Source.Contains(fieldName, StringComparison.Ordinal))
                    {
                        missingFields.Add(officialRelativePath + " :: " + className + "." + fieldName);
                    }
                }
            }

            Assert.AreEqual(57, mappedClassCount, "The active WPF Gallery ViewModel mapping count changed; update the 5.4 observable-field scan deliberately.");
            Assert.AreEqual(76, officialFieldCount, "The active WPF Gallery observable field count changed; update the 5.4 observable-field scan deliberately.");
            Assert.AreEqual(
                0,
                missingFields.Count,
                "Missing official 5.4 observable backing-field names from local official source:\n" + string.Join("\n", missingFields));
        }

        [TestMethod]
        public void MappedWpfGalleryViewModelsKeepOfficialPublicMemberNamesFromLocalSource()
        {
            var repoRoot = GetRepoRoot();
            var officialViewModelsRoot = @"D:\repos\WPF-Samples\Sample Applications\WPFGallery\ViewModels";
            if (!Directory.Exists(officialViewModelsRoot))
            {
                Assert.Inconclusive("Local official WPF Gallery source was not found at " + officialViewModelsRoot + ".");
            }

            var localSources = Directory
                .EnumerateFiles(Path.Combine(repoRoot, "ModernWpf.Gallery"), "*.cs", SearchOption.AllDirectories)
                .Where(path => !path.Contains(Path.DirectorySeparatorChar + "bin" + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
                .Where(path => !path.Contains(Path.DirectorySeparatorChar + "obj" + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
                .Select(path => new
                {
                    Path = path,
                    Source = File.ReadAllText(path)
                })
                .ToList();
            var missingMembers = new List<string>();
            var mappedClassCount = 0;
            var officialMemberCount = 0;

            foreach (var officialPath in Directory.EnumerateFiles(officialViewModelsRoot, "*.cs", SearchOption.AllDirectories))
            {
                var officialSource = File.ReadAllText(officialPath);
                var classMatch = Regex.Match(officialSource, "public\\s+partial\\s+class\\s+(\\w+)");
                if (!classMatch.Success)
                {
                    continue;
                }

                var className = classMatch.Groups[1].Value;
                var localSource = localSources.FirstOrDefault(file =>
                    Regex.IsMatch(file.Source, "\\bclass\\s+" + Regex.Escape(className) + "\\b"));
                if (localSource == null)
                {
                    missingMembers.Add(Path.GetRelativePath(officialViewModelsRoot, officialPath) + " :: missing local class " + className);
                    continue;
                }

                mappedClassCount++;
                var officialRelativePath = Path.GetRelativePath(officialViewModelsRoot, officialPath);
                var officialMembers = GetPublicCodeBehindMemberNames(officialSource)
                    .Where(memberName => !IsOfficialViewModelPublicMemberAdaptedAway(className, memberName))
                    .ToArray();

                officialMemberCount += officialMembers.Length;
                foreach (var memberName in officialMembers)
                {
                    if (!Regex.IsMatch(localSource.Source, "\\b" + Regex.Escape(memberName) + "\\b"))
                    {
                        missingMembers.Add(officialRelativePath + " :: " + className + "." + memberName + " -> " + Path.GetRelativePath(repoRoot, localSource.Path));
                    }
                }
            }

            Assert.AreEqual(57, mappedClassCount, "The active WPF Gallery ViewModel mapping count changed; update the 5.4 public-member scan deliberately.");
            Assert.AreEqual(76, officialMemberCount, "The active WPF Gallery ViewModel public member count changed; update the 5.4 public-member scan deliberately.");
            Assert.AreEqual(
                0,
                missingMembers.Count,
                "Missing official 5.4 ViewModel public member names from local official source:\n" + string.Join("\n", missingMembers));
        }

        [TestMethod]
        public void MappedWpfGalleryModelsKeepOfficialPublicPropertyNamesFromLocalSource()
        {
            var repoRoot = GetRepoRoot();
            var officialModelsRoot = @"D:\repos\WPF-Samples\Sample Applications\WPFGallery\Models";
            if (!Directory.Exists(officialModelsRoot))
            {
                Assert.Inconclusive("Local official WPF Gallery source was not found at " + officialModelsRoot + ".");
            }

            var modelMappings = new[]
            {
                new
                {
                    OfficialFileName = "Product.cs",
                    OfficialClassName = "Product",
                    LocalRelativePath = Path.Combine("ModernWpf.Gallery", "Models", "Product.cs")
                },
                new
                {
                    OfficialFileName = "Person.cs",
                    OfficialClassName = "Person",
                    LocalRelativePath = Path.Combine("ModernWpf.Gallery", "Models", "Person.cs")
                },
                new
                {
                    OfficialFileName = "IconsData.cs",
                    OfficialClassName = "IconData",
                    LocalRelativePath = Path.Combine("ModernWpf.Gallery", "Pages", "WpfGallery", "DesignGuidance", "IconData.cs")
                },
                new
                {
                    OfficialFileName = "User.cs",
                    OfficialClassName = "User",
                    LocalRelativePath = Path.Combine("ModernWpf.Gallery", "Pages", "WpfGallery", "Samples", "UserDashboardUser.cs")
                }
            };
            var missingProperties = new List<string>();
            var officialPropertyCount = 0;

            foreach (var mapping in modelMappings)
            {
                var officialPath = Path.Combine(officialModelsRoot, mapping.OfficialFileName);
                var localPath = Path.Combine(repoRoot, mapping.LocalRelativePath);
                var officialProperties = GetPublicPropertyNames(File.ReadAllText(officialPath)).ToArray();
                var localProperties = new HashSet<string>(
                    GetPublicPropertyNames(File.ReadAllText(localPath)),
                    StringComparer.Ordinal);

                officialPropertyCount += officialProperties.Length;
                foreach (var propertyName in officialProperties)
                {
                    if (!localProperties.Contains(propertyName))
                    {
                        missingProperties.Add(mapping.OfficialClassName + "." + propertyName + " -> " + mapping.LocalRelativePath);
                    }
                }
            }

            Assert.AreEqual(4, modelMappings.Length, "The copied/adapted WPF Gallery model mapping count changed; update the 5.4 model property scan deliberately.");
            Assert.AreEqual(27, officialPropertyCount, "The official copied/adapted WPF Gallery model public property count changed; update the 5.4 model property scan deliberately.");
            Assert.AreEqual(
                0,
                missingProperties.Count,
                "Missing official 5.4 model property names from local official source:\n" + string.Join("\n", missingProperties));
        }

        [TestMethod]
        public void MappedWpfGalleryModelsKeepOfficialPublicMemberNamesFromLocalSource()
        {
            var repoRoot = GetRepoRoot();
            var officialModelsRoot = @"D:\repos\WPF-Samples\Sample Applications\WPFGallery\Models";
            if (!Directory.Exists(officialModelsRoot))
            {
                Assert.Inconclusive("Local official WPF Gallery source was not found at " + officialModelsRoot + ".");
            }

            var modelMappings = new[]
            {
                new
                {
                    OfficialFileName = "Product.cs",
                    OfficialClassName = "Product",
                    LocalRelativePath = Path.Combine("ModernWpf.Gallery", "Models", "Product.cs")
                },
                new
                {
                    OfficialFileName = "Person.cs",
                    OfficialClassName = "Person",
                    LocalRelativePath = Path.Combine("ModernWpf.Gallery", "Models", "Person.cs")
                },
                new
                {
                    OfficialFileName = "IconsData.cs",
                    OfficialClassName = "IconData",
                    LocalRelativePath = Path.Combine("ModernWpf.Gallery", "Pages", "WpfGallery", "DesignGuidance", "IconData.cs")
                },
                new
                {
                    OfficialFileName = "User.cs",
                    OfficialClassName = "User",
                    LocalRelativePath = Path.Combine("ModernWpf.Gallery", "Pages", "WpfGallery", "Samples", "UserDashboardUser.cs")
                }
            };
            var missingMembers = new List<string>();
            var officialMemberCount = 0;

            foreach (var mapping in modelMappings)
            {
                var officialPath = Path.Combine(officialModelsRoot, mapping.OfficialFileName);
                var localPath = Path.Combine(repoRoot, mapping.LocalRelativePath);
                var officialMembers = GetPublicModelMemberNames(File.ReadAllText(officialPath))
                    .Where(memberName => !IsOfficialModelPublicMemberAdaptedAway(mapping.OfficialClassName, memberName))
                    .ToArray();
                var localSource = File.ReadAllText(localPath);

                officialMemberCount += officialMembers.Length;
                foreach (var memberName in officialMembers)
                {
                    if (!Regex.IsMatch(localSource, "\\b" + Regex.Escape(memberName) + "\\b"))
                    {
                        missingMembers.Add(mapping.OfficialClassName + "." + memberName + " -> " + mapping.LocalRelativePath);
                    }
                }
            }

            Assert.AreEqual(4, modelMappings.Length, "The copied/adapted WPF Gallery model mapping count changed; update the 5.4 model public-member scan deliberately.");
            Assert.AreEqual(30, officialMemberCount, "The official copied/adapted WPF Gallery model public member count changed; update the 5.4 model public-member scan deliberately.");
            Assert.AreEqual(
                0,
                missingMembers.Count,
                "Missing official 5.4 model public member names from local official source:\n" + string.Join("\n", missingMembers));
        }

        [TestMethod]
        public void MappedWpfGalleryHelpersKeepOfficialPublicMethodNamesFromLocalSource()
        {
            var repoRoot = GetRepoRoot();
            var officialHelpersRoot = @"D:\repos\WPF-Samples\Sample Applications\WPFGallery\Helpers";
            if (!Directory.Exists(officialHelpersRoot))
            {
                Assert.Inconclusive("Local official WPF Gallery source was not found at " + officialHelpersRoot + ".");
            }

            var helperMappings = new[]
            {
                new
                {
                    OfficialFileName = "AlphabeticValidationRule.cs",
                    OfficialClassName = "AlphabeticValidationRule",
                    LocalRelativePath = Path.Combine("ModernWpf.Gallery", "Helpers", "AlphabeticValidationRule.cs")
                },
                new
                {
                    OfficialFileName = "NullToVisibilityConverter.cs",
                    OfficialClassName = "NullToVisibilityConverter",
                    LocalRelativePath = Path.Combine("ModernWpf.Gallery", "Controls", "NullToVisibilityConverter.cs")
                },
                new
                {
                    OfficialFileName = "EmptyToVisibilityConverter.cs",
                    OfficialClassName = "EmptyToVisibilityConverter",
                    LocalRelativePath = Path.Combine("ModernWpf.Gallery", "Pages", "WpfGallery", "Samples", "UserDashboardConverters.cs")
                },
                new
                {
                    OfficialFileName = "ImageIdToBrushConverter.cs",
                    OfficialClassName = "ImageIdToBrushConverter",
                    LocalRelativePath = Path.Combine("ModernWpf.Gallery", "Pages", "WpfGallery", "Samples", "UserDashboardConverters.cs")
                }
            };
            var missingMethods = new List<string>();
            var officialMethodCount = 0;

            foreach (var mapping in helperMappings)
            {
                var officialPath = Path.Combine(officialHelpersRoot, mapping.OfficialFileName);
                var localPath = Path.Combine(repoRoot, mapping.LocalRelativePath);
                var officialMethods = GetPublicMethodNames(File.ReadAllText(officialPath)).ToArray();
                var localMethods = new HashSet<string>(
                    GetPublicMethodNames(File.ReadAllText(localPath)),
                    StringComparer.Ordinal);

                officialMethodCount += officialMethods.Length;
                foreach (var methodName in officialMethods)
                {
                    if (!localMethods.Contains(methodName))
                    {
                        missingMethods.Add(mapping.OfficialClassName + "." + methodName + " -> " + mapping.LocalRelativePath);
                    }
                }
            }

            Assert.AreEqual(4, helperMappings.Length, "The copied/adapted WPF Gallery helper mapping count changed; update the 5.4 helper method scan deliberately.");
            Assert.AreEqual(7, officialMethodCount, "The official copied/adapted WPF Gallery helper public method count changed; update the 5.4 helper method scan deliberately.");
            Assert.AreEqual(
                0,
                missingMethods.Count,
                "Missing official 5.4 helper method names from local official source:\n" + string.Join("\n", missingMethods));
        }

        [TestMethod]
        public void ActiveMappedCodeBehindKeepsOfficialPublicMemberNamesFromLocalSource()
        {
            var repoRoot = GetRepoRoot();
            var officialViewsRoot = @"D:\repos\WPF-Samples\Sample Applications\WPFGallery\Views";
            if (!Directory.Exists(officialViewsRoot))
            {
                Assert.Inconclusive("Local official WPF Gallery source was not found at " + officialViewsRoot + ".");
            }

            var localPagesRoot = Path.Combine(repoRoot, "ModernWpf.Gallery", "Pages");
            var localCodeBehindByFileName = Directory
                .EnumerateFiles(localPagesRoot, "*.xaml.cs", SearchOption.AllDirectories)
                .ToDictionary(Path.GetFileName, path => path, StringComparer.OrdinalIgnoreCase);
            var missingMembers = new List<string>();
            var mappedFileCount = 0;
            var officialMemberCount = 0;

            foreach (var officialPath in Directory.EnumerateFiles(officialViewsRoot, "*.xaml.cs", SearchOption.AllDirectories))
            {
                var officialFileName = Path.GetFileName(officialPath);
                if (!localCodeBehindByFileName.TryGetValue(officialFileName, out var localPath))
                {
                    continue;
                }

                mappedFileCount++;
                var officialSource = File.ReadAllText(officialPath);
                var localSource = File.ReadAllText(localPath);
                var officialRelativePath = Path.GetRelativePath(officialViewsRoot, officialPath);
                var officialMembers = GetPublicCodeBehindMemberNames(officialSource).ToArray();

                officialMemberCount += officialMembers.Length;
                foreach (var memberName in officialMembers)
                {
                    if (!Regex.IsMatch(localSource, "\\b" + Regex.Escape(memberName) + "\\b"))
                    {
                        missingMembers.Add(officialRelativePath + " :: " + memberName);
                    }
                }
            }

            Assert.AreEqual(54, mappedFileCount, "The active WPF Gallery code-behind mapping count changed; update the 5.4 public-member scan deliberately.");
            Assert.AreEqual(100, officialMemberCount, "The active WPF Gallery code-behind public member count changed; update the 5.4 public-member scan deliberately.");
            Assert.AreEqual(
                0,
                missingMembers.Count,
                "Missing official 5.4 code-behind public member names from local official source:\n" + string.Join("\n", missingMembers));
        }

        [TestMethod]
        public void MappedWpfGalleryControlsKeepOfficialPublicMemberNamesFromLocalSource()
        {
            var repoRoot = GetRepoRoot();
            var officialControlsRoot = @"D:\repos\WPF-Samples\Sample Applications\WPFGallery\Controls";
            if (!Directory.Exists(officialControlsRoot))
            {
                Assert.Inconclusive("Local official WPF Gallery source was not found at " + officialControlsRoot + ".");
            }

            var controlMappings = new[]
            {
                new
                {
                    OfficialFileName = "ColorPageExample.xaml.cs",
                    LocalRelativePath = Path.Combine("ModernWpf.Gallery", "Controls", "ColorPageExample.cs")
                },
                new
                {
                    OfficialFileName = "ColorTile.xaml.cs",
                    LocalRelativePath = Path.Combine("ModernWpf.Gallery", "Controls", "ColorTile.cs")
                },
                new
                {
                    OfficialFileName = "ControlExample.xaml.cs",
                    LocalRelativePath = Path.Combine("ModernWpf.Gallery", "Controls", "ControlExample.cs")
                },
                new
                {
                    OfficialFileName = "HeaderTile.xaml.cs",
                    LocalRelativePath = Path.Combine("ModernWpf.Gallery", "Controls", "HeaderTile.xaml.cs")
                },
                new
                {
                    OfficialFileName = "PageHeader.xaml.cs",
                    LocalRelativePath = Path.Combine("ModernWpf.Gallery", "Controls", "PageHeader.cs")
                },
                new
                {
                    OfficialFileName = "TileGallery.xaml.cs",
                    LocalRelativePath = Path.Combine("ModernWpf.Gallery", "Controls", "TileGallery.xaml.cs")
                }
            };
            var missingMembers = new List<string>();
            var officialMemberCount = 0;

            foreach (var mapping in controlMappings)
            {
                var officialPath = Path.Combine(officialControlsRoot, mapping.OfficialFileName);
                var localPath = Path.Combine(repoRoot, mapping.LocalRelativePath);
                var officialSource = File.ReadAllText(officialPath);
                var localSource = File.ReadAllText(localPath);
                var officialMembers = GetPublicCodeBehindMemberNames(officialSource).ToArray();

                officialMemberCount += officialMembers.Length;
                foreach (var memberName in officialMembers)
                {
                    if (!Regex.IsMatch(localSource, "\\b" + Regex.Escape(memberName) + "\\b"))
                    {
                        missingMembers.Add(mapping.OfficialFileName + " :: " + memberName + " -> " + mapping.LocalRelativePath);
                    }
                }
            }

            Assert.AreEqual(6, controlMappings.Length, "The copied/adapted WPF Gallery control mapping count changed; update the 5.4 control public-member scan deliberately.");
            Assert.AreEqual(29, officialMemberCount, "The official copied/adapted WPF Gallery control public member count changed; update the 5.4 control public-member scan deliberately.");
            Assert.AreEqual(
                0,
                missingMembers.Count,
                "Missing official 5.4 control public member names from local official source:\n" + string.Join("\n", missingMembers));
        }

        [TestMethod]
        public void MappedWpfGalleryCatalogItemsKeepOfficialPublicMemberNamesFromLocalSource()
        {
            var repoRoot = GetRepoRoot();
            var officialModelsRoot = @"D:\repos\WPF-Samples\Sample Applications\WPFGallery\Models";
            if (!Directory.Exists(officialModelsRoot))
            {
                Assert.Inconclusive("Local official WPF Gallery source was not found at " + officialModelsRoot + ".");
            }

            var officialSource = File.ReadAllText(Path.Combine(officialModelsRoot, "ControlsInfoDataSource.cs"));
            var officialItemSource = ExtractPublicClassSource(officialSource, "ControlInfoDataItem");
            var officialMembers = GetPublicPropertyNames(officialItemSource)
                .Concat(GetPublicMethodNames(officialItemSource))
                .Where(memberName => !IsOfficialCatalogItemMemberAdaptedAway(memberName))
                .Distinct(StringComparer.Ordinal)
                .OrderBy(memberName => memberName, StringComparer.Ordinal)
                .ToArray();

            var localSource = File.ReadAllText(Path.Combine(repoRoot, "ModernWpf.Gallery", "Models", "GalleryCatalog.cs"));
            var missingMembers = officialMembers
                .Where(memberName => !Regex.IsMatch(localSource, "\\b" + Regex.Escape(memberName) + "\\b"))
                .ToArray();

            Assert.AreEqual(10, officialMembers.Length, "The retained WPF Gallery catalog item public-member count changed; update the 5.4 catalog item scan deliberately.");
            Assert.AreEqual(
                0,
                missingMembers.Length,
                "Missing official 5.4 catalog item public member names from local official source:\n" + string.Join("\n", missingMembers));
        }

        [TestMethod]
        public void CopiedWpfGalleryCodeBehindClassesStayUnsealedLikeOfficialSource()
        {
            var repoRoot = GetRepoRoot();
            var wpfGalleryPageCodeBehind = Directory.EnumerateFiles(
                Path.Combine(repoRoot, "ModernWpf.Gallery", "Pages", "WpfGallery"),
                "*.xaml.cs",
                SearchOption.AllDirectories);
            var copiedTopLevelCodeBehind = new[]
            {
                Path.Combine(repoRoot, "ModernWpf.Gallery", "Pages", "DashboardPage.xaml.cs"),
                Path.Combine(repoRoot, "ModernWpf.Gallery", "Pages", "AllSamplesPage.xaml.cs"),
                Path.Combine(repoRoot, "ModernWpf.Gallery", "Pages", "WhatsNewPage.xaml.cs"),
                Path.Combine(repoRoot, "ModernWpf.Gallery", "Pages", "SettingsPage.xaml.cs"),
                Path.Combine(repoRoot, "ModernWpf.Gallery", "Controls", "HeaderTile.xaml.cs"),
                Path.Combine(repoRoot, "ModernWpf.Gallery", "Controls", "TileGallery.xaml.cs")
            };

            foreach (var path in wpfGalleryPageCodeBehind.Concat(copiedTopLevelCodeBehind))
            {
                var source = File.ReadAllText(path);
                var xamlFileName = Path.GetFileNameWithoutExtension(path);
                var className = Path.GetFileNameWithoutExtension(xamlFileName);
                var expectedSummaryName = GetOfficialCodeBehindSummaryName(className, xamlFileName);
                var expectedBaseType = className == "FrameWindow"
                    ? "Window"
                    : className == "HeaderTile" || className == "TileGallery"
                        ? "UserControl"
                        : "Page";
                var declaration = "public partial class " + className + " : " + expectedBaseType;
                Assert.IsFalse(
                    source.Contains("public sealed partial class", StringComparison.Ordinal),
                    Path.GetRelativePath(repoRoot, path) + " should match the official WPF Gallery unsealed partial class shape.");
                Assert.IsTrue(
                    source.Contains(declaration, StringComparison.Ordinal),
                    Path.GetRelativePath(repoRoot, path) + " should keep the official WPF Gallery explicit code-behind base type shape.");
                AssertContainsInOrder(
                    source,
                    "/// Interaction logic for " + expectedSummaryName,
                    declaration);
            }

            var sectionSource = ReadRepoFile(
                "ModernWpf.Gallery",
                "Pages",
                "WpfGallery",
                "WpfGallerySectionPages.cs");
            foreach (var className in new[]
            {
                "DesignGuidancePage",
                "SamplesPage",
                "BasicInputPage",
                "CollectionsPage",
                "DateAndTimePage",
                "LayoutPage",
                "MediaPage",
                "NavigationPage",
                "StatusAndInfoPage",
                "TextPage",
                "SystemPage"
            })
            {
                Assert.IsFalse(
                    sectionSource.Contains("public sealed class " + className + " : SectionPage", StringComparison.Ordinal),
                    className + " should remain unsealed like the official WPF Gallery section page type.");
                Assert.IsTrue(
                    sectionSource.Contains("public partial class " + className + " : SectionPage", StringComparison.Ordinal),
                    className + " should keep the official WPF Gallery partial section page declaration shape.");
            }
        }

        private static string NormalizeXamlTokenSource(string value)
        {
            return WebUtility.HtmlDecode(value)
                .Replace("&quot;", "\"")
                .Replace("&amp;", "&")
                .Replace(" ", string.Empty)
                .Replace("\r", string.Empty)
                .Replace("\n", string.Empty)
                .Replace("\t", string.Empty);
        }

        private static bool IsOfficialXamlHookTokenAdaptedAway(string relativePath, string token)
        {
            if (relativePath.EndsWith("WhatsNewPage.xaml", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            if (relativePath.EndsWith("SettingsPage.xaml", StringComparison.OrdinalIgnoreCase))
            {
                // Settings is intentionally project-owned: branding, dependencies, legal
                // links, and accessibility text must describe ModernWPF rather than Microsoft.
                return true;
            }

            if (relativePath.EndsWith(@"DesignGuidance\IconsPage.xaml", StringComparison.OrdinalIgnoreCase))
            {
                return token == "Click=\"Open_IconDesignGuidelinesPage\"" ||
                    token == "Click=\"Open_SegoeFontDownloadPage\"";
            }

            return false;
        }

        private static bool IsOfficialSamplePaneTokenAdaptedAway(string relativePath, string token)
        {
            if (relativePath.EndsWith("WhatsNewPage.xaml", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            return relativePath.EndsWith(@"Text\HyperlinkPage.xaml", StringComparison.OrdinalIgnoreCase) &&
                (token.StartsWith("HeaderText=", StringComparison.Ordinal) ||
                 token.StartsWith("XamlCode=", StringComparison.Ordinal));
        }

        private static bool IsOfficialObservableFieldAdaptedAway(string className, string fieldName)
        {
            if (fieldName == "_pageTitle" || fieldName == "_pageDescription")
            {
                return true;
            }

            if (className == "WhatsNewPageViewModel")
            {
                return fieldName == "_accentColorXamlCode" ||
                    fieldName == "_accentColorBrushApiXamlUsage" ||
                    fieldName == "_hyphenBasedLigatureXamlCode" ||
                    fieldName == "_hyphenBasedLiagatureXamlUsage" ||
                    fieldName == "_gridShorthandSyntaxXamlCode" ||
                    fieldName == "_gridShorthandSyntaxXamlUsage";
            }

            return className == "MainWindowViewModel"
                && (fieldName == "_controls" || fieldName == "_selectedControl");
        }

        private static bool IsOfficialViewModelPublicMemberAdaptedAway(string className, string memberName)
        {
            return className == "MainWindowViewModel" && memberName == "UpdateSearchText";
        }

        private static bool IsOfficialModelPublicMemberAdaptedAway(string className, string memberName)
        {
            return className == "User" && memberName == "User";
        }

        private static bool IsOfficialCatalogItemMemberAdaptedAway(string memberName)
        {
            return memberName == "IconGlyph" ||
                memberName == "PageName" ||
                memberName == "IsGroup";
        }

        private static string ExtractPublicClassSource(string source, string className)
        {
            var classMatch = Regex.Match(source, "public\\s+(?:sealed\\s+)?class\\s+" + Regex.Escape(className) + "\\b");
            Assert.IsTrue(classMatch.Success, "Missing public class " + className + " in official WPF Gallery source.");

            var braceIndex = source.IndexOf('{', classMatch.Index);
            Assert.IsTrue(braceIndex >= 0, "Missing opening brace for public class " + className + ".");

            var depth = 0;
            for (var index = braceIndex; index < source.Length; index++)
            {
                if (source[index] == '{')
                {
                    depth++;
                }
                else if (source[index] == '}')
                {
                    depth--;
                    if (depth == 0)
                    {
                        return source.Substring(classMatch.Index, index - classMatch.Index + 1);
                    }
                }
            }

            Assert.Fail("Missing closing brace for public class " + className + ".");
            return string.Empty;
        }

        private static IEnumerable<string> GetPublicPropertyNames(string source)
        {
            return Regex
                .Matches(
                    source,
                    "(?m)^\\s*public\\s+(?!class\\b)(?!record\\b)(?!event\\b)[^\\r\\n(]+?\\s+(\\w+)\\s*(?:\\{|=>)")
                .Cast<Match>()
                .Select(match => match.Groups[1].Value);
        }

        private static IEnumerable<string> GetPublicMethodNames(string source)
        {
            return Regex
                .Matches(
                    source,
                    "(?m)^\\s*public\\s+(?:override\\s+)?[^\\r\\n(;=]+?\\s+(\\w+)\\s*\\(")
                .Cast<Match>()
                .Select(match => match.Groups[1].Value);
        }

        private static IEnumerable<string> GetPublicModelMemberNames(string source)
        {
            var members = new List<string>();
            members.AddRange(
                Regex
                    .Matches(
                        source,
                        "(?m)^\\s*public\\s+(?:partial\\s+)?(?:class|record)\\s+(\\w+)\\b")
                    .Cast<Match>()
                    .Select(match => match.Groups[1].Value));
            members.AddRange(GetPublicPropertyNames(source));
            members.AddRange(
                Regex
                    .Matches(
                        source,
                        "(?m)^\\s*public\\s+(\\w+)\\s*\\(")
                    .Cast<Match>()
                    .Select(match => match.Groups[1].Value));
            members.AddRange(GetPublicMethodNames(source));

            return members.Distinct(StringComparer.Ordinal).OrderBy(memberName => memberName, StringComparer.Ordinal);
        }

        private static IEnumerable<string> GetPublicCodeBehindMemberNames(string source)
        {
            var members = new List<string>();
            members.AddRange(
                Regex
                    .Matches(
                        source,
                        "(?m)^\\s*public\\s+(?:partial\\s+)?class\\s+(\\w+)\\b")
                    .Cast<Match>()
                    .Select(match => match.Groups[1].Value));
            members.AddRange(
                Regex
                    .Matches(
                        source,
                        "(?m)^\\s*public\\s+(?!class\\b)(?!partial\\s+class\\b)(?!event\\b)[^\\r\\n{;(=]+?\\s+(\\w+)\\s*\\{")
                    .Cast<Match>()
                    .Select(match => match.Groups[1].Value));
            members.AddRange(
                Regex
                    .Matches(
                        source,
                        "(?m)^\\s*public\\s+(\\w+)\\s*\\(")
                    .Cast<Match>()
                    .Select(match => match.Groups[1].Value));
            members.AddRange(GetPublicMethodNames(source));

            return members.Distinct(StringComparer.Ordinal).OrderBy(memberName => memberName, StringComparer.Ordinal);
        }

        private static string GetOfficialCodeBehindSummaryName(string className, string xamlFileName)
        {
            if (className == "ButtonPage")
                return "Button.xaml";
            if (className == "CheckBoxPage")
                return "CheckBox.xaml";
            if (className == "ComboBoxPage")
                return "ComboBox.xaml";
            if (className == "Page1")
                return "FramePage1.xaml";
            if (className == "Page2")
                return "FramePage2.xaml";
            if (className == "RichTextEditPage")
                return "RichTextBoxPage.xaml";

            return xamlFileName;
        }

        [TestMethod]
        public void TopLevelCodeBehindKeepsOfficialPageBaseDeclarationShape()
        {
            foreach (var page in new[]
            {
                "DashboardPage",
                "AllSamplesPage",
                "WhatsNewPage",
                "SettingsPage"
            })
            {
                var source = ReadRepoFile(
                    "ModernWpf.Gallery",
                    "Pages",
                    page + ".xaml.cs");

                Assert.IsTrue(
                    source.Contains("public partial class " + page + " : Page", StringComparison.Ordinal),
                    page + " should match the official WPF Gallery top-level page base declaration shape.");
            }
        }

        [TestMethod]
        public void CopiedWpfGalleryViewModelClassesStayUnsealedLikeOfficialSource()
        {
            var repoRoot = GetRepoRoot();
            var wpfGalleryViewModels = Directory.EnumerateFiles(
                Path.Combine(repoRoot, "ModernWpf.Gallery", "Pages", "WpfGallery"),
                "*ViewModel*.cs",
                SearchOption.AllDirectories);
            var copiedTopLevelViewModels = new[]
            {
                Path.Combine(repoRoot, "ModernWpf.Gallery", "Pages", "SettingsPage.xaml.cs")
            };

            foreach (var path in wpfGalleryViewModels.Concat(copiedTopLevelViewModels))
            {
                var source = File.ReadAllText(path);
                Assert.IsFalse(
                    source.Contains("public sealed class ", StringComparison.Ordinal),
                    Path.GetRelativePath(repoRoot, path) + " should match the official WPF Gallery unsealed viewmodel class shape.");
            }
        }

        [TestMethod]
        public void CopiedWpfGalleryViewModelClassesKeepOfficialPartialDeclarationShape()
        {
            var repoRoot = GetRepoRoot();
            foreach (var file in new[]
            {
                new
                {
                    RelativePath = Path.Combine("ModernWpf.Gallery", "Pages", "SettingsPage.xaml.cs"),
                    ClassNames = new[] { "SettingsPageViewModel" }
                },
                new
                {
                    RelativePath = Path.Combine("ModernWpf.Gallery", "Pages", "WpfGallery", "WpfGalleryNavigationPageViewModels.cs"),
                    ClassNames = new[]
                    {
                        "DashboardPageViewModel",
                        "WhatsNewPageViewModel",
                        "AllSamplesPageViewModel",
                        "DesignGuidancePageViewModel",
                        "SamplesPageViewModel",
                        "BasicInputPageViewModel",
                        "CollectionsPageViewModel",
                        "DateAndTimePageViewModel",
                        "LayoutPageViewModel",
                        "MediaPageViewModel",
                        "NavigationPageViewModel",
                        "StatusAndInfoPageViewModel",
                        "TextPageViewModel",
                        "SystemPageViewModel"
                    }
                },
                new
                {
                    RelativePath = Path.Combine("ModernWpf.Gallery", "Pages", "WpfGallery", "BasicInput", "BasicInputPageViewModels.cs"),
                    ClassNames = new[] { "ButtonPageViewModel", "CheckBoxPageViewModel", "ComboBoxPageViewModel", "RadioButtonPageViewModel", "SliderPageViewModel" }
                },
                new
                {
                    RelativePath = Path.Combine("ModernWpf.Gallery", "Pages", "WpfGallery", "Collections", "CollectionsPageViewModels.cs"),
                    ClassNames = new[] { "DataGridPageViewModel", "ListBoxPageViewModel", "ListViewPageViewModel", "TreeViewPageViewModel" }
                },
                new
                {
                    RelativePath = Path.Combine("ModernWpf.Gallery", "Pages", "WpfGallery", "DateAndTime", "DateAndTimePageViewModels.cs"),
                    ClassNames = new[] { "CalendarPageViewModel", "DatePickerPageViewModel" }
                },
                new
                {
                    RelativePath = Path.Combine("ModernWpf.Gallery", "Pages", "WpfGallery", "DesignGuidance", "DesignGuidancePageViewModels.cs"),
                    ClassNames = new[] { "ColorsPageViewModel", "TypographyPageViewModel", "SpacingPageViewModel", "GeometryPageViewModel" }
                },
                new
                {
                    RelativePath = Path.Combine("ModernWpf.Gallery", "Pages", "WpfGallery", "DesignGuidance", "IconsPageViewModel.cs"),
                    ClassNames = new[] { "IconsPageViewModel" }
                },
                new
                {
                    RelativePath = Path.Combine("ModernWpf.Gallery", "Pages", "WpfGallery", "Layout", "LayoutPageViewModels.cs"),
                    ClassNames = new[] { "BorderPageViewModel", "ExpanderPageViewModel", "GridPageViewModel", "GridSplitterPageViewModel", "GroupBoxPageViewModel", "ResizeGripPageViewModel", "StackPanelPageViewModel" }
                },
                new
                {
                    RelativePath = Path.Combine("ModernWpf.Gallery", "Pages", "WpfGallery", "Media", "MediaPageViewModels.cs"),
                    ClassNames = new[] { "CanvasPageViewModel", "ImagePageViewModel" }
                },
                new
                {
                    RelativePath = Path.Combine("ModernWpf.Gallery", "Pages", "WpfGallery", "Navigation", "NavigationPageViewModels.cs"),
                    ClassNames = new[] { "MenuPageViewModel", "TabControlPageViewModel", "FramePageViewModel", "NavigationWindowPageViewModel" }
                },
                new
                {
                    RelativePath = Path.Combine("ModernWpf.Gallery", "Pages", "WpfGallery", "Samples", "UserDashboardPageViewModel.cs"),
                    ClassNames = new[] { "UserDashboardPageViewModel" }
                },
                new
                {
                    RelativePath = Path.Combine("ModernWpf.Gallery", "Pages", "WpfGallery", "StatusAndInfo", "StatusAndInfoPageViewModels.cs"),
                    ClassNames = new[] { "ProgressBarPageViewModel", "ToolTipPageViewModel" }
                },
                new
                {
                    RelativePath = Path.Combine("ModernWpf.Gallery", "Pages", "WpfGallery", "System", "SystemPageViewModels.cs"),
                    ClassNames = new[] { "FileAndFolderDialogsPageViewModel", "MessageBoxPageViewModel", "ClipboardPageViewModel" }
                },
                new
                {
                    RelativePath = Path.Combine("ModernWpf.Gallery", "Pages", "WpfGallery", "Text", "TextPageViewModels.cs"),
                    ClassNames = new[] { "LabelPageViewModel", "TextBoxPageViewModel", "TextBlockPageViewModel", "HyperlinkPageViewModel", "RichTextEditPageViewModel", "PasswordBoxPageViewModel" }
                }
            })
            {
                var source = File.ReadAllText(Path.Combine(repoRoot, file.RelativePath));
                foreach (var className in file.ClassNames)
                {
                    Assert.IsTrue(
                        source.Contains("public partial class " + className, StringComparison.Ordinal),
                        className + " should match the official WPF Gallery partial viewmodel declaration shape.");
                }
            }
        }

        [TestMethod]
        public void WpfGalleryPageViewModelProvidesObservableStateAdapter()
        {
            var observableSource = ReadRepoFile(
                "ModernWpf.Gallery",
                "Pages",
                "WpfGallery",
                "WpfGalleryObservableObject.cs");
            var source = ReadRepoFile(
                "ModernWpf.Gallery",
                "Pages",
                "WpfGallery",
                "WpfGalleryPageViewModel.cs");

            AssertContainsInOrder(
                observableSource,
                "public class WpfGalleryObservableObject : INotifyPropertyChanged",
                "public event PropertyChangedEventHandler PropertyChanged;",
                "protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null)",
                "EqualityComparer<T>.Default.Equals(field, value)",
                "OnPropertyChanged(propertyName);",
                "protected void OnPropertyChanged([CallerMemberName] string propertyName = null)",
                "handler(this, new PropertyChangedEventArgs(propertyName));");
            AssertContainsInOrder(
                source,
                "public class WpfGalleryPageViewModel : WpfGalleryObservableObject",
                "private string _pageTitle;",
                "private string _pageDescription;",
                "public string PageTitle",
                "SetProperty(ref _pageTitle, value);",
                "public string PageDescription",
                "SetProperty(ref _pageDescription, value);");
            Assert.IsFalse(
                source.Contains("public event PropertyChangedEventHandler", StringComparison.Ordinal),
                "WpfGalleryPageViewModel should use the shared observable adapter instead of duplicating notification plumbing.");
            Assert.IsFalse(
                source.Contains("protected bool SetProperty<T>", StringComparison.Ordinal),
                "WpfGalleryPageViewModel should keep SetProperty on the shared observable adapter.");
        }

        [TestMethod]
        public void TopLevelWpfGalleryViewModelsKeepOfficialStateAndNavigateSourceShape()
        {
            var source = ReadRepoFile(
                "ModernWpf.Gallery",
                "Pages",
                "WpfGallery",
                "WpfGalleryNavigationPageViewModels.cs");

            AssertContainsInOrder(
                source,
                "public partial class DashboardPageViewModel : WpfGalleryPageViewModel",
                "private IReadOnlyList<GalleryGroup> _navigationCards = GalleryCatalog.OverviewGroups;",
                "private IReadOnlyList<GalleryItem> _recentlyAddedOrUpdatedSamplesInfo = GalleryCatalog.NewOrUpdatedItems;",
                "private readonly Action<object> _navigate;",
                "public DashboardPageViewModel(Action<object> navigate)",
                ": base(string.Empty, string.Empty)",
                "_navigate = navigate;",
                "NavigateCommand = new GalleryCommand(Navigate);",
                "public IReadOnlyList<GalleryGroup> NavigationCards",
                "SetProperty(ref _navigationCards, value ?? Array.Empty<GalleryGroup>());",
                "public IReadOnlyList<GalleryItem> RecentlyAddedOrUpdatedSamplesInfo",
                "SetProperty(ref _recentlyAddedOrUpdatedSamplesInfo, value ?? Array.Empty<GalleryItem>());",
                "public ICommand NavigateCommand { get; }",
                "public void Navigate(object pageType)",
                "if (pageType is Type page)",
                "_navigate(page);",
                "else if (pageType != null && _navigate != null)",
                "_navigate(pageType);");
            AssertContainsInOrder(
                source,
                "public partial class WhatsNewPageViewModel : WpfGalleryPageViewModel",
                "private IReadOnlyList<GalleryItem> _newOrUpdatedItems = GalleryCatalog.NewOrUpdatedItems;",
                "private string _recommendedResourcesXamlCode = _recommendedResourcesXamlUsage;",
                "private readonly Action<object> _navigate;",
                "public WhatsNewPageViewModel(Action<object> navigate)",
                "GalleryBranding.WhatsNewTitle",
                "GalleryBranding.WhatsNewDescription",
                "_navigate = navigate;",
                "NavigateCommand = new GalleryCommand(Navigate);",
                "public IReadOnlyList<GalleryItem> NewOrUpdatedItems",
                "SetProperty(ref _newOrUpdatedItems, value ?? Array.Empty<GalleryItem>());",
                "public string RecommendedResourcesXamlCode",
                "SetProperty(ref _recommendedResourcesXamlCode, value);",
                "public ICommand NavigateCommand { get; }",
                "public void Navigate(object pageType)",
                "if (pageType is Type page)",
                "_navigate(page);",
                "else if (pageType != null && _navigate != null)",
                "_navigate(pageType);",
                "private const string _recommendedResourcesXamlUsage =",
                "<ui:ThemeResources />",
                "<ui:FluentControlsResources UseCompactResources=\\\"False\\\" />");
        }

        [TestMethod]
        public void SettingsViewModelKeepsOfficialObservableTitleSourceShape()
        {
            var source = ReadRepoFile(
                "ModernWpf.Gallery",
                "Pages",
                "SettingsPage.xaml.cs");

            AssertContainsInOrder(
                source,
                "using ModernWpf.Gallery.Pages.WpfGallery;",
                "public partial class SettingsPageViewModel : WpfGalleryPageViewModel",
                "public SettingsPageViewModel()",
                ": base(\"Settings\", null)");
            Assert.IsFalse(
                source.Contains("public string PageTitle", StringComparison.Ordinal),
                "Settings should reuse the shared observable page-title adapter instead of a computed PageTitle getter.");
            Assert.IsFalse(
                source.Contains("public string PageDescription", StringComparison.Ordinal),
                "Settings should reuse the shared observable page-description adapter instead of a computed PageDescription getter.");
        }

        [TestMethod]
        public void DesignGuidanceViewModelsKeepOfficialObservableStateSourceShape()
        {
            var simpleSource = ReadRepoFile(
                "ModernWpf.Gallery",
                "Pages",
                "WpfGallery",
                "DesignGuidance",
                "DesignGuidancePageViewModels.cs");

            AssertContainsInOrder(
                simpleSource,
                "public partial class ColorsPageViewModel : WpfGalleryPageViewModel",
                ": base(\"Colors\", \"Guide showing how to use colors in your app\")",
                "public partial class TypographyPageViewModel : WpfGalleryPageViewModel",
                ": base(\"Typography\", \"Guide showing how to use typography in your app\")",
                "public partial class SpacingPageViewModel : WpfGalleryPageViewModel",
                ": base(\"Spacing\", \"Guide showing how to use spacing in your app\")",
                "public partial class GeometryPageViewModel : WpfGalleryPageViewModel",
                ": base(\"Geometry\", \"\")");

            var iconographySource = ReadRepoFile(
                "ModernWpf.Gallery",
                "Pages",
                "WpfGallery",
                "DesignGuidance",
                "IconsPageViewModel.cs");

            AssertContainsInOrder(
                iconographySource,
                "public partial class IconsPageViewModel : WpfGalleryPageViewModel",
                "private ICollection<IconData> _allIcons = new List<IconData>();",
                "private IconData _selectedIcon;",
                "private string _searchText = string.Empty;",
                "private ObservableCollection<IconData> _searchFilteredIcons = new ObservableCollection<IconData>();",
                "private ObservableCollection<IconData> _displayedIcons = new ObservableCollection<IconData>();",
                "private int _currentPage = 1;",
                "private int _totalPages = 1;",
                "private int _selectedPageSizeIndex = 1; // Default to 250",
                "public IconsPageViewModel()",
                ": base(\"Icons\", \"Guide showing how to use icons in your application.\")",
                "public ICollection<IconData> AllIcons",
                "SetProperty(ref _allIcons, value ?? new List<IconData>());",
                "public ObservableCollection<IconData> SearchFilteredIcons",
                "SetProperty(ref _searchFilteredIcons, value ?? new ObservableCollection<IconData>());",
                "public ObservableCollection<IconData> DisplayedIcons",
                "SetProperty(ref _displayedIcons, value ?? new ObservableCollection<IconData>());",
                "public IconData SelectedIcon",
                "SetProperty(ref _selectedIcon, value);",
                "public string SearchText",
                "if (SetProperty(ref _searchText, value))",
                "public List<string> PageSizeOptions { get; } = new List<string> { \"100\", \"250\", \"500\", \"1000\", \"All\" };",
                "private int PageSize => SelectedPageSizeIndex == 4 ? int.MaxValue : int.Parse(PageSizeOptions[SelectedPageSizeIndex]);",
                "public ICommand LoadDataCommand { get; }",
                "AllIcons = ReadIconData().ToList();",
                "SelectedIcon = AllIcons.FirstOrDefault();",
                "SearchFilteredIcons = new ObservableCollection<IconData>(AllIcons);",
                "UpdatePagination();",
                "var selectedIconName = previousSelectedIcon?.Name;",
                "SearchFilteredIcons.Clear();",
                "var searchFilteredIconData = AllIcons.Where(icon =>",
                "(icon.Tags?.Any(tag => tag.IndexOf(filterText, comparison) >= 0) ?? false));",
                "foreach (var item in searchFilteredIconData)",
                "SearchFilteredIcons.Add(item);",
                "Func<IconData, bool> predicate =",
                "DisplayedIcons.Any(icon => icon.Name.Equals(selectedIconName)) ?",
                "icon => icon.Name.Equals(selectedIconName) :",
                "icon => true;",
                "SelectedIcon = DisplayedIcons.FirstOrDefault(predicate);",
                "private void ApplyTagFilter(string? tag)",
                "var trimmedTag = tag.Trim();",
                "if (string.Equals(trimmedTag, SearchText, StringComparison.Ordinal))",
                "SearchText = trimmedTag;",
                "private void PreviousPage()",
                "private bool CanGoToPreviousPage() => CurrentPage > 1;",
                "private void NextPage()",
                "private bool CanGoToNextPage() => CurrentPage < TotalPages;",
                "if (TotalPages == 0) TotalPages = 1;",
                "private void UpdateDisplayedIcons(bool resetSelectedIcon = true)",
                "var skip = (CurrentPage - 1) * pageSize;",
                "var iconsToDisplay = pageSize == int.MaxValue ? SearchFilteredIcons : SearchFilteredIcons.Skip(skip).Take(pageSize);",
                "if(resetSelectedIcon)");

            var loadDataStart = iconographySource.IndexOf("private void LoadData()", StringComparison.Ordinal);
            var updateSearchFilterStart = iconographySource.IndexOf("private void UpdateSearchFilter()", loadDataStart, StringComparison.Ordinal);
            var loadDataSource = iconographySource.Substring(loadDataStart, updateSearchFilterStart - loadDataStart);
            Assert.IsFalse(
                loadDataSource.Contains("CurrentPage = 1;", StringComparison.Ordinal),
                "Iconography LoadData should keep the official WPF Gallery reload behavior instead of forcing the current page back to 1.");
            Assert.IsFalse(
                iconographySource.Contains("public event PropertyChangedEventHandler", StringComparison.Ordinal),
                "Iconography should use the shared observable page-view-model adapter instead of local event plumbing.");
            Assert.IsFalse(
                iconographySource.Contains("private void OnPropertyChanged", StringComparison.Ordinal),
                "Iconography should use the shared SetProperty adapter instead of local OnPropertyChanged plumbing.");
        }

        [TestMethod]
        public void SystemViewModelsKeepOfficialObservableStateSourceShape()
        {
            var source = ReadRepoFile(
                "ModernWpf.Gallery",
                "Pages",
                "WpfGallery",
                "System",
                "SystemPageViewModels.cs");

            AssertContainsInOrder(
                source,
                "public abstract class SystemPageViewModelBase : WpfGalleryPageViewModel",
                "protected SystemPageViewModelBase(string pageTitle, string pageDescription)",
                ": base(pageTitle, pageDescription)",
                "/// <summary>",
                "/// Interaction logic for FileAndFolderDialogsPage.xaml",
                "/// </summary>",
                "public partial class FileAndFolderDialogsPageViewModel : SystemPageViewModelBase",
                "private string _singleFilePath = \"No file selected\";",
                "private string _multipleFilesPath = \"No files selected\";",
                "private string _fileContent = \"Enter text here to save to a file...\";",
                "private string _savedFilePath = \"No file saved\";",
                "private string _selectedFolderPath = \"No folder selected\";",
                ": base(",
                "\"File and Folder Dialogs\",",
                "\"Use the OpenFileDialog, SaveFileDialog, and OpenFolderDialog to let users select files and folders in a secure way.\")",
                "public string SingleFilePath",
                "SetProperty(ref _singleFilePath, value);",
                "public string MultipleFilesPath",
                "SetProperty(ref _multipleFilesPath, value);",
                "public string FileContent",
                "SetProperty(ref _fileContent, value);",
                "public string SavedFilePath",
                "SetProperty(ref _savedFilePath, value);",
                "public string SelectedFolderPath",
                "SetProperty(ref _selectedFolderPath, value);");
            AssertContainsInOrder(
                source,
                "/// <summary>",
                "/// Interaction logic for MessageBoxPage.xaml",
                "/// </summary>",
                "public partial class MessageBoxPageViewModel : SystemPageViewModelBase",
                "private string _defaultMessageResult = \"No message shown yet\";",
                "private string _customTitleResult = \"No message shown yet\";",
                "private int _selectedButtonIndex = 0;",
                "private string _differentButtonsResult = \"No button clicked yet\";",
                "private string _differentButtonsXamlCode = \"<Button Content=\\\"Show MessageBox\\\" Click=\\\"ShowMessageBoxButton_Click\\\" />\";",
                "private string _differentButtonsCSharpCode = string.Format(_differentButtonsMessageBoxSampleCSharpCodeString, \"\\tMessageBox.Show(\\\"Message\\\", \\\"Title\\\", MessageBoxButton.OK);\");",
                "private int _selectedImageIndex = 0;",
                "private string _differentImagesResult = \"No image example shown yet\";",
                "private string _differentImagesXamlCode = \"<Button Content=\\\"Show MessageBox\\\" Click=\\\"ShowMessageButton_Click\\\" />\";",
                "private string _differentImagesCSharpCode = string.Format(_differentImagesMessageBoxSampleCSharpCodeString, \"\\tMessageBox.Show(\\\"Message\\\", \\\"Title\\\", MessageBoxButton.OK, MessageBoxImage.None);\");",
                "private string _commonMessagesResult = \"No common message shown yet\";",
                "private string _commonMessagesXamlCode = @\"<WrapPanel Margin=\"\"0,0,0,10\"\">",
                "    <Button Content=\"\"Information\"\" Click=\"\"ShowInformationButton_Click\"\" />",
                "private string _commonMessagesCSharpCode = @\"// Information",
                "    MessageBox.Show(\"\"Operation completed successfully.\"\", \"\"Information\"\", MessageBoxButton.OK, MessageBoxImage.Information);",
                "private string _customDefaultResult = \"No selection made\";",
                "public MessageBoxPageViewModel()",
                ": base(\"MessageBox\", \"\")",
                "public string DifferentButtonsXamlCode",
                "private set { SetProperty(ref _differentButtonsXamlCode, value); }",
                "public string DifferentImagesXamlCode",
                "private set { SetProperty(ref _differentImagesXamlCode, value); }",
                "public string CommonMessagesXamlCode",
                "private set { SetProperty(ref _commonMessagesXamlCode, value); }",
                "public string CommonMessagesCSharpCode",
                "private set { SetProperty(ref _commonMessagesCSharpCode, value); }",
                "private void UpdateButtonCodeSnippets(int index)",
                "string content = index switch",
                "0 => \"\\tMessageBox.Show(\\\"Message\\\", \\\"Title\\\", MessageBoxButton.OK);\",",
                "1 => \"\\tvar result = MessageBox.Show(\\\"Message\\\", \\\"Title\\\", MessageBoxButton.OKCancel);\\n\" +",
                "2 => \"\\tvar result = MessageBox.Show(\\\"Message\\\", \\\"Title\\\", MessageBoxButton.AbortRetryIgnore);\\n\" +",
                "_ => \"\\tMessageBox.Show(\\\"Message\\\", \\\"Title\\\", MessageBoxButton.OK);\"",
                "DifferentButtonsCSharpCode = string.Format(_differentButtonsMessageBoxSampleCSharpCodeString, content);",
                "private void UpdateImageCodeSnippets(int index)",
                "string content = index switch",
                "0 => \"\\tMessageBox.Show(\\\"Message\\\", \\\"Title\\\", MessageBoxButton.OK, MessageBoxImage.None);\",",
                "1 => \"\\t// MessageBoxImage.Error (also Hand, Stop)\\n\" +",
                "4 => \"\\t// MessageBoxImage.Information (also Asterisk)\\n\" +",
                "_ => \"\\tMessageBox.Show(\\\"Message\\\", \\\"Title\\\", MessageBoxButton.OK, MessageBoxImage.None);\"",
                "DifferentImagesCSharpCode = string.Format(_differentImagesMessageBoxSampleCSharpCodeString, content);",
                "private const string _differentButtonsMessageBoxSampleCSharpCodeString =",
                "private const string _differentImagesMessageBoxSampleCSharpCodeString =");
            AssertContainsInOrder(
                source,
                "public partial class ClipboardPageViewModel : SystemPageViewModelBase",
                "private string _copyStatus = \"\";",
                "private string _pastedText = \"\";",
                "private string _clearStatus = \"\";",
                "private string _formatsInfo = \"\";",
                "private string _copyImageStatus = \"\";",
                "private string _pasteImageStatus = \"\";",
                "public ClipboardPageViewModel()",
                ": base(\"Clipboard\", \"\")");
            Assert.IsFalse(
                source.Contains("public event PropertyChangedEventHandler", StringComparison.Ordinal),
                "System view models should use the shared observable page-view-model adapter instead of local event plumbing.");
            Assert.IsFalse(
                source.Contains("private void OnPropertyChanged", StringComparison.Ordinal),
                "System view models should use the shared SetProperty adapter instead of local OnPropertyChanged plumbing.");
        }

        [TestMethod]
        public void WpfGalleryNavigationViewModelsKeepOfficialStateAndNavigateSourceShape()
        {
            var source = ReadRepoFile(
                "ModernWpf.Gallery",
                "Pages",
                "WpfGallery",
                "WpfGalleryNavigationPageViewModels.cs");

            AssertContainsInOrder(
                source,
                "public class WpfGalleryNavigationPageViewModel : WpfGalleryPageViewModel",
                "private IReadOnlyList<GalleryItem> _navigationCards;",
                "private readonly Action<object> _navigate;",
                "public WpfGalleryNavigationPageViewModel(",
                ": base(pageTitle, pageDescription)",
                "_navigationCards = navigationCards ?? Array.Empty<GalleryItem>();",
                "_navigate = navigate;",
                "NavigateCommand = new GalleryCommand(Navigate);",
                "public IReadOnlyList<GalleryItem> NavigationCards",
                "SetProperty(ref _navigationCards, value ?? Array.Empty<GalleryItem>());",
                "public ICommand NavigateCommand { get; }",
                "public void Navigate(object pageType)",
                "if (pageType is Type page)",
                "_navigate(page);",
                "else if (pageType != null && _navigate != null)",
                "_navigate(pageType);");
            foreach (var className in new[]
            {
                "AllSamplesPageViewModel",
                "DesignGuidancePageViewModel",
                "SamplesPageViewModel",
                "BasicInputPageViewModel",
                "CollectionsPageViewModel",
                "DateAndTimePageViewModel",
                "LayoutPageViewModel",
                "MediaPageViewModel",
                "NavigationPageViewModel",
                "StatusAndInfoPageViewModel",
                "TextPageViewModel",
                "SystemPageViewModel"
            })
            {
                StringAssert.Contains(
                    source,
                    "public partial class " + className + " : WpfGalleryNavigationPageViewModel");
            }

            AssertContainsInOrder(
                source,
                "public partial class AllSamplesPageViewModel : WpfGalleryNavigationPageViewModel",
                "public AllSamplesPageViewModel(Action<object> navigate)",
                ": base(\"All Controls\", \"\", GalleryCatalog.AllControlsItems, navigate)");
            StringAssert.Contains(source, "GetControlsInfo(\"Design Guidance\")");
            StringAssert.Contains(source, "GetControlsInfo(\"Basic Input\")");
            StringAssert.Contains(source, "GetControlsInfo(\"Date & Calendar\")");
            StringAssert.Contains(source, "GetControlsInfo(\"Media\")");
            StringAssert.Contains(source, "GetControlsInfo(\"Status & Info\")");
            Assert.IsFalse(source.Contains("GetControlsInfo(\"DesignGuidance\")", StringComparison.Ordinal));
            Assert.IsFalse(source.Contains("GetControlsInfo(\"BasicInput\")", StringComparison.Ordinal));
            Assert.IsFalse(source.Contains("GetControlsInfo(\"DateAndCalendar\")", StringComparison.Ordinal));
            Assert.IsFalse(source.Contains("GetControlsInfo(\"Media Controls\")", StringComparison.Ordinal));
            Assert.IsFalse(source.Contains("GetControlsInfo(\"StatusAndInfo\")", StringComparison.Ordinal));
        }

        [TestMethod]
        public void TextViewModelsKeepOfficialTextBoxValidatedTextSourceShape()
        {
            var source = ReadRepoFile(
                "ModernWpf.Gallery",
                "Pages",
                "WpfGallery",
                "Text",
                "TextPageViewModels.cs");

            AssertContainsInOrder(
                source,
                "public partial class TextBoxPageViewModel : WpfGalleryPageViewModel",
                "private string _validatedText = string.Empty;",
                "public TextBoxPageViewModel()",
                ": base(\"TextBox\", \"\")",
                "public string ValidatedText",
                "get { return _validatedText; }",
                "set { SetProperty(ref _validatedText, value); }");
        }

        [TestMethod]
        public void TextValidationRuleKeepsOfficialAlphabeticSourceShape()
        {
            var source = ReadRepoFile(
                "ModernWpf.Gallery",
                "Helpers",
                "AlphabeticValidationRule.cs");

            AssertContainsInOrder(
                source,
                "/// Validation rule that ensures the input contains only English alphabetic characters (a-z, A-Z).",
                "public class AlphabeticValidationRule : ValidationRule",
                "var input = value as string;",
                "if (string.IsNullOrEmpty(input))",
                "// Check if the input contains only English alphabetic characters (a-z, A-Z)",
                "if (!Regex.IsMatch(input, @\"^[a-zA-Z]+$\"))",
                "return new ValidationResult(false, \"Only English alphabetic characters (a-z, A-Z) are allowed.\");");
        }

        [TestMethod]
        public void SimpleItemViewModelsKeepOfficialObservableTitleSourceShape()
        {
            var dateSource = ReadRepoFile(
                "ModernWpf.Gallery",
                "Pages",
                "WpfGallery",
                "DateAndTime",
                "DateAndTimePageViewModels.cs");
            var mediaSource = ReadRepoFile(
                "ModernWpf.Gallery",
                "Pages",
                "WpfGallery",
                "Media",
                "MediaPageViewModels.cs");
            var statusSource = ReadRepoFile(
                "ModernWpf.Gallery",
                "Pages",
                "WpfGallery",
                "StatusAndInfo",
                "StatusAndInfoPageViewModels.cs");
            var layoutSource = ReadRepoFile(
                "ModernWpf.Gallery",
                "Pages",
                "WpfGallery",
                "Layout",
                "LayoutPageViewModels.cs");
            var navigationSource = ReadRepoFile(
                "ModernWpf.Gallery",
                "Pages",
                "WpfGallery",
                "Navigation",
                "NavigationPageViewModels.cs");
            var textSource = ReadRepoFile(
                "ModernWpf.Gallery",
                "Pages",
                "WpfGallery",
                "Text",
                "TextPageViewModels.cs");

            AssertContainsInOrder(
                dateSource,
                "public partial class CalendarPageViewModel : WpfGalleryPageViewModel",
                ": base(\"Calendar\", \"\")",
                "public partial class DatePickerPageViewModel : WpfGalleryPageViewModel",
                ": base(\"DatePicker\", \"\")");
            AssertContainsInOrder(
                mediaSource,
                "public partial class CanvasPageViewModel : WpfGalleryPageViewModel",
                ": base(\"Canvas\", \"\")",
                "public partial class ImagePageViewModel : WpfGalleryPageViewModel",
                ": base(\"Image\", \"\")");
            AssertContainsInOrder(
                statusSource,
                "public partial class ProgressBarPageViewModel : WpfGalleryPageViewModel",
                ": base(\"ProgressBar\", \"\")",
                "public partial class ToolTipPageViewModel : WpfGalleryPageViewModel",
                ": base(\"ToolTip\", \"\")");
            AssertContainsInOrder(
                layoutSource,
                "public partial class BorderPageViewModel : WpfGalleryPageViewModel",
                ": base(\"Border\", \"\")",
                "public partial class ExpanderPageViewModel : WpfGalleryPageViewModel",
                ": base(\"Expander\", \"\")",
                "public partial class GridPageViewModel : WpfGalleryPageViewModel",
                ": base(\"Grid\", \"\")",
                "public partial class GridSplitterPageViewModel : WpfGalleryPageViewModel",
                ": base(\"GridSplitter\", \"\")",
                "public partial class GroupBoxPageViewModel : WpfGalleryPageViewModel",
                ": base(\"GroupBox\", \"\")",
                "public partial class ResizeGripPageViewModel : WpfGalleryPageViewModel",
                ": base(\"ResizeGrip\", \"\")",
                "public partial class StackPanelPageViewModel : WpfGalleryPageViewModel",
                ": base(\"StackPanel\", \"\")");
            AssertContainsInOrder(
                navigationSource,
                "public partial class MenuPageViewModel : WpfGalleryPageViewModel",
                ": base(\"Menu\", \"\")",
                "public partial class TabControlPageViewModel : WpfGalleryPageViewModel",
                ": base(\"TabControl\", \"\")",
                "public partial class FramePageViewModel : WpfGalleryPageViewModel",
                ": base(\"Frame\", \"\")",
                "public partial class NavigationWindowPageViewModel : WpfGalleryPageViewModel",
                ": base(\"Navigation Window\", \"\")");
            AssertContainsInOrder(
                textSource,
                "public partial class LabelPageViewModel : WpfGalleryPageViewModel",
                ": base(\"Label\", \"\")",
                "public partial class TextBoxPageViewModel : WpfGalleryPageViewModel",
                ": base(\"TextBox\", \"\")",
                "public partial class TextBlockPageViewModel : WpfGalleryPageViewModel",
                ": base(\"TextBlock\", \"\")",
                "public partial class HyperlinkPageViewModel : WpfGalleryPageViewModel",
                ": base(\"Hyperlink\", \"\")",
                "public partial class RichTextEditPageViewModel : WpfGalleryPageViewModel",
                ": base(\"RichTextBox\", \"\")",
                "public partial class PasswordBoxPageViewModel : WpfGalleryPageViewModel",
                ": base(\"PasswordBox\", \"\")");

            foreach (var source in new[] { dateSource, mediaSource, statusSource, layoutSource, navigationSource, textSource })
            {
                Assert.IsFalse(
                    source.Contains("public event PropertyChangedEventHandler", StringComparison.Ordinal),
                    "Simple copied item view models should use the shared observable page-view-model adapter.");
                Assert.IsFalse(
                    source.Contains("private void OnPropertyChanged", StringComparison.Ordinal),
                    "Simple copied item view models should keep OnPropertyChanged on the shared observable adapter.");
                Assert.IsFalse(
                    source.Contains("private bool SetProperty", StringComparison.Ordinal),
                    "Simple copied item view models should keep SetProperty on the shared observable adapter.");
            }
        }

        [TestMethod]
        public void BasicInputViewModelsKeepOfficialStateAndCommandSourceShape()
        {
            var source = ReadRepoFile(
                "ModernWpf.Gallery",
                "Pages",
                "WpfGallery",
                "BasicInput",
                "BasicInputPageViewModels.cs").Replace("\r\n", "\n").Replace('\r', '\n');

            AssertContainsInOrder(
                source,
                "public abstract class BasicInputPageViewModelBase : WpfGalleryPageViewModel",
                "protected BasicInputPageViewModelBase(string pageTitle)",
                ": base(pageTitle, \"\")",
                "protected static ICommand CreateCommand(Action<object> execute)",
                "/// <summary>",
                "/// Interaction logic for Button.xaml",
                "/// </summary>",
                "public partial class ButtonPageViewModel : BasicInputPageViewModelBase",
                "private string _message = \"Hello World!\";",
                "private bool _isSimpleButtonEnabled = true;",
                "private bool _isUiButtonEnabled = true;",
                "SimpleButtonCheckboxCheckedCommand = CreateCommand(OnSimpleButtonCheckboxChecked);",
                "UiButtonCheckboxCheckedCommand = CreateCommand(OnUiButtonCheckboxChecked);",
                "public ICommand SimpleButtonCheckboxCheckedCommand { get; }",
                "public ICommand UiButtonCheckboxCheckedCommand { get; }",
                "public string Message",
                "SetProperty(ref _message, value);",
                "private void OnSimpleButtonCheckboxChecked(object sender)",
                "if (sender is not CheckBox checkbox)",
                "IsSimpleButtonEnabled = !(checkbox?.IsChecked ?? false);",
                "private void OnUiButtonCheckboxChecked(object sender)",
                "if (sender is not CheckBox checkbox)",
                "IsUiButtonEnabled = !(checkbox?.IsChecked ?? false);");
            AssertContainsInOrder(
                source,
                "public partial class CheckBoxPageViewModel : BasicInputPageViewModelBase",
                "private bool? _selectAllCheckBoxChecked = null;",
                "private bool _optionOneCheckBoxChecked = false;",
                "private bool _optionTwoCheckBoxChecked = true;",
                "private bool _optionThreeCheckBoxChecked = false;",
                "SingleCheckedCommand = CreateCommand(option => OnSingleChecked(option as string));",
                "private void OnSelectAllChecked(object sender)",
                "if (sender is not CheckBox checkBox)",
                "if (checkBox.IsChecked == null)\n                checkBox.IsChecked = !(\n                    OptionOneCheckBoxChecked && OptionTwoCheckBoxChecked && OptionThreeCheckBoxChecked\n                );",
                "private void OnSingleChecked(string option)",
                "if (OptionOneCheckBoxChecked && OptionTwoCheckBoxChecked && OptionThreeCheckBoxChecked)\n                SelectAllCheckBoxChecked = true;",
                "else if (!OptionOneCheckBoxChecked && !OptionTwoCheckBoxChecked && !OptionThreeCheckBoxChecked)\n                SelectAllCheckBoxChecked = false;",
                "else\n                SelectAllCheckBoxChecked = null;");
            AssertContainsInOrder(
                source,
                "public partial class ComboBoxPageViewModel : BasicInputPageViewModelBase",
                "private IList<string> _comboBoxFontFamilies = new ObservableCollection<string>",
                "\"Arial\",",
                "\"Comic Sans MS\",",
                "\"Segoe UI\",",
                "\"Times New Roman\"",
                "private IList<int> _comboBoxFontSizes = new ObservableCollection<int>",
                "8,",
                "72",
                "public IList<string> ComboBoxFontFamilies",
                "SetProperty(ref _comboBoxFontFamilies, value);",
                "public IList<int> ComboBoxFontSizes",
                "SetProperty(ref _comboBoxFontSizes, value);");
            AssertContainsInOrder(
                source,
                "public partial class RadioButtonPageViewModel : BasicInputPageViewModelBase",
                "private bool _isRadioButtonEnabled = true;",
                "private void OnRadioButtonCheckboxChecked(object sender)",
                "if (sender is not CheckBox checkbox)",
                "IsRadioButtonEnabled = !(checkbox?.IsChecked ?? false);");
            AssertContainsInOrder(
                source,
                "public partial class SliderPageViewModel : BasicInputPageViewModelBase",
                "private int _simpleSliderValue = 0;",
                "private int _rangeSliderValue = 500;",
                "private int _marksSliderValue = 0;",
                "private int _verticalSliderValue = 0;");
            Assert.IsFalse(
                source.Contains("public event PropertyChangedEventHandler", StringComparison.Ordinal),
                "Basic Input view models should use the shared observable page-view-model adapter instead of local event plumbing.");
            Assert.IsFalse(
                source.Contains("private void OnPropertyChanged", StringComparison.Ordinal),
                "Basic Input view models should use the shared SetProperty adapter instead of local OnPropertyChanged plumbing.");
        }

        [TestMethod]
        public void CollectionsViewModelsKeepOfficialConstructorAndSelectionModeSourceShape()
        {
            var source = ReadRepoFile(
                "ModernWpf.Gallery",
                "Pages",
                "WpfGallery",
                "Collections",
                "CollectionsPageViewModels.cs");

            AssertContainsInOrder(
                source,
                "public abstract class CollectionsPageViewModelBase : WpfGalleryPageViewModel",
                "protected CollectionsPageViewModelBase(string pageTitle)",
                ": base(pageTitle, \"\")",
                "public partial class DataGridPageViewModel : CollectionsPageViewModelBase",
                "private ObservableCollection<Product> _productsCollection;",
                "public DataGridPageViewModel()",
                "_productsCollection = GenerateProducts();",
                "private ObservableCollection<Product> GenerateProducts()",
                "public ObservableCollection<Product> ProductsCollection");
            AssertContainsInOrder(
                source,
                "public partial class ListBoxPageViewModel : CollectionsPageViewModelBase",
                "private ObservableCollection<string> _listBoxItems;",
                "public ListBoxPageViewModel()",
                "_listBoxItems = new ObservableCollection<string>",
                "\"Arial\",",
                "\"Times New Roman\"");
            AssertContainsInOrder(
                source,
                "public partial class ListViewPageViewModel : CollectionsPageViewModelBase",
                "private int _listViewSelectionModeComboBoxSelectedIndex = 0;",
                "public int ListViewSelectionModeComboBoxSelectedIndex",
                "get => _listViewSelectionModeComboBoxSelectedIndex;",
                "SetProperty<int>(ref _listViewSelectionModeComboBoxSelectedIndex, value);",
                "UpdateListViewSelectionMode(value);",
                "private SelectionMode _listViewSelectionMode = SelectionMode.Single;",
                "private ObservableCollection<Person> _basicListViewItems;",
                "private ObservableCollection<Person> _gridViewItems;",
                "public ListViewPageViewModel()",
                "_basicListViewItems = GeneratePersons(BasicListViewVisualTestSeed);",
                "_gridViewItems = GeneratePersons(GridViewVisualTestSeed);",
                "private ObservableCollection<Person> GeneratePersons(int visualTestSeed)",
                "private void UpdateListViewSelectionMode(int selectionModeIndex)",
                "ListViewSelectionMode = selectionModeIndex switch",
                "1 => SelectionMode.Multiple,",
                "2 => SelectionMode.Extended,",
                "_ => SelectionMode.Single");
            Assert.IsFalse(
                source.Contains("public event PropertyChangedEventHandler", StringComparison.Ordinal),
                "Collections view models should use the shared observable page-view-model adapter instead of local event plumbing.");
            Assert.IsFalse(
                source.Contains("private void OnPropertyChanged", StringComparison.Ordinal),
                "Collections view models should use the shared SetProperty adapter instead of local OnPropertyChanged plumbing.");
            Assert.IsFalse(
                source.Contains("get { return _listViewSelectionModeComboBoxSelectedIndex; }", StringComparison.Ordinal),
                "ListView selection-mode index should keep the official expression-bodied getter source shape.");
        }

        [TestMethod]
        public void CollectionsViewModelsKeepOfficialSampleGenerationSourceShape()
        {
            var source = ReadRepoFile(
                "ModernWpf.Gallery",
                "Pages",
                "WpfGallery",
                "Collections",
                "CollectionsPageViewModels.cs");

            AssertContainsInOrder(
                source,
                "public partial class DataGridPageViewModel : CollectionsPageViewModelBase",
                "private const int ProductsVisualTestSeed = 12043;",
                "private ObservableCollection<Product> GenerateProducts()",
                "var random = CreateSampleRandom(ProductsVisualTestSeed);",
                "var products = new ObservableCollection<Product> { };",
                "var adjectives = new[] { \"Red\", \"Blueberry\" };",
                "var names = new[] { \"Marmalade\", \"Dumplings\", \"Soup\" };",
                "//var units = new[] { \"grams\", \"kilograms\", \"milliliters\" };",
                "for (int i = 0; i < 50; i++)",
                "products.Add(",
                "new Product",
                "ProductName =",
                "adjectives[random.Next(0, adjectives.Length)]",
                "+ \" \"",
                "+ names[random.Next(0, names.Length)],",
                "UnitPrice = Math.Round(random.NextDouble() * 20.0, 3)",
                "return products;");
            AssertContainsInOrder(
                source,
                "public partial class ListViewPageViewModel : CollectionsPageViewModelBase",
                "private const int BasicListViewVisualTestSeed = 22043;",
                "private const int GridViewVisualTestSeed = 22044;",
                "private ObservableCollection<Person> GeneratePersons(int visualTestSeed)",
                "var random = CreateSampleRandom(visualTestSeed);",
                "var persons = new ObservableCollection<Person>();",
                "for (int i = 0; i < 50; i++)",
                "persons.Add(",
                "new Person(",
                "names[random.Next(0, names.Length)],",
                "surnames[random.Next(0, surnames.Length)],",
                "companies[random.Next(0, companies.Length)]",
                "return persons;");
        }

        [TestMethod]
        public void CopiedWpfGalleryModelClassesStayUnsealedLikeOfficialSource()
        {
            var repoRoot = GetRepoRoot();
            foreach (var file in new[]
            {
                new
                {
                    RelativePath = Path.Combine("ModernWpf.Gallery", "Models", "Product.cs"),
                    SealedDeclaration = "public sealed class Product",
                    UnsealedDeclaration = "public class Product"
                },
                new
                {
                    RelativePath = Path.Combine("ModernWpf.Gallery", "Models", "Person.cs"),
                    SealedDeclaration = "public sealed class Person",
                    UnsealedDeclaration = "public record Person"
                },
                new
                {
                    RelativePath = Path.Combine("ModernWpf.Gallery", "Pages", "WpfGallery", "DesignGuidance", "IconData.cs"),
                    SealedDeclaration = "public sealed class IconData",
                    UnsealedDeclaration = "public class IconData"
                },
                new
                {
                    RelativePath = Path.Combine("ModernWpf.Gallery", "Pages", "WpfGallery", "Samples", "UserDashboardUser.cs"),
                    SealedDeclaration = "public sealed class UserDashboardUser",
                    UnsealedDeclaration = "public class UserDashboardUser : INotifyPropertyChanged"
                }
            })
            {
                var path = Path.Combine(repoRoot, file.RelativePath);
                var source = File.ReadAllText(path);
                Assert.IsFalse(
                    source.Contains(file.SealedDeclaration, StringComparison.Ordinal),
                    Path.GetRelativePath(repoRoot, path) + " should match the official WPF Gallery unsealed model declaration shape.");
                Assert.IsTrue(
                    source.Contains(file.UnsealedDeclaration, StringComparison.Ordinal),
                    Path.GetRelativePath(repoRoot, path) + " should keep the copied WPF Gallery model declaration shape.");
            }
        }

        [TestMethod]
        public void CopiedWpfGalleryProductModelKeepsOfficialSummaryAndPlaceholderShape()
        {
            var source = ReadRepoFile(
                "ModernWpf.Gallery",
                "Models",
                "Product.cs");

            AssertContainsInOrder(
                source,
                "/// <summary>",
                "/// Product class for DataGrid page",
                "/// </summary>",
                "public class Product",
                "public int ProductId { get; set; }",
                "public int ProductCode { get; set; }",
                "public string ProductName { get; set; }",
                "public string QuantityPerUnit { get; set; }",
                "public double UnitPrice { get; set; }",
                "// public string UnitPriceString => UnitPrice.ToString(\"F2\");",
                "public int UnitsInStock { get; set; }",
                "// public bool IsVirtual { get; set; }");
        }

        [TestMethod]
        public void CopiedWpfGalleryPersonModelKeepsOfficialRecordAndInitShape()
        {
            var source = ReadRepoFile(
                "ModernWpf.Gallery",
                "Models",
                "Person.cs");

            Assert.IsTrue(
                source.Contains("public record Person", StringComparison.Ordinal),
                "Person should match the official WPF Gallery record model declaration shape.");
            Assert.IsFalse(
                source.Contains("public class Person", StringComparison.Ordinal),
                "Person should not drift back to a local-only class declaration.");
            AssertContainsInOrder(
                source,
                "/// <summary>",
                "/// Person class for User Dashboard page",
                "/// </summary>",
                "public record Person",
                "public string FirstName { get; init; }",
                "public string LastName { get; init; }",
                "public string Name => FirstName + \" \" + LastName;",
                "public string Company { get; init; }",
                "public Person(string firstName, string lastName, string company)");
        }

        [TestMethod]
        public void CopiedWpfGalleryIconDataModelKeepsOfficialPropertyAndGlyphShape()
        {
            var source = ReadRepoFile(
                "ModernWpf.Gallery",
                "Pages",
                "WpfGallery",
                "DesignGuidance",
                "IconData.cs");

            AssertContainsInOrder(
                source,
                "/// <summary>",
                "/// IconData class for icons in icon page",
                "/// </summary>",
                "public class IconData",
                "[DataMember]",
                "public string Name { get; set; }",
                "[DataMember]",
                "public string Code { get; set; }",
                "[DataMember]",
                "public List<string> Tags { get; set; } = [];",
                "public string Character => char.ConvertFromUtf32(Convert.ToInt32(Code, 16));",
                "public string CodeGlyph => \"\\\\x\" + Code;",
                "public string TextGlyph => \"&#x\" + Code + \";\";");
            Assert.IsFalse(
                source.Contains("catch (Exception)", StringComparison.Ordinal),
                "IconData.Character should keep the official expression-bodied glyph conversion shape.");
        }

        [TestMethod]
        public void CopiedWpfGalleryUserDashboardUserKeepsOfficialMemberOrderShape()
        {
            var source = ReadRepoFile(
                "ModernWpf.Gallery",
                "Pages",
                "WpfGallery",
                "Samples",
                "UserDashboardUser.cs");

            AssertContainsInOrder(
                source,
                "private string _firstName;",
                "private string _lastName;",
                "private string _company;",
                "private string _address;",
                "private bool _isNewGraduate;",
                "private string _imageId = \"91\";",
                "private int _age;",
                "private string _deletedname;",
                "private DateTime _dateOfJoining;",
                "public string Deletedname",
                "get => _deletedname;",
                "_deletedname = value;",
                "OnPropertyChanged(nameof(Deletedname));",
                "public string FirstName",
                "get => _firstName;",
                "public string LastName",
                "get => _lastName;",
                "public string Name => $\"{FirstName} {LastName}\";",
                "public string ImageId",
                "get => _imageId;",
                "public string ImageKey => $\"p{ImageId}\";",
                "public string Company",
                "get => _company;",
                "public string Address",
                "get => _address;",
                "public int Age",
                "get => _age;",
                "public DateTime DateOfJoining",
                "get => _dateOfJoining;",
                "public bool IsNewGraduate",
                "get => _isNewGraduate;",
                "public event PropertyChangedEventHandler PropertyChanged;",
                "protected void OnPropertyChanged(string propertyName)",
                "PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));",
                "public UserDashboardUser(string firstName, string lastName)",
                "FirstName = firstName;",
                "LastName = lastName;",
                "//public UserDashboardUser()",
                "//{",
                "//}",
                "public UserDashboardUser(UserDashboardUser user)",
                "ImageId = user.ImageId;",
                "FirstName = user.FirstName;",
                "LastName = user.LastName;",
                "Company = user.Company;",
                "Address = user.Address;",
                "Age = user.Age;",
                "DateOfJoining = user.DateOfJoining;",
                "IsNewGraduate = user.IsNewGraduate;",
                "public UserDashboardUser(string imageID, string firstName, string lastName, string company, string address, int age, DateTime doj, bool isNewGraduate = false)",
                "ImageId = imageID;",
                "FirstName = firstName;",
                "LastName = lastName;",
                "Company = company;",
                "Address = address;",
                "IsNewGraduate = isNewGraduate;",
                "Age = age;",
                "DateOfJoining = doj;");
            AssertContainsInOrder(
                source,
                "if (SetProperty(ref _firstName, value, nameof(FirstName)))",
                "OnPropertyChanged(nameof(Name));",
                "if (SetProperty(ref _lastName, value, nameof(LastName)))",
                "OnPropertyChanged(nameof(Name));",
                "if (SetProperty(ref _imageId, value, nameof(ImageId)))",
                "OnPropertyChanged(nameof(ImageKey));");
            Assert.IsFalse(
                source.Contains("get { return _firstName; }", StringComparison.Ordinal),
                "UserDashboardUser should keep the official expression-bodied getter source shape while retaining local SetProperty setters.");
        }

        [TestMethod]
        public void UserDashboardConvertersKeepOfficialVisibilityAndImageBrushSourceShape()
        {
            var source = ReadRepoFile(
                "ModernWpf.Gallery",
                "Pages",
                "WpfGallery",
                "Samples",
                "UserDashboardConverters.cs");

            AssertContainsInOrder(
                source,
                "/// Converts an empty string to Visibility.Collapsed",
                "public sealed class EmptyToVisibilityConverter : IValueConverter",
                "public object Convert(object value, Type targetType, object parameter, CultureInfo culture)",
                "if (value is string str)",
                "return string.IsNullOrWhiteSpace(str) ? Visibility.Collapsed : Visibility.Visible;",
                "return value is null ? Visibility.Collapsed : Visibility.Visible;",
                "public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)",
                "throw new NotImplementedException();",
                "/// Converts an image id to a brush",
                "public sealed class ImageIdToBrushConverter : IValueConverter",
                "public object Convert(object value, Type targetType, object parameter, CultureInfo culture)",
                "string imageKey = value as string;",
                "if (string.IsNullOrEmpty(imageKey))",
                "imageKey = \"p91\";",
                "else if (imageKey[0] != 'p' && imageKey[0] != 'P')",
                "imageKey = \"p\" + imageKey;",
                "return Application.Current.Resources[imageKey];",
                "public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)",
                "throw new NotImplementedException();");

            Assert.IsFalse(
                source.Contains("var imageKey = value as string;", StringComparison.Ordinal),
                "ImageIdToBrushConverter should keep the official local-variable type shape while retaining the local ImageKey fallback adapter.");
        }

        [TestMethod]
        public void UserDashboardViewModelKeepsOfficialObservableStateSourceShape()
        {
            var source = ReadRepoFile(
                "ModernWpf.Gallery",
                "Pages",
                "WpfGallery",
                "Samples",
                "UserDashboardPageViewModel.cs")
                .Replace("\r\n", "\n")
                .Replace('\r', '\n');

            AssertContainsInOrder(
                source,
                "public partial class UserDashboardPageViewModel : WpfGalleryObservableObject",
                "private const int UsersVisualTestSeed = 32043;",
                "private ObservableCollection<UserDashboardUser> _users;",
                "private UserDashboardUser _selectedUser;",
                "private bool _isEditing;",
                "private UserDashboardUser _editableUser;",
                "private bool _isReadOnly = true;",
                "private bool _isSaved;",
                "private string _deletedName = string.Empty;",
                "private readonly RelayCommand _addUserCommand;",
                "private readonly DispatcherTimer _deletedMessageTimer;",
                "public UserDashboardPageViewModel()",
                "Users = GenerateUsers();",
                "_addUserCommand = new RelayCommand(delegate { AddUser(); });",
                "_deletedMessageTimer = CreateMessageTimer(delegate { DeletedName = string.Empty; });",
                "public string DeletedName",
                "if (!SetProperty(ref _deletedName, value, \"DeletedName\"))",
                "return;",
                "if (string.IsNullOrEmpty(value))",
                "return;",
                "RestartTimer(_deletedMessageTimer);",
                "public UserDashboardUser EditableUser",
                "set { SetProperty(ref _editableUser, value, \"EditableUser\"); }",
                "public bool IsEditing",
                "set { SetProperty(ref _isEditing, value, \"IsEditing\"); }",
                "public bool IsReadOnly",
                "set { SetProperty(ref _isReadOnly, value, \"IsReadOnly\"); }",
                "public bool IsSaved",
                "set { SetProperty(ref _isSaved, value, \"IsSaved\"); }",
                "public UserDashboardUser SelectedUser",
                "if (SetProperty(ref _selectedUser, value, \"SelectedUser\") && value != null && value != EditableUser)",
                "public ObservableCollection<UserDashboardUser> Users",
                "set { SetProperty(ref _users, value, \"Users\"); }",
                "\"Suite 92, 9 Hermina Point, Bakersfield, United States\",\n                \"\"",
                "Users.Add(new UserDashboardUser(\"New User\", \"\"));",
                "SelectedUser = Users.Last();",
                "private void EditUserCancel()",
                "EditableUser = null;",
                "if (SelectedUser != null)",
                "EditableUser = new UserDashboardUser(SelectedUser);",
                "private void EditUserCommit()",
                "if (EditableUser != null && SelectedUser != null)",
                "int index = Users.IndexOf(SelectedUser);",
                "IsSaved = true;",
                "RestartTimer(_savedMessageTimer);",
                "private void EditUserStart()",
                "if (SelectedUser != null)",
                "EditableUser = new UserDashboardUser(SelectedUser);",
                "private void RemoveUser(UserDashboardUser selectedUser)",
                "int index = Users.Last().Equals(selectedUser) ?\n                Users.IndexOf(selectedUser) - 1 :\n                Users.IndexOf(selectedUser) + 1;",
                "SelectedUser = index >= 0 && index < Users.Count ?\n                           Users[index] :\n                           null;");
            Assert.IsFalse(
                source.Contains("public event PropertyChangedEventHandler", StringComparison.Ordinal),
                "UserDashboardPageViewModel should use the shared observable adapter instead of local notification plumbing.");
            Assert.IsFalse(
                source.Contains("private void OnPropertyChanged", StringComparison.Ordinal),
                "UserDashboardPageViewModel should keep OnPropertyChanged on the shared observable adapter.");
            Assert.IsFalse(
                source.Contains("private bool SetProperty", StringComparison.Ordinal),
                "UserDashboardPageViewModel should keep SetProperty on the shared observable adapter.");
        }

        [TestMethod]
        public void TopLevelCodeBehindKeepsOfficialViewModelMemberOrderShape()
        {
            foreach (var page in new[]
            {
                Tuple.Create("AllSamplesPage", "AllSamplesPageViewModel"),
                Tuple.Create("WhatsNewPage", "WhatsNewPageViewModel"),
                Tuple.Create("SettingsPage", "SettingsPageViewModel")
            })
            {
                var source = ReadRepoFile(
                    "ModernWpf.Gallery",
                    "Pages",
                    page.Item1 + ".xaml.cs");
                var viewModelIndex = source.IndexOf(
                    "public " + page.Item2 + " ViewModel { get; }",
                    StringComparison.Ordinal);
                var constructorIndex = source.IndexOf(
                    "public " + page.Item1 + "(",
                    StringComparison.Ordinal);

                Assert.IsTrue(viewModelIndex >= 0, page.Item1 + " should expose its copied page-specific ViewModel property.");
                Assert.IsTrue(constructorIndex >= 0, page.Item1 + " should keep its copied view-model constructor.");
                Assert.IsTrue(
                    viewModelIndex < constructorIndex,
                    page.Item1 + " should match the official WPF Gallery top-level code-behind member order by declaring ViewModel before the copied constructor.");
            }

            var homeSource = ReadRepoFile(
                "ModernWpf.Gallery",
                "Pages",
                "DashboardPage.xaml.cs").Replace("\r\n", "\n").Replace('\r', '\n');
            AssertContainsInOrder(
                homeSource,
                "public DashboardPage(DashboardPageViewModel viewModel)",
                "            InitializeComponent();\n            ViewModel = viewModel ?? new DashboardPageViewModel(OnNavigateCard);\n            DataContext = this;\n        }\n\n        public DashboardPageViewModel ViewModel { get; }\n\n        public Action<GalleryItem> ItemRequested { get; set; }");
        }

        [TestMethod]
        public void ShellChromeKeepsWinUITitleBarAndWpfContentSourceShape()
        {
            var mainWindowXaml = ReadRepoFile(
                "ModernWpf.Gallery",
                "MainWindow.xaml");

            AssertContainsInOrder(
                mainWindowXaml,
                "x:Key=\"BorderlessButtonStyle\"",
                "<MultiDataTrigger>",
                "<Condition Binding=\"{Binding Path=(SystemParameters.HighContrast)}\" Value=\"True\" />",
                "<Condition Binding=\"{Binding IsMouseOver, RelativeSource={RelativeSource Mode=Self}}\" Value=\"True\" />",
                "<Setter Property=\"Background\" Value=\"{DynamicResource SystemColorHighlightColorBrush}\" />",
                "<Setter Property=\"Foreground\" Value=\"{DynamicResource SystemColorHighlightTextColorBrush}\" />");
            AssertContainsInOrder(
                mainWindowXaml,
                "<Style x:Key=\"TitleBarDefaultButtonStyle\" BasedOn=\"{StaticResource BorderlessButtonStyle}\" TargetType=\"Button\">",
                "<Setter Property=\"WindowChrome.IsHitTestVisibleInChrome\" Value=\"True\" />",
                "<Setter Property=\"Border.CornerRadius\" Value=\"0\" />",
                "<Setter Property=\"Height\" Value=\"48\" />",
                "<Setter Property=\"MinHeight\" Value=\"48\" />",
                "<Style x:Key=\"WinUITitleBarNavigationButtonStyle\" BasedOn=\"{StaticResource BorderlessButtonStyle}\" TargetType=\"Button\">",
                "<Setter Property=\"WindowChrome.IsHitTestVisibleInChrome\" Value=\"True\" />",
                "<Setter Property=\"Width\" Value=\"40\" />",
                "<Setter Property=\"MinWidth\" Value=\"40\" />",
                "<Setter Property=\"Margin\" Value=\"2\" />",
                "<Setter Property=\"Padding\" Value=\"0\" />",
                "<Setter Property=\"VerticalAlignment\" Value=\"Stretch\" />",
                "<Setter Property=\"HorizontalContentAlignment\" Value=\"Center\" />",
                "<Setter Property=\"VerticalContentAlignment\" Value=\"Center\" />",
                "<Setter Property=\"Background\" Value=\"{DynamicResource SubtleFillColorTransparentBrush}\" />",
                "<Setter Property=\"Border.CornerRadius\" Value=\"{StaticResource ControlCornerRadius}\" />",
                "<Setter Property=\"FontFamily\" Value=\"{StaticResource SymbolThemeFontFamily}\" />",
                "<Setter Property=\"FontSize\" Value=\"16\" />",
                "<Setter Property=\"RenderTransform\">",
                "<TranslateTransform Y=\"1\" />",
                "<Setter Property=\"Background\" Value=\"{DynamicResource SubtleFillColorSecondaryBrush}\" />",
                "<Setter Property=\"Background\" Value=\"{DynamicResource SubtleFillColorTertiaryBrush}\" />",
                "<Style x:Key=\"TitleBarDefaultCloseButtonStyle\" BasedOn=\"{StaticResource TitleBarDefaultButtonStyle}\" TargetType=\"Button\">");
            AssertContainsInOrder(
                mainWindowXaml,
                "x:Key=\"TitleBarDefaultCloseButtonStyle\"",
                "<MultiDataTrigger>",
                "<Condition Binding=\"{Binding Path=(SystemParameters.HighContrast)}\" Value=\"True\" />",
                "<Condition Binding=\"{Binding IsMouseOver, RelativeSource={RelativeSource Mode=Self}}\" Value=\"True\" />",
                "<Setter Property=\"Background\" Value=\"{DynamicResource SystemColorHighlightColorBrush}\" />",
                "<Setter Property=\"Foreground\" Value=\"{DynamicResource SystemColorHighlightTextColorBrush}\" />");
            AssertContainsInOrder(
                mainWindowXaml,
                "x:Name=\"HighContrastBorder\"",
                "BorderBrush=\"Transparent\"",
                "BorderThickness=\"8 1 8 8\"",
                "<Grid x:Name=\"MainGrid\">",
                "<RowDefinition Height=\"48\" />",
                "Grid.Row=\"0\"",
                "Grid.ColumnSpan=\"2\"",
                "Height=\"48\"");
            Assert.IsFalse(
                mainWindowXaml.Contains("Background=\"{DynamicResource WindowBackground}\"", StringComparison.Ordinal),
                "MainWindow should keep the official WPF Gallery source shape by applying WindowBackground from code-behind instead of the Window root declaration.");
            AssertContainsInOrder(
                mainWindowXaml,
                "x:Name=\"BackButton\"",
                "Grid.Column=\"1\"",
                "AutomationProperties.Name=\"Back\"",
                "Style=\"{StaticResource WinUITitleBarNavigationButtonStyle}\"",
                "Command=\"{Binding ViewModel.BackCommand}\"",
                "IsEnabled=\"{Binding ViewModel.CanNavigateback}\"",
                "Visibility=\"{Binding ViewModel.CanNavigateback, Converter={StaticResource BooleanToVisibilityConverter}}\"",
                "WindowChrome.IsHitTestVisibleInChrome=\"True\"",
                "ToolTipService.ToolTip=\"Back\"");
            Assert.IsFalse(
                mainWindowXaml.Contains("winShell:WindowChrome.IsHitTestVisibleInChrome", StringComparison.Ordinal),
                "The title-bar hit-test attached property should keep the official WPF Gallery WindowChrome source shape without a local XML namespace prefix.");
            AssertContainsInOrder(
                mainWindowXaml,
                "Text=\"&#xE72B;\"",
                "x:Name=\"PaneToggleButton\"",
                "Grid.Column=\"2\"",
                "AutomationProperties.Name=\"Navigation\"",
                "Click=\"ToggleNavigationPane\"",
                "Style=\"{StaticResource WinUITitleBarNavigationButtonStyle}\"",
                "Text=\"&#xE700;\"",
                "Width=\"16\"",
                "Height=\"16\"",
                "Margin=\"0,0,16,0\"",
                "Style=\"{StaticResource CaptionTextBlockStyle}\"",
                "pages:GalleryAutomation.HeadingLevel=\"Level1\"",
                "Text=\"{Binding ViewModel.ApplicationTitle}\"",
                "<ui:AutoSuggestBox",
                "x:Name=\"ControlsSearchBox\"",
                "Grid.Column=\"5\"",
                "Width=\"320\"",
                "Height=\"32\"",
                "MinWidth=\"160\"",
                "MaxWidth=\"580\"",
                "Margin=\"20,0,0,0\"",
                "AutomationProperties.AutomationId=\"ControlsSearchBox\"",
                "PlaceholderText=\"Search controls and samples...\"",
                "QueryIcon=\"Find\"",
                "QuerySubmitted=\"OnSearchQuerySubmitted\"",
                "TextChanged=\"OnSearchTextChanged\"",
                "WindowChrome.IsHitTestVisibleInChrome=\"True\"");
            AssertContainsInOrder(
                mainWindowXaml,
                "<galleryShell:NavigationRootPage Grid.Row=\"1\" />");
            Assert.IsFalse(
                mainWindowXaml.Contains("x:Name=\"RootPage\"", StringComparison.Ordinal),
                "The retained NavigationRootPage host should be located structurally instead of by a local-only MainWindow name hook.");

            var navigationRootXaml = ReadRepoFile(
                "ModernWpf.Gallery",
                "Shell",
                "NavigationRootPage.xaml");

            AssertContainsInOrder(
                navigationRootXaml,
                "<StackPanel",
                "Width=\"1\"",
                "Height=\"1\"",
                "HorizontalAlignment=\"Left\"",
                "VerticalAlignment=\"Top\"",
                "Focusable=\"False\"",
                "IsHitTestVisible=\"False\"",
                "Opacity=\"0\"",
                "AutomationProperties.AutomationId=\"GalleryVisualTestCurrentRoute\"",
                "AutomationProperties.AutomationId=\"GalleryVisualTestReadyState\"",
                "AutomationProperties.AutomationId=\"GalleryVisualTestLastException\"",
                "AutomationProperties.AutomationId=\"GalleryVisualTestRefreshArtifacts\"",
                "Click=\"OnVisualTestRefreshArtifactsClick\"");
            AssertContainsInOrder(
                navigationRootXaml,
                "<ui:NavigationView",
                "AutomationProperties.Name=\"Navigation Pane\"",
                "IsBackButtonVisible=\"Collapsed\"",
                "IsPaneToggleButtonVisible=\"False\"",
                "IsSettingsVisible=\"True\"",
                "IsTabStop=\"False\"",
                "IsTitleBarAutoPaddingEnabled=\"False\"",
                "ItemInvoked=\"OnNavigationItemInvoked\"",
                "PaneDisplayMode=\"Auto\"");
            Assert.IsFalse(
                navigationRootXaml.Contains("x:Name=\"Navigation\"", StringComparison.Ordinal),
                "The retained shell NavigationView should be located structurally instead of by a local-only NavigationRootPage name hook.");
            Assert.IsFalse(
                navigationRootXaml.Contains("<ui:NavigationView.PaneFooter>", StringComparison.Ordinal),
                "The WinUI shell should use NavigationView's native Settings item.");
            Assert.IsFalse(
                navigationRootXaml.Contains("SettingsButton", StringComparison.Ordinal),
                "The WinUI shell should not keep the retired custom Settings footer.");
            Assert.IsFalse(
                navigationRootXaml.Contains("NavigationRootDataContextProxy", StringComparison.Ordinal),
                "The native Settings item should not require the retired binding proxy.");
            AssertContainsInOrder(
                navigationRootXaml,
                "PaneDisplayMode=\"Auto\"",
                "<ui:NavigationView.Resources>",
                "<Thickness x:Key=\"NavigationViewContentGridBorderThickness\">0</Thickness>",
                "<Thickness x:Key=\"NavigationViewPaneContentGridMargin\">0,4</Thickness>",
                "<CornerRadius x:Key=\"NavigationViewContentGridCornerRadius\">0</CornerRadius>",
                "<SolidColorBrush x:Key=\"NavigationViewContentBackground\" Color=\"Transparent\" />",
                "<Border",
                "Margin=\"4,0,0,0\"",
                "Padding=\"24,16,24,0\"",
                "Background=\"{DynamicResource LayerFillColorDefaultBrush}\"",
                "BorderBrush=\"{DynamicResource CardStrokeColorDefaultBrush}\"",
                "BorderThickness=\"1\"",
                "CornerRadius=\"8,0,0,0\"",
                "<Frame NavigationUIVisibility=\"Hidden\" />");
            Assert.IsFalse(
                navigationRootXaml.Contains("x:Name=\"ContentHost\"", StringComparison.Ordinal),
                "The retained shell content Frame should be located structurally instead of by a local-only NavigationRootPage name hook.");
            Assert.IsFalse(
                navigationRootXaml.Contains("HighContrastNavigationPaneEdgeCover", StringComparison.Ordinal),
                "The native WinUI pane should not need the retired manual edge cover.");

            var mainWindowCode = ReadRepoFile(
                "ModernWpf.Gallery",
                "MainWindow.xaml.cs");

            AssertContainsInOrder(
                mainWindowCode,
                "using System.Linq;",
                "using ModernWpf.Gallery.Shell;",
                "ViewModel = new MainWindowViewModel(GoBack, OpenSettings, GoForward, CanGoBack);",
                "private void GoBack()",
                "GetNavigationRootPage().GoBack();",
                "private void GoForward()",
                "GetNavigationRootPage().GoForward();",
                "private void OpenSettings()",
                "GetNavigationRootPage().OpenSettings();",
                "private void ToggleNavigationPane(object sender, RoutedEventArgs e)",
                "GetNavigationRootPage().ToggleNavigationPane();",
                "private void OnSearchTextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)",
                "GetNavigationRootPage().OnSearchTextChanged(sender, args);",
                "private void OnSearchQuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)",
                "GetNavigationRootPage().OnSearchQuerySubmitted(sender, args);",
                "internal void UpdateCanNavigateBack()",
                "ViewModel.UpdateCanNavigateBack();",
                "internal void NavigateTo(string uniqueId)",
                "GetNavigationRootPage().NavigateTo(uniqueId);",
                "private bool CanGoBack()",
                "return GetNavigationRootPage().CanGoBack;",
                "private NavigationRootPage GetNavigationRootPage()",
                "return MainGrid.Children.OfType<NavigationRootPage>().Single();");
            AssertContainsInOrder(
                mainWindowCode,
                "/// Interaction logic for MainWindow.xaml",
                "InitializeComponent();",
                "UpdateWindowBackground();",
                "ConfigureWindowChrome();",
                "UpdateMainWindowVisuals();",
                "SystemEvents.UserPreferenceChanged += SystemEvents_UserPreferenceChanged;",
                "StateChanged += OnWindowStateChanged;",
                "Activated += OnWindowActivationChanged;",
                "Deactivated += OnWindowActivationChanged;",
                "private void UpdateWindowBackground()",
                "SetResourceReference(BackgroundProperty, \"WindowBackground\");",
                "private void SystemEvents_UserPreferenceChanged(object sender, UserPreferenceChangedEventArgs e)",
                "Dispatcher.Invoke(() =>",
                "UpdateMainWindowVisuals();",
                "SystemEvents.UserPreferenceChanged -= SystemEvents_UserPreferenceChanged;",
                "MainGrid.Margin = GetMainGridMargin(WindowState, SystemParameters.HighContrast);",
                "UpdateTitleBarButtonsVisibility();",
                "if (SystemParameters.HighContrast)",
                "HighContrastBorder.SetResourceReference(",
                "System.Windows.Controls.Border.BorderBrushProperty,",
                "IsActive ? SystemColors.ActiveCaptionBrushKey : SystemColors.InactiveCaptionBrushKey);",
                "HighContrastBorder.BorderThickness = GetHighContrastBorderThickness(SystemParameters.HighContrast);",
                "chrome.NonClientFrameEdges = GetPrefferedNonClientFrameEdges();",
                "NonClientFrameEdges = GetPrefferedNonClientFrameEdges()",
                "return isHighContrast ? new Thickness(8, 1, 8, 8) : new Thickness(0);",
                "internal static NonClientFrameEdges GetPrefferedNonClientFrameEdges()",
                "if (isHighContrast || !isWindows11OrGreater)");

            var navigationRootCode = ReadRepoFile(
                "ModernWpf.Gallery",
                "Shell",
                "NavigationRootPage.xaml.cs");

            AssertContainsInOrder(
                navigationRootCode,
                "public bool CanGoBack",
                "get { return _backStack.Count > 0; }",
                "var canGoBack = CanGoBack;",
                "GetNavigationView().IsBackEnabled = canGoBack;",
                "window.UpdateCanNavigateBack();");
            AssertContainsInOrder(
                navigationRootCode,
                "private void BuildNavigationMenu()",
                "navigation.MenuItems.Add(_homeNavigationItem);",
                "navigation.MenuItems.Add(_whatsNewNavigationItem);",
                "private NavigationViewItem CreateNavigationItem(string title, NavigationTarget target, IconElement icon)",
                "Content = title,",
                "Icon = icon,",
                "Tag = target",
                "internal void OpenSettings()",
                "Navigate(NavigationTarget.Settings(), true);",
                "internal void ToggleNavigationPane()",
                "navigation.IsPaneOpen = !navigation.IsPaneOpen;",
                "private void OnNavigationItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)",
                "if (args.IsSettingsInvoked)",
                "Navigate(NavigationTarget.Settings(), true);",
                "internal void OnSearchTextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)",
                "internal void OnSearchQuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)");
            AssertContainsInOrder(
                navigationRootCode,
                "AutomationProperties.SetAutomationId(GetNavigationView(), \"GalleryNavigationView\");",
                "AutomationProperties.SetAutomationId(GetContentHost(), \"GalleryContentHost\");",
                "var contentHost = GetContentHost();",
                "contentHost.Content = CreatePage(target);",
                "contentHost.UpdateLayout();",
                "GalleryDiagnostics.PrepareInteractiveVisualState(contentHost);",
                "contentHost.UpdateLayout();",
                "private System.Windows.Controls.Frame GetContentHost()",
                "var contentBorder = (Border)GetNavigationView().Content;",
                "return (System.Windows.Controls.Frame)contentBorder.Child;",
                "private NavigationView GetNavigationView()",
                "var root = (Grid)Content;",
                "return root.Children.OfType<NavigationView>().Single();");
            Assert.IsFalse(
                navigationRootCode.Contains("AlignNavigationViewShellResourcesWithWpfGallery", StringComparison.Ordinal),
                "The WinUI shell should use native NavigationView theme resources.");
            Assert.IsFalse(
                navigationRootCode.Contains("GetHighContrastNavigationPaneEdgeCover", StringComparison.Ordinal),
                "The WinUI shell should not keep the retired manual pane edge cover.");
            Assert.IsFalse(
                navigationRootCode.Contains("SettingsButton_Click", StringComparison.Ordinal),
                "The WinUI shell should route the native Settings item through ItemInvoked.");
            AssertContainsInOrder(
                navigationRootCode,
                "GetVisualTestStatusPanel().Visibility = GalleryDiagnostics.IsEnabled",
                "private StackPanel GetVisualTestStatusPanel()",
                "var root = (Grid)Content;",
                "return root.Children.OfType<StackPanel>().Single();",
                "private TextBlock GetVisualTestStatusText(string automationId)",
                "AutomationProperties.GetAutomationId(text)",
                "SetVisualTestState(string route, string readyState)",
                "GetVisualTestStatusText(\"GalleryVisualTestCurrentRoute\").Text = GalleryDiagnostics.CurrentRoute;",
                "GetVisualTestStatusText(\"GalleryVisualTestReadyState\").Text = GalleryDiagnostics.ReadyState;",
                "GetVisualTestStatusText(\"GalleryVisualTestLastException\").Text = GalleryDiagnostics.LastException;",
                "private void OnVisualTestRefreshArtifactsClick(object sender, RoutedEventArgs e)",
                "GalleryDiagnostics.WriteVisualArtifacts(Window.GetWindow(this) ?? (DependencyObject)this)",
                "GalleryDiagnostics.WriteStatusFile();");

            var appCode = ReadRepoFile(
                "ModernWpf.Gallery",
                "App.xaml.cs");

            AssertContainsInOrder(
                appCode,
                "/// Interaction logic for App.xaml",
                "protected override void OnStartup(StartupEventArgs e)",
                "ApplyTheme(options.Theme);",
                "var window = new MainWindow();");
        }

        [TestMethod]
        public void GalleryManifestKeepsWpfGalleryRuntimeCompatibilityShape()
        {
            var manifest = ReadRepoFile(
                "ModernWpf.Gallery",
                "app.manifest");

            AssertContainsInOrder(
                manifest,
                "<requestedExecutionLevel level=\"asInvoker\" uiAccess=\"false\" />",
                "<supportedOS Id=\"{35138b9a-5d96-4fbd-8e2d-a2440225f93a}\" />",
                "<supportedOS Id=\"{4a2f28e3-53b9-4441-ba9c-d69d4a4a6e38}\" />",
                "<supportedOS Id=\"{1f676c76-80e1-4239-95bb-83d0f6d0da78}\" />",
                "<supportedOS Id=\"{8e0f7a12-bfb3-4fe8-b9a5-48fd50a15a9a}\" />",
                "<dpiAwareness xmlns=\"http://schemas.microsoft.com/SMI/2016/WindowsSettings\">PerMonitorV2</dpiAwareness>",
                "<dpiAware xmlns=\"http://schemas.microsoft.com/SMI/2005/WindowsSettings\">true/PM</dpiAware>",
                "<longPathAware xmlns=\"http://schemas.microsoft.com/SMI/2016/WindowsSettings\">true</longPathAware>",
                "<dependency>",
                "<dependentAssembly>",
                "type=\"win32\"",
                "name=\"Microsoft.Windows.Common-Controls\"",
                "version=\"6.0.0.0\"",
                "processorArchitecture=\"*\"",
                "publicKeyToken=\"6595b64144ccf1df\"",
                "language=\"*\" />");
            Assert.IsFalse(
                manifest.Contains(">true</dpiAware>", StringComparison.Ordinal),
                "The Gallery manifest should keep the official WPF Gallery true/PM DPI fallback instead of the local system-DPI fallback.");
        }

        [TestMethod]
        public void GalleryUsesModernWpfOwnedWindowIdentity()
        {
            var project = ReadRepoFile(
                "ModernWpf.Gallery",
                "ModernWpf.Gallery.csproj");
            var appXaml = ReadRepoFile(
                "ModernWpf.Gallery",
                "App.xaml");
            var mainWindowXaml = ReadRepoFile(
                "ModernWpf.Gallery",
                "MainWindow.xaml");

            AssertContainsInOrder(
                project,
                "<AssemblyTitle>$(ModernWpfDisplayName)</AssemblyTitle>",
                "<Description>Control gallery for ModernWPF.</Description>");
            Assert.IsFalse(
                project.Contains("WPFGallery", StringComparison.OrdinalIgnoreCase),
                "The Gallery project must not use the official WPF Gallery icon.");
            StringAssert.Contains(appXaml, "<DrawingImage x:Key=\"ModernWpfLogoImage\">");
            StringAssert.Contains(mainWindowXaml, "Icon=\"{StaticResource ModernWpfLogoImage}\"");
            Assert.IsFalse(
                mainWindowXaml.Contains("Assets/AppIcons", StringComparison.OrdinalIgnoreCase),
                "The Gallery title bar must use the shared ModernWPF-owned mark.");
        }

        [TestMethod]
        public void GalleryBrandingUsesAssemblyTitleAndSharedApplicationResources()
        {
            var source = ReadRepoFile(
                "ModernWpf.Gallery",
                "GalleryBranding.cs");
            var appXaml = ReadRepoFile(
                "ModernWpf.Gallery",
                "App.xaml");

            AssertContainsInOrder(
                source,
                "public static string DisplayName { get; } = GetAssemblyTitle();",
                "public static string BrandName { get; } = GetBrandName(DisplayName);",
                "public static string PreviewDisplayName { get; } = DisplayName + \" Preview\";",
                "public static string ControlsGroupTitle { get; } = BrandName + \" controls\";");
            AssertContainsInOrder(
                appXaml,
                "<x:Static x:Key=\"GalleryDisplayName\" Member=\"local:GalleryBranding.DisplayName\" />",
                "<x:Static x:Key=\"GalleryPreviewDisplayName\" Member=\"local:GalleryBranding.PreviewDisplayName\" />",
                "<x:Static x:Key=\"GalleryVersionDisplay\" Member=\"local:GalleryBranding.VersionDisplay\" />",
                "<x:Static x:Key=\"GalleryCopyrightNotice\" Member=\"local:GalleryBranding.CopyrightNotice\" />",
                "<x:Static x:Key=\"GalleryCloneCommand\" Member=\"local:GalleryBranding.CloneCommand\" />",
                "<x:Static x:Key=\"GalleryWhatsNewPageTitle\" Member=\"local:GalleryBranding.WhatsNewPageTitle\" />",
                "<x:Static x:Key=\"GalleryNewSamplesAutomationName\" Member=\"local:GalleryBranding.NewSamplesAutomationName\" />");
        }

        [TestMethod]
        public void MainWindowViewModelKeepsWpfGalleryCommandHandlerSourceShape()
        {
            var source = ReadRepoFile(
                "ModernWpf.Gallery",
                "ViewModels",
                "MainWindowViewModel.cs");

            AssertContainsInOrder(
                source,
                "private string _applicationTitle = GalleryBranding.DisplayName;",
                "private readonly Action _backAction;",
                "private readonly Action _settingsAction;",
                "private readonly Action _forwardAction;",
                "private readonly Func<bool> _canNavigateBack;",
                "_backAction = backAction;",
                "_settingsAction = settingsAction;",
                "_forwardAction = forwardAction;",
                "_canNavigateBack = canNavigateBack;",
                "BackCommand = new RelayCommand(delegate { Back(); });",
                "SettingsCommand = new RelayCommand(delegate { Settings(); });",
                "ForwardCommand = new RelayCommand(delegate { Forward(); });",
                "public void Back()",
                "_backAction();",
                "public void Settings()",
                "_settingsAction();",
                "public void Forward()",
                "_forwardAction();",
                "public void UpdateCanNavigateBack()",
                "CanNavigateback = _canNavigateBack();");
            AssertContainsInOrder(
                source,
                "public string ApplicationTitle",
                "get { return _applicationTitle; }");
            Assert.IsFalse(
                source.Contains("delegate { backAction(); }", StringComparison.Ordinal),
                "BackCommand should route through the retained official Back command-handler name instead of calling the constructor adapter directly.");
            Assert.IsFalse(
                source.Contains("delegate { settingsAction(); }", StringComparison.Ordinal),
                "SettingsCommand should route through the retained official Settings command-handler name instead of calling the constructor adapter directly.");
            Assert.IsFalse(
                source.Contains("delegate { forwardAction(); }", StringComparison.Ordinal),
                "ForwardCommand should route through the retained official Forward command-handler name instead of calling the constructor adapter directly.");
            Assert.IsFalse(
                source.Contains("CanNavigateback = canGoBack", StringComparison.Ordinal),
                "Back state should update through the retained official UpdateCanNavigateBack source hook instead of a local direct setter adapter.");
        }

        [TestMethod]
        public void ItemPageWrapperAvoidsLocalOnlyAutomationHooks()
        {
            var xaml = ReadRepoFile(
                "ModernWpf.Gallery",
                "Pages",
                "ItemPage.xaml");

            Assert.IsFalse(
                xaml.Contains("x:Name=\"PageHeader\"", StringComparison.Ordinal),
                "The generic wrapper header should be located structurally instead of by a local-only name.");
            Assert.IsFalse(
                xaml.Contains("AutomationProperties.AutomationId=\"GallerySampleHost\"", StringComparison.Ordinal),
                "The generic wrapper should not expose the local-only GallerySampleHost automation ID.");
            Assert.IsFalse(
                xaml.Contains("x:Name=\"DirectPageContentHost\"", StringComparison.Ordinal),
                "The direct-page wrapper frame should be located structurally instead of by a local-only name.");
            AssertContainsInOrder(
                xaml,
                "<ItemsControl",
                "ItemsSource=\"{Binding Examples}\"",
                "<controls:ControlExample",
                "HeaderText=\"{Binding HeaderText}\"",
                "XamlCode=\"{Binding XamlCode}\"",
                "CSharpCode=\"{Binding CSharpCode}\"",
                "ExampleContent=\"{Binding ExampleContent}\"",
                "OptionsContent=\"{Binding OptionsContent}\"",
                "Margin=\"{Binding Margin}\" />");
        }

        [TestMethod]
        public void ActiveGalleryXamlAvoidsLocalOnlyAutomationHooks()
        {
            var repoRoot = GetRepoRoot();
            var activeXamlRoots = new[]
            {
                Path.Combine(repoRoot, "ModernWpf.Gallery", "Controls"),
                Path.Combine(repoRoot, "ModernWpf.Gallery", "Pages"),
                Path.Combine(repoRoot, "ModernWpf.Gallery", "Resources")
            };
            var activeXamlFiles = activeXamlRoots
                .SelectMany(root => Directory.EnumerateFiles(root, "*.xaml", SearchOption.AllDirectories))
                .Concat(new[] { Path.Combine(repoRoot, "ModernWpf.Gallery", "MainWindow.xaml") });
            var allowedActiveAutomationIdAssignments = new[]
            {
                @"ModernWpf.Gallery\MainWindow.xaml: AutomationProperties.AutomationId=""ControlsSearchBox""",
                @"ModernWpf.Gallery\Pages\ItemPage.xaml: AutomationProperties.AutomationId=""GalleryItemPageScrollViewer""",
                @"ModernWpf.Gallery\Pages\ItemPage.xaml: AutomationProperties.AutomationId=""{Binding AutomationId}"""
            };
            var shellXamlFiles = Directory.EnumerateFiles(
                Path.Combine(repoRoot, "ModernWpf.Gallery", "Shell"),
                "*.xaml",
                SearchOption.AllDirectories);
            var forbiddenSnippets = new[]
            {
                "x:Name=\"ContentRootGrid\"",
                "x:Name=\"GallerySampleHost\"",
                "x:Name=\"AllControlsItemsControl\"",
                "x:Name=\"GroupItemsControl\"",
                "AutomationProperties.Name=\"GalleryItemPageTitle\"",
                "GalleryNav_",
                "ModernWpfGroupScrollViewer",
                "ModernWpfGroupItemsControl",
                "ContentFrameBorder",
                "HighContrastNavigationPaneEdgeCover",
                "VisualTestStatusPanel",
                "VisualTestCurrentRouteText",
                "VisualTestReadyStateText",
                "VisualTestLastExceptionText",
                "x:Name=\"RootPage\"",
                "x:Name=\"ContentHost\"",
                "x:Name=\"Navigation\"",
                "ModernWpfGalleryMainWindow",
                "GalleryNavigationRoot",
                "GalleryNavigationView",
                "GalleryContentHost",
                "SettingsIcon"
            };
            var shellForbiddenSnippets = forbiddenSnippets
                .Where(snippet => !string.Equals(snippet, "AutomationProperties.AutomationId=", StringComparison.Ordinal))
                .ToArray();
            var violations = activeXamlFiles
                .SelectMany(path =>
                {
                    var source = File.ReadAllText(path);
                    var forbidden = forbiddenSnippets
                        .Where(snippet => source.Contains(snippet, StringComparison.Ordinal))
                        .Select(snippet => Path.GetRelativePath(repoRoot, path) + ": " + snippet);
                    var unapprovedAutomationIds = File.ReadLines(path)
                        .Where(line => line.Contains("AutomationProperties.AutomationId=", StringComparison.Ordinal))
                        .Select(line => Path.GetRelativePath(repoRoot, path) + ": " + line.Trim())
                        .Where(assignment => !allowedActiveAutomationIdAssignments.Contains(assignment, StringComparer.Ordinal));
                    return forbidden.Concat(unapprovedAutomationIds);
                })
                .Concat(shellXamlFiles.SelectMany(path =>
                {
                    var source = File.ReadAllText(path);
                    return shellForbiddenSnippets
                        .Where(snippet => source.Contains(snippet, StringComparison.Ordinal))
                        .Select(snippet => Path.GetRelativePath(repoRoot, path) + ": " + snippet);
                }))
                .OrderBy(violation => violation, StringComparer.Ordinal)
                .ToArray();

            CollectionAssert.AreEqual(Array.Empty<string>(), violations);
        }

        [TestMethod]
        public void ActiveGalleryCSharpAvoidsLocalOnlyAutomationIdAssignments()
        {
            var repoRoot = GetRepoRoot();
            var galleryRoot = Path.Combine(repoRoot, "ModernWpf.Gallery");
            var allowedAssignments = new[]
            {
                @"ModernWpf.Gallery\MainWindow.xaml.cs: AutomationProperties.SetAutomationId(this, ""ModernWpfGalleryMainWindow"");",
                @"ModernWpf.Gallery\Pages\GalleryAutomation.cs: AutomationProperties.SetAutomationId(element, automationId);",
                @"ModernWpf.Gallery\Shell\NavigationRootPage.xaml.cs: AutomationProperties.SetAutomationId(this, ""GalleryNavigationRoot"");",
                @"ModernWpf.Gallery\Shell\NavigationRootPage.xaml.cs: AutomationProperties.SetAutomationId(GetNavigationView(), ""GalleryNavigationView"");",
                @"ModernWpf.Gallery\Shell\NavigationRootPage.xaml.cs: AutomationProperties.SetAutomationId(GetContentHost(), ""GalleryContentHost"");"
            };
            var violations = Directory.EnumerateFiles(galleryRoot, "*.cs", SearchOption.AllDirectories)
                .SelectMany(path => File.ReadLines(path)
                    .Where(line => line.Contains("AutomationProperties.SetAutomationId(", StringComparison.Ordinal))
                    .Select(line => Path.GetRelativePath(repoRoot, path) + ": " + line.Trim()))
                .Where(assignment => !allowedAssignments.Contains(assignment, StringComparer.Ordinal))
                .OrderBy(assignment => assignment, StringComparer.Ordinal)
                .ToArray();

            CollectionAssert.AreEqual(Array.Empty<string>(), violations);
        }

        [TestMethod]
        public void VisualCheckScriptsAvoidRetiredItemPageTitleAutomationHook()
        {
            foreach (var relativePath in new[]
            {
                Path.Combine("tools", "visual-checks", "Run-GalleryVisualChecks.ps1"),
                Path.Combine("tools", "visual-checks", "Run-WpfGalleryVisualAudit.ps1")
            })
            {
                var source = File.ReadAllText(Path.Combine(GetRepoRoot(), relativePath));
                Assert.IsFalse(
                    source.Contains("GalleryItemPageTitle", StringComparison.Ordinal),
                    relativePath + " should not rely on the retired local-only GalleryItemPageTitle automation hook.");
            }
        }

        [TestMethod]
        public void AnnotatedScrollBarSampleLayoutFollowsWinUIGallerySourceShape()
        {
            var source = File.ReadAllText(Path.Combine(
                GetRepoRoot(),
                "ModernWpf.Gallery",
                "Pages",
                "ScrollingSampleFactory.cs"));

            AssertContainsInOrder(
                source,
                "<ScrollView x:Name=\"\"scrollView\"\"",
                "scrollView.ScrollPresenter.VerticalScrollController = annotatedScrollBar.ScrollController;",
                "AnnotatedScrollBar linked to a ScrollView.",
                "var scrollViewer = new ScrollViewer",
                "Name = \"scrollViewer\"",
                "Width = AnnotatedItemWidth + 4",
                "Margin = new Thickness(12, 0, 0, 0)",
                "var slider = WinUISampleSlider.ShowValueFill(new Slider",
                "Name = \"AnnotatedScrollBarMaxHeightSlider\"",
                "Margin = new Thickness(0)",
                "ControlHelper.SetHeader(slider, \"AnnotatedScrollBar maximum height:\");",
                "var options = new Grid",
                "MinWidth = 200",
                "options.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });",
                "options.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });",
                "Text = \"Changing the AnnotatedScrollBar height refreshes its Labels layout.\"",
                "var sliderHost = new StackPanel",
                "Margin = new Thickness(0, 10, 0, 0)",
                "var sliderHeader = new TextBlock",
                "Text = \"AnnotatedScrollBar maximum height:\"",
                "sliderHost.Children.Add(sliderHeader);",
                "sliderHost.Children.Add(slider);",
                "Grid.SetRow(sliderHost, 1);",
                "options.Children.Add(sliderHost);",
                "optionsContent = options;",
                "root.Children.Add(sampleGrid);");
            Assert.IsFalse(
                source.Contains("Grid.SetRow(slider, 2);", StringComparison.Ordinal),
                "AnnotatedScrollBar options should keep the slider in the second WinUI source row.");
            Assert.IsFalse(
                source.Contains("Margin = new Thickness(52, 0, 0, 0)", StringComparison.Ordinal),
                "AnnotatedScrollBar options should use ControlExample-style padding instead of a manual left offset.");
            Assert.IsFalse(
                source.Contains("TextWrapping = TextWrapping.Wrap,\r\n                VerticalAlignment = VerticalAlignment.Center", StringComparison.Ordinal),
                "The WinUI options explanation is a single unwrapped line whose desired width sizes the options column.");
        }

        [TestMethod]
        public void PersonPictureSampleUsesWinUIGalleryProfileImage()
        {
            var source = File.ReadAllText(Path.Combine(
                GetRepoRoot(),
                "ModernWpf.Gallery",
                "Pages",
                "MediaSampleFactory.cs"));

            AssertContainsInOrder(
                source,
                "private const string PersonPictureProfileImageResourcePath =",
                "\"Assets/SampleMedia/shoulder-tap-static-payload.png\"",
                "\"https://learn.microsoft.com/windows/uwp/contacts-and-calendar/images/shoulder-tap-static-payload.png\"",
                "personPicture.ProfilePicture = CreateBitmap(ResourceUri(PersonPictureProfileImageResourcePath));");
            Assert.IsFalse(
                source.Contains("Assets/UserDashboard/64-100x100.jpg", StringComparison.Ordinal),
                "PersonPicture should not render the local dashboard portrait for the WinUI profile-image state.");
        }

        [TestMethod]
        public void StylesSamplesUseWinUIGalleryOptionsChrome()
        {
            var source = File.ReadAllText(Path.Combine(
                GetRepoRoot(),
                "ModernWpf.Gallery",
                "Pages",
                "StylesSampleFactory.cs"));

            AssertContainsInOrder(
                source,
                "var bitmapIconExample = CreateBitmapIconExampleContent(assignRootAutomationId: true, out var bitmapIconOptions);",
                "bitmapIconExample",
                "IconElementBitmapIconXaml",
                "null,",
                "bitmapIconOptions)");
            AssertContainsInOrder(
                source,
                "var shadow = new ThemeShadowChrome",
                "Depth = 32",
                "TranslationZ = 32",
                "Child = shadowRect",
                "ReservesShadowSpace = false");
            AssertContainsInOrder(
                source,
                "var shadowCastGrid = new Grid",
                "Name = \"ShadowCastGrid\"",
                "GalleryAutomation.WithAutomationId(shadowCastGrid, GalleryAutomation.SampleElementId(\"ThemeShadow\", \"ShadowCastGrid\"));");
            AssertContainsInOrder(
                source,
                "GalleryAutomation.WithAutomationId(shadow, GalleryAutomation.SampleElementId(\"ThemeShadow\", \"ShadowChrome\"));",
                "var exampleGrid = new Mux.GridEx",
                "Name = \"Example3Grid\"",
                "Padding = new Thickness(36)",
                "MinWidth = 272",
                "MinHeight = 272",
                "GalleryAutomation.WithAutomationId(exampleGrid, GalleryAutomation.SampleElementId(\"ThemeShadow\", \"Example3Grid\"));");
            Assert.IsFalse(
                source.Contains("Margin = new Thickness(42, 43, 0, 0)", StringComparison.Ordinal),
                "ThemeShadow sample should preserve the WinUI 36px caster layout instead of a magic offset.");
            Assert.IsFalse(
                source.Contains("WindowedPopupInsetMode = ThemeShadowChromeWindowedPopupInsetMode.Medium", StringComparison.Ordinal),
                "ThemeShadow sample is not a popup host and should not rely on popup inset layout.");
            AssertContainsInOrder(
                source,
                "var sliderHeader = new TextBlock",
                "Text = \"Z-translation\"",
                "Margin = new Thickness(0, 0, 0, 10)",
                "options.Children.Add(sliderHeader);",
                "options.Children.Add(translationSlider);",
                "optionsContent = options;",
                "root.Children.Add(exampleGrid);");
            AssertContainsInOrder(
                source,
                "var themeShadowExample = CreateThemeShadowExampleContent(assignRootAutomationId: true, out var optionsContent);",
                "themeShadowExample",
                "ThemeShadowXaml",
                "ThemeShadowCSharp,",
                "optionsContent)");
            Assert.IsFalse(
                source.Contains("options.Margin = new Thickness(24, 0, 0, 0)", StringComparison.Ordinal),
                "Styles samples should use ControlExample.OptionsContent instead of a floating right-margin option control.");
            Assert.IsFalse(
                source.Contains("root.Children.Add(CreateExampleWithOptions(example, monochromeButton));", StringComparison.Ordinal),
                "ItemPage-hosted styles examples should not nest options inside ExampleContent.");
        }

        [TestMethod]
        public void GalleryVisualChecksUseRenderedModernPrimaryArtifactsForSplitViewAndPersonPicture()
        {
            var source = File.ReadAllText(Path.Combine(
                GetRepoRoot(),
                "tools",
                "visual-checks",
                "Run-GalleryVisualChecks.ps1"));

            AssertContainsInOrder(
                source,
                "function Get-PrimaryCropMinimumVisibleStdDev([string]$control)",
                "\"InfoBadge\" { return 8.0 }",
                "\"ThemeShadow\" { return 4.0 }",
                "function Test-ControlRequiresPrimaryCrop([string]$control)",
                "\"ColorPicker\" { return $true }",
                "\"InfoBadge\" { return $true }",
                "\"PersonPicture\" { return $true }",
                "\"SplitView\" { return $true }",
                "\"ThemeShadow\" { return $true }",
                "function Get-RequiredReferencePrimaryCropSource([string]$control)",
                "\"SplitView\" { return \"SplitView pane and content\" }",
                "function Get-ModernPrimaryCropAutomationId([string]$control)",
                "\"SplitView\" { return \"GallerySample_SplitView_SplitView\" }",
                "\"PersonPicture\" { return \"GallerySample_PersonPicture_PersonPicture\" }",
                "\"InfoBadge\" { return \"GallerySample_InfoBadge_InfoBadge\" }",
                "function Get-ReferencePrimaryAutomationId([string]$control)",
                "\"Slider\" { return \"Slider1\" }",
                "\"ColorPicker\" { return \"ColorSpectrum\" }",
                "\"SplitView\" { return \"PaneRoot\" }",
                "\"PersonPicture\" { return \"\" }",
                "\"ThemeShadow\" { return \"\" }",
                "function New-SplitViewReferencePrimaryCrop([string]$caseDir, $window, [string]$screenshot, $sampleElement)",
                "Find-DescendantByAutomationId $sampleElement \"PaneRoot\"",
                "Find-DescendantByAutomationId $sampleElement \"content\"",
                "Cropped the WinUI SplitView pane and content columns to match the ModernWpf rendered SplitView artifact.",
                "SplitView pane and content",
                "function New-ThemeShadowModernPrimaryCrop([string]$caseDir)",
                "Cropped the source 272px ThemeShadow example grid from the stretched ModernWpf sample root.",
                "function New-ThemeShadowReferencePrimaryCrop([string]$caseDir, $window, [string]$screenshot, $sampleElement)",
                "GallerySample_ThemeShadow_Root.png",
                "$controlExampleContentInsetX = 13",
                "$controlExampleContentInsetY = 12",
                "Cropped the WinUI ThemeShadow source example grid to match the ModernWpf demo body.",
                "ThemeShadow demo body",
                "function New-PersonPictureReferencePrimaryCrop([string]$caseDir, $window, [string]$screenshot, $sampleElement)",
                "GallerySample_PersonPicture_PersonPicture.png",
                "Find-DescendantByAutomationId $window \"ProfileImageRadio\"",
                "Find-ColorfulContentCropBounds $screenshot $searchBounds $modernSize.Width $modernSize.Height",
                "Cropped the WinUI PersonPicture avatar from the first example content.",
                "PersonPicture avatar",
                "function Find-ColorfulContentCropBounds([string]$screenshot, $searchBounds, [int]$targetWidth, [int]$targetHeight)",
                "($maxChannel - $minChannel) -gt 22",
                "function Find-AccentComponentBounds([string]$screenshot, $searchBounds)",
                "$color.B -gt 140",
                "$color.G -gt 70",
                "$color.R -lt 120",
                "$minY -lt ($height * 0.65)",
                "function New-ColorPickerModernPrimaryCrop([string]$caseDir)",
                "Cropped the ModernWpf ColorPicker editor surface inside the source-equivalent root padding and bottom margin.",
                "GallerySample_ColorPicker_ColorPicker_editor-surface.png",
                "function New-ColorPickerReferencePrimaryCrop([string]$caseDir, $window, [string]$screenshot)",
                "\"ColorRepresentationComboBox\"",
                "\"BlueTextBox\"",
                "ColorPicker editor surface",
                "function New-InfoBadgeReferencePrimaryCrop([string]$caseDir, $window, [string]$screenshot, $sampleElement)",
                "GallerySample_InfoBadge_InfoBadge.png",
                "$referenceAccent = Find-AccentComponentBounds $screenshot $searchBounds",
                "Cropped the first rendered WinUI InfoBadge value badge from the sample pixels.",
                "InfoBadge value badge",
                "if ($control -eq \"SplitView\")",
                "New-SplitViewReferencePrimaryCrop $caseDir $window $screenshot $sampleElement",
                "elseif ($control -eq \"AnnotatedScrollBar\")",
                "elseif ($control -eq \"ThemeShadow\")",
                "New-ThemeShadowReferencePrimaryCrop $caseDir $window $screenshot $sampleElement",
                "elseif ($control -eq \"PersonPicture\")",
                "New-PersonPictureReferencePrimaryCrop $caseDir $window $screenshot $sampleElement",
                "elseif ($control -eq \"InfoBadge\")",
                "New-InfoBadgeReferencePrimaryCrop $caseDir $window $screenshot $sampleElement",
                "elseif ($control -eq \"ColorPicker\")",
                "New-ColorPickerReferencePrimaryCrop $caseDir $window $screenshot");
            Assert.IsTrue(
                source.Contains("New-ColorPickerModernPrimaryCrop $caseDir", StringComparison.Ordinal),
                "ColorPicker parity must crop the ModernWpf artifact to the same editor surface as the reference child bounds.");
            Assert.IsTrue(
                source.Contains("\"ColorPicker\" { return $true }", StringComparison.Ordinal) &&
                source.Contains("[ValidateSet(\"MoreButton\", \"Alpha\", \"Ring\")]", StringComparison.Ordinal) &&
                source.Contains("if ($ColorPickerState -eq \"Alpha\") { \"alpha\" } else { \"moreBtn\" }", StringComparison.Ordinal) &&
                source.Contains("function New-ColorPickerStateInteractionCrop", StringComparison.Ordinal) &&
                source.Contains("ColorPicker More-button surface", StringComparison.Ordinal) &&
                source.Contains("ColorPicker alpha surface", StringComparison.Ordinal) &&
                source.Contains("ColorPicker ring surface", StringComparison.Ordinal) &&
                source.Contains("$Height = [Math]::Max($Height, 900)", StringComparison.Ordinal) &&
                source.Contains("New-ColorPickerStateInteractionCrop $app $caseDir $window $afterPath \"after\"", StringComparison.Ordinal),
                "ColorPicker parity must exercise and compare the WinUI Gallery's More-button, alpha, and ring option states.");
            Assert.IsTrue(
                source.Contains("$primaryCropMissing = (Test-ControlRequiresPrimaryCrop $control) -and !$staticCrops.Primary.Found", StringComparison.Ordinal),
                "Controls with custom primary parity must fail when the primary crop is missing.");
            Assert.IsTrue(
                source.Contains("$primaryCropWrongSource = $staticCrops.Primary.Found -and ![string]::IsNullOrEmpty($requiredPrimaryCropSource) -and $staticCrops.Primary.Source -ne $requiredPrimaryCropSource", StringComparison.Ordinal),
                "Controls with custom reference primary crops must fail if the harness falls back to a different source.");
            Assert.IsFalse(
                source.Contains("\"PersonPicture\" { return \"ProfileImageRadio\" }", StringComparison.Ordinal),
                "PersonPicture primary parity must crop the avatar, not the profile-type radio button.");
            Assert.IsFalse(
                source.Contains("$sampleBounds.X + 59", StringComparison.Ordinal) ||
                source.Contains("$sampleBounds.Y + 106", StringComparison.Ordinal) ||
                source.Contains("$sampleBounds.Y + 38", StringComparison.Ordinal) ||
                source.Contains("$profileImageRadioBounds.Y - 27", StringComparison.Ordinal) ||
                source.Contains("$sampleBounds.X - 10", StringComparison.Ordinal),
                "PersonPicture primary parity must crop the full avatar from pixels, not from stale hard-coded offsets.");
            Assert.IsFalse(
                source.Contains("\"ThemeShadow\" { return \"svPanel\" }", StringComparison.Ordinal),
                "ThemeShadow primary parity must crop the demo body, not the whole reference page sample.");
            Assert.IsFalse(
                source.Contains("if ($control -eq \"InfoBadge\")\r\n        {\r\n            $primaryCrop = $null", StringComparison.Ordinal) ||
                source.Contains("if ($control -eq \"InfoBadge\") {\n            $primaryCrop = $null", StringComparison.Ordinal),
                "InfoBadge primary parity must use the embedded NavigationView artifact instead of discarding the ModernWpf primary crop.");
            Assert.IsFalse(
                source.Contains("Cropped the reference InfoBadge sample because the embedded NavigationView crop is larger than the badge accent.", StringComparison.Ordinal),
                "InfoBadge primary parity must fail instead of falling back to the whole reference sample when the badge cannot be found.");
        }

        [TestMethod]
        public void GalleryVisualChecksEnforceColorPickerCurrentSourceSurfaceParity()
        {
            var source = File.ReadAllText(Path.Combine(
                GetRepoRoot(),
                "tools",
                "visual-checks",
                "Run-GalleryVisualChecks.ps1"));

            AssertContainsInOrder(
                source,
                "function Get-RequiredReferencePrimaryCropSource([string]$control)",
                "\"ColorPicker\" { return \"ColorPicker editor surface\" }",
                "function Get-ReferencePrimaryCropMeanDeltaThreshold([string]$control)",
                "\"ColorPicker\" { return 4.0 }",
                "function Get-ReferencePrimaryCropSizeDeltaThreshold([string]$control)",
                "\"ColorPicker\" { return 0 }",
                "function Get-ReferenceInteractionCropMeanDeltaThreshold([string]$control)",
                "\"ColorPicker\" { return 4.0 }",
                "function Get-ReferenceInteractionCropSizeDeltaThreshold([string]$control)",
                "\"ColorPicker\" { return 0 }",
                "elseif ($control -eq \"ColorPicker\")",
                "$primarySource = \"HexTextBox\"");
        }

        [TestMethod]
        public void GalleryVisualChecksRequirePairedInteractiveStateMatrices()
        {
            var source = ReadRepoFile("tools", "visual-checks", "Run-GalleryVisualChecks.ps1");

            AssertContainsInOrder(
                source,
                "function Get-ControlVisualStateMatrixKind([string]$control)",
                "\"HyperlinkButton\" { return \"Button\" }",
                "\"RepeatButton\" { return \"Button\" }",
                "\"DropDownButton\" { return \"Button\" }",
                "\"AppBarButton\" { return \"Button\" }",
                "\"ToggleButton\" { return \"Toggle\" }",
                "\"ToggleSwitch\" { return \"Toggle\" }",
                "\"AppBarToggleButton\" { return \"Toggle\" }",
                "\"SplitButton\" { return \"Split\" }",
                "\"ToggleSplitButton\" { return \"ToggleSplit\" }");
            AssertContainsInOrder(
                source,
                "function Get-ControlVisualStateMatrixExpectedStates([string]$control)",
                "@(\"Rest\", \"PointerOver\", \"Pressed\", \"Focused\")",
                "\"OffRest\"",
                "\"OffPointerOver\"",
                "\"OffPressed\"",
                "\"OffFocused\"",
                "\"OnRest\"",
                "\"OnPointerOver\"",
                "\"OnPressed\"",
                "\"OnFocused\"",
                "\"PrimaryPointerOver\"",
                "\"PrimaryPressed\"",
                "\"SecondaryPointerOver\"",
                "\"SecondaryPressed\"",
                "\"OffPrimaryPointerOver\"",
                "\"OffSecondaryPressed\"",
                "\"OnPrimaryPointerOver\"",
                "\"OnSecondaryPressed\"",
                "\"DisabledOff\"",
                "\"DisabledOn\"");
            AssertContainsInOrder(
                source,
                "function Save-ControlVisualStateMatrixCrop(",
                "Get-ElementScreenBounds $target 4 $window",
                "Source = \"ControlVisualStateScreen\"",
                "function Save-ControlVisualStateMatrixPointerCrop(",
                "[GalleryVisualNative]::MouseDown",
                "[GalleryVisualNative]::MouseUp()",
                "function Save-ControlVisualStateMatrixFocusedCrop(",
                "[GalleryVisualNative]::PressShiftTab()",
                "[GalleryVisualNative]::PressTab()",
                "Save-ControlVisualStateMatrixCrop $app $control $caseDir $window $target $state \"Focus\"");
            AssertContainsInOrder(
                source,
                "function Compare-ControlVisualStateMatrixEvidence(",
                "Get-ControlVisualStateMatrixExpectedStates $control",
                "Compare-ControlStateImagesNormalized",
                "Get-ControlVisualStateMatrixCropSizeDeltaThreshold $control",
                "ToggleState=Off",
                "ToggleState=On",
                "Test-ControlVisualStateMatrixStateMustDiffer $stateName",
                "$stateName -match \"Focused$\"",
                "did not produce visible pixels in both applications");
            AssertContainsInOrder(
                source,
                "(Test-ControlSupportsVisualStateMatrix $control) -and $IncludeInteractions",
                "Compare-ControlVisualStateMatrixEvidence",
                "$modern[\"VisualStateComparisons\"] = $visualStateComparisons",
                "Set-VisualCheckReferenceFailure $modern $visualStateComparisons.Reason",
                "## Interactive Control State Matrices");
            AssertContainsInOrder(
                source,
                "$modernInteractionFrames = @()",
                "$modernInteractionFrames = @(\u0024modern.Interaction.Frames)",
                "$referenceInteractionFrames = @()",
                "$referenceInteractionFrames = @(\u0024referenceCapture.Interaction.Frames)",
                "$modernFrame = $modernInteractionFrames[$modernInteractionFrames.Count - 1]",
                "$referenceFrame = $referenceInteractionFrames[$referenceInteractionFrames.Count - 1]");
        }

        [TestMethod]
        public void GalleryVisualChecksRequireEveryCommandBarFlyoutTransitionState()
        {
            var source = ReadRepoFile("tools", "visual-checks", "Run-GalleryVisualChecks.ps1");

            AssertContainsInOrder(
                source,
                "function Compare-CommandBarFlyoutStateEvidence($modernStates, $referenceStates)",
                "\"PrimaryRest\"",
                "\"PrimarySharePointerOver\"",
                "\"PrimarySharePressed\"",
                "\"PrimaryMorePointerOver\"",
                "\"ExpandedAfterPrimaryMoreClick\"",
                "\"ExpandedRest\"",
                "\"ExpandedResizePointerOver\"",
                "\"ExpandedMorePointerOver\"",
                "\"ExpandedResizePressed\"",
                "\"CollapsedAfterExpandedMoreClick\"");
            AssertContainsInOrder(
                source,
                "if ($control -eq \"CommandBarFlyout\" -and $invoked)",
                "Save-CommandBarFlyoutStateCrop $app $caseDir $window \"PrimaryRest\"",
                "Save-CommandBarFlyoutPointerStateCrop $app $caseDir $window \"PrimarySharePointerOver\"",
                "Save-CommandBarFlyoutPointerStateCrop $app $caseDir $window \"PrimarySharePressed\"",
                "Save-CommandBarFlyoutPointerStateCrop $app $caseDir $window \"PrimaryMorePointerOver\"",
                "Save-CommandBarFlyoutClickTransitionStateCrop $app $caseDir $window \"ExpandedAfterPrimaryMoreClick\"",
                "Save-CommandBarFlyoutStateCrop $app $caseDir $window \"ExpandedRest\"",
                "Save-CommandBarFlyoutPointerStateCrop $app $caseDir $window \"ExpandedResizePointerOver\"",
                "Save-CommandBarFlyoutPointerStateCrop $app $caseDir $window \"ExpandedMorePointerOver\"",
                "Save-CommandBarFlyoutPointerStateCrop $app $caseDir $window \"ExpandedResizePressed\"",
                "Save-CommandBarFlyoutClickTransitionStateCrop $app $caseDir $window \"CollapsedAfterExpandedMoreClick\"");
            AssertContainsInOrder(
                source,
                "$sizeDelta -ne 0",
                "$statesRequiredToDifferFromRest -contains $stateName",
                "$modernRestDelta.MeanDelta -lt 0.25",
                "$referenceRestDelta.MeanDelta -lt 0.25",
                "$modern[\"CommandBarFlyoutStateComparisons\"] = $stateComparisons",
                "Set-VisualCheckReferenceFailure $modern $stateComparisons.Reason",
                "## CommandBarFlyout State Matrix");
        }

        [TestMethod]
        public void GalleryVisualChecksCropVisibleItemsRepeaterSourceBarRows()
        {
            var source = File.ReadAllText(Path.Combine(
                GetRepoRoot(),
                "tools",
                "visual-checks",
                "Run-GalleryVisualChecks.ps1"));
            var gallerySource = File.ReadAllText(Path.Combine(
                GetRepoRoot(),
                "ModernWpf.Gallery",
                "Pages",
                "CollectionsSampleFactory.cs"));

            AssertContainsInOrder(
                source,
                "function Test-ControlRequiresPrimaryCrop([string]$control)",
                "\"ItemsRepeater\" { return $true }",
                "function Get-RequiredReferencePrimaryCropSource([string]$control)",
                "\"ItemsRepeater\" { return \"ItemsRepeater source bar rows\" }",
                "function New-ItemsRepeaterModernPrimaryCrop([string]$caseDir, $window, [string]$screenshot)",
                "Find-DescendantByAutomationId $window \"GallerySample_ItemsRepeater_ItemsRepeater\"",
                "$contentInsetX = [Math]::Max(0, [int][Math]::Round(($repeaterBounds.Width - 425) / 2.0))",
                "X = $repeaterBounds.X + $contentInsetX",
                "Width = [Math]::Min(425, $repeaterBounds.Width)",
                "Height = [Math]::Min(88, $repeaterBounds.Height)",
                "function New-ItemsRepeaterReferencePrimaryCrop([string]$caseDir, $window, [string]$screenshot, $sampleElement)",
                "Find-DescendantByAutomationId $sampleElement \"AddBtn\"",
                "[System.Windows.Automation.ControlType]::Pane",
                "$candidateBounds.Width -ge 420 -and $candidateBounds.Width -le 440",
                "ItemsRepeater source bar rows");
            AssertContainsInOrder(
                source,
                "if ($control -eq \"ItemsRepeater\")",
                "New-ItemsRepeaterModernPrimaryCrop $caseDir $window $screenshot",
                "elseif ($control -eq \"ItemsRepeater\")",
                "New-ItemsRepeaterReferencePrimaryCrop $caseDir $window $screenshot $sampleElement");
            AssertContainsInOrder(
                source,
                "function Get-ReferencePrimaryCropMeanDeltaThreshold([string]$control)",
                "\"ItemsRepeater\" { return 1.0 }",
                "function Get-ReferencePrimaryCropSizeDeltaThreshold([string]$control)",
                "\"ItemsRepeater\" { return 0 }");
            Assert.IsFalse(
                source.Contains("$primaryCrop[\"NonBlank\"] = $true", StringComparison.Ordinal),
                "ItemsRepeater must use live source-row crops instead of relabeling a clipped VisualBrush artifact as nonblank.");
            Assert.AreEqual(
                3,
                Regex.Matches(gallerySource, "SystemControlPageBackgroundChromeLowBrush", RegexOptions.CultureInvariant).Count,
                "Horizontal, vertical, and circular ItemsRepeater bar templates must use WinUI SystemChromeLowColor.");
            Assert.IsFalse(
                gallerySource.Contains("<Border Width='{Binding MaxLength}' Background='{DynamicResource SystemControlBackgroundChromeMediumBrush}'>", StringComparison.Ordinal) ||
                gallerySource.Contains("<Border Height='{Binding MaxHeight}' Background='{DynamicResource SystemControlBackgroundChromeMediumBrush}'>", StringComparison.Ordinal) ||
                gallerySource.Contains("<Ellipse Width='{Binding MaxDiameter}' Height='{Binding MaxDiameter}' HorizontalAlignment='Center' VerticalAlignment='Center' Fill='{DynamicResource SystemControlBackgroundChromeMediumBrush}'/>", StringComparison.Ordinal),
                "ItemsRepeater bar templates must not substitute the darker medium chrome resource for WinUI SystemChromeLowColor.");
        }

        [TestMethod]
        public void GalleryVisualChecksDoNotPixelGateVolatileOrAnimatedSampleFrames()
        {
            var source = File.ReadAllText(Path.Combine(
                GetRepoRoot(),
                "tools",
                "visual-checks",
                "Run-GalleryVisualChecks.ps1"));

            AssertContainsInOrder(
                source,
                "function Get-ControlExampleComparisonMode([string]$control, [string]$automationId)",
                "$control -eq \"ItemsRepeater\"",
                "$automationId -eq \"GallerySample_ItemsRepeater_Example6\"",
                "return \"VolatileDataGeometry\"",
                "$control -in @(\"ProgressRing\", \"WinUIProgressBar\")",
                "$automationId.EndsWith(\"_Example1\"",
                "return \"AnimatedTemporal\"",
                "return \"Pixel\"");
            AssertContainsInOrder(
                source,
                "function Get-ControlExampleEvidenceRequirement([string]$comparisonMode)",
                "Randomized recipe pixels are informational.",
                "ItemsRepeater recipe layout/filter/sort runtime contract.",
                "The single-frame pixel delta is informational.",
                "recorder animation/state-transition evidence.");
            AssertContainsInOrder(
                source,
                "$comparisonMode = Get-ControlExampleComparisonMode $control $modernArtifact.AutomationId",
                "$pixelGateApplied = $comparisonMode -eq \"Pixel\"",
                "$geometryPassed = $comparison.Comparable",
                "$widthDelta -eq 0",
                "$heightDelta -le $geometryHeightTolerance",
                "$passed = if ($pixelGateApplied)",
                "$geometryPassed",
                "PixelGateApplied = $pixelGateApplied",
                "EvidenceRequirement = $evidenceRequirement");
            AssertContainsInOrder(
                source,
                "| Control | Example | Mode | Pixel delta | Threshold | ModernWpf size | WinUI size | Status | Evidence contract |",
                "Geometry passed; external proof required",
                "$comparison.EvidenceRequirement");
        }

        [TestMethod]
        public void GalleryVisualChecksRejectWpfOnlyPagesWhenUsingWinUIReference()
        {
            var source = File.ReadAllText(Path.Combine(
                GetRepoRoot(),
                "tools",
                "visual-checks",
                "Run-GalleryVisualChecks.ps1"));

            AssertContainsInOrder(
                source,
                "$WpfGalleryOnlyVisualAuditCases = @(",
                "\"Button\"",
                "\"CheckBox\"",
                "\"ComboBox\"",
                "\"PasswordBox\"",
                "\"RadioButton\"",
                "\"RichTextBox\"",
                "\"Slider\"",
                "\"TextBox\"",
                "if ($Reference -eq \"InstalledWinUI3Gallery\")",
                "$wrongReferenceControls = @($Controls | Where-Object { $WpfGalleryOnlyVisualAuditCases -contains $_ })",
                "Run-GalleryVisualChecks.ps1 uses the WinUI Gallery reference.",
                "Run-WpfGalleryVisualAudit.ps1 -Cases $($wrongReferenceControls -join ',') -Reference OfficialWpfGallery");

            var defaultControlsMatch = Regex.Match(
                source,
                @"\[string\[\]\]\$Controls\s*=\s*@\((?<items>.*?)\),\s*\r?\n\s*\[ValidateSet",
                RegexOptions.Singleline);
            var wpfOnlyControlsMatch = Regex.Match(
                source,
                @"\$WpfGalleryOnlyVisualAuditCases\s*=\s*@\((?<items>.*?)\)",
                RegexOptions.Singleline);
            Assert.IsTrue(defaultControlsMatch.Success, "Could not locate the default visual-check control list.");
            Assert.IsTrue(wpfOnlyControlsMatch.Success, "Could not locate the WPF-only visual-audit case list.");

            var defaultControls = ExtractQuotedStrings(defaultControlsMatch.Groups["items"].Value);
            var wpfOnlyControls = ExtractQuotedStrings(wpfOnlyControlsMatch.Groups["items"].Value);
            var wrongDefaultControls = defaultControls
                .Intersect(wpfOnlyControls, StringComparer.Ordinal)
                .OrderBy(control => control, StringComparer.Ordinal)
                .ToArray();
            CollectionAssert.AreEqual(
                Array.Empty<string>(),
                wrongDefaultControls,
                "The default WinUI Gallery visual-check sweep must not include WPF-only pages.");
        }

        [TestMethod]
        public void GalleryVisualChecksCoverEveryWinUIPortedControlExampleLive()
        {
            var visualChecks = ReadRepoFile("tools", "visual-checks", "Run-GalleryVisualChecks.ps1");
            var itemPageXaml = ReadRepoFile("ModernWpf.Gallery", "Pages", "ItemPage.xaml");
            var itemPageSource = ReadRepoFile("ModernWpf.Gallery", "Pages", "ItemPage.xaml.cs");
            var diagnostics = ReadRepoFile("ModernWpf.Gallery", "Testing", "GalleryDiagnostics.cs");
            var navigationRoot = ReadRepoFile("ModernWpf.Gallery", "Shell", "NavigationRootPage.xaml.cs");

            var countMapMatch = Regex.Match(
                visualChecks,
                @"\$ModernWpfVisualExampleCounts\s*=\s*\[ordered\]@\{(?<items>.*?)\r?\n\}",
                RegexOptions.Singleline);
            Assert.IsTrue(countMapMatch.Success, "Could not locate the WinUI-ported control example-count map.");
            var entries = Regex.Matches(
                    countMapMatch.Groups["items"].Value,
                    @"^\s*(?<control>[A-Za-z0-9]+)\s*=\s*(?<count>\d+)\s*$",
                    RegexOptions.Multiline)
                .Cast<Match>()
                .ToDictionary(
                    match => match.Groups["control"].Value,
                    match => int.Parse(match.Groups["count"].Value),
                    StringComparer.Ordinal);
            Assert.AreEqual(38, entries.Count, "Every curated ModernWPF control route must be in the exhaustive visual matrix.");
            Assert.AreEqual(96, entries.Values.Sum(), "Every displayed curated sample must be in the exhaustive visual matrix.");
            Assert.AreEqual(8, entries["NavigationView"]);
            Assert.AreEqual(2, entries["WinUIProgressBar"]);
            Assert.AreEqual(3, entries["TitleBar"]);
            Assert.AreEqual(1, entries["SystemBackdrop"]);

            AssertContainsInOrder(
                itemPageXaml,
                "AutomationProperties.AutomationId=\"GalleryItemPageScrollViewer\"",
                "AutomationProperties.AutomationId=\"{Binding AutomationId}\"",
                "AutomationProperties.Name=\"{Binding HeaderText}\"");
            AssertContainsInOrder(
                itemPageSource,
                "AssignExampleAutomationIds(_item.UniqueId, Examples);",
                "private static void AssignExampleAutomationIds(string uniqueId, IReadOnlyList<GalleryExample> examples)",
                "\"Example\" + (index + 1).ToString",
                "public string AutomationId { get; internal set; }");
            AssertContainsInOrder(
                diagnostics,
                "VisualScrollRequestFileName = \"modernwpf-gallery-scroll-request.txt\"",
                "VisualScrollResultFileName = \"modernwpf-gallery-scroll-result.txt\"",
                "public static bool BringVisualArtifactIntoView(DependencyObject root, string automationId)",
                "public static bool TryProcessVisualScrollRequest(DependencyObject root)",
                "element.PointToScreen(new Point())");
            AssertContainsInOrder(
                navigationRoot,
                "_visualTestCommandTimer.Tick += OnVisualTestCommandTimerTick;",
                "GalleryDiagnostics.TryProcessVisualScrollRequest",
                "GalleryDiagnostics.WriteVisualArtifacts(Window.GetWindow(this) ?? (DependencyObject)this);",
                "SetVisualTestState(route, \"Ready:\" + route);");
            AssertContainsInOrder(
                visualChecks,
                "function Scroll-ModernWpfVisualArtifactIntoView($window, [string]$artifactDir, [string]$automationId)",
                "function Get-ControlExampleArtifactEvidence([string]$control, [string]$artifactDir)",
                "function Reveal-AutomationRangeInContainer($startElement, $endElement, $container, $window)",
                "function Capture-ModernControlExampleScreenArtifacts([string]$control, [string]$caseDir, $window)",
                "function Capture-WinUIControlExampleScreenArtifacts([string]$control, [string]$caseDir, $window)",
                "$sourceButtons = @(Get-DescendantsByNameAndControlType $scrollPanel \"Source code\"",
                "$revealed = Reveal-AutomationRangeInContainer $header $pairedSourceButton $scrollPanel $window",
                "Find-WinUIControlExampleSourceButton $header $sourceButtons $window",
                "Derived from the matching WinUI ControlExample header and Source code expander.",
                "Capture-ModernControlExampleScreenArtifacts $control $caseDir $window",
                "Capture-WinUIControlExampleScreenArtifacts $control $caseDir $window",
                "Compare-ControlExampleScreenArtifactEvidence",
                "$modern[\"ControlExampleComparisons\"] = $controlExampleComparisons",
                "## Control Example Artifact Coverage",
                "## Live Control Example Coverage",
                "## Control Example Parity",
                "control-example-review.png");
            StringAssert.Contains(visualChecks, "$leftGraphics.DrawImageUnscaled($left, 0, 0)");
            StringAssert.Contains(visualChecks, "$rightGraphics.DrawImageUnscaled($right, 0, 0)");
            StringAssert.Contains(visualChecks, "$pairScale = [Math]::Min(");
            StringAssert.Contains(visualChecks, "$captureWindowHeight = [Math]::Max($originalWindowHeight, 1800)");
            StringAssert.Contains(visualChecks, "($bounds.X + $bounds.Width) -le $windowWidth");
            StringAssert.Contains(visualChecks, "($bounds.Y + $bounds.Height) -le $windowHeight");
            StringAssert.Contains(visualChecks, "$modernWpfWindowWidth = [Math]::Max(640, $Width - 73)");
            StringAssert.Contains(visualChecks, "function Set-DeterministicControlFocus($window, [string]$control)");
            StringAssert.Contains(visualChecks, "[void](Set-DeterministicControlFocus $window $control)");
            AssertContainsInOrder(
                visualChecks,
                "$focusSet = Set-DeterministicControlFocus $window $control",
                "if ($control -eq \"SelectorBar\" -and $focusSet)",
                "Refresh-ModernWpfVisualArtifacts $window $artifactDir");
            Assert.IsFalse(visualChecks.Contains("$leftGraphics.DrawImage($left, 0, 0, $width, $height)", StringComparison.Ordinal));
            Assert.IsFalse(visualChecks.Contains("$rightGraphics.DrawImage($right, 0, 0, $width, $height)", StringComparison.Ordinal));
        }

        [TestMethod]
        public void InfoBadgeNavigationViewSampleKeepsEmbeddedBadgeVisible()
        {
            var source = File.ReadAllText(Path.Combine(
                GetRepoRoot(),
                "ModernWpf.Gallery",
                "Pages",
                "StatusInfoSampleFactory.cs"));

            AssertContainsInOrder(
                source,
                "var navigationView = new Mux.NavigationView",
                "Name = \"nvSample1\"",
                "Width = 560",
                "Height = 300",
                "PaneDisplayMode = Mux.NavigationViewPaneDisplayMode.Left",
                "IsPaneOpen = true",
                "GalleryAutomation.WithAutomationId(infoBadge, GalleryAutomation.SampleElementId(\"InfoBadge\", \"InfoBadge\"))");
        }

        [TestMethod]
        public void GalleryVisualChecksRejectUnprovenWinUIReferenceThemeProbe()
        {
            var source = File.ReadAllText(Path.Combine(
                GetRepoRoot(),
                "tools",
                "visual-checks",
                "Run-GalleryVisualChecks.ps1"));

            AssertContainsInOrder(
                source,
                "function Ensure-WinUIReferenceRootTheme([string]$control, [string]$caseDir, $window)",
                "root pixels already matched the requested theme",
                "Find-ElementByNameInProcess $window.Current.ProcessId @(\"Settings\")",
                "Find-ElementByAutomationIdInProcess $window.Current.ProcessId \"themeModeComboBox\"",
                "$selectionPattern.Select()",
                "Find-ElementByAutomationIdInProcess $window.Current.ProcessId \"__GoBackInvoker\"",
                "$rootPixelsVerified",
                "$verified = $selectedTheme -eq $Theme -and $backInvoked -and $rootPixelsVerified",
                "function Ensure-WinUIReferenceTheme([string]$control, [string]$caseDir, $window)",
                "if ($control -eq \"CommandBarFlyout\")",
                "$primarySource = \"\"",
                "$primaryName = \"Click or right click the image to open a CommandBarFlyout\"",
                "Find-ReferencePrimaryByName $window $control $primaryName",
                "$mean = Get-ImageMeanLuminance $probeCropPath",
                "$postProbeCrop = Save-ElementCrop $window $postProbeScreenshot $postProbeCropPath $primaryElement $primarySource 0",
                "$verified = (([double]$postMean -lt 128.0) -eq $wantsDark)",
                "function Test-WinUIReferenceThemeProbeSucceeded($themeProbe)",
                "if ($Theme -eq \"Default\")",
                "if ($themeProbe.Contains(\"RootTheme\") -and !$themeProbe.RootTheme.Verified)",
                "if ($themeProbe.Contains(\"Verified\"))",
                "if ($themeProbe.Toggled -eq $true)",
                "return $reason.Contains(\"already matched\")");
            AssertContainsInOrder(
                source,
                "$themeProbeFailed = -not (Test-WinUIReferenceThemeProbeSucceeded $themeProbe)",
                "elseif ($themeProbeFailed) { \"Failed\" }",
                "Reference theme probe did not prove $Theme theme: $($themeProbe.Reason)");
        }

        [TestMethod]
        public void GalleryVisualChecksCaptureInteractionFramesWithoutReactivatingWindow()
        {
            var source = File.ReadAllText(Path.Combine(
                GetRepoRoot(),
                "tools",
                "visual-checks",
                "Run-GalleryVisualChecks.ps1"));

            AssertContainsInOrder(
                source,
                "function Capture-Window([IntPtr]$hwnd, [string]$path, [switch]$SkipActivate)",
                "if (!$SkipActivate)",
                "[GalleryVisualNative]::Activate($hwnd)",
                "function Capture-OpenInteractionFrame($window, [string]$path, [bool]$preferScreenCapture, [switch]$SkipActivate)",
                "if (!$SkipActivate)",
                "[GalleryVisualNative]::Activate($window.Current.NativeWindowHandle)",
                "Capture-Window $window.Current.NativeWindowHandle $path -SkipActivate:$SkipActivate",
                "Capture-ScreenRect $window.Current.NativeWindowHandle $path",
                "\"; fallback Capture-ScreenRect failed: \"",
                "[void](Capture-OpenInteractionFrame $window $baselinePath $preferScreenOpenCapture)",
                "$frameError = Capture-OpenInteractionFrame $window $framePath $preferScreenOpenCapture -SkipActivate");
        }

        [TestMethod]
        public void GalleryVisualChecksRetriesCommandBarFlyoutOpenThroughInvokePattern()
        {
            var source = File.ReadAllText(Path.Combine(
                GetRepoRoot(),
                "tools",
                "visual-checks",
                "Run-GalleryVisualChecks.ps1"));

            AssertContainsInOrder(
                source,
                "function Invoke-PopupElementFocusOnce($element)",
                "$element.SetFocus()",
                "[GalleryVisualNative]::PressSpace()",
                "function Invoke-PopupElementClickOnce($element)",
                "[GalleryVisualNative]::Click(",
                "function Expand-ElementPatternOnce($window, $element)",
                "[System.Windows.Automation.ExpandCollapsePattern]::Pattern",
                "$pattern.Expand()",
                "function Toggle-ElementPatternOnce($window, $element)",
                "[System.Windows.Automation.TogglePattern]::Pattern",
                "$pattern.Toggle()",
                "function Invoke-ElementUntilOpen($window, $element, [string[]]$openNames, [string]$control = \"\")",
                "if ($control -eq \"MenuBar\")",
                "$invoked = Invoke-MenuBarTriggerOnce $window $element $openNames",
                "Find-OpenInteractionElement $window $element $openNames $control",
                "$invoked = Invoke-ElementOnce $window $element",
                "Find-OpenInteractionElement $window $element $openNames $control",
                "$invoked = (Expand-ElementPatternOnce $window $element) -or $invoked",
                "$invoked = (Invoke-ElementPatternOnce $window $element) -or $invoked",
                "[GalleryVisualNative]::PressSpace()");
            AssertContainsInOrder(
                source,
                "function Invoke-MenuBarTriggerOnce($window, $element, [string[]]$openNames)",
                "[GalleryVisualNative]::Activate($window.Current.NativeWindowHandle)",
                "$clicked = $false",
                "[GalleryVisualNative]::Click(",
                "$clicked = $true",
                "if ($clicked -and $null -ne (Find-OpenInteractionElement $window $element $openNames \"MenuBar\"))",
                "if (Invoke-ElementPatternOnce $window $element)",
                "return $clicked");
            AssertContainsInOrder(
                source,
                "function Capture-OpenInteraction([string]$app, [string]$control, [string]$caseDir, $window, $showButton, [string[]]$openNames)",
                "if (!$IncludeInteractions -or !(Test-ControlSupportsOpenInteraction $control))",
                "$triggerElement = Get-OpenInteractionTriggerElement $window $control $showButton",
                "Invoke-ElementUntilOpen $window $triggerElement $openNames $control");
        }

        [TestMethod]
        public void GalleryVisualChecksOpensCommonClickInteractionControls()
        {
            var source = File.ReadAllText(Path.Combine(
                GetRepoRoot(),
                "tools",
                "visual-checks",
                "Run-GalleryVisualChecks.ps1"));

            AssertContainsInOrder(
                source,
                "function Test-ControlSupportsOpenInteraction([string]$control)",
                "\"TeachingTip\" { return $true }",
                "\"ComboBox\" { return $true }",
                "\"ContentDialog\" { return $true }",
                "\"Flyout\" { return $true }",
                "\"Popup\" { return $true }",
                "\"MenuBar\" { return $true }",
                "\"MenuFlyout\" { return $true }",
                "\"DropDownButton\" { return $true }",
                "\"SplitButton\" { return $true }",
                "\"ToggleSplitButton\" { return $true }",
                "\"CommandBarFlyout\" { return $true }");
            AssertContainsInOrder(
                source,
                "function Test-ControlPrefersScreenOpenCapture([string]$control)",
                "\"TeachingTip\" { return $true }",
                "\"CommandBarFlyout\" { return $true }",
                "\"MenuBar\" { return $true }");
            AssertContainsInOrder(
                source,
                "function Get-OpenInteractionNames([string]$control)",
                "\"TeachingTip\" { return @(\"This is the title\", \"Try compact mode\", \"And this is the subtitle\") }",
                "\"ComboBox\" { return @(\"Blue\", \"Green\", \"Red\", \"Yellow\") }",
                "\"ContentDialog\" { return @(\"Save your work?\", \"Upload your content to the cloud.\", \"Save\", \"Don't Save\", \"Cancel\") }",
                "\"Flyout\" { return @(\"All items will be removed. Do you want to continue?\", \"Yes, empty my cart\") }",
                "\"Popup\" { return @(\"Simple Popup\") }",
                "\"MenuBar\" { return @(\"New\", \"Open\", \"Save\", \"Exit\") }",
                "\"MenuFlyout\" { return @(\"By rating\", \"By match\", \"By distance\") }",
                "\"DropDownButton\" { return @(\"Send\", \"Reply\", \"Reply All\") }",
                "\"SplitButton\" { return @(\"Red\", \"Orange\", \"Yellow\", \"Green\", \"Blue\", \"Indigo\", \"Violet\", \"Gray\") }",
                "\"ToggleSplitButton\" { return @(\"Bulleted list\", \"Roman numerals list\") }",
                "\"CommandBarFlyout\" { return @(\"Share\", \"Save\", \"Delete\", \"Resize\", \"Move\") }",
                "function Get-OpenInteractionTriggerElement($window, [string]$control, $sampleElement)",
                "\"MenuBar\"",
                "$trigger = Find-DescendantByAnyName $sampleElement @(\"File\")",
                "$trigger = Find-ElementByNameInProcess $window.Current.ProcessId @(\"File\")",
                "$button = Find-DescendantButtonByAnyName $trigger @(\"File\")",
                "return Find-OpenInteractionTriggerTarget $trigger",
                "function Find-OpenInteractionTriggerTarget($element)",
                "[System.Windows.Automation.ControlType]::MenuItem",
                "[System.Windows.Automation.ExpandCollapsePattern]::Pattern",
                "[System.Windows.Automation.InvokePattern]::Pattern",
                "[System.Windows.Automation.TreeWalker]::ControlViewWalker.GetParent($candidate)");
            AssertContainsInOrder(
                source,
                "function Find-ElementByNameInPopupWindows($window, [string[]]$names)",
                "$candidateWindow.Current.NativeWindowHandle -eq $window.Current.NativeWindowHandle",
                "Find-DescendantByAnyName $candidateWindow $names",
                "return $null");
            AssertContainsInOrder(
                source,
                "function Find-InteractiveElementByNameInProcess([int]$processId, [string[]]$names)",
                "$window.FindAll([System.Windows.Automation.TreeScope]::Descendants, $nameCondition)",
                "[System.Windows.Automation.ControlType]::Button",
                "[System.Windows.Automation.ControlType]::MenuItem",
                "[System.Windows.Automation.ControlType]::ListItem",
                "return $element");
            AssertContainsInOrder(
                source,
                "function Test-AutomationElementUsable($element)",
                "$element.Current.IsOffscreen",
                "function Find-ElementByAutomationIdInPopupWindows($window, [string]$automationId)",
                "$candidateWindow.Current.NativeWindowHandle -eq $mainHandle",
                "Find-DescendantByAutomationId $candidateWindow $automationId",
                "function Find-CommandBarFlyoutMoreButton($window)",
                "Find-InteractiveElementByNameInProcess $window.Current.ProcessId @(\"Share\", \"Save\", \"Delete\")",
                "$popupHandle = Get-ElementNativeWindowHandle $primaryCommand",
                "Find-TopLevelElementByNativeWindowHandleInProcess $window.Current.ProcessId ([int]$popupHandle)",
                "Find-DescendantByAutomationId $popupWindow \"MoreButton\"",
                "Find-ElementByAutomationIdInPopupWindows $window \"MoreButton\"",
                "function Wait-ForCommandBarFlyoutPrimaryCommands($window, [int]$timeoutMilliseconds)",
                "Find-InteractiveElementByNameInProcess $window.Current.ProcessId @(\"Share\")",
                "Find-InteractiveElementByNameInProcess $window.Current.ProcessId @(\"Save\")",
                "Find-InteractiveElementByNameInProcess $window.Current.ProcessId @(\"Delete\")",
                "Test-AutomationElementUsable $moreButton",
                "function Wait-ForInteractiveElementByNameInProcess([int]$processId, [string[]]$names, [int]$timeoutMilliseconds)");
            AssertContainsInOrder(
                source,
                "function Find-ComboBoxOpenElement($window, $element, [string[]]$openNames)",
                "Get-ExpandCollapseStateName $element",
                "Find-ElementsByNameInProcess $window.Current.ProcessId $openNames",
                "$match.Current.ControlType -ne [System.Windows.Automation.ControlType]::ListItem",
                "$outsideClosedCombo = $rect.Y -ge ($comboRect.Bottom - 1) -or $rect.Bottom -le ($comboRect.Y + 1)",
                "function Find-SplitButtonOpenElementAtPoint($element, [string[]]$openNames)",
                "[System.Windows.Automation.AutomationElement]::FromPoint($point)",
                "[System.Windows.Automation.TreeWalker]::ControlViewWalker.GetParent($candidate)",
                "function Find-OpenInteractionElement($window, $element, [string[]]$openNames, [string]$control)",
                "if ($control -eq \"ComboBox\")",
                "return Find-ComboBoxOpenElement $window $element $openNames",
                "if ($control -eq \"SplitButton\" -or $control -eq \"ToggleSplitButton\")",
                "(Get-ExpandCollapseStateName $element) -ne \"Expanded\"",
                "return Find-SplitButtonOpenElementAtPoint $element $openNames",
                "return Find-ElementByNameInProcess $window.Current.ProcessId $openNames",
                "function Test-ControlPrefersScreenOpenCapture([string]$control)",
                "\"TeachingTip\" { return $true }",
                "\"CommandBarFlyout\" { return $true }",
                "\"MenuBar\" { return $true }",
                "default { return $false }",
                "function Test-ControlRequiresPopupWindowOpenProof([string]$control)",
                "\"CommandBarFlyout\" { return $true }",
                "\"MenuFlyout\" { return $true }",
                "\"DropDownButton\" { return $true }",
                "\"SplitButton\" { return $true }",
                "\"ToggleSplitButton\" { return $true }",
                "function Close-PreparedOpenInteractionState($window, [string]$control)",
                "$control -ne \"TeachingTip\"",
                "GallerySample_TeachingTip_TeachingTip",
                "[System.Windows.Automation.WindowPattern]::Pattern",
                "$pattern.Close()",
                "function Open-CommandBarFlyoutSecondaryCommands($window)",
                "$deadline = (Get-Date).AddMilliseconds(2500)",
                "Wait-ForCommandBarFlyoutPrimaryCommands $window 1200",
                "Find-CommandBarFlyoutMoreButton $window",
                "Invoke-ElementPatternOnce $window $moreButton",
                "Wait-ForInteractiveElementByNameInProcess $window.Current.ProcessId @(\"Resize\", \"Move\") 600",
                "Expand-ElementPatternOnce $window $moreButton",
                "Toggle-ElementPatternOnce $window $moreButton",
                "Invoke-PopupElementFocusOnce $moreButton",
                "Invoke-PopupElementClickOnce $moreButton",
                "Wait-ForInteractiveElementByNameInProcess $window.Current.ProcessId @(\"Resize\", \"Move\") 1200",
                "Invoke-ElementOnce $window $moreButton",
                "function Get-ElementNativeWindowHandle($element)",
                "$handle = [int]$candidate.Current.NativeWindowHandle",
                "return [IntPtr]$handle",
                "function Test-PopupScreenCropCandidate($window, $element)",
                "$bounds.Width -ge ($windowBounds.Width * 0.85)",
                "function Get-PopupScreenCropElement($window, $openElement)",
                "$best = $parent",
                "function Test-ScreenElementPopupCropHasContent($crop)",
                "$crop.VisibleStdDev -ge 8.0",
                "function Capture-OpenElementPopupCrop($window, $openElement, [IntPtr]$popupHandle, [string]$popupWindowPath, [string]$screenElementPath)",
                "Capture-Window $popupHandle $popupWindowPath -SkipActivate",
                "Capture-ScreenRect $popupHandle $popupWindowPath",
                "Save-ScreenElementCrop $screenElement $screenElementPath \"ScreenElement\" 10 $window",
                "Test-ScreenElementPopupCropHasContent $screenElementCrop",
                "function Capture-OpenInteractionFrame($window, [string]$path, [bool]$preferScreenCapture, [switch]$SkipActivate)",
                "if (!$SkipActivate)",
                "[GalleryVisualNative]::Activate($window.Current.NativeWindowHandle)",
                "Capture-ScreenRect $window.Current.NativeWindowHandle $path");
            AssertContainsInOrder(
                source,
                "public static void SetTopMost(IntPtr hWnd, bool topMost)",
                "SetWindowPos(hWnd, topMost ? new IntPtr(-1) : new IntPtr(-2), 0, 0, 0, 0, 0x0043)",
                "$preferScreenOpenCapture = Test-ControlPrefersScreenOpenCapture $control",
                "if ($preferScreenOpenCapture)",
                "[GalleryVisualNative]::SetTopMost($window.Current.NativeWindowHandle, $true)",
                "$triggerElement = Get-OpenInteractionTriggerElement $window $control $showButton",
                "Close-PreparedOpenInteractionState $window $control",
                "$screenCaptureTrustReference = \"\"",
                "$screenCaptureTrustDelta = $null",
                "$screenCaptureTrusted = $true",
                "$screenCaptureTrustReference = Join-Path $caseDir (\"{0}-{1}-screen-trust-reference.png\" -f $app.ToLowerInvariant(), $control)",
                "[void](Capture-OpenInteractionFrame $window $baselinePath $preferScreenOpenCapture)",
                "$baselineNonBlank = if (Test-Path $baselinePath) { Test-ImageNotBlank $baselinePath } else { $false }",
                "$baselineControlCrop = if (Test-Path $baselinePath)",
                "Save-ElementCrop $window $baselinePath $baselineControlCropPath $showButton \"UIA\" 10",
                "$baselineControlNonBlank = $null -ne $baselineControlCrop -and $baselineControlCrop.Contains(\"NonBlank\") -and $baselineControlCrop.NonBlank",
                "if (!$baselineNonBlank -or !$baselineControlNonBlank)",
                "if (!$baselineControlNonBlank)",
                "$existingBaselinePath = Join-Path $caseDir (\"{0}-{1}.png\" -f $app.ToLowerInvariant(), $control)",
                "Copy-Item -LiteralPath $existingBaselinePath -Destination $baselinePath -Force",
                "$baselineControlCrop = Save-ElementCrop $window $baselinePath $baselineControlCropPath $showButton \"UIA\" 10",
                "$screenCaptureTrustDelta = Compare-Images $screenCaptureTrustReference $baselinePath",
                "$screenCaptureTrusted = $screenCaptureTrustDelta.Comparable -and $screenCaptureTrustDelta.MeanDelta -lt 25.0",
                "$invoked = Invoke-ElementUntilOpen $window $triggerElement $openNames $control",
                "$commandBarFlyoutSecondaryExpanded = $false",
                "$commandBarFlyoutSecondaryExpanded = Open-CommandBarFlyoutSecondaryCommands $window",
                "$comboBoxOpenVisualDelta = $null",
                "$comboBoxOpenBaselineCrop = \"\"",
                "$comboBoxPopupScreenshot = \"\"",
                "$comboBoxPopupNonBlank = $false",
                "$comboBoxPopupSize = $null",
                "$comboBoxPopupCrop = $null",
                "$menuBarPopupNonBlank = $false",
                "$menuBarPopupScreenshot = \"\"",
                "$menuBarPopupSize = $null",
                "$menuBarPopupCrop = $null",
                "$openPopupNonBlank = $false",
                "$openPopupScreenshot = \"\"",
                "$openPopupSize = $null",
                "$openPopupCrop = $null",
                "$popupHandle = Get-ElementNativeWindowHandle $openElement",
                "$comboBoxPopupCrop = Capture-OpenElementPopupCrop `",
                "(Join-Path $caseDir (\"{0}-{1}-popup-screen-element.png\" -f $app.ToLowerInvariant(), $control))",
                "$comboBoxPopupScreenshot = $comboBoxPopupCrop.Screenshot",
                "elseif ($control -eq \"MenuBar\")",
                "$menuBarPopupCrop = Capture-OpenElementPopupCrop `",
                "$menuBarPopupScreenshot = $menuBarPopupCrop.Screenshot",
                "elseif (Test-ControlRequiresPopupWindowOpenProof $control)",
                "$openPopupCrop = Capture-OpenElementPopupCrop `",
                "$openPopupScreenshot = $openPopupCrop.Screenshot",
                "$comboBoxOpenBaselineCrop = Join-Path $caseDir (\"{0}-{1}-open-baseline-crop.png\" -f $app.ToLowerInvariant(), $control)",
                "$comboBoxOpenVisualDelta = Compare-ImagesNormalized $comboBoxOpenBaselineCrop $cropPath",
                "if ($control -eq \"ComboBox\")",
                "$comboBoxPopupTrusted = $comboBoxPopupNonBlank -and",
                "$comboBoxPopupCrop.Source -ne \"ScreenElement\" -or $screenCaptureTrusted",
                "$visualOpened = $comboBoxPopupTrusted -or",
                "$comboBoxOpenVisualDelta.MeanDelta -gt 5.0",
                "if ($comboBoxPopupTrusted)",
                "$crop = $comboBoxPopupCrop",
                "elseif ($control -eq \"MenuBar\")",
                "$menuBarPopupTrusted = $menuBarPopupNonBlank -and",
                "$menuBarPopupCrop.Source -ne \"ScreenElement\" -or $screenCaptureTrusted",
                "$visualOpened = $menuBarPopupTrusted",
                "if ($menuBarPopupTrusted)",
                "$crop = $menuBarPopupCrop",
                "elseif (Test-ControlRequiresPopupWindowOpenProof $control)",
                "$openPopupTrusted = $openPopupNonBlank -and",
                "$openPopupCrop.Source -ne \"ScreenElement\" -or $screenCaptureTrusted",
                "$visualOpened = $openPopupTrusted",
                "if ($openPopupTrusted)",
                "$crop = $openPopupCrop",
                "$status = if (!$baselineNonBlank -or !$baselineControlNonBlank) { \"Failed\" } elseif (!$invoked) { \"Failed\" } elseif ($null -ne $openElement -and $visualOpened) { \"Passed\" } else { \"Failed\" }",
                "\"$control screen capture did not match the Gallery window, and the popup window could not be captured.\"",
                "\"$control exposed dropdown UIA but no changed dropdown pixels were captured.\"",
                "elseif ($control -eq \"MenuBar\")",
                "$control did not expose an opened menu item.",
                "elseif ($control -eq \"CommandBarFlyout\")",
                "$control primary flyout opened, but the MoreButton did not expose Resize/Move secondary commands.",
                "elseif (Test-ControlRequiresPopupWindowOpenProof $control)",
                "$control exposed opened popup UIA but no nonblank popup pixels were captured.",
                "[GalleryVisualNative]::SetTopMost($window.Current.NativeWindowHandle, $false)",
                "TriggerName = $(if ($null -ne $triggerElement) { $triggerElement.Current.Name } else { \"\" })",
                "TriggerAutomationId = $(if ($null -ne $triggerElement) { $triggerElement.Current.AutomationId } else { \"\" })",
                "BaselineNonBlank = $baselineNonBlank",
                "BaselineControlCrop = $baselineControlCrop",
                "BaselineControlNonBlank = $baselineControlNonBlank",
                "ScreenCaptureTrustReference = $screenCaptureTrustReference",
                "ScreenCaptureTrustDelta = $screenCaptureTrustDelta",
                "ScreenCaptureTrusted = $screenCaptureTrusted",
                "ComboBoxOpenBaselineCrop = $comboBoxOpenBaselineCrop",
                "ComboBoxOpenVisualDelta = $comboBoxOpenVisualDelta",
                "ComboBoxPopupScreenshot = $comboBoxPopupScreenshot",
                "ComboBoxPopupNonBlank = $comboBoxPopupNonBlank",
                "ComboBoxPopupSize = $comboBoxPopupSize",
                "ComboBoxPopupCrop = $comboBoxPopupCrop",
                "MenuBarPopupScreenshot = $menuBarPopupScreenshot",
                "MenuBarPopupNonBlank = $menuBarPopupNonBlank",
                "MenuBarPopupSize = $menuBarPopupSize",
                "MenuBarPopupCrop = $menuBarPopupCrop",
                "OpenPopupScreenshot = $openPopupScreenshot",
                "OpenPopupNonBlank = $openPopupNonBlank",
                "OpenPopupSize = $openPopupSize",
                "OpenPopupCrop = $openPopupCrop",
                "CommandBarFlyoutSecondaryExpanded = $commandBarFlyoutSecondaryExpanded");
            AssertContainsInOrder(
                source,
                "function Get-CommandBarFlyoutOpenSurfaceElementEvidence($window, $elements = $null)",
                "Name = [string]$element.Current.Name",
                "ControlType = [string]$element.Current.ControlType.ProgrammaticName",
                "function Save-CommandBarFlyoutOpenSurfaceScreenCrop(",
                "$elements = @(Get-CommandBarFlyoutOpenSurfaceElements $window)",
                "$capturePlan = New-CommandBarFlyoutSurfaceCapturePlan $window $elements",
                "$bounds = $capturePlan.RawElementBounds",
                "$elementEvidence = @(\u0024capturePlan.Elements)",
                "RawElementBounds = $bounds",
                "Elements = $elementEvidence");
            Assert.IsFalse(
                source.Contains("$referencePopupCropNonBlank", StringComparison.Ordinal),
                "Popup-required open checks must not pass from a generic reference crop.");
            AssertContainsInOrder(
                source,
                "$openElement = if ($control -eq \"CommandBarFlyout\")",
                "Find-InteractiveElementByNameInProcess $window.Current.ProcessId @(\"Resize\", \"Move\")",
                "$openElement = Find-OpenInteractionElement $window $showButton $openNames $control",
                "if ($null -ne $openElement)",
                "else {",
                "$treePath = \"\"",
                "if (Test-ControlRequiresPopupWindowOpenProof $control)",
                "$cropElement = $null",
                "$usableFrames = @($frames.ToArray() | Where-Object { $_.NonBlank -and ![string]::IsNullOrEmpty($_.Screenshot) })");
            AssertContainsInOrder(
                source,
                "function Invoke-SplitButtonSecondaryOnce($window, $element)",
                "$rect = $element.Current.BoundingRectangle",
                "$x = [int][Math]::Round($rect.Right - [Math]::Min(12.0, [Math]::Max(6.0, $rect.Width * 0.18)))",
                "[GalleryVisualNative]::Click($x, $y)",
                "if ($control -eq \"SplitButton\" -or $control -eq \"ToggleSplitButton\")",
                "$invoked = Invoke-SplitButtonSecondaryOnce $window $element",
                "if ((Get-ExpandCollapseStateName $element) -ne \"Expanded\")",
                "$invoked = (Expand-ElementPatternOnce $window $element) -or $invoked",
                "$openElement = Find-OpenInteractionElement $window $showButton $openNames $control");
            Assert.IsFalse(
                source.Contains("$skipOpenUiaSearch = $control -eq \"SplitButton\"", StringComparison.Ordinal),
                "SplitButton open checks must not bypass opened-content UIA.");
            AssertContainsInOrder(
                source,
                "$needsSampleElement = $IncludeInteractions -and (",
                "(Test-ControlSupportsOpenInteraction $control) -or",
                "(Test-ControlSupportsStateInteraction $control) -or",
                "(Test-ControlSupportsSelectionInteraction $control) -or",
                "(Test-ControlSupportsValueInteraction $control) -or",
                "(Test-ControlSupportsOutputInteraction $control) -or",
                "(Test-ControlSupportsTextInteraction $control) -or",
                "(Test-ControlSupportsVisualStateMatrix $control))",
                "$visualStateMatrix = Capture-ControlVisualStateMatrix \"ModernWpf\" $control $caseDir $window $sample",
                "$openNames = Get-OpenInteractionNames $control",
                "$openInteraction = Capture-OpenInteraction \"ModernWpf\" $control $caseDir $window $sample $openNames",
                "$stateInteraction = Capture-StateInteraction \"ModernWpf\" $control $caseDir $window $sample",
                "$selectionInteraction = Capture-SelectionInteraction \"ModernWpf\" $control $caseDir $window $sample",
                "$valueInteraction = Capture-ValueInteraction \"ModernWpf\" $control $caseDir $window $sample",
                "$outputInteraction = Capture-OutputInteraction \"ModernWpf\" $control $caseDir $window $sample",
                "$textInteraction = Capture-TextInteraction \"ModernWpf\" $control $caseDir $window $sample",
                "$interaction = if ($null -ne $openInteraction) { $openInteraction } elseif ($null -ne $stateInteraction) { $stateInteraction } elseif ($null -ne $selectionInteraction) { $selectionInteraction } elseif ($null -ne $valueInteraction) { $valueInteraction } elseif ($null -ne $outputInteraction) { $outputInteraction } else { $textInteraction }");
        }

        [TestMethod]
        public void GalleryVisualChecksCompareProgressRingAtAFixedDeterminateValue()
        {
            var source = File.ReadAllText(Path.Combine(
                GetRepoRoot(),
                "tools",
                "visual-checks",
                "Run-GalleryVisualChecks.ps1"));
            var diagnosticsSource = File.ReadAllText(Path.Combine(
                GetRepoRoot(),
                "ModernWpf.Gallery",
                "Testing",
                "GalleryDiagnostics.cs"));

            AssertContainsInOrder(
                source,
                "function Set-ProgressRingDeterminateValue($window, [string]$app, [double]$value)",
                "\"GallerySample_ProgressRing_DeterminateProgressRing\"",
                "\"ProgressRing2\"",
                "TryFind-DescendantByAutomationId $window \"ProgressValue\"",
                "[System.Windows.Automation.RangeValuePattern]::Pattern",
                "$pattern.SetValue($value)",
                "Test-DoubleApproximatelyEqual $current $value",
                "\"ProgressRing\" { return \"GallerySample_ProgressRing_DeterminateProgressRing\" }",
                "\"ProgressRing\" { return \"ProgressRing2\" }",
                "function Get-ReferencePrimaryCropMeanDeltaThreshold([string]$control)",
                "\"ProgressRing\" { return 1.0 }",
                "function Get-ReferencePrimaryCropSizeDeltaThreshold([string]$control)",
                "\"ProgressRing\" { return 0 }",
                "Set-ProgressRingDeterminateValue $window \"ModernWpf\" 65",
                "Refresh-ModernWpfVisualArtifacts $window",
                "$focusSet = Set-DeterministicControlFocus $window $control",
                "Set-ProgressRingDeterminateValue $window \"WinUI3\" 65");
            AssertContainsInOrder(
                source,
                "function Set-DeterministicControlFocus($window, [string]$control)",
                "$control -in @(\"ProgressRing\", \"WinUIProgressBar\")",
                "Find-DescendantButtonByAnyName $window @(\"Toggle Navigation\", \"Back\")",
                "$shellButton.SetFocus()");
            Assert.IsFalse(
                source.Contains("function Reset-ProgressRingAnimationPhase", StringComparison.Ordinal),
                "ProgressRing reference proof must not depend on an indeterminate animation capture delay.");
            Assert.IsFalse(
                source.Contains("function New-ProgressRingModernPrimaryCrop", StringComparison.Ordinal),
                "ProgressRing proof should use the rendered determinate control artifact directly.");
            Assert.IsFalse(
                source.Contains("GallerySample_ProgressRing_ProgressRing_fromRoot.png", StringComparison.Ordinal),
                "ProgressRing proof should not crop the indeterminate ring from the sample root.");
            AssertContainsInOrder(
                diagnosticsSource,
                "private static Rect GetArtifactViewbox(FrameworkElement element, int width, int height)",
                "GallerySample_ProgressRing_DeterminateProgressRing",
                "return GetParentOffsetViewbox(element, width, height);");
            AssertContainsInOrder(
                source,
                "$hasPrimaryCropPair = $null -ne $modern -and $null -ne $referenceCapture",
                "if ($FailOnDifference -and !$hasPrimaryCropPair -and $comparison.Comparable",
                "if ($hasPrimaryCropPair)",
                "$primaryThreshold = Get-ReferencePrimaryCropMeanDeltaThreshold $control");
        }

        [TestMethod]
        public void GalleryVisualChecksEnforceBreadcrumbBarPixelParityThreshold()
        {
            var source = File.ReadAllText(Path.Combine(
                GetRepoRoot(),
                "tools",
                "visual-checks",
                "Run-GalleryVisualChecks.ps1"));

            AssertContainsInOrder(
                source,
                "\"BreadcrumbBar\" { return \"GallerySample_BreadcrumbBar_BreadcrumbBar\" }",
                "\"BreadcrumbBar\" { return \"BreadcrumbBar1\" }",
                "function Get-ReferencePrimaryCropMeanDeltaThreshold([string]$control)",
                "\"BreadcrumbBar\" { return 3.0 }");
        }

        [TestMethod]
        public void GalleryVisualChecksEnforceSelectorBarPixelParityThreshold()
        {
            var source = File.ReadAllText(Path.Combine(
                GetRepoRoot(),
                "tools",
                "visual-checks",
                "Run-GalleryVisualChecks.ps1"));

            AssertContainsInOrder(
                source,
                "\"SelectorBar\" { return \"GallerySample_SelectorBar_SelectorBar\" }",
                "\"SelectorBar\" { return \"PART_ItemsView\" }",
                "function Get-ReferencePrimaryCropMeanDeltaThreshold([string]$control)",
                "\"SelectorBar\" { return 3.0 }");
        }

        [TestMethod]
        public void GalleryVisualChecksEnforceTitleBarPixelParityThreshold()
        {
            var source = File.ReadAllText(Path.Combine(
                GetRepoRoot(),
                "tools",
                "visual-checks",
                "Run-GalleryVisualChecks.ps1"));

            AssertContainsInOrder(
                source,
                "\"TitleBar\" { return \"GallerySample_TitleBar_TitleBarControl\" }",
                "\"TitleBar\" { return \"TitleBarControl\" }",
                "function New-TitleBarModernPrimaryCrop([string]$caseDir)",
                "function Get-ReferencePrimaryCropMeanDeltaThreshold([string]$control)",
                "\"TitleBar\" { return 1.0 }",
                "function Get-ReferencePrimaryCropSizeDeltaThreshold([string]$control)",
                "\"TitleBar\" { return 0 }",
                "function Stabilize-WinUIReferenceResponsiveLayout([string]$control, $window)",
                "$width + 64",
                "PART_TitleText",
                "PART_SubtitleText",
                "$responsiveLayoutProbe = Stabilize-WinUIReferenceResponsiveLayout $control $window",
                "ResponsiveLayoutProbe = $responsiveLayoutProbe");
        }

        [TestMethod]
        public void GalleryVisualChecksEnforceDropDownButtonPixelParityThreshold()
        {
            var source = File.ReadAllText(Path.Combine(
                GetRepoRoot(),
                "tools",
                "visual-checks",
                "Run-GalleryVisualChecks.ps1"));

            AssertContainsInOrder(
                source,
                "\"DropDownButton\" { return \"GallerySample_DropDownButton_DropDownButton\" }",
                "\"DropDownButton\" { return \"Email\" }",
                "function Get-ReferencePrimaryCropMeanDeltaThreshold([string]$control)",
                "\"DropDownButton\" { return 4.0 }");
        }

        [TestMethod]
        public void GalleryVisualChecksEnforceSplitButtonPixelParityThreshold()
        {
            var source = File.ReadAllText(Path.Combine(
                GetRepoRoot(),
                "tools",
                "visual-checks",
                "Run-GalleryVisualChecks.ps1"));

            AssertContainsInOrder(
                source,
                "\"SplitButton\" { return \"GallerySample_SplitButton_SplitButton\" }",
                "\"SplitButton\" { return \"myColorButton\" }",
                "function Get-ReferencePrimaryCropMeanDeltaThreshold([string]$control)",
                "\"SplitButton\" { return 1.0 }");
            AssertContainsInOrder(
                source,
                "function Get-ReferencePrimaryCropSizeDeltaThreshold([string]$control)",
                "\"SplitButton\" { return 0 }");
            AssertContainsInOrder(
                source,
                "function Capture-WinUIReference([string]$control, [string]$caseDir)",
                "Move-CursorAwayFromInteractionSurface $window",
                "$staticCrops = Capture-StaticCrops \"WinUI3\" $control $caseDir $window $screenshot",
                "$showButton = Find-ReferenceInteractionTrigger $window $control",
                "$openInteraction = Capture-OpenInteraction \"WinUI3\" $control $caseDir $window $showButton $openNames");
        }

        [TestMethod]
        public void GalleryVisualChecksEnforceToggleSplitButtonPixelParityThreshold()
        {
            var source = File.ReadAllText(Path.Combine(
                GetRepoRoot(),
                "tools",
                "visual-checks",
                "Run-GalleryVisualChecks.ps1"));

            AssertContainsInOrder(
                source,
                "\"ToggleSplitButton\" { return \"GallerySample_ToggleSplitButton_ToggleSplitButton\" }",
                "\"ToggleSplitButton\" { return \"myListButton\" }",
                "function Get-ReferencePrimaryCropMeanDeltaThreshold([string]$control)",
                "\"ToggleSplitButton\" { return 2.0 }");
            AssertContainsInOrder(
                source,
                "function Get-ReferencePrimaryCropSizeDeltaThreshold([string]$control)",
                "\"ToggleSplitButton\" { return 0 }");
        }

        [TestMethod]
        public void GalleryVisualChecksEnforceToggleSwitchPixelParityThreshold()
        {
            var source = File.ReadAllText(Path.Combine(
                GetRepoRoot(),
                "tools",
                "visual-checks",
                "Run-GalleryVisualChecks.ps1"));

            AssertContainsInOrder(
                source,
                "\"ToggleSwitch\" { return \"GallerySample_ToggleSwitch_ToggleSwitch\" }",
                "\"ToggleSwitch\" { return \"simple ToggleSwitch\" }",
                "function Get-ReferencePrimaryCropMeanDeltaThreshold([string]$control)",
                "\"ToggleSwitch\" { return 1.5 }");
            AssertContainsInOrder(
                source,
                "function Get-ReferencePrimaryCropSizeDeltaThreshold([string]$control)",
                "\"ToggleSwitch\" { return 0 }");
            AssertContainsInOrder(
                source,
                "function Capture-StaticCrops([string]$app, [string]$control, [string]$caseDir, $window, [string]$screenshot)",
                "Copy-Item -LiteralPath $primaryResult.Screenshot -Destination $primaryPath -Force",
                "function Capture-ModernWpf([string]$control, [string]$caseDir)",
                "$staticCrops = Capture-StaticCrops \"ModernWpf\" $control $caseDir $window $screenshot",
                "$stateInteraction = Capture-StateInteraction \"ModernWpf\" $control $caseDir $window $sample");
        }

        [TestMethod]
        public void GalleryVisualChecksEnforceNumberBoxPixelParityThreshold()
        {
            var source = File.ReadAllText(Path.Combine(
                GetRepoRoot(),
                "tools",
                "visual-checks",
                "Run-GalleryVisualChecks.ps1"));

            AssertContainsInOrder(
                source,
                "\"NumberBox\" { return \"GallerySample_NumberBox_SpinButtonNumberBox\" }",
                "\"NumberBox\" { return \"NumberBoxSpinButtonPlacementExample\" }",
                "function Get-ReferencePrimaryCropMeanDeltaThreshold([string]$control)",
                "\"NumberBox\" { return 2.5 }",
                "function Get-ReferencePrimaryCropSizeDeltaThreshold([string]$control)",
                "\"NumberBox\" { return 0 }",
                "function Get-ReferenceInteractionCropMeanDeltaThreshold([string]$control)",
                "\"NumberBox\" { return 2.0 }",
                "function Get-ReferenceInteractionCropSizeDeltaThreshold([string]$control)",
                "\"NumberBox\" { return 0 }");
        }

        [TestMethod]
        public void GalleryVisualChecksEnforceAutoSuggestBoxPixelParityThreshold()
        {
            var source = File.ReadAllText(Path.Combine(
                GetRepoRoot(),
                "tools",
                "visual-checks",
                "Run-GalleryVisualChecks.ps1"));

            AssertContainsInOrder(
                source,
                "\"AutoSuggestBox\" { return \"GallerySample_AutoSuggestBox_AutoSuggestBox\" }",
                "\"AutoSuggestBox\" { return \"Control1\" }",
                "function Get-ReferencePrimaryCropMeanDeltaThreshold([string]$control)",
                "\"AutoSuggestBox\" { return 0.1 }",
                "function Get-ReferencePrimaryCropSizeDeltaThreshold([string]$control)",
                "\"AutoSuggestBox\" { return 0 }");
            AssertContainsInOrder(
                source,
                "function Set-EditableElementText($window, $element, [string]$text)",
                "[GalleryVisualNative]::PressCtrlA()",
                "[GalleryVisualNative]::TypeText($text)",
                "$pattern = $edit.GetCurrentPattern([System.Windows.Automation.ValuePattern]::Pattern)");
        }

        [TestMethod]
        public void GalleryVisualChecksEnforceSplitViewPixelParityThreshold()
        {
            var source = File.ReadAllText(Path.Combine(
                GetRepoRoot(),
                "tools",
                "visual-checks",
                "Run-GalleryVisualChecks.ps1"));

            AssertContainsInOrder(
                source,
                "\"SplitView\" { return \"GallerySample_SplitView_SplitView\" }",
                "function New-SplitViewReferencePrimaryCrop([string]$caseDir, $window, [string]$screenshot, $sampleElement)",
                "$paneRoot = Find-DescendantByAutomationId $sampleElement \"PaneRoot\"",
                "$content = Find-DescendantByAutomationId $sampleElement \"content\"",
                "elseif ($control -eq \"AnnotatedScrollBar\")",
                "function Get-ReferencePrimaryCropMeanDeltaThreshold([string]$control)",
                "\"SplitView\" { return 4.0 }",
                "function Get-ReferencePrimaryCropSizeDeltaThreshold([string]$control)",
                "\"SplitView\" { return 0 }");
        }

        [TestMethod]
        public void GalleryVisualChecksEnforcePersonPicturePixelParityThreshold()
        {
            var source = File.ReadAllText(Path.Combine(
                GetRepoRoot(),
                "tools",
                "visual-checks",
                "Run-GalleryVisualChecks.ps1"));

            AssertContainsInOrder(
                source,
                "\"PersonPicture\" { return \"GallerySample_PersonPicture_PersonPicture\" }",
                "function New-PersonPictureReferencePrimaryCrop([string]$caseDir, $window, [string]$screenshot, $sampleElement)",
                "$modernPrimaryArtifact = Join-Path $modernArtifactDir \"GallerySample_PersonPicture_PersonPicture.png\"",
                "elseif ($control -eq \"PersonPicture\")",
                "function Get-ReferencePrimaryCropMeanDeltaThreshold([string]$control)",
                "\"PersonPicture\" { return 0.5 }",
                "function Get-ReferencePrimaryCropSizeDeltaThreshold([string]$control)",
                "\"PersonPicture\" { return 0 }");
        }

        [TestMethod]
        public void GalleryVisualChecksEnforceThemeShadowPixelParityThreshold()
        {
            var source = File.ReadAllText(Path.Combine(
                GetRepoRoot(),
                "tools",
                "visual-checks",
                "Run-GalleryVisualChecks.ps1"));

            AssertContainsInOrder(
                source,
                "\"ThemeShadow\" { return \"GallerySample_ThemeShadow_ShadowRect\" }",
                "function New-ThemeShadowModernPrimaryCrop([string]$caseDir)",
                "$demoBodySize = [Math]::Min($modernSize.Width, $modernSize.Height)",
                "function New-ThemeShadowReferencePrimaryCrop([string]$caseDir, $window, [string]$screenshot, $sampleElement)",
                "$controlExampleContentInsetX = 13",
                "$controlExampleContentInsetY = 12",
                "elseif ($control -eq \"ThemeShadow\")",
                "function Get-ReferencePrimaryCropMeanDeltaThreshold([string]$control)",
                "\"ThemeShadow\" { return 0.3 }",
                "function Get-ReferencePrimaryCropSizeDeltaThreshold([string]$control)",
                "\"ThemeShadow\" { return 0 }");
            StringAssert.Contains(
                source,
                "$themeShadowPrimary = New-ThemeShadowModernPrimaryCrop $caseDir");
        }

        [TestMethod]
        public void GalleryVisualChecksEnforceInfoBadgePixelParityThreshold()
        {
            var source = File.ReadAllText(Path.Combine(
                GetRepoRoot(),
                "tools",
                "visual-checks",
                "Run-GalleryVisualChecks.ps1"));

            AssertContainsInOrder(
                source,
                "\"InfoBadge\" { return \"GallerySample_InfoBadge_InfoBadge\" }",
                "function New-InfoBadgeReferencePrimaryCrop([string]$caseDir, $window, [string]$screenshot, $sampleElement)",
                "TryFind-DescendantByAutomationId $window \"nvSample1\"",
                "$navigationViewBounds = Get-ElementWindowBounds $window $navigationView",
                "$referenceAccent = Find-AccentComponentBounds $screenshot $searchBounds",
                "InfoBadge value badge",
                "elseif ($control -eq \"InfoBadge\")",
                "function Get-ReferencePrimaryCropMeanDeltaThreshold([string]$control)",
                "\"InfoBadge\" { return 5.0 }",
                "function Get-ReferencePrimaryCropSizeDeltaThreshold([string]$control)",
                "\"InfoBadge\" { return 0 }");
        }

        [TestMethod]
        public void GalleryVisualChecksEnforceNavigationViewCurrentSourcePixelParity()
        {
            var source = File.ReadAllText(Path.Combine(
                GetRepoRoot(),
                "tools",
                "visual-checks",
                "Run-GalleryVisualChecks.ps1"));
            var light = ReadRepoFile("ModernWpf", "ThemeResources", "Light.xaml");
            var dark = ReadRepoFile("ModernWpf", "ThemeResources", "Dark.xaml");
            var highContrast = ReadRepoFile("ModernWpf", "ThemeResources", "HighContrast.xaml");
            var template = ReadRepoFile("ModernWpf.Controls", "NavigationView", "NavigationView.xaml");
            var styles = ReadRepoFile("ModernWpf", "Styles", "NavigationView.xaml");
            var sample = ReadRepoFile("ModernWpf.Gallery", "Pages", "NavigationSampleFactory.cs");
            var diagnostics = ReadRepoFile("ModernWpf.Gallery", "Testing", "GalleryDiagnostics.cs");

            AssertContainsInOrder(
                source,
                "\"NavigationView\" { return \"GallerySample_NavigationView_NavigationView\" }",
                "function Test-ControlRequiresPrimaryCrop([string]$control)",
                "\"NavigationView\" { return $true }",
                "function Get-RequiredReferencePrimaryCropSource([string]$control)",
                "\"NavigationView\" { return \"nvSample5\" }",
                "function Get-ReferencePrimaryCropMeanDeltaThreshold([string]$control)",
                "\"NavigationView\" { return 1.5 }",
                "function Get-ReferencePrimaryCropSizeDeltaThreshold([string]$control)",
                "\"NavigationView\" { return 0 }");
            AssertContainsInOrder(
                source,
                "function Get-NavigationViewSelectionIndicatorEvidence([string]$path, [string]$orientation = \"Left\")",
                "No solid NavigationView selection-indicator pixels were found",
                "if ($control -eq \"NavigationView\")",
                "$modern[\"NavigationViewSelectionIndicatorComparison\"]",
                "NavigationView selection-indicator pixels differed");
            AssertContainsInOrder(
                source,
                "function Get-NavigationViewSampleArtifactEvidence([string]$artifactDirectory)",
                "GallerySample_NavigationView_NavigationView",
                "GallerySample_NavigationView_TopNavigationView",
                "IndicatorOrientation = \"Top\"",
                "GallerySample_NavigationView_AdaptiveNavigationView",
                "GallerySample_NavigationView_TabsNavigationView",
                "GallerySample_NavigationView_DataBindingNavigationView",
                "GallerySample_NavigationView_FooterNavigationView",
                "GallerySample_NavigationView_HierarchicalNavigationView",
                "GallerySample_NavigationView_ApiNavigationView",
                "Get-NavigationViewSelectionIndicatorEvidence $path $expectation.IndicatorOrientation",
                "NavigationView sample artifacts failed validation",
                "Get-NavigationViewSampleArtifactEvidence $artifactDir",
                "$navigationViewSampleArtifactsFailed",
                "NavigationView sample artifacts: $($sampleArtifacts.ValidCount)/$($sampleArtifacts.ExpectedCount) valid");
            AssertContainsInOrder(
                source,
                "function Capture-NavigationViewReferenceSampleArtifacts([string]$caseDirectory, $window)",
                "ReferenceAutomationId = \"nvSample5\"",
                "ReferenceAutomationId = \"nvSample6\"",
                "ReferenceAutomationId = \"nvSample2\"",
                "ReferenceAutomationId = \"nvSample7\"",
                "ReferenceAutomationId = \"nvSample4\"",
                "ReferenceAutomationId = \"nvSample9\"",
                "ReferenceAutomationId = \"nvSample8\"",
                "ReferenceAutomationId = \"nvSample\"",
                "function Get-NavigationViewSampleMeanDeltaThreshold([string]$automationId)",
                "\"GallerySample_NavigationView_AdaptiveNavigationView\" { return 4.25 }",
                "function Compare-NavigationViewSampleArtifactEvidence($modernEvidence, $referenceEvidence)",
                "$indicatorMatches",
                "$modern[\"NavigationViewSampleComparisons\"]",
                "NavigationView all-sample reference evidence was unavailable",
                "## NavigationView Sample Matrix",
                "NavigationView sample parity: $($sampleComparisons.ValidCount)/$($sampleComparisons.ExpectedCount) passed");

            StringAssert.Contains(light, "x:Key=\"NavigationViewDefaultPaneBackground\" ResourceKey=\"SystemControlPageBackgroundChromeLowBrush\"");
            StringAssert.Contains(light, "x:Key=\"NavigationViewContentBackground\" ResourceKey=\"LayerFillColorDefaultBrush\"");
            StringAssert.Contains(light, "x:Key=\"NavigationViewItemHeaderForeground\" ResourceKey=\"TextFillColorSecondaryBrush\"");
            StringAssert.Contains(light, "x:Key=\"NavigationViewContentGridBorderBrush\" ResourceKey=\"CardStrokeColorDefaultBrush\"");
            StringAssert.Contains(dark, "x:Key=\"NavigationViewDefaultPaneBackground\" ResourceKey=\"SystemControlBackgroundChromeMediumBrush\"");
            StringAssert.Contains(dark, "x:Key=\"NavigationViewContentBackground\" ResourceKey=\"LayerFillColorDefaultBrush\"");
            StringAssert.Contains(highContrast, "x:Key=\"NavigationViewContentBackground\" ResourceKey=\"SystemColorWindowColorBrush\"");

            StringAssert.Contains(template, "Background=\"{DynamicResource NavigationViewItemIconBackground}\"");
            StringAssert.Contains(template, "BorderBrush=\"{DynamicResource NavigationViewContentGridBorderBrush}\"");
            StringAssert.Contains(template, "Value=\"{DynamicResource NavigationViewItemHeaderForeground}\"");
            StringAssert.Contains(template, "<Setter Property=\"MinHeight\" Value=\"{DynamicResource NavigationViewTopPaneHeight}\" />");
            StringAssert.Contains(template, "Margin=\"{DynamicResource TopNavigationViewTopNavGridMargin}\"");
            StringAssert.Contains(template, "VerticalScrollBarVisibility=\"Disabled\"");
            StringAssert.Contains(template, "Margin=\"{DynamicResource NavigationViewPaneContentGridMargin}\"");
            StringAssert.Contains(template, "FontFamily=\"{DynamicResource ContentControlThemeFontFamily}\"");
            StringAssert.Contains(styles, "<Thickness x:Key=\"NavigationViewHeaderMargin\">56,45,0,0</Thickness>");
            StringAssert.Contains(styles, "<Thickness x:Key=\"NavigationViewContentPresenterMargin\">48,13,0,0</Thickness>");
            AssertContainsInOrder(
                sample,
                "Text = SamplePageLoremIpsum",
                "Margin = new Thickness(0, 1, 0, 0)",
                "text.SetResourceReference(FrameworkElement.StyleProperty, \"BodyTextBlockStyle\")",
                "text.SetResourceReference(TextBlock.ForegroundProperty, \"TextFillColorPrimaryBrush\")");
            AssertContainsInOrder(
                sample,
                "GalleryAutomation.SampleElementId(\"NavigationView\", \"NavigationView\")",
                "GalleryAutomation.SampleElementId(\"NavigationView\", \"TopNavigationView\")",
                "GalleryAutomation.SampleElementId(\"NavigationView\", \"AdaptiveNavigationView\")",
                "GalleryAutomation.SampleElementId(\"NavigationView\", \"TabsNavigationView\")",
                "GalleryAutomation.SampleElementId(\"NavigationView\", \"DataBindingNavigationView\")",
                "GalleryAutomation.SampleElementId(\"NavigationView\", \"FooterNavigationView\")",
                "GalleryAutomation.SampleElementId(\"NavigationView\", \"HierarchicalNavigationView\")",
                "GalleryAutomation.SampleElementId(\"NavigationView\", \"ApiNavigationView\")");
            AssertContainsInOrder(
                sample,
                "GalleryAutomation.SampleElementId(\"NavigationView\", \"DataBindingNavigationView\")",
                "HookNavigationHeaderSelection(navigationView);",
                "navigationView.Header = \"Sample Page 1\";",
                "GalleryAutomation.SampleElementId(\"NavigationView\", \"FooterNavigationView\")",
                "navigationView.Width = 592.0;",
                "navigationView.HorizontalAlignment = HorizontalAlignment.Left;",
                "GalleryAutomation.SampleElementId(\"NavigationView\", \"HierarchicalNavigationView\")",
                "navigationView.Width = 565.0;",
                "navigationView.HorizontalAlignment = HorizontalAlignment.Left;",
                "GalleryAutomation.SampleElementId(\"NavigationView\", \"ApiNavigationView\")",
                "navigationView.Width = 458.0;",
                "var contentFrame = new Frame",
                "navigationView.Content = contentFrame;",
                "navigationView.Height = 540;",
                "contentFrame.Content = CreateNavigationSampleContent();",
                "optionsContent = CreateNavigationViewApiOptions(");
            AssertContainsInOrder(
                diagnostics,
                "element is ModernWpf.Controls.NavigationView",
                "automationId.StartsWith(\"GallerySample_NavigationView_\", StringComparison.Ordinal)");
        }

        [TestMethod]
        public void GalleryVisualChecksEnforceAppBarFamilyCurrentSourcePixelAndInteractionParity()
        {
            var source = File.ReadAllText(Path.Combine(
                GetRepoRoot(),
                "tools",
                "visual-checks",
                "Run-GalleryVisualChecks.ps1"));

            AssertContainsInOrder(
                source,
                "function Test-ControlRequiresPrimaryCrop([string]$control)",
                "\"AppBarButton\" { return $true }",
                "\"AppBarSeparator\" { return $true }",
                "\"AppBarToggleButton\" { return $true }",
                "function Get-RequiredReferencePrimaryCropSource([string]$control)",
                "\"AppBarButton\" { return \"Button1\" }",
                "\"AppBarSeparator\" { return \"Control1\" }",
                "\"AppBarToggleButton\" { return \"Button1\" }");
            AssertContainsInOrder(
                source,
                "function Test-ControlSupportsOutputInteraction([string]$control)",
                "\"AppBarButton\" { return $true }",
                "function Get-OutputInteractionTriggerNames([string]$control)",
                "\"AppBarButton\" { return @(\"SymbolIcon\") }",
                "function Get-OutputInteractionCropAutomationId([string]$control)",
                "\"AppBarButton\" { return \"Control1Output\" }",
                "function Get-OutputInteractionExpectedNames([string]$control)",
                "\"AppBarButton\" { return @(\"You clicked: Button1\") }",
                "function Test-OutputInteractionAllowsBlankBaseline([string]$control)",
                "\"AppBarButton\" { return $true }");
            AssertContainsInOrder(
                source,
                "function Capture-OutputInteraction([string]$app, [string]$control, [string]$caseDir, $window, $sampleElement)",
                "$cropAutomationId = Get-OutputInteractionCropAutomationId $control",
                "$app -eq \"ModernWpf\" -and $control -eq \"AppBarButton\"",
                "$cropAutomationId = \"GallerySample_AppBarButton_Output\"");
            AssertContainsInOrder(
                source,
                "function Test-ControlRequiresReferenceInteractionCropParity([string]$control)",
                "\"AppBarButton\" { return $true }",
                "\"AppBarToggleButton\" { return $true }",
                "function Get-ReferencePrimaryCropMeanDeltaThreshold([string]$control)",
                "\"AppBarButton\" { return 5.0 }",
                "\"AppBarSeparator\" { return 1.0 }",
                "\"AppBarToggleButton\" { return 5.0 }",
                "function Get-ReferencePrimaryCropSizeDeltaThreshold([string]$control)",
                "\"AppBarButton\" { return 0 }",
                "\"AppBarSeparator\" { return 0 }",
                "\"AppBarToggleButton\" { return 0 }",
                "function Get-ReferenceInteractionCropMeanDeltaThreshold([string]$control)",
                "\"AppBarButton\" { return 7.0 }",
                "\"AppBarToggleButton\" { return 3.0 }",
                "function Get-ReferenceInteractionCropSizeDeltaThreshold([string]$control)",
                "\"AppBarButton\" { return 2 }",
                "\"AppBarToggleButton\" { return 0 }");
        }

        [TestMethod]
        public void GalleryVisualChecksEnforceContentDialogCurrentSourceSurfaceParity()
        {
            var source = File.ReadAllText(Path.Combine(
                GetRepoRoot(),
                "tools",
                "visual-checks",
                "Run-GalleryVisualChecks.ps1"));

            AssertContainsInOrder(
                source,
                "function Test-ControlRequiresPrimaryCrop([string]$control)",
                "\"ContentDialog\" { return $true }",
                "function Get-RequiredReferencePrimaryCropSource([string]$control)",
                "\"ContentDialog\" { return \"ShowDialog\" }");
            AssertContainsInOrder(
                source,
                "function Get-ContentDialogOpenSurfaceElements($window)",
                "Find-ElementsByNameInProcess $processId @(\"Save your work?\")",
                "\"Lorem ipsum dolor sit amet, adipisicing elit.\"",
                "\"Upload your content to the cloud.\"",
                "function Find-ImageRowTransition",
                "function Save-ContentDialogOpenSurfaceCrop",
                "Source = \"ContentDialogSurface\"",
                "$contentDialogSurfaceCrop = Save-ContentDialogOpenSurfaceCrop $window $selectedFrame.Screenshot $surfaceCropPath");
            AssertContainsInOrder(
                source,
                "function Test-ControlRequiresReferenceInteractionCropParity([string]$control)",
                "\"ContentDialog\" { return $true }",
                "function Get-ReferencePrimaryCropMeanDeltaThreshold([string]$control)",
                "\"ContentDialog\" { return 4.0 }",
                "function Get-ReferencePrimaryCropSizeDeltaThreshold([string]$control)",
                "\"ContentDialog\" { return 0 }",
                "function Get-ReferenceInteractionCropMeanDeltaThreshold([string]$control)",
                "\"ContentDialog\" { return 7.0 }",
                "function Get-ReferenceInteractionCropSizeDeltaThreshold([string]$control)",
                "\"ContentDialog\" { return 2 }");
        }

        [TestMethod]
        public void GalleryVisualChecksEnforceTeachingTipCurrentSourceSurfaceParity()
        {
            var source = File.ReadAllText(Path.Combine(
                GetRepoRoot(),
                "tools",
                "visual-checks",
                "Run-GalleryVisualChecks.ps1"));

            AssertContainsInOrder(
                source,
                "function Test-ControlRequiresPrimaryCrop([string]$control)",
                "\"TeachingTip\" { return $true }",
                "function Get-RequiredReferencePrimaryCropSource([string]$control)",
                "\"TeachingTip\" { return \"TestButton1\" }");
            AssertContainsInOrder(
                source,
                "function Find-TeachingTipMarginRowTransition(",
                "function Save-TeachingTipOpenSurfaceCrop(",
                "$geometrySource = \"PixelTransition\"",
                "Source = \"TeachingTipSurface\"",
                "$teachingTipSurfaceCrop = Save-TeachingTipOpenSurfaceCrop");
            AssertContainsInOrder(
                source,
                "function Test-ControlRequiresReferenceInteractionCropParity([string]$control)",
                "\"TeachingTip\" { return $true }",
                "function Get-ReferencePrimaryCropMeanDeltaThreshold([string]$control)",
                "\"TeachingTip\" { return 4.0 }",
                "function Get-ReferencePrimaryCropSizeDeltaThreshold([string]$control)",
                "\"TeachingTip\" { return 0 }",
                "function Get-ReferenceInteractionCropMeanDeltaThreshold([string]$control)",
                "\"TeachingTip\" { return 10.0 }",
                "function Get-ReferenceInteractionCropSizeDeltaThreshold([string]$control)",
                "\"TeachingTip\" { return 0 }");
        }

        [TestMethod]
        public void GalleryVisualChecksEnforcePopupCurrentSourceSurfaceParity()
        {
            var source = File.ReadAllText(Path.Combine(
                GetRepoRoot(),
                "tools",
                "visual-checks",
                "Run-GalleryVisualChecks.ps1"));

            AssertContainsInOrder(
                source,
                "function Test-ControlRequiresPrimaryCrop([string]$control)",
                "\"Popup\" { return $true }",
                "function Get-RequiredReferencePrimaryCropSource([string]$control)",
                "\"Popup\" { return \"Show Popup (using Offset)\" }");
            AssertContainsInOrder(
                source,
                "function Save-PopupOpenSurfaceCrop($window, $openElement, [string]$path)",
                "$edgeInset = 17",
                "Width = 240",
                "Height = $edgeInset + [int]$titleBounds.Height + 8 + 32 + $edgeInset",
                "Source = \"PopupSurface\"",
                "$popupSurfaceCrop = Save-PopupOpenSurfaceCrop $window $openElement $surfaceCropPath");
            AssertContainsInOrder(
                source,
                "function Test-ControlRequiresReferenceInteractionCropParity([string]$control)",
                "\"Popup\" { return $true }",
                "function Get-ReferencePrimaryCropMeanDeltaThreshold([string]$control)",
                "\"Popup\" { return 4.0 }",
                "function Get-ReferencePrimaryCropSizeDeltaThreshold([string]$control)",
                "\"Popup\" { return 0 }",
                "function Get-ReferenceInteractionCropMeanDeltaThreshold([string]$control)",
                "\"Popup\" { return 3.0 }",
                "function Get-ReferenceInteractionCropSizeDeltaThreshold([string]$control)",
                "\"Popup\" { return 0 }");
        }

        [TestMethod]
        public void GalleryVisualChecksEnforceFlyoutCurrentSourceSurfaceParity()
        {
            var source = File.ReadAllText(Path.Combine(
                GetRepoRoot(),
                "tools",
                "visual-checks",
                "Run-GalleryVisualChecks.ps1"));

            AssertContainsInOrder(
                source,
                "function Test-ControlRequiresPrimaryCrop([string]$control)",
                "\"Flyout\" { return $true }",
                "function Get-RequiredReferencePrimaryCropSource([string]$control)",
                "\"Flyout\" { return \"Control1\" }");
            AssertContainsInOrder(
                source,
                "function Save-FlyoutOpenSurfaceCrop($window, $openElement, [string]$path)",
                "$surfaceElement = Get-PopupScreenCropElement $window $openElement",
                "return Save-ScreenElementCrop $surfaceElement $path \"FlyoutOpenSurface\" 0 $window",
                "$flyoutSurfaceCrop = Save-FlyoutOpenSurfaceCrop $window $openElement $surfaceCropPath");
            AssertContainsInOrder(
                source,
                "function Test-ControlRequiresPopupWindowOpenProof([string]$control)",
                "\"Flyout\" { return $true }",
                "function Test-ControlRequiresReferenceInteractionCropParity([string]$control)",
                "\"Flyout\" { return $true }",
                "function Get-ReferencePrimaryCropMeanDeltaThreshold([string]$control)",
                "\"Flyout\" { return 3.0 }",
                "function Get-ReferencePrimaryCropSizeDeltaThreshold([string]$control)",
                "\"Flyout\" { return 0 }",
                "function Get-ReferenceInteractionCropMeanDeltaThreshold([string]$control)",
                "\"Flyout\" { return 11.0 }",
                "function Get-ReferenceInteractionCropSizeDeltaThreshold([string]$control)",
                "\"Flyout\" { return 1 }");
        }

        [TestMethod]
        public void GalleryVisualChecksEnforceMenuBarCurrentSourceSurfaceParity()
        {
            var source = File.ReadAllText(Path.Combine(
                GetRepoRoot(),
                "tools",
                "visual-checks",
                "Run-GalleryVisualChecks.ps1"));

            AssertContainsInOrder(
                source,
                "function Test-ControlRequiresPrimaryCrop([string]$control)",
                "\"MenuBar\" { return $true }",
                "function Get-RequiredReferencePrimaryCropSource([string]$control)",
                "\"MenuBar\" { return \"Example1\" }");
            AssertContainsInOrder(
                source,
                "function Save-MenuBarOpenSurfaceCrop($window, $openElement, $popupCrop, [string]$path)",
                "$surfaceElement = Get-PopupScreenCropElement $window $openElement",
                "return Save-ScreenElementCrop $surfaceElement $path \"MenuBarOpenSurface\" 0 $window",
                "$menuBarSurfaceCrop = Save-MenuBarOpenSurfaceCrop $window $openElement $menuBarPopupCrop $surfaceCropPath");
            AssertContainsInOrder(
                source,
                "function Test-ControlRequiresReferenceInteractionCropParity([string]$control)",
                "\"MenuBar\" { return $true }",
                "function Get-ReferencePrimaryCropMeanDeltaThreshold([string]$control)",
                "\"MenuBar\" { return 3.0 }",
                "function Get-ReferencePrimaryCropSizeDeltaThreshold([string]$control)",
                "\"MenuBar\" { return 0 }",
                "function Get-ReferenceInteractionCropMeanDeltaThreshold([string]$control)",
                "\"MenuBar\" { return 9.0 }",
                "function Get-ReferenceInteractionCropSizeDeltaThreshold([string]$control)",
                "\"MenuBar\" { return 2 }");
        }

        [TestMethod]
        public void GalleryVisualChecksEnforceMenuFlyoutCurrentSourceSurfaceParity()
        {
            var source = File.ReadAllText(Path.Combine(
                GetRepoRoot(),
                "tools",
                "visual-checks",
                "Run-GalleryVisualChecks.ps1"));

            AssertContainsInOrder(
                source,
                "function Test-ControlRequiresPrimaryCrop([string]$control)",
                "\"MenuFlyout\" { return $true }",
                "function Get-RequiredReferencePrimaryCropSource([string]$control)",
                "\"MenuFlyout\" { return \"Sort\" }");
            AssertContainsInOrder(
                source,
                "function Save-MenuFlyoutOpenSurfaceCrop($window, $openElement, [string]$path)",
                "$surfaceElement = Get-PopupScreenCropElement $window $openElement",
                "$rawCrop = Save-ScreenElementCrop $surfaceElement $rawPath \"MenuFlyoutOpenSurface\" 0 $window",
                "$horizontalOverhang = [int]$rawCrop.Width - [int]$itemBounds.Width",
                "$horizontalInset = [int][Math]::Floor(($horizontalOverhang - 4) / 2)",
                "Height = [int]$rawCrop.Height",
                "$menuFlyoutSurfaceCrop = Save-MenuFlyoutOpenSurfaceCrop $window $openElement $surfaceCropPath");
            AssertContainsInOrder(
                source,
                "function Test-ControlRequiresPopupWindowOpenProof([string]$control)",
                "\"MenuFlyout\" { return $true }",
                "function Test-ControlRequiresReferenceInteractionCropParity([string]$control)",
                "\"MenuFlyout\" { return $true }",
                "function Get-ReferencePrimaryCropMeanDeltaThreshold([string]$control)",
                "\"MenuFlyout\" { return 1.0 }",
                "function Get-ReferencePrimaryCropSizeDeltaThreshold([string]$control)",
                "\"MenuFlyout\" { return 0 }",
                "function Get-ReferenceInteractionCropMeanDeltaThreshold([string]$control)",
                "\"MenuFlyout\" { return 8.0 }",
                "function Get-ReferenceInteractionCropSizeDeltaThreshold([string]$control)",
                "\"MenuFlyout\" { return 0 }");
        }

        [TestMethod]
        public void GalleryVisualChecksEnforceCommandBarCurrentSourceSurfaceParity()
        {
            var source = File.ReadAllText(Path.Combine(
                GetRepoRoot(),
                "tools",
                "visual-checks",
                "Run-GalleryVisualChecks.ps1"));

            AssertContainsInOrder(
                source,
                "function Test-ControlRequiresPrimaryCrop([string]$control)",
                "\"CommandBar\" { return $true }",
                "function Get-RequiredReferencePrimaryCropSource([string]$control)",
                "\"CommandBar\" { return \"PrimaryCommandBar\" }");
            AssertContainsInOrder(
                source,
                "function Test-ControlRequiresPopupWindowOpenProof([string]$control)",
                "\"CommandBar\" { return $true }",
                "function Test-ControlRequiresReferenceInteractionCropParity([string]$control)",
                "\"CommandBar\" { return $true }");
            AssertContainsInOrder(
                source,
                "function Save-CommandBarOpenSurfaceCrop($window, $openElement, [string]$path)",
                "return Save-ScreenElementCrop $openElement $path \"CommandBarOpenSurface\" 0 $window",
                "$commandBarSurfaceCrop = Save-CommandBarOpenSurfaceCrop $window $openElement $surfaceCropPath");
            AssertContainsInOrder(
                source,
                "function Get-ReferencePrimaryCropMeanDeltaThreshold([string]$control)",
                "\"CommandBar\" { return 2.5 }",
                "function Get-ReferencePrimaryCropSizeDeltaThreshold([string]$control)",
                "\"CommandBar\" { return 0 }",
                "function Get-ReferenceInteractionCropMeanDeltaThreshold([string]$control)",
                "\"CommandBar\" { return 2.5 }",
                "function Get-ReferenceInteractionCropSizeDeltaThreshold([string]$control)",
                "\"CommandBar\" { return 0 }");
        }

        [TestMethod]
        public void GalleryVisualChecksEnforceHyperlinkButtonCurrentSourcePixelParity()
        {
            var source = File.ReadAllText(Path.Combine(
                GetRepoRoot(),
                "tools",
                "visual-checks",
                "Run-GalleryVisualChecks.ps1"));

            AssertContainsInOrder(
                source,
                "\"HyperlinkButton\" { return \"GallerySample_HyperlinkButton_HyperlinkButton\" }",
                "function Test-ControlRequiresPrimaryCrop([string]$control)",
                "\"HyperlinkButton\" { return $true }",
                "function Get-RequiredReferencePrimaryCropSource([string]$control)",
                "\"HyperlinkButton\" { return \"Control1\" }");
            AssertContainsInOrder(
                source,
                "function Get-ReferencePrimaryCropMeanDeltaThreshold([string]$control)",
                "\"HyperlinkButton\" { return 1.6 }",
                "function Get-ReferencePrimaryCropSizeDeltaThreshold([string]$control)",
                "\"HyperlinkButton\" { return 0 }");
        }

        [TestMethod]
        public void GalleryVisualChecksEnforceGridViewPixelParityThreshold()
        {
            var source = File.ReadAllText(Path.Combine(
                GetRepoRoot(),
                "tools",
                "visual-checks",
                "Run-GalleryVisualChecks.ps1"));

            AssertContainsInOrder(
                source,
                "function Test-ControlRequiresReferenceInteractionCropParity([string]$control)",
                "\"GridView\" { return $true }",
                "function Get-ReferencePrimaryCropMeanDeltaThreshold([string]$control)",
                "\"GridView\" { return 2.0 }",
                "function Get-ReferencePrimaryCropSizeDeltaThreshold([string]$control)",
                "\"GridView\" { return 0 }",
                "function Get-ReferenceInteractionCropMeanDeltaThreshold([string]$control)",
                "\"GridView\" { return 8.0 }",
                "function Get-ReferenceInteractionCropSizeDeltaThreshold([string]$control)",
                "\"GridView\" { return 4 }");
            AssertContainsInOrder(
                source,
                "function Compare-ImagesOnCommonCanvas([string]$leftPath, [string]$rightPath)",
                "$leftBackground = $left.GetPixel(0, 0)",
                "$rightBackground = $right.GetPixel(0, 0)",
                "foreach ($offsetX in -1..1)",
                "foreach ($offsetY in -1..1)",
                "AlignmentY = $best.OffsetY",
                "$modern[\"InteractionCropReferenceComparison\"] = if ($control -eq \"GridView\")",
                "Compare-ImagesOnCommonCanvas $modern.Interaction.Crop.Screenshot $referenceCapture.Interaction.Crop.Screenshot");
        }

        [TestMethod]
        public void GalleryVisualChecksEnforceCommandBarFlyoutPixelParityThreshold()
        {
            var source = File.ReadAllText(Path.Combine(
                GetRepoRoot(),
                "tools",
                "visual-checks",
                "Run-GalleryVisualChecks.ps1"));

            AssertContainsInOrder(
                source,
                "function Test-ControlRequiresReferenceInteractionCropParity([string]$control)",
                "\"CommandBarFlyout\" { return $true }",
                "function Get-ReferencePrimaryCropMeanDeltaThreshold([string]$control)",
                "\"CommandBarFlyout\" { return 6.0 }",
                "function Get-ReferencePrimaryCropSizeDeltaThreshold([string]$control)",
                "\"CommandBarFlyout\" { return 2 }",
                "function Get-ReferenceInteractionCropMeanDeltaThreshold([string]$control)",
                "\"CommandBarFlyout\" { return 9.0 }",
                "function Get-ReferenceInteractionCropSizeDeltaThreshold([string]$control)",
                "\"CommandBarFlyout\" { return 0 }");
        }

        [TestMethod]
        public void GalleryVisualChecksEnforceAnnotatedScrollBarPixelParityThreshold()
        {
            var source = File.ReadAllText(Path.Combine(
                GetRepoRoot(),
                "tools",
                "visual-checks",
                "Run-GalleryVisualChecks.ps1"));
            var diagnosticsSource = ReadRepoFile(
                "ModernWpf.Gallery",
                "Testing",
                "GalleryDiagnostics.cs");

            AssertContainsInOrder(
                source,
                "\"AnnotatedScrollBar\" { return \"GallerySample_AnnotatedScrollBar_AnnotatedScrollBar\" }",
                "function New-AnnotatedScrollBarReferencePrimaryCrop([string]$caseDir, $window, [string]$screenshot, $sampleElement)",
                "$scrollBounds.X + $scrollBounds.Width + 5",
                "elseif ($control -eq \"AnnotatedScrollBar\")",
                "function Get-ReferencePrimaryCropMeanDeltaThreshold([string]$control)",
                "\"AnnotatedScrollBar\" { return 1.5 }",
                "function Get-ReferencePrimaryCropSizeDeltaThreshold([string]$control)",
                "\"AnnotatedScrollBar\" { return 0 }");
            AssertContainsInOrder(
                diagnosticsSource,
                "private static Rect GetArtifactViewbox(FrameworkElement element, int width, int height)",
                "GallerySample_AnnotatedScrollBar_AnnotatedScrollBar",
                "return GetParentOffsetViewbox(element, width, height);");
        }

        [TestMethod]
        public void GalleryVisualChecksEnforceInfoBarPixelParityThreshold()
        {
            var source = File.ReadAllText(Path.Combine(
                GetRepoRoot(),
                "tools",
                "visual-checks",
                "Run-GalleryVisualChecks.ps1"));

            AssertContainsInOrder(
                source,
                "\"InfoBar\" { return \"GallerySample_InfoBar_InfoBar\" }",
                "\"InfoBar\" { return \"TestInfoBar1\" }",
                "function Get-ReferencePrimaryCropMeanDeltaThreshold([string]$control)",
                "\"InfoBar\" { return 2.0 }",
                "function Get-ReferencePrimaryCropSizeDeltaThreshold([string]$control)",
                "\"InfoBar\" { return 0 }");
        }

        [TestMethod]
        public void GalleryVisualChecksEnforceRatingControlPixelParityThreshold()
        {
            var source = File.ReadAllText(Path.Combine(
                GetRepoRoot(),
                "tools",
                "visual-checks",
                "Run-GalleryVisualChecks.ps1"));

            AssertContainsInOrder(
                source,
                "\"RatingControl\" { return \"GallerySample_RatingControl_RatingControl\" }",
                "\"RatingControl\" { return \"RatingControl1\" }",
                "function Get-ReferencePrimaryCropMeanDeltaThreshold([string]$control)",
                "\"RatingControl\" { return 7.0 }",
                "function Get-ReferencePrimaryCropSizeDeltaThreshold([string]$control)",
                "\"RatingControl\" { return 0 }",
                "function Get-ReferenceInteractionCropMeanDeltaThreshold([string]$control)",
                "\"RatingControl\" { return 5.0 }",
                "function Get-ReferenceInteractionCropSizeDeltaThreshold([string]$control)",
                "\"RatingControl\" { return 0 }");
            AssertContainsInOrder(
                source,
                "function Get-ReferenceBaselineNote([string]$control)",
                "Installed WinUI Gallery 2.9.3.0 / Windows App Runtime 2.2.3.0.0 renders PlaceholderValue=0 as one filled star.",
                "$referenceBaselineNote = Get-ReferenceBaselineNote $control",
                "$modern[\"ReferenceBaselineNote\"] = $referenceBaselineNote",
                "$referenceBaselineNotes = @(",
                "## Reference Baseline Notes",
                "reference baseline: \" + $result.ReferenceBaselineNote");
        }

        [TestMethod]
        public void GalleryVisualChecksCompareRenderedIconElementPixels()
        {
            var source = File.ReadAllText(Path.Combine(
                GetRepoRoot(),
                "tools",
                "visual-checks",
                "Run-GalleryVisualChecks.ps1"));
            var diagnosticsSource = ReadRepoFile(
                "ModernWpf.Gallery",
                "Testing",
                "GalleryDiagnostics.cs");

            AssertContainsInOrder(
                source,
                "function Get-ModernPrimaryCropAutomationId([string]$control)",
                "\"IconElement\" { return \"GallerySample_IconElement_SlicesIcon\" }",
                "function New-IconElementReferencePrimaryCrop([string]$caseDir, $window, [string]$screenshot)",
                "GallerySample_IconElement_SlicesIcon.png",
                "Cropped the rendered WinUI SlicesIcon below the first example body text.",
                "function Get-ReferencePrimaryCropMeanDeltaThreshold([string]$control)",
                "\"IconElement\" { return 0.1 }");
            AssertContainsInOrder(
                diagnosticsSource,
                "private static Rect GetArtifactViewbox(FrameworkElement element, int width, int height)",
                "GallerySample_IconElement_SlicesIcon",
                "return GetParentOffsetViewbox(element, width, height);");
        }

        [TestMethod]
        public void GalleryVisualChecksCompareRenderedImageIconPixelsOnIconElementPage()
        {
            var source = File.ReadAllText(Path.Combine(
                GetRepoRoot(),
                "tools",
                "visual-checks",
                "Run-GalleryVisualChecks.ps1"));

            AssertContainsInOrder(
                source,
                "function Get-RequiredSampleAutomationId([string]$control)",
                "\"ImageIcon\" { return \"GallerySample_IconElement_ImageExample1\" }",
                "function Get-SampleRootAutomationId([string]$control)",
                "function Test-ControlRequiresPrimaryCrop([string]$control)",
                "function Get-RequiredReferencePrimaryCropSource([string]$control)",
                "\"ImageIcon\" { return \"ImageExample1\" }");
            AssertContainsInOrder(
                source,
                "function Get-ModernPrimaryCropAutomationId([string]$control)",
                "\"ImageIcon\" { return \"GallerySample_IconElement_ImageExample1\" }",
                "function Get-ReferencePrimaryAutomationId([string]$control)",
                "\"ImageIcon\" { return \"ImageExample1\" }");
            AssertContainsInOrder(
                source,
                "function Get-WinUIReferencePageTitle([string]$control)",
                "\"ImageIcon\" { return \"IconElement\" }",
                "function Get-ControlRouteId([string]$control, [string]$app)",
                "return \"IconElement\"");
            AssertContainsInOrder(
                source,
                "$primaryCrop = New-RenderedArtifactCrop $primaryArtifact $primarySource $null",
                "if ($control -eq \"ImageIcon\")",
                "$primaryCrop = $null",
                "$sampleCrop = $null");
            AssertContainsInOrder(
                source,
                "function Get-ReferencePrimaryCropMeanDeltaThreshold([string]$control)",
                "\"ImageIcon\" { return 2.0 }",
                "function Get-ReferencePrimaryCropSizeDeltaThreshold([string]$control)",
                "\"ImageIcon\" { return 0 }");
        }

        [TestMethod]
        public void WinUIProgressBarOwnsCurrentGallerySurfaceAndStrictPixelGate()
        {
            var catalog = ReadRepoFile(
                "ModernWpf.Gallery",
                "Models",
                "GalleryCatalog.cs");
            var factory = ReadRepoFile(
                "ModernWpf.Gallery",
                "Pages",
                "StatusInfoSampleFactory.cs");
            var harness = ReadRepoFile(
                "tools",
                "visual-checks",
                "Run-GalleryVisualChecks.ps1");

            AssertContainsInOrder(
                catalog,
                "\"ProgressRing\"",
                "\"WinUIProgressBar\"",
                "\"InfoBadge\"",
                "var sourceItems = GalleryCatalogData.Items;",
                ".Concat(sourceItems.Where(sourceItem =>",
                "IsModernWpfExtensionItem(sourceItem.UniqueId)");
            Assert.IsFalse(
                catalog.Contains("CreateModernWpfProgressBarItem", StringComparison.Ordinal),
                "The WinUI ProgressBar route must come from the shared generated catalog metadata.");
            AssertContainsInOrder(
                factory,
                "private const string ProgressBarIndeterminateXaml",
                "<ProgressBar Width=\"\"130\"\" IsIndeterminate=\"\"True\"\" ShowPaused=\"\"$(ShowPaused)\"\" ShowError=\"\"$(ShowError)\"\" />",
                "private const string ProgressBarDeterminateXaml",
                "<ProgressBar Width=\"\"130\"\" Value=\"\"$(DeterminateProgressValue)\"\" />",
                "case \"WinUIProgressBar\":",
                "return CreateProgressBarExamples();",
                "AutomationProperties.SetName(progressBar, \"Determinate ProgressBar example\")",
                "GalleryAutomation.SampleElementId(\"WinUIProgressBar\", \"DeterminateProgressBar\")");
            AssertContainsInOrder(
                harness,
                "function Set-ProgressBarDeterminateValue($window, [string]$app, [double]$value)",
                "\"GallerySample_WinUIProgressBar_DeterminateProgressBar\"",
                "\"ProgressBar2\"",
                "$pattern.SetValue($value)",
                "function Get-RequiredSampleAutomationId([string]$control)",
                "\"WinUIProgressBar\" { return \"GallerySample_WinUIProgressBar_DeterminateProgressBar\" }");
            AssertContainsInOrder(
                harness,
                "function Get-RequiredReferencePrimaryCropSource([string]$control)",
                "\"WinUIProgressBar\" { return \"ProgressBar2\" }",
                "function Get-ModernPrimaryCropAutomationId([string]$control)",
                "\"WinUIProgressBar\" { return \"GallerySample_WinUIProgressBar_DeterminateProgressBar\" }",
                "function Get-ReferencePrimaryAutomationId([string]$control)",
                "\"WinUIProgressBar\" { return \"ProgressBar2\" }");
            AssertContainsInOrder(
                harness,
                "function Get-WinUIReferencePageTitle([string]$control)",
                "\"WinUIProgressBar\" { return \"ProgressBar\" }",
                "function Get-ControlRouteId([string]$control, [string]$app)",
                "if ($control -eq \"WinUIProgressBar\" -and $app -eq \"WinUI3\")",
                "return \"ProgressBar\"");
            AssertContainsInOrder(
                harness,
                "function Get-ReferencePrimaryCropMeanDeltaThreshold([string]$control)",
                "\"WinUIProgressBar\" { return 2.0 }",
                "function Get-ReferencePrimaryCropSizeDeltaThreshold([string]$control)",
                "\"WinUIProgressBar\" { return 0 }");
        }

        [TestMethod]
        public void GalleryVisualChecksTogglesCommonStateInteractionControls()
        {
            var source = File.ReadAllText(Path.Combine(
                GetRepoRoot(),
                "tools",
                "visual-checks",
                "Run-GalleryVisualChecks.ps1"));

            AssertContainsInOrder(
                source,
                "[string[]]$Controls = @(",
                "\"TeachingTip\", \"ColorPicker\", \"HyperlinkButton\", \"RatingControl\", \"RepeatButton\", \"ToggleButton\"",
                "\"CommandBar\", \"CommandBarFlyout\")",
                "$WpfGalleryOnlyVisualAuditCases = @(",
                "\"Button\"",
                "\"CheckBox\"",
                "\"ComboBox\"",
                "\"RadioButton\"",
                "\"Slider\"",
                "$wrongReferenceControls = @($Controls | Where-Object { $WpfGalleryOnlyVisualAuditCases -contains $_ })",
                "throw \"Run-GalleryVisualChecks.ps1 uses the WinUI Gallery reference.");
            AssertContainsInOrder(
                source,
                "function Test-ControlSupportsStateInteraction([string]$control)",
                "\"CheckBox\" { return $true }",
                "\"ToggleButton\" { return $true }",
                "\"ToggleSwitch\" { return $true }",
                "\"AppBarToggleButton\" { return $true }");
            AssertContainsInOrder(
                source,
                "function Get-StateInteractionSettleDelayMs([string]$control)",
                "\"ToggleSwitch\" { return 220 }",
                "default { return 180 }");
            AssertContainsInOrder(
                source,
                "function Test-StateInteractionVisual([string]$control, [string]$desiredState, [string]$cropPath)",
                "$control -ne \"ToggleSwitch\" -or $desiredState -ne \"On\"",
                "ToggleSwitch On screenshot left the thumb near x=");
            AssertContainsInOrder(
                source,
                "function Get-ReferencePrimaryAutomationId([string]$control)",
                "\"Slider\" { return \"Slider1\" }",
                "function Get-ReferencePrimaryName([string]$control)",
                "\"CheckBox\" { return \"Two-state\" }",
                "\"ToggleSwitch\" { return \"simple ToggleSwitch\" }");
            AssertContainsInOrder(
                source,
                "function Find-DescendantByNameAndControlType($root, [string]$name, $controlType)",
                "[System.Windows.Automation.AutomationElement]::ControlTypeProperty",
                "function Find-ReferencePrimaryByName($root, [string]$control, [string]$name)",
                "if ($control -eq \"CheckBox\")",
                "Find-DescendantByNameAndControlType $root $name ([System.Windows.Automation.ControlType]::CheckBox)",
                "return Find-DescendantButtonByName $root $name");
            AssertContainsInOrder(
                source,
                "function Get-ModernRenderedElementArtifactPath([string]$caseDir, $element)",
                "$automationId = $element.Current.AutomationId",
                "$path = Join-Path $caseDir (\"modernwpf-artifacts\\{0}.png\" -f $automationId)",
                "function Copy-RenderedArtifactCrop([string]$sourcePath, [string]$destinationPath, [string]$source)",
                "Copy-Item -LiteralPath $sourcePath -Destination $destinationPath -Force",
                "return New-RenderedArtifactCrop $destinationPath $source $null",
                "function Refresh-ModernWpfVisualArtifacts($window, [string]$artifactDir = \"\")",
                "TryFind-DescendantByAutomationId $window \"GalleryVisualTestRefreshArtifacts\"",
                "modernwpf-gallery-status.txt",
                "Invoke-ElementPatternOnce $window $refreshButton",
                "Wait-Until -TimeoutSeconds 10 -Description \"ModernWpf visual artifact refresh\"");
            AssertContainsInOrder(
                source,
                "function Capture-StateInteraction([string]$app, [string]$control, [string]$caseDir, $window, $element)",
                "if (!$IncludeInteractions -or !(Test-ControlSupportsStateInteraction $control))",
                "$toggleElement = Get-StateInteractionTarget $window $control $element",
                "$baselineState = Get-StateInteractionStateName $control $toggleElement",
                "$desiredState = if ($baselineState -eq \"On\") { \"Off\" } else { \"On\" }",
                "$renderedArtifactPath = if ($app -eq \"ModernWpf\") { Get-ModernRenderedElementArtifactPath $caseDir $element } else { \"\" }",
                "elseif (Test-Path $baselinePath)",
                "Save-ElementCrop $window $baselinePath $baselineCropPath $element \"UIA\" 10",
                "$baselineCrop = Copy-RenderedArtifactCrop $renderedArtifactPath $baselineCropPath $renderedArtifactSource",
                "$invoked = Set-StateInteractionElementState $window $control $toggleElement $desiredState",
                "$settleDelayMs = Get-StateInteractionSettleDelayMs $control",
                "Start-Sleep -Milliseconds $settleDelayMs",
                "$afterState = Get-StateInteractionStateName $control $toggleElement",
                "$stateOutputAutomationId = Get-StateInteractionOutputAutomationId $app $control",
                "$stateOutputMatched = [string]::IsNullOrEmpty($expectedStateOutput) -or $stateOutput -eq $expectedStateOutput",
                "[void](Refresh-ModernWpfVisualArtifacts $window (Join-Path $caseDir \"modernwpf-artifacts\"))",
                "elseif (Test-Path $afterPath)",
                "Save-ElementCrop $window $afterPath $afterCropPath $element \"UIA\" 10",
                "$afterCrop = Copy-RenderedArtifactCrop $renderedArtifactPath $afterCropPath $renderedArtifactSource",
                "$stateDelta = Compare-ImagesNormalized $baselineCrop.Screenshot $afterCrop.Screenshot",
                "$stateVisual = Test-StateInteractionVisual $control $desiredState $afterCropScreenshot",
                "$stateChanged = ![string]::IsNullOrEmpty($baselineState)",
                "$visualChanged = $null -ne $stateDelta -and $stateDelta.Comparable -and $stateDelta.MeanDelta -gt 0.5",
                "elseif (!$stateOutputMatched)",
                "elseif (!$stateVisual.Passed)");
            AssertContainsInOrder(
                source,
                "$openInteraction = Capture-OpenInteraction \"WinUI3\" $control $caseDir $window $showButton $openNames",
                "$stateInteraction = Capture-StateInteraction \"WinUI3\" $control $caseDir $window $showButton",
                "$selectionInteraction = Capture-SelectionInteraction \"WinUI3\" $control $caseDir $window $showButton",
                "$valueInteraction = Capture-ValueInteraction \"WinUI3\" $control $caseDir $window $showButton",
                "$outputInteraction = Capture-OutputInteraction \"WinUI3\" $control $caseDir $window $showButton",
                "$textInteraction = Capture-TextInteraction \"WinUI3\" $control $caseDir $window $showButton",
                "$interaction = if ($null -ne $openInteraction) { $openInteraction } elseif ($null -ne $stateInteraction) { $stateInteraction } elseif ($null -ne $selectionInteraction) { $selectionInteraction } elseif ($null -ne $valueInteraction) { $valueInteraction } elseif ($null -ne $outputInteraction) { $outputInteraction } else { $textInteraction }");
        }

        [TestMethod]
        public void GalleryVisualChecksClicksCommonSelectionInteractionControls()
        {
            var source = File.ReadAllText(Path.Combine(
                GetRepoRoot(),
                "tools",
                "visual-checks",
                "Run-GalleryVisualChecks.ps1"));

            AssertContainsInOrder(
                source,
                "function Test-ControlSupportsSelectionInteraction([string]$control)",
                "\"GridView\" { return $true }");
            AssertContainsInOrder(
                source,
                "function Get-SelectionInteractionTriggerName([string]$control)",
                "\"GridView\" { return \"Item 1\" }");
            AssertContainsInOrder(
                source,
                "function Get-SelectionInteractionExpectedName([string]$control)",
                "\"GridView\" { return \"You clicked Item 1.\" }");
            AssertContainsInOrder(
                source,
                "function Get-SelectionInteractionCropAutomationId([string]$control)",
                "\"GridView\" { return \"GallerySample_GridView_ClickOutput0\" }");
            AssertContainsInOrder(
                source,
                "$cropAutomationId = if ($app -eq \"WinUI3\" -and $control -eq \"GridView\")",
                "\"ClickOutput0\"",
                "Get-SelectionInteractionCropAutomationId $control");
            AssertContainsInOrder(
                source,
                "$afterCrop = if (Test-Path $afterPath)",
                "$savedBounds = Save-Crop $baselinePath $afterCrop.Bounds $baselineCropPath 0",
                "$baselineCrop = New-RenderedArtifactCrop $baselineCropPath \"UIA\" $savedBounds",
                "$outputDifferenceBounds = Find-DifferenceBounds $baselineCrop.Screenshot $afterCrop.Screenshot 32 1",
                "$tightBaselineBounds = Save-Crop $baselineCrop.Screenshot $outputDifferenceBounds $tightBaselineCropPath 4",
                "$tightAfterBounds = Save-Crop $afterCrop.Screenshot $outputDifferenceBounds $tightAfterCropPath 4",
                "$selectionDelta = $null");
            AssertContainsInOrder(
                source,
                "function Find-SelectionInvokeTarget($element)",
                "[System.Windows.Automation.SelectionItemPattern]::Pattern",
                "[System.Windows.Automation.InvokePattern]::Pattern",
                "[System.Windows.Automation.TreeWalker]::ControlViewWalker.GetParent($candidate)",
                "function Invoke-SelectionElementOnce($window, $element)",
                "$pattern = $target.GetCurrentPattern([System.Windows.Automation.SelectionItemPattern]::Pattern)",
                "$pattern.Select()",
                "[void](Invoke-ElementOnce $window $target)",
                "$pattern = $target.GetCurrentPattern([System.Windows.Automation.InvokePattern]::Pattern)",
                "$pattern.Invoke()");
            AssertContainsInOrder(
                source,
                "function Invoke-GridViewItemClickOnce([string]$app, $window)",
                "$item = Find-ElementByNameInProcess $window.Current.ProcessId @(\"Item 1\")",
                "$target = Find-SelectionInvokeTarget $item",
                "$pattern = $target.GetCurrentPattern([System.Windows.Automation.SelectionItemPattern]::Pattern)",
                "$pattern.Select()",
                "$pattern = $target.GetCurrentPattern([System.Windows.Automation.InvokePattern]::Pattern)",
                "$pattern.Invoke()");
            AssertContainsInOrder(
                source,
                "function Capture-SelectionInteraction([string]$app, [string]$control, [string]$caseDir, $window, $sampleElement)",
                "if (!$IncludeInteractions -or !(Test-ControlSupportsSelectionInteraction $control))",
                "$triggerName = Get-SelectionInteractionTriggerName $control",
                "$trigger = if ($null -ne $cropElement) { Find-DescendantByName $cropElement $triggerName } else { $null }",
                "$trigger = Find-DescendantByName $window $triggerName",
                "$invoked = if ($control -eq \"GridView\")",
                "Invoke-GridViewItemClickOnce $app $window",
                "Invoke-SelectionElementOnce $window $trigger",
                "$selectionDelta = Compare-ImagesNormalized $baselineCrop.Screenshot $afterCrop.Screenshot",
                "$expectedName = Get-SelectionInteractionExpectedName $control",
                "$visualChanged = $null -ne $selectionDelta -and $selectionDelta.Comparable -and $selectionDelta.MeanDelta -gt 0.5");
        }

        [TestMethod]
        public void GalleryVisualChecksActivatesConfiguredValueInteractions()
        {
            var source = File.ReadAllText(Path.Combine(
                GetRepoRoot(),
                "tools",
                "visual-checks",
                "Run-GalleryVisualChecks.ps1"));

            AssertContainsInOrder(
                source,
                "function Test-ControlSupportsValueInteraction([string]$control)",
                "\"RatingControl\" { return $true }",
                "\"Slider\" { return $true }",
                "\"NumberBox\" { return $true }",
                "function Get-ValueInteractionStep([string]$control)",
                "\"RatingControl\" { return 3.0 }",
                "\"Slider\" { return 50.0 }",
                "\"NumberBox\" { return 10.0 }",
                "function Get-ValueInteractionTargetValue([string]$control, $baselineValue)",
                "\"RatingControl\" { return 3.0 }",
                "\"Slider\" { return 50.0 }",
                "function Get-ValueInteractionCropAutomationId([string]$control)",
                "\"Slider\" { return \"GallerySample_Slider_Root\" }",
                "function Get-ValueInteractionIncreaseButtonNames([string]$control)",
                "\"NumberBox\" { return @(\"Increase\", \"Increase value\", \"Up\") }");
            AssertContainsInOrder(
                source,
                "function Find-DescendantButtonByAnyName($root, [string[]]$names)",
                "Find-DescendantButtonByName $root $name",
                "function Get-ElementNumericValue($element)",
                "[System.Windows.Automation.RangeValuePattern]::Pattern",
                "return [double]$pattern.Current.Value",
                "Find-EditableDescendant $element",
                "[System.Windows.Automation.ValuePattern]::Pattern",
                "return Try-ParseDouble $pattern.Current.Value");
            AssertContainsInOrder(
                source,
                "function Invoke-ValueIncreaseOnce($window, [string]$control, $element, $expectedValue)",
                "if ($control -eq \"RatingControl\" -or $control -eq \"Slider\")",
                "[System.Windows.Automation.RangeValuePattern]::Pattern",
                "$rangePattern.SetValue([double]$expectedValue)",
                "[System.Windows.Automation.ValuePattern]::Pattern",
                "$valuePattern.SetValue(([double]$expectedValue).ToString([System.Globalization.CultureInfo]::InvariantCulture))",
                "$buttonNames = Get-ValueInteractionIncreaseButtonNames $control",
                "$button = Find-DescendantButtonByAnyName $element $buttonNames",
                "Find-ElementByNameInProcess $window.Current.ProcessId $buttonNames",
                "return Invoke-ElementPatternOnce $window $button",
                "if ($control -eq \"NumberBox\")",
                "[GalleryVisualNative]::Click($x, $y)");
            AssertContainsInOrder(
                source,
                "function Capture-ValueInteraction([string]$app, [string]$control, [string]$caseDir, $window, $element)",
                "if (!$IncludeInteractions -or !(Test-ControlSupportsValueInteraction $control))",
                "$baselineValue = Get-ElementNumericValue $element",
                "$expectedValue = Get-ValueInteractionTargetValue $control $baselineValue",
                "$cropAutomationId = Get-ValueInteractionCropAutomationId $control",
                "TryFind-DescendantByAutomationId $window $cropAutomationId",
                "$cropElement = $element",
                "if ($null -ne $baselineCrop -and $baselineCrop.Contains(\"NonBlank\") -and !$baselineCrop.NonBlank)",
                "$baselineCrop = Save-ElementCrop $window $baselinePath $baselineCropPath $cropElement \"UIA\" 10",
                "$invoked = Invoke-ValueIncreaseOnce $window $control $element $expectedValue",
                "$afterValue = Get-ElementNumericValue $element",
                "if ($null -ne $afterCrop -and $afterCrop.Contains(\"NonBlank\") -and !$afterCrop.NonBlank)",
                "$afterCrop = Save-ElementCrop $window $afterPath $afterCropPath $cropElement \"UIA\" 10",
                "$valueDelta = Compare-ImagesNormalized $baselineCrop.Screenshot $afterCrop.Screenshot",
                "$valueChanged = (Test-DoubleApproximatelyEqual $afterValue $expectedValue)",
                "$visualChanged = $null -ne $valueDelta -and $valueDelta.Comparable -and $valueDelta.MeanDelta -gt 0.1",
                "$baselineNonBlank = $null -ne $baselineCrop -and $baselineCrop.Contains(\"NonBlank\") -and $baselineCrop.NonBlank",
                "$afterNonBlank = $null -ne $afterCrop -and $afterCrop.Contains(\"NonBlank\") -and $afterCrop.NonBlank",
                "$control value interaction crop was blank before or after activation.",
                "Kind = \"Value\"",
                "ExpectedValue = $expectedValue",
                "ValueAfter = $afterValue");
        }

        [TestMethod]
        public void GalleryVisualChecksActivatesRepeatButtonOutputInteraction()
        {
            var source = File.ReadAllText(Path.Combine(
                GetRepoRoot(),
                "tools",
                "visual-checks",
                "Run-GalleryVisualChecks.ps1"));

            AssertContainsInOrder(
                source,
                "function Test-ControlSupportsOutputInteraction([string]$control)",
                "\"RepeatButton\" { return $true }",
                "function Get-OutputInteractionTriggerNames([string]$control)",
                "\"RepeatButton\" { return @(\"Click and hold\") }",
                "function Get-OutputInteractionCropAutomationId([string]$control)",
                "\"RepeatButton\" { return \"GallerySample_RepeatButton_Output\" }",
                "function Get-OutputInteractionMinimumDelta([string]$control)",
                "\"RepeatButton\" { return 0.5 }",
                "function Test-OutputInteractionAllowsBlankBaseline([string]$control)",
                "\"RepeatButton\" { return $true }");
            AssertContainsInOrder(
                source,
                "function Capture-OutputInteraction([string]$app, [string]$control, [string]$caseDir, $window, $sampleElement)",
                "if (!$IncludeInteractions -or !(Test-ControlSupportsOutputInteraction $control))",
                "$triggerNames = Get-OutputInteractionTriggerNames $control",
                "$cropAutomationId = Get-OutputInteractionCropAutomationId $control",
                "TryFind-DescendantByAutomationId $window $cropAutomationId",
                "$trigger = if (Test-ElementNameMatches $sampleElement $triggerNames) { $sampleElement } else { $null }",
                "$trigger = Find-DescendantByAnyName $sampleElement $triggerNames",
                "$invoked = Invoke-ElementPatternOnce $window $trigger",
                "$outputDelta = Compare-ImagesNormalized $baselineCrop.Screenshot $afterCrop.Screenshot",
                "$minimumDelta = Get-OutputInteractionMinimumDelta $control",
                "$allowsBlankBaseline = Test-OutputInteractionAllowsBlankBaseline $control",
                "$baselineNonBlank = $null -ne $baselineCrop -and $baselineCrop.Contains(\"NonBlank\") -and $baselineCrop.NonBlank",
                "$afterNonBlank = $null -ne $afterCrop -and $afterCrop.Contains(\"NonBlank\") -and $afterCrop.NonBlank",
                "$visualChanged = ($null -ne $outputDelta -and $outputDelta.Comparable -and $outputDelta.MeanDelta -ge $minimumDelta)",
                "$allowsBlankBaseline -and !$baselineNonBlank -and $afterNonBlank",
                "Kind = \"Output\"",
                "OutputDelta = $outputDelta",
                "VisualChanged = $visualChanged");
        }

        [TestMethod]
        public void GalleryVisualChecksEnforceRepeatButtonCurrentSourceOutputRowParity()
        {
            var source = File.ReadAllText(Path.Combine(
                GetRepoRoot(),
                "tools",
                "visual-checks",
                "Run-GalleryVisualChecks.ps1"));

            AssertContainsInOrder(
                source,
                "function Save-RepeatButtonOutputSurfaceCrop($window, [string]$screenshot, [string]$path, $triggerElement)",
                "Source = \"RepeatButtonOutputRow\"",
                "Width = 240",
                "Height = 32",
                "Save-Crop $screenshot $surfaceBounds $path 0");
            AssertContainsInOrder(
                source,
                "$app -eq \"WinUI3\" -and $control -eq \"RepeatButton\"",
                "$cropAutomationId = \"Control1Output\"",
                "Save-RepeatButtonOutputSurfaceCrop $window $baselinePath $baselineCropPath $trigger",
                "Save-RepeatButtonOutputSurfaceCrop $window $afterPath $afterCropPath $trigger");
            AssertContainsInOrder(
                source,
                "function Test-ControlRequiresReferenceInteractionCropParity([string]$control)",
                "\"RepeatButton\" { return $true }",
                "function Get-ReferencePrimaryCropMeanDeltaThreshold([string]$control)",
                "\"RepeatButton\" { return 4.0 }",
                "function Get-ReferencePrimaryCropSizeDeltaThreshold([string]$control)",
                "\"RepeatButton\" { return 0 }",
                "function Get-ReferenceInteractionCropMeanDeltaThreshold([string]$control)",
                "\"RepeatButton\" { return 11.0 }",
                "function Get-ReferenceInteractionCropSizeDeltaThreshold([string]$control)",
                "\"RepeatButton\" { return 0 }");
        }

        [TestMethod]
        public void GalleryVisualChecksEnforceToggleButtonCurrentSourceStateAndOutputParity()
        {
            var source = File.ReadAllText(Path.Combine(
                GetRepoRoot(),
                "tools",
                "visual-checks",
                "Run-GalleryVisualChecks.ps1"));

            AssertContainsInOrder(
                source,
                "function Test-ControlRequiresPrimaryCrop([string]$control)",
                "\"ToggleButton\" { return $true }",
                "function Get-RequiredReferencePrimaryCropSource([string]$control)",
                "\"ToggleButton\" { return \"Toggle1\" }");
            AssertContainsInOrder(
                source,
                "function Get-StateInteractionOutputAutomationId([string]$app, [string]$control)",
                "\"GallerySample_ToggleButton_Output\"",
                "\"Control1Output\"",
                "function Get-StateInteractionExpectedOutput([string]$control, [string]$state)",
                "return $(if ($state -eq \"On\") { \"On\" } else { \"Off\" })",
                "$stateOutputAutomationId = Get-StateInteractionOutputAutomationId $app $control",
                "$expectedStateOutput = Get-StateInteractionExpectedOutput $control $desiredState",
                "$stateOutputMatched = [string]::IsNullOrEmpty($expectedStateOutput) -or $stateOutput -eq $expectedStateOutput",
                "elseif (!$stateOutputMatched)",
                "OutputMatched = $stateOutputMatched");
            AssertContainsInOrder(
                source,
                "function Test-ControlRequiresReferenceInteractionCropParity([string]$control)",
                "\"ToggleButton\" { return $true }",
                "function Get-ReferencePrimaryCropMeanDeltaThreshold([string]$control)",
                "\"ToggleButton\" { return 3.0 }",
                "function Get-ReferencePrimaryCropSizeDeltaThreshold([string]$control)",
                "\"ToggleButton\" { return 0 }",
                "function Get-ReferenceInteractionCropMeanDeltaThreshold([string]$control)",
                "\"ToggleButton\" { return 7.0 }",
                "function Get-ReferenceInteractionCropSizeDeltaThreshold([string]$control)",
                "\"ToggleButton\" { return 0 }");
        }

        [TestMethod]
        public void GalleryInteractionRecorderValidatesRepeatButtonAndSystemBackdropOutput()
        {
            var source = File.ReadAllText(Path.Combine(
                GetRepoRoot(),
                "tools",
                "visual-checks",
                "Record-GalleryControlInteractions.ps1"));

            AssertContainsInOrder(
                source,
                "function Get-ElementHelpText($element)",
                "[System.Windows.Automation.AutomationElement]::HelpTextProperty",
                "function Get-OutputInteractionExpectedOutput([string]$control)",
                "\"RepeatButton\" { return \"Number of clicks: 1\" }",
                "function Get-OutputInteractionElementText($element, [string]$control)",
                "if ($control -eq \"RepeatButton\")",
                "$helpText = Get-ElementHelpText $element",
                "return $helpText",
                "return Get-ElementText $element");
            AssertContainsInOrder(
                source,
                "function Invoke-OutputInteraction($window, [string]$control, $sampleElement)",
                "$expectedOutput = Get-OutputInteractionExpectedOutput $control",
                "$before = Get-OutputInteractionElementText $output $control",
                "Invoke-ElementOnce $window $sampleElement",
                "$after = Get-OutputInteractionElementText $output $control",
                "$outputMatched = if ($control -eq \"SystemBackdrop\")",
                "$after.StartsWith($expectedOutput, [StringComparison]::Ordinal)",
                "OutputMatched = $outputMatched");
            AssertContainsInOrder(
                source,
                "function Test-ControlSupportsOutputInteraction([string]$control)",
                "\"SystemBackdrop\" { return $true }",
                "function Get-OutputInteractionOutputAutomationId([string]$control)",
                "\"SystemBackdrop\" { return \"GallerySample_SystemBackdrop_Status\" }",
                "function Get-OutputInteractionExpectedOutput([string]$control)",
                "\"SystemBackdrop\" { return \"Requested Mica; effective material:\" }");
        }

        [TestMethod]
        public void GalleryInteractionRecorderRequiresAutoSuggestBoxSuggestionToClose()
        {
            var source = File.ReadAllText(Path.Combine(
                GetRepoRoot(),
                "tools",
                "visual-checks",
                "Record-GalleryControlInteractions.ps1"));

            AssertContainsInOrder(
                source,
                "private const byte VK_END = 0x23;",
                "public static void End()",
                "KeyPress(VK_END);");
            AssertContainsInOrder(
                source,
                "function Get-ControlRecordingDurationSeconds([string]$control, [string]$interactionKind)",
                "if ($interactionKind -eq \"Text\" -and $control -eq \"AutoSuggestBox\")",
                "return [Math]::Max($DurationSeconds, 18)");
            AssertContainsInOrder(
                source,
                "function Invoke-SuggestionKeyboardCommit($window, $editElement, [bool]$hasMatchedOutput)",
                "$editElement.SetFocus()",
                "[System.Windows.Forms.SendKeys]::SendWait(\"{ENTER}\")",
                "[System.Windows.Forms.SendKeys]::SendWait(\"{END}{DOWN}{ENTER}\")",
                "[GalleryRecordingNative]::End()",
                "[GalleryRecordingNative]::Down()",
                "[GalleryRecordingNative]::Enter()");
            AssertContainsInOrder(
                source,
                "function Wait-ForSuggestionClosed([int]$processId, [string[]]$names, [int]$timeoutMilliseconds)",
                "Find-InteractiveElementByNameInProcess $processId $names",
                "if ($null -eq $element)",
                "return $true",
                "return $false");
            AssertContainsInOrder(
                source,
                "function Find-InvokePatternTarget($element)",
                "Test-ElementSupportsPattern $candidate ([System.Windows.Automation.InvokePattern]::Pattern)",
                "return $candidate",
                "return $null");
            AssertContainsInOrder(
                source,
                "function Invoke-SuggestionElementOnce($window, $element)",
                "$invokeTarget = Find-InvokePatternTarget $element",
                "$pattern = $invokeTarget.GetCurrentPattern([System.Windows.Automation.InvokePattern]::Pattern)",
                "Method = \"InvokePattern\"",
                "if (Click-ElementOnce $element)",
                "Method = \"ElementClick\"",
                "if (Select-ElementOnce $window $element)",
                "Method = \"SelectionItem\"");
            AssertContainsInOrder(
                source,
                "function Get-TextVisualClosedEvidence($frames, $recordingResult, $interactionResult)",
                "InitialSuggestionBounds",
                "$baselineFrame = $extractedFrames[0]",
                "$finalFrame = $extractedFrames[$extractedFrames.Count - 1]",
                "$delta = Compare-LuminanceSamples $baselineSamples $finalSamples",
                "Closed = $roundedDelta -le 4.0");
            AssertContainsInOrder(
                source,
                "function Invoke-TextInteraction($window, [string]$control, $sampleElement)",
                "$initialSuggestionBounds = Format-BoundingRectangle (Get-ElementBoundingRectangle $suggestionElement)",
                "$suggestionClosed = $false",
                "$suggestionInvoked = Click-FirstSuggestionBelowEdit $editElement",
                "$suggestionClosed = Wait-ForSuggestionClosed $window.Current.ProcessId $suggestionNames 1200",
                "$suggestionInvokeResult = Invoke-SuggestionElementOnce $window $suggestionElement",
                "$suggestionInvokeMethod = $suggestionInvokeResult.Method",
                "$suggestionClosed = Wait-ForSuggestionClosed $window.Current.ProcessId $suggestionNames 1200",
                "Invoke-SuggestionKeyboardCommit $window $editElement ($null -ne $outputElement)",
                "$suggestionInvokeResult = Invoke-SuggestionElementOnce $window $suggestionElement",
                "$suggestionInvokeMethod = $suggestionInvokeResult.Method",
                "$suggestionClosed = Wait-ForSuggestionClosed $window.Current.ProcessId $suggestionNames 1200",
                "Invoke-SuggestionKeyboardCommit $window $editElement $true",
                "$suggestionInvokeMethod = \"SelectionItemKeyboard\"",
                "$suggestionClosed = Wait-ForSuggestionClosed $window.Current.ProcessId $suggestionNames 1200",
                "InitialSuggestionBounds = $initialSuggestionBounds",
                "SuggestionClosed = $suggestionClosed",
                "RemainingSuggestionBounds = $remainingSuggestionBounds");
            AssertContainsInOrder(
                source,
                "function Test-TextEvidence($interactionResult)",
                "if ($interactionResult.Contains(\"SuggestionClosed\") -and !$interactionResult.SuggestionClosed)",
                "return $false",
                "return [bool]$interactionResult.OutputMatched");
            AssertContainsInOrder(
                source,
                "$status -eq \"Passed\" -and $interactionKind -eq \"Text\" -and !$textEvidence",
                "$interactionResult.Contains(\"SuggestionClosed\")",
                "$notes.Add(\"Text interaction left the suggestion popup visible after the claimed suggestion choice.\")");
            AssertContainsInOrder(
                source,
                "$status -eq \"Passed\" -and",
                "$interactionKind -eq \"Text\" -and",
                "$control -eq \"AutoSuggestBox\"",
                "!$textVisualClosedEvidence.Generated",
                "!$textVisualClosedEvidence.Closed",
                "$notes.Add((\"Rendered final frame still differs inside the initial suggestion popup bounds.");
            AssertContainsInOrder(
                source,
                "TextEvidence = $textEvidence",
                "TextVisualClosedEvidence = $textVisualClosedEvidence");
        }

        [TestMethod]
        public void GalleryInteractionRecorderClicksHyperlinkButtonInAppNavigationSample()
        {
            var source = File.ReadAllText(Path.Combine(
                GetRepoRoot(),
                "tools",
                "visual-checks",
                "Record-GalleryControlInteractions.ps1"));

            AssertContainsInOrder(
                source,
                "function Test-ControlSupportsRouteNavigationInteraction([string]$control)",
                "\"HyperlinkButton\" { return $true }",
                "function Get-RouteNavigationTriggerAutomationId([string]$control)",
                "\"HyperlinkButton\" { return \"GallerySample_HyperlinkButton_ClickHyperlinkButton\" }",
                "function Get-RouteNavigationExpectedRoute([string]$control)",
                "\"HyperlinkButton\" { return \"item/ToggleButton\" }",
                "function Get-RouteNavigationExpectedSampleAutomationId([string]$control)",
                "\"HyperlinkButton\" { return \"GallerySample_ToggleButton_ToggleButton\" }");
            AssertContainsInOrder(
                source,
                "function Get-ControlInteractionKind([string]$control)",
                "if (Test-ControlSupportsOutputInteraction $control) { return \"Output\" }",
                "if (Test-ControlSupportsRouteNavigationInteraction $control) { return \"RouteNavigation\" }");
            AssertContainsInOrder(
                source,
                "function Invoke-RouteNavigationInteraction($window, [string]$control, [string]$artifactDir)",
                "$beforeStatus = if (![string]::IsNullOrWhiteSpace($artifactDir)) { Read-ModernWpfStatusFile $artifactDir } else { $null }",
                "$invoked = Invoke-ElementOnce $window $trigger",
                "$ready = Wait-ModernWpfReady $window $expectedRoute $artifactDir",
                "$targetSampleVisible = Test-AutomationElementUsable $targetSample",
                "RouteNavigationChanged = $invoked -and $afterRoute -eq $expectedRoute -and $targetSampleVisible");
            AssertContainsInOrder(
                source,
                "function Invoke-RecordedInteraction($window, [string]$control, $sampleElement, [string]$artifactDir = \"\")",
                "\"RouteNavigation\" { return Invoke-RouteNavigationInteraction $window $control $artifactDir }",
                "$interactionResult = Invoke-RecordedInteraction $window $control $sampleElement $artifactDir");
            AssertContainsInOrder(
                source,
                "function Test-RouteNavigationEvidence($interactionResult)",
                "$interactionResult.Contains(\"RouteNavigationChanged\")",
                "$routeNavigationEvidence = Test-RouteNavigationEvidence $interactionResult",
                "\"RouteNavigation\" { $interactionEvidenceForKind = $routeNavigationEvidence }",
                "$status -eq \"Passed\" -and $interactionKind -eq \"RouteNavigation\" -and !$routeNavigationEvidence",
                "RouteNavigationEvidence = $routeNavigationEvidence");
        }

        [TestMethod]
        public void GalleryInteractionRecorderScopesCommandBarFlyoutMoreButtonToPopup()
        {
            var source = File.ReadAllText(Path.Combine(
                GetRepoRoot(),
                "tools",
                "visual-checks",
                "Record-GalleryControlInteractions.ps1"));

            AssertContainsInOrder(
                source,
                "function Get-ElementNativeWindowHandle($element)",
                "$handle = [int]$candidate.Current.NativeWindowHandle",
                "function Find-ElementByAutomationIdInPopupWindows($window, [string]$automationId)",
                "$candidateWindow.Current.NativeWindowHandle -eq $mainHandle",
                "Find-DescendantByAutomationId $candidateWindow $automationId",
                "function Find-CommandBarFlyoutMoreButton($window)",
                "Find-InteractiveElementByNameInProcess $window.Current.ProcessId @(\"Share\", \"Save\", \"Delete\")",
                "$popupHandle = Get-ElementNativeWindowHandle $primaryCommand",
                "Find-TopLevelElementByNativeWindowHandleInProcess $window.Current.ProcessId ([int]$popupHandle)",
                "Find-ElementByAutomationIdInPopupWindows $window \"MoreButton\"",
                "function Wait-ForCommandBarFlyoutPrimaryCommands($window, [int]$timeoutMilliseconds)",
                "Find-InteractiveElementByNameInProcess $window.Current.ProcessId @(\"Share\")",
                "Find-InteractiveElementByNameInProcess $window.Current.ProcessId @(\"Save\")",
                "Find-InteractiveElementByNameInProcess $window.Current.ProcessId @(\"Delete\")",
                "Test-AutomationElementUsable $moreButton",
                "function Wait-ForInteractiveElementByNameInProcess([int]$processId, [string[]]$names, [int]$timeoutMilliseconds)",
                "function Open-CommandBarFlyoutSecondaryCommands($window)",
                "$deadline = (Get-Date).AddMilliseconds(2500)",
                "Wait-ForCommandBarFlyoutPrimaryCommands $window 1200",
                "Find-CommandBarFlyoutMoreButton $window",
                "Wait-ForInteractiveElementByNameInProcess $window.Current.ProcessId @(\"Resize\", \"Move\") 1200");
        }

        [TestMethod]
        public void GalleryInteractionRecorderExportsDenseTransitionReviewSheets()
        {
            var source = File.ReadAllText(Path.Combine(
                GetRepoRoot(),
                "tools",
                "visual-checks",
                "Record-GalleryControlInteractions.ps1"));

            AssertContainsInOrder(
                source,
                "function Test-ControlRequiresDenseTransitionReview([string]$control, [string]$interactionKind)",
                "$interactionKind -eq \"ShellNavigation\"",
                "$interactionKind -ne \"OpenRepeat\"",
                "\"ContentDialog\" { return $true }",
                "\"MenuBar\" { return $true }",
                "\"CommandBarFlyout\" { return $true }");
            AssertContainsInOrder(
                source,
                "function Export-DenseTransitionReviewSheet([string]$videoPath, [string]$caseDir, [int]$durationSeconds)",
                "$analysisDir = Join-Path $caseDir \"analysis\"",
                "$sheetPath = Join-Path $analysisDir \"dense-transition-review.jpg\"",
                "$tileColumns = 8",
                "$effectiveDuration = if ($null -ne $actualDuration -and $actualDuration -gt 0.5) { $actualDuration } else { [double]$durationSeconds }",
                "$filter = \"fps=$reviewFps,scale=360:-1,tile=${tileColumns}x$tileRows\"",
                "Path = $sheetPath",
                "Generated = $true");
            AssertContainsInOrder(
                source,
                "| Control | Status | Interaction | Duration | Recording | Dense review | Max frame delta | Max local delta | Notes |",
                "{0:0.###}s/{1}s",
                "DenseTransitionReview",
                "if (Test-ControlRequiresDenseTransitionReview $control $interactionKind)",
                "$denseTransitionReview = Export-DenseTransitionReviewSheet $recordingPath $caseDir $recordingDurationSeconds",
                "Dense transition review sheet generated",
                "DenseTransitionReview = $denseTransitionReview");
        }

        [TestMethod]
        public void GalleryInteractionRecorderCheckpointsManifestAfterEachControl()
        {
            var source = File.ReadAllText(Path.Combine(
                GetRepoRoot(),
                "tools",
                "visual-checks",
                "Record-GalleryControlInteractions.ps1"));

            AssertContainsInOrder(
                source,
                "function Write-RunCheckpoint([string]$runDir, $results)",
                "$manifestPath = Join-Path $runDir \"recording-manifest.json\"",
                "$manifestTempPath = Join-Path $runDir \"recording-manifest.json.tmp\"",
                "$results | ConvertTo-Json -Depth 8 | Set-Content -Path $manifestTempPath -Encoding UTF8",
                "Move-Item -Path $manifestTempPath -Destination $manifestPath -Force",
                "$reportPath = Write-Report $runDir $results");
            AssertContainsInOrder(
                source,
                "$results.Add($result)",
                "[void](Write-RunCheckpoint $runDir $results)",
                "$checkpoint = Write-RunCheckpoint $runDir $results",
                "Manifest = $checkpoint.Manifest",
                "Report = $checkpoint.Report");
        }

        [TestMethod]
        public void GalleryInteractionRecorderStopsRecordingAfterEvidenceTail()
        {
            var source = File.ReadAllText(Path.Combine(
                GetRepoRoot(),
                "tools",
                "visual-checks",
                "Record-GalleryControlInteractions.ps1"));
            var recorder = File.ReadAllText(Path.Combine(
                GetRepoRoot(),
                "tools",
                "visual-checks",
                "Record-WindowRendered.ps1"));

            AssertContainsInOrder(
                recorder,
                "[string]$VideoEncoder = \"Auto\"",
                "[string]$StopFile = \"\"",
                "[switch]$KeepFrames,",
                "[switch]$BenchmarkEncoders",
                "function Get-VideoEncoderCandidates([string]$requestedEncoder, [string]$ffmpegPath)",
                "foreach ($encoder in @(\"libx264\", \"h264_nvenc\", \"h264_qsv\", \"h264_amf\"))",
                "$actualFrameCount = 0",
                "if ($actualFrameCount -gt 0 -and",
                "(Test-Path -LiteralPath $StopFile))",
                "$actualFrameCount++",
                "$encoderCandidates = if ($BenchmarkEncoders)",
                "Sort-Object ElapsedSeconds",
                "Frames = $actualFrameCount",
                "DurationSeconds = [Math]::Round($actualFrameCount / [double]$FrameRate, 3)",
                "RequestedDurationSeconds = $DurationSeconds",
                "VideoEncoder = $usedEncoder",
                "BenchmarkEncoders = [bool]$BenchmarkEncoders",
                "EncoderAttempts = $encoderAttempts.ToArray()");
            AssertContainsInOrder(
                source,
                "[string]$VideoEncoder = \"Auto\"",
                "[switch]$BenchmarkEncoders",
                "function Start-RecordingJob([int]$processId, [IntPtr]$windowHandle, [string]$outputPath, [string]$captureMode, [int]$durationSeconds, [string]$videoEncoder, [bool]$benchmarkEncoders, [int]$frameRate)",
                "$stopFile = Join-Path (Split-Path -Parent $outputPath)",
                "-VideoEncoder $encoder",
                "-BenchmarkEncoders:$benchmark",
                "-StopFile $stopSignal",
                "StopFile = $stopFile",
                "RequestedVideoEncoder = $videoEncoder",
                "BenchmarkEncoders = $benchmarkEncoders",
                "function Request-RecordingStop($recordingJob)",
                "New-Item -ItemType File -Path $recordingJob.StopFile -Force | Out-Null",
                "function Wait-RecordingJob($recordingJob, [int]$durationSeconds)",
                "function Close-GalleryRecordingProcess($process)");
            AssertContainsInOrder(
                source,
                "$recordingFrameRate = Get-ControlRecordingFrameRate $control $interactionKind",
                "$recordingJob = Start-RecordingJob $window.Current.ProcessId ([IntPtr]$window.Current.NativeWindowHandle) $recordingPath $CaptureMode $recordingDurationSeconds $VideoEncoder ([bool]$BenchmarkEncoders) $recordingFrameRate",
                "$interactionResult = Invoke-RecordedInteraction $window $control $sampleElement $artifactDir",
                "Start-Sleep -Milliseconds 350",
                "Request-RecordingStop $recordingJob",
                "Start-Sleep -Milliseconds 250",
                "Close-GalleryRecordingProcess $process",
                "$recordingResult = Wait-RecordingJob $recordingJob $recordingDurationSeconds",
                "ActualRecordingDurationSeconds = if ($null -ne $recordingResult -and $null -ne $recordingResult.DurationSeconds)");
        }

        [TestMethod]
        public void GalleryInteractionRecorderRejectsMostlyBlankRecordings()
        {
            var source = File.ReadAllText(Path.Combine(
                GetRepoRoot(),
                "tools",
                "visual-checks",
                "Record-GalleryControlInteractions.ps1"));

            AssertContainsInOrder(
                source,
                "function Get-NonBlankFrameCount($frames)",
                "function Get-ExtractedFrameCount($frames)",
                "function Get-MinimumNonBlankFrameCount([int]$extractedFrameCount)",
                "return [Math]::Max(2, [int][Math]::Ceiling($extractedFrameCount * 0.75))");
            AssertContainsInOrder(
                source,
                "$nonBlankFrameCount = Get-NonBlankFrameCount $frames",
                "$extractedFrameCount = Get-ExtractedFrameCount $frames",
                "$minimumNonBlankFrameCount = Get-MinimumNonBlankFrameCount $extractedFrameCount",
                "if ($nonBlankFrameCount -lt $minimumNonBlankFrameCount -and !$SkipFrameExtraction)",
                "Only {0} of {1} extracted poster frames were nonblank; at least {2} are required.");
            AssertContainsInOrder(
                source,
                "function Test-FrameExtracted($frame)",
                "if ($frame -is [System.Collections.IDictionary])",
                "return $frame.Contains(\"Extracted\") -and [bool]$frame.Extracted",
                "function Get-LocalFrameDeltas($frames, $recordingResult, $interactionResult)",
                "Where-Object { Test-FrameExtracted $_ }",
                "function Get-NonBlankFrameCount($frames)",
                "if ((Test-FrameExtracted $frame) -and $null -ne $frame.Stats -and $frame.Stats.NonBlank)",
                "function Get-ExtractedFrameCount($frames)",
                "if (Test-FrameExtracted $frame)");
        }

        [TestMethod]
        public void GalleryInteractionRecorderRejectsScreenCapturesWithoutGalleryAnchor()
        {
            var source = File.ReadAllText(Path.Combine(
                GetRepoRoot(),
                "tools",
                "visual-checks",
                "Record-GalleryControlInteractions.ps1"));

            AssertContainsInOrder(
                source,
                "function Compare-FrameWindowRegionToAnchorMeanDelta([string]$framePath, [string]$anchorPath, [string]$windowBoundsText, [string]$captureRectText)",
                "ConvertTo-FrameRectangle $windowBounds $captureRect $frame.Width $frame.Height 0",
                "$width = [Math]::Min([int]$region.Width, $anchor.Width)",
                "return [Math]::Round($sum / $count, 3)");
            AssertContainsInOrder(
                source,
                "function Get-ScreenRecordingGalleryAnchor($frames, $recordingResult, [string]$windowBounds, [string]$artifactDir)",
                "$threshold = 25.0",
                "$anchorPath = Join-Path $artifactDir \"ModernWpfGalleryMainWindow.png\"",
                "$frame = Get-FirstNonBlankExtractedFrame $frames",
                "$delta = Compare-FrameWindowRegionToAnchorMeanDelta $frame.Path $anchorPath $windowBounds $recordingResult.Rect",
                "Matched = ([double]$delta -le $threshold)");
            AssertContainsInOrder(
                source,
                "$windowBounds = Format-NativeWindowRectangle ([GalleryRecordingNative]::GetRect([IntPtr]$window.Current.NativeWindowHandle))",
                "if ($CaptureMode -eq \"Screen\" -and !$SkipFrameExtraction)",
                "$screenRecordingGalleryAnchor = Get-ScreenRecordingGalleryAnchor $frames $recordingResult $windowBounds $artifactDir",
                "Screen capture did not match the rendered Gallery window anchor.",
                "ScreenRecordingGalleryAnchor = $screenRecordingGalleryAnchor");
        }

        [TestMethod]
        public void GalleryInteractionRecorderSelectsRealGalleryWindowOverInputOverlays()
        {
            var source = File.ReadAllText(Path.Combine(
                GetRepoRoot(),
                "tools",
                "visual-checks",
                "Record-GalleryControlInteractions.ps1"));

            AssertContainsInOrder(
                source,
                "[int]$WindowLeft = 0,",
                "[int]$WindowTop = 0,",
                "[void][GalleryRecordingNative]::Move($window.Current.NativeWindowHandle, $WindowLeft, $WindowTop, $Width, $Height)");
            AssertContainsInOrder(
                source,
                "function Find-WindowByProcessId([int]$processId)",
                "$bestWindow = $null",
                "$bestScore = -1",
                "$handle = [IntPtr]$window.Current.NativeWindowHandle",
                "$rect = [GalleryRecordingNative]::GetRect($handle)",
                "$width = $rect.Right - $rect.Left",
                "$height = $rect.Bottom - $rect.Top",
                "if ($width -lt 400 -or $height -lt 300)",
                "if ($window.Current.AutomationId -eq \"ModernWpfGalleryMainWindow\")",
                "if ($window.Current.ClassName -eq \"Window\")",
                "if ($null -eq $bestWindow)",
                "[System.Diagnostics.Process]::GetProcessById($processId)",
                "$process.MainWindowHandle",
                "[System.Windows.Automation.AutomationElement]::FromHandle($process.MainWindowHandle)",
                "return $bestWindow");
        }

        [TestMethod]
        public void GalleryInteractionRecorderRequiresShellNavigationPointerDisclosureClicks()
        {
            var source = File.ReadAllText(Path.Combine(
                GetRepoRoot(),
                "tools",
                "visual-checks",
                "Record-GalleryControlInteractions.ps1"));

            AssertContainsInOrder(
                source,
                "function Get-ShellNavigationDisclosureClickPoint($item)",
                "$rect = $item.Current.BoundingRectangle",
                "$rect.X + ($rect.Width * 0.5)",
                "Source = \"GroupRowBody\"",
                "function Invoke-ShellNavigationDisclosure($window, $navigationView, [string]$name, [string]$targetState)",
                "$point = Get-ShellNavigationDisclosureClickPoint $item",
                "[GalleryRecordingNative]::HoldClick($point.X, $point.Y, 120)",
                "StateAfterClick = $stateAfterClick",
                "UsedAutomationFallback = $usedAutomationFallback");
            AssertContainsInOrder(
                source,
                "foreach ($click in @($designExpandedClick, $basicInputExpandedClick, $designCollapsedClick, $basicInputCollapsedClick))",
                "elseif ($click.StateAfterClick -ne $click.TargetState)",
                "pointer disclosure click expected",
                "if ($click.UsedAutomationFallback)",
                "used automation fallback for {1}; pointer disclosure proof is required.");
            AssertContainsInOrder(
                source,
                "return [ordered]@{",
                "Invoked = $designExpandedClick.Clicked -and $basicInputExpandedClick.Clicked -and $designCollapsedClick.Clicked -and $basicInputCollapsedClick.Clicked",
                "Clicks = @($designExpandedClick, $basicInputExpandedClick, $designCollapsedClick, $basicInputCollapsedClick)");
            Assert.IsFalse(source.Contains("$samplesExpandedClick", StringComparison.Ordinal));
            Assert.IsFalse(source.Contains("$samplesCollapsedClick", StringComparison.Ordinal));
        }

        [TestMethod]
        public void GalleryInteractionRecorderDoesNotTreatNoOpExpandAsInvoked()
        {
            var source = File.ReadAllText(Path.Combine(
                GetRepoRoot(),
                "tools",
                "visual-checks",
                "Record-GalleryControlInteractions.ps1"));

            AssertContainsInOrder(
                source,
                "function Invoke-ElementOnce($window, $element)",
                "$pattern = $element.GetCurrentPattern([System.Windows.Automation.ExpandCollapsePattern]::Pattern)",
                "$pattern.Expand()",
                "Start-Sleep -Milliseconds 180",
                "if ($pattern.Current.ExpandCollapseState -eq [System.Windows.Automation.ExpandCollapseState]::Expanded)",
                "return $true",
                "$pattern = $element.GetCurrentPattern([System.Windows.Automation.InvokePattern]::Pattern)");
        }

        [TestMethod]
        public void GalleryInteractionRecorderRejectsDetachedOpenRepeatPopups()
        {
            var source = File.ReadAllText(Path.Combine(
                GetRepoRoot(),
                "tools",
                "visual-checks",
                "Record-GalleryControlInteractions.ps1"));

            AssertContainsInOrder(
                source,
                "function Get-ElementBoundingRectangle($element)",
                "function Format-BoundingRectangle($rect)",
                "function Get-BoundingRectangleGap($first, $second)",
                "function Test-OpenInteractionElementAnchored($trigger, $openElement)",
                "return (Get-BoundingRectangleGap $triggerRect $openRect) -le 320.0",
                "function Test-BoundingRectangleStringAnchored($trigger, [string]$openBounds)",
                "return (Get-BoundingRectangleGap $triggerRect $openRect) -le 320.0",
                "function Test-ControlAllowsDetachedOpenRepeatElement([string]$control)",
                "return $control -eq \"MessageBox\"",
                "function Test-ControlUsesFastOpenRepeatPopupBounds([string]$control)",
                "return $control -eq \"SplitButton\" -or",
                "$control -eq \"ToggleSplitButton\"",
                "function Get-FastOpenRepeatPopupBounds($trigger, [string]$control)",
                "Test-ControlUsesFastOpenRepeatPopupBounds $control");
            AssertContainsInOrder(
                source,
                "function Get-ControlRecordingDurationSeconds([string]$control, [string]$interactionKind)",
                "if ($interactionKind -eq \"ShellNavigation\")",
                "return [Math]::Max($DurationSeconds, 18)",
                "if (Test-ControlUsesFastOpenRepeatPopupBounds $control)",
                "return [Math]::Max($DurationSeconds, 18)",
                "if ($control -eq \"MenuBar\")",
                "return [Math]::Max($DurationSeconds, 18)",
                "if ($control -eq \"ContentDialog\" -or $control -eq \"Flyout\" -or $control -eq \"Popup\" -or $control -eq \"MenuFlyout\")",
                "return [Math]::Max($DurationSeconds, 24)",
                "return [Math]::Max($DurationSeconds, 24)",
                "return $DurationSeconds");
            AssertContainsInOrder(
                source,
                "$openVisualDwellMilliseconds = switch ($control)",
                "\"CommandBar\" { 700; break }",
                "\"CommandBarFlyout\" { 800; break }",
                "\"ContentDialog\" { 1200; break }",
                "\"Flyout\" { 1200; break }",
                "\"MenuFlyout\" { 1200; break }",
                "default { 600; break }",
                "$closedVisualDwellMilliseconds = switch ($control)",
                "\"ContentDialog\" { 700; break }",
                "default { 450; break }",
                "$betweenOpenDwellMilliseconds = switch ($control)",
                "\"ContentDialog\" { 450; break }",
                "default { 250; break }",
                "$triggerBounds = Format-BoundingRectangle (Get-ElementBoundingRectangle $trigger)",
                "$visualCloseContext = New-OpenRepeatVisualCloseContext $window $control",
                "$firstOpenElementBoundsHint = if ($firstOpen) { Get-FastOpenRepeatPopupBounds $trigger $control } else { \"\" }",
                "$firstOpenElement = if ($openNames.Count -eq 0 -or ![string]::IsNullOrWhiteSpace($firstOpenElementBoundsHint)) { $null } else { Wait-ForOpenInteractionElement $window $trigger $openNames $control $openElementTimeoutMilliseconds }",
                "$firstOpenElementAnchored = $openNames.Count -eq 0 -or (Test-ControlAllowsDetachedOpenRepeatElement $control) -or (Test-OpenInteractionElementAnchored $trigger $firstOpenElement) -or (Test-BoundingRectangleStringAnchored $trigger $firstOpenElementBoundsHint)",
                "$firstOpenWindowHandle = Get-ElementNativeWindowHandle $firstOpenElement",
                "$firstOpenWindow = Find-TopLevelElementByNativeWindowHandleInProcess",
                "$firstOpenWindowIsTopLevel = $null -ne $firstOpenWindow",
                "if ($null -ne $visualCloseContext -and $firstOpenWindowIsTopLevel)",
                "$visualCloseContext[\"OpenWindowHandle\"] = $firstOpenWindowHandleValue",
                "$visualCloseContext[\"Bounds\"] = $firstOpenElementBounds",
                "$closeResult = Close-OpenInteractionElement $window $control $trigger $openNames $sampleElement $visualCloseContext $firstOpenElementBounds",
                "$secondOpenElementBoundsHint = if ($secondOpen) { Get-FastOpenRepeatPopupBounds $secondTrigger $control } else { \"\" }",
                "$secondOpenElementAnchored = $openNames.Count -eq 0 -or (Test-ControlAllowsDetachedOpenRepeatElement $control) -or (Test-OpenInteractionElementAnchored $secondTrigger $secondOpenElement) -or (Test-BoundingRectangleStringAnchored $secondTrigger $secondOpenElementBoundsHint)",
                "FirstOpenWindowHandle = $firstOpenWindowHandleValue",
                "FirstOpenWindowIsTopLevel = $firstOpenWindowIsTopLevel",
                "CloseNativeWindowChecked = $closeNativeWindowChecked",
                "CloseNativeWindowHidden = $closeNativeWindowHidden",
                "CloseExpandCollapseCollapsed = $closeExpandCollapseCollapsed",
                "CloseUiaElementGone = $closeUiaElementGone",
                "MenuFlyoutOutputMatched = $menuFlyoutOutputMatched",
                "MenuFlyoutItemSelectionClosed = $menuFlyoutItemSelectionClosed",
                "MenuFlyoutPointerSelectionClosed = $menuFlyoutPointerSelectionClosed",
                "CloseVisualChecked = $closeVisualChecked",
                "FirstOpenElementAnchored = $firstOpenElementAnchored",
                "SecondOpenElementAnchored = $secondOpenElementAnchored",
                "TriggerBounds = $triggerBounds",
                "FirstOpenElementBounds = $firstOpenElementBounds",
                "SecondOpenElementBounds = $secondOpenElementBounds");
            AssertContainsInOrder(
                source,
                "function Invoke-SampleOptionCloseAttempt($window, $button, [string]$method)",
                "$buttonWindowHandle = Get-ElementNativeWindowHandle $button",
                "$buttonUsesDetachedWindow = $buttonWindowHandle -ne [IntPtr]::Zero",
                "Find-TopLevelElementByNativeWindowHandleInProcess",
                "if (!$buttonUsesDetachedWindow)",
                "[GalleryRecordingNative]::Activate([IntPtr]$window.Current.NativeWindowHandle)",
                "if ($method -eq \"Invoke\")");
            AssertContainsInOrder(
                source,
                "function Close-WithVerifiedSampleOption($window, $sampleElement, $trigger, [string[]]$openNames, [string]$control, [string]$name, [string]$methodName, $visualCloseContext = $null)",
                "$methods = if ($control -eq \"MenuFlyout\" -or $control -eq \"MenuBar\" -or $control -eq \"Menu\")",
                "@(\"Click\", \"FocusSpace\", \"Invoke\")",
                "foreach ($method in $methods)");
            AssertContainsInOrder(
                source,
                "function Get-OpenRepeatCloseOptionName([string]$control)",
                "\"ComboBox\" { return \"Green\" }",
                "\"DatePicker\" { return \"6\" }",
                "\"DropDownButton\" { return \"Send\" }",
                "\"SplitButton\" { return \"Red\" }",
                "\"ToggleSplitButton\" { return \"Bulleted list\" }",
                "\"MenuBar\" { return \"Exit\" }",
                "\"Menu\" { return \"Exit\" }",
                "function Test-ControlSupportsTriggerToggleClose([string]$control)",
                "\"DatePicker\" { return $true }",
                "function Close-WithVerifiedOpenedElementClick($window, $trigger, [string[]]$openNames, [string]$control, [double]$xFraction, [double]$yFraction, [string]$methodName, $visualCloseContext = $null)",
                "Find-OpenInteractionElement $window $trigger $openNames $control",
                "[GalleryRecordingNative]::Click($x, $y)",
                "function Close-WithVerifiedBoundsClick($window, $trigger, [string[]]$openNames, [string]$control, [string]$bounds, [string]$methodName, $visualCloseContext = $null)",
                "ConvertFrom-BoundingRectangleString $bounds",
                "Wait-ForOpenInteractionElementGone $window $trigger $openNames $control 1200 $visualCloseContext",
                "function Close-WithVerifiedEscape($window, $trigger, [string[]]$openNames, [string]$control, [string]$methodName, $visualCloseContext = $null)",
                "[GalleryRecordingNative]::Escape()",
                "Wait-ForOpenInteractionElementGone $window $trigger $openNames $control 1200 $visualCloseContext",
                "function Close-WithVerifiedTriggerToggle($window, $trigger, [string[]]$openNames, [string]$control, [string]$methodName, $visualCloseContext = $null)",
                "Invoke-OpenElementOnce $window $control $trigger",
                "Wait-ForOpenInteractionElementGone $window $trigger $openNames $control 1200 $visualCloseContext",
                "function Close-WithVerifiedKeyboardSelection($window, $trigger, [string[]]$openNames, [string]$control, [int]$downCount, [string]$methodName, $visualCloseContext = $null)",
                "$openElement = Find-OpenInteractionElement $window $trigger $openNames $control",
                "$openElement.SetFocus()",
                "[GalleryRecordingNative]::Down()",
                "[GalleryRecordingNative]::Enter()",
                "function Close-WithVerifiedCollapsePattern($window, $trigger, [string[]]$openNames, [string]$control, $visualCloseContext = $null)",
                "$pattern = $target.GetCurrentPattern([System.Windows.Automation.ExpandCollapsePattern]::Pattern)",
                "$pattern.Collapse()",
                "function Close-OpenInteractionElement($window, [string]$control, $trigger, [string[]]$openNames, $sampleElement, $visualCloseContext = $null, [string]$openedBoundsHint = \"\")",
                "if ($control -eq \"ComboBox\")",
                "Close-WithVerifiedCollapsePattern $window $trigger $openNames $control $visualCloseContext",
                "Close-WithVerifiedKeyboardSelection $window $trigger $openNames $control 1 \"KeyboardDownEnter\" $visualCloseContext",
                "Close-WithVerifiedOpenedElementClick $window $trigger $openNames $control 0.5 1.65 \"SecondItemClick\" $visualCloseContext",
                "if ($control -eq \"DatePicker\")",
                "Close-WithVerifiedCollapsePattern $window $trigger $openNames $control $visualCloseContext",
                "Close-WithVerifiedKeyboardSelection $window $trigger $openNames $control 1 \"KeyboardDownEnter\" $visualCloseContext",
                "Close-WithVerifiedOpenedElementClick $window $trigger $openNames $control 0.78 0.46 \"DayCellClick\" $visualCloseContext",
                "if ((Test-ControlUsesFastOpenRepeatPopupBounds $control) -and ![string]::IsNullOrWhiteSpace($openedBoundsHint))",
                "Close-WithVerifiedCollapsePattern $window $trigger $openNames $control $visualCloseContext",
                "Close-WithVerifiedTriggerToggle $window $trigger $openNames $control \"FastPopupTriggerToggle\" $visualCloseContext",
                "Close-WithVerifiedEscape $window $trigger $openNames $control \"FastPopupEscape\" $visualCloseContext",
                "Close-WithVerifiedBoundsClick $window $trigger $openNames $control $openedBoundsHint \"FastPopupBoundsClick\" $visualCloseContext",
                "$openRepeatCloseOptionName = Get-OpenRepeatCloseOptionName $control",
                "Close-WithVerifiedSampleOption $window $sampleElement $trigger $openNames $control $openRepeatCloseOptionName \"LeafCloseItem\" $visualCloseContext",
                "if (Test-ControlSupportsTriggerToggleClose $control)",
                "Method = \"TriggerToggle\"");
            var closeStart = source.IndexOf(
                "function Wait-ForOpenInteractionElementGone",
                StringComparison.Ordinal);
            var closeEnd = source.IndexOf(
                "function Click-OpenInteractionDismissPoint",
                closeStart,
                StringComparison.Ordinal);
            Assert.IsTrue(closeStart >= 0 && closeEnd > closeStart);
            var closeSource = source.Substring(closeStart, closeEnd - closeStart);
            AssertContainsInOrder(
                closeSource,
                "$visualCloseContext.Contains(\"OpenWindowHandle\")",
                "[GalleryRecordingNative]::IsVisible",
                "if (!$popupWindowVisible)",
                "[void](Test-OpenRepeatVisualClosed $window $visualCloseContext)",
                "return $true",
                "if ((Get-ExpandCollapseStateName $element) -eq \"Collapsed\")",
                "$visualCloseContext[\"LastCloseExpandCollapseCollapsed\"] = $true",
                "[void](Test-OpenRepeatVisualClosed $window $visualCloseContext)",
                "return $true",
                "$openElement = Find-OpenInteractionElement $window $element $openNames $control",
                "if ($null -eq $openElement)",
                "$visualCloseContext[\"LastCloseUiaElementGone\"] = $true",
                "[void](Test-OpenRepeatVisualClosed $window $visualCloseContext)",
                "return $true");
            Assert.IsFalse(closeSource.Contains("$visualCloseResult", StringComparison.Ordinal));
            Assert.IsFalse(closeSource.Contains("if ($visualCloseResult.Closed)", StringComparison.Ordinal));
            AssertContainsInOrder(
                source,
                "function Close-OpenInteractionElement($window, [string]$control, $trigger, [string[]]$openNames, $sampleElement, $visualCloseContext = $null, [string]$openedBoundsHint = \"\")",
                "if ($control -eq \"ContentDialog\")",
                "Close-WithVerifiedSampleOption $window $sampleElement $trigger $openNames $control \"Cancel\" \"DialogCancelButton\" $visualCloseContext");
            AssertContainsInOrder(
                source,
                "if ($control -eq \"MenuFlyout\")",
                "Close-WithVerifiedSampleOption $window $sampleElement $trigger $openNames $control \"By rating\" \"LeafMenuItem\" $visualCloseContext",
                "MenuFlyoutItemSelectionClosed = $true",
                "MenuFlyoutPointerSelectionClosed = $sampleClose.Method -eq \"LeafMenuItem:Click\"",
                "$collapseClose = Close-WithVerifiedCollapsePattern $window $trigger $openNames $control $visualCloseContext",
                "Method = \"MenuFlyoutCollapse\"",
                "MenuFlyoutItemSelectionClosed = $false",
                "MenuFlyoutPointerSelectionClosed = $false",
                "$menuFlyoutOutputMatched = $control -ne \"MenuFlyout\" -or",
                "@(\"Sort by: rating\")",
                "$menuFlyoutItemSelectionClosed = $control -ne \"MenuFlyout\" -or",
                "-and $menuFlyoutOutputMatched -and $menuFlyoutItemSelectionClosed");
            AssertContainsInOrder(
                source,
                "if ($control -eq \"MenuFlyout\" -and",
                "!$interactionResult.Contains(\"MenuFlyoutItemSelectionClosed\") -or",
                "!$interactionResult.MenuFlyoutItemSelectionClosed",
                "!$interactionResult.Contains(\"MenuFlyoutOutputMatched\") -or",
                "!$interactionResult.MenuFlyoutOutputMatched",
                "$status = \"Failed\"",
                "$notes.Add(\"MenuFlyout leaf selection did not both produce the expected output and dismiss the flyout; cleanup close does not satisfy this gate.\")");
            AssertContainsInOrder(
                source,
                "function Test-ControlRequiresLiveVisualClose([string]$control)",
                "return $control -eq \"TeachingTip\" -or",
                "$control -eq \"ComboBox\" -or",
                "$control -eq \"DatePicker\" -or",
                "$control -eq \"ContentDialog\" -or",
                "function Save-WindowVisualSnapshot($window, [string]$name, $captureRect = $null)",
                "Copy-Item -LiteralPath $liveFramePath -Destination $path -Force",
                "$stats = Get-ImageStats $path",
                "if ($null -ne $stats -and $stats.NonBlank)",
                "$stats = Get-ImageStats $path",
                "if ($null -eq $stats -or !$stats.NonBlank)",
                "return $null",
                "function New-OpenRepeatVisualCloseContext($window, [string]$control)",
                "BaselinePath = $baseline.Path",
                "function Test-OpenRepeatVisualClosed($window, $visualCloseContext)",
                "$closed = $null -ne $delta -and [double]$delta -le 1.0",
                "$visualCloseContext[\"LastCloseVisualChecked\"] = $true");
            var visualCloseStart = source.IndexOf(
                "function Test-OpenRepeatVisualClosed",
                StringComparison.Ordinal);
            var visualCloseEnd = source.IndexOf(
                "function Get-PosterFrameIntervalSeconds",
                visualCloseStart,
                StringComparison.Ordinal);
            Assert.IsTrue(visualCloseStart >= 0 && visualCloseEnd > visualCloseStart);
            var visualCloseSource = source.Substring(
                visualCloseStart,
                visualCloseEnd - visualCloseStart);
            Assert.IsFalse(visualCloseSource.Contains("Closed = $true", StringComparison.Ordinal));
            AssertContainsInOrder(
                source,
                "function Get-OpenRepeatOpenThreshold([string]$control)",
                "if ($control -eq \"CommandBar\")",
                "return 3.0",
                "if ($control -eq \"CommandBarFlyout\")",
                "return 2.0",
                "return 5.0",
                "function Get-OpenRepeatClosedThreshold([string]$control)",
                "if ($control -eq \"DatePicker\")",
                "return 1.2",
                "return 1.0",
                "function Get-OpenRepeatVisualEvidence($frames, $recordingResult, $interactionResult, [string]$control = \"\")",
                "$openThreshold = Get-OpenRepeatOpenThreshold $control",
                "$closedThreshold = Get-OpenRepeatClosedThreshold $control",
                "$closedEndSeconds = if ($null -ne $interactionResult.SecondOpenStartSeconds)",
                "$firstOpenEntries = Get-OpenRepeatBaselineDeltaEntries",
                "$interactionResult.FirstOpenStartSeconds",
                "$closedEntries = Get-OpenRepeatBaselineDeltaEntries",
                "$secondOpenEntries = Get-OpenRepeatBaselineDeltaEntries",
                "$interactionResult.SecondOpenStartSeconds",
                "FirstOpenEvidence = [double]$firstOpenEntry.Delta -ge $openThreshold",
                "ClosedEvidence = [double]$closedEntry.Delta -le $closedThreshold",
                "SecondOpenEvidence = [double]$secondOpenEntry.Delta -ge $openThreshold",
                "Detection = \"BaselineDeltaEventWindowScan\"");
            AssertContainsInOrder(
                source,
                "function Get-OpenRepeatDirectFrameEvidence(",
                "$initialFrame = Get-ClosestExtractedFrame $frames $interactionResult.InitialVisualSeconds",
                "$firstOpenDelta = Compare-LuminanceSamples $closedSamples $firstSamples",
                "$closeVisualClosed = $interactionResult.Contains(\"CloseVisualClosed\") -and [bool]$interactionResult.CloseVisualClosed",
                "$closedEvidence = ([double]$initialClosedDelta -le $closedThreshold) -or $closeVisualClosed",
                "Detection = \"DirectEventFrameClosedBaselineScan\"",
                "function Get-OpenRepeatOpenThreshold([string]$control)",
                "function Get-OpenRepeatVisualEvidence($frames, $recordingResult, $interactionResult, [string]$control = \"\")",
                "$directEvidence = Get-OpenRepeatDirectFrameEvidence",
                "if ($null -ne $directEvidence)",
                "return $directEvidence");
            AssertContainsInOrder(
                source,
                "$visualOpenRepeatEvidence = if ($interactionKind -eq \"OpenRepeat\")",
                "Get-OpenRepeatVisualEvidence $frames $recordingResult $interactionResult $control");
            AssertContainsInOrder(
                source,
                "$anchored = $true",
                "$interactionResult.FirstOpenElementAnchored -and $interactionResult.SecondOpenElementAnchored",
                "$openRepeatGeometryFailed =",
                "Opened element was detached from trigger. Trigger={0}; first={1}; second={2}.");
        }

        [TestMethod]
        public void MenuFlyoutCustomPlacementDoesNotSubtractContextMenuPresenterOffset()
        {
            var source = File.ReadAllText(Path.Combine(
                GetRepoRoot(),
                "ModernWpf.Controls",
                "MenuFlyout",
                "MenuFlyout.cs"));

            AssertContainsInOrder(
                source,
                "private CustomPopupPlacement[] PositionPopup(Size popupSize, Size targetSize, Point offset)",
                "return PositionPopup(popupSize, targetSize, offset, null);");
            Assert.IsFalse(
                source.Contains("return PositionPopup(popupSize, targetSize, offset, m_presenter);", StringComparison.Ordinal),
                "MenuFlyout uses ContextMenu as its popup presenter; passing it as the child offset source detaches ShowAt popups toward the screen origin.");
        }

        [TestMethod]
        public void MenuFlyoutUsesAbsoluteContextMenuPlacementForAnchoredModes()
        {
            var source = File.ReadAllText(Path.Combine(
                GetRepoRoot(),
                "ModernWpf.Controls",
                "MenuFlyout",
                "MenuFlyout.cs"));
            var presenterSource = File.ReadAllText(Path.Combine(
                GetRepoRoot(),
                "ModernWpf.Controls",
                "MenuFlyout",
                "MenuFlyoutPresenter.cs"));

            AssertContainsInOrder(
                source,
                "Point absolutePlacementPoint = default;",
                "var hasAbsolutePlacementPoint =",
                "placement == PlacementMode.Custom &&",
                "TryGetAbsolutePlacementPoint(placementTarget, effectivePlacement, out absolutePlacementPoint);",
                "m_presenter.SetAbsolutePlacementPoint(absolutePlacementPoint);",
                "m_presenter.Placement = PlacementMode.AbsolutePoint;",
                "m_presenter.HorizontalOffset = absolutePlacementPoint.X;",
                "m_presenter.VerticalOffset = absolutePlacementPoint.Y;",
                "m_presenter.ClearValue(ContextMenu.PlacementTargetProperty);",
                "m_presenter.ClearValue(ContextMenu.PlacementRectangleProperty);",
                "if (!hasAbsolutePlacementPoint && placement == PlacementMode.Custom)",
                "m_presenter.ClearValue(ContextMenu.PlacementRectangleProperty);");
            AssertContainsInOrder(
                source,
                "private bool TryGetAbsolutePlacementPoint(",
                "var placementRect = GetPlacementRectangle(placementTarget, effectivePlacement);",
                "var topLeft = placementTarget.PointToScreen(placementRect.TopLeft);",
                "var bottomRight = placementTarget.PointToScreen(placementRect.BottomRight);",
                "var targetRect = new Rect(topLeft, bottomRight);",
                "var popupSize = GetPresenterDesiredScreenSize(placementTarget);",
                "case FlyoutPlacementMode.TopEdgeAlignedLeft:",
                "point = new Point(targetRect.Left, targetRect.Top - popupSize.Height);",
                "case FlyoutPlacementMode.BottomEdgeAlignedLeft:",
                "point = new Point(targetRect.Left, targetRect.Bottom);",
                "case FlyoutPlacementMode.LeftEdgeAlignedTop:",
                "point = new Point(targetRect.Left - popupSize.Width, targetRect.Top);",
                "case FlyoutPlacementMode.RightEdgeAlignedTop:",
                "point = new Point(targetRect.Right, targetRect.Top);",
                "private Size GetPresenterDesiredScreenSize(FrameworkElement placementTarget)",
                "m_presenter.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));",
                "source.CompositionTarget.TransformToDevice.Transform(");
            AssertContainsInOrder(
                presenterSource,
                "internal void SetAbsolutePlacementPoint(Point? point)",
                "m_absolutePlacementPoint = point;",
                "ApplyAbsolutePlacementPoint();",
                "private void ApplyAbsolutePlacementPoint()",
                "_parentPopup.Placement = PlacementMode.AbsolutePoint;",
                "_parentPopup.HorizontalOffset = m_absolutePlacementPoint.Value.X;",
                "_parentPopup.VerticalOffset = m_absolutePlacementPoint.Value.Y;",
                "MovePopupWindowToAbsolutePlacementPoint();",
                "Dispatcher.BeginInvoke(new Action(MovePopupWindowToAbsolutePlacementPoint), DispatcherPriority.Loaded);",
                "private void MovePopupWindowToAbsolutePlacementPoint()",
                "PresentationSource.FromVisual(this) is HwndSource source",
                "SetWindowPos(",
                "private static extern bool SetWindowPos(");
        }

        [TestMethod]
        public void FlyoutBaseMovesPopupHwndToAbsolutePlacementForAnchoredModes()
        {
            var source = File.ReadAllText(Path.Combine(
                GetRepoRoot(),
                "ModernWpf.Controls",
                "Flyout",
                "FlyoutBase.cs"));

            AssertContainsInOrder(
                source,
                "m_popup.Placement = PlacementMode.Custom;",
                "m_popup.PlacementTarget = placementTarget;",
                "m_popup.PlacementRectangle = GetPlacementRectangle(placementTarget, effectivePlacement);",
                "m_absolutePopupPlacementPoint = TryGetAbsolutePlacementPoint(placementTarget, effectivePlacement, out var absolutePlacementPoint)",
                "private void OnPopupOpened(object sender, EventArgs e)",
                "MovePopupWindowToAbsolutePlacementPoint();",
                "Dispatcher.BeginInvoke(new Action(MovePopupWindowToAbsolutePlacementPoint), DispatcherPriority.Loaded);");
            AssertContainsInOrder(
                source,
                "private bool TryGetAbsolutePlacementPoint(",
                "var placementRect = GetPlacementRectangle(placementTarget, effectivePlacement);",
                "var topLeft = placementTarget.PointToScreen(placementRect.TopLeft);",
                "var bottomRight = placementTarget.PointToScreen(placementRect.BottomRight);",
                "case FlyoutPlacementMode.BottomEdgeAlignedLeft:",
                "point = new Point(targetRect.Left, targetRect.Bottom);",
                "private void MovePopupWindowToAbsolutePlacementPoint()",
                "PresentationSource.FromVisual(m_popup.Child) is HwndSource source",
                "SetWindowPos(",
                "private static extern bool SetWindowPos(");
        }

        [TestMethod]
        public void GalleryInteractionRecorderAcceptsOfficialWpfRenderedPageArtifacts()
        {
            var source = File.ReadAllText(Path.Combine(
                GetRepoRoot(),
                "tools",
                "visual-checks",
                "Record-GalleryControlInteractions.ps1"));

            AssertContainsInOrder(
                source,
                "function Test-ControlSupportsRenderedPageArtifactAnchor([string]$control)",
                "\"Calendar\" { return $true }",
                "\"DataGrid\" { return $true }",
                "\"ListBox\" { return $true }",
                "\"MessageBox\" { return $true }",
                "\"TextBox\" { return $true }",
                "\"TreeView\" { return $true }",
                "default { return $false }");
            AssertContainsInOrder(
                source,
                "function Get-RenderedPageArtifactAnchor([string]$artifactDir)",
                "FileName = \"ContentPagePane.png\"; Source = \"ContentPagePaneRenderedArtifact\"",
                "FileName = \"GalleryItemPageRoot.png\"; Source = \"GalleryItemPageRootRenderedArtifact\"",
                "$stats = Get-ImageStats $path",
                "Stats = $stats");
            AssertContainsInOrder(
                source,
                "if ($null -eq $sampleElement)",
                "if (Test-ControlSupportsRenderedPageArtifactAnchor $control)",
                "$renderedPageArtifactAnchor = Get-RenderedPageArtifactAnchor $artifactDir",
                "accepted nonblank",
                "RenderedPageArtifactAnchor = $renderedPageArtifactAnchor");
        }

        [TestMethod]
        public void GalleryInteractionRecorderExercisesOfficialWpfSelectionAndTextControls()
        {
            var source = File.ReadAllText(Path.Combine(
                GetRepoRoot(),
                "tools",
                "visual-checks",
                "Record-GalleryControlInteractions.ps1"));

            AssertContainsInOrder(
                source,
                "function Test-ControlSupportsSelectionInteraction([string]$control)",
                "\"Calendar\" { return $true }",
                "\"DataGrid\" { return $true }",
                "\"ListBox\" { return $true }",
                "\"ListView\" { return $true }");
            AssertContainsInOrder(
                source,
                "function Get-SelectionInteractionContainerName([string]$control)",
                "\"Calendar\" { return \"Default\" }",
                "\"DataGrid\" { return \"Sample Data Grid\" }",
                "\"ListBox\" { return \"Color ListBox\" }",
                "\"ListView\" { return \"Basic ListView\" }");
            AssertContainsInOrder(
                source,
                "function Find-FirstSelectableDescendant($element, [bool]$preferUnselected = $true)",
                "[System.Windows.Automation.SelectionItemPattern]::Pattern",
                "if (!$preferUnselected -or !$pattern.Current.IsSelected)",
                "return $candidate");
            AssertContainsInOrder(
                source,
                "function Get-SelectionContainerSelectedItemNames($element)",
                "[System.Windows.Automation.SelectionPattern]::Pattern",
                "$pattern.Current.GetSelection()",
                "AfterContainerSelection = $afterContainerSelection");
            AssertContainsInOrder(
                source,
                "function Test-VisualSelectionEvidence([string]$control, [string]$interactionKind, $maxLocalFrameDelta)",
                "switch ($control)",
                "\"DataGrid\" { return [double]$maxLocalFrameDelta -ge 10.0 }",
                "\"SelectorBar\" { return [double]$maxLocalFrameDelta -ge 0.05 }",
                "$visualSelectionEvidence = Test-VisualSelectionEvidence $control $interactionKind $maxLocalFrameDelta",
                "VisualSelectionEvidence = $visualSelectionEvidence");
            AssertContainsInOrder(
                source,
                "function Test-ControlSupportsTextInteraction([string]$control)",
                "\"RichTextBox\" { return $true }",
                "function Get-TextInteractionInput([string]$control)",
                "\"RichTextBox\" { return \"ModernWpf rich text\" }",
                "function Get-TextInteractionTargetName([string]$control)",
                "\"RichTextBox\" { return \"simple rich text editor\" }");
            AssertContainsInOrder(
                source,
                "function Get-ControlInteractionKind([string]$control)",
                "if (Test-ControlSupportsOpenInteraction $control) { return \"OpenRepeat\" }",
                "function Test-ControlRequiresDiagnosticPreparation([string]$control)",
                "default { return $false }");
            AssertContainsInOrder(
                source,
                "function Test-ControlSupportsOptionInteraction([string]$control)",
                "\"TitleBar\" { return $true }",
                "function Get-OptionInteractionTriggerName([string]$control)",
                "\"TitleBar\" { return \"IsBackButtonVisible\" }",
                "function Get-OptionInteractionExpectedElementAutomationId([string]$control)",
                "\"TitleBar\" { return \"TitleBarBackButton\" }",
                "function Invoke-OptionInteraction($window, [string]$control, $sampleElement)",
                "$expectedElementAutomationId = Get-OptionInteractionExpectedElementAutomationId $control",
                "$beforeExpectedElementVisible = Test-AutomationElementUsable $expectedElement",
                "$afterExpectedElementVisible = Test-AutomationElementUsable $expectedElement",
                "ExpectedElementBounds = $expectedElementBounds",
                "BeforeExpectedElementVisible = $beforeExpectedElementVisible",
                "AfterExpectedElementVisible = $afterExpectedElementVisible",
                "StateOrSampleChanged = $stateOrSampleChanged",
                "ExpectedElementChanged = $expectedElementChanged",
                "OptionChanged = if ($requiresExpectedElement) { $stateOrSampleChanged -and $expectedElementChanged } else { $stateOrSampleChanged }");
            Assert.IsFalse(
                source.Contains("if ($control -eq \"ToolTip\") { return \"PreparedOpen\" }", StringComparison.Ordinal),
                "ToolTip must use the hover/open-repeat path, not the pre-opened diagnostic path.");
            Assert.IsFalse(
                source.Contains("PreparedText", StringComparison.Ordinal),
                "RichTextBox must use recorder-driven text input, not diagnostic-prepared text.");
            var diagnosticPreparationFunctionStart = source.IndexOf(
                "function Test-ControlRequiresDiagnosticPreparation([string]$control)",
                StringComparison.Ordinal);
            var diagnosticPreparationFunctionEnd = source.IndexOf(
                "\nfunction Find-ShellNavigationItem",
                diagnosticPreparationFunctionStart,
                StringComparison.Ordinal);
            Assert.IsTrue(diagnosticPreparationFunctionStart >= 0);
            Assert.IsTrue(diagnosticPreparationFunctionEnd > diagnosticPreparationFunctionStart);
            var diagnosticPreparationFunction = source.Substring(
                diagnosticPreparationFunctionStart,
                diagnosticPreparationFunctionEnd - diagnosticPreparationFunctionStart);
            Assert.IsFalse(
                diagnosticPreparationFunction.Contains("RichTextBox", StringComparison.Ordinal),
                "RichTextBox must not opt into --open-interactions diagnostic preparation.");
            AssertContainsInOrder(
                source,
                "private const uint WM_CHAR = 0x0102;",
                "public static void PressCtrlV()",
                "KeyPress(0x56);",
                "public static void TypeWindowMessageText(IntPtr hWnd, string text)",
                "SendMessage(hWnd, WM_CHAR",
                "private static void TypeUnicodeChar(char ch)",
                "KEYEVENTF_UNICODE",
                "Set-Clipboard -Value $text",
                "[GalleryRecordingNative]::PressCtrlV()",
                "[GalleryRecordingNative]::TypeText($text)",
                "[GalleryRecordingNative]::TypeWindowMessageText($window.Current.NativeWindowHandle, $text)",
                "[GalleryRecordingNative]::TypeVirtualKeyText($text)");
            var setEditableFunctionStart = source.IndexOf(
                "function Set-EditableElementText($window, $element, [string]$text)",
                StringComparison.Ordinal);
            var setEditableFunctionEnd = source.IndexOf(
                "\nfunction Wait-ForInteractiveElementByNameInProcess",
                setEditableFunctionStart,
                StringComparison.Ordinal);
            Assert.IsTrue(setEditableFunctionStart >= 0);
            Assert.IsTrue(setEditableFunctionEnd > setEditableFunctionStart);
            var setEditableFunction = source.Substring(
                setEditableFunctionStart,
                setEditableFunctionEnd - setEditableFunctionStart);
            var sendKeysIndex = setEditableFunction.IndexOf(
                "[System.Windows.Forms.SendKeys]::SendWait($text)",
                StringComparison.Ordinal);
            var unicodeFallbackIndex = setEditableFunction.IndexOf(
                "[GalleryRecordingNative]::TypeText($text)",
                StringComparison.Ordinal);
            Assert.IsTrue(sendKeysIndex >= 0);
            Assert.IsTrue(unicodeFallbackIndex > sendKeysIndex);
            StringAssert.Contains(
                setEditableFunction.Substring(sendKeysIndex, unicodeFallbackIndex - sendKeysIndex),
                "catch {");
            AssertContainsInOrder(
                source,
                "$script:LastEditableTextMethod = \"\"",
                "$script:LastEditableTextMethod = \"ClipboardPaste\"",
                "$script:LastEditableTextMethod = \"SendKeys\"",
                "$script:LastEditableTextMethod = \"UnicodeSendInput\"",
                "$script:LastEditableTextMethod = \"WindowMessage\"",
                "$script:LastEditableTextMethod = \"VirtualKey\"",
                "$script:LastEditableTextMethod = \"ValuePattern\"",
                "InputMethod = $inputMethod");
        }

        [TestMethod]
        public void GalleryInteractionRecorderDoesNotLeaveInteractiveModernPagesStatic()
        {
            var source = File.ReadAllText(Path.Combine(
                GetRepoRoot(),
                "tools",
                "visual-checks",
                "Record-GalleryControlInteractions.ps1"));

            AssertContainsInOrder(
                source,
                "function Test-ControlSupportsSelectionInteraction([string]$control)",
                "\"PersonPicture\" { return $true }",
                "function Get-SelectionInteractionTriggerName([string]$control)",
                "\"PersonPicture\" { return \"Display Name\" }");
            AssertContainsInOrder(
                source,
                "function Wait-Until([scriptblock]$Probe, [int]$TimeoutSeconds, [string]$Description)",
                "function Wait-LiveRecordingWarmupFrames([int]$frameRate, [double]$warmupSeconds, [int]$timeoutSeconds)",
                "live recorder warm-up frames",
                "$latestFrame = $frames[$frames.Count - 1]",
                "Get-ImageStats $latestFrame.FullName",
                "function Find-WindowByProcessId([int]$processId)");
            AssertContainsInOrder(
                source,
                "$script:GalleryLiveFrameDirectory = Join-Path",
                "$recordingFrameRate = Get-ControlRecordingFrameRate $control $interactionKind",
                "$recordingJob = Start-RecordingJob",
                "Wait-LiveRecordingWarmupFrames $recordingFrameRate 0.4 15",
                "$interactionResult = Invoke-RecordedInteraction $window $control $sampleElement $artifactDir");
            AssertContainsInOrder(
                source,
                "function Test-ControlSupportsValueInteraction([string]$control)",
                "\"ThemeShadow\" { return $true }",
                "function Get-ControlInteractionKind([string]$control)",
                "if (Test-ControlSupportsValueInteraction $control) { return \"Value\" }");
            AssertContainsInOrder(
                source,
                "public static void Drag(int startX, int startY, int endX, int endY, int steps, int stepDelayMilliseconds)",
                "mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, UIntPtr.Zero);",
                "mouse_event(",
                "MOUSEEVENTF_MOVE,",
                "mouse_event(MOUSEEVENTF_LEFTUP, 0, 0, 0, UIntPtr.Zero);",
                "public static void EndOverWindow(IntPtr hWnd)",
                "PostMessage(hWnd, WM_KEYDOWN, new UIntPtr(VK_END), IntPtr.Zero);",
                "PostMessage(hWnd, WM_KEYUP, new UIntPtr(VK_END), IntPtr.Zero);");
            AssertContainsInOrder(
                source,
                "function Test-BoundingRectangleStringsNearlyEqual([string]$before, [string]$after, [double]$tolerance)",
                "$beforeRect = ConvertFrom-BoundingRectangleString $before",
                "$afterRect = ConvertFrom-BoundingRectangleString $after",
                "function Get-LayoutStabilityTargetAutomationIds([string]$control)",
                "\"GallerySample_ThemeShadow_Root\"",
                "\"GallerySample_ThemeShadow_Example3Grid\"",
                "\"GallerySample_ThemeShadow_ShadowCastGrid\"",
                "\"GallerySample_ThemeShadow_ShadowChrome\"",
                "\"GallerySample_ThemeShadow_ShadowRect\"",
                "\"GallerySample_ThemeShadow_TranslationSlider\"",
                "function Get-RenderedArtifactBoundsMap($window, [string]$artifactDir, [string[]]$automationIds)",
                "function Copy-RenderedArtifactSnapshot([string]$artifactDir, [string]$snapshotName, [string[]]$automationIds)",
                "function Get-ThemeShadowArtifactEvidence([string]$beforeSnapshotDir, [string]$afterSnapshotDir)",
                "function Get-BoundingRectangleMapValue($boundsById, [string]$automationId)",
                "function Format-BoundingRectangleMap($boundsById)",
                "function Test-BoundingRectangleMapsNearlyEqual($beforeById, $afterById, [double]$tolerance)",
                "function Refresh-ModernWpfVisualArtifacts($window)",
                "Find-DescendantByAutomationId $window \"GalleryVisualTestRefreshArtifacts\"",
                "function Get-RenderedArtifactBounds([string]$artifactDir, [string]$automationId)",
                "Get-Content -LiteralPath $path -Raw",
                "function Invoke-ValueInteraction($window, [string]$control, $sampleElement, [string]$artifactDir = \"\")",
                "$layoutStabilityTargetAutomationIds = @(Get-LayoutStabilityTargetAutomationIds $control)",
                "[void](Refresh-ModernWpfVisualArtifacts $window)",
                "$beforeLayoutBoundsById = if ($hasLayoutStabilityTargets) { Get-RenderedArtifactBoundsMap $window $artifactDir $layoutStabilityTargetAutomationIds } else { $null }",
                "$beforeArtifactSnapshotDir = if ($control -eq \"ThemeShadow\"",
                "$themeShadowVisualBounds = if ($control -eq \"ThemeShadow\"",
                "$themeShadowCasterBeforeBounds = if ($control -eq \"ThemeShadow\"",
                "\"ThemeShadow\" { 64.0 }",
                "$valueInputMethod = \"RangeValuePattern\"",
                "if ($control -eq \"ThemeShadow\")",
                "$targetValue = [Math]::Max($minimum, [Math]::Min($maximum, [double]$target))",
                "$valueInputMethod = \"RangeValuePatternAnimated\"",
                "for ($step = 1; $step -le $steps; $step++)",
                "$pattern.SetValue($currentValue)",
                "$themeShadowCasterAfterBounds = if ($control -eq \"ThemeShadow\"",
                "$themeShadowCasterStable = if ($control -eq \"ThemeShadow\"",
                "ThemeShadowVisualBounds = $themeShadowVisualBounds",
                "ThemeShadowCasterBeforeBounds = $themeShadowCasterBeforeBounds",
                "ThemeShadowCasterAfterBounds = $themeShadowCasterAfterBounds",
                "ThemeShadowCasterStable = $themeShadowCasterStable",
                "ThemeShadowSourceGeometryReference = if ($control -eq \"ThemeShadow\")",
                "ValueInputMethod = $valueInputMethod",
                "DragStartPoint = $dragStartPoint",
                "DragEndPoint = $dragEndPoint",
                "SliderClickablePoint = $sliderClickablePoint",
                "LayoutStabilityTargetAutomationIds = $layoutStabilityTargetAutomationIds",
                "LayoutStabilitySource = if ($hasLayoutStabilityTargets) { \"RenderedArtifactBounds\" } else { \"\" }",
                "ThemeShadowArtifactEvidence = $themeShadowArtifactEvidence",
                "BeforeLayoutBounds = $beforeLayoutBounds",
                "AfterLayoutBounds = $afterLayoutBounds",
                "LayoutStable = Test-BoundingRectangleMapsNearlyEqual $beforeLayoutBoundsById $afterLayoutBoundsById 1.0");
            AssertContainsInOrder(
                source,
                "function Get-ControlRecordingFrameRate([string]$control, [string]$interactionKind)",
                "return [Math]::Max(30, $FrameRate)",
                "function Export-DenseAnalysisFrames([string]$videoPath, [string]$caseDir, [string]$name, [int]$fps)",
                "function Compare-ImageRegionMeanDeltaSampled($baseline, $current, $region, [int]$sampleStep)",
                "function Format-FrameRectangle($region)",
                "function Get-FrameRectangleMaxDelta($first, $second)",
                "function Get-ThemeShadowCardEdgeBounds($bitmap, $region)",
                "function Get-ThemeShadowShadowEnvelopeBounds($bitmap, $cardRegion, [int]$sampleStep = 3)",
                "function Get-ThemeShadowDenseFrameStability($videoPath, [string]$caseDir, $recordingResult, $interactionResult)",
                "$cardEdgeShiftThreshold = 1.0",
                "$maxAnalyzedFrames = 72",
                "theme-shadow-dense-frames",
                "if ($framePaths.Count -gt $maxAnalyzedFrames)",
                "BaselineCardEdgeBounds = (Format-FrameRectangle $baselineCardBounds)",
                "MaxCardEdgeShift = [Math]::Round($maxCardEdgeShift, 3)",
                "CardEdgeStable = $cardEdgeStable",
                "function Test-LayoutStabilityEvidence($interactionResult)",
                "$interactionResult.Contains(\"LayoutStable\")",
                "function Get-LocalFrameDeltaByName($localFrameDeltas, [string]$name)",
                "function Test-ThemeShadowVisualEvidence($localFrameDeltas)",
                "Get-LocalFrameDeltaByName $localFrameDeltas \"ThemeShadowVisualBounds\"",
                "function Test-ThemeShadowArtifactVisualEvidence($interactionResult)",
                "$evidence.Contains(\"VisualChanged\")",
                "$evidence.Contains(\"ShadowEnvelopeChanged\")",
                "function Test-ThemeShadowArtifactCardStabilityEvidence($interactionResult)",
                "$evidence.Contains(\"CardEdgesStable\")",
                "function Test-ThemeShadowCasterStabilityEvidence($interactionResult)",
                "$interactionResult.Contains(\"ThemeShadowCasterStable\")",
                "function Test-ThemeShadowDenseFrameStabilityEvidence($themeShadowDenseFrameStability)",
                "$themeShadowDenseFrameStability.Contains(\"Generated\")",
                "$layoutStabilityEvidence = Test-LayoutStabilityEvidence $interactionResult",
                "$themeShadowVideoVisualEvidence = Test-ThemeShadowVisualEvidence $localFrameDeltas",
                "$themeShadowArtifactVisualEvidence = Test-ThemeShadowArtifactVisualEvidence $interactionResult",
                "$themeShadowVisualEvidence = $themeShadowVideoVisualEvidence",
                "$themeShadowCasterStabilityEvidence = Test-ThemeShadowCasterStabilityEvidence $interactionResult",
                "$themeShadowDenseFrameStabilityEvidence = Test-ThemeShadowDenseFrameStabilityEvidence $themeShadowDenseFrameStability",
                "$themeShadowArtifactCardStabilityEvidence = Test-ThemeShadowArtifactCardStabilityEvidence $interactionResult",
                "$layoutStabilityEvidenceAccepted =",
                "$control -eq \"ThemeShadow\"",
                "$interactionKind -eq \"Value\"",
                "$valueEvidence -and",
                "$layoutStabilityEvidence",
                "$themeShadowCasterStabilityEvidence -and",
                "$themeShadowArtifactCardStabilityEvidence -and",
                "$themeShadowDenseFrameStabilityEvidence -and",
                "$themeShadowVisualEvidence",
                "ThemeShadow depth interaction used {0}; expected animated RangeValuePattern transition inside the recorded clip.",
                "ThemeShadow depth changed but one or more sample bounds moved or could not be proven stable.",
                "ThemeShadow depth changed but the card/caster bounds moved or were not measured.",
                "ThemeShadow before/after rendered artifacts show the card edge moved or could not be proven stable.",
                "ThemeShadow depth changed but no rendered sample-root visual delta was proven.",
                "ThemeShadow rendered card moved or changed too much in dense video frames, indicating a visible layout shift.",
                "ThemeShadow depth changed but dense video frames did not prove the rendered card stayed fixed.",
                "The visible shadow envelope may expand with depth; live WinUI source-geometry captures show the same behavior.",
                "Before/after ThemeShadow artifacts changed visually and kept the rendered card edge fixed.",
                "shadowEnvelope={4}->{5}; envelopeDelta={6}.",
                "Dense ThemeShadow video frames kept the rendered card fixed.",
                "!$localVisualEvidence -and !$layoutStabilityEvidenceAccepted",
                "LayoutStabilityEvidence = $layoutStabilityEvidence",
                "ThemeShadowCasterStabilityEvidence = $themeShadowCasterStabilityEvidence",
                "ThemeShadowVideoVisualEvidence = $themeShadowVideoVisualEvidence",
                "ThemeShadowArtifactVisualEvidence = $themeShadowArtifactVisualEvidence",
                "ThemeShadowArtifactCardStabilityEvidence = $themeShadowArtifactCardStabilityEvidence",
                "ThemeShadowDenseFrameStabilityEvidence = $themeShadowDenseFrameStabilityEvidence",
                "ThemeShadowDenseFrameStability = $themeShadowDenseFrameStability",
                "ThemeShadowVisualEvidence = $themeShadowVisualEvidence");
            AssertContainsInOrder(
                source,
                "function Test-ControlSupportsOptionInteraction([string]$control)",
                "\"IconElement\" { return $true }",
                "\"InfoBadge\" { return $true }",
                "function Get-OptionInteractionTriggerName([string]$control)",
                "\"IconElement\" { return \"Monochrome\" }",
                "function Get-OptionInteractionTriggerAutomationId([string]$control)",
                "\"InfoBadge\" { return \"ToggleInfoBadgeOpacity\" }");
        }

        [TestMethod]
        public void GalleryInteractionRecorderHoverOpensOfficialWpfToolTip()
        {
            var source = File.ReadAllText(Path.Combine(
                GetRepoRoot(),
                "tools",
                "visual-checks",
                "Record-GalleryControlInteractions.ps1"));

            AssertContainsInOrder(
                source,
                "public static void MoveCursor(int x, int y)",
                "SetCursorPos(x, y);",
                "SendMouseInput(MOUSEEVENTF_MOVE);",
                "mouse_event(MOUSEEVENTF_MOVE, 1, 0, 0, UIntPtr.Zero);",
                "mouse_event(MOUSEEVENTF_MOVE, unchecked((uint)-1), 0, 0, UIntPtr.Zero);",
                "public static void MoveCursorOverWindow(IntPtr hWnd, int x, int y)",
                "SendMessage(hWnd, WM_MOUSEMOVE, UIntPtr.Zero, new IntPtr(packedPoint));",
                "PostMessage(hWnd, WM_MOUSEMOVE, UIntPtr.Zero, new IntPtr(packedPoint));",
                "public static void HoverCursorOverWindow(IntPtr hWnd, int x, int y)",
                "SendMessage(hWnd, WM_MOUSEHOVER, UIntPtr.Zero, new IntPtr(packedPoint));",
                "PostMessage(hWnd, WM_MOUSEHOVER, UIntPtr.Zero, new IntPtr(packedPoint));");
            AssertContainsInOrder(
                source,
                "function Test-ControlSupportsOpenInteraction([string]$control)",
                "\"ToolTip\" { return $true }",
                "function Get-OpenInteractionNames([string]$control)",
                "\"ToolTip\" { return @(\"Simple ToolTip\") }");
            AssertContainsInOrder(
                source,
                "function Get-ControlInteractionKind([string]$control)",
                "if (Test-ControlSupportsOpenInteraction $control) { return \"OpenRepeat\" }");
            AssertContainsInOrder(
                source,
                "function Get-ControlRecordingDurationSeconds([string]$control, [string]$interactionKind)",
                "if ($control -eq \"ToolTip\")",
                "return [Math]::Max($DurationSeconds, 18)");
            AssertContainsInOrder(
                source,
                "function Get-ToolTipFallbackBoundsFromTriggerBounds([string]$triggerBounds)",
                "$rect = ConvertFrom-BoundingRectangleString $triggerBounds",
                "[Math]::Round($rect.X + 10, 1)",
                "[Math]::Round($rect.Y + $rect.Height + 9, 1)",
                "90,",
                "25",
                "function ConvertFrom-BoundingRectangleString([string]$bounds)");
            AssertContainsInOrder(
                source,
                "$openElementTimeoutMilliseconds = if ($control -eq \"CommandBar\")",
                "elseif ($control -eq \"ToolTip\")",
                "800");
            AssertContainsInOrder(
                source,
                "function Invoke-OpenElementOnce($window, [string]$control, $element)",
                "if ($control -eq \"ToolTip\")",
                "$windowHandle = [IntPtr]$window.Current.NativeWindowHandle",
                "$offTargetX = [Math]::Max(1, $center.X - 160)",
                "$offTargetY = [Math]::Max(1, $center.Y - 160)",
                "[GalleryRecordingNative]::MoveCursorOverWindow($windowHandle, $offTargetX, $offTargetY)",
                "[GalleryRecordingNative]::Click($offTargetX, $offTargetY)",
                "$element.SetFocus()",
                "$entryX = if ($null -eq $bounds) { $center.X - 24 } else { [int][Math]::Floor($bounds.X - 24) }",
                "for ($step = 0; $step -le 8; $step++)",
                "[GalleryRecordingNative]::MoveCursorOverWindow($windowHandle, $x, $entryY)",
                "[GalleryRecordingNative]::HoverCursorOverWindow($windowHandle, $center.X, $center.Y)",
                "Start-Sleep -Milliseconds 1200");
            AssertContainsInOrder(
                source,
                "if ($control -eq \"ToolTip\")",
                "[GalleryRecordingNative]::Activate($window.Current.NativeWindowHandle)",
                "$pattern = $element.GetCurrentPattern([System.Windows.Automation.InvokePattern]::Pattern)",
                "$pattern.Invoke()",
                "Start-Sleep -Milliseconds 550",
                "return $true",
                "$center = Get-ElementCenter $element");
            AssertContainsInOrder(
                source,
                "function Get-OpenInteractionTriggerElement($window, [string]$control, $sampleElement)",
                "if ($control -eq \"ToolTip\")",
                "return Find-ElementByNameInProcess $window.Current.ProcessId @(\"TooltipButton\")",
                "function Find-OpenInteractionElement($window, $element, [string[]]$openNames, [string]$control)",
                "if ($control -eq \"ToolTip\")",
                "return Find-ElementByNameInProcess $window.Current.ProcessId $openNames");
            AssertContainsInOrder(
                source,
                "$firstOpenElementBounds = if (![string]::IsNullOrWhiteSpace($firstOpenElementBoundsHint)) { $firstOpenElementBoundsHint } else { Format-BoundingRectangle (Get-ElementBoundingRectangle $firstOpenElement) }",
                "if ($control -eq \"ToolTip\" -and [string]::IsNullOrWhiteSpace($firstOpenElementBounds))",
                "$firstOpenElementBounds = Get-ToolTipFallbackBoundsFromTriggerBounds $triggerBounds",
                "$firstOpenElementFound = $firstOpen -and ![string]::IsNullOrWhiteSpace($firstOpenElementBounds)",
                "$firstOpenElementAnchored = $firstOpenElementFound");
            AssertContainsInOrder(
                source,
                "$secondOpenElementBounds = if (![string]::IsNullOrWhiteSpace($secondOpenElementBoundsHint)) { $secondOpenElementBoundsHint } else { Format-BoundingRectangle (Get-ElementBoundingRectangle $secondOpenElement) }",
                "if ($control -eq \"ToolTip\" -and [string]::IsNullOrWhiteSpace($secondOpenElementBounds))",
                "$secondOpenElementBounds = Get-ToolTipFallbackBoundsFromTriggerBounds $secondTriggerBounds",
                "$secondOpenElementFound = $secondOpen -and ![string]::IsNullOrWhiteSpace($secondOpenElementBounds)",
                "$secondOpenElementAnchored = $secondOpenElementFound");
            AssertContainsInOrder(
                source,
                "function Test-OpenRepeatEvidence($interactionResult)",
                "$interactionResult.Contains(\"FirstOpenElementFound\")",
                "$interactionResult.Contains(\"SecondOpenElementFound\")",
                "$interactionResult.Contains(\"ClosedElementGone\")");
            Assert.IsFalse(
                source.Contains("if ($control -eq \"ToolTip\") { return \"PreparedOpen\" }", StringComparison.Ordinal),
                "ToolTip should not pass from an already-opened diagnostic tooltip.");
        }

        [TestMethod]
        public void ThemeShadowWinUIReferenceCaptureSupportsManifestDepth()
        {
            var source = File.ReadAllText(Path.Combine(
                GetRepoRoot(),
                "tools",
                "theme-shadow",
                "WinUIReferenceCapture",
                "Program.cs"));

            AssertContainsInOrder(
                source,
                "var depth = element.TryGetProperty(\"depth\", out var depthElement)",
                "? depthElement.GetSingle()",
                ": GetDepth(fileBase);",
                "var cornerRadius = element.TryGetProperty(\"cornerRadius\", out var cornerRadiusElement)",
                "? cornerRadiusElement.GetDouble()",
                ": GetCornerRadius(fileBase);",
                "mask,",
                "depth,",
                "cornerRadius));");
        }

        [TestMethod]
        public void GalleryInteractionRecorderProvesProgressBarAnimationAndStateChange()
        {
            var source = File.ReadAllText(Path.Combine(
                GetRepoRoot(),
                "tools",
                "visual-checks",
                "Record-GalleryControlInteractions.ps1"));

            StringAssert.Contains(source, "\"ProgressRing\", \"WinUIProgressBar\", \"AnnotatedScrollBar\"");
            AssertContainsInOrder(
                source,
                "function Get-RequiredSampleAutomationId([string]$control)",
                "\"IconElement\" { return \"GallerySample_IconElement_MonochromeButton\" }",
                "\"ProgressRing\" { return \"GallerySample_ProgressRing_ProgressRing\" }",
                "\"WinUIProgressBar\" { return \"GallerySample_WinUIProgressBar_IndeterminateProgressBar\" }");
            AssertContainsInOrder(
                source,
                "function Test-ControlSupportsOptionInteraction([string]$control)",
                "\"ProgressRing\" { return $true }",
                "\"WinUIProgressBar\" { return $true }",
                "function Test-ControlRequiresAnimatedVisualProof([string]$control)",
                "return $control -in @(\"ProgressRing\", \"WinUIProgressBar\", \"CommandBarFlyout\")");
            AssertContainsInOrder(
                source,
                "function Get-OptionInteractionTriggerName([string]$control)",
                "\"ProgressRing\" { return \"Progress Options\" }",
                "\"WinUIProgressBar\" { return \"Paused\" }",
                "function Get-OptionInteractionTriggerAutomationId([string]$control)",
                "\"IconElement\" { return \"GallerySample_IconElement_MonochromeButton\" }",
                "function Invoke-OptionInteraction($window, [string]$control, $sampleElement)",
                "$beforeSelectionState = Get-SelectionItemStateName $target",
                "$afterSelectionState = Get-SelectionItemStateName $target",
                "$beforeSelectionState -ne $afterSelectionState",
                "BeforeSelectionState = $beforeSelectionState",
                "AfterSelectionState = $afterSelectionState",
                "OptionChanged = if ($requiresExpectedElement) { $stateOrSampleChanged -and $expectedElementChanged } else { $stateOrSampleChanged }");
            AssertContainsInOrder(
                source,
                "function Scroll-ModernWpfSampleIntoView([string]$artifactDir, [string]$automationId)",
                "modernwpf-gallery-scroll-request.txt",
                "modernwpf-gallery-scroll-result.txt",
                "$result -eq \"$automationId|NotFound\"",
                "$parts.Count -eq 6",
                "$sampleId = Get-RequiredSampleAutomationId $control",
                "Scroll-ModernWpfSampleIntoView $artifactDir $sampleId",
                "$sampleElement = Find-DescendantByAutomationId $window $sampleId");
        }

        [TestMethod]
        public void GalleryInteractionRecorderExercisesOfficialWpfMessageBoxDialogs()
        {
            var source = File.ReadAllText(Path.Combine(
                GetRepoRoot(),
                "tools",
                "visual-checks",
                "Record-GalleryControlInteractions.ps1"));

            AssertContainsInOrder(
                source,
                "function Test-ControlSupportsOpenInteraction([string]$control)",
                "\"MessageBox\" { return $true }",
                "function Get-OpenInteractionNames([string]$control)",
                "\"MessageBox\" { return @(\"This is a simple message box!\") }");
            AssertContainsInOrder(
                source,
                "function Get-ControlRecordingDurationSeconds([string]$control, [string]$interactionKind)",
                "if ($control -eq \"MessageBox\")",
                "return [Math]::Max($DurationSeconds, 18)");
            AssertContainsInOrder(
                source,
                "function Test-ControlAllowsDetachedOpenRepeatElement([string]$control)",
                "return $control -eq \"MessageBox\"");
            AssertContainsInOrder(
                source,
                "function Invoke-MessageBoxButtonWithDelayedClose($trigger, [string[]]$openNames, [int]$processId, [int]$dwellMilliseconds)",
                "$closer = [powershell]::Create()",
                "$closer.BeginInvoke()",
                "$pattern = $trigger.GetCurrentPattern([System.Windows.Automation.InvokePattern]::Pattern)",
                "$pattern.Invoke()",
                "$closer.EndInvoke($asyncResult)",
                "OpenElementFound = [bool]$result[0].OpenElementFound",
                "OpenElementBounds = [string]$result[0].OpenElementBounds",
                "Closed = [bool]$result[0].Closed");
            AssertContainsInOrder(
                source,
                "function Invoke-MessageBoxOpenRepeatInteraction($window, [string]$control, $sampleElement)",
                "$firstOpenResult = Invoke-MessageBoxButtonWithDelayedClose $trigger $openNames $window.Current.ProcessId 1800",
                "$secondOpenResult = Invoke-MessageBoxButtonWithDelayedClose $secondTrigger $openNames $window.Current.ProcessId 1800",
                "FirstOpenElementBounds = $firstOpenElementBounds",
                "SecondOpenElementBounds = $secondOpenElementBounds",
                "function Invoke-OpenRepeatInteraction($window, [string]$control, $sampleElement)",
                "if ($control -eq \"MessageBox\")",
                "return Invoke-MessageBoxOpenRepeatInteraction $window $control $sampleElement");
            AssertContainsInOrder(
                source,
                "function Find-MessageBoxDialogElement($window, [string[]]$names)",
                "if ([int]$candidateWindow.Current.NativeWindowHandle -eq $mainHandle)",
                "Find-DescendantByAnyName $candidateWindow $names",
                "function Find-MessageBoxDialogButton($window, [string[]]$names)",
                "Find-DescendantButtonByAnyName $candidateWindow $names");
            AssertContainsInOrder(
                source,
                "function Get-OpenInteractionTriggerElement($window, [string]$control, $sampleElement)",
                "if ($control -eq \"MessageBox\")",
                "return Find-InteractiveElementByNameInProcess $window.Current.ProcessId @(\"Simple MessageBox\")",
                "function Find-OpenInteractionElement($window, $element, [string[]]$openNames, [string]$control)",
                "if ($control -eq \"MessageBox\")",
                "return Find-MessageBoxDialogElement $window $openNames");
            AssertContainsInOrder(
                source,
                "Add-Type -TypeDefinition @\"",
                "public static class GalleryMessageBoxCloserNative",
                "public static void Enter()",
                "$okButton.GetCurrentPattern([System.Windows.Automation.InvokePattern]::Pattern).Invoke()",
                "$method = \"DialogOkButton:Invoke\"");
            AssertContainsInOrder(
                source,
                "function Test-ControlRequiresLiveVisualClose([string]$control)",
                "$control -eq \"MessageBox\" -or");
            Assert.IsFalse(
                source.Contains("InvokeAutomationPatternAsync", StringComparison.Ordinal),
                "MessageBox must not share a UIA InvokePattern object across threads; the closer re-finds elements in its own runspace.");
        }

        [TestMethod]
        public void GalleryVisualChecksTypesAutoSuggestBoxAndChoosesSuggestion()
        {
            var source = File.ReadAllText(Path.Combine(
                GetRepoRoot(),
                "tools",
                "visual-checks",
                "Run-GalleryVisualChecks.ps1"));

            AssertContainsInOrder(
                source,
                "private static extern short VkKeyScan(char ch)",
                "public static void PressCtrlA()",
                "public static void TypeText(string text)");
            AssertContainsInOrder(
                source,
                "function Test-ControlSupportsTextInteraction([string]$control)",
                "\"AutoSuggestBox\" { return $true }",
                "function Get-TextInteractionInput([string]$control)",
                "\"AutoSuggestBox\" { return \"ae\" }",
                "function Get-TextInteractionSuggestionNames([string]$control)",
                "\"AutoSuggestBox\" { return @(\"Aegean\") }",
                "function Get-TextInteractionExpectedOutputName([string]$control)",
                "\"AutoSuggestBox\" { return \"Aegean\" }");
            AssertContainsInOrder(
                source,
                "function Find-EditableDescendant($element)",
                "[System.Windows.Automation.ControlType]::Edit",
                "function Set-EditableElementText($window, $element, [string]$text)",
                "[GalleryVisualNative]::PressCtrlA()",
                "[GalleryVisualNative]::TypeText($text)",
                "[System.Windows.Automation.ValuePattern]::Pattern",
                "$pattern.SetValue($text)");
            AssertContainsInOrder(
                source,
                "function Find-ListItemOutsideElementBounds($window, $element, [string[]]$names)",
                "$match.Current.ControlType -ne [System.Windows.Automation.ControlType]::ListItem",
                "$outsideAnchor = $rect.Y -ge ($anchorRect.Bottom - 1) -or $rect.Bottom -le ($anchorRect.Y + 1)",
                "function Wait-ForListItemOutsideElementBounds($window, $element, [string[]]$names, [int]$timeoutMs = 2500)",
                "function Find-OutputTextOutsideElementBounds($window, $element, [string]$name)",
                "$match.Current.ControlType -ne [System.Windows.Automation.ControlType]::Text",
                "function Wait-ForOutputTextOutsideElementBounds($window, $element, [string]$name, [int]$timeoutMs = 2500)");
            AssertContainsInOrder(
                source,
                "function Capture-TextInteraction([string]$app, [string]$control, [string]$caseDir, $window, $element)",
                "if (!$IncludeInteractions -or !(Test-ControlSupportsTextInteraction $control))",
                "$typed = Set-EditableElementText $window $element $inputText",
                "Wait-ForListItemOutsideElementBounds $window $element $suggestionNames 3000",
                "Capture-Window $popupHandle $popupScreenshot -SkipActivate",
                "$popupNonBlank = Test-ImageNotBlank $popupScreenshot",
                "$suggestionInvoked = Invoke-ElementOnce $window $suggestionElement",
                "Wait-ForOutputTextOutsideElementBounds $window $element $expectedOutputName 3000",
                "Source = \"PopupWindow\"",
                "$installedReferenceUnavailable =",
                "$app -eq \"WinUI3\" -and",
                "$status = if ($installedReferenceUnavailable) { \"Unavailable\" }",
                "ReferenceInteractionUnavailable = $installedReferenceUnavailable",
                "SuggestionElementFound = $null -ne $suggestionElement",
                "OutputElementFound = $null -ne $outputElement");
            StringAssert.Contains(
                source,
                "elseif ($null -ne $interaction -and $interaction.Status -eq \"Failed\") { \"Failed\" }");
            StringAssert.Contains(
                source,
                "The installed WinUI Gallery accepts typed text in its basic sample but does not expose the source-defined suggestions popup.");
        }

        [TestMethod]
        public void GalleryVisualChecksFallsBackToNativeScreenCapture()
        {
            var source = File.ReadAllText(Path.Combine(
                GetRepoRoot(),
                "tools",
                "visual-checks",
                "Run-GalleryVisualChecks.ps1"));

            AssertContainsInOrder(
                source,
                "private static extern IntPtr GetDC(IntPtr hWnd);",
                "public static bool CopyScreenSurface(IntPtr hdcDest, int sourceX, int sourceY, int width, int height)",
                "IntPtr hdcSource = GetDC(IntPtr.Zero);",
                "return BitBlt(hdcDest, 0, 0, width, height, hdcSource, sourceX, sourceY, 0x00CC0020);",
                "function Capture-ScreenRect([IntPtr]$hwnd, [string]$path)",
                "$graphics.CopyFromScreen($rect.Left, $rect.Top, 0, 0, [System.Drawing.Size]::new($width, $height))",
                "$copied = [GalleryVisualNative]::CopyScreenSurface($hdc, $rect.Left, $rect.Top, $width, $height)",
                "CopyFromScreen failed and native screen capture fallback failed");
        }

        [TestMethod]
        public void WpfGalleryVisualAuditUsesSingleRenderedContentArtifactPriority()
        {
            var source = File.ReadAllText(Path.Combine(
                GetRepoRoot(),
                "tools",
                "visual-checks",
                "Run-WpfGalleryVisualAudit.ps1"));

            AssertContainsInOrder(
                source,
                "function Get-ModernRenderedContentArtifactCandidates()",
                @"FileName = ""HomeContentRootPane.png""; Source = ""HomeContentRootPaneRenderedArtifact""",
                @"FileName = ""AllControlsContentRootPane.png""; Source = ""AllControlsContentRootPaneRenderedArtifact""",
                @"FileName = ""SettingsContentRootPane.png""; Source = ""SettingsContentRootPaneRenderedArtifact""",
                @"FileName = ""ContentPagePane.png""; Source = ""ContentPagePaneRenderedArtifact""",
                @"FileName = ""GalleryItemPageRoot.png""; Source = ""GalleryItemPageRootRenderedArtifact""",
                @"FileName = ""ContentRootGrid.png""; Source = ""ContentRootGridRenderedArtifact""",
                @"FileName = ""GalleryContentHost.png""; Source = ""GalleryContentHostRenderedArtifact""",
                "function Get-ModernRenderedContentArtifactCrop");
            AssertContainsInOrder(
                source,
                "function Get-ModernRenderedContentArtifactCrop",
                "foreach ($candidate in (Get-ModernRenderedContentArtifactCandidates))",
                "$contentCrop = Get-ImageArtifactInfo $path $candidate.Source",
                "return $contentCrop",
                "function Test-ModernRenderedContentArtifact");
            AssertContainsInOrder(
                source,
                "function Test-ModernRenderedContentArtifact",
                "return $null -ne (Get-ModernRenderedContentArtifactCrop $artifactDir)",
                "function Capture-Window");
            AssertContainsInOrder(
                source,
                "function Save-ModernShellNavigationArtifactCrop",
                "$navigationArtifact = Join-Path $artifactDir \"GalleryNavigationView.png\"",
                "return Save-Crop $navigationArtifact $path 12 8 250 $height \"ModernWpfNavigationPaneRenderedArtifact\"",
                "$contentCrop = $null",
                "if (Test-ShellNavigationCase $case) {",
                "$contentCrop = Save-ModernShellNavigationArtifactCrop $artifactDir $contentCropPath",
                "if (($null -eq $contentCrop -or !$contentCrop.NonBlank) -and $windowNonBlank) {",
                "$contentCrop = Save-ModernContentCrop $window $screenshot $contentCropPath $case $isRenderedWindowArtifact",
                "$contentCrop = Save-ModernShellNavigationArtifactCrop $artifactDir $contentCropPath",
                "else {",
                "$contentCrop = Get-ModernRenderedContentArtifactCrop $artifactDir");
        }

        [TestMethod]
        public void WpfGalleryVisualAuditLaunchesOfficialDisplayRoutesWithCanonicalReadyRoutes()
        {
            var source = File.ReadAllText(Path.Combine(
                GetRepoRoot(),
                "tools",
                "visual-checks",
                "Run-WpfGalleryVisualAudit.ps1"));

            StringAssert.Contains(source, "ReadyRoute = $readyRoute");
            StringAssert.Contains(source, "$_.ReadyRoute -eq $caseId");
            StringAssert.Contains(source, "function Wait-ModernWpfRouteReady");
            StringAssert.Contains(source, "Ready:$route");
            StringAssert.Contains(source, "$case.ReadyRoute");
            StringAssert.Contains(source, "New-Case \"ShellHomeNavigation\" \"home\" @(\"Home\") \"\" \"home\"");
            StringAssert.Contains(source, "New-Case \"ShellDesignGuidance\" \"category/Design Guidance\" @(\"Design Guidance\") \"\" \"category/DesignGuidance\"");
            StringAssert.Contains(source, "New-Case \"ShellClickDesignGuidance\" \"home\" @(\"Design Guidance\") \"\" \"category/DesignGuidance\" @(\"Design Guidance\") \"home\"");
            StringAssert.Contains(source, "New-Case \"ShellClickDesignGuidanceCollapse\" \"home\" @(\"Design Guidance\") \"\" \"category/DesignGuidance\" @(\"Design Guidance\", \"Design Guidance\") \"home\"");
            StringAssert.Contains(source, "New-Case \"AllControls\" \"All Controls\" @(\"All Controls\") \"\" \"AllControls\"");
            StringAssert.Contains(source, "New-Case \"DesignGuidance\" \"category/Design Guidance\" @(\"Design Guidance\") \"\" \"category/DesignGuidance\"");
            StringAssert.Contains(source, "New-Case \"Color\" \"item/Colors\" @(\"Design Guidance\", \"Colors\") \"\" \"item/Color\"");
            StringAssert.Contains(source, "New-Case \"Iconography\" \"item/Icons\" @(\"Design Guidance\", \"Icons\") \"\" \"item/Iconography\"");
            StringAssert.Contains(source, "New-Case \"DateAndCalendar\" \"category/Date & Calendar\" @(\"Date & Calendar\") \"\" \"category/DateAndCalendar\"");
            StringAssert.Contains(source, "New-Case \"StatusAndInfo\" \"category/Status & Info\" @(\"Status & Info\") \"\" \"category/StatusAndInfo\"");
            StringAssert.Contains(source, "New-Case \"RichTextBox\" \"item/RichTextBox\" @(\"Text\", \"RichTextEdit\")");
            Assert.IsFalse(source.Contains("New-Case \"DateAndCalendar\" \"category/DateAndCalendar\"", StringComparison.Ordinal));
            Assert.IsFalse(source.Contains("New-Case \"StatusAndInfo\" \"category/StatusAndInfo\"", StringComparison.Ordinal));
            Assert.IsFalse(source.Contains("New-Case \"Iconography\" \"item/Iconography\"", StringComparison.Ordinal));
            foreach (var retiredCase in new[]
            {
                "Samples",
                "UserDashboard",
                "Media",
                "Canvas",
                "Image",
                "System",
                "FileAndFolderDialogs",
                "MessageBox",
                "Clipboard"
            })
            {
                Assert.IsFalse(
                    source.Contains("New-Case \"" + retiredCase + "\"", StringComparison.Ordinal),
                    retiredCase + " should not be scheduled by the active WPF Gallery visual audit.");
            }
        }

        [TestMethod]
        public void WpfGalleryVisualAuditValidatesShellClickExpansionState()
        {
            var source = File.ReadAllText(Path.Combine(
                GetRepoRoot(),
                "tools",
                "visual-checks",
                "Run-WpfGalleryVisualAudit.ps1"));

            AssertContainsInOrder(
                source,
                "function Get-ModernShellExpectedNavigationStates($case)",
                "\"ShellClickDesignGuidance\"",
                "ChildNames = @(\"Colors\", \"Typography\", \"Spacing\", \"Geometry\", \"Icons\")",
                "FollowingName = \"All Controls\"",
                "\"ShellClickDesignGuidanceCollapse\"",
                "HiddenChildNames = @(\"Colors\", \"Typography\", \"Spacing\", \"Geometry\", \"Icons\")",
                "FollowingName = \"All Controls\"");
            AssertContainsInOrder(
                source,
                "function Click-ModernNavigationItemChevron($item)",
                "$rect = $item.Current.BoundingRectangle",
                "[int][Math]::Round($rect.X + 32.0)",
                "[int][Math]::Round($rect.Y + [Math]::Min(20.0, $rect.Height / 2.0))");
            AssertContainsInOrder(
                source,
                "function Test-ModernShellNavigationState($window, $case)",
                "$item = Find-ModernNavigationItemByName $window $expected.Name",
                "$actualState = Get-ExpandCollapseStateName $item",
                "$actualState -ne $expected.State",
                "$rect = $item.Current.BoundingRectangle",
                "$expected.MaximumHeight -gt 0 -and $rect.Height -gt $expected.MaximumHeight",
                "$expected.Contains(\"ChildNames\")",
                "Find-DescendantByNameAndType $item $childName ([System.Windows.Automation.ControlType]::ListItem)",
                "$expected.Contains(\"FollowingName\")",
                "$followingGap = $followingRect.Top - $rect.Bottom",
                "return $failures -join \" \"");
            AssertContainsInOrder(
                source,
                "function Navigate-ModernWpfGalleryByClicks($window, $case)",
                "for ($clickIndex = 0; $clickIndex -lt $case.ModernClickPath.Count; $clickIndex++)",
                "$name = $case.ModernClickPath[$clickIndex]",
                "if ($case.Id -eq \"ShellClickDesignGuidanceCollapse\" -and $clickIndex -gt 0) {",
                "$clicked = Click-ModernNavigationItemChevron $item",
                "$clicked = Click-TreeItemHeader $item $name",
                "if (!$clicked) {",
                "$clicked = Click-Element $item");
            AssertContainsInOrder(
                source,
                "$case.Id -ne \"ShellClickDesignGuidanceCollapse\"",
                "[void](Invoke-Element $lastItem)",
                "$case.Id -ne \"ShellClickDesignGuidanceCollapse\"",
                "Wait-ModernWpfRouteReady");
            var captureStart = source.IndexOf(
                "function Capture-ModernWpf",
                StringComparison.Ordinal);
            var captureEnd = source.IndexOf(
                "function Capture-OfficialWpfGalleryDirectHost",
                captureStart,
                StringComparison.Ordinal);
            Assert.IsTrue(captureStart >= 0 && captureEnd > captureStart);
            var captureSource = source.Substring(captureStart, captureEnd - captureStart);
            AssertContainsInOrder(
                captureSource,
                "$contentNonBlank = $null -ne $contentCrop -and [bool]$contentCrop.NonBlank",
                "if ([string]::IsNullOrWhiteSpace($lastException) -and",
                "!$contentNonBlank -and",
                "if ([string]::IsNullOrWhiteSpace($lastException)) {",
                "$lastException = Test-ModernShellNavigationState $window $case",
                "if ([string]::IsNullOrWhiteSpace($lastException) -and !$contentNonBlank)",
                "$lastException = \"ModernWpf rendered content was unavailable or blank.\"",
                "Status = $(if ($contentNonBlank -and [string]::IsNullOrWhiteSpace($lastException)) { \"Passed\" } else { \"Failed\" })");
            Assert.IsFalse(captureSource.Contains(
                "Status = $(if (($windowNonBlank -or $contentCrop.NonBlank)",
                StringComparison.Ordinal));
        }

        [TestMethod]
        public void NavigationViewItemTemplateKeepsExpandableHeaderRowAuto()
        {
            var xaml = ReadRepoFile(
                "ModernWpf.Controls",
                "NavigationView",
                "NavigationView.xaml");

            AssertContainsInOrder(
                xaml,
                "<Grid x:Name=\"NVIRootGrid\">",
                "<Grid.RowDefinitions>",
                "<RowDefinition Height=\"Auto\"/>",
                "<RowDefinition Height=\"Auto\"/>",
                "</Grid.RowDefinitions>",
                "<primitives:NavigationViewItemPresenter",
                "<local:ItemsRepeater",
                "Grid.Row=\"1\"",
                "x:Name=\"NavigationViewItemMenuItemsHost\">");
        }

        [TestMethod]
        public void WpfGalleryPageStylesKeepOfficialResourceSetterSourceShape()
        {
            var xaml = ReadRepoFile(
                "ModernWpf.Gallery",
                "Resources",
                "PageStyles.xaml");

            AssertContainsInOrder(
                xaml,
                "<Style x:Key=\"BaseTextBlockStyle\" TargetType=\"TextBlock\">",
                "<Setter Property=\"FontSize\" Value=\"{StaticResource BodyTextBlockFontSize}\" />",
                "<Setter Property=\"FontWeight\" Value=\"SemiBold\" />",
                "<Setter Property=\"TextTrimming\" Value=\"CharacterEllipsis\" />",
                "<Setter Property=\"TextWrapping\" Value=\"Wrap\" />",
                "<Setter Property=\"LineStackingStrategy\" Value=\"MaxHeight\" />",
                "</Style>");
            AssertContainsInOrder(
                xaml,
                "x:Key=\"DisplayTextBlockStyle\"",
                "<Setter Property=\"FontSize\" Value=\"{StaticResource DisplayTextBlockFontSize}\" />",
                "<ImageBrush x:Key=\"p64\" ImageSource=\"pack://application:,,,/ModernWpf.Gallery;component/Assets/UserDashboard/64-100x100.jpg\" />",
                "<ImageBrush x:Key=\"p505\" ImageSource=\"pack://application:,,,/ModernWpf.Gallery;component/Assets/UserDashboard/505-100x100.jpg\" />",
                "<Style x:Key=\"ColorTilesPanelStyle\" TargetType=\"{x:Type Border}\">");
            AssertContainsInOrder(
                xaml,
                "<Style x:Key=\"ColorTilesPanelStyle\" TargetType=\"{x:Type Border}\">",
                "<Style.Setters>",
                "<Setter Property=\"Background\" Value=\"{DynamicResource ControlExampleDisplayBrush}\" />",
                "<Setter Property=\"BorderThickness\" Value=\"1\" />",
                "<Setter Property=\"BorderBrush\" Value=\"{DynamicResource CardStrokeColorDefaultBrush}\" />",
                "<Setter Property=\"CornerRadius\" Value=\"8\" />",
                "</Style.Setters>",
                "<Style x:Key=\"GalleryPageRootStyle\" TargetType=\"Grid\">");
        }

        [TestMethod]
        public void WpfGalleryConvertersKeepOfficialVisibilitySourceShape()
        {
            var nullConverterSource = ReadRepoFile(
                "ModernWpf.Gallery",
                "Controls",
                "NullToVisibilityConverter.cs");
            AssertContainsInOrder(
                nullConverterSource,
                "/// Converts a null value to Visibility.Collapsed",
                "return value is null ? Visibility.Collapsed : Visibility.Visible;",
                "throw new NotImplementedException();");

            var userDashboardConverterSource = ReadRepoFile(
                "ModernWpf.Gallery",
                "Pages",
                "WpfGallery",
                "Samples",
                "UserDashboardConverters.cs");
            AssertContainsInOrder(
                userDashboardConverterSource,
                "/// Converts an empty string to Visibility.Collapsed",
                "if (value is string str)",
                "return string.IsNullOrWhiteSpace(str) ? Visibility.Collapsed : Visibility.Visible;",
                "return value is null ? Visibility.Collapsed : Visibility.Visible;",
                "throw new NotImplementedException();",
                "/// Converts an image id to a brush");
        }

        [TestMethod]
        public void WpfGalleryTemplatesKeepOfficialNavigationCardSourceShape()
        {
            var xaml = ReadRepoFile(
                "ModernWpf.Gallery",
                "Resources",
                "Templates.xaml");
            var normalizedXaml = xaml.Replace("\r\n", "\n").Replace('\r', '\n');

            AssertContainsInOrder(
                xaml,
                "<ResourceDictionary",
                "xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\"",
                "xmlns:x=\"http://schemas.microsoft.com/winfx/2006/xaml\"",
                "xmlns:pages=\"clr-namespace:ModernWpf.Gallery.Pages\">",
                "<ItemsPanelTemplate x:Key=\"WrapPanelTemplate\">");
            StringAssert.Contains(
                normalizedXaml,
                "<WrapPanel Margin=\"10\"\n                Orientation=\"Horizontal\"/>");
            AssertContainsInOrder(
                xaml,
                "<DataTemplate x:Key=\"NavigationCardTemplate\">",
                "<Button",
                "Width=\"360\"",
                "Height=\"90\"",
                "Margin=\"7\"",
                "Padding=\"20,10\"",
                "HorizontalContentAlignment=\"Left\"",
                "AutomationProperties.Name=\"{Binding Title, StringFormat='{}{0}Page'}\"",
                "Command=\"{Binding ViewModel.NavigateCommand, RelativeSource={RelativeSource AncestorType={x:Type Page}}}\"",
                "CommandParameter=\"{Binding PageType}\">");
            StringAssert.Contains(
                normalizedXaml,
                "<Image Source=\"{Binding ImageSource}\"\n                        Width=\"50\"\n                        Height=\"50\"\n                        Margin=\"0,0,8,0\"/>");
            AssertContainsInOrder(
                xaml,
                "<TextBlock",
                "Margin=\"10,0,0,0\"",
                "Style=\"{StaticResource BodyStrongTextBlockStyle}\"",
                "Text=\"{Binding Title}\" pages:GalleryAutomation.HeadingLevel=\"Level3\" />",
                "<TextBlock",
                "Width=\"240\"",
                "Margin=\"10,0,0,0\"",
                "Foreground=\"{DynamicResource TextFillColorPrimaryBrush}\"",
                "Opacity=\"0.7\"",
                "Style=\"{StaticResource CaptionTextBlockStyle}\"",
                "Text=\"{Binding Description}\"/>");
        }

        [TestMethod]
        public void SharedControlExampleKeepsOfficialSourceCodeTemplateShape()
        {
            var xaml = ReadRepoFile(
                "ModernWpf.Gallery",
                "Controls",
                "ControlExample.xaml");
            var normalizedXaml = xaml.Replace("\r\n", "\n").Replace('\r', '\n');

            StringAssert.Contains(
                xaml,
                "<controls:NullToVisibilityConverter x:Key=\"NullToVisibilityConverter\" />");
            StringAssert.Contains(
                normalizedXaml,
                "Visibility=\"{TemplateBinding HeaderText,\n                            Converter={StaticResource NullToVisibilityConverter}}\" />");
            AssertContainsInOrder(
                xaml,
                "<Border",
                "Grid.Row=\"1\"",
                "Background=\"{DynamicResource SolidBackgroundFillColorBaseBrush}\"",
                "BorderBrush=\"{DynamicResource CardStrokeColorDefaultBrush}\"",
                "BorderThickness=\"1,1,1,0\"",
                "CornerRadius=\"8,8,0,0\"",
                "TextElement.FontSize=\"{StaticResource BodyTextBlockFontSize}\">",
                "<Grid>",
                "<Grid.ColumnDefinitions>",
                "<ColumnDefinition Width=\"*\" />",
                "x:Name=\"OutputColumn\"",
                "Width=\"Auto\"",
                "MaxWidth=\"320\" />",
                "x:Name=\"OptionsColumn\"",
                "Width=\"Auto\"",
                "MaxWidth=\"{Binding OptionsMaxWidth, RelativeSource={RelativeSource TemplatedParent}}\" />",
                "<ContentPresenter",
                "Margin=\"12\"",
                "HorizontalAlignment=\"{TemplateBinding HorizontalContentAlignment}\"",
                "VerticalAlignment=\"{TemplateBinding VerticalContentAlignment}\"",
                "Content=\"{TemplateBinding ExampleContent}\" />",
                "<Border",
                "x:Name=\"OutputRoot\"",
                "Grid.Column=\"1\"",
                "Margin=\"0,12,12,12\"",
                "Padding=\"16\"",
                "Content=\"{TemplateBinding OutputContent}\"",
                "<Border",
                "x:Name=\"OptionsRoot\"",
                "Grid.Column=\"2\"",
                "Padding=\"16\"",
                "Background=\"{DynamicResource CardBackgroundFillColorDefaultBrush}\"",
                "BorderBrush=\"{DynamicResource DividerStrokeColorDefaultBrush}\"",
                "BorderThickness=\"1,0,0,0\"",
                "CornerRadius=\"0,8,0,0\">",
                "Content=\"{TemplateBinding OptionsContent}\"");
            Assert.IsFalse(
                xaml.Contains("ControlExampleSourceExpanderStyle", StringComparison.Ordinal),
                "The source-code expander should use the official WPF Gallery default Expander template.");
            AssertContainsInOrder(
                xaml,
                "<Expander",
                "x:Name=\"SourceCodeExpander\"",
                "Grid.Row=\"2\"",
                "AutomationProperties.Name=\"{Binding HeaderText, RelativeSource={RelativeSource TemplatedParent}, StringFormat=View Source Code for {0}}\"",
                "Header=\"Source code\"",
                "<StackPanel>",
                "<StackPanel x:Name=\"XamlCodeBlock\">");
            StringAssert.Contains(
                xaml,
                "<Button Grid.Column=\"1\" Padding=\"8\" Command=\"ApplicationCommands.Copy\" CommandParameter=\"Copy_XamlCode\" ToolTipService.ToolTip=\"Copy to clipboard\" AutomationProperties.Name=\"Copy XAML Code\" >");
            StringAssert.Contains(
                xaml,
                "<TextBlock x:Name=\"CopyGlyphTextBlock\" FontFamily=\"{StaticResource SymbolThemeFontFamily}\" Text=\"&#xE8C8;\"/>");
            StringAssert.Contains(
                xaml,
                "<TextBlock x:Name=\"SuccessGlyphTextBlock\" FontFamily=\"{StaticResource SymbolThemeFontFamily}\" Text=\"&#xE73E;\" Opacity=\"0\" />");
            AssertContainsInOrder(
                xaml,
                "<EventTrigger RoutedEvent=\"Button.Click\">",
                "<EventTrigger.Actions>",
                "<Storyboard BeginTime=\"00:00:00\">",
                "<DoubleAnimation Duration=\"0:0:0.333\" Storyboard.TargetName=\"CopyGlyphTextBlock\" Storyboard.TargetProperty=\"Opacity\" To=\"0\" />",
                "<DoubleAnimation Duration=\"0:0:0.666\" BeginTime=\"0:0:0.333\" Storyboard.TargetName=\"SuccessGlyphTextBlock\" Storyboard.TargetProperty=\"Opacity\" To=\"1\" />",
                "<DoubleAnimation Storyboard.TargetName=\"SuccessGlyphTextBlock\" BeginTime=\"0:0:2\" Duration=\"0:0:0.01\" Storyboard.TargetProperty=\"Opacity\" To=\"0\" />",
                "<DoubleAnimation Storyboard.TargetName=\"CopyGlyphTextBlock\" BeginTime=\"0:0:2.1\" Duration=\"0:0:0.01\" Storyboard.TargetProperty=\"Opacity\" To=\"1\" />",
                "</EventTrigger.Actions>");
            AssertContainsInOrder(
                xaml,
                "<TextBox",
                "Style=\"{StaticResource SelectionTextBox}\"",
                "AutomationProperties.Name=\"XAML Source Code\"",
                "Text=\"{TemplateBinding XamlCode}\"/>",
                "<Border",
                "x:Name=\"Border\"",
                "Margin=\"0,20\"",
                "BorderThickness=\"1\"",
                "Visibility=\"Visible\" />");
            AssertContainsInOrder(
                xaml,
                "<StackPanel x:Name=\"CSharpCodeBlock\">",
                "<TextBlock",
                "Margin=\"0,0,0,5\"",
                "Style=\"{StaticResource BodyStrongTextBlockStyle}\"",
                "Foreground=\"{DynamicResource TextFillColorPrimaryBrush}\"",
                "Text=\"C#\" />");
            StringAssert.Contains(
                xaml,
                "<Button Grid.Column=\"1\" Padding=\"8\" Command=\"ApplicationCommands.Copy\" CommandParameter=\"Copy_CSharpCode\" FocusManager.IsFocusScope=\"True\" >");
            AssertContainsInOrder(
                xaml,
                "<TextBox",
                "Style=\"{StaticResource SelectionTextBox}\"",
                "AutomationProperties.Name=\"CSharp Source Code\"",
                "Text=\"{TemplateBinding CSharpCode}\" />");
            AssertContainsInOrder(
                xaml,
                "<Trigger Property=\"OptionsContent\" Value=\"{x:Null}\">",
                "<Setter TargetName=\"OptionsRoot\" Property=\"Visibility\" Value=\"Collapsed\" />",
                "</Trigger>",
                "<Trigger Property=\"OutputContent\" Value=\"{x:Null}\">",
                "<Setter TargetName=\"OutputRoot\" Property=\"Visibility\" Value=\"Collapsed\" />",
                "</Trigger>");
        }

        [TestMethod]
        public void SharedSupportControlCodeBehindKeepsOfficialDependencyPropertyMemberOrderShape()
        {
            var controlExampleSource = ReadRepoFile(
                "ModernWpf.Gallery",
                "Controls",
                "ControlExample.cs");
            var normalizedControlExampleSource = controlExampleSource.Replace("\r\n", "\n").Replace('\r', '\n');
            StringAssert.Contains(
                normalizedControlExampleSource,
                "public static readonly DependencyProperty HeaderTextProperty = DependencyProperty.Register(\n" +
                "            nameof(HeaderText),\n" +
                "            typeof(string),\n" +
                "            typeof(ControlExample),\n" +
                "            new PropertyMetadata(null)\n" +
                "        );");
            StringAssert.Contains(
                normalizedControlExampleSource,
                "public static readonly DependencyProperty XamlCodeSourceProperty = DependencyProperty.Register(\n" +
                "            nameof(XamlCodeSource),\n" +
                "            typeof(Uri),\n" +
                "            typeof(ControlExample),\n" +
                "            new PropertyMetadata(\n" +
                "                null,\n" +
                "                static (o, args) => ((ControlExample)o).OnXamlCodeSourceChanged((Uri)args.NewValue)\n" +
                "            )\n" +
                "        );");
            AssertContainsInOrder(
                controlExampleSource,
                "/// A control that displays an example of a control",
                "[ContentProperty(nameof(ExampleContent))]",
                "CommandManager.RegisterClassCommandBinding(typeof(ControlExample), new CommandBinding(ApplicationCommands.Copy, Copy_SourceCode));",
                "public static readonly DependencyProperty HeaderTextProperty = DependencyProperty.Register(",
                "public static readonly DependencyProperty ExampleContentProperty = DependencyProperty.Register(",
                "public static readonly DependencyProperty OptionsContentProperty = DependencyProperty.Register(",
                "public static readonly DependencyProperty XamlCodeProperty = DependencyProperty.Register(",
                "public static readonly DependencyProperty XamlCodeSourceProperty = DependencyProperty.Register(",
                "static (o, args) => ((ControlExample)o).OnXamlCodeSourceChanged((Uri)args.NewValue)",
                "public static readonly DependencyProperty CSharpCodeProperty = DependencyProperty.Register(",
                "public static readonly DependencyProperty CSharpCodeSourceProperty = DependencyProperty.Register(",
                "static (o, args) => ((ControlExample)o).OnCSharpCodeSourceChanged((Uri)args.NewValue)",
                "public string HeaderText",
                "get => (string)GetValue(HeaderTextProperty);",
                "set => SetValue(HeaderTextProperty, value);",
                "public object ExampleContent",
                "get => GetValue(ExampleContentProperty);",
                "set => SetValue(ExampleContentProperty, value);",
                "public object OptionsContent",
                "get => GetValue(OptionsContentProperty);",
                "set => SetValue(OptionsContentProperty, value);",
                "public string XamlCode",
                "get => (string)GetValue(XamlCodeProperty);",
                "set => SetValue(XamlCodeProperty, value);",
                "public Uri XamlCodeSource",
                "get => (Uri)GetValue(XamlCodeSourceProperty);",
                "set => SetValue(XamlCodeSourceProperty, value);",
                "public string CSharpCode",
                "get => (string)GetValue(CSharpCodeProperty);",
                "set => SetValue(CSharpCodeProperty, value);",
                "public Uri CSharpCodeSource",
                "get => (Uri)GetValue(CSharpCodeSourceProperty);",
                "set => SetValue(CSharpCodeSourceProperty, value);",
                "private void OnXamlCodeSourceChanged(Uri uri)",
                "XamlCode = LoadResource(uri);",
                "private void OnCSharpCodeSourceChanged(Uri uri)",
                "CSharpCode = LoadResource(uri);",
                "private static void Copy_SourceCode(object sender, RoutedEventArgs e)",
                "if (sender is ControlExample controlExample)",
                "if (!string.IsNullOrEmpty(controlExample.XamlCode))",
                "var executedArgs = (ExecutedRoutedEventArgs)e;",
                "switch (executedArgs.Parameter.ToString())",
                "case \"Copy_XamlCode\":",
                "Clipboard.SetText(controlExample.XamlCode);",
                "RaiseCopyNotification(executedArgs);",
                "case \"Copy_CSharpCode\":",
                "Clipboard.SetText(controlExample.CSharpCode);",
                "default:",
                "throw new InvalidOperationException();");
            Assert.IsFalse(
                controlExampleSource.Contains("if (executedArgs.Parameter == null)", StringComparison.Ordinal),
                "ControlExample copy command should follow the official parameter switch path instead of a local null-parameter no-op.");

            var pageHeaderSource = ReadRepoFile(
                "ModernWpf.Gallery",
                "Controls",
                "PageHeader.cs");
            var normalizedPageHeaderSource = pageHeaderSource.Replace("\r\n", "\n").Replace('\r', '\n');
            StringAssert.Contains(
                normalizedPageHeaderSource,
                "public static readonly DependencyProperty TitleProperty = DependencyProperty.Register(\n" +
                "            nameof(Title),\n" +
                "            typeof(string),\n" +
                "            typeof(PageHeader),\n" +
                "            new PropertyMetadata(null)\n" +
                "        );");
            AssertContainsInOrder(
                pageHeaderSource,
                "public static readonly DependencyProperty TitleProperty = DependencyProperty.Register(",
                "public static readonly DependencyProperty DescriptionProperty = DependencyProperty.Register(",
                "public static readonly DependencyProperty ShowDescriptionProperty = DependencyProperty.Register(",
                "public string Title",
                "get => (string)GetValue(TitleProperty);",
                "set => SetValue(TitleProperty, value);",
                "public string Description",
                "get => (string)GetValue(DescriptionProperty);",
                "set => SetValue(DescriptionProperty, value);",
                "public bool ShowDescription",
                "get => (bool)GetValue(ShowDescriptionProperty);",
                "set => SetValue(ShowDescriptionProperty, value);");
        }

        [TestMethod]
        public void SharedPageHeaderKeepsOfficialTemplateSourceShape()
        {
            var xaml = ReadRepoFile(
                "ModernWpf.Gallery",
                "Controls",
                "PageHeader.xaml");
            var normalizedXaml = xaml.Replace("\r\n", "\n").Replace('\r', '\n');

            StringAssert.Contains(
                xaml,
                "<controls:NullToVisibilityConverter x:Key=\"NullToVisibilityConverter\"/>");
            StringAssert.Contains(
                xaml,
                "<Setter Property=\"Focusable\" Value=\"False\"/>");
            StringAssert.Contains(
                normalizedXaml,
                "<StackPanel\n                        VerticalAlignment=\"Center\">");
            AssertContainsInOrder(
                xaml,
                "<Label",
                "x:Name=\"TitleTextBlock\"",
                "AutomationProperties.Name=\"{Binding Title, StringFormat='{}{0} Page', RelativeSource={RelativeSource Mode=TemplatedParent}}\"",
                "pages:GalleryAutomation.HeadingLevel=\"Level1\"",
                "KeyboardNavigation.IsTabStop=\"True\"",
                "KeyboardNavigation.TabIndex=\"0\"",
                "Focusable=\"True\"",
                "<TextBlock Style=\"{StaticResource TitleTextBlockStyle}\"",
                "Text=\"{TemplateBinding Title}\" />");
            AssertContainsInOrder(
                xaml,
                "<Label",
                "pages:GalleryAutomation.HeadingLevel=\"Level2\"",
                "KeyboardNavigation.IsTabStop=\"True\"",
                "KeyboardNavigation.TabIndex=\"1\"",
                "Visibility=\"{TemplateBinding Description, Converter={StaticResource NullToVisibilityConverter}}\"",
                "Focusable=\"True\"",
                "Style=\"{StaticResource BodyTextBlockStyle}\"/>");
            AssertContainsInOrder(
                xaml,
                "<Trigger Property=\"ShowDescription\" Value=\"False\">",
                "<Setter TargetName=\"DescriptionTextBlock\"",
                "Property=\"Visibility\"",
                "Value=\"Hidden\"/>");
        }

        [TestMethod]
        public void SharedHeaderTileKeepsOfficialDeclarationSourceShape()
        {
            var xaml = ReadRepoFile(
                "ModernWpf.Gallery",
                "Controls",
                "HeaderTile.xaml");
            var normalizedXaml = xaml.Replace("\r\n", "\n").Replace('\r', '\n');

            StringAssert.Contains(
                xaml,
                "<UserControl x:Class=\"ModernWpf.Gallery.Controls.HeaderTile\"");
            StringAssert.Contains(
                xaml,
                "Width=\"198\" Height=\"220\"");
            Assert.IsFalse(
                xaml.Contains("d:DesignHeight", StringComparison.Ordinal),
                "The shared HeaderTile should keep the official root size declaration without local design-time dimensions.");
            Assert.IsFalse(
                xaml.Contains("d:DesignWidth", StringComparison.Ordinal),
                "The shared HeaderTile should keep the official root size declaration without local design-time dimensions.");
            AssertContainsInOrder(
                xaml,
                "<Button",
                "x:Name=\"RootButton\"",
                "Margin=\"6\"",
                "BorderThickness=\"1\"",
                "HorizontalAlignment=\"Stretch\"",
                "VerticalAlignment=\"Stretch\"",
                "HorizontalContentAlignment=\"Stretch\"",
                "VerticalContentAlignment=\"Stretch\"",
                "AutomationProperties.Name=\"{Binding Title, RelativeSource={RelativeSource AncestorType=local:HeaderTile}}\"",
                "Click=\"RootButton_Click\"",
                "Padding=\"24\">");
            StringAssert.Contains(
                xaml,
                "<SolidColorBrush x:Key=\"ButtonBackground\" Color=\"{Binding Color, Source={StaticResource AcrylicBackgroundFillColorDefaultBrush}}\" Opacity=\"0.8\" />");
            StringAssert.Contains(
                xaml,
                "<SolidColorBrush x:Key=\"ButtonBackgroundPointerOver\" Color=\"{Binding Color, Source={StaticResource AcrylicBackgroundFillColorDefaultBrush}}\" Opacity=\"0.9\" />");
            StringAssert.Contains(
                xaml,
                "<SolidColorBrush x:Key=\"ButtonBackgroundPressed\" Color=\"{Binding Color, Source={StaticResource AcrylicBackgroundFillColorDefaultBrush}}\" Opacity=\"1.0\" />");
            StringAssert.Contains(
                normalizedXaml,
                "<Grid x:Name=\"ContentGrid\"\n            HorizontalAlignment=\"Stretch\"");
            AssertContainsInOrder(
                xaml,
                "<TextBlock",
                "Grid.RowSpan=\"3\"",
                "Margin=\"-12\"",
                "HorizontalAlignment=\"Right\"",
                "VerticalAlignment=\"Bottom\"",
                "FontSize=\"16\"",
                "FontFamily=\"{StaticResource SymbolThemeFontFamily}\"",
                "Foreground=\"{DynamicResource TextFillColorPrimaryBrush}\"",
                "Text=\"&#xE8A7;\" />");
            AssertContainsInOrder(
                xaml,
                "<StackPanel",
                "Grid.Row=\"1\"",
                "Orientation=\"Vertical\"",
                "Margin=\"0 16 0 0\">",
                "<TextBlock",
                "x:Name=\"TitleText\"",
                "FontSize=\"18\"",
                "Foreground=\"{DynamicResource TextFillColorPrimaryBrush}\"",
                "Style=\"{StaticResource BodyTextBlockStyle}\"",
                "Text=\"{Binding Title, RelativeSource={RelativeSource AncestorType=local:HeaderTile}}\"",
                "Margin=\"0 0 0 8\"/>",
                "<TextBlock",
                "Foreground=\"{DynamicResource TextFillColorPrimaryBrush}\"",
                "TextTrimming=\"CharacterEllipsis\"",
                "Style=\"{StaticResource CaptionTextBlockStyle}\"",
                "Text=\"{Binding Description, RelativeSource={RelativeSource AncestorType=local:HeaderTile}}\" />");

            var code = ReadRepoFile(
                "ModernWpf.Gallery",
                "Controls",
                "HeaderTile.xaml.cs");
            AssertContainsInOrder(
                code,
                "ApplyButtonResources(SystemParameters.HighContrast);",
                "if (!highContrast)",
                "RootButton.Resources[\"ButtonBackground\"] = new SolidColorBrush { Color = color, Opacity = 0.8 };",
                "RootButton.Resources[\"ButtonBackgroundPointerOver\"] = new SolidColorBrush { Color = color, Opacity = 0.9 };",
                "RootButton.Resources[\"ButtonBackgroundPressed\"] = new SolidColorBrush { Color = color, Opacity = 1.0 };",
                "RootButton.Resources[\"ButtonBackground\"] = SystemColors.ControlBrush;",
                "RootButton.Resources[\"ButtonBackgroundPointerOver\"] = SystemColors.ControlBrush;",
                "RootButton.Resources[\"ButtonBackgroundPressed\"] = SystemColors.ControlBrush;");
        }

        [TestMethod]
        public void SharedHeaderTileCodeBehindKeepsOfficialMemberAndUserPreferenceHandlerShape()
        {
            var source = ReadRepoFile(
                "ModernWpf.Gallery",
                "Controls",
                "HeaderTile.xaml.cs")
                .Replace("\r\n", "\n")
                .Replace('\r', '\n');

            Assert.IsFalse(
                source.Contains("OnUserPreferenceChanged", StringComparison.Ordinal),
                "HeaderTile should keep the official SystemEvents_UserPreferenceChanged handler name.");
            AssertContainsInOrder(
                source,
                "/// Interaction logic for HeaderTile.xaml",
                "InitializeComponent();",
                "UpdateButtonResources();",
                "SystemEvents.UserPreferenceChanged += SystemEvents_UserPreferenceChanged;",
                "ThemeManager.Current.ActualApplicationThemeChanged += OnActualApplicationThemeChanged;",
                "Unloaded += OnUnloaded;",
                "private void SystemEvents_UserPreferenceChanged(object sender, UserPreferenceChangedEventArgs e)",
                "Dispatcher.Invoke(() =>\n            {\n                UpdateButtonResources();\n            });",
                "private void OnActualApplicationThemeChanged(ThemeManager sender, object args)",
                "Dispatcher.Invoke(() =>\n            {\n                UpdateButtonResources();\n            });",
                "private void OnUnloaded(object sender, RoutedEventArgs e)",
                "SystemEvents.UserPreferenceChanged -= SystemEvents_UserPreferenceChanged;",
                "ThemeManager.Current.ActualApplicationThemeChanged -= OnActualApplicationThemeChanged;",
                "private void UpdateButtonResources()",
                "ApplyButtonResources(SystemParameters.HighContrast);",
                "public string Title",
                "public static readonly DependencyProperty TitleProperty",
                "DependencyProperty.Register(\"Title\", typeof(string), typeof(HeaderTile), new PropertyMetadata(\"\"));",
                "public string Description",
                "public static readonly DependencyProperty DescriptionProperty",
                "DependencyProperty.Register(\"ColorExplanation\", typeof(string), typeof(HeaderTile), new PropertyMetadata(\"\"));",
                "public string Link",
                "public static readonly DependencyProperty LinkProperty",
                "DependencyProperty.Register(\"Link\", typeof(string), typeof(HeaderTile), new PropertyMetadata(null));",
                "public object Source",
                "get { return (object)GetValue(SourceProperty); }",
                "public static readonly DependencyProperty SourceProperty",
                "DependencyProperty.Register(\"Source\", typeof(object), typeof(HeaderTile), new PropertyMetadata(null));",
                "private void RootButton_Click(object sender, RoutedEventArgs e)",
                "Process.Start(new ProcessStartInfo(Link) { UseShellExecute = true });",
                "protected override AutomationPeer OnCreateAutomationPeer()");
        }

        [TestMethod]
        public void SharedTileGalleryKeepsReferenceLayoutAndModernWpfDestinations()
        {
            var xaml = ReadRepoFile(
                "ModernWpf.Gallery",
                "Controls",
                "TileGallery.xaml");

            AssertContainsInOrder(
                xaml,
                "<UserControl x:Class=\"ModernWpf.Gallery.Controls.TileGallery\"",
                "xmlns:mc=\"http://schemas.openxmlformats.org/markup-compatibility/2006\"",
                "xmlns:d=\"http://schemas.microsoft.com/expression/blend/2008\"",
                "xmlns:local=\"clr-namespace:ModernWpf.Gallery.Controls\"",
                "xmlns:gallery=\"clr-namespace:ModernWpf.Gallery\"",
                "mc:Ignorable=\"d\"",
                "d:DesignHeight=\"450\" d:DesignWidth=\"800\">",
                "<Style x:Key=\"TileGalleryScrollButtonStyle\" BasedOn=\"{StaticResource DefaultButtonStyle}\" TargetType=\"Button\">");
            AssertContainsInOrder(
                xaml,
                "<ScrollViewer x:Name=\"RootScrollViewer\"",
                "VerticalScrollBarVisibility=\"Disabled\"",
                "HorizontalScrollBarVisibility=\"Hidden\"",
                "SizeChanged=\"RootScrollViewer_SizeChanged\">",
                "<StackPanel x:Name=\"TilesPanel\"",
                "Orientation=\"Horizontal\">");
            AssertContainsInOrder(
                xaml,
                "<local:HeaderTile",
                "Title=\"Getting started\"",
                "Description=\"Install ModernWpfUI and add the recommended resources.\"",
                "Link=\"{x:Static gallery:GalleryBranding.QuickStartUrl}\"",
                "Margin=\"24 0 6 0\">",
                "<Image Source=\"{StaticResource ModernWpfLogoImage}\" />");
            AssertContainsInOrder(
                xaml,
                "Title=\"Documentation\"",
                "Description=\"Read setup, migration, and compatibility guidance.\"",
                "Link=\"{x:Static gallery:GalleryBranding.DocumentationUrl}\">",
                "<Viewbox Width=\"48\" Height=\"48\">",
                "Text=\"&#xE8F1;\" />");
            AssertContainsInOrder(
                xaml,
                "Title=\"GitHub repository\"",
                "Description=\"Explore source code and project development.\"",
                "Link=\"{x:Static gallery:GalleryBranding.RepositoryUrl}\">",
                "<Viewbox Height=\"52\" Margin=\"-20 0 0 0\">",
                "<Path Data=\"{StaticResource GitHubIconGeometry}\" Fill=\"{DynamicResource TextFillColorPrimaryBrush}\"/>",
                "Title=\"NuGet package\"",
                "Description=\"Install ModernWpfUI or review available versions.\"",
                "Link=\"{x:Static gallery:GalleryBranding.NuGetPackageUrl}\">",
                "<Viewbox Width=\"48\" Height=\"48\">",
                "Text=\"&#xE7B8;\" />",
                "Title=\"Report an issue\"",
                "Description=\"Report a reproducible problem in a ModernWPF preview.\"",
                "Link=\"{x:Static gallery:GalleryBranding.NewIssueUrl}\">",
                "<Viewbox Width=\"48\" Height=\"48\">",
                "Text=\"&#xEBE8;\" />");
            AssertContainsInOrder(
                xaml,
                "<Button x:Name=\"ScrollBackButton\"",
                "Style=\"{DynamicResource TileGalleryScrollButtonStyle}\"",
                "Margin=\"8,-16,0,0\"",
                "AutomationProperties.Name=\"Scroll left\"",
                "Click=\"ScrollBackButton_Click\"",
                "ToolTip=\"Scroll left\">",
                "<TextBlock FontFamily=\"{StaticResource SymbolThemeFontFamily}\" FontSize=\"8\" Text=\"&#xEDD9;\" />",
                "<Button x:Name=\"ScrollForwardButton\"",
                "Style=\"{DynamicResource TileGalleryScrollButtonStyle}\"",
                "Margin=\"0,-16,8,0\"",
                "HorizontalAlignment=\"Right\"",
                "AutomationProperties.Name=\"Scroll right\"",
                "Click=\"ScrollForwardButton_Click\"",
                "ToolTip=\"Scroll right\">",
                "<TextBlock FontFamily=\"{StaticResource SymbolThemeFontFamily}\" FontSize=\"8\" Text=\"&#xEDDA;\" />");
        }

        [TestMethod]
        public void SharedTileGalleryCodeBehindKeepsOfficialScrollHandlerSourceShape()
        {
            var source = ReadRepoFile(
                "ModernWpf.Gallery",
                "Controls",
                "TileGallery.xaml.cs");

            AssertContainsInOrder(
                source,
                "/// Interaction logic for TileGallery.xaml",
                "public partial class TileGallery : UserControl",
                "public TileGallery()",
                "InitializeComponent();",
                "private void ScrollBackButton_Click(object sender, RoutedEventArgs e)",
                "double newOffSet = RootScrollViewer.HorizontalOffset - 210;",
                "RootScrollViewer.ScrollToHorizontalOffset(newOffSet);",
                "UpdateScrollButtonsVisibility(newOffSet);",
                "private void ScrollForwardButton_Click(object sender, RoutedEventArgs e)",
                "double newOffSet = RootScrollViewer.HorizontalOffset + 210;",
                "RootScrollViewer.ScrollToHorizontalOffset(newOffSet);",
                "UpdateScrollButtonsVisibility(newOffSet);",
                "private void UpdateScrollButtonsVisibility()",
                "double offset = RootScrollViewer.HorizontalOffset;",
                "UpdateScrollButtonsVisibility(offset);",
                "private void UpdateScrollButtonsVisibility(double newOffset)",
                "ScrollBackButton.Visibility = Visibility.Visible;",
                "ScrollForwardButton.Visibility = Visibility.Visible;",
                "if (RootScrollViewer.ActualWidth < TilesPanel.ActualWidth)",
                "if (newOffset == 0)",
                "ScrollBackButton.Visibility = Visibility.Collapsed;",
                "else if (newOffset >= RootScrollViewer.ScrollableWidth)",
                "ScrollForwardButton.Visibility = Visibility.Collapsed;",
                "private void RootScrollViewer_SizeChanged(object sender, SizeChangedEventArgs e)",
                "UpdateScrollButtonsVisibility();");
        }

        [TestMethod]
        public void SharedColorPageExampleKeepsOfficialTemplateSourceShape()
        {
            var xaml = ReadRepoFile(
                "ModernWpf.Gallery",
                "Controls",
                "ColorPageExample.xaml");
            var normalizedXaml = xaml.Replace("\r\n", "\n").Replace('\r', '\n');

            StringAssert.Contains(
                xaml,
                "<Setter Property=\"Background\" Value=\"{DynamicResource SolidBackgroundFillColorBaseBrush}\"/>");
            StringAssert.Contains(
                xaml,
                "<Border BorderThickness=\"1\" Margin=\"0,36,0,0\" Padding=\"12\" CornerRadius=\"8\" BorderBrush=\"{DynamicResource CardStrokeColorDefaultBrush}\" Background=\"{TemplateBinding Background}\">");
            StringAssert.Contains(
                normalizedXaml,
                "</Grid.RowDefinitions>\n\n                            <TextBlock Margin=\"0,0,0,12\" Style=\"{DynamicResource SubtitleTextBlockStyle}\" Text=\"{TemplateBinding Title}\" />");
            StringAssert.Contains(
                xaml,
                "<TextBlock Style=\"{DynamicResource CaptionTextBlockStyle}\" Text=\"{TemplateBinding Description}\" Grid.Row=\"1\"/>");

            var code = ReadRepoFile(
                "ModernWpf.Gallery",
                "Controls",
                "ColorPageExample.cs");
            AssertContainsInOrder(
                code,
                "/// Interaction logic for ColorPageExample.xaml",
                "[ContentProperty(nameof(ExampleContent))]",
                "public partial class ColorPageExample : UserControl",
                "public string Description",
                "public static readonly DependencyProperty DescriptionProperty",
                "DependencyProperty.Register(\"Description\", typeof(string), typeof(ColorPageExample), new PropertyMetadata(\"\"));",
                "public string Title",
                "public static readonly DependencyProperty TitleProperty",
                "DependencyProperty.Register(\"Title\", typeof(string), typeof(ColorPageExample), new PropertyMetadata(\"\"));",
                "public UIElement ExampleContent",
                "public static readonly DependencyProperty ExampleContentProperty",
                "DependencyProperty.Register(\"ExampleContent\", typeof(UIElement), typeof(ColorPageExample), new PropertyMetadata(null));");
        }

        [TestMethod]
        public void SharedColorTileTemplateKeepsOfficialDeclarationSourceShape()
        {
            var xaml = ReadRepoFile(
                "ModernWpf.Gallery",
                "Controls",
                "ColorTile.xaml");

            AssertContainsInOrder(
                xaml,
                "<Border Style=\"{DynamicResource ColorTilesPanelStyle}\" BorderThickness=\"0\" CornerRadius=\"{TemplateBinding TileRadius}\" Background=\"{TemplateBinding Background}\">",
                "Name=\"ColorNameTextBlock\"",
                "Foreground=\"{TemplateBinding Foreground}\"",
                "Style=\"{DynamicResource BodyStrongTextBlockStyle}\"",
                "Text=\"{TemplateBinding ColorName}\"",
                "x:Name=\"CopyBrushNameButton\"",
                "AutomationProperties.Name=\"{Binding ColorBrushName, StringFormat='{}Copy brush name {0} to clipboard', RelativeSource={RelativeSource Mode=TemplatedParent}}\"",
                "Grid.RowSpan=\"4\"",
                "Grid.Column=\"1\"",
                "Grid.ColumnSpan=\"2\"",
                "Padding=\"4\"",
                "Margin=\"0,12,12,0\"",
                "Background=\"Transparent\"",
                "BorderBrush=\"Transparent\"",
                "Foreground=\"{TemplateBinding Foreground}\"",
                "Command=\"ApplicationCommands.Copy\"",
                "CommandTarget=\"{Binding RelativeSource={RelativeSource AncestorType={x:Type controls:ColorTile}}}\"",
                "FocusManager.IsFocusScope=\"True\"",
                "ToolTipService.ToolTip=\"Copy brush name\"",
                "<TextBlock x:Name=\"CopyGlyphTextBlock\" FontFamily=\"{StaticResource SymbolThemeFontFamily}\" FontSize=\"16\" Text=\"&#xE8C8;\" />",
                "<TextBlock x:Name=\"SuccessGlyphTextBlock\" FontFamily=\"{StaticResource SymbolThemeFontFamily}\" FontSize=\"16\" Text=\"&#xE73E;\" Opacity=\"0\" />",
                "<DoubleAnimation Duration=\"0:0:0.333\" Storyboard.TargetName=\"CopyGlyphTextBlock\" Storyboard.TargetProperty=\"Opacity\" To=\"0\" />",
                "<DoubleAnimation Duration=\"0:0:0.666\" BeginTime=\"0:0:0.333\" Storyboard.TargetName=\"SuccessGlyphTextBlock\" Storyboard.TargetProperty=\"Opacity\" To=\"1\" />",
                "<DoubleAnimation Storyboard.TargetName=\"SuccessGlyphTextBlock\" BeginTime=\"0:0:2\" Duration=\"0:0:0.01\" Storyboard.TargetProperty=\"Opacity\" To=\"0\" />",
                "<DoubleAnimation Storyboard.TargetName=\"CopyGlyphTextBlock\" BeginTime=\"0:0:2.1\" Duration=\"0:0:0.01\" Storyboard.TargetProperty=\"Opacity\" To=\"1\" />",
                "Name=\"ColorExplanationTextBlock\"",
                "Text=\"{TemplateBinding ColorExplanation}\"",
                "Name=\"ColorBrushNameTextBlock\"",
                "Text=\"{TemplateBinding ColorBrushName}\"",
                "Visibility=\"{TemplateBinding ShowWarning, Converter={StaticResource BooleanToVisibilityConverter}}\"",
                "Visibility=\"{Binding ShowSeparator, Converter={StaticResource BooleanToVisibilityConverter}, RelativeSource={RelativeSource TemplatedParent}}\"",
                "<Setter Property=\"Foreground\" Value=\"{DynamicResource SystemColorWindowTextColorBrush}\" TargetName=\"ColorExplanationTextBlock\" />",
                "<Setter Property=\"Background\" Value=\"{DynamicResource SystemColorWindowColorBrush}\" TargetName=\"ColorExplanationTextBlock\" />",
                "<Setter Property=\"Foreground\" Value=\"{DynamicResource SystemColorWindowTextColorBrush}\" TargetName=\"ColorBrushNameTextBlock\" />",
                "<Setter Property=\"Background\" Value=\"{DynamicResource SystemColorWindowColorBrush}\" TargetName=\"ColorBrushNameTextBlock\" />",
                "<Setter Property=\"Foreground\" Value=\"{DynamicResource SystemColorWindowTextColorBrush}\" TargetName=\"ColorNameTextBlock\" />",
                "<Setter Property=\"Background\" Value=\"{DynamicResource SystemColorWindowColorBrush}\" TargetName=\"ColorNameTextBlock\" />",
                "<Setter Property=\"Foreground\" Value=\"{DynamicResource SystemColorWindowTextColorBrush}\" TargetName=\"CopyBrushNameButton\" />",
                "<Setter Property=\"Background\" Value=\"{DynamicResource SystemColorWindowColorBrush}\" TargetName=\"CopyBrushNameButton\" />");

            Assert.IsFalse(
                xaml.Contains("x:Name=\"ColorNameTextBlock\"", StringComparison.Ordinal),
                "The copied ColorTile template should keep the official Name= source shape for the color name TextBlock.");
            Assert.IsFalse(
                xaml.Contains("x:Name=\"ColorExplanationTextBlock\"", StringComparison.Ordinal),
                "The copied ColorTile template should keep the official Name= source shape for the color explanation TextBlock.");
            Assert.IsFalse(
                xaml.Contains("x:Name=\"ColorBrushNameTextBlock\"", StringComparison.Ordinal),
                "The copied ColorTile template should keep the official Name= source shape for the color brush name TextBlock.");
        }

        [TestMethod]
        public void SharedColorTileCodeBehindKeepsOfficialMemberAndCopyHandlerSourceShape()
        {
            var source = ReadRepoFile(
                "ModernWpf.Gallery",
                "Controls",
                "ColorTile.cs");

            AssertContainsInOrder(
                source,
                "/// Interaction logic for ColorTile.xaml",
                "public partial class ColorTile : UserControl",
                "static ColorTile()",
                "CommandManager.RegisterClassCommandBinding(typeof(ColorTile), new CommandBinding(ApplicationCommands.Copy, Copy_ColorBrushName));",
                "public CornerRadius TileRadius",
                "public static readonly DependencyProperty TileRadiusProperty",
                "DependencyProperty.Register(\"TileRadius\", typeof(CornerRadius), typeof(ColorTile), new PropertyMetadata(new CornerRadius(0)));",
                "public string ColorName",
                "public static readonly DependencyProperty ColorNameProperty",
                "DependencyProperty.Register(\"ColorName\", typeof(string), typeof(ColorTile), new PropertyMetadata(\"\"));",
                "public string ColorExplanation",
                "public static readonly DependencyProperty ColorExplanationProperty",
                "DependencyProperty.Register(\"ColorExplanation\", typeof(string), typeof(ColorTile), new PropertyMetadata(\"\"));",
                "public string ColorBrushName",
                "public static readonly DependencyProperty ColorBrushNameProperty",
                "DependencyProperty.Register(\"ColorBrushName\", typeof(string), typeof(ColorTile), new PropertyMetadata(\"\"));",
                "public string ColorValue",
                "public static readonly DependencyProperty ColorValueProperty",
                "DependencyProperty.Register(\"ColorValue\", typeof(string), typeof(ColorTile), new PropertyMetadata(\"\"));",
                "public bool ShowSeparator",
                "// Using a DependencyProperty as the backing store for ShowSeparator.  This enables animation, styling, binding, etc...",
                "public static readonly DependencyProperty ShowSeparatorProperty",
                "DependencyProperty.Register(\"ShowSeparator\", typeof(bool), typeof(ColorTile), new PropertyMetadata(true));",
                "public bool ShowWarning",
                "// Using a DependencyProperty as the backing store for ShowSeparator.  This enables animation, styling, binding, etc...",
                "public static readonly DependencyProperty ShowWarningProperty",
                "DependencyProperty.Register(\"ShowWarning\", typeof(bool), typeof(ColorTile), new PropertyMetadata(false));",
                "private static void Copy_ColorBrushName(object sender, RoutedEventArgs e)",
                "if (sender is ColorTile colorTile)",
                "if (!string.IsNullOrEmpty(colorTile.ColorBrushName))",
                "Clipboard.SetText(colorTile.ColorBrushName);",
                "RaiseNotification(colorTile);");
        }

        [TestMethod]
        public void DashboardPageKeepsOfficialDashboardCardListDeclarationSourceShape()
        {
            var xaml = ReadRepoFile(
                "ModernWpf.Gallery",
                "Pages",
                "DashboardPage.xaml");
            var normalizedXaml = xaml.Replace("\r\n", "\n").Replace('\r', '\n');

            AssertContainsInOrder(
                xaml,
                "<Page",
                "x:Class=\"ModernWpf.Gallery.Pages.DashboardPage\"",
                "xmlns:d=\"http://schemas.microsoft.com/expression/blend/2008\"",
                "xmlns:local=\"clr-namespace:ModernWpf.Gallery.Pages\"",
                "xmlns:mc=\"http://schemas.openxmlformats.org/markup-compatibility/2006\"",
                "xmlns:controls=\"clr-namespace:ModernWpf.Gallery.Controls\"",
                "xmlns:pages=\"clr-namespace:ModernWpf.Gallery.Pages\"",
                "xmlns:ui=\"http://schemas.modernwpf.com/2019\"",
                "Title=\"DashboardPage\"",
                "d:DesignHeight=\"450\"",
                "d:DesignWidth=\"800\"",
                "Foreground=\"{DynamicResource TextFillColorPrimaryBrush}\"",
                "mc:Ignorable=\"d\"",
                "Margin=\"-24,-16,-24,12\">");
            Assert.IsFalse(
                xaml.Contains("x:Name=\"ContentRootGrid\"", StringComparison.Ordinal),
                "The copied Home page should use the official Dashboard ScrollViewer root shape instead of a local-only root name.");
            AssertContainsInOrder(
                xaml,
                "<Style x:Key=\"DashboardPageRootStyle\" TargetType=\"Grid\">",
                "<MultiDataTrigger>",
                "<Condition Binding=\"{Binding ActualApplicationTheme, Source={x:Static ui:ThemeManager.Current}}\" Value=\"{x:Static ui:ApplicationTheme.Dark}\" />",
                "<Condition Binding=\"{Binding Path=(SystemParameters.HighContrast)}\" Value=\"False\" />",
                "<Setter Property=\"Background\" Value=\"#272727\" />",
                "<ScrollViewer >",
                "<Grid Style=\"{StaticResource DashboardPageRootStyle}\">",
                "<Grid.RowDefinitions>",
                "<RowDefinition Height=\"Auto\" />",
                "<RowDefinition Height=\"Auto\" />",
                "<RowDefinition Height=\"*\" />");
            StringAssert.Contains(
                normalizedXaml,
                "<ScrollViewer >\n\n        <Grid Style=\"{StaticResource DashboardPageRootStyle}\">");
            StringAssert.Contains(
                normalizedXaml,
                "<Border CornerRadius=\"8,0,0,0\"\n                    Grid.RowSpan=\"2\">\n                <Border.Background>\n                    <LinearGradientBrush StartPoint=\"0,0\" EndPoint=\"1,1\">");
            AssertContainsInOrder(
                xaml,
                "<Border CornerRadius=\"8,0,0,0\"",
                "Grid.RowSpan=\"2\"",
                "<StackPanel Margin=\"36,48,0,0\" VerticalAlignment=\"Top\" TextElement.Foreground=\"Black\">",
                "<TextBlock Style=\"{StaticResource SubtitleTextBlockStyle}\" Text=\"{StaticResource GalleryVersionDisplay}\" Margin=\"0,0,0,2\" pages:GalleryAutomation.HeadingLevel=\"Level1\" />",
                "<TextBlock Style=\"{StaticResource TitleLargeTextBlockStyle}\" Text=\"{StaticResource GalleryDisplayName}\" Margin=\"0,0,0,8\" pages:GalleryAutomation.HeadingLevel=\"Level1\" />",
                "<Border Background=\"Transparent\" CornerRadius=\"8,8,8,8\" MaxWidth=\"300\" HorizontalAlignment=\"Left\">",
                "<TextBlock",
                "MaxWidth=\"300\"",
                "Margin=\"0,0,0,0\"",
                "Style=\"{StaticResource BodyStrongTextBlockStyle}\"",
                "Text=\"Explore ModernWPF controls, themes, guidance, and samples for WPF applications\"",
                "TextAlignment=\"Left\"",
                "HorizontalAlignment=\"Left\"",
                "Padding=\"0,8,12,8\"/>");
            Assert.AreEqual(
                1,
                xaml.Split(new[] { "Foreground=\"Black\"" }, StringSplitOptions.None).Length - 1,
                "Home hero text should inherit black foreground from the source-shaped StackPanel.");
            AssertContainsInOrder(
                xaml,
                "<controls:TileGallery Grid.Row=\"1\" HorizontalAlignment=\"Stretch\" Margin=\"0\"/>",
                "<StackPanel Grid.Row=\"2\" Margin=\"32,24,0,0\" Orientation=\"Vertical\">");
            AssertContainsInOrder(
                xaml,
                "<TextBlock",
                "Style=\"{StaticResource BodyStrongTextBlockStyle}\"",
                "Text=\"Overview\"",
                "FontSize=\"16\"",
                "pages:GalleryAutomation.HeadingLevel=\"Level2\" />",
                "<ItemsControl",
                "Margin=\"-20,0,0,0\"",
                "AutomationProperties.Name=\"Items in group\"",
                "ItemsSource=\"{Binding ViewModel.NavigationCards}\"",
                "Focusable=\"False\"",
                "ItemsPanel=\"{StaticResource WrapPanelTemplate}\"",
                "ItemTemplate=\"{StaticResource NavigationCardTemplate}\" />");
            AssertContainsInOrder(
                xaml,
                "<TextBlock",
                "Style=\"{StaticResource BodyStrongTextBlockStyle}\"",
                "Text=\"Recently added and updated\"",
                "FontSize=\"16\"",
                "pages:GalleryAutomation.HeadingLevel=\"Level2\" />",
                "<ItemsControl",
                "Margin=\"-18,0,0,0\"",
                "AutomationProperties.Name=\"Recently Added and Updated Samples Section\"",
                "ItemsSource=\"{Binding ViewModel.RecentlyAddedOrUpdatedSamplesInfo}\"",
                "Focusable=\"False\"",
                "ItemsPanel=\"{StaticResource WrapPanelTemplate}\"",
                "ItemTemplate=\"{StaticResource NavigationCardTemplate}\" />");
        }

        [TestMethod]
        public void WhatsNewPageUsesModernWpfSpecificContentShape()
        {
            var xaml = ReadRepoFile(
                "ModernWpf.Gallery",
                "Pages",
                "WhatsNewPage.xaml");

            AssertContainsInOrder(
                xaml,
                "<Page x:Class=\"ModernWpf.Gallery.Pages.WhatsNewPage\"",
                "xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\"",
                "xmlns:x=\"http://schemas.microsoft.com/winfx/2006/xaml\"",
                "xmlns:mc=\"http://schemas.openxmlformats.org/markup-compatibility/2006\"",
                "xmlns:d=\"http://schemas.microsoft.com/expression/blend/2008\"",
                "xmlns:pages=\"clr-namespace:ModernWpf.Gallery.Pages\"",
                "xmlns:controls=\"clr-namespace:ModernWpf.Gallery.Controls\"",
                "mc:Ignorable=\"d\"",
                "Foreground=\"{DynamicResource TextFillColorPrimaryBrush}\"",
                "d:DesignHeight=\"900\"",
                "d:DesignWidth=\"800\"",
                "Title=\"{StaticResource GalleryWhatsNewPageTitle}\">");
            StringAssert.Contains(
                xaml,
                "<Style x:Key=\"UpdateCardStyle\" TargetType=\"Border\">");
            StringAssert.Contains(
                xaml,
                "x:Key=\"SectionHeadingStyle\"");
            AssertContainsInOrder(
                xaml,
                "<Grid x:Name=\"ContentPagePane\" Height=\"Auto\" Style=\"{StaticResource GalleryPageRootStyle}\">",
                "<Grid.RowDefinitions>",
                "<RowDefinition Height=\"Auto\" />",
                "<RowDefinition Height=\"*\" />",
                "<controls:PageHeader",
                "Description=\"{Binding ViewModel.PageDescription}\"",
                "ShowDescription=\"True\"",
                "Title=\"{Binding ViewModel.PageTitle}\" />",
                "<ScrollViewer",
                "Grid.Row=\"1\"",
                "HorizontalScrollBarVisibility=\"Disabled\">");
            AssertContainsInOrder(
                xaml,
                "x:Name=\"MaintenanceStatusCard\"",
                "Text=\"1.x preview\"",
                "Text=\"Active maintenance line\"",
                "The package name remains ModernWpfUI",
                "Text=\"Supported targets\"",
                "x:Name=\"Net462TargetCard\"",
                "Text=\".NET Framework 4.6.2\"",
                "x:Name=\"Net8TargetCard\"",
                "Text=\".NET 8 for Windows\"",
                "x:Name=\"Net10TargetCard\"",
                "Text=\".NET 10 for Windows\"",
                "Text=\"Recommended resource setup\"",
                "x:Name=\"ResourceSetupExample\"",
                "HeaderText=\"Application resources\"",
                "XamlCode=\"{Binding ViewModel.RecommendedResourcesXamlCode}\"",
                "Text=\"Gallery updates\"",
                "x:Name=\"WinUiShellUpdateCard\"",
                "Text=\"ModernWPF shell\"",
                "x:Name=\"CuratedSamplesUpdateCard\"",
                "Text=\"Curated sample set\"",
                "x:Name=\"TargetAwareUpdateCard\"",
                "Text=\"Target-aware behavior\"",
                "Text=\"New and updated samples\"",
                "x:Name=\"NewOrUpdatedSamples\"",
                "ItemsSource=\"{Binding ViewModel.NewOrUpdatedItems}\"");

            Assert.IsFalse(xaml.Contains("New and Enhanced Fluent Styles", StringComparison.Ordinal));
            Assert.IsFalse(xaml.Contains("Grid Shorthand Syntax", StringComparison.Ordinal));
            Assert.IsFalse(xaml.Contains("Extended MessageBox Options", StringComparison.Ordinal));
            Assert.IsFalse(xaml.Contains("Accent colors as SystemColors", StringComparison.Ordinal));
            Assert.IsFalse(xaml.Contains("Hyphen based ligature", StringComparison.Ordinal));
            Assert.IsFalse(xaml.Contains("Click=\"Open_", StringComparison.Ordinal));
        }

        [TestMethod]
        public void WhatsNewPageRoutesCurrentCatalogItems()
        {
            var source = ReadRepoFile(
                "ModernWpf.Gallery",
                "Pages",
                "WhatsNewPage.xaml.cs");

            Assert.IsFalse(
                source.Contains("Process.Start", StringComparison.Ordinal),
                "What's New should not launch copied WPF release-note links.");
            Assert.IsFalse(source.Contains("MessageBox", StringComparison.Ordinal));
            AssertContainsInOrder(
                source,
                "using ModernWpf.Gallery.Models;",
                "private void OnNavigateCard(object parameter)",
                "if (parameter is GalleryItem item)",
                "ItemRequested?.Invoke(item.UniqueId);",
                "else if (parameter is string uniqueId)",
                "ItemRequested?.Invoke(uniqueId);");
        }

        [TestMethod]
        public void SettingsPageKeepsOfficialSettingsDeclarationSourceShape()
        {
            var xaml = ReadRepoFile(
                "ModernWpf.Gallery",
                "Pages",
                "SettingsPage.xaml");

            AssertContainsInOrder(
                xaml,
                "<Page",
                "x:Class=\"ModernWpf.Gallery.Pages.SettingsPage\"",
                "xmlns:d=\"http://schemas.microsoft.com/expression/blend/2008\"",
                "xmlns:local=\"clr-namespace:ModernWpf.Gallery.Pages\"",
                "xmlns:controls=\"clr-namespace:ModernWpf.Gallery.Controls\"",
                "xmlns:ui=\"http://schemas.modernwpf.com/2019\"",
                "xmlns:mc=\"http://schemas.openxmlformats.org/markup-compatibility/2006\"",
                "Title=\"SettingsPage\"",
                "d:DesignHeight=\"450\"",
                "d:DesignWidth=\"800\"",
                "Foreground=\"{DynamicResource TextFillColorPrimaryBrush}\"",
                "mc:Ignorable=\"d\">");
            AssertContainsInOrder(
                xaml,
                "<Style x:Key=\"SettingsCardStyle\" TargetType=\"Border\">",
                "<Setter Property=\"Padding\" Value=\"0,16,0,16\" />",
                "<Setter Property=\"BorderThickness\" Value=\"0,0,0,1\" />",
                "<Setter Property=\"BorderBrush\" Value=\"{DynamicResource ExpanderHeaderBorderBrush}\" />");
            Assert.IsFalse(
                xaml.Contains("x:Name=\"ContentRootGrid\"", StringComparison.Ordinal),
                "The copied Settings root should be located structurally instead of by a local-only name.");
            StringAssert.Contains(
                xaml,
                "<Style x:Key=\"SettingsPageRootStyle\" BasedOn=\"{StaticResource GalleryPageRootStyle}\" TargetType=\"Grid\">");
            AssertContainsInOrder(
                xaml,
                "<MultiDataTrigger>",
                "<Condition Binding=\"{Binding ActualApplicationTheme, Source={x:Static ui:ThemeManager.Current}}\" Value=\"{x:Static ui:ApplicationTheme.Dark}\" />",
                "<Condition Binding=\"{Binding Path=(SystemParameters.HighContrast)}\" Value=\"False\" />",
                "<Setter Property=\"Background\" Value=\"#272727\" />");
            StringAssert.Contains(
                xaml,
                "<Grid Style=\"{StaticResource SettingsPageRootStyle}\">");
            AssertContainsInOrder(
                xaml,
                "<controls:PageHeader",
                "Grid.Row=\"0\"",
                "Margin=\"0,0,0,40\"",
                "Title=\"{Binding ViewModel.PageTitle}\"",
                "Description=\"{Binding ViewModel.PageDescription}\"/>");
            StringAssert.Contains(
                xaml,
                "<ScrollViewer Grid.Row=\"1\" Margin=\"0,0,0,24\" Padding=\"0,0,24,0\">");
            StringAssert.Contains(
                xaml,
                "<TextBlock Text=\"Appearance &amp; behavior\" FontWeight=\"SemiBold\" Margin=\"10\" FontSize=\"14\"/>");
            StringAssert.Contains(
                xaml,
                "<Grid Background=\"Transparent\" Margin=\"0,0,0,20\">");
            StringAssert.Contains(
                xaml,
                "<Border Background=\"{DynamicResource ExpanderHeaderBackground}\" BorderBrush=\"{DynamicResource ExpanderHeaderBorderBrush}\" BorderThickness=\"{StaticResource ExpanderBorderThemeThickness}\" Padding=\"{StaticResource ExpanderPadding}\" CornerRadius=\"{DynamicResource ControlCornerRadius}\">");
            StringAssert.Contains(
                xaml,
                "<TextBlock x:Name=\"AppIcon\" AutomationProperties.Name=\"App Icon\" Grid.Column=\"0\" Width=\"20\" Height=\"20\"  Margin=\"10,5,10,5\" VerticalAlignment=\"Center\" FontFamily=\"{StaticResource SymbolThemeFontFamily}\" Text=\"&#xE790;\" FontSize=\"20\"/>");
            StringAssert.Contains(
                xaml,
                "<TextBlock Text=\"App theme\" FontSize=\"14\"/>");
            StringAssert.Contains(
                xaml,
                "<TextBlock Opacity=\"0.7\" FontSize=\"12\" Style=\"{StaticResource CaptionTextBlockStyle}\">Select which app theme to display</TextBlock>");
            StringAssert.Contains(
                xaml,
                "<ComboBox x:Name=\"Change_ThemeMode\" MinWidth=\"200\" HorizontalAlignment=\"Right\" SelectedIndex=\"2\" Grid.Column=\"2\" AutomationProperties.Name=\"Change ThemeMode\" SelectionChanged=\"ThemeMode_SelectionChanged\" Margin=\"10\">");
            StringAssert.Contains(
                xaml,
                "<TextBlock Text=\"About\" FontWeight=\"SemiBold\" Margin=\"10\" FontSize=\"14\"/>");
            StringAssert.Contains(
                xaml,
                "<TextBlock Opacity=\"0.7\" Style=\"{StaticResource CaptionTextBlockStyle}\" Text=\"{StaticResource GalleryVersionDisplay}\" />");
            StringAssert.Contains(
                xaml,
                "<TextBlock Opacity=\"0.7\" Style=\"{StaticResource CaptionTextBlockStyle}\" Text=\"{StaticResource GalleryCopyrightNotice}\" />");
            StringAssert.Contains(
                xaml,
                "<TextBox Grid.Column=\"2\" Style=\"{StaticResource SelectionTextBox}\" Text=\"{StaticResource GalleryCloneCommand}\" Focusable=\"False\"/>");
            StringAssert.Contains(
                xaml,
                "<Button AutomationProperties.Name=\"Report a preview bug\" Grid.Column=\"2\" Padding=\"8\" FocusManager.IsFocusScope=\"True\" Click=\"Open_Issues\">");
            StringAssert.Contains(
                xaml,
                "<TextBlock FontFamily=\"{StaticResource SymbolThemeFontFamily}\" Text=\"&#xe8a7;\" />");
            StringAssert.Contains(
                xaml,
                "<GroupBox Grid.Row=\"2\" AutomationProperties.Name=\"Components and dependency\" BorderThickness=\"0\">");
            StringAssert.Contains(
                xaml,
                "<Hyperlink Click=\"Open_Repository\"><Run Text=\"{StaticResource GalleryDisplayName}\" /></Hyperlink>");
            StringAssert.Contains(
                xaml,
                "<Hyperlink Click=\"Open_BehaviorsInformation\" AutomationProperties.Name=\"Link to Microsoft XAML Behaviors WPF NuGet package\">Microsoft.Xaml.Behaviors.Wpf</Hyperlink>");
            StringAssert.Contains(
                xaml,
                "<GroupBox Grid.Row=\"3\" AutomationProperties.Name=\"ModernWPF license and project information\" BorderThickness=\"0\">");
        }

        [TestMethod]
        public void AllSamplesPageKeepsOfficialAllSamplesDeclarationSourceShape()
        {
            var xaml = ReadRepoFile(
                "ModernWpf.Gallery",
                "Pages",
                "AllSamplesPage.xaml");

            AssertContainsInOrder(
                xaml,
                "<Page",
                "x:Class=\"ModernWpf.Gallery.Pages.AllSamplesPage\"",
                "xmlns:d=\"http://schemas.microsoft.com/expression/blend/2008\"",
                "xmlns:local=\"clr-namespace:ModernWpf.Gallery.Pages\"",
                "xmlns:mc=\"http://schemas.openxmlformats.org/markup-compatibility/2006\"",
                "xmlns:controls=\"clr-namespace:ModernWpf.Gallery.Controls\"",
                "Title=\"AllSamplesPage\"",
                "Foreground=\"{DynamicResource TextFillColorPrimaryBrush}\"",
                "mc:Ignorable=\"d\">");
            Assert.IsFalse(
                xaml.Contains("d:DesignHeight=", StringComparison.Ordinal),
                "All Controls should match the official AllSamples root without local design-time dimensions.");
            Assert.IsFalse(
                xaml.Contains("d:DesignWidth=", StringComparison.Ordinal),
                "All Controls should match the official AllSamples root without local design-time dimensions.");
            AssertContainsInOrder(
                xaml,
                "<Grid Style=\"{StaticResource GalleryPageRootStyle}\">",
                "<Grid.RowDefinitions>",
                "<RowDefinition Height=\"Auto\" />",
                "<RowDefinition Height=\"*\" />");
            AssertContainsInOrder(
                xaml,
                "<controls:PageHeader",
                "Grid.Row=\"0\"",
                "Margin=\"0,0,0,40\"",
                "Title=\"{Binding ViewModel.PageTitle}\"",
                "Description=\"{Binding ViewModel.PageDescription}\" />");
            AssertContainsInOrder(
                xaml,
                "<ScrollViewer",
                "Grid.Row=\"1\"",
                "Margin=\"0\"",
                "VerticalScrollBarVisibility=\"Auto\">");
            AssertContainsInOrder(
                xaml,
                "<ItemsControl",
                "Grid.Row=\"1\"",
                "Margin=\"-12,0,0,0\"",
                "AutomationProperties.Name=\"Items in group\"",
                "ItemsSource=\"{Binding ViewModel.NavigationCards}\"",
                "Focusable=\"False\"",
                "ItemsPanel=\"{StaticResource WrapPanelTemplate}\"",
                "ItemTemplate=\"{StaticResource NavigationCardTemplate}\" />");
        }

        [TestMethod]
        public void SectionPageKeepsOfficialSectionDeclarationSourceShape()
        {
            var xaml = ReadRepoFile(
                "ModernWpf.Gallery",
                "Pages",
                "SectionPage.xaml");

            AssertContainsInOrder(
                xaml,
                "<Page",
                "x:Class=\"ModernWpf.Gallery.Pages.SectionPage\"",
                "xmlns:d=\"http://schemas.microsoft.com/expression/blend/2008\"",
                "xmlns:local=\"clr-namespace:ModernWpf.Gallery.Pages\"",
                "xmlns:mc=\"http://schemas.openxmlformats.org/markup-compatibility/2006\"",
                "xmlns:controls=\"clr-namespace:ModernWpf.Gallery.Controls\"",
                "Title=\"NavigationPage\"",
                "d:DesignHeight=\"450\"",
                "d:DesignWidth=\"800\"",
                "Foreground=\"{DynamicResource TextFillColorPrimaryBrush}\"",
                "mc:Ignorable=\"d\">");
            AssertContainsInOrder(
                xaml,
                "<Grid Style=\"{StaticResource GalleryPageRootStyle}\">",
                "<Grid.RowDefinitions>",
                "<RowDefinition Height=\"Auto\" />",
                "<RowDefinition Height=\"*\" />");
            AssertContainsInOrder(
                xaml,
                "<controls:PageHeader",
                "Grid.Row=\"0\"",
                "Margin=\"0,0,0,40\"",
                "Title=\"{Binding ViewModel.PageTitle}\"",
                "Description=\"{Binding ViewModel.PageDescription}\" />");
            AssertContainsInOrder(
                xaml,
                "<ItemsControl",
                "Grid.Row=\"1\"",
                "Margin=\"-12,0,0,0\"",
                "AutomationProperties.Name=\"Items in group\"",
                "ItemsSource=\"{Binding ViewModel.NavigationCards}\"",
                "Focusable=\"False\"",
                "ItemsPanel=\"{StaticResource WrapPanelTemplate}\"",
                "ItemTemplate=\"{StaticResource NavigationCardTemplate}\" />");
        }

        [TestMethod]
        public void CopiedItemCodeBehindKeepsOfficialViewModelPropertyBeforeConstructorShape()
        {
            foreach (var page in new[]
            {
                Tuple.Create("BasicInput", "ButtonPage", "ButtonPageViewModel"),
                Tuple.Create("BasicInput", "CheckBoxPage", "CheckBoxPageViewModel"),
                Tuple.Create("BasicInput", "ComboBoxPage", "ComboBoxPageViewModel"),
                Tuple.Create("BasicInput", "RadioButtonPage", "RadioButtonPageViewModel"),
                Tuple.Create("BasicInput", "SliderPage", "SliderPageViewModel"),
                Tuple.Create("Collections", "DataGridPage", "DataGridPageViewModel"),
                Tuple.Create("Collections", "ListBoxPage", "ListBoxPageViewModel"),
                Tuple.Create("Collections", "ListViewPage", "ListViewPageViewModel"),
                Tuple.Create("Collections", "TreeViewPage", "TreeViewPageViewModel"),
                Tuple.Create("DateAndTime", "CalendarPage", "CalendarPageViewModel"),
                Tuple.Create("DateAndTime", "DatePickerPage", "DatePickerPageViewModel"),
                Tuple.Create("DesignGuidance", "ColorsPage", "ColorsPageViewModel"),
                Tuple.Create("DesignGuidance", "GeometryPage", "GeometryPageViewModel"),
                Tuple.Create("DesignGuidance", "SpacingPage", "SpacingPageViewModel"),
                Tuple.Create("Media", "CanvasPage", "CanvasPageViewModel"),
                Tuple.Create("Media", "ImagePage", "ImagePageViewModel"),
                Tuple.Create("Navigation", "MenuPage", "MenuPageViewModel"),
                Tuple.Create("Navigation", "TabControlPage", "TabControlPageViewModel"),
                Tuple.Create("Samples", "UserDashboardPage", "UserDashboardPageViewModel"),
                Tuple.Create("StatusAndInfo", "ProgressBarPage", "ProgressBarPageViewModel"),
                Tuple.Create("StatusAndInfo", "ToolTipPage", "ToolTipPageViewModel"),
                Tuple.Create("System", "ClipboardPage", "ClipboardPageViewModel"),
                Tuple.Create("System", "FileAndFolderDialogsPage", "FileAndFolderDialogsPageViewModel"),
                Tuple.Create("System", "MessageBoxPage", "MessageBoxPageViewModel"),
                Tuple.Create("Text", "LabelPage", "LabelPageViewModel"),
                Tuple.Create("Text", "PasswordBoxPage", "PasswordBoxPageViewModel"),
                Tuple.Create("Text", "RichTextEditPage", "RichTextEditPageViewModel"),
                Tuple.Create("Text", "TextBlockPage", "TextBlockPageViewModel"),
                Tuple.Create("Text", "TextBoxPage", "TextBoxPageViewModel")
            })
            {
                var source = ReadRepoFile(
                    "ModernWpf.Gallery",
                    "Pages",
                    "WpfGallery",
                    page.Item1,
                    page.Item2 + ".xaml.cs");
                var viewModelIndex = source.IndexOf(
                    "public " + page.Item3 + " ViewModel { get; }",
                    StringComparison.Ordinal);
                var constructorIndex = source.IndexOf(
                    "public " + page.Item2 + "(",
                    StringComparison.Ordinal);

                Assert.IsTrue(viewModelIndex >= 0, page.Item2 + " should expose its copied page-specific ViewModel property.");
                Assert.IsTrue(constructorIndex >= 0, page.Item2 + " should keep its copied constructor.");
                Assert.IsTrue(
                    viewModelIndex < constructorIndex,
                    page.Item2 + " should match the official WPF Gallery code-behind member order by declaring ViewModel before the constructor.");
            }
        }

        [TestMethod]
        public void CopiedViewModelBackedCodeBehindKeepsOfficialConstructorInitializationOrder()
        {
            foreach (var page in new[]
            {
                Tuple.Create("BasicInput", "ButtonPage"),
                Tuple.Create("BasicInput", "CheckBoxPage"),
                Tuple.Create("BasicInput", "ComboBoxPage"),
                Tuple.Create("BasicInput", "RadioButtonPage"),
                Tuple.Create("BasicInput", "SliderPage"),
                Tuple.Create("Collections", "DataGridPage"),
                Tuple.Create("Collections", "ListBoxPage"),
                Tuple.Create("Collections", "ListViewPage"),
                Tuple.Create("Collections", "TreeViewPage"),
                Tuple.Create("DateAndTime", "CalendarPage"),
                Tuple.Create("DateAndTime", "DatePickerPage"),
                Tuple.Create("Layout", "BorderPage"),
                Tuple.Create("Layout", "ExpanderPage"),
                Tuple.Create("Layout", "GridPage"),
                Tuple.Create("Layout", "GridSplitterPage"),
                Tuple.Create("Layout", "GroupBoxPage"),
                Tuple.Create("Layout", "ResizeGripPage"),
                Tuple.Create("Layout", "StackPanelPage"),
                Tuple.Create("Media", "CanvasPage"),
                Tuple.Create("Media", "ImagePage"),
                Tuple.Create("Navigation", "FramePage"),
                Tuple.Create("Navigation", "MenuPage"),
                Tuple.Create("Navigation", "NavigationWindowPage"),
                Tuple.Create("Navigation", "TabControlPage"),
                Tuple.Create("StatusAndInfo", "ProgressBarPage"),
                Tuple.Create("StatusAndInfo", "ToolTipPage"),
                Tuple.Create("System", "ClipboardPage"),
                Tuple.Create("System", "FileAndFolderDialogsPage"),
                Tuple.Create("System", "MessageBoxPage"),
                Tuple.Create("Text", "HyperlinkPage"),
                Tuple.Create("Text", "LabelPage"),
                Tuple.Create("Text", "PasswordBoxPage"),
                Tuple.Create("Text", "RichTextEditPage"),
                Tuple.Create("Text", "TextBlockPage"),
                Tuple.Create("Text", "TextBoxPage")
            })
            {
                var source = ReadRepoFile(
                    "ModernWpf.Gallery",
                    "Pages",
                    "WpfGallery",
                    page.Item1,
                    page.Item2 + ".xaml.cs");

                AssertContainsInOrder(
                    source,
                    "public " + page.Item2 + "(",
                    "ViewModel = viewModel;",
                    "DataContext = this;",
                    "InitializeComponent();");
            }

            foreach (var page in new[]
            {
                Tuple.Create("DesignGuidance", "ColorsPage"),
                Tuple.Create("DesignGuidance", "GeometryPage"),
                Tuple.Create("DesignGuidance", "IconsPage"),
                Tuple.Create("DesignGuidance", "SpacingPage"),
                Tuple.Create("DesignGuidance", "TypographyPage"),
                Tuple.Create("Samples", "UserDashboardPage")
            })
            {
                var source = ReadRepoFile(
                    "ModernWpf.Gallery",
                    "Pages",
                    "WpfGallery",
                    page.Item1,
                    page.Item2 + ".xaml.cs");

                AssertContainsInOrder(
                    source,
                    "public " + page.Item2 + "(",
                    "InitializeComponent();",
                    "ViewModel = viewModel;",
                    "DataContext = this;");
            }
        }

        [TestMethod]
        public void SimpleDateMediaAndStatusCodeBehindKeepOfficialConstructorParagraphShape()
        {
            foreach (var page in new[]
            {
                Tuple.Create("DateAndTime", "CalendarPage"),
                Tuple.Create("DateAndTime", "DatePickerPage"),
                Tuple.Create("Media", "CanvasPage"),
                Tuple.Create("Media", "ImagePage"),
                Tuple.Create("StatusAndInfo", "ProgressBarPage"),
                Tuple.Create("StatusAndInfo", "ToolTipPage")
            })
            {
                var source = ReadRepoFile(
                    "ModernWpf.Gallery",
                    "Pages",
                    "WpfGallery",
                    page.Item1,
                    page.Item2 + ".xaml.cs");
                var normalizedSource = source.Replace("\r\n", "\n").Replace('\r', '\n');

                StringAssert.Contains(
                    normalizedSource,
                    "            DataContext = this;\n\n            InitializeComponent();");
            }
        }

        [TestMethod]
        public void BasicInputCodeBehindKeepsOfficialConstructorParagraphShape()
        {
            var buttonSource = ReadRepoFile(
                "ModernWpf.Gallery",
                "Pages",
                "WpfGallery",
                "BasicInput",
                "ButtonPage.xaml.cs").Replace("\r\n", "\n").Replace('\r', '\n');
            StringAssert.Contains(
                buttonSource,
                "            DataContext = this;\n            InitializeComponent();");

            var checkBoxSource = ReadRepoFile(
                "ModernWpf.Gallery",
                "Pages",
                "WpfGallery",
                "BasicInput",
                "CheckBoxPage.xaml.cs").Replace("\r\n", "\n").Replace('\r', '\n');
            StringAssert.Contains(
                checkBoxSource,
                "        public CheckBoxPageViewModel ViewModel { get; }\n        public CheckBoxPage(CheckBoxPageViewModel viewModel)");

            foreach (var page in new[]
            {
                "ComboBoxPage",
                "RadioButtonPage",
                "SliderPage"
            })
            {
                var source = ReadRepoFile(
                    "ModernWpf.Gallery",
                    "Pages",
                    "WpfGallery",
                    "BasicInput",
                    page + ".xaml.cs").Replace("\r\n", "\n").Replace('\r', '\n');

                StringAssert.Contains(
                    source,
                    "            DataContext = this;\n\n            InitializeComponent();");
            }
        }

        [TestMethod]
        public void CollectionsCodeBehindKeepsOfficialConstructorAdjacencyShape()
        {
            foreach (var page in new[]
            {
                Tuple.Create("DataGridPage", "DataGridPageViewModel"),
                Tuple.Create("ListBoxPage", "ListBoxPageViewModel"),
                Tuple.Create("TreeViewPage", "TreeViewPageViewModel")
            })
            {
                var source = ReadRepoFile(
                    "ModernWpf.Gallery",
                    "Pages",
                    "WpfGallery",
                    "Collections",
                    page.Item1 + ".xaml.cs").Replace("\r\n", "\n").Replace('\r', '\n');

                StringAssert.Contains(
                    source,
                    "        public " + page.Item2 + " ViewModel { get; }\n        public " + page.Item1 + "(");
            }

            var listViewSource = ReadRepoFile(
                "ModernWpf.Gallery",
                "Pages",
                "WpfGallery",
                "Collections",
                "ListViewPage.xaml.cs").Replace("\r\n", "\n").Replace('\r', '\n');
            StringAssert.Contains(
                listViewSource,
                "        public ListViewPageViewModel ViewModel { get; }\n\n        public ListViewPage(ListViewPageViewModel viewModel)");

            var dataGridSource = ReadRepoFile(
                "ModernWpf.Gallery",
                "Pages",
                "WpfGallery",
                "Collections",
                "DataGridPage.xaml.cs");
            AssertContainsInOrder(
                dataGridSource,
                "InitializeComponent();",
                "SystemEvents.UserPreferenceChanged += SystemEvents_UserPreferenceChanged;",
                "this.Loaded += (s, e) => UpdatePageVisuals();",
                "Unloaded += OnUnloaded;",
                "private void SystemEvents_UserPreferenceChanged(object sender, UserPreferenceChangedEventArgs e)",
                "Dispatcher.Invoke(() =>",
                "UpdatePageVisuals();",
                "SystemEvents.UserPreferenceChanged -= SystemEvents_UserPreferenceChanged;");
        }

        [TestMethod]
        public void TextCodeBehindKeepsOfficialConstructorAdjacencyShape()
        {
            foreach (var page in new[]
            {
                Tuple.Create("LabelPage", "LabelPageViewModel"),
                Tuple.Create("PasswordBoxPage", "PasswordBoxPageViewModel"),
                Tuple.Create("RichTextEditPage", "RichTextEditPageViewModel"),
                Tuple.Create("TextBlockPage", "TextBlockPageViewModel"),
                Tuple.Create("TextBoxPage", "TextBoxPageViewModel")
            })
            {
                var source = ReadRepoFile(
                    "ModernWpf.Gallery",
                    "Pages",
                    "WpfGallery",
                    "Text",
                    page.Item1 + ".xaml.cs").Replace("\r\n", "\n").Replace('\r', '\n');

                StringAssert.Contains(
                    source,
                    "        public " + page.Item2 + " ViewModel { get; }\n        public " + page.Item1 + "(");
            }

            var hyperlinkSource = ReadRepoFile(
                "ModernWpf.Gallery",
                "Pages",
                "WpfGallery",
                "Text",
                "HyperlinkPage.xaml.cs").Replace("\r\n", "\n").Replace('\r', '\n');
            AssertContainsInOrder(
                hyperlinkSource,
                "public HyperlinkPage(HyperlinkPageViewModel viewModel)",
                "InitializeComponent();",
                "public HyperlinkPageViewModel ViewModel { get; }");
        }

        [TestMethod]
        public void MessageBoxCodeBehindCentersOwnedShowCalls()
        {
            var source = ReadRepoFile(
                "ModernWpf.Gallery",
                "Pages",
                "WpfGallery",
                "System",
                "MessageBoxPage.xaml.cs");
            var normalizedSource = source.Replace("\r\n", "\n").Replace('\r', '\n');

            StringAssert.Contains(
                normalizedSource,
                "            var result = ShowOwnedMessageBox(\"This is a simple message box!\");\n            ViewModel.DefaultMessageResult = $\"Result: {result}\";");
            StringAssert.Contains(
                normalizedSource,
                "            var result = ShowOwnedMessageBox(\"This is a detailed description of what happened or what action is needed.\", \"Custom Title\");\n            ViewModel.CustomTitleResult = $\"Result: {result}\";");
            StringAssert.Contains(
                normalizedSource,
                "            var result = ShowOwnedMessageBox($\"This MessageBox has {buttonName} button(s).\", $\"{buttonName} Button(s)\", buttonType);\n            ViewModel.DifferentButtonsResult = $\"Result: {result}\";");
            StringAssert.Contains(
                normalizedSource,
                "            var result = ShowOwnedMessageBox($\"This MessageBox displays the {imageName} icon.\", $\"{imageName} Icon\", MessageBoxButton.OK, imageType);\n            ViewModel.DifferentImagesResult = $\"Result: {result}\";");
            StringAssert.Contains(
                normalizedSource,
                "        // 6. Common Messages (Information, Error, Warning)\n        private void ShowCommonInformation_Click(object sender, RoutedEventArgs e)");
            StringAssert.Contains(
                normalizedSource,
                "        // 7. Custom Default Button\n        private void ShowCustomDefaultButton_Click(object sender, RoutedEventArgs e)");
            StringAssert.Contains(
                normalizedSource,
                "            var result = ShowOwnedMessageBox(\"Do you want to save changes? Press Enter to select the default 'No' button.\", \"Save Changes\", MessageBoxButton.YesNoCancel, MessageBoxImage.Question, MessageBoxResult.No);\n            ViewModel.CustomDefaultResult = $\"User selected: {result}\";");
            AssertContainsInOrder(
                normalizedSource,
                "private MessageBoxResult ShowOwnedMessageBox(string messageBoxText)",
                "var owner = GetOwnerWindow();",
                "using (new MessageBoxCenteringScope(owner))",
                "return MessageBox.Show(owner, messageBoxText);");
            AssertContainsInOrder(
                normalizedSource,
                "private MessageBoxResult ShowOwnedMessageBox(string messageBoxText, string caption, MessageBoxButton button, MessageBoxImage icon, MessageBoxResult defaultResult)",
                "using (new MessageBoxCenteringScope(owner))",
                "return MessageBox.Show(owner, messageBoxText, caption, button, icon, defaultResult);");
            StringAssert.Contains(
                normalizedSource,
                "        private Window GetOwnerWindow()\n        {\n            var owner = Window.GetWindow(this) ?? Application.Current.MainWindow;\n            owner?.Activate();\n            return owner;\n        }");
            AssertContainsInOrder(
                normalizedSource,
                "private sealed class MessageBoxCenteringScope : IDisposable",
                "private const int WH_CBT = 5;",
                "private const int HCBT_ACTIVATE = 5;",
                "_hook = SetWindowsHookEx(WH_CBT, _hookProc, IntPtr.Zero, GetCurrentThreadId());",
                "if (code == HCBT_ACTIVATE)",
                "CenterDialog(wParam);",
                "SetWindowPos(dialogHandle, IntPtr.Zero, left, top, 0, 0, SWP_NOSIZE | SWP_NOZORDER | SWP_NOACTIVATE);");
            Assert.IsFalse(
                normalizedSource.Contains("MessageBox.Show(\"", StringComparison.Ordinal),
                "Gallery runtime dialogs must pass an owner window so recording and app placement stay on the Gallery window.");
            Assert.IsFalse(
                normalizedSource.Contains("MessageBox.Show($\"", StringComparison.Ordinal),
                "Gallery runtime dialogs must pass an owner window so recording and app placement stay on the Gallery window.");

            AssertContainsInOrder(
                normalizedSource,
                "var buttonType = GetMessageBoxButton(ViewModel.SelectedButtonIndex);",
                "var result = ShowOwnedMessageBox($\"This MessageBox has {buttonName} button(s).\", $\"{buttonName} Button(s)\", buttonType);",
                "private Window GetOwnerWindow()",
                "private static MessageBoxButton GetMessageBoxButton(int index)");
            StringAssert.Contains(
                normalizedSource,
                "#if NET10_0_OR_GREATER\n                case 2:\n                    return MessageBoxButton.AbortRetryIgnore;\n#endif");
            StringAssert.Contains(
                normalizedSource,
                "#if NET10_0_OR_GREATER\n                case 5:\n                    return MessageBoxButton.RetryCancel;\n                case 6:\n                    return MessageBoxButton.CancelTryContinue;\n#endif");
            AssertContainsInOrder(
                normalizedSource,
                "var imageType = GetMessageBoxImage(ViewModel.SelectedImageIndex);",
                "var result = ShowOwnedMessageBox($\"This MessageBox displays the {imageName} icon.\", $\"{imageName} Icon\", MessageBoxButton.OK, imageType);",
                "private static MessageBoxImage GetMessageBoxImage(int index)");
        }

        [TestMethod]
        public void BasicInputPagesKeepOfficialHeaderAndSampleSourceShape()
        {
            foreach (var page in new[]
            {
                "ButtonPage.xaml",
                "CheckBoxPage.xaml",
                "ComboBoxPage.xaml",
                "RadioButtonPage.xaml",
                "SliderPage.xaml"
            })
            {
                var xaml = ReadRepoFile(
                    "ModernWpf.Gallery",
                    "Pages",
                    "WpfGallery",
                    "BasicInput",
                    page);
                var normalizedXaml = xaml.Replace("\r\n", "\n").Replace('\r', '\n');
                StringAssert.Contains(
                    xaml,
                    "xmlns:local=\"clr-namespace:ModernWpf.Gallery.Pages.WpfGallery.BasicInput\"");
                StringAssert.Contains(
                    normalizedXaml,
                    "<Grid x:Name=\"ContentPagePane\" Height=\"Auto\">\n\n        <Grid.RowDefinitions>");
                StringAssert.Contains(
                    normalizedXaml,
                    "</Grid.RowDefinitions>\n        <controls:PageHeader");
                StringAssert.Contains(
                    xaml,
                    "<controls:PageHeader Margin=\"0,0,0,32\" Title=\"{Binding ViewModel.PageTitle}\" ShowDescription=\"False\" />");
                StringAssert.Contains(
                    xaml,
                    "<ScrollViewer Grid.Row=\"1\" Margin=\"0,0,0,24\" Padding=\"0,0,24,0\">");
            }

            var buttonXaml = ReadRepoFile(
                "ModernWpf.Gallery",
                "Pages",
                "WpfGallery",
                "BasicInput",
                "ButtonPage.xaml");
            AssertContainsInOrder(
                buttonXaml,
                "x:Name=\"SimpleButtonExample\"",
                "Margin=\"10\"",
                "HeaderText=\"Simple Button\"",
                "XamlCode=\"&lt;Button Content=&quot;Standard WPF button&quot; /&gt;\"",
                "<Button",
                "x:Name=\"SimpleButton\"",
                "AutomationProperties.Name=\"Standard WPF\"",
                "Content=\"Standard WPF button\"");
            AssertContainsInOrder(
                buttonXaml,
                "<!--<controls:ControlExample",
                "HeaderText=\"Button with Icon\"",
                "XamlCode=\"&lt;Button Content=&quot;Font Icon Button&quot; Icon=&quot;Fluent24&quot; /&gt;\"",
                "IsEnabled=\"{Binding ViewModel.IsUiButtonEnabled, RelativeSource={RelativeSource Mode=FindAncestor, AncestorType=local:ButtonPage}, Mode=OneWay}\"",
                "<!--<SymbolIcon Symbol=\"Fluent24\" />-->",
                "</controls:ControlExample>-->");
            AssertContainsInOrder(
                buttonXaml,
                "HeaderText=\"WPF Accent Button\"",
                "<!--<SymbolIcon Symbol=\"Fluent24\" />-->",
                "<TextBlock Text=\"WPF Accent Button\" />",
                "HeaderText=\"WPF Button with FontIcon\"",
                "HeaderText=\"WPF Button with FontIcon\"",
                "HeaderText=\"WPF Button with ImageIcon\"");

            var checkBoxXaml = ReadRepoFile(
                "ModernWpf.Gallery",
                "Pages",
                "WpfGallery",
                "BasicInput",
                "CheckBoxPage.xaml");
            var normalizedCheckBoxXaml = checkBoxXaml.Replace("\r\n", "\n").Replace('\r', '\n');
            AssertContainsInOrder(
                checkBoxXaml,
                "x:Name=\"TwoStateCheckBoxExample\"",
                "Margin=\"10\"",
                "HeaderText=\"A 2-state CheckBox.\"",
                "XamlCode=\"&lt;CheckBox Content=&quot;Two-state CheckBox&quot; /&gt;\"",
                "<CheckBox",
                "x:Name=\"TwoStateCheckBox\"",
                "AutomationProperties.Name=\"Sample Two State\"",
                "Content=\"Two-state CheckBox\"");
            StringAssert.Contains(
                normalizedCheckBoxXaml,
                "</controls:ControlExample>\n\n\n                <controls:ControlExample\n                    Margin=\"10,32,10,10\"\n                    HeaderText=\"A 3-state CheckBox.\"");
            StringAssert.Contains(
                normalizedCheckBoxXaml,
                "</controls:ControlExample>\n            </StackPanel>\n\n        </ScrollViewer>");

            var comboBoxXaml = ReadRepoFile(
                "ModernWpf.Gallery",
                "Pages",
                "WpfGallery",
                "BasicInput",
                "ComboBoxPage.xaml");
            AssertContainsInOrder(
                comboBoxXaml,
                "<StackPanel Margin=\"0,0,0,24\">",
                "x:Name=\"InlineComboBoxExample\"",
                "HeaderText=\"A ComboBox with items defined inline.\"",
                "x:Name=\"InlineComboBox\"",
                "AutomationProperties.Name=\"Sample defined inline\"",
                "<ComboBoxItem Content=\"Blue\" />",
                "<ComboBoxItem Content=\"Green\" />",
                "<ComboBoxItem Content=\"Red\" />",
                "<ComboBoxItem Content=\"Yellow\" />",
                "HeaderText=\"A ComboBox with ItemsSource set.\"",
                "AutomationProperties.Name=\"Sample item source set\"",
                "ItemsSource=\"{Binding ViewModel.ComboBoxFontFamilies, RelativeSource={RelativeSource Mode=FindAncestor, AncestorType=local:ComboBoxPage}, Mode=OneWay}\"",
                "<ComboBox.ItemTemplate>",
                "<TextBlock FontFamily=\"{Binding}\" Text=\"{Binding}\" />",
                "HeaderText=\"An editable ComboBox.\"",
                "AutomationProperties.Name=\"Editable\"",
                "IsEditable=\"True\"",
                "ItemsSource=\"{Binding ViewModel.ComboBoxFontSizes, RelativeSource={RelativeSource Mode=FindAncestor, AncestorType=local:ComboBoxPage}, Mode=OneWay}\"",
                "SelectedIndex=\"0\" />");

            var radioButtonXaml = ReadRepoFile(
                "ModernWpf.Gallery",
                "Pages",
                "WpfGallery",
                "BasicInput",
                "RadioButtonPage.xaml");
            var normalizedRadioButtonXaml = radioButtonXaml.Replace("\r\n", "\n").Replace('\r', '\n');
            StringAssert.Contains(
                radioButtonXaml,
                "<StackPanel Grid.Column=\"0\" KeyboardNavigation.TabNavigation=\"Once\" KeyboardNavigation.DirectionalNavigation=\"Cycle\">");
            AssertContainsInOrder(
                radioButtonXaml,
                "x:Name=\"StandardRadioButtonExample\"",
                "x:Name=\"DefaultRadioButtonOption1\"",
                "AutomationProperties.Name=\"Default Radio Option 1\"",
                "Content=\"Option 1\"",
                "GroupName=\"radio_group_one\"",
                "IsChecked=\"True\"",
                "GotKeyboardFocus=\"RadioButton_GotKeyboardFocus\"",
                "IsEnabled=");
            AssertContainsInOrder(
                radioButtonXaml,
                "AutomationProperties.Name=\"Left Flow Radio Option 1\"",
                "Content=\"Option 1\"",
                "FlowDirection=\"RightToLeft\"",
                "GroupName=\"radio_group_two\"",
                "GotKeyboardFocus=\"RadioButton_GotKeyboardFocus\"",
                "IsChecked=\"True\" />");
            StringAssert.Contains(
                normalizedRadioButtonXaml,
                "</controls:ControlExample>\n            </StackPanel>\n\n        </ScrollViewer>");

            var sliderXaml = ReadRepoFile(
                "ModernWpf.Gallery",
                "Pages",
                "WpfGallery",
                "BasicInput",
                "SliderPage.xaml");
            var normalizedSliderXaml = sliderXaml.Replace("\r\n", "\n").Replace('\r', '\n');
            AssertContainsInOrder(
                sliderXaml,
                "<StackPanel Margin=\"0,0,0,24\">",
                "x:Name=\"SimpleSliderExample\"",
                "HeaderText=\"A simple slider.\"",
                "x:Name=\"SimpleSlider\"",
                "AutomationProperties.Name=\"Simple\"",
                "Value=\"{Binding ViewModel.SimpleSliderValue, RelativeSource={RelativeSource Mode=FindAncestor, AncestorType=local:SliderPage}, Mode=TwoWay}\"",
                "Text=\"{Binding ViewModel.SimpleSliderValue, RelativeSource={RelativeSource Mode=FindAncestor, AncestorType=local:SliderPage}, Mode=OneWay}\"",
                "HeaderText=\"A slider with steps and range specified.\"",
                "AutomationProperties.Name=\"Range and steps specified\"",
                "TickFrequency=\"50\"",
                "Value=\"{Binding ViewModel.RangeSliderValue, RelativeSource={RelativeSource Mode=FindAncestor, AncestorType=local:SliderPage}, Mode=TwoWay}\"",
                "HeaderText=\"A slider with tick marks.\"",
                "AutomationProperties.Name=\"Tick marks\"",
                "TickFrequency=\"20\"",
                "TickPlacement=\"Both\"",
                "Value=\"{Binding ViewModel.MarksSliderValue, RelativeSource={RelativeSource Mode=FindAncestor, AncestorType=local:SliderPage}, Mode=TwoWay}\"",
                "HeaderText=\"A vertical slider with range and tick marks specified.\"",
                "AutomationProperties.Name=\"Vertical\"",
                "Orientation=\"Vertical\"",
                "Value=\"{Binding ViewModel.VerticalSliderValue, RelativeSource={RelativeSource Mode=FindAncestor, AncestorType=local:SliderPage}, Mode=TwoWay}\"");
            StringAssert.Contains(
                normalizedSliderXaml,
                "</controls:ControlExample>\n            </StackPanel>\n\n        </ScrollViewer>");
        }

        [TestMethod]
        public void CollectionsPagesKeepOfficialHeaderAndSampleSourceShape()
        {
            foreach (var page in new[]
            {
                Tuple.Create("DataGridPage.xaml", true),
                Tuple.Create("ListBoxPage.xaml", false),
                Tuple.Create("ListViewPage.xaml", true),
                Tuple.Create("TreeViewPage.xaml", false)
            })
            {
                var xaml = ReadRepoFile(
                    "ModernWpf.Gallery",
                    "Pages",
                    "WpfGallery",
                    "Collections",
                    page.Item1);
                var normalizedXaml = xaml.Replace("\r\n", "\n").Replace('\r', '\n');
                StringAssert.Contains(
                    xaml,
                    "xmlns:local=\"clr-namespace:ModernWpf.Gallery.Pages.WpfGallery.Collections\"");
                if (page.Item2)
                {
                    StringAssert.Contains(
                        xaml,
                        "xmlns:models=\"clr-namespace:ModernWpf.Gallery.Models\"");
                }

                StringAssert.Contains(
                    normalizedXaml,
                    "<Grid x:Name=\"ContentPagePane\" Height=\"Auto\">\n\n        <Grid.RowDefinitions>");
                StringAssert.Contains(
                    normalizedXaml,
                    "</Grid.RowDefinitions>\n        <controls:PageHeader");
                StringAssert.Contains(
                    xaml,
                    "<controls:PageHeader Margin=\"0,0,0,32\" Title=\"{Binding ViewModel.PageTitle}\" ShowDescription=\"False\" />");
                StringAssert.Contains(
                    xaml,
                    "<ScrollViewer Grid.Row=\"1\" Margin=\"0,0,0,24\" Padding=\"0,0,24,0\">");
            }

            var listBoxXaml = ReadRepoFile(
                "ModernWpf.Gallery",
                "Pages",
                "WpfGallery",
                "Collections",
                "ListBoxPage.xaml");
            StringAssert.Contains(
                listBoxXaml,
                "<!--<controls:ControlExample.XamlCode>");
            StringAssert.Contains(
                listBoxXaml,
                "\\t&lt;ListBoxItem Content=&quot;Blue&quot;/&gt;\\n");

            var listViewXaml = ReadRepoFile(
                "ModernWpf.Gallery",
                "Pages",
                "WpfGallery",
                "Collections",
                "ListViewPage.xaml");
            StringAssert.Contains(
                listViewXaml,
                "<!--<controls:ControlExample.XamlCode>");
            StringAssert.Contains(
                listViewXaml,
                "&lt;ListView ItemsSource=&quot;{Binding ViewModel.MyCollection}&quot;&gt;&lt;&gt;\\n");
            StringAssert.Contains(
                listViewXaml,
                "<Label Foreground=\"{DynamicResource TextFillColorPrimaryBrush}\" Opacity=\"0.7\" Content=\"Selection mode\" Target=\"{Binding ElementName=SelectionModeComboBox}\" />");
            AssertContainsInOrder(
                listViewXaml,
                "AutomationProperties.Name=\"ListView with GridView\"",
                "<GridViewColumn",
                "Header=\"First Name\"",
                "Width=\"150\"",
                "DisplayMemberBinding=\"{Binding FirstName}\" />",
                "<GridViewColumn",
                "Header=\"Last Name\"",
                "Width=\"150\"",
                "DisplayMemberBinding=\"{Binding LastName}\" />",
                "<GridViewColumn",
                "Header=\"Company\"",
                "Width=\"200\"",
                "DisplayMemberBinding=\"{Binding Company}\" />");

            var dataGridXaml = ReadRepoFile(
                "ModernWpf.Gallery",
                "Pages",
                "WpfGallery",
                "Collections",
                "DataGridPage.xaml");
            var normalizedDataGridXaml = dataGridXaml.Replace("\r\n", "\n").Replace('\r', '\n');
            AssertContainsInOrder(
                dataGridXaml,
                "<controls:ControlExample",
                "Margin=\"10\"",
                "HeaderText=\"Default DataGrid with ItemsSource.\"",
                "XamlCode=\"&lt;DataGrid ItemsSource=&quot;{Binding ViewModel.ProductsCollection, Mode=TwoWay}&quot; /&gt;\"",
                "<DataGrid",
                "x:Name=\"SampleDataGrid\"",
                "Height=\"400\"",
                "AutomationProperties.Name=\"Sample Data Grid\"",
                "ItemsSource=\"{Binding ViewModel.ProductsCollection, Mode=TwoWay}\" />");
            StringAssert.Contains(
                normalizedDataGridXaml,
                "</controls:ControlExample>\n\n            </StackPanel>");
            StringAssert.Contains(
                normalizedDataGridXaml,
                "</Grid>\n\n\n</Page>");

            var treeViewXaml = ReadRepoFile(
                "ModernWpf.Gallery",
                "Pages",
                "WpfGallery",
                "Collections",
                "TreeViewPage.xaml");
            AssertContainsInOrder(
                treeViewXaml,
                "<Grid Margin=\"0,0,0,24\">",
                "<controls:ControlExample",
                "Margin=\"10\"",
                "HeaderText=\"Simple TreeView.\"",
                "XamlCode=\"&lt;TreeView AllowDrop=&quot;True&quot; ScrollViewer.CanContentScroll=&quot;False&quot;&gt;",
                "<TreeView",
                "AllowDrop=\"True\"",
                "AutomationProperties.Name=\"Sample TreeView\"",
                "ScrollViewer.CanContentScroll=\"False\">",
                "<TreeViewItem",
                "Header=\"Work Documents\"",
                "IsExpanded=\"True\"",
                "IsSelected=\"True\">",
                "<TreeViewItem Header=\"Feature Schedule\" />",
                "<TreeViewItem Header=\"Overall Project Plan\" />",
                "<TreeViewItem Header=\"Personal Documents\">",
                "<TreeViewItem Header=\"Contractor contact info\" />",
                "<TreeViewItem Header=\"Home Remodel\">",
                "<TreeViewItem Header=\"Paint Color Scheme\" />",
                "<TreeViewItem Header=\"Flooring Woodgrain Type\" />",
                "<TreeViewItem Header=\"Kitchen Cabinet Style\" />");
        }

        [TestMethod]
        public void LayoutPagesKeepOfficialHeaderAndSampleSourceShape()
        {
            foreach (var page in new[]
            {
                Tuple.Create("BorderPage.xaml", false),
                Tuple.Create("ExpanderPage.xaml", false),
                Tuple.Create("GridPage.xaml", false),
                Tuple.Create("GridSplitterPage.xaml", false),
                Tuple.Create("GroupBoxPage.xaml", true),
                Tuple.Create("ResizeGripPage.xaml", true),
                Tuple.Create("StackPanelPage.xaml", false)
            })
            {
                var xaml = ReadRepoFile(
                    "ModernWpf.Gallery",
                    "Pages",
                    "WpfGallery",
                    "Layout",
                    page.Item1);
                var normalizedXaml = xaml.Replace("\r\n", "\n").Replace('\r', '\n');
                StringAssert.Contains(
                    xaml,
                    "xmlns:local=\"clr-namespace:ModernWpf.Gallery.Pages.WpfGallery.Layout\"");
                if (page.Item1 == "GridSplitterPage.xaml")
                {
                    StringAssert.Contains(
                        xaml,
                        "xmlns:sys=\"clr-namespace:System;assembly=System.Runtime\"");
                    AssertContainsInOrder(
                        xaml,
                        "<Page x:Class=\"ModernWpf.Gallery.Pages.WpfGallery.Layout.GridSplitterPage\"",
                        "xmlns:mc=\"http://schemas.openxmlformats.org/markup-compatibility/2006\"",
                        "xmlns:d=\"http://schemas.microsoft.com/expression/blend/2008\"",
                        "xmlns:local=\"clr-namespace:ModernWpf.Gallery.Pages.WpfGallery.Layout\"",
                        "xmlns:sys=\"clr-namespace:System;assembly=System.Runtime\"",
                        "xmlns:controls=\"clr-namespace:ModernWpf.Gallery.Controls\"",
                        "mc:Ignorable=\"d\"",
                        "d:DesignHeight=\"450\" d:DesignWidth=\"800\"",
                        "Title=\"GridSplitterPage\"",
                        "Foreground=\"{DynamicResource TextFillColorPrimaryBrush}\">");
                }
                else if (page.Item1 == "GridPage.xaml")
                {
                    AssertContainsInOrder(
                        xaml,
                        "<Page x:Class=\"ModernWpf.Gallery.Pages.WpfGallery.Layout.GridPage\"",
                        "xmlns:mc=\"http://schemas.openxmlformats.org/markup-compatibility/2006\"",
                        "xmlns:d=\"http://schemas.microsoft.com/expression/blend/2008\"",
                        "xmlns:local=\"clr-namespace:ModernWpf.Gallery.Pages.WpfGallery.Layout\"",
                        "xmlns:controls=\"clr-namespace:ModernWpf.Gallery.Controls\"",
                        "mc:Ignorable=\"d\"",
                        "d:DesignHeight=\"450\" d:DesignWidth=\"800\"",
                        "Title=\"GridPage\"",
                        "Foreground=\"{DynamicResource TextFillColorPrimaryBrush}\">");
                }
                else if (page.Item1 == "GroupBoxPage.xaml")
                {
                    AssertContainsInOrder(
                        xaml,
                        "<Page x:Class=\"ModernWpf.Gallery.Pages.WpfGallery.Layout.GroupBoxPage\"",
                        "xmlns:mc=\"http://schemas.openxmlformats.org/markup-compatibility/2006\"",
                        "xmlns:d=\"http://schemas.microsoft.com/expression/blend/2008\"",
                        "xmlns:local=\"clr-namespace:ModernWpf.Gallery.Pages.WpfGallery.Layout\"",
                        "xmlns:controls=\"clr-namespace:ModernWpf.Gallery.Controls\"",
                        "mc:Ignorable=\"d\"",
                        "d:DesignHeight=\"450\" d:DesignWidth=\"800\"",
                        "Title=\"GroupBoxPage\"",
                        "Foreground=\"{DynamicResource TextFillColorPrimaryBrush}\">");
                }
                else if (page.Item1 == "ResizeGripPage.xaml")
                {
                    AssertContainsInOrder(
                        xaml,
                        "<Page x:Class=\"ModernWpf.Gallery.Pages.WpfGallery.Layout.ResizeGripPage\"",
                        "xmlns:mc=\"http://schemas.openxmlformats.org/markup-compatibility/2006\"",
                        "xmlns:d=\"http://schemas.microsoft.com/expression/blend/2008\"",
                        "xmlns:local=\"clr-namespace:ModernWpf.Gallery.Pages.WpfGallery.Layout\"",
                        "xmlns:controls=\"clr-namespace:ModernWpf.Gallery.Controls\"",
                        "mc:Ignorable=\"d\"",
                        "d:DesignHeight=\"450\" d:DesignWidth=\"800\"",
                        "Title=\"ResizeGripPage\"",
                        "Foreground=\"{DynamicResource TextFillColorPrimaryBrush}\">");
                }

                if (page.Item2)
                {
                    StringAssert.Contains(
                        normalizedXaml,
                        "        <Grid x:Name=\"ContentPagePane\" Height=\"Auto\">\n            <Grid.RowDefinitions>");
                    StringAssert.Contains(
                        normalizedXaml,
                        "            </Grid.RowDefinitions>\n            <controls:PageHeader");
                }
                else
                {
                    StringAssert.Contains(
                        normalizedXaml,
                        "<Grid x:Name=\"ContentPagePane\" Height=\"Auto\">\n\n        <Grid.RowDefinitions>");
                    StringAssert.Contains(
                        normalizedXaml,
                        "</Grid.RowDefinitions>\n        <controls:PageHeader");
                }

                StringAssert.Contains(
                    xaml,
                    "<controls:PageHeader Margin=\"0,0,0,32\" Title=\"{Binding ViewModel.PageTitle}\" ShowDescription=\"False\" />");
                StringAssert.Contains(
                    xaml,
                    "<ScrollViewer Grid.Row=\"1\" Margin=\"0,0,0,24\" Padding=\"0,0,24,0\">");
            }

            var expanderXaml = ReadRepoFile(
                "ModernWpf.Gallery",
                "Pages",
                "WpfGallery",
                "Layout",
                "ExpanderPage.xaml");
            StringAssert.Contains(
                expanderXaml,
                "<!--  TODO: ExpandDirection  -->");

            var borderXaml = ReadRepoFile(
                "ModernWpf.Gallery",
                "Pages",
                "WpfGallery",
                "Layout",
                "BorderPage.xaml");
            AssertContainsInOrder(
                borderXaml,
                "<Border BorderBrush=\"Gray\" BorderThickness=\"2\" Padding=\"10\">",
                "<Border BorderBrush=\"CornflowerBlue\" BorderThickness=\"2\" CornerRadius=\"10\" Padding=\"15\" Background=\"LightBlue\">",
                "<TextBlock Text=\"Rounded Border\" Foreground=\"Black\" />",
                "<Border BorderBrush=\"DarkSlateGray\" BorderThickness=\"1,2,4,8\" Padding=\"10\">");

            var gridXaml = ReadRepoFile(
                "ModernWpf.Gallery",
                "Pages",
                "WpfGallery",
                "Layout",
                "GridPage.xaml");
            var normalizedGridXaml = gridXaml.Replace("\r\n", "\n").Replace('\r', '\n');
            StringAssert.Contains(
                normalizedGridXaml,
                "XamlCode=\"&lt;Grid ShowGridLines=&quot;True&quot;&gt;&#10;\n    &lt;Grid.RowDefinitions&gt;&#10;\n        &lt;RowDefinition Height=&quot;*&quot; /&gt;&#10;");
            StringAssert.Contains(
                normalizedGridXaml,
                "    &lt;TextBlock Grid.Row=&quot;2&quot; Grid.Column=&quot;2&quot; Text=&quot;Cell 9&quot; /&gt;&#10;\n&lt;/Grid&gt;\">");
            StringAssert.Contains(
                normalizedGridXaml,
                "XamlCode=\"&lt;Grid&gt;&#10;\n    &lt;Grid.RowDefinitions&gt;&#10;\n        &lt;RowDefinition Height=&quot;Auto&quot; /&gt;&#10;");
            StringAssert.Contains(
                normalizedGridXaml,
                "    &lt;Border Grid.Row=&quot;2&quot; Grid.Column=&quot;2&quot; Background=&quot;{DynamicResource ControlFillColorDefaultBrush}&quot; Margin=&quot;5&quot; Padding=&quot;10&quot;&gt;&#10;\n        &lt;TextBlock Text=&quot;Row 2, Column 2&quot; /&gt;&#10;\n    &lt;/Border&gt;&#10;\n&lt;/Grid&gt;\">");
            StringAssert.Contains(
                normalizedGridXaml,
                "XamlCode=\"&lt;Grid RowDefinitions=&quot;Auto,*,Auto&quot; ColumnDefinitions=&quot;100,2*,*&quot;&gt;&#10;\n    &lt;Border Grid.Row=&quot;0&quot; Grid.Column=&quot;0&quot; Background=&quot;{DynamicResource ControlFillColorDefaultBrush}&quot; Margin=&quot;5&quot; Padding=&quot;10&quot;&gt;&#10;");
            StringAssert.Contains(
                normalizedGridXaml,
                "    &lt;Border Grid.Row=&quot;2&quot; Grid.Column=&quot;0&quot; Grid.ColumnSpan=&quot;3&quot; Background=&quot;{DynamicResource ControlFillColorDefaultBrush}&quot; Margin=&quot;5&quot; Padding=&quot;10&quot;&gt;&#10;\n        &lt;TextBlock Text=&quot;Footer (Auto height, spans all columns)&quot; /&gt;&#10;\n    &lt;/Border&gt;&#10;\n&lt;/Grid&gt;\">");
            AssertContainsInOrder(
                gridXaml,
                "HeaderText=\"A Grid with custom sizing and spanning\"",
                "<Border Grid.Row=\"0\" Grid.Column=\"0\" Background=\"{DynamicResource ControlFillColorDefaultBrush}\" Margin=\"5\" Padding=\"10\">",
                "<Border Grid.Row=\"1\" Grid.Column=\"0\" Grid.ColumnSpan=\"3\" Background=\"{DynamicResource ControlFillColorSecondaryBrush}\" Margin=\"5\" Padding=\"10\">",
                "HeaderText=\"Grid using XAML shorthand syntax\"",
                "XamlCode=\"&lt;Grid RowDefinitions=&quot;Auto,*,Auto&quot; ColumnDefinitions=&quot;100,2*,*&quot;&gt;",
                "<Grid Height=\"300\">",
                "<Grid.RowDefinitions>",
                "<Border Grid.Row=\"0\" Grid.Column=\"0\" Background=\"{DynamicResource ControlFillColorDefaultBrush}\" Margin=\"5\" Padding=\"10\">",
                "<Border Grid.Row=\"1\" Grid.Column=\"0\" Grid.ColumnSpan=\"3\" Background=\"{DynamicResource ControlAltFillColorSecondaryBrush}\" Margin=\"5\" Padding=\"10\">",
                "<TextBlock Text=\"Main Content Area (fills available space)\" VerticalAlignment=\"Center\" HorizontalAlignment=\"Center\" />");

            var gridSplitterXaml = ReadRepoFile(
                "ModernWpf.Gallery",
                "Pages",
                "WpfGallery",
                "Layout",
                "GridSplitterPage.xaml");
            StringAssert.Contains(
                gridSplitterXaml.Replace("\r\n", "\n").Replace('\r', '\n'),
                "XamlCode=\"&lt;Grid Height=&quot;400&quot;&gt;&#10;\n    &lt;Grid.RowDefinitions&gt;&#10;");
            StringAssert.Contains(
                gridSplitterXaml.Replace("\r\n", "\n").Replace('\r', '\n'),
                Lines(
                    "        <sys:String x:Key=\"SampleText\">",
                    "            Lorem Ipsum is simply dummy text of the printing and typesetting industry.",
                    "        Lorem Ipsum has been the industry's standard dummy text ever since the 1500s.",
                    "        </sys:String>",
                    "        <sys:String x:Key=\"SampleText2\">",
                    "            When an unknown printer took a galley of type and scrambled it to",
                    "        make a type specimen book.",
                    "        </sys:String>"));
            AssertContainsInOrder(
                gridSplitterXaml,
                "<TextBlock Style=\"{DynamicResource TitleTextBlockStyle}\" Text=\"Grid Splitter\" Margin=\"0 0 0 10\"/>",
                "<Border",
                "BorderBrush=\"{DynamicResource ControlElevationBorderBrush}\"",
                "BorderThickness=\"2\"",
                "Grid.Row=\"1\"",
                "Padding=\"10\"",
                "CornerRadius=\"4\">",
                "<TextBlock TextWrapping=\"Wrap\" Text=\"{StaticResource SampleText}\" />",
                "<GridSplitter Grid.RowSpan=\"5\" Grid.Column=\"1\" ResizeDirection=\"Columns\"/>",
                "<GridSplitter Grid.Row=\"1\" Grid.ColumnSpan=\"3\" ResizeDirection=\"Rows\"/>",
                "<GridSplitter Grid.Row=\"3\" Grid.ColumnSpan=\"1\" ResizeDirection=\"Rows\"/>");

            var groupBoxXaml = ReadRepoFile(
                "ModernWpf.Gallery",
                "Pages",
                "WpfGallery",
                "Layout",
                "GroupBoxPage.xaml");
            var normalizedGroupBoxXaml = groupBoxXaml.Replace("\r\n", "\n").Replace('\r', '\n');
            StringAssert.Contains(
                normalizedGroupBoxXaml,
                "XamlCode=\"&lt;GroupBox &#10;\n   Header=&quot;User Information&quot; &#10;\n   HorizontalAlignment=&quot;Left&quot; &#10;\n   VerticalAlignment=&quot;Center&quot; &#10;\n   Width=&quot;400&quot;&gt;&#10;");
            StringAssert.Contains(
                normalizedGroupBoxXaml,
                "        &lt;Button Content=&quot;Submit&quot; HorizontalAlignment=&quot;Right&quot; Width=&quot;100&quot; Margin=&quot;0,10,0,0&quot; /&gt;&#10;\n    &lt;/StackPanel&gt;&#10;&lt;/GroupBox&gt;\">");
            AssertContainsInOrder(
                groupBoxXaml,
                "<GroupBox",
                "Header=\"User Information\"",
                "HorizontalAlignment=\"Left\"",
                "VerticalAlignment=\"Center\"",
                "Width=\"400\">",
                "<TextBox Name=\"NameTextBox\" Width=\"280\" Margin=\"10,0,0,20\" AutomationProperties.Name=\"Name Field\"/>",
                "<TextBlock Width=\"100\" Text=\"Gender:\" Margin=\"0,10,0,0\"/>",
                "<TextBox Name=\"GenderTextBox\" Width=\"280\" Margin=\"10,0,0,20\" AutomationProperties.Name=\"Gender Field\"/>",
                "<Button Content=\"Submit\" HorizontalAlignment=\"Right\" Margin=\"0,10,0,0\" />");
            StringAssert.Contains(
                normalizedGroupBoxXaml,
                "</StackPanel>\n                                <Button Content=\"Submit\" HorizontalAlignment=\"Right\" Margin=\"0,10,0,0\" />");
            StringAssert.Contains(
                groupBoxXaml,
                "&lt;Button Content=&quot;Submit&quot; HorizontalAlignment=&quot;Right&quot; Width=&quot;100&quot; Margin=&quot;0,10,0,0&quot; /&gt;");

            var resizeGripXaml = ReadRepoFile(
                "ModernWpf.Gallery",
                "Pages",
                "WpfGallery",
                "Layout",
                "ResizeGripPage.xaml");
            StringAssert.Contains(
                resizeGripXaml,
                "<StackPanel Orientation=\"Vertical\" Grid.Row=\"1\">");
            StringAssert.Contains(
                resizeGripXaml,
                "<LineBreak/>");
            AssertContainsInOrder(
                resizeGripXaml,
                "<controls:ControlExample",
                "Margin=\"10\"",
                "HeaderText=\"A ResizeGrip\"",
                "XamlCode=\"&lt;Window",
                "CSharpCode=\"private void OpenResizeGripWindow_Click");
            AssertContainsInOrder(
                resizeGripXaml,
                "<Button",
                "x:Name=\"OpenResizeGripWindow\"",
                "VerticalAlignment=\"Center\"",
                "HorizontalAlignment=\"Center\"",
                "Content=\"Open window with resize grip\"",
                "Click=\"OpenResizeGripWindow_Click\" />");

            var resizeGripCode = ReadRepoFile(
                "ModernWpf.Gallery",
                "Pages",
                "WpfGallery",
                "Layout",
                "ResizeGripPage.xaml.cs");
            AssertContainsInOrder(
                resizeGripCode,
                "private void OpenResizeGripWindow_Click(object sender, RoutedEventArgs e)",
                "Window window = new Window()",
                "ResizeMode = ResizeMode.CanResizeWithGrip,",
                "Content = new TextBlock",
                "Text = \"ResizeGrip is present at the bottom right corner of the window\",",
                "window.Show();");

            var stackPanelXaml = ReadRepoFile(
                "ModernWpf.Gallery",
                "Pages",
                "WpfGallery",
                "Layout",
                "StackPanelPage.xaml");
            AssertContainsInOrder(
                stackPanelXaml,
                "<StackPanel Orientation=\"Vertical\">",
                "<Rectangle Width=\"100\" Height=\"30\" Fill=\"CornflowerBlue\" Margin=\"5\" />",
                "<Rectangle Width=\"100\" Height=\"30\" Fill=\"LightCoral\" Margin=\"5\" />",
                "<Rectangle Width=\"100\" Height=\"30\" Fill=\"MediumSeaGreen\" Margin=\"5\" />",
                "<StackPanel Orientation=\"Horizontal\">",
                "<Rectangle Width=\"100\" Height=\"30\" Fill=\"CornflowerBlue\" Margin=\"5\" />",
                "<Rectangle Width=\"100\" Height=\"30\" Fill=\"LightCoral\" Margin=\"5\" />",
                "<Rectangle Width=\"100\" Height=\"30\" Fill=\"MediumSeaGreen\" Margin=\"5\" />");
        }

        [TestMethod]
        public void DesignGuidancePagesKeepOfficialHeaderAndSampleSourceShape()
        {
            var colorXaml = ReadRepoFile(
                "ModernWpf.Gallery",
                "Pages",
                "WpfGallery",
                "DesignGuidance",
                "ColorsPage.xaml");
            var normalizedColorXaml = colorXaml.Replace("\r\n", "\n").Replace('\r', '\n');
            StringAssert.Contains(
                colorXaml,
                "xmlns:local=\"clr-namespace:ModernWpf.Gallery.Pages.WpfGallery.DesignGuidance\"");
            AssertContainsInOrder(
                colorXaml,
                "<Page x:Class=\"ModernWpf.Gallery.Pages.WpfGallery.DesignGuidance.ColorsPage\"",
                "xmlns:mc=\"http://schemas.openxmlformats.org/markup-compatibility/2006\"",
                "xmlns:d=\"http://schemas.microsoft.com/expression/blend/2008\"",
                "xmlns:local=\"clr-namespace:ModernWpf.Gallery.Pages.WpfGallery.DesignGuidance\"",
                "xmlns:controls=\"clr-namespace:ModernWpf.Gallery.Controls\"",
                "xmlns:sys=\"clr-namespace:System;assembly=mscorlib\"",
                "mc:Ignorable=\"d\"",
                "Foreground=\"{DynamicResource TextFillColorPrimaryBrush}\"",
                "d:DesignHeight=\"450\" d:DesignWidth=\"800\"",
                "Title=\"ColorsPage\">");
            StringAssert.Contains(
                normalizedColorXaml,
                "<Grid x:Name=\"ContentPagePane\" Height=\"Auto\">\n\n        <Grid.RowDefinitions>");
            StringAssert.Contains(
                normalizedColorXaml,
                "</Grid.RowDefinitions>\n\n        <controls:PageHeader");
            StringAssert.Contains(
                colorXaml,
                "<controls:PageHeader Margin=\"0,0,0,32\" Title=\"{Binding ViewModel.PageTitle}\" Description=\"{Binding ViewModel.PageDescription}\" />");
            StringAssert.Contains(
                colorXaml,
                "<ScrollViewer Margin=\"0,0,0,24\" Grid.Row=\"1\" Padding=\"0,0,24,0\">");
            StringAssert.Contains(
                colorXaml,
                "<ComboBox x:Name=\"PageSelector\" SelectionChanged=\"OnSelectionChanged\" Loaded=\"OnLoaded\" Width=\"200\" AutomationProperties.Name=\"Page Selector\">");
            StringAssert.Contains(
                colorXaml,
                "<Frame x:Name=\"ColorSubpageNavigationFrame\" />");

            foreach (var page in new[]
            {
                "TypographyPage.xaml",
                "SpacingPage.xaml"
            })
            {
                var xaml = ReadRepoFile(
                    "ModernWpf.Gallery",
                    "Pages",
                    "WpfGallery",
                    "DesignGuidance",
                    page);
                var normalizedXaml = xaml.Replace("\r\n", "\n").Replace('\r', '\n');
                StringAssert.Contains(
                    xaml,
                    "xmlns:local=\"clr-namespace:ModernWpf.Gallery.Pages.WpfGallery.DesignGuidance\"");
                AssertContainsInOrder(
                    xaml,
                    page == "TypographyPage.xaml"
                        ? "<Page x:Class=\"ModernWpf.Gallery.Pages.WpfGallery.DesignGuidance.TypographyPage\""
                        : "<Page x:Class=\"ModernWpf.Gallery.Pages.WpfGallery.DesignGuidance.SpacingPage\"",
                    "xmlns:mc=\"http://schemas.openxmlformats.org/markup-compatibility/2006\"",
                    "xmlns:d=\"http://schemas.microsoft.com/expression/blend/2008\"",
                    "xmlns:local=\"clr-namespace:ModernWpf.Gallery.Pages.WpfGallery.DesignGuidance\"",
                    "mc:Ignorable=\"d\" Foreground=\"{DynamicResource TextFillColorPrimaryBrush}\"",
                    "xmlns:controls=\"clr-namespace:ModernWpf.Gallery.Controls\"",
                    "d:DesignHeight=\"450\" d:DesignWidth=\"800\"",
                    page == "TypographyPage.xaml"
                        ? "Title=\"TypographyPage\">"
                        : "Title=\"SpacingPage\">");
                Assert.IsFalse(
                    xaml.Contains("x:Name=\"ContentPagePane\"", StringComparison.Ordinal),
                    page + " should keep the official unnamed root Grid shape.");
                StringAssert.Contains(
                    normalizedXaml,
                    "<Grid>\n        <Grid.RowDefinitions>");
                StringAssert.Contains(
                    normalizedXaml,
                    "</Grid.RowDefinitions>\n\n        <controls:PageHeader");
                StringAssert.Contains(
                    xaml,
                    "<controls:PageHeader Margin=\"0,0,0,32\" Title=\"{Binding ViewModel.PageTitle}\" Description=\"{Binding ViewModel.PageDescription}\" />");
                StringAssert.Contains(
                    xaml,
                    "<ScrollViewer Margin=\"0,0,0,24\" Padding=\"0,0,24,0\" HorizontalScrollBarVisibility=\"Auto\" Grid.Row=\"1\">");
                if (page == "SpacingPage.xaml")
                {
                    AssertContainsInOrder(
                        xaml,
                        "x:Name=\"CardImage\"",
                        "Source=\"/Assets/Design/Cards.dark.png\"",
                        "AutomationProperties.Name=\"Example of spacing in a page with cards layout\"");
                    AssertContainsInOrder(
                        xaml,
                        "x:Name=\"DialogImage\"",
                        "Source=\"/Assets/Design/Dialog.dark.png\"",
                        "AutomationProperties.Name=\"Example of spacing in a form layout\"");
                }
            }

            var geometryXaml = ReadRepoFile(
                "ModernWpf.Gallery",
                "Pages",
                "WpfGallery",
                "DesignGuidance",
                "GeometryPage.xaml");
            var normalizedGeometryXaml = geometryXaml.Replace("\r\n", "\n").Replace('\r', '\n');
            StringAssert.Contains(
                geometryXaml,
                "xmlns:local=\"clr-namespace:ModernWpf.Gallery.Pages.WpfGallery.DesignGuidance\"");
            AssertContainsInOrder(
                geometryXaml,
                "<Page x:Class=\"ModernWpf.Gallery.Pages.WpfGallery.DesignGuidance.GeometryPage\"",
                "xmlns:mc=\"http://schemas.openxmlformats.org/markup-compatibility/2006\"",
                "xmlns:d=\"http://schemas.microsoft.com/expression/blend/2008\"",
                "xmlns:local=\"clr-namespace:ModernWpf.Gallery.Pages.WpfGallery.DesignGuidance\"",
                "mc:Ignorable=\"d\" Foreground=\"{DynamicResource TextFillColorPrimaryBrush}\"",
                "xmlns:controls=\"clr-namespace:ModernWpf.Gallery.Controls\"",
                "d:DesignHeight=\"450\" d:DesignWidth=\"800\"",
                "Title=\"GeometryPage\">");
            Assert.IsFalse(
                geometryXaml.Contains("x:Name=\"ContentPagePane\"", StringComparison.Ordinal),
                "GeometryPage.xaml should keep the official unnamed root Grid shape.");
            StringAssert.Contains(
                normalizedGeometryXaml,
                "<Grid>\n        <Grid.RowDefinitions>");
            StringAssert.Contains(
                normalizedGeometryXaml,
                "</Grid.RowDefinitions>\n\n        <controls:PageHeader");
            StringAssert.Contains(
                geometryXaml,
                "<controls:PageHeader Margin=\"0,0,0,32\" Title=\"{Binding ViewModel.PageTitle}\" ShowDescription=\"False\"/>");
            StringAssert.Contains(
                geometryXaml,
                "<ScrollViewer Margin=\"0,0,0,24\" Padding=\"0,0,24,0\" HorizontalScrollBarVisibility=\"Auto\" Grid.Row=\"1\">");
            StringAssert.Contains(
                geometryXaml,
                "<Border Height=\"300\" Width=\"500\" HorizontalAlignment=\"Left\">");
            AssertContainsInOrder(
                geometryXaml,
                "x:Name=\"GeometryImage\"",
                "Source=\"/Assets/Design/Geometry.dark.png\"",
                "AutomationProperties.Name=\"Example of corner radius.\"");

            var iconographyXaml = ReadRepoFile(
                "ModernWpf.Gallery",
                "Pages",
                "WpfGallery",
                "DesignGuidance",
                "IconsPage.xaml");
            var normalizedIconographyXaml = iconographyXaml.Replace("\r\n", "\n").Replace('\r', '\n');
            AssertContainsInOrder(
                iconographyXaml,
                "<Page x:Class=\"ModernWpf.Gallery.Pages.WpfGallery.DesignGuidance.IconsPage\"",
                "xmlns:d=\"http://schemas.microsoft.com/expression/blend/2008\"",
                "xmlns:x=\"http://schemas.microsoft.com/winfx/2006/xaml\"",
                "xmlns:i=\"http://schemas.microsoft.com/xaml/behaviors\"",
                "xmlns:mc=\"http://schemas.openxmlformats.org/markup-compatibility/2006\"",
                "xmlns:controls=\"clr-namespace:ModernWpf.Gallery.Controls\"",
                "mc:Ignorable=\"d\"",
                "Foreground=\"{DynamicResource TextFillColorPrimaryBrush}\"",
                "d:DesignHeight=\"450\" d:DesignWidth=\"800\"",
                "d:Background=\"White\"",
                "Title=\"IconsPage\">");
            AssertContainsInOrder(
                iconographyXaml,
                "<i:Interaction.Triggers>",
                "<i:EventTrigger EventName=\"Loaded\">",
                "<i:InvokeCommandAction Command=\"{Binding ViewModel.LoadDataCommand}\" />",
                "</i:EventTrigger>",
                "</i:Interaction.Triggers>",
                "<Page.Resources>");
            Assert.IsFalse(
                iconographyXaml.Contains("xmlns:local=\"clr-namespace:ModernWpf.Gallery.Pages.WpfGallery.DesignGuidance\"", StringComparison.Ordinal),
                "IconsPage.xaml should keep the current official root namespace shape, which has no local namespace declaration.");
            StringAssert.Contains(
                iconographyXaml,
                "<controls:PageHeader Margin=\"2,0,0,32\" Title=\"{Binding ViewModel.PageTitle}\" Description=\"{Binding ViewModel.PageDescription}\" />");
            AssertContainsInOrder(
                iconographyXaml,
                "<Style x:Key=\"CodeValuePresenterStyle\" TargetType=\"TextBlock\">",
                "<Setter Property=\"Padding\" Value=\"0 0 0 4\" />",
                "<Setter Property=\"Opacity\" Value=\"0.7\"/>",
                "<Setter Property=\"FontSize\" Value=\"14\"/>",
                "<Style x:Key=\"IconData\" TargetType=\"{x:Type ContentControl}\">",
                "<Setter Property=\"Focusable\" Value=\"False\"/>",
                "<Grid >",
                "<TextBlock Padding=\"0,6\" Grid.Column=\"0\" VerticalAlignment=\"Center\" Text=\"{TemplateBinding Content}\" Style=\"{StaticResource CodeValuePresenterStyle}\" TextWrapping=\"Wrap\"/>");
            AssertContainsInOrder(
                iconographyXaml,
                "<Button Grid.Column=\"1\"",
                "Padding=\"8\"",
                "FocusManager.IsFocusScope=\"True\"",
                "Command=\"ApplicationCommands.Copy\"",
                "AutomationProperties.Name=\"{Binding Tag, StringFormat='{}Copy {0} to clipboard', RelativeSource={RelativeSource Mode=TemplatedParent}}\"",
                "ToolTipService.ToolTip=\"Copy to clipboard\"",
                "CommandParameter=\"{TemplateBinding Content}\"",
                "CommandTarget=\"{Binding RelativeSource={RelativeSource AncestorType=Page}}\">");
            StringAssert.Contains(
                iconographyXaml,
                "<TextBlock x:Name=\"CopyGlyphTextBlock\" FontFamily=\"{StaticResource SymbolThemeFontFamily}\" Text=\"&#xE8C8;\"/>");
            StringAssert.Contains(
                iconographyXaml,
                "<TextBlock x:Name=\"SuccessGlyphTextBlock\" FontFamily=\"{StaticResource SymbolThemeFontFamily}\" Text=\"&#xE73E;\" Opacity=\"0\" />");
            StringAssert.Contains(
                normalizedIconographyXaml,
                "                                                    <DoubleAnimation Duration=\"0:0:0.333\" Storyboard.TargetName=\"CopyGlyphTextBlock\" Storyboard.TargetProperty=\"Opacity\" To=\"0\" />\n                                                    <DoubleAnimation Duration=\"0:0:0.666\" BeginTime=\"0:0:0.333\" Storyboard.TargetName=\"SuccessGlyphTextBlock\" Storyboard.TargetProperty=\"Opacity\" To=\"1\" />\n                                                    <DoubleAnimation Storyboard.TargetName=\"SuccessGlyphTextBlock\" BeginTime=\"0:0:2\" Duration=\"0:0:0.01\" Storyboard.TargetProperty=\"Opacity\" To=\"0\" />\n                                                    <DoubleAnimation Storyboard.TargetName=\"CopyGlyphTextBlock\" BeginTime=\"0:0:2.1\" Duration=\"0:0:0.01\" Storyboard.TargetProperty=\"Opacity\" To=\"1\" />");
            AssertContainsInOrder(
                iconographyXaml,
                "<Style x:Key=\"IconTagChipButtonStyle\" TargetType=\"Button\" BasedOn=\"{StaticResource DefaultButtonStyle}\">",
                "<Style x:Key=\"IconsListViewItemFocusVisualStyle\">",
                "<Rectangle",
                "RadiusX=\"4\"",
                "RadiusY=\"4\"",
                "Margin=\"5\"",
                "Stroke=\"{DynamicResource KeyboardFocusBorderColorBrush}\"",
                "StrokeThickness=\"2\" />",
                "<Style x:Key=\"PaginationButtonStyle\" TargetType=\"Button\" BasedOn=\"{StaticResource DefaultButtonStyle}\">",
                "<Border x:Name=\"ContentBorder\"",
                "<ContentPresenter x:Name=\"ContentPresenter\"",
                "RecognizesAccessKey=\"True\"",
                "HorizontalAlignment=\"{TemplateBinding HorizontalContentAlignment}\"",
                "VerticalAlignment=\"{TemplateBinding VerticalContentAlignment}\"",
                "Content=\"{TemplateBinding Content}\"",
                "ContentTemplate=\"{TemplateBinding ContentTemplate}\"");
            StringAssert.Contains(
                normalizedIconographyXaml,
                "<Grid Margin=\"0 0 0 10\">\n        <Grid.RowDefinitions>");
            AssertContainsInOrder(
                iconographyXaml,
                "<Expander Grid.Row=\"1\"",
                "Header=\"Instructions on how to use Fluent icons\"",
                "IsExpanded=\"False\"",
                "Margin=\"2 -8 0 0\">");
            AssertContainsInOrder(
                normalizedIconographyXaml,
                "<Run FontWeight=\"SemiBold\">\n                Use the theme font resource\n            </Run>",
                "<LineBreak />",
                "Use {StaticResource SymbolThemeFontFamily} instead of hard-coding a font family.",
                "<LineBreak/>",
                "<LineBreak/>",
                "<Span FontWeight=\"SemiBold\">\n                How to use the font\n            </Span>",
                "<LineBreak/>",
                "For optimal appearance, use these specific sizes: 16, 20, 24, 32, 40, 48, and 64.",
                "<LineBreak/>",
                "Fluent icon glyphs use predictable metrics, so layering and colorization effects can be achieved by drawing glyphs directly on top of each other.",
                "<LineBreak/>",
                "<LineBreak/>",
                "<Run FontWeight=\"SemiBold\">\n                XAML\n            </Run>",
                "<LineBreak/>",
                "<Span>\n                &lt;Grid&gt;\n            </Span>",
                "<LineBreak/>",
                "<Span>\n                &lt;TextBlock FontFamily=\"{StaticResource SymbolThemeFontFamily}\" Text=\"&amp;#xEB51;\" Foreground=\"#C72335\"/&gt;\n            </Span>",
                "<LineBreak/>",
                "<Span>\n                &#x09;&lt;TextBlock FontFamily=\"{StaticResource SymbolThemeFontFamily}\" Text=\"&amp;#xEB51;\" /&gt;\n            </Span>",
                "<LineBreak/>",
                "<Span>\n                &lt;/Grid&gt;\n            </Span>");
            AssertContainsInOrder(
                iconographyXaml,
                "<TextBox x:Name=\"IconsSearchBox\" Text=\"{Binding ViewModel.SearchText, UpdateSourceTrigger=PropertyChanged, Delay=500}\"",
                "AutomationProperties.Name=\"Search Icons by Name, Tag\"",
                "Width=\"500\"",
                "HorizontalAlignment=\"Left\"",
                "VerticalAlignment=\"Center\"",
                "GotKeyboardFocus=\"IconsSearchBox_GotKeyboardFocus\"",
                "LostKeyboardFocus=\"IconsSearchBox_LostKeyboardFocus\"",
                "TextChanged=\"IconsSearchBox_TextChanged\"/>");
            StringAssert.Contains(
                iconographyXaml,
                "<TextBlock Grid.Row=\"2\" Style=\"{StaticResource BodyStrongTextBlockStyle}\" Text=\"Fluent Icons Library\" Margin=\"2,24,0,10\" />");
            StringAssert.Contains(
                normalizedIconographyXaml,
                "<TextBlock x:Name=\"IconsSearchBoxPlaceholder\" VerticalAlignment=\"Center\" Style=\"{StaticResource BodyTextBlockStyle}\" Text=\"Search Icons by Name, Tag\" Margin=\"14,0,0,0\"\n                       IsHitTestVisible=\"False\" Foreground=\"{DynamicResource TextFillColorSecondaryBrush}\"/>");
            StringAssert.Contains(
                normalizedIconographyXaml,
                "<Grid Grid.Row=\"4\" Margin=\"2 10 2 10\">\n            <Grid.ColumnDefinitions>\n                <ColumnDefinition Width=\"*\"/>\n                <ColumnDefinition Width=\"300\"/>\n            </Grid.ColumnDefinitions>\n\n            <Border CornerRadius=\"8 0 0 8\" Background=\"{DynamicResource SubtleFillColorSecondaryBrush}\" Grid.Column=\"0\"/>");
            AssertContainsInOrder(
                iconographyXaml,
                "<ListView AutomationProperties.Name=\"Icons\" ItemsSource=\"{Binding ViewModel.DisplayedIcons}\" ScrollViewer.HorizontalScrollBarVisibility=\"Disabled\" ScrollViewer.VerticalScrollBarVisibility=\"Visible\" Padding=\"4\" SelectedItem=\"{Binding ViewModel.SelectedIcon}\" SelectionMode=\"Single\" >",
                "<WrapPanel Orientation=\"Horizontal\" Margin=\"10\" HorizontalAlignment=\"Stretch\" VerticalAlignment=\"Stretch\"/>",
                "<Style TargetType=\"ListViewItem\" BasedOn=\"{StaticResource DefaultListViewItemStyle}\">",
                "<Setter Property=\"AutomationProperties.Name\" Value=\"{Binding Name, Mode=OneWay}\"/>",
                "<Border BorderThickness=\"4\" CornerRadius=\"8\" Background=\"{DynamicResource ButtonBackground}\">",
                "<Grid Width=\"96\" Height=\"96\" ToolTip=\"{Binding Name}\" >",
                "<TextBlock Focusable=\"False\" Grid.Row=\"0\" FontFamily=\"{StaticResource SymbolThemeFontFamily}\" Text=\"{Binding Character}\" AutomationProperties.Name=\"{Binding Name, StringFormat='{}{0} Icon'}\" FontSize=\"28\" Width=\"28\" Height=\"28\" VerticalAlignment=\"Center\" HorizontalAlignment=\"Center\"/>",
                "<TextBlock Focusable=\"False\" Grid.Row=\"1\" Text=\"{Binding Name}\" Style=\"{StaticResource CaptionTextBlockStyle}\" HorizontalAlignment=\"Center\" VerticalAlignment=\"Bottom\" Foreground=\"{DynamicResource TextFillColorPrimaryBrush}\" TextTrimming=\"CharacterEllipsis\" TextWrapping=\"NoWrap\" Margin=\"6,-4,6,8\"/>");
            AssertContainsInOrder(
                iconographyXaml,
                "<Grid Grid.Column=\"1\" Grid.Row=\"0\" Background=\"{DynamicResource ButtonBackground}\">",
                "<StackPanel Orientation=\"Vertical\" Margin=\"16\">",
                "<TextBlock Text=\"{Binding ViewModel.SelectedIcon.Name}\" Style=\"{StaticResource SubtitleTextBlockStyle}\" VerticalAlignment=\"Center\"/>",
                "<TextBlock Text=\"{Binding ViewModel.SelectedIcon.Character}\" FontFamily=\"{StaticResource SymbolThemeFontFamily}\" FontSize=\"50\" Margin=\"0,12,0,32\"",
                "AutomationProperties.Name=\"{Binding ViewModel.SelectedIcon.Name, StringFormat='{}{0} Icon'}\" HorizontalAlignment=\"Left\" VerticalAlignment=\"Center\"/>",
                "<TextBlock Text=\"Icon Name\"/>",
                "<ContentControl Style=\"{StaticResource IconData}\" Content=\"{Binding ViewModel.SelectedIcon.Name}\" Tag=\"Icon Name\"/>",
                "<TextBlock Text=\"XAML\"/>",
                "<TextBlock x:Name=\"XAMLCode\" Text=\"{Binding ViewModel.SelectedIcon.TextGlyph, StringFormat='&lt;TextBlock FontFamily=&#x22;{{StaticResource SymbolThemeFontFamily}}&#x22; Text=&#x22;{0}&#x22;/&gt;'}\" Visibility=\"Collapsed\"/>",
                "<ContentControl Style=\"{StaticResource IconData}\" Content=\"{Binding ElementName=XAMLCode, Path=Text}\" Tag=\"XAML Code\"/>");
            AssertContainsInOrder(
                iconographyXaml,
                "<Grid",
                "Grid.Column=\"1\"",
                "Grid.Row=\"0\"",
                "Background=\"{DynamicResource ButtonBackground}\"",
                "<ItemsControl",
                "x:Name=\"TagsItemsControl\"",
                "ItemsSource=\"{Binding ViewModel.SelectedIcon.Tags}\"",
                "Margin=\"0,0,0,12\"",
                "Visibility=\"{Binding RelativeSource={RelativeSource Self}, Path=HasItems, Converter={StaticResource BooleanToVisibilityConverter}}\"",
                "AutomationProperties.Name=\"Selected Icon Tags\"",
                "<Button",
                "Style=\"{StaticResource IconTagChipButtonStyle}\"",
                "Command=\"{Binding ViewModel.ApplyTagFilterCommand, RelativeSource={RelativeSource AncestorType=Page}}\"",
                "AutomationProperties.Name=\"{Binding}\"",
                "CommandParameter=\"{Binding}\"",
                "<TextBlock",
                "Text=\"{Binding}\"",
                "Style=\"{StaticResource CaptionTextBlockStyle}\"",
                "Foreground=\"{DynamicResource TextFillColorPrimaryBrush}\"");
            AssertContainsInOrder(
                iconographyXaml,
                "<Grid Grid.Row=\"5\">",
                "<StackPanel",
                "Margin=\"0,0,0,0\"",
                "Orientation=\"Horizontal\"",
                "HorizontalAlignment=\"Left\"",
                "<Button",
                "Style=\"{StaticResource PaginationButtonStyle}\"",
                "Command=\"{Binding ViewModel.PreviousPageCommand}\"",
                "Margin=\"0,0,8,0\"",
                "Padding=\"8\"",
                "ToolTip=\"Previous Page\"",
                "AutomationProperties.Name=\"Previous Page\"",
                "<TextBlock",
                "FontFamily=\"{StaticResource SymbolThemeFontFamily}\"",
                "Text=\"&#xF08D;\"",
                "FontSize=\"12\"",
                "<Button",
                "Style=\"{StaticResource PaginationButtonStyle}\"",
                "Command=\"{Binding ViewModel.NextPageCommand}\"",
                "Padding=\"8\"",
                "ToolTip=\"Next Page\"",
                "AutomationProperties.Name=\"Next Page\"",
                "<StackPanel Orientation=\"Horizontal\" Grid.Column=\"1\">",
                "<TextBlock",
                "Style=\"{StaticResource BodyTextBlockStyle}\"",
                "Text=\"Icons per page\"",
                "Margin=\"10,0,0,0\"",
                "VerticalAlignment=\"Center\"",
                "<ComboBox",
                "ItemsSource=\"{Binding ViewModel.PageSizeOptions}\"",
                "SelectedIndex=\"{Binding ViewModel.SelectedPageSizeIndex}\"",
                "AutomationProperties.Name=\"Icons per page\"",
                "Margin=\"10,0,0,0\"");
            AssertContainsInOrder(
                iconographyXaml,
                "<StackPanel Margin=\"0,0,0,0\" Orientation=\"Horizontal\" HorizontalAlignment=\"Left\">",
                "<Button Style=\"{StaticResource PaginationButtonStyle}\" Command=\"{Binding ViewModel.PreviousPageCommand}\" Margin=\"0,0,8,0\" Padding=\"8\" ToolTip=\"Previous Page\"",
                "AutomationProperties.Name=\"Previous Page\">",
                "<TextBlock FontFamily=\"{StaticResource SymbolThemeFontFamily}\" Text=\"&#xF08D;\" FontSize=\"12\"/>",
                "<TextBlock Text=\"{Binding ViewModel.CurrentPage, StringFormat='Page {0} of'}\" VerticalAlignment=\"Center\" Margin=\"0,0,4,0\"/>",
                "<TextBlock Text=\"{Binding ViewModel.TotalPages}\" VerticalAlignment=\"Center\" Margin=\"0,0,8,0\"/>",
                "<Button Style=\"{StaticResource PaginationButtonStyle}\" Command=\"{Binding ViewModel.NextPageCommand}\" Padding=\"8\" ToolTip=\"Next Page\"",
                "AutomationProperties.Name=\"Next Page\">",
                "<TextBlock FontFamily=\"{StaticResource SymbolThemeFontFamily}\" Text=\"&#xF08F;\" FontSize=\"12\"/>",
                "<StackPanel Orientation=\"Horizontal\" Grid.Column=\"1\">",
                "<TextBlock Style=\"{StaticResource BodyTextBlockStyle}\" Text=\"Icons per page\" Margin=\"10,0,0,0\"",
                "VerticalAlignment=\"Center\"/>",
                "<ComboBox ItemsSource=\"{Binding ViewModel.PageSizeOptions}\"",
                "SelectedIndex=\"{Binding ViewModel.SelectedPageSizeIndex}\"",
                "AutomationProperties.Name=\"Icons per page\" Margin=\"10,0,0,0\"/>");
        }

        [TestMethod]
        public void DesignGuidanceColorCodeBehindKeepsOfficialConstructorAndHandlerOrderShape()
        {
            var source = ReadRepoFile(
                "ModernWpf.Gallery",
                "Pages",
                "WpfGallery",
                "DesignGuidance",
                "ColorsPage.xaml.cs").Replace("\r\n", "\n").Replace('\r', '\n');

            StringAssert.Contains(
                source,
                "        public ColorsPageViewModel ViewModel { get; }\n        public ColorsPage(ColorsPageViewModel viewModel)");
            AssertContainsInOrder(
                source,
                "public ColorsPage(ColorsPageViewModel viewModel)",
                "InitializeComponent();",
                "ViewModel = viewModel;",
                "DataContext = this;",
                "private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)",
                "var section = WpfGalleryColorSectionFactory.Create(PageSelector.SelectedIndex);",
                "ColorSubpageNavigationFrame.Navigate(section);",
                "private void OnLoaded(object sender, RoutedEventArgs e)",
                "PageSelector.SelectedItem = ResolveInitialSubpage();",
                "private object ResolveInitialSubpage()");
        }

        [TestMethod]
        public void DesignGuidanceDesignImageCodeBehindKeepsOfficialUserPreferenceHandlerShape()
        {
            foreach (var page in new[]
            {
                Tuple.Create("SpacingPage", "SpacingPageViewModel"),
                Tuple.Create("GeometryPage", "GeometryPageViewModel")
            })
            {
                var source = ReadRepoFile(
                    "ModernWpf.Gallery",
                    "Pages",
                    "WpfGallery",
                    "DesignGuidance",
                    page.Item1 + ".xaml.cs").Replace("\r\n", "\n").Replace('\r', '\n');

                AssertContainsInOrder(
                    source,
                    "public " + page.Item2 + " ViewModel { get; }",
                    "public " + page.Item1 + "(" + page.Item2 + " viewModel)",
                    "InitializeComponent();",
                    "UpdateImageResources();",
                    "ViewModel = viewModel;",
                    "DataContext = this;",
                    "Loaded += OnLoaded;",
                    "SystemEvents.UserPreferenceChanged += SystemEvents_UserPreferenceChanged;",
                    "ThemeManager.AddActualThemeChangedHandler(this, OnActualThemeChanged);",
                    "Unloaded += OnUnloaded;",
                    "private void OnLoaded(object sender, RoutedEventArgs e)",
                    "UpdateImageResources();",
                    "private void SystemEvents_UserPreferenceChanged(object sender, UserPreferenceChangedEventArgs e)",
                    "Dispatcher.Invoke(() =>\n            {\n                UpdateImageResources();\n            });",
                    "private void OnActualThemeChanged(object sender, RoutedEventArgs e)",
                    "UpdateImageResources();",
                    "private void OnUnloaded(object sender, RoutedEventArgs e)",
                    "Loaded -= OnLoaded;",
                    "SystemEvents.UserPreferenceChanged -= SystemEvents_UserPreferenceChanged;",
                    "ThemeManager.RemoveActualThemeChangedHandler(this, OnActualThemeChanged);",
                    "Unloaded -= OnUnloaded;");
            }
        }

        [TestMethod]
        public void DesignGuidanceColorSubsectionRootsKeepOfficialSourceShape()
        {
            foreach (var section in new[]
            {
                "Text",
                "Fill",
                "Stroke",
                "Background",
                "Signal",
                "HighContrast"
            })
            {
                var sectionName = section + "Section";
                var xaml = ReadRepoFile(
                    "ModernWpf.Gallery",
                    "Pages",
                    "WpfGallery",
                    "DesignGuidance",
                    sectionName + ".xaml");

                AssertContainsInOrder(
                    xaml,
                    "<Page x:Class=\"ModernWpf.Gallery.Pages.WpfGallery.DesignGuidance." + sectionName + "\"",
                    "xmlns:mc=\"http://schemas.openxmlformats.org/markup-compatibility/2006\"",
                    "xmlns:d=\"http://schemas.microsoft.com/expression/blend/2008\"",
                    "xmlns:local=\"clr-namespace:ModernWpf.Gallery.Pages.WpfGallery.DesignGuidance\"",
                    "xmlns:controls=\"clr-namespace:ModernWpf.Gallery.Controls\"",
                    "mc:Ignorable=\"d\"",
                    "d:DesignHeight=\"450\" d:DesignWidth=\"800\"");

                if (section == "Background" || section == "Signal")
                {
                    AssertContainsInOrder(
                        xaml,
                        "Foreground=\"{DynamicResource WindowForeground}\"",
                        "Title=\"" + sectionName + "\">");
                }
                else
                {
                    AssertContainsInOrder(
                        xaml,
                        "Title=\"" + sectionName + "\"",
                        "Foreground=\"{DynamicResource WindowForeground}\">");
                }
            }
        }

        [TestMethod]
        public void DesignGuidanceColorTextSectionKeepsOfficialSourceShape()
        {
            var xaml = ReadRepoFile(
                "ModernWpf.Gallery",
                "Pages",
                "WpfGallery",
                "DesignGuidance",
                "TextSection.xaml");

            AssertContainsInOrder(
                xaml,
                "<Page x:Class=\"ModernWpf.Gallery.Pages.WpfGallery.DesignGuidance.TextSection\"",
                "xmlns:mc=\"http://schemas.openxmlformats.org/markup-compatibility/2006\"",
                "xmlns:d=\"http://schemas.microsoft.com/expression/blend/2008\"",
                "xmlns:local=\"clr-namespace:ModernWpf.Gallery.Pages.WpfGallery.DesignGuidance\"",
                "xmlns:controls=\"clr-namespace:ModernWpf.Gallery.Controls\"",
                "mc:Ignorable=\"d\"",
                "d:DesignHeight=\"450\" d:DesignWidth=\"800\"",
                "Title=\"TextSection\"",
                "Foreground=\"{DynamicResource WindowForeground}\">");
            StringAssert.Contains(xaml, "<!--  Colors section  -->");
            AssertContainsInOrder(
                xaml,
                "<controls:ColorPageExample Title=\"Text\" Description=\"For UI labels and static text\">",
                "<TextBlock",
                "FontSize=\"42\"",
                "FontWeight=\"SemiBold\"",
                "Text=\"Aa\" />");
            AssertContainsInOrder(
                xaml,
                "<Border Style=\"{StaticResource ColorTilesPanelStyle}\" Margin=\"0,8\">",
                "<controls:ColorTile",
                "Background=\"{DynamicResource TextFillColorPrimaryBrush}\"",
                "ColorBrushName=\"TextFillColorPrimaryBrush\"",
                "TileRadius=\"8,0,0,8\"",
                "ColorExplanation=\"Rest or Hover\"",
                "ColorName=\"Text / Primary\"",
                "ColorValue=\"#000000 (E4, 89.56%)\"",
                "Foreground=\"{DynamicResource TextOnAccentFillColorPrimaryBrush}\"",
                "ShowSeparator=\"False\" />");
            AssertContainsInOrder(
                xaml,
                "<!--  Accent text  -->",
                "<Border Style=\"{StaticResource ColorTilesPanelStyle}\" Margin=\"0,8\">",
                "Background=\"{DynamicResource AccentTextFillColorPrimaryBrush}\"",
                "ColorBrushName=\"AccentTextFillColorPrimaryBrush\"",
                "ColorExplanation=\"Rest or Hover\"",
                "TileRadius=\"8,0,0,8\"",
                "ColorName=\"Accent Text / Primary\"");
            AssertContainsInOrder(
                xaml,
                "<!--  Text on accent  -->",
                "<Border Style=\"{StaticResource ColorTilesPanelStyle}\" Margin=\"0,8\">",
                "<controls:ColorTile",
                "Background=\"{DynamicResource TextOnAccentFillColorPrimaryBrush}\"",
                "ColorBrushName=\"TextOnAccentFillColorPrimaryBrush\"",
                "TileRadius=\"8,0,0,8\"",
                "ColorExplanation=\"Rest or Hover\"",
                "ColorName=\"Text on Accent / Primary\"",
                "Foreground=\"{DynamicResource TextFillColorPrimaryBrush}\" />",
                "<controls:ColorTile",
                "Grid.Column=\"2\"",
                "Background=\"{DynamicResource TextOnAccentFillColorSecondaryBrush}\"",
                "TileRadius=\"0,8,8,0\"",
                "ColorBrushName=\"TextOnAccentFillColorSecondaryBrush\"");
            AssertContainsInOrder(
                xaml,
                "<Border Style=\"{StaticResource ColorTilesPanelStyle}\" Margin=\"0,8,0,0\">",
                "Background=\"{DynamicResource TextOnAccentFillColorDisabledBrush}\"",
                "ColorBrushName=\"TextOnAccentFillColorDisabledBrush\"",
                "TileRadius=\"8,0,0,8\"",
                "ColorExplanation=\"Disabled only (not accessible)\"",
                "Background=\"{DynamicResource TextOnAccentFillColorSelectedTextBrush}\"",
                "TileRadius=\"0,8,8,0\"",
                "ColorBrushName=\"TextOnAccentFillColorSelectedTextBrush\"");
        }

        [TestMethod]
        public void SamplesPageKeepsOfficialUserDashboardSourceShape()
        {
            var xaml = ReadRepoFile(
                "ModernWpf.Gallery",
                "Pages",
                "WpfGallery",
                "Samples",
                "UserDashboardPage.xaml");
            var normalizedXaml = xaml.Replace("\r\n", "\n").Replace('\r', '\n');

            AssertContainsInOrder(
                xaml,
                "d:DesignHeight=\"450\"",
                "d:DesignWidth=\"800\"",
                "d:DataContext=\"{d:DesignInstance Type=samples:UserDashboardPage}\"",
                "Foreground=\"{DynamicResource TextFillColorPrimaryBrush}\"",
                "mc:Ignorable=\"d\"",
                "SizeChanged=\"Page_SizeChanged\"");
            StringAssert.Contains(
                xaml,
                "<Style TargetType=\"Label\" x:Key=\"GenericLabelStyle\">");
            StringAssert.Contains(
                xaml,
                "<Setter Property=\"Opacity\" Value=\"0.67\"/>");
            StringAssert.Contains(
                xaml,
                "<RowDefinition Height=\"*\" MaxHeight=\"280\"/>");
            StringAssert.Contains(
                xaml,
                "<Grid x:Name=\"UserListGrid\" Grid.Column=\"0\" Grid.RowSpan=\"2\" >");
            AssertContainsInOrder(
                xaml,
                "<ListView",
                "x:Name=\"UserList\"",
                "AutomationProperties.Name=\"Users\"",
                "Grid.Row=\"0\"",
                "Width=\"300\"",
                "Background=\"{DynamicResource CardBackgroundFillColorDefaultBrush}\"",
                "ItemsSource=\"{Binding ViewModel.Users, Mode=TwoWay}\"",
                "SelectedItem=\"{Binding ViewModel.SelectedUser, Mode=TwoWay}\"",
                "SelectionMode=\"Single\">");
            StringAssert.Contains(
                xaml,
                "<Style TargetType=\"ListViewItem\" BasedOn=\"{StaticResource DefaultListViewItemStyle}\">");
            StringAssert.Contains(
                xaml,
                "<Setter Property=\"AutomationProperties.Name\" Value=\"{Binding Name, Mode=OneWay}\"/>");
            StringAssert.Contains(
                normalizedXaml,
                "            </ListView>\n            <Button\n                x:Name=\"NewUserButton\"");
            AssertContainsInOrder(
                xaml,
                "Margin=\"12,6,0,0\"",
                "Text=\"{Binding Name, Mode=OneWay}\"",
                "pages:GalleryAutomation.HeadingLevel=\"Level3\" />");
            StringAssert.Contains(
                normalizedXaml,
                "                Visibility=\"{Binding ViewModel.SelectedUser, Converter={StaticResource NullToVisibilityConverter}}\">\n                    <Ellipse\n                      x:Name=\"UserAvatar\"\n                      Width=\"96\"\n                      Height=\"96\"\n                      Margin=\"12\"\n                      HorizontalAlignment=\"Center\"\n                      VerticalAlignment=\"Center\">");
            StringAssert.Contains(
                normalizedXaml,
                "                    <StackPanel x:Name=\"UserDetailHeaderPanel\" Orientation=\"Vertical\" VerticalAlignment=\"Center\">\n                        <TextBlock\n                          x:Name=\"UserDetailHeaderNameBox\"\n                          FontSize=\"24\" Text=\"{Binding ViewModel.SelectedUser.Name}\" />\n                        <TextBlock\n                          x:Name=\"UserDetailHeaderCompanyBox\"\n                          FontSize=\"16\" Text=\"{Binding ViewModel.SelectedUser.Company}\" />\n                    </StackPanel>\n            </StackPanel>");
            StringAssert.Contains(
                xaml,
                "<StackPanel Margin=\"20,0,20,0\" >");
            StringAssert.Contains(
                xaml,
                "<Label Content=\"First Name\" Style=\"{StaticResource GenericLabelStyle}\" FontWeight=\"SemiBold\" />");
            StringAssert.Contains(
                xaml,
                "<TextBox AutomationProperties.Name=\"First Name\" Margin=\"0,5,0,15\" Text=\"{Binding ViewModel.EditableUser.FirstName}\" IsReadOnly=\"{Binding ViewModel.IsReadOnly}\"/>");
            StringAssert.Contains(
                xaml,
                "<TextBlock x:Name=\"AgeIndicatorBox\" Padding=\"10 0 0 0\" Grid.Column=\"1\" Text=\"{Binding ViewModel.EditableUser.Age}\" VerticalAlignment=\"Center\" />");
            AssertContainsInOrder(
                xaml,
                "<Slider",
                "AutomationProperties.Name=\"Age\"",
                "Maximum=\"62\"",
                "Minimum=\"21\"",
                "IsSnapToTickEnabled=\"True\"",
                "Value=\"{Binding ViewModel.EditableUser.Age}\"",
                "ValueChanged=\"AgeSlider_ValueChanged\"",
                "IsEnabled=\"{Binding ViewModel.IsEditing}\"/>");
            StringAssert.Contains(
                xaml,
                "<DatePicker AutomationProperties.Name=\"Date of Joining\" Margin=\"0,5,0,15\" SelectedDate=\"{Binding ViewModel.EditableUser.DateOfJoining}\" IsEnabled=\"{Binding ViewModel.IsEditing}\"/>");
            StringAssert.Contains(
                xaml,
                "<CheckBox AutomationProperties.Name=\"Is user a new graduate ?\" VerticalAlignment=\"Center\" IsChecked=\"{Binding ViewModel.EditableUser.IsNewGraduate}\" IsEnabled=\"{Binding ViewModel.IsEditing}\"/>");
            StringAssert.Contains(
                xaml,
                "<TextBlock FontSize=\"14\" HorizontalAlignment=\"Left\" VerticalAlignment=\"Center\" Visibility=\"{Binding ViewModel.IsSaved, Converter={StaticResource BooleanToVisibilityConverter}}\" FontStyle=\"Italic\">");
            StringAssert.Contains(
                xaml,
                "<TextBlock FontSize=\"14\" HorizontalAlignment=\"Left\" VerticalAlignment=\"Center\" Visibility=\"{Binding ViewModel.DeletedName, Converter={StaticResource EmptyToVisibilityConverter }}\" FontStyle=\"Italic\">");
            StringAssert.Contains(
                normalizedXaml,
                "                            <TextBlock FontSize=\"14\" HorizontalAlignment=\"Left\" VerticalAlignment=\"Center\" Visibility=\"{Binding ViewModel.DeletedName, Converter={StaticResource EmptyToVisibilityConverter }}\" FontStyle=\"Italic\">\n                                 <Run Text=\"User\" />\n                                 <Run Text=\"{Binding ViewModel.DeletedName, Mode=OneWay}\" />\n                                 <Run Text=\"Deleted!\" />\n                            </TextBlock>");
            AssertContainsInOrder(
                xaml,
                "x:Name=\"edit_button\"",
                "Margin=\"10\"",
                "Command=\"{Binding ViewModel.EditUserStartCommand}\"",
                "Visibility=\"{Binding ViewModel.IsReadOnly, Converter={StaticResource BooleanToVisibilityConverter}}\"",
                "Click=\"EditButton_Click\"",
                "Content=\"Edit\" />");
            AssertContainsInOrder(
                xaml,
                "x:Name=\"save_button\"",
                "Margin=\"10\"",
                "Command=\"{Binding ViewModel.EditUserCommitCommand}\"",
                "Visibility=\"{Binding ViewModel.IsEditing, Converter={StaticResource BooleanToVisibilityConverter}}\"",
                "Click=\"SaveButton_Click\"",
                "Content=\"Save\"/>");
            StringAssert.Contains(
                normalizedXaml,
                "                                Content=\"Cancel\" />\n\n                      </StackPanel>\n                    </StackPanel>\n                </ScrollViewer>");
        }

        [TestMethod]
        public void MenuPageKeepsOfficialMenuItemSourceShape()
        {
            var xaml = ReadRepoFile(
                "ModernWpf.Gallery",
                "Pages",
                "WpfGallery",
                "Navigation",
                "MenuPage.xaml");
            var normalizedXaml = xaml.Replace("\r\n", "\n").Replace('\r', '\n');
            StringAssert.Contains(
                xaml,
                "xmlns:local=\"clr-namespace:ModernWpf.Gallery.Pages.WpfGallery.Navigation\"");
            StringAssert.Contains(
                normalizedXaml,
                "<Grid x:Name=\"ContentPagePane\" Height=\"Auto\">\n\n        <Grid.RowDefinitions>");
            StringAssert.Contains(
                normalizedXaml,
                "</Grid.RowDefinitions>\n        <controls:PageHeader");
            StringAssert.Contains(
                normalizedXaml,
                "<controls:PageHeader Margin=\"0,0,0,32\" Title=\"{Binding ViewModel.PageTitle}\" ShowDescription=\"False\" />\n\n        <ScrollViewer");
            StringAssert.Contains(
                xaml,
                "<controls:PageHeader Margin=\"0,0,0,32\" Title=\"{Binding ViewModel.PageTitle}\" ShowDescription=\"False\" />");
            StringAssert.Contains(
                xaml,
                "<ScrollViewer Grid.Row=\"1\" Margin=\"0,0,0,24\" Padding=\"0,0,24,0\">");
            StringAssert.Contains(
                xaml,
                "<Style TargetType=\"MenuItem\" BasedOn=\"{StaticResource DefaultMenuItemStyle}\">");
            StringAssert.Contains(
                xaml,
                "<EventSetter Event=\"Click\" Handler=\"MenuItem_Click\"/>");
            StringAssert.Contains(
                xaml,
                "<MenuItem AutomationProperties.Name=\"Bold\" Tag=\"Bold\" >");
            StringAssert.Contains(
                xaml,
                "<MenuItem AutomationProperties.Name=\"Italic\" Tag=\"Italic\" >");
            StringAssert.Contains(
                xaml,
                "<MenuItem AutomationProperties.Name=\"Underlined\" Tag=\"Underlined\" >");
            StringAssert.Contains(
                normalizedXaml,
                "<TextBlock\n                                    AutomationProperties.Name=\"Bold\"\n                                    Focusable=\"False\"\n                                    FontFamily=\"{StaticResource SymbolThemeFontFamily}\"\n                                    FontSize=\"12\"\n                                    Text=\"&#xE8DD;\" />");
            StringAssert.Contains(
                normalizedXaml,
                "<TextBlock\n                                    AutomationProperties.Name=\"Italic\"\n                                    Focusable=\"False\"\n                                    FontFamily=\"{StaticResource SymbolThemeFontFamily}\"\n                                    FontSize=\"12\"\n                                    Text=\"&#xE8DB;\" />");
            StringAssert.Contains(
                normalizedXaml,
                "<TextBlock\n                                    AutomationProperties.Name=\"Underlined\"\n                                    Focusable=\"False\"\n                                    FontFamily=\"{StaticResource SymbolThemeFontFamily}\"\n                                    FontSize=\"12\"\n                                    Text=\"&#xE8DC;\" />");

            var codeBehind = ReadRepoFile(
                "ModernWpf.Gallery",
                "Pages",
                "WpfGallery",
                "Navigation",
                "MenuPage.xaml.cs");
            StringAssert.Contains(
                codeBehind,
                "StatusMenuItem.Text = (menuItem.Tag != null) ? $\"You pressed {menuItem.Tag}\" : $\"You pressed {menuItem.Header}\";");
        }

        [TestMethod]
        public void NavigationSupportPagesKeepOfficialWindowLauncherSourceShape()
        {
            var frameXaml = ReadRepoFile(
                "ModernWpf.Gallery",
                "Pages",
                "WpfGallery",
                "Navigation",
                "FramePage.xaml");
            var normalizedFrameXaml = frameXaml.Replace("\r\n", "\n").Replace('\r', '\n');
            StringAssert.Contains(
                frameXaml,
                "xmlns:local=\"clr-namespace:ModernWpf.Gallery.Pages.WpfGallery.Navigation\"");
            AssertContainsInOrder(
                frameXaml,
                "<Page x:Class=\"ModernWpf.Gallery.Pages.WpfGallery.Navigation.FramePage\"",
                "xmlns:mc=\"http://schemas.openxmlformats.org/markup-compatibility/2006\"",
                "xmlns:d=\"http://schemas.microsoft.com/expression/blend/2008\"",
                "xmlns:local=\"clr-namespace:ModernWpf.Gallery.Pages.WpfGallery.Navigation\"",
                "xmlns:controls=\"clr-namespace:ModernWpf.Gallery.Controls\"",
                "mc:Ignorable=\"d\"",
                "d:DesignHeight=\"450\" d:DesignWidth=\"800\"",
                "Title=\"FramePage\"",
                "Foreground=\"{DynamicResource TextFillColorPrimaryBrush}\">");
            StringAssert.Contains(
                normalizedFrameXaml,
                "        <Grid x:Name=\"ContentPagePane\" Height=\"Auto\">\n            <Grid.RowDefinitions>");
            StringAssert.Contains(
                normalizedFrameXaml,
                "            </Grid.RowDefinitions>\n            <controls:PageHeader");
            StringAssert.Contains(
                normalizedFrameXaml,
                "<controls:PageHeader Margin=\"0,0,0,32\" Title=\"{Binding ViewModel.PageTitle}\" ShowDescription=\"False\" />\n            <ScrollViewer");
            StringAssert.Contains(
                frameXaml,
                "<controls:PageHeader Margin=\"0,0,0,32\" Title=\"{Binding ViewModel.PageTitle}\" ShowDescription=\"False\" />");
            StringAssert.Contains(
                frameXaml,
                "<ScrollViewer Grid.Row=\"1\" Margin=\"0,0,0,24\" Padding=\"0,0,24,0\">");
            AssertContainsInOrder(
                frameXaml,
                "<Button",
                "x:Name=\"OpenFrameWindow\"",
                "VerticalAlignment=\"Center\"",
                "HorizontalAlignment=\"Center\"",
                "Content=\"Open window to view Frame\"",
                "Click=\"OpenFrameWindow_Click\" />");

            var navigationWindowXaml = ReadRepoFile(
                "ModernWpf.Gallery",
                "Pages",
                "WpfGallery",
                "Navigation",
                "NavigationWindowPage.xaml");
            var normalizedNavigationWindowXaml = navigationWindowXaml.Replace("\r\n", "\n").Replace('\r', '\n');
            StringAssert.Contains(
                navigationWindowXaml,
                "xmlns:local=\"clr-namespace:ModernWpf.Gallery.Pages.WpfGallery.Navigation\"");
            AssertContainsInOrder(
                navigationWindowXaml,
                "<Page x:Class=\"ModernWpf.Gallery.Pages.WpfGallery.Navigation.NavigationWindowPage\"",
                "xmlns:mc=\"http://schemas.openxmlformats.org/markup-compatibility/2006\"",
                "xmlns:d=\"http://schemas.microsoft.com/expression/blend/2008\"",
                "xmlns:local=\"clr-namespace:ModernWpf.Gallery.Pages.WpfGallery.Navigation\"",
                "xmlns:controls=\"clr-namespace:ModernWpf.Gallery.Controls\"",
                "mc:Ignorable=\"d\"",
                "d:DesignHeight=\"450\" d:DesignWidth=\"800\"",
                "Title=\"NavigationWindowPage\"",
                "Foreground=\"{DynamicResource TextFillColorPrimaryBrush}\">");
            StringAssert.Contains(
                normalizedNavigationWindowXaml,
                "        <Grid x:Name=\"ContentPagePane\" Height=\"Auto\">\n            <Grid.RowDefinitions>");
            StringAssert.Contains(
                normalizedNavigationWindowXaml,
                "            </Grid.RowDefinitions>\n            <controls:PageHeader");
            StringAssert.Contains(
                normalizedNavigationWindowXaml,
                "<controls:PageHeader Margin=\"0,0,0,32\" Title=\"{Binding ViewModel.PageTitle}\" ShowDescription=\"False\" />\n            <ScrollViewer");
            StringAssert.Contains(
                navigationWindowXaml,
                "<controls:PageHeader Margin=\"0,0,0,32\" Title=\"{Binding ViewModel.PageTitle}\" ShowDescription=\"False\" />");
            StringAssert.Contains(
                navigationWindowXaml,
                "<ScrollViewer Grid.Row=\"1\" Margin=\"0,0,0,24\" Padding=\"0,0,24,0\">");
            AssertContainsInOrder(
                navigationWindowXaml,
                "<controls:ControlExample",
                "Margin=\"10\"",
                "HeaderText=\"A Navigation Window\"",
                "XamlCode=\"&lt;NavigationWindow",
                "CSharpCode=\"private void OpenNavigationWindow_Click(object sender, RoutedEventArgs e)");
            AssertContainsInOrder(
                navigationWindowXaml,
                "<Button",
                "x:Name=\"OpenNavigationWindow\"",
                "VerticalAlignment=\"Center\"",
                "HorizontalAlignment=\"Center\"",
                "Content=\"Open window to view NavigationWindow\"",
                "Click=\"OpenNavigationWindow_Click\" />");

            var navigationWindowCodeBehind = ReadRepoFile(
                "ModernWpf.Gallery",
                "Pages",
                "WpfGallery",
                "Navigation",
                "NavigationWindowPage.xaml.cs");
            AssertContainsInOrder(
                navigationWindowCodeBehind,
                "NavigationWindow window = new NavigationWindow()",
                "{",
                "Width = 800,",
                "Height = 450,",
                "Source = new Uri(\"pack://application:,,,/ModernWpf.Gallery;component/Pages/WpfGallery/Navigation/Page1.xaml\", UriKind.Absolute)",
                "};",
                "window.Show();");

            var frameWindowXaml = ReadRepoFile(
                "ModernWpf.Gallery",
                "Pages",
                "WpfGallery",
                "Navigation",
                "FrameWindow.xaml");
            AssertContainsInOrder(
                frameWindowXaml,
                "<Window x:Class=\"ModernWpf.Gallery.Pages.WpfGallery.Navigation.FrameWindow\"",
                "xmlns:d=\"http://schemas.microsoft.com/expression/blend/2008\"",
                "xmlns:mc=\"http://schemas.openxmlformats.org/markup-compatibility/2006\"",
                "xmlns:local=\"clr-namespace:ModernWpf.Gallery.Pages.WpfGallery.Navigation\"",
                "mc:Ignorable=\"d\"",
                "Title=\"FrameWindow\" Height=\"450\" Width=\"800\">");
            StringAssert.Contains(
                frameWindowXaml,
                "<Frame Source=\"Page1.xaml\" NavigationUIVisibility=\"Visible\"/>");

            var page1Xaml = ReadRepoFile(
                "ModernWpf.Gallery",
                "Pages",
                "WpfGallery",
                "Navigation",
                "Page1.xaml");
            AssertContainsInOrder(
                page1Xaml,
                "<Page x:Class=\"ModernWpf.Gallery.Pages.WpfGallery.Navigation.Page1\"",
                "xmlns:mc=\"http://schemas.openxmlformats.org/markup-compatibility/2006\"",
                "xmlns:d=\"http://schemas.microsoft.com/expression/blend/2008\"",
                "xmlns:local=\"clr-namespace:ModernWpf.Gallery.Pages.WpfGallery.Navigation\"",
                "mc:Ignorable=\"d\"",
                "d:DesignHeight=\"450\" d:DesignWidth=\"800\"",
                "Title=\"Page1\">");
            StringAssert.Contains(
                page1Xaml,
                "<StackPanel HorizontalAlignment=\"Center\" VerticalAlignment=\"Center\">");
            StringAssert.Contains(
                page1Xaml,
                "<TextBlock Text=\"This is Page 1\" FontSize=\"20\" Margin=\"10\" Foreground=\"{DynamicResource TextFillColorPrimaryBrush}\"/>");
            StringAssert.Contains(
                page1Xaml,
                "<Hyperlink NavigateUri=\"Page2.xaml\">This is the link to Page 2</Hyperlink>");

            var page2Xaml = ReadRepoFile(
                "ModernWpf.Gallery",
                "Pages",
                "WpfGallery",
                "Navigation",
                "Page2.xaml");
            AssertContainsInOrder(
                page2Xaml,
                "<Page x:Class=\"ModernWpf.Gallery.Pages.WpfGallery.Navigation.Page2\"",
                "xmlns:mc=\"http://schemas.openxmlformats.org/markup-compatibility/2006\"",
                "xmlns:d=\"http://schemas.microsoft.com/expression/blend/2008\"",
                "xmlns:local=\"clr-namespace:ModernWpf.Gallery.Pages.WpfGallery.Navigation\"",
                "mc:Ignorable=\"d\"",
                "d:DesignHeight=\"450\" d:DesignWidth=\"800\"",
                "Title=\"Page2\">");
            StringAssert.Contains(
                page2Xaml,
                "<TextBlock Text=\"This is Page 2\" HorizontalAlignment=\"Center\" VerticalAlignment=\"Center\" Foreground=\"{DynamicResource TextFillColorPrimaryBrush}\" FontSize=\"20\"/>");
        }

        [TestMethod]
        public void TabControlPageKeepsOfficialTabHeaderSourceShape()
        {
            var xaml = ReadRepoFile(
                "ModernWpf.Gallery",
                "Pages",
                "WpfGallery",
                "Navigation",
                "TabControlPage.xaml");
            var normalizedXaml = xaml.Replace("\r\n", "\n").Replace('\r', '\n');
            StringAssert.Contains(
                xaml,
                "xmlns:local=\"clr-namespace:ModernWpf.Gallery.Pages.WpfGallery.Navigation\"");
            StringAssert.Contains(
                normalizedXaml,
                "<Grid x:Name=\"ContentPagePane\" Height=\"Auto\">\n\n        <Grid.RowDefinitions>");
            StringAssert.Contains(
                normalizedXaml,
                "</Grid.RowDefinitions>\n        <controls:PageHeader");
            StringAssert.Contains(
                normalizedXaml,
                "<controls:PageHeader Margin=\"0,0,0,32\" Title=\"{Binding ViewModel.PageTitle}\" ShowDescription=\"False\" />\n\n        <ScrollViewer");
            StringAssert.Contains(
                xaml,
                "<controls:PageHeader Margin=\"0,0,0,32\" Title=\"{Binding ViewModel.PageTitle}\" ShowDescription=\"False\" />");
            StringAssert.Contains(
                xaml,
                "<ScrollViewer Grid.Row=\"1\" Margin=\"0,0,0,24\" Padding=\"0,0,24,0\">");
            StringAssert.Contains(
                xaml,
                "<!--<SymbolIcon Margin=\"0,0,6,0\" Symbol=\"XboxConsole24\" />-->");
            StringAssert.Contains(
                xaml,
                "<!--<SymbolIcon Margin=\"0,0,6,0\" Symbol=\"StoreMicrosoft16\" />-->");

            var codeBehind = ReadRepoFile(
                "ModernWpf.Gallery",
                "Pages",
                "WpfGallery",
                "Navigation",
                "TabControlPage.xaml.cs");
            StringAssert.Contains(
                codeBehind.Replace("\r\n", "\n"),
                "DataContext = this;\n\n            InitializeComponent();");
        }

        [TestMethod]
        public void TextPagesKeepOfficialHeaderAndInputSampleSourceShape()
        {
            foreach (var page in new[]
            {
                Tuple.Create("LabelPage.xaml", "<controls:PageHeader Margin=\"0,0,0,32\" Title=\"{Binding ViewModel.PageTitle}\" Description=\"{Binding ViewModel.PageDescription}\" />", false),
                Tuple.Create("TextBoxPage.xaml", "<controls:PageHeader Margin=\"0,0,0,32\" Title=\"{Binding ViewModel.PageTitle}\" Description=\"{Binding ViewModel.PageDescription}\" />", false),
                Tuple.Create("PasswordBoxPage.xaml", "<controls:PageHeader Margin=\"0,0,0,32\" Title=\"{Binding ViewModel.PageTitle}\" Description=\"{Binding ViewModel.PageDescription}\" />", false),
                Tuple.Create("RichTextEditPage.xaml", "<controls:PageHeader Margin=\"0,0,0,32\" Title=\"{Binding ViewModel.PageTitle}\" Description=\"{Binding ViewModel.PageDescription}\" />", false),
                Tuple.Create("TextBlockPage.xaml", "<controls:PageHeader Margin=\"0,0,0,32\" Title=\"{Binding ViewModel.PageTitle}\" Description=\"{Binding ViewModel.PageDescription}\" />", false),
                Tuple.Create("HyperlinkPage.xaml", "<controls:PageHeader Margin=\"0,0,0,32\" Title=\"{Binding ViewModel.PageTitle}\" ShowDescription=\"False\" />", true)
            })
            {
                var xaml = ReadRepoFile(
                    "ModernWpf.Gallery",
                    "Pages",
                    "WpfGallery",
                    "Text",
                    page.Item1);
                var normalizedXaml = xaml.Replace("\r\n", "\n").Replace('\r', '\n');
                StringAssert.Contains(
                    xaml,
                    "xmlns:local=\"clr-namespace:ModernWpf.Gallery.Pages.WpfGallery.Text\"");
                if (page.Item3)
                {
                    StringAssert.Contains(
                        normalizedXaml,
                        "        <Grid x:Name=\"ContentPagePane\" Height=\"Auto\">\n            <Grid.RowDefinitions>");
                    StringAssert.Contains(
                        normalizedXaml,
                        "            </Grid.RowDefinitions>\n            <controls:PageHeader");
                    StringAssert.Contains(
                        normalizedXaml,
                        page.Item2 + "\n            <ScrollViewer");
                }
                else
                {
                    StringAssert.Contains(
                        normalizedXaml,
                        "<Grid x:Name=\"ContentPagePane\" Height=\"Auto\">\n        <Grid.RowDefinitions>");
                    StringAssert.Contains(
                        normalizedXaml,
                        "</Grid.RowDefinitions>\n        <controls:PageHeader");
                    StringAssert.Contains(
                        normalizedXaml,
                        page.Item2 + "\n\n        <ScrollViewer");
                }

                StringAssert.Contains(
                    xaml,
                    page.Item2);
                StringAssert.Contains(
                    xaml,
                    "<ScrollViewer Grid.Row=\"1\" Margin=\"0,0,0,24\" Padding=\"0,0,24,0\">");
            }

            foreach (var page in new[]
            {
                Tuple.Create(
                    "LabelPage.xaml",
                    "                </controls:ControlExample>\n            </StackPanel>\n\n        </ScrollViewer>\n    </Grid>\n\n</Page>"),
                Tuple.Create(
                    "TextBoxPage.xaml",
                    "                </controls:ControlExample>\n            </StackPanel>\n\n        </ScrollViewer>\n    </Grid>\n\n</Page>"),
                Tuple.Create(
                    "PasswordBoxPage.xaml",
                    "                </controls:ControlExample>\n\n            </StackPanel>\n\n        </ScrollViewer>\n    </Grid>\n\n</Page>"),
                Tuple.Create(
                    "RichTextEditPage.xaml",
                    "            </StackPanel>\n        </ScrollViewer>\n    </Grid>\n\n</Page>"),
                Tuple.Create(
                    "TextBlockPage.xaml",
                    "                </controls:ControlExample>\n\n            </StackPanel>\n\n        </ScrollViewer>\n    </Grid>\n\n</Page>")
            })
            {
                var xaml = ReadRepoFile(
                    "ModernWpf.Gallery",
                    "Pages",
                    "WpfGallery",
                    "Text",
                    page.Item1);
                StringAssert.Contains(
                    xaml.Replace("\r\n", "\n").Replace('\r', '\n'),
                    page.Item2);
            }

            var labelXaml = ReadRepoFile(
                "ModernWpf.Gallery",
                "Pages",
                "WpfGallery",
                "Text",
                "LabelPage.xaml");
            StringAssert.Contains(
                labelXaml,
                "<Label Content=\"I am a Label.\" Foreground=\"{DynamicResource TextFillColorPrimaryBrush}\" Opacity=\"0.7\" />");
            StringAssert.Contains(
                labelXaml,
                "<!--  Target=\"{Binding ElementName=TextBoxForLabel}\"  -->");

            var textBoxXaml = ReadRepoFile(
                "ModernWpf.Gallery",
                "Pages",
                "WpfGallery",
                "Text",
                "TextBoxPage.xaml");
            AssertContainsInOrder(
                textBoxXaml,
                "xmlns:d=\"http://schemas.microsoft.com/expression/blend/2008\"",
                "xmlns:local=\"clr-namespace:ModernWpf.Gallery.Pages.WpfGallery.Text\"",
                "xmlns:mc=\"http://schemas.openxmlformats.org/markup-compatibility/2006\"",
                "xmlns:helpers=\"clr-namespace:ModernWpf.Gallery.Helpers\"");
            StringAssert.Contains(
                textBoxXaml,
                "<controls:ControlExample Margin=\"10\" HeaderText=\"A simple TextBox.\" XamlCode=\"&lt;TextBox /&gt;\">");
            AssertContainsInOrder(
                textBoxXaml,
                "<controls:ControlExample Margin=\"10,36,10,10\"",
                "HeaderText=\"A TextBox with input validation.\"",
                "XamlCode=\"&lt;TextBox&gt;");

            var passwordBoxXaml = ReadRepoFile(
                "ModernWpf.Gallery",
                "Pages",
                "WpfGallery",
                "Text",
                "PasswordBoxPage.xaml");
            AssertContainsInOrder(
                passwordBoxXaml,
                "<controls:ControlExample",
                "Margin=\"10\"",
                "HeaderText=\"A simple PasswordBox.\"",
                "XamlCode=\"&lt;PasswordBox /&gt;\"",
                "<PasswordBox AutomationProperties.Name=\"Simple Password Box\" />");

            var richTextEditXaml = ReadRepoFile(
                "ModernWpf.Gallery",
                "Pages",
                "WpfGallery",
                "Text",
                "RichTextEditPage.xaml");
            AssertContainsInOrder(
                richTextEditXaml,
                "<controls:ControlExample",
                "Margin=\"10\"",
                "HeaderText=\"A simple RichTextBox\"",
                "XamlCode=\"&lt;RichTextBox /&gt;\"",
                "<RichTextBox AutomationProperties.Name=\"simple rich text editor\" />");
            Assert.IsFalse(
                richTextEditXaml.Contains("MinHeight=\"160\"", StringComparison.Ordinal),
                "RichTextEdit should match official WPF Gallery's one-line RichTextBox instead of a recorder-inflated editor.");
            Assert.IsFalse(
                richTextEditXaml.Contains("<FlowDocument", StringComparison.Ordinal),
                "RichTextEdit should not add a custom FlowDocument to the official WPF sample.");

            var textBlockXaml = ReadRepoFile(
                "ModernWpf.Gallery",
                "Pages",
                "WpfGallery",
                "Text",
                "TextBlockPage.xaml");
            AssertContainsInOrder(
                textBlockXaml,
                "<StackPanel Margin=\"0,0,0,24\">",
                "<controls:ControlExample",
                "Margin=\"10\"",
                "HeaderText=\"A simple TextBlock.\"",
                "XamlCode=\"&lt;TextBlock Text=&quot;I am a text block.&quot; /&gt;\"",
                "<TextBlock Text=\"I am a text block.\" />",
                "<controls:ControlExample",
                "Margin=\"10,36,10,10\"",
                "HeaderText=\"A TextBlock with style applied.\"",
                "XamlCode=\"&lt;TextBlock Text=&quot;I am a styled TextBlock.&quot; FontFamily=&quot;Comic Sans MS&quot; FontStyle=&quot;Italic&quot; /&gt;\"",
                "<TextBlock",
                "FontFamily=\"Comic Sans MS\"",
                "FontStyle=\"Italic\"",
                "Text=\"I am a styled TextBlock.\" />",
                "<controls:ControlExample",
                "Margin=\"10,36,10,10\"",
                "HeaderText=\"A TextBlock with inline text elements.\"",
                "XamlCode=\"&lt;TextBlock FontSize=&quot;14&quot;&gt;",
                "<TextBlock FontSize=\"14\">",
                "<Run FontFamily=\"Times New Roman\" Foreground=\"{DynamicResource TextFillColorPrimaryBrush}\" >",
                "Text in a TextBlock doesn't have to be a simple string.",
                "<LineBreak />",
                "<Span>",
                "Text can be <Bold>bold</Bold>,&#x20;",
                "<Italic>italic</Italic>,&#x20;",
                "or <Underline>underlined</Underline>",
                "<controls:ControlExample",
                "Margin=\"10,36,10,10\"",
                "HeaderText=\"A TextBlock with wrap property.\"",
                "XamlCode=\"&lt;TextBlock FontSize=&quot;14&quot; TextWrapping=&quot;Wrap&quot;&gt;",
                "<TextBlock FontSize=\"14\" TextWrapping=\"Wrap\">",
                "The TextBlock control provides flexible text support for WPF applications.",
                "It supports a number of properties that enable precise control of presentation, such as FontFamily, FontSize, FontWeight, TextEffects, and TextWrapping.");

            var hyperlinkXaml = ReadRepoFile(
                "ModernWpf.Gallery",
                "Pages",
                "WpfGallery",
                "Text",
                "HyperlinkPage.xaml");
            AssertContainsInOrder(
                hyperlinkXaml,
                "<Page x:Class=\"ModernWpf.Gallery.Pages.WpfGallery.Text.HyperlinkPage\"",
                "xmlns:mc=\"http://schemas.openxmlformats.org/markup-compatibility/2006\"",
                "xmlns:d=\"http://schemas.microsoft.com/expression/blend/2008\"",
                "xmlns:local=\"clr-namespace:ModernWpf.Gallery.Pages.WpfGallery.Text\"",
                "xmlns:controls=\"clr-namespace:ModernWpf.Gallery.Controls\"",
                "mc:Ignorable=\"d\"",
                "d:DesignHeight=\"450\" d:DesignWidth=\"800\"",
                "Title=\"HyperlinkPage\"",
                "Foreground=\"{DynamicResource TextFillColorPrimaryBrush}\">");
            AssertContainsInOrder(
                hyperlinkXaml,
                "<controls:ControlExample",
                "Margin=\"10\"",
                "HeaderText=\"A Hyperlink with in-app navigation handling\"",
                "XamlCode=\"&lt;TextBlock Margin=&quot;20&quot;&gt;&#10;    &lt;Hyperlink NavigateUri=&quot;https://github.com/Kinnara/ModernWpf&quot; RequestNavigate=&quot;Hyperlink_RequestNavigate&quot;&gt;&#10;        ModernWPF repository&#10;    &lt;/Hyperlink&gt;&#10;&lt;/TextBlock&gt;\"",
                "CSharpCode=\"private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)",
                "<StackPanel>",
                "<TextBlock Margin=\"20\">",
                "<Hyperlink NavigateUri=\"https://github.com/Kinnara/ModernWpf\" RequestNavigate=\"Hyperlink_RequestNavigate\">",
                "ModernWPF repository",
                "</Hyperlink>",
                "x:Name=\"NavigationStatusText\"",
                "AutomationProperties.Name=\"Hyperlink navigation status\"",
                "Visibility=\"Collapsed\"");
            StringAssert.Contains(
                hyperlinkXaml,
                "e.Handled = true");
            StringAssert.Contains(
                hyperlinkXaml.Replace("\r\n", "\n").Replace('\r', '\n'),
                "            </ScrollViewer>\n        </Grid>\n\n    </Grid>\n</Page>");

            var hyperlinkCodeBehind = ReadRepoFile(
                "ModernWpf.Gallery",
                "Pages",
                "WpfGallery",
                "Text",
                "HyperlinkPage.xaml.cs");
            Assert.IsFalse(
                hyperlinkCodeBehind.Contains("Process.Start", StringComparison.Ordinal),
                "The gallery sample must not launch an external process when clicked.");
            AssertContainsInOrder(
                hyperlinkCodeBehind,
                "private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)",
                "NavigationStatusText.Text = \"Navigation request: \" + e.Uri.AbsoluteUri;",
                "NavigationStatusText.Visibility = Visibility.Visible;",
                "e.Handled = true;");
        }

        [TestMethod]
        public void StatusAndInfoPagesKeepOfficialHeaderAndToolTipSourceShape()
        {
            foreach (var page in new[]
            {
                "ProgressBarPage.xaml",
                "ToolTipPage.xaml"
            })
            {
                var xaml = ReadRepoFile(
                    "ModernWpf.Gallery",
                    "Pages",
                    "WpfGallery",
                    "StatusAndInfo",
                    page);
                var normalizedXaml = xaml.Replace("\r\n", "\n").Replace('\r', '\n');
                StringAssert.Contains(
                    xaml,
                    "xmlns:local=\"clr-namespace:ModernWpf.Gallery.Pages.WpfGallery.StatusAndInfo\"");
                StringAssert.Contains(
                    normalizedXaml,
                    "<Grid x:Name=\"ContentPagePane\" Height=\"Auto\">\n\n        <Grid.RowDefinitions>");
                StringAssert.Contains(
                    normalizedXaml,
                    "</Grid.RowDefinitions>\n        <controls:PageHeader");
                StringAssert.Contains(
                    xaml,
                    "<controls:PageHeader Margin=\"0,0,0,32\" Title=\"{Binding ViewModel.PageTitle}\" ShowDescription=\"False\" />");
                StringAssert.Contains(
                    xaml,
                    "<ScrollViewer Grid.Row=\"1\" Margin=\"0,0,0,24\" Padding=\"0,0,24,0\">");
            }

            var progressBarXaml = ReadRepoFile(
                "ModernWpf.Gallery",
                "Pages",
                "WpfGallery",
                "StatusAndInfo",
                "ProgressBarPage.xaml");
            AssertContainsInOrder(
                progressBarXaml,
                "<StackPanel Margin=\"0,0,0,24\">",
                "<controls:ControlExample",
                "Margin=\"10\"",
                "HeaderText=\"A simple progress bar.\"",
                "XamlCode=\"&lt;ProgressBar Value=&quot;40&quot; /&gt;\"",
                "<ProgressBar",
                "Margin=\"24\"",
                "AutomationProperties.Name=\"A determinate\"",
                "Value=\"40\" />",
                "<controls:ControlExample",
                "Margin=\"10,32,10,10\"",
                "HeaderText=\"An indeterminate progress bar.\"",
                "XamlCode=\"&lt;ProgressBar IsIndeterminate=&quot;True&quot; /&gt;\"",
                "<ProgressBar",
                "Margin=\"24\"",
                "AutomationProperties.Name=\"An indeterminate\"",
                "IsIndeterminate=\"True\" />");

            var toolTipXaml = ReadRepoFile(
                "ModernWpf.Gallery",
                "Pages",
                "WpfGallery",
                "StatusAndInfo",
                "ToolTipPage.xaml");
            AssertContainsInOrder(
                toolTipXaml,
                "<Button",
                "Content=\"Button with a simple ToolTip.\"",
                "ToolTipService.InitialShowDelay=\"100\"",
                "ToolTipService.Placement=\"MousePoint\"",
                "AutomationProperties.Name=\"TooltipButton\"",
                "Click=\"ToolTipButton_Click\"",
                "GotKeyboardFocus=\"ToolTipButton_GotKeyboardFocus\"",
                "MouseEnter=\"ToolTipButton_MouseEnter\"",
                "MouseMove=\"ToolTipButton_MouseMove\"",
                "<ToolTipService.ToolTip>",
                "<ToolTip x:Name=\"SimpleToolTip\" Content=\"Simple ToolTip\" />",
                "</ToolTipService.ToolTip>");

            var toolTipCode = ReadRepoFile(
                "ModernWpf.Gallery",
                "Pages",
                "WpfGallery",
                "StatusAndInfo",
                "ToolTipPage.xaml.cs").Replace("\r\n", "\n").Replace('\r', '\n');
            AssertContainsInOrder(
                toolTipCode,
                "using System;",
                "using System.Windows.Controls.Primitives;",
                "using System.Windows.Threading;",
                "using ModernWpf.Gallery.Testing;",
                "private readonly DispatcherTimer _visualTestToolTipCloseTimer;",
                "_visualTestToolTipCloseTimer = new DispatcherTimer",
                "Interval = TimeSpan.FromMilliseconds(1800)",
                "_visualTestToolTipCloseTimer.Tick += (sender, args) =>",
                "SimpleToolTip.IsOpen = false;",
                "private void ToolTipButton_GotKeyboardFocus(object sender, RoutedEventArgs e)",
                "OpenSimpleToolTip(sender as FrameworkElement);",
                "private void ToolTipButton_Click(object sender, RoutedEventArgs e)",
                "OpenSimpleToolTip(sender as FrameworkElement);",
                "private void ToolTipButton_MouseEnter(object sender, MouseEventArgs e)",
                "OpenSimpleToolTip(sender as FrameworkElement);",
                "private void ToolTipButton_MouseMove(object sender, MouseEventArgs e)",
                "OpenSimpleToolTip(sender as FrameworkElement);",
                "private void OpenSimpleToolTip(FrameworkElement placementTarget)",
                "if (!GalleryDiagnostics.IsEnabled)",
                "return;",
                "if (placementTarget == null)",
                "return;",
                "SimpleToolTip.PlacementTarget = placementTarget;",
                "SimpleToolTip.Placement = PlacementMode.Bottom;",
                "SimpleToolTip.VerticalOffset = 4;",
                "SimpleToolTip.IsOpen = true;",
                "_visualTestToolTipCloseTimer.Stop();",
                "_visualTestToolTipCloseTimer.Start();");
        }

        [TestMethod]
        public void DateAndMediaPagesKeepOfficialHeaderAndSimpleSampleSourceShape()
        {
            foreach (var page in new[]
            {
                Tuple.Create(
                    "DateAndTime",
                    "CalendarPage.xaml",
                    "<controls:PageHeader Margin=\"0,0,0,32\" Title=\"{Binding ViewModel.PageTitle}\" ShowDescription=\"False\" />",
                    "xmlns:local=\"clr-namespace:ModernWpf.Gallery.Pages.WpfGallery.DateAndTime\""),
                Tuple.Create(
                    "DateAndTime",
                    "DatePickerPage.xaml",
                    "<controls:PageHeader Margin=\"0,0,0,32\" Title=\"{Binding ViewModel.PageTitle}\" ShowDescription=\"False\" />",
                    "xmlns:local=\"clr-namespace:ModernWpf.Gallery.Pages.WpfGallery.DateAndTime\""),
                Tuple.Create(
                    "Media",
                    "CanvasPage.xaml",
                    "<controls:PageHeader Margin=\"0,0,0,32\" Title=\"{Binding ViewModel.PageTitle}\" Description=\"{Binding ViewModel.PageDescription}\" />",
                    "xmlns:local=\"clr-namespace:ModernWpf.Gallery.Pages.WpfGallery.Media\""),
                Tuple.Create(
                    "Media",
                    "ImagePage.xaml",
                    "<controls:PageHeader Margin=\"0,0,0,32\" Title=\"{Binding ViewModel.PageTitle}\" Description=\"{Binding ViewModel.PageDescription}\" />",
                    "xmlns:local=\"clr-namespace:ModernWpf.Gallery.Pages.WpfGallery.Media\"")
            })
            {
                var xaml = ReadRepoFile(
                    "ModernWpf.Gallery",
                    "Pages",
                    "WpfGallery",
                    page.Item1,
                    page.Item2);
                var normalizedXaml = xaml.Replace("\r\n", "\n").Replace('\r', '\n');
                StringAssert.Contains(xaml, page.Item4);
                StringAssert.Contains(
                    normalizedXaml,
                    "<Grid x:Name=\"ContentPagePane\" Height=\"Auto\">\n\n        <Grid.RowDefinitions>");
                StringAssert.Contains(
                    normalizedXaml,
                    "</Grid.RowDefinitions>\n        <controls:PageHeader");
                StringAssert.Contains(xaml, page.Item3);
                StringAssert.Contains(
                    xaml,
                    "<ScrollViewer Grid.Row=\"1\" Margin=\"0,0,0,24\" Padding=\"0,0,24,0\">");
            }

            var calendarXaml = ReadRepoFile(
                "ModernWpf.Gallery",
                "Pages",
                "WpfGallery",
                "DateAndTime",
                "CalendarPage.xaml");
            AssertContainsInOrder(
                calendarXaml,
                "<controls:ControlExample Margin=\"10\" HeaderText=\"A basic Calendar control.\">",
                "<controls:ControlExample.XamlCode>",
                "&lt;Calendar/&gt;",
                "<Calendar HorizontalAlignment=\"Left\" AutomationProperties.Name=\"Default\" KeyboardNavigation.IsTabStop=\"False\"/>");

            var datePickerXaml = ReadRepoFile(
                "ModernWpf.Gallery",
                "Pages",
                "WpfGallery",
                "DateAndTime",
                "DatePickerPage.xaml");
            AssertContainsInOrder(
                datePickerXaml,
                "<controls:ControlExample Margin=\"10\" HeaderText=\"A basic DatePicker control.\">",
                "<controls:ControlExample.XamlCode>",
                "&lt;DatePicker/&gt;",
                "<DatePicker",
                "MinWidth=\"200\"",
                "HorizontalAlignment=\"Left\"",
                "AutomationProperties.Name=\"Pick a date\" />");

            var canvasXaml = ReadRepoFile(
                "ModernWpf.Gallery",
                "Pages",
                "WpfGallery",
                "Media",
                "CanvasPage.xaml");
            AssertContainsInOrder(
                canvasXaml,
                "<Grid Margin=\"0,0,0,24\">",
                "<controls:ControlExample Margin=\"10\" HeaderText=\"A basic Canvas inside the ViewBox\">",
                "<controls:ControlExample.XamlCode>",
                "&lt;Viewbox Width=&quot;200&quot; Height=&quot;200&quot; &gt;\\n",
                "\\t&lt;Canvas Width=&quot;47&quot; Height=&quot;123&quot;&gt;\\n",
                "\\t\\t&lt;Path Data=&quot;M0,19H18V84h29v15H0V19Z&quot; Fill=&quot;White&quot; /&gt;\\n",
                "\\t\\t&lt;Path Data=&quot;M46,80H29V15H0V0H46V80Z&quot; Fill=&quot;White&quot; /&gt;\\n",
                "<Viewbox Width=\"200\" Height=\"200\">",
                "<Canvas Width=\"47\" Height=\"123\">",
                "<Path Data=\"M0,19H18V84h29v15H0V19Z\" Fill=\"{DynamicResource TextFillColorSecondaryBrush}\" />",
                "<Path Data=\"M46,80H29V15H0V0H46V80Z\" Fill=\"{DynamicResource TextFillColorSecondaryBrush}\" />");

            var imageXaml = ReadRepoFile(
                "ModernWpf.Gallery",
                "Pages",
                "WpfGallery",
                "Media",
                "ImagePage.xaml");
            StringAssert.Contains(
                imageXaml.Replace("\r\n", "\n").Replace('\r', '\n'),
                "</Page.Resources>\n\n\n    <Grid x:Name=\"ContentPagePane\" Height=\"Auto\">");
            AssertContainsInOrder(
                imageXaml,
                "<controls:ControlExample",
                "Margin=\"10\"",
                "HeaderText=\"Standand Image from a local file.\"",
                "XamlCode=\"&lt;Image Height=&quot;100&quot; Source=&quot;Assets\\MyImage.jpg&quot; /&gt;\"",
                "<Image",
                "Height=\"200\"",
                "HorizontalAlignment=\"Left\"",
                "Source=\"pack://application:,,,/ModernWpf.Gallery;component/Assets/SampleMedia/rainier.jpg\" />");
        }

        [TestMethod]
        public void SystemPagesKeepOfficialHeaderAndControlExampleSourceShape()
        {
            foreach (var page in new[]
            {
                Tuple.Create(
                    "FileAndFolderDialogsPage.xaml",
                    "<controls:PageHeader Margin=\"0,0,0,32\" Title=\"{Binding ViewModel.PageTitle}\" ShowDescription=\"False\" />",
                    "<ScrollViewer Grid.Row=\"1\" Margin=\"0,0,0,24\" Padding=\"0,0,24,0\">"),
                Tuple.Create(
                    "MessageBoxPage.xaml",
                    "<controls:PageHeader Margin=\"0,0,0,32\" Title=\"{Binding ViewModel.PageTitle}\" ShowDescription=\"False\" />",
                    "<ScrollViewer Grid.Row=\"1\" Margin=\"0,0,0,24\" Padding=\"0,0,24,0\">"),
                Tuple.Create(
                    "ClipboardPage.xaml",
                    "<controls:PageHeader Grid.Row=\"0\" Margin=\"0,0,0,32\" Title=\"{Binding ViewModel.PageTitle}\" ShowDescription=\"False\" />",
                    "<ScrollViewer Grid.Row=\"2\" Margin=\"0,0,0,24\" Padding=\"0,0,24,0\">")
            })
            {
                var xaml = ReadRepoFile(
                    "ModernWpf.Gallery",
                    "Pages",
                    "WpfGallery",
                    "System",
                    page.Item1);
                var normalizedXaml = xaml.Replace("\r\n", "\n").Replace('\r', '\n');
                StringAssert.Contains(
                    xaml,
                    "xmlns:local=\"clr-namespace:ModernWpf.Gallery.Pages.WpfGallery.SystemPages\"");
                StringAssert.Contains(
                    normalizedXaml,
                    "<Grid x:Name=\"ContentPagePane\" Height=\"Auto\">\n\n        <Grid.RowDefinitions>");
                if (page.Item1 == "ClipboardPage.xaml")
                {
                    StringAssert.Contains(
                        normalizedXaml,
                        "</Grid.RowDefinitions>\n\n        <controls:PageHeader");
                }
                else
                {
                    StringAssert.Contains(
                        normalizedXaml,
                        "</Grid.RowDefinitions>\n        <controls:PageHeader");
                }

                StringAssert.Contains(xaml, page.Item2);
                StringAssert.Contains(xaml, page.Item3);
                AssertControlExamplesKeepOfficialSourceAttributeOrder(xaml, page.Item1);
            }

            var fileDialogsXaml = ReadRepoFile(
                "ModernWpf.Gallery",
                "Pages",
                "WpfGallery",
                "System",
                "FileAndFolderDialogsPage.xaml");
            StringAssert.Contains(
                fileDialogsXaml.Replace("\r\n", "\n").Replace('\r', '\n'),
                "                </controls:ControlExample>\n\n            </StackPanel>\n        </ScrollViewer>\n    </Grid>\n</Page>");
            AssertContainsInOrder(
                fileDialogsXaml,
                "<controls:ControlExample",
                "Margin=\"10\"",
                "HeaderText=\"Pick Single File\"",
                "XamlCode=\"&lt;Button Content=&quot;Pick Single File&quot; Click=&quot;PickSingleFileButton_Click&quot; /&gt;\"",
                "<Button",
                "Content=\"Pick a single file\"",
                "Click=\"PickSingleFileButton_Click\"",
                "Margin=\"0,0,0,10\" />",
                "<TextBlock",
                "Text=\"{Binding ViewModel.SingleFilePath}\"",
                "TextWrapping=\"Wrap\" />",
                "HeaderText=\"Pick Multiple Files\"",
                "XamlCode=\"&lt;Button Content=&quot;Pick Multiple Files&quot; Click=&quot;PickMultipleFilesButton_Click&quot; /&gt;\"",
                "<Button",
                "Content=\"Pick multiple files\"",
                "Click=\"PickMultipleFilesButton_Click\"",
                "Margin=\"0,0,0,10\" />",
                "<TextBlock",
                "Text=\"{Binding ViewModel.MultipleFilesPath}\"",
                "TextWrapping=\"Wrap\" />",
                "HeaderText=\"Save File\"",
                "XamlCode=\"&lt;Button Content=&quot;Save File&quot; Click=&quot;SaveFileButton_Click&quot; /&gt;\"",
                "<TextBox",
                "Text=\"{Binding ViewModel.FileContent, UpdateSourceTrigger=PropertyChanged}\"",
                "AcceptsReturn=\"True\"",
                "TextWrapping=\"Wrap\"",
                "MinHeight=\"80\"",
                "Margin=\"0,0,0,10\"",
                "VerticalScrollBarVisibility=\"Auto\"",
                "AutomationProperties.Name=\"Save File Text Box\"",
                "AutomationProperties.HelpText=\"The text in the textbox will be saved to a file on button click\"/>",
                "<Button",
                "Content=\"Save a file\"",
                "Click=\"SaveFileButton_Click\"",
                "Margin=\"0,0,0,10\" />",
                "<TextBlock",
                "Text=\"{Binding ViewModel.SavedFilePath}\"",
                "TextWrapping=\"Wrap\" />",
                "HeaderText=\"Pick Folder\"",
                "XamlCode=\"&lt;Button Content=&quot;Pick Folder&quot; Click=&quot;PickFolderButton_Click&quot; /&gt;\"",
                "<Button",
                "Content=\"Pick a folder\"",
                "Click=\"PickFolderButton_Click\"",
                "Margin=\"0,0,0,10\" />",
                "<TextBlock",
                "Text=\"{Binding ViewModel.SelectedFolderPath}\"",
                "TextWrapping=\"Wrap\" />");

            var messageBoxXaml = ReadRepoFile(
                "ModernWpf.Gallery",
                "Pages",
                "WpfGallery",
                "System",
                "MessageBoxPage.xaml");
            StringAssert.Contains(
                messageBoxXaml.Replace("\r\n", "\n").Replace('\r', '\n'),
                "                </controls:ControlExample>\n\n            </StackPanel>\n        </ScrollViewer>\n    </Grid>\n</Page>");
            AssertContainsInOrder(
                messageBoxXaml,
                "<controls:ControlExample",
                "HeaderText=\"MessageBox with Different Buttons\"",
                "XamlCode=\"{Binding ViewModel.DifferentButtonsXamlCode}\"",
                "CSharpCode=\"{Binding ViewModel.DifferentButtonsCSharpCode}\"",
                "<Button",
                "Content=\"Show MessageBox\"",
                "AutomationProperties.Name=\"MessageBox with Different Buttons\"",
                "Click=\"ShowButtonFromComboBox_Click\"",
                "Margin=\"0,0,0,10\" />",
                "<TextBlock",
                "Text=\"{Binding ViewModel.DifferentButtonsResult}\"",
                "TextWrapping=\"Wrap\" />",
                "<StackPanel Grid.Column=\"1\" Margin=\"10,0,0,0\">",
                "<TextBlock Text=\"Button Type:\" Margin=\"0,0,0,5\" />",
                "<ComboBox",
                "x:Name=\"ButtonTypeComboBox\"",
                "AutomationProperties.Name=\"MessageBox Button Selector\"",
                "SelectedIndex=\"{Binding ViewModel.SelectedButtonIndex}\"",
                "MinWidth=\"150\">",
                "HeaderText=\"Information, Error, and Warning MessageBox\"",
                "<WrapPanel Margin=\"0,0,0,10\">",
                "<Button Content=\"Information\" Click=\"ShowCommonInformation_Click\" Margin=\"0,0,5,0\" />",
                "<Button Content=\"Error\" Click=\"ShowCommonError_Click\" Margin=\"0,0,5,0\" />",
                "<Button Content=\"Warning\" Click=\"ShowCommonWarning_Click\" />",
                "<TextBlock",
                "Text=\"{Binding ViewModel.CommonMessagesResult}\"",
                "TextWrapping=\"Wrap\" />");

            var clipboardXaml = ReadRepoFile(
                "ModernWpf.Gallery",
                "Pages",
                "WpfGallery",
                "System",
                "ClipboardPage.xaml");
            var normalizedClipboardXaml = clipboardXaml.Replace("\r\n", "\n").Replace('\r', '\n');
            StringAssert.Contains(
                normalizedClipboardXaml,
                "<Border Grid.Row=\"1\"\n                Background=\"{DynamicResource SubtleFillColorSecondaryBrush}\"");
            StringAssert.Contains(
                normalizedClipboardXaml,
                "<TextBlock Grid.Column=\"0\"\n                           FontFamily=\"{StaticResource SymbolThemeFontFamily}\"");
            StringAssert.Contains(
                normalizedClipboardXaml,
                "<TextBlock Grid.Column=\"1\"\n                           TextWrapping=\"Wrap\"");
            StringAssert.Contains(
                normalizedClipboardXaml,
                "<Hyperlink NavigateUri=\"https://learn.microsoft.com/en-us/dotnet/desktop/winforms/migration/clipboard-dataobject-net10\"\n                               RequestNavigate=\"Hyperlink_RequestNavigate\">");
            AssertContainsInOrder(
                clipboardXaml,
                "<Border Grid.Row=\"1\"",
                "Background=\"{DynamicResource SubtleFillColorSecondaryBrush}\"",
                "BorderBrush=\"{DynamicResource AccentFillColorDefaultBrush}\"",
                "BorderThickness=\"1\"",
                "CornerRadius=\"4\"",
                "Padding=\"16,12\"",
                "Margin=\"0,0,0,16\">",
                "<TextBlock Grid.Column=\"0\"",
                "FontFamily=\"{StaticResource SymbolThemeFontFamily}\"",
                "FontSize=\"16\"",
                "Text=\"&#xE946;\"",
                "AutomationProperties.Name=\"Info\"",
                "Foreground=\"{DynamicResource AccentFillColorDefaultBrush}\"",
                "VerticalAlignment=\"Top\"",
                "Margin=\"0,2,12,0\" />",
                "HeaderText=\"Copy text to Clipboard\"",
                "<TextBox",
                "x:Name=\"CopyTextBox\"",
                "Text=\"Hello, Clipboard!\"",
                "AutomationProperties.Name=\"Copy To Clipboard TextBox\"",
                "Margin=\"0,0,0,10\"",
                "Width=\"300\"",
                "HorizontalAlignment=\"Left\" />",
                "<Button",
                "Content=\"Copy to Clipboard\"",
                "Click=\"CopyToClipboard_Click\"",
                "Margin=\"0,0,0,10\" />",
                "HeaderText=\"Copy image to Clipboard\"",
                "<Image",
                "x:Name=\"SourceImage\"",
                "Source=\"pack://application:,,,/ModernWpf.Gallery;component/Assets/ControlImages/Clipboard.png\"",
                "Width=\"100\"",
                "Height=\"100\"",
                "HorizontalAlignment=\"Left\"",
                "Margin=\"0,0,0,10\" />",
                "HeaderText=\"Paste image from Clipboard\"",
                "<Border",
                "BorderBrush=\"Gray\"",
                "BorderThickness=\"1\"",
                "Width=\"200\"",
                "Height=\"200\"",
                "HorizontalAlignment=\"Left\">");
        }

        private static string[] ExtractQuotedStrings(string source)
        {
            return Regex.Matches(source, "\"(?<value>[^\"]+)\"")
                .Cast<Match>()
                .Select(match => match.Groups["value"].Value)
                .ToArray();
        }

        private static void AssertControlExamplesKeepOfficialSourceAttributeOrder(string xaml, string pageName)
        {
            var searchIndex = 0;
            var exampleCount = 0;
            while (true)
            {
                var startIndex = xaml.IndexOf("<controls:ControlExample", searchIndex, StringComparison.Ordinal);
                if (startIndex < 0)
                {
                    break;
                }

                var endIndex = xaml.IndexOf(">", startIndex, StringComparison.Ordinal);
                Assert.IsTrue(endIndex > startIndex, pageName + " should have a closed ControlExample start tag.");
                var startTag = xaml.Substring(startIndex, endIndex - startIndex + 1);
                var headerIndex = startTag.IndexOf("HeaderText=", StringComparison.Ordinal);
                var xamlCodeIndex = startTag.IndexOf("XamlCode=", StringComparison.Ordinal);
                var csharpCodeIndex = startTag.IndexOf("CSharpCode=", StringComparison.Ordinal);

                Assert.IsTrue(headerIndex >= 0, pageName + " ControlExample should keep an official HeaderText attribute.");
                Assert.IsTrue(xamlCodeIndex >= 0, pageName + " ControlExample should keep an official XamlCode attribute.");
                Assert.IsTrue(csharpCodeIndex >= 0, pageName + " ControlExample should keep an official CSharpCode attribute.");
                Assert.IsTrue(
                    headerIndex < xamlCodeIndex && xamlCodeIndex < csharpCodeIndex,
                    pageName + " ControlExample should match the official HeaderText, XamlCode, CSharpCode attribute order.");

                exampleCount++;
                searchIndex = endIndex + 1;
            }

            Assert.IsTrue(exampleCount > 0, pageName + " should contain copied ControlExample samples.");
        }
    }
}
