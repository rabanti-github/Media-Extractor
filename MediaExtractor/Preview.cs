/*
 * Media Extractor is an application to preview and extract packed media in Microsoft Office files (e.g. Word, PowerPoint or Excel documents)
 * Copyright Raphael Stoeckli © 2023
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
    /// Static class to generate content preview
    /// </summary>
    public static class Preview
    {
        private static Dictionary<string, Func<MemoryStream, MemoryStream>> imageConverters = null;

        /// <summary>
        /// Singleton initializer for image converter functions
        /// </summary>
        public static Dictionary<string, Func<MemoryStream, MemoryStream>> ImageConverters
        {
            get
            {
                if (imageConverters == null)
                {
                    imageConverters = new Dictionary<string, Func<MemoryStream, MemoryStream>>
                    {
                        { "jpg", GetJpeg },
                        { "png", GetPng },
                        { "bmp", GetBmp },
                        { "gif", GetGif },
                        { "tif", GetTiff },
                        { "tiff", GetTiff },
                        { "emf", GetEmf },
                        { "wmf", GetWmf },
                        { "wdp", GetWdp }
                    };
                }
                return imageConverters;
            }
        }

        /// <summary>
        /// Method to create plain text
        /// </summary>
        /// <param name="inputStream">Stream to process</param>
        /// <param name="text">result text as output parameter</param>
        /// <param name="errorMessage">Possible error message. Is empty when no error occurred</param>
        /// <returns>True if valid, otherwise false. In the later case, an error message is returned in the out parameter</returns>
        public static bool CreateText(MemoryStream inputStream, out string text, out string errorMessage)
        {
            try
            {
                StreamReader sr = new StreamReader(inputStream);
                text = sr.ReadToEnd();
                errorMessage = String.Empty;
                return true;
            }
            catch (Exception e)
            {
                errorMessage = e.Message;
                text = string.Empty;
                return false;
            }

        }

        /// <summary>
        /// Method to create XML as text
        /// </summary>
        /// <param name="inputStream">Stream to process</param>
        /// <param name="text">result text as output parameter</param>
        /// <param name="errorMessage">Possible error message. Is empty when no error occurred</param>
        /// <param name="fallbackToText">If true, the preview tries to fall back to a plain text preview, otherwise no preview is displayed</param>
        /// <returns>True if valid, otherwise false. In the later case, an error message is returned in the out parameter</returns>
        public static bool CreateXml(MemoryStream inputStream, bool fallbackToText, out string text, out string errorMessage)
        {
            bool check = CreateText(inputStream, out string tempText, out string lastError);
            if (!check)
            {
                errorMessage = lastError;
                text = string.Empty;
                return false;
            }
            try
            {
                XmlDocument doc = new XmlDocument
                {
                    XmlResolver = null
                };
                doc.LoadXml(tempText);
                StringBuilder sb = new StringBuilder();
                TextWriter tw = new StringWriter(sb);
                XmlTextWriter xtw = new XmlTextWriter(tw)
                {
                    Formatting = Formatting.Indented
                };
                doc.Save(xtw);
                xtw.Flush();
                xtw.Close();
                errorMessage = string.Empty;
                text = sb.ToString();
                return true;
            }
            catch (Exception e)
            {
                if (fallbackToText)
                {
                    errorMessage = string.Empty;
                    text = tempText;
                    return true;
                }
                else
                {
                    errorMessage = e.Message;
                    text = string.Empty;
                    return false;
                }
            }
        }

        /// <summary>
        /// Method to create an image object from the item
        /// </summary>
        /// <param name="fileExtension">Extension of the image file (necessary for the appropriate decoder)</param>
        /// <param name="inputStream">Stream to process</param>
        /// <param name="image">result image as output parameter</param>
        /// <param name="errorMessage">Possible error message. Is empty when no error occurred</param>
        /// <param name="fallbackToText">If true, the preview tries to fall back to a plain text preview, otherwise no preview is displayed</param>
        /// <param name="fallbackText">Fallback text if image could not be parsed</param>
        /// <returns>True if valid, otherwise false. In the later case, an error message is returned in the out parameter</returns>
        /// <returns></returns>
        public static bool CreateImage(string fileExtension, MemoryStream inputStream, bool fallbackToText, out BitmapImage image, out string errorMessage, out string fallbackText)
        {
            string lastError = string.Empty;
            string ext = "png"; // Default if not defined
            if (!string.IsNullOrEmpty(fileExtension))
            {
                ext = fileExtension.ToLower();
            }
            if (ImageConverters.ContainsKey(ext) && TryParseImage(ImageConverters[ext], inputStream, out image, out lastError))
            {
                errorMessage = string.Empty;
                fallbackText = string.Empty;
                return true;
            }

            foreach (KeyValuePair<string, Func<MemoryStream, MemoryStream>> item in ImageConverters)
            {
                if (TryParseImage(item.Value, inputStream, out image, out lastError))
                {
                    errorMessage = string.Empty;
                    fallbackText = string.Empty;
                    return true;
                }
            }
            image = null; // Not a valid image
            if (fallbackToText)
            {
                inputStream.Position = 0;
                if (CreateText(inputStream, out fallbackText, out lastError))
                {
                    errorMessage = string.Empty;
                    return false;
                }
            }
            errorMessage = lastError;
            fallbackText = string.Empty;
            return false;
        }

        /// <summary>
        /// Method to try parsing an image, based on the passed function reference
        /// </summary>
        /// <param name="func">Function reference</param>
        /// <returns>True if the image could be created, otherwise false</returns>
        private static bool TryParseImage(Func<MemoryStream, MemoryStream> func, MemoryStream inputStream, out BitmapImage otuput, out string errorMessage)
        {
            try
            {
                MemoryStream ms = func.Invoke(inputStream);
                BitmapImage ims = new BitmapImage();
                ims.BeginInit();
                ims.CacheOption = BitmapCacheOption.OnLoad;
                ims.StreamSource = ms;
                ims.EndInit();
                ims.Freeze();
                otuput = ims;
                errorMessage = String.Empty;
                return true;

            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                otuput = null;
                return false;
            }
        }

        /// <summary>
        /// Method, used as function reference to create a JPG
        /// </summary>
        /// <param name="input">Input memory stream</param>
        /// <returns>Output memory stream of a BitmapImage</returns>
        private static MemoryStream GetJpeg(MemoryStream input)
        {
            return GetBitmap(input, ImageFormat.Jpeg);
        }

        /// <summary>
        /// Method, used as function reference to create a PNG
        /// </summary>
        /// <param name="input">Input memory stream</param>
        /// <returns>Output memory stream of a BitmapImage</returns>
        private static MemoryStream GetPng(MemoryStream input)
        {
            return GetBitmap(input, ImageFormat.Png);
        }

        /// <summary>
        /// Method, used as function reference to create a BMP
        /// </summary>
        /// <param name="input">Input memory stream</param>
        /// <returns>Output memory stream of a BitmapImage</returns>
        private static MemoryStream GetBmp(MemoryStream input)
        {
            return GetBitmap(input, ImageFormat.Bmp);
        }

        /// <summary>
        /// Method, used as function reference to create a GIF
        /// </summary>
        /// <param name="input">Input memory stream</param>
        /// <returns>Output memory stream of a BitmapImage</returns>
        private static MemoryStream GetGif(MemoryStream input)
        {
            return GetBitmap(input, ImageFormat.Gif);
        }

        /// <summary>
        /// Method, used as function reference to create a TIFF
        /// </summary>
        /// <param name="input">Input memory stream</param>
        /// <returns>Output memory stream of a BitmapImage</returns>
        private static MemoryStream GetTiff(MemoryStream input)
        {
            return GetBitmap(input, ImageFormat.Tiff);
        }

        /// <summary>
        /// Method, used as function reference to create a WMF
        /// </summary>
        /// <param name="input">Input memory stream</param>
        /// <returns>Output memory stream of a BitmapImage</returns>
        private static MemoryStream GetWmf(MemoryStream input)
        {
            return GetMetaFile(input, ImageFormat.Wmf);
        }

        /// <summary>
        /// Method, used as function reference to create a EMF
        /// </summary>
        /// <param name="input">Input memory stream</param>
        /// <returns>Output memory stream of a BitmapImage</returns>
        private static MemoryStream GetEmf(MemoryStream input)
        {
            return GetMetaFile(input, ImageFormat.Emf);
        }

        /// <summary>
        /// Method, used as function reference to create a WDP
        /// </summary>
        /// <param name="input">Input memory stream</param>
        /// <returns>Output memory stream of a BitmapImage</returns>
        private static MemoryStream GetWdp(MemoryStream input)
        {
            MemoryStream ms = new MemoryStream();
            WmpBitmapDecoder wmp = new WmpBitmapDecoder(input, BitmapCreateOptions.None, BitmapCacheOption.Default);
            BitmapEncoder encoder = new BmpBitmapEncoder();
            encoder.Frames.Add(wmp.Frames[0]);
            encoder.Save(ms);
            ms.Flush();
            ms.Position = 0;
            return ms;
        }

        /// <summary>
        /// Generic method, used as function reference to create a metafile image
        /// </summary>
        /// <param name="input">Input memory stream</param>
        /// <param name="format"Image format to apply during the creation</param>
        /// <returns>Output memory stream of a BitmapImage</returns>
        private static MemoryStream GetMetaFile(MemoryStream input, ImageFormat format)
        {
            MemoryStream ms = new MemoryStream();
            Metafile mf = new Metafile(input);
            mf.Save(ms, format);
            ms.Flush();
            ms.Position = 0;
            return ms;
        }

        /// <summary>
        /// Generic method, used as function reference to create a bitmap-like image
        /// </summary>
        /// <param name="input">Input memory stream</param>
        /// <param name="format"Image format to apply during the creation</param>
        /// <returns>Output memory stream of a BitmapImage</returns>
        private static MemoryStream GetBitmap(MemoryStream input, ImageFormat format)
        {
            MemoryStream ms = new MemoryStream();
            using (System.Drawing.Image img = System.Drawing.Image.FromStream(input))
            {
                img.Save(ms, format);
            }
            ms.Flush();
            ms.Position = 0;
            return ms;
        }
    }
}
