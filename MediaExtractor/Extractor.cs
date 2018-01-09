using System;
using System.Collections.Generic;
using SevenZipExtractor;
using System.IO;
using System.Windows.Media.Imaging;
using System.Drawing.Imaging;

namespace MediaExtractor
{
    public class Extractor
    {

        public enum SourceFormat
        {
            word,
            excel,
            powerPoint,
            other,
        }

        public enum ImageFormat
        {
            emf,
            wmf,
            png,
            jpg,
            all,
        }

        private string lastError;
        private bool hasErrors;
        private List<ExtractorItem> images;
        private ViewModel currentModel;

        public List<ExtractorItem> Images
        {
            get { return this.images; }
        }

        public string FileName { get; set; }

        public string LastError
        {
            get { return lastError; }
        }

        public bool HasErrors
        {
            get { return hasErrors; }
        }

        public int NumberOfImages
        {
            get
            {
                if (this.images == null) { return 0; }
                else { return this.images.Count;  }
            }
        }

        public SourceFormat DocumentFormat { get; set; }

        public Extractor(string file, ViewModel model)
        {
            this.FileName = file;
            this.lastError = "";
            this.DocumentFormat =  SourceFormat.other;
            this.images = new List<ExtractorItem>();
            this.currentModel = model;
        }

        public Extractor(string file, SourceFormat format, ViewModel model)
        {
            this.FileName = file;
            this.lastError = "";
            this.DocumentFormat = format;
            this.images = new List<ExtractorItem>();
            this.currentModel = model;
        }

        public void ResetErrors()
        {
            this.hasErrors = false;
            this.lastError = string.Empty;
        }

        public void Extract(ImageFormat format)
        {
            try
            {
                // SevenZip.SevenZipExtractor ex = new SevenZipExtractor(this.FileName);
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

        /*
        public bool GetImageStreamByName(string filename, out MemoryStream stream)
        {
            if (this.images == null)
            {
                stream = null;
                hasErrors = true;
                lastError = "No images were found";
                return false;
            }
            foreach(ExtractorItem item in this.images)
            {
                if (item.FileName == filename)
                {
                    stream = item.Stream;
                    stream.Position = 0;
                    return true;
                }
            }
            stream = null;
            hasErrors = true;
            lastError = "The image with the name " + filename + " was not found";
            return false;
        }
         * */


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

            /*
            MemoryStream ms;
            if (GetImageStreamByName(filename, out ms) == false)
            {
                image = null;
                return false;
            }
            try
            {
                Metafile mf = new Metafile(ms);
                MemoryStream ms2 = new MemoryStream();
                mf.Save(ms2, System.Drawing.Imaging.ImageFormat.Png);
                ms2.Flush();
                ms2.Position = 0;
                BitmapImage ims = new BitmapImage();
                ims.BeginInit();
                ims.CacheOption = BitmapCacheOption.OnLoad;
                ims.StreamSource = ms2;
                ims.EndInit();
                image = ims;
                return true;
            }
            catch(Exception e)
            {
                this.hasErrors = true;
                this.lastError = e.Message;
                image = null;
                return false;
            }
             * */
        }


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
                    extension = ".png";
                case ImageFormat.png:
                    break;
                    extension = ".jpg";
                case ImageFormat.jpg:
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
            for(int i = 0; i < archive.Entries.Count; i++)
            {
                if ((archive.Entries[i].IsFolder == false && archive.Entries[i].FileName.ToLower().EndsWith(extension)) || allFiles == true)
                {
                    ms = new MemoryStream();
                    archive.Entries[i].Extract(ms);
                    ms.Flush();
                    ms.Position = 0;
                    split = archive.Entries[i].FileName.Split(delimiter);
                    list.Add(new ExtractorItem(split[split.Length - 1], ms, false));
                }
            }
            return list;
        }

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

        public class ExtractorItem
        {
            public string FileExtension { get; set; }
            public string FileName { get; set; }
            public MemoryStream Stream { get; set; }
            public bool ValidImage { get; set; }
            public bool IsImage { get; set; }

            private BitmapImage image;
            private bool initialized = false;

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
            

            public string ErrorMessage { get; set; }

            public ExtractorItem()
            {

            }

            public ExtractorItem(string fileName, MemoryStream stream, bool createImage)
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
                this.Stream = stream;
                this.ErrorMessage = string.Empty;
                this.IsImage = IsImage;
                if (createImage == true && this.IsImage == true)
                {
                    CreateImage(true);
                    this.initialized = true;
                }
            }

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
