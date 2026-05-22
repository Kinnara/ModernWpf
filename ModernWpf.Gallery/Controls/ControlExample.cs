using System;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;

namespace ModernWpf.Gallery.Controls
{
    [ContentProperty(nameof(ExampleContent))]
    public class ControlExample : Control
    {
        static ControlExample()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ControlExample), new FrameworkPropertyMetadata(typeof(ControlExample)));
            CommandManager.RegisterClassCommandBinding(typeof(ControlExample), new CommandBinding(ApplicationCommands.Copy, OnCopySourceCode));
        }

        public static readonly DependencyProperty HeaderTextProperty =
            DependencyProperty.Register(
                nameof(HeaderText),
                typeof(string),
                typeof(ControlExample),
                new PropertyMetadata(null));

        public string HeaderText
        {
            get { return (string)GetValue(HeaderTextProperty); }
            set { SetValue(HeaderTextProperty, value); }
        }

        public static readonly DependencyProperty ExampleContentProperty =
            DependencyProperty.Register(
                nameof(ExampleContent),
                typeof(object),
                typeof(ControlExample),
                new PropertyMetadata(null));

        public object ExampleContent
        {
            get { return GetValue(ExampleContentProperty); }
            set { SetValue(ExampleContentProperty, value); }
        }

        public static readonly DependencyProperty XamlCodeProperty =
            DependencyProperty.Register(
                nameof(XamlCode),
                typeof(string),
                typeof(ControlExample),
                new PropertyMetadata(null));

        public string XamlCode
        {
            get { return (string)GetValue(XamlCodeProperty); }
            set { SetValue(XamlCodeProperty, value); }
        }

        public static readonly DependencyProperty CSharpCodeProperty =
            DependencyProperty.Register(
                nameof(CSharpCode),
                typeof(string),
                typeof(ControlExample),
                new PropertyMetadata(null));

        public string CSharpCode
        {
            get { return (string)GetValue(CSharpCodeProperty); }
            set { SetValue(CSharpCodeProperty, value); }
        }

        protected override AutomationPeer OnCreateAutomationPeer()
        {
            return new ControlExampleAutomationPeer(this);
        }

        private static void OnCopySourceCode(object sender, ExecutedRoutedEventArgs e)
        {
            var controlExample = sender as ControlExample;
            if (controlExample == null || e.Parameter == null)
            {
                return;
            }

            string text = null;
            switch (e.Parameter.ToString())
            {
                case "Copy_XamlCode":
                    text = controlExample.XamlCode;
                    break;
                case "Copy_CSharpCode":
                    text = controlExample.CSharpCode;
                    break;
            }

            if (!string.IsNullOrEmpty(text))
            {
                try
                {
                    Clipboard.SetText(text);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error copying to clipboard: " + ex.Message);
                }
            }
        }

        private sealed class ControlExampleAutomationPeer : FrameworkElementAutomationPeer
        {
            public ControlExampleAutomationPeer(ControlExample owner)
                : base(owner)
            {
            }

            protected override AutomationControlType GetAutomationControlTypeCore()
            {
                return AutomationControlType.Group;
            }

            protected override string GetClassNameCore()
            {
                return nameof(ControlExample);
            }
        }
    }
}
