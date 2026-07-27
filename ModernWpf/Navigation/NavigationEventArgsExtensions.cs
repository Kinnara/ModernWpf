using System;
using System.Windows.Navigation;

namespace ModernWpf.Navigation
{
    public static class NavigationEventArgsExtensions
    {
        /// <summary>
        /// Gets the data type of the source page.
        /// </summary>
        /// <returns>
        /// The data type of the source page, represented as *namespace*.*type* or simply
        /// *type*.
        /// </returns>
        public static Type GetSourcePageType(this NavigatingCancelEventArgs e)
        {
            return e.Content?.GetType();
        }

        /// <summary>
        /// Gets the data type of the source page.
        /// </summary>
        /// <returns>
        /// The data type of the source page, represented as *namespace*.*type* or simply
        /// *type*.
        /// </returns>
        public static Type GetSourcePageType(this NavigationEventArgs e)
        {
            return e.Content?.GetType();
        }

        /// <summary>
        /// Gets any "Parameter" object passed to the target page for the navigation.
        /// </summary>
        /// <returns>
        /// An object that potentially passes parameters to the navigation target. May be
        /// null.
        /// </returns>
        public static object GetParameter(this NavigatingCancelEventArgs e)
        {
            return e.ExtraData;
        }

        /// <summary>
        /// Gets any "Parameter" object passed to the target page for the navigation.
        /// </summary>
        /// <returns>
        /// An object that potentially passes parameters to the navigation target. May be
        /// null.
        /// </returns>
        public static object GetParameter(this NavigationEventArgs e)
        {
            return e.ExtraData;
        }
    }
}
