using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Threading;

namespace ModernWpf.SampleApp.Controls
{
    public abstract class ControlWrapperBase<T> : BackgroundVisualHostBase where T : Control, new()
    {
        private static StyleInfoConverter _styleInfoConverter = new StyleInfoConverter();

        private readonly HashSet<string> _bindedSourcePropertyNames = new HashSet<string>();
        private readonly List<Tuple<DependencyProperty, Binding>> _pendingBindings =
            new List<Tuple<DependencyProperty, Binding>>();

        protected ControlWrapperBase()
        {
        }

        protected ControlWrapperBase(Dispatcher dispatcher) : base(dispatcher)
        {
        }

        protected override void OnInitialized(EventArgs e)
        {
            BindProperties();
            base.OnInitialized(e);
        }

        #region ControlStyle

        public static readonly DependencyProperty ControlStyleProperty =
            DependencyProperty.Register(
                nameof(ControlStyle),
                typeof(Style),
                typeof(ControlWrapperBase<T>));

        public Style ControlStyle
        {
            get => (Style)GetValue(ControlStyleProperty);
            set => SetValue(ControlStyleProperty, value);
        }

        #endregion

        protected T Control { get; private set; }

        protected abstract ControlPropertyValues PropertyValues { get; }

        protected override Visual CreateContent()
        {
            Control = new T();

            foreach (var tuple in _pendingBindings)
            {
                Control.SetBinding(tuple.Item1, tuple.Item2);
            }
            _pendingBindings.Clear();

            return Control;
        }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);

            if (_bindedSourcePropertyNames.Contains(e.Property.Name))
            {
                PropertyValues.SetValue(e.Property.Name, e.NewValue);
            }
        }

        protected virtual void BindProperties()
        {
            Bind(StyleProperty, nameof(ControlStyle));
            Bind(WidthProperty);
            Bind(HeightProperty);
            Bind(MinWidthProperty);
            Bind(MinHeightProperty);
            Bind(MaxWidthProperty);
            Bind(MaxHeightProperty);
            Bind(HorizontalAlignmentProperty);
            Bind(VerticalAlignmentProperty);
            Bind(FlowDirectionProperty);
            Bind(UseLayoutRoundingProperty);
        }

        protected void Bind(DependencyProperty targetProperty, string sourcePropertyName = null)
        {
            if (sourcePropertyName == null)
            {
                sourcePropertyName = targetProperty.Name;
            }

            IValueConverter converter = null;
            if (targetProperty.PropertyType == typeof(Style))
            {
                converter = _styleInfoConverter;
            }
            var binding = new Binding(sourcePropertyName) { Source = PropertyValues, Converter = converter };
            _pendingBindings.Add(Tuple.Create(targetProperty, binding));

            _bindedSourcePropertyNames.Add(sourcePropertyName);
            PropertyValues.SetValue(targetProperty.Name, GetValue(targetProperty));
        }
    }

    public class StyleInfo
    {
        public StyleInfo(Style style)
        {
            TargetType = style.TargetType;
            BasedOn = style.BasedOn;
            Setters = style.Setters.ToArray();
        }
               
        public Type TargetType { get; }

        public Style BasedOn { get; }

        public SetterBase[] Setters { get; }
    }

    public class StyleInfoConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is StyleInfo styleInfo)
            {
                var style = new Style(styleInfo.TargetType, styleInfo.BasedOn);
                foreach (var setter in styleInfo.Setters)
                {
                    style.Setters.Add(setter);
                }
                return style;
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class ControlPropertyValues : INotifyPropertyChanged
    {
        private readonly Dictionary<string, object> _values = new Dictionary<string, object>();

        public StyleInfo ControlStyle => GetValue<StyleInfo>();
        public double Width => GetValue<double>();
        public double Height => GetValue<double>();
        public double MinWidth => GetValue<double>();
        public double MinHeight => GetValue<double>();
        public double MaxWidth => GetValue<double>();
        public double MaxHeight => GetValue<double>();
        public HorizontalAlignment HorizontalAlignment => GetValue<HorizontalAlignment>();
        public VerticalAlignment VerticalAlignment => GetValue<VerticalAlignment>();
        public FlowDirection FlowDirection => GetValue<FlowDirection>();
        public bool UseLayoutRounding => GetValue<bool>();

        public event PropertyChangedEventHandler PropertyChanged;

        public void SetValue(string propertyName, object value)
        {
            if (value is Style style)
            {
                value = new StyleInfo(style);
            }
            else if (value is Freezable freezable)
            {
                var clone = freezable.CloneCurrentValue();
                clone.Freeze();
                value = clone;
            }

            _values[propertyName] = value;
            RaisePropertyChanged(propertyName);
        }

        protected T GetValue<T>([CallerMemberName]string propertyName = null)
        {
            if (_values.TryGetValue(propertyName, out object value))
            {
                return (T)value;
            }
            return default;
        }

        private void RaisePropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
