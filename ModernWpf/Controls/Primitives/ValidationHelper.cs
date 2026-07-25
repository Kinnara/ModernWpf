using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace ModernWpf.Controls.Primitives
{
    public static class ValidationHelper
    {
        #region IsTemplateValidationAdornerSite

        public static readonly DependencyProperty IsTemplateValidationAdornerSiteProperty =
            DependencyProperty.RegisterAttached(
                "IsTemplateValidationAdornerSite",
                typeof(bool),
                typeof(ValidationHelper),
                new PropertyMetadata(OnIsTemplateValidationAdornerSiteChanged));

        public static bool GetIsTemplateValidationAdornerSite(FrameworkElement element)
        {
            return (bool)element.GetValue(IsTemplateValidationAdornerSiteProperty);
        }

        public static void SetIsTemplateValidationAdornerSite(FrameworkElement element, bool value)
        {
            element.SetValue(IsTemplateValidationAdornerSiteProperty, value);
        }

        private static void OnIsTemplateValidationAdornerSiteChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var element = (FrameworkElement)d;
            if ((bool)e.NewValue)
            {
                var templatedParent = element.TemplatedParent;
                Debug.Assert(templatedParent != null);
                Validation.SetErrorTemplate(element, null);

                if (templatedParent != null &&
                    Validation.GetHasError(templatedParent) &&
                    Validation.GetErrorTemplate(templatedParent) is ControlTemplate errorTemplate)
                {
                    var valueSource = DependencyPropertyHelper.GetValueSource(
                        templatedParent,
                        Validation.ErrorTemplateProperty);
                    bool restoreLocalValue =
                        valueSource.BaseValueSource == BaseValueSource.Local &&
                        !valueSource.IsExpression;

                    templatedParent.SetCurrentValue(Validation.ErrorTemplateProperty, null);
                    Validation.SetValidationAdornerSiteFor(element, templatedParent);
                    templatedParent.Dispatcher.BeginInvoke(
                        System.Windows.Threading.DispatcherPriority.Loaded,
                        new System.Action(() =>
                        {
                            if (restoreLocalValue)
                            {
                                Validation.SetErrorTemplate(templatedParent, errorTemplate);
                            }
                            else
                            {
                                templatedParent.InvalidateProperty(Validation.ErrorTemplateProperty);
                            }
                        }));
                }
                else
                {
                    Validation.SetValidationAdornerSiteFor(element, templatedParent);
                }
            }
            else
            {
                element.ClearValue(Validation.ErrorTemplateProperty);
                element.ClearValue(Validation.ValidationAdornerSiteForProperty);
            }
        }

        #endregion
    }
}
