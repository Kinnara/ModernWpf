using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using ModernWpf.Controls.Primitives;
using static ModernWpf.LocalizedDialogCommands;

namespace ModernWpf.Controls
{
    public class MessageBoxTemplateSettings : DependencyObject
    {
        internal MessageBoxTemplateSettings()
        {
        }

        #region IconElement

        private static readonly DependencyPropertyKey IconElementPropertyKey =
            DependencyProperty.RegisterReadOnly(
                nameof(IconElement),
                typeof(IconElement),
                typeof(MessageBoxTemplateSettings),
                null);

        public static readonly DependencyProperty IconElementProperty = IconElementPropertyKey.DependencyProperty;

        public IconElement IconElement
        {
            get => (IconElement)GetValue(IconElementProperty);
            internal set => SetValue(IconElementPropertyKey, value);
        }

        #endregion

        #region OKButtonText

        public static readonly DependencyProperty OKButtonTextProperty =
            DependencyProperty.Register(
                nameof(OKButtonText),
                typeof(string),
                typeof(MessageBoxTemplateSettings),
                new PropertyMetadata(GetString(DialogBoxCommand.IDOK)));

        public string OKButtonText
        {
            get => (string)GetValue(OKButtonTextProperty);
            set => SetValue(OKButtonTextProperty, value);
        }

        #endregion

        #region YesButtonText

        public static readonly DependencyProperty YesButtonTextProperty =
            DependencyProperty.Register(
                nameof(YesButtonText),
                typeof(string),
                typeof(MessageBoxTemplateSettings),
                new PropertyMetadata(GetString(DialogBoxCommand.IDYES)));

        public string YesButtonText
        {
            get => (string)GetValue(YesButtonTextProperty);
            set => SetValue(YesButtonTextProperty, value);
        }

        #endregion

        #region NoButtonText

        public static readonly DependencyProperty NoButtonTextProperty =
            DependencyProperty.Register(
                nameof(NoButtonText),
                typeof(string),
                typeof(MessageBoxTemplateSettings),
                new PropertyMetadata(GetString(DialogBoxCommand.IDNO)));

        public string NoButtonText
        {
            get => (string)GetValue(NoButtonTextProperty);
            set => SetValue(NoButtonTextProperty, value);
        }

        #endregion

        #region CancelButtonText

        public static readonly DependencyProperty CancelButtonTextProperty =
            DependencyProperty.Register(
                nameof(CancelButtonText),
                typeof(string),
                typeof(MessageBoxTemplateSettings),
                new PropertyMetadata(GetString(DialogBoxCommand.IDCANCEL)));

        public string CancelButtonText
        {
            get => (string)GetValue(CancelButtonTextProperty);
            set => SetValue(CancelButtonTextProperty, value);
        }

        #endregion
    }
}
