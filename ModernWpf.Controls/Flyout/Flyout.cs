using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using ModernWpf.Controls.Primitives;

namespace ModernWpf.Controls
{
    [ContentProperty(nameof(Content))]
    [StyleTypedProperty(Property = nameof(FlyoutPresenterStyle), StyleTargetType = typeof(FlyoutPresenter))]
    public class Flyout : FlyoutBase
    {
        private const double c_translation = 40;
        private static readonly TimeSpan s_translateDuration = TimeSpan.FromMilliseconds(367);

        private static readonly PropertyPath s_opacityPath = new PropertyPath(UIElement.OpacityProperty);
        private static readonly PropertyPath s_translateXPath = new PropertyPath("(UIElement.RenderTransform).(TranslateTransform.X)");
        private static readonly PropertyPath s_translateYPath = new PropertyPath("(UIElement.RenderTransform).(TranslateTransform.Y)");
        private static readonly KeySpline s_decelerateKeySpline = new KeySpline(0.1, 0.9, 0.2, 1);

        private static readonly BitmapCache s_bitmapCacheMode = new BitmapCache();

        public Flyout()
        {
        }

        #region Content

        public static readonly DependencyProperty ContentProperty =
            DependencyProperty.Register(
                nameof(Content),
                typeof(UIElement),
                typeof(Flyout));

        public UIElement Content
        {
            get => (UIElement)GetValue(ContentProperty);
            set => SetValue(ContentProperty, value);
        }

        #endregion

        #region FlyoutPresenterStyle

        public static readonly DependencyProperty FlyoutPresenterStyleProperty =
            DependencyProperty.Register(
                nameof(FlyoutPresenterStyle),
                typeof(Style),
                typeof(Flyout));

        public Style FlyoutPresenterStyle
        {
            get => (Style)GetValue(FlyoutPresenterStyleProperty);
            set => SetValue(FlyoutPresenterStyleProperty, value);
        }

        #endregion

        internal override PopupAnimation DesiredPopupAnimation => PopupAnimation.None;

        private bool IsPopupOpenDown => TryGetPopupOffset(out Point offset) && offset.Y > 0;
        private bool IsPopupOpenRight => TryGetPopupOffset(out Point offset) && offset.X > 0;

        protected override Control CreatePresenter()
        {
            var presenter = new FlyoutPresenter();
            presenter.SetBinding(FlyoutPresenter.ContentProperty,
                new Binding { Path = new PropertyPath(ContentProperty), Source = this });
            presenter.SetBinding(FlyoutPresenter.StyleProperty,
                new Binding { Path = new PropertyPath(FlyoutPresenterStyleProperty), Source = this });
            return presenter;
        }

        internal override void OnOpened()
        {
            if (AreOpenCloseAnimationsEnabled && SharedHelpers.IsAnimationsEnabled)
            {
                PlayOpenAnimation();
            }

            base.OnOpened();
        }

        internal override void OnClosed()
        {
            if (m_openingStoryboard != null && InternalPopup.Child is Control presenter)
            {
                m_openingStoryboard.Stop(presenter);
            }

            base.OnClosed();
        }

        private void PlayOpenAnimation()
        {
            var presenter = (Control)InternalPopup.Child;
            EnsureOpeningStoryboard(presenter);

            var animateFrom = GetAnimateFrom();
            UpdateFromOffsetKeyFrames(animateFrom);

            if (!(presenter.RenderTransform is TranslateTransform))
            {
                presenter.RenderTransform = new TranslateTransform();
            }

            if (animateFrom != AnimateFrom.None)
            {
#if NET462_OR_NEWER
                var bitmapCache = new BitmapCache(VisualTreeHelper.GetDpi(presenter).PixelsPerDip);
#else
                var bitmapCache = s_bitmapCacheMode;
#endif
                presenter.CacheMode = bitmapCache;
            }

            m_openingStoryboard.Begin(presenter, true);
        }

        private void EnsureOpeningStoryboard(Control presenter)
        {
            if (m_openingStoryboard == null)
            {
                var opacityAnim = new DoubleAnimationUsingKeyFrames
                {
                    KeyFrames =
                    {
                        new DiscreteDoubleKeyFrame(0, TimeSpan.Zero),
                        new DiscreteDoubleKeyFrame(0, TimeSpan.FromMilliseconds(83)),
                        new LinearDoubleKeyFrame(1, TimeSpan.FromMilliseconds(166))
                    }
                };
                Storyboard.SetTarget(opacityAnim, presenter);
                Storyboard.SetTargetProperty(opacityAnim, s_opacityPath);

                var xAnim = new DoubleAnimationUsingKeyFrames
                {
                    KeyFrames =
                    {
                        (m_fromHorizontalOffsetKeyFrame = new DiscreteDoubleKeyFrame(0, TimeSpan.Zero)),
                        new SplineDoubleKeyFrame(0, s_translateDuration, s_decelerateKeySpline)
                    }
                };
                Storyboard.SetTarget(xAnim, presenter);
                Storyboard.SetTargetProperty(xAnim, s_translateXPath);

                var yAnim = new DoubleAnimationUsingKeyFrames
                {
                    KeyFrames =
                    {
                        (m_fromVerticalOffsetKeyFrame = new DiscreteDoubleKeyFrame(0, TimeSpan.Zero)),
                        new SplineDoubleKeyFrame(0, s_translateDuration, s_decelerateKeySpline)
                    }
                };
                Storyboard.SetTarget(yAnim, presenter);
                Storyboard.SetTargetProperty(yAnim, s_translateYPath);

                m_openingStoryboard = new Storyboard
                {
                    Children = { opacityAnim, xAnim, yAnim },
                    FillBehavior = FillBehavior.Stop
                };
                m_openingStoryboard.Completed += delegate
                {
                    presenter.ClearValue(UIElement.CacheModeProperty);
                };
            }
        }

        private AnimateFrom GetAnimateFrom()
        {
            if (InternalPopup.PlacementTarget != null)
            {
                switch (Placement)
                {
                    case FlyoutPlacementMode.Top:
                    case FlyoutPlacementMode.TopEdgeAlignedLeft:
                    case FlyoutPlacementMode.TopEdgeAlignedRight:
                    case FlyoutPlacementMode.Bottom:
                    case FlyoutPlacementMode.BottomEdgeAlignedLeft:
                    case FlyoutPlacementMode.BottomEdgeAlignedRight:
                        return IsPopupOpenDown ? AnimateFrom.Top : AnimateFrom.Bottom;
                    case FlyoutPlacementMode.Left:
                    case FlyoutPlacementMode.LeftEdgeAlignedTop:
                    case FlyoutPlacementMode.LeftEdgeAlignedBottom:
                    case FlyoutPlacementMode.Right:
                    case FlyoutPlacementMode.RightEdgeAlignedTop:
                    case FlyoutPlacementMode.RightEdgeAlignedBottom:
                        return IsPopupOpenRight ? AnimateFrom.Left : AnimateFrom.Right;
                }
            }

            return AnimateFrom.None;
        }

        private void UpdateFromOffsetKeyFrames(AnimateFrom animateFrom)
        {
            switch (animateFrom)
            {
                case AnimateFrom.None:
                    m_fromHorizontalOffsetKeyFrame.Value = 0;
                    m_fromVerticalOffsetKeyFrame.Value = 0;
                    break;
                case AnimateFrom.Top:
                    m_fromHorizontalOffsetKeyFrame.Value = 0;
                    m_fromVerticalOffsetKeyFrame.Value = -c_translation;
                    break;
                case AnimateFrom.Bottom:
                    m_fromHorizontalOffsetKeyFrame.Value = 0;
                    m_fromVerticalOffsetKeyFrame.Value = c_translation;
                    break;
                case AnimateFrom.Left:
                    m_fromHorizontalOffsetKeyFrame.Value = -c_translation;
                    m_fromVerticalOffsetKeyFrame.Value = 0;
                    break;
                case AnimateFrom.Right:
                    m_fromHorizontalOffsetKeyFrame.Value = c_translation;
                    m_fromVerticalOffsetKeyFrame.Value = 0;
                    break;
            }
        }

        private bool TryGetPopupOffset(out Point offset)
        {
            var child = InternalPopup.Child;
            var placementTarget = InternalPopup.PlacementTarget;

            if (child != null &&
                placementTarget != null &&
                child.IsVisible &&
                placementTarget.IsVisible)
            {
                offset = child.TranslatePoint(new Point(0, 0), placementTarget);
                return true;
            }

            offset = default;
            return false;
        }

        private enum AnimateFrom
        {
            None,
            Top,
            Bottom,
            Left,
            Right,
        }

        private Storyboard m_openingStoryboard;
        private DoubleKeyFrame m_fromHorizontalOffsetKeyFrame;
        private DoubleKeyFrame m_fromVerticalOffsetKeyFrame;
    }
}
