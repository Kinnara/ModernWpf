using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace ModernWpf.WinUI.TestInfra;

public static class VisualTreeTestHelper
{
    public static T? FindDescendant<T>(DependencyObject root)
        where T : DependencyObject
    {
        if (root == null)
        {
            throw new ArgumentNullException(nameof(root));
        }

        foreach (var descendant in EnumerateDescendants(root))
        {
            if (descendant is T match)
            {
                return match;
            }
        }

        return null;
    }

    public static IEnumerable<DependencyObject> EnumerateDescendants(DependencyObject root)
    {
        var count = VisualTreeHelper.GetChildrenCount(root);
        for (var i = 0; i < count; i++)
        {
            var child = VisualTreeHelper.GetChild(root, i);
            yield return child;

            foreach (var descendant in EnumerateDescendants(child))
            {
                yield return descendant;
            }
        }
    }
}
