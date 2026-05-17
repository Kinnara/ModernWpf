using System.Windows;

namespace ModernWpf.Controls.Primitives
{
    public sealed class ToggleSwitchTemplateSettings : DependencyObject
    {
        internal ToggleSwitchTemplateSettings()
        {
        }

        #region CurtainCurrentToOffOffset

        private static readonly DependencyPropertyKey CurtainCurrentToOffOffsetPropertyKey =
            DependencyProperty.RegisterReadOnly(
                nameof(CurtainCurrentToOffOffset),
                typeof(double),
                typeof(ToggleSwitchTemplateSettings),
                new PropertyMetadata(0.0));

        public static readonly DependencyProperty CurtainCurrentToOffOffsetProperty =
            CurtainCurrentToOffOffsetPropertyKey.DependencyProperty;

        public double CurtainCurrentToOffOffset
        {
            get => (double)GetValue(CurtainCurrentToOffOffsetProperty);
            internal set => SetValue(CurtainCurrentToOffOffsetPropertyKey, value);
        }

        #endregion

        #region CurtainCurrentToOnOffset

        private static readonly DependencyPropertyKey CurtainCurrentToOnOffsetPropertyKey =
            DependencyProperty.RegisterReadOnly(
                nameof(CurtainCurrentToOnOffset),
                typeof(double),
                typeof(ToggleSwitchTemplateSettings),
                new PropertyMetadata(0.0));

        public static readonly DependencyProperty CurtainCurrentToOnOffsetProperty =
            CurtainCurrentToOnOffsetPropertyKey.DependencyProperty;

        public double CurtainCurrentToOnOffset
        {
            get => (double)GetValue(CurtainCurrentToOnOffsetProperty);
            internal set => SetValue(CurtainCurrentToOnOffsetPropertyKey, value);
        }

        #endregion

        #region CurtainOffToOnOffset

        private static readonly DependencyPropertyKey CurtainOffToOnOffsetPropertyKey =
            DependencyProperty.RegisterReadOnly(
                nameof(CurtainOffToOnOffset),
                typeof(double),
                typeof(ToggleSwitchTemplateSettings),
                new PropertyMetadata(0.0));

        public static readonly DependencyProperty CurtainOffToOnOffsetProperty =
            CurtainOffToOnOffsetPropertyKey.DependencyProperty;

        public double CurtainOffToOnOffset
        {
            get => (double)GetValue(CurtainOffToOnOffsetProperty);
            internal set => SetValue(CurtainOffToOnOffsetPropertyKey, value);
        }

        #endregion

        #region CurtainOnToOffOffset

        private static readonly DependencyPropertyKey CurtainOnToOffOffsetPropertyKey =
            DependencyProperty.RegisterReadOnly(
                nameof(CurtainOnToOffOffset),
                typeof(double),
                typeof(ToggleSwitchTemplateSettings),
                new PropertyMetadata(0.0));

        public static readonly DependencyProperty CurtainOnToOffOffsetProperty =
            CurtainOnToOffOffsetPropertyKey.DependencyProperty;

        public double CurtainOnToOffOffset
        {
            get => (double)GetValue(CurtainOnToOffOffsetProperty);
            internal set => SetValue(CurtainOnToOffOffsetPropertyKey, value);
        }

        #endregion

        #region KnobCurrentToOffOffset

        private static readonly DependencyPropertyKey KnobCurrentToOffOffsetPropertyKey =
            DependencyProperty.RegisterReadOnly(
                nameof(KnobCurrentToOffOffset),
                typeof(double),
                typeof(ToggleSwitchTemplateSettings),
                new PropertyMetadata(0.0));

        public static readonly DependencyProperty KnobCurrentToOffOffsetProperty =
            KnobCurrentToOffOffsetPropertyKey.DependencyProperty;

        public double KnobCurrentToOffOffset
        {
            get => (double)GetValue(KnobCurrentToOffOffsetProperty);
            internal set => SetValue(KnobCurrentToOffOffsetPropertyKey, value);
        }

        #endregion

        #region KnobCurrentToOnOffset

        private static readonly DependencyPropertyKey KnobCurrentToOnOffsetPropertyKey =
            DependencyProperty.RegisterReadOnly(
                nameof(KnobCurrentToOnOffset),
                typeof(double),
                typeof(ToggleSwitchTemplateSettings),
                new PropertyMetadata(0.0));

        public static readonly DependencyProperty KnobCurrentToOnOffsetProperty =
            KnobCurrentToOnOffsetPropertyKey.DependencyProperty;

        public double KnobCurrentToOnOffset
        {
            get => (double)GetValue(KnobCurrentToOnOffsetProperty);
            internal set => SetValue(KnobCurrentToOnOffsetPropertyKey, value);
        }

        #endregion

        #region KnobOffToOnOffset

        private static readonly DependencyPropertyKey KnobOffToOnOffsetPropertyKey =
            DependencyProperty.RegisterReadOnly(
                nameof(KnobOffToOnOffset),
                typeof(double),
                typeof(ToggleSwitchTemplateSettings),
                new PropertyMetadata(0.0));

        public static readonly DependencyProperty KnobOffToOnOffsetProperty =
            KnobOffToOnOffsetPropertyKey.DependencyProperty;

        public double KnobOffToOnOffset
        {
            get => (double)GetValue(KnobOffToOnOffsetProperty);
            internal set => SetValue(KnobOffToOnOffsetPropertyKey, value);
        }

        #endregion

        #region KnobOnToOffOffset

        private static readonly DependencyPropertyKey KnobOnToOffOffsetPropertyKey =
            DependencyProperty.RegisterReadOnly(
                nameof(KnobOnToOffOffset),
                typeof(double),
                typeof(ToggleSwitchTemplateSettings),
                new PropertyMetadata(0.0));

        public static readonly DependencyProperty KnobOnToOffOffsetProperty =
            KnobOnToOffOffsetPropertyKey.DependencyProperty;

        public double KnobOnToOffOffset
        {
            get => (double)GetValue(KnobOnToOffOffsetProperty);
            internal set => SetValue(KnobOnToOffOffsetPropertyKey, value);
        }

        #endregion
    }
}
