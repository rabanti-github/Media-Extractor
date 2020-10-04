/*
 * Media Extractor is an application to preview and extract packed media in Microsoft Office files (e.g. Word, PowerPoint or Excel documents)
 * Copyright Raphael Stoeckli © 2020
 * This program is licensed under the MIT License.
 * You find a copy of the license in project folder or on: http://opensource.org/licenses/MIT
 */

using MediaExtractor.preview;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Xml;

namespace MediaExtractor
{
    /// <summary>
    /// Class to handle particular embedded files in a archive or file
    /// </summary>
    public class ExtractorItem
    {
        private static Dictionary<string, Func<MemoryStream, MemoryStream>> imageConverters = null;
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

        //private BitmapImage image;
        //private string genericText;
        //private bool initialized;
        ImagePreview imagePreview;
        TextPreview textPreview;
        XmlPreview xmlPreview;
        private bool validImage;
        private bool validGenericText;

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
        public bool ValidImage
        {
            get { return validImage; }
            private set { validImage = value; }
        }
        /// <summary>
        /// If true, the item was identified as valid XML or text file
        /// </summary>
        public bool ValidGenericText 
        {
            get { return validGenericText; }
            private set { validGenericText = value; }
        }
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
                return GetPreview<ImagePreview, BitmapImage>(ref imagePreview, ref validImage, ImagePreview.Initialize);
            }
        }

        /// <summary>
        /// Gets the generic text (plain text or formatted XML) if the item is a text or XML file
        /// </summary>
        public string GenericText
        {
            get
            {
                if (ItemType == Type.Text)
                {
                    return GetPreview<TextPreview, string>(ref textPreview, ref validGenericText, TextPreview.Initialize);
                }
                else if (ItemType == Type.Xml)
                {
                    return GetPreview<XmlPreview, string>(ref xmlPreview, ref validGenericText, XmlPreview.Initialize);
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        //public ViewModel CurrentModel { get; private set; }

        private V GetPreview<T,V>(ref T reference, ref bool validMarker, Func<MemoryStream, string, T> initializer) where T : AbstractPreview<V>
        {
            SetBusy();
            if (reference == null || !reference.Initialized)
            {
                reference = initializer.Invoke(Stream, FileExtension);
                reference.CreatePreview();
            }
            if (reference.ValidItem)
            {
                validMarker = true;
                ErrorMessage = string.Empty;
                SetBusy(false);
                return reference.Preview;
            }
            else
            {
                validMarker = false;
                ErrorMessage = reference.ErrorMessage;
                SetBusy(false);
                return default(V);
            }
        }

        /// <summary>
        /// Message of the last occurred error when processing the item
        /// </summary>
        public string ErrorMessage { get; set; }


        /// <summary>
        /// Constructor with parameters
        /// </summary>
        /// <param name="fileName">File name of the item</param>
        /// <param name="stream">Passed memoryStream of the item</param>
        /// <param name="path">Relative path within the archive / file</param>
        public ExtractorItem(string fileName, MemoryStream stream, string path)
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
        }


        internal void SetBusy(bool isBusy = true)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                if (isBusy)
                {
                    Mouse.OverrideCursor = Cursors.Wait;
                }
                else
                {
                    Mouse.OverrideCursor = null;
                }

            });
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
                throw new ArgumentException();
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