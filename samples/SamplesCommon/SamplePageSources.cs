using System;

namespace SamplesCommon
{
    public static class SamplePageSources
    {
        public static Uri SamplePage1 { get; } = GetUri(nameof(SamplePage1));
        public static Uri SamplePage2 { get; } = GetUri(nameof(SamplePage2));
        public static Uri SamplePage3 { get; } = GetUri(nameof(SamplePage3));
        public static Uri SamplePage4 { get; } = GetUri(nameof(SamplePage4));

        private static Uri GetUri(string pageName)
        {
            return new Uri($"SamplesCommon;component/SamplePages/{pageName}.xaml", UriKind.Relative);
        }
    }
}
