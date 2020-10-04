using System;
using System.IO;

namespace MediaExtractor.preview
{
    public class TextPreview : AbstractPreview<string>
    {
        public TextPreview(MemoryStream stream, string extension) : base(stream, extension) { }

        public static TextPreview Initialize(MemoryStream stream, string extension)
        {
            return new TextPreview(stream, extension);
        }

        public override void CreatePreview()
        {
            string genericText;
            try
            {
                StreamReader sr = new StreamReader(Stream);
                genericText = sr.ReadToEnd();
                ValidItem = true;
                ErrorMessage = String.Empty;
            }
            catch (Exception e)
            {
                ErrorMessage = e.Message;
                ValidItem = false;
                genericText = string.Empty;
            }
            this.Preview = genericText;
        }

    }
}
