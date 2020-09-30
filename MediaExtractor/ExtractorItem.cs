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
using System.Linq;
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
        private static readonly char[] EXT_SPLITTERS = new char[] { ',', ';', ' ', '.', '/', '\\', '|' };

        private static List<string> imageExtensions;
        private static List<string> textExtensions;
        private static List<string> xmlExtensions;

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
                if (image == null && !initialized)
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
                if (genericText == null && !initialized)
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
            if (!ValidGenericText)
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

    }
}