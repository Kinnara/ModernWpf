using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Resources;

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

        public static readonly DependencyProperty XamlCodeSourceProperty =
            DependencyProperty.Register(
                nameof(XamlCodeSource),
                typeof(Uri),
                typeof(ControlExample),
                new PropertyMetadata(null, OnXamlCodeSourceChanged));

        public Uri XamlCodeSource
        {
            get { return (Uri)GetValue(XamlCodeSourceProperty); }
            set { SetValue(XamlCodeSourceProperty, value); }
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

        public static readonly DependencyProperty CSharpCodeSourceProperty =
            DependencyProperty.Register(
                nameof(CSharpCodeSource),
                typeof(Uri),
                typeof(ControlExample),
                new PropertyMetadata(null, OnCSharpCodeSourceChanged));

        public Uri CSharpCodeSource
        {
            get { return (Uri)GetValue(CSharpCodeSourceProperty); }
            set { SetValue(CSharpCodeSourceProperty, value); }
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
                    RaiseCopyNotification(e);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error copying to clipboard: " + ex.Message);
                }
            }
        }

        private static void OnXamlCodeSourceChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var controlExample = (ControlExample)sender;
            controlExample.XamlCode = LoadResource(e.NewValue as Uri);
        }

        private static void OnCSharpCodeSourceChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var controlExample = (ControlExample)sender;
            controlExample.CSharpCode = LoadResource(e.NewValue as Uri);
        }

        private static string LoadResource(Uri uri)
        {
            if (uri == null)
            {
                return null;
            }

            try
            {
                var streamInfo = TryGetResourceStream(uri) ?? TryGetContentStream(uri);
                if (streamInfo != null)
                {
                    using (var streamReader = new StreamReader(streamInfo.Stream, Encoding.UTF8))
                    {
                        return streamReader.ReadToEnd();
                    }
                }

                var looseContent = LoadLooseContent(uri);
                if (looseContent != null)
                {
                    return looseContent;
                }

                return string.Empty;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                return ex.ToString();
            }
        }

        private static StreamResourceInfo TryGetResourceStream(Uri uri)
        {
            try
            {
                return Application.GetResourceStream(uri);
            }
            catch (IOException)
            {
                return null;
            }
        }

        private static StreamResourceInfo TryGetContentStream(Uri uri)
        {
            try
            {
                return Application.GetContentStream(uri);
            }
            catch (IOException)
            {
                return null;
            }
        }

        private static string LoadLooseContent(Uri uri)
        {
            if (uri.IsAbsoluteUri)
            {
                return null;
            }

            var baseDirectory = EnsureTrailingDirectorySeparator(AppDomain.CurrentDomain.BaseDirectory);
            var relativePath = uri.OriginalString.Replace('/', Path.DirectorySeparatorChar);
            var fullPath = Path.GetFullPath(Path.Combine(baseDirectory, relativePath));

            if (!fullPath.StartsWith(baseDirectory, StringComparison.OrdinalIgnoreCase) || !File.Exists(fullPath))
            {
                return null;
            }

            return File.ReadAllText(fullPath, Encoding.UTF8);
        }

        private static string EnsureTrailingDirectorySeparator(string path)
        {
            if (path.EndsWith(Path.DirectorySeparatorChar.ToString(), StringComparison.Ordinal))
            {
                return path;
            }

            return path + Path.DirectorySeparatorChar;
        }

        private static void RaiseCopyNotification(ExecutedRoutedEventArgs e)
        {
#if NET8_0_OR_GREATER
            if (!string.Equals(e.Parameter as string, "Copy_XamlCode", StringComparison.Ordinal))
            {
                return;
            }

            var sourceButton = e.OriginalSource as Button;
            if (sourceButton == null)
            {
                return;
            }

            var peer = UIElementAutomationPeer.CreatePeerForElement(sourceButton);
            if (peer == null)
            {
                return;
            }

            peer.RaiseNotificationEvent(
                AutomationNotificationKind.Other,
                AutomationNotificationProcessing.ImportantMostRecent,
                "Source Code Copied",
                "ButtonClickedActivity");
#endif
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
