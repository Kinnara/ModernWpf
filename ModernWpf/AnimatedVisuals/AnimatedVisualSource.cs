using ModernWpf.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace ModernWpf.Controls
{
    public class AnimatedVisualSource : Control
    {
        static AnimatedVisualSource()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(AnimatedVisualSource), new FrameworkPropertyMetadata(typeof(AnimatedVisualSource)));
        }

        public AnimatedVisualSource()
        {
        }

        #region FallbackIconSource

        /// <summary>
        /// Identifies the <see cref="FallbackIconSource"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty FallbackIconSourceProperty =
            DependencyProperty.Register(
                nameof(FallbackIconSource),
                typeof(IconSource),
                typeof(AnimatedVisualSource),
                null);

        /// <summary>
        /// Gets or sets the static icon to use when the animated icon cannot run.
        /// </summary>
        /// <value>The static icon to use when the animated icon cannot run. The default is <see langword="null"/>.</value>
        public IconSource FallbackIconSource
        {
            get => (IconSource)GetValue(FallbackIconSourceProperty);
            set => SetValue(FallbackIconSourceProperty, value);
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
                typeof(AnimatedVisualSource),
                null);

        /// <summary>
        /// Gets or sets a value that indicates whether the icon is mirrored when the <see cref="FlowDirection"/> is RightToLeft.
        /// </summary>
        /// <value><see langword="true"/>, if the icon is mirrored when the <see cref="FlowDirection"/> is <see cref="FlowDirection.RightToLeft"/>. Otherwise, <see langword="false"/>. The default is <see langword="false"/>.</value>
        public bool MirroredWhenRightToLeft
        {
            get => (bool)GetValue(MirroredWhenRightToLeftProperty);
            set => SetValue(MirroredWhenRightToLeftProperty, value);
        }

        #endregion

        #region State

        /// <summary>
        /// Identifies the <see cref="State"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty StateProperty =
            DependencyProperty.Register(
                nameof(State),
                typeof(string),
                typeof(AnimatedVisualSource),
                new PropertyMetadata(OnStateChanged));

        public string State
        {
            get => (string)GetValue(StateProperty);
            set => SetValue(StateProperty, value);
        }

        private static void OnStateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((AnimatedVisualSource)d).OnStatePropertyChanged(e);
        }

        #endregion

        protected virtual void OnStatePropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            VisualStateManager.GoToState(this, (string)e.NewValue, true);
        }
    }
}
