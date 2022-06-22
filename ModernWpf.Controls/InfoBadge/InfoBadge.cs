using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using ModernWpf.Controls.Primitives;

namespace ModernWpf.Controls
{
    public class InfoBadge : Control
    {
        static InfoBadge()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(InfoBadge), new FrameworkPropertyMetadata(typeof(InfoBadge)));
        }

        public InfoBadge()
        {
            SetValue(TemplateSettingsPropertyKey, new InfoBadgeTemplateSettings());

            SizeChanged += OnSizeChanged;
        }

        #region IconSource

        public IconSource IconSource
        {
            get => (IconSource)GetValue(IconSourceProperty);
            set => SetValue(IconSourceProperty, value);
        }

        public static readonly DependencyProperty IconSourceProperty =
            DependencyProperty.Register(
                nameof(IconSource),
                typeof(IconSource),
                typeof(InfoBadge),
                new PropertyMetadata(OnIconSourcePropertyChanged));

        private static void OnIconSourcePropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            ((InfoBadge)sender).OnDisplayKindPropertiesChanged();
        }

        #endregion

        #region TemplateSettings

        private static readonly DependencyPropertyKey TemplateSettingsPropertyKey =
            DependencyProperty.RegisterReadOnly(
                nameof(TemplateSettings),
                typeof(InfoBadgeTemplateSettings),
                typeof(InfoBadge),
                null);

        public static readonly DependencyProperty TemplateSettingsProperty =
            TemplateSettingsPropertyKey.DependencyProperty;

        public InfoBadgeTemplateSettings TemplateSettings
        {
            get => (InfoBadgeTemplateSettings)GetValue(TemplateSettingsProperty);
        }

        #endregion

        #region Value

        public int Value
        {
            get => (int)GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
        }

        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register(
                nameof(Value),
                typeof(int),
                typeof(InfoBadge),
                new PropertyMetadata(-1, OnValuePropertyChanged));

        private static void OnValuePropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            ((InfoBadge)sender).OnDisplayKindPropertiesChanged();
        }

        #endregion

        #region CornerRadius

        /// <summary>
        /// Identifies the CornerRadius dependency property.
        /// </summary>
        public static readonly DependencyProperty CornerRadiusProperty =
            ControlHelper.CornerRadiusProperty.AddOwner(typeof(InfoBadge));

        /// <summary>
        /// Gets or sets the radius for the corners of the control's border.
        /// </summary>
        /// <returns>
        /// The degree to which the corners are rounded, expressed as values of the CornerRadius
        /// structure.
        /// </returns>
        public CornerRadius CornerRadius
        {
            get => (CornerRadius)GetValue(CornerRadiusProperty);
            set => SetValue(CornerRadiusProperty, value);
        }

        #endregion

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            OnDisplayKindPropertiesChanged();
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            var defaultDesiredSize = base.MeasureOverride(availableSize);
            if (defaultDesiredSize.Width < defaultDesiredSize.Height)
            {
                return new Size(defaultDesiredSize.Height, defaultDesiredSize.Height);
            }
            return defaultDesiredSize;
        }

        //protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs args)
        //{
        //    base.OnPropertyChanged(args);
        //    var property = args.Property;
        //    Control thisAsControl = this;

        //    if (property == ValueProperty)
        //    {
        //        if (Value < -1)
        //        {
        //            throw new ArgumentOutOfRangeException("Value must be equal to or greater than -1");
        //        }
        //    }

        //    if (property == ValueProperty || property == IconSourceProperty)
        //    {
        //        OnDisplayKindPropertiesChanged();
        //    }
        //}

        void OnDisplayKindPropertiesChanged()
        {
            Control thisAsControl = this;
            if (Value >= 0)
            {
                VisualStateManager.GoToState(thisAsControl, "Value", true);
            }
            else
            {
                var iconSource = IconSource;
                if (iconSource != null)
                {
                    TemplateSettings.IconElement = iconSource.CreateIconElement();
                    if (iconSource is FontIconSource)
                    {
                        VisualStateManager.GoToState(thisAsControl, "FontIcon", true);
                    }
                    else
                    {
                        VisualStateManager.GoToState(thisAsControl, "Icon", true);
                    }
                }
                else
                {
                    VisualStateManager.GoToState(thisAsControl, "Dot", true);
                }
            }
        }

        void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            CornerRadius value()
            {
                var cornerRadiusValue = ActualHeight / 2;
                if (SharedHelpers.IsRS5OrHigher())
                {
                    if (ReadLocalValue(CornerRadiusProperty) == DependencyProperty.UnsetValue)
                    {
                        return new CornerRadius(cornerRadiusValue, cornerRadiusValue, cornerRadiusValue, cornerRadiusValue);
                    }
                    else
                    {
                        return CornerRadius;
                    }
                }
                return new CornerRadius(cornerRadiusValue, cornerRadiusValue, cornerRadiusValue, cornerRadiusValue);
            };

            TemplateSettings.InfoBadgeCornerRadius = value();
        }
    }
}
