
/*
 * Media Extractor is an application to preview and extract packed media in Microsoft Office files (e.g. Word, PowerPoint or Excel documents)
 * Copyright Raphael Stoeckli © 2022
 * This program is licensed under the MIT License.
 * You find a copy of the license in project folder or on: http://opensource.org/licenses/MIT
 */

namespace TranslationHelper
{
    /// <summary>
    /// Class representing a translation tuple with key, default value, translation value and comment
    /// </summary>
    public class TranslationItem
    {
        private string translatedValue;
        private string comment;

        /// <summary>
        /// Unique key of the translation item
        /// </summary>
        public string Key { get; set; }
        public string DefaultValue { get; set; }
        /// <summary>
        /// Default value (usually in English) of the translation item. Will be returned as empty sting, if null
        /// </summary>
        public string TranslatedValue
        {
            get
            {
                if (translatedValue == null) { return ""; }
                else { return translatedValue; }
            }
            set => translatedValue = value;
        }
        /// <summary>
        /// Comment of the translation item. Will be returned as empty sting, if null
        /// </summary>
        public string Comment
        {
            get
            {
                if (comment == null) { return ""; }
                else { return comment; }
            }
            set => comment = value;
        }

        /// <summary>
        /// Default constructor
        /// </summary>
        public TranslationItem()
        {

        }

    }
}
