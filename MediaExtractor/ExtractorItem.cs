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
        private BitmapImage image;
        private string genericText;
        private bool initialized = false;
        
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
        /// If true, the item is described as image file (by its file extension)
        /// </summary>
        public bool IsImage { get; set; }
        /// <summary>
        /// If true, the item is described as XML file (by its file extension)
        /// </summary>
        public bool IsXml { get; set; }
        /// <summary>
        /// If true, the item is described as text file (by its file extension)
        /// </summary>
        public bool IsText { get; set; }
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
                
                    if (IsText)
                    {
                        CreateText();
                        initialized = true;
                    }
                    else if (IsXml)
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
                FileExtension = tokens[tokens.Length - 1].ToUpper();
                if (FileExtension == "JPG" || FileExtension == "JPEG" || FileExtension == "PNG" || FileExtension == "WMF" || FileExtension == "EMF"  || FileExtension == "GIF" || FileExtension == "BMP" || FileExtension == "ICO")
                {
                    IsImage = true;
                }
                else if (FileExtension == "TXT" || FileExtension == "MD" || FileExtension == "LOG" || FileExtension == "ME" || FileExtension == "README")
                {
                    IsText = true;
                }
                else if (FileExtension == "XML" || FileExtension == "RELS")
                {
                    IsXml = true;
                }
            }
            else
            {
                FileExtension = "";
                IsImage = false;
            }

            FileName = fileName;
            Path = path;
            Stream = stream;
            ErrorMessage = String.Empty;
            IsImage = IsImage;
            if (createFile == true)
            {
                if (IsImage == true)
                {
                    CreateImage(true);
                }
                else if (IsXml == true)
                {
                    CreateXml();
                }
                else if (IsText == true)
                {
                    CreateText();
                }
                initialized = true;
            }
            
        }

        /// <summary>
        /// Method to create plain text
        /// </summary>
        public void CreateText()
        {
            try
            {
                StreamReader sr = new StreamReader(this.Stream);
                this.genericText = sr.ReadToEnd();
                sr.Close();
                ValidGenericText = true;
                this.ErrorMessage = String.Empty;
            }
            catch (Exception e)
            {
                this.ErrorMessage = e.Message;
                this.ValidGenericText = false;
                this.genericText = string.Empty;
            }
            
        }

        /// <summary>
        /// Method to create XML as text
        /// </summary>
        public void CreateXml()
        {
            CreateText();
            if (this.ValidGenericText == false)
            {
                return;
            }

            try
            {
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(this.genericText);
                StringBuilder sb = new StringBuilder();
                TextWriter tw = new StringWriter(sb);
                XmlTextWriter xtw = new XmlTextWriter(tw);
                xtw.Formatting = Formatting.Indented;
                doc.Save(xtw);
                xtw.Flush();
                xtw.Close();
                this.genericText = sb.ToString();
            }
            catch (Exception e)
            {
                this.ErrorMessage = e.Message;
                this.ValidGenericText = false;
                this.genericText = string.Empty;
            }
        }

        /// <summary>
        /// Method to create an image object from the item
        /// </summary>
        /// <param name="retry">If false, only an attempt as png file will be performed. If true, all formats (jpg, emf, bmp, gif and wmf) will be tried after a fail of a png conversion</param>
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
                    ValidImage = true;
                    this.ErrorMessage = String.Empty;
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