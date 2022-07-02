using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;

namespace ModernWpf.Controls
{
    /// <summary>
    /// Represents an icon that displays and controls a visual that can animate in response to user interaction and visual state changes.
    /// </summary>
    [ContentProperty("Source")]
    public class AnimatedIcon : IconElement
    {
        /// <summary>
        /// Initializes a new instance of the AnimatedIcon class.
        /// </summary>
        public AnimatedIcon()
        {
            Loaded += OnLoaded;
        }

        #region FallbackIconSource

        /// <summary>
        /// Identifies the <see cref="FallbackIconSource"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty FallbackIconSourceProperty =
            DependencyProperty.Register(
                nameof(FallbackIconSource),
                typeof(IconSource),
                typeof(AnimatedIcon),
                new PropertyMetadata(OnFallbackIconSourceChanged));

        /// <summary>
        /// Gets or sets the static icon to use when the animated icon cannot run.
        /// </summary>
        /// <value>The static icon to use when the animated icon cannot run. The default is <see langword="null"/>.</value>
        public IconSource FallbackIconSource
        {
            get => (IconSource)GetValue(FallbackIconSourceProperty);
            set => SetValue(FallbackIconSourceProperty, value);
        }

        private static void OnFallbackIconSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((AnimatedIcon)d).OnFallbackIconSourcePropertyChanged(e);
        }

        #endregion

        #region MirroredWhenRightToLeft

        /// <summary>
        /// Identifies the <see cref="MirroredWhenRightToLeft"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty MirroredWhenRightToLeftProperty =
            DependencyProperty.Register(
                nameof(MirroredWhenRightToLeft),
                typeof(bool),
                typeof(AnimatedIcon),
                new PropertyMetadata(OnMirroredWhenRightToLeftChanged));

        /// <summary>
        /// Gets or sets a value that indicates whether the icon is mirrored when the <see cref="FlowDirection"/> is RightToLeft.
        /// </summary>
        /// <value><see langword="true"/>, if the icon is mirrored when the <see cref="FlowDirection"/> is <see cref="FlowDirection.RightToLeft"/>. Otherwise, <see langword="false"/>. The default is <see langword="false"/>.</value>
        public bool MirroredWhenRightToLeft
        {
            get => (bool)GetValue(MirroredWhenRightToLeftProperty);
            set => SetValue(MirroredWhenRightToLeftProperty, value);
        }

        private static void OnMirroredWhenRightToLeftChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((AnimatedIcon)d).OnMirroredWhenRightToLeftPropertyChanged(e);
        }

        #endregion

        #region Source

        /// <summary>
        /// Identifies the <see cref="Source"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty SourceProperty =
            DependencyProperty.Register(
                nameof(Source),
                typeof(AnimatedVisualSource),
                typeof(AnimatedIcon),
                new PropertyMetadata(OnSourceChanged));

        /// <summary>
        /// Gets or sets the animated visual shown by the <see cref="AnimatedIcon"/> object.
        /// </summary>
        /// <value>The animated visual shown by the <see cref="AnimatedIcon"/>. The default is <see langword="null"/>.</value>
        public AnimatedVisualSource Source
        {
            get => (AnimatedVisualSource)GetValue(SourceProperty);
            set => SetValue(SourceProperty, value);
        }

        private static void OnSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((AnimatedIcon)d).OnSourcePropertyChanged(e);
        }

        #endregion

        #region State

        /// <summary>
        /// Identifies the State dependency property.
        /// </summary>
        public static readonly DependencyProperty StateProperty =
            DependencyProperty.Register(
                "State",
                typeof(string),
                typeof(AnimatedIcon),
                new PropertyMetadata(OnAnimatedIconStatePropertyChanged));


        /// <summary>
        /// Retrieves the value of the AnimatedIcon.State attached property for the specified <see cref="DependencyObject"/>.
        /// </summary>
        /// <param name="control">The object from which the property value is retrieved.</param>
        /// <returns>The current value of the AnimatedIcon.State attached property on the specified dependency object.</returns>
        public static string GetState(DependencyObject control)
        {
            return (string)control.GetValue(StateProperty);
        }

        /// <summary>
        /// Specifies the value of the AnimatedIcon.State attached property for the specified <see cref="DependencyObject"/>.
        /// </summary>
        /// <param name="control">The element on which to set the attached property.</param>
        /// <param name="value">The value of the AnimatedIcon.State attached property on the specified dependency object.</param>
        public static void SetState(DependencyObject control, string value)
        {
            control.SetValue(StateProperty, value);
        }


        private static void OnAnimatedIconStatePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is AnimatedIcon senderAsAnimatedIcon)
            {
                senderAsAnimatedIcon.OnStatePropertyChanged();
            }
        }

        #endregion

        #region FontSize

        public static readonly DependencyProperty FontSizeProperty =
            DependencyProperty.Register(
                nameof(FontSize),
                typeof(double),
                typeof(AnimatedIcon),
                new PropertyMetadata(20.0, OnFontSizeChanged));

        public double FontSize
        {
            get => (double)GetValue(FontSizeProperty);
            set => SetValue(FontSizeProperty, value);
        }

        private static void OnFontSizeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((AnimatedIcon)d).OnFontSizeChanged(e);
        }

        private void OnFontSizeChanged(DependencyPropertyChangedEventArgs e)
        {
            if (Source != null)
            {
                Source.FontSize = (double)e.NewValue;
            }
        }

        #endregion

        private void OnFallbackIconSourcePropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            var _source = Source;
            if (_source != null)
            {
                _source.FallbackIconSource = FallbackIconSource;
            }
        }

        private void OnMirroredWhenRightToLeftPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            var _source = Source;
            if (_source != null)
            {
                _source.MirroredWhenRightToLeft = MirroredWhenRightToLeft;
            }
        }

        private void OnSourcePropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            Children.Clear();

            Source.HorizontalAlignment = HorizontalAlignment.Stretch;
            Source.VerticalAlignment = VerticalAlignment.Center;

            if (ShouldInheritForegroundFromVisualParent)
            {
                Source.Foreground = VisualParentForeground;
            }

            Children.Add(Source);
        }
        
        private void OnStatePropertyChanged()
        {
            var _source = Source;
            if (_source != null)
            {
                _source.State = GetState(this);
            }
        }

        private void OnLoaded(object UnnamedParameter, RoutedEventArgs UnnamedParameter2)
        {
            // AnimatedIcon might get added to a UI which has already set the State property on an ancestor.
            // If this is the case and the animated icon being added doesn't have its own state property
            // We copy the ancestor value when we load. Additionally we attach to our ancestor's property
            // changed event for AnimatedIcon.State to copy the value to AnimatedIcon.
            var property = AnimatedIcon.StateProperty;

            (object ancestorWithState, string stateValue) GetState()
            {
                var parent = VisualTreeHelper.GetParent(this);
                while (parent != null)
                {
                    var stateValue = (string)parent.GetValue(property);
                    if (!string.IsNullOrEmpty(stateValue))
                    {
                        return (parent, stateValue);
                    }
                    parent = VisualTreeHelper.GetParent(parent);
                }
                return (null, string.Empty);
            }

            var (ancestorWithState, stateValue) = GetState();

            if (string.IsNullOrEmpty((string)GetValue(property)))
            {
                SetValue(property, stateValue);
            }

            if (ancestorWithState != null)
            {
                DependencyPropertyDescriptor descriptor = DependencyPropertyDescriptor.FromProperty(property, typeof(AnimatedIcon));
                descriptor.AddValueChanged(ancestorWithState, (sender, e) => OnAncestorAnimatedIconStatePropertyChanged(ancestorWithState, property));
            }

            // Wait until loaded to apply the fallback icon source property because we need the icon source
            // properties to be set before we create the icon element from it.  If those poperties are bound in,
            // they will not have been set during OnApplyTemplate.
            OnFallbackIconSourcePropertyChanged(new DependencyPropertyChangedEventArgs());
        }

        private void OnAncestorAnimatedIconStatePropertyChanged(object sender, DependencyProperty args)
        {
            SetValue(StateProperty, ((DependencyObject)sender).GetValue(args));
        }

        private protected override void InitializeChildren()
        {
            var _source = Source;
            if (_source != null)
            {
                Children.Add(_source);
            }
        }

        private protected override void OnShouldInheritForegroundFromVisualParentChanged()
        {
            if (Source != null)
            {
                if (ShouldInheritForegroundFromVisualParent)
                {
                    Source.Foreground = VisualParentForeground;
                }
                else
                {
                    Source.ClearValue(Control.ForegroundProperty);
                }
            }
        }

        private protected override void OnVisualParentForegroundPropertyChanged(DependencyPropertyChangedEventArgs args)
        {
            if (ShouldInheritForegroundFromVisualParent && Source != null)
            {
                Source.Foreground = (Brush)args.NewValue;
            }
        }

    }
}
