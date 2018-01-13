/*
 * Media Extractor is an application to preview and extract packed media in Microsoft Office files (e.g. Word, PowerPoint or Excel documents)
 * Copyright Raphael Stoeckli © 2018
 * This program is licensed under the MIT License.
 * You find a copy of the license in project folder or on: http://opensource.org/licenses/MIT
 */

using System;

namespace MediaExtractor
{
    /// <summary>
    /// Class to represent a ListView row / entry
    /// </summary>
    public class ListViewItem
    {
        /// <summary>
        /// Enum to define the coarse file type of the entry
        /// </summary>
        public enum FileType
        {
            /// <summary>Entry is an image</summary>
            image,
            /// <summary>Entry is not an image</summary>
            other,
            /// <summary>Entry no file at all / error</summary>
            none,
        }

        /// <summary>
        /// File name of the entry
        /// </summary>
        public string FileName { get; set; }
        /// <summary>
        /// File extension of the entry
        /// </summary>
        public string FileExtension { get; set; }
        /// <summary>
        /// Coarse file type of the entry
        /// </summary>
        public FileType Type { get; set; }
        /// <summary>
        /// Reference to the ExtractorItem of the entry
        /// </summary>
        public Extractor.ExtractorItem FileReference { get; set; }
    }
}
