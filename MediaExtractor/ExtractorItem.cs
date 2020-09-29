/*
 * Media Extractor is an application to preview and extract packed media in Microsoft Office files (e.g. Word, PowerPoint or Excel documents)
 * Copyright Raphael Stoeckli © 2020
 * This program is licensed under the MIT License.
 * You find a copy of the license in project folder or on: http://opensource.org/licenses/MIT
 */

using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Text;
using System.Windows.Media.Imaging;
using System.Xml;

namespace MediaExtractor
{
    /// <summary>
    /// Class to handle particular embedded files in a archive or file
    /// </summary>
    public class ExtractorItem
    {
        /// <summary>
        /// Enum to define the coarse file type of the entry
        /// </summary>
        public enum Type
        {
            /// <summary>Entry is an image</summary>
            Image,
            /// <summary>Entry is an XML file</summary>
            Xml,
            /// <summary>Entry is a text file</summary>
            Text,
            /// <summary>Entry is not an image</summary>
            Other,
            /// <summary>Entry no file at all / error</summary>
            None,
        }

        private BitmapImage image;
        private string genericText;
        private bool initialized;

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
        /// If true, the item was identified as valid XML or text file
        /// </summary>
        public bool ValidGenericText { get; set; }
        /// <summary>
        /// Generic type of the item
        /// </summary>
        public Type ItemType { get; set; }
        /// <summary>
        /// CRC32 hash of the file
        /// </summary>
        public uint Crc32 { get; set; }
        /// <summary>
        /// Size of the file in bytes
        /// </summary>
        public long FileSize { get; set; }
        /// <summary>
        /// Date and time of the last write access of the file
        /// </summary>
        public DateTime LastChange { get; set; }
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
        /// Gets the generic text (plain text or formatted XML) if the item is a text or XML file
        /// </summary>
        public string GenericText
        {
            get
            {
                if (genericText == null && initialized == false)
                {
                    if (ItemType == Type.Text)
                    {
                        CreateText();
                        initialized = true;
                    }
                    else if (ItemType == Type.Xml)
                    {
                        CreateXml();
                        initialized = true;
                    }
                    else
                    {
                        genericText = "";
                    }
                }
                return genericText;
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
        /// <param name="createFile">If true, an Image, text or XML object will be created</param>
        /// <param name="path">Relative path within the archive / file</param>
        public ExtractorItem(string fileName, MemoryStream stream, bool createFile, string path)
        {
            string[] tokens = fileName.Split('.');
            if (tokens.Length > 1)
            {
                FileExtension = tokens[tokens.Length - 1];
                ItemType = GetExtensionType(FileExtension);
            }
            else
            {
                FileExtension = "";
                ItemType = Type.Other;
            }

            FileName = fileName;
            Path = path;
            Stream = stream;
            ErrorMessage = String.Empty;

            if (createFile == true)
            {
                switch (ItemType)
                {
                    case Type.Image:
                        CreateImage(true);
                        break;
                    case Type.Xml:
                        CreateXml();
                        break;
                    case Type.Text:
                        CreateText();
                        break;
                }
                initialized = true;
            }

        }

        /// <summary>
        /// Gets the appropriate, generic type of the file
        /// </summary>
        /// <param name="extension">Extension of the file</param>
        /// <returns>Generic file type</returns>
        private static Type GetExtensionType(string extension)
        {
            string ext = extension.ToLower();
            switch (ext)
            {
                case "asc":
                case "bas":
                case "bat":
                case "c":
                case "cmd":
                case "config":
                case "cpp":
                case "cs":
                case "css":
                case "h":
                case "hex":
                case "htm":
                case "html":
                case "inc":
                case "ini":
                case "java":
                case "js":
                case "json":
                case "kt":
                case "ktm":
                case "kts":
                case "latex":
                case "lisp":
                case "log":
                case "lst":
                case "lua":
                case "md":
                case "me":
                case "mf":
                case "p":
                case "pas":
                case "php":
                case "pl":
                case "pp":
                case "ps":
                case "py":
                case "r":
                case "rb":
                case "readme":
                case "reg":
                case "rs":
                case "rst":
                case "sh":
                case "sql":
                case "sty":
                case "tcl":
                case "tex":
                case "ts":
                case "tsx":
                case "txt":
                case "vb":
                case "vba":
                case "vbs":
                case "yaml":
                case "yml":
                    return Type.Text;
            }
            switch (ext)
            {
                case "jpg":
                case "jpeg":
                case "png":
                case "wmf":
                case "emf":
                case "gif":
                case "bmp":
                case "ico":
                    return Type.Image;
            }
            switch (ext)
            {
                case "xml":
                case "rels":
                case "xhtml":
                case "svg":
                case "x3d":
                case "collada":
                case "graphml":
                    return Type.Text;
                default:
                    return Type.Other;
            }
        }

        /// <summary>
        /// Method to create plain text
        /// </summary>
        public void CreateText()
        {
            try
            {
                StreamReader sr = new StreamReader(Stream);
                genericText = sr.ReadToEnd();
                ValidGenericText = true;
                ErrorMessage = String.Empty;
            }
            catch (Exception e)
            {
                ErrorMessage = e.Message;
                ValidGenericText = false;
                genericText = string.Empty;
            }

        }

        /// <summary>
        /// Method to create XML as text
        /// </summary>
        public void CreateXml()
        {
            CreateText();
            if (ValidGenericText == false)
            {
                return;
            }
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(genericText);
                StringBuilder sb = new StringBuilder();
                TextWriter tw = new StringWriter(sb);
                XmlTextWriter xtw = new XmlTextWriter(tw);
                xtw.Formatting = Formatting.Indented;
                doc.Save(xtw);
                xtw.Flush();
                xtw.Close();
                genericText = sb.ToString();
            }
            catch (Exception e)
            {
                ErrorMessage = e.Message;
                ValidGenericText = false;
                genericText = string.Empty;
            }
        }

        /// <summary>
        /// Method to create an image object from the item
        /// </summary>
        /// <param name="retry">If false, only an attempt as png file will be performed. If true, all formats (jpg, emf, bmp, gif and wmf) will be tried after a fail of a png conversion</param>
        public void CreateImage(bool retry)
        {
            List<ImageFormat> formats = new List<ImageFormat>();
            formats.Add(ImageFormat.Png);
            if (retry == true)
            {
                formats.Add(ImageFormat.Jpeg);
                formats.Add(ImageFormat.Emf);
                formats.Add(ImageFormat.Bmp);
                formats.Add(ImageFormat.Gif);
                formats.Add(ImageFormat.Wmf);
            }
            foreach (ImageFormat format in formats)
            {
                try
                {
                    MemoryStream ms2 = new MemoryStream();
                    if (format == ImageFormat.Emf || format == ImageFormat.Wmf)
                    {
                        Metafile mf = new Metafile(Stream);
                        mf.Save(ms2, format);
                    }
                    else
                    {
                        System.Drawing.Image img = System.Drawing.Image.FromStream(Stream);
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
                    image = ims;
                    ValidImage = true;
                    ErrorMessage = String.Empty;
                    return;
                }
                catch (Exception e)
                {
                    ValidImage = false;
                    ErrorMessage = e.Message;
                    image = null;
                }
            }
        }

    }
}