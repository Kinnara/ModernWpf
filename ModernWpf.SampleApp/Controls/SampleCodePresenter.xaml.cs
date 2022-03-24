using ColorCodeStandard;
using SamplesCommon;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ModernWpf.SampleApp.Controls
{
    /// <summary>
    /// SampleCodePresenter.xaml 的交互逻辑
    /// </summary>
    public partial class SampleCodePresenter : UserControl
    {
        public static readonly DependencyProperty CodeProperty = DependencyProperty.Register("Code", typeof(string), typeof(SampleCodePresenter), new PropertyMetadata("", OnDependencyPropertyChanged));
        public string Code
        {
            get { return (string)GetValue(CodeProperty); }
            set { SetValue(CodeProperty, value); }
        }

        public static readonly DependencyProperty CodeSourceFileProperty = DependencyProperty.Register("CodeSourceFile", typeof(object), typeof(SampleCodePresenter), new PropertyMetadata(null, OnDependencyPropertyChanged));
        public Uri CodeSourceFile
        {
            get { return (Uri)GetValue(CodeSourceFileProperty); }
            set { SetValue(CodeSourceFileProperty, value); }
        }

        public static readonly DependencyProperty IsCSharpSampleProperty = DependencyProperty.Register("IsCSharpSample", typeof(bool), typeof(SampleCodePresenter), new PropertyMetadata(false));
        public bool IsCSharpSample
        {
            get { return (bool)GetValue(IsCSharpSampleProperty); }
            set { SetValue(IsCSharpSampleProperty, value); }
        }

        public static readonly DependencyProperty SubstitutionsProperty = DependencyProperty.Register("Substitutions", typeof(IList<ControlExampleSubstitution>), typeof(ControlExample), new PropertyMetadata(null));
        public IList<ControlExampleSubstitution> Substitutions
        {
            get { return (IList<ControlExampleSubstitution>)GetValue(SubstitutionsProperty); }
            set { SetValue(SubstitutionsProperty, value); }
        }

        public bool IsEmpty => Code.Length == 0 && CodeSourceFile == null;

        private string actualCode = "";
        private static Regex SubstitutionPattern = new Regex(@"\$\(([^\)]+)\)");

        public SampleCodePresenter()
        {
            InitializeComponent();
        }

        private static void OnDependencyPropertyChanged(DependencyObject target, DependencyPropertyChangedEventArgs args)
        {
            if (target is SampleCodePresenter presenter)
            {
                presenter.ReevaluateVisibility();
            }
        }

        private void ReevaluateVisibility()
        {
            if (Code.Length == 0 && CodeSourceFile == null)
            {
                Visibility = Visibility.Collapsed;
            }
            else
            {
                Visibility = Visibility.Visible;
            }
        }

        private void SampleCodePresenter_Loaded(object sender, RoutedEventArgs e)
        {
            ReevaluateVisibility();
            VisualStateManager.GoToState(this, IsCSharpSample ? "CSharpSample" : "XAMLSample", false);
            foreach (var substitution in Substitutions)
            {
                substitution.ValueChanged += OnValueChanged;
            }
        }

        private void CodePresenter_Loaded(object sender, RoutedEventArgs e)
        {
            GenerateSyntaxHighlightedContent();
        }

        private void SampleCodePresenter_ActualThemeChanged(FrameworkElement sender, object args)
        {
            // If the theme has changed after the user has already opened the app (ie. via settings), then the new locally set theme will overwrite the colors that are set during Loaded.
            // Therefore we need to re-format the REB to use the correct colors.
            GenerateSyntaxHighlightedContent();
        }

        private void OnValueChanged(ControlExampleSubstitution sender, object e)
        {
            GenerateSyntaxHighlightedContent();
        }

        private Uri GetDerivedSource(Uri rawSource)
        {
            // Get the full path of the source string
            string concatString = "";
            for (int i = 2; i < rawSource.Segments.Length; i++)
            {
                concatString += rawSource.Segments[i];
            }
            Uri derivedSource = new Uri(new Uri("/ControlPagesSampleCode/", UriKind.Relative), concatString);

            return derivedSource;
        }

        private void GenerateSyntaxHighlightedContent()
        {
            if (!string.IsNullOrEmpty(Code))
            {
                FormatAndRenderSampleFromString(Code, CodePresenter, IsCSharpSample ? Languages.CSharp : Languages.Xml);
            }
            else
            {
                FormatAndRenderSampleFromFile(CodeSourceFile, CodePresenter, IsCSharpSample ? Languages.CSharp : Languages.Xml);
            }
        }

        private async void FormatAndRenderSampleFromFile(Uri source, ContentPresenter presenter, ILanguage highlightLanguage)
        {
            if (source != null && source.AbsolutePath.EndsWith("txt"))
            {
                Uri derivedSource = GetDerivedSource(source);
                var file = Application.GetResourceStream(derivedSource);
                string sampleString = string.Empty;

                using (var reader = new StreamReader(file.Stream))
                {
                    await Task.Run(() => sampleString = reader.ReadToEnd());
                }

                FormatAndRenderSampleFromString(sampleString, presenter, highlightLanguage);
            }
            else
            {
                presenter.Visibility = Visibility.Collapsed;
            }
        }

        private void FormatAndRenderSampleFromString(string sampleString, ContentPresenter presenter, ILanguage highlightLanguage)
        {
            // Trim out stray blank lines at start and end.
            sampleString = sampleString.TrimStart('\n').TrimEnd();

            // Also trim out spaces at the end of each line
            sampleString = string.Join("\n", sampleString.Split('\n').Select(s => s.TrimEnd()).ToArray());

            // Perform any applicable substitutions.
            sampleString = SubstitutionPattern.Replace(sampleString, match =>
            {
                foreach (var substitution in Substitutions)
                {
                    if (substitution.Key == match.Groups[1].Value)
                    {
                        return substitution.ValueAsString();
                    }
                }
                throw new KeyNotFoundException(match.Groups[1].Value);
            });

            actualCode = sampleString;

            var sampleCodeRTB = new TextBlock { FontFamily = new FontFamily("Consolas") };

            var formatter = GenerateRichTextFormatter();
            formatter.Colorize(sampleString, highlightLanguage);

            sampleCodeRTB.Text = sampleString;
            presenter.Content = sampleCodeRTB;
        }

        private CodeColorizer GenerateRichTextFormatter()
        {
            var formatter = new CodeColorizer();
            return formatter;
        }

        private void CopyCodeButton_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(actualCode);

            VisualStateManager.GoToState(this, "ConfirmationDialogVisible", false);

            // Automatically close teachingtip after 1 seconds
            this.RunOnUIThread(async () =>
            {
                await Task.Delay(1000);
                VisualStateManager.GoToState(this, "ConfirmationDialogHidden", false);
            });
        }
    }
}
