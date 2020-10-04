using System;
using System.IO;
using System.Text;
using System.Xml;

namespace MediaExtractor.preview
{
    public class XmlPreview : AbstractPreview<string>
    {
        public XmlPreview(MemoryStream stream, string extension) : base(stream, extension) { }

        public static XmlPreview Initialize(MemoryStream stream, string extension)
        {
            return new XmlPreview(stream, extension);
        }

        public override void CreatePreview()
        {
            try
            {
                StreamReader sr = new StreamReader(Stream);
                string rawText = sr.ReadToEnd();
                XmlDocument doc = new XmlDocument();
                doc.XmlResolver = null;
                doc.LoadXml(rawText);
                StringBuilder sb = new StringBuilder();
                TextWriter tw = new StringWriter(sb);
                XmlTextWriter xtw = new XmlTextWriter(tw);
                xtw.Formatting = Formatting.Indented;
                doc.Save(xtw);
                xtw.Flush();
                xtw.Close();
                ValidItem = true;
                ErrorMessage = String.Empty;
                this.Preview = sb.ToString();
            }
            catch (Exception e)
            {
                ErrorMessage = e.Message;
                ValidItem = false;
                this.Preview = string.Empty;
            }
        }
    }
}
