using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Media.Imaging;

namespace MediaExtractor.preview
{
    public class ImagePreview : AbstractPreview<BitmapImage>
    {
        private static Dictionary<string, Func<MemoryStream, MemoryStream>> imageConverters = null;

        
        public ImagePreview(MemoryStream stream, string extension) : base(stream, extension) { }
        
        /// <summary>
        /// Singleton initializer for image converter functions
        /// </summary>
        public static Dictionary<string, Func<MemoryStream, MemoryStream>> ImageConverters
        {
            get
            {
                if (imageConverters == null)
                {
                    imageConverters = new Dictionary<string, Func<MemoryStream, MemoryStream>>();
                    imageConverters.Add("jpg", GetJpeg);
                    imageConverters.Add("png", GetPng);
                    imageConverters.Add("bmp", GetBmp);
                    imageConverters.Add("gif", GetGif);
                    imageConverters.Add("emf", GetEmf);
                    imageConverters.Add("wmf", GetWmf);
                    imageConverters.Add("wdp", GetWdp);
                }
                return imageConverters;
            }
        }

        /// <summary>
        /// Method to try parsing an image, based on the passed function reference
        /// </summary>
        /// <param name="func">Function reference</param>
        /// <returns>True if the image could be created, otherwise false</returns>
        private bool TryParseImage(Func<MemoryStream, MemoryStream> func, out BitmapImage image)
        {
            try
            {
                MemoryStream ms = func.Invoke(Stream);
                BitmapImage ims = new BitmapImage();
                ims.BeginInit();
                ims.CacheOption = BitmapCacheOption.OnLoad;
                ims.StreamSource = ms;
                ims.EndInit();
                ims.Freeze();
                image = ims;
                ValidItem = true;
                ErrorMessage = String.Empty;
                return true;

            }
            catch (Exception ex)
            {
                ValidItem = false;
                ErrorMessage = ex.Message;
                image = null;
                return false;
            }
        }

        public static ImagePreview Initialize(MemoryStream stream, string extension)
        {
            return new ImagePreview(stream, extension);
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

        /// <summary>
        /// Method to create an image object from the item
        /// </summary>
        public override void CreatePreview()
        {
            BitmapImage image;
            MemoryStream ms2;
            string ext = "png"; // Default if not defined
            if (!string.IsNullOrEmpty(FileExtension))
            {
                ext = FileExtension.ToLower();
            }
            if (ImageConverters.ContainsKey(ext) && TryParseImage(ImageConverters[ext], out image))
            {
                this.Preview = image;
                return;
            }

            foreach (KeyValuePair<string, Func<MemoryStream, MemoryStream>> item in ImageConverters)
            {
                if (TryParseImage(item.Value, out image))
                {
                    this.Preview = image;
                    return;
                }
            }
            ValidItem = false;
            this.Preview = null;
        }
    }
}
