using System;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using ModernWpf.SampleApp.Controls.Primitives;

namespace ModernWpf.SampleApp.Controls
{
    public delegate Visual CreateContentFunction();

    public class BackgroundVisualHost : BackgroundVisualHostBase
    {
        public BackgroundVisualHost()
        {
        }

        public BackgroundVisualHost(Dispatcher dispatcher) : base(dispatcher)
        {
        }

        public event EventHandler<CreatingContentEventArgs> CreatingContent;

        protected override Visual CreateContent()
        {
            var args = new CreatingContentEventArgs();
            CreatingContent?.Invoke(this, args);
            return args.Content;
        }
    }

    public class CreatingContentEventArgs : EventArgs
    {
        public Visual Content { get; set; }
    }
}
