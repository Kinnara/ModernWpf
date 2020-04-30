using System.Windows;

namespace ModernWpf.Controls.Primitives
{
    public sealed class ProgressRingTemplateSettings : DependencyObject
    {
        internal ProgressRingTemplateSettings()
        {
        }

        // TemplateSetting properties from WUXC for backwards compatibility.

        #region EllipseDiameter

        private static readonly DependencyPropertyKey EllipseDiameterPropertyKey =
            DependencyProperty.RegisterReadOnly(
                nameof(EllipseDiameter),
                typeof(double),
                typeof(ProgressRingTemplateSettings),
                null);

        public static readonly DependencyProperty EllipseDiameterProperty = EllipseDiameterPropertyKey.DependencyProperty;

        public double EllipseDiameter
        {
            get => (double)GetValue(EllipseDiameterProperty);
            internal set => SetValue(EllipseDiameterPropertyKey, value);
        }

        #endregion

        #region EllipseOffset

        private static readonly DependencyPropertyKey EllipseOffsetPropertyKey =
            DependencyProperty.RegisterReadOnly(
                nameof(EllipseOffset),
                typeof(Thickness),
                typeof(ProgressRingTemplateSettings),
                null);

        public static readonly DependencyProperty EllipseOffsetProperty = EllipseOffsetPropertyKey.DependencyProperty;

        public Thickness EllipseOffset
        {
            get => (Thickness)GetValue(EllipseOffsetProperty);
            internal set => SetValue(EllipseOffsetPropertyKey, value);
        }

        #endregion

        #region MaxSideLength

        private static readonly DependencyPropertyKey MaxSideLengthPropertyKey =
            DependencyProperty.RegisterReadOnly(
                nameof(MaxSideLength),
                typeof(double),
                typeof(ProgressRingTemplateSettings),
                null);

        public static readonly DependencyProperty MaxSideLengthProperty = MaxSideLengthPropertyKey.DependencyProperty;

        public double MaxSideLength
        {
            get => (double)GetValue(MaxSideLengthProperty);
            internal set => SetValue(MaxSideLengthPropertyKey, value);
        }

        #endregion
    }
}
