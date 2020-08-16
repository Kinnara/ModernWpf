using System.Threading;
using System.Windows;
using System.Windows.Controls;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace MUXControlsTestApp.Samples
{
    public sealed partial class DisposableUserControl : UserControl
    {

        public int Number
        {
            get { return (int)GetValue(NumberProperty); }
            set { SetValue(NumberProperty, value); }
        }

        public static readonly DependencyProperty NumberProperty =
            DependencyProperty.Register("Number", typeof(int), typeof(DisposableUserControl), new PropertyMetadata(-1));


        public static int OpenItems { get { return _counter; } }
        private static int _counter = 0;

        public DisposableUserControl()
        {
            Interlocked.Increment(ref _counter);
            this.InitializeComponent();
        }

        ~DisposableUserControl()
        {
            Interlocked.Decrement(ref _counter);
        }
    }
}
