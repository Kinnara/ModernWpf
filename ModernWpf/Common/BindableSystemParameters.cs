using System;
using System.ComponentModel;
using System.Windows;

namespace ModernWpf
{
    public class BindableSystemParameters : INotifyPropertyChanged
    {
        [ThreadStatic]
        private static BindableSystemParameters _threadLocalSingleton;

        private BindableSystemParameters()
        {
            SystemParameters.StaticPropertyChanged += OnStaticPropertyChanged;
        }

        public static BindableSystemParameters Current
        {
            get
            {
                if (_threadLocalSingleton == null)
                {
                    _threadLocalSingleton = new BindableSystemParameters();
                }
                return _threadLocalSingleton;
            }
        }

        /// <summary>
        /// Gets information about the High Contrast accessibility feature.
        /// </summary>
        /// <returns>true if the HIGHCONTRASTON option is selected; otherwise, false.</returns>
        public bool HighContrast => SystemParameters.HighContrast;

        /// <summary>
        /// Gets a value that indicates the height, in pixels, of a caption area.
        /// </summary>
        /// <returns>The height of a caption area.</returns>
        public double WindowCaptionHeight => SystemParameters.WindowCaptionHeight;

        /// <summary>
        /// Gets the size of the non-client area of the window.
        /// </summary>
        /// <returns>The size of the non-client area of the window, in device-independent units (1/96th of an inch).</returns>
        public Thickness WindowNonClientFrameThickness => SystemParameters.WindowNonClientFrameThickness;

        /// <summary>
        /// Gets the size of the resizing border around the window.
        /// </summary>
        /// <returns>The size of the resizing border around the window, in device-independent units (1/96th of an inch).</returns>
        public Thickness WindowResizeBorderThickness => SystemParameters.WindowResizeBorderThickness;

        /// <summary>
        /// Occurs when one of the properties changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            PropertyChanged?.Invoke(this, e);
        }

        private void OnStaticPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            OnPropertyChanged(e);
        }
    }
}
