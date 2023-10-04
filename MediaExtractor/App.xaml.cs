/*
 * Media Extractor is an application to preview and extract packed media in Microsoft Office files (e.g. Word, PowerPoint or Excel documents)
 * Copyright Raphael Stoeckli © 2023
 * This program is licensed under the MIT License.
 * You find a copy of the license in project folder or on: http://opensource.org/licenses/MIT
 */

using System.Globalization;
using System.Threading;
using System.Windows;

namespace MediaExtractor
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {

        /// <summary>
        /// Method executed on application startup
        /// </summary>
        /// <param name="e">Application arguments</param>
        protected override void OnStartup(StartupEventArgs e)
        {
            string locale = HandleStartupLocale();
            MainWindow window = new MediaExtractor.MainWindow();
            window.RestoreSettings();
            I18n.MatchLocale(window.CurrentModel, locale);
            window.CurrentLocale = locale;
            window.Show();
        }


        /// <summary>
        /// Handles the locale of the application on startup
        /// </summary>
        /// <returns>Locale from settings if available, otherwise system default locale</returns>
        private string HandleStartupLocale()
        {
            string locale = MediaExtractor.Properties.Settings.Default.Locale;
            if (string.IsNullOrEmpty(locale))
            {
                locale = I18n.GetSystemLocale();
            }

            Thread.CurrentThread.CurrentUICulture = new CultureInfo(locale);
            return locale;
        }


    }
}
