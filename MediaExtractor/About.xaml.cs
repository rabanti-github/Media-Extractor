using System;
using System.Reflection;
using System.Windows;
using System.Windows.Input;

namespace MediaExtractor
{
    /// <summary>
    /// Interaction logic for About.xaml
    /// </summary>
    public partial class About
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="About"/> class.
        /// </summary>
        public About()
        {
            InitializeComponent();
            Assembly assembly = Assembly.GetExecutingAssembly();
            Version v = assembly.GetName().Version;
            VersionLabel.Content = v.ToString();
            DateTime date = Utils.RetrieveLinkerTimestamp(assembly.Location);
            DateLabel.Content = date.ToString("d");
        }

        /// <summary>
        /// Opens the project website
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="MouseButtonEventArgs"/> instance containing the event data.</param>
        private void LinkLabel_MouseUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start(Properties.Settings.Default.Website);
            }
            catch
            {
                MessageBox.Show(I18n.T(I18n.Key.DialogMissingWebsite), I18n.T(I18n.Key.DialogMissingWebsiteTitle), MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        /// <summary>
        /// Closes the about window
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
