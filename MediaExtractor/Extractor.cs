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
using System.Drawing.Imaging;

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
            word,
            /// <summary>File is a Microsoft Excel file (e.g. xlsx, xltx)</summary>
            excel,
            /// <summary>File is a Microsoft PowerPoint file (e.g. pptx, potx)</summary>
            powerPoint,
            /// <summary>File is in an unspecified, other format</summary>
            other,
        }

        /// <summary>
        /// Enum for the format of embedded images
        /// </summary>
        public enum ImageFormat
        {
            /// <summary>Image is a Enhanced Meta File</summary>
            emf,
            /// <summary>Image is a Windows Meta File</summary>
            wmf,
            /// <summary>Image is a Portable Network Graphic</summary>
            png,
            /// <summary>Image is a JPEG File</summary>
            jpg,
            /// <summary>Image is in an unspecified / generic format</summary>
            all,
        }

        private string lastError;
        private bool hasErrors;
        private List<ExtractorItem> images;
        private ViewModel currentModel;

        /// <summary>
        /// List of all embedded items (usually images)
        /// </summary>
        public List<ExtractorItem> Images
        {
            get { return this.images; }
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
        /// Gets the number of embedded items (usually images)
        /// </summary>
        public int NumberOfImages
        {
            get
            {
                if (this.images == null) { return 0; }
                else { return this.images.Count;  }
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
            this.FileName = file;
            this.lastError = "";
            this.DocumentFormat =  SourceFormat.other;
            this.images = new List<ExtractorItem>();
            this.currentModel = model;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="file">name of the archive or file</param>
        /// <param name="format">Format of the archive or file</param>
        /// <param name="model">ViewModel for data binding</param>
        public Extractor(string file, SourceFormat format, ViewModel model)
        {
            this.FileName = file;
            this.lastError = "";
            this.DocumentFormat = format;
            this.images = new List<ExtractorItem>();
            this.currentModel = model;
        }

        /// <summary>
        /// Method to reset the errors occurred during the processing of the archive or file
        /// </summary>
        public void ResetErrors()
        {
            this.hasErrors = false;
            this.lastError = string.Empty;
        }

        /// <summary>
        /// Method to extract an embedded file
        /// </summary>
        /// <param name="format">Format of the embedded file</param>
        public void Extract(ImageFormat format)
        {
            try
            {
                MemoryStream ms = GetFileStream();
                SevenZipExtractor.ArchiveFile ex = new ArchiveFile(ms, SevenZipFormat.Zip);
                this.images = GetEntries(format, ref ex);
                this.currentModel.NumberOfFiles = this.images.Count;
                for(int i = 0; i < this.currentModel.NumberOfFiles; i++)
                {
                    this.images[i].CreateImage(true);
                    this.currentModel.CurrentFile = i + 1;
                }
            }
            catch(Exception e)
            {
                this.hasErrors = true;
                this.lastError = e.Message;
            }
        }

        /// <summary>
        /// Method to get the names of all embedded files as list
        /// </summary>
        /// <returns>List of file names</returns>
        public List<string> GetFileNames()
        {
            List<string> output = new List<string>();
            if (this.images != null)
            {
                foreach (ExtractorItem item in this.images)
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
            foreach (ExtractorItem item in this.images)
            {
                if (item.FileName == filename)
                {
                    image = item.Image;
                    if (item.ValidImage == false)
                    {
                        this.lastError = item.ErrorMessage;
                        this.hasErrors = true;

                    }
                    else
                    {
                        return true;
                    }
                    
                }
            }
            image = null;
            this.lastError = "Image could not be created";
            this.hasErrors = true;
            return false;
        }

        /// <summary>
        /// Method to get all embedded files as list according to the target image / file (format) type
        /// </summary>
        /// <param name="format">Target format</param>
        /// <param name="archive">Reference to the opened file (handled as archive)</param>
        /// <returns>A list of items</returns>
        private List<ExtractorItem> GetEntries(ImageFormat format, ref ArchiveFile archive)
        {
            string extension = "";
            bool allFiles = false;
            switch (format)
            {
                case ImageFormat.emf:
                    extension = ".emf";
                    break;
                case ImageFormat.wmf:
                    extension = ".wmf";
                    break; 
                case ImageFormat.png:
                    extension = ".png";
                    break;
                case ImageFormat.jpg:
                    extension = ".jpg";
                    break;
                case ImageFormat.all:
                    allFiles = true;
                    break;
                default:
                    break;
            }
            List<ExtractorItem> list = new List<ExtractorItem>();
            MemoryStream ms;
            string[] split;
            char[] delimiter = new char[] { '\\', '/' };
            string file, path;
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
                    list.Add(new ExtractorItem(file, ms, false, path));
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
                FileStream fs = new FileStream(this.FileName, FileMode.Open);
                MemoryStream ms = new MemoryStream((int)fs.Length);
                fs.CopyTo(ms);
                fs.Flush();
                fs.Close();
                ms.Position = 0;
                return ms;
            }
            catch (Exception e)
            {
                this.hasErrors = true;
                this.lastError = e.Message;
                return new MemoryStream();
            }
        }

        /// <summary>
        /// Sub-class to handle particular embedded files in a archive or file
        /// </summary>
        public class ExtractorItem
        {
            /// <summary>
            /// Relative path of the item within the archive / file
            /// </summary>
            public string Path { get; set; }
            /// <summary>
            /// File extension of the item
            /// </summary>
            public string FileExtension { get; set; }
            /// <summary>
            /// File name of the item
            /// </summary>
            public string FileName { get; set; }
            /// <summary>
            /// MemoryStream of the item
            /// </summary>
            public MemoryStream Stream { get; set; }
            /// <summary>
            /// If true, the item was identified as valid image file
            /// </summary>
            public bool ValidImage { get; set; }
            /// <summary>
            /// If true, the item is described as image file (by its file extension)
            /// </summary>
            public bool IsImage { get; set; }

            private BitmapImage image;
            private bool initialized = false;

            /// <summary>
            /// Gets the Image object if the item is a valid image
            /// </summary>
            public BitmapImage Image
            {
                get 
                {
                    if (image == null && initialized == false)
                    {
                        CreateImage(true);
                        initialized = true;
                    }
                    return image; 
                }
            }
            
            /// <summary>
            /// Message of the last occurred error when processing the item
            /// </summary>
            public string ErrorMessage { get; set; }

            /// <summary>
            /// Default constructor
            /// </summary>
            public ExtractorItem()
            {

            }

            /// <summary>
            /// Constructor with parameters
            /// </summary>
            /// <param name="fileName">File name of the item</param>
            /// <param name="stream">Passed memoryStream of the item</param>
            /// <param name="createImage">If true, an Image object will be created in case of an image file</param>
            /// <param name="path">Relative path within the archive / file</param>
            public ExtractorItem(string fileName, MemoryStream stream, bool createImage, string path)
            {
                string[] tokens = fileName.Split('.');
                if (tokens.Length > 1)
                {
                    this.FileExtension = tokens[tokens.Length - 1].ToUpper();
                    if (FileExtension == "JPG" || FileExtension == "JPEG" || FileExtension == "PNG" || FileExtension == "WMF" || FileExtension == "EMF"  || FileExtension == "GIF" || FileExtension == "BMP" || FileExtension == "ICO")
                    {
                        this.IsImage = true;
                    }
                }
                else
                {
                    this.FileExtension = "";
                    this.IsImage = false;
                }


                this.FileName = fileName;
                this.Path = path;
                this.Stream = stream;
                this.ErrorMessage = string.Empty;
                this.IsImage = IsImage;
                if (createImage == true && this.IsImage == true)
                {
                    CreateImage(true);
                    this.initialized = true;
                }
            }

            /// <summary>
            /// Method to create an image object from the item
            /// </summary>
            /// <param name="retry">If false, only an attempt as png file will be proforemd. If true, all formats (jpg, emf, bmp, gif and wmf) will be tried after a fail of a png conversion</param>
            public void CreateImage(bool retry)
            {
                List<System.Drawing.Imaging.ImageFormat> formats = new List<System.Drawing.Imaging.ImageFormat>();
                formats.Add(System.Drawing.Imaging.ImageFormat.Png);
                if (retry == true)
                {
                    formats.Add(System.Drawing.Imaging.ImageFormat.Jpeg);
                    formats.Add(System.Drawing.Imaging.ImageFormat.Emf);
                    formats.Add(System.Drawing.Imaging.ImageFormat.Bmp);
                    formats.Add(System.Drawing.Imaging.ImageFormat.Gif);
                    formats.Add(System.Drawing.Imaging.ImageFormat.Wmf);
                }
                foreach (System.Drawing.Imaging.ImageFormat format in formats)
                {
                    try
                    {
                        
                        MemoryStream ms2 = new MemoryStream();
                        if (format == System.Drawing.Imaging.ImageFormat.Emf || format == System.Drawing.Imaging.ImageFormat.Wmf)
                        {
                            Metafile mf = new Metafile(this.Stream);
                            mf.Save(ms2, format);
                        }
                        else
                        {
                            System.Drawing.Image img = System.Drawing.Image.FromStream(this.Stream);
                            img.Save(ms2, format);
                        }
                        
                        ms2.Flush();
                        ms2.Position = 0;
                        BitmapImage ims = new BitmapImage();
                        ims.BeginInit();
                        ims.CacheOption = BitmapCacheOption.OnLoad;
                        ims.StreamSource = ms2;
                        ims.EndInit();
                        ims.Freeze();
                        this.image = ims;
                        this.ValidImage = true;
                        this.ErrorMessage = string.Empty;
                        return;
                    }
                    catch (Exception e)
                    {
                        this.ValidImage = false;
                        this.ErrorMessage = e.Message;
                        this.image = null;
                    }
                }
            }

        }

    }
}
