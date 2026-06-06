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
    /// <summary>
    /// A control that displays an example of a control
    /// </summary>
    [ContentProperty(nameof(ExampleContent))]
    public class ControlExample : Control
    {
        static ControlExample()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ControlExample), new FrameworkPropertyMetadata(typeof(ControlExample)));
            CommandManager.RegisterClassCommandBinding(typeof(ControlExample), new CommandBinding(ApplicationCommands.Copy, Copy_SourceCode));
        }

        public static readonly DependencyProperty HeaderTextProperty = DependencyProperty.Register(
            nameof(HeaderText),
            typeof(string),
            typeof(ControlExample),
            new PropertyMetadata(null)
        );

        public static readonly DependencyProperty ExampleContentProperty = DependencyProperty.Register(
            nameof(ExampleContent),
            typeof(object),
            typeof(ControlExample),
            new PropertyMetadata(null)
        );

        public static readonly DependencyProperty OptionsContentProperty = DependencyProperty.Register(
            nameof(OptionsContent),
            typeof(object),
            typeof(ControlExample),
            new PropertyMetadata(null)
        );

        public static readonly DependencyProperty XamlCodeProperty = DependencyProperty.Register(
            nameof(XamlCode),
            typeof(string),
            typeof(ControlExample),
            new PropertyMetadata(null)
        );

        public static readonly DependencyProperty XamlCodeSourceProperty = DependencyProperty.Register(
            nameof(XamlCodeSource),
            typeof(Uri),
            typeof(ControlExample),
            new PropertyMetadata(
                null,
                static (o, args) => ((ControlExample)o).OnXamlCodeSourceChanged((Uri)args.NewValue)
            )
        );

        public static readonly DependencyProperty CSharpCodeProperty = DependencyProperty.Register(
            nameof(CSharpCode),
            typeof(string),
            typeof(ControlExample),
            new PropertyMetadata(null)
        );

        public static readonly DependencyProperty CSharpCodeSourceProperty = DependencyProperty.Register(
            nameof(CSharpCodeSource),
            typeof(Uri),
            typeof(ControlExample),
            new PropertyMetadata(
                null,
                static (o, args) => ((ControlExample)o).OnCSharpCodeSourceChanged((Uri)args.NewValue)
            )
        );

        public string HeaderText
        {
            get => (string)GetValue(HeaderTextProperty);
            set => SetValue(HeaderTextProperty, value);
        }

        public object ExampleContent
        {
            get => GetValue(ExampleContentProperty);
            set => SetValue(ExampleContentProperty, value);
        }

        public object OptionsContent
        {
            get => GetValue(OptionsContentProperty);
            set => SetValue(OptionsContentProperty, value);
        }

        public string XamlCode
        {
            get => (string)GetValue(XamlCodeProperty);
            set => SetValue(XamlCodeProperty, value);
        }

        public Uri XamlCodeSource
        {
            get => (Uri)GetValue(XamlCodeSourceProperty);
            set => SetValue(XamlCodeSourceProperty, value);
        }

        public string CSharpCode
        {
            get => (string)GetValue(CSharpCodeProperty);
            set => SetValue(CSharpCodeProperty, value);
        }

        public Uri CSharpCodeSource
        {
            get => (Uri)GetValue(CSharpCodeSourceProperty);
            set => SetValue(CSharpCodeSourceProperty, value);
        }

        private void OnXamlCodeSourceChanged(Uri uri)
        {
            XamlCode = LoadResource(uri);
        }

        private void OnCSharpCodeSourceChanged(Uri uri)
        {
            CSharpCode = LoadResource(uri);
        }

        private static void Copy_SourceCode(object sender, RoutedEventArgs e)
        {
            if (sender is ControlExample controlExample)
            {
                if (!string.IsNullOrEmpty(controlExample.XamlCode))
                {
                    var executedArgs = (ExecutedRoutedEventArgs)e;

                    try
                    {
                        switch (executedArgs.Parameter.ToString())
                        {
                            case "Copy_XamlCode":
                                Clipboard.SetText(controlExample.XamlCode);
                                RaiseCopyNotification(executedArgs);
                                break;
                            case "Copy_CSharpCode":
                                Clipboard.SetText(controlExample.CSharpCode);
                                break;
                            default:
                                throw new InvalidOperationException();
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error copying to clipboard: " + ex.Message);
                    }
                }
            }
        }

        protected override AutomationPeer OnCreateAutomationPeer()
        {
            return new ControlExampleAutomationPeer(this);
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
