using ModernWpf;
using ModernWpf.Controls;
using System.Windows;

namespace ModernWpfMessageBox.Test
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            string title = "Some title";
            string message = "This is a looooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooong test text!";

            System.Windows.MessageBox.Show(message, title, MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
            System.Windows.MessageBox.Show("adawdawda", title, MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
            ShutdownMode = ShutdownMode.OnExplicitShutdown;
            ModernWpf.Controls.MessageBox.Show("This is a test text!", "Some title", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
            ModernWpf.Controls.MessageBox.Show(message, title, MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
            ModernWpf.Controls.MessageBox.Show("redadwada", null, MessageBoxButton.OK, ((char)Symbol.Admin).ToString());
            ModernWpf.Controls.MessageBox.Show("redadwada", null, MessageBoxButton.OK, SymbolGlyph.Airplane);
            ModernWpf.Controls.MessageBox.Show("redadwada", null, MessageBoxButton.OK, SymbolGlyph.Airplane, MessageBoxResult.OK);
            ModernWpf.Controls.MessageBox.ShowAsync(message, title, MessageBoxButton.YesNoCancel, MessageBoxImage.Question).GetAwaiter().GetResult();
            ModernWpf.Controls.MessageBox.ShowAsync(message, title, MessageBoxButton.YesNoCancel, MessageBoxImage.Hand, MessageBoxResult.Cancel).GetAwaiter().GetResult();
            ModernWpf.Controls.MessageBox.EnableLocalization = false;
            ModernWpf.Controls.MessageBox.ShowAsync("Press Alt and you should see underscores!", null, MessageBoxButton.YesNoCancel, MessageBoxImage.Hand).GetAwaiter().GetResult();
            Shutdown();
        }
    }
}
