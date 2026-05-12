namespace ModernWpf.Gallery
{
    public partial class App
    {
        protected override void OnStartup(System.Windows.StartupEventArgs e)
        {
            base.OnStartup(e);

            var window = new MainWindow();
            MainWindow = window;
            window.Show();

            if (e.Args.Length > 0)
            {
                window.NavigateTo(e.Args[0]);
            }
        }
    }
}
