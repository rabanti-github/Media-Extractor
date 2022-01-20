/*
 * Media Extractor is an application to preview and extract packed media in Microsoft Office files (e.g. Word, PowerPoint or Excel documents)
 * Copyright Raphael Stoeckli © 2022
 * This program is licensed under the MIT License.
 * You find a copy of the license in project folder or on: http://opensource.org/licenses/MIT
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Media.Imaging;

namespace MediaExtractor
{
    /// <summary>
    /// Class to handle particular embedded files in a archive or file
    /// </summary>
    public class ExtractorItem
    {

        private static readonly char[] EXT_SPLITTERS = new char[] { ',', ';', ' ', '.', '/', '\\', '|' };

        private static List<string> imageExtensions;
        private static List<string> textExtensions;
        private static List<string> xmlExtensions;

        /// <summary>
        /// Default file endings that are previewed as text
        /// </summary>
        public const string FALLBACK_TEXT_EXTENTIONS = "asc,bas,bat,c,cfg,cmd,cpp,cs,css,csv,h,hex,htm,html,inc,inf,info,ini,java,js,json,kt,ktm,kts,latex,less,lisp,log,lst,lua,markdown,md,me,meta,mf,p,pas,php,pl,pp,ps,ps1,psm1,py,r,rb,readme,reg,rs,rst,sh,sln,sql,sty,tcl,tex,ts,tsx,txt,vb,vba,vbs,yaml,yml";
        /// <summary>
        /// Default file endings that are previewed as image
        /// </summary>
        public const string FALLBACK_IMAGE_EXTENTIONS = "jpg,jpeg,png,wmf,emf,gif,bmp,ico,wdp";
        /// <summary>
        /// Default file endings that are previewed as XML
        /// </summary>
        public const string FALLBACK_XML_EXTENTIONS = "xml,manifest,rels,xhtml,xaml,svg,pom,dtd,xsd,x3d,collada,cdxml,config,nuspec,graphml";

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

        private string errorMessage;
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
                if (image == null && !initialized)
                {
                    ValidImage = Preview.CreateImage(FileExtension, Stream, ShowGenericText, out image, out errorMessage, out genericText);
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
                if (genericText == null && !initialized)
                {
                    if (ItemType == Type.Text || ShowGenericText)
                    {
                        ValidGenericText = Preview.CreateText(Stream, out genericText, out errorMessage);
                        initialized = true;
                    }
                    else if (ItemType == Type.Xml)
                    {
                        ValidGenericText = Preview.CreateXml(Stream, ShowGenericText, out genericText, out errorMessage);
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
        public string ErrorMessage
        {
            get { return errorMessage; }
            set { errorMessage = value; }
        }

        /// <summary>
        /// Gets or sets whether unknown file formats, as well as invalid formats, will be displayed as text
        /// </summary>
        public bool ShowGenericText { get; set; }

        /// <summary>
        /// Constructor with parameters
        /// </summary>
        /// <param name="fileName">File name of the item</param>
        /// <param name="stream">Passed memoryStream of the item</param>
        /// <param name="path">Relative path within the archive / file</param>
        /// <param name="showGenericText">If true, unknown file formats, as well as invalid formats, will be displayed as text</param>
        public ExtractorItem(string fileName, MemoryStream stream, string path, bool showGenericText)
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
            ShowGenericText = showGenericText;
            FileName = fileName;
            Path = path;
            Stream = stream;
            ErrorMessage = String.Empty;
        }

        /// <summary>
        /// Meta method to create an image preview
        /// </summary>
        public void CreateImage()
        {
            ValidImage = Preview.CreateImage(FileExtension, Stream, ShowGenericText, out image, out errorMessage, out genericText);
            initialized = true;
        }

        /// <summary>
        /// Meta method to create a XML preview
        /// </summary>
        public void CreateXml()
        {
            ValidGenericText = Preview.CreateXml(Stream, ShowGenericText,  out genericText, out errorMessage);
            initialized = true;
        }

        /// <summary>
        /// Meta method to create a text preview
        /// </summary>
        public void CreateText()
        {
            ValidGenericText = Preview.CreateText(Stream, out genericText, out errorMessage);
            initialized = true;
        }

        /// <summary>
        /// Invalidates (resets) the current item
        /// </summary>
        /// <param name="showGenericText">If true, unknown file formats, as well as invalid formats, will be displayed as text</param>
        public void Invalidate(bool showGenericText)
        {
            if (ValidImage || ValidGenericText)
            {
                return;
            }
            ValidGenericText = false;
            ValidImage = false;
            initialized = false;
            genericText = null;
            image = null;
            ShowGenericText = showGenericText;
        }

        /// <summary>
        /// Method to create valid extensions for the previews
        /// </summary>
        /// <param name="texts">Raw string of separated text extensions</param>
        /// <param name="images">Raw string of separated image extensions</param>
        /// <param name="xml">Raw string of separated XML extensions</param>
        /// <returns>True if all extensions could be resolved, otherwise false</returns>
        public static bool GetExtensions(string texts, string images, string xml)
        {
            try
            {
                textExtensions = SplitExtensions(texts);
                imageExtensions = SplitExtensions(images);
                xmlExtensions = SplitExtensions(xml);
                return true;
            }
            catch
            {
                textExtensions = SplitExtensions(FALLBACK_TEXT_EXTENTIONS);
                imageExtensions = SplitExtensions(FALLBACK_IMAGE_EXTENTIONS);
                xmlExtensions = SplitExtensions(FALLBACK_XML_EXTENTIONS);
                return false;
            }
        }

        /// <summary>
        /// Method to split a raw string of file extensions into a list
        /// </summary>
        /// <param name="input">input string</param>
        /// <returns>List of lowercase, distinct file extensions</returns>
        private static List<string> SplitExtensions(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                throw new ArgumentException("Undefined extension");
            }
            string[] split = input.ToLower().Split(EXT_SPLITTERS, StringSplitOptions.RemoveEmptyEntries);
            return split.Distinct().ToList();
        }

        /// <summary>
        /// Gets the appropriate, generic type of the file
        /// </summary>
        /// <param name="extension">Extension of the file</param>
        /// <returns>Generic file type</returns>
        private static Type GetExtensionType(string extension)
        {
            string ext = extension.ToLower();
            if (textExtensions.Contains(ext))
            {
                return Type.Text;
            }
            if (xmlExtensions.Contains(ext))
            {
                return Type.Xml;
            }
            if (imageExtensions.Contains(ext))
            {
                return Type.Image;
            }
            return Type.Other;
        }

    }
}