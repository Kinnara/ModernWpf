using System;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace ModernWpf.Controls
{
    public static class VisualStates
    {
        #region GroupCommon
        /// <summary>
        /// Normal state
        /// </summary>
        public const string StateNormal = "Normal";

        /// <summary>
        /// MouseOver state
        /// </summary>
        public const string StateMouseOver = "MouseOver";

        /// <summary>
        /// Pressed state
        /// </summary>
        public const string StatePressed = "Pressed";

        /// <summary>
        /// Disabled state
        /// </summary>
        public const string StateDisabled = "Disabled";

        /// <summary>
        /// Readonly state
        /// </summary>
        public const string StateReadOnly = "ReadOnly";

        /// <summary>
        /// Common state group
        /// </summary>
        public const string GroupCommon = "CommonStates";
        #endregion GroupCommon

        public static FrameworkElement GetImplementationRoot(DependencyObject? dependencyObject)
        {
            if (dependencyObject is null)
            {
                throw new ArgumentNullException(nameof(dependencyObject));
            }

            return VisualTreeHelper.GetChildrenCount(dependencyObject) == 1
                ? VisualTreeHelper.GetChild(dependencyObject, 0) as FrameworkElement
                : null;
        }

        public static VisualStateGroup TryGetVisualStateGroup(DependencyObject dependencyObject, string groupName)
        {
            var root = GetImplementationRoot(dependencyObject);

            return root is null
                ? null
                : VisualStateManager.GetVisualStateGroups(root)?
                    .OfType<VisualStateGroup>()
                    .FirstOrDefault(group => string.CompareOrdinal(groupName, group.Name) == 0);
        }
    }
}
