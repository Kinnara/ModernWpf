using System;
using System.Collections.Generic;
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
                    Assert.IsNull(actual.CSharpCode, context);
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
            return string.Join(
                "\n",
                (xaml ?? string.Empty)
                    .Replace("\r\n", "\n")
                    .Replace('\r', '\n')
                    .Split('\n')
                    .Select(line => line.Trim()));
        }

        public sealed class ExpectedExample
        {
            public ExpectedExample(string headerText, string xamlCode)
            {
                HeaderText = headerText;
                XamlCode = xamlCode;
            }

            public string HeaderText { get; }

            public string XamlCode { get; }
        }
    }
}
