/*
 * Media Extractor is an application to preview and extract packed media in Microsoft Office files (e.g. Word, PowerPoint or Excel documents)
 * Copyright Raphael Stoeckli © 2020
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
        /// Constructor with parameters
        /// </summary>
        /// <param name="file">name of the archive or file</param>
        /// <param name="model">ViewModel for data binding</param>
        public Extractor(string file, ViewModel model)
        {
            FileName = file;
            lastError = "";
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
        public void Extract()
        {
            try
            {
                MemoryStream ms = GetFileStream();
                ArchiveFile ex = new ArchiveFile(ms, SevenZipFormat.Zip);
                embeddedFiles = GetEntries(ref ex);
                currentModel.NumberOfFiles = embeddedFiles.Count;
                for (int i = 0; i < currentModel.NumberOfFiles; i++)
                {
                    switch (embeddedFiles[i].ItemType)
                    {
                        case ExtractorItem.Type.Image:
                            embeddedFiles[i].CreateImage(true);
                            break;
                        case ExtractorItem.Type.Xml:
                            embeddedFiles[i].CreateXml();
                            break;
                        case ExtractorItem.Type.Text:
                            embeddedFiles[i].CreateText();
                            break;
                    }
                    currentModel.CurrentFile = i + 1;
                }
            }
            catch (Exception e)
            {
                hasErrors = true;
                lastError = e.Message;
            }
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
                if (item.FileName == filename && item.ItemType == ExtractorItem.Type.Image)
                {
                    image = item.Image;
                    if (!item.ValidImage)
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
            lastError = I18n.T(I18n.Key.TextErrorInvalidImage);
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
                if (item.FileName == filename && (item.ItemType == ExtractorItem.Type.Text || item.ItemType == ExtractorItem.Type.Xml))
                {
                    genericText = item.GenericText;
                    if (!item.ValidGenericText)
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
            lastError = I18n.T(I18n.Key.TextErrorInvalidText);
            hasErrors = true;
            return false;
        }

        /// <summary>
        /// Method to get all embedded files as list according to the target image / file (format) type
        /// </summary>
        /// <param name="archive">Reference to the opened file (handled as archive)</param>
        /// <returns>A list of items</returns>
        private List<ExtractorItem> GetEntries(ref ArchiveFile archive)
        {
            List<ExtractorItem> list = new List<ExtractorItem>();
            MemoryStream ms;
            string[] split;
            char[] delimiter = new char[] { '\\', '/' };
            string file, path;
            ExtractorItem item;
            for (int i = 0; i < archive.Entries.Count; i++)
            {
                if (archive.Entries[i].IsFolder)
                {
                    continue; // Skip folders as entries
                }
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
