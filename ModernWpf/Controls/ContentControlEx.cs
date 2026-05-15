using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ModernWpf.Controls.Primitives;
using ModernWpf.Media.Animation;

namespace ModernWpf.Controls
{
    public class ContentControlEx : ContentControl
    {
        static ContentControlEx()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ContentControlEx), new FrameworkPropertyMetadata(typeof(ContentControlEx)));
            HorizontalContentAlignmentProperty.OverrideMetadata(typeof(ContentControlEx), new FrameworkPropertyMetadata(HorizontalAlignment.Left));
            VerticalContentAlignmentProperty.OverrideMetadata(typeof(ContentControlEx), new FrameworkPropertyMetadata(VerticalAlignment.Top));
        }

        #region BackgroundSizing

        public static readonly DependencyProperty BackgroundSizingProperty =
            DependencyProperty.Register(
                nameof(BackgroundSizing),
                typeof(ModernWpf.Controls.BackgroundSizing),
                typeof(ContentControlEx),
                new FrameworkPropertyMetadata(ModernWpf.Controls.BackgroundSizing.InnerBorderEdge));

        public BackgroundSizing BackgroundSizing
        {
            get => (BackgroundSizing)GetValue(BackgroundSizingProperty);
            set => SetValue(BackgroundSizingProperty, value);
        }

        #endregion

        #region BackgroundTransition

        public static readonly DependencyProperty BackgroundTransitionProperty =
            DependencyProperty.Register(
                nameof(BackgroundTransition),
                typeof(BrushTransition),
                typeof(ContentControlEx),
                new PropertyMetadata(null));

        public BrushTransition BackgroundTransition
        {
            get => (BrushTransition)GetValue(BackgroundTransitionProperty);
            set => SetValue(BackgroundTransitionProperty, value);
        }

        #endregion

        #region CharacterSpacing

        public static readonly DependencyProperty CharacterSpacingProperty =
            DependencyProperty.Register(
                nameof(CharacterSpacing),
                typeof(int),
                typeof(ContentControlEx),
                new FrameworkPropertyMetadata(0));

        public int CharacterSpacing
        {
            get => (int)GetValue(CharacterSpacingProperty);
            set => SetValue(CharacterSpacingProperty, value);
        }

        #endregion

        #region ContentTransitions

        public static readonly DependencyProperty ContentTransitionsProperty =
            DependencyProperty.Register(
                nameof(ContentTransitions),
                typeof(TransitionCollection),
                typeof(ContentControlEx),
                new PropertyMetadata(null));

        public TransitionCollection ContentTransitions
        {
            get => (TransitionCollection)GetValue(ContentTransitionsProperty);
            set => SetValue(ContentTransitionsProperty, value);
        }

        #endregion

        #region CornerRadius

        public static readonly DependencyProperty CornerRadiusProperty =
            ControlHelper.CornerRadiusProperty.AddOwner(typeof(ContentControlEx));

        public CornerRadius CornerRadius
        {
            get => (CornerRadius)GetValue(CornerRadiusProperty);
            set => SetValue(CornerRadiusProperty, value);
        }

        #endregion

        #region IsTextScaleFactorEnabled

        public static readonly DependencyProperty IsTextScaleFactorEnabledProperty =
            DependencyProperty.Register(
                nameof(IsTextScaleFactorEnabled),
                typeof(bool),
                typeof(ContentControlEx),
                new FrameworkPropertyMetadata(true));

        public bool IsTextScaleFactorEnabled
        {
            get => (bool)GetValue(IsTextScaleFactorEnabledProperty);
            set => SetValue(IsTextScaleFactorEnabledProperty, value);
        }

        #endregion

        #region RecognizesAccessKey

        public static readonly DependencyProperty RecognizesAccessKeyProperty =
            DependencyProperty.Register(
                nameof(RecognizesAccessKey),
                typeof(bool),
                typeof(ContentControlEx),
                new FrameworkPropertyMetadata(false));

        public bool RecognizesAccessKey
        {
            get => (bool)GetValue(RecognizesAccessKeyProperty);
            set => SetValue(RecognizesAccessKeyProperty, value);
        }

        #endregion

        public UIElement ContentTemplateRoot => GetContentTemplateRoot();

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _contentPresenter = GetTemplateChild(ContentPresenterTemplatePartName) as ContentPresenter;
        }

        private UIElement GetContentTemplateRoot()
        {
            if (_contentPresenter == null)
            {
                return null;
            }

            int childCount = VisualTreeHelper.GetChildrenCount(_contentPresenter);
            for (int i = 0; i < childCount; i++)
            {
                if (VisualTreeHelper.GetChild(_contentPresenter, i) is UIElement child)
                {
                    return child;
                }
            }

            return null;
        }

        private const string ContentPresenterTemplatePartName = "PART_ContentPresenter";
        private ContentPresenter _contentPresenter;
    }
}
