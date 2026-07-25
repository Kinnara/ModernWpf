namespace ModernWpf.Navigation
{
    /// <summary>
    /// Specifies whether a page instance is retained by a <see cref="Controls.Frame"/>.
    /// </summary>
    public enum NavigationCacheMode
    {
        /// <summary>
        /// The page is not cached and a new instance is created for each type navigation.
        /// </summary>
        Disabled = 0,

        /// <summary>
        /// The page is cached regardless of the frame cache-size limit.
        /// </summary>
        Required = 1,

        /// <summary>
        /// The page is cached while it remains within the frame cache-size limit.
        /// </summary>
        Enabled = 2
    }
}
