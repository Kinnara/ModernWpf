// Ported from https://github.com/MahApps/MahApps.Metro/blob/develop/src/MahApps.Metro/Controls/FlipViewItem.cs

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows;

namespace ModernWpf.Controls
{
    public class FlipViewItem : ContentControl
    {
        /// <summary>Identifies the <see cref="BannerText"/> dependency property.</summary>
        public static readonly DependencyProperty BannerTextProperty
            = DependencyProperty.Register(
                nameof(BannerText),
                typeof(object),
                typeof(FlipViewItem),
                new FrameworkPropertyMetadata("Banner",
                    FrameworkPropertyMetadataOptions.AffectsRender,
                    (d, e) => ((FlipViewItem)d).ExecuteWhenLoaded(() => ((FlipViewItem)d).Owner?.SetCurrentValue(FlipView.BannerTextProperty, e.NewValue))));

        /// <summary>
        /// Gets or sets the banner text.
        /// </summary>
        public object BannerText
        {
            get => GetValue(BannerTextProperty);
            set => SetValue(BannerTextProperty, value);
        }

        /// <summary>Identifies the <see cref="Owner"/> dependency property.</summary>
        private static readonly DependencyPropertyKey OwnerPropertyKey =
            DependencyProperty.RegisterReadOnly(
                nameof(Owner),
                typeof(FlipView),
                typeof(FlipViewItem),
                new PropertyMetadata(null));

        /// <summary>Identifies the <see cref="Owner"/> dependency property.</summary>
        public static readonly DependencyProperty OwnerProperty = OwnerPropertyKey.DependencyProperty;

        public FlipView Owner
        {
            get => (FlipView)GetValue(OwnerProperty);
            protected set => SetValue(OwnerPropertyKey, value);
        }

        static FlipViewItem()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(FlipViewItem), new FrameworkPropertyMetadata(typeof(FlipViewItem)));
        }

        /// <inheritdoc />
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            var flipView = ItemsControl.ItemsControlFromItemContainer(this) as FlipView;
            SetValue(OwnerPropertyKey, flipView);
        }
    }
}
