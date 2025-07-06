using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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
using Ookii.Dialogs.Wpf;

namespace PortableInstaller
{
    public partial class MainWindow : Window
    {
        private readonly InstallerOptions _options;
        public MainWindow(InstallerOptions options)
        {
            InitializeComponent();
            _options = options;
            if (!string.IsNullOrEmpty(options.TargetPath))
                PathTextBox.Text = options.TargetPath;
        }

        private void Browse_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new Ookii.Dialogs.Wpf.VistaFolderBrowserDialog();
            if (dlg.ShowDialog() == true)
                PathTextBox.Text = dlg.SelectedPath;
        }

        public async void BeginInstall()
        {
            string target = PathTextBox.Text;
            if (string.IsNullOrEmpty(target) || !Directory.Exists(target))
            {
                System.Windows.MessageBox.Show("Please select a valid target folder.");
                return;
            }

            StatusText.Text = "Installing...";
            // Simulate install steps
            for (int i = 0; i <= 100; i += 20)
            {
                InstallProgressBar.Value = i;
                await Task.Delay(300);
            }

            // TODO: copy files from embedded resources or unpack from zip
            // TODO: create icons if _options.WriteIcons is true

            StatusText.Text = "Completed";
        }
    }
}
