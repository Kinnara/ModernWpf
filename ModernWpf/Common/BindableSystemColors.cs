using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media;

namespace ModernWpf
{
    /// <summary>
    ///     Contains bindable properties that are queries into the system's various colors.
    /// </summary>
    [Obsolete]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class BindableSystemColors
    {
        static BindableSystemColors()
        {
            SystemParameters.StaticPropertyChanged += OnSystemParametersStaticPropertyChanged;
        }

        #region Colors

        /// <summary>
        ///     System color of the same name.
        /// </summary>
        public static Color ActiveBorderColor => SystemColors.ActiveBorderColor;

        /// <summary>
        ///     System color of the same name.
        /// </summary>
        public static Color ActiveCaptionColor => SystemColors.ActiveCaptionColor;

        /// <summary>
        ///     System color of the same name.
        /// </summary>
        public static Color ActiveCaptionTextColor => SystemColors.ActiveCaptionTextColor;

        /// <summary>
        ///     System color of the same name.
        /// </summary>
        public static Color AppWorkspaceColor => SystemColors.AppWorkspaceColor;

        /// <summary>
        ///     System color of the same name.
        /// </summary>
        public static Color ControlColor => SystemColors.ControlColor;

        /// <summary>
        ///     System color of the same name.
        /// </summary>
        public static Color ControlDarkColor => SystemColors.ControlDarkColor;

        /// <summary>
        ///     System color of the same name.
        /// </summary>
        public static Color ControlDarkDarkColor => SystemColors.ControlDarkDarkColor;

        /// <summary>
        ///     System color of the same name.
        /// </summary>
        public static Color ControlLightColor => SystemColors.ControlLightColor;

        /// <summary>
        ///     System color of the same name.
        /// </summary>
        public static Color ControlLightLightColor => SystemColors.ControlLightLightColor;

        /// <summary>
        ///     System color of the same name.
        /// </summary>
        public static Color ControlTextColor => SystemColors.ControlTextColor;

        /// <summary>
        ///     System color of the same name.
        /// </summary>
        public static Color DesktopColor => SystemColors.DesktopColor;

        /// <summary>
        ///     System color of the same name.
        /// </summary>
        public static Color GradientActiveCaptionColor => SystemColors.GradientActiveCaptionColor;

        /// <summary>
        ///     System color of the same name.
        /// </summary>
        public static Color GradientInactiveCaptionColor => SystemColors.GradientInactiveCaptionColor;

        /// <summary>
        ///     System color of the same name.
        /// </summary>
        public static Color GrayTextColor => SystemColors.GrayTextColor;

        /// <summary>
        ///     System color of the same name.
        /// </summary>
        public static Color HighlightColor => SystemColors.HighlightColor;

        /// <summary>
        ///     System color of the same name.
        /// </summary>
        public static Color HighlightTextColor => SystemColors.HighlightTextColor;

        /// <summary>
        ///     System color of the same name.
        /// </summary>
        public static Color HotTrackColor => SystemColors.HotTrackColor;

        /// <summary>
        ///     System color of the same name.
        /// </summary>
        public static Color InactiveBorderColor => SystemColors.InactiveBorderColor;

        /// <summary>
        ///     System color of the same name.
        /// </summary>
        public static Color InactiveCaptionColor => SystemColors.InactiveCaptionColor;

        /// <summary>
        ///     System color of the same name.
        /// </summary>
        public static Color InactiveCaptionTextColor => SystemColors.InactiveCaptionTextColor;

        /// <summary>
        ///     System color of the same name.
        /// </summary>
        public static Color InfoColor => SystemColors.InfoColor;

        /// <summary>
        ///     System color of the same name.
        /// </summary>
        public static Color InfoTextColor => SystemColors.InfoTextColor;

        /// <summary>
        ///     System color of the same name.
        /// </summary>
        public static Color MenuColor => SystemColors.MenuColor;

        /// <summary>
        ///     System color of the same name.
        /// </summary>
        public static Color MenuBarColor => SystemColors.MenuBarColor;

        /// <summary>
        ///     System color of the same name.
        /// </summary>
        public static Color MenuHighlightColor => SystemColors.MenuHighlightColor;

        /// <summary>
        ///     System color of the same name.
        /// </summary>
        public static Color MenuTextColor => SystemColors.MenuTextColor;

        /// <summary>
        ///     System color of the same name.
        /// </summary>
        public static Color ScrollBarColor => SystemColors.ScrollBarColor;

        /// <summary>
        ///     System color of the same name.
        /// </summary>
        public static Color WindowColor => SystemColors.WindowColor;

        /// <summary>
        ///     System color of the same name.
        /// </summary>
        public static Color WindowFrameColor => SystemColors.WindowFrameColor;

        /// <summary>
        ///     System color of the same name.
        /// </summary>
        public static Color WindowTextColor => SystemColors.WindowTextColor;

        #endregion

        /// <summary>
        ///     Occurs when one of the properties changes.
        /// </summary>
        public static event PropertyChangedEventHandler StaticPropertyChanged;

        private static void OnSystemParametersStaticPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(SystemParameters.HighContrast) && SystemParameters.HighContrast)
            {
                StaticPropertyChanged?.Invoke(null, new PropertyChangedEventArgs(null));
            }
        }
    }
}
