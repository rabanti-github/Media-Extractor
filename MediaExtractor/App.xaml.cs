using System.Windows;

namespace MediaExtractor
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {

        protected override void OnStartup(StartupEventArgs e)
        {
            MainWindow window = new MediaExtractor.MainWindow();
            window.Show();
        }

    }
}
