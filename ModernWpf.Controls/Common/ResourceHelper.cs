namespace ModernWpf.Controls
{
    internal static class ResourceHelper
    {
        /// <summary>
        /// This method must be called from every control/class's constructor (<see langword="static"/> or default)
        /// present in the <strong>ModernWpf.Controls</strong> assembly
        /// that makes use of the <see cref="ResourceAccessor.GetLocalizedStringResource(string)"/> method.
        /// </summary>
        /// <remarks>
        /// This method ensures that the <see cref="ResourceAccessor"/> is aware of this (<strong>ModernWpf.Controls</strong>) assembly.
        /// We use this method instead of using the <see cref="System.Reflection.Assembly.GetCallingAssembly"/> method
        /// because it is unreliable and may cause instabilities in the end application.
        /// </remarks>
        public static void Initialize()
        {
            ResourceAccessor.modernWpfControlsAssembly ??= typeof(ResourceHelper).Assembly;
        }
    }
}
