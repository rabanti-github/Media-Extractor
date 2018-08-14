/*
 * Media Extractor is an application to preview and extract packed media in Microsoft Office files (e.g. Word, PowerPoint or Excel documents)
 * Copyright Raphael Stoeckli © 2018
 * This program is licensed under the MIT License.
 * You find a copy of the license in project folder or on: http://opensource.org/licenses/MIT
 */

using System;
using System.Collections.Generic;
using SevenZipExtractor;
using System.IO;
using System.Windows.Media.Imaging;

namespace MediaExtractor
{
    /// <summary>
    /// Class for extraction and rendering of embedded files in an opened file or archive
    /// </summary>
    public class Extractor
    {
        /// <summary>
        /// Enum for the source format of the processed file or archive
        /// </summary>
        public enum SourceFormat
        {
            /// <summary>File is a Microsoft Word file (e.g. docx, dotx)</summary>
            Word,
            /// <summary>File is a Microsoft Excel file (e.g. xlsx, xltx)</summary>
            Excel,
            /// <summary>File is a Microsoft PowerPoint file (e.g. pptx, potx)</summary>
            PowerPoint,
            /// <summary>File is in an unspecified, other format</summary>
            Other,
        }

        /// <summary>
        /// Enum for the format of embedded embeddedFiles / file
        /// </summary>
        public enum EmbeddedFormat
        {
            /// <summary>Image is a Enhanced Meta File</summary>
            Emf,
            /// <summary>Image is a Windows Meta File</summary>
            Wmf,
            /// <summary>Image is a Portable Network Graphic</summary>
            Png,
            /// <summary>Image is a JPEG File</summary>
            Jpg,
            /// <summary>File is a text file</summary>
            Txt,
            /// <summary>File is a XML file</summary>
            Xml,
            /// <summary>Image / File is in an unspecified / generic format</summary>
            All,
        }

        private string lastError;
        private bool hasErrors;
        private List<ExtractorItem> embeddedFiles;
        private ViewModel currentModel;

        /// <summary>
        /// List of all embedded items (usually embeddedFiles)
        /// </summary>
        public List<ExtractorItem> EmbeddedFiles
        {
            get { return embeddedFiles; }
        }

        /// <summary>
        /// File name of the archive or file
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// Last occurred error when processing the archive or file
        /// </summary>
        public string LastError
        {
            get { return lastError; }
        }

        /// <summary>
        /// If true, errors occurred during the processing
        /// </summary>
        public bool HasErrors
        {
            get { return hasErrors; }
        }

        /// <summary>
        /// Gets the number of embedded items (usually embeddedFiles)
        /// </summary>
        public int NumberOfImages
        {
            get
            {
                if (embeddedFiles == null) { return 0; }
                else { return embeddedFiles.Count;  }
            }
        }

        /// <summary>
        /// Source format of the archive or file
        /// </summary>
        public SourceFormat DocumentFormat { get; set; }

        /// <summary>
        /// Constructor with parameters
        /// </summary>
        /// <param name="file">name of the archive or file</param>
        /// <param name="model">ViewModel for data binding</param>
        public Extractor(string file, ViewModel model)
        {
            FileName = file;
            lastError = "";
            DocumentFormat =  SourceFormat.Other;
            embeddedFiles = new List<ExtractorItem>();
            currentModel = model;
        }

        /// <summary>
        /// Constructor with parameters
        /// </summary>
        /// <param name="file">name of the archive or file</param>
        /// <param name="format">Format of the archive or file</param>
        /// <param name="model">ViewModel for data binding</param>
        public Extractor(string file, SourceFormat format, ViewModel model)
        {
            FileName = file;
            lastError = "";
            DocumentFormat = format;
            embeddedFiles = new List<ExtractorItem>();
            currentModel = model;
        }

        /// <summary>
        /// Method to reset the errors occurred during the processing of the archive or file
        /// </summary>
        public void ResetErrors()
        {
            hasErrors = false;
            lastError = string.Empty;
        }

        /// <summary>
        /// Method to extract an embedded file
        /// </summary>
        /// <param name="format">Format of the embedded file</param>
        public void Extract(EmbeddedFormat format)
        {
            try
            {
                MemoryStream ms = GetFileStream();
                ArchiveFile ex = new ArchiveFile(ms, SevenZipFormat.Zip);
                embeddedFiles = GetEntries(format, ref ex);
                currentModel.NumberOfFiles = embeddedFiles.Count;
                for(int i = 0; i < currentModel.NumberOfFiles; i++)
                {
                    if (embeddedFiles[i].IsImage)
                    {
                        embeddedFiles[i].CreateImage(true);
                    }
                    else if (embeddedFiles[i].IsXml)
                    {
                        embeddedFiles[i].CreateXml();
                    }
                    else if (embeddedFiles[i].IsText)
                    {
                        embeddedFiles[i].CreateText();
                    }
                    currentModel.CurrentFile = i + 1;
                }
            }
            catch(Exception e)
            {
                hasErrors = true;
                lastError = e.Message;
            }
        }

        /// <summary>
        /// Method to get the names of all embedded files as list
        /// </summary>
        /// <returns>List of file names</returns>
        public List<string> GetFileNames()
        {
            List<string> output = new List<string>();
            if (embeddedFiles != null)
            {
                foreach (ExtractorItem item in embeddedFiles)
                {
                    output.Add(item.FileName);
                }
            }
            return output;
        }

        /// <summary>
        /// Method to get the ImageSource of an embedded file if applicable
        /// </summary>
        /// <param name="filename">file name to process</param>
        /// <param name="image">BitmapImage object as out parameter</param>
        /// <returns>If true, the ImageSource could be created, otherwise not</returns>
        public bool GetImageSourceByName(string filename, out BitmapImage image)
        {
            foreach (ExtractorItem item in embeddedFiles)
            {
                if (item.FileName == filename && item.IsImage)
                {
                    image = item.Image;
                    if (item.ValidImage == false)
                    {
                        lastError = item.ErrorMessage;
                        hasErrors = true;
                    }
                    else
                    {
                        return true;
                    }
                    
                }
            }
            image = null;
            lastError = "Image could not be created";
            hasErrors = true;
            return false;
        }

        /// <summary>
        /// Method to get the generic text (plain text or XML) of an embedded file if applicable
        /// </summary>
        /// <param name="filename">file name to process</param>
        /// <param name="genericText">Text as out parameter</param>
        /// <returns>If true, the generic text could be created, otherwise not</returns>
        public bool GetGenericTextByName(string filename, out string genericText)
        {
            foreach (ExtractorItem item in embeddedFiles)
            {
                if (item.FileName == filename && (item.IsText || item.IsXml))
                {
                    genericText = item.GenericText;
                    if (item.ValidGenericText == false)
                    {
                        lastError = item.ErrorMessage;
                        hasErrors = true;
                    }
                    else
                    {
                        return true;
                    }

                }
            }
            genericText = string.Empty;
            lastError = "Text preview could not be created";
            hasErrors = true;
            return false;
        }

        /// <summary>
        /// Method to get all embedded files as list according to the target image / file (format) type
        /// </summary>
        /// <param name="format">Target format</param>
        /// <param name="archive">Reference to the opened file (handled as archive)</param>
        /// <returns>A list of items</returns>
        private List<ExtractorItem> GetEntries(EmbeddedFormat format, ref ArchiveFile archive)
        {
            string extension = "";
            bool allFiles = false;
            switch (format)
            {
                case EmbeddedFormat.Emf:
                    extension = ".emf";
                    break;
                case EmbeddedFormat.Wmf:
                    extension = ".wmf";
                    break; 
                case EmbeddedFormat.Png:
                    extension = ".png";
                    break;
                case EmbeddedFormat.Jpg:
                    extension = ".jpg";
                    break;
                case EmbeddedFormat.All:
                    allFiles = true;
                    break;
            }
            List<ExtractorItem> list = new List<ExtractorItem>();
            MemoryStream ms;
            string[] split;
            char[] delimiter = new char[] { '\\', '/' };
            string file, path;
            ExtractorItem item;
            for(int i = 0; i < archive.Entries.Count; i++)
            {
                if ((archive.Entries[i].IsFolder == false && archive.Entries[i].FileName.ToLower().EndsWith(extension)) || allFiles == true)
                {
                    ms = new MemoryStream();
                    archive.Entries[i].Extract(ms);
                    ms.Flush();
                    ms.Position = 0;
                    split = archive.Entries[i].FileName.Split(delimiter);
                    file = split[split.Length - 1];
                    path = archive.Entries[i].FileName.Substring(0, archive.Entries[i].FileName.Length - file.Length);

                    item = new ExtractorItem(file, ms, false, path);
                    item.Crc32 = archive.Entries[i].CRC;
                    item.FileSize = (long)archive.Entries[i].Size;
                    item.LastChange = archive.Entries[i].LastWriteTime;
                    list.Add(item);
                }
            }
            return list;
        }

        /// <summary>
        /// Method to get the MemoryStream of the archive or file
        /// </summary>
        /// <returns>MemoryStream of the file. In case of an error, an empty stream will be returned</returns>
        private MemoryStream GetFileStream()
        {
            try
            {
                FileStream fs = new FileStream(FileName, FileMode.Open);
                MemoryStream ms = new MemoryStream((int)fs.Length);
                fs.CopyTo(ms);
                fs.Flush();
                fs.Close();
                ms.Position = 0;
                return ms;
            }
            catch (Exception e)
            {
                hasErrors = true;
                lastError = e.Message;
                return new MemoryStream();
            }
        }
    }
}
