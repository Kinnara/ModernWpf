using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Markup;
using ModernWpf.Controls.Primitives;

namespace ModernWpf.Controls
{
    [ContentProperty(nameof(Content))]
    [StyleTypedProperty(Property = nameof(FlyoutPresenterStyle), StyleTargetType = typeof(FlyoutPresenter))]
    public class Flyout : FlyoutBase
    {
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

        protected override Control CreatePresenter()
        {
            var presenter = new FlyoutPresenter();
            presenter.SetBinding(FlyoutPresenter.ContentProperty,
                new Binding { Path = new PropertyPath(ContentProperty), Source = this });
            presenter.SetBinding(FlyoutPresenter.StyleProperty,
                new Binding { Path = new PropertyPath(FlyoutPresenterStyleProperty), Source = this });
            return presenter;
        }
    }
}
