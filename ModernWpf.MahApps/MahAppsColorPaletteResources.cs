using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace ModernWpf.MahApps
{
    public class MahAppsColorPaletteResources : ResourceDictionary, ISupportInitialize
    {
        private const string ColorPrefix = "MahApps.Colors.";
        private const string BrushPrefix = "MahApps.Brushes.";

        private readonly BindingHelper _bindingHelper;

        public MahAppsColorPaletteResources()
        {
            _bindingHelper = new BindingHelper();
            _bindingHelper.AccentBaseChanged += OnAccentBaseChanged; ;
            _bindingHelper.HighlightChanged += OnHighlightChanged;
        }

        private Color? _accentBase;
        public Color? AccentBase
        {
            get => _accentBase;
            set
            {
                if (SetAccentColor(ref _accentBase, value))
                {
                    UpdateAccentShades();
                    UpdateBrush("Theme.ShowcaseBrush", value);
                }
            }
        }

        private BindingBase _accentBaseBinding;
        public BindingBase AccentBaseBinding
        {
            get => _accentBaseBinding;
            set
            {
                if (_accentBaseBinding != value)
                {
                    _accentBaseBinding = value;
                    BindingOperations.SetBinding(_bindingHelper, BindingHelper.AccentBaseProperty, value);
                }
            }
        }

        private Color? _highlight;
        public Color? Highlight
        {
            get => _highlight;
            set => SetAccentColor(ref _highlight, value);
        }

        private BindingBase _highlightBinding;
        public BindingBase HighlightBinding
        {
            get => _highlightBinding;
            set
            {
                if (_highlightBinding != value)
                {
                    _highlightBinding = value;
                    BindingOperations.SetBinding(_bindingHelper, BindingHelper.HighlightProperty, value);
                }
            }
        }

        private Color? _accent;
        private Color? Accent
        {
            get => _accent;
            set => SetAccentColor(ref _accent, value);
        }

        private Color? _accent2;
        private Color? Accent2
        {
            get => _accent2;
            set => SetAccentColor(ref _accent2, value);
        }

        private Color? _accent3;
        private Color? Accent3
        {
            get => _accent3;
            set => SetAccentColor(ref _accent3, value);
        }

        private Color? _accent4;
        private Color? Accent4
        {
            get => _accent4;
            set => SetAccentColor(ref _accent4, value);
        }

        private static Color GetAccentShade(Color accentBase, byte alpha)
        {
            accentBase.A = alpha;
            return accentBase;
        }

        private void UpdateAccentShades()
        {
            if (AccentBase.HasValue)
            {
                var accentBase = AccentBase.Value;
                Accent = GetAccentShade(accentBase, 0xCC);
                Accent2 = GetAccentShade(accentBase, 0x99);
                Accent3 = GetAccentShade(accentBase, 0x66);
                Accent4 = GetAccentShade(accentBase, 0x33);
            }
            else
            {
                Accent = null;
                Accent2 = null;
                Accent3 = null;
                Accent4 = null;
            }
        }

        private void UpdateBrush(string key, Color? color)
        {
            if (color.HasValue)
            {
                this[key] = new SolidColorBrush(color.Value);
            }
            else
            {
                Remove(key);
            }
        }

        private bool SetAccentColor(ref Color? storage, Color? value, [CallerMemberName]string propertyName = null)
        {
            if (storage != value)
            {
                string colorKey = ColorPrefix + propertyName;
                string brushKey = BrushPrefix + propertyName;

                if (storage.HasValue)
                {
                    Remove(brushKey);
                    Remove(colorKey);
                }

                storage = value;

                if (storage.HasValue)
                {
                    Add(colorKey, storage.Value);
                    Add(brushKey, new SolidColorBrush(value.Value));
                }

                return true;
            }

            return false;
        }

        private void OnHighlightChanged(object sender, RoutedPropertyChangedEventArgs<Color> e)
        {
            Highlight = e.NewValue;
        }

        private void OnAccentBaseChanged(object sender, RoutedPropertyChangedEventArgs<Color> e)
        {
            if (_accentBaseBinding != null)
            {
                AccentBase = e.NewValue;
            }
        }

        public new void EndInit()
        {
            base.EndInit();

            if (AccentBase == null && AccentBaseBinding == null)
            {
                var defaultBinding = new Binding
                {
                    Path = new PropertyPath(ThemeManager.ActualAccentColorProperty),
                    Source = ThemeManager.Current
                };
                AccentBaseBinding = defaultBinding;
                HighlightBinding = defaultBinding;
            }
        }

        void ISupportInitialize.EndInit()
        {
            EndInit();
        }

        private class BindingHelper : DependencyObject
        {
            #region Highlight

            public static readonly DependencyProperty HighlightProperty =
                DependencyProperty.Register(
                    nameof(Highlight),
                    typeof(Color),
                    typeof(BindingHelper),
                    new PropertyMetadata(OnHighlightChanged));

            public Color Highlight
            {
                get => (Color)GetValue(HighlightProperty);
                set => SetValue(HighlightProperty, value);
            }

            public event RoutedPropertyChangedEventHandler<Color> HighlightChanged;

            private static void OnHighlightChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
            {
                ((BindingHelper)d).OnHighlightChanged((Color)e.OldValue, (Color)e.NewValue);
            }

            private void OnHighlightChanged(Color oldHighlight, Color newHighlight)
            {
                HighlightChanged?.Invoke(this, new RoutedPropertyChangedEventArgs<Color>(oldHighlight, newHighlight));
            }

            #endregion

            #region AccentBase

            public static readonly DependencyProperty AccentBaseProperty =
                DependencyProperty.Register(
                    nameof(AccentBase),
                    typeof(Color),
                    typeof(BindingHelper),
                    new PropertyMetadata(OnAccentBaseChanged));

            public Color AccentBase
            {
                get => (Color)GetValue(AccentBaseProperty);
                set => SetValue(AccentBaseProperty, value);
            }

            public event RoutedPropertyChangedEventHandler<Color> AccentBaseChanged;

            private static void OnAccentBaseChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
            {
                ((BindingHelper)d).OnAccentBaseChanged((Color)e.OldValue, (Color)e.NewValue);
            }

            private void OnAccentBaseChanged(Color oldAccentBase, Color newAccentBase)
            {
                AccentBaseChanged?.Invoke(this, new RoutedPropertyChangedEventArgs<Color>(oldAccentBase, newAccentBase));
            }

            #endregion
        }
    }
}
