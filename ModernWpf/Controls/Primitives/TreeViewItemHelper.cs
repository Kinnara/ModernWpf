using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ModernWpf.Controls.Primitives
{
    public static class TreeViewItemHelper
    {
        #region IsEnabled

        public static readonly DependencyProperty IsEnabledProperty =
            DependencyProperty.RegisterAttached(
                "IsEnabled",
                typeof(bool),
                typeof(TreeViewItemHelper),
                new PropertyMetadata(OnIsEnabledChanged));

        public static bool GetIsEnabled(TreeViewItem treeViewItem)
        {
            return (bool)treeViewItem.GetValue(IsEnabledProperty);
        }

        public static void SetIsEnabled(TreeViewItem treeViewItem, bool value)
        {
            treeViewItem.SetValue(IsEnabledProperty, value);
        }

        private static void OnIsEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var treeViewItem = (TreeViewItem)d;
            if ((bool)e.NewValue)
            {
                if (treeViewItem.IsInitialized)
                {
                    UpdateIndentation(treeViewItem);
                }
                else
                {
                    treeViewItem.Initialized += OnTreeViewItemInitialized;
                }
            }
            else
            {
                treeViewItem.Initialized -= OnTreeViewItemInitialized;
                treeViewItem.ClearValue(IndentationPropertyKey);
            }
        }

        private static void OnTreeViewItemInitialized(object sender, System.EventArgs e)
        {
            var treeViewItem = (TreeViewItem)sender;
            treeViewItem.Initialized -= OnTreeViewItemInitialized;
            UpdateIndentation(treeViewItem);
        }

        #endregion

        #region CollapsedGlyph

        /// <summary>
        /// Gets the glyph to show for a collapsed tree node.
        /// </summary>
        /// <param name="treeViewItem">The element from which to read the property value.</param>
        /// <returns>The glyph to show for a collapsed tree node.</returns>
        [Obsolete]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static string GetCollapsedGlyph(TreeViewItem treeViewItem)
        {
            return (string)treeViewItem.GetValue(CollapsedGlyphProperty);
        }

        /// <summary>
        /// Sets the glyph to show for a collapsed tree node.
        /// </summary>
        /// <param name="treeViewItem">The element on which to set the attached property.</param>
        /// <param name="value">The property value to set.</param>
        [Obsolete]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static void SetCollapsedGlyph(TreeViewItem treeViewItem, string value)
        {
            treeViewItem.SetValue(CollapsedGlyphProperty, value);
        }

        /// <summary>
        /// Identifies the CollapsedGlyph dependency property.
        /// </summary>
        [Obsolete]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static readonly DependencyProperty CollapsedGlyphProperty =
            DependencyProperty.RegisterAttached(
                "CollapsedGlyph",
                typeof(string),
                typeof(TreeViewItemHelper),
                new PropertyMetadata("\uE76C"));

        #endregion

        #region ExpandedGlyph

        /// <summary>
        /// Gets the glyph to show for an expanded tree node.
        /// </summary>
        /// <param name="treeViewItem">The element from which to read the property value.</param>
        /// <returns>The glyph to show for an expanded tree node.</returns>
        [Obsolete]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static string GetExpandedGlyph(TreeViewItem treeViewItem)
        {
            return (string)treeViewItem.GetValue(ExpandedGlyphProperty);
        }

        /// <summary>
        /// Sets the glyph to show for an expanded tree node.
        /// </summary>
        /// <param name="treeViewItem">The element on which to set the attached property.</param>
        /// <param name="value">The property value to set.</param>
        [Obsolete]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static void SetExpandedGlyph(TreeViewItem treeViewItem, string value)
        {
            treeViewItem.SetValue(ExpandedGlyphProperty, value);
        }

        /// <summary>
        /// Identifies the ExpandedGlyph dependency property.
        /// </summary>
        [Obsolete]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static readonly DependencyProperty ExpandedGlyphProperty =
            DependencyProperty.RegisterAttached(
                "ExpandedGlyph",
                typeof(string),
                typeof(TreeViewItemHelper),
                new PropertyMetadata("\uE70D"));

        #endregion

        #region CollapsedPath

        /// <summary>
        /// Gets the path to show for a collapsed tree node.
        /// </summary>
        /// <param name="treeViewItem">The element from which to read the property value.</param>
        /// <returns>The glyph to show for a collapsed tree node.</returns>
        public static Geometry GetCollapsedPath(TreeViewItem treeViewItem)
        {
            return (Geometry)treeViewItem.GetValue(CollapsedPathProperty);
        }

        /// <summary>
        /// Sets the path to show for a collapsed tree node.
        /// </summary>
        /// <param name="treeViewItem">The element on which to set the attached property.</param>
        /// <param name="value">The property value to set.</param>
        public static void SetCollapsedPath(TreeViewItem treeViewItem, Geometry value)
        {
            treeViewItem.SetValue(CollapsedPathProperty, value);
        }

        /// <summary>
        /// Identifies the CollapsedPath dependency property.
        /// </summary>
        public static readonly DependencyProperty CollapsedPathProperty =
            DependencyProperty.RegisterAttached(
                "CollapsedPath",
                typeof(Geometry),
                typeof(TreeViewItemHelper));

        #endregion

        #region ExpandedPath

        /// <summary>
        /// Gets the glyph to show for an expanded tree node.
        /// </summary>
        /// <param name="treeViewItem">The element from which to read the property value.</param>
        /// <returns>The glyph to show for an expanded tree node.</returns>
        public static Geometry GetExpandedPath(TreeViewItem treeViewItem)
        {
            return (Geometry)treeViewItem.GetValue(ExpandedPathProperty);
        }

        /// <summary>
        /// Sets the glyph to show for an expanded tree node.
        /// </summary>
        /// <param name="treeViewItem">The element on which to set the attached property.</param>
        /// <param name="value">The property value to set.</param>
        public static void SetExpandedPath(TreeViewItem treeViewItem, Geometry value)
        {
            treeViewItem.SetValue(ExpandedPathProperty, value);
        }

        /// <summary>
        /// Identifies the ExpandedPath dependency property.
        /// </summary>
        public static readonly DependencyProperty ExpandedPathProperty =
            DependencyProperty.RegisterAttached(
                "ExpandedPath",
                typeof(Geometry),
                typeof(TreeViewItemHelper));

        #endregion

        #region GlyphBrush

        /// <summary>
        /// Gets the Brush used to paint node glyphs on a TreeView.
        /// </summary>
        /// <param name="treeViewItem">The element from which to read the property value.</param>
        /// <returns>The Brush used to paint node glyphs on a TreeView.</returns>
        public static Brush GetGlyphBrush(TreeViewItem treeViewItem)
        {
            return (Brush)treeViewItem.GetValue(GlyphBrushProperty);
        }

        /// <summary>
        /// Sets the Brush used to paint node glyphs on a TreeView.
        /// </summary>
        /// <param name="treeViewItem">The element on which to set the attached property.</param>
        /// <param name="value">The property value to set.</param>
        public static void SetGlyphBrush(TreeViewItem treeViewItem, Brush value)
        {
            treeViewItem.SetValue(GlyphBrushProperty, value);
        }

        /// <summary>
        /// Identifies the GlyphBrush dependency property.
        /// </summary>
        public static readonly DependencyProperty GlyphBrushProperty =
            DependencyProperty.RegisterAttached(
                "GlyphBrush",
                typeof(Brush),
                typeof(TreeViewItemHelper),
                null);

        #endregion

        #region GlyphOpacity

        /// <summary>
        /// Gets the opacity of node glyphs on a TreeView.
        /// </summary>
        /// <param name="treeViewItem">The element from which to read the property value.</param>
        /// <returns>The opacity of node glyphs on a TreeView.</returns>
        public static double GetGlyphOpacity(TreeViewItem treeViewItem)
        {
            return (double)treeViewItem.GetValue(GlyphOpacityProperty);
        }

        /// <summary>
        /// Sets the opacity of node glyphs on a TreeView.
        /// </summary>
        /// <param name="treeViewItem">The element on which to set the attached property.</param>
        /// <param name="value">The property value to set.</param>
        public static void SetGlyphOpacity(TreeViewItem treeViewItem, double value)
        {
            treeViewItem.SetValue(GlyphOpacityProperty, value);
        }

        /// <summary>
        /// Identifies the GlyphOpacity dependency property.
        /// </summary>
        public static readonly DependencyProperty GlyphOpacityProperty =
            DependencyProperty.RegisterAttached(
                "GlyphOpacity",
                typeof(double),
                typeof(TreeViewItemHelper),
                new PropertyMetadata(1.0));

        #endregion

        #region GlyphSize

        /// <summary>
        /// Gets the size of node glyphs on a TreeView.
        /// </summary>
        /// <param name="treeViewItem">The element from which to read the property value.</param>
        /// <returns>The opacity of size glyphs on a TreeView.</returns>
        public static double GetGlyphSize(TreeViewItem treeViewItem)
        {
            return (double)treeViewItem.GetValue(GlyphSizeProperty);
        }

        /// <summary>
        /// Sets the size of node glyphs on a TreeView.
        /// </summary>
        /// <param name="treeViewItem">The element on which to set the attached property.</param>
        /// <param name="value">The property value to set.</param>
        public static void SetGlyphSize(TreeViewItem treeViewItem, double value)
        {
            treeViewItem.SetValue(GlyphSizeProperty, value);
        }

        /// <summary>
        /// Identifies the GlyphSize attached property.
        /// </summary>
        public static readonly DependencyProperty GlyphSizeProperty =
            DependencyProperty.RegisterAttached(
                "GlyphSize",
                typeof(double),
                typeof(TreeViewItemHelper),
                new PropertyMetadata(12.0));

        #endregion

        #region Indentation

        private static readonly DependencyPropertyKey IndentationPropertyKey =
            DependencyProperty.RegisterAttachedReadOnly(
                "Indentation",
                typeof(Thickness),
                typeof(TreeViewItemHelper),
                null);

        /// <summary>
        /// Identifies the Indentation dependency property.
        /// </summary>
        public static readonly DependencyProperty IndentationProperty =
            IndentationPropertyKey.DependencyProperty;

        /// <summary>
        /// Gets the amount that the item is indented.
        /// </summary>
        /// <param name="treeViewItem">The element from which to read the property value.</param>
        /// <returns>The amount that the item is indented.</returns>
        public static Thickness GetIndentation(TreeViewItem treeViewItem)
        {
            return (Thickness)treeViewItem.GetValue(IndentationProperty);
        }

        private static void SetIndentation(TreeViewItem treeViewItem, Thickness value)
        {
            treeViewItem.SetValue(IndentationPropertyKey, value);
        }

        private static void UpdateIndentation(TreeViewItem item)
        {
            SetIndentation(item, new Thickness(GetDepth(item) * 16, 0, 0, 0));
        }

        #endregion

        private static int GetDepth(TreeViewItem item)
        {
            int depth = 0;
            while (ItemsControl.ItemsControlFromItemContainer(item) is TreeViewItem parentItem)
            {
                depth++;
                item = parentItem;
            }
            return depth;
        }
    }
}
