/*
 * Media Extractor is an application to preview and extract packed media in Microsoft Office files (e.g. Word, PowerPoint or Excel documents)
 * Copyright Raphael Stoeckli © 2018
 * This program is licensed under the MIT License.
 * You find a copy of the license in project folder or on: http://opensource.org/licenses/MIT
 */

namespace MediaExtractor
{
    /// <summary>
    /// Class to represent a ListView row / entry
    /// </summary>
    public class ListViewItem
    {

        /// <summary>
        /// File name of the entry
        /// </summary>
        public string FileName { get; set; }
        /// <summary>
        /// File extension of the entry
        /// </summary>
        public string FileExtension { get; set; }

        /// <summary>
        /// Relative path of the file
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// Coarse file type of the entry
        /// </summary>
        public ExtractorItem.Type Type { get; set; }
        /// <summary>
        /// Reference to the ExtractorItem of the entry
        /// </summary>
        public ExtractorItem FileReference { get; set; }

    }
}
