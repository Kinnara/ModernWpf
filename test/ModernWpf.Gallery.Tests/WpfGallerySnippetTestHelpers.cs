using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModernWpf.Gallery.Controls;

namespace ModernWpf.Gallery.Tests
{
    internal static class WpfGallerySnippetTestHelpers
    {
        public static void AssertExamples(FrameworkElement page, params ExpectedExample[] expectedExamples)
        {
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

                var actualExamples = FindDescendants<ControlExample>(page).ToArray();
                Assert.AreEqual(expectedExamples.Length, actualExamples.Length, page.GetType().Name);

                for (var i = 0; i < expectedExamples.Length; i++)
                {
                    var expected = expectedExamples[i];
                    var actual = actualExamples[i];
                    var context = page.GetType().Name + " example " + i;

                    Assert.AreEqual(expected.HeaderText, actual.HeaderText, context);
                    Assert.AreEqual(
                        NormalizeXaml(expected.XamlCode),
                        NormalizeXaml(actual.XamlCode),
                        context);
                    if (expected.CSharpCode == null)
                    {
                        Assert.IsNull(actual.CSharpCode, context);
                    }
                    else
                    {
                        Assert.AreEqual(
                            NormalizeXaml(expected.CSharpCode),
                            NormalizeXaml(actual.CSharpCode),
                            context);
                    }
                }
            }
            finally
            {
                window.Content = null;
                window.Close();
                WpfTestHost.DoEvents();
            }
        }

        public static string Lines(params string[] lines)
        {
            return string.Join("\n", lines);
        }

        public static string ReadRepoFile(params string[] relativePath)
        {
            var directory = new DirectoryInfo(GetRepoRoot());
            var candidate = Path.Combine(new[] { directory.FullName }.Concat(relativePath).ToArray());
            if (File.Exists(candidate))
            {
                return File.ReadAllText(candidate);
            }

            Assert.Fail("Could not find repository file '{0}'.", string.Join(Path.DirectorySeparatorChar.ToString(), relativePath));
            return null;
        }

        public static string GetRepoRoot()
        {
            var directory = new DirectoryInfo(AppContext.BaseDirectory);
            while (directory != null)
            {
                var candidate = Path.Combine(directory.FullName, "ModernWpf.sln");
                if (File.Exists(candidate))
                {
                    return directory.FullName;
                }

                directory = directory.Parent;
            }

            Assert.Fail("Could not find repository root from '{0}'.", AppContext.BaseDirectory);
            return null;
        }

        private static IEnumerable<T> FindDescendants<T>(DependencyObject root)
            where T : DependencyObject
        {
            var childCount = VisualTreeHelper.GetChildrenCount(root);
            for (var i = 0; i < childCount; i++)
            {
                var child = VisualTreeHelper.GetChild(root, i);
                if (child is T match)
                {
                    yield return match;
                }

                foreach (var descendant in FindDescendants<T>(child))
                {
                    yield return descendant;
                }
            }
        }

        private static string NormalizeXaml(string xaml)
        {
            var normalizedXaml = (xaml ?? string.Empty)
                .Replace(@"\r\n", "\n")
                .Replace(@"\n", "\n")
                .Replace(@"\t", "\t")
                .Replace("\r\n", "\n")
                .Replace('\r', '\n');

            var lines = normalizedXaml
                .Split('\n')
                .Select(line => line.Trim())
                .ToList();

            while (lines.Count > 0 && lines[0].Length == 0)
            {
                lines.RemoveAt(0);
            }

            while (lines.Count > 0 && lines[lines.Count - 1].Length == 0)
            {
                lines.RemoveAt(lines.Count - 1);
            }

            return string.Join(
                "\n",
                lines);
        }

        public sealed class ExpectedExample
        {
            public ExpectedExample(string headerText, string xamlCode, string cSharpCode = null)
            {
                HeaderText = headerText;
                XamlCode = xamlCode;
                CSharpCode = cSharpCode;
            }

            public string HeaderText { get; }

            public string XamlCode { get; }

            public string CSharpCode { get; }
        }
    }
}
