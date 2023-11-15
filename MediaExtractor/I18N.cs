/*
 * Media Extractor is an application to preview and extract packed media in Microsoft Office files (e.g. Word, PowerPoint or Excel documents)
 * Copyright Raphael Stoeckli © 2023
 * This program is licensed under the MIT License.
 * You find a copy of the license in project folder or on: http://opensource.org/licenses/MIT
 */

using System;
using System.Globalization;
using System.Linq;

namespace MediaExtractor
{
    /// <summary>
    /// Class for Internationalization (I18n) 
    /// </summary>
    public class I18n
    {
        /// <summary>
        /// Keys of the I18n resources. The keys must EXACTLY math with the names in the resx files 
        /// </summary>
        public enum Key
        {
            AboutAuthor,
            AboutCloseButton,
            AboutDate,
            AboutDescription,
            AboutHeader,
            AboutLicense,
            AboutVersion,
            AboutWebsite,
            ButtonOpen,
            ButtonSave,
            ButtonSaveAll,
            ButtonSaveSelected,
            CrcDescription,
            DialogButtonCancel,
            DialogButtonCancelTooltip,
            DialogButtonOverwrite,
            DialogButtonOverwriteTooltip,
            DialogButtonRename,
            DialogButtonRenameTooltip,
            DialogButtonSkip,
            DialogButtonSkipTooltip,
            DialogCancelTitle,
            DialogErrorTitle,
            DialogExplorerError,
            DialogHeader,
            DialogInvalidExtensions,
            DialogLabelCrc,
            DialogLabelDate,
            DialogLabelExisting,
            DialogLabelName,
            DialogLabelNew,
            DialogLabelSize,
            DialogLoadFailure,
            DialogLoadFilter,
            DialogLoadTitle,
            DialogMissingChangelog,
            DialogMissingChangelogTitle,
            DialogMissingLicense,
            DialogMissingLicenseTitle,
            DialogMissingRecentFile,
            DialogMissingWebsite,
            DialogMissingWebsiteTitle,
            DialogRememberCheckbox,
            DialogSaveAllTitle,
            DialogSaveCurrentTitle,
            DialogSaveErrors,
            DialogSaveFailure,
            DialogSaveFilter,
            DialogSaveSelectedTitle,
            DialogSizeWarning,
            DialogSizeWarningTitle,
            DialogUnexpectedError,
            DropAreaWatermark,
            LabelListview,
            LabelPreview,
            ListViewColumnExtension,
            ListViewColumnName,
            ListViewColumnPath,
            ListViewColumnSize,
            MenuAppearance,
            MenuAppearanceDarkmode,
            MenuAppearanceLanguage,
            MenuAppearanceLanguageDefault,
            MenuAppearanceLanguageEnglish,
            MenuAppearanceLanguageGerman,
            MenuAppearanceLanguageFrench,
            MenuAppearanceLanguageSpanish,
            MenuDocument,
            MenuDocumentGenerictextPreview,
            MenuDocumentSizeWarning,
            MenuDocumentKeepStructure,
            MenuDocumentOpenExplorer,
            MenuDocumentShowImages,
            MenuDocumentShowOther,
            MenuFile,
            MenuFileClearRecent,
            MenuFileOpen,
            MenuFileQuit,
            MenuFileRecent,
            MenuFileSaveAll,
            MenuFileSaveSelected,
            MenuHelp,
            MenuHelpAbout,
            MenuHelpChangeLog,
            MenuHelpLicense,
            MenuHelpWebsite,
            StatusEmbeddedLoaded,
            StatusLoaded,
            StatusLoadEmbeddedImageFailure,
            StatusLoadEmbeddedImageFallback,
            StatusLoadEmbeddedOtherFallback,
            StatusLoadEmbeddedOtherFailure,
            StatusLoadEmbeddedTextFailure,
            StatusLoadFailure,
            StatusLoading,
            StatusLoadingEmbedded,
            StatusPreviewSkipped,
            StatusReady,
            StatusSaveCanceled,
            StatusSaveErrorSummary,
            StatusSaveFailure,
            StatusSaveSuccess,
            StatusSaveSummary,
            TextErrorInvalidImage,
            TextErrorInvalidText,
            TextErrorMultipleFiles,
            TextErrorOneFile,
            TextInvalidFormat,
            TextInvalidPath,
            TextLockedFile,
            TextNoPreview,
            TextSaveError,
            TextSkippedMultipleFiles,
            TextSkippedOneFile,
        }

        /// <summary>
        /// Locale identifier for English (en-US)
        /// </summary>
        public const string ENGLISH = "en";
        /// <summary>
        /// Locale identifier for German (de-DE)
        /// </summary>
        public const string GERMAN = "de-DE";
        /// <summary>
        /// Locale identifier for French (fr-FR)
        /// </summary>
        public const string FRENCH = "fr-FR";
        /// <summary>
        /// Locale identifier for Spanish (es-ES)
        /// </summary>
        public const string SPANISH = "es-ES";

        /// <summary>
        /// Method to set the current locale in the view model
        /// </summary>
        /// <param name="viewModel">View model instance</param>
        /// <param name="currentLocale">Current locale as string</param>
        public static void MatchLocale(ViewModel viewModel, string currentLocale)
        {
            string systemLocale = GetSystemLocale();
            if (currentLocale == systemLocale)
            {
                SetSystemLocale(viewModel);
                return;
            }
            switch (currentLocale)
            {
                case ENGLISH:
                    viewModel.UseEnglishLocale = true;
                    viewModel.UseGermanLocale = false;
                    viewModel.UseFrenchLocale = false;
                    viewModel.UseSpanishLocale = false;
                    viewModel.UseSystemLocale = false;
                    break;
                case GERMAN:
                    viewModel.UseEnglishLocale = false;
                    viewModel.UseGermanLocale = true;
                    viewModel.UseFrenchLocale = false;
                    viewModel.UseSpanishLocale = false;
                    viewModel.UseSystemLocale = false;
                    break;
                case FRENCH:
                    viewModel.UseEnglishLocale = false;
                    viewModel.UseGermanLocale = false;
                    viewModel.UseFrenchLocale = true;
                    viewModel.UseSpanishLocale = false;
                    viewModel.UseSystemLocale = false;
                    break;
                case SPANISH:
                    viewModel.UseEnglishLocale = false;
                    viewModel.UseGermanLocale = false;
                    viewModel.UseFrenchLocale = false;
                    viewModel.UseSpanishLocale = true;
                    viewModel.UseSystemLocale = false;
                    break;
                default:
                    SetSystemLocale(viewModel);
                    break;
            }
        }

        /// <summary>
        /// Method to determine the system locale
        /// </summary>
        /// <returns>System locale as string</returns>
        public static string GetSystemLocale()
        {
            CultureInfo ci = CultureInfo.InstalledUICulture;
            return ci.Name;
        }

        /// <summary>
        /// Method to simply translate a string without any parameter
        /// </summary>
        /// <param name="key">Resource name of the translation</param>
        /// <returns>Translated term</returns>
        public static string T(Key key)
        {
            string keyString = Enum.GetName(typeof(Key), key);
            return Properties.Resources.ResourceManager.GetString(keyString);
        }

        /// <summary>
        /// Method to translate a string, using parameters, to be replaced in the translated text 
        /// </summary>
        /// <param name="key">Resource name of the translation</param>
        /// <param name="parameters">Params string array</param>
        /// <returns>Translated term with replaced text</returns>
        public static string R(Key key, params string[] parameters)
        {
            string localized = T(key).Replace("\\n", Environment.NewLine);
            return string.Format(localized, parameters);
        }

        /// <summary>
        /// Method to translate a string, using integer parameters, to be replaced in the translated text 
        /// </summary>
        /// <param name="key">Resource name of the translation</param>
        /// <param name="parameters">Params int array</param>
        /// <returns>Translated term with replaced text</returns>
        public static string R(Key key, params int[] parameters)
        {
            string[] numbers = parameters.Select(x => x.ToString()).ToArray();
            return R(key, numbers);
        }

        /// <summary>
        /// Method to set the system locale as application language
        /// </summary>
        /// <param name="viewModel">View model of the application</param>
        private static void SetSystemLocale(ViewModel viewModel)
        {
            viewModel.UseEnglishLocale = false;
            viewModel.UseGermanLocale = false;
            viewModel.UseFrenchLocale = false;
            viewModel.UseSpanishLocale = false;
            viewModel.UseSystemLocale = true;
        }

    }
}
