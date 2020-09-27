/*
 * Media Extractor is an application to preview and extract packed media in Microsoft Office files (e.g. Word, PowerPoint or Excel documents)
 * Copyright Raphael Stoeckli © 2020
 * This program is licensed under the MIT License.
 * You find a copy of the license in project folder or on: http://opensource.org/licenses/MIT
 */

namespace MediaExtractor
{
    /// <summary>
    /// Class for Internationalization (I18n) 
    /// </summary>
    public class I18n
    {

        /// <summary>
        /// Locale identifier for English (en-US)
        /// </summary>
        public const string ENGLISH = "en";
        /// <summary>
        /// Locale identifier for German (de-DE)
        /// </summary>
        public const string GERMAN = "de-DE";

        /// <summary>
        /// Method to set the current locale in the view model
        /// </summary>
        /// <param name="viewModel">View model instance</param>
        /// <param name="currentLocale">Current locale as string</param>
        public static void MatchLocale(ViewModel viewModel, string currentLocale)
        {
            switch (currentLocale)
            {
                case ENGLISH:
                    viewModel.UseEnglishLocale = true;
                    viewModel.UseGermanLocale = false;
                    break;
                case GERMAN:
                    viewModel.UseEnglishLocale = false;
                    viewModel.UseGermanLocale = true;
                    break;
                default:
                    viewModel.UseEnglishLocale = true;
                    viewModel.UseGermanLocale = false;
                    break;
            }
        }

        /// <summary>
        /// Method to simply translate a string without any parameter
        /// </summary>
        /// <param name="key">Resource name of the translation</param>
        /// <returns>Translated term</returns>
        public static string T(string key)
        {
            return Properties.Resources.ResourceManager.GetString(key);
        }

        /// <summary>
        /// Method to translate a string, using parameters, to be replaces inb 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static string R(string key, params string[] parameters)
        {
            string localized = T(key);
            return string.Format(localized, parameters);
        }

        public static string R(string key, params int[] parameters)
        {
            string localized = T(key);
            return string.Format(localized, parameters);
        }

    }
}
